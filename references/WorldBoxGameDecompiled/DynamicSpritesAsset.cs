using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DynamicSpritesAsset : Asset
{
	public bool big_atlas = true;

	public bool check_wobbly_setting;

	public UnitTextureAtlasID atlas_id;

	public string export_folder_path;

	public bool buildings;

	private Dictionary<long, Sprite> _dictionary_sprites = new Dictionary<long, Sprite>();

	private UnitSpriteConstructorAtlas _atlas;

	public override void create()
	{
		base.create();
		if (_atlas == null)
		{
			_atlas = new UnitSpriteConstructorAtlas(atlas_id, big_atlas);
		}
	}

	public void resetAtlas()
	{
		clear();
		_atlas.setBigAtlas(big_atlas);
	}

	public bool hasSprite(long pID)
	{
		return _dictionary_sprites.ContainsKey(pID);
	}

	public void addSprite(long pHashCode, Sprite pSprite)
	{
		_dictionary_sprites[pHashCode] = pSprite;
	}

	public Sprite getSprite(long pID)
	{
		_dictionary_sprites.TryGetValue(pID, out var value);
		return value;
	}

	public UnitSpriteConstructorAtlas getAtlas()
	{
		return _atlas;
	}

	public void checkAtlasDirty()
	{
		_atlas.checkDirty();
	}

	public void clear()
	{
		_dictionary_sprites.Clear();
		_atlas.clear();
		checkAtlasDirty();
	}

	public int countSprites()
	{
		return _dictionary_sprites.Count;
	}

	public int countTextures()
	{
		return _atlas.textures.Count;
	}
}
