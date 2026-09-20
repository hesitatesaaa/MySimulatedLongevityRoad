namespace ai.behaviours;

public class BehRestartTask : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		return BehResult.RestartTask;
	}
}
