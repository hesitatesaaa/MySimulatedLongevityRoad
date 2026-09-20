using System.Collections.Generic;

public static class VortexAction
{
	private static List<VortexSwitchHelper> newTiles = new List<VortexSwitchHelper>();

	internal static void moveTiles(WorldTile pCenter, BrushData pBrush)
	{
		clear();
		BrushPixelData[] pos = pBrush.pos;
		for (int i = 0; i < pos.Length; i++)
		{
			BrushPixelData brushPixelData = pos[i];
			WorldTile tile = World.world.GetTile(pCenter.x + brushPixelData.x, pCenter.y + brushPixelData.y);
			if (tile == null)
			{
				continue;
			}
			World.world.flash_effects.flashPixel(tile, 10);
			if (!Randy.randomChance(0.8f))
			{
				WorldTile random = Randy.getRandom(tile.neighbours);
				if (random != null)
				{
					newTiles.Add(new VortexSwitchHelper
					{
						tile = random,
						newType = tile.main_type,
						newTopType = tile.top_type
					});
				}
			}
		}
		foreach (VortexSwitchHelper newTile in newTiles)
		{
			MapAction.terraformTile(newTile.tile, newTile.newType, newTile.newTopType, TerraformLibrary.flash);
		}
	}

	private static void clear()
	{
		newTiles.Clear();
	}
}
