namespace ai.behaviours;

public class BehCheckSexualReproductionCiv : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isKingdomCiv())
		{
			if (!pActor.hasHouse())
			{
				return BehResult.Stop;
			}
			if (!pActor.hasLover())
			{
				return BehResult.Stop;
			}
		}
		Actor lover = pActor.lover;
		if (!lover.canCurrentTaskBeCancelledByReproduction())
		{
			return BehResult.Stop;
		}
		lover.setTask("sexual_reproduction_civ_go", pClean: true, pCleanJob: true);
		lover.timer_action = 0f;
		return forceTask(pActor, "sexual_reproduction_civ_go");
	}
}
