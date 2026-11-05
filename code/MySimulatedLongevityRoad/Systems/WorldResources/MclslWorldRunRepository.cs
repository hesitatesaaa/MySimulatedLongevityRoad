using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

internal static partial class MclslWorldRunRepository
{
    private const int MaxEvents = 400;
    private const int MaxMilestones = 128;
    private const int MaxDeaths = 600;
    private const int MaxFactionMissions = 300;
    private const int MaxFactionPressures = 180;
    private const int MaxResourceSpends = 300;
    private const int MaxRuinExplorations = 800;
    private const int MaxGeneratedItems = 1000;
    private const int MaxTianxuanActivities = 256;
    private const int MaxUsedGeneratedNames = 2400;
    private const int MaxMaterialAwardEventKeys = 8192;
    internal const int MaxSectRuinRecords = 49;
    internal const int SectRuinRecoveryFloor = 10;
    private const int MaxWorldCaveRecords = 64;
    private const int MaxWorldChangeRecords = 64;
    private const int MaxReincarnationRecords = 200;
    private const int MaxEventsPerYear = 36;
    private static MclslWorldRunState _current = new();
    private static MclslWorldRunState _normalizedRun;
    private static MclslWorldRunState _ensuredRun;
    private static int _ensuredYear = -1;
    // The anchor pass is a structural repair, not per-year work: anchors are only
    // ever appended, so once a pass over one run added everything it was going to
    // add, repeating it only re-walks the same list. The scheduler alternates the
    // world clock year and the backlogged annual year, which used to re-run that
    // scan per flip. The technique-seed pass is still executed on every call; it
    // only reuses this id index instead of scanning the lineage list per
    // definition, so a removed seed is still restored immediately.
    private static MclslWorldRunState _anchorsEnsuredRun;
    private static readonly Dictionary<string, MclslTechniqueLineageRecord> SeedLineageIndex = new(StringComparer.Ordinal);
    private static MclslWorldRunState _futureCatalogsRun;
    private static bool _futureCatalogsTracePersistence;
    private static int _futureCatalogsLastMarkedYear = -1;
    private static int _mapMarkerDataRevision;
    private static MclslWorldRunState _materialAwardKeysRun;
    private static HashSet<string> _materialAwardKeyIndex;
    private static MclslWorldRunState _eventIndexesRun;
    private static readonly Dictionary<(int Year, string Type, string Title), int> EventTitleKeys = new();
    private static readonly Dictionary<(int Year, string Type, string Body), int> EventBodyKeys = new();
    private static readonly HashSet<string> EventKeys = new(StringComparer.Ordinal);
    private static readonly Dictionary<int, int> EventCountsByYear = new();
    private static readonly Dictionary<(int Year, string Type), int> EventCountsByTypeYear = new();
    // Runtime counts of ordinary vs milestone events in _current.Events. They drive
    // a lazy trim decision so the common append path avoids the O(N) cleanup scan;
    // the authoritative recount happens whenever a real trim runs (see
    // RebuildEventCounterFromList) so any drift is absorbed at that boundary.
    private static int _eventOrdinaryCount;
    private static int _eventMilestoneCount;
    internal static MclslWorldRunState Current => _current;
    internal static int MapMarkerDataRevision => _mapMarkerDataRevision;

    /// <summary>
    /// 仅新建世界启用持续清理（族史/死亡个人历史）。缺少标记的既有 v24 存档为关闭。
    /// </summary>
    internal static bool PersistentCleanupEnabled
        => MclslCleanupPolicy.ResolvePersistentCleanup(_current?.PersistentCleanupEnabled);

    internal static void NotifyMapMarkerDataChanged()
    {
        unchecked { _mapMarkerDataRevision++; }
    }

    internal static void EnsureCurrentRun(int year)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { EnsureCurrentRunCore(year); }
        finally { MclslPerformanceProbe.End("档案.EnsureCurrentRun", sample); }
    }

    /// <summary>
    /// 放弃一个“比当前世界年更靠前”的年度（世界年回退后遗留的在途年份）时，
    /// 必须清掉该年的阶段游标与临时认领表；否则重做该年时会把旧草稿当成
    /// “已认领/已完成”，从而漏生成探索、洞天、天地变、家族等年度事件——
    /// 那才会造成真正的事件损失。
    /// </summary>
    internal static void ResetAbandonedAnnualYear(MclslAnnualBatchState batch)
    {
        if (batch == null) return;
        batch.WorldStage = 0;
        batch.StartCursor = 0;
        batch.EraSnapshotRecorded = false;
        batch.LineageActorIds?.Clear();
        batch.CaveClaims?.Clear();
        batch.ChangeClaims?.Clear();
        batch.AdventureCandidates?.Clear();
        batch.TechniqueLineagePendingIds?.Clear();
        batch.TechniqueLineagePendingInitialized = false;
        batch.SectLifecycleCursor = 0;
        batch.SectLifecycleEmitted = 0;
        batch.MarketPruneYear = 0;
        batch.MarketPruneCursor = 0;
        batch.MarketPruneListingIds?.Clear();
        batch.MarketRecycleBudgetStones = 0;
        batch.MarketRecycleSpentStones = 0;
        batch.InverseWork = null;
        batch.WorldSoulYear = 0;
        batch.WorldSoulCursor = 0;
        batch.FamilyCursor = 0;
        batch.FamilyMemberCursor = 0;
        batch.FamilySettlementYear = 0;
        batch.FamilySettlementIds?.Clear();
        batch.ModuleBlockedYears?.Clear();
        batch.WorldFailureStage = 0;
        batch.WorldFailureCount = 0;
    }

    private static void EnsureCurrentRunCore(int year)
    {
        if (MclslWorldArchiveStore.WriteLocked) return;
        // 世界年回退（读旧档 / 还真锚点）后，把年度游标夹回当前世界年：
        // 旧世界遗留的高位“请求年/活动年”会让角色永远排在“世界年+1”，
        // 就绪队列无法释放，积压恒 ≥10，回压永久停在暂停档。
        MclslAnnualBatchState annualBatch = _current.AnnualBatch ??= new MclslAnnualBatchState();
        MclslAnnualYearReconcilePolicy.BatchYears clamp =
            MclslAnnualYearReconcilePolicy.ClampBatch(annualBatch.LatestRequestedYear,
                annualBatch.LastCompletedYear, annualBatch.ActiveYear, year);
        if (clamp.LatestRequestedYear != annualBatch.LatestRequestedYear
            || clamp.LastCompletedYear != annualBatch.LastCompletedYear
            || clamp.ActiveYear != annualBatch.ActiveYear)
        {
            annualBatch.LatestRequestedYear = clamp.LatestRequestedYear;
            annualBatch.LastCompletedYear = clamp.LastCompletedYear;
            annualBatch.ActiveYear = clamp.ActiveYear;
            if (clamp.ResetWorldStage) ResetAbandonedAnnualYear(annualBatch);
            MclslWorldArchiveStore.MarkDirty();
        }
        if (ReferenceEquals(_ensuredRun, _current) && _ensuredYear == year) return;
        NormalizeCurrentRun();
        MclslReincarnationProfile profile = MclslReincarnationProfileStore.Current;
        if (string.IsNullOrWhiteSpace(_current.RunId))
        {
            _current = new MclslWorldRunState
            {
                RunId = Guid.NewGuid().ToString("N"),
                CycleNumber = Math.Max(1, profile.CurrentCycle),
                StartYear = Math.Max(0, year),
                LastProcessedYear = Math.Max(0, year),
                CultivationEpoch = MclslWorldEpochSystem.AncientLawEpoch,
                EraOriginYear = Math.Max(0, year),
                AncientLawEndYear = Math.Max(0, year) + MclslWorldEpochSystem.AncientLawDurationYears,
                TransmissionEndYear = Math.Max(0, year) + MclslWorldEpochSystem.AncientLawDurationYears + MclslWorldEpochSystem.TransmissionDurationYears,
                NewLawStartYear = Math.Max(0, year) + MclslWorldEpochSystem.AncientLawDurationYears + MclslWorldEpochSystem.TransmissionDurationYears,
                UsesNativeKingdoms = true,
                NextCaveBirthYear = Math.Max(0, year) + 80,
                PersistentCleanupEnabled = true,
                FamilyRuleMode = MclslFamilyRuleMode.Patrilineal,
                FamilyFeatureVersion = 1
            };
            InvalidateCurrentRunCaches();
            NotifyMapMarkerDataChanged();
            NormalizeCurrentRun();
            int carryLimit = Math.Max(MclslRuntimeSettings.CarrySlotLimit, profile.CarrySlotLimit);
            int limit = Math.Min(profile.KnownKnowledgeIds.Count, carryLimit);
            for (int i = 0; i < limit; i++) _current.InheritedKnowledgeIds.Add(profile.KnownKnowledgeIds[i]);
        }
        EnsureAnchors();
        EnsureAncientTechniqueSeeds(year);
        _ensuredRun = _current;
        _ensuredYear = year;
    }

    internal static void EnsureAnnualWorldState(int year)
    {
        EnsureCurrentRun(year);
        MclslWorldEpochSystem.EnsureAnnualState(year);
        if (MclslWorldEpochSystem.IsNewLawActive(year))
            EnsureNewLawCatalogs(year);
    }

    internal static void EnsureNewLawCatalogs(int year)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { EnsureNewLawCatalogsCore(year); }
        finally { MclslPerformanceProbe.End("档案.EnsureNewLawCatalogs", sample); }
    }

    private static void EnsureNewLawCatalogsCore(int year)
    {
        if (string.IsNullOrWhiteSpace(_current.RunId))
            EnsureCurrentRun(year);
        NormalizeCurrentRun();
        bool tracePersistence = IsTracePersistenceReversed();
        bool catalogStateChanged = !ReferenceEquals(_futureCatalogsRun, _current)
            || _futureCatalogsTracePersistence != tracePersistence;
        if (catalogStateChanged)
        {
            EnsureFutureCatalogs(tracePersistence);
            _futureCatalogsRun = _current;
            _futureCatalogsTracePersistence = tracePersistence;
            _futureCatalogsLastMarkedYear = -1;
        }
        if (catalogStateChanged || _futureCatalogsLastMarkedYear != year)
        {
            MclslWorldArchiveStore.MarkDirty();
            _futureCatalogsLastMarkedYear = year;
        }
    }

    internal static MclslWorldArchiveBundle ExportArchive()
    {
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            NormalizeCurrentRun();
            return new MclslWorldArchiveBundle
            {
                Version = MclslSaveVersions.WorldArchive,
                CurrentRun = _current
            };
        }
        finally { MclslPerformanceProbe.End("档案导出", sample); }
    }

    internal static bool ImportArchive(MclslWorldArchiveBundle bundle)
    {
        if (bundle == null || bundle.Version != MclslSaveVersions.WorldArchive || bundle.CurrentRun == null)
            throw new InvalidOperationException("世界档案格式不匹配；本架构仅支持新格式存档");
        MclslWorldRunState candidate = bundle.CurrentRun;
        if (string.IsNullOrWhiteSpace(candidate.RunId) || candidate.StartYear < 0
            || candidate.AncientLawEndYear <= 0 || candidate.TransmissionEndYear <= 0 || candidate.NewLawStartYear <= 0
            || candidate.EraOriginYear < 0 || candidate.AncientLawEndYear < candidate.EraOriginYear
            || candidate.TransmissionEndYear < candidate.AncientLawEndYear
            || candidate.NewLawStartYear < candidate.TransmissionEndYear)
            throw new InvalidOperationException("世界档案缺少有效身份或时代时间线");
        MclslWorldRunState previous = _current;
        try
        {
            // Normalize the detached candidate without publishing a revision. Failed
            // candidates must not become the source for saving or backup recovery.
            _current = candidate;
            Normalize();
        }
        catch
        {
            _current = previous;
            InvalidateCurrentRunCaches();
            MclslWorldRunRuntimeIndexes.Invalidate();
            throw;
        }
        InvalidateCurrentRunCaches();
        NotifyMapMarkerDataChanged();
        _normalizedRun = _current;
        InvalidateEventIndexes();
        EnsureEventIndexes();
        MclslWorldRunRuntimeIndexes.Invalidate();
        return true;
    }

    private static void TrimSectRuinsToLimit()
    {
        if (_current.SectRuins == null) return;
        int originalCount = _current.SectRuins.Count;
        _current.SectRuins.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.Id));
        if (_current.SectRuins.Count <= MaxSectRuinRecords)
        {
            if (_current.SectRuins.Count != originalCount) NotifyMapMarkerDataChanged();
            return;
        }

        HashSet<string> linkedIds = new((_current.TechniqueLineages ?? new List<MclslTechniqueLineageRecord>())
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.LinkedRuinId))
            .Select(x => x.LinkedRuinId), StringComparer.Ordinal);
        List<MclslSectRuinRecord> keep = _current.SectRuins
            .OrderByDescending(x => IsRuinAvailableForRetention(x) ? 3 : linkedIds.Contains(x.Id) ? 2 : 1)
            .ThenByDescending(x => x.BornYear)
            .Take(MaxSectRuinRecords)
            .OrderBy(x => x.BornYear)
            .ToList();
        HashSet<string> keptIds = new(keep.Select(x => x.Id), StringComparer.Ordinal);
        _current.SectRuins = keep;
        NotifyMapMarkerDataChanged();
        CleanupRemovedRuinReferences(keptIds);
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static void TrimWorldCavesToLimit()
    {
        if (_current.WorldCaves == null) return;
        _current.WorldCaves.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.Id));
        if (_current.WorldCaves.Count <= MaxWorldCaveRecords) return;
        CollectLiveResourceReferences(out HashSet<string> protectedCaves, out _);
        List<MclslWorldCaveRecord> keep = _current.WorldCaves
            .Where(x => protectedCaves.Contains(x.Id))
            .OrderBy(x => x.BornYear)
            .ToList();
        int remaining = Math.Max(0, MaxWorldCaveRecords - keep.Count);
        if (remaining > 0)
        {
            keep.AddRange(_current.WorldCaves
                .Where(x => !protectedCaves.Contains(x.Id))
                .OrderByDescending(x => x.RemainingEssence > 0 && x.Integrity > 0 && !string.Equals(x.State, "枯竭", StringComparison.Ordinal))
                .ThenByDescending(x => x.BornYear)
                .Take(remaining));
        }
        _current.WorldCaves = keep.OrderBy(x => x.BornYear).ToList();
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static void TrimWorldChangesToLimit()
    {
        if (_current.WorldChanges == null) return;
        _current.WorldChanges.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.Id));
        if (_current.WorldChanges.Count <= MaxWorldChangeRecords) return;
        CollectLiveResourceReferences(out _, out HashSet<string> protectedChanges);
        List<MclslWorldChangeRecord> keep = _current.WorldChanges
            .Where(x => protectedChanges.Contains(x.Id))
            .OrderBy(x => x.StartYear)
            .ToList();
        int remaining = Math.Max(0, MaxWorldChangeRecords - keep.Count);
        if (remaining > 0)
        {
            keep.AddRange(_current.WorldChanges
                .Where(x => !protectedChanges.Contains(x.Id))
                .OrderByDescending(x => x.RemainingMarrow > 0 && x.Intensity > 0 && !string.Equals(x.State, "消散", StringComparison.Ordinal))
                .ThenByDescending(x => x.StartYear)
                .Take(remaining));
        }
        _current.WorldChanges = keep.OrderBy(x => x.StartYear).ToList();
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static void CollectLiveResourceReferences(out HashSet<string> caveIds, out HashSet<string> changeIds)
    {
        caveIds = new HashSet<string>(MclslActorRegistry.LiveCaveReferences, StringComparer.Ordinal);
        changeIds = new HashSet<string>(MclslActorRegistry.LiveChangeReferences, StringComparer.Ordinal);
    }

    private static void TrimReincarnationRecordsToLimit()
    {
        if (_current.ReincarnationRecords == null || _current.ReincarnationRecords.Count <= MaxReincarnationRecords) return;
        _current.ReincarnationRecords = _current.ReincarnationRecords
            .Where(x => x != null)
            .OrderByDescending(x => string.Equals(x.Status, "待转", StringComparison.Ordinal))
            .ThenByDescending(x => Math.Max(x.AppliedYear, x.DeathYear))
            .Take(MaxReincarnationRecords)
            .OrderBy(x => Math.Max(x.AppliedYear, x.DeathYear))
            .ToList();
    }

    private static bool IsRuinAvailableForRetention(MclslSectRuinRecord ruin)
    {
        return ruin != null && ruin.RemainingValue > 0
            && !string.Equals(ruin.State, "搜尽", StringComparison.Ordinal)
            && !string.Equals(ruin.State, "封绝", StringComparison.Ordinal)
            && !string.Equals(ruin.State, "崩毁", StringComparison.Ordinal)
            && !string.Equals(ruin.State, "沉寂", StringComparison.Ordinal);
    }

    private static bool IsRetiredRuin(MclslSectRuinRecord ruin) => !IsRuinAvailableForRetention(ruin);

    private static void RemoveSectRuinAt(int index)
    {
        if (_current.SectRuins == null || index < 0 || index >= _current.SectRuins.Count) return;
        string removedId = _current.SectRuins[index]?.Id ?? string.Empty;
        _current.SectRuins.RemoveAt(index);
        NotifyMapMarkerDataChanged();
        if (!string.IsNullOrWhiteSpace(removedId))
        {
            HashSet<string> keptIds = new(_current.SectRuins.Where(x => x != null).Select(x => x.Id), StringComparer.Ordinal);
            CleanupRemovedRuinReferences(keptIds);
        }
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static void CleanupRemovedRuinReferences(HashSet<string> keptIds)
    {
        keptIds ??= new HashSet<string>(StringComparer.Ordinal);
        if (_current.TechniqueLineages != null)
        {
            for (int i = 0; i < _current.TechniqueLineages.Count; i++)
            {
                MclslTechniqueLineageRecord lineage = _current.TechniqueLineages[i];
                if (lineage != null && !string.IsNullOrWhiteSpace(lineage.LinkedRuinId) && !keptIds.Contains(lineage.LinkedRuinId))
                    lineage.LinkedRuinId = string.Empty;
            }
        }
        _current.RuinExplorations?.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.RuinId) || !keptIds.Contains(x.RuinId));
    }

    internal static void ResetWorld()
    {
        _current = new();
        NativeMirrorPending.Clear();
        InvalidateCurrentRunCaches();
        NotifyMapMarkerDataChanged();
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static void TrimOldest<T>(List<T> list, int max)
    {
        if (list == null || max <= 0 || list.Count <= max) return;
        list.RemoveRange(0, list.Count - max);
    }

    /// <summary>
    /// Trimmed from the head, keeping the tail, as the original. Milestones are
    /// capped separately by <see cref="MaxMilestones"/>. When <paramref name="force"/>
    /// is false the trim runs lazily: it only costs a scan after the event counters
    /// overshoot their caps by the hysteresis factor, so an append stream sitting
    /// at the cap does not rescan the whole list on every insertion. When
    /// <paramref name="force"/> is true the counters are recounted and the caps are
    /// enforced unconditionally — used at the save boundary so the archive never
    /// exceeds <see cref="MaxEvents"/>/<see cref="MaxMilestones"/>.
    /// </summary>
    private static bool TrimEventsPreservingMilestones(List<MclslRunEventRecord> list, int max, bool force = false)
    {
        if (list == null) return false;
        bool authoritative = ReferenceEquals(list, _current.Events) && ReferenceEquals(_eventIndexesRun, _current);
        // Counts are authoritative only when the index run is current. Otherwise
        // recount from the list once; this also covers the save-boundary call.
        if (!authoritative) RebuildEventCounterFromList(list);
        if (!MclslEventTrimPolicy.NeedsTrim(_eventOrdinaryCount, _eventMilestoneCount, max, MaxMilestones, force))
            return false;
        int removeOrdinary = MclslEventTrimPolicy.RemoveOrdinary(_eventOrdinaryCount, max);
        int removeMilestones = MclslEventTrimPolicy.RemoveMilestones(_eventMilestoneCount, MaxMilestones);
        // Below the cap after recount: nothing to remove.
        if (removeOrdinary <= 0 && removeMilestones <= 0) return false;
        bool visual = false;
        for (int i = 0; i < list.Count && (removeOrdinary > 0 || removeMilestones > 0);)
        {
            MclslRunEventRecord record = list[i];
            bool important = IsMilestoneEvent(record?.EventType);
            if (important ? removeMilestones <= 0 : removeOrdinary <= 0) { i++; continue; }
            visual |= !string.IsNullOrWhiteSpace(record?.MapVisualKind);
            // Rebuild without UnindexEvent in the non-authoritative path: the
            // index dictionaries are not current, so nothing to unindex there, and
            // counters are rebuilt on the next EnsureEventIndexes anyway.
            if (authoritative) UnindexEvent(record);
            else if (important) _eventMilestoneCount = Math.Max(0, _eventMilestoneCount - 1);
            else _eventOrdinaryCount = Math.Max(0, _eventOrdinaryCount - 1);
            list.RemoveAt(i);
            if (important) removeMilestones--; else removeOrdinary--;
        }
        return visual;
    }

    /// <summary>Authoritative recount of the ordinary/milestone split from a list.</summary>
    private static void RebuildEventCounterFromList(List<MclslRunEventRecord> list)
    {
        int ordinary = 0, milestone = 0;
        for (int i = 0; i < list.Count; i++)
            if (IsMilestoneEvent(list[i]?.EventType)) milestone++; else ordinary++;
        _eventOrdinaryCount = ordinary;
        _eventMilestoneCount = milestone;
    }
    private static void Increment<TKey>(Dictionary<TKey, int> index, TKey key)
    { index.TryGetValue(key, out int count); index[key] = count + 1; }
    private static void Decrement<TKey>(Dictionary<TKey, int> index, TKey key)
    {
        if (!index.TryGetValue(key, out int count)) return;
        if (count > 1) index[key] = count - 1; else index.Remove(key);
    }
    private static readonly Dictionary<long, List<MclslRunEventRecord>> ActorEvents = new();
    internal static IReadOnlyList<MclslRunEventRecord> RecentActorEvents(long id)
    {
        EnsureEventIndexes();
        return ActorEvents.TryGetValue(id, out List<MclslRunEventRecord> events)
            ? events : Array.Empty<MclslRunEventRecord>();
    }
    private static void UnindexEvent(MclslRunEventRecord record)
    {
        if (record == null) return;
        string type = record.EventType ?? string.Empty;
        Decrement(EventTitleKeys, (record.Year, type, record.Title ?? string.Empty));
        Decrement(EventBodyKeys, (record.Year, type, record.Body ?? string.Empty));
        Decrement(EventCountsByYear, record.Year);
        Decrement(EventCountsByTypeYear, (record.Year, type));
        if (!string.IsNullOrWhiteSpace(record.EventKey)) EventKeys.Remove(record.EventKey);
        if (ActorEvents.TryGetValue(record.ActorId, out List<MclslRunEventRecord> events))
        { events.Remove(record); if (events.Count == 0) ActorEvents.Remove(record.ActorId); }
        if (IsMilestoneEvent(record.EventType)) { if (_eventMilestoneCount > 0) _eventMilestoneCount--; }
        else if (_eventOrdinaryCount > 0) _eventOrdinaryCount--;
        MclslCodexWindow.InvalidateActorHistory(record.ActorId);
    }

    private static bool ContainsEventKey(string eventKey)
    {
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            EnsureEventIndexes();
            return EventKeys.Contains(eventKey);
        }
        finally { MclslPerformanceProbe.End("事件键去重", sample); }
    }

    /// <summary>
    /// Appends the record and returns it so callers can fill in actor/location
    /// metadata without re-scanning the event list. Trimming removes from the
    /// head and keeps the tail, so the freshly appended record always survives.
    /// </summary>
    private static MclslRunEventRecord AppendEvent(MclslRunEventRecord record)
    {
        EnsureEventIndexes();
        _current.Events.Add(record);
        IndexEvent(record);
        if (TrimEventsPreservingMilestones(_current.Events, MaxEvents)) NotifyMapMarkerDataChanged();
        return record;
    }

    private static void EnsureEventIndexes()
    {
        if (ReferenceEquals(_eventIndexesRun, _current)) return;
        ActorEvents.Clear();
        EventTitleKeys.Clear();
        EventBodyKeys.Clear();
        EventKeys.Clear();
        EventCountsByYear.Clear();
        EventCountsByTypeYear.Clear();
        _eventOrdinaryCount = 0;
        _eventMilestoneCount = 0;
        List<MclslRunEventRecord> events = _current?.Events;
        if (events != null)
            foreach (MclslRunEventRecord record in events)
                if (record != null) IndexEvent(record);
        _eventIndexesRun = _current;
    }

    private static void IndexEvent(MclslRunEventRecord record)
    {
        IndexActorEvent(record);
        string eventType = record.EventType ?? string.Empty;
        Increment(EventTitleKeys, (record.Year, eventType, record.Title ?? string.Empty));
        Increment(EventBodyKeys, (record.Year, eventType, record.Body ?? string.Empty));
        if (!string.IsNullOrWhiteSpace(record.EventKey)) EventKeys.Add(record.EventKey);
        EventCountsByYear.TryGetValue(record.Year, out int yearCount);
        EventCountsByYear[record.Year] = yearCount + 1;
        (int Year, string Type) typeKey = (record.Year, eventType);
        EventCountsByTypeYear.TryGetValue(typeKey, out int typeCount);
        EventCountsByTypeYear[typeKey] = typeCount + 1;
        if (IsMilestoneEvent(eventType)) _eventMilestoneCount++; else _eventOrdinaryCount++;
    }

    private static void IndexActorEvent(MclslRunEventRecord record)
    {
        if (record.ActorId > 0)
        {
            if (!ActorEvents.TryGetValue(record.ActorId, out List<MclslRunEventRecord> events))
                ActorEvents[record.ActorId] = events = new();
            if (events.Contains(record)) return;
            int position = events.Count;
            while (position > 0 && events[position - 1].Year < record.Year) position--;
            events.Insert(position, record);
            MclslCodexWindow.InvalidateActorHistory(record.ActorId);
        }
    }

    private static void InvalidateEventIndexes()
    {
        _eventIndexesRun = null;
        ActorEvents.Clear();
        EventTitleKeys.Clear();
        EventBodyKeys.Clear();
        EventKeys.Clear();
        EventCountsByYear.Clear();
        EventCountsByTypeYear.Clear();
        _eventOrdinaryCount = 0;
        _eventMilestoneCount = 0;
    }

    internal static void AddEvent(int year, string type, string title, string body)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { AddEventCore(year, type, title, body); }
        finally { MclslPerformanceProbe.End("事件记录与去重", sample); }
    }

    /// <summary>
    /// Same pipeline as <see cref="AddEvent(int,string,string,string)"/> but
    /// returns the appended record so callers can annotate it directly instead
    /// of re-scanning the list for the entry they just added.
    /// </summary>
    private static MclslRunEventRecord AddEventWithRecord(int year, string type, string title, string body)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { return AddEventCore(year, type, title, body); }
        finally { MclslPerformanceProbe.End("事件记录与去重", sample); }
    }


    /// <summary>
    /// Returns the appended record, or <c>null</c> when the event was suppressed
    /// by the silent-event filter, a per-year/type cap, or de-duplication.
    /// </summary>
    private static MclslRunEventRecord AddEventCore(int year, string type, string title, string body, string eventKey = null)
    {
        _current.Events ??= new List<MclslRunEventRecord>();
        EnsureEventIndexes();
        string eventType = type ?? string.Empty;
        int safeYear = Math.Max(0, year);
        string safeTitle = title ?? string.Empty;
        string safeBody = body ?? string.Empty;
        if (ShouldSuppressSilentEvent(eventType, safeTitle, safeBody)) return null;
        if (!AlwaysKeepEvent(eventType))
        {
            EventCountsByYear.TryGetValue(safeYear, out int yearCount);
            EventCountsByTypeYear.TryGetValue((safeYear, eventType), out int typeCount);
            int typeLimit = EventTypeYearLimit(eventType);
            if (yearCount >= MaxEventsPerYear || typeCount >= typeLimit) return null;
        }
        if (!string.IsNullOrEmpty(eventKey) ? ContainsEventKey(eventKey)
            : EventTitleKeys.ContainsKey((safeYear, eventType, safeTitle))
                || EventBodyKeys.ContainsKey((safeYear, eventType, safeBody)))
            return null;
        string category = MclslEventCatalog.CategoryForType(eventType);
        MclslRunEventRecord record = new()
        {
            EventKey = eventKey ?? string.Empty,
            Year = safeYear,
            EventType = eventType,
            Category = category,
            Title = safeTitle,
            Body = safeBody
        };
        // 坐标由调用方在 AddEventCore 返回后补写（AddEvent 的坐标重载、
        // AddMapVisualEvent 等）。镜像到原版历史必须等坐标写完再做，否则
        // 写进原版历史的消息没有 x/y，原版“定位”按钮无效。这里只登记待发，
        // 由调用方补完坐标后调用 FlushNativeMirror 真正写入，避免重复写两条。
        AppendEvent(record);
        FlushReadyNativeMirror();
        if (MclslEventCatalog.ShouldMirrorToNativeHistory(record.EventType,
                MclslEventCatalog.EffectiveImportance(record), MclslRuntimeSettings.LineageNativeHistoryEnabled))
        {
            if (record.MapX >= 0 && record.MapY >= 0)
                record.NativeLogged = MclslNativeHistoryBridge.Add(record);
            else
            {
                if (NativeMirrorPending.Count >= MaxNativeMirrorPending)
                {
                    FlushReadyNativeMirror();
                    if (NativeMirrorPending.Count >= MaxNativeMirrorPending) FlushAllNativeMirror(true);
                }
                NativeMirrorPending.Add(record);
            }
        }
        MclslWorldArchiveStore.MarkDirty();
        return record;
    }

    private const int MaxNativeMirrorPending = 64;
    private static readonly List<MclslRunEventRecord> NativeMirrorPending = new();

    internal static void FlushNativeMirror(MclslRunEventRecord record, bool force = false)
    {
        if (record == null) return;
        NativeMirrorPending.Remove(record);
        if (record.NativeLogged) return;
        if (!MclslEventCatalog.ShouldMirrorToNativeHistory(record.EventType,
                MclslEventCatalog.EffectiveImportance(record), MclslRuntimeSettings.LineageNativeHistoryEnabled))
            return;
        if (!force && (record.MapX < 0 || record.MapY < 0))
        {
            if (!NativeMirrorPending.Contains(record)) NativeMirrorPending.Add(record);
            return;
        }
        record.NativeLogged = MclslNativeHistoryBridge.Add(record);
    }

    private static void FlushReadyNativeMirror()
    {
        // 每次写入事件时，先补发那些后来已补到坐标的待发记录（无坐标的继续等待/溢出处理）。
        for (int i = NativeMirrorPending.Count - 1; i >= 0; i--)
        {
            MclslRunEventRecord pending = NativeMirrorPending[i];
            if (pending != null && pending.MapX >= 0 && pending.MapY >= 0) FlushNativeMirror(pending);
        }
    }

    private static void FlushAllNativeMirror(bool force)
    {
        for (int i = NativeMirrorPending.Count - 1; i >= 0; i--)
            FlushNativeMirror(NativeMirrorPending[i], force);
    }

    internal static void AddMaterialDiscoveryEvent(int year, string title, string body, Actor actor, string eventKey)
    {
        const string eventType = "material_discovery";
        if (!MclslRuntimeSettings.ItemAcquisitionHistoryEnabled || string.IsNullOrWhiteSpace(eventKey) || _current == null) return;
        _current.Events ??= new List<MclslRunEventRecord>();
        int safeYear = Math.Max(0, year);
        string safeTitle = title ?? string.Empty;
        string safeBody = body ?? string.Empty;

        if (ContainsEventKey(eventKey)) return;

        MclslRunEventRecord record = new()
        {
            EventKey = eventKey,
            Year = safeYear,
            EventType = eventType,
            Category = MclslEventCatalog.CategoryForType(eventType),
            Title = safeTitle,
            Body = safeBody
        };
        if (actor?.data != null)
        {
            record.ActorId = MclslActorAccessor.Id(actor);
            record.ActorName = MclslActorAccessor.DisplayName(actor);
            try
            {
                record.MapX = actor.data.x;
                record.MapY = actor.data.y;
            }
            catch (Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldResources-MclslWorldRunRepository-cs-1", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
            try
            {
                record.LocationName = actor.city?.data?.name ?? string.Empty;
                record.KingdomName = actor.kingdom?.data?.name ?? string.Empty;
            }
            catch (Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldResources-MclslWorldRunRepository-cs-2", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
        }
        if (MclslEventCatalog.ShouldMirrorToNativeHistory(record.EventType,
                MclslEventCatalog.EffectiveImportance(record), MclslRuntimeSettings.LineageNativeHistoryEnabled))
            record.NativeLogged = MclslNativeHistoryBridge.Add(record);
        AppendEvent(record);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void AddItemAcquisitionEvent(int year, Actor actor, string itemId, int count, string source)
    {
        if (actor?.data == null || count <= 0 || _current == null) return;
        MclslItemDefinition item = MclslItemCatalog.Get(itemId);
        if (item == null || !MclslItemAcquisitionHistoryPolicy.ShouldRecordFinishedProduct(
                item.Category, item.Grade, MclslRuntimeSettings.ItemAcquisitionHistoryEnabled,
                MclslRuntimeSettings.RecordLowMaterialAcquisitionHistory)) return;
        int safeYear = Math.Max(0, year);
        string actorId = MclslActorAccessor.Id(actor).ToString(System.Globalization.CultureInfo.InvariantCulture);
        string safeSource = string.IsNullOrWhiteSpace(source) ? "未知来源" : source.Trim();
        string eventKey = "item|" + actorId + "|" + item.Id + "|" + count + "|" + safeSource + "|" + safeYear;
        _current.Events ??= new List<MclslRunEventRecord>();
        if (ContainsEventKey(eventKey)) return;

        string actorName = MclslActorAccessor.DisplayName(actor);
        string title = actorName + "获得" + item.Name;
        string place = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? "野外" : actor.city.data.name;
        MclslRunEventRecord record = new()
        {
            EventKey = eventKey,
            Year = safeYear,
            EventType = "item_acquisition",
            Category = MclslEventCatalog.TreasureAcquisition,
            Title = title,
            Body = actorName + "于" + place + "通过" + safeSource + "获得【" + item.Name + "】×" + count + "。",
            ActorId = MclslActorAccessor.Id(actor),
            ActorName = actorName,
            MapX = actor.data.x,
            MapY = actor.data.y,
            LocationName = place,
            KingdomName = actor.kingdom?.data?.name ?? string.Empty
        };
        if (MclslEventCatalog.ShouldMirrorToNativeHistory(record.EventType,
                MclslEventCatalog.EffectiveImportance(record), MclslRuntimeSettings.LineageNativeHistoryEnabled))
            record.NativeLogged = MclslNativeHistoryBridge.Add(record);
        AppendEvent(record);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TryClaimMaterialAwardEventKey(string eventKey)
    {
        if (_current == null || string.IsNullOrWhiteSpace(eventKey)) return false;
        _current.MaterialAwardEventKeys ??= new List<string>();
        if (!ReferenceEquals(_materialAwardKeysRun, _current) || _materialAwardKeyIndex == null)
        {
            _materialAwardKeysRun = _current;
            _materialAwardKeyIndex = new HashSet<string>(_current.MaterialAwardEventKeys, StringComparer.Ordinal);
        }
        if (!_materialAwardKeyIndex.Add(eventKey)) return false;

        _current.MaterialAwardEventKeys.Add(eventKey);
        if (_current.MaterialAwardEventKeys.Count > MaxMaterialAwardEventKeys)
        {
            int removeCount = _current.MaterialAwardEventKeys.Count - MaxMaterialAwardEventKeys;
            for (int i = 0; i < removeCount; i++)
                _materialAwardKeyIndex.Remove(_current.MaterialAwardEventKeys[i]);
            _current.MaterialAwardEventKeys.RemoveRange(0, removeCount);
        }
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void AddEvent(int year, string type, string title, string body, Actor actor)
    {
        MclslRunEventRecord record = AddEventWithRecord(year, type, title, body);
        if (record == null || actor?.data == null) return;
        try
        {
            long actorId = MclslActorAccessor.Id(actor);
            if (record.ActorId != actorId && ActorEvents.TryGetValue(record.ActorId, out List<MclslRunEventRecord> previous))
            {
                previous.Remove(record);
                if (previous.Count == 0) ActorEvents.Remove(record.ActorId);
                MclslCodexWindow.InvalidateActorHistory(record.ActorId);
            }
            record.ActorId = actorId;
            IndexActorEvent(record);
            record.ActorName = MclslActorAccessor.DisplayName(actor);
            record.MapX = actor.data.x;
            record.MapY = actor.data.y;
            record.LocationName = actor.city?.data?.name ?? string.Empty;
            record.KingdomName = actor.kingdom?.data?.name ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(record.MapVisualKind)) NotifyMapMarkerDataChanged();
            FlushNativeMirror(record);
            MclslWorldArchiveStore.MarkDirty();
        }
        catch (Exception mclslEmptyCatchEx) { MclslGuard.Swallowed("empty-catch-code-MySimulatedLongevityRoad-Systems-WorldResources-MclslWorldRunRepository-cs-3", mclslEmptyCatchEx, MclslGuardScope.OptionalMember); }
    }

    internal static void AddEvent(int year, string type, string title, string body, int mapX, int mapY, string locationName, string kingdomName, string eventKey = null)
    {
        MclslRunEventRecord record = string.IsNullOrEmpty(eventKey)
            ? AddEventWithRecord(year, type, title, body)
            : AddEventCore(year, type, title, body, eventKey);
        if (record == null) return;
        record.MapX = mapX;
        record.MapY = mapY;
        record.LocationName = locationName ?? string.Empty;
        record.KingdomName = kingdomName ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(record.MapVisualKind)) NotifyMapMarkerDataChanged();
        FlushNativeMirror(record);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void AddMapVisualEvent(int year, string type, string title, string body, Actor actor, string visualKind, int durationYears)
    {
        MclslRunEventRecord record = AddEventWithRecord(year, type, title, body);
        if (record == null || actor?.data == null) return;
        // Reuse the record returned by the append path; the actor metadata below
        // is filled on the same object instead of scanning the list again.
        record.ActorId = MclslActorAccessor.Id(actor);
        record.ActorName = MclslActorAccessor.DisplayName(actor);
        record.MapX = actor.data.x;
        record.MapY = actor.data.y;
        record.LocationName = actor.city?.data?.name ?? string.Empty;
        record.KingdomName = actor.kingdom?.data?.name ?? string.Empty;
        MclslMapLocationResolver.ResolveActorLocation(actor, year, out int mapX, out int mapY, out string location, out string kingdom);
        ApplyMapVisualToEvent(new ApplyMapVisualToEventArgs
        {
            Record = record,
            Year = year,
            Mapx = mapX,
            Mapy = mapY,
            Locationname = string.IsNullOrWhiteSpace(location) ? actor.city?.data?.name ?? string.Empty : location,
            Kingdomname = string.IsNullOrWhiteSpace(kingdom) ? actor.kingdom?.data?.name ?? string.Empty : kingdom,
            Visualkind = visualKind,
            Durationyears = durationYears
        });
        FlushNativeMirror(record);
    }

    internal static void AddMapVisualEvent(int year, string type, string title, string body, int mapX, int mapY, string locationName, string kingdomName, string visualKind, int durationYears)
    {
        MclslRunEventRecord record = AddEventWithRecord(year, type, title, body);
        if (record == null) return;
        record.MapX = mapX;
        record.MapY = mapY;
        record.LocationName = locationName ?? string.Empty;
        record.KingdomName = kingdomName ?? string.Empty;
        ApplyMapVisualToEvent(new ApplyMapVisualToEventArgs
        {
            Record = record,
            Year = year,
            Mapx = mapX,
            Mapy = mapY,
            Locationname = locationName,
            Kingdomname = kingdomName,
            Visualkind = visualKind,
            Durationyears = durationYears
        });
        FlushNativeMirror(record);
    }

    private static void ApplyMapVisualToEvent(ApplyMapVisualToEventArgs a)
    {
        MclslRunEventRecord record = a.Record;
        int year = a.Year;
        int mapX = a.Mapx;
        int mapY = a.Mapy;
        string locationName = a.Locationname;
        string kingdomName = a.Kingdomname;
        string visualKind = a.Visualkind;
        int durationYears = a.Durationyears;
        if (record == null || string.IsNullOrWhiteSpace(visualKind)) return;
        record.MapX = mapX;
        record.MapY = mapY;
        record.LocationName = locationName ?? string.Empty;
        record.KingdomName = kingdomName ?? string.Empty;
        record.MapVisualKind = visualKind.Trim();
        record.MapVisualEndYear = Math.Max(0, year) + Math.Max(0, durationYears);
        NotifyMapMarkerDataChanged();
        MclslWorldArchiveStore.MarkDirty();
    }

    private static bool AlwaysKeepEvent(string eventType)
    {
        return eventType is "beast_tide" or "beast_tide_end"
            || string.Equals(eventType, "longevity_achieved", StringComparison.Ordinal)
            || string.Equals(eventType, "harmony_last_hit", StringComparison.Ordinal)
            || string.Equals(eventType, "manual_harmony_soul_claimed", StringComparison.Ordinal)
            || eventType.StartsWith("epoch_", StringComparison.Ordinal)
            || eventType.StartsWith("era_cycle_", StringComparison.Ordinal);
    }

    private static bool IsMilestoneEvent(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType)) return false;
        if (AlwaysKeepEvent(eventType)) return true;
        return eventType is "taishang_achieved"
            or "ancient_law_breakthrough"
            or "ancient_found_method"
            or "ancient_new_method_branch"
            or "ancient_lineage_flourish"
            or "ancient_lineage_ruin_born"
            or "ancient_remnant"
            or "ancient_remnant_secluded"
            or "epoch_wanxian_alliance_founded"
            or "epoch_five_elders_founded"
            or "epoch_mortal_miasma_truth"
            or "world_calamity_spirit_tide"
            or "world_calamity_lock_spirit"
            or "world_calamity_black_tide"
            or "world_calamity_white_mist"
            or "world_calamity_end_dharma"
            or "world_calamity_xuanhuang_terminal"
            or "sect_lifecycle_peak"
            or "sect_lifecycle_fall"
            or "sect_lifecycle_relic"
            or "world_soul_claimed"
            or "world_soul_killed"
            or "inverse_truth_reversed";
    }

    private static bool ShouldSuppressSilentEvent(string eventType, string title, string body)
    {
        string text = (eventType ?? string.Empty) + "|" + (title ?? string.Empty) + "|" + (body ?? string.Empty);
        return text.Contains("散功", StringComparison.Ordinal) || text.Contains("重修", StringComparison.Ordinal);
    }

    private static int EventTypeYearLimit(string eventType)
    {
        if (eventType.StartsWith("ancient_", StringComparison.Ordinal)) return 4;
        if (eventType.StartsWith("ruin_", StringComparison.Ordinal)) return 4;
        if (eventType.StartsWith("world_soul_", StringComparison.Ordinal)) return 4;
        if (eventType.StartsWith("faction_", StringComparison.Ordinal)) return 3;
        if (eventType == "law_conflict_death") return 4;
        if (eventType == "native_world_change" || eventType == "world_change") return 4;
        if (eventType.EndsWith("_death", StringComparison.Ordinal)) return 8;
        return 6;
    }

    internal static bool AddDiscovery(string knowledgeId, int year, string source)
    {
        if (string.IsNullOrWhiteSpace(knowledgeId)) return false;
        foreach (MclslKnowledgeDiscoveryRecord d in _current.Discoveries) if (d.KnowledgeId == knowledgeId) return false;
        MclslKnowledgeCatalog.TryGetKnowledge(knowledgeId, out MclslKnowledgeDefinition definition);
        int value = definition?.TruthValue ?? 1;
        _current.Discoveries.Add(new MclslKnowledgeDiscoveryRecord { KnowledgeId = knowledgeId, DiscoveryYear = year, SourceAnchorId = source ?? string.Empty, TruthValue = value });
        _current.RunTruthValue += value;
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static MclslTimelineAnchorState FindAnchor(string id)
    {
        foreach (MclslTimelineAnchorState state in _current.TimelineAnchors) if (state.AnchorId == id) return state;
        return null;
    }

    internal static MclslWorldCaveRecord FindCave(string id)
    {
        return MclslWorldRunRuntimeIndexes.FindCave(_current, id);
    }

    internal static MclslWorldChangeRecord FindWorldChange(string id)
    {
        return MclslWorldRunRuntimeIndexes.FindWorldChange(_current, id);
    }

    internal static MclslSectRuinRecord FindSectRuin(string id)
    {
        return MclslWorldRunRuntimeIndexes.FindSectRuin(_current, id);
    }

    internal static bool TryRegisterSectRuin(MclslSectRuinRecord ruin)
    {
        if (ruin == null || string.IsNullOrWhiteSpace(ruin.Id)) return false;
        _current.SectRuins ??= new List<MclslSectRuinRecord>();
        if (_current.SectRuins.Any(x => x != null && string.Equals(x.Id, ruin.Id, StringComparison.Ordinal))) return false;

        if (_current.SectRuins.Count >= MaxSectRuinRecords)
        {
            int removableIndex = -1;
            int oldestYear = int.MaxValue;
            for (int i = 0; i < _current.SectRuins.Count; i++)
            {
                MclslSectRuinRecord candidate = _current.SectRuins[i];
                if (!IsRetiredRuin(candidate) || candidate.BornYear >= oldestYear) continue;
                removableIndex = i;
                oldestYear = candidate.BornYear;
            }
            if (removableIndex < 0) return false;
            RemoveSectRuinAt(removableIndex);
        }

        _current.SectRuins.Add(ruin);
        NotifyMapMarkerDataChanged();
        MclslWorldRunRuntimeIndexes.Invalidate();
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void RegisterDeath(MclslDeathRecord record)
    {
        if (record == null || record.ActorId <= 0L || _current.DeathRecords.Any(x => x.ActorId == record.ActorId)) return;
        _current.DeathRecords.Add(record);
        TrimOldest(_current.DeathRecords, MaxDeaths);
        // 原版历史桥接先按既有重要性规则正常写入，再由新世界持续清理移走模组个人记录。
        RegisterDeathEvent(record);
        MclslWorldArchiveStore.MarkDirty();
        if (PersistentCleanupEnabled) ClearActorPersonalHistory(record.ActorId);
    }

    /// <summary>
    /// 新世界持续清理：清除某角色自己的模组个人编年、DeathRecords 与相关事件索引，
    /// 统一走 <see cref="UnindexEvent"/> 反索引入口，保证列表、计数与角色 UI 缓存一致。
    /// 不触碰家族成员名册、谱系与族史；重复调用不会重复改动任何索引（幂等）。
    /// </summary>
    internal static void ClearActorPersonalHistory(long actorId)
    {
        if (actorId <= 0L) return;
        EnsureEventIndexes();
        bool changed = false;
        if (_current.DeathRecords != null)
            changed |= _current.DeathRecords.RemoveAll(x => x != null && x.ActorId == actorId) > 0;
        List<MclslRunEventRecord> events = _current.Events;
        if (events != null)
        {
            for (int i = events.Count - 1; i >= 0; i--)
            {
                MclslRunEventRecord entry = events[i];
                if (entry == null || entry.ActorId != actorId) continue;
                UnindexEvent(entry);
                events.RemoveAt(i);
                changed = true;
            }
        }
        // Refresh the per-actor biography cache even when nothing matched.
        MclslCodexWindow.InvalidateActorHistory(actorId);
        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    private static void RegisterDeathEvent(MclslDeathRecord death)
    {
        _current.Events ??= new List<MclslRunEventRecord>();
        int safeYear = Math.Max(0, death.Year);
        string title = string.IsNullOrWhiteSpace(death.Title) ? death.ActorName + "陨落" : death.Title;
        string body = death.Announcement ?? string.Empty;
        EnsureEventIndexes();
        if (EventTitleKeys.ContainsKey((safeYear, "cultivator_death", title))
            || EventBodyKeys.ContainsKey((safeYear, "cultivator_death", body)))
            return;

        MclslRunEventRecord record = new()
        {
            Year = safeYear,
            EventType = "cultivator_death",
            Category = MclslEventCatalog.Death,
            Title = title,
            Body = body,
            ActorId = death.ActorId,
            ActorName = death.ActorName,
            MapX = death.MapX,
            MapY = death.MapY,
            LocationName = death.CityName,
            KingdomName = death.KingdomName
        };

        if (ShouldMirrorDeathToNativeHistory(death))
        {
            MclslRunEventRecord native = new()
            {
                Year = safeYear,
                EventType = "cultivator_death",
                Category = MclslEventCatalog.Death,
                Title = ShortNativeDeathTitle(death),
                Body = ShortNativeDeathBody(death),
                // 原版历史按 unit_id 或 x/y 提供“定位”；缺坐标时按钮不可用。
                ActorId = death.ActorId,
                MapX = death.MapX,
                MapY = death.MapY
            };
            record.NativeLogged = MclslNativeHistoryBridge.Add(native);
        }

        AppendEvent(record);
    }

    private static bool ShouldMirrorDeathToNativeHistory(MclslDeathRecord death)
    {
        if (death == null || !death.Surfaced) return false;
        if (string.Equals(death.CauseCode, "law_conflict", StringComparison.Ordinal)) return false;
        int configuredIndex = Math.Max(0, MclslRuntimeSettings.DeathPopupMinRealm - 1);
        return MclslRealmIds.Index(death.RealmId) >= configuredIndex;
    }

    private static string ShortNativeDeathTitle(MclslDeathRecord death)
    {
        string name = string.IsNullOrWhiteSpace(death?.ActorName) ? "无名修士" : death.ActorName;
        return name + "陨落";
    }

    private static string ShortNativeDeathBody(MclslDeathRecord death)
    {
        string realm = string.IsNullOrWhiteSpace(death?.RealmName) ? "道途" : death.RealmName;
        string cause = string.IsNullOrWhiteSpace(death?.CauseText) ? "身死" : death.CauseText;
        string place = string.IsNullOrWhiteSpace(death?.KingdomName) && string.IsNullOrWhiteSpace(death?.CityName)
            ? string.Empty
            : "于" + (string.IsNullOrWhiteSpace(death?.KingdomName) ? "无主之地" : death.KingdomName)
                + (string.IsNullOrWhiteSpace(death?.CityName) ? string.Empty : "·" + death.CityName);
        return place + "止步" + realm + "，" + cause + "。";
    }

    internal static void RegisterFactionMission(MclslFactionMissionRecord record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.Id)) return;
        _current.FactionMissions.Add(record);
        TrimOldest(_current.FactionMissions, MaxFactionMissions);
        if (record.FactionId?.StartsWith("ancient_", StringComparison.Ordinal) == true)
        {
            _current.AncientSectMissions ??= new List<MclslFactionMissionRecord>();
            _current.AncientSectMissions.Add(record);
        }
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void RegisterFactionPressure(MclslFactionPressureRecord record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.Id)) return;
        _current.FactionPressureEvents.Add(record);
        TrimOldest(_current.FactionPressureEvents, MaxFactionPressures);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void RegisterResourceSpend(MclslResourceSpendRecord record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.Id)) return;
        _current.ResourceSpendEvents.Add(record);
        TrimOldest(_current.ResourceSpendEvents, MaxResourceSpends);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void RegisterRuinExploration(MclslRuinExplorationRecord record)
    {
        if (record == null || string.IsNullOrWhiteSpace(record.Id)) return;
        _current.RuinExplorations.Add(record);
        TrimOldest(_current.RuinExplorations, MaxRuinExplorations);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static bool TryReserveGeneratedName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (!MclslWorldRunRuntimeIndexes.TryReserveGeneratedName(_current, name)) return false;
        TrimOldest(_current.UsedGeneratedNames, MaxUsedGeneratedNames);
        MclslWorldRunRuntimeIndexes.Invalidate();
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static int NextProceduralSequence()
    {
        _current.ProceduralSequence = Math.Max(0, _current.ProceduralSequence) + 1;
        MclslWorldArchiveStore.MarkDirty();
        return _current.ProceduralSequence;
    }

    internal static void RegisterGeneratedItem(MclslGeneratedItemRecord item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Id)) return;
        if (MclslWorldRunRuntimeIndexes.HasGeneratedItem(_current, item.Id)) return;
        _current.GeneratedItems.Add(item);
        TrimOldest(_current.GeneratedItems, MaxGeneratedItems);
        MclslWorldRunRuntimeIndexes.RegisterGeneratedName(_current, item.Name);
        TrimOldest(_current.UsedGeneratedNames, MaxUsedGeneratedNames);
        MclslWorldRunRuntimeIndexes.Invalidate();
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void EnsureAnchors()
    {
        // Timeline anchors are only ever appended; no code path removes one. A
        // completed pass therefore stays complete for this run and cannot skip
        // work, because the loop below only adds definitions that are missing.
        if (ReferenceEquals(_anchorsEnsuredRun, _current)) return;
        foreach (MclslTimelineAnchorDefinition definition in MclslKnowledgeCatalog.TimelineAnchors)
        {
            if (FindAnchor(definition.Id) != null) continue;
            int min = Math.Min(definition.EarliestYear, definition.LatestYear);
            int max = Math.Max(definition.EarliestYear, definition.LatestYear);
            int hash = MclslStableHash.Stable17x31(_current.RunId + "|" + definition.Id);
            int canonicalYear = min + (hash & int.MaxValue) % Math.Max(1, max - min + 1);
            int scheduledYear = ScheduleTimelineAnchorYear(canonicalYear);
            _current.TimelineAnchors.Add(new MclslTimelineAnchorState { AnchorId = definition.Id, ScheduledYear = scheduledYear });
        }
        _anchorsEnsuredRun = _current;
    }

    private static int ScheduleTimelineAnchorYear(int canonicalYear)
    {
        const int canonicalAncientLawEnd = 1000;
        const int canonicalNewLawStart = canonicalAncientLawEnd + MclslWorldEpochSystem.TransmissionDurationYears;
        int transmissionYear = Math.Max(1, MclslWorldEpochSystem.AncientLawDurationYears);
        if (canonicalYear <= canonicalAncientLawEnd)
        {
            int ancientOffset = Math.Max(1, (int)Math.Round(canonicalYear * (transmissionYear / (double)canonicalAncientLawEnd)));
            return _current.StartYear + MclslRuntimeSettings.ScaleTimelineYear(ancientOffset);
        }

        int newLawOffset = Math.Max(0, canonicalYear - canonicalNewLawStart);
        int scaledOffset = (int)Math.Round(newLawOffset * (transmissionYear / (double)canonicalAncientLawEnd));
        return Math.Max(
            _current.NewLawStartYear + 1,
            _current.NewLawStartYear + MclslRuntimeSettings.ScaleTimelineYear(Math.Max(1, scaledOffset)));
    }

    private static void EnsureAncientTechniqueSeeds(int year)
    {
        if (_current?.TechniqueLineages == null) return;
        // The pass runs on every call exactly as before: it seeds definitions that
        // have no lineage record yet and repairs empty fields. Only the lookup
        // shape changed - one reused id index instead of a linear scan with a
        // LINQ closure per definition - so removing a seeded record is still
        // repaired on the very next call.
        Dictionary<string, MclslTechniqueLineageRecord> existing = SeedLineageIndex;
        existing.Clear();
        for (int i = 0; i < _current.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord record = _current.TechniqueLineages[i];
            if (record?.Id == null || existing.ContainsKey(record.Id)) continue;
            existing[record.Id] = record;
        }
        int yuanYingIndex = MclslRealmIds.Index(MclslRealmIds.YuanYing);
        bool changed = false;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition definition = MclslCultivationCatalog.Techniques[i];
            if (definition == null) continue;
            int maxIndex = MclslRealmIds.Index(definition.MaxRealm);
            if (maxIndex < 0 || maxIndex > yuanYingIndex) continue;
            string id = MclslCultivationCatalog.NormalizeTechniqueId(definition.Id);
            if (existing.TryGetValue(id, out MclslTechniqueLineageRecord found))
            {
                bool detailsChanged = false;
                if (string.IsNullOrWhiteSpace(found.Name)) { found.Name = definition.Name; changed = true; detailsChanged = true; }
                if (string.IsNullOrWhiteSpace(found.SystemId)) { found.SystemId = MclslCultivationSystemIds.AncientLaw; changed = true; detailsChanged = true; }
                if (string.IsNullOrWhiteSpace(found.LawTags)) { found.LawTags = string.Join(",", definition.LawPool); changed = true; detailsChanged = true; }
                if (string.IsNullOrWhiteSpace(found.MaxRealm)) { found.MaxRealm = definition.MaxRealm; changed = true; detailsChanged = true; }
                if (string.IsNullOrWhiteSpace(found.Summary)) { found.Summary = AncientTechniqueSummary(definition); changed = true; }
                if (string.Equals(found.FounderName, "上古散修", StringComparison.Ordinal)) { found.FounderName = string.Empty; changed = true; }
                if (detailsChanged) MclslFamilySystem.NotifyTechniqueLineageChanged(found);
                continue;
            }

            MclslTechniqueLineageRecord created = new()
            {
                Id = id,
                Name = definition.Name,
                SystemId = MclslCultivationSystemIds.AncientLaw,
                LawTags = string.Join(",", definition.LawPool),
                MaxRealm = definition.MaxRealm,
                FirstSeenYear = Math.Max(0, year),
                LastSeenYear = Math.Max(0, year),
                CurrentPractitioners = 0,
                PeakPractitioners = 0,
                PeakRealm = MclslRealmIds.Mortal,
                FounderName = string.Empty,
                State = "散藏",
                SourceTechniqueId = id,
                Summary = AncientTechniqueSummary(definition)
            };
            _current.TechniqueLineages.Add(created);
            MclslFamilySystem.NotifyTechniqueLineageChanged(created);
            // Two definitions can normalize to the same id; the second one must
            // take the repair path exactly like the old linear lookup did.
            existing[id] = created;
            changed = true;
        }
        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    private static string AncientTechniqueSummary(MclslTechniqueDefinition definition)
    {
        string tags = definition?.LawPool == null || definition.LawPool.Length == 0
            ? "灵根吐纳"
            : string.Join("、", definition.LawPool);
        return "此法以" + tags + "为本，可修至" + MclslRealmIds.Display(definition?.MaxRealm) + "。";
    }

    private static void EnsureFutureCatalogs(bool tracePersistence)
    {
        long sample = MclslPerformanceProbe.Begin();
        try { EnsureFutureCatalogsCore(tracePersistence); }
        finally { MclslPerformanceProbe.End("档案.EnsureFutureCatalogs", sample); }
    }

    private static void EnsureFutureCatalogsCore(bool tracePersistence)
    {
        MclslWorldSoulRecord[] baseSoulDefinitions =
        {
            new() { Id="soul_attr_metal", Name="金魄", HeavenlyDuty="维系金行秩序，裁断扰乱天地者", LawTags="金,锋锐,秩序", Quality=4 },
            new() { Id="soul_attr_wood", Name="木魄", HeavenlyDuty="修复被抽夺的生机，催发生生之理", LawTags="木,生机,繁衍", Quality=4 },
            new() { Id="soul_attr_water", Name="水魄", HeavenlyDuty="调和水脉寒暑，平复泽国失衡", LawTags="水,寒,流转", Quality=4 },
            new() { Id="soul_attr_fire", Name="火魄", HeavenlyDuty="焚尽失控灵脉，镇压火行反噬", LawTags="火,燃烧,毁灭", Quality=4 },
            new() { Id="soul_attr_earth", Name="土魄", HeavenlyDuty="稳定大陆地脉，封镇山河裂隙", LawTags="土,稳定,封镇", Quality=4 },
            new() { Id="soul_attr_wind", Name="风魄", HeavenlyDuty="平复界域乱流，牵引风行周转", LawTags="风,速度,流转", Quality=4 },
            new() { Id="soul_attr_thunder", Name="雷魄", HeavenlyDuty="惩戒掠天修士，震散天地积怨", LawTags="雷,惩戒,毁灭", Quality=4 },
            new() { Id="soul_attr_yin", Name="阴魄", HeavenlyDuty="收束幽暗阴寒，抹平失控隐秘", LawTags="阴,寒,隐匿", Quality=4 },
            new() { Id="soul_attr_yang", Name="阳魄", HeavenlyDuty="照破沉浊晦暗，复苏天地阳和", LawTags="阳,光,生机", Quality=4 },
            new() { Id="soul_attr_space", Name="空魄", HeavenlyDuty="填补虚空溶洞，封合界域裂痕", LawTags="空间,迁跃,封镇", Quality=4 }
        };
        MclslWorldSoulRecord[] traceSoulDefinitions =
        {
            new() { Id="soul_trace_ash", Name="烬魄", HeavenlyDuty="收束毁灭余烬，留存劫后道痕", LawTags="火,毁灭,余烬", Quality=3 },
            new() { Id="soul_trace_tide", Name="潮魄", HeavenlyDuty="回收消散水脉，复续流转道痕", LawTags="水,流转,生机", Quality=3 },
            new() { Id="soul_trace_star", Name="陨魄", HeavenlyDuty="铭刻天坠星痕，重整金石秩序", LawTags="金,土,空间", Quality=3 },
            new() { Id="soul_trace_shadow", Name="影魄", HeavenlyDuty="封存幽暗残影，约束隐秘外溢", LawTags="阴,隐匿,封镇", Quality=3 },
            new() { Id="soul_trace_root", Name="根魄", HeavenlyDuty="护持枯荣余根，使废土重生", LawTags="木,生机,稳定", Quality=3 }
        };
        MclslWorldSoulRecord[] soulDefinitions = tracePersistence
            ? baseSoulDefinitions.Concat(traceSoulDefinitions).ToArray()
            : baseSoulDefinitions;
        _current.WorldSouls.RemoveAll(x => x == null || !soulDefinitions.Any(d => d.Id == x.Id));
        for (int i = 0; i < soulDefinitions.Length; i++)
        {
            MclslWorldSoulRecord definition = soulDefinitions[i];
            MclslWorldSoulRecord existing = _current.WorldSouls.FirstOrDefault(x => x?.Id == definition.Id);
            if (existing == null)
            {
                definition.NextManifestYear = Math.Max(_current.StartYear + 120 + i * 45, _current.LastProcessedYear + 30 + i * 15);
                _current.WorldSouls.Add(definition);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(existing.Name)) existing.Name = definition.Name;
                if (string.IsNullOrWhiteSpace(existing.HeavenlyDuty)) existing.HeavenlyDuty = definition.HeavenlyDuty;
                if (string.IsNullOrWhiteSpace(existing.LawTags)) existing.LawTags = definition.LawTags;
                if (existing.Quality <= 0) existing.Quality = definition.Quality;
            }
        }

        MclslInverseTruthRecord[] canonTruthDefinitions =
        {
            CanonTruth("truth_chuanfa_new_law", "传法新法", "传法天尊定理：世间生灵必须遵循传法天尊建立的修行之法。作为原著既有逆理，只还原世界规则，不占本世长生席位。"),
            CanonTruth("truth_one_heart", "万众一心", "一心天尊·赵若曦逆理：独立个体可共享记忆、感知、思想与行动，趋向同一整体。作为五老会既有背景规则，不占本世长生席位。"),
            CanonTruth("truth_wuyou", "忘忧而乐", "无忧天尊逆理：世间生灵忘却忧愁与痛苦，记忆、情绪、梦境与现实边界被改写。作为原著既有逆理，不占本世长生席位。"),
            CanonTruth("truth_wangsheng", "逆转死生", "往生天尊逆理：死去修士可在一定时间后复活并保留前世记忆；次数增加后代价加重。作为原著既有逆理，不占本世长生席位。"),
            CanonTruth("truth_mortal_miasma", "仙凡瘴之理", "白先生留下的逆理：凡俗携仙凡瘴，反能克制、侵蚀乃至诛灭高高在上的修士。作为原著既有逆理，不占本世长生席位。"),
            CanonTruth("truth_human_will", "改天换地，人人可为", "人道天尊方向：凡人亦可凭信念与勇气化作星辰，永久改变天地。作为原著既有逆理，不占本世长生席位。"),
            CanonTruth("truth_true_unreal", "真假之变", "真实天尊方向：虚假、幻象或可能发生之事，可以被赋予真实，使其成为既定事实。作为原著既有逆理，不占本世长生席位。")
        };
        MclslInverseTruthRecord[] playerTruthPool =
        {
            PlayerTruth("truth_player_many_paths", "法同异途", "本世可证逆理：同法可以分道，同源未必同途。已逆后，同一功法多人修持时的法压下降，但不会彻底删除法不可同修。"),
            PlayerTruth("truth_player_failure_steps", "败痕为阶", "本世可证逆理：今日之败，亦可成为明日登天之阶。已逆后，元婴炼化与化神抽髓失败会留下败痕，保留后续再试的优势。"),
            PlayerTruth("truth_player_wounds_forge_body", "伤可铸身", "本世可证逆理：凡未能杀我者，皆为铸我之锤。已逆后，不同境界会获得更高原生等级下限，使生命与基础战斗成长更稳定。"),
            PlayerTruth("truth_player_disaster_chance", "劫生灵机", "本世可证逆理：大劫所过之处，亦有天地新机。已逆后，陨石、风暴、地震、雷火、爆裂、疫病与大战等灾厄更容易凝成天地之变。"),
            PlayerTruth("truth_player_weak_not_fixed", "强弱非定", "本世可证逆理：强弱乃一时之势，并非天地定数。已逆后，低境或凡俗夺得天地之魄时，合道稳定更高。"),
            PlayerTruth("truth_player_trace_persistence", "道痕常在", "本世可证逆理：形虽灭，道痕不散。已逆后，洞天、天地之变与遗迹的可用次数增加，并会唤生五道额外天地之魄。"),
            PlayerTruth("truth_player_lifespan_drain", "寿元可夺", "本世可证逆理：他人余岁，可续我命。已逆后，修士可吸取他人寿元，但增益不超过当前境界寿元上限的一半。"),
            PlayerTruth("truth_player_duty_not_fixed", "职非天定", "本世可证逆理：职由行定，而非由天独断。已逆后，合道者承接天职时反噬更轻，履职更易稳定。"),
            PlayerTruth("truth_player_flawed_dao", "大道有缺", "本世可证逆理：大道本非完满，败处亦有余路。已逆后，只影响传法新法的破境劫数，使失败率降低。"),
            PlayerTruth("truth_player_all_laws_one", "万法归一", "本世可证逆理：万法异名，终可同归。已逆后，新法功法档案会低频融合相近法脉，修士功法感悟增长更稳。"),
            PlayerTruth("truth_player_reincarnation_unbroken", "轮回不灭", "本世可证逆理：身灭而识不尽。已逆后，高境修士身陨时有概率转世，以更高灵根资质重踏修行。")
        };
        MclslInverseTruthRecord[] playerTruthDefinitions = playerTruthPool
            .OrderBy(x => MclslStableHash.Stable17x31(_current.RunId + "|player_truth|" + x.Id))
            .Take(5)
            .ToArray();
        MclslInverseTruthRecord[] allTruthDefinitions = canonTruthDefinitions.Concat(playerTruthDefinitions).ToArray();
        _current.InverseTruths.RemoveAll(x => x == null || !allTruthDefinitions.Any(d => d.Id == x.Id));
        foreach (MclslInverseTruthRecord definition in allTruthDefinitions)
        {
            MclslInverseTruthRecord existing = _current.InverseTruths.FirstOrDefault(x => x?.Id == definition.Id);
            if (existing == null) _current.InverseTruths.Add(definition);
            else
            {
                existing.Name = definition.Name;
                existing.RuleDescription = definition.RuleDescription;
                existing.Category = definition.Category;
                existing.CountsTowardLongevity = definition.CountsTowardLongevity;
                if (!definition.CountsTowardLongevity)
                {
                    existing.Reversed = true;
                    existing.Progress = 100;
                    existing.ChallengerActorId = 0;
                }
            }
        }
    }

    private static MclslInverseTruthRecord CanonTruth(string id, string name, string description) => new()
    {
        Id = id,
        Name = name,
        RuleDescription = description,
        Category = "canon",
        CountsTowardLongevity = false,
        Progress = 100,
        Reversed = true
    };

    private static MclslInverseTruthRecord PlayerTruth(string id, string name, string description) => new()
    {
        Id = id,
        Name = name,
        RuleDescription = description,
        Category = "player",
        CountsTowardLongevity = true
    };

    private static void NormalizeCurrentRun()
    {
        if (ReferenceEquals(_normalizedRun, _current)) return;
        Normalize();
        _normalizedRun = _current;
    }

    private static void InvalidateCurrentRunCaches()
    {
        _normalizedRun = null;
        _ensuredRun = null;
        _ensuredYear = -1;
        _anchorsEnsuredRun = null;
        SeedLineageIndex.Clear();
        _futureCatalogsRun = null;
        _materialAwardKeysRun = null;
        _materialAwardKeyIndex = null;
        InvalidateEventIndexes();
        _futureCatalogsTracePersistence = false;
        _futureCatalogsLastMarkedYear = -1;
    }

    private static bool IsTracePersistenceReversed()
    {
        List<MclslInverseTruthRecord> truths = _current?.InverseTruths;
        if (truths == null) return false;
        for (int i = 0; i < truths.Count; i++)
        {
            MclslInverseTruthRecord truth = truths[i];
            if (truth != null && truth.Id == "truth_player_trace_persistence" && truth.Reversed) return true;
        }
        return false;
    }

    private static void Normalize()
    {
        long sample = MclslPerformanceProbe.Begin();
        try { NormalizeCore(); }
        finally { MclslPerformanceProbe.End("档案.Normalize", sample); }
    }

    private static void NormalizeCore()
    {
        _current ??= new MclslWorldRunState();
        // 缺少清理标记的既有 v24 存档解析为关闭，并在本次保存后固化为 false。
        if (_current.PersistentCleanupEnabled == null) _current.PersistentCleanupEnabled = false;
        _current.Economy ??= new MclslEconomyState();
        _current.BackgroundFactions ??= new MclslBackgroundFactionState();
        _current.JulingMarrowMetrics ??= new Dictionary<string, long>();
        _current.FiredHistoricalEvents ??= new List<string>();
        _current.PendingAncientCultivatorIds ??= new List<string>();
        if (_current.EraOriginYear <= 0) _current.EraOriginYear = Math.Max(0, _current.StartYear);
        if (_current.AncientLawEndYear <= 0) _current.AncientLawEndYear = _current.EraOriginYear + MclslWorldEpochSystem.AncientLawDurationYears;
        if (_current.TransmissionEndYear <= 0) _current.TransmissionEndYear = _current.AncientLawEndYear + MclslWorldEpochSystem.TransmissionDurationYears;
        if (_current.NewLawStartYear <= 0) _current.NewLawStartYear = _current.TransmissionEndYear;
        if (string.IsNullOrWhiteSpace(_current.CultivationEpoch))
            _current.CultivationEpoch = _current.NewLawEnabled ? MclslWorldEpochSystem.TransmissionTransitionEpoch : MclslWorldEpochSystem.AncientLawEpoch;
        _current.CurrentBackgroundEraId ??= string.Empty;
        _current.CurrentBackgroundEraName ??= string.Empty;
        _current.CurrentWorldCalamityId ??= string.Empty;
        _current.CurrentWorldCalamityName ??= string.Empty;
        _current.InheritedKnowledgeIds ??= new List<string>();
        _current.Discoveries ??= new List<MclslKnowledgeDiscoveryRecord>();
        _current.TimelineAnchors ??= new List<MclslTimelineAnchorState>();
        _current.Events ??= new List<MclslRunEventRecord>();
        _current.MaterialAwardEventKeys ??= new List<string>();
        TrimOldest(_current.MaterialAwardEventKeys, MaxMaterialAwardEventKeys);
        TrimEventsPreservingMilestones(_current.Events, MaxEvents, force: true);
        foreach (MclslRunEventRecord e in _current.Events)
        {
            if (e == null) continue;
            if (string.Equals(e.EventType, "beast_tide", StringComparison.Ordinal)
                || string.Equals(e.EventType, "beast_tide_end", StringComparison.Ordinal))
                e.Category = MclslEventCatalog.BeastTide;
            else if (string.Equals(e.EventType, "material_discovery", StringComparison.Ordinal)
                || string.Equals(e.EventType, "item_acquisition", StringComparison.Ordinal))
                e.Category = MclslEventCatalog.TreasureAcquisition;
            else if (string.IsNullOrWhiteSpace(e.Category)) e.Category = MclslEventCatalog.CategoryForType(e.EventType);
            e.MapVisualKind ??= string.Empty;
            if (string.IsNullOrWhiteSpace(e.MapVisualKind)) e.MapVisualEndYear = -1;
        }
        _current.DeathRecords ??= new List<MclslDeathRecord>();
        _current.FactionMissions ??= new List<MclslFactionMissionRecord>();
        _current.AncientSectMissions ??= new List<MclslFactionMissionRecord>();
        _current.FactionPressureEvents ??= new List<MclslFactionPressureRecord>();
        _current.ResourceSpendEvents ??= new List<MclslResourceSpendRecord>();
        _current.TechniqueLineages ??= new List<MclslTechniqueLineageRecord>();
        foreach (MclslTechniqueLineageRecord lineage in _current.TechniqueLineages)
        {
            if (lineage == null) continue;
            lineage.SectDisplayName ??= string.Empty;
            lineage.LifecycleState ??= string.Empty;
            lineage.Summary ??= string.Empty;
            if (lineage.Completeness <= 0) lineage.Completeness = 75;
        }
        _current.SectRuins ??= new List<MclslSectRuinRecord>();
        _current.RuinExplorations ??= new List<MclslRuinExplorationRecord>();
        _current.WorldCaves ??= new List<MclslWorldCaveRecord>();
        _current.WorldChanges ??= new List<MclslWorldChangeRecord>();
        _current.WorldSouls ??= new List<MclslWorldSoulRecord>();
        _current.InverseTruths ??= new List<MclslInverseTruthRecord>();
        _current.ReincarnationRecords ??= new List<MclslActorReincarnationRecord>();
        TrimSectRuinsToLimit();
        TrimWorldCavesToLimit();
        TrimWorldChangesToLimit();
        TrimReincarnationRecordsToLimit();
        TrimOldest(_current.DeathRecords, MaxDeaths);
        TrimOldest(_current.FactionMissions, MaxFactionMissions);
        TrimOldest(_current.FactionPressureEvents, MaxFactionPressures);
        TrimOldest(_current.ResourceSpendEvents, MaxResourceSpends);
        TrimOldest(_current.RuinExplorations, MaxRuinExplorations);
        foreach (MclslInverseTruthRecord truth in _current.InverseTruths)
        {
            if (truth == null) continue;
            truth.Category = string.IsNullOrWhiteSpace(truth.Category) ? "player" : truth.Category;
            if (truth.Category == "canon")
            {
                truth.CountsTowardLongevity = false;
                truth.Reversed = true;
                truth.Progress = 100;
                truth.ChallengerActorId = 0;
            }
        }
        _current.UsedGeneratedNames ??= new List<string>();
        _current.GeneratedItems ??= new List<MclslGeneratedItemRecord>();
        _current.TianxuanListings ??= new List<MclslMarketListing>();
        _current.PendingArtifactListings ??= new Queue<MclslMarketListing>();
        _current.TianxuanActivities ??= new List<MclslMarketActivity>();
        _current.PendingMarketPurchases ??= new Dictionary<long, MclslMarketPurchaseBatch>();
        foreach (long actorId in _current.PendingMarketPurchases.Keys.ToArray())
            if (actorId <= 0 || _current.PendingMarketPurchases[actorId] is not { Year: > 0 })
                _current.PendingMarketPurchases.Remove(actorId);
        _current.TianxuanActivities.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.ItemId));
        TrimOldest(_current.TianxuanActivities, MaxTianxuanActivities);
        TrimOldest(_current.UsedGeneratedNames, MaxUsedGeneratedNames);
        TrimOldest(_current.GeneratedItems, MaxGeneratedItems);
        foreach (MclslWorldSoulRecord soul in _current.WorldSouls)
        {
            if (soul == null) continue;
            if (soul.Quality <= 0) soul.Quality = 3;
            soul.State = string.IsNullOrWhiteSpace(soul.State) ? "沉寂" : soul.State;
            soul.Name ??= string.Empty;
            soul.HeavenlyDuty ??= string.Empty;
            soul.LawTags ??= string.Empty;
            soul.ManifestActorName ??= string.Empty;
            soul.HolderActorName ??= string.Empty;
            soul.HolderOrigin ??= string.Empty;
            soul.LocationName ??= "天地之间";
            soul.NativeKingdomName ??= "无主";
            if (string.IsNullOrWhiteSpace(soul.NativeTerrainEffect))
                soul.NativeTerrainEffect = MclslNativeTerrainProfileCatalog.ForSoul(MclslGeneratedObjectFactory.SplitTags(soul.LawTags), soul.HeavenlyDuty);
        }
        foreach (MclslWorldChangeRecord change in _current.WorldChanges)
        {
            if (change == null) continue;
            change.LawTags ??= string.Empty;
            change.SourceType ??= "world_event";
            if (string.IsNullOrWhiteSpace(change.NativeTerrainEffect))
                change.NativeTerrainEffect = MclslNativeTerrainProfileCatalog.ForTags(MclslGeneratedObjectFactory.SplitTags(change.LawTags), change.SourceType).Summary;
        }
        HashSet<string> normalizedGeneratedNames = new(_current.UsedGeneratedNames, StringComparer.Ordinal);
        foreach (MclslWorldCaveRecord cave in _current.WorldCaves)
            if (!string.IsNullOrWhiteSpace(cave.Name) && normalizedGeneratedNames.Add(cave.Name)) _current.UsedGeneratedNames.Add(cave.Name);
        foreach (MclslGeneratedItemRecord item in _current.GeneratedItems)
            if (!string.IsNullOrWhiteSpace(item.Name) && normalizedGeneratedNames.Add(item.Name)) _current.UsedGeneratedNames.Add(item.Name);
        foreach (MclslSectRuinRecord ruin in _current.SectRuins)
            if (!string.IsNullOrWhiteSpace(ruin.Name) && normalizedGeneratedNames.Add(ruin.Name)) _current.UsedGeneratedNames.Add(ruin.Name);
        foreach (MclslWorldChangeRecord change in _current.WorldChanges)
            if (!string.IsNullOrWhiteSpace(change.Name) && normalizedGeneratedNames.Add(change.Name)) _current.UsedGeneratedNames.Add(change.Name);
        _current.UsesNativeKingdoms = true;
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    internal static MclslWorldSoulRecord FindWorldSoulById(string soulId)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.WorldSouls == null || string.IsNullOrWhiteSpace(soulId)) return null;
        for (int i = 0; i < run.WorldSouls.Count; i++)
        {
            MclslWorldSoulRecord soul = run.WorldSouls[i];
            if (soul != null && soul.Id == soulId) return soul;
        }
        return null;
    }
}

internal sealed class ApplyMapVisualToEventArgs
{
    internal MclslRunEventRecord Record;
    internal int Year;
    internal int Mapx;
    internal int Mapy;
    internal string Locationname;
    internal string Kingdomname;
    internal string Visualkind;
    internal int Durationyears;
}
