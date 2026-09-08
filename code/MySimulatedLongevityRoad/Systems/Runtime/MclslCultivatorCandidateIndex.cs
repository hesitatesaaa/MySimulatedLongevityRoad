using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式修炼候选索引。角色注册、修士判定、年度候选和境界分桶彼此分离；
/// 所有读取均来自已维护索引，不再在 UI、统计或系统查询时隐式扫描世界。
/// </summary>
internal static class MclslCultivatorCandidateIndex
{
    private static readonly HashSet<long> CultivatorIds = new();
    private static readonly HashSet<long> AnnualCandidateIds = new();
    private static readonly HashSet<long> NativeKillObservedIds = new();
    private static readonly Dictionary<string, HashSet<long>> RealmIds = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, string> RealmByActorId = new();

    private static long[] _cultivatorIdSnapshot = Array.Empty<long>();
    private static long[] _annualCandidateSnapshot = Array.Empty<long>();
    private static Actor[] _cultivatorActorSnapshot = Array.Empty<Actor>();
    private static bool _cultivatorSnapshotDirty;
    private static bool _annualCandidateSnapshotDirty;

    private static IReadOnlyList<Actor> _worldScanActors = Array.Empty<Actor>();
    private static int _worldScanCursor;
    private static int _knownRefreshCursor;
    private static int _cleanupCursor;
    private static bool _worldScanActive;
    private static long _revision;

    internal static int KnownActorCount => MclslActorRegistry.Count;
    internal static long Revision => _revision;
    internal static int CultivatorCount => CultivatorIds.Count;
    internal static int AnnualCandidateCount => AnnualCandidateIds.Count;
    internal static bool IsCultivator(long actorId) => actorId > 0L && CultivatorIds.Contains(actorId);
    internal static bool HasPendingWorldRefresh => _worldScanActive && _worldScanCursor < _worldScanActors.Count;

    /// <summary>
    /// 兼容旧调用的冷路径世界重建。扫描游标会跨调用继续推进，绝不会每次从第 0 个角色重扫；
    /// 本方法只恢复注册表和索引，不发放真元、不尝试突破、不写入年度事件。
    /// </summary>
    internal static int RefreshFromWorldSnapshot(int budget = int.MaxValue)
    {
        if (budget <= 0) return 0;
        if (!_worldScanActive || _worldScanCursor >= _worldScanActors.Count)
        {
            try
            {
                IReadOnlyList<Actor> units = World.world?.units?.getSimpleList();
                if (units == null || units.Count == 0)
                {
                    _worldScanActors = Array.Empty<Actor>();
                }
                else
                {
                    Actor[] snapshot = new Actor[units.Count];
                    for (int i = 0; i < units.Count; i++) snapshot[i] = units[i];
                    _worldScanActors = snapshot;
                }
            }
            catch (Exception ex)
            {
                MclslDiagnostics.Error("cultivator-index:world-snapshot", "刷新世界角色快照失败: " + ex.Message);
                _worldScanActors = Array.Empty<Actor>();
            }
            _worldScanCursor = 0;
            _worldScanActive = _worldScanActors.Count > 0;
        }

        if (!_worldScanActive) return 0;
        int limit = budget == int.MaxValue
            ? _worldScanActors.Count - _worldScanCursor
            : Math.Min(budget, _worldScanActors.Count - _worldScanCursor);
        int observed = 0;
        for (int i = 0; i < limit; i++)
        {
            Actor actor = _worldScanActors[_worldScanCursor++];
            if (actor?.data == null || !MclslActorAccessor.Alive(actor)) continue;
            MclslActorRegistry.Register(actor, out _);
            if (ObserveCultivationState(actor)) MclslWorldActorQuery.MarkDirty();
            observed++;
        }

        if (_worldScanCursor >= _worldScanActors.Count)
        {
            _worldScanActors = Array.Empty<Actor>();
            _worldScanCursor = 0;
            _worldScanActive = false;
        }
        return observed;
    }

    internal static void Observe(Actor actor)
    {
        if (actor?.data == null) return;
        if (!MclslActorRegistry.Register(actor, out long actorId, out _) || actorId <= 0L) return;
        if (NativeKillObservedIds.Add(actorId)) MclslNativeKillStatisticsSystem.Observe(actor);
        if (!MclslActorAccessor.Alive(actor))
        {
            Remove(actorId);
            return;
        }

        if (ObserveCultivationState(actor)) MclslWorldActorQuery.MarkDirty();
    }

    internal static bool Resolve(long actorId, out Actor actor)
    {
        actor = null;
        if (!MclslActorRegistry.Resolve(actorId, out Actor known))
        {
            RemoveCultivationIndexes(actorId);
            return false;
        }
        if (!MclslActorAccessor.Alive(known))
        {
            Remove(actorId);
            return false;
        }
        if (!MclslEligibility.CanCultivate(known))
        {
            // 仍存活但不具备修炼资格的角色只退出修炼索引，
            // 不能从完整角色注册表中删除。
            MarkNonCultivator(actorId);
            return false;
        }
        actor = known;
        return true;
    }

    internal static void MarkNonCultivator(long actorId)
    {
        if (actorId <= 0L) return;
        bool changed = SetCultivator(actorId, false);
        changed |= SetAnnualCandidate(actorId, false);
        changed |= SetRealm(actorId, string.Empty);
        if (changed) MclslWorldActorQuery.MarkDirty();
    }

    internal static IReadOnlyList<long> GetAnnualCandidateIds()
    {
        if (_annualCandidateSnapshotDirty)
        {
            _annualCandidateSnapshot = new long[AnnualCandidateIds.Count];
            AnnualCandidateIds.CopyTo(_annualCandidateSnapshot);
            _annualCandidateSnapshotDirty = false;
        }
        return _annualCandidateSnapshot;
    }

    internal static void RefreshAnnualCandidatesFromKnownActors(int budget = 512)
    {
        IReadOnlyList<Actor> actors = MclslActorRegistry.Snapshot();
        if (actors.Count == 0 || budget <= 0)
        {
            _knownRefreshCursor = 0;
            return;
        }

        if (_knownRefreshCursor >= actors.Count) _knownRefreshCursor = 0;
        int processed = 0;
        int limit = Math.Min(budget, actors.Count);
        while (processed < limit && actors.Count > 0)
        {
            if (_knownRefreshCursor >= actors.Count) _knownRefreshCursor = 0;
            Actor actor = actors[_knownRefreshCursor++];
            processed++;
            if (actor?.data == null || !MclslActorAccessor.Alive(actor)) continue;
            if (ObserveCultivationState(actor)) MclslWorldActorQuery.MarkDirty();
        }
    }

    internal static IReadOnlyList<Actor> GetKnownActorsSnapshot()
    {
        return MclslActorRegistry.Snapshot();
    }

    internal static IReadOnlyList<Actor> GetCultivatorActorsSnapshot()
    {
        if (!_cultivatorSnapshotDirty) return _cultivatorActorSnapshot;

        // dirty 表示成员、角色状态或境界投影至少有一项变化。即使数量相同，
        // 也必须重建 ID 快照，避免“一人退出、一人加入”后仍返回旧成员。
        _cultivatorIdSnapshot = new long[CultivatorIds.Count];
        CultivatorIds.CopyTo(_cultivatorIdSnapshot);
        Array.Sort(_cultivatorIdSnapshot);

        List<Actor> actors = new(_cultivatorIdSnapshot.Length);
        for (int i = 0; i < _cultivatorIdSnapshot.Length; i++)
        {
            long actorId = _cultivatorIdSnapshot[i];
            if (!MclslActorRegistry.Resolve(actorId, out Actor actor)
                || !IsCultivationSnapshotActor(actor)) continue;
            actors.Add(actor);
        }
        _cultivatorActorSnapshot = actors.ToArray();
        _cultivatorSnapshotDirty = false;
        return _cultivatorActorSnapshot;
    }

    internal static IReadOnlyList<Actor> SelectCultivators(
        int limit,
        Func<Actor, bool> predicate = null,
        Func<Actor, int> score = null)
    {
        IReadOnlyList<Actor> source = GetCultivatorActorsSnapshot();
        if (source.Count == 0) return Array.Empty<Actor>();

        List<Actor> result = new(limit > 0 ? Math.Min(limit, source.Count) : source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            Actor actor = source[i];
            if (!MclslActorAccessor.Alive(actor)) continue;
            if (predicate != null && !predicate(actor)) continue;
            result.Add(actor);
        }

        if (score != null)
        {
            result.Sort((left, right) =>
            {
                int compare = score(right).CompareTo(score(left));
                return compare != 0
                    ? compare
                    : MclslActorAccessor.Id(left).CompareTo(MclslActorAccessor.Id(right));
            });
        }

        if (limit > 0 && result.Count > limit)
            result.RemoveRange(limit, result.Count - limit);
        return result;
    }

    internal static IReadOnlyList<Actor> SelectRealm(
        string realm,
        int limit,
        Func<Actor, bool> predicate = null,
        Func<Actor, int> score = null)
    {
        if (string.IsNullOrWhiteSpace(realm)
            || !RealmIds.TryGetValue(realm, out HashSet<long> actorIds)
            || actorIds.Count == 0) return Array.Empty<Actor>();

        List<Actor> actors = new(actorIds.Count);
        foreach (long actorId in actorIds)
        {
            if (!MclslActorRegistry.Resolve(actorId, out Actor actor)
                || !IsCultivationSnapshotActor(actor)
                || (predicate != null && !predicate(actor))) continue;
            actors.Add(actor);
        }
        if (score != null)
        {
            actors.Sort((left, right) =>
            {
                int compare = score(right).CompareTo(score(left));
                return compare != 0
                    ? compare
                    : MclslActorAccessor.Id(left).CompareTo(MclslActorAccessor.Id(right));
            });
        }
        if (limit > 0 && actors.Count > limit) actors.RemoveRange(limit, actors.Count - limit);
        return actors;
    }

    internal static IReadOnlyList<Actor> SelectRealmAtLeast(
        string realm,
        int limit,
        Func<Actor, bool> predicate = null,
        Func<Actor, int> score = null)
    {
        int threshold = MclslRealmIds.Index(realm);
        if (threshold < 0) return Array.Empty<Actor>();
        return SelectCultivators(
            limit,
            actor => MclslRealmIds.Index(MclslActorAccessor.Realm(actor)) >= threshold
                && (predicate == null || predicate(actor)),
            score);
    }

    internal static int CountRealmAtLeast(string realm)
    {
        int threshold = MclslRealmIds.Index(realm);
        if (threshold < 0) return 0;
        int count = 0;
        for (int i = threshold; i < MclslRealmIds.Ordered.Length; i++)
        {
            if (RealmIds.TryGetValue(MclslRealmIds.Ordered[i], out HashSet<long> ids)) count += ids.Count;
        }
        return count;
    }

    internal static void Remove(long actorId)
    {
        if (actorId <= 0L) return;
        RemoveCultivationIndexes(actorId);
        MclslActorRegistry.Unregister(actorId);
    }

    internal static int CleanupInvalid(int budget)
    {
        if (budget <= 0) return 0;
        int removed = MclslActorRegistry.CleanupInvalid(budget, RemoveCultivationIndexes);

        if (CultivatorIds.Count == 0) return removed;
        if (_cultivatorSnapshotDirty || _cultivatorIdSnapshot.Length != CultivatorIds.Count)
        {
            _cultivatorIdSnapshot = new long[CultivatorIds.Count];
            CultivatorIds.CopyTo(_cultivatorIdSnapshot);
        }
        if (_cleanupCursor >= _cultivatorIdSnapshot.Length) _cleanupCursor = 0;
        int checks = Math.Min(Math.Max(16, budget * 2), _cultivatorIdSnapshot.Length);
        for (int i = 0; i < checks && _cultivatorIdSnapshot.Length > 0; i++)
        {
            if (_cleanupCursor >= _cultivatorIdSnapshot.Length) _cleanupCursor = 0;
            long actorId = _cultivatorIdSnapshot[_cleanupCursor++];
            if (!MclslActorRegistry.Resolve(actorId, out Actor actor)
                || !MclslActorAccessor.Alive(actor))
            {
                Remove(actorId);
                removed++;
                continue;
            }
            if (ObserveCultivationState(actor)) MclslWorldActorQuery.MarkDirty();
        }
        return removed;
    }

    internal static void Clear()
    {
        CultivatorIds.Clear();
        AnnualCandidateIds.Clear();
        NativeKillObservedIds.Clear();
        RealmIds.Clear();
        RealmByActorId.Clear();
        _cultivatorIdSnapshot = Array.Empty<long>();
        _annualCandidateSnapshot = Array.Empty<long>();
        _cultivatorActorSnapshot = Array.Empty<Actor>();
        _cultivatorSnapshotDirty = false;
        _annualCandidateSnapshotDirty = false;
        _worldScanActors = Array.Empty<Actor>();
        _worldScanCursor = 0;
        _knownRefreshCursor = 0;
        _cleanupCursor = 0;
        _worldScanActive = false;
        _revision++;
        MclslActorRegistry.Clear();
    }

    private static bool ObserveCultivationState(Actor actor)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L) return false;

        bool eligible = MclslEligibility.CanCultivate(actor);
        bool cultivator = eligible && IsCultivationSnapshotActor(actor);
        bool wasCultivator = CultivatorIds.Contains(actorId);
        bool annualCandidate = eligible && MclslCultivationActorMarker.IsAnnualCandidate(actor);
        bool changed = SetCultivator(actorId, cultivator);
        changed |= SetAnnualCandidate(actorId, annualCandidate);
        changed |= SetRealm(actorId, cultivator ? MclslActorAccessor.Realm(actor) : string.Empty);

        if (cultivator && !wasCultivator)
            MclslLegacyActorDataRepair.RepairTechnique(actor);
        return changed;
    }

    private static bool SetCultivator(long actorId, bool included)
    {
        bool changed = included ? CultivatorIds.Add(actorId) : CultivatorIds.Remove(actorId);
        if (!changed) return false;
        _cultivatorSnapshotDirty = true;
        _cultivatorIdSnapshot = Array.Empty<long>();
        _revision++;
        return true;
    }

    private static bool SetAnnualCandidate(long actorId, bool included)
    {
        bool changed = included ? AnnualCandidateIds.Add(actorId) : AnnualCandidateIds.Remove(actorId);
        if (changed)
        {
            _annualCandidateSnapshotDirty = true;
            _revision++;
        }
        return changed;
    }

    private static bool SetRealm(long actorId, string realm)
    {
        string normalized = string.IsNullOrWhiteSpace(realm) ? string.Empty : realm.Trim();
        RealmByActorId.TryGetValue(actorId, out string previous);
        previous ??= string.Empty;
        if (string.Equals(previous, normalized, StringComparison.Ordinal)) return false;

        if (!string.IsNullOrWhiteSpace(previous)
            && RealmIds.TryGetValue(previous, out HashSet<long> previousSet))
        {
            previousSet.Remove(actorId);
            if (previousSet.Count == 0) RealmIds.Remove(previous);
        }
        RealmByActorId.Remove(actorId);

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            if (!RealmIds.TryGetValue(normalized, out HashSet<long> nextSet))
            {
                nextSet = new HashSet<long>();
                RealmIds[normalized] = nextSet;
            }
            nextSet.Add(actorId);
            RealmByActorId[actorId] = normalized;
        }
        _cultivatorSnapshotDirty = true;
        _revision++;
        return true;
    }

    private static void RemoveCultivationIndexes(long actorId)
    {
        MarkNonCultivator(actorId);
        NativeKillObservedIds.Remove(actorId);
    }

    private static bool IsCultivationSnapshotActor(Actor actor)
    {
        return actor?.data != null
            && MclslActorAccessor.Alive(actor)
            && MclslEligibility.CanCultivate(actor)
            && (MclslActorAccessor.HasCultivationPath(actor)
                || MclslCultivationActorMarker.HasCultivationMarker(actor));
    }
}
