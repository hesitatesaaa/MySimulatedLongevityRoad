using System.Collections.Generic;

public static class WorldBehaviourOcean
{
	private static List<WorldTile> tiles_to_update = new List<WorldTile>();

	public static HashSetWorldTile tiles = new HashSetWorldTile();

	public static void clear()
	{
		tiles_to_update.Clear();
		tiles.Clear();
	}

	public static void updateOcean()
	{
		if (tiles.Count == 0)
		{
			return;
		}
		tiles_to_update.Clear();
		foreach (WorldTile tile in tiles)
		{
			if (tile.world_edge)
			{
				if (!((float)Randy.randomInt(0, 100) < 30f))
				{
					tiles_to_update.Add(tile);
				}
			}
			else if (tile.IsOceanAround() && !((float)Randy.randomInt(0, 100) < 30f))
			{
				tiles_to_update.Add(tile);
			}
		}
		for (int i = 0; i < tiles_to_update.Count; i++)
		{
			WorldTile worldTile = tiles_to_update[i];
			if (worldTile.Type.can_be_filled_with_ocean)
			{
				if (worldTile.Type.explodable_by_ocean)
				{
					World.world.explosion_layer.explodeBomb(worldTile);
				}
				else
				{
					MapAction.setOcean(worldTile);
				}
			}
		}
	}
}
