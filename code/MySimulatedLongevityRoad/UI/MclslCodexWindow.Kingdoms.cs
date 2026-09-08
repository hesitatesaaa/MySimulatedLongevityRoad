using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private void DrawKingdomDistribution(string title, string empty)
    {
        if (!string.IsNullOrWhiteSpace(_kingdomDetailName))
        {
            DrawKingdomCultivatorDetail();
            return;
        }

        DrawPageHeader(title, "凡俗国度自有兴亡，仙修行迹随国势流转。");
        bool any = false;
        IReadOnlyList<MclslKingdomCodexEntry> entries = _snapshot.KingdomEntries;
        int kingdomLimit = Math.Min(entries.Count, 120);
        for (int i = 0; i < kingdomLimit; i++)
        {
            MclslKingdomCodexEntry entry = entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.Name)) continue;
            any = true;
            DrawInfoCard(entry.Name, "#9CD7FF", () =>
            {
                GUILayout.BeginHorizontal();
                DrawOverviewPill("国名", entry.Name, "#FFD37A", GUILayout.Width(220));
                DrawOverviewPill("修士", entry.TotalCultivators.ToString(), "#9CD7FF", GUILayout.Width(160));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("查看名册", GUILayout.Width(120), GUILayout.Height(32)))
                {
                    _kingdomDetailName = entry.Name;
                    _kingdomRealmFilter = MclslEventCatalog.All;
                    _scroll = Vector2.zero;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                int shown = 0;
                for (int realmIndex = MclslRealmIds.Ordered.Length - 1; realmIndex >= 0; realmIndex--)
                {
                    string realm = MclslRealmIds.Ordered[realmIndex];
                    if (!entry.RealmCounts.TryGetValue(realm, out int count) || count <= 0) continue;
                    DrawTag(MclslRealmIds.Display(realm) + count, RealmTagColor(realm));
                    shown++;
                    if (shown >= 8) break;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });
        }
        if (!any) DrawInfoCard("暂无记录", "#CFC7B2", () => GUILayout.Label(empty));
    }

    private void DrawKingdomCultivatorDetail()
    {
        MclslKingdomCodexEntry entry = FindKingdomEntry(_kingdomDetailName);
        if (entry == null)
        {
            _kingdomDetailName = string.Empty;
            _kingdomRealmFilter = MclslEventCatalog.All;
            DrawKingdomDistribution("原生诸国", "尚无国家拥有修士。");
            return;
        }

        DrawPageHeader(entry.Name + "修士名册", "按境界筛选该国现存修士。");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("返回诸国", GUILayout.Width(120f), GUILayout.Height(32f)))
        {
            _kingdomDetailName = string.Empty;
            _kingdomRealmFilter = MclslEventCatalog.All;
            _scroll = Vector2.zero;
            GUILayout.EndHorizontal();
            return;
        }
        DrawOverviewPill("修士", entry.TotalCultivators.ToString(), "#9CD7FF", GUILayout.Width(160));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(6);
        DrawKingdomRealmFilter(entry);
        GUILayout.Space(8);

        IReadOnlyList<MclslKingdomCultivatorEntry> list = entry.CultivatorsForRealm(_kingdomRealmFilter);
        bool any = false;
        int limit = Math.Min(list.Count, 180);
        for (int i = 0; i < limit; i++)
        {
            MclslKingdomCultivatorEntry cultivator = list[i];
            if (cultivator == null) continue;
            any = true;
            DrawInfoCard(cultivator.Name, RealmTagColor(cultivator.RealmId), () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(cultivator.RealmName, RealmTagColor(cultivator.RealmId));
                DrawTag("真元 " + cultivator.TrueEssence, "#9CD7FF");
                DrawTag("贡献 " + cultivator.Contribution, "#FFD37A");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });
        }
        if (!any) DrawInfoCard("暂无记录", "#CFC7B2", () => GUILayout.Label("当前筛选下暂无修士。"));
    }

    private MclslKingdomCodexEntry FindKingdomEntry(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        IReadOnlyList<MclslKingdomCodexEntry> entries = _snapshot.KingdomEntries;
        for (int i = 0; i < entries.Count; i++)
        {
            MclslKingdomCodexEntry entry = entries[i];
            if (entry != null && string.Equals(entry.Name, name, StringComparison.Ordinal)) return entry;
        }
        return null;
    }

    private void DrawKingdomRealmFilter(MclslKingdomCodexEntry entry)
    {
        GUILayout.BeginHorizontal();
        DrawRealmFilterButton(MclslEventCatalog.All, "全部 " + entry.TotalCultivators, "#CFC7B2");
        for (int i = MclslRealmIds.Ordered.Length - 1; i >= 0; i--)
        {
            string realm = MclslRealmIds.Ordered[i];
            if (!entry.RealmCounts.TryGetValue(realm, out int count) || count <= 0) continue;
            DrawRealmFilterButton(realm, MclslRealmIds.Display(realm) + " " + count, RealmTagColor(realm));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawRealmFilterButton(string realm, string label, string color)
    {
        bool selected = string.Equals(_kingdomRealmFilter, realm, StringComparison.Ordinal);
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = selected ? ParseHexColor(color, Color.gray) : Color.gray;
        if (GUILayout.Button(label, GUILayout.Width(110f), GUILayout.Height(30f)))
        {
            _kingdomRealmFilter = realm;
            _scroll = Vector2.zero;
        }
        GUI.backgroundColor = old;
    }

    private static string RealmTagColor(string realm)
    {
        return realm switch
        {
            MclslRealmIds.ChangSheng => "#F6F0A8",
            MclslRealmIds.HeDao => "#B7A7FF",
            MclslRealmIds.HuaShen => "#FF9B6A",
            MclslRealmIds.YuanYing => "#D8C778",
            MclslRealmIds.JinDan => "#FFD37A",
            MclslRealmIds.ZhuJi => "#A7E08A",
            MclslRealmIds.LianQi => "#9CD7FF",
            _ => "#CFC7B2"
        };
    }
}
