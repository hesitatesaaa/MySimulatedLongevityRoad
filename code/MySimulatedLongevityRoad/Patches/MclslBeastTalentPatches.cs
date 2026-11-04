using System;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslBeastTalentPatches
{
    internal static int Apply(Harmony harmony)
    {
        int count=0;
        foreach(string name in new[]{"canUseSpells","canUseReligionSpells"})
            if(MclslHarmonyPatchGuard.TryPatch(harmony,"Actor."+name+".beasts",
                AccessTools.Method(typeof(Actor),name,Type.EmptyTypes),
                AccessTools.Method(typeof(MclslBeastTalentPatches),nameof(BeforeNativeSpells)),null))count++;
        if(MclslHarmonyPatchGuard.TryPatch(harmony,"Actor.restoreHealth.beast-talents",
            AccessTools.Method(typeof(Actor),"restoreHealth",new[]{typeof(int)}),
            AccessTools.Method(typeof(MclslBeastTalentPatches),nameof(BeforeHealing)),null))count++;
        // Both overloads flow through the StatusAsset overload. Guarding only the
        // string overload would miss native and career control effects.
        if(MclslHarmonyPatchGuard.TryPatch(harmony,"Actor.addStatusEffect.beast-talents",
            AccessTools.Method(typeof(Actor),"addStatusEffect",new[]{typeof(StatusAsset),typeof(float),typeof(bool)}),
            AccessTools.Method(typeof(MclslBeastTalentPatches),nameof(BeforeStatus)),null))count++;
        return count;
    }
    private static void BeforeHealing(Actor __instance,ref int __0)=>__0=MclslBeastTalentSystem.Healing(__instance,__0);
    private static bool BeforeNativeSpells(Actor __instance,ref bool __result)
    {
        if(!Traits.MclslBeastActorRegistration.IsBeast(__instance))return true;
        __result=false;return false;
    }
    private static bool BeforeStatus(Actor __instance,StatusAsset __0,ref bool __result)
    {
        if(__0==null||MclslBeastTalentSystem.AllowStatus(__instance,__0.id))return true;
        __result=false;return false;
    }
}
