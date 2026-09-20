public static class WorldBehaviourActionSwampAnimation
{
	public static void updateSwampTiles()
	{
		if (TopTileLibrary.swamp_low.hashset.Count >= 10)
		{
			WorldTile random = TopTileLibrary.swamp_low.getCurrentTiles().GetRandom();
			World.world.redrawRenderedTile(random);
		}
	}
}
