namespace ai.behaviours;

public class BehStartShake : BehaviourActionActor
{
	private float _timer_shake;

	private float _wait_action;

	public BehStartShake(float pTimerShake = 1f, float pTimeWaitAction = 0f)
	{
		_timer_shake = pTimerShake;
		_wait_action = pTimeWaitAction;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.startShake(_timer_shake);
		pActor.timer_action = _wait_action;
		return BehResult.Continue;
	}
}
