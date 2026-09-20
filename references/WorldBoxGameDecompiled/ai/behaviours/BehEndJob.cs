namespace ai.behaviours;

public class BehEndJob : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.endJob();
		return BehResult.Continue;
	}
}
