namespace ai.behaviours;

public class BehConsumeTargetTile : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		WorldTile beh_tile_target = pActor.beh_tile_target;
		if (!beh_tile_target.Type.canBeEatenByGeophag())
		{
			return BehResult.Stop;
		}
		pActor.punchTargetAnimation(beh_tile_target.posV3, pFlip: false);
		pActor.consumeTopTile(beh_tile_target);
		MapAction.terraformMain(beh_tile_target, TileLibrary.pit_deep_ocean, TerraformLibrary.destroy_no_flash);
		return BehResult.Continue;
	}
}
