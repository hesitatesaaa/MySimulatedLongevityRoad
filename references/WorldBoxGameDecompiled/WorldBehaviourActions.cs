using System.Collections.Generic;
using UnityEngine;

public static class WorldBehaviourActions
{
	private static List<WorldTile> _temp_wheat_list = new List<WorldTile>();

	private static List<MapRegion> _spread_fauna_list = new List<MapRegion>();

	private static int _last_used_world_id = 0;

	private static readonly HashSet<IMetaObject> _used_for_grin_reaper = new HashSet<IMetaObject>();

	public static void buildingSparks()
	{
		if (World.world.buildings.sparkles.Count == 0 || !MapBox.isRenderGameplay())
		{
			return;
		}
		for (int i = 0; i < 1; i++)
		{
			Building random = World.world.buildings.sparkles.GetRandom();
			if (random.isUsable() && random.animation_state == BuildingAnimationState.Normal)
			{
				WorldTile random2 = random.tiles.GetRandom();
				if (random2.has_tile_up)
				{
					EffectsLibrary.spawnAtTile("fx_building_sparkle", random2.tile_up, 0.25f);
				}
			}
		}
	}

	public static void updateDisasters()
	{
		DisasterAsset randomAssetFromPool = AssetManager.disasters.getRandomAssetFromPool();
		if (randomAssetFromPool != null && Randy.randomChance(randomAssetFromPool.chance))
		{
			randomAssetFromPool?.action(randomAssetFromPool);
		}
	}

	public static void updateMigrants()
	{
		if (!WorldLawLibrary.world_law_civ_migrants.isEnabled() || World.world.cities.list.Count == 0)
		{
			return;
		}
		int pMax = Randy.randomInt(1, World.world.cities.list.Count);
		foreach (City item in World.world.cities.list.LoopRandom(pMax))
		{
			if (item.isNeutral() || item.getPopulationPeople() > 100 || item.countFoodTotal() < 10)
			{
				continue;
			}
			Building buildingOfType = item.getBuildingOfType("type_bonfire");
			if (buildingOfType == null)
			{
				continue;
			}
			Subspecies mainSubspecies = item.getMainSubspecies();
			if (mainSubspecies != null && !mainSubspecies.hasReachedPopulationLimit())
			{
				Kingdom kingdom = item.kingdom;
				ActorAsset actorAsset = mainSubspecies.getActorAsset();
				int num = Randy.randomInt(1, 4);
				for (int i = 0; i < num; i++)
				{
					Actor actor = World.world.units.spawnNewUnit(actorAsset.id, buildingOfType.current_tile, pSpawnSound: false, pMiracleSpawn: false, 0f, mainSubspecies, pGiveOwnerlessItems: true);
					actor.data.age_overgrowth = Randy.randomInt(actorAsset.age_spawn, actorAsset.age_spawn * 2);
					actor.addTrait("attractive");
					actor.addTrait("soft_skin");
					actor.setMetasFromCity(item);
					kingdom.increaseMigrants();
					actor.setKingdom(kingdom);
					item.increaseMigrants();
					actor.setCity(item);
					actor.addStatusEffect("handsome_migrant");
				}
			}
		}
	}

	public static void updateRoadDecay()
	{
		TileZone random = World.world.zone_calculator.zones.GetRandom();
		if ((random.hasCity() && random.city.hasCulture() && random.city.culture.canUseRoads()) || Randy.randomBool())
		{
			return;
		}
		foreach (WorldTile item in random.tiles.LoopRandom())
		{
			if (!Randy.randomBool() && item.Type.road)
			{
				MapAction.decreaseTile(item, pDamage: false);
			}
		}
	}

	public static void updateFarmDecay()
	{
		if (MapBox.current_world_seed_id != _last_used_world_id)
		{
			_last_used_world_id = MapBox.current_world_seed_id;
			_temp_wheat_list.Clear();
		}
		if (_temp_wheat_list.Count == 0)
		{
			foreach (WorldTile item in TopTileLibrary.field.hashset)
			{
				_temp_wheat_list.Add(item);
			}
			_temp_wheat_list.Shuffle();
		}
		for (int i = 0; i < 10; i++)
		{
			if (_temp_wheat_list.Count == 0)
			{
				break;
			}
			WorldTile worldTile = _temp_wheat_list.Pop();
			if (!worldTile.Type.farm_field)
			{
				continue;
			}
			City city = worldTile.zone.city;
			if (city == null)
			{
				MapAction.decreaseTile(worldTile, pDamage: false);
			}
			else if (!city.calculated_farm_fields.Contains(worldTile) && !city.calculated_place_for_farms.Contains(worldTile))
			{
				if (worldTile.hasBuilding() && worldTile.building.asset.wheat)
				{
					worldTile.building.startDestroyBuilding();
				}
				MapAction.decreaseTile(worldTile, pDamage: false);
			}
		}
	}

	public static void clearFarmDecay()
	{
		_temp_wheat_list.Clear();
	}

	public static void updateRuinRatSpawn()
	{
		if (!WorldLawLibrary.world_law_animals_spawn.isEnabled() || World.world.map_chunk_manager.chunks.Length == 0)
		{
			return;
		}
		ActorAsset actorAsset = AssetManager.actor_library.get("rat");
		if (actorAsset == null || actorAsset.units.Count > actorAsset.max_random_amount * 3)
		{
			return;
		}
		Kingdom kingdom = World.world.kingdoms_wild.get("ruins");
		if (kingdom.buildings.Count == 0)
		{
			return;
		}
		Building building = null;
		foreach (Building item in kingdom.buildings.LoopRandom())
		{
			if (item.isAlive() && item.asset.city_building && item.asset.hasHousingSlots())
			{
				building = item;
				break;
			}
		}
		if (building != null)
		{
			WorldTile current_tile = building.current_tile;
			int num = Randy.randomInt(2, 5);
			for (int i = 0; i < num; i++)
			{
				World.world.units.spawnNewUnit(actorAsset.id, current_tile);
			}
		}
	}

	public static void updateUnitSpawn()
	{
		if (!WorldLawLibrary.world_law_animals_spawn.isEnabled() || World.world.map_chunk_manager.chunks.Length == 0)
		{
			return;
		}
		TileIsland tileIsland = null;
		if (World.world.islands_calculator.islands_ground.Count > 0)
		{
			tileIsland = World.world.islands_calculator.getRandomIslandGround();
		}
		if (tileIsland == null)
		{
			return;
		}
		WorldTile randomTile = tileIsland.getRandomTile();
		BiomeAsset biome_asset = randomTile.Type.biome_asset;
		if (biome_asset == null || !biome_asset.pot_spawn_units_auto)
		{
			return;
		}
		string random = biome_asset.pot_units_spawn.GetRandom();
		ActorAsset actorAsset = AssetManager.actor_library.get(random);
		if (actorAsset == null || actorAsset.units.Count > actorAsset.max_random_amount)
		{
			return;
		}
		int num = 0;
		foreach (Actor item in Finder.getUnitsFromChunk(randomTile, 1))
		{
			_ = item;
			if (num++ > 3)
			{
				return;
			}
		}
		int num2 = Randy.randomInt(2, 5);
		for (int i = 0; i < num2; i++)
		{
			World.world.units.spawnNewUnit(actorAsset.id, randomTile, pSpawnSound: false, pMiracleSpawn: false, 6f, null, pGiveOwnerlessItems: true);
		}
	}

	public static void growMinerals()
	{
		if (!WorldLawLibrary.world_law_grow_minerals.isEnabled())
		{
			return;
		}
		for (int i = 0; i < World.world.islands_calculator.islands_ground.Count; i++)
		{
			TileIsland tileIsland = World.world.islands_calculator.islands_ground[i];
			if (tileIsland.getTileCount() <= 20)
			{
				continue;
			}
			MapRegion random = tileIsland.regions.GetRandom();
			if (mineralsAroundZone(random.getRandomTile().zone))
			{
				continue;
			}
			int num = tileIsland.regions.Count / 20 + 1;
			num = Mathf.Min(num, 3);
			for (int j = 0; j < num; j++)
			{
				foreach (WorldTile item in random.tiles.LoopRandom())
				{
					BiomeAsset biome = item.getBiome();
					if (biome != null && biome.grow_minerals_auto && !item.isOnFire() && !Randy.randomBool())
					{
						BuildingActions.tryGrowMineralRandom(item);
						break;
					}
				}
			}
		}
	}

	private static bool mineralsAroundZone(TileZone pZone)
	{
		if (pZone.hasAnyBuildingsInSet(BuildingList.Minerals))
		{
			return true;
		}
		TileZone[] neighbours_all = pZone.neighbours_all;
		for (int i = 0; i < neighbours_all.Length; i++)
		{
			if (neighbours_all[i].hasAnyBuildingsInSet(BuildingList.Minerals))
			{
				return true;
			}
		}
		return false;
	}

	public static bool tryGrowVegetation(Building pBuilding, bool pCheckLimit = true)
	{
		if (!pBuilding.isUsable())
		{
			return false;
		}
		BuildingAsset asset = pBuilding.asset;
		WorldTile random = pBuilding.tiles.GetRandom();
		int num = 6;
		for (int i = 0; i < num; i++)
		{
			random = random.neighboursAll.GetRandom();
		}
		if (tryToGrowOnTile(random, asset, pCheckLimit))
		{
			return true;
		}
		_spread_fauna_list.Clear();
		_spread_fauna_list.AddRange(pBuilding.current_tile.region.neighbours);
		_spread_fauna_list.Add(pBuilding.current_tile.region);
		return tryToGrowOnTile(_spread_fauna_list.GetRandom().getRandomTile(), asset, pCheckLimit);
	}

	private static bool tryToGrowOnTile(WorldTile pTile, BuildingAsset pAsset, bool pCheckLimit = true)
	{
		if (pCheckLimit && pTile.zone.hasReachedBuildingLimit(pTile, pAsset))
		{
			return false;
		}
		if (!World.world.buildings.canBuildFrom(pTile, pAsset, null))
		{
			return false;
		}
		World.world.buildings.addBuilding(pAsset, pTile);
		World.world.game_stats.data.treesGrown++;
		return true;
	}

	public static void spawnRandomVegetation()
	{
		if (!WorldLawLibrary.world_law_vegetation_random_seeds.isEnabled() || !World.world_era.grow_vegetation)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			MapRegion random = World.world.map_chunk_manager.chunks.GetRandom().regions.GetRandom();
			if (random.island.type == TileLayerType.Ground)
			{
				growVegetationAt(random, 5);
			}
		}
	}

	public static void growVegetationAt(MapRegion pRegion, int pAmount)
	{
		for (int i = 0; i < pAmount; i++)
		{
			WorldTile random = pRegion.tiles.GetRandom();
			if (!random.isOnFire())
			{
				BiomeAsset biome_asset = random.Type.biome_asset;
				if (biome_asset != null && biome_asset.grow_vegetation_auto && !Randy.randomBool())
				{
					ActionLibrary.growRandomVegetation(random, biome_asset);
				}
			}
		}
	}

	public static bool addForGrinReaper(NanoObject pObject, BaseAugmentationAsset pAugmentationAsset)
	{
		if (pObject.isRekt())
		{
			return false;
		}
		return _used_for_grin_reaper.Add((IMetaObject)pObject);
	}

	public static bool removeUsedForGrinReaper(NanoObject pObject, BaseAugmentationAsset pAugmentationAsset)
	{
		if (pObject == null)
		{
			return false;
		}
		return _used_for_grin_reaper.Remove((IMetaObject)pObject);
	}

	public static void clearGrinReaper()
	{
		_used_for_grin_reaper.Clear();
	}

	public static void updateGrinReaper()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (_used_for_grin_reaper.Count != 0 && !World.world.isWindowOnScreen())
		{
			Actor randomActorForReaper = getRandomActorForReaper();
			if (!randomActorForReaper.isRekt() && !randomActorForReaper.isInMagnet())
			{
				randomActorForReaper.getHitFullHealth(AttackType.Smile);
				EffectsLibrary.spawnAt("fx_grin_reaper", randomActorForReaper.current_position, randomActorForReaper.actor_scale);
			}
		}
	}

	private static Actor getRandomActorForReaper()
	{
		_used_for_grin_reaper.RemoveWhere((IMetaObject pMeta) => pMeta == null || !pMeta.isAlive());
		IMetaObject random = _used_for_grin_reaper.GetRandom();
		if (random == null)
		{
			return null;
		}
		if (!random.isAlive())
		{
			return null;
		}
		return random.getRandomActorForReaper();
	}
}
