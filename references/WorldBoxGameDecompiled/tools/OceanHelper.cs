using System.Collections.Generic;

namespace tools;

public class OceanHelper
{
	private static List<TileIsland> _ocean_pools_with_docks = new List<TileIsland>();

	public static bool goodForNewDock(WorldTile pTile)
	{
		if (!pTile.Type.ocean)
		{
			return false;
		}
		if (pTile.region?.island == null)
		{
			return false;
		}
		if (!pTile.region.island.goodForDocks())
		{
			return false;
		}
		if (_ocean_pools_with_docks.Contains(pTile.region.island))
		{
			return false;
		}
		return true;
	}

	public static void saveOceanPoolsWithDocks(City pCity)
	{
		List<Building> buildingListOfType = pCity.getBuildingListOfType("type_docks");
		if (buildingListOfType == null || buildingListOfType.Count == 0)
		{
			return;
		}
		for (int i = 0; i < buildingListOfType.Count; i++)
		{
			Building building = buildingListOfType[i];
			for (int j = 0; j < building.tiles.Count; j++)
			{
				addOceanPool(building.tiles[j]);
			}
		}
	}

	public static void clearOceanPools()
	{
		_ocean_pools_with_docks.Clear();
	}

	private static void addOceanPool(WorldTile pTile)
	{
		if (pTile.region.island != null && pTile.region.type == TileLayerType.Ocean)
		{
			_ocean_pools_with_docks.Add(pTile.region.island);
		}
	}

	public static WorldTile findTileForBoat(WorldTile pTileBoat, WorldTile pTileTarget)
	{
		WorldTile worldTile = findWaterTileInRegion(pTileTarget.region, pTileBoat);
		if (worldTile == null)
		{
			worldTile = findTileInWholeIsland(pTileTarget.region.island, pTileBoat);
		}
		if (worldTile == null)
		{
			return null;
		}
		for (int i = 0; i < worldTile.neighboursAll.Length; i++)
		{
			WorldTile worldTile2 = worldTile.neighboursAll[i];
			if (worldTile2.isSameIsland(pTileBoat) && worldTile2.isGoodForBoat())
			{
				worldTile = worldTile2;
				break;
			}
		}
		World.world.flash_effects.flashPixel(worldTile);
		return worldTile;
	}

	private static WorldTile findTileInWholeIsland(TileIsland pIslandTarget, WorldTile pTileBoat)
	{
		WorldTile result = null;
		int num = 10;
		float num2 = float.MaxValue;
		foreach (MapRegion item in pIslandTarget.regions.getSimpleList().LoopRandom())
		{
			WorldTile worldTile = findWaterTileInRegion(item, pTileBoat);
			if (worldTile == null)
			{
				continue;
			}
			float num3 = Toolbox.DistTile(pTileBoat, worldTile);
			if (num3 < num2)
			{
				result = worldTile;
				num2 = num3;
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
		return result;
	}

	private static WorldTile findWaterTileInRegion(MapRegion pRegion, WorldTile pBoatTile)
	{
		List<WorldTile> edgeTiles = pRegion.getEdgeTiles();
		if (edgeTiles.Count == 0)
		{
			return null;
		}
		foreach (WorldTile item in edgeTiles.LoopRandom())
		{
			if (item.isSameIsland(pBoatTile))
			{
				return item;
			}
		}
		return null;
	}
}
