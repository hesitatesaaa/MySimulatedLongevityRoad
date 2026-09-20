namespace ai.behaviours;

public class BehCheckIfInLiquid : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isInLiquid())
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
