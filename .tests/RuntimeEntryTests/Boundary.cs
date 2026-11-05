// Unity/module boundaries only. The entry, retry and save gates under test
// are the actual production MclslRuntime and initialization state machine.
using System;
using System.Collections.Generic;
static class Boundary
{
    public static string FailAt;
    public static readonly List<string> Calls = new();
    public static void Hit(string name) { Calls.Add(name); if (FailAt == name) throw new InvalidOperationException("injected: " + name); }
}
class MapStats { public int year = 12; }
class MapBox { public MapStats map_stats = new(); public bool isPaused() => false; }
static class World { public static MapBox world; }
namespace UnityEngine { static class Time { public static int frameCount; public static float unscaledDeltaTime = 0.01f; } }
namespace MySimulatedLongevityRoad.Queries { }
namespace MySimulatedLongevityRoad.UI { }
namespace MySimulatedLongevityRoad.Systems.Visual { }
namespace MySimulatedLongevityRoad.Core
{
    static class MclslRuntimeSettings { public static bool CoreEnabled = true; }
    static class MclslWorldContext
    {
        public static long Generation;
        public static void Bind() { Boundary.Hit("MclslWorldContext.Bind"); Generation++; }
        public static void Clear() { Generation++; }
    }
    static class MclslWorldArchiveStore
    {
        public static bool WriteLocked, CanSave, Waiting;
        public static void LockWrites() { WriteLocked = true; CanSave = false; }
        public static void Load() { Boundary.Hit("MclslWorldArchiveStore.Load"); CanSave = !Waiting && !WriteLocked; }
        public static void SaveNow() => Boundary.Hit("MclslWorldArchiveStore.SaveNow");
        public static void Clear() { CanSave = false; WriteLocked = false; Waiting = false; }
    }
    static class MclslPerformanceProbe
    {
        public static bool Enabled;
        public static long Begin() => 1;
        public static void End(string s, long n) { }
        public static void SetEnabled(bool v) => Enabled = v;
        public static void SampleFrame() { }
    }
    static class MclslUnityProfiler
    {
        public struct Scope : IDisposable { public void Dispose() { } }
        public static Scope Sample(string s) => default;
    }
}
namespace MySimulatedLongevityRoad.Core { static class MclslActorAnimationTime {

public static void Sample(params object[] args) { Boundary.Hit("MclslActorAnimationTime.Sample"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslActorInfoPanel {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslActorInfoPanel.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslAnnouncementSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslAnnouncementSystem.Clear"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslAnnouncementSystem.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslAnnualBackpressure {
public static bool IsPauseLevel = false; public static int LagYears = 0;
public static void Clear(params object[] args) { Boundary.Hit("MclslAnnualBackpressure.Clear"); }
public static void InitializeAfterLoad(params object[] args) { Boundary.Hit("MclslAnnualBackpressure.InitializeAfterLoad"); }
public static void TickStatus(params object[] args) { Boundary.Hit("MclslAnnualBackpressure.TickStatus"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslAnnualFrameBudget {

public static void Begin(params object[] args) { Boundary.Hit("MclslAnnualFrameBudget.Begin"); }
public static void End(params object[] args) { Boundary.Hit("MclslAnnualFrameBudget.End"); }
public static int ForLag(params object[] args) { Boundary.Hit("MclslAnnualFrameBudget.ForLag"); return 1; }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBagSystem {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslBagSystem.ClearRuntime"); }
public static void FlushForSave(params object[] args) { Boundary.Hit("MclslBagSystem.FlushForSave"); }
public static void FlushPending(params object[] args) { Boundary.Hit("MclslBagSystem.FlushPending"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBeastAnimation {

public static void Clear(params object[] args) { Boundary.Hit("MclslBeastAnimation.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBeastDeathVisualSystem {

public static void Tick(params object[] args) { Boundary.Hit("MclslBeastDeathVisualSystem.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBeastLoot {

public static void Clear(params object[] args) { Boundary.Hit("MclslBeastLoot.Clear"); }
public static void Load(params object[] args) { Boundary.Hit("MclslBeastLoot.Load"); }
public static void Save(params object[] args) { Boundary.Hit("MclslBeastLoot.Save"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslBeastLoot.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBeastSurfaceIndex {

public static void Resume(params object[] args) { Boundary.Hit("MclslBeastSurfaceIndex.Resume"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslBeastSurfaceIndex.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBeastTalentSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslBeastTalentSystem.Clear"); }
public static void Load(params object[] args) { Boundary.Hit("MclslBeastTalentSystem.Load"); }
public static void Save(params object[] args) { Boundary.Hit("MclslBeastTalentSystem.Save"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslBeastTalentSystem.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslBeastWorld {
public static bool HasManualRequest = false;
public static void Clear(params object[] args) { Boundary.Hit("MclslBeastWorld.Clear"); }
public static void Load(params object[] args) { Boundary.Hit("MclslBeastWorld.Load"); }
public static void Save(params object[] args) { Boundary.Hit("MclslBeastWorld.Save"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslBeastWorld.Tick"); }
} }
namespace MySimulatedLongevityRoad.Systems.Combat { static class MclslCareerCombat {

public static void Load(params object[] args) { Boundary.Hit("MclslCareerCombat.Load"); }
public static void Save(params object[] args) { Boundary.Hit("MclslCareerCombat.Save"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslCareerCombat.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslCodexWindow {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslCodexWindow.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Systems.Combat { static class MclslCombatCareerSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslCombatCareerSystem.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslConfigLocalization {

public static void Init(params object[] args) { Boundary.Hit("MclslConfigLocalization.Init"); }
} }
namespace MySimulatedLongevityRoad.Systems.Death { static class MclslDeathSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslDeathSystem.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslDeferredUiWork {

public static void Tick(params object[] args) { Boundary.Hit("MclslDeferredUiWork.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslDeveloperBridge {

public static void Tick(params object[] args) { Boundary.Hit("MclslDeveloperBridge.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslDiagnostics {
public static bool Enabled = false;
public static void Clear(params object[] args) { Boundary.Hit("MclslDiagnostics.Clear"); }
public static void Fatal(params object[] args) { Boundary.Hit("MclslDiagnostics.Fatal"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslEligibility {

public static void Clear(params object[] args) { Boundary.Hit("MclslEligibility.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslFamilySystem {
public static bool HasWork = false;
public static void InitializeAfterLoad(params object[] args) { Boundary.Hit("MclslFamilySystem.InitializeAfterLoad"); }
public static void TickReconciliation(params object[] args) { Boundary.Hit("MclslFamilySystem.TickReconciliation"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslFeatureWindow {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslFeatureWindow.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslFpsOverlay {

public static void ClearReport(params object[] args) { Boundary.Hit("MclslFpsOverlay.ClearReport"); }
public static void Ensure(params object[] args) { Boundary.Hit("MclslFpsOverlay.Ensure"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslFrameDeadline {

public static void Begin(params object[] args) { Boundary.Hit("MclslFrameDeadline.Begin"); }
public static void End(params object[] args) { Boundary.Hit("MclslFrameDeadline.End"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslFrameLoadMonitor {

public static void Clear(params object[] args) { Boundary.Hit("MclslFrameLoadMonitor.Clear"); }
public static void SampleFrame(params object[] args) { Boundary.Hit("MclslFrameLoadMonitor.SampleFrame"); }
} }
namespace MySimulatedLongevityRoad.Data { static class MclslHonorificNameCatalog {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslHonorificNameCatalog.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslHuanzhenSystem {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslHuanzhenSystem.ClearRuntime"); }
public static void EnsureLoaded(params object[] args) { Boundary.Hit("MclslHuanzhenSystem.EnsureLoaded"); }
public static void OnWorldLoaded(params object[] args) { Boundary.Hit("MclslHuanzhenSystem.OnWorldLoaded"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslHuanzhenSystem.Tick"); }
} }
namespace MySimulatedLongevityRoad.Traits { static class MclslImmortalActorRegistration {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslImmortalActorRegistration.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslInverseTruthSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslInverseTruthSystem.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslItemEffectDriver {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslItemEffectDriver.ClearRuntime"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslItemEffectDriver.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslManaBar {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslManaBar.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslMaobaoArchiveManager {

public static void OnWorldLoaded(params object[] args) { Boundary.Hit("MclslMaobaoArchiveManager.OnWorldLoaded"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslMapMarkerVisualSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslMapMarkerVisualSystem.Clear"); }
public static void OnWorldLoaded(params object[] args) { Boundary.Hit("MclslMapMarkerVisualSystem.OnWorldLoaded"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslMapMarkerVisualSystem.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslNativeActorFrames {

public static void Tick(params object[] args) { Boundary.Hit("MclslNativeActorFrames.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslNativeHistoryBridge {

public static void EnsureRegistered(params object[] args) { Boundary.Hit("MclslNativeHistoryBridge.EnsureRegistered"); }
} }
namespace MySimulatedLongevityRoad.Systems.Death { static class MclslNativeKillStatisticsSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslNativeKillStatisticsSystem.Clear"); }
public static void OnWorldLoaded(params object[] args) { Boundary.Hit("MclslNativeKillStatisticsSystem.OnWorldLoaded"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslRankSnapshotSource {

public static void TickUpdates(params object[] args) { Boundary.Hit("MclslRankSnapshotSource.TickUpdates"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslRealmHaloVisualSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslRealmHaloVisualSystem.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslRecipeKnowledge {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslRecipeKnowledge.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslReincarnationProfileStore {

public static void EnsureLoaded(params object[] args) { Boundary.Hit("MclslReincarnationProfileStore.EnsureLoaded"); }
public static void Flush(params object[] args) { Boundary.Hit("MclslReincarnationProfileStore.Flush"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslRuntimeCadence {

public static void Clear(params object[] args) { Boundary.Hit("MclslRuntimeCadence.Clear"); }
public static void InitializeAfterLoad(params object[] args) { Boundary.Hit("MclslRuntimeCadence.InitializeAfterLoad"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslRuntimeCadence.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslRuntimeChanges {

public static void BindMainThread(params object[] args) { Boundary.Hit("MclslRuntimeChanges.BindMainThread"); }
public static void Clear(params object[] args) { Boundary.Hit("MclslRuntimeChanges.Clear"); }
public static void Drain(params object[] args) { Boundary.Hit("MclslRuntimeChanges.Drain"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslRuntimeDriver {

public static void Ensure(params object[] args) { Boundary.Hit("MclslRuntimeDriver.Ensure"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslScheduler {
public static bool NeedsAnnualStart = false; public static bool HasFastWork = false;
public static void SavePendingAnnualStates(params object[] args) { Boundary.Hit("MclslScheduler.SavePendingAnnualStates"); }
public static void ScheduleAnnualWorld(params object[] args) { Boundary.Hit("MclslScheduler.ScheduleAnnualWorld"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslSpellCasting {

public static void Clear(params object[] args) { Boundary.Hit("MclslSpellCasting.Clear"); }
public static void Load(params object[] args) { Boundary.Hit("MclslSpellCasting.Load"); }
public static void Save(params object[] args) { Boundary.Hit("MclslSpellCasting.Save"); }
public static void Tick(params object[] args) { Boundary.Hit("MclslSpellCasting.Tick"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslSpellSystem {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslSpellSystem.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslSpiritualRootSystem {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslSpiritualRootSystem.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslTextValue {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslTextValue.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslTianxuanMarket {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslTianxuanMarket.ClearRuntime"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslTimelineSystem {

public static void TickAnnual(params object[] args) { Boundary.Hit("MclslTimelineSystem.TickAnnual"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslTraitEditorEraFilter {

public static void ClearRuntime(params object[] args) { Boundary.Hit("MclslTraitEditorEraFilter.ClearRuntime"); }
public static void RefreshVisibleEditors(params object[] args) { Boundary.Hit("MclslTraitEditorEraFilter.RefreshVisibleEditors"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslVisibleActorRenderLane {

public static void Clear(params object[] args) { Boundary.Hit("MclslVisibleActorRenderLane.Clear"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslWorldRunRepository {

public static void EnsureAnnualWorldState(params object[] args) { Boundary.Hit("MclslWorldRunRepository.EnsureAnnualWorldState"); }
public static void EnsureCurrentRun(params object[] args) { Boundary.Hit("MclslWorldRunRepository.EnsureCurrentRun"); }
public static void ResetWorld(params object[] args) { Boundary.Hit("MclslWorldRunRepository.ResetWorld"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslWorldSoulSystem {

public static void Clear(params object[] args) { Boundary.Hit("MclslWorldSoulSystem.Clear"); }
public static void OnWorldLoaded(params object[] args) { Boundary.Hit("MclslWorldSoulSystem.OnWorldLoaded"); }
public static void TickFrame(params object[] args) { Boundary.Hit("MclslWorldSoulSystem.TickFrame"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslWorldStateAudit {

public static void ReportResidual(params object[] args) { Boundary.Hit("MclslWorldStateAudit.ReportResidual"); }
} }
namespace MySimulatedLongevityRoad.Core { static class MclslWorldStateModifierSystem {

public static void ClearCache(params object[] args) { Boundary.Hit("MclslWorldStateModifierSystem.ClearCache"); }
} }
namespace MySimulatedLongevityRoad.Patches { static class MclslXuanhuangDirectionPatch {

public static void Clear(params object[] args) { Boundary.Hit("MclslXuanhuangDirectionPatch.Clear"); }
public static void PrepareWorldColors(params object[] args) { Boundary.Hit("MclslXuanhuangDirectionPatch.PrepareWorldColors"); }
public static void TickColors(params object[] args) { Boundary.Hit("MclslXuanhuangDirectionPatch.TickColors"); }
} }
namespace MySimulatedLongevityRoad.Traits { static class MclslXuanhuangRegistration {

public static void EnsureWildKingdom(params object[] args) { Boundary.Hit("MclslXuanhuangRegistration.EnsureWildKingdom"); }
} }
