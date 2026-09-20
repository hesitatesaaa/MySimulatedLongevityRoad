namespace ai.behaviours;

public class BehJoinCity : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		City city = pActor.current_zone.city;
		if (city == null)
		{
			return BehResult.Stop;
		}
		if (!city.isPossibleToJoin(pActor))
		{
			return BehResult.Stop;
		}
		if (city.isNeutral())
		{
			if (pActor.kingdom.isCiv())
			{
				city.setKingdom(pActor.kingdom);
			}
			else
			{
				Kingdom kingdom = BehaviourActionBase<Actor>.world.kingdoms.makeNewCivKingdom(pActor);
				pActor.createDefaultCultureAndLanguageAndClan();
				city.setKingdom(kingdom);
				city.setUnitMetas(pActor);
				kingdom.setUnitMetas(pActor);
			}
		}
		if (city.kingdom != pActor.kingdom)
		{
			pActor.removeFromPreviousFaction();
		}
		pActor.joinCity(city);
		pActor.setMetasFromCity(city);
		return BehResult.Continue;
	}
}
