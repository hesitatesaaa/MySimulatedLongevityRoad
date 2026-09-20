using ai.behaviours;

public class BehTryToSocialize : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.resetSocialize();
		Actor randomActorAround = getRandomActorAround(pActor);
		if (randomActorAround != null)
		{
			pActor.beh_actor_target = randomActorAround;
			if (pActor.canFallInLoveWith(randomActorAround))
			{
				pActor.becomeLoversWith(randomActorAround);
			}
			pActor.resetSocialize();
			randomActorAround.resetSocialize();
			if (pActor.hasTelepathicLink() && randomActorAround.hasTelepathicLink())
			{
				return forceTask(pActor, "socialize_do_talk", pClean: false);
			}
			return forceTask(pActor, "socialize_go_to_target", pClean: false);
		}
		return BehResult.Stop;
	}

	private Actor getRandomActorAround(Actor pActor)
	{
		using ListPool<Actor> listPool = new ListPool<Actor>(4);
		using ListPool<Actor> listPool2 = new ListPool<Actor>(4);
		bool flag = pActor.subspecies.needOppositeSexTypeForReproduction();
		bool flag2 = pActor.hasCulture() && pActor.culture.hasTrait("animal_whisperers");
		bool num = pActor.hasTelepathicLink();
		if (num)
		{
			fillUnitsViaTelepathicLink(pActor, listPool, listPool2);
		}
		int pChunkRadius = 1;
		if (num)
		{
			pChunkRadius = 2;
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, 0f, pRandom: true))
		{
			if (!pActor.canTalkWith(item))
			{
				continue;
			}
			if (pActor.isKingdomCiv())
			{
				if (item.isKingdomMob())
				{
					if (!flag2)
					{
						continue;
					}
				}
				else if (!item.isKingdomCiv())
				{
				}
			}
			else if (!pActor.isSameSpecies(item))
			{
				continue;
			}
			if (flag && pActor.canFallInLoveWith(item))
			{
				listPool.Add(item);
				break;
			}
			listPool2.Add(item);
			if (listPool2.Count > 3)
			{
				break;
			}
		}
		if (listPool.Count > 0)
		{
			return listPool.GetRandom();
		}
		if (listPool2.Count > 0)
		{
			return listPool2.GetRandom();
		}
		return null;
	}

	private void fillUnitsViaTelepathicLink(Actor pActor, ListPool<Actor> pBestTargets, ListPool<Actor> pNormalTargets)
	{
		if (pActor.hasFamily())
		{
			foreach (Actor unit in pActor.family.units)
			{
				if (pActor.canTalkWith(unit))
				{
					pNormalTargets.Add(unit);
				}
			}
		}
		foreach (Actor parent in pActor.getParents())
		{
			if (pActor.canTalkWith(parent))
			{
				pBestTargets.Add(parent);
			}
		}
	}
}
