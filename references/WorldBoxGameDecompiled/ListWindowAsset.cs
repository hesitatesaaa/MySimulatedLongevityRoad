using System;
using System.Collections.Generic;

[Serializable]
public class ListWindowAsset : Asset, IMultiLocalesAsset
{
	public string no_items_locale;

	public string no_dead_items_locale;

	public string art_path;

	public string icon_path;

	public MetaType meta_type;

	public ListComponentSetter set_list_component;

	public IEnumerable<string> getLocaleIDs()
	{
		yield return no_items_locale;
		if (!string.IsNullOrEmpty(no_dead_items_locale))
		{
			yield return no_dead_items_locale;
		}
	}
}
