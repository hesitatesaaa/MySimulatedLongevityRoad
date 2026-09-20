namespace ai.behaviours;

public class BehBuildTarget : BehActorUsableBuildingTarget
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
		null_check_city = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.beh_building_target.isUnderConstruction())
		{
			return BehResult.Stop;
		}
		if (pActor.beh_building_target.updateBuild(pActor.getConstructionSpeed()))
		{
			pActor.addLoot(SimGlobals.m.coins_for_building);
		}
		return BehResult.Continue;
	}
}
