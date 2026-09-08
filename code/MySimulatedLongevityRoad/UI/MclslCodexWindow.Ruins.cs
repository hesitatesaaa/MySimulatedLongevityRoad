using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private void DrawRuins(MclslWorldRunState run, bool secretOnly = false)
    {
        bool ancientEra = string.Equals(run?.CultivationEpoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal);
        bool privateOnly = ancientEra && !secretOnly;
        DrawPageHeader(secretOnly ? "秘境" : ancientEra ? "遗府" : "宗门遗迹", secretOnly
            ? "秘境随山河灵机浮沉，显世后可寻宗门旧法、灵石与试炼余韵。"
            : ancientEra
                ? "遗府随山河灵机浮沉，显世后可寻残卷、灵石与前人遗藏。"
                : "遗迹会随世局逐步显世。修士按境界、灵根纯度、阅历和贡献度组成限额队伍，获取功法感悟、筑基机缘、洞天线索，亦可能殒落其中。");
        List<MclslSectRuinRecord> ruins = FilterRuinsForPage(_snapshot.RuinsSorted, secretOnly, privateOnly);
        HashSet<string> ruinIds = BuildRuinIdSet(ruins);
        List<MclslRuinExplorationRecord> explorations = FilterExplorationsForPage(_snapshot.RuinExplorationsSorted, ruinIds, true);
        List<MclslTechniqueLineageRecord> lineages = FilterLineagesForPage(_snapshot.TechniqueLineagesSorted, secretOnly, privateOnly);
        GUILayout.BeginHorizontal();
        RuinModeButton(secretOnly ? "秘境" : "遗迹", ruins.Count, 0, "#9CD7FF");
        RuinModeButton("探索记录", explorations.Count, 1, "#FFD37A");
        RuinModeButton("传承", lineages.Count, 2, "#A7E08A");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (_ruinViewMode == 0)
        {
            if (ruins.Count == 0) GUILayout.Label(secretOnly ? "本世尚无秘境显世。" : ancientEra ? "本世尚无遗府显世。" : "本世尚无宗门遗迹显世。");
            foreach (MclslSectRuinRecord ruin in ruins)
            {
                DrawInfoCard(ruin.Name, QualityColor(ruin.Quality), () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag(Quality(ruin.Quality), QualityColor(ruin.Quality));
                    DrawTag(ruin.Category, "#CFC7B2");
                    DrawTag(RuinStateText(ruin), IsRuinOpen(ruin) ? "#A7E08A" : "#B8B8B8");
                    DrawTag(MclslRuinText.DangerBand(ruin.Danger), MclslRuinText.DangerColor(ruin.Danger));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    DrawMiniStat("位置", ruin.NativeKingdomName + "·" + ruin.LocationName, "#CFC7B2", GUILayout.Width(260));
                    DrawMiniStat("法则", ReplaceTags(ruin.LawTags), "#9CD7FF", GUILayout.Width(260));
                    DrawMiniStat("余藏", ruin.RemainingValue.ToString(), "#FFD37A", GUILayout.Width(120));
                    DrawMiniStat("陨落", ruin.CasualtyCount.ToString(), "#FF8877", GUILayout.Width(120));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    if (!string.IsNullOrWhiteSpace(ruin.SourceTechniqueName))
                    {
                        GUILayout.BeginHorizontal();
                        DrawMiniStat("源法", "《" + ruin.SourceTechniqueName + "》", "#FFD37A", GUILayout.Width(260));
                        string sourceState = ancientEra
                            ? RuinStateText(ruin)
                            : ((ruin.SourceTechniqueLostYear > 0 ? ruin.SourceTechniqueLostYear + "年断绝" : "旧传承")
                                + "；" + (ruin.SourceTechniqueRevivedYear > 0 ? ruin.SourceTechniqueRevivedYear + "年复现" : "未复现"));
                        DrawMiniStat("法脉", sourceState, ruin.SourceTechniqueRevivedYear > 0 ? "#A7E08A" : "#CFC7B2", GUILayout.Width(300));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    if (!string.IsNullOrWhiteSpace(ruin.LinkedLineageId))
                    {
                        string lineageName = _snapshot.LineageNameById.TryGetValue(ruin.LinkedLineageId, out string linkedLineageName) ? linkedLineageName : ruin.SourceTechniqueName;
                        string sectName = _snapshot.LineageSectById.TryGetValue(ruin.LinkedLineageId, out string linkedSectName) ? linkedSectName : string.Empty;
                        if (!ancientEra)
                            GUILayout.Label("传承链：" + RuinChainText(ruin, lineageName, sectName));
                    }
                    GUILayout.Label(ancientEra ? AncientRuinSummary(ruin) : RuinSummary(ruin));
                });
            }
            return;
        }

        if (_ruinViewMode == 2)
        {
            if (lineages.Count == 0) GUILayout.Label(secretOnly ? "本世尚无秘境道统入录。" : ancientEra ? "本世尚无遗府私传入录。" : "本世尚无法脉道统入录。");
            foreach (MclslTechniqueLineageRecord lineage in lineages)
            {
                DrawInfoCard(AncientLineageTitle(lineage), LineageColor(lineage.LifecycleState), () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawTag(string.IsNullOrWhiteSpace(lineage.LifecycleState) ? "未定" : lineage.LifecycleState, LineageColor(lineage.LifecycleState));
                    DrawTag("《" + Blank(lineage.Name) + "》", "#FFD37A");
                    DrawTag("上限 " + MclslRealmIds.Display(AncientLineageDisplayMaxRealm(lineage)), "#B7A7FF");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    DrawMiniStat("当前人数", lineage.CurrentPractitioners.ToString(), "#A7E08A", GUILayout.Width(140));
                    DrawMiniStat("传承声势", AncientLineageStrengthLabel(lineage), "#D8C778", GUILayout.Width(140));
                    DrawMiniStat("初见", lineage.FirstSeenYear + "年", "#CFC7B2", GUILayout.Width(150));
                    DrawMiniStat("最近", lineage.LastSeenYear + "年", "#CFC7B2", GUILayout.Width(150));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    DrawMiniStat("法则", ReplaceTags(lineage.LawTags), "#B7A7FF", GUILayout.Width(260));
                    DrawMiniStat("创法者", Blank(lineage.FounderName), "#FFD37A", GUILayout.Width(220));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    if (!string.IsNullOrWhiteSpace(lineage.LinkedRuinId))
                    {
                        string ruinName = _snapshot.RuinNameById.TryGetValue(lineage.LinkedRuinId, out string linkedRuinName) ? linkedRuinName : "宗门遗迹";
                        int revivalCount = _snapshot.LineageRevivalCountsById.TryGetValue(lineage.Id, out int revivals) ? revivals : 0;
                        string relation = string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal) ? "关联秘境" : "关联遗府";
                        GUILayout.Label(relation + "：" + ruinName + (revivalCount > 0 ? "，复现记录 " + revivalCount + " 条。" : "。"));
                    }
                    GUILayout.Label("传承链：" + LineageChainText(lineage));
                    if (!string.IsNullOrWhiteSpace(lineage.Summary))
                        GUILayout.Label(lineage.Summary);
                });
            }
            return;
        }

        if (explorations.Count == 0) GUILayout.Label(secretOnly ? "本世尚无秘境探索记录。" : ancientEra ? "本世尚无遗府探索记录。" : "本世尚无遗迹探索记录。");
        foreach (MclslRuinExplorationRecord record in explorations)
        {
            DrawInfoCard(record.Year + "年｜" + record.ActorName, record.Survived ? "#A7E08A" : "#FF8877", () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(record.RealmName, "#9CD7FF");
                DrawTag(record.Survived ? "生还" : "陨落", record.Survived ? "#A7E08A" : "#FF8877");
                DrawTag(record.RuinName, "#CFC7B2");
                if (!string.IsNullOrWhiteSpace(record.RevivedTechniqueName))
                    DrawTag("复现《" + record.RevivedTechniqueName + "》", "#FFD37A");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (!string.IsNullOrWhiteSpace(record.LinkedLineageId))
                {
                    string lineageName = _snapshot.LineageNameById.TryGetValue(record.LinkedLineageId, out string linkedLineageName) ? linkedLineageName : record.RevivedTechniqueName;
                    string sectName = _snapshot.LineageSectById.TryGetValue(record.LinkedLineageId, out string linkedSectName) ? linkedSectName : string.Empty;
                    GUILayout.Label("承接：" + (string.IsNullOrWhiteSpace(sectName) ? "无名道统" : sectName) + "《" + Blank(lineageName) + "》");
                }
                GUILayout.Label(record.Summary);
            });
        }
    }

    private void RuinModeButton(string label, int count, int mode, string color)
    {
        GUI.backgroundColor = _ruinViewMode == mode ? ParseHexColor(color, Color.gray) : Color.gray;
        if (GUILayout.Button(label + "\n" + count, GUILayout.Width(170), GUILayout.Height(58f)))
        {
            _ruinViewMode = mode;
            _scroll = Vector2.zero;
        }
        GUI.backgroundColor = Color.white;
    }

    private static List<MclslSectRuinRecord> FilterRuinsForPage(List<MclslSectRuinRecord> source, bool secretOnly, bool privateOnly)
    {
        List<MclslSectRuinRecord> filtered = new(source?.Count ?? 0);
        if (source == null) return filtered;
        for (int i = 0; i < source.Count; i++)
        {
            MclslSectRuinRecord ruin = source[i];
            if (ruin == null) continue;
            bool secret = IsSecretRuinPageRecord(ruin);
            if (secretOnly ? secret : !secret)
                filtered.Add(ruin);
        }
        return filtered;
    }

    private static HashSet<string> BuildRuinIdSet(List<MclslSectRuinRecord> ruins)
    {
        HashSet<string> ids = new(StringComparer.Ordinal);
        if (ruins == null) return ids;
        for (int i = 0; i < ruins.Count; i++)
        {
            string id = ruins[i]?.Id;
            if (!string.IsNullOrWhiteSpace(id)) ids.Add(id);
        }
        return ids;
    }

    private static List<MclslRuinExplorationRecord> FilterExplorationsForPage(List<MclslRuinExplorationRecord> source, HashSet<string> ruinIds, bool filteredPage)
    {
        if (!filteredPage) return source ?? new List<MclslRuinExplorationRecord>();
        List<MclslRuinExplorationRecord> filtered = new(source?.Count ?? 0);
        if (source == null || ruinIds == null || ruinIds.Count == 0) return filtered;
        for (int i = 0; i < source.Count; i++)
        {
            MclslRuinExplorationRecord record = source[i];
            if (record == null) continue;
            if (!string.IsNullOrWhiteSpace(record.RuinId) && ruinIds.Contains(record.RuinId))
                filtered.Add(record);
        }
        return filtered;
    }

    private static List<MclslTechniqueLineageRecord> FilterLineagesForPage(List<MclslTechniqueLineageRecord> source, bool secretOnly, bool privateOnly)
    {
        List<MclslTechniqueLineageRecord> filtered = new(source?.Count ?? 0);
        if (source == null) return filtered;
        for (int i = 0; i < source.Count; i++)
        {
            MclslTechniqueLineageRecord lineage = source[i];
            if (lineage == null) continue;
            bool secret = string.Equals(lineage.LifecycleState, "秘境道统", StringComparison.Ordinal);
            bool privateLine = string.Equals(lineage.LifecycleState, "遗府私传", StringComparison.Ordinal)
                || string.Equals(lineage.LifecycleState, "遗府留痕", StringComparison.Ordinal);
            if (secretOnly && secret) filtered.Add(lineage);
            else if (privateOnly && privateLine) filtered.Add(lineage);
            else if (!secretOnly && !privateOnly && !secret) filtered.Add(lineage);
        }
        return filtered;
    }

    private static bool IsSecretRuinPageRecord(MclslSectRuinRecord ruin)
    {
        if (ruin == null) return false;
        return ContainsOrdinal(ruin.Category, "秘境")
            || ContainsOrdinal(ruin.Name, "秘境")
            || ContainsOrdinal(ruin.Description, "秘境");
    }

    private static bool ContainsOrdinal(string text, string value)
    {
        return !string.IsNullOrWhiteSpace(text)
            && text.IndexOf(value, StringComparison.Ordinal) >= 0;
    }

    private static string RuinStateText(MclslSectRuinRecord ruin)
    {
        if (ruin == null) return "未详";
        if (string.Equals(ruin.State, "活跃", StringComparison.Ordinal)) return "未显世";
        if (string.IsNullOrWhiteSpace(ruin.State)) return "未显世";
        return ruin.State;
    }

    private static string RuinSummary(MclslSectRuinRecord ruin)
    {
        if (ruin == null) return "无详载。";
        if (!string.IsNullOrWhiteSpace(ruin.Description)) return ruin.Description;
        return "此地尚存旧日传承余韵。";
    }

    private static string AncientRuinSummary(MclslSectRuinRecord ruin)
    {
        if (ruin == null) return "无详载。";
        string technique = string.IsNullOrWhiteSpace(ruin.SourceTechniqueName) ? "无名道法" : "《" + ruin.SourceTechniqueName + "》";
        string state = RuinStateText(ruin);
        string laws = ReplaceTags(ruin.LawTags);
        return technique + "遗韵沉于此地，状态：" + state + "；法则：" + laws + "；余藏：" + Math.Max(0, ruin.RemainingValue) + "。";
    }
}
