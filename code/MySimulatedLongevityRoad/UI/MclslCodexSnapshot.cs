using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal sealed class MclslCodexSnapshot
{
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
    internal readonly Dictionary<string, int> RealmCounts = new();
    internal readonly List<string> KingdomLines = new();
    internal readonly List<MclslKingdomCodexEntry> KingdomEntries = new();
    internal readonly List<string> CultivatorLines = new();
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
    internal readonly Dictionary<string, int> EventCategoryCounts = new(StringComparer.Ordinal);
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
        snapshot.ArchiveVersion = MclslWorldArchiveMigration.CurrentVersion;
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
        IReadOnlyList<Actor> units = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        Dictionary<string, Dictionary<string, int>> kingdomRealms = new(StringComparer.Ordinal);
        Dictionary<string, List<string>> kingdomCultivators = new(StringComparer.Ordinal);
        Dictionary<string, MclslKingdomCodexEntry> kingdomEntries = new(StringComparer.Ordinal);
        List<CultivatorRankEntry> rankEntries = new();
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            if (!MclslActorAccessor.HasCultivationPath(actor)
                && !MclslCultivationActorMarker.HasCultivationMarker(actor)) continue;
            snapshot.Cultivators++;
            string realm = MclslActorAccessor.Realm(actor);
            if (string.IsNullOrWhiteSpace(realm))
            {
                snapshot.SensingQi++;
                rankEntries.Add(BuildRankEntry(actor, realm));
                continue;
            }
            snapshot.RealmCounts[realm] = snapshot.RealmCounts.TryGetValue(realm, out int c) ? c + 1 : 1;
            string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无国散修" : actor.kingdom.data.name;
            if (!kingdomRealms.TryGetValue(kingdom, out var map)) kingdomRealms[kingdom] = map = new();
            map[realm] = map.TryGetValue(realm, out int kc) ? kc + 1 : 1;
            if (!kingdomEntries.TryGetValue(kingdom, out MclslKingdomCodexEntry kingdomEntry))
            {
                kingdomEntry = new MclslKingdomCodexEntry { Name = kingdom };
                kingdomEntries[kingdom] = kingdomEntry;
            }
            kingdomEntry.TotalCultivators++;
            kingdomEntry.RealmCounts[realm] = kingdomEntry.RealmCounts.TryGetValue(realm, out int rc) ? rc + 1 : 1;
            kingdomEntry.Cultivators.Add(new MclslKingdomCultivatorEntry
            {
                ActorId = MclslActorAccessor.Id(actor),
                Name = MclslActorAccessor.DisplayName(actor),
                RealmId = realm,
                RealmName = MclslRealmIds.Display(realm),
                RealmIndex = MclslRealmIds.Index(realm),
                TrueEssence = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0),
                Contribution = MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Contribution, 0)
            });
            if (!kingdomCultivators.TryGetValue(kingdom, out List<string> roster))
            {
                roster = new List<string>();
                kingdomCultivators[kingdom] = roster;
            }
            if (roster.Count < 24)
                roster.Add(MclslActorAccessor.DisplayName(actor) + "·" + MclslRealmIds.Display(realm));
            rankEntries.Add(BuildRankEntry(actor, realm));
        }
        foreach (var pair in kingdomRealms.OrderByDescending(x => x.Value.Values.Sum()).ThenBy(x => x.Key))
        {
            int count = pair.Value.Values.Sum();
            string realms = string.Join("，", MclslRealmIds.Ordered.Where(pair.Value.ContainsKey).Select(r => MclslRealmIds.Display(r) + pair.Value[r]));
            string roster = kingdomCultivators.TryGetValue(pair.Key, out List<string> names)
                ? string.Join("，", names.OrderBy(x => x, StringComparer.Ordinal).Take(12))
                : string.Empty;
            snapshot.KingdomLines.Add(pair.Key + "｜修士" + count + "｜" + realms + (string.IsNullOrWhiteSpace(roster) ? string.Empty : "｜名册:" + roster));
        }
        foreach (MclslKingdomCodexEntry entry in kingdomEntries.Values
            .OrderByDescending(x => x.TotalCultivators)
            .ThenBy(x => x.Name, StringComparer.Ordinal))
        {
            entry.Cultivators.Sort((a, b) =>
            {
                int realm = b.RealmIndex.CompareTo(a.RealmIndex);
                if (realm != 0) return realm;
                int essence = b.TrueEssence.CompareTo(a.TrueEssence);
                if (essence != 0) return essence;
                int contribution = b.Contribution.CompareTo(a.Contribution);
                if (contribution != 0) return contribution;
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
            entry.RebuildRealmIndex();
            snapshot.KingdomEntries.Add(entry);
        }
        int realmRank = 1;
        foreach (string realmId in MclslRealmIds.Ordered.Reverse())
        {
            int count = snapshot.RealmCounts.TryGetValue(realmId, out int value) ? value : 0;
            if (count <= 0) continue;
            int share = snapshot.Cultivators <= 0 ? 0 : (int)Math.Round(count * 100d / snapshot.Cultivators);
            snapshot.RealmRankLines.Add("第" + realmRank + "位｜" + MclslRealmIds.Display(realmId) + "｜" + count + "人｜占修士" + share + "%");
            realmRank++;
        }
        int rank = 1;
        foreach (CultivatorRankEntry entry in rankEntries
            .OrderByDescending(x => x.RealmIndex)
            .ThenByDescending(x => x.TrueEssence)
            .ThenByDescending(x => x.Contribution)
            .ThenByDescending(x => x.Aptitude)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Take(160))
        {
            snapshot.CultivatorLines.Add("第" + rank + "名｜" + entry.Line);
            rank++;
        }
        BuildArchiveRankLines(snapshot);
        BuildCodexPageCaches(snapshot);
        return snapshot;
    }

    private static void BuildArchiveRankLines(MclslCodexSnapshot snapshot)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return;

        int deathRank = 1;
        foreach (MclslDeathRecord death in (run.DeathRecords ?? new List<MclslDeathRecord>())
            .OrderByDescending(x => MclslRealmIds.Index(x.RealmId))
            .ThenByDescending(x => x.Year)
            .ThenByDescending(x => x.Age)
            .Take(80))
        {
            snapshot.DeathRankLines.Add("第" + deathRank + "名｜" + Blank(death.ActorName) + "｜" + Blank(death.RealmName) + "｜" + death.Year + "年｜" + ShortCause(death.CauseText));
            deathRank++;
        }

        int soulRank = 1;
        foreach (MclslWorldSoulRecord soul in (run.WorldSouls ?? new List<MclslWorldSoulRecord>())
            .OrderBy(x => SoulStateOrder(x.State))
            .ThenByDescending(x => x.Quality)
            .ThenByDescending(x => x.DutyProgress)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Take(80))
        {
            string holder = soul.HolderActorId > 0 ? Blank(soul.HolderActorName) : "无主";
            snapshot.WorldSoulRankLines.Add("第" + soulRank + "位｜" + soul.Name + "｜" + Quality(soul.Quality) + "｜" + soul.State + "｜" + holder + "｜天职" + soul.DutyProgress + "%｜反噬" + soul.DutyBacklash + "%");
            soulRank++;
        }
    }

    private static void BuildCodexPageCaches(MclslCodexSnapshot snapshot)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run == null) return;

        List<MclslWorldSoulRecord> souls = run.WorldSouls ?? new List<MclslWorldSoulRecord>();
        snapshot.WorldSoulManifestedCount = souls.Count(x => string.Equals(x.State, "显化", StringComparison.Ordinal));
        snapshot.WorldSoulHeldCount = souls.Count(x => string.Equals(x.State, "已祭炼", StringComparison.Ordinal));
        snapshot.WorldSoulsSorted.AddRange(souls
            .Where(x => x != null)
            .OrderBy(x => SoulStateOrder(x.State))
            .ThenBy(x => x.Name, StringComparer.Ordinal));

        List<MclslRunEventRecord> events = run.Events ?? new List<MclslRunEventRecord>();
        snapshot.VisibleEventsSorted.AddRange(events
            .Where(x => x != null && !string.Equals(x.EventType, "cycle_start", StringComparison.Ordinal))
            .OrderByDescending(x => x.Year));
        for (int i = 0; i < snapshot.VisibleEventsSorted.Count; i++)
        {
            string category = EventCategory(snapshot.VisibleEventsSorted[i]);
            snapshot.EventCategoryCounts[category] = snapshot.EventCategoryCounts.TryGetValue(category, out int count) ? count + 1 : 1;
            if (string.Equals(category, MclslEventCatalog.DaoStruggle, StringComparison.Ordinal)
                && snapshot.DaoStruggleEventsSorted.Count < 120)
                snapshot.DaoStruggleEventsSorted.Add(snapshot.VisibleEventsSorted[i]);
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
        snapshot.EventCategoryCounts[MclslEventCatalog.All] = snapshot.VisibleEventsSorted.Count;

        List<MclslInverseTruthRecord> truths = run.InverseTruths ?? new List<MclslInverseTruthRecord>();
        for (int i = 0; i < truths.Count; i++)
        {
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

        List<MclslActorReincarnationRecord> reincarnations = run.ReincarnationRecords ?? new List<MclslActorReincarnationRecord>();
        for (int i = 0; i < reincarnations.Count; i++)
        {
            MclslActorReincarnationRecord record = reincarnations[i];
            if (record == null) continue;
            if (string.Equals(record.Status, "已转", StringComparison.Ordinal)) snapshot.ReincarnationAppliedCount++;
            else snapshot.ReincarnationPendingCount++;
        }
        snapshot.ReincarnationRecordsSorted.AddRange(reincarnations
            .Where(x => x != null)
            .OrderByDescending(x => x.AppliedYear > 0 ? x.AppliedYear : x.DeathYear)
            .ThenByDescending(x => MclslRealmIds.Index(x.SourceRealmId))
            .Take(80));

        MclslHuanzhenExternalState huanzhen = MclslHuanzhenSystem.Current;
        if (huanzhen?.Anchors != null)
        {
            snapshot.HuanzhenAnchorsSorted.AddRange(huanzhen.Anchors
                .Where(x => x != null)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Sequence));
        }
        if (huanzhen?.History != null)
        {
            snapshot.HuanzhenHistoriesSorted.AddRange(huanzhen.History
                .Where(x => x != null)
                .OrderByDescending(x => x.DeathYear)
                .Take(80));
        }

        List<MclslFactionMissionRecord> missions = run.FactionMissions ?? new List<MclslFactionMissionRecord>();
        snapshot.WanXianRecentMissions.AddRange(missions
            .Where(x => x != null && x.FactionId == "wanxian")
            .OrderByDescending(x => x.Year)
            .Take(3));
        snapshot.FiveEldersRecentMissions.AddRange(missions
            .Where(x => x != null && x.FactionId == "five_elders")
            .OrderByDescending(x => x.Year)
            .Take(3));
        List<MclslFactionPressureRecord> pressureEvents = run.FactionPressureEvents ?? new List<MclslFactionPressureRecord>();
        snapshot.WanXianRecentPressure.AddRange(pressureEvents
            .Where(x => x != null && x.FactionId == "wanxian")
            .OrderByDescending(x => x.Year)
            .Take(2));
        snapshot.FiveEldersRecentPressure.AddRange(pressureEvents
            .Where(x => x != null && x.FactionId == "five_elders")
            .OrderByDescending(x => x.Year)
            .Take(2));

        List<MclslDeathRecord> deaths = run.DeathRecords ?? new List<MclslDeathRecord>();
        snapshot.DeathsByYear.AddRange(deaths.Where(x => x != null).OrderByDescending(x => x.Year));
        for (int i = 0; i < snapshot.DeathsByYear.Count; i++)
        {
            MclslDeathRecord death = snapshot.DeathsByYear[i];
            string realm = string.IsNullOrWhiteSpace(death.RealmId) ? MclslRealmIds.Mortal : death.RealmId;
            if (!snapshot.DeathsByRealm.TryGetValue(realm, out List<MclslDeathRecord> list))
            {
                list = new List<MclslDeathRecord>();
                snapshot.DeathsByRealm[realm] = list;
            }
            list.Add(death);
        }

        List<MclslWorldCaveRecord> caves = run.WorldCaves ?? new List<MclslWorldCaveRecord>();
        snapshot.ActiveCaveCount = caves.Count(x => x != null && string.Equals(x.State, "活跃", StringComparison.Ordinal));
        snapshot.CavesSorted.AddRange(caves
            .Where(x => x != null)
            .OrderBy(x => CaveStateOrder(x.State))
            .ThenByDescending(x => x.Quality)
            .ThenBy(x => x.Name, StringComparer.Ordinal));
        snapshot.HeavenEarthEssencesSorted.AddRange((run.GeneratedItems ?? new List<MclslGeneratedItemRecord>())
            .Where(x => x != null && x.Kind == MclslGeneratedKinds.HeavenEarthEssence)
            .OrderByDescending(x => x.CreatedYear)
            .Take(200));

        List<MclslWorldChangeRecord> changes = run.WorldChanges ?? new List<MclslWorldChangeRecord>();
        snapshot.ActiveWorldChangeCount = changes.Count(x => x != null && string.Equals(x.State, "活跃", StringComparison.Ordinal));
        snapshot.WorldChangesSorted.AddRange(changes
            .Where(x => x != null)
            .OrderBy(x => WorldChangeStateOrder(x.State))
            .ThenByDescending(x => x.StartYear)
            .Take(200));
        snapshot.WorldChangeMarrowsSorted.AddRange((run.GeneratedItems ?? new List<MclslGeneratedItemRecord>())
            .Where(x => x != null && x.Kind == MclslGeneratedKinds.WorldChangeMarrow)
            .OrderByDescending(x => x.CreatedYear)
            .Take(200));

        List<MclslGeneratedItemRecord> generatedItems = run.GeneratedItems ?? new List<MclslGeneratedItemRecord>();
        snapshot.FoundationWondersSorted.AddRange(generatedItems
            .Where(x => x != null && x.Kind == MclslGeneratedKinds.FoundationWonder)
            .OrderByDescending(x => x.CreatedYear)
            .Take(240));
        for (int i = 0; i < snapshot.FoundationWondersSorted.Count; i++)
        {
            MclslGeneratedItemRecord wonder = snapshot.FoundationWondersSorted[i];
            if (wonder.Category == MclslGeneratedObjectFactory.FoundationHeaven) snapshot.FoundationHeavenCount++;
            if (wonder.Category == MclslGeneratedObjectFactory.FoundationEarth) snapshot.FoundationEarthCount++;
            if (!string.IsNullOrWhiteSpace(wonder.HolderName)) snapshot.FoundationRefinedCount++;
        }

        List<MclslSectRuinRecord> ruins = run.SectRuins ?? new List<MclslSectRuinRecord>();
        snapshot.OpenRuinCount = ruins.Count(IsRuinOpen);
        for (int i = 0; i < ruins.Count; i++)
        {
            MclslSectRuinRecord ruin = ruins[i];
            if (ruin == null || string.IsNullOrWhiteSpace(ruin.Id)) continue;
            snapshot.RuinNameById[ruin.Id] = string.IsNullOrWhiteSpace(ruin.Name) ? "无名遗迹" : ruin.Name;
            snapshot.RuinMaxExpeditionsById[ruin.Id] = MclslAdventureSystem.RuinMaxExpeditions(ruin);
        }

        List<MclslRuinExplorationRecord> explorations = run.RuinExplorations ?? new List<MclslRuinExplorationRecord>();
        for (int i = 0; i < explorations.Count; i++)
        {
            MclslRuinExplorationRecord exploration = explorations[i];
            if (exploration == null) continue;
            if (!string.IsNullOrWhiteSpace(exploration.RuinId))
                snapshot.RuinExplorationCountsByRuinId[exploration.RuinId] = snapshot.RuinExplorationCountsByRuinId.TryGetValue(exploration.RuinId, out int count) ? count + 1 : 1;
            if (!string.IsNullOrWhiteSpace(exploration.LinkedLineageId) && !string.IsNullOrWhiteSpace(exploration.RevivedTechniqueName))
                snapshot.LineageRevivalCountsById[exploration.LinkedLineageId] = snapshot.LineageRevivalCountsById.TryGetValue(exploration.LinkedLineageId, out int count) ? count + 1 : 1;
        }

        List<MclslTechniqueLineageRecord> lineageSource = run.TechniqueLineages ?? new List<MclslTechniqueLineageRecord>();
        for (int i = 0; i < lineageSource.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = lineageSource[i];
            if (lineage == null || string.IsNullOrWhiteSpace(lineage.Id)) continue;
            snapshot.LineageNameById[lineage.Id] = lineage.Name ?? string.Empty;
            snapshot.LineageSectById[lineage.Id] = lineage.SectDisplayName ?? string.Empty;
        }

        snapshot.TechniqueLineagesSorted.AddRange(lineageSource
            .Where(x => x != null)
            .OrderBy(x => LineageStateOrder(x.LifecycleState))
            .ThenByDescending(x => x.CurrentPractitioners)
            .ThenByDescending(x => MclslRealmIds.Index(x.PeakRealm))
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Take(160));
        if (MclslWorldEpochSystem.IsNewLawActive(snapshot.Year))
        {
            snapshot.DaoStruggleLineagesSorted.AddRange(lineageSource
                .Where(x => x != null
                    && x.CurrentPractitioners > 1)
                .OrderByDescending(x => x.CurrentPractitioners)
                .ThenByDescending(x => MclslRealmIds.Index(x.PeakRealm))
                .ThenBy(x => x.Name, StringComparer.Ordinal)
                .Take(20));
        }
        snapshot.RuinsSorted.AddRange(ruins
            .Where(x => x != null)
            .OrderBy(x => RuinStateOrder(x.State))
            .ThenByDescending(x => x.Quality)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Take(160));
        snapshot.RuinExplorationsSorted.AddRange(explorations
            .Where(x => x != null)
            .OrderByDescending(x => x.Year)
            .Take(160));
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

    private static CultivatorRankEntry BuildRankEntry(Actor actor, string realm)
    {
        MclslActorCultivationView cultivation = MclslActorCultivationQuery.Build(actor);
        int aptitude = cultivation.Aptitude;
        int essence = cultivation.TrueEssence;
        string essenceText = cultivation.NextRealmMinimum > 0 ? essence + "（下境最低" + cultivation.NextRealmMinimum + "）" : essence.ToString();
        float qi = cultivation.CultivationProgress;
        int contribution = cultivation.Contribution;
        int stones = cultivation.SpiritStones;
        string extra = StageBrief(cultivation);
        string line = cultivation.Name
            + "｜" + cultivation.RealmName
            + "｜灵根 " + aptitude + "·" + cultivation.GiftName
            + "｜仙缘 " + cultivation.ImmortalFate
            + "｜修炼 " + qi.ToString("0") + "%"
            + "｜心境 " + cultivation.MindState
            + "｜真元 " + essenceText
            + "｜贡献 " + contribution
            + "｜灵石 " + stones
            + extra;
        return new CultivatorRankEntry
        {
            Name = cultivation.Name,
            RealmIndex = Math.Max(0, MclslRealmIds.Index(realm)),
            TrueEssence = essence,
            Contribution = contribution,
            Aptitude = aptitude,
            Line = line
        };
    }

    private static string StageBrief(MclslActorCultivationView cultivation) => cultivation.RealmId switch
    {
        MclslRealmIds.JinDan => "｜" + ReplaceTags(string.IsNullOrWhiteSpace(cultivation.GoldenCoreLaws) ? "未悟法" : cultivation.GoldenCoreLaws),
        MclslRealmIds.YuanYing => "｜" + (string.IsNullOrWhiteSpace(cultivation.NascentEssenceName) ? "未炼天地之精" : cultivation.NascentEssenceName),
        MclslRealmIds.HuaShen => "｜" + (string.IsNullOrWhiteSpace(cultivation.DivineMarrowName) ? "未抽天地之髓" : cultivation.DivineMarrowName),
        MclslRealmIds.HeDao => "｜天地之魄·" + (string.IsNullOrWhiteSpace(cultivation.WorldSoulName) ? "无主魄位" : cultivation.WorldSoulName) + "｜逆理" + MclslInverseTruthStageCatalog.StageName(cultivation.InverseTruthProgress) + (cultivation.HarmonyLeap == 1 ? "｜祭魄跃迁" : string.Empty),
        MclslRealmIds.ChangSheng => "｜" + (string.IsNullOrWhiteSpace(cultivation.InverseTruthName) ? "未定天地之理" : cultivation.InverseTruthName) + "｜逆理" + MclslInverseTruthStageCatalog.StageName(cultivation.InverseTruthProgress),
        _ => string.Empty
    };

    private static string ReplaceTags(string value) => string.IsNullOrWhiteSpace(value) ? "无" : value.Replace(",", "、");
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

    private sealed class CultivatorRankEntry
    {
        internal string Name = string.Empty;
        internal int RealmIndex;
        internal int TrueEssence;
        internal int Contribution;
        internal int Aptitude;
        internal string Line = string.Empty;
    }
}

internal sealed class MclslKingdomCodexEntry
{
    internal string Name = string.Empty;
    internal int TotalCultivators;
    internal readonly Dictionary<string, int> RealmCounts = new(StringComparer.Ordinal);
    internal readonly List<MclslKingdomCultivatorEntry> Cultivators = new();
    internal readonly Dictionary<string, List<MclslKingdomCultivatorEntry>> CultivatorsByRealm = new(StringComparer.Ordinal);

    internal void RebuildRealmIndex()
    {
        CultivatorsByRealm.Clear();
        for (int i = 0; i < Cultivators.Count; i++)
        {
            MclslKingdomCultivatorEntry cultivator = Cultivators[i];
            if (cultivator == null) continue;
            string realm = string.IsNullOrWhiteSpace(cultivator.RealmId) ? MclslRealmIds.Mortal : cultivator.RealmId;
            if (!CultivatorsByRealm.TryGetValue(realm, out List<MclslKingdomCultivatorEntry> list))
            {
                list = new List<MclslKingdomCultivatorEntry>();
                CultivatorsByRealm[realm] = list;
            }
            list.Add(cultivator);
        }
    }

    internal IReadOnlyList<MclslKingdomCultivatorEntry> CultivatorsForRealm(string realmId)
    {
        if (string.IsNullOrWhiteSpace(realmId) || string.Equals(realmId, MclslEventCatalog.All, StringComparison.Ordinal))
            return Cultivators;
        return CultivatorsByRealm.TryGetValue(realmId, out List<MclslKingdomCultivatorEntry> list)
            ? list
            : Array.Empty<MclslKingdomCultivatorEntry>();
    }
}

internal sealed class MclslKingdomCultivatorEntry
{
    internal long ActorId;
    internal string Name = string.Empty;
    internal string RealmId = string.Empty;
    internal string RealmName = string.Empty;
    internal int RealmIndex;
    internal int TrueEssence;
    internal int Contribution;
}
