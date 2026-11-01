using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MySimulatedLongevityRoad.Core;

/// <summary>
/// Stable ID slots, recycled on removal. Cursors never shift when another ID dies.
/// <para>
/// CONTRACT: a removed slot reads back as <c>0</c>, while <see cref="Count"/> still
/// counts those holes. Any consumer that walks this list by index MUST treat
/// <c>0</c> as "no actor" and skip it; use <see cref="LiveCount"/> when the true
/// population matters. <see cref="EnumerateLive"/> yields only live ids.
/// </para>
/// </summary>
internal sealed class MclslIdSlots : IReadOnlyList<long>
{
    private readonly List<long> _slots = new();
    private readonly Dictionary<long, int> _positions = new();
    private readonly Stack<int> _free = new();
    public int Count => _slots.Count;
    internal int LiveCount => _positions.Count;
    public long this[int index] => _slots[index];

    /// <summary>Yields only live ids, skipping recycled zero slots.</summary>
    internal IEnumerable<long> EnumerateLive()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            long id = _slots[i];
            if (id != 0) yield return id;
        }
    }
    internal bool Contains(long id) => _positions.ContainsKey(id);
    internal bool Add(long id)
    {
        if (id <= 0 || _positions.ContainsKey(id)) return false;
        int slot;
        if (_free.Count > 0) { slot = _free.Pop(); _slots[slot] = id; }
        else { slot = _slots.Count; _slots.Add(id); }
        _positions.Add(id, slot);
        return true;
    }
    internal bool Remove(long id)
    {
        if (!_positions.TryGetValue(id, out int slot)) return false;
        _positions.Remove(id);
        _slots[slot] = 0;
        _free.Push(slot);
        return true;
    }
    internal void Clear() { _slots.Clear(); _positions.Clear(); _free.Clear(); }
    public IEnumerator<long> GetEnumerator() => _slots.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>One pending node per ID. Cancellation unlinks it immediately; no stale queue tails.</summary>
internal sealed class MclslChangeQueue
{
    private struct Change { internal long Id; internal int Flags; }
    private readonly LinkedList<Change> _queue = new();
    private readonly Dictionary<long, LinkedListNode<Change>> _nodes = new();
    private readonly Stack<LinkedListNode<Change>> _pool = new();
    internal int Count => _nodes.Count;
    internal void Publish(long id, int flags)
    {
        if (id <= 0 || flags == 0) return;
        if (_nodes.TryGetValue(id, out LinkedListNode<Change> node))
        {
            Change change = node.Value;
            change.Flags |= flags;
            node.Value = change;
            MclslPerformanceProbe.RecordChangeQueueMerged();
            return;
        }
        node = _pool.Count == 0 ? new LinkedListNode<Change>(default) : _pool.Pop();
        node.Value = new Change { Id = id, Flags = flags };
        _nodes.Add(id, node);
        _queue.AddLast(node);
    }
    internal bool TryTake(out long id, out int flags)
    {
        LinkedListNode<Change> node = _queue.First;
        if (node == null) { id = 0; flags = 0; return false; }
        id = node.Value.Id; flags = node.Value.Flags;
        _nodes.Remove(id); _queue.RemoveFirst(); Recycle(node);
        return true;
    }
    internal void Cancel(long id)
    {
        if (!_nodes.TryGetValue(id, out LinkedListNode<Change> node)) return;
        _nodes.Remove(id); _queue.Remove(node); Recycle(node);
    }
    private void Recycle(LinkedListNode<Change> node)
    {
        node.Value = default;
        if (_pool.Count < 8192) _pool.Push(node);
    }
    internal void Clear() { _queue.Clear(); _nodes.Clear(); _pool.Clear(); }
}

/// <summary>Coalesces worker callbacks before they touch main-thread actor indexes.</summary>
internal sealed class MclslActorThreadHandoff<T> where T : class
{
    private struct Change
    {
        internal T Actor;
        internal int Flags;
        internal bool RepairStats;
        internal long Generation;
    }

    private readonly object _sync = new();
    private readonly LinkedList<long> _order = new();
    private readonly Stack<LinkedListNode<long>> _pool = new();
    private readonly Dictionary<long, (LinkedListNode<long> Node, Change Value)> _pending = new();

    internal int Count { get { lock (_sync) return _pending.Count; } }

    internal void Publish(long id, T actor, int flags, bool repairStats, long generation = 0)
    {
        if (id <= 0 || actor == null) return;
        lock (_sync)
        {
            if (_pending.TryGetValue(id, out var entry))
            {
                Change change = entry.Value;
                change.Actor = actor;
                change.Generation = generation;
                change.Flags |= flags;
                change.RepairStats |= repairStats;
                _pending[id] = (entry.Node, change);
                MclslPerformanceProbe.RecordChangeQueueMerged();
                return;
            }
            LinkedListNode<long> node = _pool.Count == 0 ? new LinkedListNode<long>(id) : _pool.Pop();
            node.Value = id;
            _order.AddLast(node);
            _pending[id] = (node, new Change { Actor = actor, Flags = flags, RepairStats = repairStats, Generation = generation });
        }
    }

    internal bool TryTake(out long id, out T actor, out int flags, out bool repairStats)
        => TryTake(out id,out actor,out flags,out repairStats,out _);
    internal bool TryTake(out long id, out T actor, out int flags, out bool repairStats, out long generation)
    {
        lock (_sync)
        {
            if (_order.First != null)
            {
                LinkedListNode<long> node = _order.First;
                id = node.Value;
                _order.RemoveFirst();
                Recycle(node);
                Change change = _pending[id].Value;
                _pending.Remove(id);
                actor = change.Actor; flags = change.Flags; repairStats = change.RepairStats; generation = change.Generation;
                return true;
            }
        }
        id = 0; actor = null; flags = 0; repairStats = false; generation = 0;
        return false;
    }

    internal void Cancel(long id)
    {
        lock (_sync)
        {
            if (!_pending.TryGetValue(id, out var entry)) return;
            _order.Remove(entry.Node);
            Recycle(entry.Node);
            _pending.Remove(id);
        }
    }
    private void Recycle(LinkedListNode<long> node)
    {
        node.Value = 0;
        if (_pool.Count < 8192) _pool.Push(node);
    }
    internal void Clear() { lock (_sync) { _pending.Clear(); _order.Clear(); _pool.Clear(); } }
}

/// <summary>A deadline shared by all background consumers in one rendered frame.</summary>
internal static class MclslFrameDeadline
{
    private static long _deadline;
    internal static bool Expired => _deadline != 0 && Stopwatch.GetTimestamp() >= _deadline;
    internal static double RemainingMs => _deadline == 0 ? double.MaxValue
        : Math.Max(0, (_deadline - Stopwatch.GetTimestamp()) * 1000d / Stopwatch.Frequency);
    internal static void Begin(double milliseconds) => _deadline = Stopwatch.GetTimestamp()
        + (long)(Math.Max(0.1, milliseconds) * Stopwatch.Frequency / 1000d);
    internal static void End() => _deadline = 0;
}
