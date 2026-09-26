using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Systems.Combat;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslCombatPatches
{
    internal static int ApplySafely(Harmony harmony)
    {
        MethodInfo original = AccessTools.Method(typeof(Actor), "getHit");
        MethodInfo prefix = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_GetHit_RealmSuppression_Prefix));
        MethodInfo postfix = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_GetHit_KillStatistics_Postfix));
        int count = MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.getHit", original, prefix, postfix) ? 1 : 0;
        MethodInfo attack = AccessTools.Method(typeof(Actor), "tryToAttack");
        MethodInfo artifact = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_TryToAttack_Artifact_Postfix));
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.tryToAttack.artifacts", attack, null, artifact)) count++;
        MethodInfo cooldown = AccessTools.Method(typeof(Actor), "getAttackCooldown");
        MethodInfo talisman = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_GetAttackCooldown_Talisman_Postfix));
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.getAttackCooldown.talisman", cooldown, null, talisman)) count++;
        return count;
    }

    private static void Actor_GetAttackCooldown_Talisman_Postfix(Actor __instance, ref float __result)
    {
        if (__instance?.hasStatus("mclsl_item_F004") == true) __result *= 0.88f;
    }

    private static void Actor_TryToAttack_Artifact_Postfix(Actor __instance, BaseSimObject pTarget, bool __result)
    {
        if (__result) MclslArtifactSystem.OnSuccessfulAttack(__instance, pTarget);
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Actor), "getHit")]
    private static void Actor_GetHit_RealmSuppression_Prefix(
        Actor __instance,
        ref float pDamage,
        bool pFlash,
        AttackType pAttackType,
        BaseSimObject pAttacker,
        bool pSkipIfShake,
        bool pMetallicWeapon,
        ref bool pCheckDamageReduction,
        out MclslNativeKillStatisticsSystem.MclslNativeKillAttemptState __state)
    {
        __state = MclslNativeKillStatisticsSystem.CapturePotentialDivertedHit(__instance, pAttacker);
        MclslItemUseSystem.TryAutoLifeSave(__instance, pDamage);
        MclslItemUseSystem.TryAutoCombatConsumables(__instance, pDamage);
        if (__instance.hasStatus("mclsl_item_D011")) pDamage *= 0.10f;
        else if (__instance.hasStatus("mclsl_item_D010")) pDamage *= 0.80f;
        if (__instance.hasStatus("mclsl_item_F001")) pDamage *= 0.92f;
        if (pAttackType == AttackType.Weapon && pAttacker?.a?.hasStatus("mclsl_artifact_ink") == true) pDamage *= 0.75f;
        MclslRealmSuppressionSystem.Apply(ref pDamage, __instance, pAttacker);
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Actor), "getHit")]
    private static void Actor_GetHit_KillStatistics_Postfix(
        Actor __instance,
        MclslNativeKillStatisticsSystem.MclslNativeKillAttemptState __state)
    {
        MclslNativeKillStatisticsSystem.CompletePotentialDivertedHit(__instance, __state);
    }
}
