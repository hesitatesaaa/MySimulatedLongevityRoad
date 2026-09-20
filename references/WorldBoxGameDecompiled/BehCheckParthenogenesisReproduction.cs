using ai.behaviours;

public class BehCheckParthenogenesisReproduction : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		switch (pActor.subspecies.getReproductionStrategy())
		{
		case ReproductiveStrategy.Egg:
		case ReproductiveStrategy.SpawnUnitImmediate:
			BabyMaker.makeBabyViaParthenogenesis(pActor);
			break;
		case ReproductiveStrategy.Pregnancy:
		{
			BabyHelper.babyMakingStart(pActor);
			float maturationTimeSeconds = pActor.getMaturationTimeSeconds();
			pActor.addStatusEffect("pregnant_parthenogenesis", maturationTimeSeconds);
			pActor.subspecies.counterReproduction();
			break;
		}
		}
		return BehResult.Continue;
	}
}
