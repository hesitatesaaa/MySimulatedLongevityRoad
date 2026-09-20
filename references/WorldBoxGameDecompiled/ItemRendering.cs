using UnityEngine;

public static class ItemRendering
{
	public static Sprite getItemMainSpriteFrame(IHandRenderer pHandRendererAsset)
	{
		if (pHandRendererAsset == null)
		{
			return null;
		}
		Sprite[] sprites = pHandRendererAsset.getSprites();
		if (sprites.Length > 1)
		{
			return AnimationHelper.getSpriteFromList(0, sprites, 5f);
		}
		return sprites[0];
	}
}
