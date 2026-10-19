using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

// Only injected into an isolated diagnostic checkout, never shipped.
internal static class MclslBeastSpellProbe
{
    private static readonly string Output=Environment.GetEnvironmentVariable("MCLSL_BEAST_SPELL_PROBE");
    private static readonly string[] Ids={"S001","S004","S007","S008","S009","S010","S012","S013","S015","S016","S017","S019","S021","S022","S023","S026"};
    private static readonly List<object> Cases=new(),Visuals=new();
    private static readonly List<object> CameraContexts=new();
    private static readonly List<Receipt> Hits=new();
    private static readonly List<string> Errors=new();
    private static readonly HashSet<long> FixtureIds=new();
    private static readonly HashSet<string> Sampled=new();
    private static Actor Caster,Main,Friend;
    private static Actor[] Enemies;
    private static int Stage,Index,Mode,ManaBefore,ManaAfter,HitStart,HealthBefore;
    private static double MapAt=-1,Started,RealAt,GameAt;
    private static bool Done,Teleported,PauseChecked;
    private static MclslPendingSpell Cast;
    private static string ActiveCase,SavePath,ExpectedPending;
    private static WorldTimeScaleAsset PreviousSpeed;
    private static long SavedCaster,SavedMain;
    private static readonly BindingFlags Flags=BindingFlags.Static|BindingFlags.Instance|BindingFlags.NonPublic;
    private static List<MclslPendingSpell> Pending=>(List<MclslPendingSpell>)typeof(MclslSpellCasting).GetField("Pending",Flags).GetValue(null);
    internal sealed class Receipt
    {
        public string Case,Law;public long Caster,Target;public bool Dot;
        public float Requested;public int Before,After;public double GameTime;
    }
    internal static Receipt BeginHit(Actor caster,Actor target,float damage)
    {
        if(string.IsNullOrEmpty(Output)||Done||caster!=Caster||MclslSpellCasting.DamageLaw==null)return null;
        return new Receipt {Case=ActiveCase,Caster=MclslActorAccessor.Id(caster),Target=MclslActorAccessor.Id(target),Law=MclslSpellCasting.DamageLaw,
            Dot=MclslSpellCasting.DotDamage,Requested=damage,Before=target.getHealth(),GameTime=MclslSpellCasting.Now};
    }
    internal static void EndHit(Receipt receipt,Actor target)
    {if(receipt==null)return;receipt.After=target.getHealth();Hits.Add(receipt);}
    private static bool SuppressFixtureAttack(Actor __instance,ref bool __result)
    {if(!FixtureIds.Contains(MclslActorAccessor.Id(__instance)))return true;__result=false;return false;}
    internal static void Tick()
    {
        if(Done||string.IsNullOrEmpty(Output))return;
        try
        {
            // Loading finishes by unpausing the native world. Hold it before
            // the post-load observation delay so saved casts cannot resolve
            // before the diagnostic restores its actor references.
            Config.paused=Stage!=5&&Stage!=8&&(Stage!=3||Index==0&&!PauseChecked);
            if(World.world==null||SmoothLoader.isLoading()||Time.realtimeSinceStartup<90)return;
            if(MapAt<0)
            {
                Started=Time.realtimeSinceStartup;MapAt=Started;Config.paused=true;
                new Harmony("mclsl.diagnostic.spells").Patch(AccessTools.Method(typeof(Actor),"tryToAttack"),
                    prefix:new HarmonyMethod(typeof(MclslBeastSpellProbe),nameof(SuppressFixtureAttack)));
                World.world.generateNewMap();Write("fresh-map-requested");return;
            }
            if(Time.realtimeSinceStartup-MapAt<10)return;
            if(Stage==0)
            {
                Config.paused=true;Caster=Spawn("human",0);Main=Spawn("mclsl_beast_feiqiu",2);Friend=Spawn("human",3);
                Enemies=Enumerable.Range(4,7).Select(x=>Spawn("mclsl_beast_feiqiu",2)).ToArray();Stage=1;return;
            }
            if(Stage==1)
            {
                foreach(var actor in new[]{Caster,Main,Friend}.Concat(Enemies)){actor.setStatsDirty();actor.updateStats();if(actor.getMaxHealth()<=0)return;}
                Caster.data.age_overgrowth=1800;Caster.addTrait("gifts_6");
                MclslCultivationStateTransitions.TrySetCultivationSystem(Caster,MclslCultivationSystemIds.NewLaw);
                MclslCultivationStateTransitions.TrySetRealm(Caster,MclslRealmIds.ChangSheng,MclslRuntime.CurrentYear(),"spell diagnostic");
                foreach(var beast in Enemies.Prepend(Main))MclslBeastCultivation.SetRealm(beast,MclslRealmIds.ChangSheng,MclslRuntime.CurrentYear());
                MclslBeastWorld.Enabled=false;Focus(false);Stage=2;
                if(Environment.GetEnvironmentVariable("MCLSL_BEAST_SPELL_PROBE_FOCUS")=="edges"){Mode=1;Stage=4;}
                CameraContext("fixture-ready");Write("spell-fixture-ready");return;
            }
            if(Stage==2)
            {
                Config.paused=true;
                if(Index==Ids.Length){Index=0;Mode=1;Stage=4;return;}
                BeginCast(Ids[Index],Ids[Index]);Stage=3;return;
            }
            if(Stage==3)
            {
                SampleVisuals();
                if(Index==0&&!PauseChecked)
                {
                    Config.paused=true;if(Time.realtimeSinceStartup-RealAt<0.75)return;
                    bool stable=MclslSpellCasting.Now==GameAt&&Hits.Count==HitStart&&Main.getHealth()==HealthBefore&&Pending.Contains(Cast);
                    Cases.Add(new {scenario="pause",stable});if(!stable)Errors.Add("pause advanced pending spell");PauseChecked=true;
                    var tile=LegalTile(20);Main.setCurrentTilePosition(tile);Teleported=true;RealAt=Time.realtimeSinceStartup;
                }
                Config.paused=false;
                if(Pending.Contains(Cast)){if(Time.realtimeSinceStartup-RealAt>12)throw new TimeoutException("Spell failed to reach hit: "+ActiveCase);return;}
                var direct=Hits.Skip(HitStart).Where(x=>!x.Dot).ToArray();
                bool single=direct.Length==Cast.Targets.Count&&direct.Select(x=>x.Target).Distinct().Count()==direct.Length;
                bool mainHit=direct.Count(x=>x.Target==MclslActorAccessor.Id(Main))==1;
                bool mana=ManaBefore-ManaAfter==Spell(Ids[Index]).ManaCost;
                Cases.Add(new {id=Ids[Index],queued=true,manaSpent=ManaBefore-ManaAfter,manaCorrect=mana,lockedTargets=Cast.Targets,
                    mainFirst=Cast.Targets.First()==MclslActorAccessor.Id(Main),friendExcluded=!Cast.Targets.Contains(MclslActorAccessor.Id(Friend)),singleHit=single,mainHit,
                    targetLimit=Spell(Ids[Index]).MaxTargets,actualHitTime=direct.FirstOrDefault()?.GameTime,scheduledHitTime=Cast.Due,teleported=Index==0&&Teleported});
                if(!single||!mainHit||!mana)Errors.Add("spell hit or mana check failed: "+Ids[Index]);
                Index++;Stage=2;Write("spell-"+Ids[Index-1]+"-inspected");return;
            }
            if(Stage==4)
            {
                Config.paused=true;
                if(Mode==1){Focus(true);BeginQueued("offscreen","S001");Stage=5;return;}
                if(Mode==2)
                {
                    Focus(false);MclslSpellEffect.ClearRuntime();
                    for(int i=0;i<96;i++)MclslSpellEffect.Show(Caster,Main,"S001",phase:"cast",duration:10);
                    int active=(int)typeof(MclslSpellEffect).GetField("_activeCount",Flags).GetValue(null);
                    Cases.Add(new {scenario="pool-cap-precondition",active,filled=active==96});
                    if(active!=96)Errors.Add("pool cap fixture did not fill 96 objects");BeginQueued("pool-cap","S013");Stage=5;return;
                }
                if(Mode==3){MclslSpellEffect.SetEnabled(false);BeginQueued("effects-disabled","S001");Stage=5;return;}
                if(Mode==4)
                {
                    BeginQueued("became-friendly","S001");
                    if(Main.kingdom==null)throw new InvalidOperationException("Friendly fixture has no native kingdom");
                    Caster.setKingdom(Main.kingdom);Stage=5;return;
                }
                if(Mode==5){Caster=Spawn("human",25);Stage=60;return;}
                if(Mode==6){Caster=Spawn("human",27);Stage=61;return;}
                if(Mode==7)
                {
                    Caster=Spawn("human",30);Stage=6;return;
                }
            }
            if(Stage==60||Stage==61)
            {
                Caster.setStatsDirty();Caster.updateStats();if(Caster.getMaxHealth()<=0)return;
                Caster.data.age_overgrowth=1800;Caster.addTrait("gifts_6");
                MclslCultivationStateTransitions.TrySetCultivationSystem(Caster,MclslCultivationSystemIds.NewLaw);
                MclslCultivationStateTransitions.TrySetRealm(Caster,MclslRealmIds.ChangSheng,MclslRuntime.CurrentYear(),"cancellation diagnostic");
                if(Stage==60){BeginQueued("caster-death","S001");Caster.die(true,AttackType.None,false,false);}
                else
                {
                    PreviousSpeed=Config.time_scale_asset;
                    var faster=AssetManager.time_scales.list.First(x=>!x.sonic&&!x.render_skip&&x.multiplier>=2);
                    Config.setWorldSpeed(faster,false);BeginQueued("accelerated","S001");
                }
                Stage=5;return;
            }
            if(Stage==5)
            {
                Config.paused=false;SampleVisuals();
                if(Pending.Contains(Cast)){if(Time.realtimeSinceStartup-RealAt>12)throw new TimeoutException(ActiveCase);return;}
                int directHits=Hits.Skip(HitStart).Count(x=>!x.Dot);
                bool passed=Mode==5?directHits==0&&!Caster.isAlive():Mode==4?directHits==0&&!MclslCombatTargets.IsEnemy(Caster,Main):directHits==Cast.Targets.Count;
                if(Mode==6)passed=passed&&Config.time_scale_asset.multiplier>=2&&Hits.Skip(HitStart).All(x=>x.GameTime>=Cast.Due);
                Cases.Add(new {scenario=ActiveCase,passed,directHits,lockedTargets=Cast.Targets.Count,visualsEnabled=MclslSpellEffect.Enabled,
                    speedMultiplier=Config.time_scale_asset?.multiplier,gameElapsed=MclslSpellCasting.Now-GameAt,realElapsed=Time.realtimeSinceStartup-RealAt});if(!passed)Errors.Add("special case failed: "+ActiveCase);
                if(Mode==3)MclslSpellEffect.SetEnabled(true);
                if(Mode==6)Config.setWorldSpeed(PreviousSpeed,false);
                Mode++;Stage=4;Write("special-case-inspected");return;
            }
            if(Stage==6)
            {
                Config.paused=true;Caster.setStatsDirty();Caster.updateStats();if(Caster.getMaxHealth()<=0)return;
                Caster.data.age_overgrowth=1800;Caster.addTrait("gifts_6");
                MclslCultivationStateTransitions.TrySetCultivationSystem(Caster,MclslCultivationSystemIds.NewLaw);
                MclslCultivationStateTransitions.TrySetRealm(Caster,MclslRealmIds.ChangSheng,MclslRuntime.CurrentYear(),"spell save diagnostic");
                Focus(false);BeginCast("S017","pending-disk-save");
                SavedCaster=MclslActorAccessor.Id(Caster);SavedMain=MclslActorAccessor.Id(Main);
                ExpectedPending=JsonConvert.SerializeObject(Cast);SavePath=Path.Combine(Path.GetDirectoryName(Output),Path.GetFileNameWithoutExtension(Output)+"-save");
                Directory.CreateDirectory(SavePath);SaveManager.saveWorldToDirectory(SavePath,true,true);
                Stage=7;MapAt=Time.realtimeSinceStartup;Write("pending-spell-saved");World.world.save_manager.loadWorld(SavePath,false);return;
            }
            if(Stage==7)
            {
                Config.paused=true;MclslActorRegistry.ResolveKnownOrWorld(SavedCaster,out Caster);MclslActorRegistry.ResolveKnownOrWorld(SavedMain,out Main);
                var restored=Pending.SingleOrDefault(x=>x.Id==Cast.Id);
                bool equal=restored!=null&&JsonConvert.SerializeObject(restored)==ExpectedPending;
                bool mana=MclslActorAccessor.GetInt(Caster,MclslActorDataKeys.ManaCurrent)==ManaAfter;
                Cases.Add(new {scenario="pending-disk-save-restored",equal,manaUnchanged=mana,pendingId=Cast.Id,
                    expected=ExpectedPending,restored=restored==null?null:JsonConvert.SerializeObject(restored),expectedMana=ManaAfter,actualMana=MclslActorAccessor.GetInt(Caster,MclslActorDataKeys.ManaCurrent)});
                if(!equal||!mana)Errors.Add("pending spell disk restoration failed");Cast=restored;HitStart=Hits.Count;RealAt=Time.realtimeSinceStartup;Stage=8;return;
            }
            if(Stage==8)
            {
                Config.paused=false;SampleVisuals();
                if(Cast!=null&&Pending.Contains(Cast)){if(Time.realtimeSinceStartup-RealAt>12)throw new TimeoutException("restored spell never hit");return;}
                bool once=Hits.Skip(HitStart).Count(x=>!x.Dot&&x.Target==SavedMain)==1;
                Cases.Add(new {scenario="pending-disk-save-resumed",once});if(!once)Errors.Add("restored spell repeated or missed hit");
                Done=true;Write("spell-probe-completed");Application.Quit();
            }
        }
        catch(Exception ex){Errors.Add(ex.ToString());Done=true;Write("spell-probe-failed");Application.Quit();}
    }
    private static MclslSpellDefinition Spell(string id){if(!MclslSpellSystem.TryGet(id,out var spell))throw new InvalidOperationException(id);return spell;}
    private static void Prepare(string label,bool clearVisuals=true)
    {
        ActiveCase=label;if(clearVisuals)MclslSpellEffect.ClearRuntime();Main.setCurrentTilePosition(LegalTile(2));Caster.setAttackTarget(Main);Main.setAttackTarget(Caster);
        foreach(var actor in Enemies.Prepend(Main)){actor.restoreHealthPercent(1);MclslItemUseSystem.TrySetStat(actor.stats,"armor",0);}
        MclslItemUseSystem.TrySetStat(Caster.stats,"damage",10);Caster.restoreHealthPercent(1);
        MclslActorAccessor.Set(Caster,MclslActorDataKeys.ManaInitialized,1);MclslActorAccessor.Set(Caster,MclslActorDataKeys.ManaCurrent,MclslSpellSystem.MaxMana(Caster));
        ManaBefore=MclslSpellSystem.CurrentMana(Caster);HitStart=Hits.Count;HealthBefore=Main.getHealth();RealAt=Time.realtimeSinceStartup;GameAt=MclslSpellCasting.Now;
    }
    private static void BeginCast(string id,string label)
    {
        Prepare(label);MclslActorAccessor.Set(Caster,MclslActorDataKeys.LearnedSpells,"v1:"+id);MclslSpellSystem.TryCastCombat(Caster);
        ManaAfter=MclslActorAccessor.GetInt(Caster,MclslActorDataKeys.ManaCurrent);
        var queued=Pending.Where(x=>x.Caster==MclslActorAccessor.Id(Caster)&&x.Spell==id&&!x.Dot).ToArray();
        if(queued.Length!=1)throw new InvalidOperationException("Expected one queued cast, got "+queued.Length+": "+id);
        Cast=queued[0];CameraContext(label);SampleVisuals();
    }
    private static void BeginQueued(string label,string id)
    {
        Prepare(label,label!="pool-cap");if(!MclslSpellCasting.Queue(Caster,Main,Spell(id)))throw new InvalidOperationException("Queue failed: "+label);
        Cast=Pending.Last(x=>x.Caster==MclslActorAccessor.Id(Caster)&&x.Spell==id&&!x.Dot);SampleVisuals();
    }
    private static Actor Spawn(string id,int offset)
    {
        var actor=World.world.units.createNewUnit(id,LegalTile(offset),false,0f,null,null,true,false,false,false);
        FixtureIds.Add(MclslActorAccessor.Id(actor));return actor;
    }
    private static WorldTile LegalTile(int offset)
    {
        int n=0;for(int y=40;y<MapBox.height-4;y++)for(int x=40;x<MapBox.width-4;x++)
        {var tile=World.world.GetTileSimple(x,y);if(tile?.Type!=null&&!tile.Type.liquid&&n++>=offset)return tile;}
        throw new InvalidOperationException("No legal fixture tile");
    }
    private static void Focus(bool away)
    {
        Vector3 position=Caster.current_position;position.x+=away?1000:3;
        if(Camera.main!=null){position.z=Camera.main.transform.position.z;Camera.main.transform.position=position;}
        MoveCamera.instance?.focusOn(position);MoveCamera.instance?.forceZoom(20);
    }
    private static void CameraContext(string label)
    {
        var camera=Camera.main;
        CameraContexts.Add(new {label,camera=camera?.name,position=camera?.transform.position.ToString("F3"),rotation=camera?.transform.eulerAngles.ToString("F3"),
            orthographic=camera?.orthographic,orthographicSize=camera?.orthographicSize,near=camera?.nearClipPlane,far=camera?.farClipPlane,
            caster=Caster.current_position.ToString("F3"),target=Main.current_position.ToString("F3"),casterViewport=camera?.WorldToViewportPoint(Caster.current_position).ToString("F3"),
            targetViewport=camera?.WorldToViewportPoint(Main.current_position).ToString("F3")});
    }
    private static void SampleVisuals()
    {
        foreach(var effect in Resources.FindObjectsOfTypeAll<MclslSpellEffect>())
        {
            if(!effect.gameObject.activeSelf)continue;
            string id=(string)typeof(MclslSpellEffect).GetField("_spellId",Flags).GetValue(effect),phase=(string)typeof(MclslSpellEffect).GetField("_phase",Flags).GetValue(effect);
            if(!Sampled.Add(ActiveCase+"/"+id+"/"+phase))continue;
            var renderer=effect.GetComponent<SpriteRenderer>();
            Visuals.Add(new {scenario=ActiveCase,id,phase,sprite=renderer?.sprite?.name,position=effect.transform.position.ToString("F3"),rotation=effect.transform.eulerAngles.ToString("F3"),scale=effect.transform.localScale.ToString("F3"),caster=Caster.current_position.ToString("F3"),target=Main.current_position.ToString("F3")});
        }
    }
    private static void Write(string status)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Output));
        File.WriteAllText(Output,JsonConvert.SerializeObject(new {source="unity-runtime",kind="controlled-spell-integration",status,unityVersion=Application.unityVersion,
            graphicsDevice=SystemInfo.graphicsDeviceName,capturedAtUtc=DateTime.UtcNow,elapsedSeconds=Time.realtimeSinceStartup-Started,
            placedActorIds=FixtureIds,cases=Cases,damageReceipts=Hits,visualSamples=Visuals,cameraContexts=CameraContexts,errors=Errors,gameplayCasesPassed=false,
            fixture="Native attacks suppressed only for diagnostic actors; production TryCastCombat and getHit run in actual Unity. Caster damage set to 10 to retain targets. Special queue cases test cancellation/render independence. Not full AI battle, performance or visual certification."},Formatting.Indented));
        Debug.Log("[MCLSL spell runtime] "+status);
    }
}
