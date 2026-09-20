namespace ai.behaviours;

public class BehDragonCantLand : BehaviourActionActor
{
	private string task_id;

	public BehDragonCantLand(string pNextAction)
	{
		task_id = pNextAction;
	}

	public override BehResult execute(Actor pActor)
	{
		if (!Dragon.canLand(pActor))
		{
			return forceTask(pActor, task_id);
		}
		return BehResult.Continue;
	}
}
