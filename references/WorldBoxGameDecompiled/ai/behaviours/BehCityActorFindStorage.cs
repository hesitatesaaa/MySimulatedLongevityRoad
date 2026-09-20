namespace ai.behaviours;

public class BehCityActorFindStorage : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		Building storageNear = pActor.city.getStorageNear(pActor.current_tile);
		if (storageNear != null)
		{
			pActor.beh_building_target = storageNear;
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
