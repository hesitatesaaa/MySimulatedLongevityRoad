using System;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslLifecyclePatches
{
    internal static int Apply(Harmony harmony)
    {
        int count = 0;
        foreach (var target in new[] { ("setKingdom", typeof(Kingdom)), ("setFamily", typeof(Family)), ("setCity", typeof(City)), ("setSubspecies", typeof(Subspecies)) })
            if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor." + target.Item1 + ".relations",
                AccessTools.Method(typeof(Actor), target.Item1, new[] { target.Item2 }),
                postfix: AccessTools.Method(typeof(MclslLifecyclePatches), nameof(RelationChanged)))) count++;
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.setAsset.halo-qualification",
            AccessTools.Method(typeof(Actor), "setAsset", new[] { typeof(ActorAsset) }),
            postfix: AccessTools.Method(typeof(MclslLifecyclePatches), nameof(RelationChanged)))) count++;
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Subspecies.cacheTags.eligibility",
            AccessTools.Method(typeof(Subspecies), "cacheTags", Type.EmptyTypes),
            postfix: AccessTools.Method(typeof(MclslLifecyclePatches), nameof(SubspeciesChanged)))) count++;
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.findCurrentTile.beast-counts",
            AccessTools.Method(typeof(Actor), "findCurrentTile", new[] { typeof(bool) }),
            postfix: AccessTools.Method(typeof(MclslLifecyclePatches), nameof(TileChanged)))) count++;
        if (MclslHarmonyPatchGuard.TryPatch(harmony, "Actor.setAttackTarget.active-combat",
            AccessTools.Method(typeof(Actor), "setAttackTarget", new[] { typeof(BaseSimObject) }),
            postfix: AccessTools.Method(typeof(MclslLifecyclePatches), nameof(CombatChanged)))) count++;
        return count;
    }
    private static void RelationChanged(Actor __instance, System.Reflection.MethodBase __originalMethod)
    {
        MclslBeastActorRegistration.RestoreWildOwnership(__instance);
        MclslXuanhuangDirectionPatch.ObserveActor(__instance);
        if (__instance?.data == null || !MclslActorRegistry.Resolve(__instance.data.id, out _)) return;
        MclslRuntimeChanges.Publish(__instance, MclslActorChange.NativeMetadata | MclslActorChange.Membership
            | (__originalMethod.Name is "setAsset" or "setSubspecies" ? MclslActorChange.RootPopulation : MclslActorChange.None));
    }
    private static void SubspeciesChanged(Subspecies __instance)
    {
        if (!MclslEligibility.Refresh(__instance)) return;
        foreach (long id in MclslActorRelations.SubspeciesMembers(__instance))
            if (MclslActorRegistry.Resolve(id, out var actor))
                MclslRuntimeChanges.Publish(actor, MclslActorChange.Membership | MclslActorChange.Traits | MclslActorChange.RootPopulation);
    }
    private static void CombatChanged(Actor __instance)
        => MclslRuntimeChanges.PublishTargets(__instance);
    private static void TileChanged(Actor __instance) => MclslBeastPopulationIndex.Moved(__instance);
}
