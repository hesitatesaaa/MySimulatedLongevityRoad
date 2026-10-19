using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;
using MySimulatedLongevityRoad.Systems.Combat;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslPendingSpell
{
    public string Id,Spell,Law,Status;
    public long Caster;
    public List<long> Targets=new();
    public float Damage,Strength,StatusDuration;
    public double Born,TravelAt,Due;
    public int Phase,Version=2,Beat,ImpactX,ImpactY;
    public bool Dot,Terrain,Auxiliary,SelfApplied,TerrainApplied,AnyHit;
    public List<long> Applied=new();
    public string[] Linked;
}
internal static class MclslCombatTargets
{
    internal static bool IsEnemy(Actor caster,Actor target,bool released=false)
    {
        if(caster?.data==null || !released&&!MclslActorAccessor.Alive(caster)||!MclslActorAccessor.Alive(target)||caster==target)return false;
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
    private static readonly Dictionary<long,MclslPendingSpell> Actions=new();
    private static readonly Dictionary<long,Actor> Casters=new();
    private static bool _locked;
    private static int _generation;
    [ThreadStatic] internal static string DamageLaw;
    [ThreadStatic] internal static bool DotDamage;
    internal static double Now=>MclslBeastLoot.Now;
    internal static void Clear(){_generation++;Pending.Clear();Actions.Clear();Casters.Clear();_locked=false;DamageLaw=null;DotDamage=false;}
    internal static bool IsBusy(Actor actor)=>actor?.data!=null && Actions.ContainsKey(MclslActorAccessor.Id(actor));
    internal static bool IsWinding(Actor actor)=>actor?.data!=null && Actions.TryGetValue(MclslActorAccessor.Id(actor),out var action) && action.Phase==0;
    internal static void InterruptWindup(Actor actor)
    {
        if(actor?.data==null || !Actions.TryGetValue(MclslActorAccessor.Id(actor),out var action) || action.Phase!=0)return;
        Pending.Remove(action);Actions.Remove(action.Caster);Casters.Remove(action.Caster);
    }
    private static void Remove(MclslPendingSpell spell)
    {
        Pending.Remove(spell);
        if(Actions.TryGetValue(spell.Caster,out var action)&&ReferenceEquals(action,spell))
        {Actions.Remove(spell.Caster);Casters.Remove(spell.Caster);}
    }
    internal static bool Valid(MclslPendingSpell spell)=>spell!=null && spell.Version is 1 or 2
        && spell.Targets!=null && spell.Targets.Count<=8 && spell.Applied!=null && spell.Applied.Count<=8
        && spell.Linked?.Length is not >3 && spell.Phase>=0 && spell.Phase<=3 && spell.Beat>=0 && spell.Beat<=3
        && (spell.Linked==null || spell.Linked.All(x=>x is "S016" or "S008" or "S013" or "S004" or "S007" or "S017" or "S001" or "S015"))
        && spell.Applied.All(spell.Targets.Contains) && spell.Applied.Distinct().Count()==spell.Applied.Count
        && !float.IsNaN(spell.Damage)&&!float.IsInfinity(spell.Damage)&&spell.Damage>=0
        && !float.IsNaN(spell.Strength)&&!float.IsInfinity(spell.Strength)
        && !double.IsNaN(spell.Born)&&!double.IsInfinity(spell.Born)
        && !double.IsNaN(spell.TravelAt)&&!double.IsInfinity(spell.TravelAt)
        && !double.IsNaN(spell.Due)&&!double.IsInfinity(spell.Due)
        && MclslSpellSystem.TryGet(spell.Spell,out _);
    internal static void Load()
    {
        Clear();if(World.world?.map_stats?.custom_data==null)return;
        World.world.map_stats.custom_data.get(SaveKey,out string raw,"");if(raw.Length==0)return;
        try
        {
            var spells=JsonConvert.DeserializeObject<List<MclslPendingSpell>>(raw);
            if(spells==null||spells.Count>16384 || spells.Any(x=>!Valid(x)))
                throw new InvalidOperationException("待命中法术存档格式错误");
            foreach(var spell in spells)
            {
                if(spell.Version==1){spell.Version=2;spell.Auxiliary=true;}
                if(!spell.Dot&&!spell.Auxiliary)
                {
                    if(Actions.ContainsKey(spell.Caster))throw new InvalidOperationException("同一修士不能有多个主动招式");
                    Actions[spell.Caster]=spell;
                }
            }
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
    internal static bool IsProjectile(string id)=>MclslBattleRules.IsProjectile(id);
    internal static bool IsBeam(string id)=>MclslBattleRules.IsBeam(id);
    internal static bool IsArea(string id)=>MclslBattleRules.IsArea(id);
    internal static bool Queue(Actor caster,Actor main,MclslSpellDefinition spell)
    {
        if(_locked || Pending.Count>=16384 || IsBusy(caster))return false;
        bool self=MclslBattleRules.IsSelf(spell.Id);
        var targets=self?new[]{caster}:MclslCombatTargets.Lock(caster,main,spell.MaxTargets);if(targets.Length==0)return false;
        float strength=MclslBeastActorRegistration.IsOrdinary(caster)
            ?1+0.05f*Math.Max(0,MclslRealmIds.Index(MclslActorAccessor.Realm(caster))):MclslSpellProgression.Strength(caster,spell.Id);
        float damage=Math.Max(1,caster.stats.get("damage"))*spell.Power*strength;
        if(spell.Law=="火")damage*=1+MclslPhysiqueSystem.FireDamagePercent(caster)/100f;
        if(spell.Id=="S010")damage*=1.5f;if(spell.Id=="S012")damage*=2;
        damage*=1+MclslBeastItemEffects.ArtifactBonus(caster,spell.Law)+MclslBeastItemEffects.ArtifactBonus(caster,"spell");
        double preparation=MclslBattleRules.Windup(spell.Id);
        double travel=MclslBattleRules.Travel(spell.Id,(main.current_position-caster.current_position).magnitude);
        var pending=new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=MclslActorAccessor.Id(caster),Spell=spell.Id,
            Law=spell.Law,Damage=damage,Strength=strength,Targets=targets.Select(MclslActorAccessor.Id).ToList(),Born=Now,TravelAt=Now+preparation,Due=Now+preparation+travel,
            Terrain=MclslBattleRules.HeavyTerrain(spell.Id)};
        if(spell.Id=="C011")
        {pending.Linked=MclslSpellSystem.ReserveElementalChain(caster);if(pending.Linked.Length==0)return false;}
        if(spell.Id=="C009")pending.Law=MclslActorAccessor.GetString(caster,MclslActorDataKeys.SpiritualRootPrimary,spell.Law);
        SetControl(pending,spell.Id,strength);Pending.Add(pending);Actions[pending.Caster]=pending;
        Casters[pending.Caster]=caster;
        MclslBattleCombat.OnCast(caster,spell.Id);
        MclslSpellEffect.Show(caster,self?null:main,MclslBattleRules.Visual(spell.Id),phase:"cast",duration:(float)preparation,
            tint:spell.Id=="C009"?MclslBattlePrimitives.LawColor(pending.Law):(Color?)null);
        MclslBattleVisuals.Begin(caster,self?null:main,spell.Id,(float)(preparation+travel));return true;
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
            Strength=1,Auxiliary=true,Born=Now,TravelAt=Now+0.12,Due=Now+0.3,Status=effect=="yin"?"mclsl_beast_yin_bind":effect=="water"?"mclsl_beast_water_slow":effect=="fire"?"mclsl_beast_talisman_fire":null,StatusDuration=1+grade*0.5f});
        MclslSpellEffect.Show(caster,main,visual,phase:"cast",duration:0.12f);return true;
    }
    internal static void Tick()
    {
        if(_locked||World.world==null||Config.paused)return;
        int processed=0;
        int generation=_generation;
        for(int i=Pending.Count-1;i>=0 && processed<128;i--)
        {
            if(i>=Pending.Count)continue;
            var spell=Pending[i];
            if(spell.Phase==0 && Now<spell.TravelAt || spell.Phase!=0 && Now<spell.Due)continue;
            processed++;
            if(spell.Phase==3){Remove(spell);continue;}
            if(!MclslActorRegistry.ResolveKnownOrWorld(spell.Caster,out var caster)&&!Casters.TryGetValue(spell.Caster,out caster)) {Remove(spell);continue;}
            if(MclslActorAccessor.Id(caster)!=spell.Caster){Remove(spell);continue;}
            bool alive=MclslActorAccessor.Alive(caster);
            if(!alive && (spell.Phase==0 || MclslBattleRules.IsChanneled(spell.Spell)||MclslBattleRules.IsSelf(spell.Spell))) {Remove(spell);continue;}
            if(alive&&!spell.Auxiliary&&!MclslBattleProfessionSystem.CanUse(caster,spell.Spell)){Remove(spell);continue;}
            if(spell.Phase==0 && Now>=spell.TravelAt && !spell.Dot)
            {
                spell.Phase=1;
                if(IsProjectile(spell.Spell))
                {
                    foreach(long targetId in spell.Targets)
                        if(MclslActorRegistry.ResolveKnownOrWorld(targetId,out var lockedTarget)&&MclslCombatTargets.IsEnemy(caster,lockedTarget))
                            MclslSpellEffect.Show(caster,lockedTarget,MclslBattleRules.Visual(spell.Spell),phase:"travel",duration:(float)Math.Max(0.01,spell.Due-Now),
                                tint:spell.Spell=="C009"?MclslBattlePrimitives.LawColor(spell.Law):(Color?)null);
                }
                else if(spell.Targets.Count>0&&MclslActorRegistry.ResolveKnownOrWorld(spell.Targets[0],out var main)&&MclslCombatTargets.IsEnemy(caster,main))
                    MclslSpellEffect.Show(caster,main,MclslBattleRules.Visual(spell.Spell),phase:"area",duration:(float)Math.Max(0.01,spell.Due-Now));
                if(spell.Spell is "C001" or "C005" or "C010")MclslBattleCombat.Move(caster,spell.Targets[0],spell.Spell=="C010");
            }
            if(Now<spell.Due)continue;
            spell.Phase=2;
            if(MclslBattleRules.IsSelf(spell.Spell))
            {
                if(!spell.SelfApplied){spell.SelfApplied=true;MclslBattleCombat.ApplySelf(caster,spell.Spell);}
                if(generation!=_generation)return;
                spell.Phase=3;spell.Due=Now+MclslBattleRules.Recovery(spell.Spell);continue;
            }
            int beats=spell.Spell=="C011"?Math.Max(1,spell.Linked?.Length??0):MclslBattleRules.Beats(spell.Spell);
            string impactId=spell.Spell=="C011" && spell.Linked?.Length>spell.Beat?spell.Linked[spell.Beat]:spell.Spell;
            foreach(long targetId in spell.Targets)
            {
                if(spell.Applied.Contains(targetId))continue;
                if(!MclslActorRegistry.ResolveKnownOrWorld(targetId,out var target)||!MclslCombatTargets.IsEnemy(caster,target,spell.Phase>0))continue;
                spell.Applied.Add(targetId); // Save callbacks cannot replay a target already being hit.
                if(!spell.AnyHit&&target.current_tile!=null)
                {spell.AnyHit=true;spell.ImpactX=target.current_tile.pos.x;spell.ImpactY=target.current_tile.pos.y;}
                if(!spell.Dot)MclslSpellEffect.Show(caster,target,MclslBattleRules.Visual(impactId),phase:"hit",duration:0.4f,
                    tint:spell.Spell=="C009"?MclslBattlePrimitives.LawColor(spell.Law):(Color?)null);
                float damage=spell.Damage/beats;string law=spell.Law;
                if(spell.Spell=="C011" && spell.Linked?.Length>0 && MclslSpellSystem.TryGet(impactId,out var linked))
                {damage=MclslSpellSystem.LinkedDamage(caster,linked);law=linked.Law;}
                if(spell.Spell=="C014")MclslSpiritPact.Mark(caster,target);
                if(spell.Spell=="C016"&&spell.Beat==0&&spell.Applied.Count==1)MclslSpiritPact.Stampede(caster,target);
                DealDamage(caster,target,damage,law,spell.Dot);
                if(generation!=_generation)return;
                if(!MclslActorAccessor.Alive(target))continue;
                if(spell.Status=="mclsl_beast_talisman_fire")
                    for(int pulse=1;pulse<=4;pulse++)Pending.Add(new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=spell.Caster,Spell=spell.Spell,Law=spell.Law,Targets=new(){targetId},Damage=spell.Damage*0.25f/4,Due=Now+pulse,Dot=true,Phase=1});
                else if(spell.Status!=null)MclslBattleCombat.Control(target,spell.Status,spell.StatusDuration,MclslBattleRules.IsHardControl(spell.Spell));
                if(spell.Spell=="C011"&&impactId!=spell.Spell)MclslBattleCombat.LinkedControl(target,impactId);
                if(spell.Spell is "C003" or "C007")MclslBattleCombat.Control(target,"mclsl_battle_stagger",.4f,true);
                if(spell.Spell=="C005")
                {var push=(target.current_position-caster.current_position).normalized*.5f;target.addForce(push.x,push.y,.1f,false,false);}
            }
            if(!spell.Dot&&spell.Terrain&&!spell.TerrainApplied&&spell.AnyHit&&spell.Beat==beats-1)
            {
                spell.TerrainApplied=true;
                if(spell.ImpactX>=0&&spell.ImpactY>=0&&spell.ImpactX<MapBox.width&&spell.ImpactY<MapBox.height)
                    MclslSpellTerrain.Queue(World.world.GetTileSimple(spell.ImpactX,spell.ImpactY),spell.Spell,MclslRealmIds.Index(MclslActorAccessor.Realm(caster)));
            }
            if(spell.Spell=="S009"&&!spell.Dot&&alive)caster.restoreHealthPercent(0.05f*spell.Strength/beats);
            spell.Applied.Clear();spell.Beat++;
            if(spell.Beat<beats){spell.Due=Now+.16;continue;}
            if(spell.Dot||spell.Auxiliary)Remove(spell);
            else {spell.Phase=3;spell.Due=Now+MclslBattleRules.Recovery(spell.Spell);}
        }
        if(Pending.Count==0)Casters.Clear();
    }
    internal static void DealDamage(Actor caster,Actor target,float damage,string law,bool dot=false)
    {
        string oldLaw=DamageLaw;bool oldDot=DotDamage;DamageLaw=law;DotDamage=dot;
        try {MclslSpellSystem.DeliverDamage(caster,target,damage,law=="火"?AttackType.Fire:law=="physical"?AttackType.Weapon:AttackType.Divine);}
        finally {DamageLaw=oldLaw;DotDamage=oldDot;}
    }
}
