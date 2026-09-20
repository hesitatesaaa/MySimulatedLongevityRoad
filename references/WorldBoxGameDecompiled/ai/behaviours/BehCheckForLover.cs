namespace ai.behaviours;

public class BehCheckForLover : BehCitizenActionCity
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasLover())
		{
			return BehResult.Stop;
		}
		Actor lover = pActor.lover;
		if (lover.isTask("sexual_reproduction_civ_go") && lover.beh_building_target == pActor.beh_building_target && lover.ai.action_index > 3)
		{
			pActor.stayInBuilding(pActor.beh_building_target);
			lover.stayInBuilding(lover.beh_building_target);
			lover.setTask("sexual_reproduction_civ_wait", pClean: false, pCleanJob: false, pForceAction: true);
			return forceTask(pActor, "sexual_reproduction_civ_action", pClean: false, pForceAction: true);
		}
		if (!lover.isTask("sexual_reproduction_civ_go"))
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
