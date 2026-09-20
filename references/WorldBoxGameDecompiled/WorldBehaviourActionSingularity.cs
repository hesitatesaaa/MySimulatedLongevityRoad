public static class WorldBehaviourActionSingularity
{
	public static void updateSingularityTiles()
	{
		for (int i = 0; i < 3; i++)
		{
			if (!tryActionOn(TopTileLibrary.singularity_low))
			{
				break;
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (!tryActionOn(TopTileLibrary.singularity_high))
			{
				break;
			}
		}
	}

	private static bool tryActionOn(TopTileType pType)
	{
		if (pType.hashset.Count < 10)
		{
			return false;
		}
		WorldTile random = pType.getCurrentTiles().GetRandom();
		World.world.redrawRenderedTile(random);
		return true;
	}
}
