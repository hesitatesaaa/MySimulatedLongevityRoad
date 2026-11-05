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
    public int Phase,Version=2;
    public byte RemainingPulses;
    public bool Dot,Terrain,BeastTalent;
}
internal static class MclslCombatTargets
{
    internal static bool IsEnemy(Actor caster,Actor target)
    {
        if(!MclslActorAccessor.Alive(caster)||!MclslActorAccessor.Alive(target)||caster==target)return false;
        if(MclslBeastActorRegistration.IsBeast(caster)&&MclslBeastActorRegistration.IsBeast(target))return false;
        // 非妖兽对所有妖兽一律视为敌对
        if (!MclslBeastActorRegistration.IsBeast(caster) && MclslBeastActorRegistration.IsBeast(target)) return true;
        if (caster.attack_target == target || target.attack_target == caster) return true;
        if (caster.kingdom!=null&&target.kingdom!=null)return caster.kingdom.isEnemy(target.kingdom);
        return caster.attack_target==target || target.attack_target==caster;
    }
    // Former Lock built Where/GroupBy/Select/OrderBy/ThenBy iterators plus a
    // ToArray on every spell/talisman cast. This does one manual pass with id
    // dedup, a cached comparator sorted by (distance-to-main, id) and a Take cap.
    private static Actor _lockAnchorActor;
    private static readonly Comparison<Actor> LockComparison=CompareLock;
    private static int CompareLock(Actor a,Actor b)
    {
        float da=(a.current_position-_lockAnchorActor.current_position).sqrMagnitude;
        float db=(b.current_position-_lockAnchorActor.current_position).sqrMagnitude;
        int c=da.CompareTo(db);
        return c!=0?c:MclslActorAccessor.Id(a).CompareTo(MclslActorAccessor.Id(b));
    }
    internal static Actor[] Lock(Actor caster,Actor main,int maximum)
    {
        long sample=MclslPerformanceProbe.Begin();
        try { return LockCore(caster,main,maximum); }
        finally { MclslPerformanceProbe.End("战斗.法术目标锁定",sample); }
    }
    private static Actor[] LockCore(Actor caster,Actor main,int maximum)
    {
        if(!IsEnemy(caster,main))return Array.Empty<Actor>();
        if(maximum<=1 || main.current_tile==null)return new[]{main};
        long mainId=MclslActorAccessor.Id(main);
        var seen=new HashSet<long>{mainId};
        var candidates=new List<Actor>();
        foreach(Actor x in Finder.getUnitsFromChunk(main.current_tile,1,3f,false))
        {
            if(x==null||x==main)continue;
            if(!IsEnemy(caster,x))continue;
            if(!seen.Add(MclslActorAccessor.Id(x)))continue;
            candidates.Add(x);
        }
        if(candidates.Count>1){_lockAnchorActor=main;candidates.Sort(LockComparison);}
        int take=Math.Min(maximum-1,candidates.Count);
        var result=new List<Actor>(take+1){main};
        for(int i=0;i<take;i++)result.Add(candidates[i]);
        return result.ToArray();
    }
}
internal static class MclslSpellCasting
{
    internal const string SaveKey="mclsl.080.spells.pending.v2";
    private const int MaximumPending=16384;
    private static readonly SortedSet<MclslPendingSpell> Pending=new(Comparer<MclslPendingSpell>.Create((left,right)=>
    {
        int time=NextAt(left).CompareTo(NextAt(right));
        return time!=0?time:string.CompareOrdinal(left.Id,right.Id);
    }));
    private static double NextAt(MclslPendingSpell spell)=>spell.Phase==0&&!spell.Dot?spell.TravelAt:spell.Due;
    private static bool _locked;
    private static MclslModuleLoadState _loadState;
    private static int _generation;
    private static int _reservedPending;
    private static MclslPendingSpell _settlingSpell;
    [ThreadStatic] internal static string DamageLaw;
    [ThreadStatic] internal static bool DotDamage;
    internal static double Now=>MclslBeastLoot.Now;
    internal static void Clear(){_generation++;Pending.Clear();_reservedPending=0;_settlingSpell=null;_locked=false;_loadState.Clear();DamageLaw=null;DotDamage=false;}
    internal static void Load()
    {
        Clear();if(World.world?.map_stats==null)return;
        if(World.world.map_stats.custom_data==null){_loadState.Ready();return;}
        World.world.map_stats.custom_data.get(SaveKey,out string raw,"");if(raw.Length==0){_loadState.Ready();return;}
        try
        {
            var spells=JsonConvert.DeserializeObject<List<MclslPendingSpell>>(raw);
            if(spells==null||spells.Count>MaximumPending)
                throw new InvalidOperationException("待命中法术存档格式错误");
            // IDs must be unique even when their due times differ.
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var spell in spells)
            {
                if(spell==null||spell.Version!=2||string.IsNullOrEmpty(spell.Id)||string.IsNullOrEmpty(spell.Spell)
                    ||string.IsNullOrEmpty(spell.Law)||spell.Caster<=0||spell.Targets==null||spell.Targets.Count==0||spell.Targets.Count>8
                    ||spell.Phase<0||spell.Phase>1||!Finite(spell.Damage)||spell.Damage<0||!Finite(spell.Strength)||spell.Strength<0
                    ||!Finite(spell.StatusDuration)||spell.StatusDuration<0||!Finite(spell.Due)||!Finite(spell.Born)||!Finite(spell.TravelAt)
                    ||(spell.Dot ? spell.RemainingPulses<1||spell.RemainingPulses>4||spell.Targets.Count!=1||spell.Phase!=1
                        : spell.RemainingPulses!=0)
                    ||(spell.Status=="mclsl_beast_talisman_fire"&&spell.Targets.Count!=1)||!ids.Add(spell.Id))
                    throw new InvalidOperationException("待命中法术记录错误");
                for(int i=0;i<spell.Targets.Count;i++)
                {
                    if(spell.Targets[i]<=0)throw new InvalidOperationException("待命中目标错误");
                    for(int j=0;j<i;j++)if(spell.Targets[i]==spell.Targets[j])throw new InvalidOperationException("重复待命中目标");
                }
                Pending.Add(spell);
            }
            _loadState.Ready();
        }
        catch(Exception ex){Pending.Clear();_locked=true;_loadState.Fault();MclslDiagnostics.Error("spell-pending-load",ex.ToString());}
    }
    private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
    internal static void Save()
    {
        if(!_loadState.CanSave||_locked||World.world?.map_stats==null)return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(Pending));
    }
    internal static void CleanseDamageOverTime(Actor actor)
    {
        long id=MclslActorAccessor.Id(actor);
        // A damage callback may cleanse the record currently outside the set.
        if(_settlingSpell!=null&&_settlingSpell.Dot&&_settlingSpell.Law=="火"&&_settlingSpell.Targets.Contains(id))
            _settlingSpell.RemainingPulses=0;
        // Collect matches first (a SortedSet cannot be mutated while enumerating),
        // replacing Pending.Where(...).ToArray(); the list is only allocated when a
        // damage-over-time record actually matches.
        List<MclslPendingSpell> matches=null;
        foreach(var pending in Pending)
            if(pending.Dot&&(pending.Law=="火"||pending.Status=="mclsl_beast_talisman_fire"))
                (matches??=new()).Add(pending);
        if(matches==null)return;
        foreach(var pending in matches)
        {pending.Targets.Remove(id);if(pending.Targets.Count==0)Pending.Remove(pending);}
    }
    // Build the stored target-id list directly instead of targets.Select(...).ToList(),
    // dropping the transient Select iterator on every queued cast/talisman.
    private static List<long> Ids(Actor[] actors)
    {
        var ids=new List<long>(actors.Length);
        for(int i=0;i<actors.Length;i++)ids.Add(MclslActorAccessor.Id(actors[i]));
        return ids;
    }
    internal static bool IsProjectile(string id)=>id is "S001" or "S004" or "S013" or "S015" or "S017";
    internal static bool IsBeam(string id)=>id is "S009" or "S026";
    internal static bool IsArea(string id)=>id is "S010" or "S012" or "S019" or "S022" or "S023";
    internal static bool Queue(Actor caster,Actor main,MclslSpellDefinition spell)
        => !MclslBeastActorRegistration.IsBeast(caster) && QueueCore(caster,main,spell,false);

    internal static bool QueueTalent(Actor caster,Actor main,MclslSpellDefinition spell)
        => MclslBeastActorRegistration.IsBeast(caster)
            && spell.Id == MclslActorAccessor.GetString(caster,MclslBeastCultivation.TalentKey)
            && QueueCore(caster,main,spell,true);

    private static bool CanEnqueue => _loadState.Status==MclslModuleLoadStatus.Ready
        && !_locked && Pending.Count<MaximumPending-_reservedPending;

    private static bool QueueCore(Actor caster,Actor main,MclslSpellDefinition spell,bool talent)
    {
        if(!CanEnqueue)return false;
        var targets=MclslCombatTargets.Lock(caster,main,spell.MaxTargets);if(targets.Length==0)return false;
        float strength=MclslBeastActorRegistration.IsBeast(caster)
            ?1+0.05f*(Math.Max(1,MclslActorAccessor.GetInt(caster,MclslBeastCultivation.TalentLevelKey,1))-1):MclslSpellProgression.Strength(caster,spell.Id);
        float damage=Math.Max(1,caster.stats.get("damage"))*spell.Power*strength;
        if(spell.Law=="火")damage*=1+MclslPhysiqueSystem.FireDamagePercent(caster)/100f;
        if(spell.Id=="S010")damage*=1.5f;if(spell.Id=="S012")damage*=2;
        damage*=1+MclslBeastItemEffects.ArtifactBonus(caster,spell.Law)+MclslBeastItemEffects.ArtifactBonus(caster,"spell");
        double preparation=IsArea(spell.Id)?0.3:IsProjectile(spell.Id)?0.12:0.15;
        double travel=IsArea(spell.Id)?0.2:IsProjectile(spell.Id)?Math.Clamp((main.current_position-caster.current_position).magnitude/30f,0.15f,0.45f):0.15;
        var pending=new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=MclslActorAccessor.Id(caster),Spell=spell.Id,BeastTalent=talent,
            Law=spell.Law,Damage=damage,Strength=strength,Targets=Ids(targets),Born=Now,TravelAt=Now+preparation,Due=Now+preparation+travel,
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
        if(!CanEnqueue||caster.attack_target is not Actor main)return false;
        var targets=MclslCombatTargets.Lock(caster,main,effect=="wind"?2:effect=="space_area"?3:1);if(targets.Length==0)return false;
        string law=effect switch {"fire"=>"火","water"=>"水","wind"=>"风","yin"=>"阴","metal"=>"金","space" or "space_area"=>"空间",_=>"physical"};
        string visual=effect switch {"fire"=>"S004","water"=>"S013","wind"=>"S015","yin"=>"S008","space" or "space_area"=>"S022",_=>"S001"};
        Pending.Add(new MclslPendingSpell {Id=Guid.NewGuid().ToString("N"),Caster=MclslActorAccessor.Id(caster),Spell=visual,Law=law,
            Targets=Ids(targets),Damage=Math.Max(1,caster.stats.get("damage"))*(0.5f+grade*0.3f)*(1+MclslBeastItemEffects.ArtifactBonus(caster,law)),
            Strength=1,Born=Now,TravelAt=Now+0.12,Due=Now+0.3,Status=effect=="yin"?"mclsl_beast_yin_bind":effect=="water"?"mclsl_beast_water_slow":effect=="fire"?"mclsl_beast_talisman_fire":null,StatusDuration=1+grade*0.5f});
        MclslSpellEffect.Show(caster,main,visual,phase:"cast",duration:0.12f);return true;
    }
    internal static void Tick()
    {
        if(_reservedPending!=0||_locked||_loadState.Status!=MclslModuleLoadStatus.Ready||World.world==null||Config.paused)return;
        int processed=0;
        int generation=_generation;
        double now=Now;
        // Cooperative frame budget alongside the existing 128-per-tick cap: an
        // unresolved burst of due spells yields to the next frame instead of
        // overrunning. The SortedSet keeps due entries, so nothing is dropped and
        // the per-tick processing budget is unchanged.
        while(Pending.Count>0 && processed<128 && !MclslFrameDeadline.Expired)
        {
            var spell=Pending.Min;
            if(NextAt(spell)>now)break;
            Pending.Remove(spell);
            // Damage callbacks can queue another spell. Keep this record's slot
            // reserved until it is either reinserted or completely consumed.
            _reservedPending=1;
            _settlingSpell=spell;
            try
            {
                if(!MclslActorRegistry.ResolveKnownOrWorld(spell.Caster,out var caster)||!MclslActorAccessor.Alive(caster))continue;
                if(MclslBeastActorRegistration.IsBeast(caster) && !spell.BeastTalent)continue;
                if(spell.Phase==0 && Now>=spell.TravelAt && !spell.Dot)
                {
                    spell.Phase=1;
                    if(IsProjectile(spell.Spell))
                    {
                        foreach(long targetId in spell.Targets)
                            if(MclslActorRegistry.ResolveKnownOrWorld(targetId,out var lockedTarget)&&MclslCombatTargets.IsEnemy(caster,lockedTarget))
                                MclslSpellEffect.Show(caster,lockedTarget,spell.Spell,phase:"travel",duration:(float)(spell.Due-spell.TravelAt));
                    }
                    else if(spell.Targets.Count>0&&MclslActorRegistry.ResolveKnownOrWorld(spell.Targets[0],out var main)&&MclslCombatTargets.IsEnemy(caster,main))
                        MclslSpellEffect.Show(caster,main,spell.Spell,phase:"area",duration:(float)(spell.Due-spell.TravelAt));
                }
                if(now<spell.Due){Pending.Add(spell);continue;}
                processed++;
                bool continueFire=false;
                bool hit=false;
                foreach(long targetId in spell.Targets)
                {
                    if(!MclslActorRegistry.ResolveKnownOrWorld(targetId,out var target)||!MclslCombatTargets.IsEnemy(caster,target))continue;
                    hit=true;
                    if(!spell.Dot)MclslSpellEffect.Show(caster,target,spell.Spell,phase:"hit",duration:0.4f);
                    DealDamage(caster,target,spell.Damage,spell.Law,spell.Dot);
                    if(generation!=_generation)return;
                    if(!MclslActorAccessor.Alive(target))continue;
                    if(spell.Status=="mclsl_beast_talisman_fire"&&!spell.Dot)
                        continueFire=true;
                    else if(spell.Status!=null)target.addStatusEffect(spell.Status,spell.StatusDuration);
                    if(!spell.Dot && spell.Terrain)MclslSpellTerrain.Queue(target.current_tile,spell.Spell);
                }
                if(spell.Spell=="S009"&&!spell.Dot)caster.restoreHealthPercent(0.05f*spell.Strength);
                // The record is outside the SortedSet while its sort key changes.
                // Reuse the primary's object/list instead of allocating four children.
                if(continueFire)
                {
                    spell.Dot=true;spell.Phase=1;spell.Status=null;spell.Terrain=false;
                    spell.RemainingPulses=4;spell.Damage*=0.25f/4;spell.Due=now+1;
                    Pending.Add(spell);
                }
                else if(spell.Dot&&hit&&spell.RemainingPulses>1
                    &&MclslActorRegistry.ResolveKnownOrWorld(spell.Targets[0],out var survivor)&&MclslActorAccessor.Alive(survivor))
                {
                    spell.RemainingPulses--;
                    spell.Due+=1;
                    Pending.Add(spell);
                }
                }
            finally { _reservedPending=0;_settlingSpell=null; }
        }
    }
    internal static void DealDamage(Actor caster,Actor target,float damage,string law,bool dot=false)
    {
        string oldLaw=DamageLaw;bool oldDot=DotDamage;DamageLaw=law;DotDamage=dot;
        try {MclslSpellSystem.DeliverDamage(caster,target,damage,law=="火"?AttackType.Fire:law=="physical"?AttackType.Weapon:AttackType.Divine);}
        finally {DamageLaw=oldLaw;DotDamage=oldDot;}
    }
}
