using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Queries;

/// <summary>
/// 世界角色只读查询门面。人口来自完整运行时角色注册表，修士和境界统计来自专用索引；
/// 不再把“被修炼系统追踪的人数”误当成世界人口。
/// </summary>
internal static class MclslWorldActorQuery
{
    private const int DirtyRefreshCooldownFrames = 120;
    private static readonly List<Actor> CachedAliveActors = new();
    private static readonly List<Actor> CachedCultivators = new();
    private static readonly Dictionary<string, int> CachedDerivedCounts = new(StringComparer.Ordinal);
    private static readonly List<Actor> CachedAnnualCandidates = new();
    private static int _cachedYear = -1;
    private static int _cachedUnitCount = -1;
    private static int _dirtyVersion;
    private static int _cachedDirtyVersion = -1;
    private static int _lastRefreshFrame = -100000;
    private static bool _annualDetectionActive;

    internal static List<Actor> AliveActorsSnapshot(bool forceRefresh = false)
    {
        int year = MclslRuntime.CurrentYear();
        if (_annualDetectionActive && _cachedYear == year && !forceRefresh) return CachedAliveActors;
        if (!forceRefresh && _cachedYear == year)
        {
            if (_cachedDirtyVersion == _dirtyVersion) return CachedAliveActors;
            if (Time.frameCount - _lastRefreshFrame < DirtyRefreshCooldownFrames) return CachedAliveActors;
        }

        Refresh(MclslActorRegistry.Snapshot(), year);
        return CachedAliveActors;
    }

    internal static void BeginAnnualDetection(int year)
    {
        // 年度开始不再为了统计扫描完整角色注册表。世界人口读取原生容器数量，
        // 修士与境界统计读取专用索引；这里只冻结可能的后备人口快照。
        _annualDetectionActive = true;
    }

    internal static void EndAnnualDetection(int year)
    {
        _annualDetectionActive = false;
        _cachedDirtyVersion = _dirtyVersion;
    }

    internal static List<Actor> CultivatorsSnapshot()
    {
        CachedCultivators.Clear();
        IReadOnlyList<Actor> actors = MclslCultivatorCandidateIndex.GetCultivatorActorsSnapshot();
        for (int i = 0; i < actors.Count; i++) CachedCultivators.Add(actors[i]);
        return CachedCultivators;
    }

    internal static IReadOnlyList<Actor> AnnualCandidateSnapshot()
    {
        CachedAnnualCandidates.Clear();
        IReadOnlyList<long> ids = MclslCultivatorCandidateIndex.GetAnnualCandidateIds();
        for (int i = 0; i < ids.Count; i++)
        {
            if (MclslCultivatorCandidateIndex.Resolve(ids[i], out Actor actor))
                CachedAnnualCandidates.Add(actor);
        }
        return CachedAnnualCandidates;
    }

    internal static int RealmAtLeastCount(string realm)
    {
        int threshold = MclslRealmIds.Index(realm);
        if (threshold < 0) return 0;
        string cacheKey = "count:realm_at_least:" + realm + "|y:" + MclslRuntime.CurrentYear() + "|d:" + _dirtyVersion;
        if (CachedDerivedCounts.TryGetValue(cacheKey, out int cached)) return cached;
        int count = MclslCultivatorCandidateIndex.CountRealmAtLeast(realm);
        CachedDerivedCounts[cacheKey] = count;
        return count;
    }

    internal static int UnitCount()
    {
        // getSimpleList().Count 是原生单位容器的 O(1) 数量读取，不是全图扫描。
        // 它用于世界人口；修士人数仍严格来自修炼索引，二者不再混用。
        try
        {
            IReadOnlyList<Actor> units = World.world?.units?.getSimpleList();
            if (units != null) return Math.Max(0, units.Count);
        }
        catch (Exception ex)
        {
            MclslDiagnostics.Error("actor-query:unit-count", "读取世界人口失败: " + ex.Message);
        }

        AliveActorsSnapshot();
        return Math.Max(0, _cachedUnitCount);
    }

    internal static void Track(Actor actor)
    {
        if (actor?.data == null) return;
        if (!MclslActorRegistry.Register(actor, out _, out bool isNew)) return;
        MclslCultivatorCandidateIndex.Observe(actor);
        if (isNew) MarkDirty();
    }

    internal static void TrackAnnualCandidate(Actor actor)
    {
        Track(actor);
    }

    internal static void TrackIfRelevant(Actor actor)
    {
        // 完整人口注册与修炼索引分离：凡人也进入角色注册表，但不会进入年度修炼候选集。
        Track(actor);
    }

    internal static void ClearCache()
    {
        CachedAliveActors.Clear();
        CachedCultivators.Clear();
        CachedDerivedCounts.Clear();
        CachedAnnualCandidates.Clear();
        _cachedYear = -1;
        _cachedUnitCount = -1;
        _cachedDirtyVersion = -1;
        _dirtyVersion++;
        _annualDetectionActive = false;
    }

    internal static void MarkDirty()
    {
        _dirtyVersion++;
        CachedDerivedCounts.Clear();
    }

    private static void Refresh(IReadOnlyList<Actor> units, int year)
    {
        CachedAliveActors.Clear();
        CachedDerivedCounts.Clear();
        for (int i = 0; i < units.Count; i++)
        {
            Actor actor = units[i];
            if (MclslActorAccessor.Alive(actor)) CachedAliveActors.Add(actor);
        }
        _cachedYear = year;
        _cachedUnitCount = CachedAliveActors.Count;
        _cachedDirtyVersion = _dirtyVersion;
        _lastRefreshFrame = Time.frameCount;
    }
}
