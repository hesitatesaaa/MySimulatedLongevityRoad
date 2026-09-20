using UnityEngine;

namespace ai.behaviours;

public class BehConsumeActorTarget : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_actor_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		Actor a = pActor.beh_actor_target.a;
		if (Toolbox.DistTile(pActor.current_tile, a.current_tile) > 1f)
		{
			return BehResult.StepBack;
		}
		consume(pActor, a);
		if (a.hasHealth())
		{
			return BehResult.RepeatStep;
		}
		return BehResult.Continue;
	}

	private void consume(Actor pMain, Actor pTarget)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		pMain.timer_action = 0.3f;
		if (pMain.current_position.x > pTarget.current_position.x)
		{
			if (pTarget.target_angle.z > -45f)
			{
				pTarget.target_angle.z -= BehaviourActionBase<Actor>.world.elapsed * 100f;
				if (pTarget.target_angle.z < -90f)
				{
					pTarget.target_angle.z = -90f;
				}
				pTarget.rotation_cooldown = 1f;
			}
		}
		else if (pTarget.target_angle.z < 45f)
		{
			pTarget.target_angle.z += BehaviourActionBase<Actor>.world.elapsed * 100f;
			pTarget.rotation_cooldown = 1f;
		}
		if (pTarget.hasTrait("poisonous"))
		{
			pMain.addStatusEffect("poisoned");
		}
		if (pMain.target_angle.z == 0f)
		{
			pMain.punchTargetAnimation(Vector2.op_Implicit(pTarget.current_position), pFlip: false);
			int num = (int)((float)pTarget.getMaxHealth() * 0.15f) + 1;
			Clan clan = pTarget.clan;
			pTarget.getHit(num, pFlash: true, AttackType.Eaten, pMain, pSkipIfShake: false);
			pTarget.startShake(0.2f);
			if (pTarget.hasHealth())
			{
				pMain.addNutritionFromEating(num);
			}
			else
			{
				pMain.addNutritionFromEating(100, pSetMaxNutrition: true, pSetJustAte: true);
				pMain.countConsumed();
				AchievementLibrary.clannibals.checkBySignal((pMain, clan));
			}
		}
		pTarget.cancelAllBeh();
	}
}
