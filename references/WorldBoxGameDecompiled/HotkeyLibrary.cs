using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Beebyte.Obfuscator;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
[ObfuscateLiterals]
public class HotkeyLibrary : AssetLibrary<HotkeyAsset>
{
	public static HotkeyAsset cancel;

	public static HotkeyAsset console;

	public static HotkeyAsset remove;

	public static HotkeyAsset pause;

	public static HotkeyAsset hide_ui;

	public static HotkeyAsset action_jump;

	public static HotkeyAsset action_dash;

	public static HotkeyAsset action_backstep;

	public static HotkeyAsset action_talk;

	public static HotkeyAsset action_steal;

	public static HotkeyAsset action_swear;

	public static HotkeyAsset left;

	public static HotkeyAsset right;

	public static HotkeyAsset up;

	public static HotkeyAsset down;

	public static HotkeyAsset next_unit_in_multi_selection;

	public static HotkeyAsset next_tab;

	public static HotkeyAsset prev_tab;

	public static HotkeyAsset zoom_in;

	public static HotkeyAsset zoom_out;

	public static HotkeyAsset zoom;

	public static HotkeyAsset world_speed;

	public static HotkeyAsset brush;

	public static HotkeyAsset follow_unit;

	public static HotkeyAsset control_unit;

	public static HotkeyAsset fullscreen_switch;

	public static HotkeyAsset many_mod;

	public static HotkeyAsset fast_civ_mod;

	public static KeyCode[] mod_keys = (KeyCode[])(object)new KeyCode[0];

	private HotkeyAsset[] action_hotkeys = new HotkeyAsset[0];

	private Dictionary<string, float> holding_times = new Dictionary<string, float>();

	private bool holdingAnyModKey;

	private bool runModKeyCheck = true;

	private bool _last_input_active;

	private MetaType[] _meta_zones = new MetaType[10]
	{
		MetaType.Army,
		MetaType.Alliance,
		MetaType.Kingdom,
		MetaType.City,
		MetaType.Clan,
		MetaType.Religion,
		MetaType.Culture,
		MetaType.Language,
		MetaType.Family,
		MetaType.Subspecies
	};

	public override void init()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_0767: Unknown result type (might be due to invalid IL or missing references)
		//IL_0774: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0798: Unknown result type (might be due to invalid IL or missing references)
		//IL_07eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_0871: Unknown result type (might be due to invalid IL or missing references)
		//IL_087c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0887: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0933: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_0961: Unknown result type (might be due to invalid IL or missing references)
		//IL_0971: Unknown result type (might be due to invalid IL or missing references)
		//IL_098f: Unknown result type (might be due to invalid IL or missing references)
		//IL_099f: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a75: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0acc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bcb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c17: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c55: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e75: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fcc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd8: Unknown result type (might be due to invalid IL or missing references)
		base.init();
		addHotkeysForUnitControlLayer();
		fullscreen_switch = add(new HotkeyAsset
		{
			id = "fullscreen_switch",
			default_key_1 = (KeyCode)13,
			default_key_mod_1 = (KeyCode)308,
			just_pressed_action = delegate
			{
				PlayerConfig.toggleFullScreen();
			}
		});
		console = add(new HotkeyAsset
		{
			id = "console",
			default_key_1 = (KeyCode)126,
			default_key_2 = (KeyCode)96,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null)
				{
					World.world.console.Toggle();
				}
			}
		});
		cancel = add(new HotkeyAsset
		{
			id = "cancel",
			default_key_1 = (KeyCode)27,
			just_pressed_action = escapeAction
		});
		add(new HotkeyAsset
		{
			id = "back",
			default_key_1 = (KeyCode)326,
			just_pressed_action = backAction
		});
		pause = add(new HotkeyAsset
		{
			id = "pause",
			default_key_1 = (KeyCode)32,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				Config.paused = !Config.paused;
			}
		});
		hide_ui = add(new HotkeyAsset
		{
			id = "hide_ui",
			default_key_1 = (KeyCode)104,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				Config.ui_main_hidden = !Config.ui_main_hidden;
			}
		});
		remove = add(new HotkeyAsset
		{
			id = "remove",
			default_key_1 = (KeyCode)127,
			default_key_2 = (KeyCode)8,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				if (SelectedUnit.isSet())
				{
					SelectedUnit.killSelected();
				}
				else
				{
					string pID = "life_eraser";
					if (World.world.isSelectedPower("life_eraser"))
					{
						pID = "demolish";
					}
					World.world.selected_buttons.clickPowerButton(PowerButton.get(pID));
				}
			}
		});
		zoom = add(new HotkeyAsset
		{
			id = "zoom",
			use_mouse_wheel = true,
			holding_cooldown = 0f,
			check_window_not_active = true,
			check_controls_locked = true,
			allow_unit_control = true,
			holding_action = delegate(HotkeyAsset pAsset)
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				if (World.world.isPointerInGame() && (!World.world.isOverUI() || MoveCamera.inSpectatorMode()))
				{
					float y = Input.mouseScrollDelta.y;
					if (y < 0f)
					{
						MoveCamera.zoomOutWheel(pAsset);
					}
					else if (y > 0f)
					{
						MoveCamera.zoomInWheel(pAsset);
					}
				}
			}
		});
		world_speed = add(new HotkeyAsset
		{
			id = "world_speed",
			default_key_mod_1 = (KeyCode)306,
			default_key_mod_2 = (KeyCode)305,
			default_key_mod_3 = (KeyCode)310,
			check_window_not_active = true,
			check_controls_locked = true,
			use_mouse_wheel = true,
			holding_cooldown = 0f,
			holding_action = delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				float y = Input.mouseScrollDelta.y;
				WorldTimeScaleAsset time_scale_asset = Config.time_scale_asset;
				if (y < 0f)
				{
					Config.prevWorldSpeed();
				}
				else if (y > 0f)
				{
					Config.nextWorldSpeed();
				}
				if (time_scale_asset != Config.time_scale_asset)
				{
					string text = LocalizedTextManager.getText("changed_worldspeed");
					string text2 = null;
					text2 = ((Config.time_scale_asset.getLocaleID() == null) ? Toolbox.coloredText(Config.time_scale_asset.id, "#95DD5D") : Toolbox.coloredText(Config.time_scale_asset.getLocaleID(), "#95DD5D", pLocalize: true));
					text = text.Replace("$speed$", text2);
					WorldTip.instance.showToolbarText(text);
				}
			}
		});
		brush = add(new HotkeyAsset
		{
			id = "brush",
			default_key_mod_1 = (KeyCode)308,
			default_key_mod_2 = (KeyCode)307,
			check_window_not_active = true,
			check_controls_locked = true,
			use_mouse_wheel = true,
			holding_cooldown = 0f,
			holding_action = delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				float y = Input.mouseScrollDelta.y;
				string current_brush = Config.current_brush;
				if (y < 0f)
				{
					BrushLibrary.nextBrush();
				}
				else if (y > 0f)
				{
					BrushLibrary.previousBrush();
				}
				if (current_brush != Config.current_brush)
				{
					BrushData brushData = Brush.get(Config.current_brush);
					string localeID = brushData.getLocaleID();
					string text = LocalizedTextManager.getText("changed_brush");
					string text2 = Toolbox.coloredText(localeID, "#95DD5D", pLocalize: true);
					text2 = text2 + " (" + Toolbox.coloredText(brushData.size.ToString(), "#95DD5D") + ")";
					text = text.Replace("$brush$", text2);
					WorldTip.instance.showToolbarText(text);
				}
			}
		});
		many_mod = add(new HotkeyAsset
		{
			id = "many_mod",
			default_key_mod_1 = (KeyCode)303,
			default_key_mod_2 = (KeyCode)304,
			disable_for_controlled_unit = true,
			check_only_not_controllable_unit = true
		});
		fast_civ_mod = add(new HotkeyAsset
		{
			id = "fast_civ_mod",
			default_key_mod_1 = (KeyCode)305,
			default_key_mod_2 = (KeyCode)306
		});
		left = add(new HotkeyAsset
		{
			id = "left",
			default_key_1 = (KeyCode)97,
			default_key_2 = (KeyCode)276,
			holding_action = MoveCamera.move,
			holding_cooldown = 0f,
			check_window_not_active = true,
			check_controls_locked = true,
			allow_unit_control = true
		});
		right = clone("right", "left");
		t.default_key_1 = (KeyCode)100;
		t.default_key_2 = (KeyCode)275;
		up = clone("up", "left");
		t.default_key_1 = (KeyCode)119;
		t.default_key_2 = (KeyCode)273;
		down = clone("down", "left");
		t.default_key_1 = (KeyCode)115;
		t.default_key_2 = (KeyCode)274;
		clone("fast_left", "left");
		t.default_key_mod_1 = (KeyCode)303;
		t.default_key_mod_2 = (KeyCode)304;
		clone("fast_right", "right");
		t.default_key_mod_1 = (KeyCode)303;
		t.default_key_mod_2 = (KeyCode)304;
		clone("fast_up", "up");
		t.default_key_mod_1 = (KeyCode)303;
		t.default_key_mod_2 = (KeyCode)304;
		clone("fast_down", "down");
		t.default_key_mod_1 = (KeyCode)303;
		t.default_key_mod_2 = (KeyCode)304;
		zoom_in = add(new HotkeyAsset
		{
			id = "zoom_in",
			default_key_1 = (KeyCode)113,
			default_key_2 = (KeyCode)43,
			default_key_3 = (KeyCode)270,
			check_window_not_active = true,
			check_controls_locked = true,
			holding_action = MoveCamera.zoomIn,
			holding_cooldown = 0f
		});
		zoom_out = add(new HotkeyAsset
		{
			id = "zoom_out",
			default_key_1 = (KeyCode)101,
			default_key_2 = (KeyCode)45,
			default_key_3 = (KeyCode)269,
			check_window_not_active = true,
			check_controls_locked = true,
			holding_action = MoveCamera.zoomOut,
			holding_cooldown = 0f
		});
		add(new HotkeyAsset
		{
			id = "power_left",
			default_key_1 = (KeyCode)276,
			default_key_2 = (KeyCode)97,
			default_key_mod_1 = (KeyCode)306,
			default_key_mod_2 = (KeyCode)310,
			default_key_mod_3 = (KeyCode)305,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = powerMove,
			holding_action = powerMove
		});
		clone("power_right", "power_left");
		t.default_key_1 = (KeyCode)275;
		t.default_key_2 = (KeyCode)100;
		clone("power_up", "power_left");
		t.default_key_1 = (KeyCode)273;
		t.default_key_2 = (KeyCode)119;
		clone("power_down", "power_left");
		t.default_key_1 = (KeyCode)274;
		t.default_key_2 = (KeyCode)115;
		add(new HotkeyAsset
		{
			id = "toggle_power",
			default_key_1 = (KeyCode)13,
			default_key_2 = (KeyCode)271,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				PowerButton activeButton = PowersTab.getActiveTab().getActiveButton();
				if (!((Object)(object)activeButton == (Object)null))
				{
					if (activeButton.godPower != null)
					{
						string text = activeButton.godPower.id;
						if (!(text == "clock"))
						{
							if (text == "pause")
							{
								activeButton.clickSpecial();
							}
							else
							{
								activeButton.godPower.select_button_action?.Invoke(activeButton.godPower.id);
								if (activeButton.godPower.toggle_action != null)
								{
									activeButton.godPower.toggle_action?.Invoke(activeButton.godPower.id);
									PowerButtonSelector.instance.checkToggleIcons();
								}
							}
						}
						else
						{
							Config.nextWorldSpeed(pCycle: true);
						}
					}
					else if (activeButton.type == PowerButtonType.Options)
					{
						((UnityEvent)((Component)activeButton).gameObject.GetComponent<Button>().onClick).Invoke();
					}
					else
					{
						activeButton.clickButton();
					}
				}
			}
		});
		clone("toggle_power2", "toggle_power");
		t.default_key_mod_1 = (KeyCode)306;
		t.default_key_mod_2 = (KeyCode)310;
		next_tab = add(new HotkeyAsset
		{
			id = "next_tab",
			default_key_1 = (KeyCode)9,
			check_window_not_active = true,
			check_controls_locked = true,
			check_no_multi_unit_selection = true,
			just_pressed_action = delegate
			{
				Button next = PowerTabController.instance.getNext(((Object)PowersTab.getActiveTab()).name);
				PowersTab.showTabFromButton(next);
				TipButton component = ((Component)next).gameObject.GetComponent<TipButton>();
				string pText = LocalizedTextManager.getText(component.textOnClick) + "\n" + LocalizedTextManager.getText(component.textOnClickDescription);
				WorldTip.instance.showToolbarText(pText);
			}
		});
		prev_tab = add(new HotkeyAsset
		{
			id = "prev_tab",
			default_key_1 = (KeyCode)9,
			default_key_mod_1 = (KeyCode)304,
			default_key_mod_2 = (KeyCode)303,
			check_window_not_active = true,
			check_controls_locked = true,
			check_no_multi_unit_selection = true,
			just_pressed_action = delegate
			{
				PowersTab.showTabFromButton(PowerTabController.instance.getPrev(((Object)PowersTab.getActiveTab()).name));
			}
		});
		add(new HotkeyAsset
		{
			id = "hotkey_1",
			default_key_1 = (KeyCode)49,
			default_key_2 = (KeyCode)257,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate(HotkeyAsset pAsset)
			{
				string text = pAsset.id;
				string hotkeyFromData = getHotkeyFromData(text);
				if (!string.IsNullOrEmpty(hotkeyFromData))
				{
					hotkeySelectNano(pAsset, hotkeyFromData);
				}
				else
				{
					string stringVal = PlayerConfig.dict[text].stringVal;
					hotkeySelectPower(pAsset, stringVal);
				}
			}
		});
		clone("hotkey_2", "hotkey_1");
		t.default_key_1 = (KeyCode)50;
		t.default_key_2 = (KeyCode)258;
		clone("hotkey_3", "hotkey_1");
		t.default_key_1 = (KeyCode)51;
		t.default_key_2 = (KeyCode)259;
		clone("hotkey_4", "hotkey_1");
		t.default_key_1 = (KeyCode)52;
		t.default_key_2 = (KeyCode)260;
		clone("hotkey_5", "hotkey_1");
		t.default_key_1 = (KeyCode)53;
		t.default_key_2 = (KeyCode)261;
		clone("hotkey_6", "hotkey_1");
		t.default_key_1 = (KeyCode)54;
		t.default_key_2 = (KeyCode)262;
		clone("hotkey_7", "hotkey_1");
		t.default_key_1 = (KeyCode)55;
		t.default_key_2 = (KeyCode)263;
		clone("hotkey_8", "hotkey_1");
		t.default_key_1 = (KeyCode)56;
		t.default_key_2 = (KeyCode)264;
		clone("hotkey_9", "hotkey_1");
		t.default_key_1 = (KeyCode)57;
		t.default_key_2 = (KeyCode)265;
		clone("hotkey_0", "hotkey_1");
		t.default_key_1 = (KeyCode)48;
		t.default_key_2 = (KeyCode)256;
		add(new HotkeyAsset
		{
			id = "save_hotkey_1",
			default_key_1 = (KeyCode)49,
			default_key_2 = (KeyCode)257,
			default_key_mod_1 = (KeyCode)306,
			default_key_mod_2 = (KeyCode)310,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate(HotkeyAsset pAsset)
			{
				if (SelectedObjects.isNanoObjectSet())
				{
					hotkeySaveTab(pAsset);
				}
				else
				{
					hotkeySavePower(pAsset);
				}
			}
		});
		clone("save_hotkey_2", "save_hotkey_1");
		t.default_key_1 = (KeyCode)50;
		t.default_key_2 = (KeyCode)258;
		clone("save_hotkey_3", "save_hotkey_1");
		t.default_key_1 = (KeyCode)51;
		t.default_key_2 = (KeyCode)259;
		clone("save_hotkey_4", "save_hotkey_1");
		t.default_key_1 = (KeyCode)52;
		t.default_key_2 = (KeyCode)260;
		clone("save_hotkey_5", "save_hotkey_1");
		t.default_key_1 = (KeyCode)53;
		t.default_key_2 = (KeyCode)261;
		clone("save_hotkey_6", "save_hotkey_1");
		t.default_key_1 = (KeyCode)54;
		t.default_key_2 = (KeyCode)262;
		clone("save_hotkey_7", "save_hotkey_1");
		t.default_key_1 = (KeyCode)55;
		t.default_key_2 = (KeyCode)263;
		clone("save_hotkey_8", "save_hotkey_1");
		t.default_key_1 = (KeyCode)56;
		t.default_key_2 = (KeyCode)264;
		clone("save_hotkey_9", "save_hotkey_1");
		t.default_key_1 = (KeyCode)57;
		t.default_key_2 = (KeyCode)265;
		clone("save_hotkey_0", "save_hotkey_1");
		t.default_key_1 = (KeyCode)48;
		t.default_key_2 = (KeyCode)256;
		add(new HotkeyAsset
		{
			id = "zone_type_previous",
			default_key_1 = (KeyCode)122,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				switchZones(-1);
			}
		});
		clone("zone_type_next", "zone_type_previous");
		t.just_pressed_action = delegate
		{
			switchZones(1);
		};
		t.default_key_1 = (KeyCode)120;
		add(new HotkeyAsset
		{
			id = "zone_type_state_next",
			default_key_1 = (KeyCode)99,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				toggleZones(1);
			}
		});
		clone("zone_type_state_previous", "zone_type_state_next");
		t.just_pressed_action = delegate
		{
			toggleZones(-1);
		};
		t.default_key_mod_1 = (KeyCode)306;
		t.default_key_mod_2 = (KeyCode)310;
		follow_unit = add(new HotkeyAsset
		{
			id = "follow_unit",
			default_key_1 = (KeyCode)102,
			check_window_not_active = false,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				Actor unit = SelectedUnit.unit;
				if (ScrollWindow.isWindowActive())
				{
					ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
					if (!(currentWindow.screen_id != "unit") && !((Component)currentWindow).GetComponent<UnitWindow>().name_input.inputField.isFocused && SelectedUnit.isSet())
					{
						World.world.followUnit(unit);
						ScrollWindow.hideAllEvent();
					}
				}
				else if (MapBox.isRenderGameplay())
				{
					Actor actorNearCursor = World.world.getActorNearCursor();
					if (actorNearCursor == null)
					{
						if (MoveCamera.hasFocusUnit())
						{
							MoveCamera.clearFocusUnitOnly();
						}
						else if (SelectedUnit.isSet())
						{
							World.world.followUnit(unit);
						}
					}
					else if (actorNearCursor.isCameraFollowingUnit())
					{
						MoveCamera.clearFocusUnitOnly();
					}
					else
					{
						World.world.followUnit(actorNearCursor);
					}
				}
			}
		});
		control_unit = add(new HotkeyAsset
		{
			id = "control_unit",
			default_key_1 = (KeyCode)103,
			check_window_not_active = false,
			just_pressed_action = delegate
			{
				if (MoveCamera.hasFocusUnit())
				{
					World.world.move_camera.clearFocusUnitAndUnselect();
				}
				Actor unit = SelectedUnit.unit;
				if (ScrollWindow.isWindowActive())
				{
					ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
					if (!(currentWindow.screen_id != "unit") && !((Component)currentWindow).GetComponent<UnitWindow>().name_input.inputField.isFocused && SelectedUnit.isSet())
					{
						ControllableUnit.setControllableCreature(unit);
						ScrollWindow.hideAllEvent();
					}
				}
				else if (MapBox.isRenderGameplay())
				{
					Actor actorNearCursor = World.world.getActorNearCursor();
					if (ControllableUnit.isControllingUnit())
					{
						if (ControllableUnit.isControllingUnit(actorNearCursor))
						{
							ControllableUnit.clear();
							return;
						}
						if (actorNearCursor != null)
						{
							ControllableUnit.clear();
							ControllableUnit.setControllableCreature(actorNearCursor);
							return;
						}
						if (actorNearCursor == null)
						{
							ControllableUnit.clear();
							return;
						}
					}
					if (actorNearCursor == null)
					{
						if (SelectedUnit.isSet())
						{
							ControllableUnit.setControllableCreatureAndSelected(unit);
						}
					}
					else
					{
						ControllableUnit.setControllableCreatureAndSelected(actorNearCursor);
					}
				}
			}
		});
		add(new HotkeyAsset
		{
			id = "meta_window_previous",
			default_key_1 = (KeyCode)276,
			default_key_2 = (KeyCode)113,
			default_key_3 = (KeyCode)97,
			just_pressed_action = delegate
			{
				MetaSwitchManager.switchWindows(MetaSwitchManager.Direction.Left);
			},
			check_controls_locked = true,
			check_window_active = true
		});
		clone("meta_window_next", "meta_window_previous");
		t.default_key_1 = (KeyCode)275;
		t.default_key_2 = (KeyCode)101;
		t.default_key_3 = (KeyCode)100;
		t.just_pressed_action = delegate
		{
			MetaSwitchManager.switchWindows(MetaSwitchManager.Direction.Right);
		};
		add(new HotkeyAsset
		{
			id = "window_tab_next",
			default_key_1 = (KeyCode)9,
			default_key_2 = (KeyCode)115,
			default_key_3 = (KeyCode)274,
			just_pressed_action = windowTabsSwitch,
			check_controls_locked = true,
			check_window_active = true
		});
		clone("window_tab_previous", "window_tab_next");
		t.default_key_mod_1 = (KeyCode)304;
		t.default_key_mod_2 = (KeyCode)303;
		clone("window_tab_previous_2", "window_tab_next");
		t.default_key_1 = (KeyCode)119;
		t.default_key_2 = (KeyCode)273;
		t.default_key_3 = (KeyCode)0;
	}

	private void addHotkeysForUnitControlLayer()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		next_unit_in_multi_selection = add(new HotkeyAsset
		{
			id = "next_unit_in_multi_selection",
			default_key_1 = (KeyCode)9,
			check_window_not_active = true,
			check_controls_locked = true,
			check_multi_unit_selection = true,
			ignore_same_key_diagnostic = true,
			just_pressed_action = delegate
			{
				SelectedUnit.nextMainUnit();
			}
		});
		action_jump = add(new HotkeyAsset
		{
			id = "action_jump",
			default_key_1 = (KeyCode)32,
			ignore_same_key_diagnostic = true,
			check_window_not_active = true,
			check_controls_locked = true,
			check_only_controllable_unit = true
		});
		action_dash = add(new HotkeyAsset
		{
			id = "action_dash",
			default_key_1 = (KeyCode)304,
			default_key_2 = (KeyCode)303,
			ignore_same_key_diagnostic = true,
			check_window_not_active = true,
			check_controls_locked = true,
			ignore_mod_keys = true,
			check_only_controllable_unit = true
		});
		action_backstep = add(new HotkeyAsset
		{
			id = "action_backstep",
			default_key_1 = (KeyCode)306,
			default_key_2 = (KeyCode)305,
			ignore_same_key_diagnostic = true,
			check_window_not_active = true,
			check_controls_locked = true,
			ignore_mod_keys = true,
			check_only_controllable_unit = true
		});
		action_swear = add(new HotkeyAsset
		{
			id = "action_swear",
			default_key_1 = (KeyCode)102,
			ignore_same_key_diagnostic = true,
			check_window_not_active = true,
			check_controls_locked = true,
			check_only_controllable_unit = true
		});
		action_steal = add(new HotkeyAsset
		{
			id = "action_steal",
			default_key_1 = (KeyCode)113,
			ignore_same_key_diagnostic = true,
			check_window_not_active = true,
			check_controls_locked = true,
			check_only_controllable_unit = true
		});
		action_talk = add(new HotkeyAsset
		{
			id = "action_talk",
			default_key_1 = (KeyCode)116,
			ignore_same_key_diagnostic = true,
			check_window_not_active = true,
			check_controls_locked = true,
			check_only_controllable_unit = true
		});
	}

	private void switchZones(int pIndexChange)
	{
		MetaType currentMapBorderMode = Zones.getCurrentMapBorderMode(pCheckOnlyOption: true);
		int num = Array.IndexOf(_meta_zones, currentMapBorderMode);
		num += pIndexChange;
		num = Toolbox.loopIndex(num, _meta_zones.Length);
		currentMapBorderMode = _meta_zones[num];
		MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(currentMapBorderMode);
		AssetManager.powers.get(asset.power_option_zone_id).toggle_action(asset.power_option_zone_id);
		PowerButtonSelector.instance.checkToggleIcons();
		GodPower pPower = AssetManager.powers.get(asset.power_option_zone_id);
		WorldTip.instance.showToolbarText(pPower);
	}

	private void toggleZones(int pIndexChange)
	{
		MetaType currentMapBorderMode = Zones.getCurrentMapBorderMode(pCheckOnlyOption: true);
		if (currentMapBorderMode != MetaType.None)
		{
			MetaTypeAsset asset = AssetManager.meta_type_library.getAsset(currentMapBorderMode);
			GodPower godPower = AssetManager.powers.get(asset.power_option_zone_id);
			if (godPower.multi_toggle)
			{
				asset.toggleOptionZone(godPower, pIndexChange, pDisable: false);
				PowerButtonSelector.instance.checkToggleIcons();
			}
		}
	}

	private void windowTabsSwitch(HotkeyAsset pAsset)
	{
		ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
		List<WindowMetaTab> contentTabs = currentWindow.tabs.getContentTabs();
		if (contentTabs.Count >= 2)
		{
			WindowMetaTab activeTab = currentWindow.tabs.getActiveTab();
			int num = contentTabs.IndexOf(activeTab);
			switch (pAsset.id)
			{
			case "window_tab_next":
				num++;
				break;
			case "window_tab_previous":
			case "window_tab_previous_2":
				num--;
				break;
			}
			num = Toolbox.loopIndex(num, contentTabs.Count);
			WindowMetaTab windowMetaTab = contentTabs[num];
			windowMetaTab.doAction();
			WorldTip.showNowTop(windowMetaTab.getWorldTipText(), pTranslate: false);
		}
	}

	private bool navigateWindowBack(HotkeyAsset pAsset)
	{
		if (!ScrollWindow.isWindowActive())
		{
			return false;
		}
		if (ScrollWindow.isAnimationActive())
		{
			ScrollWindow.finishAnimations();
		}
		WindowHistory.clickBack();
		return true;
	}

	private bool navigateTabBack(HotkeyAsset pAsset)
	{
		if (ScrollWindow.isWindowActive())
		{
			return false;
		}
		if (!SelectedTabsHistory.showPreviousTab())
		{
			return false;
		}
		return true;
	}

	private void backAction(HotkeyAsset pAsset)
	{
		if (!navigateWindowBack(pAsset) && !navigateTabBack(pAsset) && !PowersTab.getActiveTab().getAsset().tab_type_main)
		{
			PowerTabController.showMainTab();
		}
	}

	private void escapeAction(HotkeyAsset pAsset)
	{
		if (World.world.console.isActive())
		{
			World.world.console.Hide();
		}
		else if (ControllableUnit.isControllingUnit())
		{
			ControllableUnit.clear();
		}
		else if (World.world.tutorial.isActive())
		{
			World.world.tutorial.endTutorial();
		}
		else
		{
			if (MapBox.controlsLocked() || MapBox.isControllingUnit())
			{
				return;
			}
			if (MoveCamera.hasFocusUnit())
			{
				MoveCamera.clearFocusUnitOnly();
			}
			else
			{
				if (navigateWindowBack(pAsset))
				{
					return;
				}
				if (Config.ui_main_hidden)
				{
					Config.ui_main_hidden = false;
				}
				else if (!navigateTabBack(pAsset))
				{
					if ((Object)(object)World.world.selected_buttons.selectedButton != (Object)null)
					{
						World.world.selected_buttons.unselectAll();
					}
					else if (SelectedUnit.isSet())
					{
						SelectedUnit.clear();
					}
					else if (PowersTab.isTabSelected())
					{
						World.world.selected_buttons.unselectTabs();
						SelectedObjects.unselectNanoObject();
					}
					else
					{
						ScrollWindow.showWindow("quit_game");
					}
				}
			}
		}
	}

	private void powerMove(HotkeyAsset pAsset)
	{
		PowersTab activeTab = PowersTab.getActiveTab();
		switch (pAsset.id)
		{
		case "power_left":
			activeTab.leftButton();
			break;
		case "power_right":
			activeTab.rightButton();
			break;
		case "power_up":
			activeTab.upButton();
			break;
		case "power_down":
			activeTab.downButton();
			break;
		}
	}

	public override void linkAssets()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		base.linkAssets();
		HashSet<KeyCode> hashSet = new HashSet<KeyCode>();
		HashSet<HotkeyAsset> hashSet2 = new HashSet<HotkeyAsset>();
		foreach (HotkeyAsset item in list)
		{
			item.overridden_key_1 = item.default_key_1;
			item.overridden_key_2 = item.default_key_2;
			item.overridden_key_3 = item.default_key_3;
			item.overridden_key_mod_1 = item.default_key_mod_1;
			item.overridden_key_mod_2 = item.default_key_mod_2;
			item.overridden_key_mod_3 = item.default_key_mod_3;
			if ((int)item.default_key_mod_1 != 0)
			{
				hashSet.Add(item.default_key_mod_1);
			}
			if ((int)item.default_key_mod_2 != 0)
			{
				hashSet.Add(item.default_key_mod_2);
			}
			if ((int)item.default_key_mod_3 != 0)
			{
				hashSet.Add(item.default_key_mod_3);
			}
			if (item.just_pressed_action != null)
			{
				hashSet2.Add(item);
			}
			else if (item.holding_action != null)
			{
				hashSet2.Add(item);
			}
		}
		mod_keys = hashSet.ToArray();
		action_hotkeys = hashSet2.ToArray();
	}

	public override void editorDiagnostic()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		base.editorDiagnostic();
		Dictionary<string, HotkeyAsset> dictionary = new Dictionary<string, HotkeyAsset>();
		foreach (HotkeyAsset item in list)
		{
			if (item.ignore_same_key_diagnostic)
			{
				continue;
			}
			string text = "";
			if (item.check_window_active)
			{
				text += "ui+";
			}
			using ListPool<string> listPool = new ListPool<string>();
			bool flag = (int)item.default_key_mod_1 > 0;
			if ((int)item.default_key_1 != 0)
			{
				if (flag)
				{
					if ((int)item.default_key_mod_1 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_1)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_1)/*cast due to constrained. prefix*/).ToString());
					}
					if ((int)item.default_key_mod_2 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_1)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_2)/*cast due to constrained. prefix*/).ToString());
					}
					if ((int)item.default_key_mod_3 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_1)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_3)/*cast due to constrained. prefix*/).ToString());
					}
				}
				else
				{
					listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_1)/*cast due to constrained. prefix*/).ToString());
				}
			}
			if ((int)item.default_key_2 != 0)
			{
				if (flag)
				{
					if ((int)item.default_key_mod_1 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_2)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_1)/*cast due to constrained. prefix*/).ToString());
					}
					if ((int)item.default_key_mod_2 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_2)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_2)/*cast due to constrained. prefix*/).ToString());
					}
					if ((int)item.default_key_mod_3 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_2)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_3)/*cast due to constrained. prefix*/).ToString());
					}
				}
				else
				{
					listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_2)/*cast due to constrained. prefix*/).ToString());
				}
			}
			if ((int)item.default_key_3 != 0)
			{
				if (flag)
				{
					if ((int)item.default_key_mod_1 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_3)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_1)/*cast due to constrained. prefix*/).ToString());
					}
					if ((int)item.default_key_mod_2 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_3)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_2)/*cast due to constrained. prefix*/).ToString());
					}
					if ((int)item.default_key_mod_3 != 0)
					{
						listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_3)/*cast due to constrained. prefix*/).ToString() + "+" + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_mod_3)/*cast due to constrained. prefix*/).ToString());
					}
				}
				else
				{
					listPool.Add(text + ((object)Unsafe.As<KeyCode, KeyCode>(ref item.default_key_3)/*cast due to constrained. prefix*/).ToString());
				}
			}
			foreach (ref string item2 in listPool)
			{
				string current2 = item2;
				if (dictionary.ContainsKey(current2))
				{
					BaseAssetLibrary.logAssetError("<e>" + item.id + "</e> has the same key as asset: <e>" + dictionary[current2].id + "</e>", current2);
				}
				else
				{
					dictionary.Add(current2, item);
				}
			}
		}
	}

	public static bool isHoldingControlForSelection()
	{
		if (!Input.GetKey((KeyCode)306))
		{
			return Input.GetKey((KeyCode)305);
		}
		return true;
	}

	public static bool isHoldingAlt()
	{
		if (!Input.GetKey((KeyCode)308))
		{
			return Input.GetKey((KeyCode)307);
		}
		return true;
	}

	public static bool isHoldingAnyMod()
	{
		if (AssetManager.hotkey_library == null)
		{
			return false;
		}
		return AssetManager.hotkey_library.isHoldingAnyModKey();
	}

	public void reset()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		foreach (HotkeyAsset item in list)
		{
			item.overridden_key_1 = item.default_key_1;
			item.overridden_key_2 = item.default_key_2;
			item.overridden_key_3 = item.default_key_3;
			item.overridden_key_mod_1 = item.default_key_mod_1;
			item.overridden_key_mod_2 = item.default_key_mod_2;
			item.overridden_key_mod_3 = item.default_key_mod_3;
		}
	}

	public string replaceSpecialTextKeys(string pText)
	{
		if (!pText.Contains("$"))
		{
			return pText;
		}
		foreach (HotkeyAsset item in list)
		{
			if (pText.Contains(item.id))
			{
				string oldValue = "$" + item.id + "$";
				string localizedKeys = item.getLocalizedKeys();
				pText = pText.Replace(oldValue, localizedKeys);
				if (pText.Contains("$mouse_wheel$"))
				{
					string newValue = Toolbox.coloredText("mouse_wheel", "#95DD5D", pLocalize: true);
					pText = pText.Replace("$mouse_wheel$", newValue);
				}
				if (!pText.Contains("$"))
				{
					return pText;
				}
			}
		}
		return pText;
	}

	public bool isHoldingAnyModKey()
	{
		if (!Input.anyKey)
		{
			return false;
		}
		if (runModKeyCheck)
		{
			runModKeyCheck = false;
			holdingAnyModKey = false;
			KeyCode[] array = mod_keys;
			for (int i = 0; i < array.Length; i++)
			{
				if (Input.GetKey(array[i]))
				{
					holdingAnyModKey = true;
					break;
				}
			}
		}
		return holdingAnyModKey;
	}

	public void checkHotKeyActions()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		runModKeyCheck = true;
		bool flag = Input.mouseScrollDelta.y != 0f;
		if (!World.world.has_focus || (!Input.anyKey && !flag))
		{
			return;
		}
		bool flag2 = isInputActive();
		bool flag3 = _last_input_active && !flag2;
		_last_input_active = flag2;
		if (flag2 | flag3)
		{
			return;
		}
		bool flag4 = MapBox.controlsLocked();
		bool flag5 = MapBox.isControllingUnit();
		HotkeyAsset[] array = action_hotkeys;
		foreach (HotkeyAsset hotkeyAsset in array)
		{
			if ((hotkeyAsset.use_mouse_wheel && !flag) || (hotkeyAsset.check_controls_locked && (flag4 || (flag5 && !hotkeyAsset.allow_unit_control))) || !hotkeyAsset.checkIsPossible())
			{
				continue;
			}
			if (hotkeyAsset.just_pressed_action != null && hotkeyAsset.isJustPressed())
			{
				hotkeyAsset.just_pressed_action(hotkeyAsset);
				if (hotkeyAsset.holding_action != null)
				{
					holding_times[hotkeyAsset.id] = hotkeyAsset.holding_cooldown_first_action;
				}
			}
			else if (hotkeyAsset.holding_action != null && hotkeyAsset.isHolding())
			{
				holding_times.TryGetValue(hotkeyAsset.id, out var value);
				value -= Time.deltaTime;
				if (value > 0f)
				{
					holding_times[hotkeyAsset.id] = value;
					continue;
				}
				hotkeyAsset.holding_action(hotkeyAsset);
				holding_times[hotkeyAsset.id] = hotkeyAsset.holding_cooldown;
			}
		}
	}

	private bool isInputActive()
	{
		if (!EventSystem.current.isFocused)
		{
			return false;
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if ((Object)(object)currentSelectedGameObject == (Object)null)
		{
			return false;
		}
		InputField component = currentSelectedGameObject.GetComponent<InputField>();
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		return component.isFocused;
	}

	public static bool allowedToUsePowers()
	{
		if (ScrollWindow.isWindowActive())
		{
			return false;
		}
		return true;
	}

	public void changeKey(HotkeyAsset pAsset, KeyCode pCode)
	{
	}

	public void load()
	{
	}

	public void hotkeySelectPower(HotkeyAsset pAsset, string pSelectPower)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		if (!string.IsNullOrEmpty(pSelectPower) && AssetManager.powers.get(pSelectPower) == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(pSelectPower))
		{
			showTipNothing(pAsset);
			return;
		}
		PowerButton tPowerButton = PowerButton.get(pSelectPower);
		if ((Object)(object)tPowerButton == (Object)null)
		{
			return;
		}
		if (tPowerButton.isSelected())
		{
			tPowerButton.cancelSelection();
			return;
		}
		tPowerButton.selectPowerTab((TweenCallback)delegate
		{
			World.world.selected_buttons.clickPowerButton(tPowerButton);
			if (tPowerButton.isSelected())
			{
				WorldTip.instance.showToolbarText(tPowerButton.godPower);
			}
		});
	}

	public void hotkeySelectNano(HotkeyAsset pAsset, string pSelectNano)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(pSelectNano))
		{
			showTipNothing(pAsset);
			return;
		}
		string[] array = pSelectNano.Split("|");
		string text = array[0];
		long pId = long.Parse(array[1]);
		MetaTypeAsset metaTypeAsset = AssetManager.meta_type_library.get(text);
		NanoObject nanoObject = metaTypeAsset.get(pId);
		if (nanoObject.isRekt() && array.Length < 3)
		{
			showTipNothing(pAsset);
			return;
		}
		NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
		if (SelectedObjects.isNanoObjectSet() && SelectedObjects.getSelectedNanoObject() == nanoObject)
		{
			if (selectedNanoObject == SelectedUnit.unit)
			{
				World.world.locatePosition(Vector2.op_Implicit(SelectedUnit.unit.current_position));
			}
			else if (nanoObject is IMetaObject)
			{
				Actor randomUnit = (nanoObject as IMetaObject).getRandomUnit();
				if (randomUnit != null)
				{
					World.world.locatePosition(Vector2.op_Implicit(randomUnit.current_position));
				}
			}
			return;
		}
		if (World.world.isAnyPowerSelected())
		{
			PowerButtonSelector.instance.unselectAll();
		}
		SelectedObjects.unselectNanoObject();
		SelectedUnit.clear();
		if (text == "unit")
		{
			if (array.Length >= 3)
			{
				using (ListPool<Actor> listPool = new ListPool<Actor>(array.Length))
				{
					for (int i = 1; i < array.Length; i++)
					{
						long pID = long.Parse(array[i]);
						Actor actor = World.world.units.get(pID);
						if (!actor.isRekt())
						{
							listPool.Add(actor);
						}
					}
					if (listPool.Count > 0)
					{
						SelectedUnit.selectMultiple(listPool);
						SelectedObjects.setNanoObject(SelectedUnit.unit);
						if (selectedNanoObject == SelectedUnit.unit)
						{
							World.world.locatePosition(Vector2.op_Implicit(SelectedUnit.unit.current_position));
						}
					}
					if (listPool.Count == 0)
					{
						showTipNothing(pAsset);
					}
					else if (listPool.Count == 1)
					{
						PowerTabController.showTabSelectedUnit();
					}
					else
					{
						PowerTabController.showTabMultipleUnits();
					}
					return;
				}
			}
			SelectedUnit.select(nanoObject as Actor);
			SelectedObjects.setNanoObject(SelectedUnit.unit);
			PowerTabController.showTabSelectedUnit();
		}
		else
		{
			metaTypeAsset.selectAndInspect(nanoObject, pFromNameplate: false, pCheckNameplate: false);
		}
	}

	public void showTipNothing(HotkeyAsset pAsset)
	{
		string text = LocalizedTextManager.getText("hotkey_tip_empty_tip");
		text = text.Replace("$save_hotkey$", "$save_" + pAsset.id + "$");
		text = AssetManager.hotkey_library.replaceSpecialTextKeys(text);
		WorldTip.instance.showToolbarText(text);
	}

	public void hotkeySavePower(HotkeyAsset pAsset)
	{
		string text = World.world.getSelectedPowerID();
		string text2 = pAsset.id.Replace("save_", "");
		string text3 = "";
		if (string.IsNullOrEmpty(text))
		{
			text = string.Empty;
			text3 = LocalizedTextManager.getText("hotkey_tip_cleared");
		}
		else
		{
			text3 = LocalizedTextManager.getText("hotkey_tip_saved_power");
		}
		text3 = text3.Replace("$save_hotkey$", "$" + text2 + "$");
		text3 = AssetManager.hotkey_library.replaceSpecialTextKeys(text3);
		WorldTip.instance.showToolbarText(text3);
		PlayerConfig.dict[text2].stringVal = text;
		PlayerConfig.saveData();
		getHotkeyFromData(text2) = string.Empty;
	}

	public void hotkeySaveTab(HotkeyAsset pAsset)
	{
		string text = pAsset.id.Replace("save_", "");
		string text2 = "";
		string text3;
		if (!SelectedObjects.isNanoObjectSet())
		{
			text2 = LocalizedTextManager.getText("hotkey_tip_cleared");
			text3 = string.Empty;
		}
		else
		{
			text2 = LocalizedTextManager.getText("hotkey_tip_saved_nano");
			NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
			text3 = selectedNanoObject.getMetaTypeAsset().id ?? "";
			if (SelectedUnit.isSet())
			{
				foreach (Actor allSelected in SelectedUnit.getAllSelectedList())
				{
					text3 += $"|{allSelected.id}";
				}
			}
			else
			{
				text3 += $"|{selectedNanoObject.id}";
			}
		}
		text2 = text2.Replace("$save_hotkey$", "$" + text + "$");
		text2 = AssetManager.hotkey_library.replaceSpecialTextKeys(text2);
		getHotkeyFromData(text) = text3;
		WorldTip.instance.showToolbarText(text2);
	}

	public ref string getHotkeyFromData(string pHotkeyId)
	{
		switch (pHotkeyId)
		{
		case "hotkey_1":
			return ref World.world.hotkey_tabs_data.hotkey_data_1;
		case "hotkey_2":
			return ref World.world.hotkey_tabs_data.hotkey_data_2;
		case "hotkey_3":
			return ref World.world.hotkey_tabs_data.hotkey_data_3;
		case "hotkey_4":
			return ref World.world.hotkey_tabs_data.hotkey_data_4;
		case "hotkey_5":
			return ref World.world.hotkey_tabs_data.hotkey_data_5;
		case "hotkey_6":
			return ref World.world.hotkey_tabs_data.hotkey_data_6;
		case "hotkey_7":
			return ref World.world.hotkey_tabs_data.hotkey_data_7;
		case "hotkey_8":
			return ref World.world.hotkey_tabs_data.hotkey_data_8;
		case "hotkey_9":
			return ref World.world.hotkey_tabs_data.hotkey_data_9;
		case "hotkey_0":
			return ref World.world.hotkey_tabs_data.hotkey_data_0;
		default:
			return ref World.world.hotkey_tabs_data.hotkey_data_1;
		}
	}

	public void initDebugHotkeys()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		initDebugHotkeysBase();
		initUnitDebugHotkeys();
		initDebugWindowHotkeys();
		add(new HotkeyAsset
		{
			id = "debug_autosave",
			default_key_1 = (KeyCode)115,
			default_key_mod_1 = (KeyCode)308,
			just_pressed_action = debugAutosave
		});
		add(new HotkeyAsset
		{
			id = "debug_next_test_map",
			default_key_1 = (KeyCode)280,
			just_pressed_action = delegate
			{
				if (!SmoothLoader.isLoading())
				{
					World.world.transition_screen.startTransition(TestMaps.loadNextMap);
				}
			}
		});
		add(new HotkeyAsset
		{
			id = "debug_prev_test_map",
			default_key_1 = (KeyCode)281,
			just_pressed_action = delegate
			{
				if (!SmoothLoader.isLoading())
				{
					World.world.transition_screen.startTransition(TestMaps.loadPrevMap);
				}
			}
		});
	}

	private void initDebugHotkeysBase()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		add(new HotkeyAsset
		{
			id = "export_unit_sprites",
			default_key_1 = (KeyCode)121,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				WorldTip.instance.showToolbarText("Exporting unit sprites");
				AssetManager.dynamic_sprites_library.export();
			}
		});
		add(new HotkeyAsset
		{
			id = "autotester",
			default_key_1 = (KeyCode)117,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				World.world.auto_tester.toggleAutoTester();
			}
		});
		add(new HotkeyAsset
		{
			id = "test_zones_border_growth",
			default_key_1 = (KeyCode)111,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				DebugZonesTool.actionGrowBorder();
			}
		});
		add(new HotkeyAsset
		{
			id = "test_zones_abandon_zones",
			default_key_1 = (KeyCode)112,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				WorldTile[] tiles_list = World.world.tiles_list;
				foreach (WorldTile pTile in tiles_list)
				{
					World.world.buildings.addBuilding("poop", pTile);
				}
			}
		});
		add(new HotkeyAsset
		{
			id = "test_colors",
			default_key_1 = (KeyCode)114,
			check_window_not_active = true,
			check_controls_locked = true,
			just_pressed_action = delegate
			{
				foreach (Kingdom kingdom in World.world.kingdoms)
				{
					kingdom.generateBanner();
					ColorAsset random = AssetManager.kingdom_colors_library.list.GetRandom();
					kingdom.data.setColorID(AssetManager.kingdom_colors_library.list.IndexOf(random));
					if (kingdom.updateColor(random))
					{
						World.world.zone_calculator.dirtyAndClear();
					}
				}
			}
		});
	}

	private void initDebugWindowHotkeys()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		add(new HotkeyAsset
		{
			id = "debug_building_shadow_x_increase",
			default_key_1 = (KeyCode)120,
			default_key_mod_1 = (KeyCode)306,
			just_pressed_action = debugShadow,
			check_controls_locked = true,
			check_window_active = true,
			check_debug_active = true
		});
		clone("debug_building_shadow_x_reduce", "debug_building_shadow_x_increase");
		t.default_key_mod_1 = (KeyCode)304;
		clone("debug_building_shadow_y_increase", "debug_building_shadow_x_increase");
		t.default_key_1 = (KeyCode)121;
		clone("debug_building_shadow_y_reduce", "debug_building_shadow_y_increase");
		t.default_key_mod_1 = (KeyCode)304;
		clone("debug_building_shadow_distortion_increase", "debug_building_shadow_x_increase");
		t.default_key_1 = (KeyCode)100;
		clone("debug_building_shadow_distortion_reduce", "debug_building_shadow_distortion_increase");
		t.default_key_mod_1 = (KeyCode)304;
	}

	private void initUnitDebugHotkeys()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		add(new HotkeyAsset
		{
			id = "debug_unit_set_task",
			default_key_1 = (KeyCode)118,
			default_key_mod_1 = (KeyCode)306,
			check_window_not_active = true,
			check_controls_locked = true,
			check_render_gameplay = true,
			check_debug_active = true,
			just_pressed_action = delegate
			{
				if (DebugConfig.isOn(DebugOption.DebugUnitHotkeys))
				{
					World.world.getActorNearCursor()?.addStatusEffect("budding");
				}
			}
		});
		add(new HotkeyAsset
		{
			id = "debug_general_key",
			default_key_1 = (KeyCode)110,
			check_debug_active = true,
			just_pressed_action = delegate
			{
				if (!DebugConfig.isOn(DebugOption.DebugUnitHotkeys) || !SelectedUnit.isSet())
				{
					return;
				}
				using ListPool<Actor> listPool = new ListPool<Actor>(SelectedUnit.getAllSelected());
				foreach (ref Actor item in listPool)
				{
					item.getHitFullHealth(AttackType.Divine);
				}
			}
		});
		add(new HotkeyAsset
		{
			id = "debug_monolith",
			default_key_1 = (KeyCode)109,
			default_key_mod_1 = (KeyCode)306,
			check_window_not_active = true,
			check_controls_locked = true,
			check_render_gameplay = true,
			check_debug_active = true,
			just_pressed_action = delegate
			{
				if (!DebugConfig.isOn(DebugOption.DebugMonolith))
				{
					return;
				}
				foreach (Building building in World.world.buildings)
				{
					if (building.asset.id == "monolith")
					{
						BuildingMonolith component_monolith = building.component_monolith;
						component_monolith.doMonolithAction(component_monolith.building.current_tile, pForce: true);
					}
				}
			}
		});
	}

	private void debugAutosave(HotkeyAsset pAsset)
	{
		if (Config.isEditor)
		{
			AutoSaveManager.autoSave(pSkipDelete: true, pForce: true);
		}
	}

	private void debugShadow(HotkeyAsset pAsset)
	{
		if (!DebugConfig.isOn(DebugOption.DebugWindowHotkeys) || ((Object)ScrollWindow.getCurrentWindow()).name != "building_asset")
		{
			return;
		}
		BuildingAsset asset = BaseDebugAssetWindow<BuildingAsset, BuildingDebugAssetElement>.current_element.asset;
		if (asset.shadow)
		{
			switch (pAsset.id)
			{
			case "debug_building_shadow_x_increase":
				asset.shadow_bound.x += 0.05f;
				break;
			case "debug_building_shadow_x_reduce":
				asset.shadow_bound.x -= 0.05f;
				break;
			case "debug_building_shadow_y_increase":
				asset.shadow_bound.y += 0.05f;
				break;
			case "debug_building_shadow_y_reduce":
				asset.shadow_bound.y -= 0.05f;
				break;
			case "debug_building_shadow_distortion_increase":
				asset.shadow_distortion += 0.05f;
				break;
			case "debug_building_shadow_distortion_reduce":
				asset.shadow_distortion -= 0.05f;
				break;
			}
			Debug.Log((object)("t.setShadow(" + asset.shadow_bound.x.ToString(CultureInfo.InvariantCulture) + "f, " + asset.shadow_bound.y.ToString(CultureInfo.InvariantCulture) + "f, " + asset.shadow_distortion.ToString(CultureInfo.InvariantCulture) + "f);"));
			BuildingAssetWindow.reloadSprites();
		}
	}

	public void debug(DebugTool pTool)
	{
		foreach (HotkeyAsset item in list)
		{
			if (item.just_pressed_action == null && item.holding_action == null)
			{
				if (item.isJustPressed())
				{
					pTool.setText(item.id, "just_pressed", 0f, pShowBar: false, 0L);
				}
				if (item.isHolding())
				{
					pTool.setText(item.id, "holding", 0f, pShowBar: false, 0L);
				}
			}
		}
	}
}
