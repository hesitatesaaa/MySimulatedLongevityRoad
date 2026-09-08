using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslHotPathPolicy
{
    internal const int TraitEditorGlobalRefreshFrames = 30;
    internal const int ActorInfoRefreshFrames = 180;
    internal const int LocalizationRetryFrames = 600;

    private static readonly Dictionary<string, int> LastFrameByKey = new(StringComparer.Ordinal);

    internal static bool CanRunFrameGate(string key, int intervalFrames)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        intervalFrames = Math.Max(1, intervalFrames);
        int frame = Time.frameCount;
        if (LastFrameByKey.TryGetValue(key, out int lastFrame) && frame - lastFrame < intervalFrames)
        {
            return false;
        }

        LastFrameByKey[key] = frame;
        return true;
    }

    internal static bool IsActorHotPathSafe(Actor actor)
    {
        return actor?.data != null && MclslActorAccessor.Alive(actor);
    }

    internal static bool ShouldRunBackgroundWorldStep()
    {
        return MclslRuntimeWorkBudget.ShouldRunBackgroundPhase();
    }

    internal static bool ShouldExposeDeveloperTools()
    {
        return MclslRuntimeSettings.DebugToolsVisible;
    }

    internal static void Clear()
    {
        LastFrameByKey.Clear();
    }
}
