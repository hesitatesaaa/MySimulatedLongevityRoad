namespace ai.behaviours;

public class BehFindTileForFarm : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		Building buildingOfType = pActor.city.getBuildingOfType("type_windmill");
		if (buildingOfType == null)
		{
			return BehResult.Stop;
		}
		int num = int.MaxValue;
		WorldTile worldTile = null;
		foreach (WorldTile calculated_place_for_farm in pActor.city.calculated_place_for_farms)
		{
			int num2 = Toolbox.SquaredDistTile(buildingOfType.current_tile, calculated_place_for_farm);
			if (num2 < num && (!calculated_place_for_farm.hasBuilding() || calculated_place_for_farm.building.canRemoveForFarms()) && !calculated_place_for_farm.isTargeted() && pActor.current_tile.isSameIsland(calculated_place_for_farm) && calculated_place_for_farm.IsTypeAround(TopTileLibrary.field))
			{
				num = num2;
				worldTile = calculated_place_for_farm;
			}
		}
		if (worldTile == null)
		{
			foreach (WorldTile calculated_place_for_farm2 in pActor.city.calculated_place_for_farms)
			{
				int num3 = Toolbox.SquaredDistTile(buildingOfType.current_tile, calculated_place_for_farm2);
				if (num3 < num && (!calculated_place_for_farm2.hasBuilding() || calculated_place_for_farm2.building.canRemoveForFarms()) && !calculated_place_for_farm2.isTargeted() && pActor.current_tile.isSameIsland(calculated_place_for_farm2))
				{
					num = num3;
					worldTile = calculated_place_for_farm2;
				}
			}
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
