namespace ai.behaviours;

public class BehBlackAntBuildIsland : BehaviourActionActor
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
		if (pActor.beh_tile_target.Type.liquid)
		{
			pResult = 20;
		}
		if (pResult > 0)
		{
			string pType;
			if (!pActor.beh_tile_target.Type.IsType("mountains"))
			{
				pType = "mountains";
				pResult2++;
				if (pResult2 > Toolbox.directions.Length - 1)
				{
					pResult2 = 0;
				}
			}
			else
			{
				pType = "hills";
				pResult2--;
				if (pResult2 < 0)
				{
					pResult2 = Toolbox.directions.Length - 1;
				}
			}
			Ant.antUseOnTile(pActor.beh_tile_target, pType);
			pResult--;
		}
		if (pResult == 0)
		{
			pActor.data.set("ant_steps", 40);
			pActor.data.set("direction", getRandomDirection());
			pActor.setTask("ant_black_sand");
			return BehResult.Stop;
		}
		pActor.data.set("ant_steps", pResult);
		pActor.data.set("direction", pResult2);
		return BehResult.Continue;
	}

	private static int getRandomDirection()
	{
		ActorDirection random = Randy.getRandom(Toolbox.directions);
		return Toolbox.directions.IndexOf(random);
	}
}
