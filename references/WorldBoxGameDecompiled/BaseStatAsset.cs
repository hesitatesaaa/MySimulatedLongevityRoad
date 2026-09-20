using System;
using System.ComponentModel;

[Serializable]
public class BaseStatAsset : Asset, ILocalizedAsset
{
	public bool hidden;

	public string icon;

	public bool normalize;

	public float normalize_min;

	[DefaultValue(2.1474836E+09f)]
	public float normalize_max = 2.1474836E+09f;

	public bool used_only_for_civs;

	public bool actor_data_attribute;

	public bool show_as_percents;

	[DefaultValue(1f)]
	public float tooltip_multiply_for_visual_number = 1f;

	public bool multiplier;

	public string main_stat_to_multiply;

	public int sort_rank;

	public bool ignore;

	public string translation_key;

	public string getLocaleID()
	{
		return translation_key ?? id;
	}
}
