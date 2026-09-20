namespace ai.behaviours.conditions;

public class CondDragonCanLand : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		if (!Dragon.canLand(pActor))
		{
			return false;
		}
		if (pActor.getActorComponent<Dragon>().lastLanded == pActor.current_tile)
		{
			return false;
		}
		pActor.data.get("justUp", out var pResult, false);
		if (pResult)
		{
			pActor.data.removeBool("justUp");
			return false;
		}
		return true;
	}
}
