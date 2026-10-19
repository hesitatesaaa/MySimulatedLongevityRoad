using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastHabitatIndex
{
    private static readonly Dictionary<(int Region,MclslBeastHabitat Habitat),List<WorldTile>> Tiles=new();
    private static readonly HashSet<int> Regions=new();
    private static readonly HashSet<int> Dirty=new();
    private static WorldTile[] _tiles;
    private static int _cursor,_lastYear=-1;
    private static bool _scan;
    internal static IEnumerable<int> RegionIds=>Regions;
    internal static int Region(WorldTile tile)=>tile==null?-1:(tile.pos.y/32)*((MapBox.width+31)/32)+tile.pos.x/32;
    internal static void Clear(){Tiles.Clear();Regions.Clear();Dirty.Clear();_tiles=null;_cursor=0;_lastYear=-1;_scan=false;}
    internal static void Invalidate(WorldTile tile)
    {
        if(tile==null)return;
        Dirty.Add(Region(tile));
        if(tile.neighboursAll!=null)foreach(var neighbour in tile.neighboursAll)if(neighbour!=null)Dirty.Add(Region(neighbour));
    }
    internal static bool Refresh(int year,bool useAnnualBudget=true)
    {
        if(!_scan)
        {
            if(_lastYear==year && Dirty.Count==0)return true;
            _tiles=World.world?.tiles_list;if(_tiles==null)return false;
            // Annual audit also detects native changes that bypass tile setters.
            Tiles.Clear();Regions.Clear();_cursor=0;_scan=true;
        }
        int processed=0;
        while(_cursor<_tiles.Length && processed++<128 && (!useAnnualBudget||!MclslAnnualFrameBudget.Expired))
        {
            WorldTile tile=_tiles[_cursor++];if(tile?.Type==null)continue;
            int region=Region(tile);Regions.Add(region);
            foreach(MclslBeastHabitat habitat in Enum.GetValues(typeof(MclslBeastHabitat)))
            {
                if(!MclslBeastSpawnSystem.Matches(tile,habitat))continue;
                var key=(region,habitat);
                if(!Tiles.TryGetValue(key,out var list))Tiles[key]=list=new(8);
                if(list.Count<8)list.Add(tile);
                else {int sample=MclslBeastWorld.Roll(year+"|tile|"+_cursor+"|"+habitat,Math.Max(9,_cursor));if(sample<8)list[sample]=tile;}
            }
        }
        if(_cursor<_tiles.Length)return false;
        _scan=false;_lastYear=year;Dirty.Clear();return true;
    }
    internal static WorldTile Find(MclslBeastDefinition beast,int seed,int region=-1)
    {
        List<WorldTile> candidates=new();
        foreach(var pair in Tiles)
            if(pair.Key.Habitat==beast.Habitat && (region<0||pair.Key.Region==region))
                foreach(var tile in pair.Value)if(MclslBeastSpawnSystem.Matches(tile,beast.Habitat))candidates.Add(tile);
        return candidates.Count==0?null:candidates[MclslBeastWorld.Roll(beast.Id+"|tile|"+seed,candidates.Count)];
    }
    internal static WorldTile FindNear(MclslBeastDefinition beast,WorldTile center,int seed)
    {
        if(center==null)return null;
        // Only place inside the local event area; never relocate a failed spawn
        // to an unrelated habitat on the far side of the map.
        for(int i=0;i<64;i++)
        {
            int x=center.pos.x+MclslBeastWorld.Roll(beast.Id+"|x|"+seed+"|"+i,17)-8;
            int y=center.pos.y+MclslBeastWorld.Roll(beast.Id+"|y|"+seed+"|"+i,17)-8;
            if(x<0||y<0||x>=MapBox.width||y>=MapBox.height)continue;
            var tile=World.world.GetTileSimple(x,y);
            if(MclslBeastSpawnSystem.Matches(tile,beast.Habitat))return tile;
        }
        return MclslBeastSpawnSystem.Matches(center,beast.Habitat)?center:null;
    }
}
