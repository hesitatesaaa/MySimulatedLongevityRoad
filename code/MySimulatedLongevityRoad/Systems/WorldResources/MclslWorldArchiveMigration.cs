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

        if (sourceVersion <= CurrentVersion) upgraded.Version = CurrentVersion;
        return changed;
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
