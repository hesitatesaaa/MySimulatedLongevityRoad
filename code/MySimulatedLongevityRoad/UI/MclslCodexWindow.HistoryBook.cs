using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using NeoModLoader.General;
using UnityEngine;

namespace MySimulatedLongevityRoad.UI;

internal sealed partial class MclslCodexWindow
{
    private const string HistoryWorldView = "world";
    private const string HistoryBiographyView = "biography";
    private const string HistoryFamilyView = "family";
    private const int HistoryEventPageSize = 36;
    private string _historyBookView = HistoryWorldView;
    private string _historyEventCategory = MclslEventCatalog.All;
    private bool _historyImportantOnly;
    private bool _historyLocatableOnly;
    private bool _historyFilterAncient;
    private int _historyEventPage;
    private Vector2 _historyCategoryScroll;
    private string _historySelectedFamilyId = string.Empty;
    private string _historyFamilySearch = string.Empty;
    private Vector2 _historyFamilyListScroll;
    private Vector2 _historyFamilyDetailScroll;

    private static string EventCategory(MclslRunEventRecord record)
    {
        if (record == null) return MclslEventCatalog.NativeWorld;
        return string.IsNullOrWhiteSpace(record.Category)
            ? MclslEventCatalog.CategoryForType(record.EventType) : record.Category;
    }

    private void DrawHistoryBook(MclslWorldRunState run)
    {
        DrawPageHeader(LM.Get("mclsl_history_book"), LM.Get("mclsl_history_book_description"));
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe("#8FA9C7");
        GUILayout.Label("<b><color=#8FA9C7>◇ " + LM.Get("mclsl_history_volume") + " ◇</color></b>");
        GUILayout.BeginHorizontal();
        DrawHistoryViewButton(HistoryWorldView, "mclsl_history_world");
        DrawHistoryViewButton(HistoryBiographyView, "mclsl_history_biography");
        DrawHistoryViewButton(HistoryFamilyView, "mclsl_history_family");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(8f);
        switch (_historyBookView)
        {
            case HistoryBiographyView: DrawCultivatorBiographies(run); break;
            case HistoryFamilyView: DrawHistoryFamilies(); break;
            default: DrawHistoryWorldEvents(run); break;
        }
    }

    private void DrawHistoryViewButton(string view, string labelKey)
    {
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = _historyBookView == view ? new Color(0.30f, 0.43f, 0.39f) : old;
        if (GUILayout.Button(LM.Get(labelKey), GUILayout.Width(150f), GUILayout.Height(38f)) && _historyBookView != view)
        {
            _historyBookView = view;
            _scroll = Vector2.zero;
            if (view == HistoryBiographyView) RequestBiographyRefresh();
        }
        GUI.backgroundColor = old;
    }

    private void DrawHistoryWorldEvents(MclslWorldRunState run)
    {
        bool ancient = string.Equals(run.CultivationEpoch, MclslWorldEpochSystem.AncientLawEpoch, StringComparison.Ordinal);
        if (ancient != _historyFilterAncient)
        {
            _historyFilterAncient = ancient;
            _historyEventCategory = MclslEventCatalog.All;
            _historyEventPage = 0;
        }
        DrawInfoCard(ancient ? LM.Get("mclsl_history_ancient_events") : LM.Get("mclsl_history_new_events"), "#8FA9C7",
            () => GUILayout.Label(LM.Get("mclsl_history_event_description")));
        DrawHistoryEventFilters(ancient);
        IReadOnlyList<MclslRunEventRecord> events = _snapshot.VisibleEventsSorted;
        int total = 0, matched = 0, important = 0;
        foreach (MclslRunEventRecord record in events)
        {
            if (record == null) continue;
            total++;
            if (MclslEventCatalog.EffectiveImportance(record) >= 4) important++;
            if (HistoryEventMatches(record, ancient)) matched++;
        }
        GUILayout.BeginHorizontal();
        DrawMiniStat(LM.Get("mclsl_history_total"), total.ToString(), "#CFC7B2", GUILayout.Width(150f));
        DrawMiniStat(LM.Get("mclsl_history_matched"), matched.ToString(), "#9CD7FF", GUILayout.Width(150f));
        DrawMiniStat(LM.Get("mclsl_history_high_importance"), important.ToString(), "#FFD37A", GUILayout.Width(150f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        int pages = Math.Max(1, (matched + HistoryEventPageSize - 1) / HistoryEventPageSize);
        _historyEventPage = Math.Clamp(_historyEventPage, 0, pages - 1);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(LM.Get("mclsl_history_previous"), GUILayout.Width(82f)) && _historyEventPage > 0) _historyEventPage--;
        GUILayout.Label((_historyEventPage + 1) + " / " + pages, GUILayout.Width(65f));
        if (GUILayout.Button(LM.Get("mclsl_history_next"), GUILayout.Width(82f)) && _historyEventPage + 1 < pages) _historyEventPage++;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (matched == 0) DrawEmptyCard(LM.Get("mclsl_history_no_record"), LM.Get("mclsl_history_no_match"));
        int matchIndex = 0;
        foreach (MclslRunEventRecord record in events)
        {
            if (record == null || !HistoryEventMatches(record, ancient)) continue;
            if (matchIndex >= _historyEventPage * HistoryEventPageSize
                && matchIndex < (_historyEventPage + 1) * HistoryEventPageSize) DrawHistoryEventCard(record);
            matchIndex++;
            if (matchIndex >= (_historyEventPage + 1) * HistoryEventPageSize) break;
        }
    }

    private void DrawHistoryEventFilters(bool ancient)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe("#8FA9C7");
        GUILayout.Label("<b>" + LM.Get("mclsl_history_filter") + "</b>");
        _historyCategoryScroll = GUILayout.BeginScrollView(_historyCategoryScroll, true, false,
            GUI.skin.horizontalScrollbar, GUIStyle.none, GUILayout.Height(43f));
        GUILayout.BeginHorizontal();
        DrawHistoryCategoryButton(MclslEventCatalog.All, LM.Get("mclsl_family_realm_all"));
        if (ancient)
        {
            DrawHistoryCategoryButton("teaching", LM.Get("mclsl_history_group_teaching"));
            DrawHistoryCategoryButton("breakthrough", LM.Get("mclsl_history_group_breakthrough"));
            DrawHistoryCategoryButton("mind", LM.Get("mclsl_history_group_mind"));
            DrawHistoryCategoryButton("disaster", LM.Get("mclsl_history_group_disaster"));
            DrawHistoryCategoryButton("secret", LM.Get("mclsl_history_group_secret"));
            DrawHistoryCategoryButton("observation", LM.Get("mclsl_history_group_observation"));
            DrawHistoryCategoryButton("other", LM.Get("mclsl_history_group_other"));
        }
        else
            foreach (MclslEventCategoryDefinition category in MclslEventCatalog.Categories)
                if (category.Id != MclslEventCatalog.All) DrawHistoryCategoryButton(category.Id, category.Name);
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        bool important = GUILayout.Toggle(_historyImportantOnly, LM.Get("mclsl_history_important_only"));
        bool locatable = GUILayout.Toggle(_historyLocatableOnly, LM.Get("mclsl_history_locatable_only"));
        if (important != _historyImportantOnly || locatable != _historyLocatableOnly) _historyEventPage = 0;
        _historyImportantOnly = important;
        _historyLocatableOnly = locatable;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawHistoryCategoryButton(string categoryId, string label)
    {
        Color old = GUI.backgroundColor;
        GUI.backgroundColor = _historyEventCategory == categoryId ? new Color(0.36f, 0.46f, 0.55f) : old;
        if (GUILayout.Button(label, GUILayout.Width(105f), GUILayout.Height(31f)) && _historyEventCategory != categoryId)
        {
            _historyEventCategory = categoryId;
            _historyEventPage = 0;
        }
        GUI.backgroundColor = old;
    }

    private bool HistoryEventMatches(MclslRunEventRecord record, bool ancient)
    {
        if (_historyImportantOnly && MclslEventCatalog.EffectiveImportance(record) < 4) return false;
        if (_historyLocatableOnly && !MclslEventLocator.CanLocate(record)) return false;
        if (_historyEventCategory == MclslEventCatalog.All) return true;
        return ancient ? AncientHistoryGroup(record.EventType) == _historyEventCategory
            : EventCategory(record) == _historyEventCategory;
    }

    private static string AncientHistoryGroup(string eventType)
    {
        string type = eventType ?? string.Empty;
        if (type is "ancient_law_breakthrough" or "ancient_breakthrough_failed") return "breakthrough";
        if (type is "ancient_closed_cultivation" or "ancient_sudden_insight" or "ancient_inner_demon") return "mind";
        if (type is "ancient_spiritual_convergence" or "ancient_spiritual_decline" or "ancient_earthfire" or "ancient_meteor_stone") return "disaster";
        if (type == "ancient_secret_realm") return "secret";
        if (type == "ancient_heaven_earth_resonance") return "observation";
        if (type is "ancient_master_teaching" or "ancient_found_method" or "ancient_technique_deduction"
            or "ancient_new_method_branch" or "ancient_technique_recorded" or "ancient_technique_revived"
            or "ancient_famous_technique" or "ancient_lineage_flourish" or "ancient_lineage_ruin_born"
            or "ancient_lineage_remembered") return "teaching";
        return "other";
    }

    private static void DrawHistoryEventCard(MclslRunEventRecord record)
    {
        MclslEventCategoryDefinition category = MclslEventCatalog.Category(EventCategory(record));
        int importance = MclslEventCatalog.EffectiveImportance(record);
        string stars = new string('★', importance) + new string('☆', 5 - importance);
        DrawInfoCard(record.Year + "年 · " + MclslFactionMissionSystem.DisplaySectText(record.Title),
            importance >= 4 ? "#FF8877" : category.Color, () =>
            {
                GUILayout.BeginHorizontal();
                DrawTag(category.Name, category.Color);
                GUILayout.Label(LM.Get("mclsl_history_importance") + " <color=#FFD37A>" + stars + "</color>", GUILayout.Width(205f));
                if (record.NativeLogged) DrawTag(LM.Get("mclsl_history_native_logged"), "#FFD37A");
                GUILayout.FlexibleSpace();
                if (MclslEventLocator.CanLocate(record) && GUILayout.Button(LM.Get("mclsl_history_locate"), GUILayout.Width(66f)))
                    MclslEventLocator.Locate(record);
                GUILayout.EndHorizontal();
                if (!string.IsNullOrWhiteSpace(record.Body)) GUILayout.Label(PlayerFacingEventBody(record.Body));
            });
    }

    private void OpenFamilyHistoryBook(string familyId)
    {
        _historySelectedFamilyId = familyId ?? string.Empty;
        _historyFamilySearch = string.Empty;
        _historyFamilyListScroll = Vector2.zero;
        _historyFamilyDetailScroll = Vector2.zero;
        _historyBookView = HistoryFamilyView;
        SelectCodexTab("玄黄史册");
    }

    private void DrawHistoryFamilies()
    {
        DrawPageHeader(LM.Get("mclsl_history_family"), LM.Get("mclsl_history_family_description"));
        List<MclslFamilyRecord> families = MclslFamilySystem.Families.Where(x => x != null)
            .OrderByDescending(x => x.History?.Count ?? 0).ThenBy(x => x.Name, StringComparer.Ordinal).ToList();
        List<MclslFamilyRecord> matched = string.IsNullOrWhiteSpace(_historyFamilySearch) ? families
            : families.Where(x => x.Name?.IndexOf(_historyFamilySearch.Trim(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        MclslFamilyRecord selected = matched.Find(x => x.Id == _historySelectedFamilyId);
        if (selected == null && matched.Count > 0) selected = matched[0];
        if (selected != null) _historySelectedFamilyId = selected.Id;
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(275f));
        DrawCardStripe("#A7E08A");
        GUILayout.Label("<b><color=#A7E08A>◇ " + LM.Get("mclsl_history_family_catalog") + " ◇</color></b>");
        GUILayout.Label(LM.Get("mclsl_history_search"));
        _historyFamilySearch = GUILayout.TextField(_historyFamilySearch ?? string.Empty, GUILayout.Height(31f));
        GUILayout.Label(LM.Get("mclsl_history_family_count") + " " + matched.Count);
        _historyFamilyListScroll = GUILayout.BeginScrollView(_historyFamilyListScroll, false, true,
            GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(Mathf.Clamp(_rect.height - 400f, 340f, 700f)));
        foreach (MclslFamilyRecord family in matched)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = family.Id == _historySelectedFamilyId ? new Color(0.34f, 0.48f, 0.38f) : old;
            if (GUILayout.Button("◇ " + family.Name + "\n" + LM.Get("mclsl_history_record_count") + " "
                + (family.History?.Count ?? 0) + " · " + FamilyLatestYearText(family), GUILayout.Height(57f)))
            {
                _historySelectedFamilyId = family.Id;
                _historyFamilyDetailScroll = Vector2.zero;
            }
            GUI.backgroundColor = old;
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(9f);
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (selected == null) DrawEmptyCard(LM.Get("mclsl_history_no_record"), LM.Get("mclsl_history_no_match"));
        else DrawHistoryFamilyArchive(selected);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DrawHistoryFamilyArchive(MclslFamilyRecord family)
    {
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe("#A7E08A");
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b><size=23>" + family.Name + LM.Get("mclsl_history_family_volume_suffix") + "</size></b>");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(LM.Get("mclsl_history_family_back"), GUILayout.Width(160f), GUILayout.Height(33f)))
            SelectCodexTab("修仙家族");
        GUILayout.EndHorizontal();
        GUILayout.Label("<color=#9CD7FF>" + FamilyYearSpan(family) + "</color>");
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal();
        DrawMiniStat(LM.Get("mclsl_history_record_count"), (family.History?.Count ?? 0).ToString(), "#CFC7B2", GUILayout.Width(135f));
        DrawMiniStat(LM.Get("mclsl_history_latest_year"), FamilyLatestYearText(family), "#9CD7FF", GUILayout.Width(165f));
        DrawMiniStat(LM.Get("mclsl_history_family_living"), family.Members.Count(x => x?.Alive == true).ToString(), "#A7E08A", GUILayout.Width(145f));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (family.History?.Count > 0)
            DrawInfoCard(LM.Get("mclsl_history_recent"), "#FFD37A", () => GUILayout.Label(family.History[family.History.Count - 1]));
        GUILayout.BeginVertical(GUI.skin.box);
        DrawCardStripe("#A7E08A");
        GUILayout.Label("<b><color=#A7E08A>◇ " + LM.Get("mclsl_history_annals") + " ◇</color></b>");
        if (family.History == null || family.History.Count == 0) GUILayout.Label(LM.Get("mclsl_history_no_record"));
        _historyFamilyDetailScroll = GUILayout.BeginScrollView(_historyFamilyDetailScroll, false, true,
            GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(Mathf.Clamp(_rect.height - 570f, 260f, 590f)));
        if (family.History != null)
            for (int i = family.History.Count - 1; i >= 0; i--)
            {
                string entry = family.History[i] ?? string.Empty;
                bool dated = TryFamilyHistoryYear(entry, out int year);
                DrawInfoCard(dated ? year + "年" : LM.Get("mclsl_history_old_record"), "#A7E08A", () =>
                {
                    DrawTag(FamilyHistoryKind(entry), "#A7E08A");
                    GUILayout.Label(FamilyHistoryBody(entry));
                });
            }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private static bool TryFamilyHistoryYear(string entry, out int year)
    {
        year = 0;
        if (string.IsNullOrWhiteSpace(entry)) return false;
        int marker = entry.IndexOf('年');
        return marker is > 0 and <= 8 && int.TryParse(entry.Substring(0, marker).Trim(), out year) && year >= 0;
    }

    private static string FamilyHistoryBody(string entry)
    {
        if (!TryFamilyHistoryYear(entry, out _)) return entry ?? string.Empty;
        int marker = entry.IndexOf('年');
        string body = entry.Substring(marker + 1).Trim(' ', '·', '　');
        return body.Length == 0 ? entry : body;
    }

    private static string FamilyLatestYearText(MclslFamilyRecord family)
    {
        if (family?.History != null)
            for (int i = family.History.Count - 1; i >= 0; i--)
                if (TryFamilyHistoryYear(family.History[i], out int year)) return year + "年";
        return LM.Get("mclsl_history_old_record");
    }

    private static string FamilyYearSpan(MclslFamilyRecord family)
    {
        int first = -1, last = -1;
        if (family?.History != null)
            foreach (string entry in family.History)
                if (TryFamilyHistoryYear(entry, out int year))
                {
                    if (first < 0) first = year;
                    last = year;
                }
        return first < 0 ? LM.Get("mclsl_history_old_record") : first + "—" + last + "年";
    }

    private static string FamilyHistoryKind(string entry)
    {
        string text = entry ?? string.Empty;
        if (text.Contains("立族", StringComparison.Ordinal)) return LM.Get("mclsl_history_family_founded");
        if (text.Contains("继任", StringComparison.Ordinal)) return LM.Get("mclsl_history_family_succession");
        if (text.Contains("陨落", StringComparison.Ordinal)) return LM.Get("mclsl_history_family_death");
        if (text.Contains("扶持", StringComparison.Ordinal)) return LM.Get("mclsl_history_family_support");
        return LM.Get("mclsl_history_family_affair");
    }
}
