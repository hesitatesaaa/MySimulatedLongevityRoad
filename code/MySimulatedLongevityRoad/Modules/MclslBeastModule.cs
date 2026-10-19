using MySimulatedLongevityRoad.Systems;
using HarmonyLib;
using System.Linq;
using MySimulatedLongevityRoad.Systems.Combat;

namespace MySimulatedLongevityRoad.Modules;

internal sealed class MclslBeastModule : MclslModuleBase
{
    internal override string Name=>"BeastOverhaul";
    internal override int Order=>65;
    internal override void Init()
    {
        MclslSpellSystem.RegisterStatuses();
        // Publicized native overloads differ between game builds. Patch only
        // instance tile setters; the yearly audit remains the recovery path.
        var harmony=new Harmony("mclsl.beast.habitat");
        harmony.Patch(AccessTools.Method(typeof(Actor),"updateStats",System.Type.EmptyTypes),
            prefix:new HarmonyMethod(typeof(MclslBeastModule),nameof(StatsBefore)));
        foreach(var method in AccessTools.GetDeclaredMethods(typeof(WorldTile)).Where(x=>!x.IsStatic && (x.Name=="setTileType" || x.Name=="setTileTypes" || x.Name=="setType")))
            harmony.Patch(method,postfix:new HarmonyMethod(typeof(MclslBeastModule),nameof(TileChanged)));
        harmony.Patch(AccessTools.Method(typeof(BehRestoreStats),"execute",new[]{typeof(Actor)}),
            prefix:new HarmonyMethod(typeof(MclslBeastModule),nameof(RestBefore)),postfix:new HarmonyMethod(typeof(MclslBeastModule),nameof(RestAfter)));
        foreach(string name in new[]{"regenerationEffect","regenerationEffectClan"})
            harmony.Patch(AccessTools.Method(typeof(ActionLibrary),name),prefix:new HarmonyMethod(typeof(MclslBeastModule),nameof(RegenBefore)),postfix:new HarmonyMethod(typeof(MclslBeastModule),nameof(RegenAfter)));
    }
    private static void TileChanged(WorldTile __instance)=>MclslBeastHabitatIndex.Invalidate(__instance);
    private static void StatsBefore(Actor __instance)=>MclslBeastWorld.ObserveDirtyStats(__instance);
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
    internal override void OnWorldLoaded(int year){MclslBeastWorld.Load();MclslBeastLoot.Load();MclslSpellCasting.Load();MclslBattleCombat.Load();MclslSpiritPact.Load();MclslSpellTerrain.Load();}
    internal override void TickFrame(int frameCounter){MclslBeastWorld.Tick();MclslBeastLoot.Tick();MclslBattleCombat.Tick();MclslSpellCasting.Tick();MclslSpiritPact.Tick();MclslBattleVisuals.Tick();MclslSpellTerrain.Tick();}
    internal override void PrepareForSave(){MclslBeastWorld.Save();MclslBeastLoot.Save();MclslSpellCasting.Save();MclslBattleCombat.Save();MclslSpiritPact.Save();MclslSpellTerrain.Save();}
    internal override void Clear(){MclslBeastWorld.Clear();MclslBeastLoot.Clear();MclslSpellCasting.Clear();MclslSpiritPact.Clear();MclslBattleVisuals.Clear();MclslBattleCombat.Clear();MclslSpellTerrain.Clear();}
}
