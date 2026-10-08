using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslCodexSnapshot
{
    private IEnumerator _pending;
    internal bool Ready => _pending == null;
    internal long SourceRevision;
    internal void Cancel() { (_pending as IDisposable)?.Dispose(); _pending = null; }
    internal void Advance()
    {
        if (_pending == null) return;
        long sample = MclslPerformanceProbe.Begin();
        try
        {
            // One bounded archive category per visible frame. Population is read
            // directly from indexes, and no UI task is advanced while closed.
            if (!_pending.MoveNext()) { (_pending as IDisposable)?.Dispose(); _pending = null; }
        }
        finally { MclslPerformanceProbe.End("UI.仙录分帧照录", sample); }
    }
    private static IEnumerator BuildPending(MclslCodexSnapshot snapshot)
    {
        IEnumerator current = BuildArchiveRankLines(snapshot);
        try
        {
            while (current.MoveNext()) yield return null;
            (current as IDisposable)?.Dispose(); current = BuildCodexPageCaches(snapshot);
            while (current.MoveNext()) yield return null;
        }
        finally { (current as IDisposable)?.Dispose(); }
    }
    internal int Year;
    internal int Population;
    internal int Cultivators;
    internal int SensingQi;
    internal int ArchiveVersion;
    internal int TrackedActors;
    internal int AnnualCandidates;
    internal int AnnualActorBacklog;
    internal int AnnualActorStates;
    internal int AnnualModuleBacklog;
    internal int LoadRecoveryBacklog;
    internal bool AnnualWorldWorkPending;
    internal string BackgroundEraName = string.Empty;
    internal string BackgroundEraSummary = string.Empty;
    internal int BackgroundEraStartYear;
    internal int BackgroundEraEndYear;
    internal string WorldStateName = string.Empty;
    internal string WorldStateSummary = string.Empty;
    internal IReadOnlyDictionary<string, int> RealmCounts => MclslCodexPopulationIndex.RealmCounts;
    internal IReadOnlyList<MclslKingdomCodexEntry> KingdomEntries => MclslCodexPopulationIndex.Entries;
    internal readonly List<string> RealmRankLines = new();
    internal readonly List<string> DeathRankLines = new();
    internal readonly List<string> WorldSoulRankLines = new();
    internal readonly List<MclslWorldSoulRecord> WorldSoulsSorted = new();
    internal readonly List<MclslRunEventRecord> VisibleEventsSorted = new();
    internal readonly List<MclslRunEventRecord> AncientEventsSorted = new();
    internal readonly Dictionary<string, List<MclslRunEventRecord>> AncientEventsByType = new(StringComparer.Ordinal);
    internal readonly List<MclslRunEventRecord> AncientTeachingEvents = new();
    internal readonly List<MclslRunEventRecord> AncientBreakthroughEvents = new();
    internal readonly List<MclslRunEventRecord> AncientMindEvents = new();
    internal readonly List<MclslRunEventRecord> AncientDisasterEvents = new();
    internal readonly List<MclslRunEventRecord> AncientSecretRealmEvents = new();
    internal readonly List<MclslRunEventRecord> AncientWorldSoulObservationEvents = new();
    internal readonly List<MclslRunEventRecord> DaoStruggleEventsSorted = new();
    internal readonly List<MclslRunEventRecord> HuanzhenEventsSorted = new();
    internal readonly List<MclslDeathRecord> DeathsByYear = new();
    internal readonly Dictionary<string, List<MclslDeathRecord>> DeathsByRealm = new(StringComparer.Ordinal);
    internal readonly List<MclslWorldCaveRecord> CavesSorted = new();
    internal readonly List<MclslGeneratedItemRecord> HeavenEarthEssencesSorted = new();
    internal readonly List<MclslWorldChangeRecord> WorldChangesSorted = new();
    internal readonly List<MclslGeneratedItemRecord> WorldChangeMarrowsSorted = new();
    internal readonly List<MclslTechniqueLineageRecord> TechniqueLineagesSorted = new();
    internal readonly List<MclslTechniqueLineageRecord> DaoStruggleLineagesSorted = new();
    internal readonly List<MclslSectRuinRecord> RuinsSorted = new();
    internal readonly List<MclslRuinExplorationRecord> RuinExplorationsSorted = new();
    internal readonly Dictionary<string, string> RuinNameById = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, int> RuinMaxExpeditionsById = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, int> RuinExplorationCountsByRuinId = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, int> LineageRevivalCountsById = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, string> LineageNameById = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, string> LineageSectById = new(StringComparer.Ordinal);
    internal readonly List<MclslInverseTruthRecord> PlayerTruths = new();
    internal readonly List<MclslInverseTruthRecord> VisiblePlayerTruths = new();
    internal readonly List<MclslInverseTruthRecord> CanonTruths = new();
    internal readonly List<MclslActorReincarnationRecord> ReincarnationRecordsSorted = new();
    internal readonly List<MclslHuanzhenAnchorRecord> HuanzhenAnchorsSorted = new();
    internal readonly List<MclslHuanzhenHistoryRecord> HuanzhenHistoriesSorted = new();
    internal readonly List<MclslGeneratedItemRecord> FoundationWondersSorted = new();
    internal readonly List<MclslFactionMissionRecord> WanXianRecentMissions = new();
    internal readonly Dictionary<string, List<MclslFactionMissionRecord>> AncientSectRecentMissions = new(StringComparer.Ordinal);
    internal readonly List<MclslFactionPressureRecord> WanXianRecentPressure = new();
    internal readonly List<MclslFactionMissionRecord> FiveEldersRecentMissions = new();
    internal readonly List<MclslFactionPressureRecord> FiveEldersRecentPressure = new();
    internal int WorldSoulManifestedCount;
    internal int WorldSoulHeldCount;
    internal int ActiveCaveCount;
    internal int ActiveWorldChangeCount;
    internal int OpenRuinCount;
    internal int ReversedPlayerTruthCount;
    internal int ReincarnationPendingCount;
    internal int ReincarnationAppliedCount;
    internal int FoundationHeavenCount;
    internal int FoundationEarthCount;
    internal int FoundationRefinedCount;

    internal static MclslCodexSnapshot Build()
    {
        MclslCodexSnapshot snapshot = new() { Year = MclslRuntime.CurrentYear() };
        snapshot.ArchiveVersion = MclslSaveVersions.WorldArchive;
        snapshot.TrackedActors = MclslCultivatorCandidateIndex.KnownActorCount;
        snapshot.AnnualCandidates = MclslCultivatorCandidateIndex.AnnualCandidateCount;
        snapshot.AnnualActorBacklog = MclslScheduler.AnnualActorBacklogCount;
        snapshot.AnnualActorStates = MclslScheduler.AnnualActorStateCount;
        snapshot.AnnualModuleBacklog = MclslModuleHub.AnnualModuleBacklogCount;
        snapshot.LoadRecoveryBacklog = MclslModuleHub.LoadRecoveryBacklogCount;
        snapshot.AnnualWorldWorkPending = MclslScheduler.AnnualWorldWorkPending;
        MclslBackgroundEraInfo backgroundEra = MclslWorldEraCycleSystem.Current(snapshot.Year);
        snapshot.BackgroundEraName = backgroundEra.Name;
        snapshot.BackgroundEraSummary = backgroundEra.Summary;
        snapshot.BackgroundEraStartYear = backgroundEra.StartYear;
        snapshot.BackgroundEraEndYear = backgroundEra.EndYear;
        MclslWorldStateModifiers worldState = MclslWorldStateModifierSystem.Current(snapshot.Year);
        snapshot.WorldStateName = worldState.Name;
        snapshot.WorldStateSummary = worldState.Summary;
        snapshot.Population = Math.Max(0, MclslWorldActorQuery.UnitCount());
        snapshot.Cultivators = MclslCodexPopulationIndex.Count;
        snapshot.SensingQi = MclslCodexPopulationIndex.SensingQi;
        int realmRank = 1;
        foreach (string realmId in MclslRealmIds.Ordered.Reverse())
        {
            int count = snapshot.RealmCounts.TryGetValue(realmId, out int value) ? value : 0;
            if (count <= 0) continue;
            int share = snapshot.Cultivators <= 0 ? 0 : (int)Math.Round(count * 100d / snapshot.Cultivators);
            snapshot.RealmRankLines.Add("第" + realmRank + "位｜" + MclslRealmIds.Display(realmId) + "｜" + count + "人｜占修士" + share + "%");
            realmRank++;
        }
        snapshot.SourceRevision = MclslWorldArchiveStore.Revision;
        snapshot._pending = BuildPending(snapshot);
        return snapshot;
    }

    private static IEnumerator BuildArchiveRankLines(MclslCodexSnapshot snapshot)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) yield break;

        int deathRank = 1;
        List<MclslDeathRecord> deathOrder = new();
        IEnumerator deathWork = MclslBudgetedView.Sort((run.DeathRecords ?? new List<MclslDeathRecord>()),
            deathOrder,
            (a, b) => { int order; order = MclslBudgetedView.Compare(MclslRealmIds.Index(a.RealmId), MclslRealmIds.Index(b.RealmId), true); if (order != 0) return order; order = MclslBudgetedView.Compare(a.Year, b.Year, true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Age, b.Age, true); },
            take: 80);
        try { while (deathWork.MoveNext()) yield return null; }
        finally { (deathWork as IDisposable)?.Dispose(); }
        foreach (MclslDeathRecord death in deathOrder)
        {
            if ((deathRank & 15) == 0) yield return null;
            snapshot.DeathRankLines.Add("第" + deathRank + "名｜" + Blank(death.ActorName) + "｜" + Blank(death.RealmName) + "｜" + death.Year + "年｜" + ShortCause(death.CauseText));
            deathRank++;
        }

        yield return null;
        int soulRank = 1;
        List<MclslWorldSoulRecord> soulOrder = new();
        IEnumerator soulWork = MclslBudgetedView.Sort((run.WorldSouls ?? new List<MclslWorldSoulRecord>()),
            soulOrder,
            (a, b) => { int order; order = MclslBudgetedView.Compare(SoulStateOrder(a.State), SoulStateOrder(b.State), false); if (order != 0) return order; order = MclslBudgetedView.Compare(a.Quality, b.Quality, true); if (order != 0) return order; order = MclslBudgetedView.Compare(a.DutyProgress, b.DutyProgress, true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Name, b.Name, false, StringComparer.Ordinal); },
            take: 80);
        try { while (soulWork.MoveNext()) yield return null; }
        finally { (soulWork as IDisposable)?.Dispose(); }
        foreach (MclslWorldSoulRecord soul in soulOrder)
        {
            if ((soulRank & 15) == 0) yield return null;
            string holder = soul.HolderActorId > 0 ? Blank(soul.HolderActorName) : "无主";
            snapshot.WorldSoulRankLines.Add("第" + soulRank + "位｜" + soul.Name + "｜" + Quality(soul.Quality) + "｜" + soul.State + "｜" + holder + "｜天职" + soul.DutyProgress + "%｜反噬" + soul.DutyBacklash + "%");
            soulRank++;
        }
    }

    private static IEnumerator BuildCodexPageCaches(MclslCodexSnapshot snapshot)
    {
        yield return null;
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) yield break;

        List<MclslWorldSoulRecord> souls = run.WorldSouls ?? new List<MclslWorldSoulRecord>();
        snapshot.WorldSoulManifestedCount = souls.Count(x => string.Equals(x.State, "显化", StringComparison.Ordinal));
        snapshot.WorldSoulHeldCount = souls.Count(x => string.Equals(x.State, "已祭炼", StringComparison.Ordinal));
        IEnumerator view1 = MclslBudgetedView.Sort(souls,
            snapshot.WorldSoulsSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(SoulStateOrder(a.State), SoulStateOrder(b.State), false); if (order != 0) return order; return MclslBudgetedView.Compare(a.Name, b.Name, false, StringComparer.Ordinal); },
            filter: x => x != null);
        try { while (view1.MoveNext()) yield return null; }
        finally { (view1 as IDisposable)?.Dispose(); }

        yield return null;
        List<MclslRunEventRecord> events = run.Events ?? new List<MclslRunEventRecord>();
        IEnumerator view2 = MclslBudgetedView.Sort(events,
            snapshot.VisibleEventsSorted,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null && !string.Equals(x.EventType, "cycle_start", StringComparison.Ordinal));
        try { while (view2.MoveNext()) yield return null; }
        finally { (view2 as IDisposable)?.Dispose(); }
        for (int i = 0; i < snapshot.VisibleEventsSorted.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            string category = EventCategory(snapshot.VisibleEventsSorted[i]);
            if (string.Equals(category, MclslEventCatalog.DaoStruggle, StringComparison.Ordinal)
                && snapshot.DaoStruggleEventsSorted.Count < 120)
                snapshot.DaoStruggleEventsSorted.Add(snapshot.VisibleEventsSorted[i]);
            string eventType = snapshot.VisibleEventsSorted[i].EventType ?? string.Empty;
            if (eventType.StartsWith("huanzhen_", StringComparison.Ordinal)
                && snapshot.HuanzhenEventsSorted.Count < 120)
                snapshot.HuanzhenEventsSorted.Add(snapshot.VisibleEventsSorted[i]);
            if (string.Equals(category, MclslEventCatalog.AncientWorld, StringComparison.Ordinal))
            {
                MclslRunEventRecord record = snapshot.VisibleEventsSorted[i];
                if (snapshot.AncientEventsSorted.Count < 240) snapshot.AncientEventsSorted.Add(record);
                string type = record.EventType ?? string.Empty;
                if (!snapshot.AncientEventsByType.TryGetValue(type, out List<MclslRunEventRecord> typed))
                {
                    typed = new List<MclslRunEventRecord>();
                    snapshot.AncientEventsByType[type] = typed;
                }
                if (typed.Count < 160) typed.Add(record);
            }
        }
        snapshot.AddAncientEventGroup(snapshot.AncientTeachingEvents,
            "ancient_master_teaching",
            "ancient_found_method",
            "ancient_technique_deduction",
            "ancient_new_method_branch",
            "ancient_technique_recorded",
            "ancient_technique_revived",
            "ancient_famous_technique",
            "ancient_lineage_flourish",
            "ancient_lineage_ruin_born",
            "ancient_lineage_remembered");
        snapshot.AddAncientEventGroup(snapshot.AncientBreakthroughEvents,
            "ancient_law_breakthrough",
            "ancient_breakthrough_failed");
        snapshot.AddAncientEventGroup(snapshot.AncientMindEvents,
            "ancient_closed_cultivation",
            "ancient_sudden_insight",
            "ancient_inner_demon");
        snapshot.AddAncientEventGroup(snapshot.AncientDisasterEvents,
            "ancient_spiritual_convergence",
            "ancient_spiritual_decline",
            "ancient_earthfire",
            "ancient_meteor_stone");
        snapshot.AddAncientEventGroup(snapshot.AncientSecretRealmEvents, "ancient_secret_realm");
        snapshot.AddAncientEventGroup(snapshot.AncientWorldSoulObservationEvents, "ancient_heaven_earth_resonance");
        yield return null;
        List<MclslInverseTruthRecord> truths = run.InverseTruths ?? new List<MclslInverseTruthRecord>();
        for (int i = 0; i < truths.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslInverseTruthRecord truth = truths[i];
            if (truth == null) continue;
            if (truth.CountsTowardLongevity)
            {
                snapshot.PlayerTruths.Add(truth);
                if (truth.Reversed) snapshot.ReversedPlayerTruthCount++;
                if (truth.Reversed || truth.Progress > 0 || truth.ChallengerActorId > 0)
                    snapshot.VisiblePlayerTruths.Add(truth);
            }
            else snapshot.CanonTruths.Add(truth);
        }

        yield return null;
        List<MclslActorReincarnationRecord> reincarnations = run.ReincarnationRecords ?? new List<MclslActorReincarnationRecord>();
        for (int i = 0; i < reincarnations.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslActorReincarnationRecord record = reincarnations[i];
            if (record == null) continue;
            if (string.Equals(record.Status, "已转", StringComparison.Ordinal)) snapshot.ReincarnationAppliedCount++;
            else snapshot.ReincarnationPendingCount++;
        }
        IEnumerator view3 = MclslBudgetedView.Sort(reincarnations,
            snapshot.ReincarnationRecordsSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(a.AppliedYear > 0 ? a.AppliedYear : a.DeathYear, b.AppliedYear > 0 ? b.AppliedYear : b.DeathYear, true); if (order != 0) return order; return MclslBudgetedView.Compare(MclslRealmIds.Index(a.SourceRealmId), MclslRealmIds.Index(b.SourceRealmId), true); },
            filter: x => x != null,
            take: 80);
        try { while (view3.MoveNext()) yield return null; }
        finally { (view3 as IDisposable)?.Dispose(); }

        yield return null;
        MclslHuanzhenExternalState huanzhen = MclslHuanzhenSystem.Current;
        if (huanzhen?.Anchors != null)
        {
            IEnumerator view4 = MclslBudgetedView.Sort(huanzhen.Anchors,
            snapshot.HuanzhenAnchorsSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(a.Year, b.Year, true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Sequence, b.Sequence, true); },
            filter: x => x != null);
        try { while (view4.MoveNext()) yield return null; }
        finally { (view4 as IDisposable)?.Dispose(); }
        }
        if (huanzhen?.History != null)
        {
            IEnumerator view5 = MclslBudgetedView.Sort(huanzhen.History,
            snapshot.HuanzhenHistoriesSorted,
            (a, b) => MclslBudgetedView.Compare(a.DeathYear, b.DeathYear, true),
            filter: x => x != null,
            take: 80);
        try { while (view5.MoveNext()) yield return null; }
        finally { (view5 as IDisposable)?.Dispose(); }
        }

        yield return null;
        List<MclslFactionMissionRecord> missions = run.FactionMissions ?? new List<MclslFactionMissionRecord>();
        if (snapshot.Year < run.AncientLawEndYear && run.AncientSectMissions != null)
        {
            foreach (var sect in MclslFactionMissionSystem.AncientSects)
                snapshot.AncientSectRecentMissions[sect.Id] = new List<MclslFactionMissionRecord>(3);
            int complete = 0;
            for (int i = run.AncientSectMissions.Count - 1; i >= 0 && complete < MclslFactionMissionSystem.AncientSects.Length; i--)
            {
                if ((i & 63) == 0) yield return null;
                MclslFactionMissionRecord mission = run.AncientSectMissions[i];
                if (mission == null || mission.FactionId == null
                    || !snapshot.AncientSectRecentMissions.TryGetValue(mission.FactionId, out List<MclslFactionMissionRecord> recent)
                    || recent.Count >= 3) continue;
                recent.Add(mission);
                if (recent.Count == 3) complete++;
            }
        }
        IEnumerator view6 = MclslBudgetedView.Sort(missions,
            snapshot.WanXianRecentMissions,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null && x.FactionId == "wanxian",
            take: 3);
        try { while (view6.MoveNext()) yield return null; }
        finally { (view6 as IDisposable)?.Dispose(); }
        IEnumerator view7 = MclslBudgetedView.Sort(missions,
            snapshot.FiveEldersRecentMissions,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null && x.FactionId == "five_elders",
            take: 3);
        try { while (view7.MoveNext()) yield return null; }
        finally { (view7 as IDisposable)?.Dispose(); }
        List<MclslFactionPressureRecord> pressureEvents = run.FactionPressureEvents ?? new List<MclslFactionPressureRecord>();
        IEnumerator view8 = MclslBudgetedView.Sort(pressureEvents,
            snapshot.WanXianRecentPressure,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null && x.FactionId == "wanxian",
            take: 2);
        try { while (view8.MoveNext()) yield return null; }
        finally { (view8 as IDisposable)?.Dispose(); }
        IEnumerator view9 = MclslBudgetedView.Sort(pressureEvents,
            snapshot.FiveEldersRecentPressure,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null && x.FactionId == "five_elders",
            take: 2);
        try { while (view9.MoveNext()) yield return null; }
        finally { (view9 as IDisposable)?.Dispose(); }

        yield return null;
        List<MclslDeathRecord> deaths = run.DeathRecords ?? new List<MclslDeathRecord>();
        IEnumerator view10 = MclslBudgetedView.Sort(deaths,
            snapshot.DeathsByYear,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null);
        try { while (view10.MoveNext()) yield return null; }
        finally { (view10 as IDisposable)?.Dispose(); }
        for (int i = 0; i < snapshot.DeathsByYear.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslDeathRecord death = snapshot.DeathsByYear[i];
            string realm = string.IsNullOrWhiteSpace(death.RealmId) ? MclslRealmIds.Mortal : death.RealmId;
            if (!snapshot.DeathsByRealm.TryGetValue(realm, out List<MclslDeathRecord> list))
            {
                list = new List<MclslDeathRecord>();
                snapshot.DeathsByRealm[realm] = list;
            }
            list.Add(death);
        }

        yield return null;
        List<MclslWorldCaveRecord> caves = run.WorldCaves ?? new List<MclslWorldCaveRecord>();
        snapshot.ActiveCaveCount = caves.Count(x => x != null && string.Equals(x.State, "活跃", StringComparison.Ordinal));
        IEnumerator view11 = MclslBudgetedView.Sort(caves,
            snapshot.CavesSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(CaveStateOrder(a.State), CaveStateOrder(b.State), false); if (order != 0) return order; order = MclslBudgetedView.Compare(a.Quality, b.Quality, true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Name, b.Name, false, StringComparer.Ordinal); },
            filter: x => x != null);
        try { while (view11.MoveNext()) yield return null; }
        finally { (view11 as IDisposable)?.Dispose(); }
        IEnumerator view12 = MclslBudgetedView.Sort((run.GeneratedItems ?? new List<MclslGeneratedItemRecord>()),
            snapshot.HeavenEarthEssencesSorted,
            (a, b) => MclslBudgetedView.Compare(a.CreatedYear, b.CreatedYear, true),
            filter: x => x != null && x.Kind == MclslGeneratedKinds.HeavenEarthEssence,
            take: 200);
        try { while (view12.MoveNext()) yield return null; }
        finally { (view12 as IDisposable)?.Dispose(); }

        yield return null;
        List<MclslWorldChangeRecord> changes = run.WorldChanges ?? new List<MclslWorldChangeRecord>();
        snapshot.ActiveWorldChangeCount = changes.Count(x => x != null && string.Equals(x.State, "活跃", StringComparison.Ordinal));
        IEnumerator view13 = MclslBudgetedView.Sort(changes,
            snapshot.WorldChangesSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(WorldChangeStateOrder(a.State), WorldChangeStateOrder(b.State), false); if (order != 0) return order; return MclslBudgetedView.Compare(a.StartYear, b.StartYear, true); },
            filter: x => x != null,
            take: 200);
        try { while (view13.MoveNext()) yield return null; }
        finally { (view13 as IDisposable)?.Dispose(); }
        IEnumerator view14 = MclslBudgetedView.Sort((run.GeneratedItems ?? new List<MclslGeneratedItemRecord>()),
            snapshot.WorldChangeMarrowsSorted,
            (a, b) => MclslBudgetedView.Compare(a.CreatedYear, b.CreatedYear, true),
            filter: x => x != null && x.Kind == MclslGeneratedKinds.WorldChangeMarrow,
            take: 200);
        try { while (view14.MoveNext()) yield return null; }
        finally { (view14 as IDisposable)?.Dispose(); }

        yield return null;
        List<MclslGeneratedItemRecord> generatedItems = run.GeneratedItems ?? new List<MclslGeneratedItemRecord>();
        IEnumerator view15 = MclslBudgetedView.Sort(generatedItems,
            snapshot.FoundationWondersSorted,
            (a, b) => MclslBudgetedView.Compare(a.CreatedYear, b.CreatedYear, true),
            filter: x => x != null && x.Kind == MclslGeneratedKinds.FoundationWonder,
            take: 240);
        try { while (view15.MoveNext()) yield return null; }
        finally { (view15 as IDisposable)?.Dispose(); }
        for (int i = 0; i < snapshot.FoundationWondersSorted.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslGeneratedItemRecord wonder = snapshot.FoundationWondersSorted[i];
            if (wonder.Category == MclslGeneratedObjectFactory.FoundationHeaven) snapshot.FoundationHeavenCount++;
            if (wonder.Category == MclslGeneratedObjectFactory.FoundationEarth) snapshot.FoundationEarthCount++;
            if (!string.IsNullOrWhiteSpace(wonder.HolderName)) snapshot.FoundationRefinedCount++;
        }

        yield return null;
        List<MclslSectRuinRecord> ruins = run.SectRuins ?? new List<MclslSectRuinRecord>();
        snapshot.OpenRuinCount = ruins.Count(IsRuinOpen);
        for (int i = 0; i < ruins.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslSectRuinRecord ruin = ruins[i];
            if (ruin == null || string.IsNullOrWhiteSpace(ruin.Id)) continue;
            snapshot.RuinNameById[ruin.Id] = string.IsNullOrWhiteSpace(ruin.Name) ? "无名遗迹" : ruin.Name;
            snapshot.RuinMaxExpeditionsById[ruin.Id] = MclslAdventureSystem.RuinMaxExpeditions(ruin);
        }

        List<MclslRuinExplorationRecord> explorations = run.RuinExplorations ?? new List<MclslRuinExplorationRecord>();
        for (int i = 0; i < explorations.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslRuinExplorationRecord exploration = explorations[i];
            if (exploration == null) continue;
            if (!string.IsNullOrWhiteSpace(exploration.RuinId))
                snapshot.RuinExplorationCountsByRuinId[exploration.RuinId] = snapshot.RuinExplorationCountsByRuinId.TryGetValue(exploration.RuinId, out int count) ? count + 1 : 1;
            if (!string.IsNullOrWhiteSpace(exploration.LinkedLineageId) && !string.IsNullOrWhiteSpace(exploration.RevivedTechniqueName))
                snapshot.LineageRevivalCountsById[exploration.LinkedLineageId] = snapshot.LineageRevivalCountsById.TryGetValue(exploration.LinkedLineageId, out int count) ? count + 1 : 1;
        }

        yield return null;
        List<MclslTechniqueLineageRecord> lineageSource = run.TechniqueLineages ?? new List<MclslTechniqueLineageRecord>();
        for (int i = 0; i < lineageSource.Count; i++)
        {
            if (i > 0 && (i & 31) == 0) yield return null;
            MclslTechniqueLineageRecord lineage = lineageSource[i];
            if (lineage == null || string.IsNullOrWhiteSpace(lineage.Id)) continue;
            snapshot.LineageNameById[lineage.Id] = lineage.Name ?? string.Empty;
            snapshot.LineageSectById[lineage.Id] = lineage.SectDisplayName ?? string.Empty;
        }

        IEnumerator view16 = MclslBudgetedView.Sort(lineageSource,
            snapshot.TechniqueLineagesSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(LineageStateOrder(a.LifecycleState), LineageStateOrder(b.LifecycleState), false); if (order != 0) return order; order = MclslBudgetedView.Compare(a.CurrentPractitioners, b.CurrentPractitioners, true); if (order != 0) return order; order = MclslBudgetedView.Compare(MclslRealmIds.Index(a.PeakRealm), MclslRealmIds.Index(b.PeakRealm), true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Name, b.Name, false, StringComparer.Ordinal); },
            filter: x => x != null,
            take: 160);
        try { while (view16.MoveNext()) yield return null; }
        finally { (view16 as IDisposable)?.Dispose(); }
        if (MclslWorldEpochSystem.IsNewLawActive(snapshot.Year))
        {
            IEnumerator view17 = MclslBudgetedView.Sort(lineageSource,
            snapshot.DaoStruggleLineagesSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(a.CurrentPractitioners, b.CurrentPractitioners, true); if (order != 0) return order; order = MclslBudgetedView.Compare(MclslRealmIds.Index(a.PeakRealm), MclslRealmIds.Index(b.PeakRealm), true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Name, b.Name, false, StringComparer.Ordinal); },
            filter: x => x != null
                    && x.CurrentPractitioners > 1,
            take: 20);
        try { while (view17.MoveNext()) yield return null; }
        finally { (view17 as IDisposable)?.Dispose(); }
        }
        IEnumerator view18 = MclslBudgetedView.Sort(ruins,
            snapshot.RuinsSorted,
            (a, b) => { int order; order = MclslBudgetedView.Compare(RuinStateOrder(a.State), RuinStateOrder(b.State), false); if (order != 0) return order; order = MclslBudgetedView.Compare(a.Quality, b.Quality, true); if (order != 0) return order; return MclslBudgetedView.Compare(a.Name, b.Name, false, StringComparer.Ordinal); },
            filter: x => x != null,
            take: 160);
        try { while (view18.MoveNext()) yield return null; }
        finally { (view18 as IDisposable)?.Dispose(); }
        IEnumerator view19 = MclslBudgetedView.Sort(explorations,
            snapshot.RuinExplorationsSorted,
            (a, b) => MclslBudgetedView.Compare(a.Year, b.Year, true),
            filter: x => x != null,
            take: 160);
        try { while (view19.MoveNext()) yield return null; }
        finally { (view19 as IDisposable)?.Dispose(); }
    }

    private void AddAncientEventGroup(List<MclslRunEventRecord> target, params string[] eventTypes)
    {
        if (target == null || eventTypes == null || eventTypes.Length == 0) return;
        for (int i = 0; i < eventTypes.Length && target.Count < 160; i++)
        {
            string type = eventTypes[i];
            if (string.IsNullOrWhiteSpace(type)) continue;
            if (!AncientEventsByType.TryGetValue(type, out List<MclslRunEventRecord> list)) continue;
            for (int j = 0; j < list.Count && target.Count < 160; j++)
            {
                MclslRunEventRecord record = list[j];
                if (record != null) target.Add(record);
            }
        }
        target.Sort((a, b) => (b?.Year ?? 0).CompareTo(a?.Year ?? 0));
    }

    private static string Blank(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value;
    private static string Quality(int q) => q switch { 4 => "玄奇", 3 => "天奇", 2 => "地奇", _ => "凡奇" };
    private static string ShortCause(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "未载";
        string text = value.Trim();
        return text.Length <= 32 ? text : text.Substring(0, 32) + "...";
    }

    private static int SoulStateOrder(string state) => state switch { "显化" => 0, "已祭炼" => 1, "沉寂" => 2, "散逸" => 3, _ => 4 };
    private static int CaveStateOrder(string state) => state switch { "活跃" => 0, "衰退" => 1, "枯竭" => 2, _ => 3 };
    private static int WorldChangeStateOrder(string state) => state switch { "活跃" => 0, "衰减" => 1, "平息" => 2, _ => 3 };
    private static int RuinStateOrder(string state) => state switch { "显世" => 0, "探索中" => 1, "搜尽" => 2, "崩毁" => 3, _ => 4 };
    private static int LineageStateOrder(string state) => state switch
    {
        "道统鼎盛" => 0,
        "传承兴起" => 1,
        "法脉流传" => 2,
        "遗法复现" => 3,
        "法脉支流" => 4,
        "秘境道统" => 5,
        "遗府私传" => 6,
        "遗府留痕" => 7,
        "道统断绝" => 8,
        _ => 8
    };
    private static bool IsRuinOpen(MclslSectRuinRecord ruin) => ruin != null && ruin.RemainingValue > 0 && ruin.State != "搜尽" && ruin.State != "封绝";
    private static string EventCategory(MclslRunEventRecord record)
    {
        if (record == null) return MclslEventCatalog.NativeWorld;
        return string.IsNullOrWhiteSpace(record.Category) ? MclslEventCatalog.CategoryForType(record.EventType) : record.Category;
    }


}
