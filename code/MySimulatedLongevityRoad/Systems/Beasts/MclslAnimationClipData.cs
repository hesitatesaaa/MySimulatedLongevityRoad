using System;

namespace MySimulatedLongevityRoad.Systems;

// Pure timing data: independent of Unity's render frame rate and safe to test offline.
internal sealed class MclslAnimationClipData
{
    internal readonly string[] Names;
    internal readonly double[] FrameEnds;
    internal readonly bool Loop;
    internal readonly double Duration;

    internal MclslAnimationClipData(string[] names, double[] durationsSeconds, bool loop)
    {
        if (names == null || names.Length == 0 || durationsSeconds == null
            || names.Length != durationsSeconds.Length)
            throw new ArgumentException("Animation requires matching, nonempty frames and durations");
        Names = (string[])names.Clone();
        FrameEnds = new double[names.Length];
        Loop = loop;
        double end = 0;
        for (int i = 0; i < names.Length; i++)
        {
            double duration = durationsSeconds[i];
            if (string.IsNullOrWhiteSpace(names[i]) || names[i].IndexOfAny(new[] { '/', '\\', '.' }) >= 0
                || !(duration > 0) || double.IsNaN(duration) || double.IsInfinity(duration))
                throw new ArgumentException("Invalid animation frame: " + names[i]);
            end += duration;
            if (double.IsInfinity(end)) throw new ArgumentException("Animation duration overflow");
            FrameEnds[i] = end;
        }
        Duration = end;
    }

    internal int FrameIndex(double elapsed)
    {
        if (double.IsNaN(elapsed) || elapsed <= 0) return 0;
        if (double.IsInfinity(elapsed)) return Loop ? 0 : Names.Length - 1;
        if (Loop) elapsed %= Duration;
        else if (elapsed >= Duration) return Names.Length - 1;
        int low = 0, high = FrameEnds.Length - 1;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            if (elapsed < FrameEnds[middle]) high = middle;
            else low = middle + 1;
        }
        return low;
    }
}
