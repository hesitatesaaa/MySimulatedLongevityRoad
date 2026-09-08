using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// 玄鉴式运行时角色注册表。角色引用只由原生生命周期回调与读档后的有界重建写入，
/// 查询接口绝不为了“顺便补索引”而扫描世界，也不在读取统计时改变修炼状态。
/// </summary>
internal static class MclslActorRegistry
{
    private static readonly Dictionary<long, Actor> ActorsById = new();
    private static readonly Queue<long> CleanupQueue = new();
    private static Actor[] _cachedSnapshot = Array.Empty<Actor>();
    private static long _registryRevision;
    private static long _snapshotRevision = -1L;

    internal static int Count => ActorsById.Count;

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
            InvalidateSnapshot();
        }

        if (isNew) CleanupQueue.Enqueue(actorId);
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
        if (ActorsById.Remove(actorId)) InvalidateSnapshot();
    }

    internal static IReadOnlyList<Actor> Snapshot()
    {
        if (_snapshotRevision == _registryRevision) return _cachedSnapshot;
        if (ActorsById.Count == 0)
        {
            _cachedSnapshot = Array.Empty<Actor>();
            _snapshotRevision = _registryRevision;
            return _cachedSnapshot;
        }

        List<Actor> actors = new(ActorsById.Count);
        foreach (Actor actor in ActorsById.Values)
        {
            if (actor?.data != null) actors.Add(actor);
        }
        _cachedSnapshot = actors.ToArray();
        _snapshotRevision = _registryRevision;
        return _cachedSnapshot;
    }

    internal static int CleanupInvalid(int maxRemoveCount, Action<long> onRemoved)
    {
        if (maxRemoveCount <= 0 || ActorsById.Count == 0 || CleanupQueue.Count == 0) return 0;

        int checks = Math.Min(Math.Max(maxRemoveCount * 4, 32), CleanupQueue.Count);
        int removed = 0;
        for (int i = 0; i < checks && removed < maxRemoveCount; i++)
        {
            long actorId = CleanupQueue.Dequeue();
            if (ActorsById.TryGetValue(actorId, out Actor actor)
                && actor?.data != null
                && MclslActorAccessor.Alive(actor))
            {
                CleanupQueue.Enqueue(actorId);
                continue;
            }

            if (!ActorsById.Remove(actorId)) continue;
            removed++;
            InvalidateSnapshot();
            onRemoved?.Invoke(actorId);
        }
        return removed;
    }

    internal static void Clear()
    {
        ActorsById.Clear();
        CleanupQueue.Clear();
        _cachedSnapshot = Array.Empty<Actor>();
        _registryRevision = 0L;
        _snapshotRevision = -1L;
    }

    private static void InvalidateSnapshot()
    {
        _registryRevision++;
    }
}
