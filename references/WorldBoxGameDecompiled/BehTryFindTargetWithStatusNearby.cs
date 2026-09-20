using ai.behaviours;

public class BehTryFindTargetWithStatusNearby : BehaviourActionActor
{
	private string[] _status_ids;

	public BehTryFindTargetWithStatusNearby(params string[] pStatusIDs)
	{
		_status_ids = pStatusIDs;
	}

	public override BehResult execute(Actor pActor)
	{
		Actor closestActorWithStatus = getClosestActorWithStatus(pActor, _status_ids);
		if (closestActorWithStatus == null)
		{
			WorldTile worldTile = Finder.findTileInChunk(pActor.current_tile, TileFinderType.FreeTile);
			if (worldTile == null)
			{
				return BehResult.Stop;
			}
			pActor.beh_tile_target = worldTile;
			return BehResult.Continue;
		}
		pActor.beh_tile_target = closestActorWithStatus.current_tile.getTileAroundThisOnSameIsland(pActor.current_tile);
		pActor.beh_actor_target = closestActorWithStatus;
		return BehResult.Continue;
	}

	private Actor getClosestActorWithStatus(Actor pSelf, string[] pStatusIDs)
	{
		bool flag = Randy.randomBool();
		int num = int.MaxValue;
		Actor result = null;
		foreach (Actor item in Finder.getUnitsFromChunk(pSelf.current_tile, 1, 0f, flag))
		{
			if (item == pSelf)
			{
				continue;
			}
			int num2 = Toolbox.SquaredDistTile(item.current_tile, pSelf.current_tile);
			if (num2 >= num || !pSelf.isSameIslandAs(item) || !item.hasAnyStatusEffect())
			{
				continue;
			}
			bool flag2 = false;
			foreach (string pID in pStatusIDs)
			{
				if (item.hasStatus(pID))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				num = num2;
				result = item;
				if (flag || Randy.randomBool())
				{
					break;
				}
			}
		}
		return result;
	}
}
