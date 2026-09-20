using System.Collections.Generic;
using UnityEngine;

public static class WallHelper
{
	private static Dictionary<int, WallFrameContainer> _dictionary = new Dictionary<int, WallFrameContainer>();

	public static Sprite getSprite(WorldTile pTile, TopTileType pTileAsset)
	{
		if (!_dictionary.TryGetValue(pTileAsset.index_id, out var value))
		{
			value = new WallFrameContainer();
			Sprite[] spriteList = SpriteTextureLoader.getSpriteList("walls/" + pTileAsset.id + "/wall_sheet");
			value.sprites = spriteList;
			_dictionary.Add(pTileAsset.index_id, value);
		}
		int num = ((!pTile.Type.animated_wall) ? (pTile.random_animation_seed % value.sprites.Length) : ((int)(AnimationHelper.getAnimationGlobalTime(4f) + (float)pTile.random_animation_seed) % value.sprites.Length));
		return value.sprites[num];
	}
}
