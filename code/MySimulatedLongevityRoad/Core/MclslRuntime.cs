using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems.Visual;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslRuntime
{
    private static bool _initialized;
    private static int _lastFrame = -1;
    private static int _frameCounter;
    private static int _lastYear = -1;

    internal static void Init()
    {
        if (_initialized) return;
        _initialized = true;
        MclslConfigLocalization.Init();
        MclslModuleHub.Init();
        MclslRuntimeDriver.Ensure();
    }

    internal static void OnWorldLoaded()
    {
        Init();
        MclslRuntimeDriver.Ensure();
        int year = CurrentYear();
        MclslModuleHub.OnWorldLoaded(year, MclslRuntimeSettings.CoreEnabled);
        _lastYear = year;
        _frameCounter = 0;
    }

    internal static void Tick()
    {
        int unityFrame = Time.frameCount;
        if (unityFrame == _lastFrame) return;
        _lastFrame = unityFrame;

        MclslModuleHub.TickRealtime(MclslRuntimeSettings.CoreEnabled);
        MclslRuntimeWorkBudget.SampleFrame();
        if (!MclslRuntimeSettings.CoreEnabled) return;
        _frameCounter++;
        MclslModuleHub.TickFrame(_frameCounter, true);
        if (_frameCounter % 15 == 0)
        {
            int year = CurrentYear();
            if (year != _lastYear)
            {
                _lastYear = year;
                MclslModuleHub.TickAnnual(year, true);
            }
        }
    }

    internal static void PrepareForSave()
    {
        MclslModuleHub.PrepareForSave();
    }

    internal static void ClearWorldState()
    {
        _lastFrame = -1;
        _frameCounter = 0;
        _lastYear = -1;
        MclslRuntimeWorkBudget.Clear();
        MySimulatedLongevityRoad.Data.MclslHonorificNameCatalog.ClearRuntime();
        MclslModuleHub.Clear();
        MclslVisibleActorRenderLane.Clear();
        MclslRealmHaloVisualSystem.Clear();
    }

    internal static int CurrentYear()
    {
        try { return System.Math.Max(0, World.world?.map_stats?.year ?? 0); } catch { return 0; }
    }
}
