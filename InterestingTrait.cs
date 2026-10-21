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
        using var profileScope = MclslStartupTiming.Measure("MCLS/Load/OnModLoad");
        try
        {
            // Actor registration can enqueue native frames before Runtime.Init.
            MclslRuntimeChanges.BindMainThread();
            using (MclslStartupTiming.Measure("MCLS/Load/Localization"))
            {
                MclslLocalizationBridge.LoadChineseCatalog();
                MclslConfigLocalization.Init();
            }
            ModConfig config = GetConfig();
            MclslConfig.EnsurePhysiqueAutoCollectItem(config);
            MclslRuntimeSettings.LoadFromModConfig(config);
            using (MclslStartupTiming.Measure("MCLS/Load/Traits"))
                MclslTraitRegistration.Init();
            Systems.Combat.MclslCombatCareerSystem.InitTraits();
            MclslItemUseSystem.RegisterStatuses();
            MclslSpellSystem.RegisterStatuses();
            MclslArtifactSystem.RegisterEquipmentStatuses();
            MclslArtifactRain.Init();
            using (MclslStartupTiming.Measure("MCLS/Load/WorldSouls"))
                MclslWorldSoulActorRegistration.Init();
            using (MclslStartupTiming.Measure("MCLS/Load/Immortals"))
                MclslImmortalActorRegistration.Init();
            MclslBeastConfig.Ensure(config);
            using (MclslStartupTiming.Measure("MCLS/Load/Beasts"))
                MclslBeastActorRegistration.Init();
            using (MclslStartupTiming.Measure("MCLS/Load/NamedActors"))
                MclslNamedCharacterRegistration.Init();
            MclslLivingResources.Init();
            MclslXuanhuangRegistration.Init();
            Systems.Combat.MclslCareerCombat.Init();
            Harmony harmony = new Harmony(HarmonyId);
            int patchedCount = 0;
            using (MclslStartupTiming.Measure("MCLS/Load/Harmony"))
            {
                patchedCount += MclslPatches.ApplySafely(harmony);
                patchedCount += MclslCombatPatches.ApplySafely(harmony);
                patchedCount += MclslCareerPatches.Apply(harmony);
                patchedCount += MclslXuanhuangDirectionPatch.Apply(harmony);
            }
            if (MclslHarmonyPatchGuard.FailedRequiredCount > 0)
                Debug.LogWarning("[模拟长生路] " + MclslHarmonyPatchGuard.FailedRequiredCount
                    + " 个关键补丁未挂载：" + MclslHarmonyPatchGuard.FailedRequiredSummary);
            using (MclslStartupTiming.Measure("MCLS/Load/Runtime"))
                MclslRuntime.Init();
            using (MclslStartupTiming.Measure("MCLS/Load/UI"))
                MclslUiManager.Init();
            MclslNativeHistoryBridge.EnsureRegistered();
            if (MclslHarmonyPatchGuard.FailedRequiredCount != 0)
                throw new InvalidOperationException("Required Harmony patches failed: " + MclslHarmonyPatchGuard.FailedRequiredSummary);
            MclslLocalizationRuntimeMarker.MarkLoaded();
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
            MclslLocalizationRuntimeMarker.MarkFailed();
            try { new Harmony(HarmonyId).UnpatchSelf(); }
            catch (Exception cleanup) { Debug.LogError("[模拟长生路] 补丁回收失败: " + cleanup); }
            Debug.LogError("[模拟长生路] 模组加载失败: " + ex);
            throw;
        }
    }

    internal static object GetModConfigSafe() => I?.GetConfig();
}
