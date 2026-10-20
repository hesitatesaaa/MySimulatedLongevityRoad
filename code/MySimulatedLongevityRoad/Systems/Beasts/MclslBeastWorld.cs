using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;
using Newtonsoft.Json;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslBeastWaveState
{
    public string Id, Theme, Status="生成中";
    public int Level,X,Y,Count,Cursor,Year;
    public long TargetCityId;
    public List<long> Actors=new();
}
internal sealed class MclslBeastWorldState
{
    public int Version=1,Unlocked=1,LastWaveYear,LastNaturalYear;
    public List<MclslBeastWaveState> Waves=new();
    public List<MclslNaturalBeastSpawn> NaturalPending=new();
}
internal sealed class MclslNaturalBeastSpawn
{
    public string Species;
    public int X,Y;
}

internal static class MclslBeastWorld
{
    private const string SaveKey="mclsl.beasts.world.v1";
    internal const string SourceKey="mclsl.beast.source", RankKey="mclsl.beast.rank";
    internal static MclslBeastWorldState State=new();
    private static MapBox _wildWorld;
    private static int _auditFrame;
    private static int _manualRequest;
    private static float _nextSpawnTime;
    private static bool _locked;
    private static readonly ConcurrentDictionary<long,byte> PendingRankStats=new();
    internal static bool WriteLocked=>_locked;
    internal static bool Enabled=true;
    internal static int Interval=5,MaximumLevel=7,AutomaticLimit=512,NaturalLimit=128,ManualRealm,MaximumWaveCount=64;
    internal static float CountMultiplier=1;
    internal static readonly int[] Weights=MclslBeastRules.Weights,Counts=MclslBeastRules.Counts;
    internal static IReadOnlyList<MclslBeastWaveState> Waves=>State.Waves;

    internal static int Roll(string key,int upper)
    {
        unchecked { uint h=2166136261; foreach(char c in key) h=(h^c)*16777619;
            h^=h>>16;h*=0x7feb352d;h^=h>>15;h*=0x846ca68b;h^=h>>16;
            return upper<=0?0:(int)(h%(uint)upper); }
    }
    internal static void EnsureWildKingdom()
    {
        if(AssetManager.kingdoms==null) return;
        var asset=AssetManager.kingdoms.get("mclsl_wild_beasts");
        if(asset==null)
        {
            asset=new KingdomAsset { id="mclsl_wild_beasts",mobs=true,default_kingdom_color=new ColorAsset("#7a2636") };
            asset.addTag("mclsl_wild_beasts");asset.addFriendlyTag("mclsl_wild_beasts");
            AssetManager.kingdoms.add(asset);
        }
        if(World.world!=null && !ReferenceEquals(_wildWorld,World.world))
        { if(World.world.kingdoms_wild.get(asset.id)==null)World.world.kingdoms_wild.newWildKingdom(asset);_wildWorld=World.world; }
    }
    internal static void Load()
    {
        Clear();EnsureWildKingdom();
        if(World.world?.map_stats?.custom_data==null) return;
        World.world.map_stats.custom_data.get(SaveKey,out string saved,"");
        if(saved.Length==0) return;
        try
        {
            var loaded=JsonConvert.DeserializeObject<MclslBeastWorldState>(saved);
            if(loaded?.Version!=1 || loaded.Waves==null) throw new InvalidOperationException("兽潮存档格式错误");
            State=loaded;
            State.NaturalPending??=new();
            State.Unlocked=Math.Clamp(State.Unlocked,1,7);
            foreach(var wave in State.Waves)
            {
                wave.Level=Math.Clamp(wave.Level,1,7); wave.Count=Math.Clamp(wave.Count,0,4096);
                wave.Cursor=Math.Clamp(wave.Cursor,0,wave.Count);wave.Actors??=new();
                wave.Count=Math.Min(wave.Count,Math.Max(wave.Cursor,MaximumWaveCount));
                if(wave.Status=="已结束")wave.Actors.Clear();
            }
        }
        catch(Exception ex) { _locked=true;MclslDiagnostics.Error("beast-world-load",ex.ToString()); }
    }
    internal static void Save()
    {
        if(_locked || World.world?.map_stats==null) return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(State));
    }
    internal static void Clear() { State=new();_wildWorld=null;_auditFrame=0;_manualRequest=0;_nextSpawnTime=0;_locked=false;PendingRankStats.Clear();MclslBeastSpawnSystem.Clear(); }
    internal static void RequestManual(int level)=>_manualRequest=Math.Clamp(level,1,7);
    internal static void ObserveProgress()
    {
        foreach(var actor in World.world.units.getSimpleList())
            if(MclslActorAccessor.Alive(actor) && MclslEligibility.IsCivilizedActor(actor)
                && !actor.asset.id.StartsWith("mclsl_named_",StringComparison.Ordinal)
                && !MclslImmortalActorRegistration.IsImmortal(actor))
                State.Unlocked=Math.Max(State.Unlocked,MclslRealmIds.Index(MclslActorAccessor.Realm(actor))+1);
    }
    internal static int CountAutomatic(bool naturalOnly=false,int region=-1)
    {
        int count=0;
        foreach(var actor in World.world.units.getSimpleList())
        {
            if(!MclslActorAccessor.Alive(actor)||!MclslBeastActorRegistration.IsOrdinary(actor))continue;
            string source=MclslActorAccessor.GetString(actor,SourceKey);
            if(source!="natural" && !source.StartsWith("wave:",StringComparison.Ordinal))continue;
            if(naturalOnly && source!="natural")continue;
            if(region>=0 && MclslBeastHabitatIndex.Region(actor.current_tile)!=region)continue;
            count++;
        }
        return count;
    }
    internal static void TickAnnual(int year)
    {
        ObserveProgress();
        if(_locked || !Enabled || year<=0 || State.LastWaveYear>=year || year%Math.Max(1,Interval)!=0) return;
        State.LastWaveYear=year;
        int ceiling=Math.Min(State.Unlocked,MaximumLevel),total=Weights.Take(ceiling).Sum();
        int pick=Roll(World.world.map_stats.name+"|wave|"+year,total),level=MclslBeastRules.WeightedLevel(ceiling,pick);
        Trigger(level,year);
    }
    internal static bool Trigger(int level,int year,WorldTile requested=null)
    {
        if(_locked || World.world?.units==null || State.Waves.Any(x=>x.Status=="生成中" || x.Year==year && x.Status!="已结束")
            || State.Waves.Count(x=>x.Status!="已结束")>=2) return false;
        level=Math.Clamp(level,1,7);
        var options=new List<(string Theme,WorldTile Tile)>();
        foreach(string theme in new[]{"山林","水域","阴影","神兽"})
        {
            if(theme=="神兽" && level<3)continue;
            foreach(var beast in Pool(theme,level-1))
            {
                var tile=requested!=null && MclslBeastSpawnSystem.Matches(requested,beast.Habitat)?requested:MclslBeastHabitatIndex.Find(beast,year+level);
                if(tile!=null){options.Add((theme,tile));break;}
            }
        }
        if(options.Count==0) return false;
        var option=options[Roll(year+"|theme|"+State.Waves.Count,options.Count)];
        int count=MclslBeastRules.PlannedCount(level,CountMultiplier,AutomaticLimit,CountAutomatic(),State.Waves.Sum(x=>x.Count-x.Cursor)+State.NaturalPending.Count,
            MapBox.width*MapBox.height,MaximumWaveCount);
        if(count==0)return false;
        var wave=new MclslBeastWaveState { Id=Guid.NewGuid().ToString("N"),Level=level,Theme=option.Theme,
            X=option.Tile.pos.x,Y=option.Tile.pos.y,Count=count,Year=year };
        Actor cityActor=MclslActorRegistry.CityRepresentatives.OrderBy(x=>x.current_tile==null?float.MaxValue:
            ((UnityEngine.Vector3)x.current_position-option.Tile.posV3).sqrMagnitude).FirstOrDefault();
        wave.TargetCityId=cityActor?.city?.data?.id??0;
        State.Waves.Add(wave);
        MclslWorldRunRepository.AddEvent(year,"beast_tide",level+"级"+option.Theme+"兽潮", "妖兽潮在("+wave.X+","+wave.Y+")出现，计划生成"+count+"只妖兽。",cityActor);
        Save();return true;
    }
    private static IEnumerable<MclslBeastDefinition> Pool(string theme,int realm)
        =>MclslBeastCatalog.All.Where(b=>MclslBeastContent.BySpecies[b.Id].MinRealm<=realm && theme switch
        {
            "水域"=>b.Habitat is MclslBeastHabitat.DeepSea or MclslBeastHabitat.RiverLake or MclslBeastHabitat.SeaRift,
            "阴影"=>b.Id is "yingying" or "mo" or "moqilin" or "goushou",
            "神兽"=>MclslBeastContent.BySpecies[b.Id].MinRealm>=2,
            _=>b.Habitat is not (MclslBeastHabitat.DeepSea or MclslBeastHabitat.RiverLake or MclslBeastHabitat.SeaRift or MclslBeastHabitat.RedMoon)
        });
    internal static Actor Spawn(MclslBeastDefinition beast,WorldTile tile,int realm,string source,int rank)
    {
        if(tile==null || !MclslBeastSpawnSystem.Matches(tile,beast.Habitat))return null;
        var actor=World.world.units.createNewUnit(beast.BeastAssetId,tile,false,0f,null,null,true,false,false,false);
        if(actor?.data==null)return null;
        MclslActorAccessor.Set(actor,SourceKey,source);MclslActorAccessor.Set(actor,RankKey,rank);
        MclslBeastActorRegistration.EnsureCultivationSeed(actor);
        MclslBeastCultivation.SetRealm(actor,MclslRealmIds.Ordered[Math.Clamp(realm,0,6)],MclslRuntime.CurrentYear());
        ApplyRankStats(actor);
        return actor;
    }
    internal static void Tick()
    {
        if(_locked || World.world?.units==null || Config.paused)return;
        if(UnityEngine.Time.time<_nextSpawnTime)return;
        _nextSpawnTime=UnityEngine.Time.time+0.15f;
        if(_manualRequest>0 && MclslBeastHabitatIndex.Refresh(MclslRuntime.CurrentYear(),false))
        {
            int level=_manualRequest;_manualRequest=0;
            if(!Trigger(level,MclslRuntime.CurrentYear()))MclslDiagnostics.Error("manual-beast-wave","没有符合主题的栖息地，或已达妖兽上限。");
        }
        var watch=Stopwatch.StartNew();int made=0;
        foreach(var wave in State.Waves)
        {
            if(wave.Status!="生成中")continue;
            while(wave.Cursor<wave.Count && made<2 && watch.Elapsed.TotalMilliseconds<2)
            {
                int rank=MclslBeastRules.Role(wave.Cursor,wave.Count);
                if(CountAutomatic()>=AutomaticLimit){wave.Cursor=wave.Count;break;}
                int realm=rank==0?Math.Max(0,wave.Level-2):wave.Level-1;
                var center=World.world.GetTileSimple(wave.X,wave.Y);
                var candidates=Pool(wave.Theme,realm).Select(b=>(Beast:b,Tile:MclslBeastHabitatIndex.FindNear(b,center,wave.Cursor))).Where(x=>x.Tile!=null).ToArray();
                if(candidates.Length==0)
                {
                    realm=wave.Level-1;
                    candidates=Pool(wave.Theme,realm).Select(b=>(Beast:b,Tile:MclslBeastHabitatIndex.FindNear(b,center,wave.Cursor))).Where(x=>x.Tile!=null).ToArray();
                }
                if(candidates.Length==0){wave.Cursor=wave.Count;break;}
                var selected=candidates[Roll(wave.Id+"|"+wave.Cursor,candidates.Length)];
                var beast=selected.Beast;var tile=selected.Tile;
                var actor=Spawn(beast,tile,Math.Max(realm,MclslBeastContent.BySpecies[beast.Id].MinRealm),"wave:"+wave.Id,rank);
                if(actor!=null)
                {
                    wave.Actors.Add(MclslActorAccessor.Id(actor));
                    var target=MclslActorRegistry.CityRepresentatives.FirstOrDefault(x=>x.city?.data?.id==wave.TargetCityId && MclslActorAccessor.Alive(x));
                    if(target!=null && MclslCombatTargets.IsEnemy(actor,target))actor.setAttackTarget(target);
                }
                wave.Cursor++;made++;
            }
            if(wave.Cursor>=wave.Count)wave.Status="活动中";
            if(made>=2 || watch.Elapsed.TotalMilliseconds>=2)break;
        }
        if(++_auditFrame%20==0)
        {
            foreach(var wave in State.Waves)
                if(wave.Status=="活动中" && Remaining(wave)==0){wave.Status="已结束";wave.Actors.Clear();}
            if(State.Waves.Count>64)State.Waves.RemoveAll(x=>x.Status=="已结束" && x.Year<MclslRuntime.CurrentYear()-20);
        }
    }
    internal static int Remaining(MclslBeastWaveState wave)=>wave.Actors.Count(id=>MclslActorRegistry.ResolveKnownOrWorld(id,out var actor)&&MclslActorAccessor.Alive(actor));
    internal static void ApplyRankStats(Actor actor)
    {
        if(!MclslBeastActorRegistration.IsOrdinary(actor)||actor.stats==null)return;
        long id=MclslActorAccessor.Id(actor);
        if(!PendingRankStats.TryRemove(id,out _))return;
        int rank=MclslActorAccessor.GetInt(actor,RankKey,0);if(rank<=0)return;
        float factor=rank==1?1.15f:1.5f,oldMaximum=actor.getMaxHealth();
        actor.stats.set("health",actor.stats.get("health")*factor);
        actor.stats.set("damage",actor.stats.get("damage")*factor);
        if(oldMaximum>0)actor.data.health=(int)Math.Min(int.MaxValue,actor.data.health*(actor.getMaxHealth()/oldMaximum));
        PendingRankStats.TryRemove(id,out _);
    }
    internal static void ObserveDirtyStats(Actor actor)
    {
        // Worker callbacks enqueue only an id. Never touch UI, caches, or
        // gameplay state until the native calculation has returned.
        if(actor!=null && actor.isStatsDirty() && MclslBeastActorRegistration.IsOrdinary(actor))
            PendingRankStats[MclslActorAccessor.Id(actor)]=0;
    }
    internal static void ForgetRank(long id)=>PendingRankStats.TryRemove(id,out _);
}
