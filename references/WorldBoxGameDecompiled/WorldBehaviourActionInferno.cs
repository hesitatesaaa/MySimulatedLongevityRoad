public static class WorldBehaviourActionInferno
{
	public static void updateInfernalLowAnimations()
	{
		if (TopTileLibrary.infernal_low.hashset.Count >= 10 && Randy.randomChance(0.4f))
		{
			WorldTile random = TopTileLibrary.infernal_low.getCurrentTiles().GetRandom();
			World.world.redrawRenderedTile(random);
		}
	}

	public static void updateInfernoFireAction()
	{
		tryFireAction(TopTileLibrary.infernal_high);
		tryFireAction(TopTileLibrary.infernal_low);
	}

	private static void tryFireAction(TopTileType pType)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (pType.hashset.Count >= 10 && Randy.randomChance(0.1f))
		{
			WorldTile random = pType.getCurrentTiles().GetRandom();
			random.startFire(pForce: true);
			World.world.particles_fire.spawn(random.posV3);
		}
	}
}
