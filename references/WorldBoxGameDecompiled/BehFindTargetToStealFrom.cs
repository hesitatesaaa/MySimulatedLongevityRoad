using ai.behaviours;

public class BehFindTargetToStealFrom : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Actor closestActorWithMoneys = getClosestActorWithMoneys(pActor);
		if (closestActorWithMoneys == null)
		{
			return BehResult.Stop;
		}
		pActor.beh_actor_target = closestActorWithMoneys;
		return BehResult.Continue;
	}

	private Actor getClosestActorWithMoneys(Actor pActor)
	{
		using ListPool<Actor> listPool = new ListPool<Actor>(4);
		bool pRandom = Randy.randomBool();
		int pChunkRadius = Randy.randomInt(1, 4);
		int num = Randy.randomInt(1, 4);
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, pChunkRadius, 0f, pRandom))
		{
			if (item != pActor && pActor.isSameIslandAs(item) && item.hasAnyCash())
			{
				listPool.Add(item);
				if (listPool.Count >= num)
				{
					break;
				}
			}
		}
		return Toolbox.getClosestActor(listPool, pActor.current_tile);
	}
}
