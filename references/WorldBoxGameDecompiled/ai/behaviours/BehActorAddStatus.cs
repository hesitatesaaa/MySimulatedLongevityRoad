namespace ai.behaviours;

public class BehActorAddStatus : BehaviourActionActor
{
	private string _status_id;

	private float _override_timer;

	private bool _effect_on;

	private bool _add_action_timer;

	public BehActorAddStatus(string pStatusID, float pOverrideTimer = -1f, bool pEffectOn = true, bool pAddActionTimer = false)
	{
		_status_id = pStatusID;
		_override_timer = pOverrideTimer;
		_effect_on = pEffectOn;
		_add_action_timer = pAddActionTimer;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.addStatusEffect(_status_id, _override_timer, _effect_on);
		if (_add_action_timer)
		{
			pActor.makeWait(_override_timer);
		}
		return BehResult.Continue;
	}
}
