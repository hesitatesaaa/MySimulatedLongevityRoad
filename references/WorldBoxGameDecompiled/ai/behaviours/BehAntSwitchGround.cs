namespace ai.behaviours;

public class BehAntSwitchGround : BehaviourActionActor
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
		pActor.data.get("tile_type1", out var pResult3, null);
		pActor.data.get("tile_type2", out var pResult4, null);
		if (pActor.beh_tile_target.Type.IsType(pResult4))
		{
			Ant.antUseOnTile(pActor.beh_tile_target, pResult3);
			if (pResult++ > 3)
			{
				pResult2++;
				if (pResult2 > Toolbox.directions.Length - 1)
				{
					pResult2 = 0;
				}
				pResult = 0;
			}
		}
		else
		{
			Ant.antUseOnTile(pActor.beh_tile_target, pResult4);
			if (pResult++ > 3)
			{
				pResult2--;
				if (pResult2 < 0)
				{
					pResult2 = Toolbox.directions.Length - 1;
				}
				pResult = 0;
			}
		}
		pActor.data.set("ant_steps", pResult);
		pActor.data.set("direction", pResult2);
		return BehResult.Continue;
	}
}
