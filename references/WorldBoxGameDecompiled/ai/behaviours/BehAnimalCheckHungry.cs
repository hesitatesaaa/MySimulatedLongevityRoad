namespace ai.behaviours;

public class BehAnimalCheckHungry : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.isHungry())
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
