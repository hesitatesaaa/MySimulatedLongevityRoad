using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Stable ID slots, recycled on removal. Cursors never shift when another ID dies.</summary>
internal sealed class MclslIdSlots : IReadOnlyList<long>
{
    private readonly List<long> _slots = new();
    private readonly Dictionary<long, int> _positions = new();
    private readonly Stack<int> _free = new();
    public int Count => _slots.Count;
    internal int LiveCount => _positions.Count;
    public long this[int index] => _slots[index];
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
    }

    private const int MaximumPending = 32768;
    private readonly object _sync = new();
    private readonly LinkedList<long> _order = new();
    private readonly Dictionary<long, (LinkedListNode<long> Node, Change Value)> _pending = new();

    internal int Count { get { lock (_sync) return _pending.Count; } }

    internal void Publish(long id, T actor, int flags, bool repairStats)
    {
        if (id <= 0 || actor == null) return;
        lock (_sync)
        {
            if (_pending.TryGetValue(id, out var entry))
            {
                Change change = entry.Value;
                change.Actor = actor;
                change.Flags |= flags;
                change.RepairStats |= repairStats;
                _pending[id] = (entry.Node, change);
                return;
            }
            if (_pending.Count >= MaximumPending)
            {
                // The next native callback will re-enqueue an evicted actor.
                long oldest = _order.First.Value;
                _order.RemoveFirst();
                _pending.Remove(oldest);
            }
            LinkedListNode<long> node = _order.AddLast(id);
            _pending[id] = (node, new Change { Actor = actor, Flags = flags, RepairStats = repairStats });
        }
    }

    internal bool TryTake(out long id, out T actor, out int flags, out bool repairStats)
    {
        lock (_sync)
        {
            if (_order.First != null)
            {
                id = _order.First.Value;
                _order.RemoveFirst();
                Change change = _pending[id].Value;
                _pending.Remove(id);
                actor = change.Actor; flags = change.Flags; repairStats = change.RepairStats;
                return true;
            }
        }
        id = 0; actor = null; flags = 0; repairStats = false;
        return false;
    }

    internal void Cancel(long id)
    {
        lock (_sync)
        {
            if (!_pending.TryGetValue(id, out var entry)) return;
            _order.Remove(entry.Node);
            _pending.Remove(id);
        }
    }
    internal void Clear() { lock (_sync) { _pending.Clear(); _order.Clear(); } }
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
