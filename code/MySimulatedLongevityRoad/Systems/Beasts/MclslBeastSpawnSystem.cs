using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastSpawnSystem
{
    internal static void Clear()=>MclslBeastSurfaceIndex.Clear();
    internal static byte SurfaceMask(WorldTile tile)
    {
        var type=tile?.Type;
        if(type==null || type.lava)return 0;
        return type.liquid?(byte)2:(byte)1;
    }
    internal static bool Matches(WorldTile tile,MclslBeastSurface surface)
        => (SurfaceMask(tile)&(1<<(int)surface))!=0;

}
