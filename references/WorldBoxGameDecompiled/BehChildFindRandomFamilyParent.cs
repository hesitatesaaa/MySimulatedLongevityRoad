using ai.behaviours;

public class BehChildFindRandomFamilyParent : BehaviourActionActor
{
	public override BehResult execute(Actor pBabyActor)
	{
		if (!pBabyActor.family.hasFounders())
		{
			return BehResult.Stop;
		}
		Actor randomFounder = pBabyActor.family.getRandomFounder();
		if (pBabyActor.inOwnCityBorders() && !randomFounder.inOwnCityBorders())
		{
			return BehResult.Stop;
		}
		pBabyActor.beh_actor_target = randomFounder;
		return BehResult.Continue;
	}
}
