using System.Collections.Generic;

public class WorldBehaviourTilesTemperatureFreeze : WorldBehaviourTilesRunner
{
	private static float timer_freeze_summits = 0f;

	private static List<WorldTile> _summit_tiles = new List<WorldTile>();

	public static void update()
	{
		freezeSummits();
		if (World.world_era.global_freeze_world)
		{
			updateSingleTiles();
		}
	}

	private static void freezeSummits()
	{
		if (World.world_era.overlay_sun)
		{
			return;
		}
		if (timer_freeze_summits > 0f)
		{
			timer_freeze_summits -= World.world.elapsed;
			return;
		}
		timer_freeze_summits = Randy.randomFloat(10f, 60f);
		if (TileLibrary.summit.hashset.Count == 0)
		{
			return;
		}
		_summit_tiles.AddRange(TileLibrary.summit.hashset);
		foreach (WorldTile summit_tile in _summit_tiles)
		{
			if (!Randy.randomChance(0.8f) && summit_tile.canBeFrozen())
			{
				summit_tile.freeze(5);
			}
		}
		_summit_tiles.Clear();
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
			if (worldTile.canBeFrozen())
			{
				worldTile.freeze(5);
			}
		}
	}
}
