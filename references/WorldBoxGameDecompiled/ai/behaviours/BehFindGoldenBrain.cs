using System.Collections.Generic;

namespace ai.behaviours;

public class BehFindGoldenBrain : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (WorldLawLibrary.world_law_peaceful_monsters.isEnabled())
		{
			return BehResult.Stop;
		}
		List<Building> buildings = BehaviourActionBase<Actor>.world.kingdoms_wild.get("golden_brain").buildings;
		if (buildings.Count == 0)
		{
			return BehResult.Stop;
		}
		Building closestBuildingFrom = Finder.getClosestBuildingFrom(pActor, buildings);
		if (closestBuildingFrom == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_building_target = closestBuildingFrom;
		return BehResult.Continue;
	}
}
