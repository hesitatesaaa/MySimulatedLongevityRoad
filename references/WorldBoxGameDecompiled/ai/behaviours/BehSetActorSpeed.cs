namespace ai.behaviours;

public class BehSetActorSpeed : BehaviourActionActor
{
	private float speed;

	public BehSetActorSpeed(float pSpeed = 0f)
	{
		speed = pSpeed;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.stats["speed"] = speed;
		return BehResult.Continue;
	}
}
