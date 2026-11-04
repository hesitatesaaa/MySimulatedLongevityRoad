using System;
using System.Collections.Generic;
using System.Diagnostics;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

// Initial construction visits the map once. Afterwards a terrain event changes
// only the changed tile. Biome-only changes require no index work.
internal static class MclslBeastSurfaceIndex
{
    private sealed class RegionData
    {
        internal readonly List<WorldTile>[] Tiles = new List<WorldTile>[2];
        internal readonly ushort[][] Positions = new ushort[2][];
        internal void Change(WorldTile tile,int habitat,bool included)
        {
            int local=(tile.y&31)*32+(tile.x&31);
            var positions=Positions[habitat];
            if(included)
            {
                positions ??= Positions[habitat]=new ushort[1024];
                var tiles=Tiles[habitat] ??= new List<WorldTile>();
                positions[local]=(ushort)(tiles.Count+1);tiles.Add(tile);
            }
            else
            {
                var tiles=Tiles[habitat];int index=positions[local]-1,last=tiles.Count-1;
                WorldTile moved=tiles[last];tiles[index]=moved;tiles.RemoveAt(last);
                positions[(moved.y&31)*32+(moved.x&31)]=(ushort)(index+1);
                positions[local]=0;
            }
        }
    }
    private static readonly HashSet<int> Regions = new();
    private static readonly MclslActorThreadHandoff<WorldTile> Deferred = new();
    private static RegionData[] _regions;
    private static byte[] _masks;
    private static int _columns,_buildCursor,_width,_height;
    private static bool _worldReady;
    internal static void Suspend() { _worldReady=false;Clear(); }
    internal static void Resume() => _worldReady=true;
    internal static long ClassifiedTiles { get; private set; }
    internal static long CompletedRegions { get; private set; }
    internal static int PendingTiles => Deferred.Count+(_masks==null?0:_masks.Length-_buildCursor);
    internal static bool HasPending => _masks!=null && PendingTiles>0;
    internal static IEnumerable<int> RegionIds => Regions;
    internal static int Region(WorldTile tile) => tile==null?-1
        :(tile.y/32)*((MapBox.width+31)/32)+tile.x/32;
    internal static bool Ready => _worldReady && _masks!=null && _width==MapBox.width
        && _height==MapBox.height && _buildCursor==_masks.Length;
    internal static void Clear()
    {
        Regions.Clear();Deferred.Clear();
        _regions=null;_masks=null;_buildCursor=0;ClassifiedTiles=CompletedRegions=0;
    }
    internal static void Invalidate(WorldTile tile)
    {
        if(!_worldReady || _masks==null || tile==null)return;
        if(!MclslRuntimeChanges.IsMainThread)
        {
            Deferred.Publish((long)tile.y*MapBox.width+tile.x+1,tile,1,false,MclslWorldContext.Generation);
            return;
        }
        UpdateSurface(tile);
    }
    private static void UpdateSurface(WorldTile tile)
    {
        if(!IsCurrentTile(tile))return;
        int id=tile.y*_width+tile.x;
        if(id<_buildCursor && _masks[id]!=MclslBeastSpawnSystem.SurfaceMask(tile))Classify(tile);
    }
    private static bool IsCurrentTile(WorldTile tile)
        => _worldReady && _masks!=null && World.world!=null
            && _width==MapBox.width && _height==MapBox.height
            && tile!=null && tile.x>=0 && tile.y>=0 && tile.x<_width && tile.y<_height
            && ReferenceEquals(tile,World.world.GetTileSimple(tile.x,tile.y));
    private static void Classify(WorldTile tile)
    {
        if(!IsCurrentTile(tile) || tile.Type==null)return;
        int id=tile.y*_width+tile.x;
        byte next=MclslBeastSpawnSystem.SurfaceMask(tile);
        int changed=_masks[id]^next;
        if(changed!=0)
        {
            var region=_regions[Region(tile)] ??= new RegionData();
            for(int h=0;h<2;h++)if((changed&(1u<<h))!=0)
                region.Change(tile,h,(next&(1u<<h))!=0);
            _masks[id]=next;
        }
        ClassifiedTiles++;
    }
    internal static bool Refresh(bool useAnnualBudget=true)
    {
        long sample=MclslPerformanceProbe.Begin();
        try { return RefreshCore(useAnnualBudget); }
        finally { MclslPerformanceProbe.End("灵兽.地表索引刷新",sample); }
    }
    private static bool RefreshCore(bool useAnnualBudget)
    {
        if(!_worldReady || World.world?.tiles_list==null || MapBox.width<=0 || MapBox.height<=0)return false;
        if(_masks!=null && (_width!=MapBox.width || _height!=MapBox.height))Clear();
        if(_masks==null)
        {
            _width=MapBox.width;_height=MapBox.height;
            _columns=(MapBox.width+31)/32;
            _regions=new RegionData[_columns*((MapBox.height+31)/32)];
            _masks=new byte[MapBox.width*MapBox.height];
            for(int i=0;i<_regions.Length;i++)Regions.Add(i);
        }
        double milliseconds=Math.Min(2d,MclslFrameDeadline.RemainingMs);
        if(useAnnualBudget)milliseconds=Math.Min(milliseconds,MclslAnnualFrameBudget.RemainingMs);
        long deadline=Stopwatch.GetTimestamp()+(long)(milliseconds*Stopwatch.Frequency/1000d);
        int changes=Deferred.Count;
        int built=0;
        while(_buildCursor<_masks.Length && Stopwatch.GetTimestamp()<deadline)
        {
            int id=_buildCursor;
            var tile=World.world.GetTileSimple(id%_width,id/_width);
            if(!IsCurrentTile(tile) || tile.Type==null)return false;
            Classify(tile);
            _buildCursor++;
            built++;
            if(((tile.x&31)==31 || tile.x==MapBox.width-1)
                &&((tile.y&31)==31 || tile.y==MapBox.height-1))CompletedRegions++;
        }
        if(built>0)MclslPerformanceProbe.RecordBeastSurfaceClassified(built,false);
        int incremental=0;
        while(changes-- >0 && Stopwatch.GetTimestamp()<deadline)
        {
            if(!Deferred.TryTake(out _,out var tile,out _,out _,out long generation))break;
            if(generation==MclslWorldContext.Generation){UpdateSurface(tile);incremental++;}
        }
        if(incremental>0)MclslPerformanceProbe.RecordBeastSurfaceClassified(incremental,true);
        // Completion depends on the initial build, not on future terrain events.
        return Ready;
    }
    internal static void Tick()
    { if(HasPending)Refresh(false); }
    internal static WorldTile Find(MclslBeastDefinition beast,int seed,int region=-1)
    {
        if(_regions==null)return null;
        int h=(int)beast.Surface;
        if(region>=0)
        {
            var local=_regions[region]?.Tiles[h];
            return local==null||local.Count==0?null:local[MclslBeastWorld.Roll(beast.Id+"|tile|"+seed,local.Count)];
        }
        // Preserve the previous region weighting (up to eight representatives)
        // while choosing an actual current candidate inside the selected region.
        int count=0;foreach(var bucket in _regions)count+=Math.Min(8,bucket?.Tiles[h]?.Count??0);
        if(count==0)return null;
        int pick=MclslBeastWorld.Roll(beast.Id+"|tile|"+seed,count);
        foreach(var bucket in _regions)
        {
            var candidates=bucket?.Tiles[h];int weight=Math.Min(8,candidates?.Count??0);
            if(pick<weight)return candidates[MclslBeastWorld.Roll(beast.Id+"|candidate|"+seed,candidates.Count)];
            pick-=weight;
        }
        return null;
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
            if(MclslBeastSpawnSystem.Matches(tile,beast.Surface))return tile;
        }
        return MclslBeastSpawnSystem.Matches(center,beast.Surface)?center:null;
    }
}
