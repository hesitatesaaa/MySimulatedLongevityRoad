using System;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Patches;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;
using MySimulatedLongevityRoad.Interop;
using NeoModLoader.api;
using UnityEngine;

namespace MySimulatedLongevityRoad;

internal sealed class MclslMod : BasicMod<MclslMod>
{
    internal const string HarmonyId = "shiyue.worldbox.mod.MySimulatedLongevityRoad";

    protected override void OnModLoad()
    {
        using var profileScope = MclslUnityProfiler.Sample("MCLS/Load/OnModLoad");
        try
        {
            // Actor registration can enqueue native frames before Runtime.Init.
            MclslRuntimeChanges.BindMainThread();
            MclslLocalizationRuntimeMarker.MarkLoaded();
            MclslLocalizationBridge.LoadChineseCatalog();
            MclslConfigLocalization.Init();
            ModConfig config = GetConfig();
            MclslConfig.EnsurePhysiqueAutoCollectItem(config);
            MclslRuntimeSettings.LoadFromModConfig(config);
            using (MclslUnityProfiler.Sample("MCLS/Load/Traits"))
                MclslTraitRegistration.Init();
            MclslItemUseSystem.RegisterStatuses();
            MclslSpellSystem.RegisterStatuses();
            MclslArtifactSystem.RegisterEquipmentStatuses();
            MclslArtifactRain.Init();
            using (MclslUnityProfiler.Sample("MCLS/Load/WorldSouls"))
                MclslWorldSoulActorRegistration.Init();
            using (MclslUnityProfiler.Sample("MCLS/Load/Immortals"))
                MclslImmortalActorRegistration.Init();
            MclslBeastConfig.Ensure(config);
            using (MclslUnityProfiler.Sample("MCLS/Load/Beasts"))
                MclslBeastActorRegistration.Init();
            using (MclslUnityProfiler.Sample("MCLS/Load/NamedActors"))
                MclslNamedCharacterRegistration.Init();
            Harmony harmony = new Harmony(HarmonyId);
            int patchedCount = 0;
            using (MclslUnityProfiler.Sample("MCLS/Load/Harmony"))
            {
                patchedCount += MclslPatches.ApplySafely(harmony);
                patchedCount += MclslCombatPatches.ApplySafely(harmony);
            }
            if (MclslHarmonyPatchGuard.FailedRequiredCount > 0)
                Debug.LogWarning("[模拟长生路] " + MclslHarmonyPatchGuard.FailedRequiredCount
                    + " 个关键补丁未挂载：" + MclslHarmonyPatchGuard.FailedRequiredSummary);
            MclslRuntime.Init();
            using (MclslUnityProfiler.Sample("MCLS/Load/UI"))
                MclslUiManager.Init();
            MclslNativeHistoryBridge.EnsureRegistered();
            if (MclslRuntimeSettings.DiagnosticsEnabled)
            {
                MclslFpsOverlay.Ensure();
                MclslDiagnostics.Once("harmony-loaded", "Harmony 补丁安全挂载完成：" + patchedCount);
                MclslDiagnostics.Once("module-list", MclslModuleHub.DebugModuleList());
                Debug.Log("[模拟长生路] 模组加载完成。");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[模拟长生路] 模组加载失败: " + ex);
            throw;
        }
    }

    internal static object GetModConfigSafe() => I?.GetConfig();
}
