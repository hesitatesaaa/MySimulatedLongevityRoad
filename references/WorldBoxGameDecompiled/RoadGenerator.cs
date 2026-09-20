using System.Collections.Generic;

public class RoadGenerator
{
	internal static List<WorldTile> checkedTiles = new List<WorldTile>();

	private static List<WorldTile> nextWave = new List<WorldTile>();

	private static List<WorldTile> curWave = new List<WorldTile>();

	private static List<Building> targetBuildings = new List<Building>();

	private static List<WorldTile> path = new List<WorldTile>();

	internal static void generateRoadFor(Building pBuilding)
	{
		City city = pBuilding.city;
		if (city != null && city.buildings.Count > 2 && pBuilding.door_tile.Type.ground)
		{
			calcFlow(pBuilding.door_tile, pBuilding, 12);
		}
	}

	private static void calcFlow(WorldTile pStartTile, Building pBuilding, int pMaxWave)
	{
		targetBuildings.Clear();
		checkedTiles.Clear();
		curWave.Clear();
		nextWave.Clear();
		nextWave.Add(pStartTile);
		checkedTiles.Add(pStartTile);
		int num = 0;
		while (nextWave.Count > 0 || curWave.Count > 0)
		{
			if (curWave.Count == 0)
			{
				if (num > pMaxWave)
				{
					break;
				}
				curWave.AddRange(nextWave);
				nextWave.Clear();
				num++;
			}
			WorldTile worldTile = curWave[curWave.Count - 1];
			curWave.RemoveAt(curWave.Count - 1);
			worldTile.is_checked_tile = true;
			worldTile.score = num;
			World.world.flash_effects.flashPixel(worldTile);
			if (worldTile.hasBuilding() && worldTile.building.city == pBuilding.city && worldTile.building != pBuilding && worldTile.building.current_tile == worldTile)
			{
				targetBuildings.Add(worldTile.building);
			}
			for (int i = 0; i < worldTile.neighboursAll.Length; i++)
			{
				WorldTile worldTile2 = worldTile.neighboursAll[i];
				if (!worldTile2.is_checked_tile)
				{
					worldTile2.is_checked_tile = true;
					checkedTiles.Add(worldTile2);
					if (!worldTile2.Type.liquid && worldTile2.Type.layer_type != TileLayerType.Block)
					{
						nextWave.Add(worldTile2);
					}
				}
			}
		}
		for (int j = 0; j < targetBuildings.Count; j++)
		{
			Building building = targetBuildings[j];
			path.Clear();
			if (building.asset.build_road_to)
			{
				findPath(building.door_tile);
				fillPath();
			}
		}
		for (int k = 0; k < checkedTiles.Count; k++)
		{
			WorldTile worldTile3 = checkedTiles[k];
			worldTile3.is_checked_tile = false;
			worldTile3.score = -1;
		}
	}

	private static void fillPath()
	{
		for (int i = 0; i < path.Count; i++)
		{
			MapAction.createRoadTile(path[i]);
		}
	}

	private static void findPath(WorldTile pTile)
	{
		path.Add(pTile);
		if (pTile.score <= 1)
		{
			return;
		}
		WorldTile worldTile = null;
		for (int i = 0; i < pTile.neighboursAll.Length; i++)
		{
			WorldTile worldTile2 = pTile.neighboursAll[i];
			if (worldTile2.score == pTile.score - 1)
			{
				if (worldTile == null)
				{
					worldTile = worldTile2;
				}
				else if (worldTile2.Type.road)
				{
					worldTile = worldTile2;
					findPath(worldTile);
					return;
				}
			}
		}
		findPath(worldTile);
	}
}
