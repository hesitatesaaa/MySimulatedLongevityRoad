using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslHotPathPolicy
{
    internal const int LocalizationRetryFrames = 600;


    internal static bool IsActorHotPathSafe(Actor actor)
    {
        return actor?.data != null && MclslActorAccessor.Alive(actor);
    }

    internal static bool ShouldExposeDeveloperTools()
    {
        return MclslDeveloperBridge.IsAvailable;
    }

}
