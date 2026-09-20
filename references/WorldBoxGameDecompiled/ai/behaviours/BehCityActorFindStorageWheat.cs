namespace ai.behaviours;

public class BehCityActorFindStorageWheat : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		Building buildingOfType = pActor.city.getBuildingOfType("type_windmill", pCountOnlyFinished: true, pRandom: false, pOnlyFreeTile: false, pActor.current_island);
		if (buildingOfType != null)
		{
			pActor.beh_building_target = buildingOfType;
			return BehResult.Continue;
		}
		Building storageNear = pActor.city.getStorageNear(pActor.current_tile, pOnlyFood: true);
		if (storageNear != null)
		{
			pActor.beh_building_target = storageNear;
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
