using System.Collections.Generic;
using UnityEngine;

public class DiplomacyManager : CoreSystemManager<DiplomacyRelation, DiplomacyRelationData>
{
	public static Kingdom kingdom_supreme;

	public static Kingdom kingdom_second;

	public static List<Kingdom> superpowers = new List<Kingdom>();

	private float diplomacyTick;

	private static List<Kingdom> _kingdom_sorter = new List<Kingdom>();

	private static List<DiplomacyRelation> _relations_remover = new List<DiplomacyRelation>();

	protected readonly Dictionary<string, DiplomacyRelation> _dict = new Dictionary<string, DiplomacyRelation>();

	public DiplomacyManager()
	{
		type_id = "diplomacy";
	}

	public override List<DiplomacyRelationData> save(List<DiplomacyRelation> pList = null)
	{
		List<DiplomacyRelationData> list = new List<DiplomacyRelationData>();
		using IEnumerator<DiplomacyRelation> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			DiplomacyRelation current = enumerator.Current;
			current.kingdom1 = World.world.kingdoms.get(current.data.kingdom1_id);
			current.kingdom2 = World.world.kingdoms.get(current.data.kingdom2_id);
			if (current.kingdom1 != null && current.kingdom2 != null)
			{
				list.Add(current.data);
			}
		}
		return list;
	}

	public override void loadFromSave(List<DiplomacyRelationData> pList)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			DiplomacyRelationData diplomacyRelationData = pList[i];
			Kingdom kingdom = World.world.kingdoms.get(diplomacyRelationData.kingdom1_id);
			Kingdom kingdom2 = World.world.kingdoms.get(diplomacyRelationData.kingdom2_id);
			if (kingdom != null && kingdom2 != null)
			{
				if (diplomacyRelationData.id == -1)
				{
					diplomacyRelationData.id = World.world.map_stats.getNextId(type_id);
				}
				loadObject(diplomacyRelationData);
			}
		}
	}

	public override DiplomacyRelation loadObject(DiplomacyRelationData pData)
	{
		Kingdom kingdom = World.world.kingdoms.get(pData.kingdom1_id);
		Kingdom kingdom2 = World.world.kingdoms.get(pData.kingdom2_id);
		pData.rel_id = pData.kingdom1_id + "_" + pData.kingdom2_id;
		DiplomacyRelation diplomacyRelation = base.loadObject(pData);
		_dict.Add(pData.rel_id, diplomacyRelation);
		diplomacyRelation.kingdom1 = kingdom;
		diplomacyRelation.kingdom2 = kingdom2;
		return diplomacyRelation;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (!World.world.isPaused())
		{
			if (diplomacyTick > 0f)
			{
				diplomacyTick -= pElapsed;
			}
			else if (!World.world.cities.isLocked())
			{
				diplomacyTick = 2f;
				newDiplomacyTick();
			}
		}
	}

	public void newDiplomacyTick()
	{
		findSupremeKingdom();
		checkAchievements();
	}

	private void checkAchievements()
	{
		AchievementLibrary.world_war.check();
	}

	private void findSupremeKingdom()
	{
		kingdom_supreme = null;
		kingdom_second = null;
		if (World.world.kingdoms.Count != 0)
		{
			List<Kingdom> kingdom_sorter = _kingdom_sorter;
			kingdom_sorter.AddRange(World.world.kingdoms);
			for (int i = 0; i < kingdom_sorter.Count; i++)
			{
				Kingdom kingdom = kingdom_sorter[i];
				kingdom.power = kingdom.countTotalWarriors() * 2 + kingdom.countCities() * 5 + 1;
			}
			kingdom_sorter.Sort(sortByPower);
			kingdom_supreme = kingdom_sorter[0];
			if (kingdom_sorter.Count > 1)
			{
				kingdom_second = kingdom_sorter[1];
			}
			else
			{
				kingdom_second = null;
			}
			kingdom_sorter.Clear();
		}
	}

	public int sortByPower(Kingdom o1, Kingdom o2)
	{
		return o2.power.CompareTo(o1.power);
	}

	private War startTotalWar(Kingdom pAttacker, WarTypeAsset pType)
	{
		if (World.world.kingdoms.Count == 1)
		{
			return null;
		}
		foreach (War war in pAttacker.getWars())
		{
			if (war.isMainAttacker(pAttacker) && war.isTotalWar())
			{
				return null;
			}
		}
		if (pAttacker.hasAlliance())
		{
			pAttacker.getAlliance().leave(pAttacker);
		}
		War result = World.world.wars.newWar(pAttacker, null, pType);
		using ListPool<War> listPool = new ListPool<War>(pAttacker.getWars());
		foreach (ref War item in listPool)
		{
			War current2 = item;
			if (current2.isTotalWar())
			{
				continue;
			}
			if (current2.isAttacker(pAttacker))
			{
				if (!current2.isMainAttacker(pAttacker))
				{
					current2.leaveWar(pAttacker);
				}
				else
				{
					World.world.wars.endWar(current2, WarWinner.Merged);
				}
			}
			else if (current2.isDefender(pAttacker))
			{
				if (!current2.isMainDefender(pAttacker))
				{
					current2.leaveWar(pAttacker);
				}
				else
				{
					current2.lostWar(pAttacker);
				}
			}
		}
		WorldLog.logNewTotalWar(pAttacker);
		return result;
	}

	internal War startWar(Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pAsset, bool pLog = true)
	{
		if (pAsset.total_war)
		{
			return startTotalWar(pAttacker, pAsset);
		}
		if (pAttacker == pDefender)
		{
			return null;
		}
		if (World.world.wars.getWar(pAttacker, pDefender) != null)
		{
			return null;
		}
		if (pLog)
		{
			WorldLog.logNewWar(pAttacker, pDefender);
		}
		War war = World.world.wars.newWar(pAttacker, pDefender, pAsset);
		if (pAsset.alliance_join)
		{
			Alliance alliance = pAttacker.getAlliance();
			Alliance alliance2 = pDefender.getAlliance();
			if (alliance != null)
			{
				foreach (Kingdom item in alliance.kingdoms_hashset)
				{
					war.joinAttackers(item);
				}
			}
			if (alliance2 != null)
			{
				foreach (Kingdom item2 in alliance2.kingdoms_hashset)
				{
					war.joinDefenders(item2);
				}
			}
		}
		return war;
	}

	public void eventSpite(Kingdom pKingdom)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.kingdoms.Count <= 1)
		{
			return;
		}
		using ListPool<Kingdom> listPool = new ListPool<Kingdom>(World.world.kingdoms.Count);
		War war = startWar(pKingdom, null, WarTypeLibrary.spite);
		if (war == null)
		{
			return;
		}
		pKingdom.affectKingByPowers();
		listPool.AddRange(war.getAttackers());
		listPool.AddRange(war.getDefenders());
		foreach (ref Kingdom item in listPool)
		{
			EffectsLibrary.highlightKingdomZones(item, Color.red);
		}
	}

	public void eventFriendship(Kingdom pKingdom)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		War randomWarFor = World.world.wars.getRandomWarFor(pKingdom);
		if (randomWarFor == null)
		{
			return;
		}
		using ListPool<Kingdom> listPool = new ListPool<Kingdom>(World.world.kingdoms.Count);
		if (randomWarFor.isTotalWar() || randomWarFor.isMainAttacker(pKingdom) || randomWarFor.isMainDefender(pKingdom))
		{
			listPool.AddRange(randomWarFor.getAttackers());
			listPool.AddRange(randomWarFor.getDefenders());
		}
		Alliance alliance = pKingdom.getAlliance();
		if (alliance == null)
		{
			randomWarFor.leaveWar(pKingdom);
			listPool.Add(pKingdom);
			pKingdom.affectKingByPowers();
		}
		else
		{
			foreach (Kingdom item in alliance.kingdoms_hashset)
			{
				randomWarFor.leaveWar(item);
				listPool.Add(item);
				item.affectKingByPowers();
			}
		}
		if (randomWarFor.isTotalWar())
		{
			World.world.wars.endWar(randomWarFor, WarWinner.Peace);
		}
		foreach (ref Kingdom item2 in listPool)
		{
			EffectsLibrary.highlightKingdomZones(item2, Color.green);
		}
	}

	public KingdomOpinion getOpinion(Kingdom k1, Kingdom k2)
	{
		return getRelation(k1, k2).getOpinion(k1, k2);
	}

	public int sortID(Kingdom o1, Kingdom o2)
	{
		return o1.id.CompareTo(o2.id);
	}

	public DiplomacyRelation getRelation(Kingdom pK1, Kingdom pK2)
	{
		Kingdom kingdom;
		Kingdom kingdom2;
		if (pK1.id.CompareTo(pK2.id) > 0)
		{
			kingdom = pK1;
			kingdom2 = pK2;
		}
		else
		{
			kingdom = pK2;
			kingdom2 = pK1;
		}
		string text = kingdom.id + "_" + kingdom2.id;
		if (tryGet(text, out var pObject))
		{
			return pObject;
		}
		pObject = newObject();
		pObject.data.rel_id = text;
		_dict.Add(text, pObject);
		pObject.data.kingdom1_id = kingdom.id;
		pObject.data.kingdom2_id = kingdom2.id;
		pObject.kingdom1 = kingdom;
		pObject.kingdom2 = kingdom2;
		return pObject;
	}

	public void removeRelationsFor(Kingdom pKingdom)
	{
		using (IEnumerator<DiplomacyRelation> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				DiplomacyRelation current = enumerator.Current;
				if (current.kingdom1 == pKingdom || current.kingdom2 == pKingdom)
				{
					_relations_remover.Add(current);
				}
			}
		}
		foreach (DiplomacyRelation item in _relations_remover)
		{
			removeObject(item);
		}
		_relations_remover.Clear();
	}

	public bool tryGet(string pID, out DiplomacyRelation pObject)
	{
		return _dict.TryGetValue(pID, out pObject);
	}

	public DiplomacyRelation get(string pID)
	{
		if (string.IsNullOrEmpty(pID))
		{
			return null;
		}
		tryGet(pID, out var pObject);
		return pObject;
	}

	public override void removeObject(DiplomacyRelation pObject)
	{
		_dict.Remove(pObject.data.rel_id);
		base.removeObject(pObject);
	}

	public override void clear()
	{
		diplomacyTick = 0f;
		kingdom_supreme = null;
		kingdom_second = null;
		_dict.Clear();
		base.clear();
	}
}
