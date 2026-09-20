using ai.behaviours;

public class BehFindLover : BehaviourActionActor
{
	public override bool shouldRetry(Actor pActor)
	{
		if (pActor.hasCity() && BehaviourActionBase<Actor>.world.cities.isLocked())
		{
			return true;
		}
		return base.shouldRetry(pActor);
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasLover())
		{
			return BehResult.Stop;
		}
		Actor actor = findLoverAround(pActor);
		if (actor == null)
		{
			actor = checkCityLovers(pActor);
		}
		if (actor != null)
		{
			pActor.becomeLoversWith(actor);
		}
		return BehResult.Continue;
	}

	private Actor findLoverAround(Actor pActor)
	{
		Actor result = null;
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, 1))
		{
			if (checkIfPossibleLover(pActor, item))
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private bool checkIfPossibleLover(Actor pActor, Actor pTarget)
	{
		if (pTarget == pActor)
		{
			return false;
		}
		if (!pTarget.hasSubspecies())
		{
			return false;
		}
		if (!pTarget.isAlive())
		{
			return false;
		}
		if (!pTarget.canFallInLoveWith(pActor))
		{
			return false;
		}
		return true;
	}

	private Actor checkCityLovers(Actor pActor)
	{
		if (!pActor.hasCity())
		{
			return null;
		}
		Actor result = null;
		foreach (Actor item in pActor.city.getUnits().LoopRandom())
		{
			if (checkIfPossibleLover(pActor, item) && item.inOwnCityBorders())
			{
				result = item;
				break;
			}
		}
		return result;
	}
}
