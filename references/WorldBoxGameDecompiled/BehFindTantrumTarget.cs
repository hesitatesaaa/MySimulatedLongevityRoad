using ai.behaviours;

public class BehFindTantrumTarget : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.beh_actor_target != null && pActor.isTargetOkToAttack(pActor.beh_actor_target.a))
		{
			return BehResult.Continue;
		}
		Actor closestActor = getClosestActor(pActor);
		if (closestActor == null)
		{
			return forceTask(pActor, "random_move");
		}
		pActor.beh_actor_target = closestActor;
		return BehResult.Continue;
	}

	private Actor getClosestActor(Actor pActor)
	{
		bool pRandom = Randy.randomBool();
		WorldTile current_tile = pActor.current_tile;
		float num = 2.1474836E+09f;
		Actor result = null;
		foreach (Actor item in Finder.getUnitsFromChunk(current_tile, 1, 0f, pRandom))
		{
			float num2 = Toolbox.SquaredDistTile(item.current_tile, current_tile);
			if (!(num2 >= num) && pActor.isTargetOkToAttack(item) && (!item.hasStatusStunned() || pActor.areFoes(item)))
			{
				num = num2;
				result = item;
				if (Randy.randomBool())
				{
					break;
				}
			}
		}
		return result;
	}
}
