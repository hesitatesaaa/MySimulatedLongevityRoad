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
    private static int _preparedYear=-1;
    internal static void Clear(){ Pending.Clear();_preparedYear=-1;MclslBeastHabitatIndex.Clear(); }
    internal static bool TickAnnual(int year)
    {
        if(MclslBeastWorld.WriteLocked || World.world?.units==null || year<=0)return true;
        if(!MclslBeastHabitatIndex.Refresh(year))return false;
        MclslBeastWorld.TickAnnual(year);
        if(!MclslBeastConfig.NaturalSpawnEnabled)return true;
        if(Pending.Count==0 && MclslBeastWorld.State.NaturalPending.Count>0)
        {
            foreach(var saved in MclslBeastWorld.State.NaturalPending)
            {
                var beast=MclslBeastCatalog.All.FirstOrDefault(x=>x.Id==saved.Species);
                if(beast!=null && saved.X>=0 && saved.Y>=0 && saved.X<MapBox.width && saved.Y<MapBox.height)
                    Pending.Enqueue((beast,World.world.GetTileSimple(saved.X,saved.Y)));
            }
            _preparedYear=year;
        }
        if(Pending.Count==0 && MclslBeastWorld.State.LastNaturalYear>=year)return true;
        if(_preparedYear!=year)
        {
            _preparedYear=year;Pending.Clear();
            int remaining=Math.Min(MclslBeastWorld.NaturalLimit-MclslBeastWorld.CountAutomatic(true),
                MclslBeastWorld.AutomaticLimit-MclslBeastWorld.CountAutomatic()-MclslBeastWorld.State.Waves.Sum(x=>x.Count-x.Cursor));
            var common=MclslBeastRules.CommonSpecies;
            if(year%5==0)
                foreach(int region in MclslBeastHabitatIndex.RegionIds.OrderBy(x=>x))
                {
                    if(remaining<=0)break;
                    int regionCount=MclslBeastWorld.CountAutomatic(true,region);
                    if(regionCount>=4 || !Pass(year,"region_"+region,200000))continue;
                    var pool=MclslBeastCatalog.All.Where(b=>common.Contains(b.Id)
                        && MclslBeastContent.BySpecies[b.Id].MinRealm<MclslBeastWorld.State.Unlocked
                        && MclslBeastHabitatIndex.Find(b,year,region)!=null && MclslBeastConfig.Chance(b)>0).ToArray();
                    int total=pool.Sum(b=>MclslBeastConfig.Chance(b));if(total<=0)continue;
                    int roll=MclslBeastWorld.Roll(region+"|species|"+year,total);
                    var selected=pool[0];foreach(var b in pool){if(roll<MclslBeastConfig.Chance(b)){selected=b;break;}roll-=MclslBeastConfig.Chance(b);}
                    int amount=Math.Min(Math.Min(4-regionCount,remaining),1+MclslBeastWorld.Roll(region+"|count|"+year,2));
                    for(int i=0;i<amount;i++)Pending.Enqueue((selected,MclslBeastHabitatIndex.Find(selected,year+i,region)));
                    remaining-=amount;
                }
            foreach(var beast in MclslBeastCatalog.All)
            {
                if(remaining<=0)break;
                if(common.Contains(beast.Id)||beast.Id=="moqilin"||MclslBeastContent.BySpecies[beast.Id].MinRealm>=MclslBeastWorld.State.Unlocked)continue;
                if(beast.Id=="yingying"&&!Pass(year,"red_moon",MclslBeastConfig.RedMoonChancePerMillion))continue;
                if(beast.Id=="tianshou"&&!Pass(year,"revival",MclslBeastConfig.RevivalChancePerMillion))continue;
                if(!Pass(year,beast.Id,MclslBeastConfig.Chance(beast)))continue;
                var selected=beast;
                if(beast.Id=="qilin"&&Pass(year,"moqilin_variant",MclslBeastConfig.Chance(MclslBeastCatalog.All.First(x=>x.Id=="moqilin"))))
                    selected=MclslBeastCatalog.All.First(x=>x.Id=="moqilin");
                var tile=MclslBeastHabitatIndex.Find(selected,year);
                if(tile==null||MclslBeastWorld.CountAutomatic(true,MclslBeastHabitatIndex.Region(tile))>=4)continue;
                int queuedInRegion=Pending.Count(x=>MclslBeastHabitatIndex.Region(x.Tile)==MclslBeastHabitatIndex.Region(tile));
                if(queuedInRegion+MclslBeastWorld.CountAutomatic(true,MclslBeastHabitatIndex.Region(tile))>=4)continue;
                Pending.Enqueue((selected,tile));remaining--;
            }
            MclslBeastWorld.State.NaturalPending=Pending.Where(x=>x.Tile!=null).Select(x=>new MclslNaturalBeastSpawn
                { Species=x.Beast.Id,X=x.Tile.pos.x,Y=x.Tile.pos.y }).ToList();
            MclslBeastWorld.State.LastNaturalYear=year;MclslBeastWorld.Save();
        }
        var watch=Stopwatch.StartNew();int made=0;
        while(Pending.Count>0 && made++<4 && watch.Elapsed.TotalMilliseconds<2 && !MclslAnnualFrameBudget.Expired)
        {
            var spawn=Pending.Dequeue();
            if(MclslBeastWorld.State.NaturalPending.Count>0)MclslBeastWorld.State.NaturalPending.RemoveAt(0);
            if(MclslBeastWorld.CountAutomatic()>=MclslBeastWorld.AutomaticLimit)continue;
            MclslBeastWorld.Spawn(spawn.Beast,spawn.Tile,MclslBeastContent.BySpecies[spawn.Beast.Id].MinRealm,"natural",0);
        }
        if(Pending.Count>0)return false;
        MclslBeastWorld.State.LastNaturalYear=year;MclslBeastWorld.Save();return true;
    }
    internal static bool Matches(WorldTile tile, MclslBeastHabitat habitat)
    {
        if (tile?.Type == null) return false;
        var type = tile.Type;
        string biome = type.biome_id ?? string.Empty;
        string id = type.id ?? string.Empty;
        bool water = type.liquid;
        bool mountain = type.mountains || type.edge_mountains || Contains(biome, "rock") || Contains(biome, "mountain");
        bool forest = Contains(biome, "forest") || Contains(biome, "jungle") || Contains(biome, "wood");
        bool wet = Contains(biome, "wet") || Contains(biome, "swamp") || Contains(biome, "marsh");
        bool grass = Contains(biome, "grass") || Contains(biome, "plain");
        bool coast = HasLandAndWaterNeighbour(tile);
        switch (habitat)
        {
            case MclslBeastHabitat.DeepSea: return water && type.ocean && !coast;
            case MclslBeastHabitat.RiverLake: return water && (!type.ocean || coast);
            case MclslBeastHabitat.Mountain: return !water && mountain;
            case MclslBeastHabitat.Volcano: return type.lava || Contains(biome, "infernal") || Contains(biome, "volcan");
            case MclslBeastHabitat.MountainForest: return !water && (mountain || forest);
            case MclslBeastHabitat.ForestGrass: return !water && (forest || grass);
            case MclslBeastHabitat.LakeWetlandCoast: return (water && coast) || (!water && wet);
            case MclslBeastHabitat.BurntForest: return !water && (type.lava || Contains(biome, "burn") || Contains(biome, "infernal"));
            case MclslBeastHabitat.RidgeCoast: return !water && (mountain || coast);
            case MclslBeastHabitat.WetForest: return !water && (wet || forest);
            case MclslBeastHabitat.OldForestRuin: return !water && (forest || Contains(biome, "ruin"));
            case MclslBeastHabitat.WildMountain: return !water && (mountain || Contains(biome, "waste"));
            case MclslBeastHabitat.IslandCoast: return !water && coast;
            case MclslBeastHabitat.GrassHill: return !water && (grass || type.edge_hills);
            case MclslBeastHabitat.CorruptBorder: return !water && (Contains(biome, "corrupt") || Contains(biome, "waste"));
            case MclslBeastHabitat.SeaRift: return water && type.ocean && !coast && (Contains(biome, "deep") || Contains(id, "deep"));
            case MclslBeastHabitat.RedMoon: return !water;
            case MclslBeastHabitat.HighMountain: return !water && type.mountains;
            default: return !water;
        }
    }

    private static bool HasLandAndWaterNeighbour(WorldTile tile)
    {
        WorldTile[] neighbours = tile.neighboursAll;
        if (neighbours == null) return false;
        bool wet = tile.Type.liquid;
        for (int i = 0; i < neighbours.Length; i++)
            if (neighbours[i]?.Type != null && neighbours[i].Type.liquid != wet) return true;
        return false;
    }

    private static bool Contains(string value, string token)
        => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

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
