using UnityEngine;

public class DebugAnimatedVariation
{
	public bool animated;

	public Sprite[] frames;

	public DebugAnimatedVariation(Sprite[] pFrames, bool pAnimated)
	{
		animated = pAnimated;
		frames = pFrames;
	}
}
