using System.Collections.Generic;

public class CityPlaceFinder : CityZoneWorkerBase
{
	private bool _dirty;

	internal List<TileZone> zones = new List<TileZone>();

	internal bool isDirty()
	{
		if (!DebugConfig.isOn(DebugOption.SystemCityPlaceFinder))
		{
			return false;
		}
		return _dirty;
	}

	internal void recalc()
	{
		if (isDirty())
		{
			_dirty = false;
			clearAll();
			prepareBasicZones();
			prepareQueueFromCities();
			startWave();
			createFinalList();
		}
	}

	internal override void clearAll()
	{
		base.clearAll();
		zones.Clear();
		List<TileZone> list = World.world.zone_calculator.zones;
		for (int i = 0; i < list.Count; i++)
		{
			list[i].setGoodForNewCity(pValue: true);
		}
	}

	private void prepareBasicZones()
	{
		List<TileZone> list = World.world.zone_calculator.zones;
		for (int i = 0; i < list.Count; i++)
		{
			TileZone tileZone = list[i];
			if (!tileZone.canStartCityHere())
			{
				tileZone.setGoodForNewCity(pValue: false);
			}
			else if (tileZone.centerTile.region.island.getTileCount() < 300)
			{
				tileZone.setGoodForNewCity(pValue: false);
			}
		}
	}

	private void prepareQueueFromCities()
	{
		prepareWave();
		foreach (City city in World.world.cities)
		{
			checkCity(city, _wave);
		}
	}

	private void checkCity(City pCity, Queue<ZoneConnection> pWaveQ)
	{
		WorldTile tile = pCity.getTile();
		if (tile == null)
		{
			return;
		}
		TileIsland island = tile.region.island;
		foreach (TileZone border_zone in pCity.border_zones)
		{
			for (int i = 0; i < border_zone.centerTile.chunk.regions.Count; i++)
			{
				MapRegion mapRegion = border_zone.chunk.regions[i];
				if (mapRegion.isTypeGround() && mapRegion.zones.Contains(border_zone) && mapRegion.island == island)
				{
					queueConnection(new ZoneConnection(border_zone, mapRegion), pWaveQ, pSetChecked: true);
				}
			}
		}
	}

	private void startWave()
	{
		Queue<ZoneConnection> queue = _wave;
		Queue<ZoneConnection> queue2 = _next_wave;
		using ListPool<MapRegion> listPool = new ListPool<MapRegion>();
		int num = 3;
		int num2 = 0;
		while (queue2.Count > 0 || queue.Count > 0)
		{
			if (queue.Count == 0)
			{
				Queue<ZoneConnection> queue3 = queue;
				queue = queue2;
				queue2 = queue3;
				num2++;
			}
			if (num2 > num)
			{
				break;
			}
			ZoneConnection zoneConnection = queue.Dequeue();
			TileZone zone = zoneConnection.zone;
			MapRegion region = zoneConnection.region;
			TileZone[] neighbours = zone.neighbours;
			foreach (TileZone tileZone in neighbours)
			{
				if (tileZone.hasCity())
				{
					continue;
				}
				listPool.Clear();
				if (!TileZone.hasZonesConnectedViaRegions(zone, tileZone, region, listPool))
				{
					continue;
				}
				for (int j = 0; j < listPool.Count; j++)
				{
					MapRegion pRegion = listPool[j];
					ZoneConnection zoneConnection2 = new ZoneConnection(tileZone, pRegion);
					if (_zones_checked.Add(zoneConnection2))
					{
						tileZone.setGoodForNewCity(pValue: false);
						queueConnection(zoneConnection2, queue2);
					}
				}
			}
		}
	}

	private void createFinalList()
	{
		for (int i = 0; i < World.world.zone_calculator.zones.Count; i++)
		{
			TileZone tileZone = World.world.zone_calculator.zones[i];
			if (tileZone.isGoodForNewCity())
			{
				zones.Add(tileZone);
			}
		}
	}

	public bool hasPossibleZones()
	{
		if (_dirty)
		{
			return false;
		}
		return zones.Count > 0;
	}

	internal void setDirty()
	{
		_dirty = true;
		clearCurrentZones();
	}

	private void clearCurrentZones()
	{
		if (zones.Count != 0)
		{
			for (int i = 0; i < zones.Count; i++)
			{
				zones[i].setGoodForNewCity(pValue: false);
			}
			zones.Clear();
		}
	}
}
