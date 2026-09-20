namespace ai.behaviours;

public class BehGoOrSwimToTileTarget : BehGoToTileTarget
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
		walk_on_water = true;
	}
}
