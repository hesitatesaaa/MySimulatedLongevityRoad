namespace ai.behaviours;

public class BehStayInBuildingTarget : BehCityActor
{
	private float min;

	private float max;

	public BehStayInBuildingTarget(float pMin = 0f, float pMax = 1f)
	{
		min = pMin;
		max = pMax;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.timer_action = Randy.randomFloat(min, max);
		pActor.stayInBuilding(pActor.beh_building_target);
		pActor.beh_tile_target = null;
		pActor.beh_building_target.startShake(0.5f);
		return BehResult.Continue;
	}
}
