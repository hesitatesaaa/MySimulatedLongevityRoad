namespace ai.behaviours;

public class BehaviourTaskActorLibrary : AssetLibrary<BehaviourTaskActor>
{
	public override void init()
	{
		base.init();
		initTasksMobs();
		initTasksSocializing();
		initTasksSubspeciesTraits();
		initTasksReproductionSexual();
		initTasksReproductionAsexual();
		initTasksChildren();
		initTasksStatusRelated();
		initTasksKings();
		initTasksLeaders();
		initTasksWarriors();
		initTasksSleep();
		initTasksPoop();
		initTasksThinkingReflectionHappiness();
		initTasksClanLeader();
		initTasksBoats();
		initTasksDragons();
		initTasksFingers();
		initTasksUFOs();
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "nothing",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_nothing"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconClock");
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "fighting",
			in_combat = true,
			locale_key = "task_unit_fight"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconWar");
		t.addBeh(new BehFightCheckEnemyIsOk());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: true, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "move_from_block",
			move_from_block = true,
			ignore_fight_check = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehMoveAwayFromBlock());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "swim_to_island",
			ignore_fight_check = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconTileShallowWater");
		t.addBeh(new BehGoToStablePlace());
		t.addBeh(new BehGoOrSwimToTileTarget());
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "move_to_water",
			ignore_fight_check = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconTileShallowWater");
		t.addBeh(new BehGoToStablePlace());
		t.addBeh(new BehGoOrSwimToTileTarget());
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "check_if_stuck_on_small_land",
			ignore_fight_check = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconTileSoil");
		t.addBeh(new BehCheckIfOnSmallLand());
		t.addBeh(new BehWalkIntoWaterCorner());
		t.addBeh(new BehGoOrSwimToTileTarget());
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "pollinate",
			locale_key = "task_unit_pollinate"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconBee");
		t.addBeh(new BehBeeCheckHome());
		t.addBeh(new BehFindBuilding("type_flower", pOnlyNonTargeted: true, pOnlyWithResources: false));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 3f, pLand: true));
		t.addBeh(new BehPollinate());
		t.addBeh(new BehBeeCheckReturnHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehBeeReturnHome());
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "bee_find_hive",
			locale_key = "task_unit_bee_find_hive"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/iconBeehive");
		t.addBeh(new BehBeeCheckNoHome());
		t.addBeh(new BehFindBuilding("type_hive", pOnlyNonTargeted: false, pOnlyWithResources: false));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehBeeJoinHive());
		t.addBeh(new BehBeeReturnHome());
		BehaviourTaskActor obj9 = new BehaviourTaskActor
		{
			id = "bee_create_hive",
			locale_key = "task_unit_bee_create_hive"
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.setIcon("ui/Icons/iconBeehive");
		t.addBeh(new BehBeeCheckNoHome());
		t.addBeh(new BehFindTile(TileFinderType.Biome));
		t.addBeh(new BehGoToTileTarget());
		for (int i = 0; i < 5; i++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, null, 0.5f, 40f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehBeeCreateHive());
		t.addBeh(new BehBeeJoinHive());
		t.addBeh(new BehBeeReturnHome());
		BehaviourTaskActor obj10 = new BehaviourTaskActor
		{
			id = "random_move",
			locale_key = "task_unit_move"
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait());
		BehaviourTaskActor obj11 = new BehaviourTaskActor
		{
			id = "random_fun_move",
			locale_key = "task_unit_move"
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		for (int j = 0; j < 6; j++)
		{
			t.addBeh(new BehFindRandomNeighbourTile());
			t.addBeh(new BehGoToTileTarget());
			t.addBeh(new BehRandomWait(0f, 0.01f));
		}
		BehaviourTaskActor obj12 = new BehaviourTaskActor
		{
			id = "run_away",
			speed_multiplier = 2f,
			ignore_fight_check = true,
			locale_key = "task_unit_flee"
		};
		pAsset = obj12;
		t = obj12;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconAgile");
		t.addBeh(new BehFindRandomFarTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait());
		BehaviourTaskActor obj13 = new BehaviourTaskActor
		{
			id = "run_away_from_carnivore",
			speed_multiplier = 2f,
			ignore_fight_check = true,
			locale_key = "task_unit_flee"
		};
		pAsset = obj13;
		t = obj13;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconAgile");
		t.addBeh(new BehFindRandomFarTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait());
		BehaviourTaskActor obj14 = new BehaviourTaskActor
		{
			id = "print_start"
		};
		pAsset = obj14;
		t = obj14;
		add(pAsset);
		t.setIcon("ui/Icons/iconPrinterStar");
		t.addBeh(new BehPrinterSetup());
		BehaviourTaskActor obj15 = new BehaviourTaskActor
		{
			id = "print_step"
		};
		pAsset = obj15;
		t = obj15;
		add(pAsset);
		t.setIcon("ui/Icons/iconPrinterStar");
		t.addBeh(new BehPrinterStep());
		BehaviourTaskActor obj16 = new BehaviourTaskActor
		{
			id = "worm_move"
		};
		pAsset = obj16;
		t = obj16;
		add(pAsset);
		t.setIcon("ui/Icons/iconWorm");
		t.addBeh(new BehFindRandomTile8Directions());
		t.addBeh(new BehWormDive());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehWormDig());
		BehaviourTaskActor obj17 = new BehaviourTaskActor
		{
			id = "sandspider_move"
		};
		pAsset = obj17;
		t = obj17;
		add(pAsset);
		t.setIcon("ui/Icons/iconSandSpider");
		t.addBeh(new BehFindRandomTile4Directions());
		t.addBeh(new BehSandspiderCheckSand());
		t.addBeh(new BehSandspiderCheckDie());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehSandspiderBuildSand());
		BehaviourTaskActor obj18 = new BehaviourTaskActor
		{
			id = "ant_black_island"
		};
		pAsset = obj18;
		t = obj18;
		add(pAsset);
		t.setIcon("ui/Icons/iconAntBlack");
		t.addBeh(new BehFindRandomTile4Directions());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehBlackAntBuildIsland());
		BehaviourTaskActor obj19 = new BehaviourTaskActor
		{
			id = "ant_black_sand"
		};
		pAsset = obj19;
		t = obj19;
		add(pAsset);
		t.setIcon("ui/Icons/iconAntBlack");
		t.addBeh(new BehFindRandomTile4Directions());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehBlackAntBuildSand());
		BehaviourTaskActor obj20 = new BehaviourTaskActor
		{
			id = "ant_red_move"
		};
		pAsset = obj20;
		t = obj20;
		add(pAsset);
		t.setIcon("ui/Icons/iconAntRed");
		t.addBeh(new BehAntSetup());
		t.addBeh(new BehFindRandomTile4Directions());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehAntSwitchGround());
		BehaviourTaskActor obj21 = new BehaviourTaskActor
		{
			id = "ant_blue_move"
		};
		pAsset = obj21;
		t = obj21;
		add(pAsset);
		t.setIcon("ui/Icons/iconAntBlue");
		t.addBeh(new BehFindRandomTile4Directions());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehBlueAntSwitchGround());
		BehaviourTaskActor obj22 = new BehaviourTaskActor
		{
			id = "ant_green_move"
		};
		pAsset = obj22;
		t = obj22;
		add(pAsset);
		t.setIcon("ui/Icons/iconAntGreen");
		t.addBeh(new BehFindRandomTile4Directions());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		t.addBeh(new BehGreenAntSwitchGround());
		BehaviourTaskActor obj23 = new BehaviourTaskActor
		{
			id = "random_wait_short_1",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_wait"
		};
		pAsset = obj23;
		t = obj23;
		add(pAsset);
		t.setIcon("ui/Icons/iconClock");
		t.addBeh(new BehRandomWait(0.1f));
		BehaviourTaskActor obj24 = new BehaviourTaskActor
		{
			id = "investigate_curiosity",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj24;
		t = obj24;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_inquisitive_nature");
		t.addBeh(new BehCheckCuriosityTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 3f));
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehRandomWait(1f, 3f));
		t.addBeh(new BehFindRandomNeighbourTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 3f));
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj25 = new BehaviourTaskActor
		{
			id = "random_animal_move",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj25;
		t = obj25;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehAnimalFindTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 6f));
		BehaviourTaskActor obj26 = new BehaviourTaskActor
		{
			id = "random_move_towards_civ_building",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj26;
		t = obj26;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehFindRandomCivBuildingTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 4f));
		BehaviourTaskActor obj27 = new BehaviourTaskActor
		{
			id = "stay_in_random_house",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_nothing"
		};
		pAsset = obj27;
		t = obj27;
		add(pAsset);
		t.setIcon("ui/Icons/iconBuildings");
		t.addBeh(new BehCityActorFindBuilding("random_house_building"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget(10f, 60f));
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj28 = new BehaviourTaskActor
		{
			id = "stay_in_own_home",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_nothing"
		};
		pAsset = obj28;
		t = obj28;
		add(pAsset);
		t.setIcon("ui/Icons/iconHoused");
		t.addBeh(new BehBuildingTargetHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget(10f, 60f));
		t.addBeh(new BehRestoreStats(0.1f, 0.2f));
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj29 = new BehaviourTaskActor
		{
			id = "generate_loot",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_nothing"
		};
		pAsset = obj29;
		t = obj29;
		add(pAsset);
		t.setIcon("ui/Icons/iconMoney");
		t.addBeh(new BehBuildingTargetHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget(2f, 6f));
		t.addBeh(new BehGenerateLootFromHouse());
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj30 = new BehaviourTaskActor
		{
			id = "random_swim",
			locale_key = "task_unit_move"
		};
		pAsset = obj30;
		t = obj30;
		add(pAsset);
		t.setIcon("ui/Icons/iconTileShallowWater");
		t.addBeh(new BehRandomSwim());
		t.addBeh(new BehRandomWait(0f, 2f));
		BehaviourTaskActor obj31 = new BehaviourTaskActor
		{
			id = "make_decision"
		};
		pAsset = obj31;
		t = obj31;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconStupid");
		t.addBeh(new BehMakeDecision());
		BehaviourTaskActor obj32 = new BehaviourTaskActor
		{
			id = "try_new_plot",
			locale_key = "task_unit_plot"
		};
		pAsset = obj32;
		t = obj32;
		add(pAsset);
		t.setIcon("ui/Icons/iconPlot");
		t.addBeh(new BehTryNewPlot());
		BehaviourTaskActor obj33 = new BehaviourTaskActor
		{
			id = "check_plot",
			locale_key = "task_unit_plot"
		};
		pAsset = obj33;
		t = obj33;
		add(pAsset);
		t.setIcon("ui/Icons/iconPlot");
		t.addBeh(new BehCheckPlot());
		BehaviourTaskActor obj34 = new BehaviourTaskActor
		{
			id = "progress_plot",
			force_hand_tool = "coffee_cup",
			locale_key = "task_unit_plot"
		};
		pAsset = obj34;
		t = obj34;
		add(pAsset);
		t.setIcon("ui/Icons/iconPlot");
		t.addBeh(new BehCheckPlotIsOk());
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehWait(2f));
		t.addBeh(new BehCheckPlotProgress());
		t.addBeh(new BehSpawnPlotProgressEffect());
		t.addBeh(new BehCheckNeeds(0));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj35 = new BehaviourTaskActor
		{
			id = "try_to_eat_city_food",
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj35;
		t = obj35;
		add(pAsset);
		t.setIcon("ui/Icons/iconHunger");
		t.addBeh(new BehCheckHasMoneyForCityFood());
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0.1f));
		for (int k = 0; k < 3; k++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 0.2f));
		}
		t.addBeh(new BehTryToEatCityFood());
		for (int l = 0; l < 3; l++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 0.2f));
		}
		t.addBeh(new BehRandomWait(1f, 5f));
		BehaviourTaskActor obj36 = new BehaviourTaskActor
		{
			id = "find_house"
		};
		pAsset = obj36;
		t = obj36;
		add(pAsset);
		t.setIcon("ui/Icons/iconBuildings");
		t.addBeh(new BehFindHouse());
		BehaviourTaskActor obj37 = new BehaviourTaskActor
		{
			id = "random_move_near_house",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj37;
		t = obj37;
		add(pAsset);
		t.setIcon("ui/Icons/iconLivingHouse");
		t.addBeh(new BehFindRandomFrontTileNearHouse());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 6f));
		BehaviourTaskActor obj38 = new BehaviourTaskActor
		{
			id = "end_job"
		};
		pAsset = obj38;
		t = obj38;
		add(pAsset);
		t.setIcon("ui/Icons/iconClose");
		t.addBeh(new BehEndJob());
		BehaviourTaskActor obj39 = new BehaviourTaskActor
		{
			id = "check_end_job"
		};
		pAsset = obj39;
		t = obj39;
		add(pAsset);
		t.setIcon("ui/Icons/iconClose");
		t.addBeh(new BehCheckEndCityActorJob());
		BehaviourTaskActor obj40 = new BehaviourTaskActor
		{
			id = "check_city_destroyed"
		};
		pAsset = obj40;
		t = obj40;
		add(pAsset);
		t.setIcon("ui/Icons/iconWar");
		t.addBeh(new BehCheckCityDestroyed());
		BehaviourTaskActor obj41 = new BehaviourTaskActor
		{
			id = "build_civ_city_here"
		};
		pAsset = obj41;
		t = obj41;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehCheckBuildCity());
		t.addBeh(new BehEndJob());
		BehaviourTaskActor obj42 = new BehaviourTaskActor
		{
			id = "try_to_start_new_civilization"
		};
		pAsset = obj42;
		t = obj42;
		add(pAsset);
		t.setIcon("ui/Icons/iconKingdom");
		t.addBeh(new BehCheckEnemyNotNear());
		t.addBeh(new BehCheckStartCivilization());
		BehaviourTaskActor obj43 = new BehaviourTaskActor
		{
			id = "check_join_city"
		};
		pAsset = obj43;
		t = obj43;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehJoinCity());
		t.addBeh(new BehEndJob());
		BehaviourTaskActor obj44 = new BehaviourTaskActor
		{
			id = "check_join_empty_nearby_city"
		};
		pAsset = obj44;
		t = obj44;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehFindNearbyPotentialCivCityToJoin());
		BehaviourTaskActor obj45 = new BehaviourTaskActor
		{
			id = "try_to_read",
			force_hand_tool = "book",
			cancellable_by_reproduction = true,
			cancellable_by_socialize = true,
			locale_key = "task_unit_read"
		};
		pAsset = obj45;
		t = obj45;
		add(pAsset);
		t.setIcon("ui/Icons/iconBooks");
		t.addBeh(new BehTryToRead());
		for (int m = 0; m < 7; m++)
		{
			t.addBeh(new BehFindRandomNeighbourTile());
			t.addBeh(new BehGoToTileTarget());
			t.addBeh(new BehSpawnHmmEffect());
			for (int n = 0; n < 5; n++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 1f, 6f));
			}
			t.addBeh(new BehActorReverseFlip());
			for (int num = 0; num < 5; num++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 1f, 6f));
			}
		}
		t.addBeh(new BehFinishReading());
		BehaviourTaskActor obj46 = new BehaviourTaskActor
		{
			id = "citizen"
		};
		pAsset = obj46;
		t = obj46;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehRandomWait(1f, 5f));
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		BehaviourTaskActor obj47 = new BehaviourTaskActor
		{
			id = "try_to_take_city_item"
		};
		pAsset = obj47;
		t = obj47;
		add(pAsset);
		t.setIcon("ui/Icons/items/icon_sword_wood");
		t.addBeh(new BehActorTryToTakeItemFromCity());
		BehaviourTaskActor obj48 = new BehaviourTaskActor
		{
			id = "city_idle_walking",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_walk"
		};
		pAsset = obj48;
		t = obj48;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehCityActorGetRandomIdleTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 6f));
		BehaviourTaskActor obj49 = new BehaviourTaskActor
		{
			id = "city_walking_to_danger_zone",
			locale_key = "task_unit_investigate"
		};
		pAsset = obj49;
		t = obj49;
		add(pAsset);
		t.setIcon("ui/Icons/iconWar");
		t.addBeh(new BehCityActorGetRandomDangerZone());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 6f));
		BehaviourTaskActor obj50 = new BehaviourTaskActor
		{
			id = "find_city_job"
		};
		pAsset = obj50;
		t = obj50;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehCityActorFindNewJob());
		BehaviourTaskActor obj51 = new BehaviourTaskActor
		{
			id = "give_tax"
		};
		pAsset = obj51;
		t = obj51;
		add(pAsset);
		t.setIcon("ui/Icons/iconTax");
		t.addBeh(new BehActorGiveTax());
		BehaviourTaskActor obj52 = new BehaviourTaskActor
		{
			id = "do_hunting",
			cancellable_by_reproduction = true,
			in_combat = true,
			ignore_fight_check = true,
			locale_key = "task_unit_hunt"
		};
		pAsset = obj52;
		t = obj52;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobHunter");
		t.addBeh(new BehFindTargetForHunter());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: false, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehAttackActorHuntingTarget());
		addActionsForDeliverResources(t);
		BehaviourTaskActor obj53 = new BehaviourTaskActor
		{
			id = "make_items"
		};
		pAsset = obj53;
		t = obj53;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobBlacksmith");
		t.addBeh(new BehBuildingTargetHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget(10f, 15f));
		t.addBeh(new BehMakeItem());
		t.addBeh(new BehActorTryToTakeItemFromCity());
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj54 = new BehaviourTaskActor
		{
			id = "cleaning",
			force_hand_tool = "hammer",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_cleaning_ruins"
		};
		pAsset = obj54;
		t = obj54;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobCleaner");
		t.addBeh(new BehCityActorFindBuilding("ruins"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0.3f, 0.3f));
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num2 = 0; num2 < 3; num2++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Ruin, "event:/SFX/CIVILIZATIONS/CleanRuins", 1f, 40f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Ruin, "event:/SFX/CIVILIZATIONS/CleanRuins", 0f, 40f, pCheckFlip: true, pLandIfHovering: true));
		t.addBeh(new BehRemoveRuins());
		addActionsForDeliverResources(t);
		BehaviourTaskActor obj55 = new BehaviourTaskActor
		{
			id = "manure_cleaning",
			force_hand_tool = "basket",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_cleaning_poop"
		};
		pAsset = obj55;
		t = obj55;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobCleaner");
		t.addBeh(new BehCityActorFindBuilding("type_poop"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num3 = 0; num3 < 5; num3++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/CIVILIZATIONS/CollectHerbs"));
		}
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t);
		t.addBeh(new BehCheckNeeds(6));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj56 = new BehaviourTaskActor
		{
			id = "put_out_fire",
			force_hand_tool = "bucket",
			locale_key = "task_unit_extinguish",
			is_fireman = true
		};
		pAsset = obj56;
		t = obj56;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobFireman");
		t.addBeh(new BehCityActorFindFireZone());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehCityActorFindClosestFire());
		t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 0f, -20f, pCheckFlip: false));
		t.addBeh(new BehRandomWait(0.1f, 0.5f));
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile));
		t.addBeh(new BehCityActorRemoveFire());
		t.addBeh(new BehRandomWait(0.4f, 1.2f));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj57 = new BehaviourTaskActor
		{
			id = "try_build_building",
			force_hand_tool = "hammer",
			locale_key = "task_unit_build"
		};
		pAsset = obj57;
		t = obj57;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobBuilder");
		t.addBeh(new BehCityActorFindBuilding("new_building"));
		t.addBeh(new BehSetNextTask("build_building", pClean: false, pForce: true));
		BehaviourTaskActor obj58 = new BehaviourTaskActor
		{
			id = "build_building",
			force_hand_tool = "hammer",
			locale_key = "task_unit_build"
		};
		pAsset = obj58;
		t = obj58;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobBuilder");
		t.addBeh(new BehCheckStillUnderConstruction());
		t.addBeh(new BehFindConstructionTile());
		t.addBeh(new BehGoToTileTarget());
		for (int num4 = 0; num4 < 5; num4++)
		{
			t.addBeh(new BehCheckStillUnderConstruction());
			t.addBeh(new BehLookAtBuildingTarget());
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, "event:/SFX/BUILDINGS/СonstructionBuildingGeneric", 0f, 40f, pCheckFlip: true, pLandIfHovering: true));
			t.addBeh(new BehBuildTarget());
		}
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj59 = new BehaviourTaskActor
		{
			id = "build_road",
			force_hand_tool = "hammer",
			locale_key = "task_unit_build"
		};
		pAsset = obj59;
		t = obj59;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobRoadBuilder");
		t.addBeh(new BehFindTile(TileFinderType.NewRoad)
		{
			null_check_city = true
		});
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0.2f));
		for (int num5 = 0; num5 < 3; num5++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, "event:/SFX/CIVILIZATIONS/BuildRoad", 1f, 40f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, "event:/SFX/CIVILIZATIONS/BuildRoad", 0f, 40f, pCheckFlip: true, pLandIfHovering: true));
		t.addBeh(new BehCityActorCreateRoad());
		t.addBeh(new BehRandomWait(1f, 2f));
		t.addBeh(new BehCheckNeeds(10));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj60 = new BehaviourTaskActor
		{
			id = "collect_fruits",
			force_hand_tool = "basket",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_collect_food"
		};
		pAsset = obj60;
		t = obj60;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobGathererBushes");
		t.addBeh(new BehCityActorFindBuilding("type_fruits"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num6 = 0; num6 < 4; num6++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/CIVILIZATIONS/CollectFruits"));
		}
		t.addBeh(new BehResourceGatheringAnimation(0f, "event:/SFX/CIVILIZATIONS/CollectFruits"));
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t);
		t.addBeh(new BehCheckNeeds(5));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj61 = new BehaviourTaskActor
		{
			id = "collect_honey",
			force_hand_tool = "basket",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_collect_honey"
		};
		pAsset = obj61;
		t = obj61;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobGathererHoney");
		t.addBeh(new BehCityActorFindBuilding("type_hive"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num7 = 0; num7 < 4; num7++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, null));
		}
		t.addBeh(new BehResourceGatheringAnimation(0f, null));
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t);
		t.addBeh(new BehCheckNeeds(3));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj62 = new BehaviourTaskActor
		{
			id = "claim_land",
			force_hand_tool = "flag",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_claim_land"
		};
		pAsset = obj62;
		t = obj62;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenLandClaimer");
		t.addBeh(new BehActorCheckZoneTarget());
		t.addBeh(new BehGoToTileTarget());
		for (int num8 = 0; num8 < 21; num8++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.5f, 0.5f));
			t.addBeh(new BehSpawnCityBorderEffect());
		}
		t.addBeh(new BehClaimZoneForCityActorBorder());
		t.addBeh(new BehSpawnCityBorderEffect(5));
		t.addBeh(new BehEndJob());
		BehaviourTaskActor obj63 = new BehaviourTaskActor
		{
			id = "collect_herbs",
			force_hand_tool = "basket",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_collect_herbs"
		};
		pAsset = obj63;
		t = obj63;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobGathererHerbs");
		t.addBeh(new BehCityActorFindBuilding("type_vegetation"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num9 = 0; num9 < 4; num9++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/CIVILIZATIONS/CollectHerbs"));
		}
		t.addBeh(new BehResourceGatheringAnimation(0f, "event:/SFX/CIVILIZATIONS/CollectHerbs"));
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t);
		t.addBeh(new BehCheckNeeds(4));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj64 = new BehaviourTaskActor
		{
			id = "chop_trees",
			force_hand_tool = "axe",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_chop"
		};
		pAsset = obj64;
		t = obj64;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobWoodcutter");
		t.addBeh(new BehCityActorFindBuilding("type_tree"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num10 = 0; num10 < 7; num10++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/CIVILIZATIONS/ChopTree"));
		}
		t.addBeh(new BehResourceGatheringAnimation(0f, "event:/SFX/CIVILIZATIONS/ChopTree"));
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t);
		BehaviourTaskActor obj65 = new BehaviourTaskActor
		{
			id = "mine_deposit",
			force_hand_tool = "pickaxe",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_mine"
		};
		pAsset = obj65;
		t = obj65;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobMinerDeposit");
		t.addBeh(new BehCityActorFindBuilding("type_mineral"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int num11 = 0; num11 < 6; num11++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/CIVILIZATIONS/MiningMineral"));
		}
		t.addBeh(new BehResourceGatheringAnimation(0f, "event:/SFX/CIVILIZATIONS/MiningMineral"));
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t);
		t.addBeh(new BehCheckNeeds(3));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj66 = new BehaviourTaskActor
		{
			id = "farmer_make_field",
			force_hand_tool = "hoe",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_farm"
		};
		pAsset = obj66;
		t = obj66;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobFarmer");
		t.addBeh(new BehFindTileForFarm());
		t.addBeh(new BehGoToTileTarget());
		for (int num12 = 0; num12 < 6; num12++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, "event:/SFX/CIVILIZATIONS/MakeFarmField", 1f, 40f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehMakeFarm());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj67 = new BehaviourTaskActor
		{
			id = "farmer_plant_crops",
			force_hand_tool = "hoe",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_farm"
		};
		pAsset = obj67;
		t = obj67;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobFarmer");
		t.addBeh(new BehFindFarmField());
		t.addBeh(new BehGoToTileTarget());
		for (int num13 = 0; num13 < 4; num13++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, "event:/SFX/CIVILIZATIONS/PlantCrops", 1f, 40f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehPlantCrops());
		t.addBeh(new BehRandomWait(1f, 2f));
		t.addBeh(new BehCheckNeeds(6));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj68 = new BehaviourTaskActor
		{
			id = "farmer_harvest",
			force_hand_tool = "hoe",
			locale_key = "task_unit_farm"
		};
		pAsset = obj68;
		t = obj68;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobFarmer");
		t.addBeh(new BehFindWheat());
		t.addBeh(new BehGoToBuildingTarget());
		for (int num14 = 0; num14 < 3; num14++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/CIVILIZATIONS/HarvestCrops"));
		}
		t.addBeh(new BehResourceGatheringAnimation(0f, "event:/SFX/CIVILIZATIONS/HarvestCrops"));
		t.addBeh(new BehExtractResourcesFromBuilding());
		addActionsForDeliverResources(t, pWheatStorage: true);
		t.addBeh(new BehCheckNeeds(6));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj69 = new BehaviourTaskActor
		{
			id = "farmer_fertilize_crops",
			force_hand_tool = "bucket",
			cancellable_by_reproduction = true,
			locale_key = "task_unit_farm"
		};
		pAsset = obj69;
		t = obj69;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobFarmer");
		t.addBeh(new BehCityActorFindStorage());
		t.addBeh(new BehGoToBuildingTarget());
		t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, null, 1f, 40f, pCheckFlip: true, pLandIfHovering: true));
		t.addBeh(new BehCityActorGetResourceFromStorage("fertilizer", 5));
		for (int num15 = 0; num15 < 5; num15++)
		{
			t.addBeh(new BehCityActorFindUngrownCrop());
			t.addBeh(new BehGoToBuildingTarget());
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, null, 0f, 40f, pCheckFlip: true, pLandIfHovering: true));
			t.addBeh(new BehThrowResourceAnimation("fertilizer"));
			t.addBeh(new BehWait());
			t.addBeh(new BehCityActorFertilizeCrop());
		}
		addActionsForDeliverResources(t);
		t.addBeh(new BehRandomWait(1f, 2f));
		t.addBeh(new BehCheckNeeds(6));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj70 = new BehaviourTaskActor
		{
			id = "farmer_random_move",
			force_hand_tool = "hoe",
			locale_key = "task_unit_farm"
		};
		pAsset = obj70;
		t = obj70;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobFarmer");
		t.addBeh(new BehFindRandomFarmTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 3f));
		BehaviourTaskActor obj71 = new BehaviourTaskActor
		{
			id = "store_resources",
			locale_key = "task_unit_store_resources"
		};
		pAsset = obj71;
		t = obj71;
		add(pAsset);
		t.setIcon("ui/Icons/iconCityInventory");
		addActionsForDeliverResources(t);
		BehaviourTaskActor obj72 = new BehaviourTaskActor
		{
			id = "mine",
			force_hand_tool = "pickaxe",
			locale_key = "task_unit_mine"
		};
		pAsset = obj72;
		t = obj72;
		add(pAsset);
		t.setIcon("ui/Icons/citizen_jobs/iconCitizenJobMiner");
		t.addBeh(new BehCityActorFindBuilding("type_mine"));
		t.addBeh(new BehGoToBuildingTarget());
		t.addBeh(new BehStayInBuildingTarget(10f, 15f));
		t.addBeh(new BehGetResourcesFromMine());
		t.addBeh(new BehExitBuilding());
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 2f));
		t.addBeh(new BehCheckNeeds(3));
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj73 = new BehaviourTaskActor
		{
			id = "try_to_return_to_home_city",
			flag_boat_related = true
		};
		pAsset = obj73;
		t = obj73;
		add(pAsset);
		t.setIcon("ui/Icons/iconHoused");
		t.addBeh(new BehTaxiCheck());
		t.addBeh(new BehRandomWait(1f));
		t.addBeh(new BehTaxiFindShipTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehTaxiEmbark());
		BehaviourTaskActor obj74 = new BehaviourTaskActor
		{
			id = "force_into_a_boat",
			flag_boat_related = true
		};
		pAsset = obj74;
		t = obj74;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehTaxiFindShipTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehTaxiEmbark());
		BehaviourTaskActor obj75 = new BehaviourTaskActor
		{
			id = "embark_into_boat",
			flag_boat_related = true
		};
		pAsset = obj75;
		t = obj75;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehTaxiEmbark());
		BehaviourTaskActor obj76 = new BehaviourTaskActor
		{
			id = "sit_inside_boat",
			flag_boat_related = true
		};
		pAsset = obj76;
		t = obj76;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehTaxiSitInside());
		t.addBeh(new BehTaxiSitInside());
		t.addBeh(new BehTaxiSitInside());
		BehaviourTaskActor obj77 = new BehaviourTaskActor
		{
			id = "wait",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_wait"
		};
		pAsset = obj77;
		t = obj77;
		add(pAsset);
		t.setIcon("ui/Icons/iconClock");
		t.addBeh(new BehRandomWait(0.5f, 1.3f));
		BehaviourTaskActor obj78 = new BehaviourTaskActor
		{
			id = "wait5",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_wait"
		};
		pAsset = obj78;
		t = obj78;
		add(pAsset);
		t.setIcon("ui/Icons/iconClock");
		t.addBeh(new BehRandomWait(1f, 5f));
		BehaviourTaskActor obj79 = new BehaviourTaskActor
		{
			id = "wait10",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_wait"
		};
		pAsset = obj79;
		t = obj79;
		add(pAsset);
		t.setIcon("ui/Icons/iconClock");
		t.addBeh(new BehRandomWait(1f, 10f));
		BehaviourTaskActor obj80 = new BehaviourTaskActor
		{
			id = "replenish_energy",
			cancellable_by_socialize = false,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_replenish_energy"
		};
		pAsset = obj80;
		t = obj80;
		add(pAsset);
		t.setIcon("ui/Icons/iconStamina");
		t.addBeh(new BehFindBuilding("type_well", pOnlyNonTargeted: false, pOnlyWithResources: false));
		t.addBeh(new BehGoToBuildingTarget());
		for (int num16 = 0; num16 < 5; num16++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, null, 1f, 20f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehReplenishEnergy());
		BehaviourTaskActor obj81 = new BehaviourTaskActor
		{
			id = "repair_equipment",
			cancellable_by_socialize = false,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_repair_equipment"
		};
		pAsset = obj81;
		t = obj81;
		add(pAsset);
		t.setIcon("ui/Icons/iconReforge");
		t.addBeh(new BehCheckCanRepairEquipment());
		t.addBeh(new BehFindBuilding("type_barracks", pOnlyNonTargeted: false, pOnlyWithResources: false));
		t.addBeh(new BehGoToBuildingTarget());
		for (int num17 = 0; num17 < 5; num17++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, null, 1f, 20f));
		}
		t.addBeh(new BehRepairEquipment());
	}

	private void initTasksThinkingReflectionHappiness()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "try_to_steal_money"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconThief");
		t.addBeh(new BehFindTargetToStealFrom());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pPathOnWater: false, pCheckCanAttackTarget: false, pCalibrateTargetPosition: true));
		for (int i = 0; i < 2; i++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Actor, null, 0.3f, 30f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehStealFromTarget());
		t.addBeh(new BehSetNextTask("run_away"));
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "reflection"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconBre");
		t.addBeh(new BehReflection());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "madness_random_emotion"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconMadness");
		t.addBeh(new BehMadnessRandomEmotion());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "happy_laughing"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconLaughing");
		t.addBeh(new BehTryFindTargetWithStatusNearby("laughing", "crying", "swearing", "singing"));
		t.addBeh(new BehGoToTileTarget());
		for (int j = 0; j < 4; j++)
		{
			t.addBeh(new BehActorAddStatus("laughing", 2f, pEffectOn: false));
			for (int k = 0; k < 3; k++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 0.2f, -20f, pCheckFlip: false));
			}
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehWait(0.3f));
		}
		t.addBeh(new BehAddHappiness("just_laughed"));
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "singing"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconSinging");
		t.addBeh(new BehTryFindTargetWithStatusNearby("laughing", "crying", "swearing", "singing"));
		t.addBeh(new BehGoToTileTarget());
		for (int l = 0; l < 5; l++)
		{
			t.addBeh(new BehJumpingAnimation(0f, 5f));
			t.addBeh(new BehActorAddStatus("singing", 3f, pEffectOn: false));
			for (int m = 0; m < 3; m++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 0.4f, -30f, pCheckFlip: false));
				t.addBeh(new BehActorReverseFlip());
				t.addBeh(new BehWait(0.1f));
			}
		}
		t.addBeh(new BehAddHappiness("just_sang"));
		t.addBeh(new BehFinishSinging());
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "swearing"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconSwearing");
		t.addBeh(new BehTryFindTargetWithStatusNearby("laughing", "crying", "swearing", "singing"));
		t.addBeh(new BehGoToTileTarget());
		for (int n = 0; n < 4; n++)
		{
			t.addBeh(new BehActorAddStatus("swearing", 2f, pEffectOn: false));
			for (int num = 0; num < 3; num++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 0.2f, -20f, pCheckFlip: false));
			}
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehWait(0.3f));
		}
		t.addBeh(new BehAddHappiness("just_swore"));
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "crying"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconCrying");
		t.addBeh(new BehTryFindTargetWithStatusNearby("laughing", "crying", "swearing", "singing"));
		t.addBeh(new BehGoToTileTarget());
		for (int num2 = 0; num2 < 4; num2++)
		{
			t.addBeh(new BehActorAddStatus("crying", 2f, pEffectOn: false));
			for (int num3 = 0; num3 < 3; num3++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 0.2f, -20f, pCheckFlip: false));
			}
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehWait(0.3f));
		}
		t.addBeh(new BehAddHappiness("just_cried"));
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "possessed_following"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/iconPossessed");
		t.addBeh(new BehTryFindTargetWithStatusNearby("possessed"));
		t.addBeh(new BehCopyAggro());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehFindRandomNeighbourTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0.5f, 2f));
		t.addBeh(new BehRandomSocializeTopic(1.5f, 3f, 0.1f));
		BehaviourTaskActor obj9 = new BehaviourTaskActor
		{
			id = "start_tantrum"
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.setIcon("ui/Icons/iconTantrum");
		for (int num4 = 0; num4 < 6; num4++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.1f, 0.1f));
		}
		t.addBeh(new BehActorAddStatus("tantrum", 60f));
		BehaviourTaskActor obj10 = new BehaviourTaskActor
		{
			id = "do_tantrum"
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.setIcon("ui/Icons/iconTantrum");
		t.addBeh(new BehFindTantrumTarget());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.RaycastWithAttackRange, pPathOnWater: false, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehAddAggroForBehTarget());
		BehaviourTaskActor obj11 = new BehaviourTaskActor
		{
			id = "punch_a_tree"
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.setIcon("ui/Icons/iconRage");
		t.addBeh(new BehFindBuilding("type_tree", pOnlyNonTargeted: true, pOnlyWithResources: false));
		t.addBeh(new BehGoToBuildingTarget());
		for (int num5 = 0; num5 < 2; num5++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/HIT/HitWood"));
		}
		t.addBeh(new BehGetDamaged(1, AttackType.Gravity));
		t.addBeh(new BehActorAddStatus("crying", 5f, pEffectOn: false, pAddActionTimer: true));
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj12 = new BehaviourTaskActor
		{
			id = "punch_a_building"
		};
		pAsset = obj12;
		t = obj12;
		add(pAsset);
		t.setIcon("ui/Icons/iconRage");
		t.addBeh(new BehFindBuilding("type_house", pOnlyNonTargeted: false, pOnlyWithResources: false));
		t.addBeh(new BehGoToBuildingTarget());
		for (int num6 = 0; num6 < 2; num6++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/HIT/HitGeneric"));
		}
		t.addBeh(new BehGetDamaged(1, AttackType.Gravity));
		t.addBeh(new BehActorAddStatus("crying", 5f, pEffectOn: false, pAddActionTimer: true));
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj13 = new BehaviourTaskActor
		{
			id = "start_fire"
		};
		pAsset = obj13;
		t = obj13;
		add(pAsset);
		t.setIcon("ui/Icons/iconFire");
		t.addBeh(new BehWait());
	}

	private void initTasksClanLeader()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "kill_unruly_clan_members"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/clan_traits/clan_trait_deathbound");
		t.addBeh(new BehClanChiefCheckMembersToKill());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "banish_unruly_clan_members"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/clan_traits/clan_trait_blood_pact");
		t.addBeh(new BehClanChiefCheckMembersToBanish());
	}

	private void initTasksPoop()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "try_to_poop"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconPoop");
		t.addBeh(new BehDecideWhereToPoop());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "try_to_launch_fireworks"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconFireworks");
		t.addBeh(new BehTryFindTargetWithStatusNearby("laughing", "crying", "swearing", "singing"));
		t.addBeh(new BehGoToTileTarget());
		for (int i = 0; i < 1; i++)
		{
			t.addBeh(new BehActorAddStatus("laughing", 2f, pEffectOn: false));
			for (int j = 0; j < 3; j++)
			{
				t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Nothing, null, 0.2f, -20f, pCheckFlip: false, pLandIfHovering: true));
			}
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehWait(0.3f));
		}
		t.addBeh(new BehLaunchFireworks());
		t.addBeh(new BehAddHappiness("just_laughed"));
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "poop_inside"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconPoop");
		t.addBeh(new BehBuildingTargetHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget(10f, 60f));
		t.addBeh(new BehPoopInside());
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "poop_outside"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconPoop");
		t.addBeh(new BehFindTile(TileFinderType.FreeTile));
		t.addBeh(new BehGoToTileTarget());
		for (int k = 0; k < 4; k++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.5f, 0.5f));
		}
		t.addBeh(new BehPoopOutside());
	}

	private void initTasksSleep()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "decide_where_to_sleep"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconSleep");
		t.addBeh(new BehDecideWhereToSleep());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "sleep_inside"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconHoused");
		t.addBeh(new BehBuildingTargetHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget());
		t.addBeh(new BehTrySleep());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "sleep_outside"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconHomeless");
		t.addBeh(new BehFindTile(TileFinderType.FreeTile));
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0f, 2f));
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehRandomWait(0f, 2f));
		t.addBeh(new BehFindRandomNeighbourTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0f, 2f));
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehRandomWait(0f, 2f));
		t.addBeh(new BehTrySleep(pSleepOutside: true));
	}

	private void initTasksKings()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "king_check_new_city_foundation"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehKingCheckNewCityFoundation());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "king_change_kingdom_language"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconLanguage");
		t.addBeh(new BehChangeKingdomLanguage());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "king_change_kingdom_culture"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconCulture");
		t.addBeh(new BehChangeKingdomCulture());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "king_change_kingdom_religion"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconReligion");
		t.addBeh(new BehChangeKingdomReligion());
	}

	private void initTasksWarriors()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "warrior_random_move",
			locale_key = "task_unit_move"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "warrior_army_captain_idle_walking_city",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_walk"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehCityActorGetRandomBorderTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 6f));
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "warrior_army_captain_waiting",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_walk"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehCityActorGetRandomBorderTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(10f, 20f));
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "check_warrior_limit"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconWar");
		t.addBeh(new BehCheckCityActorWarriorLimit());
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "warrior_try_join_army_group"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconWar");
		t.addBeh(new BehCheckCityActorArmyGroup());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "warrior_army_leader_move_random",
			speed_multiplier = 0.8f
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(10f, 20f));
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "warrior_army_leader_move_to_attack_target",
			speed_multiplier = 0.8f,
			cancellable_by_socialize = false,
			cancellable_by_reproduction = false
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowAttackTarget");
		t.addTaskVerifier(new BehVerifierAttackZone());
		t.addBeh(new BehCityActorCheckAttack());
		t.addBeh(new BehGoToTileTarget
		{
			limit_pathfinding_regions = 6
		});
		t.addBeh(new BehWarriorCaptainWait());
		t.addBeh(new BehRestartTask());
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "warrior_army_follow_leader",
			speed_multiplier = 1.3f,
			cancellable_by_socialize = false,
			cancellable_by_reproduction = false
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/iconLoyalty");
		t.addBeh(new BehFindTileNearbyGroupLeader());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj9 = new BehaviourTaskActor
		{
			id = "check_warrior_transport",
			flag_boat_related = true,
			cancellable_by_socialize = false,
			cancellable_by_reproduction = false
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehCityActorWarriorTaxiCheck());
		t.addBeh(new BehRandomWait(1f, 5f));
		t.addBeh(new BehTaxiFindShipTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehTaxiEmbark());
		BehaviourTaskActor obj10 = new BehaviourTaskActor
		{
			id = "warrior_train_with_dummy"
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.setIcon("ui/Icons/iconWarfare");
		t.addBeh(new BehCityActorFindBuilding("type_training_dummies"));
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehLookAtBuildingTarget());
		for (int i = 0; i < 10; i++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, "event:/SFX/BUILDINGS/DestroyBuildingWood", 0f, 40f, pCheckFlip: true, pLandIfHovering: true));
			t.addBeh(new BehActorAddExperience(0, 2));
			t.addBeh(new BehRandomWait(0f, 2f));
			t.addBeh(new BehDealDamageToTargetBuilding(0.05f, 0.15f));
		}
		t.addBeh(new BehActorTryToAddRandomCombatSkill());
		t.addBeh(new BehRandomWait(3f, 6f));
	}

	private void initTasksLeaders()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "leader_change_city_language"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconLanguage");
		t.addBeh(new BehChangeCityActorLanguage());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "leader_change_city_culture"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconCulture");
		t.addBeh(new BehChangeCityActorCulture());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "leader_change_city_religion"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconReligion");
		t.addBeh(new BehChangeCityActorReligion());
	}

	private void initTasksStatusRelated()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "strange_urge_finish",
			locale_key = "task_strange_urge_finish"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconStrangeUrge");
		for (int i = 0; i < 6; i++)
		{
			t.addBeh(new BehFindRandomNeighbourTile());
			t.addBeh(new BehGoToTileTarget());
			t.addBeh(new BehRandomWait(0.5f, 5f));
		}
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "status_confused",
			locale_key = "task_unit_confused"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconConfused");
		for (int j = 0; j < 6; j++)
		{
			t.addBeh(new BehFindRandomNeighbourTile());
			t.addBeh(new BehGoToTileTarget());
			t.addBeh(new BehRandomWait(0f, 0.01f));
		}
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "status_soul_harvested",
			locale_key = "status_title_soul_harvested"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconSoulHarvested");
		t.addBeh(new BehStartShake());
		for (int k = 0; k < 12; k++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.1f, 0.1f));
		}
		t.addBeh(new BehCheckSoulBorneReproduction());
	}

	private void initTasksMobs()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "diet_tiles",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_geophagy");
		t.addBeh(new BehFindTileForEating());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true,
			walk_on_blocks = true
		});
		for (int i = 0; i < 10; i++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 1f, 20f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehConsumeTargetTile());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "diet_wood",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_xylophagy");
		t.addBeh(new BehFindBuilding("type_tree", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int j = 0; j < 4; j++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehConsumeTargetBuilding());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "diet_minerals",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_lithotroph");
		t.addBeh(new BehFindBuilding("type_mineral", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int k = 0; k < 4; k++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehConsumeTargetBuilding());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "diet_fruits",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_frugivore");
		t.addBeh(new BehFindBuilding("type_fruits", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int l = 0; l < 4; l++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehConsumeTargetBuilding());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "diet_vegetation",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_folivore");
		t.addBeh(new BehFindBuilding("type_vegetation", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int m = 0; m < 4; m++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehConsumeTargetBuilding());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "diet_flowers",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_florivore");
		t.addBeh(new BehFindBuilding("type_flower", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int n = 0; n < 4; n++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehConsumeTargetBuilding());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "diet_nectar",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_florivore");
		t.addBeh(new BehFindBuilding("type_flower", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int num = 0; num < 4; num++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehNectarNectarFromFlower());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "diet_crops",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_granivore");
		t.addBeh(new BehFindBuilding("type_crops", pOnlyNonTargeted: false, pOnlyWithResources: true));
		t.addBeh(new BehGoToBuildingTarget());
		for (int num2 = 0; num2 < 4; num2++)
		{
			t.addBeh(new BehResourceGatheringAnimation(1f, "event:/SFX/NATURE/AnimalEatPlant"));
		}
		t.addBeh(new BehConsumeTargetBuilding());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj9 = new BehaviourTaskActor
		{
			id = "diet_grass",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_graminivore");
		t.addBeh(new BehFindTile(TileFinderType.Grass));
		t.addBeh(new BehGoToTileTarget());
		for (int num3 = 0; num3 < 4; num3++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 1f, 20f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehConsumeGrass());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj10 = new BehaviourTaskActor
		{
			id = "diet_meat",
			cancellable_by_reproduction = true,
			ignore_fight_check = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_carnivore");
		t.addBeh(new BehFindMeatSource());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: false, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehConsumeActorTarget());
		BehaviourTaskActor obj11 = new BehaviourTaskActor
		{
			id = "diet_blood",
			cancellable_by_reproduction = true,
			ignore_fight_check = true,
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_hematophagy");
		t.addBeh(new BehFindMeatSource(MeatTargetType.Meat, pCheckForFactions: false));
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: false, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehConsumeActorsBloodTarget());
		BehaviourTaskActor obj12 = new BehaviourTaskActor
		{
			id = "diet_meat_insect",
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj12;
		t = obj12;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_insectivore");
		t.addBeh(new BehFindMeatInsectSource());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: false, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehConsumeActorTarget());
		BehaviourTaskActor obj13 = new BehaviourTaskActor
		{
			id = "diet_same_species",
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj13;
		t = obj13;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_cannibalism");
		t.addBeh(new BehFindMeatSameSpeciesSource(pCheckForFactions: false));
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameTile, pPathOnWater: false, pCheckCanAttackTarget: true, pCalibrateTargetPosition: true));
		t.addBeh(new BehConsumeActorTarget());
		BehaviourTaskActor obj14 = new BehaviourTaskActor
		{
			id = "diet_algae",
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj14;
		t = obj14;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_algivore");
		t.addBeh(new BehFindTile(TileFinderType.Water));
		t.addBeh(new BehGoOrSwimToTileTarget());
		for (int num4 = 0; num4 < 4; num4++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 1f, 20f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehReplenishNutrition());
		BehaviourTaskActor obj15 = new BehaviourTaskActor
		{
			id = "diet_fish",
			locale_key = "task_unit_eat",
			diet = true
		};
		pAsset = obj15;
		t = obj15;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_diet_piscivore");
		t.addBeh(new BehFindTile(TileFinderType.Water));
		t.addBeh(new BehGoOrSwimToTileTarget());
		for (int num5 = 0; num5 < 4; num5++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 1f, 20f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehReplenishNutrition());
		BehaviourTaskActor obj16 = new BehaviourTaskActor
		{
			id = "family_alpha_move",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj16;
		t = obj16;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconStrong");
		t.addBeh(new BehFamilyAlphaMove());
		t.addBeh(new BehGoToTileTarget());
		BehaviourTaskActor obj17 = new BehaviourTaskActor
		{
			id = "family_group_follow",
			cancellable_by_socialize = true,
			cancellable_by_reproduction = true,
			locale_key = "task_unit_follow_family"
		};
		pAsset = obj17;
		t = obj17;
		add(pAsset);
		t.setIcon("ui/Icons/iconFamiliesZones");
		t.addBeh(new BehFamilyFollowAlpha());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameRegion));
		BehaviourTaskActor obj18 = new BehaviourTaskActor
		{
			id = "family_check_existence"
		};
		pAsset = obj18;
		t = obj18;
		add(pAsset);
		t.setIcon("ui/Icons/iconFamiliesZones");
		t.addBeh(new BehFamilyCheckMembers());
		BehaviourTaskActor obj19 = new BehaviourTaskActor
		{
			id = "family_group_leave"
		};
		pAsset = obj19;
		t = obj19;
		add(pAsset);
		t.setIcon("ui/Icons/iconFamiliesZones");
		t.addBeh(new BehFamilyGroupLeave());
		BehaviourTaskActor obj20 = new BehaviourTaskActor
		{
			id = "family_group_join_or_new_herd"
		};
		pAsset = obj20;
		t = obj20;
		add(pAsset);
		t.setIcon("ui/Icons/iconFamiliesZones");
		t.addBeh(new BehFamilyGroupJoin());
		t.addBeh(new BehFamilyGroupNew());
		BehaviourTaskActor obj21 = new BehaviourTaskActor
		{
			id = "attack_golden_brain"
		};
		pAsset = obj21;
		t = obj21;
		add(pAsset);
		t.setIcon("ui/Icons/iconGoldBrain");
		t.addBeh(new BehFindGoldenBrain());
		t.addBeh(new BehGoToBuildingTarget());
		BehaviourTaskActor obj22 = new BehaviourTaskActor
		{
			id = "follow_desire_target",
			locale_key = "task_unit_weird_desire"
		};
		pAsset = obj22;
		t = obj22;
		add(pAsset);
		t.setIcon("ui/Icons/iconGoldBrain");
		t.addBeh(new BehFindDesireWaypoint());
		t.addBeh(new BehGoToBuildingTarget());
		BehaviourTaskActor obj23 = new BehaviourTaskActor
		{
			id = "crab_eat",
			diet = true
		};
		pAsset = obj23;
		t = obj23;
		add(pAsset);
		t.setIcon("ui/Icons/iconCrab");
		t.addBeh(new BehAnimalCheckHungry());
		t.addBeh(new BehRandomWait(2f, 5f));
		t.addBeh(new BehFindTileBeach());
		t.addBeh(new BehGoOrSwimToTileTarget());
		for (int num6 = 0; num6 < 4; num6++)
		{
			t.addBeh(new BehAngleAnimation(AngleAnimationTarget.Tile, string.Empty, 1f, 20f, pCheckFlip: true, pLandIfHovering: true));
		}
		t.addBeh(new BehReplenishNutrition());
		BehaviourTaskActor obj24 = new BehaviourTaskActor
		{
			id = "crab_danger_check"
		};
		pAsset = obj24;
		t = obj24;
		add(pAsset);
		t.setIcon("ui/Icons/iconBloodRain");
		t.addBeh(new BehCheckIfOnGround());
		t.addBeh(new BehActiveCrabDangerCheck());
		BehaviourTaskActor obj25 = new BehaviourTaskActor
		{
			id = "crab_burrow"
		};
		pAsset = obj25;
		t = obj25;
		add(pAsset);
		t.setIcon("ui/Icons/iconCrab");
		t.addBeh(new BehCheckIfOnGround());
		t.addBeh(new BehFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehCrabBurrow());
		BehaviourTaskActor obj26 = new BehaviourTaskActor
		{
			id = "make_skeleton"
		};
		pAsset = obj26;
		t = obj26;
		add(pAsset);
		t.setIcon("ui/Icons/iconSkeleton");
		t.addBeh(new BehMagicMakeSkeleton());
		t.addBeh(new BehRandomWait(3f, 3f));
		BehaviourTaskActor obj27 = new BehaviourTaskActor
		{
			id = "skeleton_move",
			locale_key = "task_unit_move"
		};
		pAsset = obj27;
		t = obj27;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehSkeletonFindTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 6f));
		BehaviourTaskActor obj28 = new BehaviourTaskActor
		{
			id = "spawn_fertilizer"
		};
		pAsset = obj28;
		t = obj28;
		add(pAsset);
		t.setIcon("ui/Icons/iconFertilizerPlants");
		t.addBeh(new BehSpawnTreeFertilizer());
		t.addBeh(new BehRandomWait(1f, 3f));
		BehaviourTaskActor obj29 = new BehaviourTaskActor
		{
			id = "check_cure"
		};
		pAsset = obj29;
		t = obj29;
		add(pAsset);
		t.setIcon("ui/Icons/iconHealth");
		t.addBeh(new BehCheckCure());
		t.addBeh(new BehRandomWait(1f, 3f));
		BehaviourTaskActor obj30 = new BehaviourTaskActor
		{
			id = "burn_tumors"
		};
		pAsset = obj30;
		t = obj30;
		add(pAsset);
		t.setIcon("ui/Icons/iconFire");
		t.addBeh(new BehBurnTumorTiles());
		t.addBeh(new BehRandomWait(1f, 3f));
		BehaviourTaskActor obj31 = new BehaviourTaskActor
		{
			id = "check_heal"
		};
		pAsset = obj31;
		t = obj31;
		add(pAsset);
		t.setIcon("ui/Icons/iconHealth");
		t.addBeh(new BehHeal());
		t.addBeh(new BehRandomWait(1f, 3f));
		BehaviourTaskActor obj32 = new BehaviourTaskActor
		{
			id = "random_teleport"
		};
		pAsset = obj32;
		t = obj32;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconMageSlayer");
		t.addBeh(new BehRandomTeleport());
		t.addBeh(new BehRandomWait(0.5f));
		BehaviourTaskActor obj33 = new BehaviourTaskActor
		{
			id = "teleport_back_home"
		};
		pAsset = obj33;
		t = obj33;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconMageSlayer");
		t.addBeh(new BehTeleportHome());
		t.addBeh(new BehRandomWait(0.5f));
		BehaviourTaskActor obj34 = new BehaviourTaskActor
		{
			id = "run_to_water_when_on_fire",
			locale_key = "task_unit_run_to_water"
		};
		pAsset = obj34;
		t = obj34;
		add(pAsset);
		t.setIcon("ui/Icons/iconFire");
		t.addBeh(new BehShortRandomMove());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehShortRandomMove());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehShortRandomMove());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehFindTileWhenOnFire());
		t.addBeh(new BehGoOrSwimToTileTarget());
		BehaviourTaskActor obj35 = new BehaviourTaskActor
		{
			id = "short_move",
			cancellable_by_socialize = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj35;
		t = obj35;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehShortRandomMove());
		t.addBeh(new BehGoToTileTarget());
	}

	private void initTasksFingers()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "godfinger_find_target"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerSetFlying(pFlying: true));
		t.addBeh(new BehFingerFindTarget());
		t.addBeh(new BehFingerGoTowardsTileTarget(5));
		t.addBeh(new BehFingerSetFlying(pFlying: true, 2f));
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehSetNextTask("godfinger_draw"));
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "godfinger_draw"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerCheckCanDraw());
		t.addBeh(new BehFingerSetFlying(pFlying: false));
		t.addBeh(new BehFingerFindCloseTile());
		t.addBeh(new BehFingerWaitForFlying());
		t.addBeh(new BehFingerDrawToTileTarget());
		t.addBeh(new BehSetNextTask("godfinger_find_target"));
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "godfinger_move"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerSetFlying(pFlying: true));
		t.addBeh(new BehRandomWait(0.1f, 0.4f));
		t.addBeh(new BehFingerFindRandomTile());
		t.addBeh(new BehFingerGoTowardsTileTarget());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "godfinger_random_fun_move"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerSetFlying(pFlying: true));
		for (int i = 0; i < 6; i++)
		{
			t.addBeh(new BehFingerFindRandomTile(5));
			t.addBeh(new BehFingerGoTowardsTileTarget(5));
			t.addBeh(new BehRandomWait(0f, 0.01f));
		}
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "godfinger_circle_move"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerSetFlying(pFlying: true));
		t.addBeh(new BehRandomWait(0.1f, 0.4f));
		t.addBeh(new BehFingerGoToCircleTarget(25, 75));
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "godfinger_circle_move_small"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerSetFlying(pFlying: true));
		t.addBeh(new BehRandomWait(0.1f, 0.4f));
		t.addBeh(new BehFingerGoToCircleTarget(5, 15));
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "godfinger_circle_move_big"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconGodFinger");
		t.addBeh(new BehFingerSetFlying(pFlying: true));
		t.addBeh(new BehRandomWait(0.1f, 0.4f));
		t.addBeh(new BehFingerGoToCircleTarget(75, 150));
	}

	private void initTasksDragons()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "dragon_fly",
			locale_key = "task_unit_dragon_fly"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconDragon");
		t.addBeh(new BehDragonSetAnimation(DragonState.Fly, pLooped: true, pForceRestart: false));
		t.addBeh(new BehActorSetFlying(pFlying: true));
		t.addBeh(new BehDragonSleepy());
		t.addBeh(new BehDragonCheckAttackTargetAlive());
		t.addBeh(new BehDragonZombieFindGoldenBrain());
		t.addBeh(new BehDragonCheckAttackTile());
		t.addBeh(new BehDragonCheckAttackCity());
		t.addBeh(new BehDragonFindRandomTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRepeatTaskChance(0.7f));
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "dragon_sleep",
			locale_key = "task_unit_dragon_sleep"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconSleep");
		t.addBeh(new BehDragonSetAnimation(DragonState.SleepStart, pLooped: false));
		t.addBeh(new BehActorSetFlip(pFlip: false));
		t.addBeh(new BehActorSetFlying(pFlying: false));
		t.addBeh(new BehDragonFinishAnimation());
		t.addBeh(new BehDragonSetAnimation(DragonState.SleepLoop));
		t.addBeh(new BehDragonSleep());
		t.addBeh(new BehDragonFinishAnimation());
		t.addBeh(new BehSetNextTask("dragon_wakeup"));
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "dragon_wakeup",
			locale_key = "task_unit_dragon_sleep"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconDragon");
		t.addBeh(new BehDragonSetAnimation(DragonState.SleepUp, pLooped: false));
		t.addBeh(new BehActorSetFlip(pFlip: false));
		t.addBeh(new BehDragonFinishAnimation());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "dragon_slide",
			locale_key = "task_unit_dragon_attack"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconFire");
		t.addBeh(new BehDragonSetAnimation(DragonState.Slide, pLooped: false));
		t.addBeh(new BehActorSetFlying(pFlying: true));
		t.addBeh(new BehDragonSleepy());
		t.addBeh(new BehActorAddStatus("invincible", 2f));
		t.addBeh(new BehActorSetBool("justSlid", pBoolValue: true));
		t.addBeh(new BehDragonSlide());
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "dragon_land",
			locale_key = "task_unit_dragon_land"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconDragon");
		t.addBeh(new BehDragonSetAnimation(DragonState.Landing, pLooped: false));
		t.addBeh(new BehDragonSleepy());
		t.addBeh(new BehDragonCheckAttackTargetAlive());
		t.addBeh(new BehDragonCheckOverTargetCity());
		t.addBeh(new BehDragonFinishAnimation());
		t.addBeh(new BehActorSetFlying(pFlying: false));
		t.addBeh(new BehDragonCantLand("dragon_up"));
		t.addBeh(new BehDragonLanded());
		t.addBeh(new BehDragonCheckOverTargetActor());
		t.addBeh(new BehDragonCheckOverTargetCity());
		t.addBeh(new BehActorSetInt("justLanded", 2));
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "dragon_land_attack",
			locale_key = "task_unit_dragon_attack"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconFire");
		t.addBeh(new BehDragonSetAnimation(DragonState.LandAttack, pLooped: false));
		t.addBeh(new BehDragonSleepy());
		t.addBeh(new BehActorSetFlying(pFlying: false));
		t.addBeh(new BehDragonLandAttack());
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "dragon_up",
			locale_key = "task_unit_dragon_fly"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconDragon");
		t.addBeh(new BehDragonSetAnimation(DragonState.Up, pLooped: false));
		t.addBeh(new BehDragonFlyUp());
		t.addBeh(new BehDragonSleepy());
		t.addBeh(new BehActorSetFlying(pFlying: true));
		t.addBeh(new BehDragonFinishAnimation());
		t.addBeh(new BehActorSetBool("justUp", pBoolValue: true));
		t.addBeh(new BehActorCheckBool("justGotHit", "dragon_fly"));
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "dragon_idle",
			locale_key = "task_unit_dragon_normal"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/iconDragon");
		t.addBeh(new BehDragonSetAnimation(DragonState.Idle));
		t.addBeh(new BehActorSetFlying(pFlying: false));
		t.addBeh(new BehDragonIdle());
		t.addBeh(new BehDragonFinishAnimation());
	}

	private void initTasksUFOs()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "ufo_idle"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconUFO");
		t.addBeh(new BehSetActorSpeed(20f));
		t.addBeh(new BehUFOBeam());
		t.addBeh(new BehUFOCheckExplore());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "ufo_hit"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconUFO");
		t.addBeh(new BehUFOSelectTarget());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "ufo_flee"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/actor_traits/iconAgile");
		t.addBeh(new BehSetActorSpeed(100f));
		t.addBeh(new BehUFOBeam());
		t.addBeh(new BehGetRandomZoneTile());
		t.addBeh(new BehGoToTileTarget());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "ufo_fly"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconUFO");
		t.addBeh(new BehSetActorSpeed(20f));
		t.addBeh(new BehUFOBeam());
		t.addBeh(new BehUFOFindTarget());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehUFOCheckAttackCity());
		t.addBeh(new BehUFOExplore());
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "ufo_explore"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconInspectZoneToggle");
		t.addBeh(new BehSetActorSpeed(10f));
		t.addBeh(new BehUFOBeam());
		t.addBeh(new BehGetRandomZoneTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehUFOCheckExplore());
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "ufo_chase"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconUFO");
		t.addBeh(new BehSetActorSpeed(50f));
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehSetNextTask("ufo_attack"));
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "ufo_attack"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowAttackTarget");
		t.addBeh(new BehUFOBeam(pEnabled: true));
	}

	private void initTasksBoats()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "boat_check_existence"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehBoatCheckExistence());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "boat_check_limits",
			locale_key = "task_unit_move"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehBoatCheckLimit());
		t.addBeh(new BehBoatCheckHomeDocks());
		t.addBeh(new BehBoatSetHomeDockTarget());
		t.addBeh(new BehBoatFindTileInDock());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0f, 3f));
		t.addBeh(new BehStayInBuildingTarget(2f, 4f));
		t.addBeh(new BehBoatRemoveIfLimit());
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "boat_idle",
			locale_key = "task_unit_nothing"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconClock");
		t.addBeh(new BehBoatDamageCheck());
		t.addBeh(new BehBoatFindOceanNeutralTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 10f));
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "boat_danger_check",
			locale_key = "task_unit_flee"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehCheckIfInLiquid());
		t.addBeh(new BehBoatDangerCheck());
		t.addBeh(new BehBoatFindWaterTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(3f, 10f));
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "boat_transport_check"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehRandomWait());
		t.addBeh(new BehBoatTransportCheck());
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "boat_transport_check_taxi"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconBoat");
		t.addBeh(new BehBoatFindRequest());
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "boat_transport_go_load",
			locale_key = "task_unit_boat_load_units"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconCityInventory");
		t.addBeh(new BehBoatTransportFindTilePickUp());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait());
		t.addBeh(new BehBoatTransportDoLoading());
		t.addBeh(new BehSetNextTask("boat_transport_go_unload", pClean: false));
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "boat_transport_go_unload",
			locale_key = "task_unit_boat_unload_units"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/iconCityInventory");
		t.addBeh(new BehRandomWait());
		t.addBeh(new BehBoatTransportFindTileUnload());
		t.addBeh(new BehGoToTileTarget
		{
			walk_on_water = true
		});
		t.addBeh(new BehRandomWait());
		t.addBeh(new BehBoatTransportUnloadUnits());
		t.addBeh(new BehRandomWait(3f, 10f));
		t.addBeh(new BehEndJob());
		BehaviourTaskActor obj9 = new BehaviourTaskActor
		{
			id = "boat_trading",
			locale_key = "task_unit_boat_trade"
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.setIcon("ui/Icons/iconMoney");
		t.addBeh(new BehBoatCheckHomeDocks());
		t.addBeh(new BehBoatFindTargetForTrade());
		t.addBeh(new BehBoatFindTileInDock());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehStayInBuildingTarget(2f, 5f));
		t.addBeh(new BehBoatMakeTrade());
		t.addBeh(new BehExitBuilding());
		t.addBeh(new BehSetNextTask("boat_return_to_dock"));
		BehaviourTaskActor obj10 = new BehaviourTaskActor
		{
			id = "boat_fishing",
			locale_key = "task_unit_boat_catch_fish"
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.setIcon("ui/Icons/iconResFish");
		t.addBeh(new BehBoatCheckHomeDocks());
		t.addBeh(new BehBoatFindWaterTile());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait());
		t.addBeh(new BehBoatFishing());
		t.addBeh(new BehRandomWait(5f, 10f));
		t.addBeh(new BehBoatCollectFish());
		t.addBeh(new BehRandomWait(1f, 2f));
		t.addBeh(new BehBoatCheckFishingRepeat());
		BehaviourTaskActor obj11 = new BehaviourTaskActor
		{
			id = "boat_return_to_dock",
			locale_key = "task_unit_return_to_dock"
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowDestination");
		t.addBeh(new BehBoatCheckHomeDocks());
		t.addBeh(new BehBoatSetHomeDockTarget());
		t.addBeh(new BehBoatFindTileInDock());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehRandomWait(0f, 3f));
		t.addBeh(new BehStayInBuildingTarget(2f, 4f));
		t.addBeh(new BehUnloadResources());
		t.addBeh(new BehRepairInDock());
		t.addBeh(new BehExitBuilding());
		t.addBeh(new BehEndJob());
	}

	private void initTasksReproductionAsexual()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "asexual_reproduction_budding",
			locale_key = "task_unit_reproduce"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_reproduction_budding");
		for (int i = 0; i < 4; i++)
		{
			t.addBeh(new BehStartShake(0.1f, 0.1f));
			t.addBeh(new BehActorReverseFlip());
		}
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehCheckBuddingReproduction());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "asexual_reproduction_divine",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_reproduction_divine");
		for (int j = 0; j < 4; j++)
		{
			t.addBeh(new BehStartShake(0.1f, 0.1f));
			t.addBeh(new BehActorReverseFlip());
		}
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehCheckDivineReproduction());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "asexual_reproduction_spores",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_reproduction_spores");
		t.addBeh(new BehStartShake());
		for (int k = 0; k < 4; k++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.1f, 0.1f));
		}
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehCheckSporeReproduction());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "asexual_reproduction_fission",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_reproduction_fission");
		for (int l = 0; l < 4; l++)
		{
			t.addBeh(new BehStartShake(0.1f, 0.1f));
			t.addBeh(new BehActorReverseFlip());
		}
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehCheckFissionReproduction());
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "asexual_reproduction_vegetative",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_reproduction_vegetative");
		for (int m = 0; m < 4; m++)
		{
			t.addBeh(new BehStartShake(0.1f, 0.1f));
			t.addBeh(new BehActorReverseFlip());
		}
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehFindTile(TileFinderType.Dirt));
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehCheckVegetativeReproduction());
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "asexual_reproduction_parthenogenesis",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_reproduction_parthenogenesis");
		for (int n = 0; n < 4; n++)
		{
			t.addBeh(new BehStartShake(0.1f, 0.1f));
			t.addBeh(new BehActorReverseFlip());
		}
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehCheckParthenogenesisReproduction());
	}

	private void initTasksReproductionSexual()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "check_lover_city"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconCity");
		t.addBeh(new BehCheckSameCityActorLover());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "find_lover"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconArrowLover");
		t.addBeh(new BehFindLover());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_try",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		t.addBeh(new BehCheckReproductionBasics());
		t.addBeh(new BehSexualReproductionTry());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_check_outside",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		t.addBeh(new BehCheckSexualReproductionOutside());
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_inside",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		t.addBeh(new BehCheckSexualReproductionCiv());
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_outside",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		t.addBeh(new BehGoToActorTarget());
		t.addBeh(new BehRandomWait(0.5f));
		t.addBeh(new BehAnimalBreedingTime());
		t.addBeh(new BehRandomWait(0.5f));
		t.addBeh(new BehAnimalBreedingTime());
		t.addBeh(new BehRandomWait(0.5f));
		t.addBeh(new BehAnimalBreedingTime());
		t.addBeh(new BehRandomWait(0.5f));
		t.addBeh(new BehCheckForBabiesFromSexualReproduction());
		t.addBeh(new BehRandomWait(1f, 3f));
		BehaviourTaskActor obj7 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_civ_go",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		t.addBeh(new BehBuildingTargetLoverHome());
		t.addBeh(new BehGetTargetBuildingMainTile());
		t.addBeh(new BehGoToTileTarget());
		for (int i = 0; i < 6; i++)
		{
			t.addBeh(new BehRandomWait(1f, 2f));
			t.addBeh(new BehCheckForLover());
			t.addBeh(new BehRandomWait(1f, 2f));
		}
		BehaviourTaskActor obj8 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_civ_action",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		for (int j = 0; j < 5; j++)
		{
			t.addBeh(new BehStayInBuildingTarget(1f, 2f));
			t.addBeh(new BehShakeBuilding());
			t.addBeh(new BehSpawnHeartsFromBuilding());
		}
		t.addBeh(new BehCheckForBabiesFromSexualReproduction());
		t.addBeh(new BehExitBuilding());
		BehaviourTaskActor obj9 = new BehaviourTaskActor
		{
			id = "sexual_reproduction_civ_wait",
			locale_key = "task_unit_reproduce"
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.setIcon("ui/Icons/iconLovers");
		for (int k = 0; k < 5; k++)
		{
			t.addBeh(new BehStayInBuildingTarget(1f, 2f));
		}
		t.addBeh(new BehExitBuilding());
	}

	private void initTasksChildren()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "child_random_flips",
			cancellable_by_socialize = true,
			locale_key = "task_unit_play"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/iconChildren");
		for (int i = 0; i < 4; i++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.2f, 0.2f));
		}
		t.addBeh(new BehActorChangeHappiness("just_played"));
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "child_play_at_one_spot",
			cancellable_by_socialize = true,
			locale_key = "task_unit_play"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/iconChildren");
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehJumpingAnimation(1.5f, 1.5f));
		t.addBeh(new BehActorReverseFlip());
		t.addBeh(new BehActorChangeHappiness("just_played"));
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "child_random_jump",
			cancellable_by_socialize = true,
			locale_key = "task_unit_play"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/iconChildren");
		for (int j = 0; j < 2; j++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(1.5f, 1.5f));
		}
		t.addBeh(new BehActorRandomJump());
		t.addBeh(new BehRandomWait(1f, 3f));
		t.addBeh(new BehActorChangeHappiness("just_played"));
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "child_follow_parent",
			cancellable_by_socialize = true,
			locale_key = "task_unit_move"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/iconAdults");
		t.addBeh(new BehChildFindRandomFamilyParent());
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.SameRegion));
		t.addBeh(new BehRepeatTaskChance(0.1f));
	}

	private void initTasksSubspeciesTraits()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "try_affect_dreams"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/subspecies_traits/subspecies_trait_dreamweavers");
		t.addBeh(new BehFindTile(TileFinderType.FreeTile));
		t.addBeh(new BehGoToTileTarget());
		for (int i = 0; i < 4; i++)
		{
			t.addBeh(new BehActorReverseFlip());
			t.addBeh(new BehJumpingAnimation(0.5f, 0.5f));
		}
		t.addBeh(new BehAffectDreams());
	}

	private void initTasksSocializing()
	{
		BehaviourTaskActor obj = new BehaviourTaskActor
		{
			id = "socialize_initial_check",
			locale_key = "task_unit_socialize"
		};
		BehaviourTaskActor pAsset = obj;
		t = obj;
		add(pAsset);
		t.setIcon("ui/Icons/culture_traits/culture_trait_gossip_lovers");
		t.addBeh(new BehSocializeStartCheck());
		BehaviourTaskActor obj2 = new BehaviourTaskActor
		{
			id = "socialize_try_to_start_near_bonfire",
			locale_key = "task_unit_socialize"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.setIcon("ui/Icons/culture_traits/culture_trait_gossip_lovers");
		t.addBeh(new BehCityActorFindBuilding("type_bonfire"));
		t.addBeh(new BehFindRandomTileNearBuildingTarget());
		t.addBeh(new BehGoToTileTarget());
		t.addBeh(new BehTryToSocialize());
		BehaviourTaskActor obj3 = new BehaviourTaskActor
		{
			id = "socialize_try_to_start_immediate",
			locale_key = "task_unit_socialize"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.setIcon("ui/Icons/culture_traits/culture_trait_gossip_lovers");
		t.addBeh(new BehTryToSocialize());
		BehaviourTaskActor obj4 = new BehaviourTaskActor
		{
			id = "socialize_go_to_target",
			locale_key = "task_unit_socialize"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.setIcon("ui/Icons/culture_traits/culture_trait_gossip_lovers");
		t.addBeh(new BehGoToActorTarget(GoToActorTargetType.NearbyTileClosest, pPathOnWater: false, pCheckCanAttackTarget: false, pCalibrateTargetPosition: true));
		t.addBeh(new BehCheckNearActorTarget());
		t.addBeh(new BehSetNextTask("socialize_do_talk", pClean: false));
		BehaviourTaskActor obj5 = new BehaviourTaskActor
		{
			id = "socialize_do_talk",
			locale_key = "task_unit_socialize"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.setIcon("ui/Icons/culture_traits/culture_trait_gossip_lovers");
		t.addBeh(new BehDoTalk());
		t.addBeh(new BehFinishTalk());
		t.addBeh(new BehRandomWait(1f, 2f));
		BehaviourTaskActor obj6 = new BehaviourTaskActor
		{
			id = "socialize_receiving",
			locale_key = "task_unit_socialize"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.setIcon("ui/Icons/culture_traits/culture_trait_gossip_lovers");
		t.addBeh(new BehSocializeTalk());
	}

	private void addActionsForDeliverResources(BehaviourTaskActor pTask, bool pWheatStorage = false)
	{
		pTask.addBeh(new BehCheckHasResources());
		pTask.addBeh(new BehRandomWait(0.7f, 1.2f));
		if (pWheatStorage)
		{
			pTask.addBeh(new BehCityActorFindStorageWheat());
		}
		else
		{
			pTask.addBeh(new BehCityActorFindStorage());
		}
		pTask.addBeh(new BehFindRaycastTileForBuildingTarget());
		pTask.addBeh(new BehGoToTileTarget());
		pTask.addBeh(new BehLookAtBuildingTarget());
		pTask.addBeh(new BehAngleAnimation(AngleAnimationTarget.Building, string.Empty, 0.1f, 20f));
		pTask.addBeh(new BehThrowResources());
		pTask.addBeh(new BehRandomWait(0f, 0.2f));
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (BehaviourTaskActor item in list)
		{
			string force_hand_tool = item.force_hand_tool;
			if (!string.IsNullOrEmpty(force_hand_tool))
			{
				item.cached_hand_tool_asset = AssetManager.unit_hand_tools.get(force_hand_tool);
			}
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (BehaviourTaskActor item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
