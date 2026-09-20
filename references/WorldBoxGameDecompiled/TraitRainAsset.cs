using System;
using UnityEngine;

[Serializable]
public class TraitRainAsset : Asset
{
	public RainListGetter get_list;

	public RainStateGetter get_state;

	public RainStateSetter set_state;

	public string path_art;

	private Sprite _sprite_art;

	public string path_art_void;

	private Sprite _sprite_art_void;

	public Sprite getSpriteArt()
	{
		if ((Object)(object)_sprite_art == (Object)null)
		{
			_sprite_art = SpriteTextureLoader.getSprite(path_art);
		}
		return _sprite_art;
	}

	public Sprite getSpriteArtVoid()
	{
		if ((Object)(object)_sprite_art_void == (Object)null)
		{
			_sprite_art_void = SpriteTextureLoader.getSprite(path_art_void);
		}
		return _sprite_art_void;
	}
}
