using ai.behaviours;

public class BehActorReverseFlip : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.setFlip(!pActor.flip);
		return BehResult.Continue;
	}
}
