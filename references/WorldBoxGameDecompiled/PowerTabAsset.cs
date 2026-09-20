using System;
using UnityEngine;

[Serializable]
public class PowerTabAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	[NonSerialized]
	public PowersTab gameplay_tab;

	public bool tab_type_selected;

	public bool tab_type_main;

	public PowerTabGetter get_power_tab;

	public string window_id;

	public MetaType meta_type;

	public PowerTabAction on_main_tab_select;

	public PowerTabAction on_main_info_click;

	public PowerTabActionCheck on_update_check_active;

	public PowerTabWorldtipAction get_localized_worldtip;

	public string icon_path;

	public string locale_key;

	[NonSerialized]
	public float last_scroll_position;

	public void tryToShowPowerTab()
	{
		PowersTab powersTab = get_power_tab?.Invoke();
		if ((Object)(object)powersTab == (Object)null)
		{
			Debug.LogError((object)("PowerTabAsset: tryToShowPowerTab: get_power_tab returned null for " + meta_type));
		}
		else
		{
			powersTab.tryToShowTab();
		}
	}

	public string getLocaleID()
	{
		return locale_key;
	}

	public string getDescriptionID()
	{
		if (locale_key == null)
		{
			return null;
		}
		return locale_key + "_info";
	}

	public Sprite getIcon()
	{
		return SpriteTextureLoader.getSprite(icon_path);
	}
}
