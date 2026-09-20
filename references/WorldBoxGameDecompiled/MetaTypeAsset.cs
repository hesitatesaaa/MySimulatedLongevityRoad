using System;
using UnityEngine;

[Serializable]
public class MetaTypeAsset : Asset
{
	public MetaType map_mode;

	public string option_id = string.Empty;

	public string power_option_zone_id = string.Empty;

	public string power_tab_id = string.Empty;

	public string window_name;

	public bool force_zone_when_selected;

	public int[] ranks;

	public string[] reports;

	public MetaCheckUnitWindowAction check_unit_has_meta;

	public MetaUnitSetMetaForWindow set_unit_set_meta_for_meta_for_window;

	public MetaZoneDrawAction draw_zones;

	public MetaZoneClickAction click_action_zone;

	public MetaZoneHighlightAction check_cursor_highlight;

	public MetaZoneGetMeta tile_get_metaobject;

	public MetaZoneGetMetaSimple tile_get_metaobject_0;

	public MetaZoneGetMetaSimple tile_get_metaobject_1;

	public MetaZoneGetMetaSimple tile_get_metaobject_2;

	public MetaZoneTooltipAction check_tile_has_meta;

	public MetaZoneTooltipAction check_cursor_tooltip;

	public MetaTooltipShowAction cursor_tooltip_action;

	public MetaZoneDynamicAction dynamic_zones;

	public MetaTypeAction window_action_clear;

	public MetaTypeAction selected_tab_action;

	public MetaTypeActionAsset selected_tab_action_meta;

	public MetaTypeHistoryAction window_history_action_update;

	public MetaTypeHistoryAction window_history_action_restore;

	public int dynamic_zone_option = 2;

	public bool has_dynamic_zones;

	public string icon_list;

	public string icon_single_path;

	public MetaTypeListAction get_list;

	public MetaTypeListPoolAction get_sorted_list;

	public MetaTypeListHasAction has_any;

	public MetaSelectedGetter get_selected;

	public MetaSelectedSetter set_selected;

	public MetaGetter get;

	public MetaStatAction stat_hover;

	public MetaStatAction stat_click;

	public MetaTypeListPoolAction custom_sorted_list;

	public string[] decision_ids;

	[NonSerialized]
	public DecisionAsset[] decisions_assets;

	[NonSerialized]
	public OptionAsset option_asset;

	public bool unit_amount_alpha;

	public bool set_icon_for_cancel_button;

	private static int _last_call_frame = -1;

	public static string last_meta_type;

	public bool hasDecisions()
	{
		string[] array = decision_ids;
		if (array == null)
		{
			return false;
		}
		return array.Length != 0;
	}

	public int getZoneOptionState()
	{
		if (!Zones.getForcedMapMode().isNone() && Zones.isPowerForcedMapModeEnabled())
		{
			return 0;
		}
		return option_asset.current_int_value;
	}

	internal void toggleOptionZone(GodPower pPower, int pDirection = 1, bool pDisable = true)
	{
		PlayerOptionData data = option_asset.data;
		if (data.boolVal)
		{
			data.intVal += pDirection;
			if (data.intVal > option_asset.max_value)
			{
				data.intVal = 0;
				if (pDisable)
				{
					data.boolVal = false;
				}
			}
			if (data.intVal < 0)
			{
				data.intVal = option_asset.max_value;
			}
		}
		else
		{
			data.boolVal = true;
		}
		if (pPower.map_modes_switch)
		{
			if (data.boolVal)
			{
				PowerLibrary.disableAllOtherMapModes(pPower.id);
			}
			else
			{
				WorldTip.instance.startHide();
			}
		}
		PlayerConfig.saveData();
		string translatedName = pPower.getTranslatedName();
		string translatedDescription = pPower.getTranslatedDescription();
		string translatedOption = option_asset.getTranslatedOption();
		if (data.boolVal)
		{
			WorldTip.instance.showToolbarText(translatedName + " - " + translatedOption, translatedDescription);
		}
	}

	public bool isMetaZoneOptionSelectedFluid()
	{
		return getZoneOptionState() == dynamic_zone_option;
	}

	public bool isActive(bool pOnlyOption = false)
	{
		if (pOnlyOption)
		{
			return isOptionActive();
		}
		if (!isOptionActive())
		{
			return Zones.isPowerForceMapMode(map_mode);
		}
		return true;
	}

	public bool isOptionActive()
	{
		return PlayerConfig.optionBoolEnabled(option_id);
	}

	public ListPool<NanoObject> getSortedList()
	{
		if (get_sorted_list != null)
		{
			return get_sorted_list();
		}
		if (custom_sorted_list != null)
		{
			return custom_sorted_list();
		}
		return new ListPool<NanoObject>(get_list());
	}

	public bool hasRanks()
	{
		return ranks != null;
	}

	public void setListGetter(MetaTypeListPoolAction pListAction)
	{
		get_sorted_list = pListAction;
	}

	public void selectAndInspect(NanoObject pNewNanoObject, bool pFromNameplate = false, bool pCheckNameplate = true, bool pClearAction = false)
	{
		int frameCount = Time.frameCount;
		if (_last_call_frame == frameCount)
		{
			return;
		}
		_last_call_frame = frameCount;
		if (pCheckNameplate && World.world.nameplate_manager.isOverNameplate() && !pFromNameplate)
		{
			return;
		}
		NanoObject nanoObject = get_selected();
		string text = last_meta_type;
		set_selected(pNewNanoObject);
		SelectedObjects.setNanoObject(pNewNanoObject);
		bool flag = nanoObject.isRekt();
		bool flag2 = nanoObject == pNewNanoObject;
		bool flag3 = text == last_meta_type;
		bool flag4 = power_tab_id == PowersTab.getActiveTab().getAsset().id;
		if (HotkeyLibrary.isHoldingAnyMod())
		{
			ScrollWindow.showWindow(window_name);
			return;
		}
		if ((!flag & flag2 & flag3 & flag4) && !pClearAction)
		{
			ScrollWindow.showWindow(window_name);
			return;
		}
		SelectedTabsHistory.addToHistory(pNewNanoObject);
		if (selected_tab_action != null)
		{
			selected_tab_action();
		}
		else if (selected_tab_action_meta != null)
		{
			SelectedUnit.clear();
			selected_tab_action_meta(this);
		}
		else if (!pClearAction)
		{
			ScrollWindow.showWindow(window_name);
		}
		else
		{
			SelectedTabsHistory.showPreviousTab();
		}
	}

	public Sprite getIconSprite()
	{
		return SpriteTextureLoader.getSprite(icon_single_path);
	}
}
