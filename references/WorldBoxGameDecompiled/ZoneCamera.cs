using System.Collections.Generic;
using UnityEngine;

public class ZoneCamera
{
	private readonly List<TileZone> _visible_zones = new List<TileZone>();

	private readonly HashSet<MapChunk> _set_visible_chunks = new HashSet<MapChunk>();

	private readonly List<MapChunk> _list_visible_chunks = new List<MapChunk>();

	private readonly ZoneCalculator _zone_manager;

	private int _last_start_x = -1;

	private int _last_start_y = -1;

	private int _last_width = -1;

	private int _last_height = -1;

	private int _last_main_x = -1;

	public ZoneCamera()
	{
		_zone_manager = World.world.zone_calculator;
	}

	private void calculateBounds(out int pResultStartX, out int pResultStartY, out int pResultWidth, out int pResultHeight, out int pResultMainX)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		num++;
		num3++;
		TileZone zoneWithinCamera = getZoneWithinCamera(0, 0);
		TileZone zoneWithinCamera2 = getZoneWithinCamera(1, 0);
		TileZone zoneWithinCamera3 = getZoneWithinCamera(0, 1);
		TileZone zoneWithinCamera4 = getZoneWithinCamera(1, 1);
		int num4 = ((zoneWithinCamera3.x <= zoneWithinCamera.x) ? zoneWithinCamera.x : zoneWithinCamera3.x);
		int num5 = ((zoneWithinCamera2.y <= zoneWithinCamera.y) ? zoneWithinCamera.y : zoneWithinCamera2.y);
		num4 -= num;
		num5 -= num2;
		int num6 = zoneWithinCamera4.x - num4 + num;
		int num7 = zoneWithinCamera4.y - num5 + num3;
		if (zoneWithinCamera2.x < zoneWithinCamera4.x)
		{
			num6 = zoneWithinCamera2.x - num4;
		}
		if (zoneWithinCamera3.y < zoneWithinCamera4.y)
		{
			num7 = zoneWithinCamera3.y - num5;
		}
		int num8 = num4;
		if (num8 > 0)
		{
			num8++;
		}
		if (num4 < 0)
		{
			num4 = 0;
		}
		if (num5 < 0)
		{
			num5 = 0;
		}
		if (num6 > _zone_manager.zones_total_x)
		{
			num6 = _zone_manager.zones_total_x;
		}
		if (num7 > _zone_manager.zones_total_y)
		{
			num7 = _zone_manager.zones_total_y;
		}
		pResultStartX = num4;
		pResultStartY = num5;
		pResultWidth = num6;
		pResultHeight = num7;
		pResultMainX = num8;
	}

	internal void update()
	{
		Bench.bench("zone_camera", "zone_camera_total");
		Bench.bench("calc_bounds", "zone_camera");
		calculateBounds(out var pResultStartX, out var pResultStartY, out var pResultWidth, out var pResultHeight, out var pResultMainX);
		Bench.benchEnd("calc_bounds", "zone_camera", pSaveCounter: false, 0L);
		if (pResultStartX != _last_start_x || pResultStartY != _last_start_y || pResultWidth != _last_width || pResultHeight != _last_height || pResultMainX != _last_main_x)
		{
			_last_start_x = pResultStartX;
			_last_start_y = pResultStartY;
			_last_width = pResultWidth;
			_last_height = pResultHeight;
			_last_main_x = pResultMainX;
			Bench.bench("clear", "zone_camera");
			clear();
			Bench.benchEnd("clear", "zone_camera", pSaveCounter: false, 0L);
			Bench.bench("fill", "zone_camera");
			fillVisibleZones(pResultStartX, pResultStartY, pResultWidth, pResultHeight, pResultMainX);
			Bench.benchEnd("fill", "zone_camera", pSaveCounter: false, 0L);
			Bench.benchEnd("zone_camera", "zone_camera_total", pSaveCounter: false, 0L);
		}
	}

	private void fillVisibleZones(int pStartX, int pStartY, int pWidth, int pHeight, int pMainX)
	{
		int zones_total_x = _zone_manager.zones_total_x;
		int zones_total_y = _zone_manager.zones_total_y;
		HashSet<MapChunk> set_visible_chunks = _set_visible_chunks;
		List<TileZone> visible_zones = _visible_zones;
		float power_bar_position_y = World.world.move_camera.power_bar_position_y;
		if (pStartX == 0 && pStartY == 0 && pWidth == zones_total_x && pHeight == zones_total_y)
		{
			visible_zones.AddRange(_zone_manager.zones);
			foreach (TileZone item in visible_zones)
			{
				item.visible = true;
				item.visible_main_centered = true;
			}
			_list_visible_chunks.AddRange(World.world.map_chunk_manager.chunks);
			set_visible_chunks.UnionWith(_list_visible_chunks);
			return;
		}
		for (int i = 0; i <= pWidth; i++)
		{
			for (int j = 0; j <= pHeight; j++)
			{
				int num = pStartX + i;
				if (num < 0 || num >= zones_total_x)
				{
					continue;
				}
				int num2 = pStartY + j;
				if (num2 >= 0 && num2 < zones_total_y)
				{
					TileZone zoneUnsafe = _zone_manager.getZoneUnsafe(num, num2);
					visible_zones.Add(zoneUnsafe);
					set_visible_chunks.Add(zoneUnsafe.chunk);
					zoneUnsafe.visible = true;
					if (num >= pMainX && i < pWidth && j < pHeight && !((float)zoneUnsafe.top_left_corner_tile.y < power_bar_position_y))
					{
						zoneUnsafe.visible_main_centered = true;
					}
				}
			}
		}
		_list_visible_chunks.AddRange(set_visible_chunks);
	}

	public List<TileZone> getVisibleZones()
	{
		return _visible_zones;
	}

	public List<MapChunk> getVisibleChunks()
	{
		return _list_visible_chunks;
	}

	public bool hasVisibleZones()
	{
		return _visible_zones.Count > 0;
	}

	public int countVisibleZones()
	{
		return _visible_zones.Count;
	}

	private TileZone getZoneWithinCamera(int pX, int pY, float pBonusY = 0f)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = World.world.camera.ViewportToWorldPoint(new Vector3((float)pX, (float)pY, World.world.camera.nearClipPlane));
		int pX2 = (int)val.x;
		int pY2 = (int)val.y + (int)(pBonusY * 8f);
		int pX3 = _zone_manager.zones_total_x - 1;
		int pY3 = _zone_manager.zones_total_y - 1;
		WorldTile tile = World.world.GetTile(pX2, pY2);
		if (tile == null)
		{
			if (pX == 0 && pY == 0)
			{
				return _zone_manager.getZone(0, 0);
			}
			if (pX == 1 && pY == 1)
			{
				return _zone_manager.getZone(pX3, pY3);
			}
			if (pX == 0 && pY == 1)
			{
				return _zone_manager.getZone(0, pY3);
			}
			if (pX == 1 && pY == 0)
			{
				return _zone_manager.getZone(pX3, 0);
			}
			return null;
		}
		return tile.zone;
	}

	public void clear()
	{
		List<TileZone> visible_zones = _visible_zones;
		foreach (TileZone item in visible_zones)
		{
			item.visible = false;
			item.visible_main_centered = false;
		}
		visible_zones.Clear();
		_list_visible_chunks.Clear();
		_set_visible_chunks.Clear();
	}

	public void fullClear()
	{
		_visible_zones.Clear();
		_last_start_x = -1;
		_last_start_y = -1;
		_last_width = -1;
		_last_height = -1;
		_last_main_x = -1;
	}
}
