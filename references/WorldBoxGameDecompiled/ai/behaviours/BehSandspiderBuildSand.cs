namespace ai.behaviours;

public class BehSandspiderBuildSand : BehGoToTileTarget
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!pActor.beh_tile_target.Type.IsType("sand"))
		{
			pActor.data.get("ant_steps", out var pResult, 0);
			Ant.antUseOnTile(pActor.beh_tile_target, "sand");
			pActor.data.set("ant_steps", ++pResult);
			pActor.data.removeBool("changed_direction");
		}
		else if (Randy.randomChance(0.1f))
		{
			pActor.data.get("ant_steps", out var pResult2, 0);
			pActor.data.set("ant_steps", ++pResult2);
			pActor.data.removeBool("changed_direction");
		}
		return BehResult.Continue;
	}
}
