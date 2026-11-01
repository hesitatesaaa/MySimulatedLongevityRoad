using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems;
using MySimulatedLongevityRoad.Systems.Combat;
using Newtonsoft.Json;

internal static class Program
{
    private static int _checks;
    private static void Check(bool value,string name)
    {
        if(!value)throw new Exception("FAIL "+name);
        _checks++;Console.WriteLine("PASS "+name);
    }
    private static void Reject(Action action,string name)
    {
        bool rejected=false;try{action();}catch{rejected=true;}
        Check(rejected,name);
    }
    private static void Main()
    {
        Scheduling();LoadState();Career();Spells();Allocations();
        Console.WriteLine($"ALL {_checks} CHECKS PASSED (offline substitutes; not Unity acceptance)");
    }
    private static void Scheduling()
    {
        var active=new HashSet<long>{1,2,3};var round=new MclslOwnerRound();
        round.Begin(active);
        Check(round.InProgress,"zero budget leaves round pending");
        var visited=new HashSet<long>();
        while(round.InProgress){Check(round.TryTake(active,out long id)&&visited.Add(id),"one slot per opportunity, no repeat");}
        Check(visited.SetEquals(active),"all owners eventually processed");
        round.Begin(active);round.TryTake(active,out long first);
        active.Remove(2);active.Add(4);visited.Clear();visited.Add(first);
        while(round.InProgress){if(round.TryTake(active,out long id))visited.Add(id);}
        Check(!visited.Contains(4),"new owner waits for next round");
        round.Begin(active);visited.Clear();while(round.InProgress){if(round.TryTake(active,out long id))visited.Add(id);}
        Check(visited.SetEquals(active),"removed owner skipped and new owner included next round");
        round.Begin(active);round.Clear();Check(!round.InProgress,"world clear cancels pending cursor");
        active.Clear();round.Begin(active);Check(!round.InProgress,"empty set has no work");
        active.UnionWith(new long[]{1,2,3});round.Clear();
        Check(round.TryBegin(active,1),"talent round starts when interval elapsed");
        round.TryTake(active,out _);
        Check(round.TryBegin(active,1.01),"unfinished talent round resumes before next interval");
        while(round.InProgress)round.TryTake(active,out _);
        round.Complete(1.02,0.1);
        Check(!round.TryBegin(active,1.11)&&round.TryBegin(active,1.12),"completed talent round waits 0.1 seconds");
        round.Clear();Check(round.TryBegin(active,0),"world change resets interval and cursor");
    }
    private static void LoadState()
    {
        var state=new MclslModuleLoadState();Check(!state.CanSave,"unloaded cannot save");
        state.Ready();Check(state.CanSave,"loaded empty module may save");
        MclslWorldContext.Generation++;Check(!state.CanSave,"old world generation cannot save");
        state.Fault();Check(!state.CanSave&&state.Status==MclslModuleLoadStatus.Faulted,"fault cannot save");
        state.Clear();Check(state.Status==MclslModuleLoadStatus.Unloaded,"clear unbinds module");
    }
    private static void Career()
    {
        Reset();MclslCombatCareerSystem.ResetCache();var actor=NewActor(10);
        var state=MclslCombatCareerSystem.Read(actor);
        Check(state.Career==-1&&actor.Data.ContainsKey(MclslCombatCareerCatalog.StateKey),"new career state initialized and saved");
        string[] invalid={"null","{","{\"Version\":999}","{\"Cooldowns\":[]}","{\"Mastery\":null}","{\"SkillExperience\":[0]}","{\"Career\":6}","{\"Rank\":6}","{\"CooldownClock\":\"NaN\"}","{\"Cooldowns\":[\"Infinity\",0,0,0,0]}"};
        foreach(string raw in invalid)
        {
            actor.Data[MclslCombatCareerCatalog.StateKey]=raw;int errors=MclslDiagnostics.Errors;
            var safe=MclslCombatCareerSystem.Read(actor);
            Check(safe.Career==-1&&safe.Rank==0&&MclslCombatCareerSystem.Blocked(actor),"invalid career is disabled: "+raw);
            Check(ReferenceEquals(safe,MclslCombatCareerSystem.Read(actor))&&MclslDiagnostics.Errors==errors+1,"bad raw parsed/logged only once");
            MclslCombatCareerSystem.Write(actor,new MclslCombatCareerState{Career=1,Rank=1});
            Check(actor.Data[MclslCombatCareerCatalog.StateKey]==raw,"failed raw preserved on write");
        }
        actor.Data[MclslCombatCareerCatalog.StateKey]="{\"Career\":1,\"Rank\":2,\"CooldownClock\":10,\"Cooldowns\":[12,0,0,0,0]}";
        MclslBeastLoot.Now=100;state=MclslCombatCareerSystem.Read(actor);
        Check(!MclslCombatCareerSystem.Blocked(actor)&&state.Cooldowns[0]==102,"changed valid raw recovers and rebases once");
        Check(MclslCombatCareerSystem.Read(actor).Cooldowns[0]==102,"cached cooldown is not rebased twice");
        var other=NewActor(11);other.Data[MclslCombatCareerCatalog.StateKey]="null";
        Check(!ReferenceEquals(state,MclslCombatCareerSystem.Read(other)),"disabled states are not shared");
        var battles=new Dictionary<long,MclslCareerBattleState>();var summons=new Dictionary<long,long>();var active=new HashSet<long>();
        void Load(string raw){battles.Clear();summons.Clear();active.Clear();MclslCareerStateValidation.LoadBattles(raw,battles,summons,active,100);}
        Load("[]");Check(battles.Count==0,"empty battle list accepted");
        foreach(string raw in new[]{"null","{","[null]","[{\"Owner\":1,\"Version\":99}]","[{\"Owner\":1,\"Marks\":null}]","[{\"Owner\":1,\"Clock\":\"Infinity\"}]","[{\"Owner\":1},{\"Owner\":1}]","[{\"Owner\":1,\"Summons\":[{\"Unit\":5},{\"Unit\":5}]}]"})Reject(()=>Load(raw),"invalid battle rejected: "+raw);
        Load("[{\"Owner\":1,\"Clock\":90,\"LastAction\":0,\"Zones\":[{\"Tier\":1,\"Expires\":-20}]}]");
        Check(battles[1].Zones[0].Expires==-10&&battles[1].LastAction==0,"expired finite times and idle sentinel preserved");
    }
    private static Actor NewActor(long id)
    {
        var actor=new Actor{Id=id};MclslActorRegistry.Actors[id]=actor;return actor;
    }
    private static void Reset()
    {
        MclslSpellCasting.Clear();MclslWorldContext.Generation++;World.world=new();
        MclslActorRegistry.Actors.Clear();MclslSpellSystem.Hits.Clear();MclslSpellSystem.OnDamage=null;
        MclslBeastLoot.Now=0;MclslFrameDeadline.Expired=false;Config.paused=false;
    }
    private static (Actor,Actor) Pair()
    {
        Reset();MclslSpellCasting.Load();var a=NewActor(1);var b=NewActor(2);a.attack_target=b;b.attack_target=a;return(a,b);
    }
    private static List<MclslPendingSpell> Saved()
    {
        MclslSpellCasting.Save();return JsonConvert.DeserializeObject<List<MclslPendingSpell>>(World.world.map_stats.custom_data.Values[MclslSpellCasting.SaveKey]);
    }
    private static void Spells()
    {
        var(a,b)=Pair();Check(MclslSpellCasting.QueueTalisman(a,"fire",1),"fire talisman queued");
        MclslBeastLoot.Now=0.3;MclslSpellCasting.Tick();var pending=Saved();
        Check(pending.Count==1&&pending[0].RemainingPulses==4&&pending[0].Version==2,"one primary becomes one four-pulse record");
        Check(MclslSpellSystem.Hits.SequenceEqual(new[]{64f}),"primary damage unchanged");
        MclslBeastLoot.Now=1.29;MclslSpellCasting.Tick();Check(MclslSpellSystem.Hits.Count==1,"no early pulse");
        MclslBeastLoot.Now=1.3;MclslSpellCasting.Tick();pending=Saved();Check(pending[0].RemainingPulses==3&&pending[0].Due==2.3,"first pulse advances by one second");
        MclslSpellCasting.Load();MclslBeastLoot.Now=10;MclslSpellCasting.Tick();
        Check(MclslSpellSystem.Hits.SequenceEqual(new[]{64f,4f,4f,4f,4f})&&Saved().Count==0,"reload and overdue catchup preserve remaining hits and total");
        (a,b)=Pair();MclslSpellCasting.QueueTalisman(a,"fire",1);MclslBeastLoot.Now=0.3;MclslSpellCasting.Tick();
        MclslSpellCasting.CleanseDamageOverTime(b);MclslBeastLoot.Now=10;MclslSpellCasting.Tick();Check(Saved().Count==0&&MclslSpellSystem.Hits.Count==1,"cleanse removes all remaining pulses");
        (a,b)=Pair();MclslSpellCasting.QueueTalisman(a,"fire",1);MclslBeastLoot.Now=0.3;MclslSpellCasting.Tick();
        MclslSpellSystem.OnDamage=()=>MclslSpellCasting.CleanseDamageOverTime(b);
        MclslBeastLoot.Now=10;MclslSpellCasting.Tick();MclslSpellSystem.OnDamage=null;
        Check(Saved().Count==0&&MclslSpellSystem.Hits.Count==2,"cleanse inside pulse damage callback cancels remaining pulses");
        (a,b)=Pair();MclslSpellCasting.QueueTalisman(a,"fire",1);MclslBeastLoot.Now=0.3;MclslSpellCasting.Tick();b.Health=0;
        MclslBeastLoot.Now=10;MclslSpellCasting.Tick();Check(Saved().Count==0&&MclslSpellSystem.Hits.Count==1,"dead target cancels remaining pulses");
        (a,b)=Pair();MclslSpellCasting.QueueTalisman(a,"fire",1);MclslFrameDeadline.Expired=true;
        MclslBeastLoot.Now=10;MclslSpellCasting.Tick();Check(MclslSpellSystem.Hits.Count==0,"zero budget does not consume pending work");
        MclslFrameDeadline.Expired=false;MclslSpellCasting.Tick();Check(MclslSpellSystem.Hits.Count==1,"pending work resumes after budget returns");
        Capacity();Preservation();
    }
    private static void Capacity()
    {
        var(a,b)=Pair();var records=new List<MclslPendingSpell>();
        for(int i=0;i<16384;i++)records.Add(new(){Id=i.ToString("D6"),Caster=1,Targets=new(){2},Spell="S004",Law="火",Phase=1,Due=0,Damage=64,Strength=1,Status="mclsl_beast_talisman_fire"});
        World.world.map_stats.custom_data.set(MclslSpellCasting.SaveKey,JsonConvert.SerializeObject(records));MclslSpellCasting.Load();
        Check(!MclslSpellCasting.QueueTalisman(a,"fire",1)&&!MclslSpellCasting.Queue(a,b,new()),"all enqueue paths reject full queue");
        a.Beast=true;a.Data[MclslBeastCultivation.TalentKey]="S004";
        Check(!MclslSpellCasting.QueueTalent(a,b,new()),"beast talent also rejects full queue");a.Beast=false;
        int accepted=0;MclslSpellSystem.OnDamage=()=>{if(MclslSpellCasting.QueueTalisman(a,"fire",1))accepted++;};
        MclslSpellCasting.Tick();MclslSpellSystem.OnDamage=null;var saved=Saved();
        Check(saved.Count==16384&&accepted==0&&MclslSpellSystem.Hits.Count==128,"callback reservation prevents overflow; 128 hit cap retained");
        Check(saved.Count(x=>x.Dot)==128,"fire expansion never multiplies record count");
        MclslSpellCasting.Load();Check(Saved().Count==16384,"maximum-size current save roundtrips");
    }
    private static void Preservation()
    {
        Reset();var data=World.world.map_stats.custom_data;data.set(MclslSpellCasting.SaveKey,"sentinel");
        MclslSpellCasting.Save();Check(data.Values[MclslSpellCasting.SaveKey]=="sentinel","unloaded save preserves original raw");
        foreach(string raw in new[]{"null","{","[null]","[{\"Version\":1}]","[{\"Id\":\"x\",\"Caster\":1,\"Spell\":\"S004\",\"Law\":\"火\",\"Targets\":[2],\"Due\":\"Infinity\"}]"})
        {
            data.set(MclslSpellCasting.SaveKey,raw);MclslSpellCasting.Load();MclslSpellCasting.Save();
            Check(data.Values[MclslSpellCasting.SaveKey]==raw,"faulted queue preserves raw: "+raw);
        }
        data.set(MclslSpellCasting.SaveKey,"[]");MclslSpellCasting.Load();MclslWorldContext.Generation++;
        data.set(MclslSpellCasting.SaveKey,"other-world");MclslSpellCasting.Save();Check(data.Values[MclslSpellCasting.SaveKey]=="other-world","generation mismatch blocks overwrite");
        data.Values.Clear();data.set("mclsl.080.spells.pending.v1","old");MclslSpellCasting.Load();
        Check(Saved().Count==0&&data.Values["mclsl.080.spells.pending.v1"]=="old","only current-format queue is read");
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double ReadSkill(MclslCareerSkill skill)=>skill.Mana+skill.Cooldown+skill.Power;
    private static void Allocations()
    {
        int[] mana={18,30,55,90,150};float[] cooldown={8,12,18,26,40};float[] power={1.05f,1.25f,1.55f,1.8f,2.3f};
        for(int i=0;i<5;i++){var s=MclslCombatCareerCatalog.Skill(0,i+1);Check(s.Mana==mana[i]&&s.Cooldown==cooldown[i]&&s.Power==power[i],"skill constants unchanged tier "+(i+1));}
        Reject(()=>ReadSkill(new(0,0,false,"","")),"invalid skill tier fails explicitly");
        var active=new HashSet<long>(Enumerable.Range(1,1000).Select(x=>(long)x));var round=new MclslOwnerRound();var skill=MclslCombatCareerCatalog.Skill(0,1);double sum=0;
        for(int i=0;i<100;i++){round.Begin(active);while(round.InProgress)round.TryTake(active,out _);sum+=ReadSkill(skill);}
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<1000;i++){round.Begin(active);while(round.InProgress)round.TryTake(active,out _);sum+=ReadSkill(skill);}
        long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
        Check(bytes==0&&sum>0,"warm production round maintenance and skill getters allocate zero bytes (CoreCLR only)");
    }
}
