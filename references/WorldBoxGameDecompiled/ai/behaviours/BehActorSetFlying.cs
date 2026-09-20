namespace ai.behaviours;

public class BehActorSetFlying : BehaviourActionActor
{
	private bool _flying;

	public BehActorSetFlying(bool pFlying)
	{
		_flying = pFlying;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.setFlying(_flying);
		return BehResult.Continue;
	}
}
