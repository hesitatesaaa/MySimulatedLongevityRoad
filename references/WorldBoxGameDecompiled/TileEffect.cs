using UnityEngine;

public class TileEffect : BaseEffect
{
	public void load(TileEffectAsset pAsset)
	{
		Sprite[] sprites = pAsset.getSprites();
		sprite_animation.setFrames(sprites);
		sprite_animation.resetAnim();
		sprite_animation.timeBetweenFrames = pAsset.time_between_frames;
	}
}
