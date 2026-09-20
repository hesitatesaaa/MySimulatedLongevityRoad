namespace ai.behaviours;

public class BehJumpingAnimation : BehaviourActionActor
{
	private float _timer_action;

	private float _timer_jumping;

	public BehJumpingAnimation(float pTimerAction, float pTimerJumpAnimation)
	{
		_timer_action = pTimerAction;
		_timer_jumping = pTimerJumpAnimation;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.timer_jump_animation = _timer_jumping;
		pActor.timer_action = _timer_action;
		return BehResult.Continue;
	}
}
