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
    private static readonly string[] FamilyArchiveViews = { "家族总览", "族人名录", "家库传承", "城市影响", "家族纪事" };
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
    private long _familyCachedRevision = -1;
    private string _familyMemberFilter = "在世";
    private int _familyTechniquePage;

    // 世界清除/重载时释放 UI 侧的家族列表缓存（成员/功法/城市缓存由 family 系统自行释放）。
    internal static void ClearFamilyRuntime()
    {
        if (_instance == null) return;
        _instance._familyCachedRun = null;
        _instance._familySorted = new List<MclslFamilyRecord>();
        _instance._familyMatched = new List<MclslFamilyRecord>();
        _instance._familyCachedFilter = null;
        _instance._familyCachedSearch = null;
        _instance._familyCachedCount = -1;
        _instance._familyCachedRevision = -1;
        _instance._selectedFamilyId = string.Empty;
        _instance._selectedFamilyMemberId = 0;
        _instance._familyPage = 0;
        _instance._familyMemberPage = 0;
        _instance._familyTechniquePage = 0;
    }

    private void DrawFamilies(MclslWorldRunState run)
    {
        DrawPageHeader("山河万象 · " + LM.Get("mclsl_codex_families"), "照录家门、族人、传承与城市影响；家族每五年结算一次。");
        DrawFamilyRealmFilter();
        IReadOnlyList<MclslFamilyRecord> source = MclslFamilySystem.Families;
        if (source.Count == 0) { GUILayout.Label("尚无修士家族。有人入道后将建立家族档案。"); return; }
        // 家族 Name 仅在创建与 RepairFamilyNames 赋值（均有 NotifyChanged / 加载），因此只按 Revision+筛选更新，
        // 不再做无变化的 2 秒全量重排。
        if (!ReferenceEquals(_familyCachedRun, run) || _familyCachedCount != source.Count
            || _familyCachedFilter != _familyRealmFilter || _familyCachedRevision != MclslFamilySystem.Revision)
        {
            _familySorted = source.Where(x => x != null)
                .Where(x => _familyRealmFilter == MclslEventCatalog.All || FamilyRealmBucket(x) == _familyRealmFilter)
                .OrderByDescending(x => MclslFamilySystem.MemberSummary(x).HighestRealmIndex)
                .ThenByDescending(x => MclslFamilySystem.MemberSummary(x).Living)
                .ThenBy(x => x.Name, StringComparer.Ordinal).ToList();
            _familyCachedRun = run;
            _familyCachedCount = source.Count;
            _familyCachedRevision = MclslFamilySystem.Revision;
            _familyCachedFilter = _familyRealmFilter;
            _familyCachedSearch = null;
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
            if (GUILayout.Button(family.Name + (family.Extinct ? "　·　绝嗣" : "　·　在世"), GUILayout.Height(34f)))
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

    private static string FamilyRealmBucket(MclslFamilyRecord family)
    {
        int index = MclslFamilySystem.MemberSummary(family).HighestRealmIndex;
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
            case "族人名录": DrawFamilyMembersArchive(family); break;
            case "家库传承": DrawFamilyInheritanceArchive(family); break;
            case "城市影响": DrawFamilyCitiesArchive(family); break;
            case "家族纪事": DrawFamilyHistoryArchive(family); break;
            default: DrawFamilyOverviewArchive(family); break;
        }
    }

    private void DrawFamilyOverviewArchive(MclslFamilyRecord family)
    {
        MclslFamilyMemberRecord head = MclslFamilySystem.FindFamilyMember(family, family.HeadActorId);
        MclslFamilyMemberRecord focus = MclslFamilySystem.FindFamilyMember(family, family.FocusActorId);
        MclslFamilyMemberSummary summary = MclslFamilySystem.MemberSummary(family);
        DrawInfoCard("家门概览", "#A7E08A", () =>
        {
            GUILayout.BeginHorizontal();
            DrawMiniStat("家主", head?.Name ?? "未定", "#FFD37A", GUILayout.Width(210f));
            DrawMiniStat("重点扶持对象", focus?.Name ?? (family.FocusIsManual ? "手动指定" : "自动选择"), "#9CD7FF", GUILayout.Width(210f));
            DrawMiniStat("在世修士", summary.Cultivators.ToString(), "#A7E08A", GUILayout.Width(140f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DrawMiniStat("在世凡人", summary.Mortals.ToString(), "#CFC7B2", GUILayout.Width(160f));
            DrawMiniStat("在世总人数", summary.Living.ToString(), "#A7E08A", GUILayout.Width(160f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DrawMiniStat("家库灵石", MclslFamilySystem.TreasuryStones(family).ToString(), "#FFD37A", GUILayout.Width(160f));
            DrawMiniStat("藏品", family.Items.Sum(x => Math.Max(0, x?.Count ?? 0)).ToString(), "#D8C778", GUILayout.Width(140f));
            DrawMiniStat("功法资料", MclslFamilySystem.TechniqueEntries(family).Count.ToString(), "#B7A7FF", GUILayout.Width(140f));
            DrawMiniStat("影响城镇", family.CityInfluence.Count.ToString(), "#A7E08A", GUILayout.Width(140f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("查看族人", GUILayout.Height(36f))) _familyArchiveView = "族人名录";
            if (GUILayout.Button("查看传承", GUILayout.Height(36f))) _familyArchiveView = "家库传承";
            if (GUILayout.Button("查看城市", GUILayout.Height(36f))) _familyArchiveView = "城市影响";
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (head?.Alive == true && GUILayout.Button("定位家主", GUILayout.Height(36f))
                && MclslActorRegistry.ResolveKnownOrWorld(head.ActorId, out Actor leader))
                try
                {
                    ActionLibrary.openUnitWindow(leader);
                }
                catch (System.Exception mclslEmptyCatchEx)
                {
                    MySimulatedLongevityRoad.Core.MclslDiagnostics.Probe("empty-catch-code-MySimulatedLongevityRoad-UI-MclslCodexWindow-FamilyArchive-cs-1", "仙录家族：定位家主失败: " + mclslEmptyCatchEx.Message);
                }
            if (focus?.Alive == true && GUILayout.Button("定位扶持对象", GUILayout.Height(36f))
                && MclslActorRegistry.ResolveKnownOrWorld(focus.ActorId, out Actor supported))
                try
                {
                    ActionLibrary.openUnitWindow(supported);
                }
                catch (System.Exception mclslEmptyCatchEx)
                {
                    MySimulatedLongevityRoad.Core.MclslDiagnostics.Probe("empty-catch-code-MySimulatedLongevityRoad-UI-MclslCodexWindow-FamilyArchive-cs-2", "仙录家族：定位扶持对象失败: " + mclslEmptyCatchEx.Message);
                }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("扶持方式：" + (family.FocusIsManual ? "手动指定" : "自动择优"));
            if (family.FocusIsManual && GUILayout.Button("恢复自动", GUILayout.Width(110f)))
                MclslFamilySystem.SetAutomaticFocus(family);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("修士每年按需取用家库实物；扶持对象每" + MclslFamilySystem.AidInterval(family) + "年优先取用1件，任意年龄/辈分均可指定。");
        });
        DrawFamilyHistoryArchive(family, 4);
    }

    private void DrawFamilyMembersArchive(MclslFamilyRecord family)
    {
        GUILayout.BeginHorizontal();
        foreach (string filter in new[] { "在世", "已故/失联", "全部" })
            if (GUILayout.Button(filter) && _familyMemberFilter != filter)
            { _familyMemberFilter = filter; _familyMemberPage = 0; }
        GUILayout.EndHorizontal();
        // 直接读取缓存的 Generation+Name 排序视图，绘制时不再 Where/Order/ToList。
        MclslFamilyMemberFilter filterMode = _familyMemberFilter == "全部" ? MclslFamilyMemberFilter.All
            : _familyMemberFilter == "在世" ? MclslFamilyMemberFilter.Living : MclslFamilyMemberFilter.Gone;
        IReadOnlyList<MclslFamilyMemberRecord> members = MclslFamilySystem.MemberView(family, filterMode);
        DrawInfoCard("族人名录", "#9CD7FF", () =>
        {
            int pages = Math.Max(1, (members.Count + 15) / 16);
            _familyMemberPage = Math.Clamp(_familyMemberPage, 0, pages - 1);
            GUILayout.BeginHorizontal();
            GUILayout.Label("共 " + members.Count + " 人 · 第 " + (_familyMemberPage + 1) + "/" + pages + " 页");
            if (_familyMemberPage > 0 && GUILayout.Button("上一页", GUILayout.Width(74f))) _familyMemberPage--;
            if (_familyMemberPage + 1 < pages && GUILayout.Button("下一页", GUILayout.Width(74f))) _familyMemberPage++;
            GUILayout.EndHorizontal();
            if (members.Count == 0) GUILayout.Label("暂无符合筛选条件的族人。");
            for (int i = _familyMemberPage * 16; i < Math.Min(members.Count, (_familyMemberPage + 1) * 16); i++)
            {
                MclslFamilyMemberRecord member = members[i];
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(member.Name, GUILayout.Width(215f));
                DrawTag("第" + member.Generation + "代", "#CFC7B2");
                DrawTag(member.Missing ? "失联" : !member.Alive ? "已故" : member.IsCultivator
                    ? MclslRealmIds.Display(MclslRealmIds.Ordered[Math.Clamp(member.RealmIndex, 0, 6)]) : "凡人", "#9CD7FF");
                GUILayout.FlexibleSpace();
                if (member.ActorId == family.HeadActorId) DrawTag("家主", "#FFD37A");
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && member.Alive && !member.Missing;
                if (GUILayout.Button(LM.Get("mclsl_history_locate"), GUILayout.Width(68f))
                    && MclslActorRegistry.ResolveKnownOrWorld(member.ActorId, out Actor located)
                    && MclslActorAccessor.Alive(located))
                {
                    CloseWindow();
                    MclslEventLocator.Locate(located);
                }
                GUI.enabled = previousEnabled;
                if (GUILayout.Button("查看", GUILayout.Width(68f))) _selectedFamilyMemberId = member.ActorId;
                GUI.enabled = previousEnabled;
                GUILayout.EndHorizontal();
            }
        });
        MclslFamilyMemberRecord managed = MclslFamilySystem.FindFamilyMember(family, _selectedFamilyMemberId);
        DrawFamilyMemberLineage(family, managed);
        if (managed is { Alive: true, Missing: false, IsCultivator: true }
            && MclslActorRegistry.ResolveKnownOrWorld(managed.ActorId, out Actor actor)
            && MclslActorAccessor.Alive(actor)) DrawFamilyMemberControls(family, actor);
    }

    private void DrawFamilyInheritanceArchive(MclslFamilyRecord family)
    {
        MclslFamilyMemberRecord managed = MclslFamilySystem.FindFamilyMember(family, _selectedFamilyMemberId);
        DrawInfoCard("家库藏品", "#D8C778", () =>
        {
            // 同种物品合并显示：复用家库运行时索引的 canonical ItemId 汇总，
            // 不再逐条列出每个实例。数据与实例本身不变，仅合并展示。
            IReadOnlyList<KeyValuePair<string, int>> entries = MclslFamilySystem.FamilyStock(family)?.Aggregate
                ?? (IReadOnlyList<KeyValuePair<string, int>>)Array.Empty<KeyValuePair<string, int>>();
            if (entries.Count == 0) GUILayout.Label("家库暂无模组物品。");
            int shown = 0;
            foreach (KeyValuePair<string, int> entry in entries)
            {
                if (shown++ >= 40) break;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label((MclslItemCatalog.Get(entry.Key)?.Name ?? "未知物品") + " ×" + entry.Value);
                GUILayout.FlexibleSpace();
                if (managed is { Alive: true, Missing: false, IsCultivator: true }
                    && MclslActorRegistry.ResolveKnownOrWorld(managed.ActorId, out Actor recipient)
                    && GUILayout.Button("领用一件", GUILayout.Width(86f))) MclslFamilySystem.TransferItem(family, recipient, entry.Key, false);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("领用物品前，请先在“族人名录”中选择在世修士。");
        });
        DrawInfoCard("功法传承", "#B7A7FF", () =>
        {
            // 持久资料缓存的同一份视图：总览 Count 与 16 条分页共用，顺序稳定。
            IReadOnlyList<KeyValuePair<string, MclslFamilyTechniqueRecord>> entries = MclslFamilySystem.TechniqueEntries(family);
            if (entries.Count == 0) { GUILayout.Label("暂无入录功法。"); return; }
            int pages = Math.Max(1, (entries.Count + 15) / 16);
            _familyTechniquePage = Math.Clamp(_familyTechniquePage, 0, pages - 1);
            GUILayout.BeginHorizontal();
            GUILayout.Label("共 " + entries.Count + " 部 · 第 " + (_familyTechniquePage + 1) + "/" + pages + " 页");
            if (_familyTechniquePage > 0 && GUILayout.Button("上一页", GUILayout.Width(74f))) _familyTechniquePage--;
            if (_familyTechniquePage + 1 < pages && GUILayout.Button("下一页", GUILayout.Width(74f))) _familyTechniquePage++;
            GUILayout.EndHorizontal();
            for (int i = _familyTechniquePage * 16; i < Math.Min(entries.Count, (_familyTechniquePage + 1) * 16); i++)
            {
                KeyValuePair<string, MclslFamilyTechniqueRecord> entry = entries[i];
                MclslFamilyTechniqueRecord record = entry.Value;
                bool unknown = record == null || string.IsNullOrWhiteSpace(record.Name);
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(unknown ? "未识别功法《" + entry.Key + "》" : "《" + record.Name + "》", GUILayout.Width(230f));
                if (record != null && !string.IsNullOrWhiteSpace(record.SystemId)) DrawTag(FamilyTechniqueSystemDisplay(record.SystemId), "#9CD7FF");
                DrawTag(record == null || string.IsNullOrWhiteSpace(record.MaxRealm) ? "上限资料缺失" : "上限" + MclslRealmIds.Display(record.MaxRealm), "#D8C778");
                if (record != null && !string.IsNullOrWhiteSpace(record.LawTags)) GUILayout.Label("法则：" + record.LawTags.Replace(",", "、"));
                GUILayout.EndHorizontal();
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
                // 前3家族由系统缓存在五年结算/影响变化时更新，绘制时不再遍历全部家族。
                IReadOnlyList<KeyValuePair<MclslFamilyRecord, int>> leaders = MclslFamilySystem.CityInfluenceLeaders(city.Key);
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label("<b>" + MclslFamilySystem.FamilyCityName(family, city.Key) + "</b>　·　本族影响 " + city.Value);
                GUILayout.Label(string.Join("　", leaders.Select((x, index) => (index == 0 ? "主家" : "第" + (index + 1) + "家")
                    + " " + x.Key.Name + " " + x.Value)));
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

    private void DrawFamilyMemberLineage(MclslFamilyRecord family, MclslFamilyMemberRecord member)
    {
        if (member == null) return;
        DrawInfoCard("族谱关系 · " + member.Name, "#9CD7FF", () =>
        {
            GUILayout.BeginHorizontal();
            DrawMiniStat("代数", "第" + member.Generation + "代", "#CFC7B2", GUILayout.Width(120f));
            GUILayout.Label("父母：" + FamilyParentLabel(family, member));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            // 子女映射只构建一次：优先 ActorRelations 直接子女，已故/失联用族谱父母补齐。
            IReadOnlyList<MclslFamilyMemberRecord> children = MclslFamilySystem.FamilyChildren(family, member);
            if (children.Count == 0) { GUILayout.Label("本族子女：暂无记录。"); return; }
            GUILayout.Label("本族子女：" + children.Count + " 人（点击跳转）");
            const int perRow = 6;
            for (int start = 0; start < children.Count; start += perRow)
            {
                GUILayout.BeginHorizontal();
                for (int i = start; i < Math.Min(children.Count, start + perRow); i++)
                {
                    MclslFamilyMemberRecord child = children[i];
                    if (GUILayout.Button(child.Name + (child.Alive ? string.Empty : "（已故）"), GUILayout.Width(124f)))
                        JumpFamilyMember(family, child.ActorId);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        });
    }

    // 切换合适的筛选与页码，确保目标在族人名录中可见；使用同一缓存顺序/筛选/分页。
    private void JumpFamilyMember(MclslFamilyRecord family, long actorId)
    {
        _selectedFamilyMemberId = actorId;
        MclslFamilyMemberRecord target = MclslFamilySystem.FindFamilyMember(family, actorId);
        bool living = target != null && target.Alive && !target.Missing;
        _familyMemberFilter = living ? "在世" : "已故/失联";
        IReadOnlyList<MclslFamilyMemberRecord> visible = MclslFamilySystem.MemberView(family,
            living ? MclslFamilyMemberFilter.Living : MclslFamilyMemberFilter.Gone);
        int index = -1;
        for (int i = 0; i < visible.Count; i++)
            if (visible[i] != null && visible[i].ActorId == actorId) { index = i; break; }
        _familyMemberPage = index >= 0 ? index / 16 : 0;
    }

    private static string FamilyParentLabel(MclslFamilyRecord family, MclslFamilyMemberRecord member)
    {
        string first = FamilyParentName(family, member.ParentId1);
        string second = FamilyParentName(family, member.ParentId2);
        if (first.Length == 0 && second.Length == 0) return "未知";
        if (second.Length == 0) return first;
        if (first.Length == 0) return second;
        return first + "、" + second;
    }

    private static string FamilyParentName(MclslFamilyRecord family, long parentId)
    {
        if (parentId <= 0) return string.Empty;
        MclslFamilyMemberRecord parent = MclslFamilySystem.FindFamilyMember(family, parentId);
        if (parent != null && !string.IsNullOrWhiteSpace(parent.Name)) return parent.Name;
        return MclslActorRegistry.ResolveKnownOrWorld(parentId, out Actor actor)
            ? MclslActorAccessor.DisplayName(actor) : "未知";
    }

    private static string FamilyTechniqueSystemDisplay(string systemId)
    {
        if (systemId == MclslCultivationSystemIds.AncientLaw) return "旧法";
        if (systemId == MclslCultivationSystemIds.NewLaw) return "新法";
        return systemId;
    }
}
