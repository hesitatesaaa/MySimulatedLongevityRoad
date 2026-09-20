using System.Collections.Generic;

namespace ai.behaviours;

public class BehFindTileBeach : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		BehaviourActionActor.possible_moves.Clear();
		findEdgesInRegion(pActor.current_tile.region);
		if (BehaviourActionActor.possible_moves.Count == 0)
		{
			for (int i = 0; i < pActor.current_tile.region.neighbours.Count; i++)
			{
				MapRegion pRegion = pActor.current_tile.region.neighbours[i];
				findEdgesInRegion(pRegion);
			}
		}
		if (BehaviourActionActor.possible_moves.Count == 0)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = BehaviourActionActor.possible_moves.GetRandom();
		BehaviourActionActor.possible_moves.Clear();
		return BehResult.Continue;
	}

	private void findEdgesInRegion(MapRegion pRegion)
	{
		List<WorldTile> edgeTiles = pRegion.getEdgeTiles();
		int count = edgeTiles.Count;
		int num = Randy.randomInt(0, count);
		for (int i = 0; i < count; i++)
		{
			int index = (i + num) % count;
			WorldTile worldTile = edgeTiles[index];
			if (worldTile.Type.ocean)
			{
				BehaviourActionActor.possible_moves.Add(worldTile);
				break;
			}
		}
	}
}
