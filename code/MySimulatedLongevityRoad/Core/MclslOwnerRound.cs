using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Core;

/// <summary>A reused snapshot, retained until every slot has had its turn.</summary>
internal sealed class MclslOwnerRound
{
    private readonly List<long> _owners = new();
    private int _cursor;
    private double _nextStart;
    internal bool InProgress => _cursor < _owners.Count;
    internal bool TryBegin(HashSet<long> active, double now)
    {
        if (InProgress) return true;
        if (now < _nextStart) return false;
        Begin(active);
        return true;
    }
    internal void Complete(double now, double interval) => _nextStart = now + interval;
    internal void Begin(HashSet<long> active)
    {
        _owners.Clear();
        _owners.AddRange(active);
        _cursor = 0;
    }
    // The caller checks the shared deadline BEFORE taking a slot. Removed IDs
    // are cheap to skip; newly awakened IDs enter the next snapshot.
    internal bool TryTake(HashSet<long> active, out long id)
    {
        if (!InProgress) { id = 0; return false; }
        id = _owners[_cursor++];
        return active.Contains(id);
    }
    internal void Clear() { _owners.Clear(); _cursor = 0; _nextStart = 0; }
}
