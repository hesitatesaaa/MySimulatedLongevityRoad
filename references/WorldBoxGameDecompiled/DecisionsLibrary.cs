public class DecisionsLibrary : AssetLibrary<DecisionAsset>
{
	public DecisionAsset[] list_only_civ;

	public DecisionAsset[] list_only_children;

	public DecisionAsset[] list_only_city;

	public DecisionAsset[] list_only_animal;

	public DecisionAsset[] list_others;

	public override void init()
	{
		base.init();
		initDecisionsGeneral();
		initDecisionsTraits();
		initDecisionsChildren();
		initDecisionsAnimals();
		initDecisionDiets();
		initDecisionSleep();
		initDecisionsHerd();
		initDecisionsCivs();
		initDecisionsKings();
		initDecisionsWarriors();
		initDecisionsLeaders();
		initDecisionsBoats();
		initDecisionsBees();
		initDecisionsOther();
		initDecisionsUnique();
		initDecisionsSocialize();
		initDecisionsReproduction();
		initDecisionsClans();
		initDecisionsNomads();
		initDecisionsStatusRelated();
	}

	private void initDecisionsWarriors()
	{
		add(new DecisionAsset
		{
			id = "warrior_try_join_army_group",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconSoldier",
			cooldown = 5,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				return !pActor.hasArmy();
			},
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "check_warrior_limit",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconArmyList",
			cooldown = 60,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				return pActor.inOwnCityBorders() ? true : false;
			},
			weight = 0.7f
		});
		add(new DecisionAsset
		{
			id = "city_walking_to_danger_zone",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconArrowAttackTarget",
			cooldown = 5,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.city.isInDanger())
				{
					return false;
				}
				return pActor.inOwnCityBorders() ? true : false;
			},
			weight = 2.7f
		});
		add(new DecisionAsset
		{
			id = "warrior_army_captain_idle_walking_city",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconArmyList",
			cooldown = 20,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isArmyGroupLeader())
				{
					return false;
				}
				return !pActor.city.hasAttackZoneOrder();
			},
			weight = 1.3f
		});
		add(new DecisionAsset
		{
			id = "warrior_army_captain_waiting",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconClock",
			cooldown = 20,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isArmyGroupLeader())
				{
					return false;
				}
				return !pActor.city.hasAttackZoneOrder();
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "warrior_army_leader_move_random",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 1,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				return pActor.isArmyGroupLeader() ? true : false;
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "warrior_army_leader_move_to_attack_target",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconArrowAttackTarget",
			cooldown = 1,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isArmyGroupLeader())
				{
					return false;
				}
				return pActor.city.hasAttackZoneOrder() ? true : false;
			},
			weight = 2f
		});
		add(new DecisionAsset
		{
			id = "warrior_army_follow_leader",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconLoyalty",
			cooldown = 1,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isArmyGroupWarrior())
				{
					return false;
				}
				if (!pActor.army.hasCaptain())
				{
					return false;
				}
				WorldTile current_tile = pActor.army.getCaptain().current_tile;
				if (!pActor.current_tile.isSameIsland(current_tile))
				{
					return false;
				}
				return pActor.city.hasAttackZoneOrder() ? true : false;
			},
			weight = 5f
		});
		add(new DecisionAsset
		{
			id = "warrior_random_move",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 4,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isArmyGroupWarrior())
				{
					return false;
				}
				if (!pActor.army.hasCaptain())
				{
					return true;
				}
				WorldTile current_tile = pActor.army.getCaptain().current_tile;
				return !pActor.current_tile.isSameIsland(current_tile);
			},
			weight = 1.6f
		});
		add(new DecisionAsset
		{
			id = "check_warrior_transport",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBoat",
			cooldown = 6,
			unique = true,
			weight = 2f
		});
		add(new DecisionAsset
		{
			id = "warrior_train_with_dummy",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconWarfare",
			cooldown = 100,
			unique = true,
			city_must_be_safe = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.inOwnCityBorders())
				{
					return false;
				}
				return pActor.city.hasBuildingType("type_training_dummies", pCountOnlyFinished: true, pActor.current_island) ? true : false;
			},
			weight = 1.1f
		});
	}

	private void initDecisionsStatusRelated()
	{
		add(new DecisionAsset
		{
			id = "check_swearing",
			priority = NeuroLayer.Layer_3_High,
			task_id = "swearing",
			path_icon = "ui/Icons/iconSwearing",
			cooldown_on_launch_failure = true,
			cooldown = 60,
			unique = true,
			action_check_launch = (Actor _) => Randy.randomChance(0.1f) ? true : false,
			weight = 0.1f
		});
		add(new DecisionAsset
		{
			id = "do_tantrum",
			priority = NeuroLayer.Layer_4_Critical,
			path_icon = "ui/Icons/iconTantrum",
			cooldown = 1,
			unique = true,
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "possessed_following",
			priority = NeuroLayer.Layer_4_Critical,
			path_icon = "ui/Icons/iconPossessed",
			cooldown = 1,
			unique = true,
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "status_confused",
			priority = NeuroLayer.Layer_4_Critical,
			path_icon = "ui/Icons/iconConfused",
			cooldown = 1,
			unique = true,
			weight = 3.5f
		});
		add(new DecisionAsset
		{
			id = "run_to_water_when_on_fire",
			priority = NeuroLayer.Layer_4_Critical,
			path_icon = "ui/Icons/iconFire",
			cooldown = 1,
			unique = true,
			action_check_launch = (Actor pActor) => pActor.asset.run_to_water_when_on_fire ? true : false,
			weight = 5f
		});
	}

	private void initDecisionsNomads()
	{
		add(new DecisionAsset
		{
			id = "try_to_start_new_civilization",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconKingdom",
			cooldown_on_launch_failure = true,
			cooldown = 30,
			unique = true,
			only_sapient = true,
			only_safe = true,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isKing())
				{
					return false;
				}
				if (pActor.hasCity())
				{
					return false;
				}
				if (pActor.current_zone.hasCity())
				{
					if (!pActor.current_zone.city.isNeutral())
					{
						return false;
					}
				}
				else if (!pActor.canBuildNewCity())
				{
					return false;
				}
				return true;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "check_join_city",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconCity",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.hasCity())
				{
					return false;
				}
				if (pActor.kingdom.asset.is_forced_by_trait)
				{
					return false;
				}
				return pActor.current_zone.hasCity() ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "check_join_empty_nearby_city",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconCity",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.hasCity())
				{
					return false;
				}
				return !pActor.kingdom.asset.is_forced_by_trait;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "build_civ_city_here",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconCity",
			cooldown = 60,
			only_adult = true,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.isKingdomCiv())
				{
					return false;
				}
				if (pActor.hasCity())
				{
					return false;
				}
				return !Finder.isEnemyNearOnSameIsland(pActor);
			},
			weight = 1f
		});
	}

	private void initDecisionsHerd()
	{
		add(new DecisionAsset
		{
			id = "family_check_existence",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconFamily",
			cooldown = 60,
			unique = true,
			action_check_launch = (Actor pActor) => pActor.family.countUnits() <= 1,
			weight = 2f
		});
		add(new DecisionAsset
		{
			id = "family_alpha_move",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconFamily",
			cooldown = 200,
			unique = true,
			only_herd = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isSapient())
				{
					return false;
				}
				return pActor.family.isAlpha(pActor) ? true : false;
			},
			weight = 0.8f
		});
		add(new DecisionAsset
		{
			id = "family_group_follow",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconFamily",
			cooldown = 5,
			unique = true,
			only_herd = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isSapient())
				{
					return false;
				}
				return !pActor.family.isAlpha(pActor);
			},
			weight = 0.7f
		});
		add(new DecisionAsset
		{
			id = "family_group_leave",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconFamily",
			cooldown = 20,
			unique = true,
			only_herd = true,
			only_adult = true,
			only_safe = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isSapient())
				{
					return false;
				}
				if (!pActor.family.isFull())
				{
					return false;
				}
				return !pActor.family.isAlpha(pActor);
			},
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "family_group_join_or_new_herd",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconFamily",
			cooldown = 60,
			only_herd = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isSapient())
				{
					return false;
				}
				return !pActor.hasFamily();
			},
			weight = 0.3f
		});
	}

	private void initDecisionSleep()
	{
		add(new DecisionAsset
		{
			id = "bored_sleep",
			priority = NeuroLayer.Layer_0_Minimal,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			cooldown = 90,
			only_safe = true,
			city_must_be_safe = true,
			weight = 0.05f
		});
		add(new DecisionAsset
		{
			id = "polyphasic_sleep",
			priority = NeuroLayer.Layer_1_Low,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			unique = true,
			cooldown = 90,
			only_safe = true,
			city_must_be_safe = true,
			weight = 0.8f
		});
		add(new DecisionAsset
		{
			id = "monophasic_sleep",
			priority = NeuroLayer.Layer_1_Low,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			unique = true,
			cooldown = 200,
			only_safe = true,
			city_must_be_safe = true,
			weight = 0.75f
		});
		add(new DecisionAsset
		{
			id = "sleep_at_winter_age",
			priority = NeuroLayer.Layer_2_Moderate,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			unique = true,
			cooldown = 30,
			only_safe = true,
			city_must_be_safe = true,
			action_check_launch = (Actor _) => World.world.era_manager.isWinter() ? true : false,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "sleep_at_dark_age",
			priority = NeuroLayer.Layer_2_Moderate,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			unique = true,
			cooldown = 30,
			only_safe = true,
			city_must_be_safe = true,
			action_check_launch = (Actor _) => World.world.era_manager.isNight() ? true : false,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "sleep_when_not_chaos_age",
			priority = NeuroLayer.Layer_2_Moderate,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			unique = true,
			cooldown = 30,
			only_safe = true,
			city_must_be_safe = true,
			action_check_launch = (Actor _) => !World.world.era_manager.isChaosAge(),
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "sleep_at_light_age",
			priority = NeuroLayer.Layer_2_Moderate,
			task_id = "decide_where_to_sleep",
			path_icon = "ui/Icons/iconSleep",
			unique = true,
			cooldown = 30,
			only_safe = true,
			city_must_be_safe = true,
			action_check_launch = (Actor _) => World.world.era_manager.isLightAge() ? true : false,
			weight = 1f
		});
	}

	private void initDecisionDiets()
	{
		add(new DecisionAsset
		{
			id = "diet_wood",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_xylophagy",
			unique = true,
			cooldown = 30,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_tiles",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_geophagy",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_minerals",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_lithotroph",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_algae",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_algivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_fish",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_piscivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_fruits",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_frugivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_flowers",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_florivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_nectar",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_nectarivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_crops",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_granivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_vegetation",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_folivore",
			unique = true,
			cooldown = 10,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_grass",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_graminivore",
			unique = true,
			cooldown = 20,
			only_safe = true,
			only_hungry = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "diet_meat",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_carnivore",
			unique = true,
			cooldown = 55,
			only_safe = true,
			only_hungry = true,
			cooldown_on_launch_failure = true,
			weight = 0.96f
		});
		add(new DecisionAsset
		{
			id = "diet_blood",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_hematophagy",
			unique = true,
			cooldown = 55,
			only_safe = true,
			only_hungry = true,
			cooldown_on_launch_failure = true,
			weight = 0.96f
		});
		add(new DecisionAsset
		{
			id = "diet_meat_insect",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_insectivore",
			unique = true,
			cooldown = 55,
			only_safe = true,
			only_hungry = true,
			cooldown_on_launch_failure = true,
			weight = 0.96f
		});
		add(new DecisionAsset
		{
			id = "diet_same_species",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_diet_cannibalism",
			unique = true,
			cooldown = 60,
			only_safe = true,
			only_hungry = true,
			cooldown_on_launch_failure = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.getNutritionRatio() >= 0.1f)
				{
					return false;
				}
				return pActor.hasStatus("starving") ? true : false;
			},
			weight = 0.3f
		});
	}

	private void initDecisionsAnimals()
	{
		add(new DecisionAsset
		{
			id = "run_away_from_carnivore",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/actor_traits/iconAgile",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.isAnimal())
				{
					return false;
				}
				bool flag = false;
				if (!pActor.isCarnivore())
				{
					flag = Finder.isEnemyNearOnSameIslandAndCarnivore(pActor);
				}
				return flag ? true : false;
			},
			weight = 2f
		});
		add(new DecisionAsset
		{
			id = "check_if_stuck_on_small_land",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconTileSoil",
			cooldown = 90,
			action_check_launch = (Actor pActor) => !pActor.current_tile.region.island.isGoodIslandForActor(pActor),
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "run_away",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/actor_traits/iconAgile",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.isFighting())
				{
					return false;
				}
				return pActor.getHealthRatio() > 0.2f;
			},
			weight = 3.1f
		});
		add(new DecisionAsset
		{
			id = "run_away_being_sus",
			priority = NeuroLayer.Layer_3_High,
			task_id = "run_away",
			path_icon = "ui/Icons/actor_traits/iconAgile",
			cooldown = 10,
			unique = true,
			action_check_launch = (Actor pActor) => true,
			weight = 5f
		});
	}

	private void initDecisionsSocialize()
	{
		add(new DecisionAsset
		{
			id = "socialize_initial_check",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/culture_traits/culture_trait_gossip_lovers",
			cooldown = 30,
			unique = true,
			cooldown_on_launch_failure = true,
			action_check_launch = (Actor pActor) => pActor.canSocialize() ? true : false,
			weight = 0.5f
		});
	}

	private void initDecisionsReproduction()
	{
		add(new DecisionAsset
		{
			id = "sexual_reproduction_try",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_sexual",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 20,
			only_adult = true,
			only_safe = true,
			city_must_be_safe = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.canBreed())
				{
					return false;
				}
				if (!pActor.hasLover())
				{
					return false;
				}
				if (pActor.isHungry())
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "asexual_reproduction_divine",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_divine",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 60,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.canBreed())
				{
					return false;
				}
				if (pActor.hasLover())
				{
					return false;
				}
				if (pActor.hasTrait("miracle_bearer"))
				{
					return false;
				}
				if (pActor.getAgeRatio() < 0.6f)
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "status_soul_harvested",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconSoulHarvested",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 40,
			only_safe = true,
			action_check_launch = delegate(Actor pActor)
			{
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "asexual_reproduction_fission",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_fission",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 20,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.canBreed())
				{
					return false;
				}
				if (pActor.getHealthRatio() < 0.9f)
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "asexual_reproduction_budding",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_budding",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 50,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.canBreed())
				{
					return false;
				}
				if (pActor.getHealthRatio() < 0.9f)
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "asexual_reproduction_parthenogenesis",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_parthenogenesis",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 40,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.isSexFemale())
				{
					return false;
				}
				if (!pActor.canBreed())
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "asexual_reproduction_spores",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_spores",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 60,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.canBreed())
				{
					return false;
				}
				if (!pActor.hasStatus("just_ate"))
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight_calculate_custom = (Actor pActor) => (!pActor.hasReachedOffspringLimit()) ? 2f : 0.1f
		});
		add(new DecisionAsset
		{
			id = "asexual_reproduction_vegetative",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_reproduction_vegetative",
			unique = true,
			cooldown_on_launch_failure = true,
			cooldown = 30,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.canBreed())
				{
					return false;
				}
				if (pActor.hasStatus("taking_roots"))
				{
					return false;
				}
				pActor.subspecies.countReproductionNeuron();
				return true;
			},
			weight = 2.5f
		});
	}

	private void initDecisionsClans()
	{
		add(new DecisionAsset
		{
			id = "try_new_plot",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconPlot",
			cooldown = 40,
			list_civ = true,
			only_adult = true,
			only_safe = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.hasPlot())
				{
					return false;
				}
				return pActor.hasClan() ? true : false;
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "check_plot",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconPlot",
			cooldown = 3,
			only_safe = true,
			unique = true,
			action_check_launch = (Actor pActor) => pActor.hasPlot() ? true : false,
			weight = 5f
		});
	}

	private void initDecisionsKings()
	{
		add(new DecisionAsset
		{
			id = "king_check_new_city_foundation",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconCity",
			cooldown = 60,
			unique = true,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!WorldLawLibrary.world_law_kingdom_expansion.isEnabled())
				{
					return false;
				}
				return !pActor.kingdom.hasEnemies();
			},
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "king_change_kingdom_language",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconLanguage",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasLanguage())
				{
					return false;
				}
				return (pActor.kingdom.getLanguage() != pActor.language) ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "king_change_kingdom_culture",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconCulture",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCulture())
				{
					return false;
				}
				return (pActor.kingdom.getCulture() != pActor.culture) ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "king_change_kingdom_religion",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconReligion",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasReligion())
				{
					return false;
				}
				return (pActor.kingdom.getReligion() != pActor.religion) ? true : false;
			},
			weight = 1f
		});
	}

	private void initDecisionsLeaders()
	{
		add(new DecisionAsset
		{
			id = "leader_change_city_language",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconLanguage",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasLanguage())
				{
					return false;
				}
				return (pActor.city.getLanguage() != pActor.language) ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "leader_change_city_culture",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconCulture",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasCulture())
				{
					return false;
				}
				return (pActor.city.getCulture() != pActor.culture) ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "leader_change_city_religion",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconReligion",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.hasReligion())
				{
					return false;
				}
				return (pActor.city.getReligion() != pActor.religion) ? true : false;
			},
			weight = 1f
		});
	}

	private void initDecisionsCivs()
	{
		add(new DecisionAsset
		{
			id = "try_to_return_to_home_city",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconHoused",
			cooldown = 15,
			unique = true,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (pActor.inOwnCityBorders())
				{
					return false;
				}
				return !pActor.inOwnCityIsland();
			},
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "random_move_near_house",
			priority = NeuroLayer.Layer_0_Minimal,
			path_icon = "ui/Icons/iconLivingHouse",
			cooldown = 30,
			action_check_launch = (Actor pActor) => pActor.hasHouse() ? true : false,
			weight = 0.3f
		});
		add(new DecisionAsset
		{
			id = "try_to_read",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBooks",
			cooldown = 120,
			only_adult = true,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (pActor.stats["intelligence"] <= 5f)
				{
					return false;
				}
				return pActor.city.hasBooksToRead(pActor) ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "try_affect_dreams",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/subspecies_traits/subspecies_trait_dreamweavers",
			cooldown = 120,
			only_adult = true,
			unique = true,
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "try_to_take_city_item",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconEquipmentEditor2",
			cooldown = 180,
			unique = true,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.inOwnCityBorders())
				{
					return false;
				}
				return pActor.city.data.equipment.hasAnyItem() ? true : false;
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "make_items",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconReforge",
			cooldown = 90,
			unique = true,
			only_adult = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasHouse())
				{
					return false;
				}
				if (!pActor.inOwnCityBorders())
				{
					return false;
				}
				return pActor.city.hasResourcesForNewItems() ? true : false;
			},
			weight = 0.4f
		});
		add(new DecisionAsset
		{
			id = "city_idle_walking",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconCity",
			cooldown = 5,
			unique = true,
			action_check_launch = (Actor pActor) => pActor.city.hasZones() ? true : false,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "store_resources",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconCityInventory",
			cooldown = 5,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.isCarryingResources())
				{
					return false;
				}
				if (!pActor.city.hasStorageBuilding())
				{
					return false;
				}
				return (!pActor.isWarrior() || pActor.inOwnCityBorders()) ? true : false;
			},
			weight = 3.1f
		});
		add(new DecisionAsset
		{
			id = "stay_in_own_home",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconHoused",
			cooldown = 15,
			list_civ = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasHouse())
				{
					return false;
				}
				if (pActor.getHappinessRatio() > 0.5f)
				{
					return false;
				}
				return Randy.randomChance(0.2f) ? true : false;
			},
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "generate_loot",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconLoot",
			cooldown = 60,
			list_civ = true,
			action_check_launch = (Actor pActor) => pActor.hasHouse() ? true : false,
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "try_to_eat_city_food",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconHunger",
			cooldown = 10,
			unique = true,
			only_hungry = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				return pActor.city.hasSuitableFood(pActor.subspecies) ? true : false;
			},
			weight = 2.6f
		});
		add(new DecisionAsset
		{
			id = "find_city_job",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconShowTasks",
			cooldown = 50,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!pActor.isInCityIsland())
				{
					return false;
				}
				if (!pActor.city.jobs.hasAnyTask())
				{
					return false;
				}
				if (!pActor.canWork())
				{
					return false;
				}
				return (pActor.city.getPopulationPeople() <= 30 || !pActor.isWarrior()) ? true : false;
			},
			weight_calculate_custom = delegate(Actor pActor)
			{
				float result = 2f;
				if (pActor.isStarving())
				{
					result = 0.3f;
				}
				else if (pActor.isHungry())
				{
					result = 1f;
				}
				return result;
			}
		});
		add(new DecisionAsset
		{
			id = "claim_land",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/citizen_jobs/iconCitizenLandClaimer",
			cooldown = 60,
			unique = true,
			cooldown_on_launch_failure = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				return pActor.city.canGrowZones() ? true : false;
			},
			weight_calculate_custom = (Actor pActor) => 2f + pActor.stats["stewardship"] / 5f * 0.1f
		});
		add(new DecisionAsset
		{
			id = "put_out_fire",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconMoney",
			cooldown = 1,
			cooldown_on_launch_failure = true,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.inOwnCityBorders())
				{
					return false;
				}
				if (!pActor.city.isCityUnderDangerFire())
				{
					return false;
				}
				return !pActor.hasStatus("burning");
			},
			weight = 4f
		});
		add(new DecisionAsset
		{
			id = "give_tax",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconMoney",
			cooldown = 90,
			cooldown_on_launch_failure = true,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.inOwnCityBorders())
				{
					return false;
				}
				return (pActor.data.loot >= pActor.kingdom.getLootMin()) ? true : false;
			},
			weight = 2.55f
		});
		add(new DecisionAsset
		{
			id = "check_city_destroyed",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconWar",
			cooldown = 10,
			list_civ = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.hasCity())
				{
					return false;
				}
				if (pActor.profession_asset == null)
				{
					return false;
				}
				return pActor.profession_asset.cancel_when_no_city ? true : false;
			},
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "check_lover_city",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconCity",
			cooldown = 30,
			only_adult = true,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isKing())
				{
					return false;
				}
				if (pActor.isCityLeader())
				{
					return false;
				}
				if (pActor.isSexMale())
				{
					return false;
				}
				if (!pActor.hasLover())
				{
					return false;
				}
				if (!pActor.lover.hasCity())
				{
					return false;
				}
				if (!pActor.lover.hasHouse())
				{
					return false;
				}
				return !pActor.hasSameCity(pActor.lover);
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "find_lover",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconArrowLover",
			cooldown = 50,
			only_adult = true,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.hasLover())
				{
					return false;
				}
				return pActor.isBreedingAge() ? true : false;
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "find_house",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBuildings",
			cooldown = 10,
			unique = true,
			action_check_launch = (Actor pActor) => !pActor.hasHouse(),
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "replenish_energy",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconStamina",
			cooldown = 30,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isStaminaFull() && pActor.isManaFull())
				{
					return false;
				}
				return pActor.getCity().hasBuildingType("type_well", pCountOnlyFinished: true, pActor.current_island) ? true : false;
			},
			weight = 0.3f
		});
		add(new DecisionAsset
		{
			id = "repair_equipment",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconReforge",
			cooldown = 300,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasEquipment())
				{
					return false;
				}
				return pActor.getCity().hasBuildingType("type_barracks", pCountOnlyFinished: true, pActor.current_island) ? true : false;
			},
			weight = 0.3f
		});
	}

	private void initDecisionsBoats()
	{
		add(new DecisionAsset
		{
			id = "boat_check_existence",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconBoat",
			cooldown = 5,
			unique = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "boat_danger_check",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconBoat",
			cooldown = 5,
			unique = true,
			weight = 1.25f
		});
		add(new DecisionAsset
		{
			id = "boat_idle",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconBoat",
			cooldown = 5,
			unique = true,
			weight = 0.75f
		});
		add(new DecisionAsset
		{
			id = "boat_check_limits",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconBoat",
			cooldown = 15,
			unique = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "boat_fishing",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconResFish",
			cooldown = 1,
			unique = true,
			weight = 1.3f
		});
		add(new DecisionAsset
		{
			id = "boat_trading",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconCityInventory",
			cooldown = 1,
			unique = true,
			weight = 1.3f
		});
		add(new DecisionAsset
		{
			id = "boat_transport_check",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBoat",
			cooldown = 1,
			unique = true,
			weight = 1.3f
		});
	}

	private void initDecisionsBees()
	{
		add(new DecisionAsset
		{
			id = "pollinate",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconBee",
			cooldown = 10,
			unique = true,
			weight = 1.3f,
			action_check_launch = (Actor pActor) => (!(pActor.asset.id == "bee") || pActor.hasHomeBuilding()) ? true : false
		});
		add(new DecisionAsset
		{
			id = "bee_find_hive",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBeehive",
			cooldown = 20,
			only_mob = true,
			unique = true,
			weight = 1.5f,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.isKingdomCiv())
				{
					return false;
				}
				return !pActor.hasHomeBuilding();
			}
		});
		add(new DecisionAsset
		{
			id = "bee_create_hive",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBeehive",
			cooldown = 200,
			unique = true,
			only_mob = true,
			cooldown_on_launch_failure = true,
			weight = 1.5f,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.hasHomeBuilding())
				{
					return false;
				}
				return pActor.isSexFemale() ? true : false;
			}
		});
	}

	private void initDecisionsGeneral()
	{
		add(new DecisionAsset
		{
			id = "swim_to_island",
			priority = NeuroLayer.Layer_4_Critical,
			path_icon = "ui/Icons/iconTileShallowWater",
			cooldown = 1,
			action_check_launch = (Actor pActor) => !pActor.isInStablePlace(),
			weight = 4f
		});
		add(new DecisionAsset
		{
			id = "move_to_water",
			priority = NeuroLayer.Layer_4_Critical,
			path_icon = "ui/Icons/iconTileShallowWater",
			cooldown = 1,
			action_check_launch = (Actor pActor) => !pActor.isInStablePlace(),
			weight = 4f
		});
		add(new DecisionAsset
		{
			id = "random_move",
			priority = NeuroLayer.Layer_0_Minimal,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 1,
			weight = 0.2f
		});
		add(new DecisionAsset
		{
			id = "random_fun_move",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 10,
			list_animal = true,
			weight = 0.2f
		});
		add(new DecisionAsset
		{
			id = "wait5",
			priority = NeuroLayer.Layer_0_Minimal,
			path_icon = "ui/Icons/iconClock",
			cooldown = 10,
			weight = 0.1f
		});
	}

	private void initDecisionsTraits()
	{
		add(new DecisionAsset
		{
			id = "follow_desire_target",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconGoldBrain",
			cooldown = 5,
			unique = true,
			weight = 4f
		});
		add(new DecisionAsset
		{
			id = "try_to_poop",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconPoop",
			cooldown = 60,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.asset.actor_size == ActorSize.S0_Bug)
				{
					return false;
				}
				return !pActor.hasSubspeciesTrait("reproduction_spores");
			},
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "try_to_launch_fireworks",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconFireworks",
			cooldown = 60,
			unique = true,
			action_check_launch = (Actor pActor) => pActor.hasEnoughMoney(SimGlobals.m.festive_fireworks_cost) ? true : false,
			weight = 1.5f
		});
		add(new DecisionAsset
		{
			id = "reflection",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconBre",
			cooldown = 100,
			unique = true,
			action_check_launch = (Actor pActor) => pActor.subspecies.can_process_emotions ? true : false,
			weight = 2.5f
		});
		add(new DecisionAsset
		{
			id = "madness_random_emotion",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/actor_traits/iconMadness",
			cooldown = 60,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasSubspecies())
				{
					return false;
				}
				return pActor.subspecies.can_process_emotions ? true : false;
			},
			weight = 2.5f
		});
		add(new DecisionAsset
		{
			id = "try_to_steal_money",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/actor_traits/iconThief",
			cooldown = 60,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.isHungry())
				{
					return false;
				}
				return pActor.canGetFoodFromCity() ? true : false;
			},
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "kill_unruly_clan_members",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/clan_traits/clan_trait_deathbound",
			cooldown = 200,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.clan.getChief() != pActor)
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			weight = 3f
		});
		add(new DecisionAsset
		{
			id = "banish_unruly_clan_members",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/clan_traits/clan_trait_blood_pact",
			cooldown = 200,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (pActor.clan.getChief() != pActor)
				{
					return false;
				}
				return pActor.kingdom.hasEnemies() ? true : false;
			},
			weight = 3f
		});
	}

	private void initDecisionsUnique()
	{
		add(new DecisionAsset
		{
			id = "check_heal",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconHealth",
			cooldown = 5,
			unique = true,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "make_skeleton",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconSkeleton",
			cooldown = 10,
			unique = true,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "spawn_fertilizer",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconFertilizerPlants",
			cooldown = 10,
			unique = true,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "random_teleport",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 20,
			unique = true,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "teleport_back_home",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 60,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.hasCity())
				{
					return false;
				}
				if (!Randy.randomChance(0.3f))
				{
					return false;
				}
				WorldTile tile = pActor.city.getTile();
				if (tile == null)
				{
					return false;
				}
				return !tile.isSameIsland(pActor.current_tile);
			},
			unique = true,
			weight = 1f
		});
		add(new DecisionAsset
		{
			id = "check_cure",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconHealth",
			cooldown = 15,
			unique = true,
			action_check_launch = (Actor _) => Randy.randomChance(0.3f) ? true : false,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "burn_tumors",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconFire",
			cooldown = 10,
			unique = true,
			action_check_launch = (Actor _) => Randy.randomChance(0.5f) ? true : false,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "random_move_towards_civ_building",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconArrowDestination",
			cooldown = 10,
			unique = true,
			weight = 0.5f
		});
		add(new DecisionAsset
		{
			id = "random_swim",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconTileShallowWater",
			cooldown = 10,
			unique = true,
			weight = 2f
		});
	}

	private void initDecisionsOther()
	{
		add(new DecisionAsset
		{
			id = "attack_golden_brain",
			priority = NeuroLayer.Layer_3_High,
			path_icon = "ui/Icons/iconGoldBrain",
			cooldown = 60,
			only_mob = true,
			unique = true,
			weight = 1f
		});
	}

	private void initDecisionsChildren()
	{
		add(new DecisionAsset
		{
			id = "child_random_flips",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconChildren",
			cooldown = 5,
			list_baby = true,
			weight = 0.1f
		});
		add(new DecisionAsset
		{
			id = "child_play_at_one_spot",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconChildren",
			cooldown = 5,
			list_baby = true,
			weight = 0.1f
		});
		add(new DecisionAsset
		{
			id = "child_random_jump",
			priority = NeuroLayer.Layer_1_Low,
			path_icon = "ui/Icons/iconChildren",
			cooldown = 5,
			list_baby = true,
			weight = 0.1f
		});
		add(new DecisionAsset
		{
			id = "child_follow_parent",
			priority = NeuroLayer.Layer_2_Moderate,
			path_icon = "ui/Icons/iconAdults",
			cooldown = 10,
			unique = true,
			action_check_launch = delegate(Actor pActor)
			{
				if (!pActor.family.hasFounders())
				{
					return false;
				}
				return pActor.isBaby() ? true : false;
			},
			weight = 0.2f
		});
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (DecisionAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}

	public override void linkAssets()
	{
		using ListPool<DecisionAsset> listPool = new ListPool<DecisionAsset>();
		using ListPool<DecisionAsset> listPool2 = new ListPool<DecisionAsset>();
		using ListPool<DecisionAsset> listPool3 = new ListPool<DecisionAsset>();
		using ListPool<DecisionAsset> listPool4 = new ListPool<DecisionAsset>();
		using ListPool<DecisionAsset> listPool5 = new ListPool<DecisionAsset>();
		int num = 0;
		foreach (DecisionAsset item in list)
		{
			item.decision_index = num++;
			item.priority_int_cached = (int)item.priority;
			item.has_weight_custom = item.weight_calculate_custom != null;
			if (!item.unique)
			{
				if (item.list_baby)
				{
					listPool2.Add(item);
				}
				else if (item.list_animal)
				{
					listPool4.Add(item);
				}
				else if (item.list_civ)
				{
					listPool.Add(item);
				}
				else
				{
					listPool5.Add(item);
				}
			}
		}
		list_only_civ = listPool.ToArray();
		list_only_children = listPool2.ToArray();
		list_only_city = listPool3.ToArray();
		list_only_animal = listPool4.ToArray();
		list_others = listPool5.ToArray();
		base.linkAssets();
	}

	public override void editorDiagnostic()
	{
		foreach (DecisionAsset item in list)
		{
			checkSpriteExists("path_icon", item.path_icon, item);
		}
		foreach (DecisionAsset item2 in list)
		{
			int num = 0;
			if (item2.unique && (item2.list_civ || item2.list_baby || item2.list_animal))
			{
				BaseAssetLibrary.logAssetError("<e>" + item2.id + "</e>: Unique but also has list setting?");
			}
			if (list_only_civ.Contains(item2))
			{
				num++;
			}
			if (list_only_children.Contains(item2))
			{
				num++;
			}
			if (list_only_city.Contains(item2))
			{
				num++;
			}
			if (list_only_animal.Contains(item2))
			{
				num++;
			}
			if (list_others.Contains(item2))
			{
				num++;
			}
			if (item2.unique && num > 0)
			{
				BaseAssetLibrary.logAssetError("<e>" + item2.id + "</e>: Unique but also in a list?");
			}
			if (!item2.unique && num == 0)
			{
				BaseAssetLibrary.logAssetError("<e>" + item2.id + "</e>: Not unique but not in any list?");
			}
			if (num > 1)
			{
				BaseAssetLibrary.logAssetError("<e>" + item2.id + "</e>: In multiple lists?");
			}
		}
		base.editorDiagnostic();
	}
}
