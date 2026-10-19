using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Visual;
using Newtonsoft.Json;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems.Combat;

// A spirit is deliberately not an Actor: no native population, drops or breeding hooks.
internal sealed class MclslSpiritProjection
{
    public long Owner,Target;
    public string Species,Profession;
    public float Durability;
    public double Expires,NextAttack,GuardUntil,GuardReady;
    [JsonIgnore] internal GameObject Object;
    [JsonIgnore] internal SpriteRenderer Renderer;
    [JsonIgnore] internal GameObject RimObject;
    [JsonIgnore] internal SpriteRenderer Rim;
    [JsonIgnore] internal Vector3 Position;
    [JsonIgnore] internal bool Reserved;
    [JsonIgnore] internal bool RimReserved;
}

internal static class MclslSpiritPact
{
    private const string Key="mclsl.battle.spirits.v1";
    private static readonly List<MclslSpiritProjection> Spirits=new();
    private static readonly Dictionary<long,List<MclslSpiritProjection>> Owned=new();
    private static readonly string[] Species={"baihu","qingluan","xuanwu","qinglong"};
    private static bool _locked;
    private static int _cursor;
    internal static int Count(Actor owner)
    {
        long id=MclslActorAccessor.Id(owner);int count=0;
        if(Owned.TryGetValue(id,out var owned))
            foreach(var spirit in owned)if(spirit.Expires>MclslSpellCasting.Now&&spirit.Durability>0)count++;
        return count;
    }
    internal static void Summon(Actor owner)
    {
        if(_locked||!MclslActorAccessor.Alive(owner))return;
        int limit=MclslBattleRules.SpiritLimit(MclslRealmIds.Index(MclslActorAccessor.Realm(owner)));
        long id=MclslActorAccessor.Id(owner);
        if(!Owned.TryGetValue(id,out var owned))Owned[id]=owned=new();
        for(int i=owned.Count-1;i>=0;i--)
            if(owned[i].Expires<=MclslSpellCasting.Now||owned[i].Durability<=0)
            {var old=owned[i];Hide(old);Spirits.Remove(old);owned.RemoveAt(i);}
        int count=Count(owner);
        for(int i=count;i<limit;i++)
        {
            var spirit=new MclslSpiritProjection{Owner=MclslActorAccessor.Id(owner),Species=Species[i],Profession=MclslBattleProfessionSystem.Main(owner),
                Durability=Math.Max(1,owner.getMaxHealth()*.2f),Expires=MclslSpellCasting.Now+30,
                NextAttack=MclslSpellCasting.Now+.7,Position=owner.current_position};
            if(Spirits.Count>=65536)break;
            Spirits.Add(spirit);owned.Add(spirit);
        }
    }
    internal static void Mark(Actor owner,Actor target)
    {
        if(!MclslCombatTargets.IsEnemy(owner,target))return;
        if(Owned.TryGetValue(MclslActorAccessor.Id(owner),out var owned))
            foreach(var spirit in owned)spirit.Target=MclslActorAccessor.Id(target);
    }
    internal static void Guard(Actor owner)
    {
        if(!Owned.TryGetValue(MclslActorAccessor.Id(owner),out var owned))return;
        foreach(var spirit in owned)
            if(spirit.Owner==MclslActorAccessor.Id(owner)&&spirit.Durability>0&&spirit.GuardReady<=MclslSpellCasting.Now)
            {spirit.GuardUntil=MclslSpellCasting.Now+3;break;}
    }
    internal static void ApplyGuard(Actor owner,ref float damage)
    {
        if(damage<=0)return;
        if(!Owned.TryGetValue(MclslActorAccessor.Id(owner),out var owned))return;
        foreach(var spirit in owned)
        {
            if(spirit.Owner!=MclslActorAccessor.Id(owner)||spirit.GuardUntil<=MclslSpellCasting.Now
                ||spirit.Expires<=MclslSpellCasting.Now||spirit.Durability<=0)continue;
            float blocked=Math.Min(damage*.6f,spirit.Durability);
            spirit.Durability-=blocked;damage-=blocked;spirit.GuardUntil=0;spirit.GuardReady=MclslSpellCasting.Now+8;
            spirit.Position=owner.current_position;
            MclslSpellEffect.Show(owner,null,"S003",phase:"hit",duration:.25f);break;
        }
    }
    internal static void Stampede(Actor owner,Actor target)
    {
        Mark(owner,target);
        // The main spell owns the total damage. These extra beast copies are presentation only.
        if(!Owned.TryGetValue(MclslActorAccessor.Id(owner),out var owned))return;
        foreach(var spirit in owned)
        {
            if(spirit.Owner!=MclslActorAccessor.Id(owner))continue;
            Sprite sprite=SpriteFor(spirit,true);
            if(sprite!=null)MclslBattleVisuals.BeastRush(owner,target,sprite,
                Math.Max(.01f,MclslActorAnimationCatalog.Get("actors/Beasts/"+spirit.Species+"/Beast")?.Scale??1));
        }
    }
    private static void Hide(MclslSpiritProjection spirit)
    {
        if(spirit.Object!=null)UnityEngine.Object.Destroy(spirit.Object);
        if(spirit.RimObject!=null)UnityEngine.Object.Destroy(spirit.RimObject);
        spirit.Object=null;spirit.Renderer=null;
        spirit.RimObject=null;spirit.Rim=null;
        if(spirit.Reserved){spirit.Reserved=false;MclslSpellEffect.ReleaseExternal();}
        if(spirit.RimReserved){spirit.RimReserved=false;MclslSpellEffect.ReleaseExternal();}
    }
    private static Sprite SpriteFor(MclslSpiritProjection spirit,bool attack)
    {
        string path="actors/Beasts/"+spirit.Species+"/Beast";
        var form=MclslActorAnimationCatalog.Get(path);
        var clip=MclslActorAnimationCatalog.Clip(path,attack?"attack":"walk");
        if(form==null||clip==null)return null;
        string frame=clip.Names[clip.FrameIndex(MclslSpellCasting.Now)];
        return MclslNativeActorFrames.Request(path+"/main/"+frame);
    }
    private static void Render(MclslSpiritProjection spirit,Actor owner,Actor target,int slot)
    {
        double now=MclslSpellCasting.Now;
        Vector3 end=target==null?(Vector3)owner.current_position:(Vector3)target.current_position;
        float angle=(float)(now*1.5+slot*1.7);
        end+=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*1.1f;
        // Deterministic simulated-time motion; no frame-rate-dependent world movement.
        spirit.Position=end;
        if(!MclslSpellEffect.Enabled||!MclslBattleVisuals.Visible(end)){Hide(spirit);return;}
        Sprite sprite=SpriteFor(spirit,target!=null&&now>=spirit.NextAttack-.25);
        if(sprite==null)return;
        if(spirit.Object==null)
        {
            if(!MclslSpellEffect.TryReserveExternal())return;
            spirit.Reserved=true;
            if(!MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(end,1,2,sprite,out spirit.Object,out spirit.Renderer))
            {Hide(spirit);return;}
        }
        spirit.Renderer.sprite=sprite;
        spirit.Renderer.color=new Color(.35f,.95f,.8f,.65f);
        spirit.Renderer.flipX=target!=null&&target.current_position.x<owner.current_position.x;
        var form=MclslActorAnimationCatalog.Get("actors/Beasts/"+spirit.Species+"/Beast");
        float scale=form?.Scale??1;
        spirit.Object.transform.localScale=Vector3.one*Math.Max(.01f,scale);
        spirit.Object.transform.position=end;
        if(spirit.RimObject==null&&MclslSpellEffect.TryReserveExternal())
        {
            spirit.RimReserved=true;
            if(!MclslWorldSpriteRenderLayer.TryCreateStaticWorldSprite(end,1,1,sprite,out spirit.RimObject,out spirit.Rim))
            {spirit.RimReserved=false;MclslSpellEffect.ReleaseExternal();}
        }
        if(spirit.RimObject!=null)
        {
            spirit.Rim.sprite=sprite;spirit.Rim.flipX=spirit.Renderer.flipX;
            spirit.Rim.color=new Color(.65f,1,.9f,.35f);
            spirit.RimObject.transform.position=end;
            spirit.RimObject.transform.localScale=spirit.Object.transform.localScale*1.08f;
        }
    }
    internal static void Tick()
    {
        if(_locked||World.world==null||Config.paused||Spirits.Count==0)return;
        int budget=Math.Min(64,Spirits.Count);
        while(budget-->0&&Spirits.Count>0)
        {
            if(_cursor>=Spirits.Count)_cursor=0;
            var spirit=Spirits[_cursor++];double now=MclslSpellCasting.Now;
            if(!MclslActorRegistry.ResolveKnownOrWorld(spirit.Owner,out var owner)||!MclslActorAccessor.Alive(owner)
                ||MclslBattleProfessionSystem.Main(owner)!=spirit.Profession||spirit.Expires<=now||spirit.Durability<=0)
            {
                Hide(spirit);Spirits.RemoveAt(--_cursor);
                if(Owned.TryGetValue(spirit.Owner,out var list)){list.Remove(spirit);if(list.Count==0)Owned.Remove(spirit.Owner);}
                continue;
            }
            Actor target=null;
            if(spirit.Target!=0)MclslActorRegistry.ResolveKnownOrWorld(spirit.Target,out target);
            if(!MclslCombatTargets.IsEnemy(owner,target))target=owner.attack_target as Actor;
            if(!MclslCombatTargets.IsEnemy(owner,target))target=null;
            if(target!=null&&(target.current_position-owner.current_position).sqrMagnitude>144)target=null;
            spirit.Target=target==null?0:MclslActorAccessor.Id(target);
            Render(spirit,owner,target,_cursor%4);
            if(target==null||now<spirit.NextAttack||MclslBattleCombat.Controlled(owner))continue;
            spirit.NextAttack=now+1.2; // Persist before damage callbacks: no replay after save/reload.
            bool previous=MclslBattleCombat.ProjectionDamage;
            MclslBattleCombat.ProjectionDamage=true;
            try{MclslSpellCasting.DealDamage(owner,target,Math.Max(1,owner.stats.get("damage"))*.35f,"木");}
            finally{MclslBattleCombat.ProjectionDamage=previous;}
            if(MclslSpellSystem.TryGet("C013",out var spell))MclslSpellProgression.RecordCast(owner,spell,Core.MclslRuntime.CurrentYear());
        }
    }
    internal static void RemoveOwner(Actor actor)
    {
        if(!Owned.TryGetValue(MclslActorAccessor.Id(actor),out var owned))return;
        foreach(var s in owned){Hide(s);Spirits.Remove(s);}
        Owned.Remove(MclslActorAccessor.Id(actor));_cursor=0;
    }
    internal static void Clear(){foreach(var spirit in Spirits)Hide(spirit);Spirits.Clear();Owned.Clear();_cursor=0;_locked=false;}
    internal static void Save()
    {
        if(_locked||World.world?.map_stats==null)return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(Key,JsonConvert.SerializeObject(Spirits));
    }
    internal static void Load()
    {
        Clear();if(World.world?.map_stats?.custom_data==null)return;
        World.world.map_stats.custom_data.get(Key,out string raw,"");if(raw.Length==0)return;
        try
        {
            var spirits=JsonConvert.DeserializeObject<List<MclslSpiritProjection>>(raw);
            if(spirits==null||spirits.Count>65536)throw new InvalidOperationException("灵契投影数量异常");
            var counts=new Dictionary<long,int>();
            foreach(var s in spirits)
            {
                if(s==null||Array.IndexOf(Species,s.Species)<0||float.IsNaN(s.Durability)||float.IsInfinity(s.Durability)
                    ||s.Durability<0||!Finite(s.Expires)||!Finite(s.NextAttack)||!Finite(s.GuardUntil)||!Finite(s.GuardReady))
                    throw new InvalidOperationException("灵契投影格式错误");
                counts.TryGetValue(s.Owner,out int count);if(count>=4)throw new InvalidOperationException("灵契投影超限");
                counts[s.Owner]=count+1;Spirits.Add(s);
                if(!Owned.TryGetValue(s.Owner,out var owned))Owned[s.Owner]=owned=new();
                owned.Add(s);
            }
        }
        catch(Exception ex){Spirits.Clear();Owned.Clear();_locked=true;Core.MclslDiagnostics.Error("spirit-load",ex.Message);}
    }
    private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
}
