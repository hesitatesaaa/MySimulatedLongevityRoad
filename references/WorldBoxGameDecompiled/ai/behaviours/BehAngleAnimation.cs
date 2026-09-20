namespace ai.behaviours;

public class BehAngleAnimation : BehaviourActionActor
{
	private AngleAnimationTarget _target;

	private float _timer_action;

	private float _angle;

	private string _sound_event_id;

	private bool _check_flip;

	public BehAngleAnimation(AngleAnimationTarget pTarget, string pSound = null, float pTimerAction = 0f, float pAngle = 40f, bool pCheckFlip = true, bool pLandIfHovering = false)
	{
		_sound_event_id = pSound;
		_angle = pAngle;
		_target = pTarget;
		_timer_action = pTimerAction;
		_check_flip = pCheckFlip;
		land_if_hovering = pLandIfHovering;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		switch (_target)
		{
		case AngleAnimationTarget.Tile:
			null_check_tile_target = true;
			break;
		case AngleAnimationTarget.Building:
			null_check_building_target = true;
			check_building_target_non_usable = true;
			break;
		case AngleAnimationTarget.Ruin:
			null_check_building_target = true;
			break;
		case AngleAnimationTarget.Actor:
			null_check_actor_target = true;
			break;
		}
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		WorldTile worldTile = pActor.current_tile;
		switch (_target)
		{
		case AngleAnimationTarget.Tile:
			worldTile = pActor.beh_tile_target;
			break;
		case AngleAnimationTarget.Building:
			worldTile = pActor.beh_building_target.current_tile;
			pActor.beh_building_target.startShake(0.3f);
			break;
		case AngleAnimationTarget.Actor:
			if (pActor.beh_actor_target.a.isInsideSomething())
			{
				return BehResult.Stop;
			}
			worldTile = pActor.beh_actor_target.current_tile;
			break;
		}
		pActor.punchTargetAnimation(worldTile.posV3, _check_flip, pReverse: false, _angle);
		if (!string.IsNullOrEmpty(_sound_event_id))
		{
			MusicBox.playSound(_sound_event_id, worldTile, pGameViewOnly: true);
		}
		pActor.timer_action = _timer_action;
		return BehResult.Continue;
	}
}
