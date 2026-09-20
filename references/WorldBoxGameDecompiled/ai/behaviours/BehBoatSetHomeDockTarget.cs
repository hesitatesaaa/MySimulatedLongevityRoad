namespace ai.behaviours;

public class BehBoatSetHomeDockTarget : BehBoat
{
	public override BehResult execute(Actor pActor)
	{
		checkHomeDocks(pActor);
		Building homeBuilding = boat.actor.getHomeBuilding();
		if (homeBuilding == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_building_target = homeBuilding;
		return BehResult.Continue;
	}
}
