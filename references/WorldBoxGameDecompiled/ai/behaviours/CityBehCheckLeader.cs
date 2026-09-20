namespace ai.behaviours;

public class CityBehCheckLeader : BehaviourActionCity
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		uses_clans = true;
	}

	public override BehResult execute(City pCity)
	{
		checkLeaderClan(pCity);
		checkFindLeader(pCity);
		return BehResult.Continue;
	}

	private void checkLeaderClan(City pCity)
	{
		if (pCity.hasLeader())
		{
			Actor leader = pCity.leader;
			if (!leader.hasClan())
			{
				BehaviourActionBase<City>.world.clans.newClan(leader, pAddDefaultTraits: true);
			}
		}
	}

	private void checkFindLeader(City pCity)
	{
		if (pCity.hasLeader() || pCity.isGettingCaptured())
		{
			return;
		}
		Actor actor = null;
		actor = tryGetClanLeader(pCity);
		if (actor != null)
		{
			if (actor.city != pCity)
			{
				actor.stopBeingWarrior();
			}
			actor.joinCity(pCity);
			pCity.setLeader(actor, pNew: true);
			return;
		}
		int num = 0;
		foreach (Actor unit in pCity.units)
		{
			if (unit.isKing() || unit.isCityLeader())
			{
				continue;
			}
			int num2 = 1;
			if (unit.is_profession_citizen)
			{
				if (unit.isFavorite())
				{
					num2 += 2;
				}
				int num3 = ActorTool.attributeDice(unit, num2);
				if (actor == null || num3 > num)
				{
					actor = unit;
					num = num3;
				}
			}
		}
		if (actor != null)
		{
			pCity.setLeader(actor, pNew: true);
		}
	}

	private Actor tryGetClanLeader(City pCity)
	{
		Kingdom kingdom = pCity.kingdom;
		Clan clan = null;
		if (kingdom.data.royal_clan_id.hasValue())
		{
			clan = BehaviourActionBase<City>.world.clans.get(kingdom.data.royal_clan_id);
		}
		using ListPool<Actor> listPool = new ListPool<Actor>();
		using ListPool<Actor> listPool2 = new ListPool<Actor>();
		foreach (City city in kingdom.getCities())
		{
			foreach (Actor unit in city.getUnits())
			{
				if (unit.isUnitFitToRule() && !unit.isKing() && !unit.isCityLeader() && unit.hasClan())
				{
					if (clan != null && unit.clan == clan)
					{
						listPool.Add(unit);
					}
					else
					{
						listPool2.Add(unit);
					}
				}
			}
		}
		Actor result = null;
		if (listPool.Count > 0)
		{
			if (pCity.hasCulture())
			{
				return ListSorters.getUnitSortedByAgeAndTraits(listPool, pCity.culture);
			}
			listPool.Sort(ListSorters.sortUnitByAgeOldFirst);
			return listPool[0];
		}
		if (listPool2.Count > 0)
		{
			if (pCity.hasCulture())
			{
				return ListSorters.getUnitSortedByAgeAndTraits(listPool2, pCity.culture);
			}
			listPool2.Sort(ListSorters.sortUnitByAgeOldFirst);
			return listPool2[0];
		}
		return result;
	}
}
