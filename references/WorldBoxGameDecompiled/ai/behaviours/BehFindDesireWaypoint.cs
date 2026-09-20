using System.Collections.Generic;

namespace ai.behaviours;

public class BehFindDesireWaypoint : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		string building_attractor_id = pActor.kingdom.asset.building_attractor_id;
		if (string.IsNullOrEmpty(building_attractor_id))
		{
			return BehResult.Stop;
		}
		BuildingAsset buildingAsset = AssetManager.buildings.get(building_attractor_id);
		if (buildingAsset == null)
		{
			return BehResult.Stop;
		}
		HashSet<Building> buildings = buildingAsset.buildings;
		if (buildings.Count == 0)
		{
			return BehResult.Stop;
		}
		Building closestBuildingFrom = Finder.getClosestBuildingFrom(pActor, buildings);
		if (closestBuildingFrom == null)
		{
			return BehResult.Stop;
		}
		if (Toolbox.DistTile(pActor.current_tile, closestBuildingFrom.current_tile) < 10f)
		{
			return BehResult.Stop;
		}
		pActor.beh_building_target = closestBuildingFrom;
		return BehResult.Continue;
	}
}
