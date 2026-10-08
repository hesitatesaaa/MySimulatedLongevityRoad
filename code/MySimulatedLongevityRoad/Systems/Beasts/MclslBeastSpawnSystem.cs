using System;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;
using MySimulatedLongevityRoad.Traits;

namespace MySimulatedLongevityRoad.Systems;

internal static class MclslBeastSpawnSystem
{
    private const string LastCompletedYearKey = "mclsl.beasts.last_spawn_year";
    private static WorldTile[] _tiles;
    private static WorldTile[] _candidates;
    private static int[] _candidateCounts;
    private static int _cursor;
    private static int _year = -1;

    internal static void Clear()
    {
        _tiles = null;
        _candidates = null;
        _candidateCounts = null;
        _cursor = 0;
        _year = -1;
    }

    internal static bool TickAnnual(int year)
    {
        if (!MclslBeastConfig.NaturalSpawnEnabled || World.world?.units == null || year <= 0)
            return true;
        if (CompletedYear() >= year) return true;
        if (_year != year || _tiles == null) Begin(year);
        if (_tiles == null) return true;

        int processed = 0;
        while (_cursor < _tiles.Length && processed < 128 && !MclslAnnualFrameBudget.Expired)
        {
            WorldTile tile = _tiles[_cursor++];
            processed++;
            if (tile?.Type == null) continue;
            for (int i = 0; i < MclslBeastCatalog.All.Length; i++)
            {
                MclslBeastDefinition beast = MclslBeastCatalog.All[i];
                if (beast.Habitat == MclslBeastHabitat.RedMoon) continue;
                if (!Matches(tile, beast.Habitat)) continue;
                int seen = ++_candidateCounts[i];
                if (PositiveHash(year, beast.Id, _cursor) % seen == 0) _candidates[i] = tile;
            }
        }
        if (_cursor < _tiles.Length) return false;

        Finish(year);
        Clear();
        return true;
    }

    private static void Begin(int year)
    {
        _year = year;
        _tiles = World.world?.tiles_list;
        _cursor = 0;
        _candidates = new WorldTile[MclslBeastCatalog.All.Length];
        _candidateCounts = new int[MclslBeastCatalog.All.Length];
    }

    private static void Finish(int year)
    {
        bool redMoon = Pass(year, "red_moon", MclslBeastConfig.RedMoonChancePerMillion);
        bool revival = Pass(year, "mountain_revival", MclslBeastConfig.RevivalChancePerMillion);
        MclslBeastDefinition moqilin = null;
        for (int i = 0; i < MclslBeastCatalog.All.Length; i++)
        {
            MclslBeastDefinition beast = MclslBeastCatalog.All[i];
            if (beast.Id == "moqilin") { moqilin = beast; continue; }
            WorldTile tile = _candidates[i];
            if (beast.Habitat == MclslBeastHabitat.RedMoon)
            {
                if (!redMoon) continue;
                tile = FindRedMoonTile();
            }
            if (beast.Id == "tianshou" && !revival) continue;
            if (tile == null || !Pass(year, beast.Id, MclslBeastConfig.Chance(beast))) continue;
            if (beast.Id == "qilin" && moqilin != null
                && Pass(year, "moqilin_variant", MclslBeastConfig.Chance(moqilin)))
            {
                if (TryChooseLineage(moqilin, out _)) beast = moqilin;
            }
            int group = beast.MinGroup + PositiveHash(year, beast.Id, 71)
                % (beast.MaxGroup - beast.MinGroup + 1);
            for (int member = 0; member < group; member++)
            {
                if (!TryChooseLineage(beast, out Subspecies lineage)) break;
                WorldTile placement = member == 0 ? tile : Nearby(tile, beast.Habitat);
                if (placement == null) break;
                Actor actor = World.world.units.createNewUnit(beast.BeastAssetId, placement,
                    false, 0f, lineage, null, true, false, false, false);
                if (actor?.data == null) break;
                MclslBeastActorRegistration.ReconcileForm(actor);
                MclslBeastActorRegistration.EnsureCultivationSeed(actor);
            }
        }
        try
        {
            if (World.world.map_stats.custom_data == null)
                World.world.map_stats.custom_data = new SaveCustomData();
            World.world.map_stats.custom_data.set(LastCompletedYearKey, year);
        }
        catch (Exception ex) { MclslDiagnostics.Error("beast-spawn-year", ex.Message); }
    }

    private static int CompletedYear()
    {
        try
        {
            if (World.world?.map_stats?.custom_data == null) return 0;
            World.world.map_stats.custom_data.get(LastCompletedYearKey, out int value, 0);
            return value;
        }
        catch { return 0; }
    }

    private static bool TryChooseLineage(MclslBeastDefinition beast, out Subspecies chosen)
    {
        chosen = null;
        var list = World.world?.subspecies?.list;
        if (list == null) return true;
        bool found = false;
        for (int i = 0; i < list.Count; i++)
        {
            Subspecies subspecies = list[i];
            string assetId = subspecies?.getActorAsset()?.id;
            if (assetId != beast.BeastAssetId && assetId != beast.AscendedAssetId) continue;
            found = true;
            if (subspecies.hasReachedPopulationLimit()) continue;
            chosen = subspecies;
            return true;
        }
        // The first spawn may create the species through the native pipeline.
        // Once lineages exist, only a lineage below its own edited limit qualifies.
        return !found;
    }

    private static WorldTile FindRedMoonTile()
    {
        for (int i = 0; i < _candidates.Length; i++)
            if (_candidates[i] != null && !_candidates[i].Type.liquid) return _candidates[i];
        return null;
    }

    private static WorldTile Nearby(WorldTile center, MclslBeastHabitat habitat)
    {
        WorldTile[] neighbours = center?.neighboursAll;
        if (neighbours == null) return center;
        for (int i = 0; i < neighbours.Length; i++)
            if (Matches(neighbours[i], habitat)) return neighbours[i];
        return center;
    }

    private static bool Matches(WorldTile tile, MclslBeastHabitat habitat)
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
