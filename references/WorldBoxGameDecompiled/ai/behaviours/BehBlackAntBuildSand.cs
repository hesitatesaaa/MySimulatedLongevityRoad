namespace ai.behaviours;

public class BehBlackAntBuildSand : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("ant_steps", out var pResult, 0);
		pActor.data.get("direction", out var pResult2, 0);
		if (pResult > 0)
		{
			pResult--;
			if (!pActor.beh_tile_target.Type.IsType("mountains") && !pActor.beh_tile_target.Type.IsType("hills"))
			{
				Ant.antUseOnTile(pActor.beh_tile_target, "sand");
			}
			pResult2 = getRandomDirection();
		}
		pActor.data.set("ant_steps", pResult);
		pActor.data.set("direction", pResult2);
		if (pResult == 0)
		{
			pActor.setTask("ant_black_island");
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}

	private static int getRandomDirection()
	{
		ActorDirection random = Randy.getRandom(Toolbox.directions);
		return Toolbox.directions.IndexOf(random);
	}
}
