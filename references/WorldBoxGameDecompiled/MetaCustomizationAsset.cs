using System;
using System.Collections.Generic;

[Serializable]
public class MetaCustomizationAsset : Asset, IMultiLocalesAsset
{
	public string localization_title;

	public MetaType meta_type;

	public string banner_prefab_id;

	public MetaBanner get_banner;

	public bool editable = true;

	public bool option_1_editable = true;

	public bool option_2_editable = true;

	public bool option_2_color_editable = true;

	public bool color_editable = true;

	public MetaCustomizationComponent customize_component;

	public string customize_window_id;

	public MetaCustomizationOptionGet option_1_get;

	public MetaCustomizationOptionSet option_1_set;

	public MetaCustomizationOptionGet option_2_get;

	public MetaCustomizationOptionSet option_2_set;

	public MetaCustomizationOptionGet color_get;

	public MetaCustomizationOptionSet color_set;

	public MetaCustomizationCounter option_1_count;

	public MetaCustomizationCounter option_2_count;

	public MetaCustomizationCounter color_count;

	public MetaCustomizationColorLibrary color_library;

	public MetaCustomization on_new_color = delegate
	{
		World.world.zone_calculator.dirtyAndClear();
	};

	public string title_locale;

	public string option_1_locale;

	public string option_2_locale;

	public string color_locale;

	public string icon_banner;

	public string icon_creature;

	public IEnumerable<string> getLocaleIDs()
	{
		if (editable)
		{
			yield return localization_title;
			yield return title_locale;
			yield return option_1_locale;
			yield return option_2_locale;
			yield return color_locale;
		}
	}
}
