namespace ai.behaviours;

public class BehCheckEndCityActorJob : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		CitizenJobAsset citizen_job = pActor.a.citizen_job;
		int num = pActor.city.jobs.countOccupied(citizen_job);
		int num2 = pActor.city.jobs.countCurrentJobs(citizen_job);
		if (num > num2)
		{
			pActor.endJob();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
