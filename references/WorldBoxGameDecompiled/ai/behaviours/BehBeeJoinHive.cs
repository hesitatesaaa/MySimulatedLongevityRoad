namespace ai.behaviours;

public class BehBeeJoinHive : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_building_target = true;
		check_building_target_non_usable = true;
	}

	public override BehResult execute(Actor pActor)
	{
		Building beh_building_target = pActor.beh_building_target;
		pActor.setHomeBuilding(beh_building_target);
		return BehResult.Continue;
	}
}
