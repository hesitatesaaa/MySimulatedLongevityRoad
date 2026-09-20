namespace ai.behaviours;

public class BehCheckStillUnderConstruction : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_city = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.beh_building_target.isUnderConstruction())
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
