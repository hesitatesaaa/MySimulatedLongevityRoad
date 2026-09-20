namespace ai.behaviours;

public class BehFindBuildingWithFood : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		Building storageWithFoodNear = pActor.city.getStorageWithFoodNear(pActor.current_tile);
		if (storageWithFoodNear != null)
		{
			pActor.beh_building_target = storageWithFoodNear;
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
