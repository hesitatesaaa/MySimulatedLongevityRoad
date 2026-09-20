using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class StatisticsAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	public string localized_key;

	public string localized_key_description;

	public LocaleGetter locale_getter;

	public int rarity;

	[DefaultValue("#Status_stat")]
	public string steam_activity = "#Status_stat";

	public StatisticsStringAction string_action = (StatisticsAsset pAsset) => (pAsset.long_action != null) ? pAsset.long_action(pAsset).ToText() : null;

	public StatisticsLongAction long_action;

	public MetaIdGetter get_meta_id;

	[NonSerialized]
	public string last_value = string.Empty;

	public bool is_world_statistics;

	public bool is_game_statistics;

	public WorldStatsTabs world_stats_tabs;

	[DefaultValue(MetaType.None)]
	public MetaType world_stats_meta_type;

	[DefaultValue(MetaType.None)]
	public MetaType list_window_meta_type;

	public string path_icon;

	private Sprite _icon;

	public Sprite getIcon()
	{
		if ((Object)(object)_icon == (Object)null && !string.IsNullOrEmpty(path_icon))
		{
			_icon = SpriteTextureLoader.getSprite(path_icon);
		}
		return _icon;
	}

	public string getLocaleID()
	{
		return localized_key.Underscore() ?? id;
	}

	public string getDescriptionID()
	{
		string text = getLocaleID() + "_description";
		if (!string.IsNullOrEmpty(localized_key_description))
		{
			text = localized_key_description;
		}
		if (LocalizedTextManager.stringExists(text))
		{
			return text;
		}
		return getLocaleID() + "_description";
	}
}
