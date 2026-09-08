using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private void DrawWorldEvents(MclslWorldRunState run)
    {
        DrawPageHeader("世界纪事", "本世重要修行事件、天地档案与人间世事。");
        DrawEventCategoryBar();
        GUILayout.Space(8);
        List<MclslRunEventRecord> visibleEvents = _snapshot.VisibleEventsSorted;
        if (visibleEvents.Count > 0
            && !string.Equals(_eventCategory, MclslEventCatalog.All, StringComparison.Ordinal)
            && !_snapshot.EventCategoryCounts.ContainsKey(_eventCategory))
            _eventCategory = MclslEventCatalog.All;
        int shown = 0;
        if (visibleEvents.Count == 0) GUILayout.Label("本世尚无可记之事。");
        foreach (MclslRunEventRecord e in visibleEvents)
        {
            if (!string.Equals(_eventCategory, MclslEventCatalog.All, StringComparison.Ordinal)
                && !string.Equals(EventCategory(e), _eventCategory, StringComparison.Ordinal))
                continue;
            if (shown++ >= 240) break;
            MclslEventCategoryDefinition category = MclslEventCatalog.Category(EventCategory(e));
            DrawInfoCard(e.Year + "年｜" + e.Title, category.Color, () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(category.Name, category.Color);
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(e) && GUILayout.Button("定位", GUILayout.Width(62f)))
                    MclslEventLocator.Locate(e);
                GUILayout.EndHorizontal();
                GUILayout.Label(PlayerFacingEventBody(e.Body));
            });
        }
    }

    private void DrawEventCategoryBar()
    {
        GUILayout.BeginHorizontal();
        foreach (MclslEventCategoryDefinition category in MclslEventCatalog.Categories)
        {
            int count = _snapshot.EventCategoryCounts.TryGetValue(category.Id, out int cachedCount) ? cachedCount : 0;
            GUI.backgroundColor = string.Equals(_eventCategory, category.Id, StringComparison.Ordinal) ? ParseHexColor(category.Color, Color.gray) : Color.gray;
            if (GUILayout.Button(category.Name + " " + count, GUILayout.Height(34f), GUILayout.Width(category.Id == MclslEventCatalog.All ? 120f : 150f)))
            {
                _eventCategory = category.Id;
                _scroll = Vector2.zero;
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private static string EventCategory(MclslRunEventRecord record)
    {
        if (record == null) return MclslEventCatalog.NativeWorld;
        return string.IsNullOrWhiteSpace(record.Category) ? MclslEventCatalog.CategoryForType(record.EventType) : record.Category;
    }
}
