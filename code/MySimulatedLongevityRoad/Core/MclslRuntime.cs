using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Visual;
using MySimulatedLongevityRoad.UI;
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
        MclslRuntimeChanges.BindMainThread();
        MclslConfigLocalization.Init();
        MclslModuleHub.Init();
        MclslFpsOverlay.Ensure();
        MclslRuntimeDriver.Ensure();
    }

    internal static void OnWorldLoaded()
    {
        Init();
        MclslRuntimeDriver.Ensure();
        int year = CurrentYear();
        MclslRuntimeChanges.Clear();
        MclslModuleHub.OnWorldLoaded(year, MclslRuntimeSettings.CoreEnabled);
        MclslMaobaoArchiveManager.OnWorldLoaded();
        MclslAnnualBackpressure.InitializeAfterLoad();
        _lastYear = year;
        _frameCounter = 0;
    }

    internal static void Tick()
    {
        MclslRuntimeChanges.BindMainThread();
        MclslDeveloperBridge.Tick();
        int unityFrame = Time.frameCount;
        if (unityFrame == _lastFrame) return;
        _lastFrame = unityFrame;
        long modFrameSample = MclslPerformanceProbe.Begin();
        MclslFrameDeadline.Begin(3d);
        try
        {
            long realtimeSample = MclslPerformanceProbe.Begin();
            using (MclslUnityProfiler.Sample("MCLS/Runtime/RealtimeModules"))
                MclslModuleHub.TickRealtime(MclslRuntimeSettings.CoreEnabled);
            MclslPerformanceProbe.End("实时模块", realtimeSample);
            MclslRuntimeWorkBudget.SampleFrame();
            MclslAnnualBackpressure.TickStatus();
            MclslRuntimeChanges.Drain(32);
            MclslTraitEditorEraFilter.RefreshVisibleEditors();
            MclslItemEffectDriver.Tick();
            if (!MclslRuntimeSettings.CoreEnabled) return;
            _frameCounter++;
            int year = CurrentYear();
            if (year > _lastYear && year > 0)
            {
                _lastYear = year;
                using (MclslUnityProfiler.Sample("MCLS/Runtime/AnnualDispatch"))
                    MclslModuleHub.TickAnnual(year, true);
            }
            long frameSample = MclslPerformanceProbe.Begin();
            using (MclslUnityProfiler.Sample("MCLS/Runtime/FrameModules"))
                MclslModuleHub.TickFrame(_frameCounter, true);
            MclslPerformanceProbe.End("帧模块", frameSample);
            MclslRuntimeChanges.Drain(32);
            MclslBagSystem.FlushPending();

        }
        finally
        {
            MclslFrameDeadline.End();
            MclslPerformanceProbe.End("模组每帧CPU", modFrameSample);
            MclslPerformanceProbe.SampleFrame();
        }
    }

    internal static void PrepareForSave()
    {
        MclslBagSystem.FlushForSave();
        MclslModuleHub.PrepareForSave();
    }

    internal static void ClearWorldState()
    {
        MclslDiagnostics.Clear();
        MclslPerformanceProbe.SetEnabled(MclslPerformanceProbe.Enabled);
        MclslFpsOverlay.ClearReport();
        MclslAnnualBackpressure.Clear();
        _lastFrame = -1;
        _frameCounter = 0;
        _lastYear = -1;
        MclslRuntimeWorkBudget.Clear();
        MclslRuntimeChanges.Clear();
        MclslBagSystem.ClearRuntime();
        MclslActorInfoPanel.ClearRuntime();
        MclslManaBar.ClearRuntime();
        MclslTextValue.ClearRuntime();
        MclslTraitEditorEraFilter.ClearRuntime();
        MclslFeatureWindow.ClearRuntime();
        MclslCodexWindow.ClearRuntime();
        MclslTianxuanMarket.ClearRuntime();
        MclslSpellSystem.ClearRuntime();
        MclslItemEffectDriver.ClearRuntime();
        MclslRecipeKnowledge.ClearRuntime();
        MclslSpiritualRootSystem.ClearRuntime();
        MySimulatedLongevityRoad.Traits.MclslImmortalActorRegistration.ClearRuntime();
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
