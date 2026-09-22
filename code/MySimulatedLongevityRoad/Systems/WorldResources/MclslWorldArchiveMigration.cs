using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Only repairs obsolete save structures. It must not rebalance realms, resources,
/// event probabilities or timeline outcomes.
/// </summary>
internal static class MclslWorldArchiveMigration
{
    internal const int CurrentVersion = MclslSaveVersions.WorldArchive;

    private static readonly HashSet<string> AbandonedDaoStruggleEvents = new(StringComparer.Ordinal)
    {
        "newlaw_law_pressure",
        "newlaw_dao_struggle_pressure",
        "newlaw_law_seat_changed",
        "newlaw_dao_branch_created",
        "newlaw_dao_bottleneck",
        "newlaw_dao_retarget"
    };

    internal static bool Upgrade(MclslWorldArchiveBundle source, out MclslWorldArchiveBundle upgraded)
    {
        upgraded = source ?? new MclslWorldArchiveBundle();
        upgraded.CurrentRun ??= new MclslWorldRunState();
        int sourceVersion = Math.Max(1, upgraded.Version);
        bool changed = sourceVersion < CurrentVersion;

        // Exact abandoned prototype artifacts are safe to remove regardless of the
        // declared bundle version; some test saves were written with the same version number.
        changed |= RemoveAbandonedDaoStruggleData(upgraded.CurrentRun);
        if (sourceVersion < 12)
            changed |= RepairOldLawRuinFieldLeak(upgraded.CurrentRun);
        if (sourceVersion < 13)
        {
            changed |= RepairTechniqueCompleteness(upgraded.CurrentRun);
            changed |= RepairOldWorldSoulObservationText(upgraded.CurrentRun);
        }

        changed |= RepairSpatialRecords(upgraded.CurrentRun);

        if (sourceVersion <= CurrentVersion) upgraded.Version = CurrentVersion;
        return changed;
    }

    private static bool RepairSpatialRecords(MclslWorldRunState run)
    {
        if (run == null) return false;
        run.MapNodes ??= new List<MclslMapNodeRecord>();
        run.Sects ??= new List<MclslSectRecord>();
        run.SpatialTasks ??= new List<MclslSpatialTaskRecord>();
        bool changed = false;

        for (int i = 0; i < run.MapNodes.Count; i++)
        {
            MclslMapNodeRecord node = run.MapNodes[i];
            if (node == null) continue;
            if (string.IsNullOrWhiteSpace(node.MarkerKey)) { node.MarkerKey = "mclsl_node_" + node.Id; changed = true; }
            if (string.IsNullOrWhiteSpace(node.OwnerKind)) { node.OwnerKind = "none"; changed = true; }
            if (string.IsNullOrWhiteSpace(node.VisibilityState)) { node.VisibilityState = node.MapX >= 0 && node.MapY >= 0 ? "discovered" : "lost"; changed = true; }
            if (string.IsNullOrWhiteSpace(node.LifecycleState)) { node.LifecycleState = "active"; changed = true; }
        }

        if (run.WorldCaves != null)
        {
            for (int i = 0; i < run.WorldCaves.Count; i++)
            {
                MclslWorldCaveRecord cave = run.WorldCaves[i];
                if (cave == null || string.IsNullOrWhiteSpace(cave.Id)) continue;
                changed |= EnsureNode(run, "node_cave_" + cave.Id, "cave", cave.Id, cave.Name, cave.MapX, cave.MapY,
                    cave.Quality, cave.LawTags, 0, cave.RemainingEssence, cave.BornYear, cave.LocationName,
                    cave.NativeKingdomName, cave.RemainingEssence > 0 ? "active" : "depleted");
            }
        }

        if (run.WorldChanges != null)
        {
            for (int i = 0; i < run.WorldChanges.Count; i++)
            {
                MclslWorldChangeRecord change = run.WorldChanges[i];
                if (change == null || string.IsNullOrWhiteSpace(change.Id)) continue;
                changed |= EnsureNode(run, "node_world_change_" + change.Id, "world_change", change.Id, change.Name, change.MapX, change.MapY,
                    change.Quality, change.LawTags, Math.Clamp(change.Intensity, 0, 100), change.RemainingMarrow, change.StartYear,
                    change.LocationName, change.NativeKingdomName, change.RemainingMarrow > 0 ? "active" : "depleted");
            }
        }

        if (run.SectRuins != null)
        {
            for (int i = 0; i < run.SectRuins.Count; i++)
            {
                MclslSectRuinRecord ruin = run.SectRuins[i];
                if (ruin == null || string.IsNullOrWhiteSpace(ruin.Id)) continue;
                changed |= EnsureNode(run, "node_ruin_" + ruin.Id, "ruin", ruin.Id, ruin.Name, ruin.MapX, ruin.MapY,
                    ruin.Quality, ruin.LawTags, ruin.Danger, ruin.RemainingValue, ruin.BornYear, ruin.LocationName,
                    ruin.NativeKingdomName, ruin.RemainingValue > 0 ? "active" : "depleted");
            }
        }

        for (int i = 0; i < run.Sects.Count; i++)
        {
            MclslSectRecord sect = run.Sects[i];
            if (sect == null) continue;
            sect.MemberActorIds ??= new List<long>();
            sect.ControlledNodeIds ??= new List<string>();
            sect.Inventory ??= new Dictionary<string, int>();
            if (string.IsNullOrWhiteSpace(sect.State)) { sect.State = "active"; changed = true; }
        }
        for (int i = 0; i < run.SpatialTasks.Count; i++)
        {
            MclslSpatialTaskRecord task = run.SpatialTasks[i];
            if (task == null) continue;
            if (string.IsNullOrWhiteSpace(task.State)) { task.State = "assigned"; changed = true; }
            if (task.LastCommandFrame == 0) { task.LastCommandFrame = -10000; changed = true; }
        }
        return changed;
    }

    private static bool EnsureNode(
        MclslWorldRunState run,
        string id,
        string type,
        string sourceId,
        string name,
        int x,
        int y,
        int quality,
        string lawTags,
        int danger,
        int remaining,
        int bornYear,
        string location,
        string kingdom,
        string lifecycle)
    {
        MclslMapNodeRecord node = run.MapNodes.Find(x => x != null && string.Equals(x.Id, id, StringComparison.Ordinal));
        bool changed = false;
        if (node == null)
        {
            node = new MclslMapNodeRecord { Id = id, MarkerKey = "mclsl_node_" + id };
            run.MapNodes.Add(node);
            changed = true;
        }
        changed |= Set(node.NodeType, type, value => node.NodeType = value);
        changed |= Set(node.SourceType, type, value => node.SourceType = value);
        changed |= Set(node.SourceRecordId, sourceId, value => node.SourceRecordId = value);
        changed |= Set(node.Name, name, value => node.Name = value);
        changed |= Set(node.LawTags, lawTags, value => node.LawTags = value);
        changed |= Set(node.LocationName, location, value => node.LocationName = value);
        changed |= Set(node.NativeKingdomNameSnapshot, kingdom, value => node.NativeKingdomNameSnapshot = value);
        string preservedLifecycle = node.LifecycleState == "controlled" && lifecycle != "depleted"
            ? "controlled"
            : node.LifecycleState == "contested" && lifecycle == "active"
                ? "contested"
                : lifecycle;
        changed |= Set(node.LifecycleState, preservedLifecycle, value => node.LifecycleState = value);
        if (node.MapX != x) { node.MapX = x; changed = true; }
        if (node.MapY != y) { node.MapY = y; changed = true; }
        if (node.Quality != Math.Max(1, quality)) { node.Quality = Math.Max(1, quality); changed = true; }
        if (node.Danger != Math.Max(0, danger)) { node.Danger = Math.Max(0, danger); changed = true; }
        if (node.RemainingValue != Math.Max(0, remaining)) { node.RemainingValue = Math.Max(0, remaining); changed = true; }
        if (node.BornYear <= 0 && bornYear > 0) { node.BornYear = bornYear; changed = true; }
        string visibility = x >= 0 && y >= 0 ? "discovered" : "lost";
        if (!string.Equals(node.VisibilityState, visibility, StringComparison.Ordinal)) { node.VisibilityState = visibility; changed = true; }
        return changed;
    }

    private static bool Set(string target, string value, Action<string> setter)
    {
        string next = value ?? string.Empty;
        if (string.Equals(target, next, StringComparison.Ordinal)) return false;
        setter(next);
        return true;
    }

    private static bool RemoveAbandonedDaoStruggleData(MclslWorldRunState run)
    {
        if (run == null) return false;
        bool changed = false;

        if (run.Events != null)
        {
            int removed = run.Events.RemoveAll(record => record != null
                && AbandonedDaoStruggleEvents.Contains(record.EventType ?? string.Empty));
            changed |= removed > 0;
        }

        if (run.TechniqueLineages != null)
        {
            HashSet<string> removedIds = new(StringComparer.Ordinal);
            for (int i = run.TechniqueLineages.Count - 1; i >= 0; i--)
            {
                MclslTechniqueLineageRecord lineage = run.TechniqueLineages[i];
                if (!IsAbandonedAutomaticBranch(lineage)) continue;
                if (!string.IsNullOrWhiteSpace(lineage.Id)) removedIds.Add(lineage.Id);
                run.TechniqueLineages.RemoveAt(i);
                changed = true;
            }

            for (int i = 0; i < run.TechniqueLineages.Count; i++)
            {
                MclslTechniqueLineageRecord lineage = run.TechniqueLineages[i];
                if (lineage == null) continue;
                string normalizedSource = MclslCultivationCatalog.NormalizeTechniqueId(lineage.SourceTechniqueId);
                if (!string.Equals(lineage.SourceTechniqueId, normalizedSource, StringComparison.Ordinal))
                {
                    lineage.SourceTechniqueId = normalizedSource;
                    changed = true;
                }
                if (removedIds.Contains(lineage.ParentLineageId ?? string.Empty))
                {
                    lineage.ParentLineageId = string.Empty;
                    changed = true;
                }
                if (removedIds.Contains(lineage.BranchRootId ?? string.Empty))
                {
                    lineage.BranchRootId = string.Empty;
                    changed = true;
                }
                int childCount = CountChildren(run.TechniqueLineages, lineage.Id);
                if (lineage.BranchCount != childCount)
                {
                    lineage.BranchCount = childCount;
                    changed = true;
                }
            }

            if (run.SectRuins != null)
            {
                for (int i = 0; i < run.SectRuins.Count; i++)
                {
                    MclslSectRuinRecord ruin = run.SectRuins[i];
                    if (ruin == null) continue;
                    if (removedIds.Contains(ruin.LinkedLineageId ?? string.Empty))
                    {
                        ruin.LinkedLineageId = string.Empty;
                        changed = true;
                    }
                    string normalizedSource = MclslCultivationCatalog.NormalizeTechniqueId(ruin.SourceTechniqueId);
                    if (!string.Equals(ruin.SourceTechniqueId, normalizedSource, StringComparison.Ordinal))
                    {
                        ruin.SourceTechniqueId = normalizedSource;
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }



    private static bool RepairTechniqueCompleteness(MclslWorldRunState run)
    {
        if (run?.TechniqueLineages == null) return false;
        bool changed = false;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = run.TechniqueLineages[i];
            if (lineage == null || lineage.Completeness > 0) continue;
            string sourceId = MclslCultivationCatalog.NormalizeTechniqueId(lineage.SourceTechniqueId);
            bool catalogued = !string.IsNullOrWhiteSpace(sourceId) && MclslCultivationCatalog.TryTechnique(sourceId, out _);
            bool recovered = (lineage.Summary ?? string.Empty).IndexOf("遗迹", StringComparison.Ordinal) >= 0
                || (lineage.Summary ?? string.Empty).IndexOf("复现", StringComparison.Ordinal) >= 0
                || lineage.RevivedYear > 0;
            lineage.Completeness = recovered ? 86 : catalogued ? 100 : 75;
            changed = true;
        }
        return changed;
    }

    private static bool RepairOldWorldSoulObservationText(MclslWorldRunState run)
    {
        if (run?.Events == null) return false;
        bool changed = false;
        for (int i = 0; i < run.Events.Count; i++)
        {
            MclslRunEventRecord record = run.Events[i];
            if (record == null || !IsOldLawYear(run, record.Year)) continue;
            if (!string.Equals(record.EventType, "ancient_world_soul_observation", StringComparison.Ordinal)) continue;
            record.EventType = "ancient_heaven_earth_resonance";
            record.Title = (record.Title ?? string.Empty).Replace("天地之魄", "山河道痕").Replace("现世", "显现");
            record.Body = (record.Body ?? string.Empty).Replace("天地之魄", "山河道痕").Replace("天职", "天地气机");
            changed = true;
        }
        return changed;
    }

    private static bool RepairOldLawRuinFieldLeak(MclslWorldRunState run)
    {
        if (run == null) return false;
        bool changed = false;

        if (run.RuinExplorations != null)
        {
            for (int i = 0; i < run.RuinExplorations.Count; i++)
            {
                MclslRuinExplorationRecord record = run.RuinExplorations[i];
                if (record == null || !IsOldLawYear(run, record.Year)) continue;
                string repairedReward = RepairOldLawRewardText(record.RewardText);
                if (!string.Equals(record.RewardText, repairedReward, StringComparison.Ordinal))
                {
                    record.RewardText = repairedReward;
                    changed = true;
                }
                string repairedSummary = RepairOldLawSummary(record.Summary);
                if (!string.Equals(record.Summary, repairedSummary, StringComparison.Ordinal))
                {
                    record.Summary = repairedSummary;
                    changed = true;
                }
            }
        }

        if (run.Events != null)
        {
            for (int i = 0; i < run.Events.Count; i++)
            {
                MclslRunEventRecord record = run.Events[i];
                if (record == null || !IsOldLawYear(run, record.Year)) continue;
                string repairedBody = RepairOldLawSummary(record.Body);
                if (!string.Equals(record.Body, repairedBody, StringComparison.Ordinal))
                {
                    record.Body = repairedBody;
                    changed = true;
                }
            }
        }

        return changed;
    }

    private static bool IsOldLawYear(MclslWorldRunState run, int year)
    {
        if (run == null) return false;
        if (run.NewLawStartYear > 0) return year < run.NewLawStartYear;
        return !run.NewLawEnabled && !run.TransmissionProved;
    }

    private static string RepairOldLawSummary(string value)
    {
        string text = value ?? string.Empty;
        text = text.Replace("法则平衡，适配0%", "旧法气机与自身传承相互印证")
            .Replace("法则相斥，适配0%", "旧法气机与自身传承相互印证")
            .Replace("法则相近，适配0%", "旧法气机与自身传承相互印证");

        string repairedReward = RepairOldLawRewardText(text);
        return repairedReward;
    }

    private static string RepairOldLawRewardText(string value)
    {
        string text = value ?? string.Empty;
        text = ReplaceRewardPrefix(text, "筑基奇物线索，奇遇概率+", "前人修行札记，功法参悟+");
        text = ReplaceRewardPrefix(text, "洞天残图，下一次洞天争夺强度+", "旧法讲义残页，功法参悟+");
        text = text.Replace("古老洞天线索", "前辈闭关石刻");
        int caveStart = text.IndexOf("发现洞天“", StringComparison.Ordinal);
        if (caveStart >= 0)
        {
            int caveEnd = text.IndexOf('”', caveStart + 5);
            string original = caveEnd >= caveStart ? text.Substring(caveStart, caveEnd - caveStart + 1) : text[caveStart..];
            text = text.Replace(original, "前辈闭关石刻");
        }
        return text;
    }

    private static string ReplaceRewardPrefix(string text, string oldPrefix, string newPrefix)
    {
        int index = text.IndexOf(oldPrefix, StringComparison.Ordinal);
        if (index < 0) return text;
        int numberStart = index + oldPrefix.Length;
        int numberEnd = numberStart;
        while (numberEnd < text.Length && char.IsDigit(text[numberEnd])) numberEnd++;
        string number = numberEnd > numberStart ? text[numberStart..numberEnd] : "0";
        return text[..index] + newPrefix + number + text[numberEnd..];
    }

    private static bool IsAbandonedAutomaticBranch(MclslTechniqueLineageRecord lineage)
    {
        if (lineage == null || string.IsNullOrWhiteSpace(lineage.Id)) return false;
        if (lineage.Id.IndexOf("_branch_", StringComparison.Ordinal) < 0) return false;
        string summary = lineage.Summary ?? string.Empty;
        return summary.IndexOf("避同法相逼", StringComparison.Ordinal) >= 0
            || string.Equals(lineage.State, "支脉初立", StringComparison.Ordinal)
            || string.Equals(lineage.LifecycleState, "法脉支流", StringComparison.Ordinal);
    }

    private static int CountChildren(List<MclslTechniqueLineageRecord> lineages, string parentId)
    {
        if (lineages == null || string.IsNullOrWhiteSpace(parentId)) return 0;
        int count = 0;
        for (int i = 0; i < lineages.Count; i++)
        {
            MclslTechniqueLineageRecord child = lineages[i];
            if (child != null && string.Equals(child.ParentLineageId, parentId, StringComparison.Ordinal)) count++;
        }
        return count;
    }
}
