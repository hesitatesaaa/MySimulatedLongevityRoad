namespace ai.behaviours;

public class BehBlueAntSwitchGround : BehaviourActionActor
{
	private const string tileType1 = "sand";

	private const string tileType2 = "shallow_waters";

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("direction", out var pResult, 0);
		if (pActor.beh_tile_target.Type.liquid)
		{
			pResult++;
			if (pResult > Toolbox.directions.Length - 1)
			{
				pResult = 0;
			}
			Ant.antUseOnTile(pActor.beh_tile_target, "sand");
		}
		else
		{
			pResult--;
			if (pResult < 0)
			{
				pResult = Toolbox.directions.Length - 1;
			}
			Ant.antUseOnTile(pActor.beh_tile_target, "shallow_waters");
		}
		pActor.data.set("direction", pResult);
		return BehResult.Continue;
	}
}
