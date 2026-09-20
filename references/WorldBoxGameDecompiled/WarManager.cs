using System.Collections.Generic;
using UnityEngine;

public class WarManager : MetaSystemManager<War, WarData>
{
	public KingdomCheckCache cache_war_check = new KingdomCheckCache();

	private List<War> _end_wars = new List<War>();

	public WarManager()
	{
		type_id = "war";
	}

	protected override void updateDirtyUnits()
	{
	}

	public override void clear()
	{
		base.clear();
		warStateChanged();
	}

	public override void checkDeadObjects()
	{
		if (Count <= 20)
		{
			return;
		}
		using (IEnumerator<War> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				War current = enumerator.Current;
				if (!current.isFavorite() && current.hasEnded())
				{
					_end_wars.Add(current);
				}
			}
		}
		if (_end_wars.Count != 0)
		{
			int num = Count - 20;
			_end_wars.Sort((War a, War b) => a.data.died_time.CompareTo(b.data.died_time));
			int num2 = Mathf.Min(_end_wars.Count, num);
			for (int num3 = 0; num3 < num2; num3++)
			{
				War pObject = _end_wars[num3];
				removeObject(pObject);
			}
			_end_wars.Clear();
		}
	}

	public void warStateChanged()
	{
		cache_war_check.clear();
		Kingdom.cache_enemy_check.clear();
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		int count = list.Count;
		while (count-- > 0)
		{
			list[count].update();
		}
	}

	public void stopAllWars()
	{
		_end_wars.AddRange(this);
		endWars(WarWinner.Peace);
	}

	private void endWars(WarWinner pWinner = WarWinner.Nobody)
	{
		if (_end_wars.Count == 0)
		{
			return;
		}
		foreach (War end_war in _end_wars)
		{
			endWar(end_war, pWinner);
		}
		_end_wars.Clear();
	}

	public bool isInWarWith(Kingdom pKingdom, Kingdom pKingdomTarget)
	{
		long hash = cache_war_check.getHash(pKingdom, pKingdomTarget);
		if (cache_war_check.dict.TryGetValue(hash, out var value))
		{
			return value;
		}
		foreach (War war in pKingdom.getWars())
		{
			if (war.isInWarWith(pKingdom, pKingdomTarget))
			{
				cache_war_check.dict.Add(hash, value: true);
				return true;
			}
		}
		cache_war_check.dict.Add(hash, value: false);
		return false;
	}

	public War getWar(Kingdom pAttacker, Kingdom pDefender, bool pOnlyMain = true)
	{
		using (IEnumerator<War> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				War current = enumerator.Current;
				if (!current.hasEnded())
				{
					if (current.isMainAttacker(pAttacker) && current.isMainDefender(pDefender))
					{
						return current;
					}
					if (current.isMainAttacker(pDefender) && current.isMainDefender(pAttacker))
					{
						return current;
					}
				}
			}
		}
		if (!pOnlyMain)
		{
			using IEnumerator<War> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				War current2 = enumerator.Current;
				if (!current2.hasEnded() && current2.isInWarWith(pAttacker, pDefender))
				{
					return current2;
				}
			}
		}
		return null;
	}

	public War getForcedWar(Kingdom pKingdom)
	{
		using (IEnumerator<War> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				War current = enumerator.Current;
				if (!current.hasEnded() && current.getAsset().forced_war && current.isMainDefender(pKingdom))
				{
					return current;
				}
			}
		}
		return null;
	}

	public IEnumerable<War> getWars(Kingdom pKingdom, bool pRandom = false)
	{
		foreach (War item in pRandom ? list.LoopRandom() : this)
		{
			if (!item.hasEnded() && item.hasKingdom(pKingdom))
			{
				yield return item;
			}
		}
	}

	public IEnumerable<War> getWars(Alliance pAlliance, bool pRandom = false)
	{
		foreach (War item in pRandom ? list.LoopRandom() : this)
		{
			if (item.hasEnded())
			{
				continue;
			}
			foreach (Kingdom item2 in pAlliance.kingdoms_list)
			{
				if (item.hasKingdom(item2))
				{
					yield return item;
					break;
				}
			}
		}
	}

	public bool hasWars(Kingdom pKingdom)
	{
		using (IEnumerator<War> enumerator = getWars(pKingdom).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public bool hasWars(Alliance pAlliance)
	{
		using (IEnumerator<War> enumerator = getWars(pAlliance).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public bool haveCommonEnemy(Kingdom pKingdom1, Kingdom pKingdom2)
	{
		using ListPool<Kingdom> listPool = pKingdom1.getEnemiesKingdoms();
		using ListPool<Kingdom> listPool2 = pKingdom2.getEnemiesKingdoms();
		foreach (ref Kingdom item in listPool)
		{
			Kingdom current = item;
			foreach (ref Kingdom item2 in listPool2)
			{
				Kingdom current2 = item2;
				if (current == current2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public War getRandomWarFor(Kingdom pKingdom)
	{
		using (IEnumerator<War> enumerator = getWars(pKingdom, pRandom: true).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	public void getWarCities(Kingdom pKingdom, ListPool<City> pCities)
	{
		using ListPool<Kingdom> listPool = getEnemiesOf(pKingdom);
		for (int i = 0; i < listPool.Count; i++)
		{
			Kingdom kingdom = listPool[i];
			pCities.AddRange(kingdom.getCities());
		}
	}

	public ListPool<Kingdom> getNeutralKingdoms(Kingdom pKingdom, bool pOnlyWithoutWars = false, bool pOnlyWithoutAlliances = false)
	{
		ListPool<Kingdom> listPool = new ListPool<Kingdom>(World.world.kingdoms.Count);
		Alliance alliance = pKingdom.getAlliance();
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom != pKingdom && !isInWarWith(pKingdom, kingdom) && (!pOnlyWithoutWars || !kingdom.hasEnemies()))
			{
				Alliance alliance2 = kingdom.getAlliance();
				if ((!pOnlyWithoutAlliances || alliance2 == null) && !Alliance.isSame(alliance, alliance2))
				{
					listPool.Add(kingdom);
				}
			}
		}
		return listPool;
	}

	public ListPool<Kingdom> getEnemiesOf(Kingdom pKingdom)
	{
		ListPool<Kingdom> listPool = new ListPool<Kingdom>(World.world.kingdoms.Count);
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom != pKingdom && pKingdom.isEnemy(kingdom))
			{
				listPool.Add(kingdom);
			}
		}
		return listPool;
	}

	public void endWar(War pWar, WarWinner pWinner = WarWinner.Nobody)
	{
		if (pWar.isAlive() && !pWar.hasEnded())
		{
			World.world.game_stats.data.peacesMade++;
			World.world.map_stats.peacesMade++;
			pWar.setWinner(pWinner);
			warStateChanged();
			WorldLog.logWarEnded(pWar);
			pWar.endForSides(pWinner);
			pWar.data.died_time = World.world.getCurWorldTime();
		}
	}

	public War newWar(Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pType)
	{
		World.world.game_stats.data.warsStarted++;
		World.world.map_stats.warsStarted++;
		warStateChanged();
		War war = newObject();
		war.newWar(pAttacker, pDefender, pType);
		war.data.started_by_actor_name = pAttacker.king?.getName();
		war.data.started_by_actor_id = pAttacker.king?.getID() ?? (-1);
		war.data.started_by_kingdom_name = pAttacker.data.name;
		war.data.started_by_kingdom_id = pAttacker.data.id;
		string name_template = pType.name_template;
		Kingdom pKingdom = ((!pType.kingdom_for_name_attacker) ? pDefender : pAttacker);
		string pName = NameGenerator.generateNameFromTemplate(name_template, null, pKingdom);
		war.setName(pName);
		checkHappinessFromStartOfWar(pAttacker);
		return war;
	}

	public static void checkHappinessFromStartOfWar(Kingdom pKingdom)
	{
		if (!pKingdom.hasCulture() || !pKingdom.culture.hasTrait("happiness_from_war"))
		{
			return;
		}
		for (int i = 0; i < pKingdom.units.Count; i++)
		{
			Actor actor = pKingdom.units[i];
			if (actor.hasCulture() && actor.culture.hasTrait("happiness_from_war"))
			{
				actor.changeHappiness("just_started_war");
			}
		}
	}

	public long countActiveWars()
	{
		long num = 0L;
		foreach (War activeWar in getActiveWars())
		{
			_ = activeWar;
			num++;
		}
		return num;
	}

	public IEnumerable<War> getActiveWars()
	{
		using IEnumerator<War> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			War current = enumerator.Current;
			if (!current.hasEnded())
			{
				yield return current;
			}
		}
	}

	public override bool hasAny()
	{
		if (Count == 0)
		{
			return false;
		}
		using (IEnumerator<War> enumerator = getActiveWars().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}
}
