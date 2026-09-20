using System.Collections.Generic;

public static class BuildingHelper
{
	private static List<WorldTile> _list_tiles = new List<WorldTile>();

	public static void tryToBuildNear(WorldTile pTile, string pAssetID)
	{
		BuildingAsset buildingAsset = AssetManager.buildings.get(pAssetID);
		if (buildingAsset != null)
		{
			if (World.world.buildings.canBuildFrom(pTile, buildingAsset, null))
			{
				World.world.buildings.addBuilding(buildingAsset, pTile);
			}
			else
			{
				tryToBuildNear(pTile, buildingAsset);
			}
		}
	}

	public static bool tryToBuildNear(WorldTile pTile, BuildingAsset pAsset)
	{
		List<WorldTile> list_tiles = _list_tiles;
		fillEmptyTilesAroundMine(pTile, list_tiles);
		bool result = tryToPlaceBuilding(pAsset, list_tiles);
		list_tiles.Clear();
		return result;
	}

	private static void fillEmptyTilesAroundMine(WorldTile pTile, List<WorldTile> pList)
	{
		pList.Clear();
		int num = 4;
		int num2 = pTile.x - num;
		int num3 = pTile.y - num;
		for (int i = 0; i < num * 2; i++)
		{
			for (int j = 0; j < num * 2; j++)
			{
				WorldTile tile = World.world.GetTile(i + num2, j + num3);
				if (tile != null && (!tile.hasBuilding() || !tile.building.isUsable() || !tile.building.asset.city_building))
				{
					pList.Add(tile);
				}
			}
		}
	}

	private static bool tryToPlaceBuilding(BuildingAsset pAsset, List<WorldTile> pList)
	{
		foreach (WorldTile item in pList.LoopRandom())
		{
			if (World.world.buildings.canBuildFrom(item, pAsset, null))
			{
				if (World.world.buildings.addBuilding(pAsset, item) != null)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}
}
