namespace ai.behaviours;

public class BehLookAtBuildingTarget : BehActorBuildingTarget
{
	private float _timer;

	public BehLookAtBuildingTarget(float pTimer = 0.3f)
	{
		_timer = pTimer;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		pActor.lookTowardsPosition(pActor.beh_building_target.current_position);
		pActor.timer_action = _timer;
		return BehResult.Continue;
	}
}
