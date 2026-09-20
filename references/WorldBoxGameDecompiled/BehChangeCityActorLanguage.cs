using ai.behaviours;

public class BehChangeCityActorLanguage : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasLanguage())
		{
			pActor.city.setLanguage(pActor.language);
		}
		return BehResult.Continue;
	}
}
