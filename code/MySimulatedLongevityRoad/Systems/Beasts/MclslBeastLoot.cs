using System;
using System.Collections.Generic;
using System.Globalization;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Systems.Death;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslBeastLootDelivery
{
    public long VictimId,RecipientId;
    public int Year;
    public Dictionary<string,int> Items=new();
}
internal sealed class MclslBeastLootState
{
    public int Version=1;
    public List<long> Committed=new();
    public List<MclslBeastLootDelivery> Pending=new();
}
internal static class MclslBeastLoot
{
    private const string Key="mclsl.beasts.loot.v1";
    private static MclslBeastLootState _state=new();
    private static HashSet<long> _committed=new();
    private static int _retry;
    private static bool _locked;
    internal static double Now=>World.world==null?0:Convert.ToDouble(World.world.getCurWorldTime());
    internal static void Clear(){_state=new();_committed.Clear();_retry=0;_locked=false;}
    internal static void Load()
    {
        Clear();if(World.world?.map_stats?.custom_data==null)return;
        World.world.map_stats.custom_data.get(Key,out string raw,"");if(raw.Length==0)return;
        try
        {
            var state=JsonConvert.DeserializeObject<MclslBeastLootState>(raw);
            if(state?.Version!=1||state.Committed==null||state.Pending==null)throw new InvalidOperationException("掉落存档格式错误");
            _state=state;_committed=new(state.Committed);
        }
        catch(Exception ex){_locked=true;MclslDiagnostics.Error("beast-loot-load",ex.ToString());}
    }
    internal static void Save()
    {
        if(_locked||World.world?.map_stats==null)return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(Key,JsonConvert.SerializeObject(_state));
    }
    internal static void ObserveHit(Actor victim,BaseSimObject attacker)
    {
        // Native death is deferred. Hits arriving after lethal damage must
        // not replace the source while the zero-health actor still isAlive.
        if(!MclslBeastActorRegistration.IsOrdinary(victim)||victim.getHealth()<=0)return;
        Actor owner=attacker as Actor;
        if(owner!=null)
        {
            // Mod summons can explicitly bind their authoritative owner id.
            long.TryParse(MclslActorAccessor.GetString(owner,"mclsl.summon.owner_id"),out long ownerId);
            if(ownerId<=0)ownerId=MclslActorAccessor.GetInt(owner,"mclsl.summon.owner_id",0);
            if(ownerId>0 && MclslActorRegistry.ResolveKnownOrWorld(ownerId,out var resolved))owner=resolved;
        }
        MclslActorAccessor.Set(victim,"mclsl.beast.last_source",(owner==null?0:MclslActorAccessor.Id(owner)).ToString(CultureInfo.InvariantCulture));
        MclslActorAccessor.Set(victim,"mclsl.beast.last_hit_time",Now.ToString("R",CultureInfo.InvariantCulture));
    }
    internal static void Commit(Actor victim,string scriptedCause,AttackType deathType)
    {
        // A recent attack must not turn a later natural death or editor
        // deletion into a kill reward. Damage deaths retain their native type.
        if(deathType is AttackType.Age or AttackType.None or AttackType.Starvation)return;
        if(_locked || !MclslBeastActorRegistration.IsOrdinary(victim)||!string.IsNullOrWhiteSpace(scriptedCause))return;
        long id=MclslActorAccessor.Id(victim);
        if(id<=0 || _committed.Contains(id)||MclslActorAccessor.GetInt(victim,"mclsl.beast.loot_committed",0)!=0)return;
        if(!long.TryParse(MclslActorAccessor.GetString(victim,"mclsl.beast.last_source"),out long recipient)
            || !double.TryParse(MclslActorAccessor.GetString(victim,"mclsl.beast.last_hit_time"),NumberStyles.Float,CultureInfo.InvariantCulture,out double time)
            || Now<time || Now-time>30 || !MclslActorRegistry.ResolveKnownOrWorld(recipient,out var killer)
            || !MclslActorAccessor.Alive(killer)||!MclslEligibility.IsCivilizedActor(killer))return;
        var beast=MclslBeastCatalog.ForAsset(victim.asset.id);
        var row=MclslBeastContent.BySpecies[beast.Id];
        int realm=MclslRealmIds.Index(MclslActorAccessor.Realm(victim));
        int grade=MclslBeastContent.MaterialGrade(realm),rank=Math.Clamp(MclslActorAccessor.GetInt(victim,MclslBeastWorld.RankKey),0,2),quantity=rank+1;
        var delivery=new MclslBeastLootDelivery {VictimId=id,RecipientId=recipient,Year=MclslRuntime.CurrentYear()};
        delivery.Items.Add(MclslBeastContent.Common("blood",grade),quantity);
        if(Roll(id,"bone")<50)delivery.Items.Add(MclslBeastContent.Common("bone",grade),quantity);
        if(Roll(id,"skin")<50)delivery.Items.Add(MclslBeastContent.Common("skin",grade),quantity);
        int coreChance=realm<2?0:realm==2?30:realm==3?50:100;
        if(Roll(id,"core")<coreChance)delivery.Items.Add(MclslBeastContent.Common("core",grade),quantity);
        if(rank==2 || Roll(id,"signature")<(rank==1?50:25))delivery.Items.Add(row.MaterialId,rank==2?2:1);
        _state.Pending.Add(delivery);_committed.Add(id);_state.Committed.Add(id);
        MclslActorAccessor.Set(victim,"mclsl.beast.loot_committed",1);
        if(_state.Committed.Count>8192){long old=_state.Committed[0];_state.Committed.RemoveAt(0);_committed.Remove(old);}
        Deliver(delivery);Save();
    }
    private static int Roll(long id,string suffix)=>MclslBeastWorld.Roll(id+"|loot|"+suffix,100);
    private static bool Deliver(MclslBeastLootDelivery delivery)
    {
        if(!MclslActorRegistry.ResolveKnownOrWorld(delivery.RecipientId,out var actor)||!MclslActorAccessor.Alive(actor))
        { _state.Pending.Remove(delivery);return true; }
        if(MclslBagSystem.IsLocked(actor))return false;
        MclslBagSystem.BeginTransaction(actor);
        try
        {
            var bag=MclslBagSystem.Read(actor);
            foreach(var reward in delivery.Items)
                if(reward.Value<=0 || MclslItemCatalog.Get(reward.Key)==null)throw new InvalidOperationException("非法妖兽奖励");
            string backup=JsonConvert.SerializeObject(bag.Items);
            try
            { foreach(var reward in delivery.Items)MclslBagSystem.Add(bag,reward.Key,reward.Value,acquiredYear:delivery.Year);MclslBagSystem.Write(actor,bag); }
            catch {bag.Items=JsonConvert.DeserializeObject<List<MclslOwnedItem>>(backup);throw;}
            _state.Pending.Remove(delivery);
            foreach(var reward in delivery.Items)
                MclslWorldRunRepository.AddItemAcquisitionEvent(delivery.Year,actor,reward.Key,reward.Value,"击杀妖兽");
            return true;
        }
        finally{MclslBagSystem.EndTransaction(actor);}
    }
    internal static void Tick()
    {
        if(_locked||++_retry%120!=0)return;
        for(int i=Math.Min(_state.Pending.Count,16)-1;i>=0;i--)Deliver(_state.Pending[i]);
    }
}
