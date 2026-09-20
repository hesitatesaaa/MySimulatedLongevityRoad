using System.Collections.Generic;

namespace ai.behaviours;

public class BehGetResourcesFromMine : BehActorUsableBuildingTarget
{
	private const string ASSET_FOR_NO_BIOME = "mineral_stone";

	private static List<string> pool_mineral_assets_default = new List<string>();

	public override void create()
	{
		base.create();
		if (pool_mineral_assets_default.Count == 0)
		{
			initPool();
		}
	}

	private static void initPool()
	{
		addToPool("mineral_gems", 1, pool_mineral_assets_default);
		addToPool("mineral_gems", 4, pool_mineral_assets_default);
		addToPool("mineral_stone", 20, pool_mineral_assets_default);
		addToPool("mineral_metals", 10, pool_mineral_assets_default);
	}

	private static void addToPool(string pID, int pAmount, List<string> pPool)
	{
		for (int i = 0; i < pAmount; i++)
		{
			pPool.Add(pID);
		}
	}

	public override BehResult execute(Actor pActor)
	{
		if (Randy.randomChance(0.4f))
		{
			return BehResult.Continue;
		}
		BuildingAsset randomAssetFromPool = getRandomAssetFromPool(pActor);
		if (randomAssetFromPool == null)
		{
			return BehResult.Continue;
		}
		BuildingHelper.tryToBuildNear(pActor.beh_building_target.current_tile, randomAssetFromPool);
		pActor.addLoot(SimGlobals.m.coins_for_mine);
		return BehResult.Continue;
	}

	private static BuildingAsset getRandomAssetFromPool(Actor pActor)
	{
		Building beh_building_target = pActor.beh_building_target;
		WorldTile worldTile = beh_building_target.current_tile;
		if (!worldTile.Type.is_biome)
		{
			bool flag = false;
			foreach (WorldTile tile in beh_building_target.tiles)
			{
				if (tile.Type.is_biome)
				{
					flag = true;
					worldTile = tile;
					break;
				}
			}
			if (!flag)
			{
				for (int i = 0; i < worldTile.neighboursAll.Length; i++)
				{
					WorldTile worldTile2 = worldTile.neighboursAll[i];
					if (worldTile2.Type.is_biome)
					{
						flag = true;
						worldTile = worldTile2;
						break;
					}
				}
			}
			if (!flag)
			{
				return AssetManager.buildings.get("mineral_stone");
			}
		}
		List<string> list = worldTile.Type.biome_asset?.pot_minerals_spawn;
		if (list == null)
		{
			list = pool_mineral_assets_default;
		}
		string random = list.GetRandom();
		return AssetManager.buildings.get(random);
	}
}
