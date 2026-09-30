using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.UI;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式修炼候选索引。角色注册、修士判定、年度候选和境界分桶彼此分离；
/// 所有读取均来自已维护索引，不再在 UI、统计或系统查询时隐式扫描世界。
/// </summary>
internal static class MclslCultivatorCandidateIndex
{
    private static readonly MclslIdSlots CultivatorIds = new();
    private static readonly MclslIdSlots AnnualCandidateIds = new();
    private static readonly HashSet<long> NativeKillObservedIds = new();
    private static readonly Dictionary<string, MclslIdSlots> RealmIds = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, string> RealmByActorId = new();

    private static readonly IReadOnlyList<Actor> CultivatorView = MclslActorRegistry.CreateView(CultivatorIds);

    private static int _cleanupCursor;
    private static long _revision;

    internal static int KnownActorCount => MclslActorRegistry.Count;
    internal static long Revision => _revision;
    internal static int CultivatorCount => CultivatorIds.LiveCount;
    internal static int AnnualCandidateCount => AnnualCandidateIds.LiveCount;
    internal static bool IsCultivator(long actorId) => actorId > 0L && CultivatorIds.Contains(actorId);

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
        MclslScheduler.ForgetAnnualActor(actorId);
        bool changed = SetCultivator(actorId, false);
        changed |= SetAnnualCandidate(actorId, false);
        changed |= SetRealm(actorId, string.Empty);
        MclslTechniqueOccupationSystem.Forget(actorId);
        if (changed) MclslWorldActorQuery.MarkDirty();
    }

    internal static IReadOnlyList<long> GetAnnualCandidateIds()
    {
        return AnnualCandidateIds;
    }

    internal static void AuditRegisteredActors()
    {
        MclslActorRegistry.Audit(64, actor =>
        {
            if (actor?.data == null) return;
            if (!MclslActorAccessor.Alive(actor)) { Remove(MclslActorAccessor.Id(actor)); return; }
            MclslRuntimeChanges.Publish(actor, MclslActorChange.Native);
            bool wasCandidate = AnnualCandidateIds.Contains(MclslActorAccessor.Id(actor));
            if (ObserveCultivationState(actor)) MclslWorldActorQuery.MarkDirty();
            if (!wasCandidate && AnnualCandidateIds.Contains(MclslActorAccessor.Id(actor)))
                MclslScheduler.RegisterAndEnqueueAnnualActor(actor);
        });
    }

    internal static IReadOnlyList<Actor> GetKnownActorsSnapshot()
    {
        return MclslActorRegistry.Snapshot();
    }

    internal static IReadOnlyList<Actor> GetCultivatorActorsSnapshot()
    {
        return CultivatorView;
    }

    internal static int CountRealmAtLeast(string realm)
    {
        int threshold = MclslRealmIds.Index(realm);
        if (threshold < 0) return 0;
        int count = 0;
        for (int i = threshold; i < MclslRealmIds.Ordered.Length; i++)
        {
            if (RealmIds.TryGetValue(MclslRealmIds.Ordered[i], out MclslIdSlots ids)) count += ids.LiveCount;
        }
        return count;
    }

    internal static IReadOnlyList<Actor> RealmActors(string realm)
        => realm != null && RealmIds.TryGetValue(realm, out MclslIdSlots ids)
            ? MclslActorRegistry.CreateView(ids) : Array.Empty<Actor>();

    internal static void Remove(long actorId)
    {
        if (actorId <= 0L) return;
        try { RemoveCultivationIndexes(actorId); }
        finally { MclslActorRegistry.Unregister(actorId); }
    }

    internal static int CleanupInvalid(int budget)
    {
        if (budget <= 0) return 0;
        int removed = MclslActorRegistry.CleanupInvalid(budget, RemoveCultivationIndexes);

        if (CultivatorIds.Count == 0) return removed;
        if (_cleanupCursor >= CultivatorIds.Count) _cleanupCursor = 0;
        int checks = Math.Min(Math.Max(16, budget * 2), CultivatorIds.Count);
        for (int i = 0; i < checks && !MclslFrameDeadline.Expired; i++)
        {
            if (_cleanupCursor >= CultivatorIds.Count) _cleanupCursor = 0;
            long actorId = CultivatorIds[_cleanupCursor++];
            if (actorId == 0) continue;
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
        _cleanupCursor = 0;
        _revision++;
        MclslActorRegistry.Clear();
    }

    private static bool ObserveCultivationState(Actor actor)
    {
        long actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L) return false;

        bool eligible = MclslEligibility.CanCultivate(actor);
        bool cultivator = eligible && IsCultivationSnapshotActor(actor);
        bool annualCandidate = eligible && MclslCultivationActorMarker.IsAnnualCandidate(actor);
        if (!annualCandidate) MclslScheduler.ForgetAnnualActor(actorId);
        bool changed = SetCultivator(actorId, cultivator);
        changed |= SetAnnualCandidate(actorId, annualCandidate);
        changed |= SetRealm(actorId, cultivator ? MclslActorAccessor.Realm(actor) : string.Empty);

        if (cultivator)
        {
            // Reconcile the technique index while the annual known-actor scan is
            // already visiting this actor. This replaces a full index rebuild once
            // per year and also catches state changes made outside our own writers.
            MclslTechniqueOccupationSystem.OnCultivationStateChanged(actor);
        }
        return changed;
    }

    private static bool SetCultivator(long actorId, bool included)
    {
        bool changed = included ? CultivatorIds.Add(actorId) : CultivatorIds.Remove(actorId);
        if (!changed) return false;
        if (!included) { MclslRankSnapshotSource.Remove(actorId); MclslActorProjectionIndex.Remove(actorId); }
        else if (MclslActorRegistry.Resolve(actorId, out Actor actor))
            MclslRuntimeChanges.Publish(actor, MclslActorChange.Data);
        else MclslRankSnapshotSource.Remove(actorId);
        _revision++;
        return true;
    }

    private static bool SetAnnualCandidate(long actorId, bool included)
    {
        bool changed = included ? AnnualCandidateIds.Add(actorId) : AnnualCandidateIds.Remove(actorId);
        if (changed)
        {
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
            && RealmIds.TryGetValue(previous, out MclslIdSlots previousSet))
        {
            previousSet.Remove(actorId);
            if (previousSet.LiveCount == 0) RealmIds.Remove(previous);
        }
        RealmByActorId.Remove(actorId);

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            if (!RealmIds.TryGetValue(normalized, out MclslIdSlots nextSet))
            {
                nextSet = new MclslIdSlots();
                RealmIds[normalized] = nextSet;
            }
            nextSet.Add(actorId);
            RealmByActorId[actorId] = normalized;
        }
        _revision++;
        return true;
    }

    private static void RemoveCultivationIndexes(long actorId)
    {
        MarkNonCultivator(actorId);
        NativeKillObservedIds.Remove(actorId);
        MclslNativeKillStatisticsSystem.Forget(actorId);
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
