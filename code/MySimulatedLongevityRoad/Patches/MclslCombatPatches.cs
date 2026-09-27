using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Systems.Combat;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;

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
        MethodInfo attackTalisman = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_TryToAttack_Talisman_Postfix));
        MethodInfo attackGuard = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_TryToAttack_Immortal_Prefix));
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.tryToAttack.consumableTalisman", attack, attackGuard, attackTalisman)) count++;
        MethodInfo cooldown = AccessTools.Method(typeof(Actor), "getAttackCooldown");
        MethodInfo talisman = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_GetAttackCooldown_Talisman_Postfix));
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.getAttackCooldown.talisman", cooldown, null, talisman)) count++;
        MethodInfo force = AccessTools.Method(typeof(Actor), "addForce", new[]
            { typeof(float), typeof(float), typeof(float), typeof(bool), typeof(bool) });
        MethodInfo forceGuard = AccessTools.Method(typeof(MclslCombatPatches), nameof(Actor_AddForce_EarthTalisman_Prefix));
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.addForce.earthTalisman", force, forceGuard, null)) count++;
        return count;
    }

    private static void Actor_AddForce_EarthTalisman_Prefix(Actor __instance,
        ref float __0, ref float __1, ref float __2)
    {
        if (__instance?.hasStatus("mclsl_item_F022") != true) return;
        __0 *= 0.5f;
        __1 *= 0.5f;
        __2 *= 0.5f;
    }

    private static void Actor_GetAttackCooldown_Talisman_Postfix(Actor __instance, ref float __result)
    {
        if (__instance?.hasStatus("mclsl_item_F004") == true) __result *= 0.88f;
    }

    private static void Actor_TryToAttack_Talisman_Postfix(Actor __instance, bool __result)
    {
        if (!__result) return;
        MclslItemUseSystem.TryAutoCombatTalisman(__instance);
        MclslSpellSystem.TryCastCombat(__instance);
    }

    private static bool Actor_TryToAttack_Immortal_Prefix(Actor __instance, ref bool __result)
    {
        if (!MclslImmortalActorRegistration.IsImmortal(__instance)) return true;
        __result = false;
        return false;
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
        bool lifeSaved = MclslItemUseSystem.TryAutoLifeSave(__instance, pDamage);
        MclslItemUseSystem.TryAutoCombatConsumables(__instance, pDamage, lifeSaved);
        if (__instance.hasStatus("mclsl_item_D011")) pDamage *= 0.10f;
        else if (__instance.hasStatus("mclsl_item_D010")) pDamage *= 0.80f;
        else if (__instance.hasStatus("mclsl_item_D018")) pDamage *= 0.50f;
        if (__instance.hasStatus("mclsl_item_F001")) pDamage *= 0.92f;
        if (__instance.hasStatus("mclsl_item_D015") && pAttackType == AttackType.Fire) pDamage *= 0.65f;
        if (__instance.hasStatus("mclsl_item_F023") && MclslSpellSystem.IsApplyingSpellDamage)
        {
            pDamage *= 0.75f;
            __instance.finishStatusEffect("mclsl_item_F023");
        }
        if (__instance.hasStatus("mclsl_item_F025")) pDamage *= 0.85f;
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
