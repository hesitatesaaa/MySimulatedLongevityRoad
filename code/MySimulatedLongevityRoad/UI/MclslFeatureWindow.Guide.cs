using UnityEngine;
using NeoModLoader.General;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslFeatureWindow
{
    private Vector2 _guideMenuScroll;
    private void DrawGuide()
    {
        GUILayout.Label("模组游玩倍速最高20倍 · 玄黄族日常年度修炼 +10%", _muted);
        bool compact = _rect.width < 800f;
        if (compact)
        {
            GUILayout.Label("模组交流qq群：1049012706", _muted);
            _guideMenuScroll=GUILayout.BeginScrollView(_guideMenuScroll,false,true,GUIStyle.none,GUI.skin.verticalScrollbar,GUILayout.Height(92f));
            GUILayout.BeginVertical();
            for (int i = 0; i < GuideChapters.Length; i++)
            {
                int page = i;
                if (GUILayout.Button(GuideChapters[i].Title, i == _guidePage ? _selectedTab : _tab, GUILayout.MinHeight(42f), GUILayout.ExpandWidth(true)))
                    SelectGuidePage(page);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            DrawGuideChapter();
            DrawGuideActions();
        }
        else
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            GUILayout.BeginVertical(_jadePanel, GUILayout.Width(272f), GUILayout.ExpandHeight(true));
            GUILayout.Label("玄黄卷目", _section);
            GUILayout.Label("选择主题阅读", _muted);
            GUILayout.Space(8f);
            _guideMenuScroll=GUILayout.BeginScrollView(_guideMenuScroll,false,true,GUIStyle.none,GUI.skin.verticalScrollbar,GUILayout.ExpandHeight(true));
            for (int i = 0; i < GuideChapters.Length; i++)
            {
                int page = i;
                if (GUILayout.Button((i + 1).ToString("00") + "  " + GuideChapters[i].Title, i == _guidePage ? _selectedTab : _tab,
                    GUILayout.MinHeight(42f), GUILayout.ExpandWidth(true))) SelectGuidePage(page);
                GUILayout.Space(4f);
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("模组交流qq群：1049012706", _muted);
            GUILayout.EndVertical();
            GUILayout.Space(9f);
            DrawGuideChapter();
            GUILayout.Space(9f);
            GUILayout.BeginVertical(_jadePanel, GUILayout.Width(176f), GUILayout.ExpandHeight(true));
            DrawGuideActions();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(45f);
        if (GUILayout.Button("‹ 上一卷", _button, GUILayout.Width(115f), GUILayout.Height(35f)))
            SelectGuidePage((_guidePage + GuideChapters.Length - 1) % GuideChapters.Length);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("下一卷 ›", _primaryButton, GUILayout.Width(115f), GUILayout.Height(35f)))
            SelectGuidePage((_guidePage + 1) % GuideChapters.Length);
        GUILayout.Space(45f);
        GUILayout.EndHorizontal();
    }

    private void SelectGuidePage(int page)
    {
        if (page < 0 || page >= GuideChapters.Length) return;
        _guidePage = page;
        _guideScroll = Vector2.zero;
    }

    private void DrawGuideChapter()
    {
        GUILayout.BeginVertical(_innerPanel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label("第 " + (_guidePage + 1) + " 卷", _muted, GUILayout.Width(72f));
        GUILayout.Label(GuideChapters[_guidePage].Title, _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label("本版功能速览", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        _guideScroll = GUILayout.BeginScrollView(_guideScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        GUILayout.BeginVertical(_card);
        GUILayout.Label("本卷概览", _section);
        GUILayout.Space(6f);
        GUILayout.Label(GuideChapters[_guidePage].Body, _body);
        GUILayout.EndVertical();
        GUILayout.Space(10f);
        GUILayout.Label("查看要点", _section);
        for (int i = 0; i < GuideChapters[_guidePage].Points.Length; i++)
        {
            GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(53f));
            GUILayout.Label((i + 1).ToString("00"), _section, GUILayout.Width(37f));
            GUILayout.Label(GuideChapters[_guidePage].Points[i], _body);
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawGuideActions()
    {
        GUILayout.Label("继续查阅", _section);
        GUILayout.Label("从介绍直达对应界面", _muted);
        GUILayout.Space(9f);
        if (GUILayout.Button("打开玄黄仙录", _primaryButton, GUILayout.Height(42f)))
        {
            _visible = false;
            ReleaseInputBlocker();
            ReleaseFeaturePause();
            MclslCodexWindow.Show();
        }
        GUILayout.Space(5f);
        if (GUILayout.Button("查看修士榜", _button, GUILayout.Height(42f)))
        {
            _visible = false;
            ReleaseInputBlocker();
            ReleaseFeaturePause();
            MclslRankWindow.ShowWindow();
        }
        GUILayout.Space(5f);
        if (GUILayout.Button("前往还真之门", _button, GUILayout.Height(42f)))
        {
            _visible = false;
            ReleaseInputBlocker();
            ReleaseFeaturePause();
            MclslCodexWindow.ShowHuanzhenSpace();
        }
        GUILayout.Space(12f);
        GUILayout.Label("介绍依据当前模组功能整理；具体规则以游戏内实际状态为准。", _muted);
    }

}
