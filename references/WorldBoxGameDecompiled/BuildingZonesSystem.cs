using System.Collections.Generic;

public class BuildingZonesSystem
{
	private static bool _dirty;

	public static void setDirty()
	{
		_dirty = true;
	}

	public static void update()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		List<TileZone> zones = World.world.zone_calculator.zones;
		using ListPool<TileZone> listPool = new ListPool<TileZone>();
		for (int i = 0; i < zones.Count; i++)
		{
			TileZone tileZone = zones[i];
			if (tileZone.isDirty())
			{
				listPool.Add(tileZone);
			}
		}
		for (int j = 0; j < listPool.Count; j++)
		{
			TileZone tileZone2 = listPool[j];
			tileZone2.clearBuildingLists();
			tileZone2.setDirty(pValue: false);
			foreach (Building item in tileZone2.buildings_all)
			{
				if (item.isOnRemove() || item.isRemoved())
				{
					continue;
				}
				if (item.current_tile.zone == tileZone2)
				{
					tileZone2.buildings_render_list.Add(item);
				}
				tileZone2.addBuildingToSet(item);
				if (item.asset.city_building && !tileZone2.hasCity())
				{
					if (item.isCiv())
					{
						item.makeAbandoned();
					}
					else
					{
						item.makeAbandoned();
					}
				}
			}
		}
	}
}
