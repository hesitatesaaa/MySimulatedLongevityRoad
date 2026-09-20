using System.Collections.Generic;

namespace ai.behaviours;

public class BehMoveAwayFromBlock : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.current_tile.region.island.type != TileLayerType.Block)
		{
			return BehResult.Stop;
		}
		WorldTile worldTile = null;
		int num = int.MaxValue;
		worldTile = getBestTileToMove(pActor.current_tile.region, pActor);
		if (worldTile == null)
		{
			foreach (MapRegion neighbour in pActor.current_tile.region.neighbours)
			{
				WorldTile bestTileToMove = getBestTileToMove(neighbour, pActor);
				if (bestTileToMove != null)
				{
					int num2 = Toolbox.SquaredDistTile(pActor.current_tile, bestTileToMove);
					if (num2 < num)
					{
						worldTile = bestTileToMove;
						num = num2;
					}
				}
			}
		}
		if (worldTile == null)
		{
			foreach (MapRegion insideRegionEdge in pActor.current_tile.region.island.insideRegionEdges)
			{
				WorldTile bestTileToMove2 = getBestTileToMove(insideRegionEdge, pActor);
				if (bestTileToMove2 != null)
				{
					int num3 = Toolbox.SquaredDistTile(pActor.current_tile, bestTileToMove2);
					if (num3 < num)
					{
						worldTile = bestTileToMove2;
						num = num3;
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

	private static WorldTile getBestTileToMove(MapRegion pRegion, Actor pActor)
	{
		List<WorldTile> edgeTiles = pRegion.getEdgeTiles();
		if (edgeTiles.Count == 0)
		{
			return null;
		}
		WorldTile current_tile = pActor.current_tile;
		WorldTile result = null;
		int num = int.MaxValue;
		TileIsland island = current_tile.region.island;
		foreach (WorldTile item in edgeTiles.LoopRandom())
		{
			int num2 = Toolbox.SquaredDistTile(current_tile, item);
			if (num2 < num)
			{
				MapRegion region = item.region;
				if (isGoodTileRegion(region, pActor) && region.island != island && region.island.getTileCount() > 5)
				{
					result = item;
					num = num2;
				}
			}
		}
		return result;
	}

	private static bool isGoodTileRegion(MapRegion pRegion, Actor pActor)
	{
		if (pRegion.type == TileLayerType.Ocean && pActor.isDamagedByOcean())
		{
			return false;
		}
		if (pRegion.type == TileLayerType.Lava && pActor.asset.die_in_lava)
		{
			return false;
		}
		return true;
	}
}
