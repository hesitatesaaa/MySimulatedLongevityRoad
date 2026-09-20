using System;

namespace ai.behaviours;

public class BehRepairInDock : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		check_building_target_non_usable = true;
		null_check_building_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasMaxHealth())
		{
			return BehResult.Continue;
		}
		int num = pActor.getMaxHealth() - pActor.getHealth();
		num = ((num > 100) ? 100 : num);
		pActor.restoreHealth(num);
		float num2 = num / 25;
		pActor.timer_action = (float)Math.Ceiling(num2);
		pActor.stayInBuilding(pActor.beh_building_target);
		pActor.beh_tile_target = null;
		pActor.beh_building_target.startShake(0.5f);
		if (!pActor.hasMaxHealth())
		{
			return BehResult.RepeatStep;
		}
		return BehResult.Continue;
	}
}
