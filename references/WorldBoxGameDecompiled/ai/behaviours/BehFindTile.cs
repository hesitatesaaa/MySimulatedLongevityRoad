namespace ai.behaviours;

public class BehFindTile : BehaviourActionActor
{
	private TileFinderType _type;

	public BehFindTile(TileFinderType pType)
	{
		_type = pType;
	}

	public override BehResult execute(Actor pActor)
	{
		if (_type == TileFinderType.NewRoad)
		{
			using (ListPool<WorldTile> listPool = new ListPool<WorldTile>(5))
			{
				WorldTile roadTileToBuild = pActor.city.getRoadTileToBuild(pActor);
				if (roadTileToBuild != null)
				{
					listPool.Add(roadTileToBuild);
				}
				if (listPool.Count == 0)
				{
					return BehResult.Stop;
				}
				pActor.beh_tile_target = Randy.getRandom(listPool);
				return BehResult.Continue;
			}
		}
		WorldTile worldTile = Finder.findTileInChunk(pActor.current_tile, _type);
		if (worldTile != null)
		{
			pActor.beh_tile_target = worldTile;
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
