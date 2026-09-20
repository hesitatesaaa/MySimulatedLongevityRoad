using System;
using System.ComponentModel;

[Serializable]
public class MetaRepresentationAsset : Asset, ILocalizedAsset
{
	public MetaType meta_type;

	public string title_locale;

	public IconPathGetter icon_getter;

	public CheckActorHasMeta check_has_meta;

	public GetMetaFromActor meta_getter;

	public GetMetaTotalFromActor meta_getter_total;

	public GetWorldUnits world_units_getter;

	public string general_icon_path;

	public bool show_none_percent;

	[DefaultValue(true)]
	public bool show_none_percent_for_total = true;

	[DefaultValue(true)]
	public bool show_species_icon = true;

	public string getLocaleID()
	{
		return title_locale;
	}
}
