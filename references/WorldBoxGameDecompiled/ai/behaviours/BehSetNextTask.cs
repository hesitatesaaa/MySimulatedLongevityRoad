namespace ai.behaviours;

public class BehSetNextTask : BehaviourActionActor
{
	private bool _clean;

	private bool _force;

	private string _task_id;

	public BehSetNextTask(string taskID, bool pClean = true, bool pForce = false)
	{
		_task_id = taskID;
		_clean = pClean;
		_force = pForce;
	}

	public override BehResult execute(Actor pActor)
	{
		return forceTask(pActor, _task_id, _clean, _force);
	}
}
