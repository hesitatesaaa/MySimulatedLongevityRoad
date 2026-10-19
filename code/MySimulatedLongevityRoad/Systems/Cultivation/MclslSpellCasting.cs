using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslPendingSpell
{
    public string Id,Spell,Law,Status;
    public long Caster;
    public List<long> Targets=new();
    public float Damage,Strength,StatusDuration;
    public double Born,TravelAt,Due;
    public int Phase,Version=1;
    public bool Dot,Terrain;
}
internal static class MclslCombatTargets
{
    internal static bool IsEnemy(Actor caster,Actor target)
    {
        if(!MclslActorAccessor.Alive(caster)||!MclslActorAccessor.Alive(target)||caster==target)return false;
        if(MclslBeastActorRegistration.IsOrdinary(caster)&&MclslBeastActorRegistration.IsOrdinary(target))return false;
        if(caster.kingdom!=null&&target.kingdom!=null)return caster.kingdom.isEnemy(target.kingdom);
        return caster.attack_target==target || target.attack_target==caster;
    }
    internal static Actor[] Lock(Actor caster,Actor main,int maximum)
    {
        if(!IsEnemy(caster,main))return Array.Empty<Actor>();
        var result=new List<Actor>{main};
        if(maximum>1 && main.current_tile!=null)
            result.AddRange(Finder.getUnitsFromChunk(main.current_tile,1,3f,false).Where(x=>x!=main&&IsEnemy(caster,x))
                .GroupBy(MclslActorAccessor.Id).Select(x=>x.First()).OrderBy(x=>(x.current_position-main.current_position).sqrMagnitude)
                .ThenBy(MclslActorAccessor.Id).Take(maximum-1));
        return result.ToArray();
    }
}
internal static class MclslSpellCasting
{
    private const string SaveKey="mclsl.spells.pending.v1";
    private static readonly List<MclslPendingSpell> Pending=new();
    private static bool _locked;
    private static int _generation;
    [ThreadStatic] internal static string DamageLaw;
    [ThreadStatic] internal static bool DotDamage;
    internal static double Now=>MclslBeastLoot.Now;
    internal static void Clear(){_generation++;Pending.Clear();_locked=false;DamageLaw=null;DotDamage=false;}
    internal static void Load()
    {
        Clear();if(World.world?.map_stats?.custom_data==null)return;
        World.world.map_stats.custom_data.get(SaveKey,out string raw,"");if(raw.Length==0)return;
        try
        {
            var spells=JsonConvert.DeserializeObject<List<MclslPendingSpell>>(raw);
            if(spells==null||spells.Count>16384 || spells.Any(x=>x.Version!=1 || x.Targets==null || x.Targets.Count>8 || float.IsNaN(x.Damage)||float.IsInfinity(x.Damage) || x.Damage<0 || double.IsNaN(x.Due)))
                throw new InvalidOperationException("待命中法术存档格式错误");
            Pending.AddRange(spells);
        }
        catch(Exception ex){_locked=true;MclslDiagnostics.Error("spell-pending-load",ex.ToString());}
    }
    internal static void Save()
    {
        if(_locked||World.world?.map_stats==null)return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(Pending));
    }
    internal static bool IsProjectile(string id)=>id is "S001" or "S004" or "S013" or "S015" or "S017";
    internal static bool IsBeam(string id)=>id is "S009" or "S026";
    internal static bool IsArea(string id)=>id is "S010" or "S012" or "S019" or "S022" or "S023";
    internal static bool Queue(Actor caster,Actor main,MclslSpellDefinition spell)
    {
        if(_locked || Pending.Count>=16384)return false;
        var targets=MclslCombatTargets.Lock(caster,main,spell.MaxTargets);if(targets.Length==0)return false;
        float strength=MclslBeastActorRegistration.IsOrdinary(caster)
            ?1+0.05f*Math.Max(0,MclslRealmIds.Index(MclslActorAccessor.Realm(caster))):MclslSpellProgression.Strength(caster,spell.Id);
        float damage=Math.Max(1,caster.stats.get("damage"))*spell.Power*strength;
        if(spell.Law=="火")damage*=1+MclslPhysiqueSystem.FireDamagePercent(caster)/100f;
        if(spell.Id=="S010")damage*=1.5f;if(spell.Id=="S012")damage*=2;
        damage*=1+MclslBeastItemEffects.ArtifactBonus(caster,spell.Law)+MclslBeastItemEffects.ArtifactBonus(caster,"spell");
        double preparation=IsArea(spell.Id)?0.3:IsProjectile(spell.Id)?0.12:0.15;
        double travel=IsArea(spell.Id)?0.2:IsProjectile(spell.Id)?Math.Clamp((main.current_position-caster.current_position).magnitude/30f,0.15f,0.45f):0.15;
        var pending=new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=MclslActorAccessor.Id(caster),Spell=spell.Id,
            Law=spell.Law,Damage=damage,Strength=strength,Targets=targets.Select(MclslActorAccessor.Id).ToList(),Born=Now,TravelAt=Now+preparation,Due=Now+preparation+travel,
            Terrain=spell.Id is "S019" or "S022" or "S023" or "S026"};
        SetControl(pending,spell.Id,strength);Pending.Add(pending);
        MclslSpellEffect.Show(caster,main,spell.Id,phase:"cast",duration:(float)preparation);return true;
    }
    private static void SetControl(MclslPendingSpell spell,string id,float strength)
    {
        (string Status,float Duration) control=id switch
        {
            "S008"=>("mclsl_spell_yin_bind",2),"S007"=>("mclsl_spell_shock",1.5f),"S012"=>("mclsl_spell_all_bind",3),
            "S013"=>("mclsl_spell_frost_slow",1),"S016"=>("mclsl_spell_vine_bind",2),"S017"=>("mclsl_spell_armor_break",4),
            "S021"=>("mclsl_spell_soul_slow",3),"S023"=>("mclsl_spell_domain_slow",2.5f),"S026"=>("mclsl_spell_origin_slow",1.5f),_=>(null,0)
        };
        spell.Status=control.Status;spell.StatusDuration=Math.Min(4.5f,control.Duration*strength);
    }
    internal static bool QueueTalisman(Actor caster,string effect,int grade)
    {
        if(_locked||Pending.Count>=16380||caster.attack_target is not Actor main)return false;
        var targets=MclslCombatTargets.Lock(caster,main,effect=="wind"?2:effect=="space_area"?3:1);if(targets.Length==0)return false;
        string law=effect switch {"fire"=>"火","water"=>"水","wind"=>"风","yin"=>"阴","metal"=>"金","space" or "space_area"=>"空间",_=>"physical"};
        string visual=effect switch {"fire"=>"S004","water"=>"S013","wind"=>"S015","yin"=>"S008","space" or "space_area"=>"S022",_=>"S001"};
        Pending.Add(new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=MclslActorAccessor.Id(caster),Spell=visual,Law=law,
            Targets=targets.Select(MclslActorAccessor.Id).ToList(),Damage=Math.Max(1,caster.stats.get("damage"))*(0.5f+grade*0.3f)*(1+MclslBeastItemEffects.ArtifactBonus(caster,law)),
            Strength=1,Born=Now,TravelAt=Now+0.12,Due=Now+0.3,Status=effect=="yin"?"mclsl_beast_yin_bind":effect=="water"?"mclsl_beast_water_slow":effect=="fire"?"mclsl_beast_talisman_fire":null,StatusDuration=1+grade*0.5f});
        MclslSpellEffect.Show(caster,main,visual,phase:"cast",duration:0.12f);return true;
    }
    internal static void Tick()
    {
        if(_locked||World.world==null||Config.paused)return;
        int processed=0;
        int generation=_generation;
        for(int i=Pending.Count-1;i>=0 && processed<128;i--)
        {
            var spell=Pending[i];
            if(spell.Phase==0 && Now<spell.TravelAt || spell.Phase!=0 && Now<spell.Due)continue;
            if(!MclslActorRegistry.ResolveKnownOrWorld(spell.Caster,out var caster)||!MclslActorAccessor.Alive(caster)) {Pending.RemoveAt(i);continue;}
            if(spell.Phase==0 && Now>=spell.TravelAt && !spell.Dot)
            {
                spell.Phase=1;
                if(IsProjectile(spell.Spell))
                {
                    foreach(long targetId in spell.Targets)
                        if(MclslActorRegistry.ResolveKnownOrWorld(targetId,out var lockedTarget)&&MclslCombatTargets.IsEnemy(caster,lockedTarget))
                            MclslSpellEffect.Show(caster,lockedTarget,spell.Spell,phase:"travel",duration:(float)Math.Max(0.01,spell.Due-Now));
                }
                else if(spell.Targets.Count>0&&MclslActorRegistry.ResolveKnownOrWorld(spell.Targets[0],out var main)&&MclslCombatTargets.IsEnemy(caster,main))
                    MclslSpellEffect.Show(caster,main,spell.Spell,phase:"area",duration:(float)Math.Max(0.01,spell.Due-Now));
            }
            if(Now<spell.Due)continue;
            Pending.RemoveAt(i);processed++;
            foreach(long targetId in spell.Targets)
            {
                if(!MclslActorRegistry.ResolveKnownOrWorld(targetId,out var target)||!MclslCombatTargets.IsEnemy(caster,target))continue;
                if(!spell.Dot)MclslSpellEffect.Show(caster,target,spell.Spell,phase:"hit",duration:0.4f);
                DealDamage(caster,target,spell.Damage,spell.Law,spell.Dot);
                if(generation!=_generation)return;
                if(!MclslActorAccessor.Alive(target))continue;
                if(spell.Status=="mclsl_beast_talisman_fire")
                    for(int pulse=1;pulse<=4;pulse++)Pending.Add(new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=spell.Caster,Spell=spell.Spell,Law=spell.Law,Targets=new(){targetId},Damage=spell.Damage*0.25f/4,Due=Now+pulse,Dot=true,Phase=1});
                else if(spell.Status!=null)target.addStatusEffect(spell.Status,spell.StatusDuration);
                if(!spell.Dot && spell.Terrain)MclslSpellTerrain.Queue(target.current_tile,spell.Spell);
            }
            if(spell.Spell=="S009"&&!spell.Dot)caster.restoreHealthPercent(0.05f*spell.Strength);
        }
    }
    internal static void DealDamage(Actor caster,Actor target,float damage,string law,bool dot=false)
    {
        string oldLaw=DamageLaw;bool oldDot=DotDamage;DamageLaw=law;DotDamage=dot;
        try {MclslSpellSystem.DeliverDamage(caster,target,damage,law=="火"?AttackType.Fire:law=="physical"?AttackType.Weapon:AttackType.Divine);}
        finally {DamageLaw=oldLaw;DotDamage=oldDot;}
    }
}
