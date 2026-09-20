using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class GodPower : Asset, IDescriptionAsset, ILocalizedAsset
{
	internal static List<PowerButton> powers_rank_0 = new List<PowerButton>();

	internal static List<PowerButton> powers_rank_1 = new List<PowerButton>();

	internal static List<PowerButton> powers_rank_2 = new List<PowerButton>();

	internal static List<PowerButton> powers_rank_3 = new List<PowerButton>();

	internal static List<PowerButton> powers_rank_4 = new List<PowerButton>();

	internal static List<PowerButton> premium_buttons = new List<PowerButton>();

	internal static List<GodPower> premium_powers = new List<GodPower>();

	internal static Dictionary<string, GodPower> god_powers_on_canvas = new Dictionary<string, GodPower>();

	public string name = "DEFAULT NAME";

	public bool requires_premium;

	[DefaultValue(PowerRank.Rank0_free)]
	public PowerRank rank;

	public string path_icon;

	public bool multiple_spawn_tip;

	public bool show_unit_stats_overview;

	public bool show_tool_sizes;

	public bool unselect_when_window;

	public bool make_buildings_transparent;

	[DefaultValue(MetaType.None)]
	public MetaType force_map_mode;

	public bool ignore_cursor_icon;

	public bool hold_action;

	public float click_interval;

	public float particle_interval;

	[DefaultValue(0.95f)]
	public float falling_chance = 0.95f;

	public string sound_drawing;

	public string sound_event;

	public string tile_type;

	[NonSerialized]
	internal TileType cached_tile_type_asset;

	public string top_tile_type;

	[NonSerialized]
	internal TopTileType cached_top_tile_type_asset;

	public string drop_id;

	[NonSerialized]
	internal DropAsset cached_drop_asset;

	public string force_brush;

	public bool terraform;

	public bool draw_lines;

	[DefaultValue(PowerActionType.PowerSpecial)]
	public PowerActionType type;

	[DefaultValue(MouseHoldAnimation.Default)]
	public MouseHoldAnimation mouse_hold_animation;

	public bool highlight;

	public PowerActionWithID click_brush_action;

	public PowerActionWithID click_action;

	public PowerActionWithID click_special_action;

	public PowerAction click_power_brush_action;

	public PowerAction click_power_action;

	public PowerButtonClickAction select_button_action;

	public bool disabled_on_mobile;

	public string toggle_name;

	public bool multi_toggle;

	public PowerToggleAction toggle_action;

	public string actor_asset_id;

	public string[] actor_asset_ids;

	[DefaultValue(6f)]
	public float actor_spawn_height = 6f;

	public bool show_spawn_effect;

	public string printers_print;

	public bool ignore_fast_spawn;

	public bool set_used_camera_drag_on_long_move;

	public bool can_drag_map;

	[DefaultValue(true)]
	public bool tester_enabled = true;

	[DefaultValue(true)]
	public bool track_activity = true;

	public bool map_modes_switch;

	public bool allow_unit_selection;

	public bool show_close_actor;

	[DefaultValue(true)]
	public bool activate_on_hotkey_select = true;

	[DefaultValue(true)]
	public bool surprises_units = true;

	[NonSerialized]
	public Sprite sprite_icon;

	[JsonIgnore]
	public bool has_sound_drawing => sound_drawing != null;

	[JsonIgnore]
	public bool has_sound_event => sound_event != null;

	[JsonIgnore]
	public OptionAsset option_asset => AssetManager.options_library.get(toggle_name);

	public bool isSelected()
	{
		return PowerButtonSelector.instance.selectedButton?.godPower == this;
	}

	public bool isAvailable()
	{
		return true;
	}

	internal static void addPower(GodPower pPower, PowerButton pButton)
	{
		god_powers_on_canvas[pPower.id] = pPower;
		if (pPower.requires_premium)
		{
			premium_powers.Add(pPower);
			premium_buttons.Add(pButton);
		}
		if (!pPower.requires_premium)
		{
			powers_rank_0.Add(pButton);
			return;
		}
		switch (pPower.rank)
		{
		case PowerRank.Rank1_common:
			powers_rank_1.Add(pButton);
			break;
		case PowerRank.Rank2_normal:
			powers_rank_2.Add(pButton);
			break;
		case PowerRank.Rank3_good:
			powers_rank_3.Add(pButton);
			break;
		case PowerRank.Rank4_awesome:
			powers_rank_4.Add(pButton);
			break;
		case PowerRank.Rank0_free:
		case PowerRank.Rank5_noAwards:
			break;
		}
	}

	public Sprite getIconSprite()
	{
		return SpriteTextureLoader.getSprite("ui/Icons/" + path_icon);
	}

	public static void diagnostic()
	{
		LogText.log("Ranked Powers", "Print");
		printRankedPowerButtons("rank0", powers_rank_0);
		printRankedPowerButtons("rank1", powers_rank_1);
		printRankedPowerButtons("rank2", powers_rank_2);
		printRankedPowerButtons("rank3", powers_rank_3);
		printRankedPowerButtons("rank4", powers_rank_4);
		printRankedPowers("premium powers", premium_powers);
	}

	private static void printRankedPowerButtons(string pID, List<PowerButton> pList)
	{
		string text = "";
		foreach (PowerButton p in pList)
		{
			text = text + p.godPower.id + ", ";
		}
		if (text.Length > 2)
		{
			text = text.Substring(0, text.Length - 2);
		}
		LogText.log(pID, text);
	}

	private static void printRankedPowers(string pID, List<GodPower> pList)
	{
		string text = "";
		foreach (GodPower p in pList)
		{
			text = text + p.id + ", ";
		}
		text = text.Substring(0, text.Length - 2);
		LogText.log(pID, text);
	}

	public string getActorAssetID()
	{
		if (actor_asset_id != null)
		{
			return actor_asset_id;
		}
		string[] array = actor_asset_ids;
		if (array != null && array.Length != 0)
		{
			return actor_asset_ids[0];
		}
		return null;
	}

	public ActorAsset getActorAsset()
	{
		return AssetManager.actor_library.get(getActorAssetID());
	}

	public string getLocaleID()
	{
		return name.Underscore();
	}

	public string getDescriptionID()
	{
		return getLocaleID() + "_description";
	}

	public string getTranslatedName()
	{
		return getLocaleID().Localize();
	}

	public string getTranslatedDescription()
	{
		return getDescriptionID().Localize();
	}
}
