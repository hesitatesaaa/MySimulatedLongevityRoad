namespace ai.behaviours;

public class BehActorSetFlip : BehaviourActionActor
{
	private bool flip;

	public BehActorSetFlip(bool pFlip)
	{
		flip = pFlip;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.setFlip(flip);
		return BehResult.Continue;
	}
}
