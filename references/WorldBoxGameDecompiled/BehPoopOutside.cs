using ai.behaviours;

public class BehPoopOutside : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.poop(pApplyForce: true);
		return BehResult.Continue;
	}
}
