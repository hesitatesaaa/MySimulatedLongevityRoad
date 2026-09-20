namespace ai.behaviours;

public class BehAttackActorHuntingTarget : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		BaseSimObject beh_actor_target = pActor.beh_actor_target;
		if (pActor.isInWaterAndCantAttack())
		{
			return BehResult.Stop;
		}
		if (beh_actor_target == null || !beh_actor_target.isAlive())
		{
			pActor.makeWait(0.5f);
			return BehResult.Continue;
		}
		if (pActor.isInAttackRange(beh_actor_target))
		{
			bool num = pActor.tryToAttack(beh_actor_target);
			if (num && pActor.hasRangeAttack())
			{
				pActor.makeWait(0.5f);
			}
			if (num && !beh_actor_target.isAlive())
			{
				return BehResult.Continue;
			}
			if (beh_actor_target.isAlive())
			{
				return BehResult.RepeatStep;
			}
			return BehResult.Continue;
		}
		return BehResult.StepBack;
	}
}
