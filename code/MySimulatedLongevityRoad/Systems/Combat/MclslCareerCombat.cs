using System;
using System.Collections.Generic;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using MySimulatedLongevityRoad.Queries;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Combat;

internal sealed class MclslCareerCast
{
    public int Tier, Phase;
    public long Target;
    public double Due;
}
internal sealed class MclslCareerZone
{
    public int Tier, X, Y;
    public double Expires, Pulse;
}
internal sealed class MclslCareerSummon
{
    public long Unit;
    public int Kind;
    public double Expires, Pulse;
}
internal sealed class MclslCareerMark
{
    public int SwordHits, Sigils;
    public double Expires, OwnerHit, SpiritHit, NextAssist;
}
internal sealed class MclslCareerBattleState
{
    public long Owner;
    public int Career, Sword, Rage, Elements, LastElement;
    public bool Counter, FullCycle;
    public double LastAction, NextDecision, GuardUntil, ResonanceUntil, UnloadUntil, NextPassiveEffect, NextAudio, Clock;
    public bool[] TrialThisBout = new bool[4];
    public MclslCareerCast Cast;
    public Dictionary<long,MclslCareerMark> Marks = new();
    public List<MclslCareerZone> Zones = new();
    public List<MclslCareerSummon> Summons = new();
}

internal static class MclslCareerCombat
{
    private const string SaveKey="mclsl.combat_career.battle.v1";
    private static readonly Dictionary<long,MclslCareerBattleState> Battles = new();
    private static readonly Dictionary<long,long> SummonOwners = new();
    private static double Now => MclslSpellCasting.Now;
    internal static void Init()
    {
        foreach(string id in new[]{"casting","slow","break","interrupt"})
        {
            var status=new StatusAsset { id="mclsl_career_"+id,duration=4,base_stats=new BaseStats(),
                path_icon="trait/MclslCombatCareer_"+(id=="casting"?"elemental":"martial") };
            status.base_stats[id=="break"?"armor":"multiplier_speed"]=id=="break"?-12:id=="casting"?-1f:-0.35f;
            AssetManager.status.add(status);
        }
        // Templates supply native animal AI; living-resources binds the authored spirits.
        string[] templates={"wolf","turtle","butterfly","sheep"};
        for(int i=0;i<4;i++)
        {
            var asset=AssetManager.actor_library.clone("mclsl_career_spirit_"+i,templates[i]);
            asset.name_locale="mclsl_career_spirit_"+i;
            MclslLocalizationBridge.RegisterKey(asset.name_locale,new[]{"契灵·狼","契灵·岩龟","契灵·雷隼","契灵·鹿"}[i]);
            asset.default_subspecies_traits=new List<string>(asset.default_subspecies_traits);
            asset.default_subspecies_traits.RemoveAll(t=>t.StartsWith("reproduction_",StringComparison.Ordinal));
            asset.default_subspecies_traits.Add("reproduction_none");
            asset.can_evolve_into_new_species=false;asset.source_meat=false;asset.source_meat_insect=false;
            asset.resources_given=new List<ResourceContainer>();asset.count_as_unit=false;
            asset.has_baby_form=false;asset.use_phenotypes=false;asset.has_advanced_textures=false;
            string path="actors/CareerSpirits/"+i+"/";
            asset.texture_id=asset.id;
            asset.texture_asset=new ActorTextureSubAsset(path,false);
            asset.texture_asset.texture_path_main=path+"main";
            asset.texture_asset.loadShadow();
            asset.need_colored_sprite=false;
            asset.animation_idle=new[]{"idle_0","idle_1","idle_2","idle_3"};
            asset.animation_walk=new[]{"walk_0","walk_1","walk_2","walk_3"};
            asset.animation_swim=asset.animation_walk;
            asset.animation_idle_speed=0.18f;asset.animation_walk_speed=0.12f;
            asset.animation_swim_speed=0.12f;asset.animation_speed_based_on_walk_speed=false;
            asset.base_stats["scale"]=1;
            asset.icon=asset.id;
            SpriteTextureLoader._cached_sprites["ui/Icons/"+asset.id]=
                SpriteTextureLoader.getSprite("ui/CareerSkills/CP_spirit_"+(i+1)+"A");
            Sprite first=SpriteTextureLoader.getSprite(path+"main/idle_0");
            if(first==null)throw new InvalidOperationException("Missing spirit artwork: "+path);
            AssetManager.actor_library.loadTexturesAndSprites(asset);
            asset.cached_sprite=first;asset._cached_sprite=first;
        }
    }
    internal static void Track(Actor actor)
    {
        long id=MclslActorAccessor.Id(actor);
        if(!Battles.ContainsKey(id))Battles[id]=new MclslCareerBattleState { Owner=id,Career=MclslCombatCareerSystem.Read(actor).Career };
    }
    private static MclslCareerBattleState Battle(Actor actor) { Track(actor);return Battles[MclslActorAccessor.Id(actor)]; }
    private static bool Passive(Actor actor,int tier)=>MclslCombatCareerSystem.HasPassive(actor,MclslCombatCareerSystem.Career(actor),tier);
    internal static void Clear(){Battles.Clear();SummonOwners.Clear();Protections.Clear();}
    internal static void Save()
    {
        foreach (long id in Battles.Keys)
            if(MclslActorRegistry.ResolveKnownOrWorld(id,out Actor actor))
                MclslCombatCareerSystem.Write(actor,MclslCombatCareerSystem.Read(actor));
        foreach(var battle in Battles.Values)battle.Clock=Now;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(Battles.Values));
    }
    internal static void Load()
    {
        Clear();
        if(World.world.map_stats.custom_data==null)return;
        World.world.map_stats.custom_data.get(SaveKey,out string data,"");
        if(data.Length>0)foreach(var battle in JsonConvert.DeserializeObject<List<MclslCareerBattleState>>(data))
        {
            double elapsed=Now-battle.Clock;
            battle.LastAction+=elapsed;battle.NextDecision+=elapsed;battle.GuardUntil+=elapsed;
            battle.ResonanceUntil+=elapsed;battle.UnloadUntil+=elapsed;
            battle.NextPassiveEffect+=elapsed;battle.NextAudio+=elapsed;
            if(battle.Cast!=null)battle.Cast.Due+=elapsed;
            foreach(var mark in battle.Marks.Values)
            {mark.Expires+=elapsed;mark.OwnerHit+=elapsed;mark.SpiritHit+=elapsed;mark.NextAssist+=elapsed;}
            foreach(var zone in battle.Zones){zone.Expires+=elapsed;zone.Pulse+=elapsed;}
            foreach(var summon in battle.Summons){summon.Expires+=elapsed;summon.Pulse+=elapsed;}
            Battles[battle.Owner]=battle;
            foreach(var summon in battle.Summons)SummonOwners[summon.Unit]=battle.Owner;
        }
    }
    internal static void RemoveOwner(Actor actor)
    {
        long id=MclslActorAccessor.Id(actor);
        if(!Battles.TryGetValue(id,out var battle))return;
        foreach(var summon in battle.Summons)
        {
            SummonOwners.Remove(summon.Unit);
            if(MclslActorRegistry.ResolveKnownOrWorld(summon.Unit,out var unit)&&MclslActorAccessor.Alive(unit))
                unit.dieAndDestroy((AttackType)11);
        }
        actor.finishStatusEffect("mclsl_career_casting");Battles.Remove(id);
        foreach(long key in Protections.Where(p=>p.Key==id||p.Value.Owner==id).Select(p=>p.Key).ToArray()) Protections.Remove(key);
    }
    private static Actor Resolve(long id) => MclslActorRegistry.ResolveKnownOrWorld(id,out var actor)?actor:null;
    private static bool Enemy(Actor owner,Actor target)=>MclslCombatTargets.IsEnemy(owner,target);
    private static bool Injured(Actor actor)=>actor.getHealth()<actor.getMaxHealth();
    private static Actor[] Nearby(Actor actor,WorldTile tile,float radius)
        => Finder.getUnitsFromChunk(tile,1,radius,false).Where(MclslActorAccessor.Alive).Distinct().ToArray();
    private static Actor[] Friends(Actor owner,WorldTile tile,float radius)
        => Nearby(owner,tile,radius).Where(a=>a==owner || owner.kingdom!=null&&a.kingdom==owner.kingdom).ToArray();
    private static MclslCareerMark Mark(MclslCareerBattleState battle,Actor target)
    {
        long id=MclslActorAccessor.Id(target);
        if(!battle.Marks.TryGetValue(id,out var mark)||mark.Expires<Now)
            battle.Marks[id]=mark=new MclslCareerMark();
        mark.Expires=Now+15;return mark;
    }
    internal static void Tick()
    {
        if(World.world==null||Config.paused)return;
        foreach(var battle in Battles.Values.ToArray())
        {
            Actor owner=Resolve(battle.Owner);
            if(!MclslActorAccessor.Alive(owner))
            {
                if(owner!=null)RemoveOwner(owner);
                else {foreach(var summon in battle.Summons)
                    {SummonOwners.Remove(summon.Unit);Actor orphan=Resolve(summon.Unit);if(MclslActorAccessor.Alive(orphan))orphan.dieAndDestroy((AttackType)11);}
                    Battles.Remove(battle.Owner);}
                continue;
            }
            if(MclslCombatCareerSystem.Read(owner).Career!=battle.Career){RemoveOwner(owner);continue;}
            if(battle.LastAction>0&&Now-battle.LastAction>15)
            {battle.TrialThisBout=new bool[4];battle.Marks.Clear();battle.Elements=0;battle.LastAction=0;}
            if(battle.Cast!=null) TickCast(owner,battle);
            TickZones(owner,battle);TickSummons(owner,battle);
            if(Now>=battle.NextDecision)
            {
                battle.NextDecision=Now+0.25;
                TryCast(owner);
            }
        }
    }
    internal static bool TryCast(Actor actor)
    {
        if(!MclslCombatCareerSystem.Applicable(actor))return false;
        var state=MclslCombatCareerSystem.Read(actor);
        if(state.Career<0||!MclslActorAccessor.IsCultivator(actor))return false;
        var battle=Battle(actor);if(battle.Cast!=null)return true;
        if(actor.attack_target is not Actor target||!Enemy(actor,target))return false;
        int usable=Math.Min(state.Rank,MclslRealmIds.Index(MclslActorAccessor.Realm(actor))+1);
        for(int tier=usable;tier>=1;tier--)
        {
            var skill=MclslCombatCareerCatalog.Skill(state.Career,tier);
            if(state.Cooldowns[tier-1]>Now||MclslSpellSystem.CurrentMana(actor)<Mana(actor,skill))continue;
            if(skill.Item.Length>0&&MclslBagSystem.Count(MclslBagSystem.Read(actor),skill.Item)<1)continue;
            if((state.Career==1||state.Career==2||state.Career==3)&&tier==4 && actor.getHealth()>actor.getMaxHealth()*0.75f)continue;
            if(state.Career==4&&battle.Zones.Any(z=>z.Tier==tier))continue;
            if(state.Career==5)
            {
                if(tier==5&&battle.Summons.Count==0)continue;
                if(tier<5 && (battle.Summons.Any(s=>s.Kind==tier-1)||battle.Summons.Count>=Math.Min(3,(state.Rank+1)/2)))continue;
            }
            if(Vector2.Distance(actor.current_position,target.current_position)>(state.Career==1?4:10))continue;
            battle.Cast=new MclslCareerCast { Tier=tier,Target=MclslActorAccessor.Id(target),Due=Now+skill.Preparation };
            if(skill.Preparation>0){actor.stopMovement();actor.addStatusEffect("mclsl_career_casting",skill.Preparation+0.1f);}
            MclslSpellEffect.Show(actor,target,skill.Id,phase:"cast",duration:Math.Max(0.12f,skill.Preparation));
            if(skill.Preparation==0)TickCast(actor,battle);
            return true;
        }
        return false;
    }
    internal static int Mana(Actor actor,MclslCareerSkill skill)
        => Mathf.CeilToInt(skill.Mana*((skill.Career==2||skill.Career==3)&&Passive(actor,1)?0.9f:1f));
    private static void TickCast(Actor actor,MclslCareerBattleState battle)
    {
        var cast=battle.Cast;var skill=MclslCombatCareerCatalog.Skill(battle.Career,cast.Tier);
        if(cast.Phase==0 && (actor.hasStatus("mclsl_career_interrupt")||actor.hasStatus("mclsl_spell_shock")||actor.hasStatus("frozen")))
        {Interrupt(actor);return;}
        if(Now<cast.Due)return;
        Actor target=Resolve(cast.Target);
        if(!Enemy(actor,target)){battle.Cast=null;actor.finishStatusEffect("mclsl_career_casting");return;}
        if(cast.Phase==0)
        {
            var state=MclslCombatCareerSystem.Read(actor);
            int mana=MclslSpellSystem.CurrentMana(actor),cost=Mana(actor,skill);
            if(state.Career!=battle.Career || state.Rank<skill.Tier
                || !MclslActorAccessor.IsCultivator(actor)
                || MclslRealmIds.Index(MclslActorAccessor.Realm(actor))<skill.Tier-1
                || state.Cooldowns[skill.Tier-1]>Now || mana<cost
                || skill.Item.Length>0&&MclslBagSystem.Count(MclslBagSystem.Read(actor),skill.Item)<1)
            {
                MclslSpellEffect.CancelCareerCast(actor,skill.Id);
                battle.Cast=null;actor.finishStatusEffect("mclsl_career_casting");return;
            }
            if(skill.Item.Length>0)
            {
                var bag=MclslBagSystem.Read(actor);MclslBagSystem.Remove(bag,skill.Item);MclslBagSystem.Write(actor,bag);
            }
            MclslActorAccessor.Set(actor,MclslActorDataKeys.ManaCurrent,mana-cost);
            state.Cooldowns[skill.Tier-1]=Now+skill.Cooldown*(battle.Career==0&&skill.Tier==4&&Passive(actor,4)?0.85:1);
            actor.finishStatusEffect("mclsl_career_casting");
            cast.Phase=1;cast.Due=Now+0.2;
            MclslSpellEffect.Show(actor,target,skill.Id,phase:skill.Career==4?"area":"travel",duration:0.2f);
            if(Now>=battle.NextAudio){MusicBox.playSoundVisibleOnly(actor.asset.sound_attack,actor.current_tile);battle.NextAudio=Now+0.3;}
            MclslCombatCareerSystem.Write(actor,state);
            return;
        }
        battle.Cast=null;Execute(actor,target,battle,skill);
    }
    internal static void Interrupt(Actor actor)
    {
        if(!Battles.TryGetValue(MclslActorAccessor.Id(actor),out var battle)||battle.Cast==null||battle.Cast.Phase!=0)return;
        var state=MclslCombatCareerSystem.Read(actor);
        var skill=MclslCombatCareerCatalog.Skill(battle.Career,battle.Cast.Tier);
        MclslSpellEffect.CancelCareerCast(actor,skill.Id);
        MclslActorAccessor.Set(actor,MclslActorDataKeys.ManaCurrent,Math.Max(0,MclslSpellSystem.CurrentMana(actor)-Mathf.CeilToInt(Mana(actor,skill)*0.2f)));
        state.Cooldowns[skill.Tier-1]=Now+3;battle.Cast=null;actor.finishStatusEffect("mclsl_career_casting");
        MclslSpellEffect.Show(actor,null,skill.Id,phase:"cancel",duration:0.15f);
        MclslCombatCareerSystem.Write(actor,state);
    }
    private static void Execute(Actor owner,Actor target,MclslCareerBattleState battle,MclslCareerSkill skill)
    {
        int c=skill.Career,t=skill.Tier;var state=MclslCombatCareerSystem.Read(owner);
        if(c==4)
        {
            WorldTile tile=t==2||t==4?owner.current_tile:target.current_tile;
            battle.Zones.Add(new MclslCareerZone { Tier=t,X=tile.pos.x,Y=tile.pos.y,Expires=Now+(6+t)*(Passive(owner,2)?1.2:1),Pulse=Now });
        }
        else if(c==5)
        {
            if(t==5)
            {
                foreach(var summon in battle.Summons){summon.Expires+=8;Actor unit=Resolve(summon.Unit);if(MclslActorAccessor.Alive(unit))unit.restoreHealthPercent(0.25f);}
                battle.ResonanceUntil=Now+8;
            }
            else SpawnSpirit(owner,battle,t-1);
        }
        else if(t==4 && c is 1 or 2 or 3)
        {
            foreach(Actor ally in Friends(owner,owner.current_tile,5).Take(c==3?3:1))
            {
                float before=ally.getHealth();ally.restoreHealthPercent(c==3?0.12f:0.08f);
                if(ally==owner)battle.GuardUntil=Now+6+(c==3&&Passive(owner,4)?2:0);
                else {ally.addStatusEffect("mclsl_career_slow",0.01f);AddProtection(ally,owner,6);}
                MclslSpellEffect.Show(owner,ally,skill.Id,phase:"hit",duration:0.6f);
                if(before<ally.getHealth()&&ally.has_attack_target)Trial(owner,3);
            }
            if(c==2){MclslSpellSystem.RestoreMana(owner,0.04f);if(battle.FullCycle)Trial(owner,3);}
        }
        else
        {
            Actor[] targets=skill.Targets>1?Nearby(owner,target.current_tile,4).Where(a=>Enemy(owner,a)).OrderBy(a=>a==target?0:1).Take(skill.Targets).ToArray():new[]{target};
            foreach(Actor enemy in targets) Hit(owner,enemy,battle,skill);
        }
        MclslCombatCareerSystem.GainSkillExperience(state,t,1);
        MclslCombatCareerSystem.Promote(owner,state);MclslCombatCareerSystem.Write(owner,state);
    }
    private static void Hit(Actor owner,Actor target,MclslCareerBattleState battle,MclslCareerSkill skill)
    {
        int c=skill.Career,t=skill.Tier;var mark=Mark(battle,target);
        int trials=0;
        float power=skill.Power*(1+0.05f*(MclslCombatCareerSystem.Read(owner).Mastery[t-1]-1));
        if(c==0)
        {
            if(Passive(owner,1))power*=1.1f;
            if(t==2&&mark.SwordHits>0)power*=1.25f;
            if(t>=3){power*=1+battle.Sword*0.08f;if(t==5&&battle.Sword==5&&Passive(owner,5))power*=1.25f;battle.Sword=0;}
            if(Passive(owner,2)&&mark.SwordHits>=2){target.addStatusEffect("mclsl_career_break",4);trials|=2;PassiveFx(owner,2,target);}
            if(Passive(owner,3))battle.Sword=Math.Min(5,battle.Sword+1);
            if(t==3)trials|=4;if(t==4)trials|=8;
        }
        if(c==1)
        {
            if(t==2){Interrupt(target);target.addStatusEffect("mclsl_career_interrupt",1);trials|=2;}
            if(t>=3){power*=1+battle.Rage*0.1f;trials|=4;if(t==5)battle.Rage=0;}
            if(battle.Counter&&Passive(owner,5)){power*=1.25f;battle.Counter=false;PassiveFx(owner,5,target);}
            if(t==3)target.addStatusEffect("mclsl_career_slow",2);
        }
        if(c==2)
        {
            int element=t==1?1:t==2?2:t==3?4:0;
            int prior=battle.Elements;
            if(Passive(owner,2))battle.Elements|=element;
            if(t==2)target.addStatusEffect("mclsl_career_slow",3);
            if((prior&(prior-1))!=0){trials|=2;if(t>=3){power*=Passive(owner,3)?1.5f:1.25f;trials|=4;}}
            if(battle.Elements==7){battle.FullCycle=true;PassiveFx(owner,3,target);}
            if(t==5&&battle.FullCycle&&Passive(owner,5)){power*=1.3f;battle.FullCycle=false;}
            if(Passive(owner,4)&&battle.LastElement!=0&&element!=0&&battle.LastElement!=element){MclslSpellSystem.RestoreMana(owner,0.02f);PassiveFx(owner,4,owner);}
            if(element!=0)battle.LastElement=element;if(t>=3)battle.Elements=0;
        }
        if(c==3)
        {
            if(t==2){target.addStatusEffect("mclsl_career_interrupt",1.2f);trials|=2;}
            if(t>=3){power*=1+mark.Sigils*0.15f;if(mark.Sigils>=3){trials|=4;if(t==5&&Passive(owner,5))power*=1.25f;}mark.Sigils=0;}
            else if(Passive(owner,2))mark.Sigils=Math.Min(5,mark.Sigils+(t==2&&Passive(owner,3)?2:1));
        }
        int health=target.getHealth();
        MclslSpellCasting.DealDamage(owner,target,Math.Max(1,owner.stats.get("damage"))*power,skill.Law);
        if(target.getHealth()<health)
        {
            Trial(owner,0,target);
            for(int i=1;i<4;i++)if((trials&(1<<i))!=0)Trial(owner,i,target);
        }
        MclslSpellEffect.Show(owner,target,skill.Id,phase:"hit",duration:0.4f);
    }
    private static void TickZones(Actor owner,MclslCareerBattleState battle)
    {
        for(int i=battle.Zones.Count-1;i>=0;i--)
        {
            var zone=battle.Zones[i];if(Now>=zone.Expires){battle.Zones.RemoveAt(i);continue;}
            if(Now<zone.Pulse)continue;zone.Pulse=Now+1;
            WorldTile tile=World.world.GetTile(zone.X,zone.Y);var skill=MclslCombatCareerCatalog.Skill(4,zone.Tier);
            MclslSpellEffect.Show(owner,null,skill.Id,tile,phase:"area",duration:1f);
            foreach(Actor unit in Nearby(owner,tile,4).Take(8))
            {
                if(Enemy(owner,unit))
                {
                    if(zone.Tier==1){unit.addStatusEffect("mclsl_career_slow",Passive(owner,1)?2.4f:2);Trial(owner,0,unit);}
                    if(zone.Tier==3||zone.Tier==5)
                    {
                        float factor=Passive(owner,3)&&owner.attack_target==unit?1.25f:1;
                        int before=unit.getHealth();
                        MclslSpellCasting.DealDamage(owner,unit,owner.stats.get("damage")*0.25f*factor,"土",true);
                        if(unit.getHealth()<before)Trial(owner,2,unit);
                    }
                }
                else if(unit==owner||owner.kingdom!=null&&unit.kingdom==owner.kingdom)
                {
                    if(zone.Tier==2&&unit.has_attack_target && MclslSpellSystem.RestoreMana(unit,0.025f)>0)Trial(owner,1);
                    if(zone.Tier==4||zone.Tier==5)AddProtection(unit,owner,1.5);
                }
            }
        }
    }
    private sealed class Protection { public long Owner;public double Until; }
    private static readonly Dictionary<long,Protection> Protections=new();
    private static void AddProtection(Actor target,Actor owner,double duration)
        =>Protections[MclslActorAccessor.Id(target)]=new Protection{Owner=MclslActorAccessor.Id(owner),Until=Now+duration};
    private static void SpawnSpirit(Actor owner,MclslCareerBattleState battle,int kind)
    {
        Actor spirit=World.world.units.createNewUnit("mclsl_career_spirit_"+kind,owner.current_tile,false,0,null,null,true,false,false,false);
        if(owner.kingdom!=null)spirit.setKingdom(owner.kingdom);
        spirit.stats["health"]=Math.Max(20,owner.getMaxHealth()*(kind==1?0.4f:0.2f));
        spirit.stats["damage"]=Math.Max(1,owner.stats.get("damage")*(kind==1?0.15f:0.3f));
        spirit.restoreHealthPercent(1);
        var summon=new MclslCareerSummon { Unit=MclslActorAccessor.Id(spirit),Kind=kind,Expires=Now+(12+kind*3)*(Passive(owner,1)?1.2:1),Pulse=Now };
        battle.Summons.Add(summon);SummonOwners[summon.Unit]=battle.Owner;
        MclslWorldActorQuery.Track(spirit);
    }
    private static void TickSummons(Actor owner,MclslCareerBattleState battle)
    {
        for(int i=battle.Summons.Count-1;i>=0;i--)
        {
            var summon=battle.Summons[i];Actor unit=Resolve(summon.Unit);
            if(!MclslActorAccessor.Alive(unit)||Now>=summon.Expires)
            {SummonOwners.Remove(summon.Unit);if(MclslActorAccessor.Alive(unit))unit.dieAndDestroy((AttackType)11);battle.Summons.RemoveAt(i);continue;}
            if(Now<summon.Pulse)continue;summon.Pulse=Now+1;
            if(owner.kingdom!=null&&unit.kingdom!=owner.kingdom)unit.setKingdom(owner.kingdom);
            if(owner.attack_target is Actor target && Enemy(owner,target))
            {
                unit.attack_target=target;unit.beh_actor_target=target;unit.has_attack_target=true;
                if(summon.Kind==2&&Vector2.Distance(unit.current_position,target.current_position)<10)
                {
                    float factor=battle.ResonanceUntil>Now?1.3f:1;
                    if(Passive(owner,5)&&battle.Summons.Select(s=>s.Kind).Distinct().Count()>=3)factor*=1.25f;
                    int before=target.getHealth();
                    MclslSpellCasting.DealDamage(unit,target,owner.stats.get("damage")*0.2f*factor,"雷");
                    if(target.getHealth()<before)Trial(owner,0,target);
                }
            }
            if(summon.Kind==3)
            {
                foreach(Actor ally in Friends(owner,unit.current_tile,4).Where(a=>a.has_attack_target&&Injured(a)).Take(3))
                {int before=ally.getHealth();ally.restoreHealthPercent(Passive(owner,4)?0.036f:0.03f);
                    if(ally.getHealth()>before){Trial(owner,3);if(Passive(owner,4))PassiveFx(owner,4,ally);}}
            }
        }
    }
    internal static float Incoming(Actor victim,Actor attacker,float damage)
    {
        if(Protections.TryGetValue(MclslActorAccessor.Id(victim),out var protection)&&protection.Until>Now)
        {
            Actor owner=Resolve(protection.Owner);
            if(MclslActorAccessor.Alive(owner))
            {damage*=MclslCombatCareerSystem.Career(owner)==4&&Passive(owner,4)?0.7f:0.8f;
                if(MclslCombatCareerSystem.Career(owner)==4){Trial(owner,3,attacker);if(Passive(owner,4))PassiveFx(owner,4,victim);}}
        }
        if(!MclslCombatCareerSystem.Applicable(victim)||MclslCombatCareerSystem.Career(victim)<0)return damage;
        var battle=Battle(victim);int career=battle.Career;
        if(battle.GuardUntil>Now)damage*=0.7f;
        if(career==1)
        {
            if(Passive(victim,1))damage*=0.9f;
            if(Passive(victim,2)){damage*=0.92f;battle.Counter=true;Trial(victim,1,attacker);PassiveFx(victim,2,victim);}
            if(Passive(victim,3))battle.Rage=Math.Min(5,battle.Rage+1);
            if(Passive(victim,4)&&victim.getHealth()<victim.getMaxHealth()*0.3f){damage*=0.8f;Trial(victim,3,attacker);PassiveFx(victim,4,victim);}
        }
        if(career==5&&battle.Summons.Any(s=>s.Kind==1&&s.Expires>Now))
        {damage*=Passive(victim,2)?0.75f:0.85f;Trial(victim,1,attacker);PassiveFx(victim,2,victim);}
        return damage;
    }
    internal static void EffectiveHit(Actor attacker,Actor target,int actualDamage)
    {
        if(actualDamage<=0)return;
        long id=MclslActorAccessor.Id(attacker);
        bool spirit=SummonOwners.TryGetValue(id,out long ownerId);
        if(spirit)attacker=Resolve(ownerId);
        if(!MclslCombatCareerSystem.Applicable(attacker)||MclslCombatCareerSystem.Career(attacker)<0||!Enemy(attacker,target))return;
        var battle=Battle(attacker);var mark=Mark(battle,target);mark.SwordHits++;
        if(battle.Career==1&&Passive(attacker,3))battle.Rage=Math.Min(5,battle.Rage+1);
        Trial(attacker,0,target);
        if(battle.Career==5)
        {
            if(spirit)mark.SpiritHit=Now;else mark.OwnerHit=Now;
            if(Passive(attacker,3)&&mark.OwnerHit>0&&mark.SpiritHit>0
                &&Math.Abs(mark.OwnerHit-mark.SpiritHit)<=3 &&Now>=mark.NextAssist)
            {
                mark.NextAssist=Now+3;
                Trial(attacker,2,target);PassiveFx(attacker,3,target);
                MclslSpellCasting.DealDamage(attacker,target,attacker.stats.get("damage")*0.2f,"木");
            }
        }
    }
    internal static float SummonDamage(Actor attacker,float damage)
    {
        if(!SummonOwners.TryGetValue(MclslActorAccessor.Id(attacker),out long ownerId))return damage;
        Actor owner=Resolve(ownerId);
        if(!MclslActorAccessor.Alive(owner))return damage;
        var battle=Battle(owner);
        float factor=battle.ResonanceUntil>Now?1.3f:1f;
        if(Passive(owner,5)&&battle.Summons.Select(s=>s.Kind).Distinct().Count()>=3)factor*=1.25f;
        return Math.Max(damage,owner.stats.get("damage")*0.3f*factor);
    }
    private static void Trial(Actor owner,int index,Actor enemy=null)
    {
        var battle=Battle(owner);
        if(enemy!=null && (!Enemy(owner,enemy)||MclslRealmIds.Index(MclslActorAccessor.Realm(enemy))<MclslRealmIds.Index(MclslActorAccessor.Realm(owner))-1))return;
        battle.LastAction=Now;
        if(battle.TrialThisBout[index])return;battle.TrialThisBout[index]=true;
        var state=MclslCombatCareerSystem.Read(owner);state.Trials[index]++;
        int year=MclslRuntime.CurrentYear();if(state.BattleYear!=year){state.BattleYear=year;state.BattleExperience=0;}
        if(state.BattleExperience<4){state.BattleExperience++;state.Experience++;}
        MclslCombatCareerSystem.Promote(owner,state);MclslCombatCareerSystem.Write(owner,state);
    }
    private static void PassiveFx(Actor owner,int tier,Actor target)
    {
        var battle=Battle(owner);if(Now<battle.NextPassiveEffect)return;battle.NextPassiveEffect=Now+0.5;
        MclslSpellEffect.Show(owner,target,MclslCombatCareerCatalog.Skill(battle.Career,tier,true).Id,phase:"hit",duration:0.35f);
    }
}
