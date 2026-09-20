namespace ai.behaviours;

public class BehDragonLandAttack : BehDragon
{
	public override BehResult execute(Actor pActor)
	{
		SpriteAnimation spriteAnimation = dragon.spriteAnimation;
		if (spriteAnimation.currentFrameIndex == 4)
		{
			pActor.data.set("shouldAttack", pData: true);
		}
		if (spriteAnimation.currentFrameIndex == 5)
		{
			pActor.data.get("shouldAttack", out var pResult, false);
			if (pResult)
			{
				pActor.data.removeBool("shouldAttack");
				foreach (WorldTile item in dragon.landAttackTiles(pActor.current_tile))
				{
					if (item != null && (item.hasUnits() || !Randy.randomBool()))
					{
						dragon.attackTile(item);
					}
				}
				pActor.data.get("landAttacks", out var pResult2, 0);
				pActor.data.set("landAttacks", ++pResult2);
			}
		}
		if (spriteAnimation.currentFrameIndex < spriteAnimation.frames.Length - 1)
		{
			return BehResult.RepeatStep;
		}
		return BehResult.Continue;
	}
}
