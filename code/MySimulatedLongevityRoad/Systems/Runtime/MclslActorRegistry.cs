using System;
using System.Collections.Generic;
using System.Collections;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式运行时角色注册表。角色引用只由原生生命周期回调与读档后的有界重建写入，
/// 查询接口绝不为了“顺便补索引”而扫描世界，也不在读取统计时改变修炼状态。
/// </summary>
internal static class MclslActorRegistry
{
    private static readonly Dictionary<long, Actor> ActorsById = new();
    private static readonly Dictionary<long, (long Attack, long Behavior)> TargetsByActor = new();
    private static readonly Dictionary<long, MclslOrderedIdIndex<long>> Targeters = new();
    internal static void RefreshTargets(Actor actor, bool forceEligibility = false)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0 || !ActorsById.ContainsKey(id)) return;
        Actor attackTarget = actor.attack_target as Actor, behaviorTarget = actor.beh_actor_target as Actor;
        long attack = MclslActorAccessor.Alive(attackTarget) ? MclslActorAccessor.Id(attackTarget) : 0;
        long behavior = MclslActorAccessor.Alive(behaviorTarget) ? MclslActorAccessor.Id(behaviorTarget) : 0;
        TargetsByActor.TryGetValue(id, out var before);
        if (!forceEligibility && before.Attack == attack && before.Behavior == behavior) return;
        if ((attack > 0 || behavior > 0) && !MclslEligibility.CanClaimWorldSoul(actor)) attack = behavior = 0;
        if (before.Attack == attack && before.Behavior == behavior) return;
        RemoveTargets(id);
        if (attack == 0 && behavior == 0) return;
        TargetsByActor[id] = (attack, behavior);
        AddTargeter(attack, id); if (behavior != attack) AddTargeter(behavior, id);
    }
    private static void AddTargeter(long target, long id)
    {
        if (target <= 0) return;
        if (!Targeters.TryGetValue(target, out var order)) Targeters[target] = order = new((a, b) => a.CompareTo(b));
        order.Upsert(id, id);
    }
    private static void RemoveTargeter(long target, long id)
    {
        if (!Targeters.TryGetValue(target, out var order)) return;
        order.Remove(id); if (order.Count == 0) Targeters.Remove(target);
    }
    private static void RemoveTargets(long id)
    {
        if (!TargetsByActor.TryGetValue(id, out var targets)) return;
        RemoveTargeter(targets.Attack, id); RemoveTargeter(targets.Behavior, id); TargetsByActor.Remove(id);
    }
    internal static Actor FindTargeter(long target)
    {
        if (!Targeters.TryGetValue(target, out var order)) return null;
        for (int i = 0; i < Math.Min(16, order.Count); i++)
        {
            if (!Resolve(order[i], out Actor actor) || !MclslEligibility.CanClaimWorldSoul(actor)) continue;
            if (MclslActorAccessor.Id(actor.attack_target as Actor) == target || MclslActorAccessor.Id(actor.beh_actor_target as Actor) == target) return actor;
        }
        return null;
    }
    private static readonly MclslIdSlots Ids = new();
    private static readonly IReadOnlyList<Actor> View = new ActorView(Ids);
    private static readonly Dictionary<string, MclslIdSlots> TraitIds = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, List<string>> TraitsByActor = new();
    private static readonly Dictionary<string, MclslIdSlots> IdentityIds = new(StringComparer.Ordinal);
    private static readonly Dictionary<long, string> IdentityByActor = new();
    private static readonly Dictionary<long, (string Cave, string Change)> ResourceByActor = new();
    private static readonly Dictionary<string, int> CaveReferences = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> ChangeReferences = new(StringComparer.Ordinal);
    internal static IEnumerable<string> LiveCaveReferences => CaveReferences.Keys;
    internal static IEnumerable<string> LiveChangeReferences => ChangeReferences.Keys;
    private static readonly Dictionary<long, long> CityByActor = new();
    private static readonly Dictionary<long, MclslOrderedIdIndex<long>> CityActors = new();
    private static readonly MclslOrderedIdIndex<long> CityOrder = new((a, b) => a.CompareTo(b));
    private static readonly IReadOnlyList<Actor> CityView = new CityRepresentativesView();
    internal static IReadOnlyList<Actor> CityRepresentatives => CityView;
    private sealed class CityRepresentativesView : IReadOnlyList<Actor>
    {
        public int Count => Math.Min(64, CityOrder.Count);
        public Actor this[int index]
        {
            get
            {
                long city = CityOrder[index];
                return CityActors.TryGetValue(city, out var order) && order.Count > 0
                    && Resolve(order[0], out Actor actor) && MclslActorAccessor.Alive(actor) ? actor : null;
            }
        }
        public IEnumerator<Actor> GetEnumerator() { for (int i = 0; i < Count; i++) yield return this[i]; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private static int _cleanupCursor;
    private static int _auditCursor;
    private static long _registryRevision;
    private sealed class ActorView : IReadOnlyList<Actor>
    {
        private readonly IReadOnlyList<long> _ids;
        internal ActorView(IReadOnlyList<long> ids) => _ids = ids;
        public int Count => _ids.Count;
        public Actor this[int index] => Resolve(_ids[index], out Actor actor) ? actor : null;
        public IEnumerator<Actor> GetEnumerator()
        { for (int i = 0; i < Count; i++) yield return this[i]; }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    internal static IReadOnlyList<Actor> CreateView(IReadOnlyList<long> ids) => new ActorView(ids);

    internal static int Count => ActorsById.Count;
    internal static long Revision => _registryRevision;

    internal static bool Register(Actor actor, out long actorId)
    {
        return Register(actor, out actorId, out _);
    }

    internal static bool Register(Actor actor, out long actorId, out bool isNew)
    {
        actorId = 0L;
        isNew = false;
        if (actor?.data == null) return false;

        actorId = MclslActorAccessor.Id(actor);
        if (actorId <= 0L) return false;

        isNew = !ActorsById.TryGetValue(actorId, out Actor existing);
        if (isNew || !ReferenceEquals(existing, actor))
        {
            ActorsById[actorId] = actor;
            Ids.Add(actorId);
            RefreshTraits(actor); RefreshIdentity(actor); RefreshResourceReferences(actor); RefreshLocation(actor); RefreshTargets(actor);
            InvalidateSnapshot();
            MclslRuntimeChanges.Publish(actor, MclslActorChange.All);
        }

        return true;
    }

    internal static bool Resolve(long actorId, out Actor actor)
    {
        actor = null;
        if (actorId <= 0L || !ActorsById.TryGetValue(actorId, out Actor known)) return false;
        if (known?.data == null) return false;
        actor = known;
        return true;
    }

    internal static bool ResolveKnownOrWorld(long actorId, out Actor actor)
    {
        if (Resolve(actorId, out actor)) return true;
        actor = actorId > 0L ? World.world?.units?.get(actorId) : null;
        if (actor?.data == null) return false;
        Register(actor, out _);
        return true;
    }

    internal static void Unregister(long actorId)
    {
        if (actorId <= 0L) return;
        ActorsById.TryGetValue(actorId, out Actor removed);
        bool existed = ActorsById.Remove(actorId);
        Ids.Remove(actorId);
        RemoveSecondaryIndexes(actorId);
        MclslBagSystem.Forget(actorId);
        MclslItemEffectDriver.Forget(actorId);
        MclslRuntimeChanges.Remove(actorId);
        if (existed) InvalidateSnapshot();
        if (removed == null) return;
        // Remove the authoritative identity before invoking gameplay callbacks.
        // One failed callback must not retain a dead actor or its other caches.
        for (int domain = 0; domain < 5; domain++)
        {
            try
            {
                switch (domain)
                {
                    case 0: MclslAncientMentorshipSystem.OnActorRemoved(removed); break;
                    case 1: MclslSpellSystem.Forget(removed); break;
                    case 2: MclslArtifactSystem.Forget(removed); break;
                    case 3: MclslRecipeKnowledge.Forget(removed); break;
                    case 4: MclslSpiritualRootSystem.Forget(removed); break;
                }
            }
            catch (Exception ex) { MclslDiagnostics.Error("actor-forget-" + domain, "人物清理阶段 " + domain + " 失败：" + ex.Message); }
        }
    }

    internal static IReadOnlyList<Actor> Snapshot()
    {
        return View;
    }

    internal static int CleanupInvalid(int maxRemoveCount, Action<long> onRemoved)
    {
        if (maxRemoveCount <= 0 || ActorsById.Count == 0) return 0;

        int checks = Math.Min(Math.Max(maxRemoveCount * 4, 32), Ids.Count);
        int removed = 0;
        for (int i = 0; i < checks && removed < maxRemoveCount && !MclslFrameDeadline.Expired; i++)
        {
            if (_cleanupCursor >= Ids.Count) _cleanupCursor = 0;
            long actorId = Ids[_cleanupCursor++];
            if (actorId == 0) continue;
            if (ActorsById.TryGetValue(actorId, out Actor actor)
                && actor?.data != null
                && MclslActorAccessor.Alive(actor))
            {
                continue;
            }

            Unregister(actorId);
            removed++;
            onRemoved?.Invoke(actorId);
        }
        return removed;
    }

    internal static void Audit(int budget, Action<Actor> observe)
    {
        int checks = Math.Min(budget, Ids.Count);
        for (int i = 0; i < checks && !MclslFrameDeadline.Expired; i++)
        {
            if (_auditCursor >= Ids.Count) _auditCursor = 0;
            long id = Ids[_auditCursor++];
            if (!ActorsById.TryGetValue(id, out Actor actor)) continue;
            observe(actor);
        }
    }

    internal static void Clear()
    {
        TargetsByActor.Clear(); Targeters.Clear();
        TraitIds.Clear(); TraitsByActor.Clear(); IdentityIds.Clear(); IdentityByActor.Clear();
        CaveReferences.Clear(); ChangeReferences.Clear(); ResourceByActor.Clear();
        CityByActor.Clear(); CityActors.Clear(); CityOrder.Clear();
        ActorsById.Clear();
        Ids.Clear();
        _cleanupCursor = _auditCursor = 0;
        _registryRevision = 0L;
    }

    internal static IReadOnlyList<Actor> TraitActors(string traitId)
        => traitId != null && TraitIds.TryGetValue(traitId, out MclslIdSlots ids)
            ? CreateView(ids) : Array.Empty<Actor>();

    internal static void RefreshTraits(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0 || !ActorsById.ContainsKey(id)) return;
        if (!TraitsByActor.TryGetValue(id, out List<string> previous)) TraitsByActor[id] = previous = new();
        bool same = actor.traits != null && previous.Count == actor.traits.Count;
        if (same)
        {
            int i = 0;
            foreach (ActorTrait trait in actor.traits)
                if (trait?.id != previous[i++]) { same = false; break; }
        }
        if (same) return;
        foreach (string trait in previous) RemoveBucketId(TraitIds, trait, id);
        previous.Clear();
        if (actor.traits == null) return;
        foreach (ActorTrait trait in actor.traits)
        {
            if (string.IsNullOrWhiteSpace(trait?.id) || previous.Contains(trait.id)) continue;
            previous.Add(trait.id); AddBucketId(TraitIds, trait.id, id);
        }
    }

    internal static void RefreshLocation(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0 || !ActorsById.ContainsKey(id)) return;
        long city = actor.city?.data?.id ?? 0;
        CityByActor.TryGetValue(id, out long previous);
        if (previous == city) return;
        RemoveLocation(id);
        if (city <= 0) return;
        if (!CityActors.TryGetValue(city, out var order))
        {
            CityActors[city] = order = new((a, b) => a.CompareTo(b));
            CityOrder.Upsert(city, city);
        }
        CityByActor[id] = city; order.Upsert(id, id);
    }
    private static void RemoveLocation(long id)
    {
        if (!CityByActor.TryGetValue(id, out long city)) return;
        CityByActor.Remove(id);
        if (!CityActors.TryGetValue(city, out var order)) return;
        order.Remove(id);
        if (order.Count == 0) { CityActors.Remove(city); CityOrder.Remove(city); }
    }

    internal static void RefreshIdentity(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0 || !ActorsById.ContainsKey(id)) return;
        string next = MclslActorAccessor.GetString(actor, MclslActorDataKeys.HuanzhenIdentity);
        IdentityByActor.TryGetValue(id, out string before);
        if (before == next) return;
        if (!string.IsNullOrEmpty(before)) RemoveBucketId(IdentityIds, before, id);
        IdentityByActor.Remove(id);
        if (next.Length > 0) { IdentityByActor[id] = next; AddBucketId(IdentityIds, next, id); }
    }

    internal static Actor FindIdentity(string identity)
    {
        if (string.IsNullOrEmpty(identity) || !IdentityIds.TryGetValue(identity, out MclslIdSlots ids)) return null;
        for (int i = 0; i < ids.Count; i++)
            if (Resolve(ids[i], out Actor actor) && MclslActorAccessor.Alive(actor)) return actor;
        return null;
    }

    internal static void RefreshResourceReferences(Actor actor)
    {
        long id = MclslActorAccessor.Id(actor);
        if (id <= 0 || !ActorsById.ContainsKey(id)) return;
        string cave = MclslActorAccessor.GetString(actor, MclslActorDataKeys.NascentCaveId);
        string change = MclslActorAccessor.GetString(actor, MclslActorDataKeys.DivineChangeId);
        ResourceByActor.TryGetValue(id, out var before);
        if (before.Cave != cave) { Decrement(CaveReferences, before.Cave); Increment(CaveReferences, cave); }
        if (before.Change != change) { Decrement(ChangeReferences, before.Change); Increment(ChangeReferences, change); }
        if (cave.Length == 0 && change.Length == 0) ResourceByActor.Remove(id);
        else ResourceByActor[id] = (cave, change);
    }

    private static void RemoveSecondaryIndexes(long id)
    {
        RemoveTargets(id); Targeters.Remove(id);
        RemoveLocation(id);
        if (TraitsByActor.TryGetValue(id, out List<string> traits))
        { foreach (string trait in traits) RemoveBucketId(TraitIds, trait, id); TraitsByActor.Remove(id); }
        if (IdentityByActor.TryGetValue(id, out string identity))
        { RemoveBucketId(IdentityIds, identity, id); IdentityByActor.Remove(id); }
        if (ResourceByActor.TryGetValue(id, out var resources))
        { Decrement(CaveReferences, resources.Cave); Decrement(ChangeReferences, resources.Change); ResourceByActor.Remove(id); }
    }
    private static void AddBucketId(Dictionary<string, MclslIdSlots> buckets, string key, long id)
    {
        if (!buckets.TryGetValue(key, out MclslIdSlots ids)) buckets[key] = ids = new();
        ids.Add(id);
    }
    private static void RemoveBucketId(Dictionary<string, MclslIdSlots> buckets, string key, long id)
    {
        if (!buckets.TryGetValue(key, out MclslIdSlots ids)) return;
        ids.Remove(id); if (ids.LiveCount == 0) buckets.Remove(key);
    }
    private static void Increment(Dictionary<string, int> counts, string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        counts.TryGetValue(key, out int count); counts[key] = count + 1;
    }
    private static void Decrement(Dictionary<string, int> counts, string key)
    {
        if (string.IsNullOrEmpty(key) || !counts.TryGetValue(key, out int count)) return;
        if (count <= 1) counts.Remove(key); else counts[key] = count - 1;
    }

    private static void InvalidateSnapshot()
    {
        _registryRevision++;
    }
}
