using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslWorldRunRepository
{
    private const int MaxEvents = 400;
    private const int MaxDeaths = 600;
    private const int MaxFactionMissions = 300;
    private const int MaxFactionPressures = 180;
    private const int MaxResourceSpends = 300;
    private const int MaxRuinExplorations = 800;
    private const int MaxGeneratedItems = 1000;
    private const int MaxUsedGeneratedNames = 2400;
    internal const int MaxSectRuinRecords = 49;
    internal const int SectRuinRecoveryFloor = 10;
    private const int MaxWorldCaveRecords = 64;
    private const int MaxWorldChangeRecords = 64;
    private const int MaxReincarnationRecords = 200;
    private const int MaxEventsPerYear = 36;
    private static MclslWorldRunState _current = new();
    internal static MclslWorldRunState Current => _current;

    internal static void EnsureCurrentRun(int year)
    {
        Normalize();
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
                NextCaveBirthYear = Math.Max(0, year) + 80
            };
            int carryLimit = Math.Max(MclslRuntimeSettings.CarrySlotLimit, profile.CarrySlotLimit);
            int limit = Math.Min(profile.KnownKnowledgeIds.Count, carryLimit);
            for (int i = 0; i < limit; i++) _current.InheritedKnowledgeIds.Add(profile.KnownKnowledgeIds[i]);
        }
        EnsureAnchors();
        EnsureAncientTechniqueSeeds(year);
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
        Normalize();
        if (string.IsNullOrWhiteSpace(_current.RunId))
            EnsureCurrentRun(year);
        EnsureFutureCatalogs();
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static MclslWorldArchiveBundle ExportArchive()
    {
        Normalize();
        return JsonConvert.DeserializeObject<MclslWorldArchiveBundle>(JsonConvert.SerializeObject(new MclslWorldArchiveBundle { Version = MclslWorldArchiveMigration.CurrentVersion, CurrentRun = _current })) ?? new();
    }

    internal static bool ImportArchive(MclslWorldArchiveBundle bundle)
    {
        bool changed = MclslWorldArchiveMigration.Upgrade(bundle, out MclslWorldArchiveBundle upgraded);
        _current = upgraded?.CurrentRun ?? new();
        Normalize();
        MclslWorldRunRuntimeIndexes.Invalidate();
        return changed;
    }

    private static void TrimSectRuinsToLimit()
    {
        if (_current.SectRuins == null) return;
        _current.SectRuins.RemoveAll(x => x == null || string.IsNullOrWhiteSpace(x.Id));
        if (_current.SectRuins.Count <= MaxSectRuinRecords) return;

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
        caveIds = new HashSet<string>(StringComparer.Ordinal);
        changeIds = new HashSet<string>(StringComparer.Ordinal);
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        if (actors == null) return;
        for (int i = 0; i < actors.Count; i++)
        {
            Actor actor = actors[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            string caveId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentCaveId, string.Empty);
            string changeId = MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineChangeId, string.Empty);
            if (!string.IsNullOrWhiteSpace(caveId)) caveIds.Add(caveId);
            if (!string.IsNullOrWhiteSpace(changeId)) changeIds.Add(changeId);
        }
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
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static void TrimOldest<T>(List<T> list, int max)
    {
        if (list == null || max <= 0 || list.Count <= max) return;
        list.RemoveRange(0, list.Count - max);
    }

    private static void TrimEventsPreservingMilestones(List<MclslRunEventRecord> list, int max)
    {
        if (list == null || max <= 0 || list.Count <= max) return;
        int nonMilestoneCount = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (!IsMilestoneEvent(list[i]?.EventType)) nonMilestoneCount++;
        }

        int removeCount = Math.Max(0, nonMilestoneCount - max);
        for (int i = 0; i < list.Count && removeCount > 0;)
        {
            MclslRunEventRecord record = list[i];
            if (record == null || !IsMilestoneEvent(record.EventType))
            {
                list.RemoveAt(i);
                removeCount--;
                continue;
            }
            i++;
        }
    }

    internal static void AddEvent(int year, string type, string title, string body)
    {
        _current.Events ??= new List<MclslRunEventRecord>();
        string eventType = type ?? string.Empty;
        int safeYear = Math.Max(0, year);
        string safeTitle = title ?? string.Empty;
        string safeBody = body ?? string.Empty;
        if (ShouldSuppressSilentEvent(eventType, safeTitle, safeBody)) return;
        if (!AlwaysKeepEvent(eventType))
        {
            int yearCount = 0;
            int typeCount = 0;
            int typeLimit = EventTypeYearLimit(eventType);
            for (int i = _current.Events.Count - 1; i >= 0; i--)
            {
                MclslRunEventRecord existing = _current.Events[i];
                if (existing == null || existing.Year != safeYear) continue;
                yearCount++;
                if (string.Equals(existing.EventType, eventType, StringComparison.Ordinal)) typeCount++;
                if (yearCount >= MaxEventsPerYear || typeCount >= typeLimit) return;
            }
        }
        if (_current.Events.Any(x => x.Year == safeYear
            && string.Equals(x.EventType, eventType, StringComparison.Ordinal)
            && (string.Equals(x.Title, safeTitle, StringComparison.Ordinal) || string.Equals(x.Body, safeBody, StringComparison.Ordinal))))
            return;
        string category = MclslEventCatalog.CategoryForType(eventType);
        MclslRunEventRecord record = new()
        {
            Year = safeYear,
            EventType = eventType,
            Category = category,
            Title = safeTitle,
            Body = safeBody
        };
        if (MclslEventCatalog.ShouldMirrorToNativeHistory(eventType))
            record.NativeLogged = MclslNativeHistoryBridge.Add(record);
        _current.Events.Add(record);
        TrimEventsPreservingMilestones(_current.Events, MaxEvents);
        MclslWorldArchiveStore.MarkDirty();
    }

    internal static void AddEvent(int year, string type, string title, string body, Actor actor)
    {
        AddEvent(year, type, title, body);
        if (actor?.data == null || _current?.Events == null || _current.Events.Count == 0) return;
        int safeYear = Math.Max(0, year);
        string safeType = type ?? string.Empty;
        string safeTitle = title ?? string.Empty;
        string safeBody = body ?? string.Empty;
        MclslRunEventRecord record = null;
        for (int i = _current.Events.Count - 1; i >= 0; i--)
        {
            MclslRunEventRecord candidate = _current.Events[i];
            if (candidate == null || candidate.Year != safeYear) continue;
            if (!string.Equals(candidate.EventType, safeType, StringComparison.Ordinal)) continue;
            if (!string.Equals(candidate.Title, safeTitle, StringComparison.Ordinal)) continue;
            if (!string.Equals(candidate.Body, safeBody, StringComparison.Ordinal)) continue;
            record = candidate;
            break;
        }
        if (record == null) return;
        try
        {
            record.ActorId = MclslActorAccessor.Id(actor);
            record.ActorName = MclslActorAccessor.DisplayName(actor);
            record.MapX = actor.data.x;
            record.MapY = actor.data.y;
            record.LocationName = actor.city?.data?.name ?? string.Empty;
            record.KingdomName = actor.kingdom?.data?.name ?? string.Empty;
            MclslWorldArchiveStore.MarkDirty();
        }
        catch { }
    }

    internal static void AddEvent(int year, string type, string title, string body, int mapX, int mapY, string locationName, string kingdomName)
    {
        AddEvent(year, type, title, body);
        if (_current?.Events == null || _current.Events.Count == 0) return;
        int safeYear = Math.Max(0, year);
        string safeType = type ?? string.Empty;
        string safeTitle = title ?? string.Empty;
        string safeBody = body ?? string.Empty;
        for (int i = _current.Events.Count - 1; i >= 0; i--)
        {
            MclslRunEventRecord record = _current.Events[i];
            if (record == null || record.Year != safeYear) continue;
            if (!string.Equals(record.EventType, safeType, StringComparison.Ordinal)
                || !string.Equals(record.Title, safeTitle, StringComparison.Ordinal)
                || !string.Equals(record.Body, safeBody, StringComparison.Ordinal)) continue;
            record.MapX = mapX;
            record.MapY = mapY;
            record.LocationName = locationName ?? string.Empty;
            record.KingdomName = kingdomName ?? string.Empty;
            MclslWorldArchiveStore.MarkDirty();
            return;
        }
    }

    private static bool AlwaysKeepEvent(string eventType)
    {
        return string.Equals(eventType, "longevity_achieved", StringComparison.Ordinal)
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
        MclslWorldRunRuntimeIndexes.Invalidate();
        MclslWorldArchiveStore.MarkDirty();
        return true;
    }

    internal static void RegisterDeath(MclslDeathRecord record)
    {
        if (record == null || record.ActorId <= 0L || _current.DeathRecords.Any(x => x.ActorId == record.ActorId)) return;
        _current.DeathRecords.Add(record);
        TrimOldest(_current.DeathRecords, MaxDeaths);
        RegisterDeathEvent(record);
        MclslWorldArchiveStore.MarkDirty();
    }

    private static void RegisterDeathEvent(MclslDeathRecord death)
    {
        _current.Events ??= new List<MclslRunEventRecord>();
        int safeYear = Math.Max(0, death.Year);
        string title = string.IsNullOrWhiteSpace(death.Title) ? death.ActorName + "陨落" : death.Title;
        string body = death.Announcement ?? string.Empty;
        if (_current.Events.Any(x => x.Year == safeYear
            && string.Equals(x.EventType, "cultivator_death", StringComparison.Ordinal)
            && (string.Equals(x.Title, title, StringComparison.Ordinal) || string.Equals(x.Body, body, StringComparison.Ordinal))))
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
                Body = ShortNativeDeathBody(death)
            };
            record.NativeLogged = MclslNativeHistoryBridge.Add(native);
        }

        _current.Events.Add(record);
        TrimEventsPreservingMilestones(_current.Events, MaxEvents);
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
        foreach (MclslTimelineAnchorDefinition definition in MclslKnowledgeCatalog.TimelineAnchors)
        {
            if (FindAnchor(definition.Id) != null) continue;
            int min = Math.Min(definition.EarliestYear, definition.LatestYear);
            int max = Math.Max(definition.EarliestYear, definition.LatestYear);
            int hash = StableHash(_current.RunId + "|" + definition.Id);
            int canonicalYear = min + (hash & int.MaxValue) % Math.Max(1, max - min + 1);
            int scheduledYear = ScheduleTimelineAnchorYear(canonicalYear);
            _current.TimelineAnchors.Add(new MclslTimelineAnchorState { AnchorId = definition.Id, ScheduledYear = scheduledYear });
        }
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
        bool changed = false;
        for (int i = 0; i < MclslCultivationCatalog.Techniques.Count; i++)
        {
            MclslTechniqueDefinition definition = MclslCultivationCatalog.Techniques[i];
            if (definition == null) continue;
            int maxIndex = MclslRealmIds.Index(definition.MaxRealm);
            if (maxIndex < 0 || maxIndex > MclslRealmIds.Index(MclslRealmIds.YuanYing)) continue;
            string id = MclslCultivationCatalog.NormalizeTechniqueId(definition.Id);
            MclslTechniqueLineageRecord existing = _current.TechniqueLineages.FirstOrDefault(x => x?.Id == id);
            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(existing.Name)) { existing.Name = definition.Name; changed = true; }
                if (string.IsNullOrWhiteSpace(existing.SystemId)) { existing.SystemId = MclslCultivationSystemIds.AncientLaw; changed = true; }
                if (string.IsNullOrWhiteSpace(existing.LawTags)) { existing.LawTags = string.Join(",", definition.LawPool); changed = true; }
                if (string.IsNullOrWhiteSpace(existing.MaxRealm)) { existing.MaxRealm = definition.MaxRealm; changed = true; }
                if (string.IsNullOrWhiteSpace(existing.Summary)) { existing.Summary = AncientTechniqueSummary(definition); changed = true; }
                if (string.Equals(existing.FounderName, "上古散修", StringComparison.Ordinal)) { existing.FounderName = string.Empty; changed = true; }
                continue;
            }

            _current.TechniqueLineages.Add(new MclslTechniqueLineageRecord
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
            });
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

    private static void EnsureFutureCatalogs()
    {
        bool tracePersistence = _current.InverseTruths?.Any(x => x != null && x.Id == "truth_player_trace_persistence" && x.Reversed) == true;
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
            .OrderBy(x => StableHash(_current.RunId + "|player_truth|" + x.Id))
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

    private static void Normalize()
    {
        _current ??= new MclslWorldRunState();
        _current.BackgroundFactions ??= new MclslBackgroundFactionState();
        _current.FiredHistoricalEvents ??= new List<string>();
        _current.PendingAncientCultivatorIds ??= new List<string>();
        if (_current.EraOriginYear <= 0) _current.EraOriginYear = Math.Max(0, _current.StartYear);
        if (_current.AncientLawEndYear <= 0) _current.AncientLawEndYear = _current.EraOriginYear + MclslWorldEpochSystem.AncientLawDurationYears;
        if (_current.TransmissionEndYear <= 0) _current.TransmissionEndYear = _current.AncientLawEndYear + MclslWorldEpochSystem.TransmissionDurationYears;
        if (_current.NewLawStartYear <= 0) _current.NewLawStartYear = _current.TransmissionEndYear;
        if (!_current.FiredHistoricalEvents.Contains("epoch_chuanfa_dao"))
        {
            _current.AncientLawEndYear = _current.EraOriginYear + MclslWorldEpochSystem.AncientLawDurationYears;
            _current.TransmissionEndYear = _current.AncientLawEndYear + MclslWorldEpochSystem.TransmissionDurationYears;
            _current.NewLawStartYear = _current.TransmissionEndYear;
        }
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
        TrimEventsPreservingMilestones(_current.Events, MaxEvents);
        foreach (MclslRunEventRecord e in _current.Events)
            if (e != null && string.IsNullOrWhiteSpace(e.Category))
                e.Category = MclslEventCatalog.CategoryForType(e.EventType);
        _current.DeathRecords ??= new List<MclslDeathRecord>();
        _current.FactionMissions ??= new List<MclslFactionMissionRecord>();
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
        foreach (MclslWorldCaveRecord cave in _current.WorldCaves)
            if (!string.IsNullOrWhiteSpace(cave.Name) && !ContainsGeneratedName(cave.Name)) _current.UsedGeneratedNames.Add(cave.Name);
        foreach (MclslGeneratedItemRecord item in _current.GeneratedItems)
            if (!string.IsNullOrWhiteSpace(item.Name) && !ContainsGeneratedName(item.Name)) _current.UsedGeneratedNames.Add(item.Name);
        foreach (MclslSectRuinRecord ruin in _current.SectRuins)
            if (!string.IsNullOrWhiteSpace(ruin.Name) && !ContainsGeneratedName(ruin.Name)) _current.UsedGeneratedNames.Add(ruin.Name);
        foreach (MclslWorldChangeRecord change in _current.WorldChanges)
            if (!string.IsNullOrWhiteSpace(change.Name) && !ContainsGeneratedName(change.Name)) _current.UsedGeneratedNames.Add(change.Name);
        _current.UsesNativeKingdoms = true;
        MclslWorldRunRuntimeIndexes.Invalidate();
    }

    private static bool ContainsGeneratedName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || _current?.UsedGeneratedNames == null) return false;
        List<string> names = _current.UsedGeneratedNames;
        for (int i = 0; i < names.Count; i++)
        {
            if (string.Equals(names[i], name, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static int StableHash(string value)
    {
        unchecked { int hash = 17; foreach (char c in value ?? string.Empty) hash = hash * 31 + c; return hash; }
    }
}
