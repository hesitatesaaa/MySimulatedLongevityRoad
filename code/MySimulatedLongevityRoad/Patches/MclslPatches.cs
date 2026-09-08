using System;
using System.Reflection;
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
        count += TryPatchPair(harmony, "Debug.LogError", typeof(Debug), nameof(Debug.LogError), new[] { typeof(object) }, prefix: nameof(Debug_LogError_FilterMclslMissingText_Prefix));
        count += TryPatchPair(harmony, "Debug.LogWarning", typeof(Debug), nameof(Debug.LogWarning), new[] { typeof(object) }, prefix: nameof(Debug_LogWarning_FilterMclslMissingText_Prefix));
        count += TryPatchPair(harmony, "Subspecies.generateName", typeof(Subspecies), "generateName", Type.EmptyTypes, postfix: nameof(Subspecies_GenerateName_WorldSoul_Postfix));
        count += TryPatchPair(harmony, "Subspecies.getUnitSpriteForBanner", typeof(Subspecies), "getUnitSpriteForBanner", Type.EmptyTypes, prefix: nameof(Subspecies_GetUnitSpriteForBanner_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "MapBox.updateSimulation", typeof(MapBox), "updateSimulation", Type.EmptyTypes, postfix: nameof(MapBox_UpdateSimulation_Postfix));
        count += TryPatchPair(harmony, "MapBox.generateNewMap", typeof(MapBox), nameof(MapBox.generateNewMap), Type.EmptyTypes, prefix: nameof(MapBox_GenerateNewMap_Prefix));
        count += TryPatchPair(harmony, "MapBox.finishingUpLoading", typeof(MapBox), "finishingUpLoading", Type.EmptyTypes, postfix: nameof(MapBox_FinishingUpLoading_Postfix));
        count += TryPatchPair(harmony, "MapBox.clearWorld", typeof(MapBox), "clearWorld", Type.EmptyTypes, postfix: nameof(MapBox_ClearWorld_Postfix));
        count += TryPatchPair(harmony, "SaveManager.currentWorldToSavedMap", typeof(SaveManager), nameof(SaveManager.currentWorldToSavedMap), Type.EmptyTypes, prefix: nameof(SaveManager_CurrentWorldToSavedMap_Prefix));
        count += TryPatchPair(harmony, "SaveManager.loadWorld", typeof(SaveManager), nameof(SaveManager.loadWorld), new[] { typeof(string), typeof(bool) }, prefix: nameof(SaveManager_LoadWorld_Prefix), finalizer: nameof(SaveManager_LoadWorld_Finalizer));
        count += TryPatchPair(harmony, "Actor.addTrait.asset", typeof(Actor), "addTrait", new[] { typeof(ActorTrait), typeof(bool) }, postfix: nameof(Actor_AddTrait_Postfix));
        count += TryPatchPair(harmony, "Actor.addTrait.id", typeof(Actor), "addTrait", new[] { typeof(string), typeof(bool) }, postfix: nameof(Actor_AddTraitById_Postfix));
        count += TryPatchPair(harmony, "Actor.updateAge", typeof(Actor), "updateAge", Type.EmptyTypes, postfix: nameof(Actor_UpdateAge_RegisterPostfix));
        count += TryPatchPair(harmony, "Actor.updateStats.lifespanRepair", typeof(Actor), "updateStats", Type.EmptyTypes, postfix: nameof(Actor_UpdateStats_LifespanRepair_Postfix));
        count += TryPatchPair(harmony, "Actor.removeTrait", typeof(Actor), "removeTrait", new[] { typeof(string) }, postfix: nameof(Actor_RemoveTrait_Postfix));
        count += TryPatchPairOptional(harmony, "ActorTraitsEditor.OnEnable", typeof(ActorTraitsEditor), "OnEnable", Type.EmptyTypes, postfix: nameof(ActorTraitsEditor_OnEnable_Postfix));
        count += TryPatchPair(harmony, "Actor.die", typeof(Actor), "die", new[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) }, prefix: nameof(Actor_Die_Prefix), postfix: nameof(Actor_Die_Postfix));
        count += TryPatchPair(harmony, "UnitWindow.OnEnable", typeof(UnitWindow), "OnEnable", Type.EmptyTypes, postfix: nameof(UnitWindow_OnEnable_Postfix));
        count += TryPatchPair(harmony, "UnitWindow.showStatsRows", typeof(UnitWindow), "showStatsRows", Type.EmptyTypes, postfix: nameof(UnitWindow_ShowStatsRows_Postfix));
        count += TryPatchPair(harmony, "UnitWindow.showInfo", typeof(UnitWindow), "showInfo", Type.EmptyTypes, postfix: nameof(UnitWindow_ShowInfo_Postfix));
        count += TryPatchPair(harmony, "UnitStatsElement.showContent.safe", typeof(UnitStatsElement), "showContent", Type.EmptyTypes, finalizer: nameof(UnitStatsElement_ShowContent_Finalizer));
        count += TryPatchPair(harmony, "Actor.calculateMainSprite", typeof(Actor), "calculateMainSprite", Type.EmptyTypes, prefix: nameof(Actor_CalculateMainSprite_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "ActorManager.precalculateRenderDataParallel", typeof(ActorManager), "precalculateRenderDataParallel", Type.EmptyTypes, postfix: nameof(ActorManager_PrecalculateRenderDataParallel_Halo_Postfix));
        count += TryPatchPair(harmony, "ActorManager.precalculateRenderDataNormal", typeof(ActorManager), "precalculateRenderDataNormal", Type.EmptyTypes, postfix: nameof(ActorManager_PrecalculateRenderDataNormal_Halo_Postfix));
        count += TryPatchPair(harmony, "Actor.makeStunned", typeof(Actor), nameof(Actor.makeStunned), new[] { typeof(float) }, prefix: nameof(Actor_MakeStunned_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.makeSleep", typeof(Actor), nameof(Actor.makeSleep), new[] { typeof(float) }, prefix: nameof(Actor_MakeSleep_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.calculateForce", typeof(Actor), nameof(Actor.calculateForce), new[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) }, prefix: nameof(Actor_CalculateForce_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "Actor.applyRandomForce", typeof(Actor), nameof(Actor.applyRandomForce), new[] { typeof(float), typeof(float) }, prefix: nameof(Actor_ApplyRandomForce_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "BaseSimObject.addStatusEffect", typeof(BaseSimObject), nameof(BaseSimObject.addStatusEffect), new[] { typeof(StatusAsset), typeof(float), typeof(bool) }, prefix: nameof(BaseSimObject_AddStatusEffect_WorldSoul_Prefix));
        count += TryPatchPair(harmony, "ScrollWindow.hide", typeof(ScrollWindow), nameof(ScrollWindow.hide), Type.EmptyTypes, prefix: nameof(ScrollWindow_Hide_RankRightClickGuard_Prefix));
        count += TryPatchPair(harmony, "ScrollWindow.hideAllEvent", typeof(ScrollWindow), "hideAllEvent", Type.EmptyTypes, prefix: nameof(ScrollWindow_HideAllEvent_RankRightClickGuard_Prefix));
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
        MethodInfo exact = AccessTools.Method(targetType, targetName, targetArgs);
        if (exact != null || targetType == null || string.IsNullOrWhiteSpace(targetName)) return exact;

        MethodInfo best = null;
        MethodInfo[] methods = targetType.GetMethods(
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo candidate = methods[i];
            if (!string.Equals(candidate.Name, targetName, StringComparison.Ordinal)) continue;
            if (best == null || candidate.GetParameters().Length < best.GetParameters().Length)
                best = candidate;
        }
        return best;
    }

    private static int TryPatchPairOptional(Harmony harmony, string key, Type targetType, string targetName, Type[] targetArgs, string prefix = null, string postfix = null, string finalizer = null)
    {
        MethodInfo original = ResolveTargetMethod(targetType, targetName, targetArgs);
        if (original == null) return 0;
        MethodInfo prefixMethod = string.IsNullOrWhiteSpace(prefix) ? null : AccessTools.Method(typeof(MclslPatches), prefix);
        MethodInfo postfixMethod = string.IsNullOrWhiteSpace(postfix) ? null : AccessTools.Method(typeof(MclslPatches), postfix);
        MethodInfo finalizerMethod = string.IsNullOrWhiteSpace(finalizer) ? null : AccessTools.Method(typeof(MclslPatches), finalizer);
        return MclslHarmonyPatchGuard.TryPatch(harmony, key, original, prefixMethod, postfixMethod, finalizerMethod) ? 1 : 0;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogError), new Type[] { typeof(object) })]
    private static bool Debug_LogError_FilterMclslMissingText_Prefix(object __0)
    {
        return !MclslLocalizationBridge.ShouldSuppressMissingTextLog(__0);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), new Type[] { typeof(object) })]
    private static bool Debug_LogWarning_FilterMclslMissingText_Prefix(object __0)
    {
        return !MclslLocalizationBridge.ShouldSuppressMissingTextLog(__0);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Subspecies), "generateName")]
    private static void Subspecies_GenerateName_WorldSoul_Postfix(Subspecies __instance)
    {
        MclslWorldSoulActorRegistration.NormalizeSubspecies(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Subspecies), "getUnitSpriteForBanner")]
    private static bool Subspecies_GetUnitSpriteForBanner_WorldSoul_Prefix(Subspecies __instance, ref UnityEngine.Sprite __result)
    {
        if (!MclslWorldSoulActorRegistration.IsWorldSoulSubspecies(__instance)) return true;
        __result = MclslWorldSoulActorRegistration.TryGetBannerSprite();
        return __result == null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapBox), "updateSimulation")]
    private static void MapBox_UpdateSimulation_Postfix()
    {
        MclslRuntime.Tick();
        int frame = Time.frameCount;
        if (MclslHotPathPolicy.CanRunFrameGate("runtime-localization-retry", MclslHotPathPolicy.LocalizationRetryFrames))
        {
            TryPatch("runtime-localization-retry", () => MclslLocalizationBridge.RetryRuntimeKeys());
        }
        if (MclslHotPathPolicy.CanRunFrameGate("trait-editor-era-refresh", MclslHotPathPolicy.TraitEditorGlobalRefreshFrames))
        {
            TryPatch("trait-editor-era-refresh", () => MclslTraitEditorEraFilter.RefreshActiveEditorsThrottled(frame));
        }
        if (MclslHotPathPolicy.CanRunFrameGate("actor-info-refresh", MclslHotPathPolicy.ActorInfoRefreshFrames))
        {
            TryPatch("actor-info-refresh", () => MclslActorInfoPanel.RefreshActiveWindowsThrottled(frame));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapBox), nameof(MapBox.generateNewMap))]
    private static void MapBox_GenerateNewMap_Prefix() => MclslHuanzhenSystem.PrepareForNewWorld();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapBox), "finishingUpLoading")]
    private static void MapBox_FinishingUpLoading_Postfix()
    {
        MclslRuntimeSettings.LoadFromModConfig(MySimulatedLongevityRoad.MclslMod.GetModConfigSafe());
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
    private static void SaveManager_LoadWorld_Prefix(string __0) => MclslHuanzhenSystem.PrepareForAnyWorldLoad(__0);

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
        MclslTraitGrantRouter.HandleAddedTrait(__instance, __0?.id);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "addTrait", new Type[] { typeof(string), typeof(bool) })]
    private static void Actor_AddTraitById_Postfix(Actor __instance, string __0, bool __result)
    {
        if (!__result) return;
        MclslTraitGrantRouter.HandleAddedTrait(__instance, __0);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "updateAge")]
    private static void Actor_UpdateAge_RegisterPostfix(Actor __instance)
    {
        MclslScheduler.RegisterAndEnqueueAnnualActor(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "updateStats")]
    private static void Actor_UpdateStats_LifespanRepair_Postfix(Actor __instance)
    {
        MclslLongevityRules.ApplyRuntimeLifespan(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Actor), "removeTrait", new Type[] { typeof(string) })]
    private static void Actor_RemoveTrait_Postfix(Actor __instance, string __0)
    {
        MclslTraitGrantRouter.HandleRemovedTrait(__instance, __0);
    }

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
        __state = new MclslDeathPatchState(
            MclslDeathSystem.Capture(__instance, __1),
            MclslHuanzhenSystem.CaptureDeath(__instance),
            MclslWorldSoulSystem.CaptureDeath(__instance));
        return true;
    }

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

        MclslDeathEventRouter.CommitActorDeath(__instance, __1, __state);
        long deadActorId = MclslActorAccessor.Id(__instance);
        MclslCultivatorCandidateIndex.Remove(deadActorId);
        MclslWorldActorQuery.MarkDirty();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "OnEnable")]
    private static void UnitWindow_OnEnable_Postfix(UnitWindow __instance)
    {
        TryPatch("unit-window-track-on-enable", () => MclslWorldActorQuery.TrackIfRelevant(__instance?.actor));
        TryPatch("unit-window-gender-on-enable", () => MclslGenderToggleButton.Refresh(__instance));
        TryPatch("unit-window-info-panel-enable", () => MclslActorInfoPanel.Refresh(__instance, resetScrollForNewActor: true));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "showStatsRows")]
    private static void UnitWindow_ShowStatsRows_Postfix(UnitWindow __instance)
    {
        TryPatch("unit-window-track-stats", () => MclslWorldActorQuery.TrackIfRelevant(__instance?.actor));
        TryPatch("unit-window-gender-stats", () => MclslGenderToggleButton.Refresh(__instance));
        TryPatch("unit-window-overview-stats", () => MclslActorOverviewStatsFormatter.Refresh(__instance));
        TryPatch("unit-window-info-panel-stats", () => MclslActorInfoPanel.Refresh(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UnitWindow), "showInfo")]
    private static void UnitWindow_ShowInfo_Postfix(UnitWindow __instance)
    {
        TryPatch("unit-window-track-info", () => MclslWorldActorQuery.TrackIfRelevant(__instance?.actor));
        TryPatch("unit-window-gender-info", () => MclslGenderToggleButton.Refresh(__instance));
        TryPatch("unit-window-overview-info", () => MclslActorOverviewStatsFormatter.Refresh(__instance));
        TryPatch("unit-window-info-panel-info", () => MclslActorInfoPanel.Refresh(__instance));
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(UnitStatsElement), "showContent")]
    private static Exception UnitStatsElement_ShowContent_Finalizer(Exception __exception)
    {
        return __exception is NullReferenceException ? null : __exception;
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
            if (!MclslWorldSoulActorRegistration.TryGetRenderSprite(__instance, out Sprite sprite)) return true;
            __result = sprite;
            MclslWorldSoulActorRegistration.UpdateFrameData(__instance, sprite);
            return false;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning("[模拟长生路] 天地之魄主贴图刷新失败: " + ex.Message);
            return true;
        }
    }

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
    private static bool BaseSimObject_AddStatusEffect_WorldSoul_Prefix(BaseSimObject __instance, StatusAsset __0)
    {
        if (__instance == null || !__instance.isActor() || __instance.a == null) return true;
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
