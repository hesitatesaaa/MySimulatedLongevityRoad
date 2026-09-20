using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TraitTools
{
	public static bool hasOppositeTraits<TTrait>(this TTrait pTraitToCheck) where TTrait : BaseTrait<TTrait>
	{
		if (pTraitToCheck.opposite_traits != null)
		{
			return pTraitToCheck.opposite_traits.Count > 0;
		}
		return false;
	}

	public static bool hasOppositeTrait<TTrait>(this TTrait pTraitToCheck, HashSet<TTrait> pTraits) where TTrait : BaseTrait<TTrait>
	{
		if (!pTraitToCheck.hasOppositeTraits())
		{
			return false;
		}
		if (pTraits.Overlaps(pTraitToCheck.opposite_traits))
		{
			return true;
		}
		return false;
	}

	public static bool hasOppositeTrait(this ActorTrait pTraitToCheck, HashSet<ActorTrait> pTraits)
	{
		if (!pTraitToCheck.hasOppositeTraits())
		{
			return false;
		}
		if (pTraits.Overlaps(pTraitToCheck.opposite_traits))
		{
			return true;
		}
		return false;
	}

	public static bool hasOppositeTrait(string pTraitID, HashSet<ActorTrait> pTraits)
	{
		return AssetManager.traits.get(pTraitID).hasOppositeTrait(pTraits);
	}

	public static void loadTraits(Actor pActor, List<string> pListTraitIDs)
	{
		if (pListTraitIDs == null || pListTraitIDs.Count == 0)
		{
			return;
		}
		pActor.clearTraitCache();
		pActor.traits.Clear();
		foreach (string pListTraitID in pListTraitIDs)
		{
			ActorTrait actorTrait = AssetManager.traits.get(pListTraitID);
			if (actorTrait != null)
			{
				pActor.traits.Add(actorTrait);
			}
		}
	}

	public static ActorTrait getNewRandomTrait(string pGroup, HashSet<ActorTrait> pTraits)
	{
		using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>();
		foreach (ActorTrait item in AssetManager.traits.list)
		{
			int rate = item.getRate(pGroup);
			if (rate != 0 && !(item.group_id != pGroup) && !pTraits.Contains(item) && !item.hasOppositeTrait(pTraits))
			{
				for (int i = 0; i < rate; i++)
				{
					listPool.Add(item);
				}
			}
		}
		if (listPool.Count == 0)
		{
			Debug.LogError((object)"No Trait Found? How possible? 2");
			return null;
		}
		return listPool.GetRandom();
	}

	public static ActorTrait getMostUsedTraitFromPopulation(List<Actor> pUnits, HashSet<ActorTrait> pTraits, string pGroup)
	{
		Dictionary<ActorTrait, int> dictionary = new Dictionary<ActorTrait, int>();
		ActorTrait actorTrait;
		foreach (Actor pUnit in pUnits)
		{
			if (!pUnit.isAlive())
			{
				continue;
			}
			foreach (ActorTrait trait in pUnit.getTraits())
			{
				if (!(trait.group_id != pGroup) && !pTraits.Contains(trait) && !trait.hasOppositeTrait(pTraits))
				{
					if (dictionary.ContainsKey(trait))
					{
						actorTrait = trait;
						dictionary[actorTrait]++;
					}
					else
					{
						dictionary[trait] = 1;
					}
				}
			}
		}
		if (dictionary.Count == 0)
		{
			return null;
		}
		using (ListPool<ActorTrait> listPool = new ListPool<ActorTrait>(3))
		{
			foreach (KeyValuePair<ActorTrait, int> item in dictionary.OrderByDescending((KeyValuePair<ActorTrait, int> kv) => kv.Value))
			{
				listPool.Add(item.Key);
				if (listPool.Count >= 3)
				{
					break;
				}
			}
			using ListPool<ActorTrait> listPool2 = new ListPool<ActorTrait>();
			ActorTrait actorTrait2 = listPool[0];
			for (int num = 0; num < actorTrait2.getRate(pGroup) * 2; num++)
			{
				listPool2.Add(actorTrait2);
			}
			if (listPool.Count > 1)
			{
				ActorTrait actorTrait3 = listPool[1];
				for (int num2 = 0; num2 < actorTrait3.getRate(pGroup); num2++)
				{
					listPool2.Add(actorTrait3);
				}
			}
			if (listPool.Count > 2)
			{
				ActorTrait actorTrait4 = listPool[2];
				for (int num3 = 0; num3 < actorTrait4.getRate(pGroup); num3++)
				{
					listPool2.Add(actorTrait4);
				}
			}
			actorTrait = ((listPool2.Count != 0) ? listPool2.GetRandom() : null);
		}
		return actorTrait;
	}

	public static void recalculateTraitBonuses(BaseStats pBaseStats, HashSet<ActorTrait> pTraits, bool pClear = true)
	{
		if (pClear)
		{
			pBaseStats.clear();
		}
		foreach (ActorTrait pTrait in pTraits)
		{
			pBaseStats.mergeStats(pTrait.base_stats);
		}
	}
}
