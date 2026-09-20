using System.Collections.Generic;

public class CityZoneGrowth : CityZoneWorkerBase
{
	private const float MOD_RADIUS = 0.75f;

	public TileZone getZoneToClaim(Actor pActor, City pCity, bool pDebug = false, HashSet<TileZone> pSetToFill = null, int pBonusRange = 0)
	{
		clearAll();
		WorldTile tile = pCity.getTile();
		if (tile == null)
		{
			return null;
		}
		bool pStopWaveWhenEmptyZoneFound = !pDebug;
		startWaveFromTile(pActor, tile, pCity, pStopWaveWhenEmptyZoneFound, pBonusRange);
		if (pDebug && pSetToFill != null)
		{
			foreach (ZoneConnection item in _zones_checked)
			{
				pSetToFill.Add(item.zone);
			}
			return null;
		}
		return checkGrowBorder(pCity);
	}

	private TileZone checkGrowBorder(City pCity)
	{
		bool num = Randy.randomChance(0.7f);
		TileZone result = null;
		if (num)
		{
			TileZone randomZone = getRandomZone(pCity);
			if (randomZone != null)
			{
				result = randomZone;
			}
		}
		else
		{
			result = getRandomCheckedZone(pCity);
		}
		return result;
	}

	private TileZone getRandomZone(City pCity)
	{
		using ListPool<TileZone> list = new ListPool<TileZone>(pCity.border_zones);
		WorldTile tile = pCity.getTile();
		if (tile == null)
		{
			return null;
		}
		TileZone zone = tile.zone;
		float num = (float)pCity.getZoneRange() * 0.75f;
		num *= num;
		foreach (TileZone item in list.LoopRandom())
		{
			foreach (TileZone item2 in item.neighbours.LoopRandom())
			{
				if (item2.canBeClaimedByCity(pCity) && item2.centerTile.isSameIsland(tile) && !((float)Toolbox.SquaredDist(zone.x, zone.y, item2.x, item2.y) > num))
				{
					return item2;
				}
			}
		}
		return null;
	}

	private TileZone getBestZoneFromList(City pCity, List<TileZone> pList)
	{
		TileZone result = null;
		TileZone zone = pCity.getTile().zone;
		int num = int.MaxValue;
		for (int i = 0; i < pList.Count; i++)
		{
			TileZone tileZone = pList[i];
			int num2 = Toolbox.SquaredDist(tileZone.x, tileZone.y, zone.x, zone.y);
			if (num2 < num)
			{
				result = tileZone;
				num = num2;
			}
		}
		return result;
	}

	private TileZone getRandomCheckedZone(City pCity)
	{
		using ListPool<TileZone> listPool = new ListPool<TileZone>(_zones_checked.Count);
		foreach (ZoneConnection item in _zones_checked)
		{
			TileZone zone = item.zone;
			if (zone.canBeClaimedByCity(pCity))
			{
				listPool.Add(zone);
			}
		}
		if (listPool.Count > 0)
		{
			return listPool.GetRandom();
		}
		return null;
	}

	private void startWaveFromTile(Actor pActor, WorldTile pTile, City pCity, bool pStopWaveWhenEmptyZoneFound = true, int pBonusRange = 0)
	{
		prepareWave();
		if (pActor == null)
		{
			pActor = pCity.leader;
		}
		Queue<ZoneConnection> queue = _wave;
		Queue<ZoneConnection> queue2 = _next_wave;
		queueConnection(new ZoneConnection(pTile.zone, pTile.region), queue, pSetChecked: true);
		using ListPool<MapRegion> listPool = new ListPool<MapRegion>();
		int num = pCity.getZoneRange() + pBonusRange;
		float num2 = (float)num * 0.75f;
		num2 *= num2;
		int num3 = 0;
		bool flag = false;
		while ((queue2.Count > 0 || queue.Count > 0) && !(pStopWaveWhenEmptyZoneFound & flag))
		{
			if (queue.Count == 0)
			{
				Queue<ZoneConnection> queue3 = queue;
				queue = queue2;
				queue2 = queue3;
				num3++;
				if (num3 > num)
				{
					break;
				}
			}
			ZoneConnection zoneConnection = queue.Dequeue();
			TileZone zone = zoneConnection.zone;
			MapRegion region = zoneConnection.region;
			for (int i = 0; i < zone.neighbours.Length; i++)
			{
				TileZone tileZone = zone.neighbours[i];
				if ((pStopWaveWhenEmptyZoneFound && tileZone.hasCity() && !tileZone.isSameCityHere(pCity)) || tileZone.tiles_with_ground == 0 || (pActor != null && pActor.hasSubspecies() && !tileZone.checkCanSettleInThisBiomes(pActor.subspecies)))
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
					if (!_zones_checked.Add(zoneConnection2))
					{
						continue;
					}
					if (tileZone.canBeClaimedByCity(pCity))
					{
						flag = true;
					}
					if (!((float)Toolbox.SquaredDist(pTile.zone.x, pTile.zone.y, tileZone.x, tileZone.y) > num2))
					{
						if (pStopWaveWhenEmptyZoneFound & flag)
						{
							break;
						}
						queueConnection(zoneConnection2, queue2);
					}
				}
			}
		}
	}
}
