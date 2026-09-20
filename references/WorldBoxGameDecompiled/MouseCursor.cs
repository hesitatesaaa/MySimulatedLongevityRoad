using UnityEngine;

public class MouseCursor : MonoBehaviour
{
	private const int CURSOR_DEFAULT_0 = 0;

	private const int CURSOR_DOWN_1 = 1;

	private const int CURSOR_UP_2 = 2;

	private const int CURSOR_HOLD_6 = 6;

	private const int CURSOR_DRAG_7 = 7;

	private const int CURSOR_PINKIE_8 = 8;

	private const int CURSOR_SPRINKLE_13 = 13;

	private const int CURSOR_DRAW_17 = 17;

	private const int CURSOR_ATTACK = 22;

	private const int CURSOR_INSPECT = 26;

	private const int CURSOR_OVER_UI = 27;

	private const int CLICK_FRAMES = 6;

	private const int UP_FRAMES = 4;

	private const int PINKIE_FRAMES = 5;

	private const int SPRINKLE_FRAMES = 4;

	private const int DRAW_FRAMES = 5;

	private const int ATTACK_FRAMES = 4;

	private const int ANIM_SPEED = 5;

	public Texture2D mouseCursorDefault;

	public Texture2D mouseCursorDown;

	public Texture2D mouseCursorUp1;

	public Texture2D mouseCursorUp2;

	public Texture2D mouseCursorUp3;

	public Texture2D mouseCursorUp4;

	public Texture2D mouseCursorHold;

	public Texture2D mouseCursorDrag;

	public Texture2D mouseCursorPinkie1;

	public Texture2D mouseCursorPinkie2;

	public Texture2D mouseCursorPinkie3;

	public Texture2D mouseCursorPinkie4;

	public Texture2D mouseCursorPinkie5;

	public Texture2D mouseCursorSprinkle1;

	public Texture2D mouseCursorSprinkle2;

	public Texture2D mouseCursorSprinkle3;

	public Texture2D mouseCursorSprinkle4;

	public Texture2D mouseCursorDraw1;

	public Texture2D mouseCursorDraw2;

	public Texture2D mouseCursorDraw3;

	public Texture2D mouseCursorDraw4;

	public Texture2D mouseCursorDraw5;

	public Texture2D mouseCursorAttack1;

	public Texture2D mouseCursorAttack2;

	public Texture2D mouseCursorAttack3;

	public Texture2D mouseCursorAttack4;

	public Texture2D mouseCursorInspect;

	public Texture2D mouseCursorOverUI;

	private static int _counter = 0;

	private static bool _pressed = false;

	private static int _pressing = 0;

	private static int _dragged = 0;

	private static bool _right = false;

	private static bool _middle = false;

	private static bool _anim_done = true;

	private static int _cur_cursor = -1;

	private static Texture2D[] _cursors;

	private static int _last_texture_id = -1;

	private static bool _can_drag = false;

	private static bool _selected_unit_attack = false;

	private static MouseHoldAnimation _mouse_hold_animation = MouseHoldAnimation.Default;

	private static Texture2D _selected_cursor_texture;

	private void Awake()
	{
		if (!Input.mousePresent)
		{
			Object.Destroy((Object)(object)this);
			return;
		}
		_cursors = (Texture2D[])(object)new Texture2D[28]
		{
			mouseCursorDefault, mouseCursorDown, mouseCursorUp1, mouseCursorUp2, mouseCursorUp3, mouseCursorUp4, mouseCursorHold, mouseCursorDrag, mouseCursorPinkie1, mouseCursorPinkie2,
			mouseCursorPinkie3, mouseCursorPinkie4, mouseCursorPinkie5, mouseCursorSprinkle1, mouseCursorSprinkle2, mouseCursorSprinkle3, mouseCursorSprinkle4, mouseCursorDraw1, mouseCursorDraw2, mouseCursorDraw3,
			mouseCursorDraw4, mouseCursorDraw5, mouseCursorAttack1, mouseCursorAttack2, mouseCursorAttack3, mouseCursorAttack4, mouseCursorInspect, mouseCursorOverUI
		};
	}

	private void OnEnable()
	{
		setCursorDefault();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
		{
			if (!_pressed)
			{
				_pressing = 0;
				_dragged = 0;
				_counter = 0;
			}
			_pressed = true;
			_anim_done = false;
			_right = Input.GetMouseButtonDown(1);
			_middle = Input.GetMouseButtonDown(2);
		}
		else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
		{
			if (!Input.anyKey)
			{
				_pressed = false;
				_pressing = 0;
				_dragged = 0;
				_right = Input.GetMouseButtonUp(1);
				_middle = Input.GetMouseButtonUp(2);
			}
			else
			{
				_right = Input.GetMouseButton(1);
				_middle = Input.GetMouseButton(2);
			}
		}
		else if (_pressed)
		{
			if (!Input.anyKey)
			{
				_pressed = false;
				_pressing = 0;
				_dragged = 0;
				_right = false;
				_middle = false;
			}
			else
			{
				_right = Input.GetMouseButton(1);
				_middle = Input.GetMouseButton(2);
				_pressing++;
				if (Config.isDraggingItem())
				{
					_dragged++;
				}
			}
		}
		_can_drag = World.world.isOverUI() || World.world.canDragMap();
		_selected_unit_attack = ControllableUnit.isControllingUnit();
		_mouse_hold_animation = World.world.getSelectedPowerHoldAnimation();
		if (_selected_unit_attack)
		{
			setCursor(22);
		}
		else if (_can_drag && Config.isDraggingItem() && _pressed && _dragged > 3)
		{
			if (_dragged < 10)
			{
				setCursor(6);
			}
			else
			{
				setCursor(7);
			}
		}
		else if (!_pressed && _anim_done)
		{
			if (shouldShowInspectCursor())
			{
				setCursorInspect();
			}
			else if (_right)
			{
				setCursor(8);
			}
			else
			{
				setCursorDefault();
			}
			_counter = 0;
		}
		else if (_can_drag && _pressed && _pressing > 3)
		{
			if (!World.world.isOverUI() || World.world.player_control.isPointerOverUIScroll())
			{
				if (_pressing < 10)
				{
					setCursor(6);
				}
				else
				{
					setCursor(7);
				}
			}
			else if (_right)
			{
				setCursor(8);
			}
			else
			{
				setCursor(1);
			}
		}
		else if (!_pressed && shouldShowInspectCursor())
		{
			setCursorInspect();
		}
		else if (_right)
		{
			int num = Mathf.CeilToInt((float)(_counter / 5));
			if (num > 0 && num < 5)
			{
				setCursor(num + 8);
			}
			else
			{
				setCursor(8);
			}
			_counter++;
			if (_counter > 40)
			{
				_counter = 0;
				if (!_pressed)
				{
					_anim_done = true;
				}
			}
		}
		else
		{
			int num2 = 10;
			int num3 = 0;
			switch (_mouse_hold_animation)
			{
			case MouseHoldAnimation.Sprinkle:
				num2 = 4;
				num3 = Mathf.CeilToInt((float)(_counter / 5)) % 4;
				setCursor(13 + num3);
				break;
			case MouseHoldAnimation.Draw:
				num2 = 5;
				num3 = Mathf.CeilToInt((float)(_counter / 5)) % 5;
				setCursor(17 + num3);
				break;
			default:
				num2 = 6;
				num3 = Mathf.CeilToInt((float)(_counter / 5));
				if (num3 < 6)
				{
					setCursor(num3);
				}
				else
				{
					setCursorDefault();
				}
				break;
			}
			_counter++;
			if (_counter >= num2 * 5 && !_pressed)
			{
				_anim_done = true;
			}
		}
		renderCursor();
	}

	private void renderCursor()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		if ((Object)(object)_selected_cursor_texture != (Object)null)
		{
			num = ((object)_selected_cursor_texture).GetHashCode();
		}
		if (_last_texture_id != num)
		{
			Cursor.SetCursor(_selected_cursor_texture, Vector2.zero, (CursorMode)0);
			_last_texture_id = num;
		}
	}

	private void setCursor(int pCursor = -1)
	{
		_cur_cursor = pCursor;
		_selected_cursor_texture = _cursors[pCursor];
		if ((Object)(object)_selected_cursor_texture == (Object)null)
		{
			_selected_cursor_texture = mouseCursorDefault;
		}
	}

	private bool shouldShowInspectCursor()
	{
		if (UnitSelectionEffect.last_actor != null)
		{
			return true;
		}
		if (World.world.isOverUI())
		{
			return false;
		}
		if (ScrollWindow.isWindowActive())
		{
			return false;
		}
		if (World.world.quality_changer.isLowRes())
		{
			WorldTile mouseTilePosCachedFrame = World.world.getMouseTilePosCachedFrame();
			if (mouseTilePosCachedFrame == null)
			{
				return false;
			}
			if (Zones.showMapBorders())
			{
				MetaTypeAsset cachedMapMetaAsset = World.world.getCachedMapMetaAsset();
				if (cachedMapMetaAsset == null)
				{
					return false;
				}
				if (cachedMapMetaAsset.check_tile_has_meta(mouseTilePosCachedFrame.zone, cachedMapMetaAsset, cachedMapMetaAsset.getZoneOptionState()))
				{
					return true;
				}
			}
		}
		if (World.world.nameplate_manager.isOverNameplate())
		{
			return true;
		}
		return false;
	}

	private void setCursorDefault()
	{
		MapBox world = World.world;
		if (world != null && world.isOverUiButton())
		{
			setCursor(27);
		}
		else
		{
			setCursor(0);
		}
	}

	private void setCursorInspect()
	{
		setCursor(26);
	}

	public static void debug(DebugTool pTool)
	{
		pTool.setText("_counter:", _counter, 0f, pShowBar: false, 0L);
		pTool.setText("_cur_cursor:", _cur_cursor, 0f, pShowBar: false, 0L);
		string text = "null";
		pTool.setText("_cur_cursor:", _cur_cursor switch
		{
			0 => "mouseCursorDefault", 
			1 => "mouseCursorDown", 
			2 => "mouseCursorUp1", 
			3 => "mouseCursorUp2", 
			4 => "mouseCursorUp3", 
			5 => "mouseCursorUp4", 
			6 => "mouseCursorHold", 
			7 => "mouseCursorDrag", 
			8 => "mouseCursorPinkie1", 
			9 => "mouseCursorPinkie2", 
			10 => "mouseCursorPinkie3", 
			11 => "mouseCursorPinkie4", 
			12 => "mouseCursorPinkie5", 
			13 => "mouseCursorSprinkle1", 
			14 => "mouseCursorSprinkle2", 
			15 => "mouseCursorSprinkle3", 
			16 => "mouseCursorSprinkle4", 
			17 => "mouseCursorDraw1", 
			18 => "mouseCursorDraw2", 
			19 => "mouseCursorDraw3", 
			20 => "mouseCursorDraw4", 
			21 => "mouseCursorDraw5", 
			_ => _cur_cursor.ToString(), 
		}, 0f, pShowBar: false, 0L);
		pTool.setText("_pressed:", _pressed, 0f, pShowBar: false, 0L);
		pTool.setText("_pressing:", _pressing, 0f, pShowBar: false, 0L);
		pTool.setText("_dragged:", _dragged, 0f, pShowBar: false, 0L);
		pTool.setText("_right:", _right, 0f, pShowBar: false, 0L);
		pTool.setText("_middle:", _middle, 0f, pShowBar: false, 0L);
		pTool.setText("_anim_done:", _anim_done, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("_can_drag:", _can_drag, 0f, pShowBar: false, 0L);
		pTool.setText("_hold_animation:", _mouse_hold_animation, 0f, pShowBar: false, 0L);
		pTool.setSeparator();
		pTool.setText("_last_texture_id:", _last_texture_id, 0f, pShowBar: false, 0L);
		pTool.setText("_selected_cursor_texture:", ((object)_selected_cursor_texture).GetHashCode().ToString(), 0f, pShowBar: false, 0L);
	}
}
