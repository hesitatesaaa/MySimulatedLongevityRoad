using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal sealed class MclslBattleState
{
    public long Actor,Enemy;
    public int EnemyRealm=-1,Repeats;
    public string Last="";
    public double Since=-1,LastHit,LastQualifiedHit,NextThink,ParryUntil,GuardUntil,ControlUntil,WindowStart,ControlUsed,ProtectedUntil;
    public bool Eligible;
}

internal static class MclslBattleCombat
{
    private const string SaveKey="mclsl.battle.runtime.v1";
    private static readonly Dictionary<long,MclslBattleState> States=new();
    private static readonly List<long> Order=new();
    private static int _cursor;
    private static bool _locked;
    [ThreadStatic] internal static bool ProjectionDamage;
    private static MclslBattleState State(Actor actor)
    {
        long id=MclslActorAccessor.Id(actor);
        if(!States.TryGetValue(id,out var state)){state=new(){Actor=id};States[id]=state;Order.Add(id);}
        return state;
    }
    internal static void Observe(Actor actor)
    {
        if(MclslActorAccessor.Alive(actor)&&MclslActorAccessor.IsCultivator(actor))State(actor);
    }
    internal static void ObserveHit(Actor victim,Actor attacker,float damage)
    {
        if(ProjectionDamage || damage<=0 || !MclslCombatTargets.IsEnemy(attacker,victim))return;
        RegisterBattle(attacker,victim);RegisterBattle(victim,attacker);
    }
    private static void RegisterBattle(Actor actor,Actor enemy)
    {
        if(!MclslActorAccessor.IsCultivator(actor))return;
        var state=State(actor);double now=MclslSpellCasting.Now;
        int realm=MclslRealmIds.Index(MclslActorAccessor.Realm(enemy));
        if(MclslBattleRules.InsightEligible(20,MclslRealmIds.Index(MclslActorAccessor.Realm(actor)),realm,true,false))
        {
            if(state.Since<0)state.Since=now;
            state.LastQualifiedHit=now;state.Eligible=true;state.Enemy=MclslActorAccessor.Id(enemy);state.EnemyRealm=realm;
        }
        state.LastHit=now;
    }
    internal static bool Controlled(Actor actor)=>actor?.data!=null && States.TryGetValue(MclslActorAccessor.Id(actor),out var s)
        && MclslSpellCasting.Now<s.ControlUntil;
    internal static void Control(Actor actor,string status,float duration,bool hard)
    {
        if(!MclslActorAccessor.Alive(actor))return;
        if(!hard){actor.addStatusEffect(status,duration);return;}
        var s=State(actor);double now=MclslSpellCasting.Now;
        double granted=MclslBattleRules.ControlDuration(duration,now,ref s.WindowStart,ref s.ControlUsed,ref s.ProtectedUntil);
        if(granted<=0){actor.addStatusEffect("mclsl_battle_slow",Math.Min(2,duration));return;}
        s.ControlUntil=Math.Max(s.ControlUntil,now+granted);
        actor.stopMovement();actor.addStatusEffect("mclsl_battle_stagger",(float)granted);
        MclslSpellCasting.InterruptWindup(actor);
    }
    internal static void LinkedControl(Actor target,string id)
    {
        switch(id)
        {
            case "S016": Control(target,"mclsl_spell_vine_bind",.4f,true);break;
            case "S008": Control(target,"mclsl_spell_yin_bind",.4f,true);break;
            case "S007": Control(target,"mclsl_spell_shock",.4f,true);break;
            case "S013": Control(target,"mclsl_spell_frost_slow",1,false);break;
            case "S017": Control(target,"mclsl_spell_armor_break",4,false);break;
        }
    }
    internal static void OnCast(Actor actor,string id)
    {
        var s=State(actor);s.Repeats=s.Last==id?Math.Min(4,s.Repeats+1):0;s.Last=id;
    }
    internal static float Score(Actor actor,Actor target,MclslSpellDefinition spell,int mana)
    {
        if(Controlled(actor))return 0;
        var s=State(actor);string id=spell.Id;
        float distance=(target.current_position-actor.current_position).magnitude;
        float health=actor.getHealth()/Math.Max(1,actor.getMaxHealth());
        float score=10+spell.Power*3-Math.Max(0,spell.ManaCost)/(float)Math.Max(1,mana)*4;
        bool threat=MclslSpellCasting.IsWinding(target);
        if(s.Last==id)score-=5+3*s.Repeats;
        if(MclslBattleRules.IsArea(id))
        {
            int crowd=MclslCombatTargets.Lock(actor,target,spell.MaxTargets).Length;
            score+=Math.Min(12,(crowd-1)*4);
            if(crowd<2)score-=4;
        }
        if(id is "C001" or "C005")score+=distance>2&&distance<8?12:-10;
        if(id is "C003" or "C007")score+=distance<4?7:-14;
        if(id=="C002")return threat||distance<2&&health<.8f?score+20:0;
        if(id=="C006")return threat||health<.65f?score+14:0;
        if(id=="C010")return distance<3||health<.5f?score+18:0;
        if(id=="C013")return MclslSpiritPact.Count(actor)<MclslBattleRules.SpiritLimit(MclslRealmIds.Index(MclslActorAccessor.Realm(actor)))?score+30:0;
        if(id=="C014")return MclslSpiritPact.Count(actor)>0?score+5:0;
        if(id=="C015")return MclslSpiritPact.Count(actor)>0&&(threat||health<.7f)?score+22:0;
        if(id=="C016"&&MclslSpiritPact.Count(actor)==0)return 0;
        if(id=="C011"&&MclslSpellSystem.ReserveElementalChain(actor).Length==0)return 0;
        if(MclslBattleRules.IsHardControl(id))score+=threat?14:3;
        if(MclslBattleRules.IsProjectile(id))score+=distance>3?5:0;
        if(MclslBattleRules.IsProfession(id)&&MclslBattleRules.Rank(id)==3)score+=health<.7f?8:-2;
        string main=MclslBattleProfessionSystem.Main(actor);
        if(MclslBattleProfessionSystem.ProfessionForSpell(id)==main)score+=3;
        return Math.Max(.5f,score);
    }
    internal static void ApplySelf(Actor actor,string id)
    {
        var s=State(actor);double now=MclslSpellCasting.Now;
        switch(id)
        {
            case "C002": s.ParryUntil=now+1;break;
            case "C006": s.GuardUntil=now+2.5;break;
            case "C010": s.GuardUntil=now+1.5;Move(actor,0,true);break;
            case "C013": MclslSpiritPact.Summon(actor);break;
            case "C015": MclslSpiritPact.Guard(actor);break;
            default:MclslSpellSystem.ApplySelfEffect(actor,id);break;
        }
        MclslSpellEffect.Show(actor,null,MclslBattleRules.Visual(id),phase:"hit",duration:.4f);
    }
    internal static bool Braced(Actor actor)=>actor?.data!=null&&States.TryGetValue(MclslActorAccessor.Id(actor),out var s)
        &&s.GuardUntil>MclslSpellCasting.Now;
    internal static void Defend(Actor actor,Actor attacker,ref float damage)
    {
        if(damage<=0||!MclslActorAccessor.Alive(actor))return;
        double now=MclslSpellCasting.Now;
        if(States.TryGetValue(MclslActorAccessor.Id(actor),out var s))
        {
            if(s.GuardUntil>now)damage*=.6f;
            if(s.ParryUntil>now && MclslCombatTargets.IsEnemy(actor,attacker))
            {
                s.ParryUntil=0;damage*=.25f;
                MclslSpellEffect.Show(actor,attacker,"S001",phase:"hit",duration:.25f);
                MclslSpellCasting.DealDamage(actor,attacker,Math.Max(1,actor.stats.get("damage"))*.7f,"金");
            }
        }
        MclslSpiritPact.ApplyGuard(actor,ref damage);
        if(damage>=Math.Max(1,actor.getMaxHealth())*.06f&&!Braced(actor))MclslSpellCasting.InterruptWindup(actor);
    }
    internal static void Move(Actor actor,long targetId,bool retreat)
    {
        Actor target=null;
        if(targetId!=0)MclslActorRegistry.ResolveKnownOrWorld(targetId,out target);
        if(target==actor||target==null)target=actor.attack_target as Actor;
        if(!MclslCombatTargets.IsEnemy(actor,target)||actor.current_tile==null)return;
        Vector2 direction=(target.current_position-actor.current_position).normalized*(retreat?-1:1);
        WorldTile destination=null;
        int steps=(int)Math.Min(3,(target.current_position-actor.current_position).magnitude);
        for(int i=1;i<=steps;i++)
        {
            Vector2 point=actor.current_position+direction*i;
            int x=(int)Math.Round(point.x),y=(int)Math.Round(point.y);
            if(x<0||y<0||x>=MapBox.width||y>=MapBox.height)break;
            WorldTile tile=World.world.GetTileSimple(x,y);
            if(tile==null||tile.Type==null||tile.Type.liquid||tile.Type.block)break;
            destination=tile;
        }
        if(destination!=null){actor.stopMovement();actor.setCurrentTilePosition(destination);}
    }
    internal static void Tick()
    {
        if(_locked||World.world==null||Config.paused||Order.Count==0)return;
        double now=MclslSpellCasting.Now;
        int budget=Math.Min(32,Order.Count);
        while(budget-->0 && Order.Count>0)
        {
            if(_cursor>=Order.Count)_cursor=0;
            long id=Order[_cursor++];var s=States[id];
            if(!MclslActorRegistry.ResolveKnownOrWorld(id,out var actor)||!MclslActorAccessor.Alive(actor))
            {States.Remove(id);Order.RemoveAt(--_cursor);continue;}
            if(now<s.NextThink)continue;s.NextThink=now+.2;
            bool fighting=actor.attack_target is Actor target&&MclslCombatTargets.IsEnemy(actor,target);
            if(fighting && !Controlled(actor))MclslSpellSystem.TryCastCombat(actor);
            if(s.Since>=0 && !fighting && now-s.LastHit>=2)
            {
                bool eligible=s.Eligible&&MclslBattleRules.InsightEligible(s.LastQualifiedHit-s.Since,
                    MclslRealmIds.Index(MclslActorAccessor.Realm(actor)),s.EnemyRealm,true,false);
                // Consume the encounter before Learn publishes any callbacks.
                s.Since=-1;s.Eligible=false;
                if(eligible)
                {
                    int serial=MclslActorAccessor.GetInt(actor,"mclsl.battle.v1.encounters",0)+1;
                    MclslActorAccessor.Set(actor,"mclsl.battle.v1.encounters",serial);
                    unchecked
                    {
                        uint hash=(uint)id*2654435761u^(uint)serial*2246822519u;
                        hash^=hash>>16;
                        MclslBattleProfessionSystem.TryBattleInsight(actor,(int)(hash%10000));
                    }
                }
            }
        }
    }
    internal static void Clear(){States.Clear();Order.Clear();_cursor=0;_locked=false;ProjectionDamage=false;}
    internal static void Save()
    {
        if(_locked||World.world?.map_stats==null)return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(States.Values));
    }
    internal static void Load()
    {
        Clear();if(World.world?.map_stats?.custom_data==null)return;
        World.world.map_stats.custom_data.get(SaveKey,out string raw,"");if(raw.Length==0)return;
        try
        {
            var states=JsonConvert.DeserializeObject<List<MclslBattleState>>(raw);
            if(states==null||states.Count>65536)throw new InvalidOperationException("斗法状态数量异常");
            foreach(var s in states)
            {
                if(s==null||States.ContainsKey(s.Actor)||!Finite(s.Since)||!Finite(s.LastHit)||!Finite(s.LastQualifiedHit)||!Finite(s.NextThink)
                    ||!Finite(s.ParryUntil)||!Finite(s.GuardUntil)||!Finite(s.ControlUntil)||!Finite(s.WindowStart)
                    ||!Finite(s.ControlUsed)||!Finite(s.ProtectedUntil)||s.ControlUsed<0||s.ControlUsed>1.201)
                    throw new InvalidOperationException("斗法状态格式错误");
                States.Add(s.Actor,s);Order.Add(s.Actor);
            }
        }
        catch(Exception ex){States.Clear();Order.Clear();_locked=true;MclslDiagnostics.Error("battle-load",ex.Message);}
    }
    private static bool Finite(double v)=>!double.IsNaN(v)&&!double.IsInfinity(v);
}
