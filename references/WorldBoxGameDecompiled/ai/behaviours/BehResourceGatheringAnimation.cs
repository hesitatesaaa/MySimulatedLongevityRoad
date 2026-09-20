namespace ai.behaviours;

public class BehResourceGatheringAnimation : BehaviourActionActor
{
	private float _timer_action;

	private string _sound_event_id;

	public BehResourceGatheringAnimation(float pTimerAction, string pSound = "", bool pLandIfHovering = true)
	{
		_sound_event_id = pSound;
		_timer_action = pTimerAction;
		land_if_hovering = pLandIfHovering;
	}

	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_building_target = true;
		check_building_target_non_usable = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (pActor.beh_building_target.asset.gatherable && !pActor.beh_building_target.hasResourcesToCollect())
		{
			return BehResult.Stop;
		}
		pActor.punchTargetAnimation(pActor.beh_building_target.current_tile.posV3);
		pActor.beh_building_target.resourceGathering(BehaviourActionBase<Actor>.world.elapsed);
		pActor.beh_building_target.startShake(0.01f);
		if (!string.IsNullOrEmpty(_sound_event_id))
		{
			MusicBox.playSound(_sound_event_id, pActor.beh_building_target.current_tile, pGameViewOnly: true, pVisibleOnly: true);
		}
		pActor.timer_action = _timer_action;
		return BehResult.Continue;
	}
}
