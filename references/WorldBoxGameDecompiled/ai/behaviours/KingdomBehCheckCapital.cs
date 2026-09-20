namespace ai.behaviours;

public class KingdomBehCheckCapital : BehaviourActionKingdom
{
	public override BehResult execute(Kingdom pKingdom)
	{
		if (!pKingdom.hasCities())
		{
			return BehResult.Continue;
		}
		if (pKingdom.hasCapital() && pKingdom.capital.isAlive())
		{
			return BehResult.Continue;
		}
		City city = null;
		foreach (City city2 in pKingdom.cities)
		{
			if (city == null || city2.buildings.Count > city.buildings.Count)
			{
				city = city2;
			}
		}
		pKingdom.setCapital(city);
		return BehResult.Continue;
	}
}
