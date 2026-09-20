namespace ai.behaviours.conditions;

public class CondDragonSleeping : BehaviourActorCondition
{
	public override bool check(Actor pActor)
	{
		string text = pActor.ai.task?.id;
		if (!(text == "dragon_sleep"))
		{
			return text == "dragon_wakeup";
		}
		return true;
	}
}
