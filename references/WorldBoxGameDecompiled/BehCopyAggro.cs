using ai.behaviours;

public class BehCopyAggro : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Actor actor = pActor.beh_actor_target?.a;
		if (actor == null)
		{
			return BehResult.Continue;
		}
		pActor.copyAggroFrom(actor);
		copyEnemiesOf(pActor, actor);
		return BehResult.Continue;
	}

	private void copyEnemiesOf(Actor pCopyTo, Actor pTarget)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTarget.current_tile, 1, 0f, pRandom: true))
		{
			if (item != pCopyTo && item.isInAggroList(pTarget) && pCopyTo.isSameIslandAs(item))
			{
				pCopyTo.addAggro(item);
			}
		}
	}
}
