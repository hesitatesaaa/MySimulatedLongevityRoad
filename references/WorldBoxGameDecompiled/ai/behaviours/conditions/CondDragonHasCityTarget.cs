namespace ai.behaviours.conditions;

public class CondDragonHasCityTarget : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		pActor.data.get("attacksForCity", out var pResult, 0);
		if (pResult == 0)
		{
			return false;
		}
		pActor.data.get("cityToAttack", out var pResult2, -1L);
		return pResult2.hasValue();
	}
}
