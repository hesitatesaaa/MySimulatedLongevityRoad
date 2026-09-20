namespace ai.behaviours;

public class BehCheckSexualReproductionOutside : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.canBreed())
		{
			return BehResult.Stop;
		}
		if (!pActor.hasLover())
		{
			return BehResult.Stop;
		}
		Actor lover = pActor.lover;
		if (!lover.canBreed())
		{
			return BehResult.Stop;
		}
		if (!lover.canCurrentTaskBeCancelledByReproduction())
		{
			return BehResult.Stop;
		}
		if (tryStartBreeding(pActor, lover))
		{
			return BehResult.RepeatStep;
		}
		pActor.addAfterglowStatus();
		return BehResult.Stop;
	}

	internal bool tryStartBreeding(Actor pActor, Actor pLover)
	{
		pActor.setTask("sexual_reproduction_outside", pClean: true, pCleanJob: true);
		pActor.beh_actor_target = pLover;
		pLover.makeWait();
		return true;
	}
}
