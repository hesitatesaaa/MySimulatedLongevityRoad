using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class TileEffectAsset : Asset
{
	[DefaultValue(1)]
	public int rate = 1;

	[DefaultValue(1f)]
	public float chance = 1f;

	public string path_sprite;

	[DefaultValue(0.1f)]
	public float time_between_frames = 0.1f;

	private Sprite[] _cached_sprites;

	public HashSet<string> tile_types;

	public void addTileType(string pType)
	{
		if (tile_types == null)
		{
			tile_types = new HashSet<string>();
		}
		tile_types.Add(pType);
	}

	public void addTileTypes(params string[] pTypes)
	{
		if (tile_types == null)
		{
			tile_types = new HashSet<string>(pTypes);
		}
		else
		{
			tile_types.UnionWith(pTypes);
		}
	}

	public Sprite[] getSprites()
	{
		if (_cached_sprites == null)
		{
			_cached_sprites = SpriteTextureLoader.getSpriteList(path_sprite);
		}
		return _cached_sprites;
	}
}
