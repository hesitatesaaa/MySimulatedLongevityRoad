using System;
using System.Collections.Generic;

public static class TileActionLibrary
{
	public static bool damage(WorldTile pTile, Actor pActor)
	{
		throw new NotImplementedException("damage logic per tile not implemented yet");
	}

	public static bool landmine(WorldTile pTile, Actor pActor)
	{
		if (pActor.isFlying())
		{
			return false;
		}
		if (pActor.isInAir() || pActor.isHovering())
		{
			return false;
		}
		if (pActor.hasTrait("weightless"))
		{
			return false;
		}
		World.world.explosion_layer.explodeBomb(pTile);
		return true;
	}

	public static bool setUnitOnFire(WorldTile pTile, Actor pActor)
	{
		if (pActor.hasTrait("burning_feet"))
		{
			return false;
		}
		pActor.addStatusEffect("burning");
		pTile.startFire(pForce: true);
		return true;
	}

	public static bool giveTumorTrait(WorldTile pTile, Actor pActor)
	{
		if (pActor.asset.immune_to_tumor)
		{
			return false;
		}
		if (!pActor.asset.can_turn_into_tumor)
		{
			return false;
		}
		pActor.addTrait("tumor_infection");
		return true;
	}

	public static bool giveSlownessStatus(WorldTile pTile, Actor pActor)
	{
		if (pActor.asset.immune_to_slowness)
		{
			return false;
		}
		pActor.addStatusEffect("slowness");
		return true;
	}

	public static bool giveMadnessTrait(WorldTile pTile, Actor pActor)
	{
		if (pActor.asset.immune_to_tumor)
		{
			return false;
		}
		if (!pActor.asset.can_turn_into_tumor)
		{
			return false;
		}
		pActor.addTrait("madness");
		return true;
	}

	public static BuildingAsset getGrowTypeRandomMineral(WorldTile pTile)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset == null)
		{
			return null;
		}
		if (biome_asset.pot_minerals_spawn == null)
		{
			return null;
		}
		string random = biome_asset.pot_minerals_spawn.GetRandom();
		return AssetManager.buildings.get(random);
	}

	public static BuildingAsset getGrowTypeRandomTrees(WorldTile pTile)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset == null)
		{
			return null;
		}
		return getRandomAssetFromPot(biome_asset.pot_trees_spawn);
	}

	public static BuildingAsset getGrowTypeRandomPlants(WorldTile pTile)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset == null)
		{
			return null;
		}
		return getRandomAssetFromPot(biome_asset.pot_plants_spawn);
	}

	public static BuildingAsset getGrowTypeRandomBushes(WorldTile pTile)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset == null)
		{
			return null;
		}
		return getRandomAssetFromPot(biome_asset.pot_bushes_spawn);
	}

	private static BuildingAsset getRandomAssetFromPot(List<string> pPotList)
	{
		if (pPotList == null)
		{
			return null;
		}
		string random = pPotList.GetRandom();
		return AssetManager.buildings.get(random);
	}

	public static BuildingAsset getGrowTypeSand(WorldTile pTile)
	{
		bool flag = pTile.zone.hasLiquid();
		if (!flag)
		{
			TileZone[] neighbours_all = pTile.zone.neighbours_all;
			for (int i = 0; i < neighbours_all.Length; i++)
			{
				flag = neighbours_all[i].hasLiquid();
				if (flag)
				{
					break;
				}
			}
		}
		if (flag)
		{
			return AssetManager.buildings.get("palm_tree");
		}
		return AssetManager.buildings.get("cacti_tree");
	}
}
