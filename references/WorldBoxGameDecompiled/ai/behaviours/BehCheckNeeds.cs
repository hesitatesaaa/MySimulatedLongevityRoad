namespace ai.behaviours;

public class BehCheckNeeds : BehCityActor
{
	private int _max_restarts;

	public BehCheckNeeds(int pRestarts)
	{
		_max_restarts = pRestarts;
	}

	public override BehResult execute(Actor pActor)
	{
		if (_max_restarts > 0 && pActor.ai.restarts >= _max_restarts)
		{
			return BehResult.Stop;
		}
		if (pActor.isStarving() && pActor.city.hasAnyFood())
		{
			pActor.endJob();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
