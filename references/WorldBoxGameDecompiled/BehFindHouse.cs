using ai.behaviours;

public class BehFindHouse : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasHouse())
		{
			return BehResult.Stop;
		}
		Building building = null;
		foreach (Building building2 in pActor.city.buildings)
		{
			if (!building2.isUnderConstruction() && building2.hasResidentSlots())
			{
				building = building2;
				break;
			}
		}
		if (building == null)
		{
			building = tryToFindFamilyHouse(pActor);
		}
		if (building == null)
		{
			return BehResult.Stop;
		}
		pActor.setHomeBuilding(building);
		pActor.changeHappiness("just_found_house", building.asset.housing_happiness);
		return BehResult.Continue;
	}

	private static Building tryToFindFamilyHouse(Actor pActor)
	{
		if (!pActor.hasFamily())
		{
			return null;
		}
		int num = 0;
		Family family = pActor.family;
		foreach (Actor item in pActor.family.units.LoopRandom())
		{
			if (item == pActor)
			{
				continue;
			}
			if (++num > 5)
			{
				break;
			}
			if (item.hasHouse() && item.city == pActor.city)
			{
				Building building = checkBuilding(item.home_building, family);
				if (building != null)
				{
					return building;
				}
			}
		}
		return null;
	}

	private static Building checkBuilding(Building pGetHomeBuilding, Family pFamily)
	{
		foreach (long resident in pGetHomeBuilding.residents)
		{
			Actor actor = BehaviourActionBase<Actor>.world.units.get(resident);
			if (actor != null && actor.isAlive() && actor.family == pFamily)
			{
				actor.clearHomeBuilding();
				return pGetHomeBuilding;
			}
		}
		return null;
	}
}
