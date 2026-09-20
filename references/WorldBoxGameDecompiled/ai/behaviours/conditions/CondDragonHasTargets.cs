namespace ai.behaviours.conditions;

public class CondDragonHasTargets : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		return pActor.getActorComponent<Dragon>().aggroTargets.Count > 0;
	}
}
