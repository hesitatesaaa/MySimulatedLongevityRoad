using UnityEngine.UI;

public class BuildingDebugAnimationVariation : DebugAnimationVariation
{
	public Image shadow;

	public SpriteAnimation shadow_animation;

	public void update(float pElapsed)
	{
		sprite_animation.update(pElapsed);
		shadow_animation.update(pElapsed);
	}

	public void toggleAnimation(bool pState)
	{
		if (pState)
		{
			sprite_animation.isOn = true;
			shadow_animation.isOn = true;
		}
		else
		{
			sprite_animation.stopAnimations();
			shadow_animation.stopAnimations();
		}
	}

	public void setFrame(int pIndex)
	{
		sprite_animation.currentFrameIndex = pIndex;
		sprite_animation.updateFrame();
		shadow_animation.currentFrameIndex = pIndex;
		shadow_animation.updateFrame();
	}
}
