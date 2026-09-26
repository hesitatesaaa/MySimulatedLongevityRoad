using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using MySimulatedLongevityRoad.Systems;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

/// <summary>Developer-package-only opt-in timing counters. Disabled probes cost one branch.</summary>
internal static class MclslPerformanceProbe
{
    private sealed class Sample { internal long Ticks; internal long MaximumTicks; internal int Calls; }
    private static readonly Dictionary<string, Sample> Samples = new(StringComparer.Ordinal);
    private static readonly StringBuilder Buffer = new(512);
    private static Func<long> _allocatedBytesReader;
    private static bool _allocationReaderResolved;
    private static int _sampleStartFrame;
    private static int _sampleStartGc0;
    private static int _sampleStartGc1;
    private static int _lastSampledFrame = -1;
    private static long _lastAllocatedBytes = -1L;
    private static long _frameAllocationTotal;
    private static long _frameAllocationPeak;
    private static int _allocationFrameCount;
    private static float _minimumFps;
    private static float _maximumFrameMilliseconds;

    internal static bool Enabled { get; private set; }

    internal static void SetEnabled(bool enabled)
    {
        Enabled = enabled;
        Samples.Clear();
        _sampleStartFrame = Time.frameCount;
        _sampleStartGc0 = GC.CollectionCount(0);
        _sampleStartGc1 = GC.CollectionCount(1);
        _lastSampledFrame = -1;
        _frameAllocationTotal = 0L;
        _frameAllocationPeak = 0L;
        _allocationFrameCount = 0;
        _minimumFps = float.PositiveInfinity;
        _maximumFrameMilliseconds = 0f;
        _lastAllocatedBytes = enabled ? ReadAllocatedBytes() : -1L;
    }

    internal static long Begin() => Enabled ? Stopwatch.GetTimestamp() : 0L;

    internal static void End(string lane, long started)
    {
        if (!Enabled || started == 0L || string.IsNullOrWhiteSpace(lane)) return;
        if (!Samples.TryGetValue(lane, out Sample sample)) Samples[lane] = sample = new Sample();
        long elapsed = Math.Max(0L, Stopwatch.GetTimestamp() - started);
        sample.Ticks += elapsed;
        if (elapsed > sample.MaximumTicks) sample.MaximumTicks = elapsed;
        sample.Calls++;
    }

    internal static void SampleFrame()
    {
        if (!Enabled) return;
        int frame = Time.frameCount;
        if (_lastSampledFrame == frame) return;
        _lastSampledFrame = frame;

        float delta = Time.unscaledDeltaTime;
        if (delta > 0f && delta <= 2f)
        {
            float fps = 1f / delta;
            if (fps < _minimumFps) _minimumFps = fps;
            float milliseconds = delta * 1000f;
            if (milliseconds > _maximumFrameMilliseconds) _maximumFrameMilliseconds = milliseconds;
        }

        long allocated = ReadAllocatedBytes();
        if (_lastAllocatedBytes >= 0L && allocated >= _lastAllocatedBytes)
        {
            long frameAllocation = allocated - _lastAllocatedBytes;
            _frameAllocationTotal += frameAllocation;
            if (frameAllocation > _frameAllocationPeak) _frameAllocationPeak = frameAllocation;
            _allocationFrameCount++;
        }
        _lastAllocatedBytes = allocated;
    }

    internal static string Summary()
    {
        if (!Enabled) return "性能采样未开启。";
        Buffer.Length = 0;
        int frames = Math.Max(0, Time.frameCount - _sampleStartFrame);
        Buffer.Append("采样 ").Append(frames).Append(" 帧；当前 ")
            .Append(Mathf.RoundToInt(1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime))).Append(" FPS；GC0/1 ")
            .Append(GC.CollectionCount(0) - _sampleStartGc0).Append('/').Append(GC.CollectionCount(1) - _sampleStartGc1)
            .Append("；最低 ").Append(float.IsPositiveInfinity(_minimumFps) ? 0f : _minimumFps).Append(" FPS；最长帧 ")
            .Append(_maximumFrameMilliseconds.ToString("F1")).Append(" ms；GC Alloc ");
        if (_allocationFrameCount > 0)
            Buffer.Append((_frameAllocationTotal / _allocationFrameCount).ToString()).Append(" / ").Append(_frameAllocationPeak).Append(" B/帧（均值/峰值）");
        else
            Buffer.Append("不可用");
        Buffer
            .Append("；年度队列 ").Append(MclslScheduler.AnnualActorBacklogCount).Append("。\n");
        foreach (KeyValuePair<string, Sample> pair in Samples)
        {
            double ms = pair.Value.Ticks * 1000d / Stopwatch.Frequency;
            double maximumMs = pair.Value.MaximumTicks * 1000d / Stopwatch.Frequency;
            Buffer.Append(pair.Key).Append("：").Append(ms.ToString("F1"))
                .Append("ms 总/").Append(maximumMs.ToString("F2")).Append("ms 峰/")
                .Append(pair.Value.Calls).Append(" 次；");
        }
        return Buffer.ToString();
    }

    private static long ReadAllocatedBytes()
    {
        if (!_allocationReaderResolved)
        {
            _allocationReaderResolved = true;
            try
            {
                MethodInfo method = typeof(GC).GetMethod(
                    "GetAllocatedBytesForCurrentThread",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);
                if (method != null) _allocatedBytesReader = (Func<long>)method.CreateDelegate(typeof(Func<long>));
            }
            catch { _allocatedBytesReader = null; }
        }

        try { return _allocatedBytesReader?.Invoke() ?? -1L; }
        catch { return -1L; }
    }
}
