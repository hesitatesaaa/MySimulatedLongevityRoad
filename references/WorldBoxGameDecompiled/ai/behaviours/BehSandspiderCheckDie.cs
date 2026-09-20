namespace ai.behaviours;

public class BehSandspiderCheckDie : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("ant_steps", out var pResult, 0);
		if (pActor.beh_tile_target == null || pResult > 20)
		{
			pActor.dieSimpleNone();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
