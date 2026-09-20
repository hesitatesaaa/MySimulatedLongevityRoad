public static class DiplomacyHelpersRebellion
{
	public static void startRebellion(Actor pActor, Plot pPlot, bool pCheckForHappiness)
	{
		City city = pActor.city;
		Kingdom kingdom = city.kingdom;
		if (pActor.isCityLeader())
		{
			pActor.city.removeLeader();
		}
		Kingdom kingdom2 = city.makeOwnKingdom(pActor, pRebellion: true);
		using ListPool<City> listPool = new ListPool<City>();
		listPool.Add(city);
		pActor.joinCity(city);
		War war = null;
		foreach (War war2 in kingdom.getWars())
		{
			if (war2.isMainAttacker(kingdom) && war2.getAsset() == WarTypeLibrary.rebellion)
			{
				war = war2;
				war.joinDefenders(kingdom2);
				break;
			}
		}
		if (war == null)
		{
			war = World.world.diplomacy.startWar(kingdom, kingdom2, WarTypeLibrary.rebellion);
			if (kingdom.hasAlliance())
			{
				foreach (Kingdom item in kingdom.getAlliance().kingdoms_hashset)
				{
					if (item != kingdom && item.isOpinionTowardsKingdomGood(kingdom))
					{
						war.joinAttackers(item);
					}
				}
			}
		}
		foreach (Actor unit in pPlot.units)
		{
			City city2 = unit.city;
			if (city2 != null && city2.kingdom != kingdom2 && city2.kingdom == kingdom)
			{
				city2.joinAnotherKingdom(kingdom2, pCaptured: false, pRebellion: true);
			}
		}
		int num = kingdom.countCities();
		int maxCities = kingdom2.getMaxCities();
		maxCities -= listPool.Count;
		if (maxCities < 0)
		{
			maxCities = 0;
		}
		if (maxCities > num / 3)
		{
			maxCities = (int)((float)num / 3f);
		}
		for (int i = 0; i < maxCities; i++)
		{
			if (!checkMoreAlignedCities(kingdom2, kingdom, listPool, pCheckForHappiness))
			{
				break;
			}
		}
	}

	public static bool checkMoreAlignedCities(Kingdom pNewKingdom, Kingdom pOldKingdom, ListPool<City> pNewCities, bool pCheckForHappiness)
	{
		using ListPool<City> listPool = new ListPool<City>(World.world.cities.Count);
		addNeighbourCities(listPool, pNewCities);
		if (listPool.Count == 0)
		{
			listPool.AddRange(pOldKingdom.getCities());
		}
		if (listPool.Count == 0)
		{
			return false;
		}
		foreach (City item in listPool.LoopRandom())
		{
			if (item.kingdom == pOldKingdom && !item.isCapitalCity() && (!pCheckForHappiness || !item.isHappy()) && !Randy.randomBool())
			{
				item.joinAnotherKingdom(pNewKingdom, pCaptured: false, pRebellion: true);
				return true;
			}
		}
		return true;
	}

	private static void addNeighbourCities(ListPool<City> pTempCitiesToCheck, ListPool<City> pRebelledCities)
	{
		foreach (ref City pRebelledCity in pRebelledCities)
		{
			pRebelledCity.recalculateNeighbourCities();
		}
		foreach (ref City pRebelledCity2 in pRebelledCities)
		{
			City current = pRebelledCity2;
			pTempCitiesToCheck.AddRange(current.neighbours_cities);
		}
	}
}
