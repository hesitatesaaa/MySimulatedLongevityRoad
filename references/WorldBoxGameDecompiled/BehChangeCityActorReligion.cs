using ai.behaviours;

public class BehChangeCityActorReligion : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.hasReligion())
		{
			pActor.city.setReligion(pActor.religion);
		}
		return BehResult.Continue;
	}
}
