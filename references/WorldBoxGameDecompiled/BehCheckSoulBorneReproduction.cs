using ai.behaviours;

public class BehCheckSoulBorneReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.hasStatus("soul_harvested"))
		{
			return BehResult.Stop;
		}
		if (pActor.hasStatus("pregnant"))
		{
			return BehResult.Stop;
		}
		if (BabyHelper.isMetaLimitsReached(pActor))
		{
			return BehResult.Stop;
		}
		pActor.finishStatusEffect("soul_harvested");
		BabyMaker.startSoulborneBirth(pActor);
		return BehResult.Continue;
	}
}
