namespace ai.behaviours;

public class BehFightCheckEnemyIsOk : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.has_attack_target)
		{
			return BehResult.Stop;
		}
		if (!pActor.isEnemyTargetAlive())
		{
			return BehResult.Stop;
		}
		if (!pActor.shouldContinueToAttackTarget())
		{
			pActor.clearAttackTarget();
			return BehResult.Stop;
		}
		if (!pActor.canAttackTarget(pActor.attack_target))
		{
			pActor.ignoreTarget(pActor.attack_target);
			pActor.clearAttackTarget();
			return BehResult.Stop;
		}
		if (!pActor.isInAttackRange(pActor.attack_target))
		{
			if (pActor.isWaterCreature())
			{
				if ((!pActor.attack_target.isInLiquid() && !pActor.asset.force_land_creature) || pActor.attack_target.isFlying())
				{
					pActor.ignoreTarget(pActor.attack_target);
					pActor.clearAttackTarget();
					return BehResult.Stop;
				}
			}
			else if ((pActor.attack_target.isInLiquid() && !pActor.isWaterCreature()) || pActor.attack_target.isFlying())
			{
				pActor.ignoreTarget(pActor.attack_target);
				pActor.clearAttackTarget();
				return BehResult.Stop;
			}
		}
		int x = pActor.chunk.x;
		int y = pActor.chunk.y;
		int x2 = pActor.attack_target.chunk.x;
		int y2 = pActor.attack_target.chunk.y;
		float num = 1f;
		if (Toolbox.Dist(x, y, x2, y2) >= (float)SimGlobals.m.unit_chunk_sight_range + num)
		{
			pActor.clearAttackTarget();
			return BehResult.Stop;
		}
		pActor.beh_actor_target = pActor.attack_target;
		return BehResult.Continue;
	}
}
