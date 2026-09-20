namespace ai.behaviours;

public class BehFindTileWhenOnFire : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		WorldTile worldTile = findWaterIn(pActor.chunk);
		if (worldTile == null)
		{
			MapChunk[] neighbours_all = pActor.chunk.neighbours_all;
			foreach (MapChunk pChunk in neighbours_all)
			{
				worldTile = findWaterIn(pChunk);
				if (worldTile != null)
				{
					break;
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

	private WorldTile findWaterIn(MapChunk pChunk)
	{
		foreach (MapRegion item in pChunk.regions.LoopRandom())
		{
			if (item.type == TileLayerType.Ocean)
			{
				return item.tiles.GetRandom();
			}
		}
		return null;
	}
}
