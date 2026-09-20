using UnityEngine;

public static class BabyHelper
{
	public static Actor debugTryToMakeUnit(Actor pActor)
	{
		WorldTile current_tile = pActor.current_tile;
		Actor actor = null;
		foreach (Actor item in Finder.getUnitsFromChunk(current_tile, 1, 10f))
		{
			if (item != pActor && item.subspecies == pActor.subspecies)
			{
				actor = item;
				break;
			}
		}
		if (actor == null)
		{
			return null;
		}
		return BabyMaker.makeBaby(pActor, actor);
	}

	public static void countBirth(Actor pBaby)
	{
		World.world.game_stats.data.creaturesBorn++;
		World.world.map_stats.creaturesBorn++;
		if (pBaby.hasCity())
		{
			pBaby.city.increaseBirths();
		}
		if (pBaby.hasClan())
		{
			pBaby.clan.increaseBirths();
		}
		if (pBaby.hasFamily())
		{
			pBaby.family.increaseBirths();
		}
		if (pBaby.hasSubspecies())
		{
			pBaby.subspecies.increaseBirths();
		}
		if (pBaby.isKingdomCiv())
		{
			pBaby.kingdom.increaseBirths();
		}
	}

	public static void applyParentsMeta(Actor pParent1, Actor pParent2, Actor pBaby)
	{
		Subspecies babySubspecies = getBabySubspecies(pParent1, pParent2);
		pBaby.setSubspecies(babySubspecies);
		Family family = pParent1.family;
		Clan clan = checkGreatClan(pParent1, pParent2);
		if (clan != null && !clan.isFull())
		{
			pBaby.setClan(clan);
		}
		if (babySubspecies.isSapient())
		{
			if (pParent1.hasCity())
			{
				pBaby.setCity(pParent1.city);
			}
			else if (pParent2 != null && pParent2.hasCity())
			{
				pBaby.setCity(pParent2.city);
			}
		}
		if (family != null)
		{
			pBaby.setFamily(family);
			pBaby.saveOriginFamily(family.data.id);
		}
		using ListPool<Culture> listPool = new ListPool<Culture>(2);
		using ListPool<Religion> listPool2 = new ListPool<Religion>(2);
		using ListPool<Language> listPool3 = new ListPool<Language>(2);
		using ListPool<int> listPool4 = new ListPool<int>(2);
		listPool4.Add(pParent1.data.phenotype_index);
		if (pParent1.hasCulture())
		{
			listPool.Add(pParent1.culture);
		}
		if (pParent1.hasReligion())
		{
			listPool2.Add(pParent1.religion);
		}
		if (pParent1.hasLanguage())
		{
			listPool3.Add(pParent1.language);
		}
		if (pParent2 != null)
		{
			if (pParent2.hasCulture())
			{
				listPool.Add(pParent2.culture);
			}
			if (pParent2.hasReligion())
			{
				listPool2.Add(pParent2.religion);
			}
			if (pParent2.hasLanguage())
			{
				listPool3.Add(pParent2.language);
			}
			if (pParent2.subspecies == pBaby.subspecies)
			{
				listPool4.Add(pParent2.data.phenotype_index);
			}
		}
		if (listPool.Count > 0 && babySubspecies.has_advanced_memory)
		{
			pBaby.setCulture(listPool.GetRandom());
		}
		if (listPool2.Count > 0 && babySubspecies.has_advanced_memory)
		{
			pBaby.setReligion(listPool2.GetRandom());
		}
		if (listPool3.Count > 0 && babySubspecies.has_advanced_communication)
		{
			pBaby.joinLanguage(listPool3.GetRandom());
		}
		if (pParent1 != null && pParent1.hasCultureTrait("ancestors_knowledge"))
		{
			string bestAtribute = getBestAtribute(pParent1);
			if (bestAtribute != null)
			{
				pBaby.data[bestAtribute] = (float)(int)pParent1.data[bestAtribute] * 0.5f + 1f;
			}
		}
		if (pParent2 != null && pParent2.hasCultureTrait("ancestors_knowledge"))
		{
			string bestAtribute2 = getBestAtribute(pParent2);
			if (bestAtribute2 != null)
			{
				pBaby.data[bestAtribute2] = (float)(int)pParent2.data[bestAtribute2] * 0.5f + 1f;
			}
		}
		pBaby.data.phenotype_index = listPool4.GetRandom();
		pBaby.data.phenotype_shade = Actor.getRandomPhenotypeShade();
		if (babySubspecies.hasTrait("parental_care"))
		{
			pBaby.addStatusEffect("invincible", 90f);
		}
	}

	private static string getBestAtribute(Actor pParent1)
	{
		string result = null;
		int num = 0;
		if (pParent1.data["intelligence"] > (float)num)
		{
			num = (int)pParent1.data["intelligence"];
			result = "intelligence";
		}
		if (pParent1.data["warfare"] > (float)num)
		{
			num = (int)pParent1.data["warfare"];
			result = "warfare";
		}
		if (pParent1.data["diplomacy"] > (float)num)
		{
			num = (int)pParent1.data["diplomacy"];
			result = "diplomacy";
		}
		if (pParent1.data["stewardship"] > (float)num)
		{
			num = (int)pParent1.data["stewardship"];
			result = "stewardship";
		}
		return result;
	}

	private static Clan checkGreatClan(Actor pParent1, Actor pParent2)
	{
		Clan clan = null;
		if (pParent1.isKing())
		{
			clan = pParent1.clan;
		}
		else if (pParent2 != null && pParent2.isKing())
		{
			clan = pParent2.clan;
		}
		if (clan == null)
		{
			if (pParent1.isCityLeader() && pParent2 != null && pParent2.isCityLeader())
			{
				clan = ((!Randy.randomBool()) ? pParent2.clan : pParent1.clan);
			}
			else if (pParent1 != null && pParent1.isCityLeader())
			{
				clan = pParent1.clan;
			}
			else if (pParent2 != null && pParent2.isCityLeader())
			{
				clan = pParent2.clan;
			}
		}
		return clan;
	}

	private static Subspecies getBabySubspecies(Actor pParent1, Actor pParent2)
	{
		Subspecies subspecies = pParent1.subspecies;
		Subspecies subspecies2 = pParent2?.subspecies ?? subspecies;
		if (subspecies.isSapient() && subspecies.isSapient() != subspecies2.isSapient())
		{
			if (subspecies.isSapient())
			{
				return subspecies;
			}
			return subspecies2;
		}
		if (subspecies != subspecies2 && subspecies.getGeneration() != subspecies2.getGeneration())
		{
			if (subspecies.getGeneration() > subspecies2.getGeneration())
			{
				return subspecies;
			}
			return subspecies2;
		}
		if (Randy.randomBool())
		{
			return subspecies;
		}
		return subspecies2;
	}

	public static bool canMakeBabies(Actor pActor)
	{
		if (!pActor.isAdult())
		{
			return false;
		}
		if (!pActor.canProduceBabies())
		{
			return false;
		}
		if (pActor.hasReachedOffspringLimit())
		{
			return false;
		}
		if (!pActor.haveNutritionForNewBaby())
		{
			return false;
		}
		return true;
	}

	public static bool isMetaLimitsReached(Actor pActor)
	{
		if (pActor.subspecies.hasReachedPopulationLimit())
		{
			return true;
		}
		if (pActor.hasCity())
		{
			if (pActor.city.hasReachedWorldLawLimit())
			{
				return true;
			}
			Actor lover = pActor.lover;
			bool num = pActor.isImportantPerson() && !pActor.hasReachedOffspringLimit();
			bool flag = lover != null && lover.isImportantPerson() && !lover.hasReachedOffspringLimit();
			if (num | flag)
			{
				return false;
			}
			if (pActor.subspecies.isReproductionSexual() && pActor.current_children_count == 0)
			{
				return false;
			}
			if (!pActor.city.hasFreeHouseSlots())
			{
				return true;
			}
		}
		return false;
	}

	public static void countMakeChild(Actor pParent1, Actor pParent2)
	{
		if (!pParent1.isRekt())
		{
			pParent1.increaseBirths();
		}
		if (!pParent2.isRekt())
		{
			pParent2.increaseBirths();
		}
	}

	public static void babyMakingStart(Actor pActor)
	{
		pActor.subspecies.all_actions_actor_birth?.Invoke(pActor, pActor.current_tile);
	}

	public static void traitsClone(Actor pActorTarget, Actor pParent1)
	{
		foreach (ActorTrait trait in pParent1.getTraits())
		{
			if (trait.rate_birth != 0 || trait.rate_inherit != 0)
			{
				pActorTarget.addTrait(trait);
			}
		}
	}

	public static void traitsInherit(Actor pActorTarget, Actor pParent1, Actor pParent2)
	{
		using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>(128);
		int pCounter = 0;
		int pCounter2 = 0;
		addTraitsFromParentToList(pParent1, listPool, out pCounter);
		if (pParent2 != null)
		{
			addTraitsFromParentToList(pParent2, listPool, out pCounter2);
		}
		if (listPool.Count != 0)
		{
			int num = (int)((float)(pCounter + pCounter2) * 0.25f);
			num = Mathf.Max(1, num);
			for (int i = 0; i < num; i++)
			{
				ActorTrait random = listPool.GetRandom();
				pActorTarget.addTrait(random.id);
			}
		}
	}

	private static void addTraitsFromParentToList(Actor pActor, ListPool<ActorTrait> pList, out int pCounter)
	{
		int num = 0;
		foreach (ActorTrait trait in pActor.getTraits())
		{
			if (trait.rate_inherit != 0 || trait.rate_birth != 0)
			{
				num++;
				pList.AddTimes(trait.rate_birth, trait);
				pList.AddTimes(trait.rate_inherit, trait);
			}
		}
		pCounter = num;
	}
}
