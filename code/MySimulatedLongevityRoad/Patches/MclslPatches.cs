using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Modules;
using MySimulatedLongevityRoad.Queries;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Systems.Visual;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.UI;
using UnityEngine;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslPatches
{

    internal static int ApplySafely(Harmony harmony)
    {
        int count = 0;
        count += TryPatchPair(harmony, "UnitSelectionEffect.setVisible.custom", typeof(UnitSelectionEffect), "setVisible", new[] { typeof(bool) }, postfix: nameof(UnitSelectionEffect_SetVisible_Custom_Postfix));
        foreach (string method in new[] { "drawSelectedUnits", "drawUnitsToBeSelectedBySquareTool" })
            count += MclslHarmonyPatchGuard.TryPatch(harmony, "QuantumSpriteLibrary." + method,
                AccessTools.Method(typeof(QuantumSpriteLibrary), method, new[] { typeof(QuantumSpriteAsset) }),
                transpiler: AccessTools.Method(typeof(MclslPatches), nameof(SelectedUnits_Scale_Transpiler))) ? 1 : 0;
        count += TryPatchPair(harmony, "ActorAsset.getSpriteIcon.custom", typeof(ActorAsset), "getSpriteIcon", Type.EmptyTypes, prefix: nameof(ActorAsset_GetSpriteIcon_Custom_Prefix));
        count += TryPatchPair(harmony, "DynamicActorSpriteCreatorUI.getUnitSpriteForUI.custom", typeof(DynamicActorSpriteCreatorUI), "getUnitSpriteForUI",
            new[] { typeof(ActorAsset), typeof(Sprite), typeof(AnimationContainerUnit), typeof(bool), typeof(ActorSex), typeof(int), typeof(int), typeof(ColorAsset), typeof(long), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool) },
            prefix: nameof(DynamicActorSpriteCreatorUI_GetUnitSpriteForUI_Custom_Prefix));
        count += TryPatchPair(harmony, "UnitAvatarLoader.loadItemSprites.custom", typeof(UnitAvatarLoader), "loadItemSprites", Type.EmptyTypes, prefix: nameof(UnitAvatarLoader_LoadItemSprites_Custom_Prefix));
        count += TryPatchPair(harmony, "Image.SetNativeSize.custom", typeof(UnityEngine.UI.Image), "SetNativeSize", Type.EmptyTypes, postfix: nameof(Image_SetNativeSize_Custom_Postfix));
        count += TryPatchPair(harmony, "ActorManager.checkNewSpecies.beast", typeof(ActorManager), "checkNewSpecies", new[] { typeof(ActorAsset), typeof(WorldTile), typeof(Actor), typeof(Actor).MakeByRefType(), typeof(bool), typeof(bool), typeof(Subspecies) }, prefix: nameof(ActorManager_CheckNewSpecies_Beast_Prefix));
        count += TryPatchPair(harmony, "LocalizedTextManager.getText", typeof(LocalizedTextManager), nameof(LocalizedTextManager.getText), new[] { typeof(string), typeof(UnityEngine.UI.Text), typeof(bool) }, prefix: nameof(LocalizedTextManager_GetText_NormalizeMclslKey_Prefix), postfix: nameof(LocalizedTextManager_GetText_MclslFallback_Postfix));
        count += TryPatchPair(harmony, "LocalizedTextManager.stringExists", typeof(LocalizedTextManager), nameof(LocalizedTextManager.stringExists), new[] { typeof(string) }, prefix: nameof(LocalizedTextManager_StringExists_NormalizeMclslKey_Prefix), postfix: nameof(LocalizedTextManager_StringExists_MclslFallback_Postfix));
        count += TryPatchPair(harmony, "LocalizedTextManager.setLanguage", typeof(LocalizedTextManager), nameof(LocalizedTextManager.setLanguage), new[] { typeof(string) }, postfix: nameof(LocalizedTextManager_SetLanguage_RestoreMclslChinese_Postfix));
        count += TryPatchPair(harmony, "Subspecies.generateName", typeof(Subspecies), "generateName", new[] { typeof(ActorAsset), typeof(WorldTile) }, postfix: nameof(Subspecies_GenerateName_WorldSoul_Postfix));
        count += TryPatchPair(harmony, "Subspecies.getUnitSpriteForBanner", typeof(Subspecies), "getUnitSpriteForBanner", Type.EmptyTypes, prefix: nameof(Subspecies_GetUnitSpriteForBanner_WorldSoul_Prefix));
        MethodInfo simulation = AccessTools.Method(typeof(MapBox), "updateSimulation", new[] { typeof(float) });
        bool protectedSimulation = MclslHarmonyPatchGuard.TryPatch(harmony, "MapBox.updateSimulation(float)", simulation,
            AccessTools.Method(typeof(MclslPatches), nameof(MapBox_UpdateSimulation_Prefix)), null);
        MclslAnnualBackpressure.SetPatchAvailable(protectedSimulation);
        if (protectedSimulation) count++;
        count += TryPatchPair(harmony, "MapBox.generateNewMap", typeof(MapBox), nameof(MapBox.generateNewMap), Type.EmptyTypes, prefix: nameof(MapBox_GenerateNewMap_Prefix));
        count += TryPatchPair(harmony, "MapBox.finishingUpLoading", typeof(MapBox), "finishingUpLoading", Type.EmptyTypes, postfix: nameof(MapBox_FinishingUpLoading_Postfix));
        count += TryPatchPair(harmony, "MapBox.clearWorld", typeof(MapBox), "clearWorld", Type.EmptyTypes, postfix: nameof(MapBox_ClearWorld_Postfix));
        count += TryPatchPair(harmony, "SaveManager.currentWorldToSavedMap", typeof(SaveManager), nameof(SaveManager.currentWorldToSavedMap), Type.EmptyTypes, prefix: nameof(SaveManager_CurrentWorldToSavedMap_Prefix));
        count += TryPatchPair(harmony, "SaveManager.loadWorld", typeof(SaveManager), nameof(SaveManager.loadWorld), new[] { typeof(string), typeof(bool) }, prefix: nameof(SaveManager_LoadWorld_Prefix), finalizer: nameof(SaveManager_LoadWorld_Finalizer));
        count += TryPatchPair(harmony, "Actor.addTrait.asset", typeof(Actor), "addTrait", new[] { typeof(ActorTrait), typeof(bool) }, prefix: nameof(Actor_AddPhysique_Guard), postfix: nameof(Actor_AddTrait_Postfix));
        count += TryPatchPair(harmony, "Actor.addTrait.id", typeof(Actor), "addTrait", new[] { typeof(string), typeof(bool) }, prefix: nameof(Actor_AddPhysiqueById_Guard), postfix: nameof(Actor_AddTraitById_Postfix));
        count += TryPatchPair(harmony, "Actor.updateAge", typeof(Actor), "updateAge", Type.EmptyTypes, postfix: nameof(Actor_UpdateAge_RegisterPostfix));
        count += TryPatchPair(harmony, "Actor.updateStats.lifespanRepair", typeof(Actor), "updateStats", Type.EmptyTypes, postfix: nameof(Actor_UpdateStats_LifespanRepair_Postfix));
        count += TryPatchPair(harmony, "Actor.removeTrait", typeof(Actor), "removeTrait", new[] { typeof(string) }, prefix: nameof(Actor_RemoveSpiritualRoot_Guard), postfix: nameof(Actor_RemoveTrait_Postfix));
        count += TryPatchPairOptional(harmony, "ActorTraitsEditor.OnEnable", typeof(ActorTraitsEditor), "OnEnable", Type.EmptyTypes, postfix: nameof(ActorTraitsEditor_OnEnable_Postfix));
        count += TryPatchPairOptional(harmony, "ActorTraitsEditor.OnDisable", typeof(ActorTraitsEditor), "OnDisable", Type.EmptyTypes, postfix: nameof(ActorTraitsEditor_OnDisable_Postfix));
        count += TryPatchPair(harmony, "Actor.setAttackTarget", typeof(Actor), "setAttackTarget", new[] { typeof(BaseSimObject) }, postfix: nameof(Actor_TargetChanged_Postfix));
        count += TryPatchPair(harmony, "Actor.clearAttackTarget", typeof(Actor), "clearAttackTarget", Type.EmptyTypes, postfix: nameof(Actor_TargetChanged_Postfix));
        count += TryPatchPair(harmony, "Actor.clearBeh", typeof(Actor), "clearBeh", Type.EmptyTypes, postfix: nameof(Actor_TargetChanged_Postfix));
        count += TryPatchPair(harmony, "Actor.clearTasks", typeof(Actor), "clearTasks", Type.EmptyTypes, postfix: nameof(Actor_TargetChanged_Postfix));
        count += TryPatchPair(harmony, "Actor.setTask", typeof(Actor), "setTask", new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool) }, postfix: nameof(Actor_TargetChanged_Postfix));
        count += TryPatchPair(harmony, "Actor.die", typeof(Actor), "die", new[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) }, prefix: nameof(Actor_Die_Prefix), postfix: nameof(Actor_Die_Postfix));
        count += TryPatchPair(harmony, "UnitWindow.OnEnable", typeof(UnitWindow), "OnEnable", Type.EmptyTypes, postfix: nameof(UnitWindow_OnEnable_Postfix));
        count += TryPatchPairOptional(harmony, "UnitWindow.OnDisable", typeof(UnitWindow), "OnDisable", Type.EmptyTypes, postfix: nameof(UnitWindow_OnDisable_Postfix));
        count += TryPatchPair(harmony, "UnitWindow.showStatsRows", typeof(UnitWindow), "showStatsRows", Type.EmptyTypes, postfix: nameof(UnitWindow_ShowStatsRows_Postfix));
        count += TryPatchPair(harmony, "UnitWindow.showInfo", typeof(UnitWindow), "showInfo", Type.EmptyTypes, postfix: nameof(UnitWindow_ShowInfo_Postfix));
        count += TryPatchPairOptional(harmony, "SelectedUnitTab.showStatBars.mana", typeof(SelectedUnitTab), "showStatBars", new[] { typeof(Actor) }, postfix: nameof(SelectedUnitTab_ShowStatBars_Mana_Postfix));
        count += TryPatchPairOptional(harmony, "UnitBarsElement.showContent.mana", typeof(UnitBarsElement), "showContent", Type.EmptyTypes, postfix: nameof(UnitBarsElement_ShowContent_Mana_Postfix));
        count += TryPatchPairOptional(harmony, "PlayerControl.checkClickTouchInspectSelect", typeof(PlayerControl), "checkClickTouchInspectSelect", Type.EmptyTypes, postfix: nameof(PlayerControl_CheckClickTouchInspectSelect_Postfix));
        count += TryPatchPair(harmony, "UnitStatsElement.showContent.safe", typeof(UnitStatsElement), "showContent", Type.EmptyTypes, finalizer: nameof(UnitStatsElement_ShowContent_Finalizer));
        count += TryPatchPair(harmony, "Actor.calculateMainSprite", typeof(Actor), "calculateMainSprite", Type.EmptyTypes, prefix: nameof(Actor_CalculateMainSprite_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.calculateColoredSprite.fullArt", typeof(Actor), "calculateColoredSprite", new[] { typeof(Sprite), typeof(bool) }, prefix: nameof(Actor_CalculateColoredSprite_FullArt_Prefix));
        count += TryPatchPair(harmony, "Actor.isSapient.beast", typeof(Actor), "isSapient", Type.EmptyTypes, postfix: nameof(Actor_IsSapient_Beast_Postfix));
        count += TryPatchPair(harmony, "Actor.attackTargetActions.animation", typeof(Actor), "attackTargetActions", new[] { typeof(BaseSimObject), typeof(WorldTile) }, postfix: nameof(Actor_AttackTargetActions_Animation_Postfix));
        count += TryPatchPair(harmony, "Actor.getHit.animation", typeof(Actor), "getHit", new[] { typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool) }, postfix: nameof(Actor_GetHit_Animation_Postfix));
        count += TryPatchPair(harmony, "ActorManager.createBabyActorFromData.beast", typeof(ActorManager), "createBabyActorFromData", new[] { typeof(ActorData), typeof(WorldTile), typeof(City) }, postfix: nameof(ActorManager_CreateBabyActorFromData_Beast_Postfix));
        count += TryPatchPair(harmony, "ActorManager.createNewUnit.beast", typeof(ActorManager), "createNewUnit", new[] { typeof(string), typeof(WorldTile), typeof(bool), typeof(float), typeof(Subspecies), typeof(Subspecies), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }, postfix: nameof(ActorManager_CreateBabyActorFromData_Beast_Postfix));
        count += TryPatchPair(harmony, "ActorManager.precalculateRenderDataParallel", typeof(ActorManager), "precalculateRenderDataParallel", Type.EmptyTypes, prefix: nameof(ActorManager_PrecalculateRenderData_AnimationTime_Prefix), postfix: nameof(ActorManager_PrecalculateRenderDataParallel_Halo_Postfix));
        count += TryPatchPair(harmony, "ActorManager.precalculateRenderDataNormal", typeof(ActorManager), "precalculateRenderDataNormal", Type.EmptyTypes, prefix: nameof(ActorManager_PrecalculateRenderData_AnimationTime_Prefix), postfix: nameof(ActorManager_PrecalculateRenderDataNormal_Halo_Postfix));
        count += TryPatchPair(harmony, "Actor.makeStunned", typeof(Actor), nameof(Actor.makeStunned), new[] { typeof(float) }, prefix: nameof(Actor_MakeStunned_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.makeSleep", typeof(Actor), nameof(Actor.makeSleep), new[] { typeof(float) }, prefix: nameof(Actor_MakeSleep_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.calculateForce", typeof(Actor), nameof(Actor.calculateForce), new[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) }, prefix: nameof(Actor_CalculateForce_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.applyRandomForce", typeof(Actor), nameof(Actor.applyRandomForce), new[] { typeof(float), typeof(float) }, prefix: nameof(Actor_ApplyRandomForce_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "BaseSimObject.addStatusEffect", typeof(BaseSimObject), nameof(BaseSimObject.addStatusEffect), new[] { typeof(StatusAsset), typeof(float), typeof(bool) }, prefix: nameof(BaseSimObject_AddStatusEffect_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "ScrollWindow.hide", typeof(ScrollWindow), nameof(ScrollWindow.hide), new[] { typeof(string), typeof(bool) }, prefix: nameof(ScrollWindow_Hide_RankRightClickGuard_Prefix));
        count += TryPatchPair(harmony, "ScrollWindow.hideAllEvent", typeof(ScrollWindow), "hideAllEvent", new[] { typeof(bool) }, prefix: nameof(ScrollWindow_HideAllEvent_RankRightClickGuard_Prefix));
        count += MclslUnitWindowLifecycle.Apply(harmony);
        return count;
    }

    private static int TryPatchPair(Harmony harmony, string key, Type targetType, string targetName, Type[] targetArgs, string prefix = null, string postfix = null, string finalizer = null)
    {
        MethodInfo original = ResolveTargetMethod(targetType, targetName, targetArgs);
        MethodInfo prefixMethod = string.IsNullOrWhiteSpace(prefix) ? null : AccessTools.Method(typeof(MclslPatches), prefix);
        MethodInfo postfixMethod = string.IsNullOrWhiteSpace(postfix) ? null : AccessTools.Method(typeof(MclslPatches), postfix);
        MethodInfo finalizerMethod = string.IsNullOrWhiteSpace(finalizer) ? null : AccessTools.Method(typeof(MclslPatches), finalizer);
        return MclslHarmonyPatchGuard.TryPatch(harmony, key, original, prefixMethod, postfixMethod, finalizerMethod) ? 1 : 0;
    }

    private static MethodInfo ResolveTargetMethod(Type targetType, string targetName, Type[] targetArgs)
    {
        if (targetType == null || string.IsNullOrWhiteSpace(targetName)) return null;
        // A similarly named overload is not a compatible Harmony target.
        return AccessTools.Method(targetType, targetName, targetArgs);
    }

    private static int TryPatchPairOptional(Harmony harmony, string key, Type targetType, string targetName, Type[] targetArgs, string prefix = null, string postfix = null, string finalizer = null)
    {
        MethodInfo original = ResolveTargetMethod(targetType, targetName, targetArgs);
        if (original == null) return 0;
        MethodInfo prefixMethod = string.IsNullOrWhiteSpace(prefix) ? null : AccessTools.Method(typeof(MclslPatches), prefix);
        MethodInfo postfixMethod = string.IsNullOrWhiteSpace(postfix) ? null : AccessTools.Method(typeof(MclslPatches), postfix);
        MethodInfo finalizerMethod = string.IsNullOrWhiteSpace(finalizer) ? null : AccessTools.Method(typeof(MclslPatches), finalizer);
        return MclslHarmonyPatchGuard.TryPatch(harmony, key, original, prefixMethod, postfixMethod, finalizerMethod, required: false) ? 1 : 0;
    }

    private static bool LocalizedTextManager_GetText_NormalizeMclslKey_Prefix(ref string __0, ref string __result)
    {
        if (MclslTextValue.TryDecode(__0, out string inlineText)) { __result = inlineText; return false; }
        MclslLocalizationBridge.NormalizeLookupKey(ref __0);
        if (MclslLocalizationBridge.TryResolveRuntimeKey(__0, out string chinese)
            && !MclslLocalizationBridge.TryGetActiveText(__0, out _))
        {
            __result = chinese;
            return false;
        }
        return true;
    }

    private static void LocalizedTextManager_GetText_MclslFallback_Postfix(string __0, ref string __result)
    {
        if ((string.IsNullOrEmpty(__result) || string.Equals(__result, __0, StringComparison.Ordinal))
            && MclslLocalizationBridge.TryResolveRuntimeKey(__0, out string chinese))
            __result = chinese;
    }

    private static void LocalizedTextManager_StringExists_NormalizeMclslKey_Prefix(ref string __0)
    {
        MclslLocalizationBridge.NormalizeLookupKey(ref __0);
    }

    private static void LocalizedTextManager_StringExists_MclslFallback_Postfix(string __0, ref bool __result)
    {
        if (!__result && MclslLocalizationBridge.TryResolveRuntimeKey(__0, out _)) __result = true;
    }

    private static void LocalizedTextManager_SetLanguage_RestoreMclslChinese_Postfix()
    {
        MclslLocalizationBridge.ReapplyAfterLanguageChange();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Subspecies), "generateName")]
    private static void Subspecies_GenerateName_WorldSoul_Postfix(Subspecies __instance)
    {
        MclslActorPresentation.NormalizeSubspecies(__instance);
        MclslWorldSoulActorRegistration.NormalizeSubspecies(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Subspecies), "getUnitSpriteForBanner")]
    private static bool Subspecies_GetUnitSpriteForBanner_WorldSoul_Prefix(Subspecies __instance, ref UnityEngine.Sprite __result)
    {
        MclslActorPresentation.NormalizeSubspecies(__instance);
        __result = MclslActorPresentation.Portrait(__instance?.data?.species_id);
        if (__result != null) return false;
        __result = MclslImmortalActorRegistration.TryGetBannerSprite(__instance);
        if (__result != null) return false;
        if (!MclslWorldSoulActorRegistration.IsWorldSoulSubspecies(__instance)) return true;
        __result = MclslWorldSoulActorRegistration.TryGetBannerSprite();
        return __result == null;
    }

    private static bool MapBox_UpdateSimulation_Prefix(ref float __0) => MclslAnnualBackpressure.Apply(ref __0);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapBox), nameof(MapBox.generateNewMap))]
    private static void MapBox_GenerateNewMap_Prefix()
    {
        MclslMapMarkerVisualSystem.Clear();
        MclslArtifactSystem.ClearRuntime();
        MclslItemEffectDriver.ClearRuntime();
        MclslBagSystem.ClearRuntime();
        MclslTianxuanMarket.ClearRuntime();
        MclslHuanzhenSystem.PrepareForNewWorld();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapBox), "finishingUpLoading")]
    private static void MapBox_FinishingUpLoading_Postfix()
    {
        MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
        MclslFpsOverlay.SetVisible(MclslRuntimeSettings.ShowFps);
        MclslRuntime.OnWorldLoaded();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapBox), "clearWorld")]
    private static void MapBox_ClearWorld_Postfix() => MclslRuntime.ClearWorldState();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.currentWorldToSavedMap))]
    private static void SaveManager_CurrentWorldToSavedMap_Prefix() => MclslRuntime.PrepareForSave();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.loadWorld), new Type[] { typeof(string), typeof(bool) })]
    private static void SaveManager_LoadWorld_Prefix(string __0)
    {
        MclslMapMarkerVisualSystem.Clear();
        MclslArtifactSystem.ClearRuntime();
        MclslItemEffectDriver.ClearRuntime();
        MclslBagSystem.ClearRuntime();
        MclslTianxuanMarket.ClearRuntime();
        MclslHuanzhenSystem.PrepareForAnyWorldLoad(__0);
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.loadWorld), new Type[] { typeof(string), typeof(bool) })]
    private static Exception SaveManager_LoadWorld_Finalizer(Exception __exception)
    {
        if (__exception != null) MclslHuanzhenSystem.AbortWorldLoadBinding();
        return __exception;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "addTrait", new Type[] { typeof(ActorTrait), typeof(bool) })]
    private static void Actor_AddTrait_Postfix(Actor __instance, ActorTrait __0, bool __result)
    {
        if (!__result) return;
        MclslTraitRegistration.InvalidateNativeRealmTraitSync(__instance);
        MclslTraitGrantRouter.HandleAddedTrait(__instance, __0?.id);
        MclslRuntimeChanges.Publish(__instance, MclslActorChange.Traits);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), "addTrait", new Type[] { typeof(ActorTrait), typeof(bool) })]
    private static bool Actor_AddPhysique_Guard(Actor __instance, ActorTrait __0, ref bool __result)
    {
        if (__instance?.data == null || MclslActorAccessor.Id(__instance) <= 0
            || !MclslPhysiqueSystem.IsPhysique(__0?.id)) return true;
        // Native restore may add traits before the subspecies (and therefore
        // isSapient) is initialized. Let that pending state through and let the
        // lifecycle reconciliation decide once identity is available.
        if (!MclslEligibility.TryGetSpecialPhysiqueEligibility(__instance, out bool eligible)
            || eligible) return true;
        __result = false;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "addTrait", new Type[] { typeof(string), typeof(bool) })]
    private static void Actor_AddTraitById_Postfix(Actor __instance, string __0, bool __result)
    {
        if (!__result) return;
        MclslTraitRegistration.InvalidateNativeRealmTraitSync(__instance);
        MclslTraitGrantRouter.HandleAddedTrait(__instance, __0);
        MclslRuntimeChanges.Publish(__instance, MclslActorChange.Traits);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), "addTrait", new Type[] { typeof(string), typeof(bool) })]
    private static bool Actor_AddPhysiqueById_Guard(Actor __instance, string __0, ref bool __result)
    {
        if (__instance?.data == null || MclslActorAccessor.Id(__instance) <= 0
            || !MclslPhysiqueSystem.IsPhysique(__0)) return true;
        if (!MclslEligibility.TryGetSpecialPhysiqueEligibility(__instance, out bool eligible)
            || eligible) return true;
        __result = false;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "updateAge")]
    private static void Actor_UpdateAge_RegisterPostfix(Actor __instance)
    {
        MclslPhysiqueSystem.TryRollAtBirth(__instance);
        MclslPhysiqueSystem.ReconcileOwner(__instance);
        MclslScheduler.RegisterAndEnqueueAnnualActor(__instance);
        MclslArtifactSystem.OnActorInitialized(__instance);
        MclslRuntimeChanges.Publish(__instance, MclslActorChange.Native);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "updateStats")]
    private static void Actor_UpdateStats_LifespanRepair_Postfix(Actor __instance)
    {
        // WorldBox calls updateStats from Parallel.ForEach. Only the per-actor ID
        // crosses this callback; caches and gameplay repair run on the main thread.
        MclslRuntimeChanges.PublishNativeStats(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "removeTrait", new Type[] { typeof(string) })]
    private static void Actor_RemoveTrait_Postfix(Actor __instance, string __0)
    {
        MclslTraitRegistration.InvalidateNativeRealmTraitSync(__instance);
        MclslTraitGrantRouter.HandleRemovedTrait(__instance, __0);
        MclslRuntimeChanges.Publish(__instance, MclslActorChange.Traits);
    }

    private static bool Actor_RemoveSpiritualRoot_Guard(Actor __instance, string __0)
    {
        if (MclslTraitGrantRouter.IsSuppressed || !MclslTraitRegistration.IsGiftTrait(__0)) return true;
        return !MclslPhysiqueSystem.HasAny(__instance);
    }

    private static void ActorTraitsEditor_OnDisable_Postfix(ActorTraitsEditor __instance) => MclslTraitEditorEraFilter.OnClosed(__instance);

    private static void ActorTraitsEditor_OnEnable_Postfix(ActorTraitsEditor __instance)
    {
        TryPatch("trait-editor-era-on-enable", () => MclslTraitEditorEraFilter.Apply(__instance));
    }

    private static bool LooksLikeMclslTraitId(string traitId)
    {
        if (string.IsNullOrWhiteSpace(traitId)) return false;
        if (traitId == MclslTraitRegistration.HuanzhenTraitId) return true;
        if (traitId == MclslTraitRegistration.WorldSoulEntityTraitId) return true;
        return traitId.StartsWith("gifts_", StringComparison.Ordinal)
            || traitId.StartsWith("realm_", StringComparison.Ordinal)
            || traitId.StartsWith("Mclsl", StringComparison.Ordinal);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), "die", new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) })]
    private static bool Actor_Die_Prefix(Actor __instance, AttackType __1, out MclslDeathPatchState __state)
    {
        MclslWorldActorQuery.TrackIfRelevant(__instance);
        if (MclslWorldSoulSystem.ShouldBlockNonCombatDeath(__instance, __1))
        {
            MclslNativeKillStatisticsSystem.RollbackDivertedDeath(__instance);
            __state = new MclslDeathPatchState(
                MySimulatedLongevityRoad.Data.Death.MclslDeathSnapshot.Empty,
                MclslHuanzhenSystem.CaptureDeath(__instance),
                MclslWorldSoulSystem.CaptureDeath(__instance));
            return false;
        }
        if (MclslDeathSystem.TryDivertNativeDeath(__instance, __1))
        {
            MclslNativeKillStatisticsSystem.RollbackDivertedDeath(__instance);
            __state = new MclslDeathPatchState(
                MySimulatedLongevityRoad.Data.Death.MclslDeathSnapshot.Empty,
                MclslHuanzhenSystem.CaptureDeath(__instance),
                MclslWorldSoulSystem.CaptureDeath(__instance));
            return false;
        }
        MclslBeastAnimation.PrepareDeath(__instance);
        __state = new MclslDeathPatchState(
            MclslDeathSystem.Capture(__instance, __1),
            MclslHuanzhenSystem.CaptureDeath(__instance),
            MclslWorldSoulSystem.CaptureDeath(__instance),
            __instance?.data!=null&&__instance.getHealth()<=0);
        return true;
    }

    private static void Actor_TargetChanged_Postfix(Actor __instance)
        => MclslRuntimeChanges.Publish(__instance, MclslActorChange.Native);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "die", new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) })]
    private static void Actor_Die_Postfix(Actor __instance, AttackType __1, MclslDeathPatchState __state)
    {
        bool dead;
        try { dead = __instance?.data != null && !__instance.isAlive(); }
        catch (Exception ex)
        {
            ReportPatchFailure("actor-death-state", ex);
            dead = false;
        }
        if (!dead)
        {
            MclslNativeKillStatisticsSystem.RollbackDivertedDeath(__instance);
            return;
        }

        MclslBeastAnimation.MarkDeath(__instance);

        long deadActorId = MclslActorAccessor.Id(__instance);
        try { MclslDeathEventRouter.CommitActorDeath(__instance, __1, __state); }
        finally
        {
            MclslCultivatorCandidateIndex.Remove(deadActorId);
            MclslWorldActorQuery.MarkDirty();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "OnEnable")]
    private static void UnitWindow_OnEnable_Postfix(UnitWindow __instance)
    {
        TryPatch("developer-select-actor-on-open", () => MclslDeveloperBridge.SetSelectedActor(__instance?.actor));
        TryPatch("unit-window-track-on-enable", () => MclslWorldActorQuery.TrackIfRelevant(__instance?.actor));
        TryPatch("unit-window-gender-on-enable", () => MclslGenderToggleButton.Refresh(__instance));
        TryPatch("unit-window-qiankun-tab", () => MclslBackpackTabSystem.Bind(__instance));
        TryPatch("unit-window-info-panel-enable", () => MclslActorInfoPanel.Refresh(__instance, resetScrollForNewActor: true));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "OnDisable")]
    private static void UnitWindow_OnDisable_Postfix(UnitWindow __instance)
    {
        TryPatch("unit-window-info-panel-disable", () => MclslActorInfoPanel.OnWindowClosed(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "showStatsRows")]
    private static void UnitWindow_ShowStatsRows_Postfix(UnitWindow __instance)
    {
        TryPatch("developer-select-actor-on-stats", () => MclslDeveloperBridge.SetSelectedActor(__instance?.actor));
        TryPatch("unit-window-track-stats", () => MclslWorldActorQuery.TrackIfRelevant(__instance?.actor));
        TryPatch("unit-window-gender-stats", () => MclslGenderToggleButton.Refresh(__instance));
        TryPatch("unit-window-overview-stats", () => MclslActorOverviewStatsFormatter.Refresh(__instance));
        TryPatch("unit-window-info-panel-stats", () => MclslActorInfoPanel.Refresh(__instance));
    }

    private static void SelectedUnitTab_ShowStatBars_Mana_Postfix(SelectedUnitTab __instance, Actor pActor)
    {
        TryPatch("selected-unit-mana-bar", () => MclslManaBar.UpdateSelected(__instance, pActor));
    }

    private static void UnitBarsElement_ShowContent_Mana_Postfix(UnitBarsElement __instance)
    {
        TryPatch("unit-bars-mana", () => MclslManaBar.UpdateUnit(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "showInfo")]
    private static void UnitWindow_ShowInfo_Postfix(UnitWindow __instance)
    {
        TryPatch("developer-select-actor-on-info", () => MclslDeveloperBridge.SetSelectedActor(__instance?.actor));
        TryPatch("unit-window-track-info", () => MclslWorldActorQuery.TrackIfRelevant(__instance?.actor));
        TryPatch("unit-window-gender-info", () => MclslGenderToggleButton.Refresh(__instance));
        TryPatch("unit-window-overview-info", () => MclslActorOverviewStatsFormatter.Refresh(__instance));
        TryPatch("unit-window-info-panel-info", () => MclslActorInfoPanel.Refresh(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerControl), "checkClickTouchInspectSelect")]
    private static void PlayerControl_CheckClickTouchInspectSelect_Postfix()
    {
        if (!MclslDeveloperBridge.IsAvailable) return;
        TryPatch("developer-select-actor-on-map-click", () =>
        {
            Actor actor = World.world?.getActorNearCursor();
            if (actor?.data != null && actor.isAlive()) MclslDeveloperBridge.SetSelectedActor(actor);
        });
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(UnitStatsElement), "showContent")]
    private static Exception UnitStatsElement_ShowContent_Finalizer(Exception __exception)
    {
        return __exception; // Do not conceal failures in native coroutine creation.
    }

    private static void TryPatch(string key, Action action)
    {
        try { action?.Invoke(); }
        catch (Exception ex) { ReportPatchFailure(key, ex); }
    }

    private static void ReportPatchFailure(string key, Exception ex)
    {
        MclslDiagnostics.Error("patch:" + key, key + " 失败: " + ex.Message);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), "calculateMainSprite")]
    private static bool Actor_CalculateMainSprite_WorldSoul_Prefix(Actor __instance, ref Sprite __result)
    {
        try
        {
            if (MclslWorldSoulActorRegistration.TryGetRenderSprite(__instance, out Sprite sprite))
            {
                __result = sprite;
                MclslWorldSoulActorRegistration.UpdateFrameData(__instance, sprite);
                return false;
            }
            if (MclslBeastAnimation.TryGetRenderSprite(__instance, out sprite))
            {
                __result = sprite;
                MclslBeastAnimation.UpdateFrameData(__instance, sprite);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning("[模拟长生路] 自定义角色主贴图刷新失败: " + ex.Message);
            return true;
        }
    }

    private static bool Actor_CalculateColoredSprite_FullArt_Prefix(Sprite __0, ref Sprite __result)
    {
        // Ownership survives imported textures with empty/native names. Workers
        // consult only the registry; never read pixels or mutate textures here.
        if (!MclslSpriteOwnership.Contains(__0)) return true;
        // These whole-body sprites already contain their authored skin/hair colors.
        // ActorRenderData.colors still supplies damage/status tinting separately.
        __result = __0;
        return false;
    }

    private static void ActorManager_CheckNewSpecies_Beast_Prefix(ref ActorAsset __0)
    {
        string speciesId = MclslActorPresentation.CanonicalSpeciesId(__0?.id);
        if (__0 == null || __0.id == speciesId) return;
        // Generate the shared lineage directly, avoiding an orphan humanoid
        // subspecies for every manual humanoid placement.
        __0 = AssetManager.actor_library.get(speciesId) ?? __0;
    }

    private static void UnitSelectionEffect_SetVisible_Custom_Postfix(UnitSelectionEffect __instance, bool __0)
    {
        Actor actor = UnitSelectionEffect.last_actor;
        if (!__0 || MclslActorPresentation.Portrait(actor?.asset?.id) == null) return;
        // Native setVisible already supplies the ground position and visibility.
        // Only detach its scale from the enlarged whole-body sprite.
        float scale = MclslActorPresentation.SelectionScale(actor);
        __instance.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static IEnumerable<CodeInstruction> SelectedUnits_Scale_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        FieldInfo scale = AccessTools.Field(typeof(BaseSimObject), "current_scale");
        FieldInfo y = AccessTools.Field(typeof(Vector3), "y");
        int replaced = 0;
        for (int i = 0; i + 1 < codes.Count; i++)
        {
            if (codes[i].opcode != OpCodes.Ldflda || !Equals(codes[i].operand, scale)
                || !codes[i + 1].LoadsField(y)) continue;
            codes[i].opcode = OpCodes.Call;
            codes[i].operand = AccessTools.Method(typeof(MclslActorPresentation), nameof(MclslActorPresentation.SelectionScale));
            codes[i + 1].opcode = OpCodes.Nop;
            codes[i + 1].operand = null;
            replaced++;
        }
        if (replaced != 1) throw new InvalidOperationException("原版选中标记缩放入口不匹配：" + replaced);
        return codes;
    }

    private static bool ActorAsset_GetSpriteIcon_Custom_Prefix(ActorAsset __instance, ref Sprite __result)
    {
        __result = MclslActorPresentation.Portrait(__instance.id);
        return __result == null;
    }

    private static bool DynamicActorSpriteCreatorUI_GetUnitSpriteForUI_Custom_Prefix(ActorAsset __0, Sprite __1, long __8, ref Sprite __result)
    {
        if (__0?.id is MclslXuanhuangRegistration.Id or "zombie_mclsl_xuanhuang")
        {
            // The authored body already includes its head. Live avatars also
            // retain the prepared nation color; asset previews supply a body.
            Actor actor=__8>0?World.world?.units?.get(__8):null;
            __result=actor!=null?actor.calculateMainSprite():__1;
            return false;
        }
        __result = MclslActorPresentation.Portrait(__0?.id);
        return __result == null;
    }

    private static bool UnitAvatarLoader_LoadItemSprites_Custom_Prefix(UnitAvatarLoader __instance)
    {
        if (MclslActorPresentation.Portrait(__instance.getData()?.asset?.id) == null) return true;
        __instance._show_item = false;
        return false;
    }

    private static void Image_SetNativeSize_Custom_Postfix(UnityEngine.UI.Image __instance)
    {
        Sprite sprite = __instance.sprite;
        if (sprite == null || !sprite.name.StartsWith("mclsl_ui_", StringComparison.Ordinal)) return;
        float scale = 24f / Math.Max(sprite.rect.width, sprite.rect.height);
        __instance.rectTransform.sizeDelta = sprite.rect.size * scale;
        __instance.preserveAspect = true;
    }

    private static void Actor_IsSapient_Beast_Postfix(Actor __instance, ref bool __result)
    {
        if (MclslBeastActorRegistration.IsBeast(__instance))
            __result = MclslBeastActorRegistration.HasYuanyingWisdom(__instance);
    }

    private static void Actor_AttackTargetActions_Animation_Postfix(Actor __instance)
        => MclslBeastAnimation.MarkAttack(__instance);

    private static void Actor_GetHit_Animation_Postfix(Actor __instance, float __0)
    {
        if (__0 > 0f && __instance != null && __instance.isAlive())
            MclslBeastAnimation.MarkHit(__instance);
    }

    private static void ActorManager_CreateBabyActorFromData_Beast_Postfix(Actor __result)
    {
        if (__result?.data == null) return;
        MclslActorPresentation.ReconcileLineage(__result);
        if (!MclslBeastActorRegistration.IsBeast(__result)) return;
        MclslBeastActorRegistration.ReconcileForm(__result);
        MclslBeastActorRegistration.EnsureCultivationSeed(__result);
    }

    private static void ActorManager_PrecalculateRenderData_AnimationTime_Prefix()
        => MclslActorAnimationTime.Sample();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActorManager), "precalculateRenderDataParallel")]
    private static void ActorManager_PrecalculateRenderDataParallel_Halo_Postfix(ActorManager __instance)
    {
        try { MclslVisibleActorRenderLane.Apply(__instance); }
        catch (Exception ex) { UnityEngine.Debug.LogWarning("[模拟长生路] 境界Halo渲染刷新失败: " + ex.Message); }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ActorManager), "precalculateRenderDataNormal")]
    private static void ActorManager_PrecalculateRenderDataNormal_Halo_Postfix(ActorManager __instance)
    {
        try { MclslVisibleActorRenderLane.Apply(__instance); }
        catch (Exception ex) { UnityEngine.Debug.LogWarning("[模拟长生路] 境界Halo渲染刷新失败: " + ex.Message); }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), nameof(Actor.makeStunned))]
    private static bool Actor_MakeStunned_WorldSoul_Prefix(Actor __instance, float pTime = 5f)
    {
        return !MclslWorldSoulActorRegistration.IsWorldSoulActor(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), nameof(Actor.makeSleep))]
    private static bool Actor_MakeSleep_WorldSoul_Prefix(Actor __instance, float pTime)
    {
        return !MclslWorldSoulActorRegistration.IsWorldSoulActor(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), nameof(Actor.calculateForce))]
    private static bool Actor_CalculateForce_WorldSoul_Prefix(Actor __instance,
        float pStartX, float pStartY, float pTargetX, float pTargetY,
        float pForceAmountDirection, float pForceHeight = 0f,
        bool pCheckCancelJobOnLand = false)
    {
        return !MclslWorldSoulActorRegistration.IsWorldSoulActor(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Actor), nameof(Actor.applyRandomForce))]
    private static bool Actor_ApplyRandomForce_WorldSoul_Prefix(Actor __instance,
        float pMinHeight = 1.5f, float pMaxHeight = 2f)
    {
        return !MclslWorldSoulActorRegistration.IsWorldSoulActor(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BaseSimObject), nameof(BaseSimObject.addStatusEffect),
        new Type[] { typeof(StatusAsset), typeof(float), typeof(bool) })]
    private static bool BaseSimObject_AddStatusEffect_WorldSoul_Prefix(BaseSimObject __instance, StatusAsset __0, ref float __1)
    {
        if (__instance == null || !__instance.isActor() || __instance.a == null) return true;
        MclslItemUseSystem.ShortenCommonNegativeStatus(__instance.a, __0, ref __1);
        if (!MclslWorldSoulActorRegistration.IsWorldSoulActor(__instance.a)) return true;
        return __0 == null || !MclslWorldSoulActorRegistration.IsBlockedControlStatus(__0.id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScrollWindow), nameof(ScrollWindow.hide))]
    private static bool ScrollWindow_Hide_RankRightClickGuard_Prefix(ScrollWindow __instance)
    {
        return !MclslRankWindow.ShouldBlockRightClickClose(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScrollWindow), "hideAllEvent")]
    private static bool ScrollWindow_HideAllEvent_RankRightClickGuard_Prefix()
    {
        return !MclslRankWindow.ShouldBlockGlobalRightClickClose();
    }

}
