using System;
using System.Collections.Generic;
using UnityPools;

namespace ai.behaviours;

public class KingdomBehCheckKing : BehaviourActionKingdom
{
	public override BehResult execute(Kingdom pKingdom)
	{
		if (pKingdom.data.timer_new_king > 0f)
		{
			return BehResult.Continue;
		}
		if (pKingdom.hasKing())
		{
			Actor king = pKingdom.king;
			if (king.isAlive())
			{
				tryToGiveGoldenTooth(king);
				checkClanCreation(king);
				return BehResult.Continue;
			}
		}
		pKingdom.clearKingData();
		if (pKingdom.data.royal_clan_id != -1)
		{
			Clan clan = BehaviourActionBase<Kingdom>.world.clans.get(pKingdom.data.royal_clan_id);
			bool flag = !clan.isRekt();
			Actor actor = findKingFromRoyalClan(pKingdom);
			if (actor == null)
			{
				if (pKingdom.countCities() == 1)
				{
					if (pKingdom.capital != null && pKingdom.capital.hasLeader())
					{
						Actor leader = pKingdom.capital.leader;
						pKingdom.capital.removeLeader();
						leader.stopBeingWarrior();
						pKingdom.setKing(leader);
					}
				}
				else
				{
					checkKingdomChaos(pKingdom);
				}
			}
			else if ((pKingdom.hasCulture() && pKingdom.culture.hasTrait("shattered_crown")) & flag)
			{
				checkShatteredCrownEvent(pKingdom, actor, clan);
			}
			if (!flag)
			{
				pKingdom.data.royal_clan_id = -1L;
			}
		}
		else
		{
			Actor kingFromLeaders = SuccessionTool.getKingFromLeaders(pKingdom);
			if (kingFromLeaders != null)
			{
				makeKingAndMoveToCapital(pKingdom, kingFromLeaders);
			}
			else
			{
				checkKingdomChaos(pKingdom);
			}
		}
		return BehResult.Continue;
	}

	private void checkKingdomChaos(Kingdom pMainKingdom)
	{
		bool flag = false;
		using ListPool<City> listPool = new ListPool<City>(pMainKingdom.countCities());
		foreach (City city in pMainKingdom.getCities())
		{
			if (city != pMainKingdom.capital && city.hasLeader())
			{
				listPool.Add(city);
			}
		}
		if (listPool.Count == 0)
		{
			return;
		}
		foreach (City item in listPool.LoopRandom())
		{
			Actor leader = item.leader;
			if (leader != null && leader.isAlive())
			{
				item.makeOwnKingdom(leader, pRebellion: false, pFellApart: true);
				flag = true;
			}
		}
		if (flag)
		{
			if (pMainKingdom.hasAlliance())
			{
				pMainKingdom.getAlliance().leave(pMainKingdom);
			}
			WorldLog.logFracturedKingdom(pMainKingdom);
		}
	}

	private void checkShatteredCrownEvent(Kingdom pMainKingdom, Actor pMainKing, Clan pRoyalClan)
	{
		if (!isRebellionsEnabled() || pRoyalClan == null)
		{
			return;
		}
		ListPool<Actor> tTempActors = new ListPool<Actor>(pRoyalClan.units.Count);
		try
		{
			using ListPool<City> listPool = new ListPool<City>(pMainKingdom.countCities());
			foreach (Actor unit in pRoyalClan.units)
			{
				if (!unit.isRekt() && unit != pMainKing && !unit.isKing())
				{
					tTempActors.Add(unit);
				}
			}
			foreach (City city3 in pMainKingdom.getCities())
			{
				if (city3 != pMainKingdom.capital)
				{
					listPool.Add(city3);
				}
			}
			if (tTempActors.Count == 0 || listPool.Count == 0)
			{
				return;
			}
			Dictionary<long, int> dictionary = UnsafeCollectionPool<Dictionary<long, int>, KeyValuePair<long, int>>.Get();
			using ListPool<Kingdom> listPool2 = new ListPool<Kingdom>();
			dictionary[pMainKingdom.id] = pMainKingdom.countCities();
			listPool.Shuffle();
			tTempActors.Shuffle();
			listPool.Sort(delegate(City a, City b)
			{
				int num = tTempActors.CountAll((Actor t) => t.city == a);
				int value = tTempActors.CountAll((Actor t) => t.city == b);
				return num.CompareTo(value);
			});
			bool flag = false;
			while (listPool.Count > 0 && tTempActors.Count > 0)
			{
				City city = listPool.Pop();
				Actor actor = null;
				if (city.hasLeader() && tTempActors.Contains(city.leader))
				{
					actor = city.leader;
				}
				else
				{
					foreach (ref Actor item in tTempActors)
					{
						Actor current3 = item;
						if (current3.city == city)
						{
							actor = current3;
							break;
						}
					}
				}
				if (actor == null)
				{
					actor = tTempActors.Last();
				}
				tTempActors.Remove(actor);
				if (actor.isCityLeader())
				{
					actor.city.removeLeader();
				}
				Kingdom kingdom = city.makeOwnKingdom(actor, pRebellion: false, pFellApart: true);
				listPool2.Add(kingdom);
				dictionary[kingdom.id] = 1;
				dictionary[pMainKingdom.id]--;
				actor.stopBeingWarrior();
				actor.joinCity(city);
				flag = true;
			}
			while (listPool.Count > 0)
			{
				City city2 = listPool.Pop();
				Kingdom random = listPool2.GetRandom();
				if (dictionary[random.id] < dictionary[pMainKingdom.id])
				{
					dictionary[random.id]++;
					dictionary[pMainKingdom.id]--;
					city2.joinAnotherKingdom(random, pCaptured: false, pRebellion: true);
				}
			}
			UnsafeCollectionPool<Dictionary<long, int>, KeyValuePair<long, int>>.Release(dictionary);
			if (flag)
			{
				if (pMainKingdom.hasAlliance())
				{
					pMainKingdom.getAlliance().leave(pMainKingdom);
				}
				WorldLog.logShatteredCrown(pMainKingdom);
			}
		}
		finally
		{
			if (tTempActors != null)
			{
				((IDisposable)tTempActors).Dispose();
			}
		}
	}

	private void checkClanCreation(Actor pActor)
	{
		if (!pActor.hasClan())
		{
			BehaviourActionBase<Kingdom>.world.clans.newClan(pActor, pAddDefaultTraits: true);
		}
	}

	private void tryToGiveGoldenTooth(Actor pActor)
	{
		if (pActor.getAge() > 45 && Randy.randomChance(0.05f))
		{
			pActor.addTrait("golden_tooth");
		}
	}

	private bool isRebellionsEnabled()
	{
		return WorldLawLibrary.world_law_rebellions.isEnabled();
	}

	private Actor findKingFromRoyalClan(Kingdom pKingdom)
	{
		Actor actor = SuccessionTool.getKingFromRoyalClan(pKingdom);
		if (actor == null && pKingdom.hasCulture() && (pKingdom.culture.hasTrait("unbroken_chain") || !isRebellionsEnabled()))
		{
			actor = SuccessionTool.getKingFromLeaders(pKingdom);
		}
		if (actor == null)
		{
			return null;
		}
		makeKingAndMoveToCapital(pKingdom, actor);
		return actor;
	}

	private void makeKingAndMoveToCapital(Kingdom pKingdom, Actor pNewKing)
	{
		if (pNewKing.hasCity())
		{
			pNewKing.stopBeingWarrior();
			if (pNewKing.isCityLeader())
			{
				pNewKing.city.removeLeader();
			}
		}
		if (pKingdom.hasCapital() && pNewKing.city != pKingdom.capital)
		{
			pNewKing.joinCity(pKingdom.capital);
		}
		pKingdom.setKing(pNewKing);
		WorldLog.logNewKing(pKingdom);
	}
}
