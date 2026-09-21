using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Developer-package-only opt-in timing counters. Disabled probes cost one branch.</summary>
internal static class MclslPerformanceProbe
{
    private sealed class Sample { internal long Ticks; internal int Calls; }
    private static readonly Dictionary<string, Sample> Samples = new(StringComparer.Ordinal);
    private static readonly StringBuilder Buffer = new(512);
    private static int _sampleStartFrame;
    private static int _sampleStartGc0;
    private static int _sampleStartGc1;

    internal static bool Enabled { get; private set; }

    internal static void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        Samples.Clear();
        _sampleStartFrame = Time.frameCount;
        _sampleStartGc0 = GC.CollectionCount(0);
        _sampleStartGc1 = GC.CollectionCount(1);
    }

    internal static long Begin() => Enabled ? Stopwatch.GetTimestamp() : 0L;

    internal static void End(string lane, long started)
    {
        if (!Enabled || started == 0L || string.IsNullOrWhiteSpace(lane)) return;
        if (!Samples.TryGetValue(lane, out Sample sample)) Samples[lane] = sample = new Sample();
        sample.Ticks += Stopwatch.GetTimestamp() - started;
        sample.Calls++;
    }

    internal static string Summary()
    {
        if (!Enabled) return "性能采样未开启。";
        Buffer.Length = 0;
        float seconds = Mathf.Max(0.001f, Time.unscaledTime);
        int frames = Math.Max(0, Time.frameCount - _sampleStartFrame);
        Buffer.Append("采样 ").Append(frames).Append(" 帧；当前 ")
            .Append(Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime))).Append(" FPS；GC0/1 ")
            .Append(GC.CollectionCount(0) - _sampleStartGc0).Append('/').Append(GC.CollectionCount(1) - _sampleStartGc1)
            .Append("；年度队列 ").Append(MclslScheduler.AnnualActorBacklogCount).Append("。\n");
        foreach (KeyValuePair<string, Sample> pair in Samples)
        {
            double ms = pair.Value.Ticks * 1000d / Stopwatch.Frequency;
            Buffer.Append(pair.Key).Append("：").Append(ms.ToString("F1"))
                .Append("ms/").Append(pair.Value.Calls).Append(" 次；");
        }
        return Buffer.ToString();
    }
}
