using System.Collections.Generic;

public class CityManager : MetaSystemManager<City, CityData>
{
	private bool _dirty_buildings;

	public CityManager()
	{
		type_id = "city";
	}

	public City newCity(Kingdom pKingdom, TileZone pZone, Actor pOriginalActor)
	{
		World.world.game_stats.data.citiesCreated++;
		World.world.map_stats.citiesCreated++;
		City city = newObject();
		city.data.founder_id = pOriginalActor.getID();
		city.data.founder_name = pOriginalActor.name;
		city.data.original_actor_asset = pOriginalActor.asset.id;
		city.data.equipment = new CityEquipment();
		city.setKingdom(pKingdom);
		city.addZone(pZone);
		TileZone[] neighbours_all = pZone.neighbours_all;
		foreach (TileZone tileZone in neighbours_all)
		{
			if (tileZone.city == null)
			{
				city.addZone(tileZone);
			}
		}
		World.world.city_zone_helper.city_place_finder.setDirty();
		return city;
	}

	public City buildNewCity(Actor pActor, TileZone pZone)
	{
		Kingdom kingdom = pActor.kingdom;
		City city = World.world.cities.newCity(kingdom, pZone, pActor);
		city.setUnitMetas(pActor);
		city.newCityEvent(pActor);
		WorldLog.logNewCity(city);
		return city;
	}

	public bool tryToCreateCity(Actor pActor, ListPool<Building> pBuildingList)
	{
		if (pActor.current_tile.zone.hasCity())
		{
			return false;
		}
		return true;
	}

	public bool canStartNewCityCivilizationHere(Actor pActor)
	{
		if (pActor.kingdom.asset.is_forced_by_trait)
		{
			return false;
		}
		if (!pActor.canBuildNewCity())
		{
			return false;
		}
		KingdomAsset kingdomAsset = AssetManager.kingdoms.get(pActor.asset.kingdom_id_civilization);
		if (kingdomAsset == null || !kingdomAsset.civ)
		{
			return false;
		}
		WorldTile current_tile = pActor.current_tile;
		TileZone zone = current_tile.zone;
		TileZone[] neighbours = zone.neighbours;
		foreach (TileZone tileZone in neighbours)
		{
			if (tileZone.hasCity())
			{
				WorldTile tile = tileZone.city.getTile();
				if (tile != null && tile.isSameIsland(current_tile))
				{
					tileZone.city.addZone(zone);
					return false;
				}
			}
		}
		return true;
	}

	public City buildFirstCivilizationCity(Actor pActor)
	{
		City city = buildNewCity(pActor, pActor.current_zone);
		pActor.joinCity(city);
		city.setUnitMetas(pActor);
		city.convertSameSpeciesAroundUnit(pActor);
		return city;
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			City city = actor.city;
			if (city != null && city.isDirtyUnits())
			{
				city.listUnit(actor);
			}
		}
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
		using IEnumerator<City> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			City current = enumerator.Current;
			Kingdom kingdom = current.kingdom;
			for (int i = 0; i < current.zones.Count; i++)
			{
				foreach (Building item in current.zones[i].buildings_all)
				{
					if (item.asset.city_building && item.isUsable())
					{
						item.setKingdomCiv(kingdom);
						current.listBuilding(item);
					}
				}
			}
		}
	}

	public void setDirtyBuildings(City pCity)
	{
		_dirty_buildings = true;
		World.world.kingdoms.setDirtyBuildings();
	}

	private void clearAllBuildingLists()
	{
		using IEnumerator<City> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearBuildingList();
		}
	}

	protected override void addObject(City pObject)
	{
		pObject.init();
		base.addObject(pObject);
	}

	public override City loadObject(CityData pData)
	{
		City city = base.loadObject(pData);
		city.loadCity(pData);
		return city;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		using IEnumerator<City> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			City current = enumerator.Current;
			current.update(pElapsed);
			current.clearCursorOver();
		}
	}

	public void updateAge()
	{
		using IEnumerator<City> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.updateAge();
		}
	}

	public override List<CityData> save(List<City> pList = null)
	{
		List<CityData> list = new List<CityData>();
		using IEnumerator<City> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			City current = enumerator.Current;
			current.save();
			list.Add(current.data);
		}
		return list;
	}

	private void checkForCityErrors(SavedMap pSaveData)
	{
		List<CityData> list = new List<CityData>();
		for (int i = 0; i < pSaveData.cities.Count; i++)
		{
			CityData cityData = pSaveData.cities[i];
			if (cityData.zones.Count != 0)
			{
				TileZone tileZone = World.world.zone_calculator.getZone(cityData.zones[0].x, cityData.zones[0].y);
				if (pSaveData.saveVersion < 7)
				{
					tileZone = findZoneViaBuilding(cityData.id, pSaveData.buildings);
				}
				if (tileZone != null)
				{
					list.Add(cityData);
				}
			}
		}
		pSaveData.cities = list;
	}

	public void loadCities(SavedMap pSaveData)
	{
		checkForCityErrors(pSaveData);
		for (int i = 0; i < pSaveData.cities.Count; i++)
		{
			CityData cityData = pSaveData.cities[i];
			City city = loadObject(cityData);
			if (city == null || pSaveData.saveVersion < 7)
			{
				continue;
			}
			for (int j = 0; j < cityData.zones.Count; j++)
			{
				ZoneData zoneData = cityData.zones[j];
				TileZone zone = World.world.zone_calculator.getZone(zoneData.x, zoneData.y);
				if (zone != null)
				{
					city.addZone(zone);
				}
			}
		}
	}

	public override void removeObject(City pObject)
	{
		World.world.game_stats.data.citiesDestroyed++;
		World.world.map_stats.citiesDestroyed++;
		WorldLog.logCityDestroyed(pObject);
		pObject.destroyCity();
		base.removeObject(pObject);
		World.world.city_zone_helper.city_place_finder.setDirty();
		World.world.cultures.setDirtyCities();
		World.world.kingdoms.setDirtyCities();
		World.world.languages.setDirtyCities();
		World.world.religions.setDirtyCities();
	}

	private TileZone findZoneViaBuilding(long pID, List<BuildingData> pList)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			BuildingData buildingData = pList[i];
			if (buildingData.cityID == pID)
			{
				return World.world.GetTileSimple(buildingData.mainX, buildingData.mainY).zone;
			}
		}
		return null;
	}

	public override bool isLocked()
	{
		if (isUnitsDirty())
		{
			return true;
		}
		if (World.world.kingdoms.hasDirtyCities())
		{
			return true;
		}
		return false;
	}
}
