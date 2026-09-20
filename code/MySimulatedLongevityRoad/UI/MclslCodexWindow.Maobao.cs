using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private List<MclslMaobaoCandidate> _maobaoCandidates = new();
    private string _maobaoFilter = "全部";
    private string _maobaoActionMessage = string.Empty;
    private Vector2 _maobaoCandidateScroll;
    private Vector2 _maobaoRecordScroll;
#pragma warning disable CS0649
    private long _maobaoFocusActorId;
#pragma warning restore CS0649

    internal static void ShowMaobao()
    {
        MclslMaobaoWindow.Show();
    }

    internal static void ShowMaobaoForActor(Actor actor)
    {
        MclslMaobaoWindow.Show(actor, saveActor: true);
    }

    private void DrawMaobao()
    {
        IReadOnlyList<MclslMaobaoArchiveManager.SavedActorPacket> records = MclslMaobaoArchiveManager.GetSavedActors();
        int alive = records.Count(x => IsPacketActorAlive(x));

        DrawPageHeader("猫宝·登名留档", "猫宝按登名石方式保存完整角色快照。保存的角色可以在地图上重新放置，右侧人物栏的猫宝快捷按钮可直接刻名或移除。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("已留名", records.Count + "/" + MclslMaobaoArchiveManager.SavedActorLimit, "#F0D58B", GUILayout.Width(180));
        DrawOverviewPill("仍在世", alive.ToString(), "#8FE3D1", GUILayout.Width(170));
        DrawOverviewPill("候选留影", _maobaoCandidates.Count.ToString(), "#9CD7FF", GUILayout.Width(170));
        DrawOverviewPill("观照年份", MclslRuntime.CurrentYear() + "年", "#B7A7FF", GUILayout.Width(180));
        if (_maobaoFocusActorId > 0L)
        {
            string focusName = "已绑定人物";
            if (MclslCultivatorCandidateIndex.Resolve(_maobaoFocusActorId, out Actor focusActor)) focusName = MclslActorAccessor.DisplayName(focusActor);
            DrawOverviewPill("当前人物", focusName, "#D8C778", GUILayout.Width(220));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        DrawInfoCard("猫宝玉台", "#8FE3D1", () =>
        {
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.24f, 0.43f, 0.39f, 1f);
            if (GUILayout.Button("照见本世强者", GUILayout.Width(180), GUILayout.Height(38)))
            {
                _maobaoCandidates = MclslMaobaoSystem.BuildCandidates();
                _maobaoActionMessage = "猫宝已重新观照本世，列出境界与真元居前的修士。";
            }
            GUI.backgroundColor = new Color(0.34f, 0.31f, 0.22f, 1f);
            if (GUILayout.Button("刷新全部留影", GUILayout.Width(180), GUILayout.Height(38)))
            {
                int count = MclslMaobaoSystem.RefreshRecords();
                _maobaoActionMessage = "已刷新 " + count + " 名仍在世界中的完整角色档案。";
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Label(string.IsNullOrWhiteSpace(_maobaoActionMessage)
                ? "留影是观察档案，不会复制角色数据，也不会改变人物修为。"
                : _maobaoActionMessage);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        });

        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(520), GUILayout.ExpandHeight(true));
        DrawCardStripe("#6FAE9D");
        GUILayout.Label("<size=20><b>当世照影</b></size>");
        GUILayout.Label("<color=#B9B0A0>候选来自现有修士索引，最多十二人；点击刻名后才写入存档。</color>");
        float listHeight = Math.Max(300f, _rect.height - 355f);
        _maobaoCandidateScroll = GUILayout.BeginScrollView(_maobaoCandidateScroll, false, true, GUILayout.Height(listHeight));
        if (_maobaoCandidates.Count == 0)
        {
            GUILayout.Label("猫宝尚未照见可留名的修士。");
        }
        for (int i = 0; i < _maobaoCandidates.Count; i++) DrawMaobaoCandidate(_maobaoCandidates[i], i + 1);
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(8);
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawCardStripe("#D6BE86");
        GUILayout.BeginHorizontal();
        GUILayout.Label("<size=20><b>时序名录</b></size>", GUILayout.Width(180));
        DrawMaobaoFilterButton("全部");
        DrawMaobaoFilterButton("当世");
        DrawMaobaoFilterButton("往昔");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        List<MclslMaobaoArchiveManager.SavedActorPacket> visible = records
            .Where(x => x != null && (_maobaoFilter == "全部" || (_maobaoFilter == "当世" ? IsPacketActorAlive(x) : !IsPacketActorAlive(x))))
            .OrderByDescending(x => x?.SaveTime ?? string.Empty, StringComparer.Ordinal)
            .ToList();
        _maobaoRecordScroll = GUILayout.BeginScrollView(_maobaoRecordScroll, false, true, GUILayout.Height(listHeight));
        if (!visible.Any()) GUILayout.Label("此卷尚无完整登名档案。可从左侧选择修士刻名入宝。 ");
        foreach (MclslMaobaoArchiveManager.SavedActorPacket record in visible) DrawMaobaoRecord(record);
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DrawMaobaoCandidate(MclslMaobaoCandidate candidate, int rank)
    {
        if (candidate == null) return;
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b><color=#F0D58B>第 " + rank + " 席</color>　" + Blank(candidate.Name) + "</b>", GUILayout.Width(270));
        DrawTag(Blank(candidate.RealmName), "#9CD7FF");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label("功法：" + Blank(candidate.TechniqueName) + "｜道途：" + Blank(candidate.CultivationSystemName));
        GUILayout.BeginHorizontal();
        GUILayout.Label("真元 " + candidate.TrueEssence + "｜归属 " + Blank(candidate.FactionName));
        GUILayout.FlexibleSpace();
        bool recorded = MclslMaobaoSystem.IsRecorded(candidate.ActorId);
        if (GUILayout.Button(recorded ? "刷新完整档案" : "刻名入宝", GUILayout.Width(116), GUILayout.Height(32)))
        {
            MclslMaobaoSystem.TryRecord(candidate, out _maobaoActionMessage);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawMaobaoRecord(MclslMaobaoArchiveManager.SavedActorPacket record)
    {
        if (record?.ActorData == null) return;
        bool alive = IsPacketActorAlive(record);
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe(record.SourceActorId == _maobaoFocusActorId ? "#D8C778" : alive ? "#8FE3D1" : "#777D88");
        GUILayout.BeginHorizontal();
        GUILayout.Label("<size=19><b>" + Blank(record.Name) + "</b></size>", GUILayout.Width(250));
        DrawTag(alive ? "当世" : "档案", alive ? "#8FE3D1" : "#A8ABB3");
        string realm = record.SourceActorId > 0L && MclslActorRegistry.ResolveKnownOrWorld(record.SourceActorId, out Actor actor)
            ? MclslActorAccessor.Realm(actor) : string.Empty;
        DrawTag(Blank(realm, "完整快照"), "#FFD37A");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label("保存时间 " + Blank(record.SaveTime) + "｜源角色ID " + record.SourceActorId);
        GUILayout.Label("已保存原生角色数据、物品引用、特质与本 Mod custom_data；放置时会重新生成当前世界编号。");
        GUILayout.BeginHorizontal();
        GUILayout.Label("保存年份 " + record.SavedYear + "｜资源 " + Blank(record.AssetId));
        GUILayout.FlexibleSpace();
        bool canLocate = alive && record.SourceActorId > 0L;
        bool oldEnabled = GUI.enabled;
        GUI.enabled = oldEnabled && canLocate;
        if (GUILayout.Button("定位修士", GUILayout.Width(90), GUILayout.Height(32)))
        {
            if (MclslActorRegistry.ResolveKnownOrWorld(record.SourceActorId, out Actor target))
            {
                CloseWindow();
                try { ActionLibrary.openUnitWindow(target); } catch { }
            }
        }
        GUI.enabled = oldEnabled;
        GUI.backgroundColor = new Color(0.18f, 0.45f, 0.28f, 1f);
        if (GUILayout.Button("放", GUILayout.Width(42), GUILayout.Height(32)))
            _maobaoActionMessage = MclslMaobaoPlaceMode.Begin(record.ActorId) ? "请选择地图位置放置“" + Blank(record.Name) + "”。" : "放置模式启动失败。";
        GUI.backgroundColor = new Color(0.42f, 0.25f, 0.22f, 1f);
        if (GUILayout.Button("拂去留影", GUILayout.Width(110), GUILayout.Height(32)))
            MclslMaobaoSystem.Remove(record.ActorId, out _maobaoActionMessage);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(4);
    }

    private static bool IsPacketActorAlive(MclslMaobaoArchiveManager.SavedActorPacket packet)
    {
        return packet != null && packet.SourceActorId > 0L
            && MclslActorRegistry.ResolveKnownOrWorld(packet.SourceActorId, out Actor actor)
            && MclslActorAccessor.Alive(actor);
    }

    private void DrawMaobaoFilterButton(string filter)
    {
        GUI.backgroundColor = string.Equals(_maobaoFilter, filter, StringComparison.Ordinal)
            ? new Color(0.32f, 0.48f, 0.43f, 1f)
            : new Color(0.24f, 0.25f, 0.27f, 1f);
        if (GUILayout.Button(filter, GUILayout.Width(82), GUILayout.Height(32))) _maobaoFilter = filter;
        GUI.backgroundColor = Color.white;
    }
}
