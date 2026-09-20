public class WorldBehaviourTilesTemperatureUnFreeze : WorldBehaviourTilesRunner
{
	public static void update()
	{
		if (World.world_era.global_unfreeze_world && !WorldLawLibrary.world_law_forever_cold.isEnabled())
		{
			updateSingleTiles();
		}
	}

	public static void updateSingleTiles()
	{
		WorldBehaviourTilesRunner.checkTiles();
		WorldTile[] tiles_to_check = WorldBehaviourTilesRunner._tiles_to_check;
		int num = World.world.map_chunk_manager.amount_x * 10;
		if (WorldBehaviourTilesRunner._tile_next_check + num >= tiles_to_check.Length)
		{
			num = tiles_to_check.Length - WorldBehaviourTilesRunner._tile_next_check;
		}
		while (num-- > 0)
		{
			WorldBehaviourTilesRunner._tiles_to_check.ShuffleOne(WorldBehaviourTilesRunner._tile_next_check);
			WorldTile worldTile = tiles_to_check[WorldBehaviourTilesRunner._tile_next_check++];
			if (worldTile.isTemporaryFrozen() && (!worldTile.Type.mountains || checkMountainUnfreeze(worldTile)))
			{
				worldTile.unfreeze(5 + World.world_era.temperature_damage_bonus);
			}
		}
	}

	private static bool checkMountainUnfreeze(WorldTile pTile)
	{
		if (World.world_era.global_unfreeze_world_mountains)
		{
			return true;
		}
		if (pTile.Type.summit && !World.world_era.overlay_sun)
		{
			return false;
		}
		if (Randy.randomChance(0.9f))
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < pTile.neighboursAll.Length; i++)
		{
			if (!pTile.neighboursAll[i].isFrozen())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		return true;
	}
}
