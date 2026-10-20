using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MySimulatedLongevityRoad.Systems;

// Render workers only request immutable keys. Loading and releasing are owner-thread work.
internal sealed class MclslActorFrameCache<T> where T : class
{
    private sealed class Entry
    {
        internal T Value;
        internal long Bytes;
        internal double LastRequest;
    }

    private readonly object _gate = new();
    private readonly int _owner = Thread.CurrentThread.ManagedThreadId;
    private readonly long _budget;
    private readonly double _grace;
    private readonly Dictionary<string, long> _sizes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Entry> _resident = new(StringComparer.Ordinal);
    private readonly HashSet<string> _pending = new(StringComparer.Ordinal);
    private readonly HashSet<string> _failed = new(StringComparer.Ordinal);
    private readonly Queue<string> _requests = new();
    private long _bytes;
    private double _now;

    internal MclslActorFrameCache(long budgetBytes, double evictionGraceSeconds = 10)
    {
        if (budgetBytes <= 0 || double.IsNaN(evictionGraceSeconds)
            || double.IsInfinity(evictionGraceSeconds) || evictionGraceSeconds < 0)
            throw new ArgumentException("Invalid native-frame cache limits");
        _budget = budgetBytes;
        _grace = evictionGraceSeconds;
    }

    internal long ResidentBytes { get { lock (_gate) return _bytes; } }
    internal int PendingCount { get { lock (_gate) return _pending.Count; } }
    internal void Register(string key, long bytes)
    {
        OwnerOnly();
        if (string.IsNullOrEmpty(key) || bytes <= 0 || bytes > _budget)
            throw new ArgumentException("Frame exceeds cache budget or has invalid size");
        lock (_gate)
        {
            if (_sizes.TryGetValue(key, out long previous) && previous != bytes)
                throw new ArgumentException("Frame metadata changed after registration");
            _sizes[key] = bytes;
        }
    }

    internal T Request(string key)
    {
        lock (_gate)
        {
            if (_resident.TryGetValue(key, out Entry entry))
            {
                entry.LastRequest = _now;
                return entry.Value;
            }
            if (_sizes.ContainsKey(key) && !_failed.Contains(key) && _pending.Add(key))
                _requests.Enqueue(key);
            return null;
        }
    }

    internal int Pump(double unscaledNow, int maxLoads, double maxMilliseconds,
        Func<string, T> load, Action<T> release)
    {
        OwnerOnly();
        if (double.IsNaN(unscaledNow) || double.IsInfinity(unscaledNow) || unscaledNow < _now
            || maxLoads <= 0 || double.IsNaN(maxMilliseconds) || double.IsInfinity(maxMilliseconds)
            || maxMilliseconds <= 0) throw new ArgumentException("Invalid cache pump limits/time");
        lock (_gate)
        {
            _now = unscaledNow;
            if (_requests.Count == 0) return 0;
        }
        long started = Stopwatch.GetTimestamp();
        int count = 0;
        while (count < maxLoads && (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency < maxMilliseconds)
        {
            string key;
            long size;
            List<T> evicted = null;
            bool blocked;
            lock (_gate)
            {
                if (_requests.Count == 0) break;
                key = _requests.Peek();
                size = _sizes[key];
                while (_bytes + size > _budget)
                {
                    string oldest = null;
                    double oldestTime = double.PositiveInfinity;
                    foreach (var pair in _resident)
                        if (_now - pair.Value.LastRequest >= _grace && pair.Value.LastRequest < oldestTime)
                        { oldest = pair.Key; oldestTime = pair.Value.LastRequest; }
                    if (oldest == null) break;
                    Entry old = _resident[oldest];
                    _resident.Remove(oldest);
                    _bytes -= old.Bytes;
                    (evicted ??= new List<T>()).Add(old.Value);
                }
                blocked = _bytes + size > _budget;
                // Nothing cold can be released safely. Keep the request queued.
                // Do not decode a texture and then discover it exceeds the budget.
                if (!blocked) _requests.Dequeue();
            }
            if (evicted != null)
                foreach (T value in evicted) release(value);
            if (blocked) return count;
            T loaded = null;
            try { loaded = load(key); }
            finally
            {
                lock (_gate)
                {
                    _pending.Remove(key);
                    if (loaded == null) _failed.Add(key);
                    else
                    {
                        _resident.Add(key, new Entry { Value = loaded, Bytes = size, LastRequest = _now });
                        _bytes += size;
                    }
                }
            }
            count++;
        }
        return count;
    }

    internal void Clear(Action<T> release)
    {
        OwnerOnly();
        var values = new List<T>();
        lock (_gate)
        {
            foreach (Entry entry in _resident.Values) values.Add(entry.Value);
            _resident.Clear();
            _pending.Clear();
            _requests.Clear();
            _failed.Clear();
            _bytes = 0;
        }
        foreach (T value in values) release(value);
    }

    private void OwnerOnly()
    {
        if (Thread.CurrentThread.ManagedThreadId != _owner)
            throw new InvalidOperationException("Native textures must load/release on the registration thread");
    }
}
