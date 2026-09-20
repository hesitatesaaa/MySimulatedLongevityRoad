namespace ai.behaviours;

public class BehDragonSleepy : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasTrait("zombie"))
		{
			return BehResult.Continue;
		}
		pActor.data.get("sleepy", out var pResult, 0);
		pActor.data.set("sleepy", ++pResult);
		return BehResult.Continue;
	}
}
