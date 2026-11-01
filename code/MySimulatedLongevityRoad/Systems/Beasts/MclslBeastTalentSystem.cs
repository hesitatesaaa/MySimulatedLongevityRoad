using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslTalentEffectState
{
    public string Kind;
    public long Target;
    public double Due, Expires;
    public float Amount, X, Y;
    public int Pulses;
}
internal sealed class MclslTalentActorState
{
    public long Owner;
    public string Talent;
    public double Ready, Clock, Decision;
    public List<MclslTalentEffectState> Effects=new();
}
internal sealed class MclslTalentWorldState
{
    public int Version=1;
    public List<MclslTalentActorState> Actors=new();
}

// Independent identity, cooldowns and pending work. Only target selection, world
// clock, damage defenses and the generic sprite renderer are shared with spells.
internal static class MclslBeastTalentSystem
{
    internal const string SaveKey="mclsl.080.beast_talents.runtime.v1";
    private static readonly Dictionary<long,MclslTalentActorState> Actors=new();
    private static readonly Dictionary<long,List<MclslTalentEffectState>> OnTarget=new();
    private static readonly Dictionary<MclslTalentEffectState,long> Owners=new();
    private static readonly HashSet<long> ActiveOwners = new();
    private static readonly MclslOwnerRound OwnerRound = new();
    private static MclslModuleLoadState _loadState;
    private static readonly List<MclslTalentEffectState> TickEffects = new();
    private static readonly List<MclslTalentEffectState> BeforeDamageScratch = new();
    private static bool _beforeDamageScanning;
    internal static void Wake(Actor actor)
    { if(actor?.data!=null && Actors.ContainsKey(actor.data.id)) ActiveOwners.Add(actor.data.id); }
    private static int _generation;
    private static bool _locked, _settling;
    internal static double Now=>MclslBeastLoot.Now;
    internal static MclslBeastTalentDefinition Definition(Actor actor)
        =>MclslBeastActorRegistration.IsOrdinary(actor)?MclslBeastTalentCatalog.ForSpecies(MclslBeastCatalog.ForAsset(actor.asset.id)?.Id):null;
    internal static int Level(Actor actor)=>Math.Max(1,MclslRealmIds.Index(MclslActorAccessor.Realm(actor))+1);
    internal static float Strength(Actor actor)=>1+0.05f*(Level(actor)-1);
    private static Actor Resolve(long id)=>MclslActorRegistry.ResolveKnownOrWorld(id,out var actor)?actor:null;
    private static bool Alive(Actor actor)=>MclslActorAccessor.Alive(actor)&&actor.getHealth()>0;
    private static long Id(Actor actor)=>MclslActorAccessor.Id(actor);
    private static bool Enemy(Actor a,Actor b)=>MclslCombatTargets.IsEnemy(a,b)&&Alive(b);
    // Former Nearby(...).Any(a=>a.getHealth()<a.getMaxHealth()*0.85f); avoids the
    // LINQ predicate closure + iterator allocated on every qilin cast decision.
    private static bool AnyInjuredNearby(Actor owner,Vector2 point,float radius)
    {
        Actor[] pool=Nearby(owner,point,radius,false);
        for(int i=0;i<pool.Length;i++){Actor a=pool[i];if(a.getHealth()<a.getMaxHealth()*0.85f)return true;}
        return false;
    }
    internal static void Ensure(Actor actor)
    {
        var definition=Definition(actor);if(definition==null||!Alive(actor)||_locked)return;
        long id=Id(actor);
        if(!Actors.TryGetValue(id,out var state))
        {
            Actors[id]=state=new MclslTalentActorState {Owner=id,Talent=definition.Id,Ready=0,Clock=Now};
        }
        state.Talent=definition.Id;
        if(definition.Species is "qilin" or "qingluan" or "fenghuang" || actor.has_attack_target) ActiveOwners.Add(id);
        if(MclslActorAccessor.GetString(actor,MclslBeastCultivation.TalentKey)!=definition.Id)
            MclslActorAccessor.Set(actor,MclslBeastCultivation.TalentKey,definition.Id);
        int level=Level(actor);
        if(MclslActorAccessor.GetInt(actor,MclslBeastCultivation.TalentLevelKey)!=level)
            MclslActorAccessor.Set(actor,MclslBeastCultivation.TalentLevelKey,level);
    }
    internal static double Cooldown(Actor actor)
    {Ensure(actor);return Actors.TryGetValue(Id(actor),out var state)?Math.Max(0,state.Ready-Now):0;}
    private static IEnumerable<MclslTalentEffectState> TargetEffects(Actor actor)
        =>actor!=null&&OnTarget.TryGetValue(Id(actor),out var list)?list:Array.Empty<MclslTalentEffectState>();
    private static bool Has(Actor actor,string kind)
    {
        if(actor==null || !OnTarget.TryGetValue(Id(actor),out var effects))return false;
        for(int i=0;i<effects.Count;i++)
        {var effect=effects[i];if(effect.Kind==kind && effect.Expires>Now && Alive(Resolve(Owners[effect])))return true;}
        return false;
    }
    private static string Status(string talent,string kind)=>"mclsl_talent_"+talent+"_"+kind;
    internal static void RegisterStatuses()
    {
        foreach(var d in MclslBeastTalentCatalog.All)
        {
            MclslLocalizationBridge.RegisterKey(d.Id,d.Name);
            MclslLocalizationBridge.RegisterKey(d.Id+" Description",d.Description);
            foreach(string kind in new[]{"slow","break","weaken","shield","regen","wind","cleanse","poison","sleep","rage","guard","mark","seal","pressure"})
            {
                var status=new StatusAsset {id=Status(d.Id,kind),duration=8,base_stats=new BaseStats(),path_icon=d.Icon,
                    locale_id=d.Id,locale_description=d.Id+" Description"};
                if(kind is "slow" or "pressure")status.base_stats["multiplier_speed"]=-0.35f;
                if(kind=="sleep")status.base_stats["multiplier_speed"]=-1f;
                if(kind=="break")status.base_stats["armor"]=-20;
                if(kind=="cleanse")status.base_stats["multiplier_speed"]=0.25f;
                AssetManager.status.add(status);
            }
        }
    }
    private static void Index(MclslTalentActorState owner,MclslTalentEffectState effect)
    {
        Owners[effect]=owner.Owner;
        if(effect.Target==0)return;
        if(!OnTarget.TryGetValue(effect.Target,out var list))OnTarget[effect.Target]=list=new();
        list.Add(effect);
    }
    // Allocation-free replacement for Effects.FirstOrDefault(...) on the hot
    // talent-activation path (previously allocated a closure per Add/Activate).
    private static MclslTalentEffectState FindEffect(MclslTalentActorState state,string kind,long targetId)
    {
        var list=state.Effects;
        for(int i=0;i<list.Count;i++)
        {
            var e=list[i];
            if(e.Kind==kind&&e.Target==targetId&&e.Expires>Now)return e;
        }
        return null;
    }
    private static MclslTalentEffectState Add(MclslTalentActorState state,string kind,Actor target,double duration,float amount=0,int pulses=0,Vector2? point=null)
    {
        if(!Actors.TryGetValue(state.Owner,out var current)||!ReferenceEquals(state,current))return null;
        if(target!=null&&kind is "slow" or "sleep" or "pressure")
            if(!AllowStatus(target,Status(state.Talent,kind)))return null;
        // A domain refreshes its own debuff instead of allocating one per pulse.
        var existing=FindEffect(state,kind,target==null?0L:Id(target));
        if(existing!=null&&kind is "weaken" or "pressure")
        {existing.Expires=Now+duration;target.addStatusEffect(Status(state.Talent,kind),(float)duration);return existing;}
        Vector2 p=point??target?.current_position??Vector2.zero;
        var effect=new MclslTalentEffectState {Kind=kind,Target=target==null?0:Id(target),Amount=amount,
            Expires=Now+duration,Due=Now+(pulses>0?duration/pulses:duration),Pulses=pulses,X=p.x,Y=p.y};
        state.Effects.Add(effect);Index(state,effect);ActiveOwners.Add(state.Owner);
        if(target!=null&&kind is "slow" or "break" or "weaken" or "shield" or "regen" or "cleanse" or "poison" or "sleep" or "rage" or "guard" or "mark" or "seal" or "pressure")
            target.addStatusEffect(Status(state.Talent,kind),(float)duration);
        return effect;
    }
    private static void Remove(MclslTalentActorState state,MclslTalentEffectState effect)
    {
        state.Effects.Remove(effect);Owners.Remove(effect);
        if(effect.Target==0)return;
        if(OnTarget.TryGetValue(effect.Target,out var list))
        {
            list.Remove(effect);if(list.Count==0)OnTarget.Remove(effect.Target);
            // Manual replacement for list.Any(predicate): no closure/iterator on the
            // per-removal path. An empty list still counts as "no matching owner"
            // (Any() on empty is false), matching the original status teardown.
            bool sameKindOwner=false;
            for(int i=0;i<list.Count;i++)
            {
                var other=list[i];
                if(other.Kind==effect.Kind&&Owners.TryGetValue(other,out long owner)&&Actors[owner].Talent==state.Talent){sameKindOwner=true;break;}
            }
            if(!sameKindOwner)Resolve(effect.Target)?.finishStatusEffect(Status(state.Talent,effect.Kind));
        }
    }
    // Reusable scan buffers + a cached comparator: the former Nearby built six
    // LINQ iterators (Where×3, GroupBy, Select, OrderBy/ThenBy) plus a ToArray on
    // every talent activation. A single manual pass with id dedup, one in-place
    // sort by (distance, id) and a 16 cap is allocation-light. The returned array
    // is a copy, so a nested scan from a damage callback cannot corrupt it.
    private static readonly List<Actor> NearbyScratch=new();
    private static readonly HashSet<long> NearbySeen=new();
    private static Vector2 _nearbySortPoint;
    private static readonly Comparison<Actor> NearbyComparison=CompareNearby;
    private static int CompareNearby(Actor x,Actor y)
    {
        float dx=(x.current_position-_nearbySortPoint).sqrMagnitude;
        float dy=(y.current_position-_nearbySortPoint).sqrMagnitude;
        int c=dx.CompareTo(dy);
        return c!=0?c:Id(x).CompareTo(Id(y));
    }
    private static Actor[] Nearby(Actor owner,Vector2 point,float radius,bool enemy=true)
    {
        long sample=MclslPerformanceProbe.Begin();
        try { return NearbyCore(owner,point,radius,enemy); }
        finally { MclslPerformanceProbe.End("战斗.兽才目标扫描",sample); }
    }
    private static Actor[] NearbyCore(Actor owner,Vector2 point,float radius,bool enemy)
    {
        WorldTile tile=World.world.GetTile(Mathf.RoundToInt(point.x),Mathf.RoundToInt(point.y));
        if(tile==null)return Array.Empty<Actor>();
        float sq=radius*radius;
        var result=NearbyScratch;result.Clear();
        var seen=NearbySeen;seen.Clear();
        foreach(Actor a in Finder.getUnitsFromChunk(tile,1,radius,false))
        {
            if(a==null||!Alive(a))continue;
            if((a.current_position-point).sqrMagnitude>sq)continue;
            if(enemy){if(!Enemy(owner,a))continue;}
            else if(!(a==owner||MclslBeastActorRegistration.IsBeast(a)&&!Enemy(owner,a)))continue;
            if(!seen.Add(Id(a)))continue;
            result.Add(a);
        }
        if(result.Count>1){_nearbySortPoint=point;result.Sort(NearbyComparison);}
        int take=Math.Min(16,result.Count);
        if(result.Count>take)result.RemoveRange(take,result.Count-take);
        MclslPerformanceProbe.RecordCombatTargetScan(take);
        return result.ToArray();
    }
    private static void Visual(Actor owner,Actor target,string phase,float seconds=0.4f,Vector2? point=null)
    {
        var definition=Definition(owner);if(definition==null)return;
        WorldTile tile=point.HasValue?World.world.GetTile(Mathf.RoundToInt(point.Value.x),Mathf.RoundToInt(point.Value.y)):null;
        MclslSpellEffect.Show(owner,target,definition.Id,tile,phase,seconds);
    }
    private static void Damage(MclslTalentActorState state,Actor owner,Actor target,float amount,bool dot=false)
    {
        if(!Actors.TryGetValue(state.Owner,out var current)||!ReferenceEquals(state,current))return;
        if(!Enemy(owner,target)||amount<=0)return;
        bool old=_settling;_settling=true;
        try {MclslSpellCasting.DealDamage(owner,target,amount,Definition(owner)?.Law??"physical",dot);}
        finally {_settling=old;}
        Visual(owner,target,"hit");
    }
    internal static bool TryCast(Actor actor,Actor target=null)
    {
        if(_locked||Config.paused||!Alive(actor))return false;
        Ensure(actor);var d=Definition(actor);if(d==null||!Actors.TryGetValue(Id(actor),out var state)||Now<state.Ready)return false;
        target??=actor.attack_target as Actor;
        bool combat=Enemy(actor,target);
        if(d.Species=="fenghuang") {if(actor.getHealth()>=actor.getMaxHealth()*0.35f)return false;}
        else if(d.Species=="qingluan") {if(!Controlled(actor))return false;}
        else if(d.Species=="qilin")
        {if(actor.getHealth()>=actor.getMaxHealth()*0.85f
            &&!AnyInjuredNearby(actor,actor.current_position,d.Radius))return false;}
        else if(!combat||Has(actor,"sleep"))return false;
        if(combat&&d.Radius>0&&(target.current_position-actor.current_position).sqrMagnitude>(d.Radius+2)*(d.Radius+2))return false;
        int mana=MclslSpellSystem.CurrentMana(actor);if(mana<d.Mana)return false;
        state.Ready=Now+d.Cooldown;state.Clock=Now;
        MclslActorAccessor.Set(actor,MclslActorDataKeys.ManaCurrent,mana-d.Mana);
        Visual(actor,actor,"cast",0.3f);
        if(d.Species is "qilin" or "qingluan" or "fenghuang" or "xuanwu" or "hongmao")
            Activate(actor,state,actor,d,Math.Max(1,actor.stats.get("damage"))*d.Power*Strength(actor));
        else
        {
            var pending=Add(state,"cast",target,d.Species=="huanglong"?0.7:0.3,
                Math.Max(1,actor.stats.get("damage"))*d.Power*Strength(actor),point:target.current_position);
            pending.Due=pending.Expires;
        }
        return true;
    }
    private static bool ControlStatus(string id)
        =>id!=null&&(id.Contains("slow")||id.Contains("bind")||id.Contains("sleep")||id.Contains("pressure")||id=="frozen"
            ||AssetManager.status.get(id)?.base_stats?.get("multiplier_speed")<0);
    private static bool Controlled(Actor actor)
    {
        // Manual scan instead of getStatusesIds().Any(ControlStatus): same order and
        // short-circuit, without allocating a LINQ predicate/iterator per cast check.
        if(actor.hasAnyStatusEffectRaw())
            foreach(string id in actor.getStatusesIds())if(ControlStatus(id))return true;
        return Has(actor,"slow")||Has(actor,"sleep")||Has(actor,"pressure");
    }
    private static void Cleanse(Actor actor,bool poison)
    {
        string[] ids=poison?new[]{"burning","poisoned","poison","mclsl_beast_talisman_fire"}:
            new[]{"slowness","frozen","mclsl_spell_yin_bind","mclsl_spell_all_bind","mclsl_spell_vine_bind","mclsl_spell_frost_slow","mclsl_career_slow","mclsl_beast_water_slow","mclsl_beast_yin_bind"};
        foreach(string id in ids)actor.finishStatusEffect(id);
        if(!poison&&actor.hasAnyStatusEffectRaw())
            foreach(string id in actor.getStatusesIds().Where(ControlStatus).ToArray())actor.finishStatusEffect(id);
        if(poison)MclslSpellCasting.CleanseDamageOverTime(actor);
        foreach(var e in TargetEffects(actor).ToArray())
            if(poison?e.Kind is "poison" or "burn":e.Kind is "slow" or "sleep" or "pressure")Remove(Actors[Owners[e]],e);
    }
    private static void Push(Actor actor,Vector2 from,float force)
    {Vector2 delta=(actor.current_position-from).normalized;actor.addForce(delta.x*force,delta.y*force,0,false,false);}
    private static void Activate(Actor actor,MclslTalentActorState state,Actor target,MclslBeastTalentDefinition d,float damage,Vector2? fixedPoint=null)
    {
        Vector2 point=fixedPoint??target.current_position;
        Visual(actor,target,"area",0.55f,point);
        switch(d.Species)
        {
            case "kunpeng":
            {
                Vector2 direction=(point-actor.current_position).normalized;
                Actor[] area=Nearby(actor,actor.current_position,d.Radius);int hits=0;
                for(int i=0;i<area.Length&&hits<6;i++)
                {
                    Actor a=area[i];
                    if(Vector2.Dot((a.current_position-actor.current_position).normalized,direction)<0.35f)continue;
                    hits++;
                    Damage(state,actor,a,damage);
                    Push(a,actor.current_position,0.2f+0.7f*(1-Mathf.Clamp01(Vector2.Distance(a.current_position,actor.current_position)/d.Radius)));
                }
            }break;
            case "qinglong":
            {
                Actor previous=target;var used=new HashSet<long>();
                for(int i=0;i<3&&previous!=null;i++)
                {
                    used.Add(Id(previous));Add(state,"chain",previous,0.15+i*0.18,damage*(float)Math.Pow(0.75,i));
                    Actor[] pool=Nearby(actor,previous.current_position,d.Radius);Actor next=null;
                    for(int ci=0;ci<pool.Length;ci++)if(!used.Contains(Id(pool[ci]))){next=pool[ci];break;}
                    previous=next;
                }
            }break;
            case "huanglong":
            {
                Actor[] area=Nearby(actor,point,d.Radius);int hits=0;
                for(int i=0;i<area.Length&&hits<6;i++)
                {
                    Actor a=area[i];hits++;
                    bool inner=Vector2.Distance(a.current_position,point)<d.Radius*0.5f;
                    Damage(state,actor,a,damage*(inner?1:0.6f));if(!inner)Add(state,"slow",a,3);
                }
            }break;
            case "zhuque":
            {
                Vector2 line=(point-actor.current_position).normalized;
                Actor[] area=Nearby(actor,actor.current_position,d.Radius);int hits=0;
                for(int i=0;i<area.Length&&hits<6;i++)
                {
                    Actor a=area[i];
                    if(Vector2.Dot(a.current_position-actor.current_position,line)<0)continue;
                    if(Math.Abs((a.current_position.x-actor.current_position.x)*line.y-(a.current_position.y-actor.current_position.y)*line.x)>=1.7f)continue;
                    hits++;
                    Damage(state,actor,a,damage);Add(state,"burn",a,4,damage*0.125f,4);
                }
            }break;
            case "baihu":
                bool broken=FindEffect(state,"break",Id(target))!=null;
                Damage(state,actor,target,damage*(broken?1.5f:1));Add(state,"break",target,12);break;
            case "qilin":
            {
                actor.restoreHealthPercent(0.12f*Strength(actor));Visual(actor,actor,"hit");
                Actor[] pool=Nearby(actor,actor.current_position,d.Radius,false);
                Actor friend=null;float best=float.MaxValue;
                for(int i=0;i<pool.Length;i++)
                {
                    Actor a=pool[i];
                    if(a==actor||a.getHealth()>=a.getMaxHealth())continue;
                    float frac=(float)a.getHealth()/a.getMaxHealth();
                    if(frac<best){best=frac;friend=a;}
                }
                if(friend!=null){friend.restoreHealthPercent(0.12f*Strength(actor));Visual(actor,friend,"hit");}
            }break;
            case "moqilin":Add(state,"ink-zone",null,6,damage,6,point);break;
            case "xuanwu":Add(state,"shield",actor,8,actor.getMaxHealth()*0.18f*Strength(actor));break;
            case "fenghuang":Cleanse(actor,true);Add(state,"regen",actor,6,actor.getMaxHealth()*0.04f*Strength(actor),6);break;
            case "dafeng":Add(state,"wind-zone",null,4,damage,4,point);break;
            case "manman":
            {
                Damage(state,actor,target,damage);
                Actor[] pool=Nearby(actor,actor.current_position,d.Radius,false);bool kin=false;
                for(int i=0;i<pool.Length;i++){Actor a=pool[i];if(a!=actor&&Definition(a)?.Species=="manman"){kin=true;break;}}
                if(kin)Add(state,"chain",target,0.25,damage);
            }break;
            case "xuanniao":
                Damage(state,actor,target,damage);
                if(Alive(target))MclslActorAccessor.Set(target,MclslActorDataKeys.ManaCurrent,(int)(MclslSpellSystem.CurrentMana(target)*0.85f));break;
            case "qingluan":Cleanse(actor,false);Add(state,"cleanse",actor,4);break;
            case "changliao":
                var poison=FindEffect(state,"poison",Id(target));
                if(poison!=null){float remaining=poison.Amount*poison.Pulses;Remove(state,poison);Damage(state,actor,target,remaining);}
                Damage(state,actor,target,damage);if(Alive(target))Add(state,"poison",target,6,damage*0.15f,6);break;
            case "mo":
                if(!Has(target,"sleep"))
                {if(Add(state,"sleep",target,2)!=null)MclslSpellSystem.RestoreMana(actor,0.08f);}break;
            case "hongmao":actor.data.health=Math.Max(1,(int)(actor.getHealth()*0.95f));Add(state,"rage",actor,6);break;
            case "feiqiu":
            {
                Actor[] area=Nearby(actor,actor.current_position,d.Radius);int hits=0;
                for(int i=0;i<area.Length&&hits<6;i++){Actor a=area[i];hits++;Damage(state,actor,a,damage);Push(a,actor.current_position,0.65f);}
                Add(state,"guard",actor,4);
            }break;
            case "yingying":Damage(state,actor,target,damage);if(Alive(target))Add(state,"mark",target,6);break;
            case "goushou":Damage(state,actor,target,damage);if(Alive(target))Add(state,"seal",target,6);break;
            case "liejiejing":
            {
                Actor[] area=Nearby(actor,point,d.Radius);int hits=0;
                for(int i=0;i<area.Length&&hits<6;i++){hits++;Damage(state,actor,area[i],damage);}
                Add(state,"rift",null,0.8,damage,point:point);
            }break;
            case "tianshou":Add(state,"gravity-zone",null,6,damage,6,actor.current_position);break;
        }
    }
    internal static float BeforeDamage(Actor victim,Actor source,float damage,AttackType type)
    {
        if(damage<=0||!Alive(victim))return damage;
        if(source!=null)
        {
            if(Has(source,"weaken"))damage*=0.75f;
            if(type==AttackType.Weapon&&!MclslSpellSystem.IsApplyingSpellDamage&&Has(source,"rage"))damage*=1.3f;
        }
        if(Has(victim,"guard"))damage*=0.75f;
        // Single reusable snapshot of the victim's effects: the former
        // TargetEffects(victim).ToArray() allocated a fresh array on every hit
        // that carried any effect. Nested scans fall back to a private copy so a
        // re-entrant damage callback cannot corrupt the shared scratch buffer.
        if(!OnTarget.TryGetValue(Id(victim),out var shieldEffects)||shieldEffects.Count==0)return Math.Max(0,damage);
        List<MclslTalentEffectState> scan;
        bool borrowed=!_beforeDamageScanning;
        if(borrowed){_beforeDamageScanning=true;BeforeDamageScratch.Clear();BeforeDamageScratch.AddRange(shieldEffects);scan=BeforeDamageScratch;}
        else scan=new List<MclslTalentEffectState>(shieldEffects);
        try
        {
        foreach(var effect in scan)
        {
            if(effect.Kind!="shield"||effect.Expires<=Now||!Alive(Resolve(Owners[effect])))continue;
            float blocked=Math.Min(damage,effect.Amount);effect.Amount-=blocked;damage-=blocked;
            if(effect.Amount<=0)
            {
                var state=Actors[Owners[effect]];Remove(state,effect);
                Add(state,"shell-burst",victim,0.01,Math.Max(1,victim.stats.get("damage"))*1.1f*Strength(victim));
            }
            if(damage<=0)break;
        }
        }
        finally { if(borrowed)_beforeDamageScanning=false; }
        return Math.Max(0,damage);
    }
    internal static void AfterDamage(Actor victim,Actor source,int actualDamage)
    {
        if(actualDamage<=0)return;
        if(OnTarget.TryGetValue(Id(victim),out var effects)&&effects.Count>0)
        {
            List<MclslTalentEffectState> scan;
            bool borrowed=!_beforeDamageScanning;
            if(borrowed){_beforeDamageScanning=true;BeforeDamageScratch.Clear();BeforeDamageScratch.AddRange(effects);scan=BeforeDamageScratch;}
            else scan=new List<MclslTalentEffectState>(effects);
            try
            {
            foreach(var effect in scan)
            {
                if(effect.Kind=="sleep")Remove(Actors[Owners[effect]],effect);
                else if(effect.Kind=="mark"&&!_settling&&source!=null&&Owners[effect]==Id(source))effect.Amount+=actualDamage*0.25f;
            }
            }
            finally { if(borrowed)_beforeDamageScanning=false; }
        }
        if(Alive(victim)&&Definition(victim)?.Species=="fenghuang")TryCast(victim);
    }
    internal static int Healing(Actor target,int amount)=>Has(target,"seal")?Math.Max(0,(int)Math.Floor(amount*0.5f)):amount;
    internal static bool Sleeping(Actor actor)=>Has(actor,"sleep");
    internal static float AttackInterval(Actor actor)=>Has(actor,"pressure")?1.3f:1;
    internal static bool AllowStatus(Actor actor,string id)
        =>!Has(actor,"cleanse")||!ControlStatus(id);

    internal static void Tick()
    {
        long sample=MclslPerformanceProbe.Begin();
        try { TickCore(); }
        finally { MclslPerformanceProbe.End("战斗.兽才逻辑",sample); }
    }
    private static void TickCore()
    {
        if(_locked||_loadState.Status!=MclslModuleLoadStatus.Ready||World.world==null||Config.paused||MclslFrameDeadline.Expired)return;
        if(!OwnerRound.TryBegin(ActiveOwners,Now))return;
        int generation=_generation;
        while(OwnerRound.InProgress)
        {
            if(generation!=_generation)return;
            // Continue the same snapshot after yielding to the shared deadline.
            if(MclslFrameDeadline.Expired)return;
            if(!OwnerRound.TryTake(ActiveOwners,out long ownerId)||!Actors.TryGetValue(ownerId,out var state))continue;
            Actor actor=Resolve(state.Owner);
            if(!Alive(actor)||Definition(actor)?.Id!=state.Talent)
            {while(state.Effects.Count>0)Remove(state,state.Effects[state.Effects.Count-1]);Actors.Remove(state.Owner);ActiveOwners.Remove(state.Owner);continue;}
            if(Now>=state.Decision){state.Decision=Now+0.5;TryCast(actor);}
            if(generation!=_generation)return;
            if(state.Effects.Count==0 && !actor.has_attack_target
                && Definition(actor)?.Species is not ("qilin" or "qingluan" or "fenghuang"))
            {ActiveOwners.Remove(state.Owner);continue;}
            TickEffects.Clear();TickEffects.AddRange(state.Effects);
            for(int effectIndex=0;effectIndex<TickEffects.Count;effectIndex++)
            {
                var effect=TickEffects[effectIndex];
                if(generation!=_generation)return;
                if(!state.Effects.Contains(effect))continue;
                Actor target=effect.Target==0?null:Resolve(effect.Target);
                if(effect.Target!=0&&(!Alive(target)||target!=actor&&!Enemy(actor,target))) {Remove(state,effect);continue;}
                int duePulses=0;
                while(effect.Pulses>0&&effect.Due<=Now&&duePulses++<6)
                {
                    if(generation!=_generation)return;
                    effect.Pulses--;effect.Due+=1;
                    if(effect.Kind=="regen")target.restoreHealth((int)Math.Round(effect.Amount));
                    else if(effect.Kind is "poison" or "burn")Damage(state,actor,target,effect.Amount,true);
                    else if(effect.Kind.EndsWith("-zone",StringComparison.Ordinal))
                    {
                        Vector2 point=effect.Kind=="gravity-zone"?actor.current_position:new Vector2(effect.X,effect.Y);
                        Visual(actor,actor,"area",0.7f,point);
                        Actor[] areaUnits=Nearby(actor,point,MclslBeastTalentCatalog.ForId(state.Talent).Radius);
                        for(int ai=0;ai<areaUnits.Length&&ai<6;ai++)
                        {
                            Actor a=areaUnits[ai];
                            if(effect.Kind=="ink-zone")Add(state,"weaken",a,1.2);
                            else if(effect.Kind=="gravity-zone")Add(state,"pressure",a,1.2);
                            else if(effect.Kind=="wind-zone")Push(a,point,-0.35f);
                            if(effect.Kind=="wind-zone")Damage(state,actor,a,effect.Amount,true);
                        }
                    }
                }
                if(generation!=_generation)return;
                if(Now<effect.Expires)continue;
                // Remove BEFORE delivering hits so nested damage/save callbacks cannot replay it.
                Remove(state,effect);
                switch(effect.Kind)
                {
                    case "cast":Visual(actor,target,"travel",0.2f);Activate(actor,state,target,MclslBeastTalentCatalog.ForId(state.Talent),effect.Amount,new Vector2(effect.X,effect.Y));break;
                    case "chain":Damage(state,actor,target,effect.Amount);break;
                    case "mark":Damage(state,actor,target,effect.Amount);break;
                    case "shell-burst":
                    {
                        Actor[] area=Nearby(actor,actor.current_position,6);
                        for(int i=0;i<area.Length&&i<6;i++)Damage(state,actor,area[i],effect.Amount);
                    }break;
                    case "rift":
                    {
                        var point=new Vector2(effect.X,effect.Y);Visual(actor,actor,"hit",0.4f,point);
                        Actor[] area=Nearby(actor,point,8);
                        for(int i=0;i<area.Length&&i<6;i++)Damage(state,actor,area[i],effect.Amount);
                    }break;
                }
                if(generation!=_generation)return;
                Visual(actor,target??actor,"cancel",0.25f,effect.Target==0?new Vector2(effect.X,effect.Y):(Vector2?)null);
            }
        }
        OwnerRound.Complete(Now,0.1);
    }
    internal static void Save()
    {
        if(!_loadState.CanSave||_locked||World.world?.map_stats==null)return;
        foreach(var state in Actors.Values)state.Clock=Now;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(new MclslTalentWorldState{Actors=Actors.Values.ToList()}));
    }
    internal static void Load()
    {
        Clear();if(World.world?.map_stats==null)return;
        string raw="";World.world.map_stats?.custom_data?.get(SaveKey,out raw,"");
        if(!string.IsNullOrEmpty(raw))try
        {
            var world=JsonConvert.DeserializeObject<MclslTalentWorldState>(raw);
            if(world?.Version!=1||world.Actors==null||world.Actors.Count>65536)
                throw new InvalidOperationException("Invalid current talent state");
            foreach(var state in world.Actors)
            {
                if(state==null||state.Effects==null||state.Effects.Count>256||double.IsNaN(state.Clock)||double.IsInfinity(state.Clock)
                    ||double.IsNaN(state.Ready)||double.IsInfinity(state.Ready)||MclslBeastTalentCatalog.ForId(state.Talent)==null)
                    throw new InvalidOperationException("本命神通单位状态错误");
                Actor actor=Resolve(state.Owner);if(!Alive(actor)||Definition(actor)?.Id!=state.Talent)continue;
                double shift=Now-state.Clock;state.Ready+=shift;state.Decision=Now;
                if(Actors.ContainsKey(state.Owner))throw new InvalidOperationException("重复神通单位");
                Actors.Add(state.Owner,state);ActiveOwners.Add(state.Owner);
                foreach(var effect in state.Effects)
                {
                    if(effect==null||effect.Kind==null||float.IsNaN(effect.Amount)||float.IsInfinity(effect.Amount)||effect.Amount<0
                        ||double.IsNaN(effect.Expires)||double.IsInfinity(effect.Expires)||double.IsNaN(effect.Due)||double.IsInfinity(effect.Due)
                        ||float.IsNaN(effect.X)||float.IsInfinity(effect.X)||float.IsNaN(effect.Y)||float.IsInfinity(effect.Y)||effect.Pulses<0||effect.Pulses>6)
                        throw new InvalidOperationException("本命神通待结算状态错误");
                    effect.Expires+=shift;effect.Due+=shift;Index(state,effect);
                }
            }
        }
        catch(Exception ex){Clear();_locked=true;_loadState.Fault();MclslDiagnostics.Error("beast-talents-load",ex.ToString());return;}
        // Native status lists may have been rebuilt before module loading. Restore
        // their visual/stat layer from the authoritative, rebased runtime state.
        foreach(var state in Actors.Values)
            foreach(var effect in state.Effects)
                if(effect.Expires>Now&&effect.Target!=0&&effect.Kind is "slow" or "break" or "weaken" or "shield" or "regen" or "cleanse" or "poison" or "sleep" or "rage" or "guard" or "mark" or "seal" or "pressure")
                {
                    Actor target=Resolve(effect.Target);
                    if(Alive(target))target.addStatusEffect(Status(state.Talent,effect.Kind),(float)(effect.Expires-Now));
                }
        _loadState.Ready();
    }
    internal static void Clear()
    {
        _generation++;
        foreach(var state in Actors.Values.ToArray())foreach(var effect in state.Effects.ToArray())Remove(state,effect);
        Actors.Clear();ActiveOwners.Clear();OwnerRound.Clear();TickEffects.Clear();OnTarget.Clear();Owners.Clear();_locked=false;_settling=false;_loadState.Clear();
    }
}
