using System.Collections.Generic;
using UnityEngine;

public class DebugLayer : MapLayer
{
	internal static List<TileZone> fmod_zones_to_draw = new List<TileZone>();

	private HashSet<WorldTile> _tiles;

	public Color color1;

	public Color color2;

	public Color color_red;

	public Color color_active_path;

	private bool used;

	private List<MapRegion> _forced_global_path;

	protected override void UpdateDirty(float pElapsed)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.instance.debugButton.gameObject.activeSelf)
		{
			clear();
			return;
		}
		color_active_path = new Color(1f, 1f, 1f, 0.5f);
		used = false;
		clear();
		if (_forced_global_path != null && _forced_global_path.Count > 0)
		{
			drawRegionPath(_forced_global_path);
		}
		if (DebugConfig.isOn(DebugOption.CityZones))
		{
			drawZones();
		}
		else if (DebugConfig.isOn(DebugOption.Chunks))
		{
			drawChunks();
		}
		if (DebugConfig.isOn(DebugOption.PathRegions))
		{
			drawPathRegions();
		}
		if (DebugConfig.isOn(DebugOption.ActivePaths))
		{
			drawActivePaths();
		}
		if (DebugConfig.isOn(DebugOption.CityPlaces))
		{
			drawCityPlaces();
		}
		if (DebugConfig.isOn(DebugOption.RenderCityDangerZones))
		{
			drawCityDangerZones();
		}
		if (DebugConfig.isOn(DebugOption.RenderVisibleZones))
		{
			drawVisibleZones();
		}
		if (DebugConfig.isOn(DebugOption.RenderCityCenterZones))
		{
			drawCityCenterZones();
		}
		if (DebugConfig.isOn(DebugOption.RenderCityFarmPlaces))
		{
			drawCityFarmZones();
		}
		if (DebugConfig.isOn(DebugOption.Buildings))
		{
			drawBuildings();
		}
		if (DebugConfig.isOn(DebugOption.FmodZones))
		{
			drawFmodZones();
		}
		if (DebugConfig.isOn(DebugOption.ConstructionTiles))
		{
			drawConstructionTiles();
		}
		if (DebugConfig.isOn(DebugOption.UnitsInside))
		{
			drawUnitsInside();
		}
		if (DebugConfig.isOn(DebugOption.TargetedBy))
		{
			drawTargetedBy();
		}
		if (DebugConfig.isOn(DebugOption.UnitKingdoms))
		{
			drawUnitKingdoms();
		}
		if (DebugConfig.isOn(DebugOption.DisplayUnitTiles))
		{
			drawUnitTiles();
		}
		if (DebugConfig.isOn(DebugOption.ProKing))
		{
			drawProfession(UnitProfession.King);
		}
		if (DebugConfig.isOn(DebugOption.ProLeader))
		{
			drawProfession(UnitProfession.Leader);
		}
		if (DebugConfig.isOn(DebugOption.ProUnit))
		{
			drawProfession(UnitProfession.Unit);
		}
		if (DebugConfig.isOn(DebugOption.ProWarrior))
		{
			drawProfession(UnitProfession.Warrior);
		}
		if (used)
		{
			if (!((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(true);
			}
			updatePixels();
		}
		else if (((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void drawUnitKingdoms()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (Actor unit in World.world.units)
		{
			if (unit.kingdom != null && unit.kingdom.getColor() != null)
			{
				Color val = Color32.op_Implicit(unit.kingdom.getColor().getColorMain32());
				pixels[unit.current_tile.data.tile_id] = Color32.op_Implicit(val);
				_tiles.Add(unit.current_tile);
			}
		}
	}

	private void drawUnitTiles()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.hasUnits())
			{
				pixels[worldTile.data.tile_id] = Color32.op_Implicit(Color.blue);
				_tiles.Add(worldTile);
			}
		}
	}

	private void drawTargetedBy()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.isTargeted())
			{
				pixels[worldTile.data.tile_id] = Color32.op_Implicit(Color.blue);
				_tiles.Add(worldTile);
			}
		}
	}

	private void drawProfession(UnitProfession pPro)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (Actor unit in World.world.units)
		{
			if (unit.isProfession(pPro))
			{
				Color blue = Color.blue;
				pixels[unit.current_tile.data.tile_id] = Color32.op_Implicit(blue);
				_tiles.Add(unit.current_tile);
			}
		}
	}

	private void drawCitizenJobs(string pID)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (Actor unit in World.world.units)
		{
			if (unit.ai.job != null && !(pID != unit.ai.job.id))
			{
				Color red = Color.red;
				pixels[unit.current_tile.data.tile_id] = Color32.op_Implicit(red);
				_tiles.Add(unit.current_tile);
			}
		}
	}

	private void drawUnitsInside()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (Actor unit in World.world.units)
		{
			if (unit.is_inside_building)
			{
				pixels[unit.current_tile.data.tile_id] = Color32.op_Implicit(Color.green);
				_tiles.Add(unit.current_tile);
			}
		}
	}

	private void drawConstructionTiles()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (!worldTile.hasBuilding() || !worldTile.building.asset.docks)
			{
				continue;
			}
			(TileZone[], int) allZonesFromTile = Toolbox.getAllZonesFromTile(worldTile);
			TileZone[] item = allZonesFromTile.Item1;
			int item2 = allZonesFromTile.Item2;
			for (int j = 0; j < item2; j++)
			{
				TileZone pZone = item[j];
				foreach (WorldTile item3 in worldTile.building.checkZoneForDockConstruction(pZone))
				{
					pixels[item3.data.tile_id] = Color32.op_Implicit(Color.red);
					_tiles.Add(item3);
				}
			}
		}
	}

	private void drawFmodZones()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (TileZone item in fmod_zones_to_draw)
		{
			fill(item.tiles, Color.yellow);
		}
	}

	private void drawBuildings()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.hasBuilding())
			{
				if (worldTile.building.kingdom != null && worldTile.building.isKingdomCiv())
				{
					pixels[worldTile.data.tile_id] = worldTile.building.kingdom.getColor().getColorMain32();
				}
				else
				{
					pixels[worldTile.data.tile_id] = Color32.op_Implicit(Color.red);
				}
				pixels[worldTile.building.current_tile.data.tile_id] = Color32.op_Implicit(Color.magenta);
				pixels[worldTile.building.door_tile.data.tile_id] = Color32.op_Implicit(Color.yellow);
				_tiles.Add(worldTile.building.current_tile);
				_tiles.Add(worldTile.building.door_tile);
				_tiles.Add(worldTile);
			}
		}
	}

	private void drawCityCenterZones()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (City city in World.world.cities)
		{
			WorldTile tile = city.getTile();
			if (tile != null)
			{
				fill(tile.zone.tiles, Color.red);
			}
		}
	}

	private void drawCityFarmZones()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (City city in World.world.cities)
		{
			fill(city.calculated_place_for_farms.getSimpleList(), Color.blue);
			fill(city.calculated_farm_fields.getSimpleList(), Color.cyan);
			fill(city.calculated_crops.getSimpleList(), Color.green);
			fill(city.calculated_grown_wheat.getSimpleList(), Color.yellow);
		}
	}

	private void drawVisibleZones()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		List<TileZone> visibleZones = World.world.zone_camera.getVisibleZones();
		for (int i = 0; i < visibleZones.Count; i++)
		{
			TileZone tileZone = visibleZones[i];
			if (tileZone.visible_main_centered)
			{
				fill(tileZone.tiles, Color.green);
			}
			else if (tileZone.visible)
			{
				fill(tileZone.tiles, Color.blue);
			}
		}
	}

	private void drawCityDangerZones()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (City city in World.world.cities)
		{
			foreach (TileZone danger_zone in city.danger_zones)
			{
				fill(danger_zone.tiles, Color.red);
			}
		}
	}

	private void drawCityPlaces()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (TileZone zone in World.world.zone_calculator.zones)
		{
			if (zone.city != null)
			{
				fill(zone.tiles, Color.yellow);
			}
			else if (zone.isGoodForNewCity())
			{
				fill(zone.tiles, Color.blue);
			}
		}
	}

	private void drawActivePaths()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (Actor unit in World.world.units)
		{
			if (unit.current_path_global != null)
			{
				drawRegionPath(unit.current_path_global);
				fill(unit.current_path, Color.blue);
			}
		}
	}

	public void drawRegionPath(List<MapRegion> pRegions)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (MapRegion pRegion in pRegions)
		{
			fill(pRegion.tiles, color_active_path);
		}
	}

	public void forceDrawRegionPath(List<MapRegion> pRegions)
	{
		_forced_global_path.Clear();
		_forced_global_path.AddRange(pRegions);
	}

	private void drawPathRegions()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		for (int i = 0; i < chunks.Length; i++)
		{
			foreach (MapRegion region in chunks[i].regions)
			{
				if (region.path_wave_id != -1)
				{
					fill(region.tiles, new Color(1f, 1f, 0f, 0.9f));
				}
			}
		}
		List<MapRegion> last_globalPath = World.world.region_path_finder.last_globalPath;
		if (last_globalPath == null || last_globalPath.Count <= 0 || World.world.region_path_finder?.tileStart?.region == null || World.world.region_path_finder?.tileTarget?.region == null)
		{
			return;
		}
		foreach (MapRegion item in World.world.region_path_finder.last_globalPath)
		{
			fill(item.tiles, Color.blue);
		}
		fill(World.world.region_path_finder.tileStart.region.tiles, Color.green);
		fill(World.world.region_path_finder.tileTarget.region.tiles, new Color(1f, 0f, 0f, 0.3f));
	}

	private void fill(List<WorldTile> pTiles, Color pColor, bool pEdge = false)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		createTextureNew();
		for (int i = 0; i < pTiles.Count; i++)
		{
			WorldTile worldTile = pTiles[i];
			if (!pEdge || worldTile.region != null)
			{
				_tiles.Add(worldTile);
				pixels[worldTile.data.tile_id] = Color32.op_Implicit(pColor);
			}
		}
	}

	private void fill(WorldTile[] pTiles, Color pColor, bool pEdge = false)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		createTextureNew();
		foreach (WorldTile worldTile in pTiles)
		{
			if (!pEdge || worldTile.region != null)
			{
				_tiles.Add(worldTile);
				pixels[worldTile.data.tile_id] = Color32.op_Implicit(pColor);
			}
		}
	}

	private void drawZones()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		foreach (TileZone zone in World.world.zone_calculator.zones)
		{
			if ((zone.x + zone.y) % 2 == 0)
			{
				zone.debug_zone_color = color1;
			}
			else
			{
				zone.debug_zone_color = color2;
			}
			fill(zone.tiles, zone.debug_zone_color);
		}
	}

	private void testCityLayout()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		DebugVariables instance = DebugVariables.instance;
		if (instance != null && !instance.layout_city_test)
		{
			return;
		}
		used = true;
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		if (mouseTilePos == null)
		{
			return;
		}
		TileZone pCursorZone = mouseTilePos?.zone;
		foreach (TileZone zone in World.world.zone_calculator.zones)
		{
			bool flag = true;
			if (!TownPlans.debugVisualizeZone(zone, pCursorZone))
			{
				flag = false;
			}
			if (flag)
			{
				zone.debug_zone_color = color1;
			}
			else
			{
				zone.debug_zone_color = color_red;
			}
			fill(zone.tiles, zone.debug_zone_color);
		}
	}

	private void drawChunks()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		used = true;
		MapChunk[] chunks = World.world.map_chunk_manager.chunks;
		foreach (MapChunk mapChunk in chunks)
		{
			fill(mapChunk.tiles, mapChunk.color);
		}
	}

	internal override void clear()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		HashSet<WorldTile> tiles = _tiles;
		if (tiles.Count == 0)
		{
			return;
		}
		foreach (WorldTile item in tiles)
		{
			if (item.data.tile_id <= pixels.Length - 1)
			{
				pixels[item.data.tile_id] = Color32.op_Implicit(Color.clear);
			}
		}
		_tiles.Clear();
		createTextureNew();
	}

	public DebugLayer()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_tiles = new HashSet<WorldTile>();
		color1 = Color.gray;
		color2 = Color.white;
		color_red = Color.red;
		_forced_global_path = new List<MapRegion>();
		base._002Ector();
	}
}
