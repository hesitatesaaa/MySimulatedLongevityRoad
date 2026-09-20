namespace ai.behaviours;

public class BehWait : BehaviourActionActor
{
	private float _wait_interval;

	public BehWait(float pWaitInterval = 1f)
	{
		_wait_interval = pWaitInterval;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.timer_action = _wait_interval;
		return BehResult.Continue;
	}
}
