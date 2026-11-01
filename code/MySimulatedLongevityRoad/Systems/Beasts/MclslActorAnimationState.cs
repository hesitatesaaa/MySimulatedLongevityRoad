using System;

namespace MySimulatedLongevityRoad.Systems;

internal enum MclslActorAction : byte { Idle, Walk, Attack, Hit, Death }

internal sealed class MclslActorAnimationState
{
    private double _attackStart = double.NegativeInfinity;
    private double _hitStart = double.NegativeInfinity;
    private double _hitEnd = double.NegativeInfinity;
    private double _lastNow = double.NegativeInfinity;
    private double _stateStart;
    private MclslActorAction _mode;
    private bool _known;
    internal void Attack(double now)
    {
        Observe(now);
        // Hit owns the visual state until it finishes; do not queue lower-priority attacks.
        if (now < _hitEnd) return;
        _attackStart = now;
    }
    internal void Hit(double now, double duration)
    {
        Observe(now);
        _hitStart = now;
        _hitEnd = now + Math.Max(0, duration);
        _attackStart = double.NegativeInfinity;
    }
    internal MclslActorAction Select(double now, bool moving, double attackDuration,
        double hitDuration, out double elapsed)
    {
        Observe(now);
        MclslActorAction selected;
        double start;
        if (now - _hitStart < hitDuration) { selected = MclslActorAction.Hit; start = _hitStart; }
        else if (now - _attackStart < attackDuration) { selected = MclslActorAction.Attack; start = _attackStart; }
        else { selected = moving ? MclslActorAction.Walk : MclslActorAction.Idle; start = now; }
        if (!_known || _mode != selected) { _known = true; _mode = selected; _stateStart = start; }
        elapsed = Math.Max(0, now - (selected == MclslActorAction.Hit || selected == MclslActorAction.Attack ? start : _stateStart));
        return selected;
    }
    private void Observe(double now)
    {
        if (now < _lastNow) { _known = false; _attackStart = _hitStart = _hitEnd = double.NegativeInfinity; }
        _lastNow = now;
    }
}
