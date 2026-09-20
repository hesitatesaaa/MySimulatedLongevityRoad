using UnityEngine;
using ai.behaviours;

public class BehStealFromTarget : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_actor_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Actor a = pActor.beh_actor_target.a;
		if (a == null || !a.isAlive() || a.isInsideSomething())
		{
			return BehResult.Stop;
		}
		if (pActor.distanceToActorTile(a) > 2f)
		{
			return BehResult.Stop;
		}
		bool flag = false;
		float pWaitTimerForThief = 0.5f;
		float pTargetStunnedTimer = 1f;
		bool pAddAggro = false;
		if (a.canSeeTileBasedOnDirection(pActor.current_tile))
		{
			if (Randy.randomChance(0.4f))
			{
				flag = true;
				pTargetStunnedTimer = 1f;
				pWaitTimerForThief = 0.9f;
				pAddAggro = true;
			}
		}
		else if (Randy.randomChance(0.7f))
		{
			flag = true;
			pTargetStunnedTimer = 5f;
			pWaitTimerForThief = 1f;
		}
		else
		{
			pActor.makeWait(1f);
		}
		if (flag)
		{
			pActor.spawnSlashTalk(a.current_position);
			pActor.punchTargetAnimation(Vector2.op_Implicit(a.current_position), pFlip: false, pReverse: false, -20f);
			pActor.stealActionFrom(a, pTargetStunnedTimer, pWaitTimerForThief, pAddAggro);
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}
}
