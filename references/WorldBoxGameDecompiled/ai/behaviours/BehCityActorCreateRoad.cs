namespace ai.behaviours;

public class BehCityActorCreateRoad : BehCityActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		MapAction.createRoadTile(pActor.beh_tile_target);
		pActor.addLoot(SimGlobals.m.coins_for_road);
		return BehResult.Continue;
	}
}
