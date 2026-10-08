using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private static readonly string[] FamilyArchiveViews = { "家族总览", "在世族人", "家库传承", "城市影响", "家族纪事" };
    private static readonly string[] FamilyArchiveViewKeys = { "mclsl_family_view_overview", "mclsl_family_view_members",
        "mclsl_family_view_inheritance", "mclsl_family_view_cities", "mclsl_family_view_history" };
    private string _familyArchiveView = "家族总览";
    private string _familySearch = string.Empty;
    private Vector2 _familySelectorScroll;
    private MclslWorldRunState _familyCachedRun;
    private List<MclslFamilyRecord> _familySorted = new();
    private List<MclslFamilyRecord> _familyMatched = new();
    private string _familyCachedFilter, _familyCachedSearch;
    private int _familyCachedCount = -1;
    private float _familyNextRefresh;

    private void DrawFamilies(MclslWorldRunState run)
    {
        DrawPageHeader("山河万象 · " + LM.Get("mclsl_codex_families"), "照录家门、族人、传承与城市影响；家族每五年结算一次。");
        DrawFamilyRealmFilter();
        IReadOnlyList<MclslFamilyRecord> source = MclslFamilySystem.Families;
        if (source.Count == 0) { GUILayout.Label("尚无修士家族。有人入道后将建立家族档案。"); return; }
        if (!ReferenceEquals(_familyCachedRun, run) || _familyCachedCount != source.Count
            || _familyCachedFilter != _familyRealmFilter || Time.unscaledTime >= _familyNextRefresh)
        {
            _familySorted = source.Where(x => x != null)
                .Where(x => _familyRealmFilter == MclslEventCatalog.All || FamilyRealmBucket(x) == _familyRealmFilter)
                .OrderByDescending(FamilyHighestRealmIndex)
                .ThenByDescending(x => x.Members.Count(m => m?.Alive == true))
                .ThenBy(x => x.Name, StringComparer.Ordinal).ToList();
            _familyCachedRun = run;
            _familyCachedCount = source.Count;
            _familyCachedFilter = _familyRealmFilter;
            _familyCachedSearch = null;
            _familyNextRefresh = Time.unscaledTime + 2f;
        }
        List<MclslFamilyRecord> families = _familySorted;
        if (families.Count == 0) { GUILayout.Label(LM.Get("mclsl_family_realm_empty")); return; }
        if (!families.Any(x => x.Id == _selectedFamilyId)) _selectedFamilyId = families[0].Id;
        MclslFamilyRecord selected = families.Find(x => x.Id == _selectedFamilyId);

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical(XianLuUIStyles.PanelStyle,
            GUILayout.Width(Mathf.Clamp(_rect.width * 0.22f, 220f, 290f)));
        DrawCardStripe("#A7E08A");
        GUILayout.Label("<b>家族名录 · " + families.Count + "</b>");
        _familySearch = GUILayout.TextField(_familySearch ?? string.Empty, GUILayout.Height(31f));
        if (_familyCachedSearch != _familySearch)
        {
            string query = _familySearch.Trim();
            _familyMatched = query.Length == 0 ? families
                : families.Where(x => x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            _familyCachedSearch = _familySearch;
        }
        List<MclslFamilyRecord> matched = _familyMatched;
        int pages = Math.Max(1, (matched.Count + 19) / 20);
        _familyPage = Math.Clamp(_familyPage, 0, pages - 1);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("上一页", GUILayout.Width(72f)) && _familyPage > 0) _familyPage--;
        GUILayout.Label((_familyPage + 1) + " / " + pages, GUILayout.Width(60f));
        if (GUILayout.Button("下一页", GUILayout.Width(72f)) && _familyPage + 1 < pages) _familyPage++;
        GUILayout.EndHorizontal();
        _familySelectorScroll = GUILayout.BeginScrollView(_familySelectorScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.Height(Mathf.Clamp(_rect.height - 330f, 340f, 650f)));
        if (matched.Count == 0) GUILayout.Label("没有找到家族。");
        for (int i = _familyPage * 20; i < Math.Min(matched.Count, (_familyPage + 1) * 20); i++)
        {
            MclslFamilyRecord family = matched[i];
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = family.Id == _selectedFamilyId ? new Color(0.36f, 0.49f, 0.40f) : old;
            if (GUILayout.Button(family.Name + "　·　在世", GUILayout.Height(34f)))
            {
                _selectedFamilyId = family.Id;
                _selectedFamilyMemberId = 0;
                _familyMemberPage = 0;
                _familyArchiveView = "家族总览";
                _scroll = Vector2.zero;
            }
            GUI.backgroundColor = old;
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(8f);
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (selected != null) DrawFamilyArchive(selected);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private static int FamilyHighestRealmIndex(MclslFamilyRecord family)
    {
        int highest = -1;
        if (family?.Members == null) return highest;
        foreach (MclslFamilyMemberRecord member in family.Members)
            if (member?.Alive == true && member.RealmIndex > highest && member.RealmIndex < MclslRealmIds.Ordered.Length)
                highest = member.RealmIndex;
        return highest;
    }

    private static string FamilyRealmBucket(MclslFamilyRecord family)
    {
        int index = FamilyHighestRealmIndex(family);
        return index < 0 ? "remnant" : MclslRealmIds.Ordered[index];
    }

    private void DrawFamilyRealmFilter()
    {
        int perRow = Mathf.Max(3, Mathf.FloorToInt((_rect.width - 100f) / 82f));
        int column = 0;
        GUILayout.BeginVertical(XianLuUIStyles.PanelStyle);
        GUILayout.Label(LM.Get("mclsl_family_realm_filter"), XianLuUIStyles.SectionHeaderStyle);
        GUILayout.BeginHorizontal();
        DrawFamilyRealmFilterButton(MclslEventCatalog.All, LM.Get("mclsl_family_realm_all"));
        column++;
        DrawFamilyRealmFilterButton("remnant", LM.Get("mclsl_family_realm_remnant"));
        column++;
        foreach (string realm in MclslRealmIds.Ordered)
        {
            if (column >= perRow) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); column = 0; }
            DrawFamilyRealmFilterButton(realm, MclslRealmIds.Display(realm));
            column++;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawFamilyRealmFilterButton(string realmId, string label)
    {
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = _familyRealmFilter == realmId ? new Color(0.39f, 0.54f, 0.44f) : old;
        if (GUILayout.Button(label, GUILayout.Width(76f), GUILayout.Height(32f)) && _familyRealmFilter != realmId)
        {
            _familyRealmFilter = realmId;
            _familyPage = 0;
            _familySelectorScroll = Vector2.zero;
        }
        GUI.backgroundColor = old;
    }

    private void DrawFamilyArchive(MclslFamilyRecord family)
    {
        GUILayout.BeginVertical(XianLuUIStyles.CardStyle);
        DrawCardStripe("#A7E08A");
        GUILayout.Label("<b><size=23>" + family.Name + "</size></b>　·　家族档案");
        GUILayout.BeginHorizontal();
        for (int i = 0; i < FamilyArchiveViews.Length; i++)
        {
            string view = FamilyArchiveViews[i];
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = view == _familyArchiveView ? new Color(0.39f, 0.54f, 0.44f) : old;
            if (GUILayout.Button(LM.Get(FamilyArchiveViewKeys[i]), GUILayout.Height(36f))) { _familyArchiveView = view; _scroll = Vector2.zero; }
            GUI.backgroundColor = old;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(6f);
        switch (_familyArchiveView)
        {
            case "在世族人": DrawFamilyMembersArchive(family); break;
            case "家库传承": DrawFamilyInheritanceArchive(family); break;
            case "城市影响": DrawFamilyCitiesArchive(family); break;
            case "家族纪事": DrawFamilyHistoryArchive(family); break;
            default: DrawFamilyOverviewArchive(family); break;
        }
    }

    private void DrawFamilyOverviewArchive(MclslFamilyRecord family)
    {
        MclslFamilyMemberRecord head = family.Members.Find(x => x?.ActorId == family.HeadActorId);
        MclslFamilyMemberRecord focus = family.Members.Find(x => x?.ActorId == family.FocusActorId);
        DrawInfoCard("家门概览", "#A7E08A", () =>
        {
            GUILayout.BeginHorizontal();
            DrawMiniStat("家主", head?.Name ?? "未定", "#FFD37A", GUILayout.Width(210f));
            DrawMiniStat("重点扶持", focus?.Name ?? "自动选择", "#9CD7FF", GUILayout.Width(210f));
            DrawMiniStat("在世修士", family.Members.Count(x => x?.Alive == true).ToString(), "#A7E08A", GUILayout.Width(140f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DrawMiniStat("家库灵石", MclslFamilySystem.TreasuryStones(family).ToString(), "#FFD37A", GUILayout.Width(160f));
            DrawMiniStat("藏品", family.Items.Sum(x => Math.Max(0, x?.Count ?? 0)).ToString(), "#D8C778", GUILayout.Width(140f));
            DrawMiniStat("功法资料", family.TechniqueIds.Count.ToString(), "#B7A7FF", GUILayout.Width(140f));
            DrawMiniStat("影响城镇", family.CityInfluence.Count.ToString(), "#A7E08A", GUILayout.Width(140f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("查看族人", GUILayout.Height(36f))) _familyArchiveView = "在世族人";
            if (GUILayout.Button("查看传承", GUILayout.Height(36f))) _familyArchiveView = "家库传承";
            if (GUILayout.Button("查看城市", GUILayout.Height(36f))) _familyArchiveView = "城市影响";
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (head?.Alive == true && GUILayout.Button("定位家主", GUILayout.Height(36f))
                && MclslActorRegistry.ResolveKnownOrWorld(head.ActorId, out Actor leader))
                try { ActionLibrary.openUnitWindow(leader); } catch { }
            if (focus?.Alive == true && GUILayout.Button("定位扶持对象", GUILayout.Height(36f))
                && MclslActorRegistry.ResolveKnownOrWorld(focus.ActorId, out Actor supported))
                try { ActionLibrary.openUnitWindow(supported); } catch { }
            GUILayout.EndHorizontal();
        });
        DrawFamilyHistoryArchive(family, 4);
    }

    private void DrawFamilyMembersArchive(MclslFamilyRecord family)
    {
        List<MclslFamilyMemberRecord> members = family.Members.Where(x => x?.Alive == true)
            .OrderBy(x => x.Generation).ThenBy(x => x.Name, StringComparer.Ordinal).ToList();
        DrawInfoCard("族人名录", "#9CD7FF", () =>
        {
            int pages = Math.Max(1, (members.Count + 15) / 16);
            _familyMemberPage = Math.Clamp(_familyMemberPage, 0, pages - 1);
            GUILayout.BeginHorizontal();
            GUILayout.Label("共 " + members.Count + " 人 · 第 " + (_familyMemberPage + 1) + "/" + pages + " 页");
            if (_familyMemberPage > 0 && GUILayout.Button("上一页", GUILayout.Width(74f))) _familyMemberPage--;
            if (_familyMemberPage + 1 < pages && GUILayout.Button("下一页", GUILayout.Width(74f))) _familyMemberPage++;
            GUILayout.EndHorizontal();
            if (members.Count == 0) GUILayout.Label("暂无在世族人。");
            for (int i = _familyMemberPage * 16; i < Math.Min(members.Count, (_familyMemberPage + 1) * 16); i++)
            {
                MclslFamilyMemberRecord member = members[i];
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(member.Name, GUILayout.Width(215f));
                DrawTag("第" + member.Generation + "代", "#CFC7B2");
                DrawTag(MclslRealmIds.Display(MclslRealmIds.Ordered[Math.Clamp(member.RealmIndex, 0, 6)]), "#9CD7FF");
                GUILayout.FlexibleSpace();
                if (member.ActorId == family.HeadActorId) DrawTag("家主", "#FFD37A");
                if (GUILayout.Button(LM.Get("mclsl_history_locate"), GUILayout.Width(68f))
                    && MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor located)
                    && MclslActorAccessor.Alive(located))
                {
                    CloseWindow();
                    MclslEventLocator.Locate(located);
                }
                if (GUILayout.Button("管理", GUILayout.Width(68f))) _selectedFamilyMemberId = member.ActorId;
                GUILayout.EndHorizontal();
            }
        });
        MclslFamilyMemberRecord managed = family.Members.Find(x => x?.ActorId == _selectedFamilyMemberId);
        if (managed?.Alive == true && MclslActorRegistry.ResolveKnownOrWorld(managed.ActorId, out Actor actor)
            && MclslActorAccessor.Alive(actor)) DrawFamilyMemberControls(family, actor);
    }

    private void DrawFamilyInheritanceArchive(MclslFamilyRecord family)
    {
        MclslFamilyMemberRecord managed = family.Members.Find(x => x?.ActorId == _selectedFamilyMemberId);
        DrawInfoCard("家库藏品", "#D8C778", () =>
        {
            if (family.Items.Count == 0) GUILayout.Label("家库暂无模组物品。");
            foreach (MclslOwnedItem item in family.Items.Where(x => x != null).Take(40).ToArray())
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label((MclslItemCatalog.Get(item.ItemId)?.Name ?? "未知物品") + " ×" + item.Count);
                GUILayout.FlexibleSpace();
                if (managed?.Alive == true && MclslActorRegistry.ResolveKnownOrWorld(managed.ActorId, out Actor recipient)
                    && GUILayout.Button("领用一件", GUILayout.Width(86f))) MclslFamilySystem.TransferItem(family, recipient, item.ItemId, false);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("领用物品前，请先在“在世族人”中选择管理对象。");
        });
        DrawInfoCard("功法传承", "#B7A7FF", () =>
        {
            if (family.TechniqueIds.Count == 0) GUILayout.Label("暂无入录功法。");
            foreach (string id in family.TechniqueIds)
            {
                MclslTechniqueDefinition technique = MclslCultivationCatalog.Techniques.FirstOrDefault(x => x.Id == id);
                if (technique != null) GUILayout.Label("《" + technique.Name + "》");
            }
        });
    }

    private void DrawFamilyCitiesArchive(MclslFamilyRecord family)
    {
        DrawInfoCard("城市影响", "#A7E08A", () =>
        {
            if (family.CityInfluence.Count == 0) GUILayout.Label("暂无城市影响记录；每五年结算一次。");
            foreach (var city in family.CityInfluence.OrderByDescending(x => x.Value))
            {
                var leaders = MclslFamilySystem.Families.Where(x => x?.CityInfluence?.ContainsKey(city.Key) == true)
                    .OrderByDescending(x => x.CityInfluence[city.Key]).ThenBy(x => x.Id, StringComparer.Ordinal).Take(3).ToList();
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("<b>" + FamilyCityName(family, city.Key) + "</b>　·　本族影响 " + city.Value);
                GUILayout.Label(string.Join("　", leaders.Select((x, index) => (index == 0 ? "主家" : "第" + (index + 1) + "家")
                    + " " + x.Name + " " + x.CityInfluence[city.Key])));
                GUILayout.EndVertical();
            }
        });
    }

    private void DrawFamilyHistoryArchive(MclslFamilyRecord family, int limit = 30)
    {
        DrawInfoCard("家族纪事", "#CFC7B2", () =>
        {
            if (limit >= 30 && GUILayout.Button(LM.Get("mclsl_history_open_family"), GUILayout.Width(170f), GUILayout.Height(34f)))
                OpenFamilyHistoryBook(family.Id);
            if (family.History.Count == 0) GUILayout.Label("尚无家族纪事。");
            foreach (string entry in family.History.AsEnumerable().Reverse().Take(limit))
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(entry);
                GUILayout.EndHorizontal();
            }
        });
    }
}
