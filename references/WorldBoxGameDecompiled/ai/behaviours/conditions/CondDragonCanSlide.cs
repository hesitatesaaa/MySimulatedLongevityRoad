namespace ai.behaviours.conditions;

public class CondDragonCanSlide : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		if (!pActor.getActorComponent<Dragon>().hasTargetsForSlide())
		{
			return false;
		}
		pActor.data.get("justSlid", out var pResult, false);
		pActor.data.removeBool("justSlid");
		return !pResult;
	}
}
