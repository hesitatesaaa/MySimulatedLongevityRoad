using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerControl
{
	public const int TOUCH_POWER_ACTIVATION_FRAMES = 5;

	internal float timer_spawn_pixels;

	private float _over_ui_timeout;

	private GameObject _gui_check_game_object;

	private static bool? _is_pointer_over_ui_object;

	private Vector2Int _last_click;

	private Vector2 _origin_touch;

	private Vector2 _current_touch;

	internal WorldTile first_pressed_tile;

	internal TileType first_pressed_type;

	internal TopTileType first_pressed_top_type;

	internal bool first_click;

	public double click_started_at;

	private float _click_timer;

	private int _last_check;

	internal float inspect_timer_click;

	internal int touch_ticks_skip;

	private double _last_time_touched_ui;

	internal bool already_used_zoom;

	internal bool already_used_camera_drag;

	internal bool already_used_power;

	internal float controls_lock_timer;

	private PointerEventData _event_data_current_position;

	private readonly List<RaycastResult> _results;

	public Vector2 square_selection_position_current;

	private Vector2 square_selection_position_start_last;

	public bool square_selection_started;

	private bool _ignore_square_selection;

	private int _square_selection_ended_frame;

	private WorldTile _cached_mouse_tile_pos;

	public const float DRAG_DETECTION_THRESHOLD_PERCENT = 0.007f;

	public PlayerControl()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		first_click = true;
		_last_check = -1;
		_event_data_current_position = new PointerEventData(EventSystem.current);
		_results = new List<RaycastResult>();
		base._002Ector();
	}

	public bool isSelectionHappens()
	{
		return square_selection_started;
	}

	public bool isAnyInputHappening()
	{
		if (InputHelpers.mouseSupported)
		{
			if (InputHelpers.GetMouseButton(0))
			{
				return true;
			}
			return false;
		}
		if (InputHelpers.touchCount > 0)
		{
			return true;
		}
		return false;
	}

	internal void updateControls()
	{
		_cached_mouse_tile_pos = getMouseTilePos();
		if (timer_spawn_pixels > 0f)
		{
			timer_spawn_pixels -= World.world.delta_time;
		}
		AssetManager.hotkey_library.checkHotKeyActions();
		if (controlsLocked())
		{
			return;
		}
		if (isOverUI(pCheckTimeout: false))
		{
			_over_ui_timeout = 0.05f;
		}
		else if (_over_ui_timeout > 0f)
		{
			_over_ui_timeout -= Time.deltaTime;
		}
		if (isControllingUnit())
		{
			updateTouch();
			return;
		}
		if (DebugConfig.isOn(DebugOption.MakeUnitsFollowCursor))
		{
			WorldTile mouseTilePosCachedFrame = getMouseTilePosCachedFrame();
			if (mouseTilePosCachedFrame != null && !isOverUI() && InputHelpers.GetMouseButtonDown(0))
			{
				Bench.bench("test_follow");
				foreach (Actor unit in World.world.units)
				{
					if (unit.isFavorite() && unit.current_tile.region.island == mouseTilePosCachedFrame.region.island)
					{
						unit.stopMovement();
						unit.goTo(getMouseTilePosCachedFrame());
					}
				}
				Bench.benchEnd("test_follow", "main", pSaveCounter: false, 0L);
			}
		}
		checkTrailerModeButtons();
		if (!Globals.TRAILER_MODE && !((Component)World.world.canvas).gameObject.activeSelf)
		{
			return;
		}
		World.world.magnet.magnetAction(pFromUpdate: true);
		Boulder.checkRelease();
		if (InputHelpers.GetMouseButtonUp(0))
		{
			timer_spawn_pixels = 0f;
		}
		if (!isAnyInputHappening())
		{
			already_used_zoom = false;
			already_used_camera_drag = false;
			touch_ticks_skip = 0;
			already_used_power = false;
		}
		updateTouch();
		if (controls_lock_timer > 0f)
		{
			controls_lock_timer -= Time.deltaTime;
		}
		else
		{
			if (World.world.isGameplayControlsLocked())
			{
				return;
			}
			if ((InputHelpers.GetMouseButton(1) || InputHelpers.GetMouseButton(0)) && isOverUI())
			{
				_last_time_touched_ui = World.world.getCurSessionTime();
			}
			if (!checkSquareMultiSelection())
			{
				finishSquareSelection();
			}
			if (isOverUI() || tryToMoveSelectedUnits() || checkClickTouchInspectSelect())
			{
				return;
			}
			if (!already_used_zoom && MoveCamera.inSpectatorMode() && !MoveCamera.camera_drag_activated && InputHelpers.GetMouseButtonUp(0) && getDistanceBetweenOriginAndCurrentTouch() < 20f)
			{
				Actor actorFromTile = ActionLibrary.getActorFromTile(getMouseTilePosCachedFrame());
				if (actorFromTile != null)
				{
					World.world.locateAndFollow(actorFromTile, null, null);
					return;
				}
			}
			if (InputHelpers.GetMouseButton(1) || InputHelpers.GetMouseButton(0))
			{
				inspect_timer_click += Time.deltaTime;
			}
			else
			{
				inspect_timer_click = 0f;
			}
			if (!World.world.isAnyPowerSelected())
			{
				checkEmptyClick();
			}
			else
			{
				if (World.world.selected_buttons.selectedButton.godPower == null || already_used_zoom || already_used_camera_drag)
				{
					return;
				}
				GodPower godPower = World.world.selected_buttons.selectedButton.godPower;
				if (!Globals.TRAILER_MODE)
				{
					highlightCursor(godPower);
				}
				if (InputHelpers.touchCount > 1)
				{
					already_used_zoom = true;
					already_used_camera_drag = true;
					already_used_power = false;
				}
				else
				{
					if (InputHelpers.touchCount > 0 && touch_ticks_skip < 5 && godPower.hold_action)
					{
						return;
					}
					bool flag = HotkeyLibrary.many_mod.isHolding() || godPower.hold_action;
					if (!flag && !godPower.ignore_fast_spawn && godPower.type == PowerActionType.PowerSpawnActor && DebugConfig.isOn(DebugOption.FastSpawn))
					{
						flag = true;
					}
					if (flag)
					{
						if (InputHelpers.GetMouseButton(0))
						{
							if (!first_click && DebugConfig.isOn(DebugOption.UltraFastSpawn))
							{
								for (int i = 0; i < 10; i++)
								{
									clickedStart();
								}
							}
							else
							{
								clickedStart();
							}
						}
						else
						{
							((Vector2Int)(ref _last_click)).Set(-1, -1);
							first_pressed_tile = null;
							first_pressed_type = null;
							first_pressed_top_type = null;
							first_click = true;
							click_started_at = Time.time;
						}
					}
					else
					{
						bool flag2 = false;
						if (InputHelpers.touchSupported && InputHelpers.touchCount > 0 && InputHelpers.GetMouseButtonUp(0))
						{
							flag2 = true;
						}
						else if (Input.mousePresent && InputHelpers.GetMouseButtonDown(0))
						{
							flag2 = true;
						}
						if (flag2)
						{
							clickedStart();
							return;
						}
						first_pressed_tile = null;
						first_pressed_type = null;
						first_pressed_top_type = null;
						first_click = true;
					}
				}
			}
		}
	}

	private void updateTouch()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!InputHelpers.touchSupported)
		{
			return;
		}
		if (InputHelpers.touchCount > 0 && InputHelpers.GetMouseButtonDown(0) && isOverUI())
		{
			already_used_power = true;
			return;
		}
		if (InputHelpers.touchCount == 0)
		{
			touch_ticks_skip = 0;
			already_used_zoom = false;
			already_used_camera_drag = false;
			already_used_power = false;
			_origin_touch = Vector2.zero;
			_current_touch = Vector2.zero;
			return;
		}
		if (InputHelpers.touchCount > 0)
		{
			touch_ticks_skip++;
		}
		if (InputHelpers.touchCount == 1)
		{
			Touch touch = Input.GetTouch(0);
			Vector2 position = ((Touch)(ref touch)).position;
			if (_origin_touch == Vector2.zero)
			{
				_origin_touch = position;
			}
			else
			{
				_current_touch = position;
			}
		}
		else
		{
			_origin_touch = Vector2.zero;
		}
		if (!already_used_power && InputHelpers.touchCount > 1 && isTouchMoreThanDragThreshold())
		{
			already_used_camera_drag = true;
		}
	}

	public bool isTouchMoreThanDragThreshold()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (_origin_touch == Vector2.zero || _current_touch == Vector2.zero)
		{
			return false;
		}
		return getCurrentDragDistance() > 0.007f;
	}

	public float getCurrentDragDistance()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (_origin_touch == Vector2.zero || _current_touch == Vector2.zero)
		{
			return 0f;
		}
		float distanceBetweenOriginAndCurrentTouch = getDistanceBetweenOriginAndCurrentTouch();
		float num = Mathf.Sqrt((float)(Screen.width * Screen.width + Screen.height * Screen.height));
		return distanceBetweenOriginAndCurrentTouch / num;
	}

	public float getDistanceBetweenOriginAndCurrentTouch()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_origin_touch == Vector2.zero || _current_touch == Vector2.zero)
		{
			return 0f;
		}
		return Toolbox.DistVec2Float(_origin_touch, _current_touch);
	}

	private bool checkSquareMultiSelection()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (ControllableUnit.isControllingUnit())
		{
			return false;
		}
		if (World.world.isAnyPowerSelected())
		{
			return false;
		}
		if (ScrollWindow.isWindowActive() || ScrollWindow.isAnimationActive())
		{
			return false;
		}
		if (MapBox.isRenderMiniMap())
		{
			square_selection_started = false;
			_ignore_square_selection = true;
			return true;
		}
		if (!InputHelpers.mouseSupported)
		{
			return false;
		}
		if (!InputHelpers.GetMouseButton(0))
		{
			return false;
		}
		if (InputHelpers.GetMouseButton(1))
		{
			return false;
		}
		if (InputHelpers.GetMouseButton(2))
		{
			return false;
		}
		if (!square_selection_started && !_ignore_square_selection)
		{
			if (isOverUI())
			{
				_ignore_square_selection = true;
			}
			else
			{
				square_selection_started = true;
				square_selection_position_current = getMousePos();
			}
		}
		return true;
	}

	private void finishSquareSelection()
	{
		if (!_ignore_square_selection && square_selection_started)
		{
			checkSelectedUnits();
			((Vector2)(ref square_selection_position_start_last)).Set(square_selection_position_current.x, square_selection_position_current.y);
			_square_selection_ended_frame = Time.frameCount;
		}
		_ignore_square_selection = false;
		square_selection_started = false;
		((Vector2)(ref square_selection_position_current)).Set(-1f, -1f);
	}

	private void checkSelectedUnits()
	{
		using ListPool<Actor> listPool = getUnitsToBeSelected();
		if (listPool == null || listPool.Count == 0)
		{
			return;
		}
		if (!HotkeyLibrary.isHoldingControlForSelection() && !HotkeyLibrary.many_mod.isHolding())
		{
			SelectedUnit.clear();
		}
		if (HotkeyLibrary.isHoldingControlForSelection())
		{
			foreach (ref Actor item in listPool)
			{
				SelectedUnit.unselect(item);
			}
		}
		else
		{
			SelectedUnit.selectMultiple(listPool);
		}
		if (SelectedUnit.isSet())
		{
			SelectedObjects.setNanoObject(SelectedUnit.unit);
			if (listPool.Count == 1)
			{
				PowerTabController.showTabSelectedUnit();
			}
			else
			{
				PowerTabController.showTabMultipleUnits();
			}
		}
	}

	public bool isSquareSelectionMinSizeAchievedLast()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = square_selection_position_start_last;
		Vector2 mousePos = getMousePos();
		int num = Mathf.FloorToInt(val.x);
		int num2 = Mathf.FloorToInt(val.y);
		int num3 = Mathf.FloorToInt(mousePos.x);
		int num4 = Mathf.FloorToInt(mousePos.y);
		if (num == -1 || num2 == -1 || num3 == -1 || num4 == -1)
		{
			return false;
		}
		int num5 = Mathf.Min(num, num3);
		int num6 = Mathf.Max(num, num3);
		int num7 = Mathf.Min(num2, num4);
		int num8 = Mathf.Max(num2, num4);
		if (num6 - num5 < 2 && num8 - num7 < 2)
		{
			return false;
		}
		return true;
	}

	public ListPool<Actor> getUnitsToBeSelected()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = square_selection_position_current;
		Vector2 mousePos = getMousePos();
		int num = Mathf.FloorToInt(val.x);
		int num2 = Mathf.FloorToInt(val.y);
		int num3 = Mathf.FloorToInt(mousePos.x);
		int num4 = Mathf.FloorToInt(mousePos.y);
		if (num == -1 || num2 == -1 || num3 == -1 || num4 == -1)
		{
			return null;
		}
		int num5 = Mathf.Min(num, num3);
		int num6 = Mathf.Max(num, num3);
		int num7 = Mathf.Min(num2, num4);
		int num8 = Mathf.Max(num2, num4);
		if (num6 - num5 < 2 && num8 - num7 < 2)
		{
			return null;
		}
		num5 = Mathf.Clamp(num5, 0, MapBox.width - 1);
		num6 = Mathf.Clamp(num6, 0, MapBox.width - 1);
		num7 = Mathf.Clamp(num7, 0, MapBox.height - 1);
		num8 = Mathf.Clamp(num8, 0, MapBox.height - 1);
		ListPool<Actor> tList = new ListPool<Actor>();
		for (int i = num5; i <= num6; i++)
		{
			for (int j = num7; j <= num8; j++)
			{
				World.world.GetTile(i, j)?.doUnits(delegate(Actor tActor)
				{
					if (!tActor.isInsideSomething() && tActor.asset.can_be_inspected)
					{
						tList.Add(tActor);
					}
				});
			}
		}
		return tList;
	}

	internal void clickedFinal(Vector2Int pPos, GodPower pPower = null, bool pTrack = true)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (pPower == null)
		{
			pPower = World.world.selected_buttons.selectedButton.godPower;
		}
		if (pPower.requires_premium && !Config.hasPremium)
		{
			ScrollWindow.showWindow("steam");
			return;
		}
		WorldTile tile = World.world.GetTile(((Vector2Int)(ref pPos)).x, ((Vector2Int)(ref pPos)).y);
		string[] obj = new string[5] { pPower.id, " ", null, null, null };
		Vector2Int pos = tile.pos;
		obj[2] = ((Vector2Int)(ref pos)).x.ToString();
		obj[3] = ":";
		pos = tile.pos;
		obj[4] = ((Vector2Int)(ref pos)).y.ToString();
		LogText.log("Clicked", string.Concat(obj));
		if (pPower.click_special_action != null)
		{
			pPower.click_special_action(tile, pPower.id);
			return;
		}
		if (first_pressed_type == null)
		{
			first_pressed_tile = tile;
			first_pressed_type = tile.main_type;
			first_pressed_top_type = tile.top_type;
		}
		if (pPower.click_interval > 0f)
		{
			if (_click_timer >= 0f)
			{
				_click_timer -= World.world.delta_time;
				return;
			}
			_click_timer = pPower.click_interval;
		}
		if (pPower.click_power_action != null || pPower.click_power_brush_action != null)
		{
			if (pPower.click_power_brush_action != null)
			{
				pPower.click_power_brush_action(tile, pPower);
			}
			else if (pPower.click_power_action != null)
			{
				pPower.click_power_action(tile, pPower);
			}
			if (pTrack)
			{
				PowerTracker.PlusOne(pPower);
			}
		}
		else if (pPower.click_action != null || pPower.click_brush_action != null)
		{
			if (pPower.click_brush_action != null)
			{
				pPower.click_brush_action(tile, pPower.id);
			}
			else if (pPower.click_action != null)
			{
				pPower.click_action(tile, pPower.id);
			}
			if (pTrack)
			{
				PowerTracker.PlusOne(pPower);
			}
		}
	}

	private void checkEmptyClick()
	{
		if (!InputHelpers.GetMouseButtonUp(0))
		{
			return;
		}
		if (!PixelDetector.GetSpritePixelColorUnderMousePointer((MonoBehaviour)(object)World.world, out var pVector) || ((Vector2Int)(ref pVector)).x == -1)
		{
			((Vector2Int)(ref _last_click)).Set(-1, -1);
		}
		else
		{
			if (World.world.GetTile(((Vector2Int)(ref pVector)).x, ((Vector2Int)(ref pVector)).y) == null)
			{
				return;
			}
			foreach (Actor unit in World.world.units)
			{
				if (!(Toolbox.Dist(unit.current_tile.posV3.x, unit.current_tile.posV3.y, ((Vector2Int)(ref pVector)).x, ((Vector2Int)(ref pVector)).y) > 10f))
				{
					unit.asset.action_click?.Invoke(unit, unit.current_tile);
				}
			}
		}
	}

	internal void clickedStart()
	{
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		already_used_power = true;
		if (!PixelDetector.GetSpritePixelColorUnderMousePointer((MonoBehaviour)(object)World.world, out var pVector) || ((Vector2Int)(ref pVector)).x == -1)
		{
			((Vector2Int)(ref _last_click)).Set(-1, -1);
			return;
		}
		GodPower godPower = World.world.selected_buttons.selectedButton.godPower;
		string pID = Config.current_brush;
		if (godPower != null && !string.IsNullOrEmpty(godPower.force_brush))
		{
			pID = godPower.force_brush;
		}
		BrushData brushData = Brush.get(pID);
		if (brushData.continuous && godPower.draw_lines && ((Vector2Int)(ref _last_click)).x != -1 && (((Vector2Int)(ref pVector)).x != ((Vector2Int)(ref _last_click)).x || ((Vector2Int)(ref pVector)).y != ((Vector2Int)(ref _last_click)).y))
		{
			int num = (int)(Toolbox.Dist(((Vector2Int)(ref pVector)).x, ((Vector2Int)(ref pVector)).y, ((Vector2Int)(ref _last_click)).x, ((Vector2Int)(ref _last_click)).y) / (float)(brushData.size + 1)) + 1;
			Vector2 val2 = default(Vector2);
			Vector2Int pPos = default(Vector2Int);
			for (int i = 0; i < num; i++)
			{
				Vector2 val = new Vector2((float)((Vector2Int)(ref pVector)).x, (float)((Vector2Int)(ref pVector)).y);
				((Vector2)(ref val2))._002Ector((float)((Vector2Int)(ref _last_click)).x, (float)((Vector2Int)(ref _last_click)).y);
				Vector2 val3 = Vector2.Lerp(val, val2, (float)i / (float)num);
				((Vector2Int)(ref pPos))._002Ector((int)val3.x, (int)val3.y);
				if (((Vector2Int)(ref pPos)).x >= 0 && ((Vector2Int)(ref pPos)).x < MapBox.width && ((Vector2Int)(ref pPos)).y >= 0 && ((Vector2Int)(ref pPos)).y < MapBox.height)
				{
					clickedFinal(pPos, godPower, pTrack: false);
				}
			}
		}
		clickedFinal(pVector, godPower);
		first_click = false;
		((Vector2Int)(ref _last_click)).Set(((Vector2Int)(ref pVector)).x, ((Vector2Int)(ref pVector)).y);
	}

	private bool tryToMoveSelectedUnits()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported)
		{
			if (!InputHelpers.GetMouseButtonUp(1))
			{
				return false;
			}
		}
		else if (!InputHelpers.GetMouseButtonUp(0))
		{
			return false;
		}
		if (!SelectedUnit.isSet())
		{
			return false;
		}
		if (MoveCamera.camera_drag_activated)
		{
			return false;
		}
		if (already_used_camera_drag)
		{
			return false;
		}
		if (inspect_timer_click > 0.15f)
		{
			return false;
		}
		WorldTile worldTile = getMouseTilePosCachedFrame();
		Vector3 pPos = Vector2.op_Implicit(getMousePos());
		if (worldTile != null)
		{
			pPos = worldTile.posV3;
		}
		BaseEffect baseEffect = EffectsLibrary.spawnAt("fx_move", pPos, 0.1f);
		if ((Object)(object)baseEffect != (Object)null)
		{
			baseEffect.sprite_renderer.color = World.world.getArchitectColor();
		}
		inspect_timer_click = 0.16f;
		if (worldTile == null)
		{
			return false;
		}
		bool flag = HotkeyLibrary.isHoldingControlForSelection();
		Actor actorTargetNearCursor = getActorTargetNearCursor();
		using ListPool<Actor> listPool = new ListPool<Actor>(SelectedUnit.getAllSelected());
		listPool.Remove(SelectedUnit.unit);
		listPool.Insert(0, SelectedUnit.unit);
		foreach (ref Actor item in listPool)
		{
			Actor current = item;
			bool flag2 = false;
			if (flag && actorTargetNearCursor != null)
			{
				current.cancelAllBeh();
				current.addAggro(actorTargetNearCursor);
				current.startFightingWith(actorTargetNearCursor);
				if (Toolbox.DistTile(current.current_tile, actorTargetNearCursor.current_tile) > 7f)
				{
					tryToMoveSelectedUnit(current, worldTile, pCancelBeh: false);
				}
			}
			else
			{
				flag2 = tryToMoveSelectedUnit(current, worldTile);
			}
			if (flag2)
			{
				worldTile = worldTile.getNeighbourTileSameIsland();
			}
		}
		QuantumSpriteLibrary.last_order_timestamp = World.world.getCurSessionTime();
		return true;
	}

	private Actor getActorTargetNearCursor()
	{
		Actor actorNearCursor = World.world.getActorNearCursor();
		if (actorNearCursor == null || !actorNearCursor.isAlive())
		{
			return null;
		}
		return actorNearCursor;
	}

	private bool tryToMoveSelectedUnit(Actor pActor, WorldTile pTile, bool pCancelBeh = true)
	{
		if (!pActor.asset.allow_strange_urge_movement)
		{
			return false;
		}
		if (pTile.Type.block)
		{
			return false;
		}
		if (pActor.asset.id == "dragon" && !pActor.isFlying())
		{
			return false;
		}
		pActor.stopSleeping();
		bool flag = pTile.Type.liquid;
		if (pActor.isWaterCreature() || pActor.asset.is_boat)
		{
			flag = true;
		}
		if (!flag && !pTile.isSameIsland(pActor.current_tile))
		{
			flag = true;
		}
		bool lava = pTile.Type.lava;
		if (pCancelBeh)
		{
			pActor.cancelAllBeh();
			pActor.stopMovement();
		}
		pActor.goTo(pTile, flag, pWalkOnBlocks: false, lava);
		pActor.addStatusEffect("strange_urge", 100f, pColorEffect: false);
		pActor.clearWait();
		return true;
	}

	private bool checkClickTouchInspectSelect()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!canInspectWithMainTouch() && !canInspectWithRightClick())
		{
			return false;
		}
		if (!DebugConfig.isOn(DebugOption.InspectObjectsOnClick))
		{
			return false;
		}
		if (MoveCamera.camera_drag_activated)
		{
			return false;
		}
		if (_over_ui_timeout > 0f)
		{
			return false;
		}
		if (already_used_zoom)
		{
			return false;
		}
		if (already_used_camera_drag)
		{
			return false;
		}
		if (isActionHappening())
		{
			return false;
		}
		if (inspect_timer_click > 0.15f)
		{
			return false;
		}
		if (MoveCamera.inSpectatorMode())
		{
			return false;
		}
		if (World.world.getCurSessionTime() - _last_time_touched_ui < 0.20000000298023224)
		{
			return false;
		}
		if (!(Toolbox.DistVec2Float(_origin_touch, _current_touch) < 20f))
		{
			return false;
		}
		NameplateText cursor_over_text = World.world.nameplate_manager.cursor_over_text;
		if ((Object)(object)cursor_over_text != (Object)null)
		{
			NanoObject nano_object = cursor_over_text.nano_object;
			nano_object.getMetaType().getAsset().selectAndInspect(nano_object, pFromNameplate: true);
			return true;
		}
		WorldTile mouseTilePosCachedFrame = getMouseTilePosCachedFrame();
		if (mouseTilePosCachedFrame == null)
		{
			return true;
		}
		MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
		if (MapBox.isRenderMiniMap() && cachedMapMetaAsset != null && Zones.showMapBorders())
		{
			cachedMapMetaAsset.click_action_zone(mouseTilePosCachedFrame);
		}
		else
		{
			bool flag = false;
			if (HotkeyLibrary.many_mod.isHolding())
			{
				if (SelectedUnit.isSet())
				{
					flag = true;
				}
				if (ActionLibrary.inspectUnitSelectedMeta())
				{
					return true;
				}
			}
			if (isAllowedToSelectUnits())
			{
				Actor actorNearCursor = World.world.getActorNearCursor();
				if (actorNearCursor != null)
				{
					if (SelectedUnit.isSelected(actorNearCursor))
					{
						if (SelectedUnit.isMainSelected(actorNearCursor))
						{
							if (isSquareSelectionMinSizeAchievedLast())
							{
								if (Time.frameCount != _square_selection_ended_frame)
								{
									ActionLibrary.inspectUnit();
								}
							}
							else
							{
								ActionLibrary.inspectUnit();
							}
						}
						else
						{
							SelectedUnit.makeMainSelected(actorNearCursor);
						}
					}
					else
					{
						if (!flag)
						{
							SelectedUnit.clear();
						}
						SelectedUnit.select(actorNearCursor);
						SelectedObjects.setNanoObject(actorNearCursor);
						PowerTabController.showTabSelectedUnit();
					}
				}
			}
			else if (!ScrollWindow.isWindowActive())
			{
				ActionLibrary.inspectUnit();
			}
		}
		return true;
	}

	public void updateCurrentPosition()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		if (_last_check == Time.frameCount)
		{
			return;
		}
		_last_check = Time.frameCount;
		bool flag = false;
		if (InputHelpers.touchSupported && InputHelpers.touchCount != 0)
		{
			if (_event_data_current_position == null)
			{
				_event_data_current_position = new PointerEventData(EventSystem.current);
			}
			PointerEventData event_data_current_position = _event_data_current_position;
			Touch touch = Input.GetTouch(0);
			float x = ((Touch)(ref touch)).position.x;
			touch = Input.GetTouch(0);
			event_data_current_position.position = new Vector2(x, ((Touch)(ref touch)).position.y);
			flag = true;
		}
		if (!flag && InputHelpers.mouseSupported && Input.mousePosition.x >= 0f && Input.mousePosition.y >= 0f && Input.mousePosition.x <= (float)Screen.width && Input.mousePosition.y <= (float)Screen.height)
		{
			if (_event_data_current_position == null)
			{
				_event_data_current_position = new PointerEventData(EventSystem.current);
			}
			_event_data_current_position.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			flag = true;
		}
		if (!flag)
		{
			_event_data_current_position = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isBusyWithUI()
	{
		if (!ScrollWindow.isWindowActive() && !ScrollWindow.isAnimationActive())
		{
			return isOverUI();
		}
		return true;
	}

	public bool isOverUI(bool pCheckTimeout = true)
	{
		if (pCheckTimeout && _over_ui_timeout > 0f)
		{
			return true;
		}
		if (isPointerOverUIObject())
		{
			return true;
		}
		return false;
	}

	private bool isAllowedToSelectUnits()
	{
		return true;
	}

	private bool canInspectUnitWithCurrentPower()
	{
		if (World.world.isAnyPowerSelected())
		{
			if (World.world.selected_buttons.selectedButton.godPower.allow_unit_selection)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private bool canInspectWithRightClick()
	{
		if (!InputHelpers.mouseSupported)
		{
			return false;
		}
		if (!canInspectUnitWithCurrentPower())
		{
			return false;
		}
		return Input.GetMouseButtonUp(1);
	}

	private bool canInspectWithMainTouch()
	{
		if (!canInspectUnitWithCurrentPower())
		{
			return false;
		}
		return Input.GetMouseButtonUp(0);
	}

	public bool isPointerInGame()
	{
		updateCurrentPosition();
		if (_event_data_current_position == null)
		{
			return false;
		}
		return true;
	}

	public bool isPointerOverUIObject()
	{
		if (_is_pointer_over_ui_object.HasValue)
		{
			return _is_pointer_over_ui_object.Value;
		}
		updateCurrentPosition();
		if (_event_data_current_position == null)
		{
			return false;
		}
		List<RaycastResult> results = _results;
		EventSystem.current.RaycastAll(_event_data_current_position, results);
		_is_pointer_over_ui_object = results.Count > 0;
		return _is_pointer_over_ui_object.Value;
	}

	public bool isPointerOverUIButton()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		updateCurrentPosition();
		if (_event_data_current_position == null)
		{
			return false;
		}
		List<RaycastResult> results = _results;
		EventSystem.current.RaycastAll(_event_data_current_position, results);
		for (int i = 0; i < results.Count; i++)
		{
			RaycastResult val = results[i];
			if (((RaycastResult)(ref val)).isValid)
			{
				GameObject gameObject = ((RaycastResult)(ref val)).gameObject;
				if (gameObject.HasComponent<Button>())
				{
					return true;
				}
				if (gameObject.HasComponent<EventTrigger>())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool isTouchOverUI(Touch pTouch)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if ((int)((Touch)(ref pTouch)).phase == 3 || (int)((Touch)(ref pTouch)).phase == 4)
		{
			return false;
		}
		return EventSystem.current.IsPointerOverGameObject(((Touch)(ref pTouch)).fingerId);
	}

	public bool isPointerOverUIScroll()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		updateCurrentPosition();
		if (_event_data_current_position == null)
		{
			return false;
		}
		List<RaycastResult> results = _results;
		if (results.Count == 0)
		{
			EventSystem.current.RaycastAll(_event_data_current_position, results);
		}
		for (int i = 0; i < results.Count; i++)
		{
			RaycastResult val = results[i];
			if (!((RaycastResult)(ref val)).isValid)
			{
				return false;
			}
			_gui_check_game_object = ((RaycastResult)(ref val)).gameObject;
			if ((Object)(object)_gui_check_game_object == (Object)null)
			{
				return false;
			}
			if (_gui_check_game_object.HasComponent<ScrollRectExtended>())
			{
				return true;
			}
			if (!(((Object)_gui_check_game_object).name == "Scroll View"))
			{
				continue;
			}
			Transform val2 = _gui_check_game_object.transform.Find("Viewport/Content");
			if ((Object)(object)val2 != (Object)null)
			{
				Vector2 sizeDelta = _gui_check_game_object.GetComponent<RectTransform>().sizeDelta;
				if (((Component)val2).GetComponent<RectTransform>().sizeDelta.y > sizeDelta.y)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool isActionHappening()
	{
		if (Earthquake.isQuakeActive())
		{
			return true;
		}
		AutoTesterBot auto_tester = World.world.auto_tester;
		if (auto_tester != null && auto_tester.active)
		{
			return false;
		}
		if (Input.touchSupported && Input.touchCount > 1)
		{
			return true;
		}
		if (Input.mousePresent && (Input.GetMouseButton(0) || Input.GetMouseButton(2)))
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool controlsLocked()
	{
		if (MapBox.instance.tutorial.isActive())
		{
			return true;
		}
		if (Config.lockGameControls)
		{
			return true;
		}
		if (Config.isDraggingItem())
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isControllingUnit()
	{
		if (ControllableUnit.isControllingUnit())
		{
			return true;
		}
		if (MapBox.instance.stack_effects.isLocked())
		{
			return true;
		}
		return false;
	}

	private void highlightCursor(GodPower pPower)
	{
		if (!Config.isComputer && !Config.isEditor)
		{
			return;
		}
		WorldTile mouseTilePos = World.world.getMouseTilePos();
		if (mouseTilePos == null)
		{
			return;
		}
		World.world.flash_effects.flashPixel(mouseTilePos, 10);
		if (pPower != null)
		{
			if (!string.IsNullOrEmpty(pPower.force_brush))
			{
				highlightFrom(mouseTilePos, Brush.get(pPower.force_brush));
			}
			else if (pPower.highlight || pPower.show_tool_sizes)
			{
				highlightFrom(mouseTilePos, Config.current_brush_data);
			}
		}
	}

	private void highlightFrom(WorldTile pTile, BrushData pBrushData)
	{
		for (int i = 0; i < pBrushData.pos.Length; i++)
		{
			WorldTile tile = World.world.GetTile(pBrushData.pos[i].x + pTile.x, pBrushData.pos[i].y + pTile.y);
			if (tile != null)
			{
				World.world.flash_effects.flashPixel(tile, 20);
			}
		}
	}

	public void clearLateUpdate()
	{
		_is_pointer_over_ui_object = null;
		_results.Clear();
	}

	public void clear()
	{
		_results.Clear();
		first_click = true;
		_cached_mouse_tile_pos = null;
	}

	public Vector2 getMousePos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		return Vector2.op_Implicit(World.world.camera.ScreenToWorldPoint(Vector2.op_Implicit(val)));
	}

	public WorldTile getMouseTilePosCachedFrame()
	{
		return _cached_mouse_tile_pos;
	}

	public WorldTile getMouseTilePos()
	{
		if (!PixelDetector.GetSpritePixelColorUnderMousePointer((MonoBehaviour)(object)World.world, out var pVector))
		{
			return null;
		}
		return World.world.GetTile(((Vector2Int)(ref pVector)).x, ((Vector2Int)(ref pVector)).y);
	}

	public bool getTouchPos(out Touch pTouch, bool pOnlyGameplay = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		pTouch = default(Touch);
		bool result = false;
		Touch[] touches = Input.touches;
		foreach (Touch val in touches)
		{
			if (!pOnlyGameplay || !World.world.isTouchOverUI(val))
			{
				pTouch = val;
				result = true;
				break;
			}
		}
		return result;
	}

	internal void checkTrailerModeButtons()
	{
		if (Globals.TRAILER_MODE)
		{
			if (!ScrollWindow.isWindowActive() && Input.GetKeyDown((KeyCode)290))
			{
				((Component)World.world.canvas).gameObject.SetActive(!((Component)World.world.canvas).gameObject.activeSelf);
			}
			if (Input.GetKeyDown((KeyCode)96))
			{
				TrailerModeSettings.startEvent();
			}
			if (Input.GetKeyDown((KeyCode)289))
			{
				DebugConfig.switchOption(DebugOption.FastSpawn);
			}
			if (Input.GetKeyDown((KeyCode)288))
			{
				DebugConfig.switchOption(DebugOption.SonicSpeed);
			}
			if (Input.GetKeyDown((KeyCode)32))
			{
				Config.paused = !Config.paused;
			}
			if (Input.GetKeyDown((KeyCode)257) || Input.GetKeyDown((KeyCode)49))
			{
				World.world.selected_buttons.clickPowerButton(PowerButton.get("seeds"));
			}
			else if (Input.GetKeyDown((KeyCode)258) || Input.GetKeyDown((KeyCode)50))
			{
				World.world.selected_buttons.clickPowerButton(PowerButton.get("tile_shallow_waters"));
			}
			else if (Input.GetKeyDown((KeyCode)259) || Input.GetKeyDown((KeyCode)51))
			{
				World.world.selected_buttons.clickPowerButton(PowerButton.get("fruit_bush"));
			}
			else if (Input.GetKeyDown((KeyCode)260) || Input.GetKeyDown((KeyCode)52))
			{
				World.world.selected_buttons.clickPowerButton(PowerButton.get("cat"));
			}
			else if (Input.GetKeyDown((KeyCode)261) || Input.GetKeyDown((KeyCode)53))
			{
				World.world.selected_buttons.clickPowerButton(PowerButton.get("atomic_bomb"));
			}
			else if (Input.GetKeyDown((KeyCode)262) || Input.GetKeyDown((KeyCode)54))
			{
				World.world.selected_buttons.clickPowerButton(PowerButton.get("czar_bomba"));
			}
		}
	}
}
