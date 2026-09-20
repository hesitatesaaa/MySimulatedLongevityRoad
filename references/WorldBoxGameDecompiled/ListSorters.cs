public class ListSorters
{
	public static int sortUnitByAgeOldFirst(Actor pActor1, Actor pActor2)
	{
		return -pActor2.data.created_time.CompareTo(pActor1.data.created_time);
	}

	public static int sortUnitByAgeYoungFirst(Actor pActor1, Actor pActor2)
	{
		return pActor2.data.created_time.CompareTo(pActor1.data.created_time);
	}

	public static int sortUnitByKills(Actor pActor1, Actor pActor2)
	{
		return -pActor1.data.kills.CompareTo(pActor2.data.kills);
	}

	public static int sortUnitByRenown(Actor pActor1, Actor pActor2)
	{
		return -pActor1.data.renown.CompareTo(pActor2.data.renown);
	}

	public static int sortUnitByGoldCoins(Actor pActor1, Actor pActor2)
	{
		return -pActor1.data.money.CompareTo(pActor2.data.money);
	}

	public static int sortUnitByGender(Actor pActor1, Actor pActor2, ActorSex pTopGender)
	{
		if (pActor1.data.sex == pActor2.data.sex)
		{
			return 0;
		}
		if (pActor1.data.sex == pTopGender)
		{
			return -1;
		}
		return 1;
	}

	public static int sortUnitByStats(Actor pActor1, Actor pActor2, string pStatId)
	{
		float num = pActor1.stats.get(pStatId);
		float value = pActor2.stats.get(pStatId);
		return -num.CompareTo(value);
	}

	public static Actor getUnitSortedByAgeAndTraits(ListPool<Actor> pUnits, Culture pCulture)
	{
		sortUnitsSortedByAgeAndTraits(pUnits, pCulture);
		return pUnits[0];
	}

	public static void sortUnitsSortedByAgeAndTraits(ListPool<Actor> pUnits, Culture pCulture)
	{
		if (pCulture == null)
		{
			pUnits.Sort(sortUnitByAgeOldFirst);
			return;
		}
		if (pCulture.hasTrait("ultimogeniture"))
		{
			pUnits.Sort(sortUnitByAgeYoungFirst);
		}
		else
		{
			pUnits.Sort(sortUnitByAgeOldFirst);
		}
		bool num = pCulture.hasTrait("diplomatic_ascension");
		bool flag = pCulture.hasTrait("warriors_ascension");
		bool flag2 = pCulture.hasTrait("golden_rule");
		bool flag3 = pCulture.hasTrait("fames_crown");
		if (num)
		{
			pUnits.Sort((Actor a1, Actor a2) => sortUnitByStats(a1, a2, "diplomacy"));
		}
		else if (flag)
		{
			pUnits.Sort((Actor a1, Actor a2) => sortUnitByStats(a1, a2, "warfare"));
		}
		else if (flag3)
		{
			pUnits.Sort((Actor a1, Actor a2) => sortUnitByRenown(a1, a2));
		}
		else if (flag2)
		{
			pUnits.Sort((Actor a1, Actor a2) => sortUnitByGoldCoins(a1, a2));
		}
		bool flag4 = pCulture.hasTrait("patriarchy");
		bool flag5 = pCulture.hasTrait("matriarchy");
		if (flag4 | flag5)
		{
			ActorSex tSex = ((!flag4) ? ActorSex.Female : ActorSex.Male);
			pUnits.Sort((Actor a1, Actor a2) => sortUnitByGender(a1, a2, tSex));
		}
	}
}
