namespace ai.behaviours;

public class BehGoToTileTarget : BehaviourActionActor
{
	public bool walk_on_water;

	public bool walk_on_blocks;

	public int limit_pathfinding_regions;

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
		walk_on_water = false;
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.goTo(pActor.beh_tile_target, walk_on_water, walk_on_blocks, pWalkOnLava: false, limit_pathfinding_regions) == ExecuteEvent.False)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
