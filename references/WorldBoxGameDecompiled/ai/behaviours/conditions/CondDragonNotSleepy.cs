namespace ai.behaviours.conditions;

public class CondDragonNotSleepy : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		pActor.data.get("sleepy", out var pResult, 0);
		return pResult < 10;
	}
}
