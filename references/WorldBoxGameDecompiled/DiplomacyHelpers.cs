public static class DiplomacyHelpers
{
	public static WarManager wars => World.world.wars;

	public static DiplomacyManager diplomacy => World.world.diplomacy;

	public static bool isWarNeeded(Kingdom pKingdom)
	{
		if (!pKingdom.hasCities())
		{
			return false;
		}
		if (!pKingdom.hasCapital())
		{
			return false;
		}
		if (pKingdom.data.timestamp_last_war != -1.0 && Date.getYearsSince(pKingdom.data.timestamp_last_war) <= SimGlobals.m.diplomacy_years_war_timeout)
		{
			return false;
		}
		if (wars.hasWars(pKingdom))
		{
			return false;
		}
		if (pKingdom.countTotalWarriors() <= SimGlobals.m.diplomacy_years_war_min_warriors)
		{
			return false;
		}
		float num = pKingdom.getPopulationPeople();
		float num2 = pKingdom.getPopulationTotalPossible();
		if (pKingdom.countCities() < 4 && num < num2 * 0.6f)
		{
			return false;
		}
		return true;
	}

	public static Kingdom getWarTarget(Kingdom pInitiatorKingdom)
	{
		Kingdom result = null;
		float num = float.MaxValue;
		int num2 = pInitiatorKingdom.countTotalWarriors();
		if (pInitiatorKingdom.hasAlliance())
		{
			num2 = pInitiatorKingdom.getAlliance().countWarriors();
		}
		using ListPool<Kingdom> listPool = wars.getNeutralKingdoms(pInitiatorKingdom);
		foreach (ref Kingdom item in listPool)
		{
			Kingdom current = item;
			if (!current.hasCities() || !current.hasCapital() || current.getAge() < SimGlobals.m.minimum_kingdom_age_for_attack)
			{
				continue;
			}
			int num3 = 0;
			num3 = ((!current.hasAlliance()) ? current.countTotalWarriors() : current.getAlliance().countWarriors());
			if (num2 >= num3 && pInitiatorKingdom.capital.reachableFrom(current.capital) && !((float)Date.getYearsSince(diplomacy.getRelation(pInitiatorKingdom, current).data.timestamp_last_war_ended) < (float)SimGlobals.m.minimum_years_between_wars) && !pInitiatorKingdom.isOpinionTowardsKingdomGood(current))
			{
				float num4 = Kingdom.distanceBetweenKingdom(pInitiatorKingdom, current);
				if (num4 < num)
				{
					num = num4;
					result = current;
				}
			}
		}
		return result;
	}

	public static Kingdom getAllianceTarget(Kingdom pKingdomStarter)
	{
		if (pKingdomStarter.isSupreme())
		{
			return null;
		}
		using ListPool<Kingdom> listPool = World.world.wars.getNeutralKingdoms(pKingdomStarter, pOnlyWithoutWars: true, pOnlyWithoutAlliances: true);
		if (listPool.Count == 0)
		{
			return null;
		}
		foreach (Kingdom item in listPool.LoopRandom())
		{
			if (item.hasKing() && !item.isSupreme() && !item.king.hasPlot() && pKingdomStarter.isOpinionTowardsKingdomGood(item) && item.getRenown() >= PlotsLibrary.alliance_create.min_renown_kingdom)
			{
				bool flag = false;
				if (pKingdomStarter.countCities() <= 2 && item.countCities() <= 2 && !pKingdomStarter.hasNearbyKingdoms() && !item.hasNearbyKingdoms())
				{
					flag = true;
				}
				if (!flag && areKingdomsClose(item, pKingdomStarter))
				{
					flag = true;
				}
				if (flag)
				{
					return item;
				}
			}
		}
		return null;
	}

	public static bool areKingdomsClose(Kingdom pMain, Kingdom pTarget)
	{
		foreach (City city in pMain.getCities())
		{
			foreach (City city2 in pTarget.getCities())
			{
				if (City.nearbyBorders(city, city2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool isThereActiveCityConquest(Kingdom pKingdom, Kingdom pTargetKingdom)
	{
		foreach (City city in pKingdom.getCities())
		{
			if (city.isGettingCapturedBy(pTargetKingdom))
			{
				return true;
			}
		}
		foreach (City city2 in pTargetKingdom.getCities())
		{
			if (city2.isGettingCapturedBy(pKingdom))
			{
				return true;
			}
		}
		return false;
	}

	public static bool isThereFightBetween(Kingdom pKingdom1, Kingdom pKingdom2)
	{
		if (isThereActiveCityFight(pKingdom1, pKingdom2))
		{
			return true;
		}
		if (isThereActiveCityFight(pKingdom2, pKingdom1))
		{
			return true;
		}
		return false;
	}

	private static bool isThereActiveCityFight(Kingdom pDefenderKingdom, Kingdom pAttackerKingdom)
	{
		foreach (City city in pDefenderKingdom.getCities())
		{
			if (!city.hasArmy())
			{
				continue;
			}
			Army army = city.army;
			if (army.hasCaptain())
			{
				Actor captain = army.getCaptain();
				if (captain.current_tile.hasCity() && captain.current_tile.zone_city.kingdom == pAttackerKingdom)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool areDefendersGettingCaptured(this War pWar)
	{
		foreach (Kingdom defender in pWar.getDefenders())
		{
			if (defender.isGettingCaptured())
			{
				return true;
			}
		}
		return false;
	}

	public static bool areAttackersGettingCaptured(this War pWar)
	{
		foreach (Kingdom attacker in pWar.getAttackers())
		{
			if (attacker.isGettingCaptured())
			{
				return true;
			}
		}
		return false;
	}

	public static bool areAttackersAttackingAnotherCity(this War pWar)
	{
		foreach (Kingdom attacker in pWar.getAttackers())
		{
			if (attacker.isAttackingAnotherCity())
			{
				return true;
			}
		}
		return false;
	}

	public static bool areDefendersAttackingAnotherCity(this War pWar)
	{
		foreach (Kingdom defender in pWar.getDefenders())
		{
			if (defender.isAttackingAnotherCity())
			{
				return true;
			}
		}
		return false;
	}

	public static bool isAttackingAnotherCity(this Kingdom pAttackerKingdom)
	{
		foreach (City city in pAttackerKingdom.getCities())
		{
			if (!city.hasArmy())
			{
				continue;
			}
			Army army = city.army;
			if (army.hasCaptain())
			{
				Actor captain = army.getCaptain();
				if (captain.current_tile.hasCity() && captain.current_tile.zone_city.kingdom.isEnemy(pAttackerKingdom))
				{
					return true;
				}
			}
		}
		return false;
	}
}
