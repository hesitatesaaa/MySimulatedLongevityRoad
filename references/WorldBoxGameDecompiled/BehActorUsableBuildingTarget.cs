public class BehActorUsableBuildingTarget : BehActorBuildingTarget
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
	}
}
