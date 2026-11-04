$ErrorActionPreference = 'Stop'
$source = Get-Content (Join-Path $PSScriptRoot '../code/MySimulatedLongevityRoad/Systems/Beasts/MclslBeastSurfaceIndex.cs') -Raw
$source = $source.Replace('namespace MySimulatedLongevityRoad.Systems;', 'namespace MySimulatedLongevityRoad.Systems {') + "`n}"
$stubs = @"
public class WorldTile { public int x,y; public WorldTile pos=>this; public object Type=new object(); public byte Mask=1; }
public class MapBox {
    public static int width,height;
    public WorldTile[] tiles_list;
    public WorldTile GetTileSimple(int x,int y)=>tiles_list[y*width+x];
}
public static class World { public static MapBox world; }
namespace MySimulatedLongevityRoad.Data {
    public class MclslBeastDefinition { public int Surface; public string Id="test"; }
}
namespace MySimulatedLongevityRoad.Core {
    public static class MclslWorldContext { public static long Generation=1; }
    public static class MclslRuntimeChanges { public static bool IsMainThread=true; }
    public static class MclslFrameDeadline { public static double RemainingMs=>100; }
    public static class MclslAnnualFrameBudget { public static double RemainingMs=>100; }
    public static class MclslPerformanceProbe { public static long Begin()=>0; public static void End(string name,long sample){} public static void RecordBeastSurfaceClassified(int tiles,bool incremental){} }
    public class MclslActorThreadHandoff<T> {
        private readonly System.Collections.Generic.Queue<(T,long)> q=new();
        public int Count=>q.Count;
        public void Clear()=>q.Clear();
        public void Publish(long id,T tile,int a,bool b,long generation)=>q.Enqueue((tile,generation));
        public bool TryTake(out long id,out T tile,out int a,out bool b,out long generation) {
            id=0;a=0;b=false;tile=default;generation=0;
            if(q.Count==0)return false;var entry=q.Dequeue();tile=entry.Item1;generation=entry.Item2;return true;
        }
    }
}
namespace MySimulatedLongevityRoad.Systems {
    public static class MclslBeastSpawnSystem {
        public static byte SurfaceMask(WorldTile tile)=>tile?.Type==null?(byte)0:tile.Mask;
        public static bool Matches(WorldTile tile,int surface)=>(SurfaceMask(tile)&(1<<surface))!=0;
    }
    public static class MclslBeastWorld { public static int Roll(string key,int upper)=>0; }
    public static class SurfaceIndexRegression {
        static void Check(bool ok,string name) { if(!ok)throw new System.Exception(name); }
        static void Map(int width,int height) {
            MapBox.width=width;MapBox.height=height;World.world=new MapBox { tiles_list=new WorldTile[width*height] };
            for(int i=0;i<width*height;i++)World.world.tiles_list[i]=new WorldTile { x=i%width,y=i/width };
        }
        static void Build() { for(int i=0;i<1000 && !MclslBeastSurfaceIndex.Refresh(false);i++) {} Check(MclslBeastSurfaceIndex.Ready,"build completes"); }
        public static string Run() {
            var land=new Data.MclslBeastDefinition();var sea=new Data.MclslBeastDefinition { Surface=1 };
            Map(2,2);MclslBeastSurfaceIndex.Resume();Build();
            var tile=World.world.tiles_list[0];tile.Mask=2;MclslBeastSurfaceIndex.Invalidate(tile);
            Check(MclslBeastSurfaceIndex.Find(sea,0)==tile,"land to sea");
            tile.Mask=1;MclslBeastSurfaceIndex.Invalidate(tile);MclslBeastSurfaceIndex.Invalidate(tile);
            Check(MclslBeastSurfaceIndex.Find(sea,0)==null,"sea removal and duplicate event");
            var stale=tile;Map(1,1);MclslBeastSurfaceIndex.Invalidate(stale);Build();
            Check(MclslBeastSurfaceIndex.Find(land,0)==World.world.tiles_list[0],"resized map rebuild");
            MclslBeastSurfaceIndex.Invalidate(new WorldTile { x=-1,y=0 });
            MclslBeastSurfaceIndex.Invalidate(new WorldTile { x=0,y=0,Mask=2 });
            Check(MclslBeastSurfaceIndex.Find(sea,0)==null,"foreign and negative tiles ignored");
            MclslBeastSurfaceIndex.Suspend();World.world.tiles_list=null;
            MclslBeastSurfaceIndex.Invalidate(stale);MclslBeastSurfaceIndex.Tick();
            Check(!MclslBeastSurfaceIndex.Refresh(false),"no build during clear");
            Map(1,1);World.world.tiles_list[0]=null;MclslBeastSurfaceIndex.Resume();
            Check(!MclslBeastSurfaceIndex.Refresh(false),"null tile deferred");
            tile=new WorldTile { Type=null };World.world.tiles_list[0]=tile;
            Check(!MclslBeastSurfaceIndex.Refresh(false),"uninitialized tile deferred");
            tile.Type=new object();Build();
            Check(MclslBeastSurfaceIndex.Find(land,0)==tile,"deferred tile retried");
            Core.MclslRuntimeChanges.IsMainThread=false;tile.Mask=2;MclslBeastSurfaceIndex.Invalidate(tile);
            Core.MclslRuntimeChanges.IsMainThread=true;Build();
            Check(MclslBeastSurfaceIndex.Find(sea,0)==tile,"deferred terrain event");
            return "PASS: transitions, duplicate events, resize, stale tiles, clear, incomplete tiles, deferred events";
        }
    }
}
"@
Add-Type -TypeDefinition ($source + $stubs)
[MySimulatedLongevityRoad.Systems.SurfaceIndexRegression]::Run()
