using System.Collections.Generic;
using ai;

public static class SuccessionTool
{
	public static Actor findNextHeir(Kingdom pKingdom, Actor pExculdeActor = null)
	{
		return getKingFromRoyalClan(pKingdom, pExculdeActor);
	}

	public static Actor getKingFromRoyalClan(Kingdom pKingdom, Actor pExcludeActor = null)
	{
		if (!pKingdom.data.royal_clan_id.hasValue())
		{
			return null;
		}
		Clan clan = World.world.clans.get(pKingdom.data.royal_clan_id);
		if (clan == null)
		{
			return null;
		}
		List<Actor> units = clan.units;
		using ListPool<Actor> listPool = new ListPool<Actor>();
		for (int i = 0; i < units.Count; i++)
		{
			Actor actor = units[i];
			if (actor != pExcludeActor && clan.fitToRule(actor, pKingdom))
			{
				listPool.Add(actor);
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		if (pKingdom.hasCulture())
		{
			return ListSorters.getUnitSortedByAgeAndTraits(listPool, pKingdom.culture);
		}
		listPool.Sort(ListSorters.sortUnitByAgeOldFirst);
		return listPool[0];
	}

	public static Actor getKingFromLeaders(Kingdom pKingdom)
	{
		Actor actor = null;
		using ListPool<Actor> listPool = new ListPool<Actor>();
		foreach (City city in pKingdom.getCities())
		{
			if (city.hasLeader())
			{
				listPool.Add(city.leader);
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		int num = 0;
		foreach (ref Actor item in listPool)
		{
			Actor current2 = item;
			int num2 = ActorTool.attributeDice(current2);
			if (actor == null || num2 > num)
			{
				num = num2;
				actor = current2;
			}
		}
		if (actor == null)
		{
			return null;
		}
		return actor;
	}
}
