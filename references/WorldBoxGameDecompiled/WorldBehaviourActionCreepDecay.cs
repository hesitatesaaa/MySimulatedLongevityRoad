using System.Collections.Generic;

public static class WorldBehaviourActionCreepDecay
{
	private const int MAX_CREEP_TO_DESTROY_IN_ONE_STEP = 3;

	private static List<WorldTile> next_wave = new List<WorldTile>();

	private static List<WorldTile> cur_wave = new List<WorldTile>();

	private static HashSetWorldTile checked_tiles = new HashSetWorldTile();

	private static HashSetWorldTile not_checked_tiles = new HashSetWorldTile();

	private static List<WorldTile> _list_of_disconnected_tiles = new List<WorldTile>();

	public static void checkCreep()
	{
		if (!WorldLawLibrary.world_law_forever_tumor_creep.isEnabled())
		{
			checkBiome("tumor", "biome_tumor");
			checkBiome("biomass", "biome_biomass");
			checkBiome("super_pumpkin", "biome_pumpkin");
			checkBiome("cybercore", "biome_cybertile");
		}
	}

	private static void checkBiome(string pCreepHubID, string pBiomeID)
	{
		BuildingAsset buildingAsset = AssetManager.buildings.get(pCreepHubID);
		Kingdom kingdom = World.world.kingdoms_wild.get(buildingAsset.kingdom);
		BiomeAsset biomeAsset = AssetManager.biome_library.get(pBiomeID);
		clear();
		addToNotChecked(biomeAsset.getTileLow());
		addToNotChecked(biomeAsset.getTileHigh());
		if (not_checked_tiles.Count == 0)
		{
			return;
		}
		if (kingdom.buildings.Count > 0)
		{
			List<Building> buildings = kingdom.buildings;
			for (int i = 0; i < buildings.Count; i++)
			{
				Building building = buildings[i];
				if (building.isUsable())
				{
					checkTile(building.current_tile);
					next_wave.Add(building.current_tile);
				}
			}
		}
		startWave(pBiomeID);
		if (not_checked_tiles.Count > 0)
		{
			destroyNonCheckedCreep();
		}
	}

	private static void startWave(string pBiomeID)
	{
		if (next_wave.Count == 0)
		{
			return;
		}
		cur_wave.AddRange(next_wave);
		next_wave.Clear();
		while (cur_wave.Count > 0)
		{
			WorldTile worldTile = cur_wave[cur_wave.Count - 1];
			cur_wave.RemoveAt(cur_wave.Count - 1);
			for (int i = 0; i < worldTile.neighboursAll.Length; i++)
			{
				WorldTile worldTile2 = worldTile.neighboursAll[i];
				if (!(worldTile2.Type.biome_id != pBiomeID) && !checked_tiles.Contains(worldTile2))
				{
					checkTile(worldTile2);
					next_wave.Add(worldTile2);
				}
			}
		}
		if (next_wave.Count > 0)
		{
			startWave(pBiomeID);
		}
	}

	private static void destroyNonCheckedCreep()
	{
		foreach (WorldTile not_checked_tile in not_checked_tiles)
		{
			_list_of_disconnected_tiles.Add(not_checked_tile);
		}
		foreach (WorldTile item in _list_of_disconnected_tiles.LoopRandom(3))
		{
			MapAction.decreaseTile(item, pDamage: false);
		}
	}

	private static void checkTile(WorldTile pTile)
	{
		checked_tiles.Add(pTile);
		not_checked_tiles.Remove(pTile);
	}

	private static void addToNotChecked(TopTileType pTileType)
	{
		if (pTileType.hashset.Count != 0)
		{
			not_checked_tiles.UnionWith(pTileType.hashset);
		}
	}

	public static void clear()
	{
		checked_tiles.Clear();
		not_checked_tiles.Clear();
		next_wave.Clear();
		cur_wave.Clear();
		_list_of_disconnected_tiles.Clear();
	}
}
