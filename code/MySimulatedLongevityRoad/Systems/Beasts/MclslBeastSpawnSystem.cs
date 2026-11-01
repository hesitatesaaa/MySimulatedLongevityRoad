using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastSpawnSystem
{
    private static readonly Queue<(MclslBeastDefinition Beast,WorldTile Tile)> Pending=new();
    // Reusable candidate pool: the prepare pass no longer allocates a fresh
    // species array for every region it evaluates.
    private static readonly List<MclslBeastDefinition> PoolScratch=new();
    private static int _preparedYear=-1;
    internal static void Clear(){ Pending.Clear();_preparedYear=-1;MclslBeastSurfaceIndex.Clear(); }
    internal static bool TickAnnual(int year)
    {
        long sample=MclslPerformanceProbe.Begin();
        try { return TickAnnualCore(year); }
        finally { MclslPerformanceProbe.End("灵兽.年度生成准备",sample); }
    }
    private static bool TickAnnualCore(int year)
    {
        if(MclslBeastWorld.WriteLocked || World.world?.units==null || year<=0)return true;
        if(!MclslBeastSurfaceIndex.Refresh())return false;
        MclslBeastWorld.TickAnnual(year);
        if(!MclslBeastConfig.NaturalSpawnEnabled)return true;
        bool carriedQueue=Pending.Count>0 || MclslBeastWorld.State.NaturalPending.Count>0;
        if(Pending.Count==0 && MclslBeastWorld.State.NaturalPending.Count>0)
        {
            foreach(var saved in MclslBeastWorld.State.NaturalPending)
            {
                var beast=MclslBeastCatalog.All.FirstOrDefault(x=>x.Id==saved.Species);
                if(beast!=null && saved.X>=0 && saved.Y>=0 && saved.X<MapBox.width && saved.Y<MapBox.height)
                    Pending.Enqueue((beast,World.world.GetTileSimple(saved.X,saved.Y)));
            }
            _preparedYear=year;
            MclslBeastWorld.State.NaturalPending=Pending.Where(x=>x.Tile!=null).Select(x=>new MclslNaturalBeastSpawn
                { Species=x.Beast.Id,X=x.Tile.pos.x,Y=x.Tile.pos.y }).ToList();
        }
        // A carried queue consumes this judgment year too. Persist before draining
        // so reloading after the last item cannot accidentally start a second roll.
        if(carriedQueue && MclslBeastRules.RareNaturalYear(year)
            && MclslBeastWorld.State.LastNaturalYear<year)
        {
            MclslBeastWorld.State.LastNaturalYear=year;
            MclslBeastWorld.Save();
        }
        // Non-judgment years skip the whole natural candidate/probability/terrain scan.
        // A queue that survives from a previous year is still drained below; only a
        // genuine prepare year clears and rebuilds it, so repeated calls, reloads and
        // restores never re-roll, and missed years are never back-filled.
        bool prepare=Pending.Count==0 && MclslBeastRules.ShouldPrepareNatural(year,MclslBeastWorld.State.LastNaturalYear,true);
        if(Pending.Count==0 && !prepare)return true;
        if(prepare && _preparedYear!=year)
        {
            _preparedYear=year;Pending.Clear();
            int remaining=Math.Min(MclslBeastWorld.NaturalLimit-MclslBeastWorld.CountAutomatic(true),
                MclslBeastWorld.AutomaticLimit-MclslBeastWorld.CountAutomatic()-MclslBeastWorld.State.Waves.Sum(x=>x.Count-x.Cursor));
            var common=MclslBeastRules.CommonSpecies;
            if(MclslBeastRules.CommonNaturalYear(year))
                foreach(int region in MclslBeastSurfaceIndex.RegionIds.OrderBy(x=>x))
                {
                    if(remaining<=0)break;
                    int regionCount=MclslBeastWorld.CountAutomatic(true,region);
                    if(regionCount>=4 || !Pass(year,"region_"+region,MclslBeastRules.RegionalNaturalChancePerMillion))continue;
                    PoolScratch.Clear();
                    foreach(var b in MclslBeastCatalog.All)
                        if(common.Contains(b.Id)
                            && MclslBeastContent.BySpecies[b.Id].MinRealm<MclslBeastWorld.State.Unlocked
                            && MclslBeastSurfaceIndex.Find(b,year,region)!=null && MclslBeastConfig.Chance(b)>0)
                            PoolScratch.Add(b);
                    if(PoolScratch.Count==0)continue;
                    MclslPerformanceProbe.RecordBeastSpawnScan(PoolScratch.Count);
                    int total=0;foreach(var b in PoolScratch)total+=MclslBeastConfig.Chance(b);if(total<=0)continue;
                    int roll=MclslBeastWorld.Roll(region+"|species|"+year,total);
                    var selected=PoolScratch[0];foreach(var b in PoolScratch){if(roll<MclslBeastConfig.Chance(b)){selected=b;break;}roll-=MclslBeastConfig.Chance(b);}
                    int amount=Math.Min(Math.Min(4-regionCount,remaining),1+MclslBeastWorld.Roll(region+"|count|"+year,2));
                    for(int i=0;i<amount;i++)Pending.Enqueue((selected,MclslBeastSurfaceIndex.Find(selected,year+i,region)));
                    remaining-=amount;
                }
            foreach(var beast in MclslBeastCatalog.All)
            {
                if(remaining<=0)break;
                if(common.Contains(beast.Id)||beast.Id=="moqilin"||MclslBeastContent.BySpecies[beast.Id].MinRealm>=MclslBeastWorld.State.Unlocked)continue;
                if(!MclslBeastRules.RareNaturalYear(year))continue;
                if(beast.Id=="yingying"&&!Pass(year,"red_moon",MclslBeastConfig.RedMoonChancePerMillion))continue;
                if(beast.Id=="tianshou"&&!Pass(year,"revival",MclslBeastConfig.RevivalChancePerMillion))continue;
                if(!Pass(year,beast.Id,MclslBeastRules.NaturalChance(MclslBeastConfig.Chance(beast))))continue;
                var selected=beast;
                if(beast.Id=="qilin"&&Pass(year,"moqilin_variant",MclslBeastConfig.Chance(MclslBeastCatalog.All.First(x=>x.Id=="moqilin"))))
                    selected=MclslBeastCatalog.All.First(x=>x.Id=="moqilin");
                var tile=MclslBeastSurfaceIndex.Find(selected,year);
                if(tile==null||MclslBeastWorld.CountAutomatic(true,MclslBeastSurfaceIndex.Region(tile))>=4)continue;
                int queuedInRegion=Pending.Count(x=>MclslBeastSurfaceIndex.Region(x.Tile)==MclslBeastSurfaceIndex.Region(tile));
                if(queuedInRegion+MclslBeastWorld.CountAutomatic(true,MclslBeastSurfaceIndex.Region(tile))>=4)continue;
                Pending.Enqueue((selected,tile));remaining--;
            }
            MclslBeastWorld.State.NaturalPending=Pending.Where(x=>x.Tile!=null).Select(x=>new MclslNaturalBeastSpawn
                { Species=x.Beast.Id,X=x.Tile.pos.x,Y=x.Tile.pos.y }).ToList();
            MclslBeastWorld.State.LastNaturalYear=year;MclslBeastWorld.Save();
        }
        var watch=Stopwatch.StartNew();int made=0;int spawned=0;
        while(Pending.Count>0 && made++<4 && watch.Elapsed.TotalMilliseconds<2 && !MclslAnnualFrameBudget.Expired)
        {
            var spawn=Pending.Dequeue();
            if(MclslBeastWorld.State.NaturalPending.Count>0)MclslBeastWorld.State.NaturalPending.RemoveAt(0);
            if(MclslBeastWorld.CountAutomatic()>=MclslBeastWorld.AutomaticLimit)continue;
            if(MclslBeastWorld.Spawn(spawn.Beast,spawn.Tile,MclslBeastContent.BySpecies[spawn.Beast.Id].MinRealm,"natural",0)!=null)spawned++;
        }
        MclslPerformanceProbe.RecordBeastSpawnPrepared(spawned);
        if(Pending.Count>0)return false;
        // Off-year drains leave the judgment clock untouched.
        MclslBeastWorld.Save();return true;
    }
    internal static byte SurfaceMask(WorldTile tile)
    {
        var type=tile?.Type;
        if(type==null || type.lava)return 0;
        return type.liquid?(byte)2:(byte)1;
    }
    internal static bool Matches(WorldTile tile,MclslBeastSurface surface)
        => (SurfaceMask(tile)&(1<<(int)surface))!=0;

    private static bool Pass(int year, string key, int chance)
        => chance > 0 && PositiveHash(year, key, 17) % 1000000 < chance;

    private static int PositiveHash(int year, string key, int salt)
    {
        unchecked
        {
            int hash = 23 + year * 37 + salt;
            string world = World.world?.map_stats?.name ?? string.Empty;
            for (int i = 0; i < world.Length; i++) hash = hash * 31 + world[i];
            for (int i = 0; i < key.Length; i++) hash = hash * 31 + key[i];
            // Avalanche the year/salt input: a linear hash would cluster rare
            // annual outcomes into long runs instead of spreading them out.
            uint mixed = (uint)hash;
            mixed ^= mixed >> 16;
            mixed *= 0x7feb352d;
            mixed ^= mixed >> 15;
            mixed *= 0x846ca68b;
            mixed ^= mixed >> 16;
            return (int)(mixed & int.MaxValue);
        }
    }
}
