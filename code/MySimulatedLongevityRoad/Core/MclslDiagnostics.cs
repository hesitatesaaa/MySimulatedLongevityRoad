using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslDiagnostics
{
    internal static bool Enabled => MclslRuntimeSettings.DiagnosticsEnabled;
    private const int MaxRememberedKeys = 512;
    // Concurrent containers: empty-catch instrumentation and native sprite
    // callbacks can reach this class from worker threads, so a plain
    // Dictionary would race with the main-thread writers.
    private static readonly ConcurrentDictionary<string, byte> OnceKeys = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, int> LastFrameByKey = new(StringComparer.Ordinal);

    internal static int CacheCount => OnceKeys.Count + LastFrameByKey.Count;
    internal static void Clear() { OnceKeys.Clear(); LastFrameByKey.Clear(); }

    /// <summary>
    /// Throttle clock. <c>Time.frameCount</c> is a native query and is only
    /// safe on the main thread; worker callbacks fall back to a monotonic
    /// tick so repeats still collapse without touching the frame counter.
    /// The fallback is only used for throttling, never for correctness.
    /// </summary>
    private static int Clock
    {
        get
        {
            try
            {
                if (MclslRuntimeChanges.IsMainThread) return Time.frameCount;
            }
            catch { }
            return Environment.TickCount;
        }
    }

    private static void RememberFrame(string key, int clock)
    {
        if (LastFrameByKey.Count >= MaxRememberedKeys && !LastFrameByKey.ContainsKey(key))
            LastFrameByKey.Clear();
        LastFrameByKey[key] = clock;
    }

    internal static void Once(string key, string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key)) return;
        if (OnceKeys.Count >= MaxRememberedKeys && !OnceKeys.ContainsKey(key)) OnceKeys.Clear();
        if (!OnceKeys.TryAdd(key, 0)) return;
        Debug.Log("[模拟长生路][诊断] " + message);
    }

    internal static void Throttle(string key, int frameCount, int intervalFrames, string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key)) return;
        intervalFrames = intervalFrames <= 0 ? 60 : intervalFrames;
        if (LastFrameByKey.TryGetValue(key, out int last) && frameCount - last < intervalFrames) return;
        RememberFrame(key, frameCount);
        Debug.Log("[模拟长生路][诊断] " + message);
    }

    internal static void Error(string key, string message)
    {
        if (string.IsNullOrWhiteSpace(key)) key = message ?? "unknown";
        int frame = Clock;
        if (LastFrameByKey.TryGetValue("error:" + key, out int last) && frame - last < 3600) return;
        RememberFrame("error:" + key, frame);
        Debug.LogWarning("[模拟长生路][诊断][" + key + "] " + message);
    }

    // 修炼链逐角色日志属于编译期诊断能力。发布包未定义该符号时，
    // 编译器会连同参数构造一起移除调用，避免年度流水线产生海量字符串与日志。
    [System.Diagnostics.Conditional("MCLSL_CULTIVATION_DIAGNOSTICS")]
    internal static void Cultivation(string key, string message)
    {
        if (!Enabled) return;
        if (string.IsNullOrWhiteSpace(key)) key = "cultivation";
        Debug.Log("[模拟长生路][修炼链][" + key + "] " + message);
    }

    [System.Diagnostics.Conditional("MCLSL_CULTIVATION_DIAGNOSTICS")]
    internal static void CultivationThrottle(string key, int frameCount, int intervalFrames, string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key)) return;
        intervalFrames = intervalFrames <= 0 ? 60 : intervalFrames;
        if (LastFrameByKey.TryGetValue("cultivation:" + key, out int last) && frameCount - last < intervalFrames) return;
        RememberFrame("cultivation:" + key, frameCount);
        Debug.Log("[模拟长生路][修炼链][" + key + "] " + message);
    }
}
