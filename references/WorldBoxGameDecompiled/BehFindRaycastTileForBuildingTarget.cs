using System.Collections.Generic;
using ai.behaviours;

public class BehFindRaycastTileForBuildingTarget : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Building beh_building_target = pActor.beh_building_target;
		if (beh_building_target == null)
		{
			return BehResult.Stop;
		}
		WorldTile current_tile = beh_building_target.current_tile;
		WorldTile current_tile2 = pActor.current_tile;
		if (!current_tile.isSameIsland(current_tile2))
		{
			return BehResult.Stop;
		}
		List<WorldTile> list = PathfinderTools.raycast(current_tile2, current_tile);
		WorldTile worldTile = null;
		float resourceThrowDistance = pActor.getResourceThrowDistance();
		for (int i = 0; i < list.Count; i++)
		{
			WorldTile worldTile2 = list[i];
			if (worldTile2.isSameIsland(current_tile2) && Toolbox.DistTile(worldTile2, current_tile) < resourceThrowDistance)
			{
				worldTile = worldTile2;
				break;
			}
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
