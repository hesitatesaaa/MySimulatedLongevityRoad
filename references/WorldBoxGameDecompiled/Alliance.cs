using System.Collections.Generic;
using UnityEngine;
using db;

public class Alliance : MetaObject<AllianceData>
{
	public List<Kingdom> kingdoms_list = new List<Kingdom>();

	public HashSet<Kingdom> kingdoms_hashset = new HashSet<Kingdom>();

	public int power;

	protected override MetaType meta_type => MetaType.Alliance;

	public override BaseSystemManager manager => World.world.alliances;

	public void createNewAlliance()
	{
		string pName = NameGenerator.getName("alliance_name");
		setName(pName);
		generateNewMetaObject();
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
		power = 0;
	}

	public override int countTotalMoney()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countTotalMoney();
		}
		return num;
	}

	public override int countHappyUnits()
	{
		if (kingdoms_list.Count == 0)
		{
			return 0;
		}
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countHappyUnits();
		}
		return num;
	}

	public override int countSick()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countSick();
		}
		return num;
	}

	public override int countHungry()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countHungry();
		}
		return num;
	}

	public override int countStarving()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countStarving();
		}
		return num;
	}

	public override int countChildren()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countChildren();
		}
		return num;
	}

	public override int countAdults()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countAdults();
		}
		return num;
	}

	public override int countHomeless()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countHomeless();
		}
		return num;
	}

	public override IEnumerable<Family> getFamilies()
	{
		List<Kingdom> tKingdoms = kingdoms_list;
		for (int i = 0; i < tKingdoms.Count; i++)
		{
			Kingdom kingdom = tKingdoms[i];
			foreach (Family family in kingdom.getFamilies())
			{
				yield return family;
			}
		}
	}

	public override bool hasFamilies()
	{
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].hasFamilies())
			{
				return true;
			}
		}
		return false;
	}

	public override int countMales()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countMales();
		}
		return num;
	}

	public override int countFemales()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countFemales();
		}
		return num;
	}

	public override int countHoused()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countHoused();
		}
		return num;
	}

	public void setType(AllianceType pType)
	{
		data.alliance_type = pType;
	}

	public bool isForcedType()
	{
		return data.alliance_type == AllianceType.Forced;
	}

	public bool isNormalType()
	{
		return data.alliance_type == AllianceType.Normal;
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.kingdom_colors_library;
	}

	public override void generateBanner()
	{
		Sprite[] backgroundsList = World.world.alliances.getBackgroundsList();
		data.banner_background_id = Randy.randomInt(0, backgroundsList.Length);
		Sprite[] iconsList = World.world.alliances.getIconsList();
		data.banner_icon_id = Randy.randomInt(0, iconsList.Length);
	}

	public void addFounders(Kingdom pKingdom1, Kingdom pKingdom2)
	{
		data.founder_kingdom_name = pKingdom1.data.name;
		data.founder_kingdom_id = pKingdom1.getID();
		data.founder_actor_name = pKingdom1.king?.getName();
		data.founder_actor_id = pKingdom1.king?.getID() ?? (-1);
		join(pKingdom1, pRecalc: true, pForce: true);
		join(pKingdom2, pRecalc: true, pForce: true);
	}

	public void update()
	{
		power = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			power += kingdom.power;
		}
	}

	public bool checkActive()
	{
		bool flag = false;
		List<Kingdom> list = kingdoms_list;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Kingdom kingdom = list[num];
			if (!kingdom.isAlive())
			{
				leave(kingdom, pRecalc: false);
				kingdoms_list.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			recalculate();
		}
		if (kingdoms_list.Count >= 2)
		{
			return true;
		}
		return false;
	}

	public void dissolve()
	{
		foreach (Kingdom item in kingdoms_hashset)
		{
			item.allianceLeave(this);
		}
		kingdoms_hashset.Clear();
	}

	public void recalculate()
	{
		kingdoms_list.Clear();
		kingdoms_list.AddRange(kingdoms_hashset);
		mergeWars();
	}

	public bool canJoin(Kingdom pKingdom)
	{
		foreach (Kingdom item in kingdoms_hashset)
		{
			if (!pKingdom.isOpinionTowardsKingdomGood(item))
			{
				return false;
			}
		}
		return true;
	}

	public bool join(Kingdom pKingdom, bool pRecalc = true, bool pForce = false)
	{
		if (hasKingdom(pKingdom))
		{
			return false;
		}
		if (!pForce && !canJoin(pKingdom))
		{
			return false;
		}
		kingdoms_hashset.Add(pKingdom);
		if (hasWars())
		{
			if (hasWarsWith(pKingdom))
			{
				foreach (War attackerWar in getAttackerWars())
				{
					if (attackerWar.isDefender(pKingdom))
					{
						attackerWar.leaveWar(pKingdom);
					}
				}
				foreach (War defenderWar in getDefenderWars())
				{
					if (defenderWar.isAttacker(pKingdom))
					{
						defenderWar.leaveWar(pKingdom);
					}
				}
			}
			foreach (War attackerWar2 in getAttackerWars())
			{
				attackerWar2.joinAttackers(pKingdom);
			}
			foreach (War defenderWar2 in getDefenderWars())
			{
				if (!defenderWar2.isTotalWar())
				{
					defenderWar2.joinDefenders(pKingdom);
				}
			}
		}
		if (pKingdom.hasEnemies())
		{
			foreach (War war in pKingdom.getWars())
			{
				if (war.isTotalWar())
				{
					continue;
				}
				if (war.isMainAttacker(pKingdom))
				{
					foreach (Kingdom item in kingdoms_list)
					{
						war.joinAttackers(item);
					}
				}
				if (!war.isMainDefender(pKingdom))
				{
					continue;
				}
				foreach (Kingdom item2 in kingdoms_list)
				{
					war.joinDefenders(item2);
				}
			}
		}
		pKingdom.allianceJoin(this);
		if (pRecalc)
		{
			recalculate();
		}
		data.timestamp_member_joined = World.world.getCurWorldTime();
		return true;
	}

	public void leave(Kingdom pKingdom, bool pRecalc = true)
	{
		kingdoms_hashset.Remove(pKingdom);
		if (hasWars())
		{
			foreach (War attackerWar in getAttackerWars())
			{
				if (!attackerWar.isMainAttacker(pKingdom))
				{
					attackerWar.leaveWar(pKingdom);
					continue;
				}
				foreach (Kingdom item in kingdoms_hashset)
				{
					attackerWar.leaveWar(item);
				}
			}
			foreach (War defenderWar in getDefenderWars())
			{
				if (!defenderWar.isMainDefender(pKingdom))
				{
					defenderWar.leaveWar(pKingdom);
					continue;
				}
				foreach (Kingdom item2 in kingdoms_hashset)
				{
					defenderWar.leaveWar(item2);
				}
			}
		}
		pKingdom.allianceLeave(this);
		if (pRecalc)
		{
			recalculate();
		}
	}

	public override void save()
	{
		base.save();
		data.kingdoms = new List<long>();
		foreach (Kingdom item in kingdoms_hashset)
		{
			data.kingdoms.Add(item.id);
		}
	}

	public override void loadData(AllianceData pData)
	{
		base.loadData(pData);
		foreach (long kingdom2 in data.kingdoms)
		{
			Kingdom kingdom = World.world.kingdoms.get(kingdom2);
			if (kingdom != null)
			{
				kingdoms_hashset.Add(kingdom);
			}
		}
		recalculate();
	}

	public int countBuildings()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countBuildings();
		}
		return num;
	}

	public int countZones()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countZones();
		}
		return num;
	}

	public override int countUnits()
	{
		return countPopulation();
	}

	public int countPopulation()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.getPopulationPeople();
		}
		return num;
	}

	public int countCities()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countCities();
		}
		return num;
	}

	public int countKingdoms()
	{
		return kingdoms_hashset.Count;
	}

	public string getMotto()
	{
		if (string.IsNullOrEmpty(data.motto))
		{
			data.motto = NameGenerator.getName("alliance_mottos");
		}
		return data.motto;
	}

	public int countWarriors()
	{
		int num = 0;
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom kingdom = list[i];
			num += kingdom.countTotalWarriors();
		}
		return num;
	}

	public static bool isSame(Alliance pAlliance1, Alliance pAlliance2)
	{
		if (pAlliance1 == null || pAlliance2 == null)
		{
			return false;
		}
		return pAlliance1 == pAlliance2;
	}

	public bool hasWarsWith(Kingdom pKingdom)
	{
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom pKingdom2 = list[i];
			if (pKingdom.isInWarWith(pKingdom2))
			{
				return true;
			}
		}
		return false;
	}

	public bool hasSupremeKingdom()
	{
		if (DiplomacyManager.kingdom_supreme == null)
		{
			return false;
		}
		return hasKingdom(DiplomacyManager.kingdom_supreme);
	}

	public bool hasKingdom(Kingdom pKingdom)
	{
		return kingdoms_hashset.Contains(pKingdom);
	}

	public bool hasSharedBordersWithKingdom(Kingdom pKingdom)
	{
		List<Kingdom> list = kingdoms_list;
		for (int i = 0; i < list.Count; i++)
		{
			Kingdom pTarget = list[i];
			if (DiplomacyHelpers.areKingdomsClose(pKingdom, pTarget))
			{
				return true;
			}
		}
		return false;
	}

	public bool hasWars()
	{
		return World.world.wars.hasWars(this);
	}

	public IEnumerable<War> getWars(bool pRandom = false)
	{
		return World.world.wars.getWars(this, pRandom);
	}

	public void mergeWars()
	{
		if (!hasWars())
		{
			return;
		}
		using ListPool<War> listPool = new ListPool<War>(getWars());
		for (int i = 0; i < listPool.Count; i++)
		{
			War war = listPool[i];
			if (war.hasEnded())
			{
				continue;
			}
			for (int j = i + 1; j < listPool.Count; j++)
			{
				War war2 = listPool[j];
				if (!war2.hasEnded() && war.isSameAs(war2))
				{
					if (war.data.created_time < war2.data.created_time)
					{
						World.world.wars.endWar(war2, WarWinner.Merged);
					}
					else
					{
						World.world.wars.endWar(war, WarWinner.Merged);
					}
					mergeWars();
					return;
				}
			}
		}
	}

	public IEnumerable<War> getAttackerWars()
	{
		foreach (War war in getWars())
		{
			foreach (Kingdom item in kingdoms_list)
			{
				if (war.isAttacker(item))
				{
					yield return war;
					break;
				}
			}
		}
	}

	public IEnumerable<War> getDefenderWars()
	{
		foreach (War war in getWars())
		{
			foreach (Kingdom item in kingdoms_list)
			{
				if (war.isDefender(item))
				{
					yield return war;
					break;
				}
			}
		}
	}

	public override IEnumerable<Actor> getUnits()
	{
		List<Kingdom> tKingdoms = kingdoms_list;
		for (int i = 0; i < tKingdoms.Count; i++)
		{
			Kingdom kingdom = tKingdoms[i];
			foreach (Actor unit in kingdom.getUnits())
			{
				yield return unit;
			}
		}
	}

	public override bool isReadyForRemoval()
	{
		return false;
	}

	public override Actor getRandomUnit()
	{
		return kingdoms_list.GetRandom().getRandomUnit();
	}

	public Sprite getBackgroundSprite()
	{
		return World.world.alliances.getBackgroundsList()[data.banner_background_id];
	}

	public Sprite getIconSprite()
	{
		return World.world.alliances.getIconsList()[data.banner_icon_id];
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "alliance");
		kingdoms_list.Clear();
		kingdoms_hashset.Clear();
		base.Dispose();
	}
}
