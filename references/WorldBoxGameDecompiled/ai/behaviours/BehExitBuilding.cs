namespace ai.behaviours;

public class BehExitBuilding : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_building_target = true;
		check_building_target_non_usable = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.exitBuilding();
		pActor.beh_building_target.startShake(0.01f);
		return BehResult.Continue;
	}
}
