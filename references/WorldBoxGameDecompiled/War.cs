using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using db;

public class War : MetaObject<WarData>
{
	private readonly List<Kingdom> _list_attackers = new List<Kingdom>();

	private readonly List<Kingdom> _list_defenders = new List<Kingdom>();

	private readonly HashSet<Kingdom> _hashset_attackers = new HashSet<Kingdom>();

	private readonly HashSet<Kingdom> _hashset_defenders = new HashSet<Kingdom>();

	private WarTypeAsset _asset;

	protected override MetaType meta_type => MetaType.War;

	public override BaseSystemManager manager => World.world.wars;

	[CanBeNull]
	public Kingdom main_attacker => getMainAttacker();

	[CanBeNull]
	public Kingdom main_defender => getMainDefender();

	[CanBeNull]
	public Kingdom getMainAttacker()
	{
		return World.world.kingdoms.get(data.main_attacker) ?? World.world.kingdoms.db_get(data.main_attacker);
	}

	[CanBeNull]
	public Kingdom getMainDefender()
	{
		return World.world.kingdoms.get(data.main_defender) ?? World.world.kingdoms.db_get(data.main_defender);
	}

	public bool isMainAttacker(Kingdom pKingdom)
	{
		return pKingdom.getID() == data.main_attacker;
	}

	public bool isMainDefender(Kingdom pKingdom)
	{
		return pKingdom.getID() == data.main_defender;
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
	}

	public override ColorAsset getColor()
	{
		Kingdom mainAttacker = getMainAttacker();
		if (!mainAttacker.isRekt())
		{
			return mainAttacker.getColor();
		}
		using (IEnumerator<Kingdom> enumerator = (hasEnded() ? getAllAttackers() : getAttackers()).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.getColor();
			}
		}
		Kingdom mainDefender = getMainDefender();
		if (!mainDefender.isRekt())
		{
			return mainDefender.getColor();
		}
		using (IEnumerator<Kingdom> enumerator = (hasEnded() ? getAllDefenders() : getDefenders()).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.getColor();
			}
		}
		return null;
	}

	public WarTypeAsset getAsset()
	{
		if (_asset == null)
		{
			_asset = AssetManager.war_types_library.get(data.war_type);
		}
		return _asset;
	}

	public void newWar(Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pAsset)
	{
		data.main_attacker = pAttacker.id;
		if (pDefender != null)
		{
			data.main_defender = pDefender.id;
		}
		_asset = pAsset;
		data.war_type = pAsset.id;
		joinAttackers(pAttacker);
		if (pDefender != null)
		{
			joinDefenders(pDefender);
		}
		prepare();
	}

	public override void clearLastYearStats()
	{
		addRenown(1);
	}

	public override void increaseBirths()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public override void increaseKills()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public void increaseDeathsDefenders(AttackType pAttackType)
	{
		data.dead_defenders++;
		increaseDeaths(pAttackType);
		addRenown(1);
	}

	public void increaseDeathsAttackers(AttackType pAttackType)
	{
		data.dead_attackers++;
		increaseDeaths(pAttackType);
		addRenown(1);
	}

	public void leaveWar(Kingdom pKingdom)
	{
		addRenown(25);
		removeFromWar(pKingdom, pInPeace: true);
	}

	public void lostWar(Kingdom pKingdom)
	{
		addRenown(50);
		removeFromWar(pKingdom, pInPeace: false);
		update();
	}

	internal void removeFromWar(Kingdom pKingdom, bool pInPeace)
	{
		if (isAttacker(pKingdom))
		{
			foreach (Kingdom hashset_defender in _hashset_defenders)
			{
				World.world.diplomacy.getRelation(pKingdom, hashset_defender).data.timestamp_last_war_ended = World.world.getCurWorldTime();
			}
		}
		else
		{
			foreach (Kingdom hashset_attacker in _hashset_attackers)
			{
				World.world.diplomacy.getRelation(pKingdom, hashset_attacker).data.timestamp_last_war_ended = World.world.getCurWorldTime();
			}
		}
		removeAttacker(pKingdom, pInPeace);
		removeDefender(pKingdom, pInPeace);
		if (isMainAttacker(pKingdom) && !trySelectNewMainAttacker())
		{
			World.world.wars.endWar(this, pInPeace ? WarWinner.Peace : WarWinner.Defenders);
		}
		else if (isMainDefender(pKingdom) && !trySelectNewMainDefender())
		{
			World.world.wars.endWar(this, (!pInPeace) ? WarWinner.Attackers : WarWinner.Peace);
		}
		else
		{
			pKingdom.checkEndWar();
			if (pInPeace)
			{
				pKingdom.madePeace(this);
			}
			else
			{
				pKingdom.lostWar(this);
			}
		}
		prepare();
	}

	public int countAttackers()
	{
		return _list_attackers.Count;
	}

	public bool trySelectNewMainAttacker()
	{
		if (countAttackers() <= 1)
		{
			return false;
		}
		_list_attackers.Sort(KingdomListComponent.sortByArmy);
		foreach (Kingdom list_attacker in _list_attackers)
		{
			if (list_attacker.id != data.main_attacker)
			{
				data.main_attacker = list_attacker.id;
				return true;
			}
		}
		return false;
	}

	public bool trySelectNewMainDefender()
	{
		if (countDefenders() <= 1)
		{
			return false;
		}
		_list_defenders.Sort(KingdomListComponent.sortByArmy);
		foreach (Kingdom list_defender in _list_defenders)
		{
			if (list_defender.id != data.main_defender)
			{
				data.main_defender = list_defender.id;
				return true;
			}
		}
		return false;
	}

	public IEnumerable<Kingdom> getAttackers()
	{
		if (hasEnded())
		{
			return getHistoricAttackers();
		}
		return _list_attackers;
	}

	public IEnumerable<Kingdom> getHistoricAttackers()
	{
		foreach (long list_attacker in data.list_attackers)
		{
			Kingdom kingdom = World.world.kingdoms.get(list_attacker);
			if (kingdom != null)
			{
				yield return kingdom;
				continue;
			}
			DeadKingdom deadKingdom = World.world.kingdoms.db_get(list_attacker);
			if (deadKingdom != null)
			{
				yield return deadKingdom;
			}
		}
	}

	public IEnumerable<Kingdom> getAllAttackers()
	{
		foreach (Kingdom attacker in getAttackers())
		{
			yield return attacker;
		}
		foreach (Kingdom pastAttacker in getPastAttackers())
		{
			yield return pastAttacker;
		}
		foreach (Kingdom diedAttacker in getDiedAttackers())
		{
			yield return diedAttacker;
		}
	}

	public IEnumerable<Kingdom> getAllDefenders()
	{
		foreach (Kingdom defender in getDefenders())
		{
			yield return defender;
		}
		foreach (Kingdom pastDefender in getPastDefenders())
		{
			yield return pastDefender;
		}
		foreach (Kingdom diedDefender in getDiedDefenders())
		{
			yield return diedDefender;
		}
	}

	public IEnumerable<Kingdom> getPastAttackers()
	{
		foreach (long past_attacker in data.past_attackers)
		{
			Kingdom kingdom = World.world.kingdoms.get(past_attacker);
			if (kingdom != null)
			{
				yield return kingdom;
				continue;
			}
			DeadKingdom deadKingdom = World.world.kingdoms.db_get(past_attacker);
			if (deadKingdom != null)
			{
				yield return deadKingdom;
			}
		}
	}

	public IEnumerable<Kingdom> getDiedAttackers()
	{
		foreach (long died_attacker in data.died_attackers)
		{
			Kingdom kingdom = World.world.kingdoms.get(died_attacker);
			if (kingdom != null)
			{
				yield return kingdom;
				continue;
			}
			DeadKingdom deadKingdom = World.world.kingdoms.db_get(died_attacker);
			if (deadKingdom != null)
			{
				yield return deadKingdom;
			}
		}
	}

	public IEnumerable<Kingdom> getActiveParties()
	{
		bool tAttackersFirst = Randy.randomBool();
		foreach (Kingdom item in tAttackersFirst ? getAttackers() : getDefenders())
		{
			if (item.isAlive())
			{
				yield return item;
			}
		}
		foreach (Kingdom item2 in tAttackersFirst ? getDefenders() : getAttackers())
		{
			if (item2.isAlive())
			{
				yield return item2;
			}
		}
	}

	public string getAttackersColorTextString()
	{
		Kingdom mainAttacker = getMainAttacker();
		if (mainAttacker != null)
		{
			return mainAttacker.getColor().color_text;
		}
		using (IEnumerator<Kingdom> enumerator = (hasEnded() ? getAllAttackers() : getAttackers()).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.getColor().color_text;
			}
		}
		return "#F3961F";
	}

	public string getDefendersColorTextString()
	{
		if (isTotalWar())
		{
			return "#F3961F";
		}
		Kingdom mainDefender = getMainDefender();
		if (mainDefender != null)
		{
			return mainDefender.getColor().color_text;
		}
		return "#F3961F";
	}

	public int countDefenders()
	{
		if (!isTotalWar())
		{
			return _list_defenders.Count;
		}
		return World.world.kingdoms.Count - 1;
	}

	public IEnumerable<Kingdom> getDefenders()
	{
		if (hasEnded())
		{
			foreach (Kingdom historicDefender in getHistoricDefenders())
			{
				yield return historicDefender;
			}
			yield break;
		}
		if (!isTotalWar())
		{
			foreach (Kingdom list_defender in _list_defenders)
			{
				yield return list_defender;
			}
			yield break;
		}
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (!isMainAttacker(kingdom))
			{
				yield return kingdom;
			}
		}
	}

	public IEnumerable<Kingdom> getHistoricDefenders()
	{
		foreach (long list_defender in data.list_defenders)
		{
			Kingdom kingdom = World.world.kingdoms.get(list_defender);
			if (kingdom != null)
			{
				yield return kingdom;
				continue;
			}
			DeadKingdom deadKingdom = World.world.kingdoms.db_get(list_defender);
			if (deadKingdom != null)
			{
				yield return deadKingdom;
			}
		}
	}

	public IEnumerable<Kingdom> getPastDefenders()
	{
		foreach (long past_defender in data.past_defenders)
		{
			Kingdom kingdom = World.world.kingdoms.get(past_defender);
			if (kingdom != null)
			{
				yield return kingdom;
				continue;
			}
			DeadKingdom deadKingdom = World.world.kingdoms.db_get(past_defender);
			if (deadKingdom != null)
			{
				yield return deadKingdom;
			}
		}
	}

	public IEnumerable<Kingdom> getDiedDefenders()
	{
		foreach (long died_defender in data.died_defenders)
		{
			Kingdom kingdom = World.world.kingdoms.get(died_defender);
			if (kingdom != null)
			{
				yield return kingdom;
				continue;
			}
			DeadKingdom deadKingdom = World.world.kingdoms.db_get(died_defender);
			if (deadKingdom != null)
			{
				yield return deadKingdom;
			}
		}
	}

	public void update()
	{
		if (hasEnded())
		{
			return;
		}
		if (!main_attacker.isAlive())
		{
			lostWar(main_attacker);
			return;
		}
		if (isTotalWar())
		{
			if (World.world.kingdoms.Count <= 1)
			{
				World.world.wars.endWar(this, WarWinner.Attackers);
				return;
			}
		}
		else if (!main_defender.isAlive())
		{
			lostWar(main_defender);
			return;
		}
		if (getAge() > 10 && !isTotalWar())
		{
			if (main_attacker.countCities() == 0)
			{
				lostWar(main_attacker);
				return;
			}
			if (main_defender.countCities() == 0)
			{
				lostWar(main_defender);
				return;
			}
		}
		for (int i = 0; i < _list_attackers.Count; i++)
		{
			Kingdom kingdom = _list_attackers[i];
			if (!kingdom.isAlive())
			{
				lostWar(kingdom);
				return;
			}
		}
		if (!isTotalWar())
		{
			for (int i = 0; i < _list_defenders.Count; i++)
			{
				Kingdom kingdom2 = _list_defenders[i];
				if (!kingdom2.isAlive())
				{
					lostWar(kingdom2);
					return;
				}
			}
		}
		if (isTotalWar())
		{
			if (_list_attackers.Count == 0 || World.world.kingdoms.Count == 1)
			{
				Debug.LogError((object)"[1] should never happen here");
			}
		}
		else if (_list_attackers.Count == 0 || _list_defenders.Count == 0)
		{
			Debug.LogError((object)"[2] should never happen here");
		}
	}

	public bool isAttacker(Kingdom pKingdom)
	{
		return _hashset_attackers.Contains(pKingdom);
	}

	public bool isDefender(Kingdom pKingdom)
	{
		if (isTotalWar() && !isMainAttacker(pKingdom))
		{
			return true;
		}
		return _hashset_defenders.Contains(pKingdom);
	}

	public List<Kingdom> getOppositeSideKingdom(Kingdom pKingdom)
	{
		if (isAttacker(pKingdom))
		{
			return _list_defenders;
		}
		if (isDefender(pKingdom))
		{
			return _list_attackers;
		}
		return null;
	}

	public bool isTotalWar()
	{
		return getAsset().total_war;
	}

	public bool isInWarWith(Kingdom pKingdom, Kingdom pTarget)
	{
		if (isTotalWar())
		{
			if (isMainAttacker(pKingdom))
			{
				return true;
			}
			if (isMainAttacker(pTarget))
			{
				return true;
			}
			return false;
		}
		if (isAttacker(pKingdom) && isDefender(pTarget))
		{
			return true;
		}
		if (isDefender(pKingdom) && isAttacker(pTarget))
		{
			return true;
		}
		return false;
	}

	public bool onTheSameSide(Kingdom pKingdom1, Kingdom pKingdom2)
	{
		if (isAttacker(pKingdom1) && isAttacker(pKingdom2))
		{
			return true;
		}
		if (isDefender(pKingdom1) && isDefender(pKingdom2))
		{
			return true;
		}
		return false;
	}

	public bool hasKingdom(Kingdom pKingdom)
	{
		if (isTotalWar())
		{
			return true;
		}
		if (isAttacker(pKingdom))
		{
			return true;
		}
		if (isDefender(pKingdom))
		{
			return true;
		}
		return false;
	}

	public void joinAttackers(Kingdom pKingdom)
	{
		if (!data.list_attackers.Contains(pKingdom.id))
		{
			addRenown(5);
			data.past_attackers.Remove(pKingdom.id);
			data.list_attackers.Add(pKingdom.id);
			prepare();
		}
	}

	public void joinDefenders(Kingdom pKingdom)
	{
		if (!isTotalWar() && !data.list_defenders.Contains(pKingdom.id))
		{
			addRenown(5);
			data.past_defenders.Remove(pKingdom.id);
			data.list_defenders.Add(pKingdom.id);
			prepare();
		}
	}

	public override void loadData(WarData pData)
	{
		base.loadData(pData);
		prepare();
	}

	public void prepare()
	{
		_list_attackers.Clear();
		_list_defenders.Clear();
		_hashset_attackers.Clear();
		_hashset_defenders.Clear();
		if (data.died_attackers == null)
		{
			data.died_attackers = new List<long>();
		}
		if (data.died_defenders == null)
		{
			data.died_defenders = new List<long>();
		}
		if (data.past_attackers == null)
		{
			data.past_attackers = new List<long>();
		}
		if (data.past_defenders == null)
		{
			data.past_defenders = new List<long>();
		}
		foreach (long list_attacker in data.list_attackers)
		{
			Kingdom kingdom = World.world.kingdoms.get(list_attacker);
			if (kingdom != null)
			{
				_list_attackers.Add(kingdom);
				_hashset_attackers.Add(kingdom);
			}
		}
		foreach (long list_defender in data.list_defenders)
		{
			Kingdom kingdom2 = World.world.kingdoms.get(list_defender);
			if (kingdom2 != null)
			{
				_list_defenders.Add(kingdom2);
				_hashset_defenders.Add(kingdom2);
			}
		}
		World.world.wars.warStateChanged();
	}

	public int getDeadAttackers()
	{
		return data.dead_attackers;
	}

	public int getDeadDefenders()
	{
		return data.dead_defenders;
	}

	public void endForSides(WarWinner pWinner)
	{
		foreach (Kingdom hashset_attacker in _hashset_attackers)
		{
			hashset_attacker.checkEndWar();
			switch (pWinner)
			{
			case WarWinner.Attackers:
				hashset_attacker.wonWar(this);
				break;
			case WarWinner.Defenders:
				hashset_attacker.lostWar(this);
				break;
			case WarWinner.Peace:
				hashset_attacker.madePeace(this);
				break;
			}
		}
		foreach (Kingdom hashset_defender in _hashset_defenders)
		{
			hashset_defender.checkEndWar();
			switch (pWinner)
			{
			case WarWinner.Attackers:
				hashset_defender.lostWar(this);
				break;
			case WarWinner.Defenders:
				hashset_defender.wonWar(this);
				break;
			case WarWinner.Peace:
				hashset_defender.madePeace(this);
				break;
			}
		}
		if (pWinner == WarWinner.Merged)
		{
			return;
		}
		foreach (Kingdom hashset_attacker2 in _hashset_attackers)
		{
			foreach (Kingdom hashset_defender2 in _hashset_defenders)
			{
				World.world.diplomacy.getRelation(hashset_attacker2, hashset_defender2).data.timestamp_last_war_ended = World.world.getCurWorldTime();
			}
		}
	}

	public int countKingdoms()
	{
		if (isTotalWar())
		{
			return World.world.kingdoms.Count;
		}
		return 0 + countAttackers() + countDefenders();
	}

	public int countCities()
	{
		return countAttackersCities() + countDefendersCities();
	}

	public int countAttackersCities()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllAttackers() : getAttackers())
		{
			num += item.countCities();
		}
		return num;
	}

	public int countDefendersCities()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllDefenders() : getDefenders())
		{
			num += item.countCities();
		}
		return num;
	}

	public int countDefendersPopulation()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllDefenders() : getDefenders())
		{
			num += item.getPopulationPeople();
		}
		return num;
	}

	public int countDefendersWarriors()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllDefenders() : getDefenders())
		{
			num += item.countTotalWarriors();
		}
		return num;
	}

	public int countDefendersMoney()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllDefenders() : getDefenders())
		{
			num += item.countTotalMoney();
		}
		return num;
	}

	public int countAttackersPopulation()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllAttackers() : getAttackers())
		{
			num += item.getPopulationPeople();
		}
		return num;
	}

	public int countAttackersWarriors()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllAttackers() : getAttackers())
		{
			num += item.countTotalWarriors();
		}
		return num;
	}

	public int countAttackersMoney()
	{
		int num = 0;
		foreach (Kingdom item in hasEnded() ? getAllAttackers() : getAttackers())
		{
			num += item.countTotalMoney();
		}
		return num;
	}

	public int countTotalPopulation()
	{
		return countAttackersPopulation() + countDefendersPopulation();
	}

	public int countTotalArmy()
	{
		return countAttackersWarriors() + countDefendersWarriors();
	}

	public override int countUnits()
	{
		return countTotalPopulation();
	}

	public override IEnumerable<Actor> getUnits()
	{
		foreach (Kingdom attacker in getAttackers())
		{
			foreach (Actor unit in attacker.getUnits())
			{
				yield return unit;
			}
		}
		foreach (Kingdom defender in getDefenders())
		{
			foreach (Actor unit2 in defender.getUnits())
			{
				yield return unit2;
			}
		}
	}

	public override Actor getRandomUnit()
	{
		using ListPool<Kingdom> list = new ListPool<Kingdom>(getActiveParties());
		foreach (Kingdom item in list.LoopRandom())
		{
			Actor randomUnit = item.getRandomUnit();
			if (randomUnit != null)
			{
				return randomUnit;
			}
		}
		return null;
	}

	public override bool isReadyForRemoval()
	{
		return false;
	}

	public bool hasEnded()
	{
		if (isAlive())
		{
			return hasDied();
		}
		return true;
	}

	public bool isSameAs(War pWar)
	{
		if (hasEnded())
		{
			return false;
		}
		if (pWar == null)
		{
			return false;
		}
		if (pWar.hasEnded())
		{
			return false;
		}
		if (!_hashset_attackers.SetEquals(pWar._hashset_attackers) && !_hashset_defenders.SetEquals(pWar._hashset_attackers))
		{
			return false;
		}
		if (!_hashset_defenders.SetEquals(pWar._hashset_defenders) && !_hashset_attackers.SetEquals(pWar._hashset_defenders))
		{
			return false;
		}
		return true;
	}

	public int getYearEnded()
	{
		return Date.getYear(data.died_time);
	}

	public int getYearStarted()
	{
		return Date.getYear(data.created_time);
	}

	public int getDuration()
	{
		if (hasEnded())
		{
			return getYearEnded() - getYearStarted();
		}
		return Date.getYearsSince(data.created_time);
	}

	public void setWinner(WarWinner pWinner)
	{
		if (pWinner != WarWinner.Nobody)
		{
			data.winner = pWinner;
		}
	}

	public void removeAttacker(Kingdom pKingdom, bool pInPeace)
	{
		if (data.list_attackers.Contains(pKingdom.id))
		{
			data.list_attackers.Remove(pKingdom.id);
			if (!pInPeace || !pKingdom.isAlive())
			{
				data.died_attackers.Add(pKingdom.id);
			}
			else
			{
				data.past_attackers.Add(pKingdom.id);
			}
		}
	}

	public void removeDefender(Kingdom pKingdom, bool pInPeace)
	{
		if (data.list_defenders.Contains(pKingdom.id))
		{
			data.list_defenders.Remove(pKingdom.id);
			if (!pInPeace || !pKingdom.isAlive())
			{
				data.died_defenders.Add(pKingdom.id);
			}
			else
			{
				data.past_defenders.Add(pKingdom.id);
			}
		}
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "war");
		_list_attackers.Clear();
		_list_defenders.Clear();
		_hashset_attackers.Clear();
		_hashset_defenders.Clear();
		_asset = null;
		base.Dispose();
	}

	public override string ToString()
	{
		string text = "War: ";
		text += (isAlive() ? "alive " : "dead ");
		if (isAlive())
		{
			text = text + base.id + " ";
			text += " a:";
			text += string.Join(",", from tKingdom in getAttackers()
				select tKingdom.id);
			text += " d:";
			text += string.Join(",", from tKingdom in getDefenders()
				select tKingdom.id);
			text = text + " t:" + data.war_type;
			text = text + " w:" + data.winner;
			text = text + " e:" + hasEnded();
		}
		return text;
	}
}
