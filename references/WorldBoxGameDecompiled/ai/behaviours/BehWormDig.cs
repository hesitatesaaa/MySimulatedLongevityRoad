using System;

namespace ai.behaviours;

public class BehWormDig : BehaviourActionActor
{
	protected override void setupErrorChecks()
	{
		base.setupErrorChecks();
		null_check_tile_target = true;
	}

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("size", out var pResult, 0);
		if (pResult > 0 && pActor.beh_tile_target.Height < 220)
		{
			BehaviourActionBase<Actor>.world.loopWithBrush(pActor.beh_tile_target, Brush.get(pResult), tileDrawWorm);
		}
		else
		{
			BehaviourActionBase<Actor>.world.loopWithBrush(pActor.beh_tile_target, Brush.get(pResult), tileFlashWorm);
		}
		return BehResult.RestartTask;
	}

	public static bool tileFlashWorm(WorldTile pTile, string pPowerID)
	{
		BehaviourActionBase<Actor>.world.flash_effects.flashPixel(pTile, 20);
		return true;
	}

	public static bool tileDrawWorm(WorldTile pTile, string pPowerID)
	{
		wormTile(pTile);
		return true;
	}

	public static void wormTile(WorldTile pTile)
	{
		BehaviourActionBase<Actor>.world.flash_effects.flashPixel(pTile, 20);
		if (pTile.top_type != null)
		{
			MapAction.decreaseTile(pTile, pDamage: false);
		}
		else if (pTile.Type.increase_to != null && !pTile.Type.road)
		{
			bool num = pTile.Type.increase_to.id.StartsWith("mountain", StringComparison.Ordinal);
			bool flag = pTile.Type.increase_to.id.StartsWith("hill", StringComparison.Ordinal);
			if (!num && !flag && (pTile.Type.decrease_to == null || Randy.randomBool()))
			{
				MapAction.increaseTile(pTile, pDamage: false, "destroy");
			}
			else if (pTile.Type.decrease_to != null)
			{
				MapAction.decreaseTile(pTile, pDamage: false, "destroy");
			}
		}
	}
}
