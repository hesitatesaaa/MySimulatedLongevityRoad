using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal enum MclslRuntimeStressTier : byte
{
    Normal = 0,
    Mild = 1,
    Severe = 2,
    Critical = 3
}

internal static class MclslFrameLoadMonitor
{
    private const float SmoothFactor = 0.08f;
    private const float MildFrameSeconds = 1f / 30f;
    private const float SevereFrameSeconds = 1f / 22f;
    private const float CriticalFrameSeconds = 1f / 14f;
    private const float StressHoldSeconds = 4f;

    private static float _smoothedDelta = 1f / 30f;
    private static float _stressHoldUntil = -1f;
    private static int _lastSampledUnityFrame = -1;
    private static MclslRuntimeStressTier _stressTier;

    internal static MclslRuntimeStressTier StressTier => _stressTier;

    internal static void SampleFrame()
    {
        int unityFrame = Time.frameCount;
        if (_lastSampledUnityFrame == unityFrame) return;
        _lastSampledUnityFrame = unityFrame;

        float delta = Time.unscaledDeltaTime;
        if (delta <= 0f || delta > 2f) return;

        _smoothedDelta = Mathf.Lerp(_smoothedDelta, delta, SmoothFactor);
        MclslRuntimeStressTier tier = MclslRuntimeStressTier.Normal;
        if (_smoothedDelta >= CriticalFrameSeconds) tier = MclslRuntimeStressTier.Critical;
        else if (_smoothedDelta >= SevereFrameSeconds) tier = MclslRuntimeStressTier.Severe;
        else if (_smoothedDelta >= MildFrameSeconds) tier = MclslRuntimeStressTier.Mild;

        if (tier >= MclslRuntimeStressTier.Severe)
            _stressHoldUntil = Time.unscaledTime + StressHoldSeconds;
        else if (Time.unscaledTime < _stressHoldUntil)
            tier = MclslRuntimeStressTier.Severe;

        _stressTier = tier;
    }

    internal static void Clear()
    {
        _smoothedDelta = 1f / 30f;
        _stressHoldUntil = -1f;
        _lastSampledUnityFrame = -1;
        _stressTier = MclslRuntimeStressTier.Normal;
    }
}
