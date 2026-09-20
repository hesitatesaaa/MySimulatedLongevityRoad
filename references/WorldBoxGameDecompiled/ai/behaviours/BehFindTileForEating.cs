using System.Collections.Generic;

namespace ai.behaviours;

public class BehFindTileForEating : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		WorldTile worldTile = null;
		worldTile = findTileAround(pActor.current_tile.neighboursAll);
		if (worldTile == null)
		{
			worldTile = findTileAround(pActor.current_tile.region.tiles);
		}
		if (worldTile == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = worldTile;
		return BehResult.Continue;
	}

	private WorldTile findTileAround(IEnumerable<WorldTile> pList)
	{
		WorldTile worldTile = null;
		foreach (WorldTile p in pList)
		{
			if (p.Type.canBeEatenByGeophag())
			{
				if (worldTile == null)
				{
					worldTile = p;
				}
				else if (Randy.randomBool())
				{
					worldTile = p;
					break;
				}
			}
		}
		return worldTile;
	}
}
