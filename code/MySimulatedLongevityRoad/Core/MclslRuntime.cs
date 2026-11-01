using System.Threading;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Visual;
using MySimulatedLongevityRoad.UI;
using UnityEngine;

namespace MySimulatedLongevityRoad.Core;

internal static class MclslRuntime
{
    private static bool _initialized;
    private static bool _coreWorldInitialized;
    private static int _lastFrame = -1;
    private static int _frameCounter;
    private static int _lastYear = -1;
    // Fixed-speed presentation clock. It advances by one unscaled real second per
    // rendered second, so animated effects keep their authored pace at any
    // simulation speed. It freezes while the world is paused (the same API the
    // native simulation branches on) and restarts when the world changes.
    private static double _displayClock;
    internal static double DisplayClock => Volatile.Read(ref _displayClock);

    internal static void Init()
    {
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Load/Runtime");
        if (_initialized) return;
        MclslRuntimeChanges.BindMainThread();
        MclslConfigLocalization.Init();
        MclslReincarnationProfileStore.EnsureLoaded();
        MclslHuanzhenSystem.EnsureLoaded();
        MclslFpsOverlay.Ensure();
        MclslRuntimeDriver.Ensure();
        _initialized = true;
    }

    internal static void OnWorldLoaded()
    {
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Load/WorldEntry");
        Init();
        _displayClock = 0d;
        MclslBeastAnimation.Clear();
        MclslRuntimeDriver.Ensure();
        int year = CurrentYear();
        MclslWorldContext.Bind();
        _coreWorldInitialized = false;
        MclslRuntimeChanges.Clear();
        Systems.Combat.MclslCombatCareerSystem.Clear();
        MclslBeastTalentSystem.Clear(); MclslBeastWorld.Clear();
        MclslBeastLoot.Clear(); MclslSpellCasting.Clear();
        Traits.MclslXuanhuangRegistration.EnsureWildKingdom();
        Patches.MclslXuanhuangDirectionPatch.PrepareWorldColors();
        if (MclslRuntimeSettings.CoreEnabled)
        {
            _coreWorldInitialized = true;
            MclslWorldStateModifierSystem.ClearCache();
            MclslWorldArchiveStore.Load();
            if (MclslWorldArchiveStore.WriteLocked) return;
            MclslWorldRunRepository.EnsureCurrentRun(year);
            MclslNativeHistoryBridge.EnsureRegistered();
            Systems.Death.MclslNativeKillStatisticsSystem.OnWorldLoaded(year);
        }
        MclslHuanzhenSystem.OnWorldLoaded();
        if (MclslRuntimeSettings.CoreEnabled)
        {
            LoadCoreModules(year);
        }
        MclslBeastSurfaceIndex.Resume();
        MclslFamilySystem.InitializeAfterLoad();
        MclslMaobaoArchiveManager.OnWorldLoaded();
        MclslAnnualBackpressure.InitializeAfterLoad();
        _lastYear = year;
        _frameCounter = 0;
        _displayClock = 0d;
    }

    private static void LoadCoreModules(int year)
    {
        // Attempt once per world, including faults; configuration callbacks never
        // parse archives or repeatedly retry a bad module on the frame path.
        _coreWorldInitialized = true;
        Systems.Combat.MclslCareerCombat.Load();
        MclslTimelineSystem.TickAnnual(year);
        MclslWorldRunRepository.EnsureAnnualWorldState(year);
        MclslRuntimeCadence.InitializeAfterLoad(year);
        MclslWorldSoulSystem.OnWorldLoaded();
        MclslBeastWorld.Load(); MclslBeastLoot.Load();
        MclslBeastTalentSystem.Load(); MclslSpellCasting.Load();
        MclslMapMarkerVisualSystem.OnWorldLoaded();
        MclslScheduler.ScheduleAnnualWorld(year);
    }

    internal static void Tick()
    {
        MclslRuntimeChanges.BindMainThread();
        MclslDeveloperBridge.Tick();
        int unityFrame = Time.frameCount;
        if (unityFrame == _lastFrame) return;
        _lastFrame = unityFrame;
        AdvanceDisplayClock();
        MclslActorAnimationTime.Sample();
        long modFrameSample = MclslPerformanceProbe.Begin();
        // 方案甲：总帧预算 4ms；暂停档额外 +1ms 追积压。
        MclslFrameDeadline.Begin(MclslAnnualBackpressure.IsPauseLevel ? 5d : 4d);
        try
        {
            MclslDeferredUiWork.Tick();
            if(MclslRuntimeSettings.CoreEnabled && !_coreWorldInitialized && World.world?.map_stats!=null)
            {
                _coreWorldInitialized = true;
                int year = CurrentYear();
                MclslWorldStateModifierSystem.ClearCache();
                MclslWorldArchiveStore.Load();
                if(MclslWorldArchiveStore.WriteLocked)return;
                MclslWorldRunRepository.EnsureCurrentRun(year);
                MclslNativeHistoryBridge.EnsureRegistered();
                Systems.Death.MclslNativeKillStatisticsSystem.OnWorldLoaded(year);
                LoadCoreModules(year);
            }
            if (MclslWorldArchiveStore.WriteLocked) return;
            MclslFrameLoadMonitor.SampleFrame();
            MclslAnnualBackpressure.TickStatus();
            MclslRuntimeChanges.Drain();
            MclslRankSnapshotSource.TickUpdates();
            Patches.MclslXuanhuangDirectionPatch.TickColors();
            if (MclslBeastWorld.HasManualRequest) MclslBeastWorld.Tick();
            if (MclslFamilySystem.HasWork) MclslFamilySystem.TickReconciliation();
            // Native frame streaming is the only main-thread PNG decode in the
            // loop. It now runs inside the probe scope and backs off with the
            // same stress tiers as every other budgeted lane instead of always
            // spending up to 4 ms outside the frame deadline.
            MclslNativeActorFrames.Tick(
                MclslScheduler.HasFastWork ? 0.5d : 2.5d,
                6);
            if (MclslRuntimeSettings.CoreEnabled)
            {
                _frameCounter++;
                int year = CurrentYear();
                if (year > 0)
                {
                    bool yearChanged = year > _lastYear;
                    if (yearChanged) _lastYear = year;
                    // 正常年变化启动下一年；暂停/低速下世界年不变，但只要“年度完成年落后且无活动年”
                    // 也继续启动下一年追帧，从而消化积压后自动恢复。
                    if (yearChanged || MclslScheduler.NeedsAnnualStart)
                        using (MclslUnityProfiler.Sample("MCLS/Runtime/AnnualDispatch"))
                            MclslScheduler.ScheduleAnnualWorld(year);
                }
                MclslAnnualFrameBudget.Begin(MclslAnnualFrameBudget.ForLag(MclslAnnualBackpressure.LagYears));
                try
                {
                    using (MclslUnityProfiler.Sample("MCLS/Runtime/PriorityAnnualWork"))
                        MclslRuntimeCadence.Tick(_frameCounter);
                }
                finally { MclslAnnualFrameBudget.End(); }
            }
            long realtimeSample = MclslPerformanceProbe.Begin();
            using (MclslUnityProfiler.Sample("MCLS/Runtime/RealtimeModules"))
                MclslHuanzhenSystem.Tick();
            MclslPerformanceProbe.End("实时模块", realtimeSample);
            MclslTraitEditorEraFilter.RefreshVisibleEditors();
            MclslItemEffectDriver.Tick();
            if (!MclslRuntimeSettings.CoreEnabled) return;
            Systems.Combat.MclslCareerCombat.Tick();
            long frameSample = MclslPerformanceProbe.Begin();
            using (MclslUnityProfiler.Sample("MCLS/Runtime/FrameModules"))
                {
                    MclslWorldSoulSystem.TickFrame(_frameCounter);
                    MclslBeastWorld.Tick(); MclslBeastLoot.Tick();
                    MclslSpellCasting.Tick(); MclslBeastTalentSystem.Tick();
                    MclslMapMarkerVisualSystem.Tick(Time.frameCount);
                    MclslBeastDeathVisualSystem.Tick();
                    MclslAnnouncementSystem.Tick();
                }
            MclslPerformanceProbe.End("帧模块", frameSample);
            MclslRuntimeChanges.Drain();
            MclslBeastSurfaceIndex.Tick();
            MclslBagSystem.FlushPending();

        }
        finally
        {
            MclslFrameDeadline.End();
            MclslPerformanceProbe.End("模组每帧CPU", modFrameSample);
            MclslPerformanceProbe.SampleFrame();
        }
    }

    private static void AdvanceDisplayClock()
    {
        if (World.world == null || World.world.isPaused()) return;
        Volatile.Write(ref _displayClock, DisplayClock + Time.unscaledDeltaTime);
    }

    internal static void PrepareForSave()
    {
        if (MclslWorldArchiveStore.WriteLocked) return;
        MclslScheduler.SavePendingAnnualStates();
        MclslBagSystem.FlushForSave();
        Systems.Combat.MclslCareerCombat.Save();
        MclslReincarnationProfileStore.Flush();
        MclslWorldArchiveStore.SaveNow();
        MclslBeastWorld.Save(); MclslBeastLoot.Save();
        MclslBeastTalentSystem.Save(); MclslSpellCasting.Save();
    }

    internal static void ClearWorldState()
    {
        _coreWorldInitialized = false;
        MclslWorldContext.Clear();
        MclslBeastAnimation.Clear();
        MclslDiagnostics.Clear();
        MclslPerformanceProbe.SetEnabled(MclslPerformanceProbe.Enabled);
        MclslFpsOverlay.ClearReport();
        MclslAnnualBackpressure.Clear();
        _lastFrame = -1;
        _frameCounter = 0;
        _lastYear = -1;
        _displayClock = 0d;
        MclslFrameLoadMonitor.Clear();
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
        MclslRuntimeCadence.Clear();
        MclslHuanzhenSystem.ClearRuntime();
        MclslWorldSoulSystem.Clear(); MclslInverseTruthSystem.Clear();
        MclslBeastTalentSystem.Clear(); MclslBeastWorld.Clear();
        MclslBeastLoot.Clear(); MclslSpellCasting.Clear();
        MclslMapMarkerVisualSystem.Clear(); MclslAnnouncementSystem.Clear();
        MclslWorldRunRepository.ResetWorld(); MclslWorldArchiveStore.Clear();
        MclslEligibility.Clear(); Systems.Death.MclslDeathSystem.Clear();
        Systems.Death.MclslNativeKillStatisticsSystem.Clear();
        MclslWorldStateModifierSystem.ClearCache();
        MclslVisibleActorRenderLane.Clear();
        MclslRealmHaloVisualSystem.Clear();
    }

    internal static int CurrentYear()
    {
        return System.Math.Max(0, World.world?.map_stats?.year ?? 0);
    }
}
