namespace ai.behaviours.conditions;

public class CondActorFlying : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		return pActor.isFlying();
	}
}
