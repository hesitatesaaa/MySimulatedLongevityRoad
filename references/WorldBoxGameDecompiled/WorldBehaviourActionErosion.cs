using System.Collections.Generic;

public static class WorldBehaviourActionErosion
{
	private const int MAX_TILES_IN_LIST = 5;

	private static List<WorldTile> list = new List<WorldTile>();

	public static void updateErosion()
	{
		if (!WorldLawLibrary.world_law_erosion.isEnabled())
		{
			return;
		}
		list.Clear();
		foreach (TileIsland item in World.world.islands_calculator.islands.LoopRandom())
		{
			if (item.type != TileLayerType.Ground)
			{
				continue;
			}
			for (int i = 0; i < 5; i++)
			{
				WorldTile randomTile = item.getRandomTile();
				if (randomTile != null && randomTile.Type.can_errode_to_sand && (randomTile.Type.can_be_biome || randomTile.Type.grass) && randomTile.IsOceanAround() && !list.Contains(randomTile))
				{
					list.Add(randomTile);
					if (list.Count >= 5)
					{
						break;
					}
				}
			}
			if (list.Count >= 5)
			{
				break;
			}
		}
		if (list.Count != 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				MapAction.terraformMain(list[j], TileLibrary.sand, AssetManager.terraform.get("flash"));
			}
		}
	}

	public static void clear()
	{
		list.Clear();
	}
}
