namespace ai.behaviours;

public class BehCheckCityActorArmyGroup : BehCitizenActionCity
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.isKingdomCiv())
		{
			pActor.stopBeingWarrior();
			return BehResult.Stop;
		}
		if (pActor.city.hasArmy())
		{
			Army army = pActor.city.army;
			if (army.isGroupInCityAndHaveLeader())
			{
				pActor.setArmy(army);
			}
		}
		return BehResult.Continue;
	}
}
