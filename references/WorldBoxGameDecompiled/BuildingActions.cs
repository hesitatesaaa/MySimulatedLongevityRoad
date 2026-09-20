public static class BuildingActions
{
	public static void tryGrowVegetationRandom(WorldTile pTile, VegetationType pType, bool pOnStart = false, bool pCheckLimit = true, bool pCheckRandom = true)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset == null || !biome_asset.grow_vegetation_auto)
		{
			return;
		}
		BuildingAsset buildingAsset = null;
		switch (pType)
		{
		case VegetationType.Plants:
			if (biome_asset.grow_type_selector_plants != null)
			{
				buildingAsset = biome_asset.grow_type_selector_plants(pTile);
			}
			break;
		case VegetationType.Trees:
			if (biome_asset.grow_type_selector_trees != null)
			{
				buildingAsset = biome_asset.grow_type_selector_trees(pTile);
			}
			break;
		case VegetationType.Bushes:
			if (biome_asset.grow_type_selector_bushes != null)
			{
				buildingAsset = biome_asset.grow_type_selector_bushes(pTile);
			}
			break;
		}
		if (buildingAsset == null)
		{
			return;
		}
		if (buildingAsset.limit_in_radius > 0)
		{
			pCheckLimit = true;
		}
		if ((!pCheckLimit || !pTile.zone.hasReachedBuildingLimit(pTile, buildingAsset)) && (!pCheckRandom || !(buildingAsset.vegetation_random_chance < Randy.random())) && World.world.buildings.canBuildFrom(pTile, buildingAsset, null))
		{
			World.world.buildings.addBuilding(buildingAsset, pTile);
			if (buildingAsset.flora_type == FloraType.Tree)
			{
				World.world.game_stats.data.treesGrown++;
			}
			else if (buildingAsset.flora_type == FloraType.Plant || buildingAsset.flora_type == FloraType.Fungi)
			{
				World.world.game_stats.data.floraGrown++;
			}
			if (buildingAsset.has_sound_spawn)
			{
				MusicBox.playSound(buildingAsset.sound_spawn, pTile, pGameViewOnly: true, pVisibleOnly: true);
			}
		}
	}

	public static void tryGrowMineralRandom(WorldTile pTile, bool pOnStart = false, bool pCheckLimit = true)
	{
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.grow_minerals_auto && (!pTile.hasBuilding() || !pTile.building.isUsable()))
		{
			BuildingAsset buildingAsset = biome.grow_type_selector_minerals(pTile);
			if (buildingAsset != null && (!pCheckLimit || !pTile.zone.hasReachedBuildingLimit(pTile, buildingAsset)) && World.world.buildings.canBuildFrom(pTile, buildingAsset, null))
			{
				World.world.buildings.addBuilding(buildingAsset, pTile);
			}
		}
	}

	public static Building tryGrowVegetation(WorldTile pTile, string pTemplateID, bool pSfx = false, bool pCheckLimit = true)
	{
		BuildingAsset buildingAsset = AssetManager.buildings.get(pTemplateID);
		if (pTile.hasBuilding() && pTile.building.isUsable())
		{
			return null;
		}
		if (buildingAsset == null)
		{
			return null;
		}
		if (pCheckLimit && pTile.zone.hasReachedBuildingLimit(pTile, buildingAsset))
		{
			return null;
		}
		if (!World.world.buildings.canBuildFrom(pTile, buildingAsset, null))
		{
			return null;
		}
		Building result = World.world.buildings.addBuilding(buildingAsset, pTile, pCheckForBuild: false, pSfx);
		World.world.game_stats.data.floraGrown++;
		return result;
	}

	public static void spawnBeehives(int pAmount)
	{
		for (int i = 0; i < pAmount; i++)
		{
			WorldTile random = World.world.tiles_list.GetRandom();
			if (random.Type.grass)
			{
				World.world.buildings.addBuilding("beehive", random, pCheckForBuild: true);
			}
		}
	}

	public static void spawnResource(int pAmount, string pType, bool pRandomSize = true)
	{
		for (int i = 0; i < pAmount; i++)
		{
			WorldTile random = World.world.tiles_list.GetRandom();
			if (random.Type.ground)
			{
				World.world.buildings.addBuilding(pType, random, pCheckForBuild: true);
			}
		}
	}
}
