using System.Collections.Generic;

namespace ai.behaviours;

public class BehWormDigEat : BehaviourActionActor
{
	private static List<BrushPixelData> myRange = new List<BrushPixelData>();

	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("worm_size", out var pResult, 1);
		if (pActor.current_tile.Height < 220)
		{
			loopWithBrush(pActor.current_tile, pResult, tileDrawWorm);
		}
		checkForWorms(pActor.current_tile, pResult, pActor);
		return BehResult.Continue;
	}

	public static void checkForWorms(WorldTile pCenterTile, int pBrushSize, Actor pActor)
	{
		int num = 0;
		int num2 = 0;
		BrushData brushData = Brush.get(pBrushSize, "hcirc_");
		for (int i = 0; i < brushData.pos.Length; i++)
		{
			num = pCenterTile.x + brushData.pos[i].x;
			num2 = pCenterTile.y + brushData.pos[i].y;
			if (num >= 0 && num < MapBox.width && num2 >= 0 && num2 < MapBox.height)
			{
				WorldTile tileSimple = BehaviourActionBase<Actor>.world.GetTileSimple(num, num2);
				checkWorms(tileSimple, pActor);
				BehaviourActionBase<Actor>.world.flash_effects.flashPixel(tileSimple, 10, ColorType.Purple);
			}
		}
	}

	public void loopWithBrush(WorldTile pCenterTile, int pBrushSize, PowerActionWithID pAction, string pPowerID = null)
	{
		int num = 0;
		int num2 = 0;
		BrushData brushData = Brush.get(pBrushSize, "hcirc_");
		for (int i = 0; i < brushData.pos.Length; i++)
		{
			num = pCenterTile.x + brushData.pos[i].x;
			num2 = pCenterTile.y + brushData.pos[i].y;
			if (num >= 0 && num < MapBox.width && num2 >= 0 && num2 < MapBox.height)
			{
				WorldTile tileSimple = BehaviourActionBase<Actor>.world.GetTileSimple(num, num2);
				pAction(tileSimple, pPowerID);
			}
		}
	}

	public static void checkWorms(WorldTile pTile, Actor pActor)
	{
		pTile.doUnits(delegate(Actor tActor)
		{
			if (pActor.data.id != tActor.data.id && tActor.asset.id == "worm")
			{
				pActor.data.get("worm_size", out var pResult, 1);
				tActor.data.get("worm_size", out var pResult2, 1);
				tActor.dieSimpleNone();
				pResult += pResult2;
				pActor.data.set("worm_size", pResult);
			}
		});
	}

	public static bool tileDrawWorm(WorldTile pTile, string pPowerID)
	{
		if (pTile == null)
		{
			return false;
		}
		BehWormDig.wormTile(pTile);
		if (pTile.Type.ocean && pTile.Type.liquid && Randy.randomChance(0.25f))
		{
			spawnBurst(pTile, "rain", pCreateGround: false);
		}
		if (pTile.Type.lava)
		{
			LavaHelper.removeLava(pTile);
			if (Randy.randomChance(0.25f))
			{
				spawnBurst(pTile, "lava");
			}
		}
		if (pTile.isOnFire())
		{
			pTile.stopFire();
		}
		if (Randy.randomChance(0.25f))
		{
			if (pTile.Type.IsType("sand"))
			{
				spawnBurst(pTile, "pixel", pCreateGround: false);
			}
			else if (pTile.Type.can_be_farm)
			{
				spawnBurst(pTile, "pixel", pCreateGround: false);
			}
		}
		return true;
	}

	private static void spawnBurst(WorldTile pTile, string pType, bool pCreateGround = true)
	{
		if (BehaviourActionBase<Actor>.world.drop_manager.getActiveIndex() <= 300)
		{
			BehaviourActionBase<Actor>.world.drop_manager.spawnParabolicDrop(pTile, pType, 0f, 0.62f, 104f, 0.7f, 23.5f);
		}
	}
}
