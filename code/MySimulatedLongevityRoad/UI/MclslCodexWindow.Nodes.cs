using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private void DrawMapNodes(MclslWorldRunState run)
    {
        DrawPageHeader("山河节点", "洞天、遗迹与天地之变已归入同一张山河节点图谱；修士会沿原版寻路前往节点，并在抵达后留下控制与传承记录。");
        GUILayout.BeginHorizontal();
        DrawMiniStat("节点", _snapshot.MapNodesSorted.Count.ToString(), "#9CD7FF", GUILayout.Width(150));
        DrawMiniStat("宗门", _snapshot.SectsSorted.Count.ToString(), "#A7E08A", GUILayout.Width(150));
        DrawMiniStat("空间任务", _snapshot.SpatialTasksSorted.Count.ToString(), "#FFD37A", GUILayout.Width(150));
        DrawMiniStat("进行中", ActiveTaskCount(_snapshot.SpatialTasksSorted).ToString(), "#FFB36B", GUILayout.Width(150));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(8f);

        DrawInfoCard("节点档案", "#9CD7FF", () =>
        {
            if (_snapshot.MapNodesSorted.Count == 0)
            {
                GUILayout.Label("本世尚未形成可追踪的山河节点。");
                return;
            }
            for (int i = 0; i < _snapshot.MapNodesSorted.Count; i++)
            {
                MclslMapNodeRecord node = _snapshot.MapNodesSorted[i];
                if (node == null) continue;
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>" + Safe(node.Name, "无名节点") + "</b>");
                DrawTag(NodeTypeText(node.NodeType), NodeTypeColor(node.NodeType));
                DrawTag(NodeStateText(node.LifecycleState), NodeStateColor(node.LifecycleState));
                if (!string.IsNullOrWhiteSpace(node.OwnerNameSnapshot)) DrawTag(node.OwnerNameSnapshot, "#D8C778");
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(node) && GUILayout.Button("定位", GUILayout.Width(62f)))
                    MclslEventLocator.Locate(node);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                DrawMiniStat("坐标", node.MapX + ", " + node.MapY, "#CFC7B2", GUILayout.Width(145));
                DrawMiniStat("位置", Safe(node.NativeKingdomNameSnapshot, "无主荒域") + "·" + Safe(node.LocationName, "未知"), "#CFC7B2", GUILayout.Width(255));
                DrawMiniStat("品质", node.Quality.ToString(), "#FFD37A", GUILayout.Width(105));
                DrawMiniStat("余量", node.RemainingValue.ToString(), "#A7E08A", GUILayout.Width(105));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (!string.IsNullOrWhiteSpace(node.LawTags)) GUILayout.Label("法则：" + node.LawTags);
                GUILayout.EndVertical();
            }
        });

        DrawInfoCard("空间任务", "#FFD37A", () =>
        {
            if (_snapshot.SpatialTasksSorted.Count == 0)
            {
                GUILayout.Label("当前没有沿地图移动的修士任务。");
                return;
            }
            for (int i = 0; i < _snapshot.SpatialTasksSorted.Count; i++)
            {
                MclslSpatialTaskRecord task = _snapshot.SpatialTasksSorted[i];
                if (task == null) continue;
                MclslMapNodeRecord node = MclslMapNodeSystem.Find(task.TargetNodeId);
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(Safe(task.ActorNameSnapshot, "无名修士") + " → " + Safe(node?.Name, task.TargetNodeId), GUILayout.Width(300f));
                DrawTag(TaskStateText(task.State), TaskStateColor(task.State));
                GUILayout.Label(task.Progress + "%", GUILayout.Width(58f));
                GUILayout.Label(Safe(task.FailureReason, "沿原版寻路前往"), GUILayout.ExpandWidth(true));
                if (MclslEventLocator.CanLocate(node) && GUILayout.Button("定位", GUILayout.Width(62f))) MclslEventLocator.Locate(node);
                GUILayout.EndHorizontal();
            }
        });

        DrawInfoCard("宗门与控制", "#A7E08A", () =>
        {
            if (_snapshot.SectsSorted.Count == 0)
            {
                GUILayout.Label("尚无达到金丹境、能够维持独立传承的宗门。");
                return;
            }
            for (int i = 0; i < _snapshot.SectsSorted.Count; i++)
            {
                MclslSectRecord sect = _snapshot.SectsSorted[i];
                if (sect == null) continue;
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label("<b>" + Safe(sect.Name, "无名宗门") + "</b>", GUILayout.Width(230f));
                DrawTag(sect.State == "active" ? "存续" : "覆灭", sect.State == "active" ? "#A7E08A" : "#FF8877");
                GUILayout.Label("门人 " + sect.MemberCountSnapshot + "｜控制节点 " + (sect.ControlledNodeIds?.Count ?? 0), GUILayout.Width(230f));
                GUILayout.Label("坐标 " + sect.SeatMapX + ", " + sect.SeatMapY, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }
        });
    }

    private static int ActiveTaskCount(List<MclslSpatialTaskRecord> tasks)
    {
        int count = 0;
        if (tasks == null) return count;
        for (int i = 0; i < tasks.Count; i++)
        {
            string state = tasks[i]?.State;
            if (state == "assigned" || state == "moving" || state == "arrived") count++;
        }
        return count;
    }

    private static string Safe(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static string NodeTypeText(string type) => type switch
    {
        MclslMapNodeSystem.Cave => "洞天",
        MclslMapNodeSystem.Ruin => "遗迹",
        MclslMapNodeSystem.WorldChange => "天地之变",
        _ => "节点"
    };

    private static string NodeTypeColor(string type) => type switch
    {
        MclslMapNodeSystem.Cave => "#9CD7FF",
        MclslMapNodeSystem.Ruin => "#A7E08A",
        MclslMapNodeSystem.WorldChange => "#B7A7FF",
        _ => "#CFC7B2"
    };

    private static string NodeStateText(string state) => state switch
    {
        "controlled" => "已控制",
        "contested" => "争夺中",
        "depleted" => "余藏耗尽",
        "collapsed" => "已崩塌",
        "lost" => "坐标失联",
        _ => "活跃"
    };

    private static string NodeStateColor(string state) => state switch
    {
        "controlled" => "#A7E08A",
        "contested" => "#FFD37A",
        "collapsed" or "lost" => "#FF8877",
        _ => "#CFC7B2"
    };

    private static string TaskStateText(string state) => state switch
    {
        "moving" => "移动中",
        "arrived" => "已抵达",
        "completed" => "已完成",
        "failed" => "失败",
        _ => "已派遣"
    };

    private static string TaskStateColor(string state) => state switch
    {
        "completed" => "#A7E08A",
        "failed" => "#FF8877",
        "moving" or "arrived" => "#FFD37A",
        _ => "#9CD7FF"
    };
}
