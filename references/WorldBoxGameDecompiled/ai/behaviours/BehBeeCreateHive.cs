namespace ai.behaviours;

public class BehBeeCreateHive : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (isAnotherBeehiveNearby(pActor))
		{
			return BehResult.Stop;
		}
		Building beh_building_target = BehaviourActionBase<Actor>.world.buildings.addBuilding("beehive", pActor.beh_tile_target, pCheckForBuild: true);
		pActor.beh_building_target = beh_building_target;
		return BehResult.Continue;
	}

	public static bool isAnotherBeehiveNearby(Actor pActor)
	{
		foreach (Building item in Finder.getBuildingsFromChunk(pActor.current_tile, 2))
		{
			if (item.asset.id == "beehive")
			{
				return true;
			}
		}
		return false;
	}
}
