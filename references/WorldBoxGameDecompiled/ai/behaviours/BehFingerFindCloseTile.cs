namespace ai.behaviours;

public class BehFingerFindCloseTile : BehFinger
{
	public override BehResult execute(Actor pActor)
	{
		pActor.findCurrentTile(pCheckNeighbours: false);
		if (finger.target_tiles.Count == 0)
		{
			return BehResult.Stop;
		}
		pActor.beh_tile_target = finger.target_tiles.GetRandom();
		if (finger.target_tiles.Contains(pActor.current_tile))
		{
			while (pActor.beh_tile_target.region != pActor.current_tile.region)
			{
				pActor.beh_tile_target = finger.target_tiles.GetRandom();
			}
		}
		return BehResult.Continue;
	}
}
