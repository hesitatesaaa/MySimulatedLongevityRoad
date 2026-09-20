namespace ai.behaviours;

public class BehCityActorFindNewJob : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.city.setCitizenJob(pActor);
		return BehResult.Continue;
	}
}
