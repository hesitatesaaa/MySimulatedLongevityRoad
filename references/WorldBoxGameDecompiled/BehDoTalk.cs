using UnityEngine;
using ai.behaviours;

public class BehDoTalk : BehaviourActionActor
{
	public BehDoTalk()
	{
		socialize = true;
	}

	public override BehResult execute(Actor pActor)
	{
		Actor actor = pActor.beh_actor_target?.a;
		if (actor == null)
		{
			return BehResult.Stop;
		}
		if (!stillCanTalk(actor))
		{
			return BehResult.Stop;
		}
		if ((!pActor.hasTelepathicLink() || !actor.hasTelepathicLink()) && (float)Toolbox.SquaredDistTile(actor.current_tile, pActor.current_tile) > 16f)
		{
			return BehResult.Stop;
		}
		pActor.data.get("socialize", out var pResult, 0);
		int num = Randy.randomInt(5, 10);
		if (pResult > num)
		{
			return BehResult.Continue;
		}
		continueTalk(pActor, actor);
		return BehResult.RepeatStep;
	}

	private bool stillCanTalk(Actor pTarget)
	{
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (pTarget.isLying())
		{
			return false;
		}
		return true;
	}

	private void continueTalk(Actor pActor, Actor pTarget)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		pActor.data.get("socialize", out var pResult, 0);
		pActor.data.set("socialize", ++pResult);
		bool flag = false;
		if (Randy.randomChance(0.4f))
		{
			pActor.clearLastTopicSprite();
			flag = true;
		}
		else if (Randy.randomChance(0.4f))
		{
			pTarget.clearLastTopicSprite();
			flag = true;
		}
		if (!flag && (Object)(object)pTarget.getTopicSpriteTrait() != (Object)null && Randy.randomChance(0.45f))
		{
			pActor.cloneTopicSprite(pTarget.getSocializeTopic());
		}
		pActor.lookTowardsPosition(pTarget.current_position);
		pTarget.lookTowardsPosition(pActor.current_position);
		pTarget.setTask("socialize_receiving", pClean: true, pCleanJob: false, pForceAction: true);
		float num = 10f;
		if (Randy.randomBool())
		{
			pActor.playIdleSound();
		}
		else
		{
			pTarget.playIdleSound();
		}
		pActor.setTargetAngleZ(Randy.randomFloat(0f - num, num));
		pTarget.setTargetAngleZ(Randy.randomFloat(0f - num, num));
		pTarget.timer_action = (pActor.timer_action = Randy.randomFloat(1.1f, 3.3f));
		if (pActor.timestamp_tween_session_social == 0.0)
		{
			pActor.timestamp_tween_session_social = BehaviourActionBase<Actor>.world.getCurSessionTime();
			pTarget.timestamp_tween_session_social = BehaviourActionBase<Actor>.world.getCurSessionTime();
		}
	}
}
