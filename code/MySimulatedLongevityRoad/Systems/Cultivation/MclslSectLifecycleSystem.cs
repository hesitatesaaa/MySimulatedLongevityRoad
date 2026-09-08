using System;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslSectLifecycleSystem
{
    private const int MaxEventsPerYear = 3;

    internal static void ResolveAnnual(int year)
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        if (run?.TechniqueLineages == null || run.TechniqueLineages.Count == 0) return;
        if (run.LastSectLifecycleYear == year) return;

        int emitted = 0;
        bool changed = false;
        for (int i = 0; i < run.TechniqueLineages.Count; i++)
        {
            MclslTechniqueLineageRecord record = run.TechniqueLineages[i];
            if (record == null) continue;

            if (string.IsNullOrWhiteSpace(record.SectDisplayName))
            {
                record.SectDisplayName = BuildDisplayName(record);
                changed = true;
            }

            string nextState = DetermineState(record);
            if (string.Equals(record.LifecycleState, nextState, StringComparison.Ordinal)) continue;

            string previous = record.LifecycleState ?? string.Empty;
            record.LifecycleState = nextState;
            record.LifecycleYear = Math.Max(0, year);
            changed = true;

            if (emitted >= MaxEventsPerYear || record.LastLifecycleEventYear == year || IsTrivialFirstSight(record, previous, nextState))
                continue;

            EmitLifecycleEvent(year, record, nextState);
            record.LastLifecycleEventYear = year;
            emitted++;
        }

        run.LastSectLifecycleYear = year;
        if (changed) MclslWorldArchiveStore.MarkDirty();
    }

    private static bool IsTrivialFirstSight(MclslTechniqueLineageRecord record, string previous, string nextState)
    {
        return string.IsNullOrWhiteSpace(previous)
            && nextState == "法脉流传"
            && record.PeakPractitioners < 8
            && MclslRealmIds.Index(record.PeakRealm) < MclslRealmIds.Index(MclslRealmIds.JinDan);
    }

    private static string DetermineState(MclslTechniqueLineageRecord record)
    {
        if (record.LifecycleState == "遗府私传" || record.LifecycleState == "秘境道统") return record.LifecycleState;
        if (record.State == "复现") return "遗法复现";
        if (record.CurrentPractitioners <= 0)
        {
            if (!string.IsNullOrWhiteSpace(record.LinkedRuinId)) return "遗府私传";
            return "道统断绝";
        }

        int peakRealm = Math.Max(0, MclslRealmIds.Index(record.PeakRealm));
        if (record.CurrentPractitioners >= 40 || peakRealm >= MclslRealmIds.Index(MclslRealmIds.YuanYing))
            return "道统鼎盛";
        if (record.CurrentPractitioners >= 8 || peakRealm >= MclslRealmIds.Index(MclslRealmIds.JinDan))
            return "传承兴起";
        return "法脉流传";
    }

    private static void EmitLifecycleEvent(int year, MclslTechniqueLineageRecord record, string state)
    {
        string sect = string.IsNullOrWhiteSpace(record.SectDisplayName) ? BuildDisplayName(record) : record.SectDisplayName;
        string realm = MclslRealmIds.Display(record.PeakRealm);
        string type;
        string title;
        string body;

        switch (state)
        {
            case "道统鼎盛":
                type = "sect_lifecycle_peak";
                title = sect + "鼎盛";
                body = "《" + record.Name + "》传者" + record.CurrentPractitioners + "人，最高已至" + realm + "。";
                break;
            case "传承兴起":
                type = "sect_lifecycle_rise";
                title = sect + "渐兴";
                body = "《" + record.Name + "》传承渐广，最高已至" + realm + "。";
                break;
            case "道统断绝":
                type = "sect_lifecycle_fall";
                title = sect + "断绝";
                body = "《" + record.Name + "》当世无传，旧脉归寂。";
                break;
            case "遗府留痕":
                type = "sect_lifecycle_relic";
                title = sect + "留痕";
                body = "《" + record.Name + "》旧脉虽绝，仍有遗府残卷留世。";
                break;
            case "遗府私传":
                type = "sect_lifecycle_private_relic";
                title = sect + "留痕";
                body = "《" + record.Name + "》旧脉虽绝，前人私藏仍有残卷留世。";
                break;
            case "秘境道统":
                type = "sect_lifecycle_secret_lineage";
                title = sect + "留传";
                body = "《" + record.Name + "》随旧日宗门秘境留存，尚待后人续接。";
                break;
            case "遗法复现":
                type = "sect_lifecycle_revived";
                title = "《" + record.Name + "》复现";
                body = "旧法重见天日，" + sect + "再入玄黄档案。";
                break;
            default:
                return;
        }

        MclslWorldRunRepository.AddEvent(year, type, title, body);
    }

    private static string BuildDisplayName(MclslTechniqueLineageRecord record)
    {
        string name = record?.Name ?? string.Empty;
        name = name.Replace("《", string.Empty).Replace("》", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "无名";
        if (name.EndsWith("诀", StringComparison.Ordinal)
            || name.EndsWith("经", StringComparison.Ordinal)
            || name.EndsWith("功", StringComparison.Ordinal)
            || name.EndsWith("法", StringComparison.Ordinal))
            name = name.Substring(0, Math.Max(1, name.Length - 1));
        return name + "道统";
    }
}
