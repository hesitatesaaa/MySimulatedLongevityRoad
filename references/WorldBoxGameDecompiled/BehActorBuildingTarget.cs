using ai.behaviours;

public class BehActorBuildingTarget : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_building_target = true;
	}
}
