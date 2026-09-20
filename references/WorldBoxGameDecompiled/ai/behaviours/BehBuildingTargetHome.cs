namespace ai.behaviours;

public class BehBuildingTargetHome : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		Building homeBuilding = pActor.getHomeBuilding();
		if (homeBuilding == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_building_target = homeBuilding;
		return BehResult.Continue;
	}
}
