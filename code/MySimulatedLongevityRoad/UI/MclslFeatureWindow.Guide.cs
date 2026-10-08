using UnityEngine;
using NeoModLoader.General;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslFeatureWindow
{
    private void DrawGuide()
    {
        bool compact = _rect.width < 800f;
        if (compact)
        {
            GUILayout.Label("模组交流qq群：1049012706", _muted);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < GuideTitles.Length; i++)
            {
                int page = i;
                if (GUILayout.Button(LM.Get("mclsl_guide_chapter_" + i), i == _guidePage ? _selectedTab : _tab, GUILayout.Height(38f)))
                    SelectGuidePage(page);
            }
            GUILayout.EndHorizontal();
            DrawGuideChapter();
            DrawGuideActions();
        }
        else
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            GUILayout.BeginVertical(_jadePanel, GUILayout.Width(205f), GUILayout.ExpandHeight(true));
            GUILayout.Label("玄黄卷目", _section);
            GUILayout.Label("选择主题阅读", _muted);
            GUILayout.Space(8f);
            for (int i = 0; i < GuideTitles.Length; i++)
            {
                int page = i;
                if (GUILayout.Button((i + 1).ToString("00") + "  " + LM.Get("mclsl_guide_chapter_" + i), i == _guidePage ? _selectedTab : _tab,
                    GUILayout.Height(49f))) SelectGuidePage(page);
                GUILayout.Space(4f);
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("模组交流qq群：1049012706", _muted);
            GUILayout.EndVertical();
            GUILayout.Space(9f);
            DrawGuideChapter();
            GUILayout.Space(9f);
            GUILayout.BeginVertical(_jadePanel, GUILayout.Width(214f), GUILayout.ExpandHeight(true));
            DrawGuideActions();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(45f);
        if (GUILayout.Button("‹ 上一卷", _button, GUILayout.Width(115f), GUILayout.Height(35f)))
            SelectGuidePage((_guidePage + GuideTitles.Length - 1) % GuideTitles.Length);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("下一卷 ›", _primaryButton, GUILayout.Width(115f), GUILayout.Height(35f)))
            SelectGuidePage((_guidePage + 1) % GuideTitles.Length);
        GUILayout.Space(45f);
        GUILayout.EndHorizontal();
    }

    private void SelectGuidePage(int page)
    {
        if (page < 0 || page >= GuideTitles.Length) return;
        _guidePage = page;
        _guideScroll = Vector2.zero;
    }

    private void DrawGuideChapter()
    {
        GUILayout.BeginVertical(_innerPanel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.BeginHorizontal();
        GUILayout.Label("第 " + (_guidePage + 1) + " 卷", _muted, GUILayout.Width(72f));
        GUILayout.Label(LM.Get("mclsl_guide_chapter_" + _guidePage), _section);
        GUILayout.FlexibleSpace();
        GUILayout.Label("本版功能速览", _muted);
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);
        _guideScroll = GUILayout.BeginScrollView(_guideScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar,
            GUILayout.ExpandHeight(true));
        GUILayout.BeginVertical(_card);
        GUILayout.Label("本卷概览", _section);
        GUILayout.Space(6f);
        GUILayout.Label(GuideBodies[_guidePage], _body);
        GUILayout.EndVertical();
        GUILayout.Space(10f);
        GUILayout.Label("查看要点", _section);
        for (int i = 0; i < GuidePoints[_guidePage].Length; i++)
        {
            GUILayout.BeginHorizontal(_card, GUILayout.MinHeight(53f));
            GUILayout.Label((i + 1).ToString("00"), _section, GUILayout.Width(37f));
            GUILayout.Label(GuidePoints[_guidePage][i], _body);
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
