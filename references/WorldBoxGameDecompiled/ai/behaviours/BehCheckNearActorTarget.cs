namespace ai.behaviours;

public class BehCheckNearActorTarget : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_actor_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Actor a = pActor.beh_actor_target.a;
		if (!pActor.canTalkWith(a))
		{
			return BehResult.Stop;
		}
		if (Toolbox.SquaredDistVec2Float(pActor.current_position, a.current_position) < 4f)
		{
			return BehResult.Continue;
		}
		return BehResult.RestartTask;
	}
}
