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
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Load/Runtime");
        if (_initialized) return;
        MclslRuntimeChanges.BindMainThread();
        MclslConfigLocalization.Init();
        MclslModuleHub.Init();
        MclslFpsOverlay.Ensure();
        MclslRuntimeDriver.Ensure();
        _initialized = true;
    }

    internal static void OnWorldLoaded()
    {
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Load/WorldEntry");
        Init();
        MclslBeastAnimation.Clear();
        MclslRuntimeDriver.Ensure();
        int year = CurrentYear();
        MclslRuntimeChanges.Clear();
        Systems.Combat.MclslCombatCareerSystem.Clear();
        Patches.MclslXuanhuangDirectionPatch.Clear();
        Systems.Combat.MclslCareerCombat.Load();
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
        MclslActorAnimationTime.Sample();
        long modFrameSample = MclslPerformanceProbe.Begin();
        MclslFrameDeadline.Begin(3d);
        try
        {
            MclslDeferredUiWork.Tick();
            if (MclslWorldArchiveStore.WriteLocked) return;
            MclslRuntimeWorkBudget.SampleFrame();
            MclslAnnualBackpressure.TickStatus();
            MclslRuntimeChanges.Drain(16);
            // Native frame streaming is the only main-thread PNG decode in the
            // loop. It now runs inside the probe scope and backs off with the
            // same stress tiers as every other budgeted lane instead of always
            // spending up to 4 ms outside the frame deadline.
            MclslNativeActorFrames.Tick(
                MclslRuntimeWorkBudget.ScaleMilliseconds(2.5d, 0.5d),
                MclslRuntimeWorkBudget.ScaleCount(6, 1));
            if (MclslRuntimeSettings.CoreEnabled)
            {
                _frameCounter++;
                int year = CurrentYear();
                if (year > _lastYear && year > 0)
                {
                    _lastYear = year;
                    using (MclslUnityProfiler.Sample("MCLS/Runtime/AnnualDispatch"))
                        MclslModuleHub.TickAnnual(year, true);
                }
                MclslAnnualFrameBudget.Begin(MclslAnnualFrameBudget.ForLag(MclslAnnualBackpressure.LagYears));
                try
                {
                    using (MclslUnityProfiler.Sample("MCLS/Runtime/PriorityAnnualWork"))
                        MclslModuleHub.TickPriorityAnnualWork(_frameCounter, true);
                }
                finally { MclslAnnualFrameBudget.End(); }
            }
            long realtimeSample = MclslPerformanceProbe.Begin();
            using (MclslUnityProfiler.Sample("MCLS/Runtime/RealtimeModules"))
                MclslModuleHub.TickRealtime(MclslRuntimeSettings.CoreEnabled);
            MclslPerformanceProbe.End("实时模块", realtimeSample);
            MclslTraitEditorEraFilter.RefreshVisibleEditors();
            MclslItemEffectDriver.Tick();
            if (!MclslRuntimeSettings.CoreEnabled) return;
            Systems.Combat.MclslCareerCombat.Tick();
            long frameSample = MclslPerformanceProbe.Begin();
            using (MclslUnityProfiler.Sample("MCLS/Runtime/FrameModules"))
                MclslModuleHub.TickFrame(_frameCounter, true);
            MclslPerformanceProbe.End("帧模块", frameSample);
            MclslRuntimeChanges.Drain(32);
            MclslBagSystem.FlushPending();
            MclslFamilySystem.TickReconciliation();

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
        if (MclslWorldArchiveStore.WriteLocked) return;
        MclslBagSystem.FlushForSave();
        Systems.Combat.MclslCareerCombat.Save();
        MclslModuleHub.PrepareForSave();
    }

    internal static void ClearWorldState()
    {
        MclslBeastAnimation.Clear();
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
        Systems.Combat.MclslCombatCareerSystem.Clear();
        Patches.MclslXuanhuangDirectionPatch.Clear();
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
