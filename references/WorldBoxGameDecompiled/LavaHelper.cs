public static class LavaHelper
{
	public static void heatUpLava(WorldTile pTile)
	{
		if (!string.IsNullOrEmpty(pTile.Type.lava_increase))
		{
			changeLavaTile(pTile, pTile.Type.lava_increase);
		}
	}

	public static void coolDownLava(WorldTile pTile)
	{
		if (!string.IsNullOrEmpty(pTile.Type.lava_decrease))
		{
			changeLavaTile(pTile, pTile.Type.lava_decrease);
		}
		else
		{
			putOut(pTile);
		}
	}

	public static void addLava(WorldTile pTile, string pType = "lava3")
	{
		if (!pTile.Type.lava || !string.IsNullOrEmpty(pTile.Type.lava_increase))
		{
			pTile.startFire();
			MapAction.terraformMain(pTile, AssetManager.tiles.get(pType), TerraformLibrary.lava_damage);
		}
	}

	private static void changeLavaTile(WorldTile pTile, string pType)
	{
		MapAction.terraformMain(pTile, AssetManager.tiles.get(pType), TerraformLibrary.lava_damage);
	}

	public static void putOut(WorldTile pTile)
	{
		if (!WorldLawLibrary.world_law_forever_lava.isEnabled() && !WorldLawLibrary.world_law_gaias_covenant.isEnabled())
		{
			MapAction.increaseTile(pTile, pDamage: false);
		}
	}

	public static void removeLava(WorldTile pTile)
	{
		MapAction.decreaseTile(pTile, pDamage: false);
	}
}
