namespace ai.behaviours;

public class BehCheckSameCityActorLover : BehCitizenActionCity
{
	public override bool errorsFound(Actor pObject)
	{
		if (base.errorsFound(pObject))
		{
			return true;
		}
		if (!pObject.hasLover())
		{
			return true;
		}
		if (pObject.lover.isRekt())
		{
			return true;
		}
		if (pObject.lover.city.isRekt())
		{
			return true;
		}
		return false;
	}

	public override BehResult execute(Actor pActor)
	{
		City city = pActor.lover.city;
		pActor.clearHomeBuilding();
		pActor.stopBeingWarrior();
		pActor.joinCity(city);
		return BehResult.Continue;
	}
}
