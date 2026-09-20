using System.Collections.Generic;

namespace ai.behaviours;

public class BehFindTileNearbyGroupLeader : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasArmy())
		{
			return BehResult.Stop;
		}
		if (!pActor.army.hasCaptain())
		{
			return BehResult.Stop;
		}
		Actor captain = pActor.army.getCaptain();
		WorldTile worldTile = null;
		List<WorldTile> current_path = captain.current_path;
		if (current_path != null && current_path.Count > 0)
		{
			worldTile = current_path[current_path.Count - 1].region.tiles.GetRandom();
		}
		else
		{
			MapRegion mapRegion = captain.current_tile.region;
			if (mapRegion.tiles.Count < 20 && mapRegion.neighbours.Count > 0)
			{
				mapRegion = mapRegion.neighbours.GetRandom();
			}
			worldTile = mapRegion.tiles.GetRandom();
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}
}
