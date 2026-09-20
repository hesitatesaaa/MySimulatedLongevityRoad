using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class WindowAsset : Asset
{
	public bool preload;

	public string related_parent_window;

	[NonSerialized]
	public MetaTypeAsset meta_type_asset;

	[DefaultValue(true)]
	public bool window_toolbar_enabled = true;

	public string icon_path = "iconAye";

	public HoveringBGIconsGetter get_hovering_icons = getDefaultHoveringIconPath;

	[DefaultValue(true)]
	public bool is_testable = true;

	[NonSerialized]
	private Sprite _cached_icon;

	public Sprite getSprite()
	{
		if (_cached_icon == null)
		{
			_cached_icon = SpriteTextureLoader.getSprite("ui/Icons/" + icon_path);
		}
		return _cached_icon;
	}

	private static IEnumerable<string> getDefaultHoveringIconPath(WindowAsset pAsset)
	{
		yield return pAsset.icon_path;
	}
}
