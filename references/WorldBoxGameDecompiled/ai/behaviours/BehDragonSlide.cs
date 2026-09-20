namespace ai.behaviours;

public class BehDragonSlide : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		SpriteAnimation spriteAnimation = dragon.spriteAnimation;
		if (spriteAnimation.currentFrameIndex == 7)
		{
			foreach (WorldTile item in dragon.attackRange(pActor.flip))
			{
				if (item != null && (item.hasUnits() || !Randy.randomBool()))
				{
					dragon.attackTile(item);
				}
			}
		}
		if (spriteAnimation.currentFrameIndex < spriteAnimation.frames.Length - 1)
		{
			return BehResult.RepeatStep;
		}
		return BehResult.Continue;
	}
}
