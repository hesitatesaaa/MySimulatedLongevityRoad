namespace ai.behaviours;

public class BehDragonFinishAnimation : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.flipAnimationActive())
		{
			return BehResult.RepeatStep;
		}
		SpriteAnimation spriteAnimation = dragon.spriteAnimation;
		if (spriteAnimation.currentFrameIndex < spriteAnimation.frames.Length - 1)
		{
			return BehResult.RepeatStep;
		}
		return BehResult.Continue;
	}
}
