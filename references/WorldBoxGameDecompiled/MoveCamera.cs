using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MoveCamera : BaseMapObject
{
	private Vector3 _origin;

	private bool _is_zooming;

	internal const float ORTHOGRAPHIC_SIZE_MIN = 10f;

	internal float orthographic_size_max = 130f;

	private float _target_zoom;

	private Vector3 _first_touch;

	internal Camera main_camera;

	internal static MoveCamera instance;

	private WhooshState _whoosh_state;

	private Action _focus_reached_callback;

	private Action _focus_cancel_callback;

	private float _focus_zoom = -1000000f;

	private float _focus_timer;

	private static Actor _focus_unit;

	private static bool _spectator_mode;

	private static float _touch_dist;

	public static bool camera_drag_activated;

	public static int camera_drag_activated_frame;

	public static bool camera_drag_run;

	private float _last_width;

	private float _last_height;

	private bool _first_touch_on_ui;

	internal float camera_zoom_speed = 5f;

	internal float camera_move_speed = 0.01f;

	internal float camera_move_max = 0.06f;

	private Vector2 _move_velocity;

	private readonly Vector2?[] _old_touch_positions = new Vector2?[2];

	private Vector2 _old_touch_vector;

	private float _old_touch_distance;

	private Rect _visible_bounds;

	private Rect _visible_bounds_without_power_bar;

	public float power_bar_position_y;

	private bool _skip_reset_zoom;

	private bool _mouse_controls_used_last;

	private void Awake()
	{
		instance = this;
		main_camera = Camera.main;
	}

	internal override void create()
	{
		base.create();
		resetZoom();
		_target_zoom = main_camera.orthographicSize;
	}

	public static Actor getFocusUnit()
	{
		return _focus_unit;
	}

	public static void setFocusUnit(Actor pActor)
	{
		_focus_unit = pActor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool hasFocusUnit()
	{
		return _focus_unit != null;
	}

	public static bool isCameraFollowingUnit(Actor pActor)
	{
		return _focus_unit == pActor;
	}

	internal void focusOn(Vector3 pPos)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		clearFocusUnitAndUnselect();
		_target_zoom = 15f;
		_focus_zoom = _target_zoom;
		pPos.z = ((Component)this).transform.position.z;
		((Component)this).transform.position = pPos;
	}

	internal void focusOn(Vector3 pPos, Action pFocusReachedCallback, Action pFocusCancelCallback)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		clearFocusUnitAndUnselect();
		_target_zoom = 15f;
		_focus_zoom = _target_zoom;
		_focus_reached_callback = pFocusReachedCallback;
		_focus_cancel_callback = pFocusCancelCallback;
		pPos.z = ((Component)this).transform.position.z;
		((Component)this).transform.position = pPos;
	}

	internal void focusOnAndFollow(Actor pActor, Action pFocusReachedCallback, Action pFocusCancelCallback)
	{
		clearFocusUnitAndUnselect();
		Config.ui_main_hidden = false;
		_target_zoom = 15f;
		_focus_zoom = _target_zoom;
		_focus_reached_callback = pFocusReachedCallback;
		_focus_cancel_callback = pFocusCancelCallback;
		_focus_unit = pActor;
		_focus_timer = 0f;
		WorldTip.addWordReplacement("$name$", _focus_unit.coloredName);
		WorldTip.showNowTop("tip_following_unit");
		PowerTracker.spectatingUnit(_focus_unit.getName());
		PowerButtonSelector.instance.setPower(PowerButtonSelector.instance.followUnit);
	}

	internal void resetZoom()
	{
		int num = ((Screen.width >= Screen.height) ? (Screen.height / 4) : (Screen.width / 4));
		if (MapBox.width > MapBox.height)
		{
			orthographic_size_max = (int)((float)MapBox.width * 1.1f);
		}
		else
		{
			orthographic_size_max = (int)((float)MapBox.height * 1.1f);
		}
		if ((float)num > orthographic_size_max)
		{
			num = (int)orthographic_size_max;
		}
		_target_zoom = num;
		main_camera.orthographicSize = Mathf.Clamp(_target_zoom, 10f, orthographic_size_max);
		World.world.setZoomOrthographic(main_camera.orthographicSize);
		_mouse_controls_used_last = false;
		main_camera.farClipPlane = (float)MapBox.height * 1.1f;
	}

	public void forceZoom(float pZoom)
	{
		_target_zoom = pZoom;
		zoomToBounds(pForce: true);
	}

	public void setTargetZoom(float pValue)
	{
		_target_zoom = pValue;
	}

	public float getTargetZoom()
	{
		return _target_zoom;
	}

	private void updateZoomControls()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.touchSupported)
		{
			bool flag = false;
			if (UltimateJoystick.getJoyCount() == 2)
			{
				flag = UltimateJoystick.GetJoystickState("JoyRight") || UltimateJoystick.GetJoystickState("JoyLeft");
			}
			if (flag)
			{
				return;
			}
			bool flag2 = !World.world.player_control.already_used_power || ControllableUnit.isControllingUnit();
			if ((InputHelpers.touchCount == 2) & flag2)
			{
				World.world.player_control.already_used_zoom = true;
				Touch touch = Input.GetTouch(0);
				Touch touch2 = Input.GetTouch(1);
				Vector2 val = ((Touch)(ref touch)).position - ((Touch)(ref touch)).deltaPosition;
				Vector2 val2 = ((Touch)(ref touch2)).position - ((Touch)(ref touch2)).deltaPosition;
				Vector2 val3 = val - val2;
				float magnitude = ((Vector2)(ref val3)).magnitude;
				val3 = ((Touch)(ref touch)).position - ((Touch)(ref touch2)).position;
				float magnitude2 = ((Vector2)(ref val3)).magnitude;
				float num = magnitude - magnitude2;
				_target_zoom += num * 0.2f * (main_camera.orthographicSize * 0.015f);
			}
		}
		if (inSpectatorMode())
		{
			followFocusUnit();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool inSpectatorMode()
	{
		if (_spectator_mode && !hasFocusUnit())
		{
			instance.clearFocusUnitAndUnselect();
		}
		_spectator_mode = hasFocusUnit();
		return _spectator_mode;
	}

	private void checkFocusReached()
	{
		if (main_camera.orthographicSize == _focus_zoom)
		{
			if (_focus_reached_callback != null)
			{
				_focus_reached_callback();
			}
			clearFocus();
		}
		if (_target_zoom != _focus_zoom)
		{
			if (_focus_cancel_callback != null)
			{
				_focus_cancel_callback();
			}
			clearFocus();
		}
	}

	private void followFocusUnit()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		if (!hasFocusUnit())
		{
			return;
		}
		Actor focus_unit = _focus_unit;
		if (!focus_unit.isAlive())
		{
			Actor actor = focus_unit.attackedBy?.a;
			if (actor != null && actor.isAlive())
			{
				WorldTip.addWordReplacement("$name$", focus_unit.coloredName);
				WorldTip.addWordReplacement("$killer$", actor.coloredName);
				WorldTip.showNowTop("tip_followed_unit_killed");
				Actor a = actor.a;
				focus_unit.attackedBy = null;
				setFocusUnit(a);
				_focus_timer = 0f;
			}
			else
			{
				WorldTip.addWordReplacement("$name$", focus_unit.coloredName);
				WorldTip.showNowTop("tip_followed_unit_died");
				clearFocusUnitAndUnselect();
			}
		}
		else if (camera_drag_run || InputHelpers.touchCount > 0)
		{
			_focus_timer = 0f;
		}
		else
		{
			Vector3 val = Vector2.op_Implicit(focus_unit.current_position);
			val.z = ((Component)this).transform.position.z;
			if (_focus_timer <= 1f)
			{
				_focus_timer += Time.deltaTime;
				_focus_timer = Mathf.Clamp(_focus_timer, 0f, 1f);
				val.x = iTween.easeOutCubic(((Component)this).transform.position.x, val.x, _focus_timer);
				val.y = iTween.easeOutCubic(((Component)this).transform.position.y, val.y, _focus_timer);
			}
			((Component)this).transform.position = val;
		}
	}

	private void clearFocus()
	{
		_focus_reached_callback = null;
		_focus_cancel_callback = null;
		_focus_zoom = -1000000f;
	}

	public static void clearFocusUnitOnly()
	{
		_focus_unit = null;
	}

	internal void clearFocusUnitAndUnselect()
	{
		clearFocusUnitOnly();
		_focus_timer = 0f;
		if (World.world.isSelectedPower("follow_unit"))
		{
			PowerButtonSelector.instance.unselectAll();
		}
	}

	private void zoomToBounds(bool pForce = false)
	{
		float num = (World.world.player_control.isSelectionHappens() ? World.world.quality_changer.getZoomRateBoundLow() : orthographic_size_max);
		_target_zoom = Mathf.Clamp(_target_zoom, 10f, num);
		if (main_camera.orthographicSize == _target_zoom)
		{
			return;
		}
		if (_target_zoom > main_camera.orthographicSize)
		{
			Camera obj = main_camera;
			obj.orthographicSize += Time.deltaTime * camera_zoom_speed * (Mathf.Abs(main_camera.orthographicSize - _target_zoom) + 5f);
			if (main_camera.orthographicSize > _target_zoom)
			{
				main_camera.orthographicSize = Mathf.Clamp(_target_zoom, 10f, orthographic_size_max);
			}
		}
		else if (_target_zoom < main_camera.orthographicSize)
		{
			Camera obj2 = main_camera;
			obj2.orthographicSize -= Time.deltaTime * camera_zoom_speed * (Mathf.Abs(main_camera.orthographicSize - _target_zoom) + 5f);
			if (main_camera.orthographicSize < _target_zoom)
			{
				main_camera.orthographicSize = Mathf.Clamp(_target_zoom, 10f, orthographic_size_max);
			}
		}
		if (pForce)
		{
			main_camera.orthographicSize = _target_zoom;
		}
		World.world.setZoomOrthographic(main_camera.orthographicSize);
	}

	private void updateMouseCameraDrag()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		if (ControllableUnit.isControllingUnit())
		{
			return;
		}
		camera_drag_run = false;
		bool flag = false;
		bool flag2 = false;
		if (InputHelpers.mouseSupported)
		{
			flag = checkMouseInputDown();
			flag2 = checkMouseInput();
		}
		if (!flag2)
		{
			clearTouches();
			return;
		}
		if (flag && World.world.isOverUI())
		{
			clearTouches();
			return;
		}
		if (flag && _origin.x == -1f && _origin.z == -1f)
		{
			_origin = getMousePos();
		}
		if ((_origin.x == -1f && _origin.y == -1f && _origin.z == -1f) || !flag2)
		{
			return;
		}
		camera_drag_run = true;
		Vector3 position = ((Component)this).transform.position;
		position.z = 0f;
		Vector3 val = getMousePos() - position;
		if (Toolbox.DistVec3(_origin, getMousePos()) > 0.1f)
		{
			camera_drag_activated = true;
			camera_drag_activated_frame = Time.frameCount;
		}
		Vector3 val2 = _origin - val;
		val2.z = 0f;
		if (InputHelpers.touchSupported)
		{
			_touch_dist = Toolbox.DistVec3(_first_touch, getTouchPos(pScreenCoords: true));
			if (World.world.player_control.touch_ticks_skip > 5)
			{
				if (_touch_dist >= 20f || (float)World.world.player_control.touch_ticks_skip > 0.3f)
				{
					World.world.player_control.already_used_zoom = true;
					World.world.player_control.already_used_power = false;
				}
			}
			else if (InputHelpers.touchCount == 1)
			{
				return;
			}
		}
		if (InputHelpers.mouseSupported)
		{
			Vector3 val3 = position;
			((Component)this).transform.position = val2;
			Vector2 val4 = Vector2.op_Implicit(val2 - val3);
			if (((Vector2)(ref val4)).magnitude > 0.01f)
			{
				Vector2 val5 = val4 * 0.2f;
				addVelocity(val5.x, val5.y);
				_mouse_controls_used_last = true;
			}
			else
			{
				_move_velocity = Vector2.zero;
			}
			checkDistanceMoved(val3);
			cameraToBounds();
		}
	}

	private void updateVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		Vector2 move_velocity = _move_velocity;
		if (move_velocity.x != 0f || move_velocity.y != 0f)
		{
			float decayFactor = getDecayFactor();
			_move_velocity *= decayFactor;
			if (Mathf.Abs(_move_velocity.x) < 0.01f)
			{
				_move_velocity.x = 0f;
			}
			if (Mathf.Abs(_move_velocity.y) < 0.01f)
			{
				_move_velocity.y = 0f;
			}
			if (!InputHelpers.mouseSupported || !InputHelpers.GetMouseButton(1))
			{
				Vector3 val = Vector2.op_Implicit(_move_velocity);
				Transform transform = ((Component)this).transform;
				transform.position += val;
				setWhooshState(WhooshState.NeedWhoosh);
				cameraToBounds();
			}
		}
	}

	private float getDecayFactor()
	{
		if (_mouse_controls_used_last)
		{
			return Mathf.Pow(0.8f, Time.deltaTime / (1f / 60f));
		}
		return 0.8f;
	}

	private void checkDistanceMoved(Vector3 pOldPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		float num = Toolbox.DistVec3(((Component)this).transform.position, pOldPosition);
		Vector3 val = main_camera.ScreenToWorldPoint(new Vector3(0f, 0f, main_camera.nearClipPlane));
		Vector3 val2 = main_camera.ScreenToWorldPoint(new Vector3((float)Screen.width, (float)Screen.height, main_camera.nearClipPlane)) - val;
		float num2 = ((Vector3)(ref val2)).magnitude * 0.007f;
		if (num > num2)
		{
			GodPower selected_power = World.world.selected_power;
			if (selected_power != null && selected_power.set_used_camera_drag_on_long_move)
			{
				World.world.player_control.already_used_camera_drag = true;
			}
		}
		if (num > num2 * 1.2f)
		{
			setWhooshState(WhooshState.NeedWhoosh);
		}
	}

	private bool checkMouseInputDown()
	{
		if (InputHelpers.GetMouseButtonDown(1))
		{
			return true;
		}
		if (InputHelpers.GetMouseButtonDown(2))
		{
			return true;
		}
		if (InputHelpers.GetMouseButtonDown(0))
		{
			if (!Input.mousePresent)
			{
				return true;
			}
			if (MapBox.isRenderMiniMap())
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool checkMouseInput()
	{
		if (InputHelpers.GetMouseButton(1))
		{
			return true;
		}
		if (InputHelpers.GetMouseButton(2))
		{
			return true;
		}
		if (InputHelpers.GetMouseButton(0))
		{
			if (!Input.mousePresent)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private void clearTouches()
	{
		((Vector3)(ref _first_touch)).Set(-1f, -1f, -1f);
		((Vector3)(ref _origin)).Set(-1f, -1f, -1f);
		if (camera_drag_activated && Time.frameCount > camera_drag_activated_frame + 2)
		{
			camera_drag_activated = false;
		}
	}

	private void cameraToBounds()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = new Vector3
		{
			x = Mathf.Clamp(((Component)this).transform.position.x, 0f, (float)MapBox.width),
			y = Mathf.Clamp(((Component)this).transform.position.y, 0f, (float)MapBox.height),
			z = -0.5f
		};
		((Component)this).transform.position = position;
		World.world.nameplate_manager.update();
	}

	private Vector3 getTouchPos(bool pScreenCoords = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		int num = 0;
		int touchCount = InputHelpers.touchCount;
		for (int i = 0; i < touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if ((int)((Touch)(ref touch)).phase != 4 && (int)((Touch)(ref touch)).phase != 3)
			{
				val += ((Touch)(ref touch)).position;
				num++;
			}
		}
		Vector3 val2 = Vector2.op_Implicit(val / (float)num);
		if (pScreenCoords)
		{
			return val2;
		}
		return main_camera.ScreenToWorldPoint(val2);
	}

	private Vector3 getMousePos()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.mouseSupported)
		{
			return Vector2.op_Implicit(World.world.getMousePos());
		}
		return Vector3.one;
	}

	private void setWhooshState(WhooshState pState)
	{
		if (pState != WhooshState.NeedWhoosh || _whoosh_state != WhooshState.WhooshPlayed)
		{
			_whoosh_state = pState;
		}
	}

	private bool isNoInputDetected()
	{
		if (_move_velocity.x == 0f && _move_velocity.y == 0f && InputHelpers.touchCount == 0)
		{
			if (!InputHelpers.GetMouseButton(0) && !InputHelpers.GetMouseButton(1))
			{
				return !InputHelpers.GetMouseButton(2);
			}
			return false;
		}
		return false;
	}

	private void LateUpdate()
	{
		updateVisibleBounds();
		if (!World.world.tutorial.isActive())
		{
			if (_whoosh_state == WhooshState.NeedWhoosh)
			{
				setWhooshState(WhooshState.WhooshPlayed);
			}
			if (isNoInputDetected())
			{
				setWhooshState(WhooshState.Idle);
			}
		}
	}

	private void updateVisibleBounds()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		Vector3 powerBarLeftCornerViewportPos = ToolbarButtons.instance.getPowerBarLeftCornerViewportPos();
		Vector2 val = Vector2.op_Implicit(World.world.camera.ScreenToWorldPoint(powerBarLeftCornerViewportPos));
		power_bar_position_y = val.y;
		if (power_bar_position_y < 0f)
		{
			power_bar_position_y = 0f;
		}
		Camera obj = main_camera;
		float nearClipPlane = obj.nearClipPlane;
		Vector3 val2 = obj.ViewportToWorldPoint(new Vector3(0f, 0f, nearClipPlane));
		Vector3 val3 = obj.ViewportToWorldPoint(new Vector3(1f, 1f, nearClipPlane));
		((Rect)(ref _visible_bounds)).x = val2.x;
		((Rect)(ref _visible_bounds)).y = val2.y;
		((Rect)(ref _visible_bounds)).width = val3.x - ((Rect)(ref _visible_bounds)).x;
		((Rect)(ref _visible_bounds)).height = val3.y - ((Rect)(ref _visible_bounds)).y;
		((Rect)(ref _visible_bounds_without_power_bar)).x = val2.x;
		((Rect)(ref _visible_bounds_without_power_bar)).y = power_bar_position_y;
		((Rect)(ref _visible_bounds_without_power_bar)).width = val3.x - ((Rect)(ref _visible_bounds_without_power_bar)).x;
		((Rect)(ref _visible_bounds_without_power_bar)).height = val3.y - ((Rect)(ref _visible_bounds_without_power_bar)).y;
	}

	public bool isWithinCameraView(Vector2 pPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Rect visible_bounds = _visible_bounds;
		return checkBounds(pPos, visible_bounds);
	}

	public bool isWithinCameraViewNotPowerBar(Vector2 pPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Rect visible_bounds_without_power_bar = _visible_bounds_without_power_bar;
		return checkBounds(pPos, visible_bounds_without_power_bar);
	}

	private bool checkBounds(Vector2 pPos, Rect pBounds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return ((Rect)(ref pBounds)).Contains(pPos);
	}

	public void update()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.tutorial.isActive())
		{
			return;
		}
		int pixelWidth = main_camera.pixelWidth;
		int pixelHeight = main_camera.pixelHeight;
		if (_last_width != (float)pixelWidth || _last_height != (float)pixelHeight)
		{
			_last_width = pixelWidth;
			_last_height = pixelHeight;
			if (_skip_reset_zoom)
			{
				_skip_reset_zoom = false;
			}
			else
			{
				resetZoom();
			}
			return;
		}
		if (Globals.TRAILER_MODE)
		{
			updateTrailerMode();
		}
		if (InputHelpers.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			if ((int)((Touch)(ref touch)).phase == 0 && World.world.isOverUI())
			{
				_first_touch_on_ui = true;
			}
		}
		else
		{
			_first_touch_on_ui = false;
		}
		if (!ScrollWindow.isWindowActive() && (!World.world.isOverUI() || inSpectatorMode()))
		{
			updateZoomControls();
		}
		if (_target_zoom != main_camera.orthographicSize)
		{
			zoomToBounds();
		}
		if (_focus_zoom > -1000000f)
		{
			checkFocusReached();
		}
		if (World.world.isGameplayControlsLocked() || ScrollWindow.isAnimationActive() || _first_touch_on_ui)
		{
			clearTouches();
			_old_touch_positions[0] = null;
			_old_touch_positions[1] = null;
			return;
		}
		if (InputHelpers.touchSupported)
		{
			updateMobileCamera();
		}
		if (InputHelpers.mouseSupported && (!InputHelpers.touchSupported || InputHelpers.touchCount <= 0))
		{
			updateMouseCameraDrag();
			if (!ScrollWindow.isWindowActive() && !ControllableUnit.isControllingUnit())
			{
				updateVelocity();
			}
		}
	}

	public Vector2 getVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _move_velocity;
	}

	private bool ignoreTouchControls()
	{
		if (!World.world.isOverUI() && !ScrollWindow.isWindowActive())
		{
			return ScrollWindow.isAnimationActive();
		}
		return true;
	}

	private void updateMobileCamera()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		if (InputHelpers.touchCount == 0)
		{
			_old_touch_positions[0] = null;
			_old_touch_positions[1] = null;
		}
		else
		{
			if ((World.world.isAnyPowerSelected() && World.world.selected_power.hold_action && InputHelpers.touchCount == 1) || World.world.player_control.already_used_power || ControllableUnit.isControllingUnit())
			{
				return;
			}
			Vector3 position = ((Component)this).transform.position;
			Touch touch;
			if (InputHelpers.touchCount == 1)
			{
				if (!_old_touch_positions[0].HasValue || _old_touch_positions[1].HasValue)
				{
					Vector2?[] old_touch_positions = _old_touch_positions;
					touch = Input.GetTouch(0);
					old_touch_positions[0] = ((Touch)(ref touch)).position;
					_old_touch_positions[1] = null;
				}
				else
				{
					touch = Input.GetTouch(0);
					Vector2 position2 = ((Touch)(ref touch)).position;
					Vector3 position3 = ((Component)this).transform.position;
					Transform transform = ((Component)this).transform;
					Vector2? val = _old_touch_positions[0];
					Vector2 val2 = position2;
					Vector2? val3 = (val.HasValue ? new Vector2?(val.GetValueOrDefault() - val2) : ((Vector2?)null));
					float orthographicSize = main_camera.orthographicSize;
					Vector2? val4 = (val3.HasValue ? new Vector2?(val3.GetValueOrDefault() * orthographicSize) : ((Vector2?)null));
					float num = main_camera.pixelHeight;
					Vector3 val5 = transform.TransformDirection(Vector2.op_Implicit((val4.HasValue ? new Vector2?(val4.GetValueOrDefault() / num * 2f) : ((Vector2?)null)).Value));
					Vector3 position4 = position3 + val5;
					((Component)this).transform.position = position4;
					_old_touch_positions[0] = position2;
					cameraToBounds();
				}
			}
			else if (!_old_touch_positions[1].HasValue)
			{
				Vector2?[] old_touch_positions2 = _old_touch_positions;
				touch = Input.GetTouch(0);
				old_touch_positions2[0] = ((Touch)(ref touch)).position;
				Vector2?[] old_touch_positions3 = _old_touch_positions;
				touch = Input.GetTouch(1);
				old_touch_positions3[1] = ((Touch)(ref touch)).position;
				Vector2? val4 = _old_touch_positions[0];
				Vector2? val3 = _old_touch_positions[1];
				_old_touch_vector = ((val4.HasValue & val3.HasValue) ? new Vector2?(val4.GetValueOrDefault() - val3.GetValueOrDefault()) : ((Vector2?)null)).Value;
				_old_touch_distance = ((Vector2)(ref _old_touch_vector)).magnitude;
			}
			else
			{
				Vector2 val6 = default(Vector2);
				((Vector2)(ref val6))._002Ector((float)main_camera.pixelWidth, (float)main_camera.pixelHeight);
				Vector2[] array = new Vector2[2];
				touch = Input.GetTouch(0);
				array[0] = ((Touch)(ref touch)).position;
				touch = Input.GetTouch(1);
				array[1] = ((Touch)(ref touch)).position;
				Vector2[] array2 = (Vector2[])(object)array;
				Vector2 old_touch_vector = array2[0] - array2[1];
				float magnitude = ((Vector2)(ref old_touch_vector)).magnitude;
				Transform transform2 = ((Component)this).transform;
				Vector3 position5 = transform2.position;
				Transform transform3 = ((Component)this).transform;
				Vector2? val7 = _old_touch_positions[0];
				Vector2? val8 = _old_touch_positions[1];
				Vector2? val = ((val7.HasValue & val8.HasValue) ? new Vector2?(val7.GetValueOrDefault() + val8.GetValueOrDefault()) : ((Vector2?)null));
				Vector2 val2 = val6;
				Vector2? val3 = (val.HasValue ? new Vector2?(val.GetValueOrDefault() - val2) : ((Vector2?)null));
				float orthographicSize = main_camera.orthographicSize;
				Vector2? val4 = (val3.HasValue ? new Vector2?(val3.GetValueOrDefault() * orthographicSize) : ((Vector2?)null));
				float num = val6.y;
				transform2.position = position5 + transform3.TransformDirection(Vector2.op_Implicit((val4.HasValue ? new Vector2?(val4.GetValueOrDefault() / num) : ((Vector2?)null)).Value));
				if (magnitude != 0f && _old_touch_distance != magnitude)
				{
					main_camera.orthographicSize = Mathf.Clamp(main_camera.orthographicSize * (_old_touch_distance / magnitude), 10f, orthographic_size_max);
				}
				World.world.setZoomOrthographic(main_camera.orthographicSize);
				Transform transform4 = ((Component)this).transform;
				transform4.position -= ((Component)this).transform.TransformDirection(Vector2.op_Implicit((array2[0] + array2[1] - val6) * main_camera.orthographicSize / val6.y));
				cameraToBounds();
				_old_touch_positions[0] = array2[0];
				_old_touch_positions[1] = array2[1];
				_old_touch_vector = old_touch_vector;
				_old_touch_distance = magnitude;
				World.world.player_control.already_used_zoom = true;
			}
			checkDistanceMoved(position);
		}
	}

	private static float getMoveDistance(bool pFast = false)
	{
		float num = Time.deltaTime * 55f;
		if (pFast)
		{
			num *= 2.5f;
		}
		return num * instance._target_zoom * instance.camera_move_speed;
	}

	public static void move(HotkeyAsset pAsset)
	{
		float moveDistance = getMoveDistance(pAsset.id.StartsWith("fast_"));
		switch (pAsset.id)
		{
		case "up":
		case "fast_up":
			instance.addVelocity(0f, moveDistance);
			break;
		case "down":
		case "fast_down":
			instance.addVelocity(0f, 0f - moveDistance);
			break;
		case "right":
		case "fast_right":
			instance.addVelocity(moveDistance, 0f);
			break;
		case "left":
		case "fast_left":
			instance.addVelocity(0f - moveDistance, 0f);
			break;
		}
		instance.clampVelocity();
		instance._mouse_controls_used_last = false;
	}

	private void addVelocity(float pX, float pY)
	{
		_move_velocity.x += pX;
		_move_velocity.y += pY;
	}

	private void clampVelocity()
	{
		float num = (0f - _target_zoom) * camera_move_max;
		float num2 = _target_zoom * camera_move_max;
		_move_velocity.y = Mathf.Clamp(_move_velocity.y, num, num2);
		_move_velocity.x = Mathf.Clamp(_move_velocity.x, num, num2);
	}

	public static void zoomIn(HotkeyAsset pAsset)
	{
		instance._target_zoom -= instance.main_camera.orthographicSize * 0.05f;
	}

	public static void zoomOut(HotkeyAsset pAsset)
	{
		instance._target_zoom += instance.main_camera.orthographicSize * 0.05f;
	}

	public static void zoomInWheel(HotkeyAsset pAsset)
	{
		instance._target_zoom -= instance.main_camera.orthographicSize * 0.2f;
	}

	public static void zoomOutWheel(HotkeyAsset pAsset)
	{
		instance._target_zoom += instance.main_camera.orthographicSize * 0.2f;
	}

	private void updateTrailerMode()
	{
		if (Input.GetKeyUp((KeyCode)291))
		{
			camera_zoom_speed -= 0.2f;
			if (camera_zoom_speed < 0f)
			{
				camera_zoom_speed = 0.2f;
			}
		}
		if (Input.GetKeyUp((KeyCode)292))
		{
			camera_zoom_speed += 0.2f;
		}
		if (Input.GetKeyUp((KeyCode)111))
		{
			camera_move_max -= 0.1f;
			if (camera_move_max < 0.01f)
			{
				camera_move_max = 0.01f;
			}
		}
		if (Input.GetKeyUp((KeyCode)112))
		{
			camera_move_max += 0.1f;
		}
		if (Input.GetKeyUp((KeyCode)107))
		{
			camera_move_speed -= 0.01f;
			if (camera_move_speed < 0.01f)
			{
				camera_move_speed = 0.01f;
			}
		}
		if (Input.GetKeyUp((KeyCode)108))
		{
			camera_move_speed += 0.01f;
		}
		if (Input.GetKeyDown((KeyCode)114) && _target_zoom != main_camera.orthographicSize)
		{
			if (_target_zoom > main_camera.orthographicSize)
			{
				_target_zoom = main_camera.orthographicSize + _target_zoom * 0.1f;
			}
			else
			{
				_target_zoom = main_camera.orthographicSize - _target_zoom * 0.1f;
			}
		}
	}

	public void debug(DebugTool pTool)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		pTool.setText("bounds_normal:", _visible_bounds, 0f, pShowBar: false, 0L);
		pTool.setText("bounds_wth_power_bar:", _visible_bounds_without_power_bar, 0f, pShowBar: false, 0L);
		pTool.setText("is_no_input_detected:", isNoInputDetected(), 0f, pShowBar: false, 0L);
		pTool.setText("_whooshState:", _whoosh_state, 0f, pShowBar: false, 0L);
		pTool.setText("InputHelpers.touchCount:", InputHelpers.touchCount, 0f, pShowBar: false, 0L);
		pTool.setText("world.isGameplayControlsLocked():", World.world.isGameplayControlsLocked(), 0f, pShowBar: false, 0L);
		pTool.setText("ScrollWindow.animationActive:", ScrollWindow.isAnimationActive(), 0f, pShowBar: false, 0L);
		pTool.setText("firstTouchOnUI", _first_touch_on_ui, 0f, pShowBar: false, 0L);
		pTool.setText("world.alreadyUsedZoom", World.world.player_control.already_used_zoom, 0f, pShowBar: false, 0L);
		pTool.setText("world.alreadyUsedPower", World.world.player_control.already_used_power, 0f, pShowBar: false, 0L);
		pTool.setText("world.already_used_camera_drag", World.world.player_control.already_used_camera_drag, 0f, pShowBar: false, 0L);
		pTool.setText("_touch_dist", _touch_dist, 0f, pShowBar: false, 0L);
		pTool.setText("cameraDragRun", camera_drag_run, 0f, pShowBar: false, 0L);
		pTool.setText("camera_drag_activated", camera_drag_activated, 0f, pShowBar: false, 0L);
		if (UltimateJoystick.getJoyCount() == 2)
		{
			pTool.setText("JoyRight", UltimateJoystick.GetJoystickState("JoyRight"), 0f, pShowBar: false, 0L);
			pTool.setText("JoyLeft", UltimateJoystick.GetJoystickState("JoyLeft"), 0f, pShowBar: false, 0L);
		}
	}

	public void skipResetZoom()
	{
		_skip_reset_zoom = true;
	}
}
