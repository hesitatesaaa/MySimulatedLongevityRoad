namespace ai.behaviours;

public class BehCheckIfOnGround : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isInLiquid())
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
