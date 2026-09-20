using Beebyte.Obfuscator;

[ObfuscateLiterals]
public class DisasterLibrary : AssetLibrary<DisasterAsset>
{
	public override void init()
	{
		base.init();
		add(new DisasterAsset
		{
			id = "tornado",
			rate = 3,
			chance = 0.5f,
			min_world_cities = 3,
			world_log = "disaster_tornado",
			min_world_population = 100,
			type = DisasterType.Nature
		});
		t.ages_allow.Add("age_tears");
		t.ages_allow.Add("age_ash");
		t.ages_allow.Add("age_chaos");
		t.ages_allow.Add("age_wonders");
		t.ages_allow.Add("age_moon");
		t.action = spawnTornado;
		add(new DisasterAsset
		{
			id = "heatwave",
			rate = 4,
			chance = 0.5f,
			world_log = "disaster_heatwave",
			premium_only = false,
			type = DisasterType.Nature
		});
		t.ages_allow.Add("age_sun");
		t.action = spawnHeatwave;
		add(new DisasterAsset
		{
			id = "small_meteorite",
			rate = 5,
			chance = 0.5f,
			world_log = "disaster_meteorite",
			min_world_population = 400,
			min_world_cities = 3,
			premium_only = true,
			type = DisasterType.Nature
		});
		t.ages_forbid.Add("age_hope");
		t.ages_forbid.Add("age_sun");
		t.action = spawnMeteorite;
		add(new DisasterAsset
		{
			id = "small_earthquake",
			rate = 3,
			chance = 0.4f,
			world_log = "disaster_earthquake",
			min_world_population = 400,
			min_world_cities = 5,
			type = DisasterType.Nature
		});
		t.ages_forbid.Add("age_hope");
		t.ages_forbid.Add("age_sun");
		t.action = spawnSmallEarthquake;
		add(new DisasterAsset
		{
			id = "hellspawn",
			rate = 2,
			chance = 0.9f,
			min_world_cities = 5,
			world_log = "disaster_hellspawn",
			min_world_population = 300,
			premium_only = true,
			spawn_asset_unit = "demon",
			max_existing_units = 5,
			units_min = 2,
			units_max = 5
		});
		t.ages_allow.Add("age_chaos");
		t.action = simpleUnitAssetSpawnUsingIslands;
		add(new DisasterAsset
		{
			id = "ice_ones_awoken",
			rate = 2,
			chance = 0.9f,
			min_world_cities = 5,
			world_log = "disaster_ice_ones",
			min_world_population = 300,
			premium_only = true,
			spawn_asset_unit = "cold_one",
			max_existing_units = 5,
			units_min = 10,
			units_max = 20
		});
		t.ages_allow.Add("age_despair");
		t.ages_allow.Add("age_ice");
		t.action = simpleUnitAssetSpawnUsingIslands;
		add(new DisasterAsset
		{
			id = "sudden_snowman",
			rate = 3,
			chance = 0.9f,
			min_world_cities = 5,
			world_log = "disaster_sudden_snowman",
			min_world_population = 100,
			spawn_asset_unit = "snowman",
			max_existing_units = 5,
			units_min = 20,
			units_max = 40
		});
		t.ages_allow.Add("age_ice");
		t.action = simpleUnitAssetSpawnUsingIslands;
		add(new DisasterAsset
		{
			id = "garden_surprise",
			rate = 1,
			chance = 0.9f,
			min_world_cities = 5,
			world_log = "disaster_garden_surprise",
			min_world_population = 800,
			spawn_asset_building = "super_pumpkin",
			spawn_asset_unit = "lil_pumpkin",
			max_existing_units = 5,
			units_min = 50,
			units_max = 100
		});
		t.ages_allow.Add("age_sun");
		t.ages_allow.Add("age_wonders");
		t.action = gardenSurprise;
		add(new DisasterAsset
		{
			id = "dragon_from_farlands",
			rate = 1,
			chance = 0.9f,
			min_world_cities = 10,
			world_log = "disaster_dragon_from_farlands",
			min_world_population = 3000,
			spawn_asset_unit = "dragon",
			max_existing_units = 1,
			units_min = 1,
			units_max = 1
		});
		t.ages_allow.Add("age_chaos");
		t.ages_allow.Add("age_dark");
		t.ages_allow.Add("age_despair");
		t.action = spawnDragon;
		add(new DisasterAsset
		{
			id = "ash_bandits",
			rate = 1,
			chance = 0.9f,
			min_world_cities = 10,
			world_log = "disaster_bandits",
			min_world_population = 700,
			spawn_asset_unit = "bandit",
			max_existing_units = 10,
			units_min = 15,
			units_max = 30
		});
		t.ages_allow.Add("age_ash");
		t.action = simpleUnitAssetSpawnUsingIslands;
		add(new DisasterAsset
		{
			id = "alien_invasion",
			rate = 1,
			chance = 0.9f,
			min_world_cities = 10,
			world_log = "disaster_alien_invasion",
			min_world_population = 1500,
			spawn_asset_unit = "UFO",
			max_existing_units = 1,
			units_min = 5,
			units_max = 10
		});
		t.ages_allow.Add("age_moon");
		t.action = startAlienInvasion;
		add(new DisasterAsset
		{
			id = "biomass",
			rate = 1,
			chance = 0.9f,
			min_world_cities = 10,
			world_log = "disaster_biomass",
			min_world_population = 700,
			spawn_asset_building = "biomass",
			spawn_asset_unit = "bioblob",
			max_existing_units = 10,
			units_min = 20,
			units_max = 30
		});
		t.ages_allow.Add("age_ash");
		t.action = spawnBiomass;
		add(new DisasterAsset
		{
			id = "tumor",
			rate = 1,
			chance = 0.9f,
			min_world_cities = 10,
			world_log = "disaster_tumor",
			min_world_population = 700,
			spawn_asset_building = "tumor",
			spawn_asset_unit = "tumor_monster_unit",
			max_existing_units = 10,
			units_min = 20,
			units_max = 30
		});
		t.ages_allow.Add("age_moon");
		t.action = spawnTumor;
		add(new DisasterAsset
		{
			id = "wild_mage",
			rate = 1,
			chance = 0.8f,
			world_log = "disaster_evil_mage",
			min_world_population = 400,
			min_world_cities = 5,
			premium_only = true,
			max_existing_units = 1,
			spawn_asset_unit = "evil_mage",
			units_min = 1,
			units_max = 1
		});
		t.ages_forbid.Add("age_hope");
		t.action = spawnEvilMage;
		add(new DisasterAsset
		{
			id = "underground_necromancer",
			rate = 2,
			chance = 0.9f,
			world_log = "disaster_underground_necromancer",
			min_world_population = 200,
			min_world_cities = 4,
			premium_only = true,
			spawn_asset_unit = "necromancer",
			max_existing_units = 1,
			units_min = 1,
			units_max = 1
		});
		t.ages_allow.Add("age_dark");
		t.ages_allow.Add("age_despair");
		t.action = spawnNecromancer;
		add(new DisasterAsset
		{
			id = "mad_thoughts",
			rate = 2,
			chance = 0.7f,
			world_log = "disaster_mad_thoughts",
			min_world_cities = 5,
			min_world_population = 150
		});
		t.ages_forbid.Add("age_hope");
		t.ages_forbid.Add("age_wonders");
		t.action = spawnMadThought;
		add(new DisasterAsset
		{
			id = "greg_abominations",
			rate = 1,
			chance = 0.5f,
			world_log = "disaster_greg_abominations",
			min_world_population = 1000,
			min_world_cities = 3,
			spawn_asset_unit = "greg",
			max_existing_units = 1,
			units_min = 30,
			units_max = 55
		});
		t.ages_allow.Add("age_despair");
		t.action = spawnGreg;
	}

	public DisasterAsset getRandomAssetFromPool()
	{
		using ListPool<DisasterAsset> listPool = new ListPool<DisasterAsset>();
		for (int i = 0; i < list.Count; i++)
		{
			DisasterAsset disasterAsset = list[i];
			if ((disasterAsset.ages_allow.Count <= 0 || disasterAsset.ages_allow.Contains(World.world_era.id)) && (disasterAsset.ages_forbid.Count <= 0 || !disasterAsset.ages_forbid.Contains(World.world_era.id)))
			{
				for (int j = 0; j < disasterAsset.rate; j++)
				{
					listPool.Add(disasterAsset);
				}
			}
		}
		if (listPool.Count == 0)
		{
			return null;
		}
		DisasterAsset random = listPool.GetRandom();
		if (random.type == DisasterType.Nature)
		{
			if (!WorldLawLibrary.world_law_disasters_nature.isEnabled())
			{
				return null;
			}
		}
		else if (random.type == DisasterType.Other && !WorldLawLibrary.world_law_disasters_other.isEnabled())
		{
			return null;
		}
		if (random.min_world_cities > World.world.cities.Count)
		{
			return null;
		}
		if (random.min_world_population > World.world.units.Count)
		{
			return null;
		}
		return random;
	}

	public void spawnMadThought(DisasterAsset pAsset)
	{
		City random = World.world.cities.getRandom();
		if (random == null || random.getPopulationPeople() < 50 || random.getTile() == null)
		{
			return;
		}
		using ListPool<Actor> listPool = new ListPool<Actor>(random.countUnits());
		foreach (Actor item in random.units.LoopRandom())
		{
			if (item.city == random && Randy.randomChance(0.2f))
			{
				listPool.Add(item);
			}
		}
		for (int i = 0; i < listPool.Count; i++)
		{
			listPool[i].addTrait("madness");
		}
		WorldLog.logDisaster(pAsset, random.getTile(), null, random);
	}

	public void spawnGreg(DisasterAsset pAsset)
	{
		if (!DebugConfig.isOn(DebugOption.Greg) || !checkUnitSpawnLimits(pAsset))
		{
			return;
		}
		City random = World.world.cities.getRandom();
		if (random != null)
		{
			Building buildingOfType = random.getBuildingOfType("type_mine");
			if (buildingOfType != null)
			{
				WorldTile current_tile = buildingOfType.current_tile;
				Actor actor = spawnDisasterUnit(pAsset, current_tile);
				WorldLog.logDisaster(pAsset, current_tile, actor.getName(), random, actor);
				spawnDisasterUnit(pAsset, current_tile.region.tiles.GetRandom());
				AchievementLibrary.greg.check();
			}
		}
	}

	public void spawnNecromancer(DisasterAsset pAsset)
	{
		if (!checkUnitSpawnLimits(pAsset))
		{
			return;
		}
		City random = World.world.cities.getRandom();
		if (random == null)
		{
			return;
		}
		Building buildingOfType = random.getBuildingOfType("type_mine");
		if (buildingOfType != null)
		{
			WorldTile current_tile = buildingOfType.current_tile;
			Actor actor = spawnDisasterUnit(pAsset, current_tile);
			WorldLog.logDisaster(pAsset, current_tile, actor.getName(), random, actor);
			int num = Randy.randomInt(5, 25);
			for (int i = 0; i < num; i++)
			{
				World.world.units.createNewUnit("skeleton", current_tile.region.tiles.GetRandom(), pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: true, pAdultAge: true);
			}
		}
	}

	public void spawnEvilMage(DisasterAsset pAsset)
	{
		if (checkUnitSpawnLimits(pAsset))
		{
			TileIsland randomIslandGround = World.world.islands_calculator.getRandomIslandGround();
			if (randomIslandGround != null)
			{
				WorldTile randomTile = randomIslandGround.getRandomTile();
				Actor actor = spawnDisasterUnit(pAsset, randomTile);
				WorldLog.logDisaster(pAsset, randomTile, actor.getName(), null, actor);
			}
		}
	}

	public void spawnHeatwave(DisasterAsset pAsset)
	{
		if (!WorldLawLibrary.world_law_disasters_nature.isEnabled())
		{
			return;
		}
		int num = Randy.randomInt(1, 3);
		WorldTile worldTile = null;
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			TileIsland randomIslandGround = World.world.islands_calculator.getRandomIslandGround();
			if (randomIslandGround != null)
			{
				worldTile = randomIslandGround.getRandomTile();
				flag = true;
				WorldTile[] neighboursAll = worldTile.neighboursAll;
				for (int j = 0; j < neighboursAll.Length; j++)
				{
					neighboursAll[j].startFire();
				}
			}
		}
		if (flag)
		{
			WorldLog.logDisaster(pAsset, worldTile);
		}
	}

	public void spawnSmallEarthquake(DisasterAsset pAsset)
	{
		WorldTile random = World.world.tiles_list.GetRandom();
		Earthquake.startQuake(random, EarthquakeType.SmallDisaster);
		WorldLog.logDisaster(pAsset, random);
	}

	public void spawnDragon(DisasterAsset pAsset)
	{
		if (checkUnitSpawnLimits(pAsset))
		{
			TileZone tileZone = ((!Randy.randomBool()) ? World.world.zone_calculator.getZone(World.world.zone_calculator.zones_total_x - 1, Randy.randomInt(0, World.world.zone_calculator.zones_total_y)) : World.world.zone_calculator.getZone(0, Randy.randomInt(0, World.world.zone_calculator.zones_total_y)));
			WorldTile centerTile = tileZone.centerTile;
			spawnDisasterUnit(pAsset, centerTile);
			WorldLog.logDisaster(pAsset, centerTile);
		}
	}

	public void startAlienInvasion(DisasterAsset pAsset)
	{
		if (checkUnitSpawnLimits(pAsset))
		{
			TileZone tileZone = ((!Randy.randomBool()) ? World.world.zone_calculator.getZone(World.world.zone_calculator.zones_total_x - 1, Randy.randomInt(0, World.world.zone_calculator.zones_total_y)) : World.world.zone_calculator.getZone(0, Randy.randomInt(0, World.world.zone_calculator.zones_total_y)));
			WorldTile centerTile = tileZone.centerTile;
			spawnDisasterUnit(pAsset, centerTile);
			WorldLog.logDisaster(pAsset, centerTile);
		}
	}

	public void spawnBiomass(DisasterAsset pAsset)
	{
		if (!checkUnitSpawnLimits(pAsset))
		{
			return;
		}
		Building building = null;
		foreach (City item in World.world.cities.list.LoopRandom())
		{
			Building random = Randy.getRandom(item.buildings);
			if (random != null && random.isUsable())
			{
				building = random;
				break;
			}
		}
		if (building != null)
		{
			WorldTile random2 = building.tiles.GetRandom();
			if (spawnDisasterBuilding(pAsset, random2) != null)
			{
				spawnDisasterUnit(pAsset, random2);
				WorldLog.logDisaster(pAsset, random2);
			}
		}
	}

	public void spawnTumor(DisasterAsset pAsset)
	{
		if (!checkUnitSpawnLimits(pAsset))
		{
			return;
		}
		Building building = null;
		foreach (City item in World.world.cities.list.LoopRandom())
		{
			Building random = Randy.getRandom(item.buildings);
			if (random != null && random.isUsable())
			{
				building = random;
				break;
			}
		}
		if (building != null)
		{
			WorldTile random2 = building.tiles.GetRandom();
			if (spawnDisasterBuilding(pAsset, random2) != null)
			{
				spawnDisasterUnit(pAsset, random2);
				WorldLog.logDisaster(pAsset, random2);
			}
		}
	}

	public void gardenSurprise(DisasterAsset pAsset)
	{
		if (!checkUnitSpawnLimits(pAsset))
		{
			return;
		}
		Building building = null;
		foreach (City item in World.world.cities.list.LoopRandom())
		{
			Building buildingOfType = item.getBuildingOfType("type_windmill");
			if (buildingOfType != null && buildingOfType.isAlive())
			{
				building = buildingOfType;
				break;
			}
		}
		if (building != null)
		{
			WorldTile random = building.tiles.GetRandom();
			if (spawnDisasterBuilding(pAsset, random) != null)
			{
				spawnDisasterUnit(pAsset, random);
				WorldLog.logDisaster(pAsset, random);
			}
		}
	}

	public void spawnTornado(DisasterAsset pAsset)
	{
		WorldTile random = World.world.tiles_list.GetRandom();
		EffectsLibrary.spawnAtTile("fx_tornado", random, 0.5f);
		WorldLog.logDisaster(pAsset, random);
	}

	public void spawnMeteorite(DisasterAsset pAsset)
	{
		WorldTile random = World.world.tiles_list.GetRandom();
		Meteorite.spawnMeteoriteDisaster(random);
		WorldLog.logDisaster(pAsset, random);
	}

	public void simpleUnitAssetSpawnUsingIslands(DisasterAsset pAsset)
	{
		if (checkUnitSpawnLimits(pAsset))
		{
			TileIsland randomIslandGround = World.world.islands_calculator.getRandomIslandGround();
			if (randomIslandGround != null)
			{
				WorldTile randomTile = randomIslandGround.getRandomTile();
				spawnDisasterUnit(pAsset, randomTile);
				WorldLog.logDisaster(pAsset, randomTile);
			}
		}
	}

	private bool checkUnitSpawnLimits(DisasterAsset pAsset)
	{
		if (string.IsNullOrEmpty(pAsset.spawn_asset_unit))
		{
			return false;
		}
		if (AssetManager.actor_library.get(pAsset.spawn_asset_unit).units.Count >= pAsset.max_existing_units)
		{
			return false;
		}
		return true;
	}

	private Actor spawnDisasterUnit(DisasterAsset pAsset, WorldTile pTile)
	{
		EffectsLibrary.spawn("fx_spawn", pTile);
		Actor result = null;
		int num = Randy.randomInt(pAsset.units_min, pAsset.units_max);
		for (int i = 0; i < num; i++)
		{
			result = World.world.units.createNewUnit(pAsset.spawn_asset_unit, pTile, pMiracleSpawn: false, 0f, null, null, pSpawnWithItems: true, pAdultAge: true, pGiveOwnerlessItems: true);
		}
		return result;
	}

	private Building spawnDisasterBuilding(DisasterAsset pAsset, WorldTile pTile)
	{
		if (string.IsNullOrEmpty(pAsset.spawn_asset_building))
		{
			return null;
		}
		return World.world.buildings.addBuilding(pAsset.spawn_asset_building, pTile, pCheckForBuild: true);
	}
}
