using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Systems.Combat;
using MySimulatedLongevityRoad.Systems.Death;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslCombatPatches
{
    internal static int ApplySafely(Harmony harmony)
    {
        MethodInfo original = AccessTools.Method(typeof(Actor), "getHit");
        MethodInfo prefix = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_GetHit_RealmSuppression_Prefix));
        MethodInfo postfix = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_GetHit_KillStatistics_Postfix));
        return MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.getHit", original, prefix, postfix) ? 1 : 0;
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
