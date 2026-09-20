using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class HotkeyAsset : Asset
{
	public KeyCode default_key_mod_1;

	public KeyCode default_key_mod_2;

	public KeyCode default_key_mod_3;

	public KeyCode default_key_1;

	public KeyCode default_key_2;

	public KeyCode default_key_3;

	public KeyCode overridden_key_1;

	public KeyCode overridden_key_2;

	public KeyCode overridden_key_3;

	public KeyCode overridden_key_mod_1;

	public KeyCode overridden_key_mod_2;

	public KeyCode overridden_key_mod_3;

	public bool use_mouse_wheel;

	public HotkeyAction just_pressed_action;

	public HotkeyAction holding_action;

	[DefaultValue(0.1f)]
	public float holding_cooldown = 0.1f;

	[DefaultValue(0.33f)]
	public float holding_cooldown_first_action = 0.33f;

	public bool ignore_same_key_diagnostic;

	public bool disable_for_controlled_unit;

	public bool ignore_mod_keys;

	public bool check_only_controllable_unit;

	public bool check_only_not_controllable_unit;

	public bool check_controls_locked;

	public bool check_window_active;

	public bool check_window_not_active;

	public bool check_render_gameplay;

	public bool check_render_minimap;

	public bool check_debug_active;

	public bool check_no_multi_unit_selection;

	public bool check_no_selection;

	public bool check_multi_unit_selection;

	public bool allow_unit_control;

	public bool isJustPressed()
	{
		if (!Input.anyKeyDown)
		{
			return false;
		}
		if (disable_for_controlled_unit && ControllableUnit.isControllingUnit())
		{
			return false;
		}
		if (hasModKey())
		{
			if (!isHoldingModKey())
			{
				return false;
			}
			if (!hasKey() && isJustPressedModKey())
			{
				return true;
			}
		}
		else if (!ignore_mod_keys && isHoldingAnyModKey())
		{
			return false;
		}
		if (hasKey() && isJustPressedKey())
		{
			return true;
		}
		return false;
	}

	public bool isHolding()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (use_mouse_wheel)
		{
			if (Input.mouseScrollDelta.y == 0f)
			{
				return false;
			}
		}
		else if (!Input.anyKey)
		{
			return false;
		}
		if (disable_for_controlled_unit && ControllableUnit.isControllingUnit())
		{
			return false;
		}
		if (hasModKey())
		{
			if (!isHoldingModKey())
			{
				return false;
			}
			if (!hasKey() && isHoldingModKey())
			{
				return true;
			}
		}
		else if (!ignore_mod_keys && isHoldingAnyModKey())
		{
			return false;
		}
		if (hasKey() && isHoldingKey())
		{
			return true;
		}
		if (use_mouse_wheel)
		{
			return true;
		}
		return false;
	}

	private bool isHoldingModKey()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.anyKey)
		{
			return false;
		}
		if (disable_for_controlled_unit && ControllableUnit.isControllingUnit())
		{
			return false;
		}
		if ((int)default_key_mod_1 != 0 && Input.GetKey(default_key_mod_1))
		{
			return true;
		}
		if ((int)default_key_mod_2 != 0 && Input.GetKey(default_key_mod_2))
		{
			return true;
		}
		if ((int)default_key_mod_3 != 0 && Input.GetKey(default_key_mod_3))
		{
			return true;
		}
		return false;
	}

	public static bool isHoldingAnyModKey()
	{
		return AssetManager.hotkey_library.isHoldingAnyModKey();
	}

	private bool hasKey()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		return (int)default_key_1 > 0;
	}

	private bool hasModKey()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		return (int)default_key_mod_1 > 0;
	}

	public string getLocalizedKeys()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		string localizedKey = HotkeysLocalized.getLocalizedKey(default_key_1);
		string localizedKey2 = HotkeysLocalized.getLocalizedKey(default_key_2);
		string localizedKey3 = HotkeysLocalized.getLocalizedKey(default_key_3);
		string localizedKey4 = HotkeysLocalized.getLocalizedKey(default_key_mod_1);
		string localizedKey5 = HotkeysLocalized.getLocalizedKey(default_key_mod_2);
		string localizedKey6 = HotkeysLocalized.getLocalizedKey(default_key_mod_3);
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(localizedKey))
		{
			list.Add(localizedKey);
		}
		if (!string.IsNullOrEmpty(localizedKey2))
		{
			list.Add(localizedKey2);
		}
		if (!string.IsNullOrEmpty(localizedKey3))
		{
			list.Add(localizedKey3);
		}
		List<string> list2 = new List<string>();
		if (!string.IsNullOrEmpty(localizedKey4))
		{
			list2.Add(localizedKey4);
		}
		if (!string.IsNullOrEmpty(localizedKey5))
		{
			list2.Add(localizedKey5);
		}
		if (!string.IsNullOrEmpty(localizedKey6))
		{
			list2.Add(localizedKey6);
		}
		list = new List<string>(new HashSet<string>(list));
		list2 = new List<string>(new HashSet<string>(list2));
		if (hasKey() && hasModKey())
		{
			int num = Mathf.Max(list.Count, list2.Count);
			string text2 = "";
			string text3 = "";
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					text += " / ";
				}
				if (i < list2.Count)
				{
					text2 = list2[i];
				}
				if (i < list.Count)
				{
					text3 = list[i];
				}
				text = text + text2 + " + " + text3;
			}
		}
		else if (hasModKey())
		{
			text += string.Join(", ", list2);
		}
		else if (hasKey())
		{
			text += string.Join(", ", list);
		}
		return text;
	}

	private bool isHoldingKey()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.anyKey)
		{
			return false;
		}
		if ((int)default_key_1 != 0 && Input.GetKey(default_key_1))
		{
			return true;
		}
		if ((int)default_key_2 != 0 && Input.GetKey(default_key_2))
		{
			return true;
		}
		if ((int)default_key_3 != 0 && Input.GetKey(default_key_3))
		{
			return true;
		}
		return false;
	}

	private bool isJustPressedKey()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.anyKeyDown)
		{
			return false;
		}
		if ((int)default_key_1 != 0 && Input.GetKeyDown(default_key_1))
		{
			return true;
		}
		if ((int)default_key_2 != 0 && Input.GetKeyDown(default_key_2))
		{
			return true;
		}
		if ((int)default_key_3 != 0 && Input.GetKeyDown(default_key_3))
		{
			return true;
		}
		return false;
	}

	private bool isJustPressedModKey()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.anyKeyDown)
		{
			return false;
		}
		if ((int)default_key_mod_1 != 0 && Input.GetKeyDown(default_key_mod_1))
		{
			return true;
		}
		if ((int)default_key_mod_2 != 0 && Input.GetKeyDown(default_key_mod_2))
		{
			return true;
		}
		if ((int)default_key_mod_3 != 0 && Input.GetKeyDown(default_key_mod_3))
		{
			return true;
		}
		return false;
	}

	public bool checkIsPossible()
	{
		if (check_render_gameplay && !MapBox.isRenderGameplay())
		{
			return false;
		}
		if (check_render_minimap && !MapBox.isRenderMiniMap())
		{
			return false;
		}
		if (check_window_active)
		{
			if (!ScrollWindow.isWindowActive())
			{
				return false;
			}
			if (ScrollWindow.isAnimationActive())
			{
				return false;
			}
		}
		if (check_window_not_active && ScrollWindow.isWindowActive())
		{
			return false;
		}
		if (check_no_selection && SelectedUnit.isSet())
		{
			return false;
		}
		if (check_no_multi_unit_selection && SelectedUnit.multipleSelected())
		{
			return false;
		}
		if (check_multi_unit_selection && !SelectedUnit.multipleSelected())
		{
			return false;
		}
		if (check_only_not_controllable_unit && ControllableUnit.isControllingUnit())
		{
			return false;
		}
		if (check_only_controllable_unit && !ControllableUnit.isControllingUnit())
		{
			return false;
		}
		return true;
	}
}
