public class WorldBehaviourActionLava : WorldBehaviourTilesRunner
{
	public const int CHUNK_MOD_AMOUNT = 15;

	public static void update()
	{
		if (hasLava())
		{
			AchievementLibrary.the_hell.check();
			updateSingleTiles();
		}
	}

	public static bool hasLava()
	{
		if (TileLibrary.lava0.hashset.Count > 0)
		{
			return true;
		}
		if (TileLibrary.lava1.hashset.Count > 0)
		{
			return true;
		}
		if (TileLibrary.lava2.hashset.Count > 0)
		{
			return true;
		}
		if (TileLibrary.lava3.hashset.Count > 0)
		{
			return true;
		}
		return false;
	}

	private static void updateSingleTiles()
	{
		WorldBehaviourTilesRunner.checkTiles();
		WorldTile[] tiles_to_check = WorldBehaviourTilesRunner._tiles_to_check;
		int num = World.world.map_chunk_manager.amount_x * 15;
		if (WorldBehaviourTilesRunner._tile_next_check + num >= tiles_to_check.Length)
		{
			num = tiles_to_check.Length - WorldBehaviourTilesRunner._tile_next_check;
		}
		while (num-- > 0)
		{
			WorldBehaviourTilesRunner._tiles_to_check.ShuffleOne(WorldBehaviourTilesRunner._tile_next_check);
			WorldTile worldTile = tiles_to_check[WorldBehaviourTilesRunner._tile_next_check++];
			if (worldTile.Type.lava)
			{
				tryToMoveLava(worldTile);
			}
		}
	}

	private static void damageBuildingOnTile(WorldTile pTile)
	{
		if (pTile.hasBuilding() && pTile.building.asset.affected_by_lava)
		{
			float pDamage = pTile.building.getMaxHealthPercent(0.4f);
			pTile.building.getHit(pDamage);
		}
	}

	private static bool tryToMoveLava(WorldTile pTile)
	{
		bool result = false;
		damageBuildingOnTile(pTile);
		if (WorldLawLibrary.world_law_forever_lava.isEnabled())
		{
			return false;
		}
		if (World.world.getWorldTimeElapsedSince(pTile.timestamp_type_changed) < (float)pTile.Type.lava_change_state_after)
		{
			return false;
		}
		if (pTile.Type.lava_level == 0)
		{
			if (WorldLawLibrary.world_law_forever_lava.isEnabled())
			{
				return false;
			}
			bool flag = false;
			WorldTile[] neighbours = pTile.neighbours;
			foreach (WorldTile worldTile in neighbours)
			{
				if (worldTile.Type.lava && worldTile.Type.lava_level > 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				changeLavaTile(pTile, TileLibrary.hills);
				result = true;
			}
		}
		else if (Randy.randomBool())
		{
			result = moveLava(pTile);
			if (pTile.flash_state <= 0 && Randy.randomChance(0.995f))
			{
				pTile.startFire(pForce: true);
			}
		}
		return result;
	}

	private static bool moveLava(WorldTile pLavaTile)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (Randy.randomChance(0.1f))
		{
			World.world.particles_fire.spawn(pLavaTile.posV3);
		}
		WorldTile worldTile = null;
		foreach (WorldTile item in pLavaTile.neighbours.LoopRandom())
		{
			if (!item.Type.lava)
			{
				if (item.isTemporaryFrozen())
				{
					World.world.heat.addTile(item, 15);
					item.unfreeze(99);
				}
				else
				{
					item.startFire();
				}
			}
			if (item.Type.hold_lava || item.Type.lava_level == pLavaTile.Type.lava_level || (item.Type.lava && string.IsNullOrEmpty(item.Type.lava_increase)))
			{
				continue;
			}
			if (worldTile == null)
			{
				worldTile = item;
			}
			else
			{
				if (worldTile.Type.render_z < item.Type.render_z)
				{
					continue;
				}
				if (worldTile.Type.lava)
				{
					if (item.Type.lava_level < worldTile.Type.lava_level)
					{
						worldTile = item;
					}
				}
				else
				{
					worldTile = item;
				}
			}
		}
		if (worldTile == null)
		{
			return false;
		}
		changeLavaTile(pLavaTile, pLavaTile.Type.lava_decrease);
		if (!worldTile.Type.lava)
		{
			changeLavaTile(worldTile, TileLibrary.lava0);
		}
		else
		{
			changeLavaTile(worldTile, pLavaTile.Type.lava_increase);
		}
		World.world.flash_effects.flashPixel(worldTile, 10);
		return true;
	}

	private static void changeLavaTile(WorldTile pTile, TileType pType)
	{
		if (pTile.Type != pType)
		{
			MapAction.terraformMain(pTile, pType, TerraformLibrary.lava_damage);
			damageBuildingOnTile(pTile);
		}
	}

	private static void changeLavaTile(WorldTile pTile, string pType)
	{
		changeLavaTile(pTile, AssetManager.tiles.get(pType));
	}
}
