using System.Collections.Generic;

public static class ZoneMetaDataVisualizer
{
	public const float FADE_TIME = 5f;

	public static readonly Dictionary<TileZone, ZoneMetaData> zone_data_dict = new Dictionary<TileZone, ZoneMetaData>();

	private static readonly List<TileZone> _to_remove = new List<TileZone>();

	private static MetaType _last_meta_type = MetaType.None;

	public static bool hasZoneData(TileZone pZone)
	{
		return zone_data_dict.ContainsKey(pZone);
	}

	public static ZoneMetaData getZoneMetaData(TileZone pZone)
	{
		zone_data_dict.TryGetValue(pZone, out var value);
		return value;
	}

	public static ListPool<TileZone> getZonesWithMeta(IMetaObject pMeta)
	{
		ListPool<TileZone> listPool = new ListPool<TileZone>();
		foreach (ZoneMetaData value in zone_data_dict.Values)
		{
			if (value.meta_object == pMeta)
			{
				listPool.Add(value.zone);
			}
		}
		return listPool;
	}

	private static bool shouldUpdateEntry(ZoneMetaData pData, IMetaObject pNewMetaObject)
	{
		IMetaObject meta_object = pData.meta_object;
		if (meta_object == null)
		{
			return true;
		}
		if (meta_object.getMetaTypeAsset().map_mode != pNewMetaObject.getMetaTypeAsset().map_mode)
		{
			return true;
		}
		if (pData.previous_priority_amount < pNewMetaObject.countUnits())
		{
			return true;
		}
		if (meta_object == pNewMetaObject)
		{
			return true;
		}
		return false;
	}

	public static void countMetaZone(TileZone pZone, IMetaObject pMetaObject, double pTimestamp)
	{
		if (zone_data_dict.TryGetValue(pZone, out var value))
		{
			if (shouldUpdateEntry(value, pMetaObject))
			{
				value.meta_object = pMetaObject;
				value.timestamp = pTimestamp;
				value.previous_priority_amount = pMetaObject.countUnits();
				zone_data_dict[pZone] = value;
			}
		}
		else
		{
			value = new ZoneMetaData
			{
				meta_object = pMetaObject,
				zone = pZone,
				timestamp = pTimestamp,
				timestamp_new = pTimestamp,
				previous_priority_amount = pMetaObject.countUnits()
			};
			zone_data_dict.Add(pZone, value);
		}
	}

	private static void start()
	{
		_to_remove.Clear();
	}

	private static void checkDynamicZones()
	{
		MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
		if (cachedMapMetaAsset != null && cachedMapMetaAsset.map_mode != _last_meta_type)
		{
			clearAll();
			_last_meta_type = cachedMapMetaAsset.map_mode;
		}
		if (cachedMapMetaAsset != null && cachedMapMetaAsset.has_dynamic_zones && cachedMapMetaAsset.isMetaZoneOptionSelectedFluid())
		{
			cachedMapMetaAsset.dynamic_zones();
		}
	}

	private static void clearOldAndDeadZones()
	{
		double curWorldTime = World.world.getCurWorldTime();
		List<TileZone> to_remove = _to_remove;
		foreach (KeyValuePair<TileZone, ZoneMetaData> item in zone_data_dict)
		{
			ZoneMetaData value = item.Value;
			if (value.meta_object == null || !value.meta_object.isAlive())
			{
				to_remove.Add(item.Key);
			}
			else if (value.getDiffTime(curWorldTime) > 5f)
			{
				to_remove.Add(item.Key);
			}
		}
		foreach (TileZone item2 in to_remove)
		{
			zone_data_dict.Remove(item2);
		}
		_to_remove.Clear();
	}

	public static void updateMetaZones()
	{
		Bench.bench("fluid_zones_data", "fluid_zones_data_total");
		Bench.bench("start", "fluid_zones_data");
		start();
		Bench.benchEnd("start", "fluid_zones_data", pSaveCounter: false, 0L);
		Bench.bench("checkDynamicZones", "fluid_zones_data");
		checkDynamicZones();
		Bench.benchEnd("checkDynamicZones", "fluid_zones_data", pSaveCounter: false, 0L);
		Bench.bench("clearOldAndDeadZones", "fluid_zones_data");
		clearOldAndDeadZones();
		Bench.benchEnd("clearOldAndDeadZones", "fluid_zones_data", pSaveCounter: false, 0L);
		Bench.bench("checkCenterTitles", "fluid_zones_data");
		checkCenterTitles();
		Bench.benchEnd("checkCenterTitles", "fluid_zones_data", pSaveCounter: false, 0L);
		Bench.benchEnd("fluid_zones_data", "fluid_zones_data_total", pSaveCounter: false, 0L);
	}

	private static void checkCenterTitles()
	{
		foreach (Culture culture in World.world.cultures)
		{
			culture.updateTitleCenter();
		}
	}

	public static void clearAll()
	{
		zone_data_dict.Clear();
	}
}
