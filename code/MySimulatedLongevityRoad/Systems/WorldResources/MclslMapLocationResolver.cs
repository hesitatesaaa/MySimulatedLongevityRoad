using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Data;
using UnityEngine;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>
/// Resolves coordinates only for newly created records. It deliberately never
/// repairs an old record from a location string.
/// </summary>
internal static class MclslMapLocationResolver
{
    internal static bool ResolveActorLocation(Actor actor, int seed, out int mapX, out int mapY, out string location, out string kingdom)
    {
        mapX = -1;
        mapY = -1;
        location = string.Empty;
        kingdom = string.Empty;
        if (actor?.data != null && TryUseCoordinate(actor.data.x, actor.data.y, out mapX, out mapY))
        {
            location = string.IsNullOrWhiteSpace(actor.city?.data?.name) ? DescribeTile(mapX, mapY) : actor.city.data.name + "附近";
            kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无主" : actor.kingdom.data.name;
            return true;
        }

        return TryPickValidTile(seed, out mapX, out mapY, out location, out kingdom);
    }

    internal static bool TryPickValidTile(int seed, out int mapX, out int mapY, out string location, out string kingdom)
    {
        mapX = -1;
        mapY = -1;
        location = string.Empty;
        kingdom = string.Empty;
        if (World.world == null || MapBox.width <= 0 || MapBox.height <= 0) return false;

        int minX = Math.Min(5, Math.Max(0, MapBox.width - 1));
        int minY = Math.Min(5, Math.Max(0, MapBox.height - 1));
        int spanX = Math.Max(1, MapBox.width - minX - 5);
        int spanY = Math.Max(1, MapBox.height - minY - 5);
        for (int attempt = 0; attempt < 96; attempt++)
        {
            int x = minX + MySimulatedLongevityRoad.Core.MclslStableHash.Positive43x53(seed + "|x|" + attempt) % spanX;
            int y = minY + MySimulatedLongevityRoad.Core.MclslStableHash.Positive43x53(seed + "|y|" + attempt) % spanY;
            if (!TryUseCoordinate(x, y, out mapX, out mapY)) continue;
            location = DescribeTile(mapX, mapY);
            kingdom = "无主";
            return true;
        }

        return false;
    }

    internal static bool TryUseCoordinate(int x, int y, out int mapX, out int mapY)
    {
        mapX = -1;
        mapY = -1;
        if (World.world == null || x < 0 || y < 0 || x >= MapBox.width || y >= MapBox.height) return false;
        WorldTile tile = TryGetTile(x, y);
        if (tile?.Type == null || !tile.Type.ground || tile.Type.block) return false;
        mapX = x;
        mapY = y;
        return true;
    }

    private static WorldTile TryGetTile(int x, int y)
    {
        try { return World.world.GetTileSimple(x, y) ?? World.world.GetTile(x, y); }
        catch
        {
            try { return World.world.GetTile(x, y); }
            catch { return null; }
        }
    }

    private static string DescribeTile(int x, int y) => "地图坐标(" + x + "," + y + ")";


    internal sealed class LocationSeed
    {
        internal string Location;
        internal string Kingdom;
        internal int X = -1;
        internal int Y = -1;
    }
    internal static LocationSeed PickLocation(int hash)
    {
        List<LocationSeed> locations = new();
        IReadOnlyList<Actor> units = MclslActorRegistry.CityRepresentatives;
        if (units != null)
        {
            HashSet<string> seen = new(StringComparer.Ordinal);
            for (int i = 0; i < units.Count && locations.Count < 64; i++)
            {
                Actor actor = units[i];
                if (actor?.city == null) continue;
                string city = actor.city?.data?.name;
                if (string.IsNullOrWhiteSpace(city) || !seen.Add(city)) continue;
                string kingdom = string.IsNullOrWhiteSpace(actor.kingdom?.data?.name) ? "无主" : actor.kingdom.data.name;
                locations.Add(new LocationSeed { Location = city + "附近", Kingdom = kingdom, X = actor.data.x, Y = actor.data.y });
            }
        }
        if (locations.Count > 0) return locations[(hash & int.MaxValue) % locations.Count];
        return new LocationSeed { Location = MclslProceduralLexicon.CaveOrigins[(hash & int.MaxValue) % MclslProceduralLexicon.CaveOrigins.Length], Kingdom = "无主" };
    }
}
