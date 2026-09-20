namespace ai.behaviours;

public class BehFingerCheckCanDraw : BehFingerDrawAction
{
	protected override void setupErrorChecks()
	{
		check_has_target_tiles = true;
		check_current_tile_in_target_tiles = true;
		check_target_tile_in_target_tiles = false;
		base.setupErrorChecks();
	}
}
