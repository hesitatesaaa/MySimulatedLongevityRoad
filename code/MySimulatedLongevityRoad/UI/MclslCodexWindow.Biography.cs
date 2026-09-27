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
    private const int BiographyPageSize = 120;
    private long _biographyActorId;
    private long _biographyScrollRequestActorId;
    private string _biographySearch = string.Empty;
    private int _biographyPage;

    internal static void ShowBiographyForActor(Actor actor)
    {
        Show();
        if (_instance == null) return;
        _instance._biographyActorId = MclslActorAccessor.Id(actor);
        _instance._biographyScrollRequestActorId = _instance._biographyActorId;
        _instance._biographySearch = string.Empty;
        _instance._biographyPage = 0;
        MclslCodexTab[] tabs = ActiveTabs();
        for (int i = 0; i < tabs.Length; i++)
            if (string.Equals(tabs[i].Title, "修士列传", StringComparison.Ordinal)) { _instance._tab = i; break; }
        _instance._scroll = Vector2.zero;
    }

    private void DrawCultivatorBiographies(MclslWorldRunState run)
    {
        DrawPageHeader("修士列传", "记录本世修士的道途、功法、猫宝留影与生死纪事；可搜索姓名，或从人物信息直达对应记录。 ");
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        string search = _biographySearch?.Trim() ?? string.Empty;
        List<Actor> matches = new(actors?.Count ?? 0);
        if (actors != null)
        {
            for (int i = 0; i < actors.Count; i++)
            {
                Actor candidate = actors[i];
                if (candidate == null || !MclslActorAccessor.Alive(candidate)) continue;
                if (search.Length > 0 && MclslActorAccessor.DisplayName(candidate).IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                matches.Add(candidate);
            }
        }

        matches.Sort(CompareBiographyActors);
        int pageCount = Math.Max(1, (matches.Count + BiographyPageSize - 1) / BiographyPageSize);
        if (_biographyScrollRequestActorId != 0)
        {
            int targetIndex = -1;
            for (int i = 0; i < matches.Count; i++)
            {
                if (MclslActorAccessor.Id(matches[i]) == _biographyScrollRequestActorId)
                {
                    targetIndex = i;
                    break;
                }
            }
            if (targetIndex >= 0) _biographyPage = targetIndex / BiographyPageSize;
            else _biographyScrollRequestActorId = 0;
        }
        _biographyPage = Math.Clamp(_biographyPage, 0, pageCount - 1);

        GUILayout.BeginHorizontal();
        GUILayout.Label("搜索修士姓名", GUILayout.Width(100f));
        string nextSearch = GUILayout.TextField(_biographySearch ?? string.Empty, GUILayout.Width(260f));
        if (!string.Equals(nextSearch, _biographySearch, StringComparison.Ordinal))
        {
            _biographySearch = nextSearch;
            _biographyPage = 0;
            _scroll = Vector2.zero;
        }
        if (GUILayout.Button("清空搜索", GUILayout.Width(90f)))
        {
            _biographySearch = string.Empty;
            _biographyPage = 0;
            _scroll = Vector2.zero;
        }
        GUILayout.Label("搜索结果：" + matches.Count + " 人　·　第 " + (_biographyPage + 1) + "/" + pageCount + " 页");
        if (_biographyPage > 0 && GUILayout.Button("上一页", GUILayout.Width(76f)))
        {
            _biographyPage--;
            _scroll = Vector2.zero;
        }
        if (_biographyPage + 1 < pageCount && GUILayout.Button("下一页", GUILayout.Width(76f)))
        {
            _biographyPage++;
            _scroll = Vector2.zero;
        }
        GUILayout.EndHorizontal();

        if (matches.Count == 0)
        {
            GUILayout.Label(search.Length > 0
                ? "没有找到姓名包含“" + search + "”的在世修士。"
                : "暂无符合条件的在世修士。死亡人物仍可通过修士生死簿查看。");
            return;
        }

        int first = _biographyPage * BiographyPageSize;
        int last = Math.Min(matches.Count, first + BiographyPageSize);
        for (int i = first; i < last; i++)
        {
            Actor actor = matches[i];
            long actorId = MclslActorAccessor.Id(actor);
            bool selected = actorId == _biographyActorId;
            GUILayout.BeginVertical(GUI.skin.box);
            DrawCardStripe(selected ? "#8FE3D1" : "#638D92");
            GUILayout.BeginHorizontal();
            GUILayout.Label("<size=18><b>" + MclslActorAccessor.DisplayName(actor) + "</b></size>", GUILayout.Width(230));
            DrawTag(MclslRealmIds.Display(MclslActorAccessor.Realm(actor)), "#9CD7FF");
            DrawTag("真元 " + MclslActorAccessor.GetInt(actor, MclslActorDataKeys.TrueEssence, 0), "#8FE3D1");
            if (selected) DrawTag("已定位", "#8FE3D1");
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
            if (actorId == _biographyScrollRequestActorId)
            {
                Rect targetRect = GUILayoutUtility.GetLastRect();
                _scroll.y = Math.Max(0f, targetRect.y - 48f);
                _biographyScrollRequestActorId = 0;
            }
        }
    }

    private static int CompareBiographyActors(Actor left, Actor right)
    {
        int realm = MclslRealmIds.Index(MclslActorAccessor.Realm(right))
            .CompareTo(MclslRealmIds.Index(MclslActorAccessor.Realm(left)));
        return realm != 0 ? realm : string.Compare(MclslActorAccessor.DisplayName(left), MclslActorAccessor.DisplayName(right), StringComparison.Ordinal);
    }

    private static string Blank(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
