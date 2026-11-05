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
    public int Version=1,Unlocked=1,LastWaveYear;
    public List<MclslBeastWaveState> Waves=new();
}

internal static class MclslBeastWorld
{
    private const string SaveKey="mclsl.080.beasts.world.v1";
    internal const string SourceKey="mclsl.beast.source", RankKey="mclsl.beast.rank";
    internal static MclslBeastWorldState State=new();
    private static MapBox _wildWorld;
    private static int _auditFrame;
    // Set only when a manual summon could not finish its habitat scan inside
    // the click. It outranks scheduled spawning so the player command is never
    // delayed by an ordinary wave.
    private static int _manualRequest;
    internal static string ManualFeedback {get;private set;}="";
    private static string TriggerFailure="";
    private static float _nextSpawnTime;
    private static bool _locked;
    private static MclslModuleLoadState _loadState;
    private static readonly ConcurrentDictionary<long,byte> PendingRankStats=new();
    // Catalogue membership and per-species MinRealm are static for the process
    // lifetime, so a (theme, realm) eligibility scan is cached instead of being
    // re-filtered on every wave-spawn attempt. Per-spawn candidate tuples are
    // collected into a reusable buffer rather than a fresh LINQ array.
    private static readonly Dictionary<(string Theme,int Realm),MclslBeastDefinition[]> PoolCache=new();
    private static readonly List<(MclslBeastDefinition Beast,WorldTile Tile)> WaveCandidates=new(16);
    internal static bool WriteLocked=>_locked;
    internal static bool HasManualRequest=>_manualRequest>0;
    internal static bool Enabled=true;
    internal static int Interval=5,MaximumLevel=7,AutomaticLimit=512,ManualRealm,MaximumWaveCount=64;
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
        if(World.world?.map_stats==null) return;
        if(World.world.map_stats.custom_data==null){_loadState.Ready();return;}
        World.world.map_stats.custom_data.get(SaveKey,out string saved,"");
        if(saved.Length==0){_loadState.Ready();return;}
        try
        {
            var loaded=JsonConvert.DeserializeObject<MclslBeastWorldState>(saved);
            if(loaded?.Version!=1 || loaded.Waves==null) throw new InvalidOperationException("兽潮存档格式错误");
            State=loaded;
            State.Unlocked=Math.Clamp(State.Unlocked,1,7);
            foreach(var wave in State.Waves)
            {
                wave.Level=Math.Clamp(wave.Level,1,7); wave.Count=Math.Clamp(wave.Count,0,4096);
                wave.Cursor=Math.Clamp(wave.Cursor,0,wave.Count);wave.Actors??=new();
                wave.Count=Math.Min(wave.Count,Math.Max(wave.Cursor,MaximumWaveCount));
                if(wave.Status=="已结束")wave.Actors.Clear();
            }
            _loadState.Ready();
        }
        catch(Exception ex) { State=new();_locked=true;_loadState.Fault();MclslDiagnostics.Error("beast-world-load",ex.ToString()); }
    }
    internal static void Save()
    {
        if(!_loadState.CanSave || _locked || World.world?.map_stats==null) return;
        World.world.map_stats.custom_data??=new SaveCustomData();
        World.world.map_stats.custom_data.set(SaveKey,JsonConvert.SerializeObject(State));
    }
    internal static void Clear() { State=new();_wildWorld=null;_auditFrame=0;_manualRequest=0;ManualFeedback="";_nextSpawnTime=0;_locked=false;_loadState.Clear();PendingRankStats.Clear();PoolCache.Clear();WaveCandidates.Clear();MclslBeastSpawnSystem.Clear(); }
    internal static void RequestManual(int level)
    {
        if(_locked){ManualFeedback="兽潮存档异常，暂不能召潮。";return;}
        if(World.world?.units==null){ManualFeedback="请先进入世界。";return;}
        level=Math.Clamp(level,1,7);
        int year=MclslRuntime.CurrentYear();
        if(!MclslBeastSurfaceIndex.Ready)
        {
            _manualRequest=level;
            ManualFeedback="正在扫描栖息地，随后立即引动"+level+"级兽潮。";
            return;
        }
        _manualRequest=0;
        ManualFeedback=Trigger(level,year)
            ?level+"级兽潮已引动，正在分批生成。":"召潮未成功："+TriggerFailure;
    }
    internal static void ObserveProgress() => MclslBeastPopulationIndex.PublishProgress();
    internal static int CountAutomatic(int region=-1)
        => MclslBeastPopulationIndex.Count(region);
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
        TriggerFailure="";
        if(_locked)return CannotTrigger("兽潮存档异常。");
        if(World.world?.units==null)return CannotTrigger("请先进入世界。");
        if(State.Waves.Any(x=>x.Status=="生成中"))return CannotTrigger("已有兽潮正在生成，请稍候。");
        if(State.Waves.Any(x=>x.Year==year&&x.Status!="已结束"))return CannotTrigger("本年已有活动兽潮，请待其结束或进入下一年。");
        if(State.Waves.Count(x=>x.Status!="已结束")>=2)return CannotTrigger("同时活动兽潮已达两场上限。");
        level=Math.Clamp(level,1,7);
        var options=new List<(string Theme,WorldTile Tile)>();
        foreach(string theme in new[]{"山林","水域","阴影","神兽"})
        {
            if(theme=="神兽" && level<3)continue;
            foreach(var beast in Pool(theme,level-1))
            {
                var tile=requested!=null && MclslBeastSpawnSystem.Matches(requested,beast.Surface)?requested:MclslBeastSurfaceIndex.Find(beast,year+level);
                if(tile!=null){options.Add((theme,tile));break;}
            }
        }
        if(options.Count==0)return CannotTrigger("没有符合该等级与主题的海陆生成位置。");
        var option=options[Roll(year+"|theme|"+State.Waves.Count,options.Count)];
        int count=MclslBeastRules.PlannedCount(level,CountMultiplier,AutomaticLimit,CountAutomatic(),State.Waves.Sum(x=>x.Count-x.Cursor),
            MapBox.width*MapBox.height,MaximumWaveCount);
        if(count==0)return CannotTrigger("已达自动妖兽总上限，暂无生成名额。");
        var wave=new MclslBeastWaveState { Id=Guid.NewGuid().ToString("N"),Level=level,Theme=option.Theme,
            X=option.Tile.pos.x,Y=option.Tile.pos.y,Count=count,Year=year };
        Actor cityActor=MclslActorRegistry.CityRepresentatives.OrderBy(x=>x.current_tile==null?float.MaxValue:
            ((UnityEngine.Vector3)x.current_position-option.Tile.posV3).sqrMagnitude).FirstOrDefault();
        wave.TargetCityId=cityActor?.city?.data?.id??0;
        State.Waves.Add(wave);
        string title=level+"级"+option.Theme+"兽潮";
        string species=string.Join("、",Pool(option.Theme,level-1).Select(b=>b.Name));
        MclslWorldRunRepository.AddEvent(year,"beast_tide",title,
            year+"年，妖兽潮在("+wave.X+","+wave.Y+")出现，计划生成"+count+"只；妖兽："+species+"。",
            wave.X,wave.Y,"兽潮发生地",cityActor?.city?.kingdom?.data?.name??string.Empty,"beast_tide|"+wave.Id);
        if(level>=6)MclslAnnouncementSystem.Enqueue(title+"于("+wave.X+","+wave.Y+")来袭！", "#FFB15E",10f,1,"beast_tide|"+wave.Id);
        Save();return true;
    }
    private static bool CannotTrigger(string reason){TriggerFailure=reason;return false;}
    private static IEnumerable<MclslBeastDefinition> Pool(string theme,int realm)
    {
        // Cache the filtered eligibility list per (theme, realm). Returning the
        // same array keeps Trigger/Tick/title building behaviour identical while
        // eliminating a catalogue-wide Where scan on every spawn attempt.
        if(!PoolCache.TryGetValue((theme,realm),out var cached))
        {
            cached=MclslBeastCatalog.All.Where(b=>MclslBeastContent.BySpecies[b.Id].MinRealm<=realm && theme switch
            {
                "水域"=>b.Surface==MclslBeastSurface.Water,
                "阴影"=>b.Id is "yingying" or "mo" or "moqilin" or "goushou",
                "神兽"=>MclslBeastContent.BySpecies[b.Id].MinRealm>=2,
                _=>b.Surface==MclslBeastSurface.Land && b.Id!="yingying"
            }).ToArray();
            PoolCache[(theme,realm)]=cached;
        }
        return cached;
    }
    // Fill the reusable candidate buffer with every (beast, valid habitat tile)
    // pair for one spawn attempt. The caller previously built this with
    // Select(...).Where(...).ToArray() on every attempt, allocating closures and
    // arrays; the tuple set and ordering are unchanged.
    private static void FillWaveCandidates(string theme,int realm,WorldTile center,int seed)
    {
        WaveCandidates.Clear();
        foreach(var b in Pool(theme,realm))
        {
            var tile=MclslBeastSurfaceIndex.FindNear(b,center,seed);
            if(tile!=null)WaveCandidates.Add((b,tile));
        }
    }
    internal static Actor Spawn(MclslBeastDefinition beast,WorldTile tile,int realm,string source,int rank)
    {
        if(tile==null || !MclslBeastSpawnSystem.Matches(tile,beast.Surface))return null;
        var actor=World.world.units.createNewUnit(beast.BeastAssetId,tile,false,0f,null,null,true,false,false,false);
        if(actor?.data==null)return null;
        MclslActorAccessor.Set(actor,SourceKey,source);MclslActorAccessor.Set(actor,RankKey,rank);
        MclslBeastActorRegistration.EnsureCultivationSeed(actor);
        string wantedRealm=MclslRealmIds.Ordered[Math.Clamp(realm,0,6)];
        if(MclslActorAccessor.Realm(actor)!=wantedRealm)
            MclslBeastCultivation.SetRealm(actor,wantedRealm,MclslRuntime.CurrentYear());
        ApplyRankStats(actor);
        MclslBeastPopulationIndex.Observe(actor);
        return actor;
    }
    internal static void Tick()
    {
        long sample=MclslPerformanceProbe.Begin();
        try { TickCore(); }
        finally { MclslPerformanceProbe.End("灵兽.兽潮刷新",sample); }
    }
    private static void TickCore()
    {
        if(_locked || World.world?.units==null)return;
        MclslBeastPopulationIndex.ValidateSome(8);
        // A manual request prepares the sea/land index before ordinary spawning.
        if(_manualRequest>0)
        {
            int year=MclslRuntime.CurrentYear();
            if(MclslBeastSurfaceIndex.Refresh(false)||MclslBeastSurfaceIndex.Ready)
            {
                int level=_manualRequest;_manualRequest=0;
                ManualFeedback=Trigger(level,year)
                    ?level+"级兽潮已引动，正在分批生成。":"召潮未成功："+TriggerFailure;
            }
        }
        if(Config.paused || UnityEngine.Time.time<_nextSpawnTime)return;
        _nextSpawnTime=UnityEngine.Time.time+0.15f;
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
                FillWaveCandidates(wave.Theme,realm,center,wave.Cursor);
                if(WaveCandidates.Count==0)
                {
                    realm=wave.Level-1;
                    FillWaveCandidates(wave.Theme,realm,center,wave.Cursor);
                }
                if(WaveCandidates.Count==0){wave.Cursor=wave.Count;break;}
                var selected=WaveCandidates[Roll(wave.Id+"|"+wave.Cursor,WaveCandidates.Count)];
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
            int auditYear=MclslRuntime.CurrentYear();
            for(int w=0;w<State.Waves.Count;w++)
            {
                var wave=State.Waves[w];
                if(wave.Status=="已结束")continue;
                if(wave.Cursor<wave.Count)continue;            // 还在生成，跳过
                // 只要还有存活妖兽就保持"活动中"；只有全部死亡才结束（不做超时强制收场）。
                if(Remaining(wave)!=0)continue;
                wave.Status="已结束";
                MclslWorldRunRepository.AddEvent(auditYear,"beast_tide_end",
                    wave.Level+"级"+wave.Theme+"兽潮结束",
                    wave.Year+"年始于("+wave.X+","+wave.Y+")，实际生成"+wave.Actors.Count+"只，已无存活妖兽。",
                    wave.X,wave.Y,"兽潮发生地",string.Empty,"beast_tide_end|"+wave.Id);
                wave.Actors.Clear();Save();
            }
            if(State.Waves.Count>64)State.Waves.RemoveAll(x=>x.Status=="已结束" && x.Year<auditYear-20);
        }
    }
    internal static int Remaining(MclslBeastWaveState wave)
        => wave.Actors.Count(id => MclslActorRegistry.TryPeekAlive(id, out _));

    /// <summary>妖兽死亡：立刻从所属兽潮摘除并同步人口索引，不等周期性注册表清理。</summary>
    internal static void OnBeastDeath(Actor actor)
    {
        try
        {
            long id = MclslActorAccessor.Id(actor);
            if (id <= 0L) return;
            MclslBeastPopulationIndex.Forget(id);
            string source = MclslActorAccessor.GetString(actor, SourceKey);
            if (!source.StartsWith("wave:", StringComparison.Ordinal)) return;
            string waveId = source.Substring(5);
            for (int i = 0; i < State.Waves.Count; i++)
                if (State.Waves[i].Id == waveId) { State.Waves[i].Actors.Remove(id); break; }
        }
        catch (Exception ex) { MclslDiagnostics.Error("beast-on-death", "妖兽死亡剪枝失败: " + ex.Message); }
    }
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
    internal static void MarkRankStatsDirty(Actor actor)
    {
        // Worker callbacks enqueue only an id. Never touch UI, caches, or
        // gameplay state until the native calculation has returned.
        if(MclslBeastActorRegistration.IsOrdinary(actor))
            PendingRankStats[MclslActorAccessor.Id(actor)]=0;
    }
    internal static void ForgetRank(long id)=>PendingRankStats.TryRemove(id,out _);
}
