using ai.behaviours;

public class CityBehCheckLoyalty : BehaviourActionCity
{
	public override BehResult execute(City pCity)
	{
		check(pCity);
		return BehResult.Continue;
	}

	public static void check(City pCity)
	{
		pCity.getLoyalty(pForceRecalc: true);
	}
}
