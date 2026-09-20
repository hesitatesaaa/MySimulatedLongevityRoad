namespace ai.behaviours;

public class BehCheckCityActorWarriorLimit : BehCityActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.city;
		if (!pActor.inOwnCityBorders())
		{
			return BehResult.Stop;
		}
		city.checkIfWarriorStillOk(pActor);
		return BehResult.Continue;
	}
}
