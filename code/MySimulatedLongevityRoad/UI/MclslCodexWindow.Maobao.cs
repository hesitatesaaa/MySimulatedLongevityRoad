using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
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

    internal static void ShowMaobao()
    {
        if (_instance == null)
        {
            GameObject host = new("MclslCodexWindow");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<MclslCodexWindow>();
        }
        MclslWorldRunRepository.EnsureCurrentRun(MclslRuntime.CurrentYear());
        _instance._snapshot = MclslCodexSnapshot.Build();
        _instance._rect = FitRect();
        _instance._standaloneHuanzhenSpace = false;
        _instance._standaloneMaobao = true;
        _instance._scroll = Vector2.zero;
        _instance._maobaoCandidateScroll = Vector2.zero;
        _instance._maobaoRecordScroll = Vector2.zero;
        _instance._maobaoCandidates = MclslMaobaoSystem.BuildCandidates();
        _instance._maobaoActionMessage = string.Empty;
        _instance._visible = true;
        _instance.enabled = true;
        CreateOverlayBlocker();
        _instance.ApplyCodexPause();
    }

    private void DrawMaobao()
    {
        MclslWorldRunState run = MclslWorldRunRepository.Current;
        List<MclslMaobaoRecord> records = run?.MaobaoRecords ?? new List<MclslMaobaoRecord>();
        int alive = records.Count(x => x != null && x.Alive);

        DrawPageHeader("猫宝·时序留名", "猫宝观照当世修士，将一刻境界、功法与所在位置刻入时序玉简。它只在打开或手动刷新时读取现有修士索引，不会常驻扫描世界。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("已留名", records.Count + "/" + MclslMaobaoSystem.MaxRecords, "#F0D58B", GUILayout.Width(180));
        DrawOverviewPill("仍在世", alive.ToString(), "#8FE3D1", GUILayout.Width(170));
        DrawOverviewPill("候选留影", _maobaoCandidates.Count.ToString(), "#9CD7FF", GUILayout.Width(170));
        DrawOverviewPill("观照年份", MclslRuntime.CurrentYear() + "年", "#B7A7FF", GUILayout.Width(180));
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
                _maobaoActionMessage = "已刷新 " + count + " 道仍在世的留影；失去踪迹者转入往昔卷。";
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
        List<MclslMaobaoRecord> visible = records
            .Where(x => x != null && (_maobaoFilter == "全部" || (_maobaoFilter == "当世" ? x.Alive : !x.Alive)))
            .OrderByDescending(x => x.LastObservedYear)
            .ThenByDescending(x => MclslRealmIds.Index(x.RealmId))
            .ToList();
        _maobaoRecordScroll = GUILayout.BeginScrollView(_maobaoRecordScroll, false, true, GUILayout.Height(listHeight));
        if (!visible.Any()) GUILayout.Label("此卷尚无留名。可从左侧选择修士刻入猫宝。 ");
        foreach (MclslMaobaoRecord record in visible) DrawMaobaoRecord(record);
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
        if (GUILayout.Button(recorded ? "刷新留影" : "刻名入宝", GUILayout.Width(116), GUILayout.Height(32)))
        {
            MclslMaobaoSystem.TryRecord(candidate, out _maobaoActionMessage);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawMaobaoRecord(MclslMaobaoRecord record)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe(record.Alive ? "#8FE3D1" : "#777D88");
        GUILayout.BeginHorizontal();
        GUILayout.Label("<size=19><b>" + Blank(record.ActorName) + "</b></size>", GUILayout.Width(250));
        DrawTag(record.Alive ? "当世" : "往昔", record.Alive ? "#8FE3D1" : "#A8ABB3");
        DrawTag(Blank(record.RealmName), "#FFD37A");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label(record.Inscription);
        GUILayout.Label("初录 " + record.FirstRecordedYear + "年｜末观 " + record.LastObservedYear + "年｜功法 " + Blank(record.TechniqueName));
        GUILayout.BeginHorizontal();
        GUILayout.Label("道途 " + Blank(record.CultivationSystemName) + "｜归属 " + Blank(record.FactionName));
        GUILayout.FlexibleSpace();
        bool canLocate = MclslEventLocator.CanLocate(record);
        bool oldEnabled = GUI.enabled;
        GUI.enabled = oldEnabled && canLocate;
        if (GUILayout.Button(record.Alive ? "定位修士" : "定位旧址", GUILayout.Width(110), GUILayout.Height(32)))
        {
            CloseWindow();
            MclslEventLocator.Locate(record);
        }
        GUI.enabled = oldEnabled;
        GUI.backgroundColor = new Color(0.42f, 0.25f, 0.22f, 1f);
        if (GUILayout.Button("拂去留影", GUILayout.Width(110), GUILayout.Height(32)))
            MclslMaobaoSystem.Remove(record.Id, out _maobaoActionMessage);
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(4);
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
