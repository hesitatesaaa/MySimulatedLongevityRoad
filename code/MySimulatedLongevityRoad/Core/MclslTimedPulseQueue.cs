using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Core;

/// <summary>ID-only timed effects. Each accepted dose retains its complete pulse schedule.</summary>
internal sealed class MclslTimedPulseQueue
{
    private readonly struct Pulse
    {
        internal readonly long Ticket, Owner;
        internal readonly double Due, Interval;
        internal readonly float Portion;
        internal readonly int Remaining;
        internal Pulse(long ticket, long owner, double due, double interval, float portion, int remaining)
        { Ticket = ticket; Owner = owner; Due = due; Interval = interval; Portion = portion; Remaining = remaining; }
        internal Pulse Next(double now) => new(Ticket, Owner, now + Interval, Interval, Portion, Remaining - 1);
    }
    private readonly int _capacity, _perOwner;
    private readonly MclslOrderedIdIndex<Pulse> _due = new((a, b) => a.Due.CompareTo(b.Due));
    private readonly Dictionary<long, HashSet<long>> _owners = new();
    private long _nextTicket;
    internal MclslTimedPulseQueue(int capacity = 65536, int perOwner = 64)
    {
        if (capacity < 1 || perOwner < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        _capacity = capacity; _perOwner = perOwner;
    }
    internal int Count => _due.Count;
    internal int OwnerCount => _owners.Count;
    internal bool CanSchedule(long owner) => owner > 0 && Count < _capacity
        && (!_owners.TryGetValue(owner, out var tickets) || tickets.Count < _perOwner);
    internal bool Schedule(long owner, double now, float portion, int times, double interval)
    {
        if (!CanSchedule(owner) || portion <= 0 || float.IsNaN(portion) || float.IsInfinity(portion)
            || times < 1 || interval <= 0 || double.IsNaN(now) || double.IsInfinity(now)
            || double.IsNaN(interval) || double.IsInfinity(interval)) return false;
        long ticket = ++_nextTicket;
        _due.Upsert(ticket, new(ticket, owner, now + interval, interval, portion, times));
        if (!_owners.TryGetValue(owner, out var tickets)) _owners[owner] = tickets = new();
        tickets.Add(ticket);
        return true;
    }
    internal int Tick(double now, int maxPulses, Func<long, float, bool> apply, Func<bool> expired = null)
    {
        int applied = 0;
        while (Count > 0 && applied < maxPulses && !(expired?.Invoke() ?? false))
        {
            Pulse pulse = _due[0];
            if (pulse.Due > now) break;
            // Advance before the callback: an exception cannot replay this pulse.
            if (pulse.Remaining > 1) _due.Upsert(pulse.Ticket, pulse.Next(now));
            else
            {
                _due.Remove(pulse.Ticket);
                if (_owners.TryGetValue(pulse.Owner, out var tickets))
                { tickets.Remove(pulse.Ticket); if (tickets.Count == 0) _owners.Remove(pulse.Owner); }
            }
            applied++;
            if (!apply(pulse.Owner, pulse.Portion)) Cancel(pulse.Owner);
        }
        return applied;
    }
    internal void Cancel(long owner)
    {
        if (!_owners.TryGetValue(owner, out var tickets)) return;
        foreach (long ticket in tickets) _due.Remove(ticket);
        _owners.Remove(owner);
    }
    internal void Clear() { _due.Clear(); _owners.Clear(); _nextTicket = 0; }
}
