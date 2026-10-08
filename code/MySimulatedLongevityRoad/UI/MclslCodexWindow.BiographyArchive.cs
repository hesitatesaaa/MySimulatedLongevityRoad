using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private static readonly string[] BiographyViews = { "生平", "修行", "家族宗门", "纪事" };
    private static readonly string[] BiographyViewKeys = { "mclsl_biography_view_life", "mclsl_biography_view_cultivation",
        "mclsl_biography_view_relations", "mclsl_biography_view_events" };
    private string _biographyView = "生平";
    private Vector2 _biographyListScroll;

    private void DrawCultivatorBiographies(MclslWorldRunState run)
    {
        DrawPageHeader(LM.Get("mclsl_history_biography"), "按人物分卷照录在世修士的生平、修行、家门归属与本世纪事。");
        DrawBiographyRealmFilter();
        IReadOnlyList<MclslRankEntry> matches = _biographyMatches
            ?? (IReadOnlyList<MclslRankEntry>)Array.Empty<MclslRankEntry>();
        int pages = Math.Max(1, (matches.Count + BiographyPageSize - 1) / BiographyPageSize);
        if (_biographyScrollRequestActorId > 0)
        {
            int target = _biographyMatches?.IndexOf(_biographyScrollRequestActorId) ?? -1;
            if (target >= 0)
            {
                _biographyPage = target / BiographyPageSize;
                _biographyListScroll.y = target % BiographyPageSize * 38f;
                _biographyScrollRequestActorId = 0;
            }
            else if (_biographyCursor < 0 && _biographyWorldCursor < 0) _biographyScrollRequestActorId = 0;
        }
        _biographyPage = Math.Clamp(_biographyPage, 0, pages - 1);
        if (matches.Count > 0 && (_biographyActorId <= 0 || _biographyMatches?.IndexOf(_biographyActorId) < 0))
            _biographyActorId = matches[_biographyPage * BiographyPageSize].ActorId;

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(255f));
        DrawCardStripe("#8FE3D1");
        GUILayout.Label("<b>◇ " + LM.Get("mclsl_history_biography") + " · " + matches.Count + "</b>");
        GUILayout.Label("搜索姓名");
        string next = GUILayout.TextField(_biographySearch ?? string.Empty, GUILayout.Height(31f));
        if (!string.Equals(next, _biographySearch, StringComparison.Ordinal))
        {
            _biographySearch = next;
            _biographyPage = 0;
            _biographyListScroll = Vector2.zero;
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("清空", GUILayout.Width(55f))) { _biographySearch = string.Empty; _biographyPage = 0; }
        if (GUILayout.Button("上一页", GUILayout.Width(65f)) && _biographyPage > 0)
        { _biographyPage--; _biographyListScroll = Vector2.zero; }
        GUILayout.Label((_biographyPage + 1) + "/" + pages, GUILayout.Width(42f));
        if (GUILayout.Button("下一页", GUILayout.Width(65f)) && _biographyPage + 1 < pages)
        { _biographyPage++; _biographyListScroll = Vector2.zero; }
        GUILayout.EndHorizontal();
        _biographyListScroll = GUILayout.BeginScrollView(_biographyListScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.Height(Mathf.Clamp(_rect.height - 350f, 340f, 640f)));
        if (matches.Count == 0) GUILayout.Label(_biographyCursor >= 0 || _biographyWorldCursor >= 0
            ? "正在照录修士名录……" : "暂无符合条件的在世修士。");
        int first = _biographyPage * BiographyPageSize;
        for (int i = first; i < Math.Min(matches.Count, first + BiographyPageSize); i++)
        {
            MclslRankEntry entry = matches[i];
            if (!MclslActorAccessor.Alive(entry.Actor)) continue;
            Color old = GUI.backgroundColor;
            if (entry.ActorId == _biographyActorId) GUI.backgroundColor = new Color(0.35f, 0.50f, 0.48f);
            if (GUILayout.Button(BiographyPersonalName(entry), GUILayout.Height(34f)))
            {
                _biographyActorId = entry.ActorId;
                _biographyView = "纪事";
            }
            GUI.backgroundColor = old;
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(8f);
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        MclslRankEntry selected = null;
        int selectedIndex = _biographyMatches?.IndexOf(_biographyActorId) ?? -1;
        if (selectedIndex >= 0 && selectedIndex < matches.Count) selected = matches[selectedIndex];
        if (selected?.Actor != null && MclslActorAccessor.Alive(selected.Actor)) DrawBiographyArchive(selected.Actor);
        else GUILayout.Label("选择一位修士查看列传。");
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DrawBiographyRealmFilter()
    {
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label(LM.Get("mclsl_family_realm_filter"), GUILayout.Width(82f));
        DrawBiographyRealmFilterButton(MclslEventCatalog.All, LM.Get("mclsl_family_realm_all"));
        DrawBiographyRealmFilterButton(BiographySensingQiFilter, LM.Get("mclsl_biography_realm_sensing"));
        foreach (string realm in MclslRealmIds.Ordered)
            DrawBiographyRealmFilterButton(realm, MclslRealmIds.Display(realm));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void DrawBiographyRealmFilterButton(string realmId, string label)
    {
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = _biographyRealmFilter == realmId ? new Color(0.35f, 0.50f, 0.48f) : old;
        if (GUILayout.Button(label, GUILayout.Width(76f), GUILayout.Height(32f)) && _biographyRealmFilter != realmId)
        {
            _biographyRealmFilter = realmId;
            _biographyPage = 0;
            _biographyListScroll = Vector2.zero;
            _biographyScrollRequestActorId = 0;
            _biographyMatches = null;
            _biographyNext = null;
            _biographyQuery = null;
            _biographyCursor = 0;
        }
        GUI.backgroundColor = old;
    }

    private void DrawBiographyArchive(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        IReadOnlyList<MclslRunEventRecord> recorded = MclslWorldRunRepository.RecentActorEvents(id);
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe("#8FE3D1");
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b><size=23>" + MclslActorAccessor.DisplayName(actor) + "</size></b>");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(LM.Get("mclsl_biography_open_actor"), GUILayout.Width(92f), GUILayout.Height(34f)))
            try { ActionLibrary.openUnitWindow(actor); } catch { }
        GUILayout.EndHorizontal();
        GUILayout.Label("<color=#9CD7FF>" + LM.Get("mclsl_history_year_span") + " · "
            + (recorded.Count == 0 ? LM.Get("mclsl_history_no_record") : recorded[0].Year + "—" + recorded[recorded.Count - 1].Year)
            + "</color>");
        GUILayout.BeginHorizontal();
        DrawMiniStat(LM.Get("mclsl_history_record_count"), recorded.Count.ToString(), "#CFC7B2", GUILayout.Width(130f));
        DrawMiniStat(LM.Get("mclsl_history_latest_year"), recorded.Count == 0 ? "—" : recorded[recorded.Count - 1].Year.ToString(), "#9CD7FF", GUILayout.Width(155f));
        DrawMiniStat("境界", MclslRealmIds.Display(MclslActorAccessor.Realm(actor)), "#A7E08A", GUILayout.Width(155f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (recorded.Count > 0)
            DrawInfoCard(LM.Get("mclsl_history_recent"), "#FFD37A", () => GUILayout.Label(recorded[recorded.Count - 1].Title));
        GUILayout.BeginHorizontal();
        for (int i = 0; i < BiographyViews.Length; i++)
        {
            string view = BiographyViews[i];
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = view == _biographyView ? new Color(0.34f, 0.49f, 0.47f) : old;
            if (GUILayout.Button(LM.Get(BiographyViewKeys[i]), GUILayout.Height(36f))) _biographyView = view;
            GUI.backgroundColor = old;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(6f);
        switch (_biographyView)
        {
            case "修行":
                DrawInfoCard("修行道途", "#9CD7FF", () =>
                {
                    GUILayout.Label("境界：" + MclslRealmIds.Display(MclslActorAccessor.Realm(actor)));
                    GUILayout.Label("真元：" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0));
                    GUILayout.Label("功法：" + Blank(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty), "未记录"));
                    GUILayout.Label("心境：" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 0));
                    GUILayout.Label("资质：" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.Aptitude, 50));
                });
                break;
            case "家族宗门":
                MclslFamilySystem.Observe(actor);
                MclslFamilyRecord family = MclslFamilySystem.FamilyOf(actor);
                DrawInfoCard("家门与归属", "#A7E08A", () =>
                {
                    GUILayout.Label("所属家族：" + (family?.Name ?? "无"));
                    GUILayout.Label("当前宗门：" + Blank(MclslFactionMissionSystem.CurrentSectName(actor, MclslRuntime.CurrentYear()), "无"));
                    string affiliation = MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty);
                    GUILayout.Label("当前组织：" + (affiliation switch { "wanxian" => "万仙盟", "five_elders" => "五老会", _ => "无" }));
                    GUILayout.Label("旧法宗门出身：" + Blank(MclslFactionMissionSystem.DisplaySectText(
                        MclslActorAccessor.GetString(actor, MclslActorDataKeys.AncientSectOrigin, string.Empty)), "未记录"));
                    if (family != null && GUILayout.Button("进入家族档案", GUILayout.Width(145f), GUILayout.Height(36f)))
                    {
                        MclslCodexTab[] tabs = ActiveTabs();
                        for (int i = 0; i < tabs.Length; i++) if (tabs[i].Title == "修仙家族") { SelectCodexTab(i); break; }
                        _selectedFamilyId = family.Id;
                    }
                });
                break;
            case "纪事":
                DrawInfoCard("本世纪事", "#CFC7B2", () =>
                {
                    IReadOnlyList<MclslRunEventRecord> events = recorded;
                    if (events.Count == 0) GUILayout.Label("暂无个人纪事。");
                    for (int i = events.Count - 1, shown = 0; i >= 0 && shown < 30; i--, shown++)
                    {
                        MclslRunEventRecord record = events[i];
                        DrawHistoryEventCard(record);
                    }
                });
                break;
            default:
                DrawInfoCard("生平概览", "#8FE3D1", () =>
                {
                    GUILayout.BeginHorizontal();
                    DrawMiniStat("境界", MclslRealmIds.Display(MclslActorAccessor.Realm(actor)), "#9CD7FF", GUILayout.Width(160f));
                    DrawMiniStat("年龄", Mathf.FloorToInt(actor.getAge()) + "岁", "#CFC7B2", GUILayout.Width(120f));
                    DrawMiniStat("所在", actor.city?.data?.name ?? "野外", "#A7E08A", GUILayout.Width(180f));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Label("功法道统：" + Blank(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty), "未记录"));
                    string recent = BiographyRecentText(id);
                    if (recent.Length > 0) GUILayout.Label("近年纪事：" + recent);
                    if (MclslMaobaoSystem.IsRecorded(id)) DrawTag("猫宝已刻名", "#D8C778");
                });
                break;
        }
    }
}
