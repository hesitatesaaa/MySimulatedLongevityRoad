using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal enum MclslRuntimeStressTier : byte
{
    Normal = 0,
    Mild = 1,
    Severe = 2,
    Critical = 3
}

internal static class MclslRuntimeWorkBudget
{
    private const float SmoothFactor = 0.08f;
    private const float MildFrameSeconds = 1f / 30f;
    private const float SevereFrameSeconds = 1f / 22f;
    private const float CriticalFrameSeconds = 1f / 14f;
    private const float StressHoldSeconds = 4f;

    private static float _smoothedDelta = 1f / 30f;
    private static float _stressHoldUntil = -1f;
    private static int _lastSampledUnityFrame = -1;
    private static int _lastFastSchedulerUnityFrame = -1;
    private static int _backgroundStrideCounter;
    private static int _annualPriorityStrideCounter;
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

    internal static int ScaleCount(int baseBudget, int minimum)
    {
        if (baseBudget <= 0) return 0;
        float multiplier = _stressTier switch
        {
            MclslRuntimeStressTier.Mild => 0.50f,
            MclslRuntimeStressTier.Severe => 0.28f,
            MclslRuntimeStressTier.Critical => 0.12f,
            _ => 1f
        };
        return Mathf.Max(Mathf.Max(1, minimum), Mathf.RoundToInt(baseBudget * multiplier));
    }

    internal static double ScaleMilliseconds(double baseBudget, double minimum)
    {
        if (baseBudget <= 0d) return 0d;
        double multiplier = _stressTier switch
        {
            MclslRuntimeStressTier.Mild => 0.55d,
            MclslRuntimeStressTier.Severe => 0.32d,
            MclslRuntimeStressTier.Critical => 0.16d,
            _ => 1d
        };
        return System.Math.Max(minimum, baseBudget * multiplier);
    }

    internal static bool TryBeginFastSchedulerPass()
    {
        int unityFrame = Time.frameCount;
        if (_lastFastSchedulerUnityFrame == unityFrame) return false;
        _lastFastSchedulerUnityFrame = unityFrame;
        return true;
    }

    internal static bool ShouldRunBackgroundPhase()
    {
        int stride = _stressTier switch
        {
            MclslRuntimeStressTier.Mild => 3,
            MclslRuntimeStressTier.Severe => 5,
            MclslRuntimeStressTier.Critical => 8,
            _ => 1
        };
        _backgroundStrideCounter++;
        if (_backgroundStrideCounter < stride) return false;
        _backgroundStrideCounter = 0;
        return true;
    }

    internal static bool ShouldRunAnnualActorPriorityPass()
    {
        int stride = _stressTier switch
        {
            MclslRuntimeStressTier.Mild => 2,
            MclslRuntimeStressTier.Severe => 3,
            MclslRuntimeStressTier.Critical => 4,
            _ => 1
        };
        _annualPriorityStrideCounter++;
        if (_annualPriorityStrideCounter < stride) return false;
        _annualPriorityStrideCounter = 0;
        return true;
    }

    internal static void Clear()
    {
        _smoothedDelta = 1f / 30f;
        _stressHoldUntil = -1f;
        _lastSampledUnityFrame = -1;
        _lastFastSchedulerUnityFrame = -1;
        _backgroundStrideCounter = 0;
        _annualPriorityStrideCounter = 0;
        _stressTier = MclslRuntimeStressTier.Normal;
    }
}
