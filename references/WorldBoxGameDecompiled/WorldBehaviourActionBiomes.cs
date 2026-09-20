using System.Collections.Generic;

public class WorldBehaviourActionBiomes : WorldBehaviourTilesRunner
{
	private static List<WorldTile> _bonus_tiles = new List<WorldTile>();

	public static void clear()
	{
		WorldBehaviourTilesRunner.clearTilesToCheck();
		_bonus_tiles.Clear();
	}

	public static void update()
	{
		AchievementLibrary.the_hell.check();
		if (WorldLawLibrary.world_law_grow_grass.isEnabled() && !World.world.era_manager.isWinter())
		{
			updateSingleTiles();
		}
	}

	public static void updateSingleTiles()
	{
		WorldBehaviourTilesRunner.checkTiles();
		int num = World.world.map_chunk_manager.amount_x * (SimGlobals.m.biomes_growth_speed + World.world_era.bonus_biomes_growth);
		if (num <= 0)
		{
			return;
		}
		WorldTile[] tiles_to_check = WorldBehaviourTilesRunner._tiles_to_check;
		if (WorldBehaviourTilesRunner._tile_next_check + num >= tiles_to_check.Length)
		{
			num = tiles_to_check.Length - WorldBehaviourTilesRunner._tile_next_check;
		}
		while (num-- > 0)
		{
			WorldBehaviourTilesRunner._tiles_to_check.ShuffleOne(WorldBehaviourTilesRunner._tile_next_check);
			WorldTile worldTile = tiles_to_check[WorldBehaviourTilesRunner._tile_next_check++];
			if (worldTile.Type.is_biome && worldTile.burned_stages <= 0)
			{
				trySpreadBiomeAround(worldTile, worldTile, pCheckRoad: true, pCheckBonuses: true);
			}
		}
		checkBonusTilesGrowth();
	}

	private static void checkBonusTilesGrowth()
	{
		if (_bonus_tiles.Count == 0)
		{
			return;
		}
		foreach (WorldTile bonus_tile in _bonus_tiles)
		{
			trySpreadBiomeAround(bonus_tile, bonus_tile);
		}
		_bonus_tiles.Clear();
	}

	public static void trySpreadBiomeAround(WorldTile pAroundTile, WorldTile pSpreader, bool pCheckRoad = false, bool pCheckBonuses = false, bool pForce = false)
	{
		trySpreadBiomeAround(pAroundTile, pSpreader.Type, pCheckRoad, pCheckBonuses, pForce);
	}

	public static void trySpreadBiomeAround(WorldTile pAroundTile, TileTypeBase pSpreadType, bool pCheckRoad = false, bool pCheckBonuses = false, bool pForce = false, bool pSkipEraCheck = false)
	{
		if (!pSpreadType.is_biome)
		{
			return;
		}
		BiomeAsset biome_asset = pSpreadType.biome_asset;
		bool spread_ignore_burned_stages = biome_asset.spread_ignore_burned_stages;
		if (!biome_asset.spread_biome || (!pSkipEraCheck && !string.IsNullOrEmpty(biome_asset.spread_only_in_era) && World.world_era.id != biome_asset.spread_only_in_era))
		{
			return;
		}
		for (int i = 0; i < pAroundTile.neighbours.Length; i++)
		{
			WorldTile worldTile = pAroundTile.neighbours[i];
			if (!spread_ignore_burned_stages && worldTile.burned_stages > 0)
			{
				continue;
			}
			if (worldTile.Type.road & pCheckRoad)
			{
				trySpreadBiomeAround(worldTile, pSpreadType);
			}
			if (!worldTile.Type.can_be_biome)
			{
				continue;
			}
			BiomeAsset biome_asset2 = worldTile.Type.biome_asset;
			if (biome_asset2 == biome_asset)
			{
				continue;
			}
			bool flag = false;
			if (worldTile.Type.soil | pForce)
			{
				flag = true;
			}
			else if (WorldLawLibrary.world_law_biome_overgrowth.isEnabled())
			{
				if (biome_asset2.grow_strength == biome_asset.grow_strength)
				{
					if (Randy.randomChance(0.05f))
					{
						flag = true;
					}
				}
				else
				{
					int num = Randy.randomInt(0, biome_asset.grow_strength);
					int num2 = Randy.randomInt(0, biome_asset2.grow_strength);
					if (num > num2)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			MusicBox.playSound("event:/SFX/NATURE/GrassSpawn", worldTile, pGameViewOnly: true, pVisibleOnly: true);
			TopTileType tile = biome_asset.getTile(worldTile);
			MapAction.growGreens(worldTile, tile);
			if (pCheckBonuses)
			{
				HashSet<string> biomes = World.world_era.biomes;
				if (biomes != null && biomes.Contains(worldTile.Type.biome_id))
				{
					_bonus_tiles.Add(worldTile);
				}
			}
		}
	}
}
