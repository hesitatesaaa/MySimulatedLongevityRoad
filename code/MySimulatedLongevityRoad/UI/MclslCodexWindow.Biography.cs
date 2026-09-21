using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using UnityEngine;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private long _biographyActorId;

    internal static void ShowBiographyForActor(Actor actor)
    {
        Show();
        if (_instance == null) return;
        _instance._biographyActorId = MclslActorAccessor.Id(actor);
        MclslCodexTab[] tabs = ActiveTabs();
        for (int i = 0; i < tabs.Length; i++)
            if (string.Equals(tabs[i].Title, "修士列传", StringComparison.Ordinal)) { _instance._tab = i; break; }
        _instance._scroll = Vector2.zero;
    }

    private void DrawCultivatorBiographies(MclslWorldRunState run)
    {
        DrawPageHeader("修士列传", "独立记录本世修士的道途、功法、猫宝留影与生死纪事；点击右侧按钮可直接查看人物。 ");
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        List<Actor> visible = actors == null ? new List<Actor>() : actors.Where(x => x != null && MclslActorAccessor.Alive(x)).OrderByDescending(x => MclslRealmIds.Index(MclslActorAccessor.Realm(x))).ThenBy(x => MclslActorAccessor.DisplayName(x), StringComparer.Ordinal).Take(120).ToList();
        if (visible.Count == 0)
        {
            GUILayout.Label("暂无符合条件的在世修士。死亡人物仍可通过修士生死簿查看。");
            return;
        }
        for (int i = 0; i < visible.Count; i++)
        {
            Actor actor = visible[i];
            long actorId = MclslActorAccessor.Id(actor);
            bool selected = actorId == _biographyActorId;
            GUILayout.BeginVertical(GUI.skin.box);
            DrawCardStripe(selected ? "#8FE3D1" : "#638D92");
            GUILayout.BeginHorizontal();
            GUILayout.Label("<size=18><b>" + MclslActorAccessor.DisplayName(actor) + "</b></size>", GUILayout.Width(230));
            DrawTag(MclslRealmIds.Display(MclslActorAccessor.Realm(actor)), "#9CD7FF");
            DrawTag("真元 " + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0), "#8FE3D1");
            if (MclslMaobaoSystem.IsRecorded(actorId)) DrawTag("猫宝已刻名", "#D8C778");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("查看人物", GUILayout.Width(90)))
            {
                _biographyActorId = actorId;
                try { ActionLibrary.openUnitWindow(actor); } catch { }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("功法道统：" + Blank(MclslActorAccessor.GetString(actor, MclslActorDataKeys.TechniqueName, string.Empty), "未记录") + "　心境：" + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.MindState, 0));
            GUILayout.Label("势力：" + Blank(MclslActorAccessor.GetString(actor, MclslActorDataKeys.FactionAffiliation, string.Empty), "无归属") + "　当前年份：" + MclslRuntime.CurrentYear());
            List<MclslRunEventRecord> events = (run?.Events ?? new List<MclslRunEventRecord>()).Where(x => x != null && x.ActorId == actorId).OrderByDescending(x => x.Year).Take(3).ToList();
            if (events.Count > 0) GUILayout.Label("最近纪事：" + string.Join("；", events.Select(x => x.Year + "年" + x.Title)));
            GUILayout.EndVertical();
        }
    }

    private static string Blank(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
