namespace ai.behaviours;

public class CityBehCheckArmy : BehaviourActionCity
{
	public override BehResult execute(City pCity)
	{
		pCity.checkArmyExistence();
		if (pCity.hasArmy())
		{
			return BehResult.Continue;
		}
		if (!pCity.hasAnyWarriors())
		{
			return BehResult.Continue;
		}
		Actor randomWarrior = pCity.getRandomWarrior();
		if (randomWarrior == null)
		{
			return BehResult.Continue;
		}
		BehaviourActionBase<City>.world.armies.newArmy(randomWarrior, pCity);
		return BehResult.Continue;
	}
}
