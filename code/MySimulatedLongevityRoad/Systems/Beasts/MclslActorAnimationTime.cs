using System;
using System.Threading;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslActorAnimationTime
{
    private static double _now;
    // Use the same simulation clock as mana recovery. It already includes native
    // speed, pause and annual backpressure; multiplying by timeScale again is wrong.
    // Native sprite calculation runs inside Parallel.For. Those workers must
    // never read Unity's Time API or query mutable world state.
    internal static double Now => Volatile.Read(ref _now);

    internal static void Sample()
    {
        if (!MclslRuntimeChanges.IsMainThread) return;
        double sampled = World.world == null ? 0d : Convert.ToDouble(World.world.getCurWorldTime());
        if (!double.IsNaN(sampled) && !double.IsInfinity(sampled)) Volatile.Write(ref _now, sampled);
    }

    internal static void Clear() => Volatile.Write(ref _now, 0d);
}
