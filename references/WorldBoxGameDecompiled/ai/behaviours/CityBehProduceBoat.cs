namespace ai.behaviours;

public class CityBehProduceBoat : BehaviourActionCity
{
	public override BehResult execute(City pCity)
	{
		if (pCity.timer_build_boat > 0f)
		{
			return BehResult.Stop;
		}
		Building buildingOfType = pCity.getBuildingOfType("type_docks", pCountOnlyFinished: true, pRandom: true);
		if (buildingOfType == null)
		{
			return BehResult.Stop;
		}
		Actor actor = buildingOfType.component_docks.buildBoatFromHere(pCity);
		if (actor == null)
		{
			return BehResult.Stop;
		}
		actor.joinKingdom(pCity.kingdom);
		actor.joinCity(pCity);
		pCity.timer_build_boat = 10f;
		return BehResult.Continue;
	}
}
