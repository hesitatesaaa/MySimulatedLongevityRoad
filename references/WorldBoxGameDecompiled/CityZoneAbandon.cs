using System.Collections.Generic;

public class CityZoneAbandon : CityZoneWorkerBase
{
	private List<ListPool<TileZone>> _split_areas = new List<ListPool<TileZone>>();

	private HashSetTileZone _zones_to_check = new HashSetTileZone();

	public void checkCities()
	{
		foreach (City city in World.world.cities)
		{
			city.checkAbandon();
		}
	}

	public void check(City pCity, bool pDebug = false, HashSet<TileZone> pSetToFill = null)
	{
		if (pCity.buildings.Count == 0)
		{
			return;
		}
		clearAll();
		prepareCityZones(pCity);
		startCheckingFromBuildings(pCity);
		_split_areas.Sort(sorter);
		if (pDebug)
		{
			return;
		}
		abandonLeftoverZones(pCity);
		if (_split_areas.Count >= 2)
		{
			_split_areas[0].Dispose();
			_split_areas.RemoveAt(0);
			if (_split_areas.Count > 0)
			{
				abandonSmallAreas(pCity);
			}
		}
	}

	private void startCheckingFromBuildings(City pCity)
	{
		for (int i = 0; i < pCity.buildings.Count; i++)
		{
			Building building = pCity.buildings[i];
			WorldTile current_tile = building.current_tile;
			if (!building.asset.docks)
			{
				startWaveFromTile(current_tile, pCity);
			}
		}
	}

	private void startWaveFromTile(WorldTile pTile, City pCity)
	{
		if (!_zones_to_check.Contains(pTile.zone))
		{
			return;
		}
		prepareWave();
		Queue<ZoneConnection> queue = _wave;
		Queue<ZoneConnection> queue2 = _next_wave;
		ListPool<TileZone> listPool = new ListPool<TileZone>(_wave.Count + _next_wave.Count);
		_split_areas.Add(listPool);
		queueConnection(new ZoneConnection(pTile.zone, pTile.region), queue, true);
		using ListPool<MapRegion> listPool2 = new ListPool<MapRegion>();
		int num = 0;
		while (queue2.Count > 0 || queue.Count > 0)
		{
			if (queue.Count == 0)
			{
				Queue<ZoneConnection> queue3 = queue;
				queue = queue2;
				queue2 = queue3;
				num++;
			}
			ZoneConnection zoneConnection = queue.Dequeue();
			TileZone zone = zoneConnection.zone;
			MapRegion region = zoneConnection.region;
			if (zone.isSameCityHere(pCity))
			{
				listPool.Add(zone);
			}
			TileZone[] neighbours = zone.neighbours;
			foreach (TileZone tileZone in neighbours)
			{
				if (!tileZone.isSameCityHere(pCity) || tileZone.tiles_with_ground == 0)
				{
					continue;
				}
				listPool2.Clear();
				if (!TileZone.hasZonesConnectedViaRegions(zone, tileZone, region, listPool2))
				{
					continue;
				}
				for (int j = 0; j < listPool2.Count; j++)
				{
					MapRegion pRegion = listPool2[j];
					ZoneConnection zoneConnection2 = new ZoneConnection(tileZone, pRegion);
					if (_zones_checked.Add(zoneConnection2))
					{
						queueConnection(zoneConnection2, queue2);
					}
				}
			}
		}
	}

	private void abandonLeftoverZones(City pCity)
	{
		if (_zones_to_check.Count == 0)
		{
			return;
		}
		foreach (TileZone item in _zones_to_check)
		{
			pCity.removeZone(item);
		}
	}

	private void abandonSmallAreas(City pCity)
	{
		for (int i = 0; i < _split_areas.Count; i++)
		{
			ListPool<TileZone> listPool = _split_areas[i];
			for (int j = 0; j < listPool.Count; j++)
			{
				TileZone pZone = listPool[j];
				pCity.removeZone(pZone);
			}
		}
	}

	private void prepareCityZones(City pCity)
	{
		_zones_to_check.UnionWith(pCity.zones);
	}

	internal override void clearAll()
	{
		base.clearAll();
		foreach (ListPool<TileZone> split_area in _split_areas)
		{
			split_area.Dispose();
		}
		_split_areas.Clear();
		_zones_to_check.Clear();
	}

	private static int sorter(ListPool<TileZone> pList1, ListPool<TileZone> pList2)
	{
		return pList2.Count.CompareTo(pList1.Count);
	}

	protected override void queueConnection(ZoneConnection pConnection, Queue<ZoneConnection> pWave, bool pCheck = false)
	{
		base.queueConnection(pConnection, pWave, pCheck);
		_zones_to_check.Remove(pConnection.zone);
	}
}
