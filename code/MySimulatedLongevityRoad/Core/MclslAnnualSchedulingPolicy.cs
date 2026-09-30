using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MySimulatedLongevityRoad.Core;

// Membership is exclusive. Waiting actors do not consume dequeue attempts.
internal sealed class MclslAnnualReadyQueue
{
    private sealed class Entry
    {
        internal int Year;
        internal bool Ready;
        internal LinkedListNode<long> Node;
    }
    private readonly Stack<Entry> _freeEntries = new();
    private readonly LinkedList<long> _ready = new();
    private readonly SortedDictionary<int, LinkedList<long>> _waiting = new();
    private readonly Dictionary<long, Entry> _entries = new();
    internal int ReadyCount => _ready.Count;
    internal int WaitingCount => _entries.Count - _ready.Count;
    internal int Count => _entries.Count;
    internal int NewestWaitingYear
    {
        get { int year = 0; foreach (var pair in _waiting) year = pair.Key; return year; }
    }
    internal int OldestWaitingYear
    {
        get { foreach (var pair in _waiting) return pair.Key; return 0; }
    }

    internal void Enqueue(long id, int year, int worldYear)
    {
        if (id <= 0 || year <= 0) return;
        bool ready = worldYear > 0 && year <= worldYear;
        if (_entries.TryGetValue(id, out Entry entry))
        {
            if (entry.Year == year && entry.Ready == ready) return;
            Remove(id);
        }
        LinkedList<long> list = _ready;
        if (!ready && !_waiting.TryGetValue(year, out list))
            _waiting[year] = list = new LinkedList<long>();
        entry = _freeEntries.Count > 0 ? _freeEntries.Pop() : new Entry { Node = new LinkedListNode<long>(id) };
        entry.Year = year;
        entry.Ready = ready;
        entry.Node.Value = id;
        list.AddLast(entry.Node);
        _entries[id] = entry;
    }

    internal bool TryDequeue(out long id)
    {
        id = 0;
        if (_ready.First == null) return false;
        id = _ready.First.Value;
        Entry entry = _entries[id];
        _ready.RemoveFirst();
        _entries.Remove(id);
        Recycle(entry);
        return true;
    }

    internal int Release(int worldYear, int budget, Func<bool> expired)
    {
        int released = 0;
        while (released < budget && !expired())
        {
            int year = OldestWaitingYear;
            if (year <= 0 || year > worldYear) break;
            LinkedList<long> bucket = _waiting[year];
            long id = bucket.First.Value;
            bucket.RemoveFirst();
            if (bucket.Count == 0) _waiting.Remove(year);
            Entry entry = _entries[id];
            entry.Ready = true;
            _ready.AddLast(entry.Node);
            released++;
        }
        return released;
    }

    internal void Remove(long id)
    {
        if (!_entries.TryGetValue(id, out Entry entry)) return;
        if (entry.Ready) _ready.Remove(entry.Node);
        else
        {
            LinkedList<long> bucket = _waiting[entry.Year];
            bucket.Remove(entry.Node);
            if (bucket.Count == 0) _waiting.Remove(entry.Year);
        }
        _entries.Remove(id);
        Recycle(entry);
    }

    private void Recycle(Entry entry)
    {
        if (_freeEntries.Count < 512) _freeEntries.Push(entry);
    }

    internal void Clear() { _ready.Clear(); _waiting.Clear(); _entries.Clear(); _freeEntries.Clear(); }
}

internal sealed class MclslAnnualSpeedPolicy
{
    internal int Level { get; private set; }
    private double _recoverySince = -1d;
    internal float Factor => Level switch { 1 => 0.5f, 2 => 0.25f, 3 => 0f, _ => 1f };
    internal void Reset() { Level = 0; _recoverySince = -1d; }
    internal void Update(int lag, double unscaledTime, bool enabled)
    {
        if (!enabled) { Reset(); return; }
        int requested = lag >= 8 ? 3 : lag >= 6 ? 2 : lag >= 3 ? 1 : 0;
        if (requested > Level) { Level = requested; _recoverySince = -1d; return; }
        int recoverAt = Level switch { 3 => 5, 2 => 3, 1 => 1, _ => -1 };
        if (lag > recoverAt || Level == 0) { _recoverySince = -1d; return; }
        if (_recoverySince < 0d || unscaledTime < _recoverySince) _recoverySince = unscaledTime;
        if (unscaledTime - _recoverySince < 2d) return;
        Level--;
        _recoverySince = -1d;
    }
}

internal static class MclslAnnualFrameBudget
{
    private static long _started;
    private static double _limit;
    private static bool _active;
    internal static double ForPressure(int tier) => tier switch { 1 => 1d, 2 => 0.75d, 3 => 0.5d, _ => 1.5d };
    internal static void Begin(double milliseconds) { _started = Stopwatch.GetTimestamp(); _limit = milliseconds; _active = true; }
    internal static double RemainingMs => Math.Min(MclslFrameDeadline.RemainingMs, !_active ? double.MaxValue
        : Math.Max(0d, _limit - (Stopwatch.GetTimestamp() - _started) * 1000d / Stopwatch.Frequency));
    internal static bool Expired => RemainingMs <= 0d;
    internal static void End() => _active = false;
}
