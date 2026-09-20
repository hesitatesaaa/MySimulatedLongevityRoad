using ai.behaviours;

public class CityBehCheckFarms : BehaviourActionCity
{
	public override bool shouldRetry(City pCity)
	{
		return false;
	}

	public override BehResult execute(City pCity)
	{
		check(pCity);
		return BehResult.Continue;
	}

	public static void check(City pCity)
	{
		pCity.calculated_place_for_farms.Clear();
		pCity.calculated_grown_wheat.Clear();
		pCity.calculated_farm_fields.Clear();
		pCity.calculated_crops.Clear();
		behFindTileForFarm(pCity);
		pCity.calculated_place_for_farms.checkAddRemove();
		pCity.calculated_farm_fields.checkAddRemove();
		pCity.calculated_crops.checkAddRemove();
		behCheckWheat(pCity);
		pCity.calculated_grown_wheat.checkAddRemove();
	}

	private static void behCheckWheat(City pCity)
	{
		foreach (WorldTile calculated_crop in pCity.calculated_crops)
		{
			if (calculated_crop.hasBuilding() && calculated_crop.building.asset.wheat && calculated_crop.building.component_wheat.isMaxLevel())
			{
				pCity.calculated_grown_wheat.Add(calculated_crop);
			}
		}
	}

	private static void behFindTileForFarm(City pCity)
	{
		Building buildingOfType = pCity.getBuildingOfType("type_windmill");
		if (buildingOfType != null)
		{
			checkRegion(buildingOfType.current_tile.region, buildingOfType, pCity);
			for (int i = 0; i < buildingOfType.current_tile.region.neighbours.Count; i++)
			{
				checkRegion(buildingOfType.current_tile.region.neighbours[i], buildingOfType, pCity);
			}
		}
	}

	private static void checkRegion(MapRegion pRegion, Building pBuilding, City pCity)
	{
		MapChunk chunk = pRegion.chunk;
		for (int i = 0; i < chunk.zones.Count; i++)
		{
			checkZone(chunk.zones[i], pBuilding, pCity);
		}
	}

	private static void checkZone(TileZone pZone, Building pBuilding, City pCity)
	{
		if (!pZone.isSameCityHere(pCity))
		{
			return;
		}
		WorldTile[] tiles = pZone.tiles;
		int num = tiles.Length;
		for (int i = 0; i < num; i++)
		{
			WorldTile worldTile = tiles[i];
			if ((float)Toolbox.SquaredDistTile(pBuilding.current_tile, worldTile) > 81f)
			{
				continue;
			}
			if (worldTile.Type.can_be_farm)
			{
				pCity.calculated_place_for_farms.Add(worldTile);
			}
			if (worldTile.Type.farm_field)
			{
				pCity.calculated_farm_fields.Add(worldTile);
				if (worldTile.hasBuilding() && worldTile.building.asset.wheat)
				{
					pCity.calculated_crops.Add(worldTile);
				}
			}
		}
	}
}
