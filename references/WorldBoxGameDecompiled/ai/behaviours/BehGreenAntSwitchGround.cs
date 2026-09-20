namespace ai.behaviours;

public class BehGreenAntSwitchGround : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("direction", out var pResult, 0);
		string pType;
		if (pActor.beh_tile_target.Type.liquid)
		{
			pType = "sand";
			pResult--;
		}
		else if (pActor.beh_tile_target.Type.IsType("sand"))
		{
			pType = "soil_low";
			pResult++;
		}
		else if (pActor.beh_tile_target.Type.IsType("soil_low"))
		{
			pType = "soil_high";
			pResult--;
		}
		else if (pActor.beh_tile_target.Type.IsType("soil_high"))
		{
			pType = "soil_low";
			pResult++;
		}
		else
		{
			pType = "sand";
			pResult--;
		}
		if (pResult > Toolbox.directions.Length - 1)
		{
			pResult = 0;
		}
		if (pResult < 0)
		{
			pResult = Toolbox.directions.Length - 1;
		}
		Ant.antUseOnTile(pActor.beh_tile_target, pType);
		pActor.data.set("direction", pResult);
		return BehResult.Continue;
	}
}
