namespace ai.behaviours;

public class BehBuildingTargetLoverHome : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasLover())
		{
			return BehResult.Stop;
		}
		Building loverHomeBuilding = getLoverHomeBuilding(pActor, pActor.lover);
		if (loverHomeBuilding == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_building_target = loverHomeBuilding;
		return BehResult.Continue;
	}

	private Building getLoverHomeBuilding(Actor pActor1, Actor pActor2)
	{
		if (pActor1.hasHouse() && pActor2.hasHouse())
		{
			if (pActor1.isSexMale())
			{
				return pActor1.getHomeBuilding();
			}
			return pActor2.getHomeBuilding();
		}
		if (pActor1.hasHouse())
		{
			return pActor1.getHomeBuilding();
		}
		if (pActor2.hasHouse())
		{
			return pActor2.getHomeBuilding();
		}
		return null;
	}
}
