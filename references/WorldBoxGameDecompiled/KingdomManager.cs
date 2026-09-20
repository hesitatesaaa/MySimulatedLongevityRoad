using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SQLite;
using UnityEngine;
using db;

public class KingdomManager : MetaSystemManager<Kingdom, KingdomData>
{
	private bool _dirty_cities = true;

	private bool _dirty_buildings = true;

	protected readonly Dictionary<long, DeadKingdom> _dead_kingdoms = new Dictionary<long, DeadKingdom>();

	public KingdomManager()
	{
		type_id = "kingdom";
	}

	public Kingdom makeNewCivKingdom(Actor pActor, string pID = null, bool pLog = true)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		World.world.game_stats.data.kingdomsCreated++;
		World.world.map_stats.kingdomsCreated++;
		Kingdom kingdom = newObject();
		kingdom.newCivKingdom(pActor);
		pActor.stopBeingWarrior();
		pActor.joinKingdom(kingdom);
		kingdom.setKing(pActor);
		kingdom.location = Vector2.op_Implicit(pActor.current_position);
		if (pLog)
		{
			WorldLog.logNewKingdom(kingdom);
		}
		return kingdom;
	}

	protected override void addObject(Kingdom pObject)
	{
		base.addObject(pObject);
		World.world.zone_calculator?.setDrawnZonesDirty();
		pObject.createAI();
	}

	public override void removeObject(Kingdom pKingdom)
	{
		World.world.game_stats.data.kingdomsDestroyed++;
		World.world.map_stats.kingdomsDestroyed++;
		WorldLog.logKingdomDestroyed(pKingdom);
		World.world.diplomacy.removeRelationsFor(pKingdom);
		if (World.world.isSelectedPower("relations") && SelectedMetas.selected_kingdom == pKingdom)
		{
			World.world.selected_buttons.unselectAll();
		}
		if (Config.whisper_A == pKingdom)
		{
			Config.whisper_A = null;
		}
		if (Config.unity_A == pKingdom)
		{
			Config.unity_A = null;
		}
		pKingdom.makeSurvivorsToNomads();
		World.world.zone_calculator.setDrawnZonesDirty();
		using ListPool<War> listPool = new ListPool<War>(pKingdom.getWars());
		foreach (ref War item in listPool)
		{
			item.removeFromWar(pKingdom, pInPeace: false);
		}
		pKingdom.getAlliance()?.leave(pKingdom);
		World.world.cultures.setDirtyKingdoms();
		World.world.languages.setDirtyKingdoms();
		World.world.religions.setDirtyKingdoms();
		base.removeObject(pKingdom);
		DBInserter.insertData((BaseSystemData)pKingdom.data, type_id);
	}

	public Kingdom getCivOrWildViaID(long pID)
	{
		if (pID < 0)
		{
			return World.world.kingdoms_wild.get(pID);
		}
		return World.world.kingdoms.get(pID);
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		using (IEnumerator<Kingdom> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.clearCursorOver();
			}
		}
		if (!World.world.isPaused())
		{
			updateCivKingdoms(pElapsed);
		}
	}

	private void updateCivKingdoms(float pElapsed)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].updateCiv(pElapsed);
		}
	}

	public void updateAge()
	{
		using IEnumerator<Kingdom> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.updateAge();
		}
	}

	[CanBeNull]
	public unsafe DeadKingdom db_get(long pID)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (Config.disable_db)
		{
			return null;
		}
		if (_dead_kingdoms.TryGetValue(pID, out var value))
		{
			return value;
		}
		SQLiteConnectionWithLock syncConnection = DBManager.getSyncConnection();
		LockWrapper val = syncConnection.Lock();
		try
		{
			KingdomData kingdomData = ((SQLiteConnection)syncConnection).Find<KingdomData>((object)pID);
			if (kingdomData == null)
			{
				return null;
			}
			kingdomData.from_db = true;
			value = new DeadKingdom();
			value.loadData(kingdomData);
			_dead_kingdoms[pID] = value;
			return value;
		}
		finally
		{
			((IDisposable)(*(LockWrapper*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override void clear()
	{
		foreach (DeadKingdom value in _dead_kingdoms.Values)
		{
			value.Dispose();
		}
		_dead_kingdoms.Clear();
		base.clear();
	}

	public override bool isLocked()
	{
		if (isUnitsDirty())
		{
			return true;
		}
		if (_dirty_cities)
		{
			return true;
		}
		return false;
	}

	protected override void updateDirtyUnits()
	{
		for (int i = 0; i < World.world.units.units_only_dying.Count; i++)
		{
			World.world.units.units_only_dying[i].kingdom.preserveAlive();
		}
		List<Actor> units_only_civ = World.world.units.units_only_civ;
		for (int j = 0; j < units_only_civ.Count; j++)
		{
			Actor actor = units_only_civ[j];
			if (actor.kingdom.isDirtyUnits())
			{
				actor.kingdom.listUnit(actor);
			}
		}
	}

	public void beginChecksCities()
	{
		if (_dirty_cities)
		{
			updateDirtyCities();
		}
		_dirty_cities = false;
	}

	public void updateDirtyCities()
	{
		clearAllCitiesLists();
		foreach (City city in World.world.cities)
		{
			city.kingdom.listCity(city);
		}
	}

	public void clearAllCitiesLists()
	{
		using (IEnumerator<Kingdom> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.clearListCities();
			}
		}
		WildKingdomsManager.neutral.clearListCities();
	}

	public bool hasDirtyCities()
	{
		return _dirty_cities;
	}

	public void setDirtyCities()
	{
		_dirty_cities = true;
	}

	public void beginChecksBuildings()
	{
		if (_dirty_buildings)
		{
			updateDirtyBuildings();
		}
		_dirty_buildings = false;
	}

	private void updateDirtyBuildings()
	{
		clearAllBuildingLists();
		foreach (City city in World.world.cities)
		{
			if (!city.kingdom.wild)
			{
				city.kingdom.addBuildings(city.buildings);
			}
		}
	}

	public void setDirtyBuildings()
	{
		_dirty_buildings = true;
	}

	private void clearAllBuildingLists()
	{
		using IEnumerator<Kingdom> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearBuildingList();
		}
	}
}
