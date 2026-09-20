using ai.behaviours;

public class BehChangeCityActorCulture : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasCulture())
		{
			pActor.city.setCulture(pActor.culture);
		}
		return BehResult.Continue;
	}
}
