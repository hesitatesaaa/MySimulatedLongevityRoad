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
    private const int BiographyPageSize = 24;
    private const string BiographySensingQiFilter = "sensing_qi";
    private MclslOrderedIdIndex<MclslRankEntry> _biographyMatches;
    private MclslOrderedIdIndex<MclslRankEntry> _biographyNext;
    private int _biographyCursor = -1;
    private string _biographyQuery;
    private string _biographyIndexedRealmFilter;
    private IReadOnlyList<Actor> _biographyWorldActors;
    private int _biographyWorldCursor = -1;
    private readonly HashSet<long> _biographySeenActorIds = new();
    private readonly Dictionary<long, string> _biographyRecentText = new();
    private static int CompareBiographyEntries(MclslRankEntry a, MclslRankEntry b)
    {
        int realm = MclslRealmIds.Index(b.RealmId).CompareTo(MclslRealmIds.Index(a.RealmId));
        return realm != 0 ? realm : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
    }

    private static string BiographyPersonalName(MclslRankEntry entry)
    {
        string name = MclslHonorificNameCatalog.PersonalName(entry?.Actor);
        return string.IsNullOrWhiteSpace(name) ? entry?.Name ?? string.Empty : name;
    }

    private static bool BiographyEntryMatches(MclslRankEntry entry, string query, string realmFilter)
    {
        if (entry == null) return false;
        if (realmFilter == BiographySensingQiFilter)
        {
            if (!string.IsNullOrWhiteSpace(entry.RealmId)) return false;
        }
        else if (realmFilter != MclslEventCatalog.All && entry.RealmId != realmFilter) return false;
        return string.IsNullOrEmpty(query)
            || BiographyPersonalName(entry).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void RefreshBiographyIndex()
    {
        if (!_visible) return;
        if (CurrentTabTitle() != "玄黄史册"
            || _historyBookView != HistoryBiographyView) return;
        if (_biographyWorldCursor >= 0)
        {
            for (int i = 0; i < 64 && _biographyWorldCursor < _biographyWorldActors.Count; i++)
            {
                Actor actor = _biographyWorldActors[_biographyWorldCursor++];
                if (actor?.data == null) continue;
                MclslCultivatorCandidateIndex.Observe(actor);
                if (MclslCultivatorCandidateIndex.IsCultivator(MclslActorAccessor.Id(actor)))
                {
                    _biographySeenActorIds.Add(MclslActorAccessor.Id(actor));
                    MclslRankSnapshotSource.UpdateActor(actor);
                }
            }
            if (_biographyWorldCursor < _biographyWorldActors.Count) return;
            _biographyWorldActors = null;
            _biographyWorldCursor = -1;
            IReadOnlyList<MclslRankEntry> existing = MclslRankSnapshotSource.MembershipEntries;
            long[] stale = existing.Where(x => !_biographySeenActorIds.Contains(x.ActorId)).Select(x => x.ActorId).ToArray();
            foreach (long id in stale) MclslRankSnapshotSource.Remove(id);
            _biographySeenActorIds.Clear();
            MclslCultivatorCandidateIndex.CleanupInvalid(256);
            _biographyQuery = null;
        }
        string query = _biographySearch?.Trim() ?? string.Empty;
        if (_biographyMatches == null || _biographyQuery != query
            || _biographyIndexedRealmFilter != _biographyRealmFilter)
        {
            _biographyQuery = query;
            _biographyIndexedRealmFilter = _biographyRealmFilter;
            _biographyNext = new MclslOrderedIdIndex<MclslRankEntry>(CompareBiographyEntries);
            _biographyCursor = 0;
            _biographyMatches ??= new MclslOrderedIdIndex<MclslRankEntry>(CompareBiographyEntries);
        }
        if (_biographyCursor < 0) return;
        IReadOnlyList<MclslRankEntry> source = MclslRankSnapshotSource.MembershipEntries;
        long deadline = System.Diagnostics.Stopwatch.GetTimestamp() + System.Diagnostics.Stopwatch.Frequency / 2000;
        for (int i = 0; i < 64 && _biographyCursor < source.Count
            && System.Diagnostics.Stopwatch.GetTimestamp() < deadline; i++)
        {
            MclslRankEntry entry = source[_biographyCursor++];
            if (BiographyEntryMatches(entry, _biographyQuery, _biographyIndexedRealmFilter))
                _biographyNext.Upsert(entry.ActorId, entry);
        }
        if (_biographyCursor < source.Count) return;
        _biographyMatches = _biographyNext; _biographyNext = null; _biographyCursor = -1;
    }
    internal static void OnEntryChanged(long id, MclslRankEntry entry)
    {
        MclslCodexWindow window = _instance;
        if (window == null || !window._visible) return;
        window._biographyRecentText.Remove(id);
        bool included = BiographyEntryMatches(entry, window._biographyQuery, window._biographyIndexedRealmFilter);
        if (included)
        {
            window._biographyMatches?.Upsert(id, entry);
            window._biographyNext?.Upsert(id, entry);
        }
        else { window._biographyMatches?.Remove(id); window._biographyNext?.Remove(id); }
    }
    internal static void ClearBiographyRuntime()
    {
        if (_instance == null) return;
        _instance._biographyMatches = null; _instance._biographyNext = null;
        _instance._biographyCursor = -1; _instance._biographyQuery = null;
        _instance._biographyIndexedRealmFilter = null;
        _instance._biographyRecentText.Clear();
        _instance._biographyWorldActors = null; _instance._biographyWorldCursor = -1;
        _instance._biographySeenActorIds.Clear();
    }
    private void RequestBiographyRefresh()
    {
        try { _biographyWorldActors = World.world?.units?.getSimpleList(); }
        catch (Exception ex) { MclslDiagnostics.Error("biography-world-scan", ex.Message); _biographyWorldActors = null; }
        _biographyWorldCursor = _biographyWorldActors == null ? -1 : 0;
        _biographySeenActorIds.Clear();
        if (_biographyWorldCursor < 0) _biographyQuery = null;
    }
    internal static void InvalidateActorHistory(long id)
    {
        if (id > 0) _instance?._biographyRecentText.Remove(id);
    }
    private string BiographyRecentText(long id)
    {
        if (_biographyRecentText.TryGetValue(id, out string text)) return text;
        IReadOnlyList<MclslRunEventRecord> events = MclslWorldRunRepository.RecentActorEvents(id);
        System.Text.StringBuilder builder = new();
        for (int i = events.Count - 1, shown = 0; i >= 0 && shown < 3; i--, shown++)
        { if (shown > 0) builder.Append("；"); builder.Append(events[i].Year).Append("年").Append(MclslFactionMissionSystem.DisplaySectText(events[i].Title)); }
        text = builder.ToString(); _biographyRecentText[id] = text;
        return text;
    }
    private long _biographyActorId;
    private long _biographyScrollRequestActorId;
    private string _biographySearch = string.Empty;
    private string _biographyRealmFilter = MclslEventCatalog.All;
    private int _biographyPage;

    internal static void ShowBiographyForActor(Actor actor)
    {
        Show();
        if (_instance == null) return;
        _instance._biographyActorId = MclslActorAccessor.Id(actor);
        _instance._biographyScrollRequestActorId = _instance._biographyActorId;
        _instance._biographySearch = string.Empty;
        _instance._biographyRealmFilter = MclslEventCatalog.All;
        _instance._biographyIndexedRealmFilter = null;
        _instance._biographyMatches = null;
        _instance._biographyNext = null;
        _instance._biographyCursor = -1;
        _instance._biographyPage = 0;
        _instance._biographyView = "纪事";
        _instance._historyBookView = HistoryBiographyView;
        _instance.SelectCodexTab("玄黄史册");
        _instance.RequestBiographyRefresh();
        _instance._scroll = Vector2.zero;
    }

    private static string Blank(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;
}
