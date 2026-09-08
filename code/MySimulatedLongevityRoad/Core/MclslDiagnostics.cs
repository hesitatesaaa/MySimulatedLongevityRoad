using System.Collections.Generic;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslDiagnostics
{
    internal static bool Enabled => MclslRuntimeSettings.DiagnosticsEnabled;
    private static readonly HashSet<string> OnceKeys = new();
    private static readonly Dictionary<string, int> LastFrameByKey = new();

    internal static void Once(string key, string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key)) return;
        if (!OnceKeys.Add(key)) return;
        Debug.Log("[模拟长生路][诊断] " + message);
    }

    internal static void Throttle(string key, int frameCount, int intervalFrames, string message)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(key)) return;
        intervalFrames = intervalFrames <= 0 ? 60 : intervalFrames;
        if (LastFrameByKey.TryGetValue(key, out int last) && frameCount - last < intervalFrames) return;
        LastFrameByKey[key] = frameCount;
        Debug.Log("[模拟长生路][诊断] " + message);
    }

    internal static void Error(string key, string message)
    {
        if (!Enabled) return;
        Debug.LogWarning("[模拟长生路][诊断] " + message);
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
        LastFrameByKey["cultivation:" + key] = frameCount;
        Debug.Log("[模拟长生路][修炼链][" + key + "] " + message);
    }
}
