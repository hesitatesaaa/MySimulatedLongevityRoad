using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Only injected into a diagnostic checkout, never shipped.
internal static class MclslBeastDeathProbe
{
    private static readonly string Output=Environment.GetEnvironmentVariable("MCLSL_BEAST_DEATH_PROBE");
    private static readonly List<object> Cases=new();private static readonly List<string> Errors=new();
    private static Actor Killer,Victim;private static int Stage,Index,BloodBefore;private static double MapAt=-1,Started,At;
    private static bool Done;private static string Case;
    private static readonly string[] Names={"recent-hit-then-native-age-death","recent-hit-then-native-editor-none","recent-hit-then-healthy-forced-divine-death","recent-hit-then-environment-fire","real-divine-kill","real-fire-kill","real-weapon-kill","environment-lethal-then-actor-hit-before-commit","physical-hit-followed-by-guaranteed-spell"};
    private static bool SuppressFixtureAttack(Actor __instance,ref bool __result)
    {if(__instance!=Killer&&__instance!=Victim)return true;__result=false;return false;}
    internal static void Tick()
    {
        if(Done||string.IsNullOrEmpty(Output))return;
        try
        {
            Config.paused=Stage!=3;
            if(World.world==null||SmoothLoader.isLoading()||Time.realtimeSinceStartup<90)return;
            if(MapAt<0){Started=Time.realtimeSinceStartup;MapAt=Started;
                new Harmony("mclsl.diagnostic.loot").Patch(AccessTools.Method(typeof(Actor),"tryToAttack"),prefix:new HarmonyMethod(typeof(MclslBeastDeathProbe),nameof(SuppressFixtureAttack)));
                World.world.generateNewMap();Write("fresh-map-requested");return;}
            if(Time.realtimeSinceStartup-MapAt<10)return;
            if(Stage==0){Killer=Spawn("human",0);Stage=1;return;}
            if(Stage==1)
            {
                Killer.setStatsDirty();Killer.updateStats();if(Killer.getMaxHealth()<=0)return;
                Killer.data.age_overgrowth=100;Killer.addTrait("gifts_6");
                MclslCultivationStateTransitions.TrySetCultivationSystem(Killer,MclslCultivationSystemIds.NewLaw);
                MclslCultivationStateTransitions.TrySetRealm(Killer,MclslRealmIds.JinDan,MclslRuntime.CurrentYear(),"loot diagnostic");
                MclslBeastWorld.Enabled=false;Stage=2;
            }
            if(Stage==2)
            {
                if(Index==Names.Length){Done=true;Write("death-probe-completed");Application.Quit();return;}
                if(Victim==null){Victim=Spawn("mclsl_beast_feiqiu",10+Index);At=Time.realtimeSinceStartup;return;}
                Victim.setStatsDirty();Victim.updateStats();if(Victim.getMaxHealth()<=0)return;
                MclslBeastCultivation.SetRealm(Victim,MclslRealmIds.LianQi,MclslRuntime.CurrentYear());
                BloodBefore=MclslBagSystem.Count(Killer,"YM_blood_1");Case=Names[Index];Killer.setAttackTarget(Victim);Victim.setAttackTarget(Killer);
                if(Index<4)
                {
                    MclslSpellSystem.DeliverDamage(Killer,Victim,1,AttackType.Divine);
                    if(Victim.getHealth()<=0)throw new InvalidOperationException("Nonlethal fixture hit became lethal");
                    if(Index==0){Victim.data.age_overgrowth=10000;Victim.die(true,AttackType.Age,false,false);}
                    if(Index==1)Victim.dieAndDestroy(AttackType.None);
                    if(Index==2)Victim.dieAndDestroy(AttackType.Divine);
                    // A consecutive native hit normally respects the brief
                    // shake immunity from the preceding nonlethal hit. This
                    // fixture needs an actual lethal environmental hit.
                    if(Index==3)Victim.getHit(1000000000,true,AttackType.Fire,null,false);
                }
                else if(Index==7)
                {
                    MclslSpellSystem.DeliverDamage(null,Victim,1000000000,AttackType.Divine);
                    if(Victim.getHealth()!=0)throw new InvalidOperationException("Environment fixture was not lethal");
                    MclslSpellSystem.DeliverDamage(Killer,Victim,1,AttackType.Weapon);
                }
                else if(Index==8)
                {
                    Victim.getHit(1,true,AttackType.Weapon,Killer);
                    if(Victim.getHealth()<=0)throw new InvalidOperationException("Physical fixture became lethal");
                    MclslSpellSystem.DeliverDamage(Killer,Victim,1000000000,AttackType.Divine);
                }
                else MclslSpellSystem.DeliverDamage(Killer,Victim,1000000000,Index==4?AttackType.Divine:Index==5?AttackType.Fire:AttackType.Weapon);
                At=Time.realtimeSinceStartup;Stage=3;return;
            }
            if(Stage==3)
            {
                if(Victim.isAlive()&&Time.realtimeSinceStartup-At<5)return;
                int gained=MclslBagSystem.Count(Killer,"YM_blood_1")-BloodBefore;
                bool committed=MclslActorAccessor.GetInt(Victim,"mclsl.beast.loot_committed")==1;
                bool passed=!Victim.isAlive()&&(Index<4||Index==7?gained==0&&!committed:gained==1&&committed);
                Cases.Add(new {scenario=Case,victimId=MclslActorAccessor.Id(Victim),gained,committed,passed,aliveAfter=Victim.isAlive(),healthAfter=Victim.getHealth()});
                if(!passed)Errors.Add("death attribution failed: "+Case);Index++;Victim=null;Stage=2;Write("death-case-inspected");
            }
        }
        catch(Exception ex){Errors.Add(ex.ToString());Done=true;Write("death-probe-failed");Application.Quit();}
    }
    private static Actor Spawn(string id,int offset)
    {
        int n=0;for(int y=30;y<MapBox.height-4;y++)for(int x=30;x<MapBox.width-4;x++)
        {var tile=World.world.GetTileSimple(x,y);if(tile?.Type!=null&&!tile.Type.liquid&&n++>=offset)return World.world.units.createNewUnit(id,tile,false,0f,null,null,true,false,false,false);}
        throw new InvalidOperationException("No legal diagnostic tile");
    }
    private static void Write(string status)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Output));File.WriteAllText(Output,JsonConvert.SerializeObject(new {source="unity-runtime",kind="controlled-death-loot-integration",status,unityVersion=Application.unityVersion,
            capturedAtUtc=DateTime.UtcNow,elapsedSeconds=Time.realtimeSinceStartup-Started,killerId=MclslActorAccessor.Id(Killer),cases=Cases,errors=Errors,gameplayCasesPassed=false,
            fixture="Real native damage and death routes. Age and editor cases force the native death reason immediately after a nonlethal hit; no waiting for natural lifespan exhaustion. Not full AI battle/performance."},Formatting.Indented));
        Debug.Log("[MCLSL loot runtime] "+status);
    }
}
