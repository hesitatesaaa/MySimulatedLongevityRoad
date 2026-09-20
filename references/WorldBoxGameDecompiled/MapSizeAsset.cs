using System;
using UnityEngine;

[Serializable]
public class MapSizeAsset : Asset, ILocalizedAsset
{
	public Sprite cached_sprite;

	public string path_icon;

	public bool show_warning;

	public int size = 1;

	public string getLocaleID()
	{
		return "map_size_" + id;
	}

	public Sprite getIconSprite()
	{
		if ((Object)(object)cached_sprite == (Object)null)
		{
			cached_sprite = SpriteTextureLoader.getSprite("ui/Icons/" + path_icon);
		}
		return cached_sprite;
	}
}
