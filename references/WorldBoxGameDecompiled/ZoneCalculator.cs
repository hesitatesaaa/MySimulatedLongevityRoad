using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ZoneCalculator : MapLayer
{
	public readonly List<TileZone> zones;

	private readonly Dictionary<int, TileZone> _zones_dict_id;

	internal TileZone[,] map;

	private bool _dirty;

	private int _last_zone_state;

	public float outline_animation;

	private bool _outline_animation_in;

	private float _cached_map_opacity;

	private bool _cached_ony_favorited_meta;

	private bool _cached_check_animation;

	private bool _cached_should_be_clear_color;

	private bool _last_should_be_clear_color;

	private Kingdom _last_selected_kingdom;

	public int zones_total_x;

	public int zones_total_y;

	private const float ALPHA_NON_FAVORITED_META = 0.5f;

	private const float ALPHA_NON_SELECTED_META = 0.6f;

	public Color color1;

	public Color color2;

	private readonly Color32 _color_clear;

	private readonly HashSetTileZone _to_clean_up;

	private float _night_multiplier;

	private int _debug_redrawn_last;

	private float _redraw_timer;

	private bool _dirty_draw_zones;

	public float minimap_opacity;

	public float border_brightness;

	private MetaTypeAsset _mode_asset;

	private bool _selection_changed_dirty;

	private NanoObject _cursor_nano_object;

	private NanoObject _selected_nano_object;

	private readonly HashSetTileZone _current_drawn_zones;

	private int _debug_redrawn_last_amount;

	internal override void create()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.create();
		colorValues = new Color(1f, 0.46f, 0.19f, 1f);
		Color color = sprRnd.color;
		color.a = 0.78f;
		sprRnd.color = color;
	}

	public void clearDebug()
	{
		if (DebugConfig.isOn(DebugOption.DebugZones))
		{
			for (int i = 0; i < zones.Count; i++)
			{
				zones[i].clearDebug();
			}
		}
	}

	internal override void clear()
	{
		base.clear();
		for (int i = 0; i < zones.Count; i++)
		{
			zones[i].clear();
		}
		_current_drawn_zones.Clear();
		_to_clean_up.Clear();
		_last_selected_kingdom = null;
	}

	public void clean()
	{
		foreach (TileZone zone in zones)
		{
			zone.Dispose();
		}
		zones.Clear();
		_zones_dict_id.Clear();
		map = null;
	}

	public void generate()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		zones.Clear();
		_zones_dict_id.Clear();
		int num = 8;
		zones_total_x = Config.ZONE_AMOUNT_X * num;
		zones_total_y = Config.ZONE_AMOUNT_Y * num;
		map = new TileZone[zones_total_x, zones_total_y];
		int num2 = 0;
		for (int i = 0; i < zones_total_y; i++)
		{
			for (int j = 0; j < zones_total_x; j++)
			{
				TileZone tileZone = new TileZone
				{
					x = j,
					y = i,
					id = num2++
				};
				if ((j + i) % 2 == 0)
				{
					tileZone.debug_zone_color = color1;
				}
				else
				{
					tileZone.debug_zone_color = color2;
				}
				map[j, i] = tileZone;
				zones.Add(tileZone);
				_zones_dict_id.Add(tileZone.id, tileZone);
				fillZone(tileZone);
			}
		}
		World.world.tilemap.generate(zones.Count);
		generateNeighbours();
		zones.Shuffle();
	}

	public TileZone getZoneByID(int pID)
	{
		return _zones_dict_id[pID];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileZone getZone(int pX, int pY)
	{
		if (pX < 0 || pX >= zones_total_x || pY < 0 || pY >= zones_total_y)
		{
			return null;
		}
		return map[pX, pY];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TileZone getZoneUnsafe(int pX, int pY)
	{
		return map[pX, pY];
	}

	private void generateNeighbours()
	{
		using ListPool<TileZone> listPool = new ListPool<TileZone>(4);
		using ListPool<TileZone> listPool2 = new ListPool<TileZone>(8);
		List<TileZone> list = zones;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			TileZone tileZone = list[i];
			TileZone zone = getZone(tileZone.x - 1, tileZone.y);
			tileZone.addNeighbour(zone, TileDirection.Left, listPool, listPool2);
			zone = getZone(tileZone.x + 1, tileZone.y);
			tileZone.addNeighbour(zone, TileDirection.Right, listPool, listPool2);
			zone = getZone(tileZone.x, tileZone.y - 1);
			tileZone.addNeighbour(zone, TileDirection.Down, listPool, listPool2);
			zone = getZone(tileZone.x, tileZone.y + 1);
			tileZone.addNeighbour(zone, TileDirection.Up, listPool, listPool2);
			zone = getZone(tileZone.x - 1, tileZone.y - 1);
			tileZone.addNeighbour(zone, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			zone = getZone(tileZone.x - 1, tileZone.y + 1);
			tileZone.addNeighbour(zone, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			zone = getZone(tileZone.x + 1, tileZone.y - 1);
			tileZone.addNeighbour(zone, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			zone = getZone(tileZone.x + 1, tileZone.y + 1);
			tileZone.addNeighbour(zone, TileDirection.Null, listPool, listPool2, pDiagonal: true);
			tileZone.neighbours = listPool.ToArray();
			tileZone.neighbours_all = listPool2.ToArray();
			listPool.Clear();
			listPool2.Clear();
		}
	}

	private void fillZone(TileZone pZone)
	{
		int num = pZone.x * 8;
		int num2 = pZone.y * 8;
		int num3 = 4;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				WorldTile tile = World.world.GetTile(i + num, j + num2);
				if (tile != null)
				{
					tile.zone = pZone;
					pZone.addTile(tile, i, j);
					if (i == num3 && j == num3)
					{
						pZone.centerTile = tile;
					}
				}
			}
		}
	}

	private void updateOutlineAnimation(float pElapsed)
	{
		if (_selected_nano_object == null && _cursor_nano_object == null)
		{
			_outline_animation_in = true;
			outline_animation = 0f;
		}
		else if (_outline_animation_in)
		{
			outline_animation += pElapsed * 2f;
			if (outline_animation >= 1f)
			{
				outline_animation = 1f;
				_outline_animation_in = false;
			}
		}
		else
		{
			outline_animation -= pElapsed * 2f;
			if (outline_animation <= 0f)
			{
				outline_animation = 0f;
				_outline_animation_in = true;
			}
		}
	}

	public void updateAnimationsAndSelections()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		_cached_map_opacity = getMapOpacity();
		_cached_ony_favorited_meta = PlayerConfig.optionBoolEnabled("only_favorited_meta");
		_cached_check_animation = !ScrollWindow.isWindowActive() && (!_cursor_nano_object.isRekt() || !_selected_nano_object.isRekt());
		_cached_should_be_clear_color = shouldBeClearColor();
		checkCursorNanoObject();
		checkSelectedNanoObject();
		updateOutlineAnimation(World.world.delta_time);
		MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
		int last_zone_state = cachedMapMetaAsset?.getZoneOptionState() ?? (-1);
		checkClearAllZonesToRedraw();
		checkDrawnZonesDirty();
		_last_should_be_clear_color = _cached_should_be_clear_color;
		bool flag = Zones.showMapBorders();
		if ((cachedMapMetaAsset != null) & flag)
		{
			((Renderer)sprRnd).enabled = true;
			setMode(cachedMapMetaAsset);
			_last_zone_state = last_zone_state;
		}
		else
		{
			setMode(null);
			((Renderer)sprRnd).enabled = false;
		}
		Color white = Color.white;
		white.r = border_brightness;
		white.g = border_brightness;
		white.b = border_brightness;
		if (World.world.era_manager.getCurrentAge().overlay_darkness)
		{
			_night_multiplier = Mathf.Lerp(_night_multiplier, 0.6f, World.world.delta_time * 0.5f);
		}
		else
		{
			_night_multiplier = Mathf.Lerp(_night_multiplier, 1f, World.world.delta_time * 0.5f);
		}
		white.a = _cached_map_opacity;
		sprRnd.color = white;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
	}

	private void checkClearAllZonesToRedraw()
	{
		int num = World.world.getCachedMapMetaAsset()?.getZoneOptionState() ?? (-1);
		if ((num != -1 && num != _last_zone_state) || _selection_changed_dirty || _last_should_be_clear_color != _cached_should_be_clear_color)
		{
			clearCurrentDrawnZones();
			_selection_changed_dirty = false;
		}
	}

	public override void draw(float pElapsed)
	{
		redrawZones();
	}

	private static float getCameraScaleZoom()
	{
		return Mathf.Clamp(MoveCamera.instance.main_camera.orthographicSize / 20f, 1f, 30f);
	}

	private void setDirty()
	{
		_dirty = true;
	}

	private void setMode(MetaTypeAsset pAsset)
	{
		if (_mode_asset != pAsset)
		{
			clearAllRedrawTimers();
		}
		_mode_asset = pAsset;
	}

	public void clearAllRedrawTimers()
	{
		clearTimer();
		clearWorldBehaviourTimer();
	}

	public void clearTimer()
	{
		_redraw_timer = 0f;
	}

	public void clearWorldBehaviourTimer()
	{
		AssetManager.world_behaviours.get("zones_meta_data_visualizer").manager.timerClear();
	}

	public bool isModeNone()
	{
		return _mode_asset == null;
	}

	private bool isBorderColor_relations(TileZone pZone, City pCity, bool pCheckFriendly = false)
	{
		if (pZone != null && pZone.city != pCity && pZone.city != null && pZone.city.kingdom == pCity.kingdom)
		{
			return false;
		}
		if (pZone != null && pZone.city != null)
		{
			return pZone.city.kingdom != pCity.kingdom;
		}
		return true;
	}

	public void debug(DebugTool pTool)
	{
		if (_debug_redrawn_last_amount != 0)
		{
			_debug_redrawn_last = _debug_redrawn_last_amount;
		}
		pTool.setText("_toCleanUp", _to_clean_up.Count, 0f, pShowBar: false, 0L);
		pTool.setText("_lastDrawnZones", _current_drawn_zones.Count, 0f, pShowBar: false, 0L);
		pTool.setText("redrawn_last", _debug_redrawn_last, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
	}

	public TileZone getMapCenterZone()
	{
		int num = zones_total_x / 2;
		int num2 = zones_total_y / 2;
		return map[num, num2];
	}

	public void drawZoneMeta(TileZone pZone, MetaTypeAsset pMetaTypeAsset, MetaZoneGetMetaSimple pZoneGetDelegate)
	{
		IMetaObject metaObject = pZoneGetDelegate(pZone);
		bool pUp = isBorderColorSameNanoObject(pZone.zone_up, pZoneGetDelegate, metaObject);
		bool pDown = isBorderColorSameNanoObject(pZone.zone_down, pZoneGetDelegate, metaObject);
		bool pLeft = isBorderColorSameNanoObject(pZone.zone_left, pZoneGetDelegate, metaObject);
		bool pRight = isBorderColorSameNanoObject(pZone.zone_right, pZoneGetDelegate, metaObject);
		MetaObjectData pMetaData = null;
		if (metaObject != null)
		{
			pMetaData = metaObject.getMetaData();
		}
		drawZoneMeta(metaObject, pZone, pUp, pDown, pLeft, pRight, pMetaData, pMetaTypeAsset);
	}

	public void drawZoneAlliance(TileZone pZone, int pZoneOption)
	{
		Alliance allianceOnZone = pZone.getAllianceOnZone(pZoneOption);
		bool pUp = isBorderColor_alliance(pZone.zone_up, allianceOnZone, pZoneOption);
		bool pDown = isBorderColor_alliance(pZone.zone_down, allianceOnZone, pZoneOption);
		bool pLeft = isBorderColor_alliance(pZone.zone_left, allianceOnZone, pZoneOption);
		bool pRight = isBorderColor_alliance(pZone.zone_right, allianceOnZone, pZoneOption);
		drawZoneMeta(allianceOnZone, pZone, pUp, pDown, pLeft, pRight, allianceOnZone.data, MetaTypeLibrary.alliance);
	}

	private bool isBorderColor_alliance(TileZone pZone, Alliance pAlliance, int pZoneOption, bool pCheckFriendly = false)
	{
		if (pZone == null)
		{
			return true;
		}
		NanoObject allianceOnZone = pZone.getAllianceOnZone(pZoneOption);
		if (allianceOnZone == null)
		{
			return true;
		}
		if (allianceOnZone == pAlliance)
		{
			return false;
		}
		return true;
	}

	private void drawZoneMeta(IMetaObject pMeta, TileZone pZone, bool pUp, bool pDown, bool pLeft, bool pRight, MetaObjectData pMetaData, MetaTypeAsset pMetaTypeAsset)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		int pHashCode = -1;
		if (pMeta != null)
		{
			pHashCode = pMeta.GetHashCode();
		}
		int pID = generateIdForDraw(_mode_asset, pHashCode, pUp, pDown, pLeft, pRight);
		bool pLastAnimated = false;
		int pColorAssetID = 0;
		Color32 pColorBorderOut;
		Color32 pColorBorderInside;
		if (pMeta != null && pMeta.isAlive())
		{
			ColorAsset color = pMeta.getColor();
			pColorAssetID = color.index_id;
			pColorBorderOut = color.getColorMainSecond32();
			pColorBorderInside = ((!_cached_should_be_clear_color) ? color.getColorBorderInsideAlpha32() : _color_clear);
			pLastAnimated = checkFadeAndSelectionColors(pZone, ref pColorBorderInside, ref pColorBorderOut, 0f, pMeta, pMetaTypeAsset, pMetaData.favorite);
		}
		else
		{
			pColorBorderInside = _color_clear;
			pColorBorderOut = _color_clear;
		}
		if (pZone.checkShouldReRender(pHashCode, pID, pColorAssetID, pLastAnimated))
		{
			applyMetaColorsToZone(pZone, ref pColorBorderInside, ref pColorBorderOut, pUp, pDown, pLeft, pRight);
		}
	}

	public void drawZoneCity(TileZone pZone)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		City city = pZone.city;
		Kingdom kingdom = city.kingdom;
		ColorAsset color = city.getColor();
		Color32 pColorBorderInside = color.getColorBorderInsideAlpha32();
		Color32 pColorBorderOut = color.getColorMainSecond32();
		if (_cached_should_be_clear_color)
		{
			pColorBorderInside = _color_clear;
		}
		bool pUp = isBorderColor_cities(pZone.zone_up, city, pCheckFriendly: true);
		bool pDown = isBorderColor_cities(pZone.zone_down, city);
		bool pLeft = isBorderColor_cities(pZone.zone_left, city);
		bool pRight = isBorderColor_cities(pZone.zone_right, city, pCheckFriendly: true);
		int hashCode = city.GetHashCode();
		int num = generateIdForDraw(_mode_asset, hashCode, pUp, pDown, pLeft, pRight);
		num += kingdom.GetHashCode();
		bool pLastAnimated = checkFadeAndSelectionColors(pZone, ref pColorBorderInside, ref pColorBorderOut, 0f, city, MetaTypeLibrary.city, city.data.favorite);
		if (pZone.checkShouldReRender(hashCode, num, 0, pLastAnimated))
		{
			applyMetaColorsToZone(pZone, ref pColorBorderInside, ref pColorBorderOut, pUp, pDown, pLeft, pRight);
		}
	}

	private bool isBorderColorSameNanoObject(TileZone pZone, MetaZoneGetMetaSimple pZoneGetMetaDelegate, IMetaObject pNanoObjectMain)
	{
		if (pZone == null)
		{
			return true;
		}
		IMetaObject metaObject = pZoneGetMetaDelegate(pZone);
		if (metaObject == null)
		{
			return true;
		}
		if (metaObject == pNanoObjectMain)
		{
			return false;
		}
		return true;
	}

	private bool isBorderColor_cities(TileZone pZone, City pCityMain, bool pCheckFriendly = false)
	{
		if (pZone == null)
		{
			return true;
		}
		City city = pZone.city;
		Kingdom kingdom = city?.kingdom;
		if (pCheckFriendly && city != pCityMain && city != null && kingdom == pCityMain.kingdom)
		{
			return false;
		}
		if (city != null && kingdom == pCityMain.kingdom)
		{
			return city != pCityMain;
		}
		return true;
	}

	private bool checkShouldDrawObject(bool pMetaFavorite)
	{
		if (_cached_ony_favorited_meta && !pMetaFavorite)
		{
			return false;
		}
		return true;
	}

	private float getMapOpacity()
	{
		float num = ((!MapBox.isRenderMiniMap()) ? Mathf.Clamp(getCameraScaleZoom() * 0.3f, 0f, 0.78f) : minimap_opacity);
		return num * _night_multiplier;
	}

	private bool shouldBeClearColor()
	{
		if (MapBox.isRenderGameplay())
		{
			return true;
		}
		return false;
	}

	public void drawEnd(TileZone pZone)
	{
		_current_drawn_zones.Add(pZone);
		_to_clean_up.Remove(pZone);
	}

	private void checkCursorNanoObject()
	{
		NanoObject nanoObject = null;
		if (MapBox.isRenderMiniMap() && !World.world.isOverUI())
		{
			WorldTile mouseTilePosCachedFrame = World.world.getMouseTilePosCachedFrame();
			MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
			if (mouseTilePosCachedFrame != null && cachedMapMetaAsset != null)
			{
				nanoObject = cachedMapMetaAsset.tile_get_metaobject?.Invoke(mouseTilePosCachedFrame.zone, cachedMapMetaAsset.getZoneOptionState()) as NanoObject;
			}
		}
		if (_cursor_nano_object != nanoObject)
		{
			_selection_changed_dirty = true;
		}
		_cursor_nano_object = nanoObject;
	}

	private void checkSelectedNanoObject()
	{
		NanoObject nanoObject = null;
		if (MapBox.isRenderMiniMap())
		{
			MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
			if (cachedMapMetaAsset != null)
			{
				nanoObject = cachedMapMetaAsset.get_selected();
			}
		}
		if (nanoObject != _selected_nano_object)
		{
			_selection_changed_dirty = true;
		}
		_selected_nano_object = nanoObject;
	}

	private void redrawZones()
	{
		Bench.bench("borders_renderer", "borders_renderer_total");
		Bench.bench("clearAllRedrawTimers", "borders_renderer");
		if (_last_selected_kingdom != SelectedMetas.selected_kingdom)
		{
			_last_selected_kingdom = SelectedMetas.selected_kingdom;
			clearAllRedrawTimers();
		}
		Bench.benchEnd("clearAllRedrawTimers", "borders_renderer", pSaveCounter: false, 0L);
		if (_redraw_timer > 0f)
		{
			_redraw_timer -= Time.deltaTime;
			Bench.clearBenchmarkEntrySkipMultiple("borders_renderer_total", "_to_clean_up.union", "draw_zones.Invoke", "clearDrawnZones", "updatePixels");
			return;
		}
		_redraw_timer = 0.01f;
		Bench.bench("_to_clean_up.union", "borders_renderer");
		_debug_redrawn_last_amount = 0;
		if (_current_drawn_zones.Any())
		{
			_to_clean_up.UnionWith(_current_drawn_zones);
		}
		Bench.benchEnd("_to_clean_up.union", "borders_renderer", pSaveCounter: false, 0L);
		Bench.bench("draw_zones.Invoke", "borders_renderer");
		if (_mode_asset != null)
		{
			_mode_asset.draw_zones(_mode_asset);
		}
		Bench.benchEnd("draw_zones.Invoke", "borders_renderer", pSaveCounter: false, 0L);
		Bench.bench("clearDrawnZones", "borders_renderer");
		if (_to_clean_up.Any())
		{
			clearDrawnZones();
		}
		Bench.benchEnd("clearDrawnZones", "borders_renderer", pSaveCounter: false, 0L);
		Bench.bench("updatePixels", "borders_renderer");
		if (_dirty)
		{
			_dirty = false;
			updatePixels();
		}
		Bench.benchEnd("updatePixels", "borders_renderer", pSaveCounter: false, 0L);
		Bench.benchEnd("borders_renderer", "borders_renderer_total", pSaveCounter: false, 0L);
	}

	private void clearDrawnZones()
	{
		foreach (TileZone item in _to_clean_up)
		{
			drawZoneClear(item);
			item.resetRenderHelpers();
			_current_drawn_zones.Remove(item);
		}
		_to_clean_up.Clear();
	}

	public void dirtyAndClear()
	{
		setDrawnZonesDirty();
		clearCurrentDrawnZones();
	}

	internal void clearCurrentDrawnZones(bool pCleanTimer = true)
	{
		foreach (TileZone current_drawn_zone in _current_drawn_zones)
		{
			drawZoneClear(current_drawn_zone);
			current_drawn_zone.resetRenderHelpers();
		}
		_current_drawn_zones.Clear();
		if (pCleanTimer)
		{
			clearAllRedrawTimers();
		}
	}

	private int generateIdForDraw(MetaTypeAsset pModeAsset, int pHashCode, bool pUp, bool pDown, bool pLeft, bool pRight)
	{
		int num = (pModeAsset.GetHashCode() + 1) * 10000000;
		if (pUp)
		{
			num += 100000;
		}
		if (pDown)
		{
			num += 10000;
		}
		if (pLeft)
		{
			num += 1000;
		}
		if (pRight)
		{
			num += 100;
		}
		return num;
	}

	public MetaType getCurrentModeDebug()
	{
		MetaType metaType = Zones.getForcedMapMode();
		if (metaType.isNone())
		{
			MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
			if (cachedMapMetaAsset != null)
			{
				metaType = cachedMapMetaAsset.map_mode;
			}
		}
		return metaType;
	}

	private void applyAlphaFadeToColor(ref Color32 pColorBorderInside, ref Color32 pColorBorderOutside, MetaTypeAsset pMetaTypeAsset, float pDiff, int pUnits, double pTimestampNew)
	{
		float cached_map_opacity = _cached_map_opacity;
		float num = pDiff / 5f;
		float num2 = 0.6f;
		float num3 = (float)(int)pColorBorderInside.a / 255f;
		float num4 = (float)(int)pColorBorderOutside.a / 255f;
		float num5 = 1f;
		World.world.getWorldTimeElapsedSince(pTimestampNew);
		_ = 1f;
		float num6 = 1f - num;
		float num7 = Mathf.Clamp01((num - num2) / (1f - num2));
		float num8 = (1f - num7 * num7 * num7) * (cached_map_opacity * num5);
		num6 *= cached_map_opacity * num5;
		byte a = (byte)(num8 * num3 * 255f);
		byte a2 = (byte)(num6 * num4 * 255f);
		pColorBorderInside.a = a;
		pColorBorderOutside.a = a2;
	}

	private bool shouldShowSelectionFor(IMetaObject pNanoObject)
	{
		if (pNanoObject == _cursor_nano_object || pNanoObject == _selected_nano_object)
		{
			return true;
		}
		return false;
	}

	private bool checkFadeAndSelectionColors(TileZone pZone, ref Color32 pColorBorderInside, ref Color32 pColorBorderOut, float pDiff, IMetaObject pMetaObjectToDraw, MetaTypeAsset pMetaTypeAsset, bool pFavorite)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (_cached_check_animation)
		{
			if (shouldShowSelectionFor(pMetaObjectToDraw))
			{
				flag = true;
			}
			else if (!_selected_nano_object.isRekt())
			{
				pColorBorderInside.a = (byte)((float)(int)pColorBorderInside.a * 0.6f);
				pColorBorderOut.a = (byte)((float)(int)pColorBorderOut.a * 0.6f);
			}
		}
		bool flag2 = false;
		flag2 = flag || ((!SelectedUnit.isSet()) ? checkShouldDrawObject(pFavorite) : (SelectedUnit.unit.getMetaObjectOfType(pMetaTypeAsset.map_mode) == pMetaObjectToDraw));
		if (!flag2)
		{
			pColorBorderInside.a = (byte)((float)(int)pColorBorderInside.a * 0.5f);
			pColorBorderOut.a = (byte)((float)(int)pColorBorderOut.a * 0.5f);
		}
		if (!flag2 && _cached_should_be_clear_color)
		{
			pColorBorderInside = _color_clear;
		}
		if (flag)
		{
			pZone.resetRenderHelpers();
			float num = outline_animation;
			pColorBorderOut = Color32.Lerp(pColorBorderOut, Toolbox.color_black_32, num);
		}
		return flag;
	}

	public void drawBegin()
	{
	}

	internal void setDrawnZonesDirty()
	{
		_dirty_draw_zones = true;
	}

	private void checkDrawnZonesDirty()
	{
		if (_dirty_draw_zones)
		{
			_dirty_draw_zones = false;
			clearAllRedrawTimers();
		}
	}

	public void drawGenericFluid(ZoneMetaData pData, MetaTypeAsset pMetaTypeAsset)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		TileZone zone = pData.zone;
		IMetaObject meta_object = pData.meta_object;
		bool pFavorite = meta_object.isFavorite();
		double curWorldTime = World.world.getCurWorldTime();
		float diffTime = pData.getDiffTime(curWorldTime);
		if (!(diffTime > 5f))
		{
			ColorAsset color = meta_object.getColor();
			Color32 pColorBorderInside = Color32.op_Implicit(color.getColorText());
			Color32 pColorBorderOutside = Color32.op_Implicit(color.getColorText());
			if (diffTime != 0f)
			{
				applyAlphaFadeToColor(ref pColorBorderInside, ref pColorBorderOutside, pMetaTypeAsset, diffTime, meta_object.countUnits(), pData.timestamp_new);
			}
			bool pUp = false;
			bool pDown = false;
			bool pLeft = false;
			bool pRight = false;
			bool num = shouldShowSelectionFor(meta_object);
			if (num)
			{
				pUp = isBorderNanoMetaFluid(zone.zone_up, meta_object, curWorldTime);
				pDown = isBorderNanoMetaFluid(zone.zone_down, meta_object, curWorldTime);
				pLeft = isBorderNanoMetaFluid(zone.zone_left, meta_object, curWorldTime);
				pRight = isBorderNanoMetaFluid(zone.zone_right, meta_object, curWorldTime);
			}
			int hashCode = meta_object.GetHashCode();
			int last_drawn_id = generateIdForDraw(_mode_asset, hashCode, pUp, pDown, pLeft, pRight);
			zone.last_drawn_id = last_drawn_id;
			zone.last_drawn_hashcode = hashCode;
			checkFadeAndSelectionColors(zone, ref pColorBorderInside, ref pColorBorderOutside, diffTime, meta_object, pMetaTypeAsset, pFavorite);
			if (num)
			{
				applyMetaColorsToZone(zone, ref pColorBorderInside, ref pColorBorderOutside, pUp, pDown, pLeft, pRight);
			}
			else
			{
				applyMetaColorsToZoneFull(zone, ref pColorBorderInside);
			}
		}
	}

	private bool isBorderNanoMetaFluid(TileZone pZone, IMetaObject pMetaMain, double pCurTime)
	{
		if (pZone == null)
		{
			return true;
		}
		if (ZoneMetaDataVisualizer.hasZoneData(pZone))
		{
			ZoneMetaData zoneMetaData = ZoneMetaDataVisualizer.getZoneMetaData(pZone);
			if (zoneMetaData.getDiffTime(pCurTime) > 5f)
			{
				return true;
			}
			if (zoneMetaData.meta_object == pMetaMain)
			{
				return false;
			}
		}
		return true;
	}

	private void applyMetaColorsToZoneFull(TileZone pZone, ref Color32 pColor)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		setDirty();
		WorldTile[] tiles = pZone.tiles;
		Color32[] array = pixels;
		int num = tiles.Length;
		Color32 val = array[tiles[0].data.tile_id];
		if (val.r != pColor.r || val.g != pColor.g || val.b != pColor.b || val.a != pColor.a)
		{
			for (int i = 0; i < num; i++)
			{
				int tile_id = tiles[i].data.tile_id;
				array[tile_id] = pColor;
			}
			_debug_redrawn_last_amount++;
		}
	}

	private void applyMetaColorsToZone(TileZone pZone, ref Color32 pColorInside, ref Color32 pColorOutside, bool pUp, bool pDown, bool pLeft, bool pRight)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		setDirty();
		WorldTile[] tiles = pZone.tiles;
		Color32[] array = pixels;
		int num = tiles.Length;
		for (int i = 0; i < num; i++)
		{
			WorldTile obj = tiles[i];
			int tile_id = obj.data.tile_id;
			WorldTileZoneBorder world_tile_zone_border = obj.world_tile_zone_border;
			if (!world_tile_zone_border.border)
			{
				array[tile_id] = pColorInside;
			}
			else if (pUp && world_tile_zone_border.border_up)
			{
				array[tile_id] = pColorOutside;
			}
			else if (pDown && world_tile_zone_border.border_down)
			{
				array[tile_id] = pColorOutside;
			}
			else if (pLeft && world_tile_zone_border.border_left)
			{
				array[tile_id] = pColorOutside;
			}
			else if (pRight && world_tile_zone_border.border_right)
			{
				array[tile_id] = pColorOutside;
			}
			else
			{
				array[tile_id] = pColorInside;
			}
		}
		_debug_redrawn_last_amount++;
	}

	private void drawZoneClear(TileZone pZone)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		colorZone(pZone, Toolbox.clear);
	}

	private void colorZone(TileZone pZone, Color32 pColor)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		setDirty();
		Color32[] array = pixels;
		WorldTile[] tiles = pZone.tiles;
		int num = tiles.Length;
		Color32 val = array[tiles[0].data.tile_id];
		if (val.r != pColor.r || val.g != pColor.g || val.b != pColor.b || val.a != pColor.a)
		{
			for (int i = 0; i < num; i++)
			{
				int tile_id = tiles[i].data.tile_id;
				array[tile_id] = pColor;
			}
		}
	}

	public ZoneCalculator()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		zones = new List<TileZone>();
		_zones_dict_id = new Dictionary<int, TileZone>();
		_last_zone_state = -1;
		color1 = Color.gray;
		color2 = Color.white;
		_color_clear = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);
		_to_clean_up = new HashSetTileZone();
		_night_multiplier = 1f;
		minimap_opacity = 1f;
		border_brightness = 1f;
		_current_drawn_zones = new HashSetTileZone();
		base._002Ector();
	}
}
