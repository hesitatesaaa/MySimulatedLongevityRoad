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

// Keep the selected baseline policy while using the newer bounded work budget.
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
    internal const int OperationLimit = 4096;
    internal static Func<long> Timestamp = Stopwatch.GetTimestamp;
    private static long _started;
    private static double _limit;
    private static bool _active;
    private static int _operations;
    internal static bool Active => _active;
    internal static int Operations => _operations;
    internal static long ProgressVersion { get; private set; }
    internal static void ReportProgress() { ProgressVersion++; }
    internal static double LastAvailableMs { get; private set; }
    internal static double ForLag(int lagYears) => lagYears >= 3 ? 2.5d : 1.5d;
    internal static double ElapsedMs => !_active ? 0d
        : Math.Max(0d, (Timestamp() - _started) * 1000d / Stopwatch.Frequency);
    internal static int RemainingOperations => !_active ? OperationLimit : Math.Max(0, OperationLimit - _operations);
    internal static double LastElapsedMs { get; private set; }
    internal static bool LastTimeExhausted { get; private set; }
    internal static bool LastOperationsExhausted { get; private set; }
    internal static void Begin(double milliseconds)
    {
        _started = Timestamp(); _limit = milliseconds; _operations = 0; _active = true;
        ProgressVersion = 0;
        LastAvailableMs = Math.Min(milliseconds, MclslFrameDeadline.RemainingMs);
    }
    internal static double RemainingMs => Math.Min(MclslFrameDeadline.RemainingMs,
        !_active ? double.MaxValue : Math.Max(0d, _limit - ElapsedMs));
    internal static bool Expired => RemainingMs <= 0d || RemainingOperations == 0;
    internal static bool TryConsumeOperation()
    {
        if (Expired) return false;
        if (_active) _operations++;
        return true;
    }
    internal static void ConsumeOperations(int count) { if (_active) _operations += Math.Max(0, count); }
    internal static void End()
    {
        LastElapsedMs = ElapsedMs;
        LastTimeExhausted = RemainingMs <= 0d;
        LastOperationsExhausted = RemainingOperations == 0;
        _active = false;
    }
}


internal enum MclslAnnualAdvance { Progressed, StageCompleted, Waiting, BudgetExhausted, Blocked }

// Fixed-size, runtime-only counters. Formatting happens only when the report refreshes.
internal static class MclslAnnualWorkMetrics
{
    private sealed class Sample
    {
        internal string Name;
        internal long Calls, Completed, Overruns, WorkItems;
        internal double Ms, MaxMs;
    }
    private static readonly Sample[] Samples = new Sample[40];
    private static readonly long[] Exits = new long[5];
    internal static long Starts, StageTransitions, SpareBudgetExits;
    internal static void Record(int slot, string name, double ms, bool completed, double availableMs, int workItems = 1)
    {
        Sample sample = Samples[slot] ??= new Sample { Name = name };
        sample.Calls++; sample.Ms += ms; sample.MaxMs = Math.Max(sample.MaxMs, ms);
        sample.WorkItems += workItems;
        if (completed) sample.Completed++;
        if (ms > availableMs) sample.Overruns++;
    }
    internal static void Exit(MclslAnnualAdvance reason)
    {
        Exits[(int)reason]++;
        if ((reason == MclslAnnualAdvance.Waiting || reason == MclslAnnualAdvance.Blocked)
            && !MclslAnnualFrameBudget.Expired) SpareBudgetExits++;
    }
    internal static string Report()
    {
        var text = new System.Text.StringBuilder();
        text.Append(" annualStarts=").Append(Starts).Append(" stageTransitions=").Append(StageTransitions)
            .Append(" spareBudgetExits=").Append(SpareBudgetExits);
        for (int i = 0; i < Exits.Length; i++)
            text.Append(" exit.").Append((MclslAnnualAdvance)i).Append('=').Append(Exits[i]);
        foreach (Sample sample in Samples)
        {
            if (sample == null || sample.Calls == 0) continue;
            text.AppendLine().Append(sample.Name).Append(" calls=").Append(sample.Calls)
                .Append(" completed=").Append(sample.Completed).Append(" workItems=").Append(sample.WorkItems)
                .Append(" cpuMs=").Append(sample.Ms.ToString("F3"))
                .Append(" maxMs=").Append(sample.MaxMs.ToString("F3")).Append(" overruns=").Append(sample.Overruns);
        }
        return text.ToString();
    }
    internal static void Clear()
    {
        Starts = StageTransitions = SpareBudgetExits = 0;
        Array.Clear(Exits, 0, Exits.Length);
        foreach (Sample sample in Samples)
            if (sample != null) { sample.Calls = sample.Completed = sample.Overruns = sample.WorkItems = 0; sample.Ms = sample.MaxMs = 0; }
    }
}

internal static class MclslInitialCultivationPolicy
{
    internal static bool ShouldApply(int year, int startYear, int lastCultivationYear, bool annualStepActive)
        => year > 0 && startYear == year && lastCultivationYear < year && !annualStepActive;

    internal static bool IsPending(int startYear, int lastCultivationYear)
        => startYear > 0 && lastCultivationYear < startYear;

    internal static int NormalizeCompletedYear(int lastYear, int annualYear, int worldYear)
    {
        int latest = Math.Max(annualYear, worldYear);
        return annualYear <= 0 || lastYear <= latest ? lastYear : Math.Max(-1, latest - 1);
    }

    internal static int InitialCompletedYear(int startYear, int requestedYear)
        => Math.Max(0, startYear > 0 ? startYear - 1 : requestedYear - 1);
}

// Two linked lists keep retry order separate from arrival order. All operations,
// including the oldest outstanding wait, are O(1); no population scan or lazy tombstones.
internal sealed class MclslFirstCultivationQueue
{
    private sealed class Entry
    {
        internal double Arrived, RetryAt;
        internal LinkedListNode<long> Arrival, Work;
    }
    private readonly Dictionary<long, Entry> _entries = new();
    private readonly LinkedList<long> _arrival = new(), _work = new();
    internal int Count => _entries.Count;
    internal double LongestWait(double now) => _arrival.First == null ? 0d
        : Math.Max(0d, now - _entries[_arrival.First.Value].Arrived);
    internal void Add(long id, double now)
    {
        if (id <= 0 || _entries.ContainsKey(id)) return;
        _entries.Add(id, new Entry { Arrived = now, RetryAt = now,
            Arrival = _arrival.AddLast(id), Work = _work.AddFirst(id) });
    }
    internal bool TryTake(double now, out long id)
    {
        id = 0;
        if (_work.First == null) return false;
        // Fresh arrivals go ahead of delayed retries; retries rotate without scanning.
        id = _work.First.Value;
        Entry entry = _entries[id];
        _work.RemoveFirst(); _work.AddLast(entry.Work);
        if (entry.RetryAt > now) { id = 0; return false; }
        entry.RetryAt = now + 1d;
        return true;
    }
    internal double Remove(long id, double now)
    {
        if (!_entries.TryGetValue(id, out Entry entry)) return -1d;
        _arrival.Remove(entry.Arrival); _work.Remove(entry.Work); _entries.Remove(id);
        return Math.Max(0d, now - entry.Arrived);
    }
    internal void Clear() { _entries.Clear(); _arrival.Clear(); _work.Clear(); }
}
