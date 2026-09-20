using System;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private void DrawHuanzhenChronicle()
    {
        int successes = 0;
        int failures = 0;
        for (int i = 0; i < _snapshot.HuanzhenHistoriesSorted.Count; i++)
        {
            MclslHuanzhenHistoryRecord history = _snapshot.HuanzhenHistoriesSorted[i];
            if (history == null) continue;
            if (string.Equals(history.Result, "还真成功", StringComparison.Ordinal)) successes++;
            else failures++;
        }

        DrawPageHeader("还真纪事", "独立收录还真降临、手动授予、锚点变动、还真归来与死亡回溯结果。");
        GUILayout.BeginHorizontal();
        DrawOverviewPill("关键纪事", _snapshot.HuanzhenEventsSorted.Count.ToString(), "#7FAFB7", GUILayout.Width(180));
        DrawOverviewPill("成功回溯", successes.ToString(), "#8FC2AE", GUILayout.Width(180));
        DrawOverviewPill("未能回溯", failures.ToString(), "#C08D7A", GUILayout.Width(180));
        DrawOverviewPill("现存锚点", _snapshot.HuanzhenAnchorsSorted.Count.ToString(), "#B6A86F", GUILayout.Width(180));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);

        DrawPageHeader("本世关键纪事", "按年份倒序排列；含地点的记录可直接定位。");
        if (_snapshot.HuanzhenEventsSorted.Count == 0)
        {
            DrawEmptyCard("暂无还真纪事", "还真降临、手动授予、建立锚点或成功归来后会在此留下记录。");
        }
        else
        {
            for (int i = 0; i < _snapshot.HuanzhenEventsSorted.Count; i++)
            {
                MclslRunEventRecord record = _snapshot.HuanzhenEventsSorted[i];
                if (record != null) DrawEventCard(record);
            }
        }

        DrawPageHeader("死亡回溯结果", "记录每次死亡时还真是否发动、选用的锚点与避环层数。");
        if (_snapshot.HuanzhenHistoriesSorted.Count == 0)
        {
            DrawEmptyCard("暂无回溯结果", "还真持有者死亡后会在此记录成功回溯或未发动的原因。");
            return;
        }

        for (int i = 0; i < _snapshot.HuanzhenHistoriesSorted.Count; i++)
        {
            MclslHuanzhenHistoryRecord history = _snapshot.HuanzhenHistoriesSorted[i];
            if (history == null) continue;
            bool success = string.Equals(history.Result, "还真成功", StringComparison.Ordinal);
            DrawInfoCard(history.DeathYear + "年｜" + Blank(history.HostName), success ? "#8FC2AE" : "#C08D7A", () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(MclslRealmIds.Display(history.RealmId), "#9CBFD0");
                DrawTag("避环 " + history.LoopDepth, "#AAA1C2");
                DrawTag(history.AnchorYear < 0 ? "无可用锚点" : "回到 " + history.AnchorYear + "年", "#C9B873");
                DrawTag(success ? "回溯成功" : "未能回溯", success ? "#8FC2AE" : "#C08D7A");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Label(Blank(history.Result));
            });
        }
    }
}
