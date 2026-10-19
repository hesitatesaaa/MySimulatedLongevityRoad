using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslJulingMarrowMetrics
{
    internal static void Record(string source, int count)
    {
        if (count <= 0 || MclslWorldArchiveStore.WriteLocked) return;
        var run = MclslWorldRunRepository.Current;
        run.JulingMarrowMetrics ??= new Dictionary<string, long>();
        long previous = run.JulingMarrowMetrics.GetValueOrDefault(source);
        run.JulingMarrowMetrics[source] = previous > long.MaxValue - count ? long.MaxValue : previous + count;
        MclslWorldArchiveStore.MarkDirty();
    }
}
