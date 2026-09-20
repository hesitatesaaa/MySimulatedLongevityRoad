namespace ai.behaviours;

public class BehTaxiSitInside : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.timer_action = 1f;
		return BehResult.RestartTask;
	}
}
