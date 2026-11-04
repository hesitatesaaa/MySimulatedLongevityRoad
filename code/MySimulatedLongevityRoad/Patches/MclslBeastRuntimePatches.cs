using MySimulatedLongevityRoad.Systems;
using HarmonyLib;

namespace MySimulatedLongevityRoad.Patches;

internal static class MclslBeastRuntimePatches
{
    internal static void Apply(Harmony harmony)
    {
        MclslHarmonyPatchGuard.TryPatch(harmony,"MapBox.clearWorld.habitat",
            AccessTools.Method(typeof(MapBox),"clearWorld",System.Type.EmptyTypes),
            prefix:AccessTools.Method(typeof(MclslBeastSurfaceIndex),nameof(MclslBeastSurfaceIndex.Suspend)));
        // Surface changes are coalesced by the sea/land index.
        MclslHarmonyPatchGuard.TryPatch(harmony,"WorldTile.setTileType.habitat",
            AccessTools.Method(typeof(WorldTile),"setTileType",new[]{typeof(TileType),typeof(bool)}),
            postfix:AccessTools.Method(typeof(MclslBeastRuntimePatches),nameof(TileChanged)));
        MclslHarmonyPatchGuard.TryPatch(harmony,"WorldTile.setTopTileType.habitat",
            AccessTools.Method(typeof(WorldTile),"setTopTileType",new[]{typeof(TopTileType),typeof(bool)}),
            postfix:AccessTools.Method(typeof(MclslBeastRuntimePatches),nameof(TileChanged)));
        harmony.Patch(AccessTools.Method(typeof(BehRestoreStats),"execute",new[]{typeof(Actor)}),
            prefix:new HarmonyMethod(typeof(MclslBeastRuntimePatches),nameof(RestBefore)),postfix:new HarmonyMethod(typeof(MclslBeastRuntimePatches),nameof(RestAfter)));
        foreach(string name in new[]{"regenerationEffect","regenerationEffectClan"})
            harmony.Patch(AccessTools.Method(typeof(ActionLibrary),name),prefix:new HarmonyMethod(typeof(MclslBeastRuntimePatches),nameof(RegenBefore)),postfix:new HarmonyMethod(typeof(MclslBeastRuntimePatches),nameof(RegenAfter)));
    }
    private static void TileChanged(WorldTile __instance)=>MclslBeastSurfaceIndex.Invalidate(__instance);
    private static void RestBefore(Actor __0,out float __state)=>__state=__0?.getHealth()??0;
    private static void RestAfter(Actor __0,float __state)=>AmplifyRecovery(__0,__state);
    private static void RegenBefore(BaseSimObject __0,out float __state)=>__state=__0?.a?.getHealth()??0;
    private static void RegenAfter(BaseSimObject __0,float __state)=>AmplifyRecovery(__0?.a,__state);
    private static void AmplifyRecovery(Actor actor,float before)
    {
        if(!MclslActorAccessor.Alive(actor))return;
        float gained=actor.getHealth()-before,bonus=MclslBeastItemEffects.ArtifactBonus(actor,"health_regen");
        if(gained>0 && bonus>0)actor.restoreHealth((int)System.Math.Round(gained*bonus));
    }
}
