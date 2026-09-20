namespace ai.behaviours;

public class BehCityActorFindUngrownCrop : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		using ListPool<Building> listPool = new ListPool<Building>();
		foreach (WorldTile calculated_crop in city.calculated_crops)
		{
			Building building = calculated_crop.building;
			if (!building.isRekt() && building.asset.wheat && !building.component_wheat.isMaxLevel())
			{
				listPool.Add(building);
			}
		}
		if (listPool.Count == 0)
		{
			return BehResult.Stop;
		}
		pActor.beh_building_target = listPool.GetRandom();
		return BehResult.Continue;
	}
}
