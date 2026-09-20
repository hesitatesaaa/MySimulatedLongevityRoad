using System.Collections.Generic;

namespace ai.behaviours;

public class BehWalkIntoWaterCorner : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		TileIsland island = pActor.current_tile.region.island;
		if (island.isGoodIslandForActor(pActor))
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = null;
		int num = int.MaxValue;
		foreach (MapRegion insideRegionEdge in island.insideRegionEdges)
		{
			List<WorldTile> edgeTiles = insideRegionEdge.getEdgeTiles();
			for (int i = 0; i < edgeTiles.Count; i++)
			{
				WorldTile worldTile2 = edgeTiles[i];
				if (worldTile2.Type.ocean)
				{
					int num2 = Toolbox.SquaredDistTile(pActor.current_tile, worldTile2);
					if (num2 < num)
					{
						worldTile = worldTile2;
						num = num2;
					}
				}
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
