using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslCareerPatches
{
    internal struct HitState { internal Actor Source;internal int Before; }
    internal static int Apply(Harmony harmony)
    {
        MethodInfo original=AccessTools.Method(typeof(Actor),"getHit",new[]{typeof(float),typeof(bool),typeof(AttackType),typeof(BaseSimObject),typeof(bool),typeof(bool),typeof(bool)});
        return MclslHarmonyPatchGuard.TryPatch(harmony,"Actor.getHit.combat-career",original,
            AccessTools.Method(typeof(MclslCareerPatches),nameof(BeforeHit)),
            AccessTools.Method(typeof(MclslCareerPatches),nameof(AfterHit)))?1:0;
    }
    private static void BeforeHit(Actor __instance,BaseSimObject pAttacker,ref float pDamage,out HitState __state)
    {
        __state=new HitState { Source=pAttacker as Actor,Before=__instance.getHealth() };
        if(__state.Source!=null&&MclslCombatTargets.IsEnemy(__state.Source,__instance))
            pDamage=MclslCareerCombat.Incoming(__instance,__state.Source,
                MclslCareerCombat.SummonDamage(__state.Source,pDamage));
    }
    private static void AfterHit(Actor __instance,HitState __state)
    {
        if(__state.Source!=null)
            MclslCareerCombat.EffectiveHit(__state.Source,__instance,__state.Before-__instance.getHealth());
    }
}
