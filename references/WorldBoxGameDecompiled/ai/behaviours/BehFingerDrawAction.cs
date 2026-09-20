namespace ai.behaviours;

public class BehFingerDrawAction : BehFinger
{
	public bool check_has_target_tiles = true;

	public bool check_current_tile_in_target_tiles = true;

	public bool check_target_tile_in_target_tiles = true;

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		if (check_target_tile_in_target_tiles)
		{
			null_check_tile_target = true;
		}
	}

	public override bool errorsFound(Actor pActor)
	{
		if (base.errorsFound(pActor))
		{
			return true;
		}
		finger = pActor.children_special[0] as GodFinger;
		if (check_has_target_tiles && finger.target_tiles.Count == 0)
		{
			return true;
		}
		if (check_current_tile_in_target_tiles)
		{
			pActor.findCurrentTile(pCheckNeighbours: false);
			if (!finger.target_tiles.Contains(pActor.current_tile))
			{
				bool flag = false;
				if (pActor.beh_tile_target != null && Toolbox.DistTile(pActor.current_tile, pActor.beh_tile_target) < 6f)
				{
					flag = true;
				}
				else
				{
					WorldTile[] neighboursAll = pActor.current_tile.neighboursAll;
					foreach (WorldTile item in neighboursAll)
					{
						if (finger.target_tiles.Contains(item))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return true;
				}
			}
		}
		if (check_target_tile_in_target_tiles && !finger.target_tiles.Contains(pActor.beh_tile_target))
		{
			return true;
		}
		return false;
	}
}
