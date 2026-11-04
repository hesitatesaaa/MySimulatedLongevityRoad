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
    private static readonly ConcurrentDictionary<string, int> LastTickByKey = new(StringComparer.Ordinal);

    internal static int CacheCount => OnceKeys.Count + LastFrameByKey.Count + LastTickByKey.Count;
    internal static void Clear() { OnceKeys.Clear(); LastFrameByKey.Clear(); LastTickByKey.Clear(); }

    /// <summary>
    /// Diagnostic-level error: gated by <see cref="Enabled"/> and throttled to at
    /// most one line per key per 60 seconds. Uses Environment.TickCount (ms) so the
    /// window has the same meaning on the main thread and on worker callbacks.
    /// </summary>
    internal static void Error(string key, string message)
    {
        if (!Enabled) return;
        if (LogTick("error:" + (string.IsNullOrWhiteSpace(key) ? (message ?? "unknown") : key), message, Debug.LogWarning))
            return;
    }

    /// <summary>
    /// Critical error: always emitted (not gated by the diagnostics switch), throttled
    /// per key. Message is written verbatim. Reserved for存档损坏、外部状态写入失败等必须可见的问题.
    /// </summary>
    internal static void Fatal(string key, string message)
        => LogTick("fatal:" + (string.IsNullOrWhiteSpace(key) ? (message ?? "unknown") : key), message, Debug.LogError);

    /// <summary>Informational diagnostic log: gated by the diagnostics switch, message verbatim.</summary>
    internal static void Log(string key, string message)
    {
        if (!Enabled) return;
        Debug.Log(message);
    }

    /// <summary>Diagnostic warning: gated by the diagnostics switch, message verbatim.</summary>
    internal static void Warn(string key, string message)
    {
        if (!Enabled) return;
        Debug.LogWarning(message);
    }

    /// <summary>Important warning that must stay visible regardless of the diagnostics switch, throttled per key.</summary>
    internal static void Alert(string key, string message)
        => LogTick("alert:" + (string.IsNullOrWhiteSpace(key) ? (message ?? "unknown") : key), message, Debug.LogWarning);

    /// <summary>User-facing notification: always shown, not throttled by diagnostics, message verbatim.</summary>
    internal static void Notify(string key, string message)
        => Debug.Log(message);

    /// <summary>
    /// One-shot probe: fully silent while diagnostics are disabled, and emitted at
    /// most once per key while enabled. Intended for expected failures of optional
    /// reflection/member probes (replaces treating them as errors).
    /// </summary>
    internal static void Probe(string key, string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key)) return;
        if (OnceKeys.Count >= MaxRememberedKeys && !OnceKeys.ContainsKey(key)) OnceKeys.Clear();
        if (!OnceKeys.TryAdd(key, 0)) return;
        Debug.Log("[模拟长生路][诊断][probe] " + message);
    }

    /// <summary>
    /// Lazy probe: the message factory runs only when diagnostics are enabled, so
    /// hot-path catch blocks no longer build diagnostic strings on the disabled path.
    /// Same one-shot-per-key semantics as <see cref="Probe"/>.
    /// </summary>
    internal static void ProbeLazy(string key, Func<string> messageFactory)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key) || messageFactory == null) return;
        if (OnceKeys.Count >= MaxRememberedKeys && !OnceKeys.ContainsKey(key)) OnceKeys.Clear();
        if (!OnceKeys.TryAdd(key, 0)) return;
        Debug.Log("[模拟长生路][诊断][probe] " + messageFactory());
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

    private static bool LogTick(string key, string message, Action<string> log)
    {
        int now = Environment.TickCount;
        if (LastTickByKey.TryGetValue(key, out int last) && unchecked(now - last) < 60000) return false;
        if (LastTickByKey.Count >= MaxRememberedKeys && !LastTickByKey.ContainsKey(key)) LastTickByKey.Clear();
        LastTickByKey[key] = now;
        log(message);
        return true;
    }

    private static void RememberFrame(string key, int clock)
    {
        if (LastFrameByKey.Count >= MaxRememberedKeys && !LastFrameByKey.ContainsKey(key))
            LastFrameByKey.Clear();
        LastFrameByKey[key] = clock;
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
