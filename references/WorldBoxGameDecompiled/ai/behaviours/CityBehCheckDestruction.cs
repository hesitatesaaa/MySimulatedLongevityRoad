namespace ai.behaviours;

public class CityBehCheckDestruction : BehaviourActionCity
{
	public override BehResult execute(City pCity)
	{
		if (!pCity.isGettingCaptured())
		{
			return BehResult.Continue;
		}
		Kingdom capturingKingdom = pCity.getCapturingKingdom();
		if (capturingKingdom.isRekt())
		{
			return BehResult.Continue;
		}
		Actor king = capturingKingdom.king;
		if (king == null || !king.hasXenophobic())
		{
			return BehResult.Continue;
		}
		if (capturingKingdom.getSpecies() == pCity.kingdom.getSpecies())
		{
			return BehResult.Continue;
		}
		foreach (Actor unit in pCity.getUnits())
		{
			if (unit.current_zone.city == pCity)
			{
				return BehResult.Continue;
			}
		}
		pCity.kingdom.decreaseHappinessFromRazedCity(pCity);
		capturingKingdom.increaseHappinessFromDestroyingCity();
		foreach (Actor unit2 in pCity.getUnits())
		{
			unit2.stopBeingWarrior();
			unit2.joinCity(null);
		}
		if (pCity.hasLeader())
		{
			pCity.removeLeader();
		}
		return BehResult.Continue;
	}
}
