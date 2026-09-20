using Unity.Mathematics;

public class TesterBehTaskLibrary : AssetLibrary<BehaviourTaskTester>
{
	public override void init()
	{
		base.init();
		BehaviourTaskTester obj = new BehaviourTaskTester
		{
			id = "close_all_windows"
		};
		BehaviourTaskTester pAsset = obj;
		t = obj;
		add(pAsset);
		t.addBeh(new TesterBehCloseWindows());
		BehaviourTaskTester obj2 = new BehaviourTaskTester
		{
			id = "open_random_window"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.addBeh(new TesterBehCloseWindows());
		t.addBeh(new TesterBehOpenWindow("random"));
		t.addBeh(new TesterBehWait(1f));
		BehaviourTaskTester obj3 = new BehaviourTaskTester
		{
			id = "open_next_window"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.addBeh(new TesterBehCloseWindows());
		t.addBeh(new TesterBehOpenNextWindow());
		BehaviourTaskTester obj4 = new BehaviourTaskTester
		{
			id = "open_next_meta_window"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.addBeh(new TesterBehCloseWindows());
		t.addBeh(new TesterBehOpenNextWindow(pOnlyMeta: true));
		BehaviourTaskTester obj5 = new BehaviourTaskTester
		{
			id = "open_random_meta_window"
		};
		pAsset = obj5;
		t = obj5;
		add(pAsset);
		t.addBeh(new TesterBehCloseWindows());
		t.addBeh(new TesterBehOpenNextWindow(pOnlyMeta: true, pRandom: true));
		BehaviourTaskTester obj6 = new BehaviourTaskTester
		{
			id = "screenshot_window"
		};
		pAsset = obj6;
		t = obj6;
		add(pAsset);
		t.addBeh(new TesterBehScreenshotTooltips());
		t.addBeh(new TesterBehScreenshotWindow());
		t.addBeh(new TesterBehScrollWindow());
		BehaviourTaskTester obj7 = new BehaviourTaskTester
		{
			id = "test_window_and_tooltips"
		};
		pAsset = obj7;
		t = obj7;
		add(pAsset);
		t.addBeh(new TesterBehScreenshotTooltips(pScreenshot: false));
		t.addBeh(new TesterBehScrollWindow());
		BehaviourTaskTester obj8 = new BehaviourTaskTester
		{
			id = "setup_window_test"
		};
		pAsset = obj8;
		t = obj8;
		add(pAsset);
		t.addBeh(new TesterBehResetTweens());
		t.addBeh(new TesterBehSetResolution(562, 1000, "iPhone 7 66x"));
		t.addBeh(new TesterBehLoadWorld(1, "mapTemplates/Editor/map_windows"));
		t.addBeh(new TesterBehWait(1f));
		t.addBeh(new TesterBehSaveWorldIfEmpty(1));
		t.addBeh(new TesterBehWait(1f));
		t.addBeh(new TesterBehCheckWorld());
		t.addBeh(new TesterBehWait(1f));
		BehaviourTaskTester obj9 = new BehaviourTaskTester
		{
			id = "reset_tweens"
		};
		pAsset = obj9;
		t = obj9;
		add(pAsset);
		t.addBeh(new TesterBehResetTweens());
		BehaviourTaskTester obj10 = new BehaviourTaskTester
		{
			id = "next_language"
		};
		pAsset = obj10;
		t = obj10;
		add(pAsset);
		t.addBeh(new TesterBehSetNextLanguage());
		t.addBeh(new TesterBehScreenshotFolder());
		BehaviourTaskTester obj11 = new BehaviourTaskTester
		{
			id = "add_language_file"
		};
		pAsset = obj11;
		t = obj11;
		add(pAsset);
		t.addBeh(new TesterBehCopyCurrentLanguage());
		BehaviourTaskTester obj12 = new BehaviourTaskTester
		{
			id = "prepare_window_testdata"
		};
		pAsset = obj12;
		t = obj12;
		add(pAsset);
		t.addBeh(new TesterBehSetupWindowTest());
		BehaviourTaskTester obj13 = new BehaviourTaskTester
		{
			id = "clear_window_testdata"
		};
		pAsset = obj13;
		t = obj13;
		add(pAsset);
		t.addBeh(new TesterBehClearWindowTest());
		t.addBeh(new TesterBehCloseWindows());
		BehaviourTaskTester obj14 = new BehaviourTaskTester
		{
			id = "fill_world_lava"
		};
		pAsset = obj14;
		t = obj14;
		add(pAsset);
		t.addBeh(new TesterBehFillWorld("lava0"));
		t.addBeh(new TesterBehFillWorld("lava1"));
		t.addBeh(new TesterBehFillWorld("lava2"));
		t.addBeh(new TesterBehFillWorld("lava3"));
		BehaviourTaskTester obj15 = new BehaviourTaskTester
		{
			id = "fill_world_pit"
		};
		pAsset = obj15;
		t = obj15;
		add(pAsset);
		for (int i = 0; i < 50; i++)
		{
			t.addBeh(new TesterBehFillWorld("pit_close_ocean"));
		}
		BehaviourTaskTester obj16 = new BehaviourTaskTester
		{
			id = "fill_world_water"
		};
		pAsset = obj16;
		t = obj16;
		add(pAsset);
		for (int j = 0; j < 50; j++)
		{
			t.addBeh(new TesterBehFillWorld("deep_ocean"));
		}
		BehaviourTaskTester obj17 = new BehaviourTaskTester
		{
			id = "fill_world_soil"
		};
		pAsset = obj17;
		t = obj17;
		add(pAsset);
		for (int k = 0; k < 50; k++)
		{
			t.addBeh(new TesterBehFillWorld("soil_low"));
		}
		BehaviourTaskTester obj18 = new BehaviourTaskTester
		{
			id = "fill_world_randomly"
		};
		pAsset = obj18;
		t = obj18;
		add(pAsset);
		for (int l = 0; l < 50; l++)
		{
			t.addBeh(new TesterBehFillWorld("random"));
		}
		BehaviourTaskTester obj19 = new BehaviourTaskTester
		{
			id = "destroy_sim_objects"
		};
		pAsset = obj19;
		t = obj19;
		add(pAsset);
		t.addBeh(new TesterBehDestroySimObjects());
		BehaviourTaskTester obj20 = new BehaviourTaskTester
		{
			id = "end_test"
		};
		pAsset = obj20;
		t = obj20;
		add(pAsset);
		t.addBeh(new TesterBehEndJobTest());
		BehaviourTaskTester obj21 = new BehaviourTaskTester
		{
			id = "restart_test"
		};
		pAsset = obj21;
		t = obj21;
		add(pAsset);
		t.addBeh(new TesterBehRestartJobTest());
		BehaviourTaskTester obj22 = new BehaviourTaskTester
		{
			id = "stop_autotester"
		};
		pAsset = obj22;
		t = obj22;
		add(pAsset);
		t.addBeh(new TesterBehEndAutoTester());
		BehaviourTaskTester obj23 = new BehaviourTaskTester
		{
			id = "shutdown"
		};
		pAsset = obj23;
		t = obj23;
		add(pAsset);
		t.addBeh(new TesterBehShutdown());
		BehaviourTaskTester obj24 = new BehaviourTaskTester
		{
			id = "set_speed_1"
		};
		pAsset = obj24;
		t = obj24;
		add(pAsset);
		t.addBeh(new TesterBehLog("Changing game speed"));
		t.addBeh(new TesterBehChangeWorldSpeed("x1"));
		BehaviourTaskTester obj25 = new BehaviourTaskTester
		{
			id = "set_speed_5"
		};
		pAsset = obj25;
		t = obj25;
		add(pAsset);
		t.addBeh(new TesterBehLog("Changing game speed"));
		t.addBeh(new TesterBehChangeWorldSpeed("x5"));
		BehaviourTaskTester obj26 = new BehaviourTaskTester
		{
			id = "set_speed_sonic"
		};
		pAsset = obj26;
		t = obj26;
		add(pAsset);
		t.addBeh(new TesterBehLog("Changing game speed"));
		t.addBeh(new TesterBehChangeWorldSpeed("x40"));
		BehaviourTaskTester obj27 = new BehaviourTaskTester
		{
			id = "set_speed_sonic_5"
		};
		pAsset = obj27;
		t = obj27;
		add(pAsset);
		t.addBeh(new TesterBehLog("Changing game speed"));
		t.addBeh(new TesterBehChangeWorldSpeed("x200"));
		BehaviourTaskTester obj28 = new BehaviourTaskTester
		{
			id = "pause"
		};
		pAsset = obj28;
		t = obj28;
		add(pAsset);
		t.addBeh(new TesterBehLog("Pausing game"));
		t.addBeh(new TesterBehPause(pValue: true));
		BehaviourTaskTester obj29 = new BehaviourTaskTester
		{
			id = "unpause"
		};
		pAsset = obj29;
		t = obj29;
		add(pAsset);
		t.addBeh(new TesterBehLog("Resuming game"));
		t.addBeh(new TesterBehPause(pValue: false));
		BehaviourTaskTester obj30 = new BehaviourTaskTester
		{
			id = "setup_laws"
		};
		pAsset = obj30;
		t = obj30;
		add(pAsset);
		t.addBeh(new TesterBehLog("Setting up world laws"));
		t.addBeh(new TesterBehResetSeeds(123));
		t.addBeh(new TesterBehChangeWorldLaw("world_law_hunger", pValue: false));
		t.addBeh(new TesterBehChangeWorldLaw("world_law_old_age", pValue: false));
		BehaviourTaskTester obj31 = new BehaviourTaskTester
		{
			id = "try_new_city"
		};
		pAsset = obj31;
		t = obj31;
		add(pAsset);
		t.addBeh(new TesterBehLog("Spawning 50 units to build a city"));
		t.addBeh(new TesterBehFindTileForCity());
		t.addBeh(new TesterBehSpawnRandomCivUnit(50, "tile_target"));
		BehaviourTaskTester obj32 = new BehaviourTaskTester
		{
			id = "wait_for_cities"
		};
		pAsset = obj32;
		t = obj32;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 3 cities"));
		t.addBeh(new TesterBehWaitFor("city", 3));
		BehaviourTaskTester obj33 = new BehaviourTaskTester
		{
			id = "cull_mobs"
		};
		pAsset = obj33;
		t = obj33;
		add(pAsset);
		t.addBeh(new TesterBehRequire("unit", 1000));
		t.addBeh(new TesterBehCullMobs());
		t.addBeh(new TesterBehWait(0.5f));
		BehaviourTaskTester obj34 = new BehaviourTaskTester
		{
			id = "fix_water"
		};
		pAsset = obj34;
		t = obj34;
		add(pAsset);
		t.addBeh(new TesterBehRequireUnits("god_finger", 25, RequireCondition.AtMost));
		t.addBeh(new TesterBehRequireGroundRatio(0.3f, RequireCondition.AtMost));
		t.addBeh(new TesterBehSpawnPowerActor("god_finger", 50));
		BehaviourTaskTester obj35 = new BehaviourTaskTester
		{
			id = "cull_godfinger"
		};
		pAsset = obj35;
		t = obj35;
		add(pAsset);
		t.addBeh(new TesterBehRequireUnits("god_finger", 1));
		t.addBeh(new TesterBehRequireGroundRatio(0.5f));
		t.addBeh(new TesterBehCullUnits("god_finger"));
		BehaviourTaskTester obj36 = new BehaviourTaskTester
		{
			id = "cull_units"
		};
		pAsset = obj36;
		t = obj36;
		add(pAsset);
		t.addBeh(new TesterBehRequire("unit", 1000));
		t.addBeh(new TesterBehClearFavorites());
		t.addBeh(new TesterBehSpawnPower("infinity_coin"));
		BehaviourTaskTester obj37 = new BehaviourTaskTester
		{
			id = "pick_random_meta_objects_graph"
		};
		pAsset = obj37;
		t = obj37;
		add(pAsset);
		t.addBeh(new TesterBehLog("Picking random meta objects"));
		t.addBeh(new TesterBehSelectRandomMetaObjects(pPickSelectedObjects: true));
		BehaviourTaskTester obj38 = new BehaviourTaskTester
		{
			id = "pick_random_meta_objects"
		};
		pAsset = obj38;
		t = obj38;
		add(pAsset);
		t.addBeh(new TesterBehSelectRandomMetaObjects());
		BehaviourTaskTester obj39 = new BehaviourTaskTester
		{
			id = "open_gene_editor"
		};
		pAsset = obj39;
		t = obj39;
		add(pAsset);
		t.addBeh(new TesterBehLog("Opening subspecies window with gene editor tab"));
		t.addBeh(new TesterBehSelectRandomMetaObjects());
		t.addBeh(new TesterBehOpenWindow("subspecies"));
		t.addBeh(new TesterBehOpenWindowTab("Genetics"));
		BehaviourTaskTester obj40 = new BehaviourTaskTester
		{
			id = "open_city_chart_window"
		};
		pAsset = obj40;
		t = obj40;
		add(pAsset);
		t.addBeh(new TesterBehLog("Opening city window with chart tab"));
		t.addBeh(new TesterBehOpenWindow("city"));
		t.addBeh(new TesterBehOpenWindowTab("Statistics"));
		t.addBeh(new TesterBehClickRandomButton(typeof(ButtonGraphCategory)));
		t.addBeh(new TesterBehClickRandomButton(typeof(ButtonGraphCategory)));
		t.addBeh(new TesterBehClickRandomButton(typeof(ButtonGraphCategory)));
		t.addBeh(new TesterBehWait(1f));
		t.addBeh(new TesterBehCloseWindows());
		BehaviourTaskTester obj41 = new BehaviourTaskTester
		{
			id = "open_kingdom_chart_window"
		};
		pAsset = obj41;
		t = obj41;
		add(pAsset);
		t.addBeh(new TesterBehLog("Opening kingdom window with chart tab"));
		t.addBeh(new TesterBehOpenWindow("kingdom"));
		t.addBeh(new TesterBehOpenWindowTab("Statistics"));
		t.addBeh(new TesterBehClickRandomButton(typeof(ButtonGraphCategory)));
		t.addBeh(new TesterBehClickRandomButton(typeof(ButtonGraphCategory)));
		t.addBeh(new TesterBehClickRandomButton(typeof(ButtonGraphCategory)));
		t.addBeh(new TesterBehWait(1f));
		t.addBeh(new TesterBehCloseWindows());
		BehaviourTaskTester obj42 = new BehaviourTaskTester
		{
			id = "open_city_window"
		};
		pAsset = obj42;
		t = obj42;
		add(pAsset);
		t.addBeh(new TesterBehLog("Opening city window"));
		t.addBeh(new TesterBehOpenWindow("city"));
		BehaviourTaskTester obj43 = new BehaviourTaskTester
		{
			id = "random_meta_switch"
		};
		pAsset = obj43;
		t = obj43;
		add(pAsset);
		t.addBeh(new TesterBehRandomMetaSwitch());
		BehaviourTaskTester obj44 = new BehaviourTaskTester
		{
			id = "open_kingdom_window"
		};
		pAsset = obj44;
		t = obj44;
		add(pAsset);
		t.addBeh(new TesterBehLog("Opening kingdom window"));
		t.addBeh(new TesterBehOpenWindow("kingdom"));
		BehaviourTaskTester obj45 = new BehaviourTaskTester
		{
			id = "random_window_tab"
		};
		pAsset = obj45;
		t = obj45;
		add(pAsset);
		t.addBeh(new TesterBehRandomWindowTab());
		Random val = default(Random);
		((Random)(ref val))._002Ector(100u);
		BehaviourTaskTester obj46 = new BehaviourTaskTester
		{
			id = "random_bombs_and_kingdoms"
		};
		pAsset = obj46;
		t = obj46;
		add(pAsset);
		t.addBeh(new TesterBehLog("Spawning random units and bombs"));
		int pWaitYears = ((Random)(ref val)).NextInt(2, 5);
		int num = ((Random)(ref val)).NextInt(25, 50);
		t.addBeh(new TesterBehWaitYears(pWaitYears));
		t.addBeh(new TesterBehWaitForYear());
		t.addBeh(new TesterBehLog("Spawning units and bombs"));
		for (int m = 0; m < num; m++)
		{
			t.addBeh(new TesterBehSpawnRandomKingdomUnit());
		}
		t.addBeh(new TesterBehSpawnRandomBomb());
		BehaviourTaskTester obj47 = new BehaviourTaskTester
		{
			id = "show_compare_window"
		};
		pAsset = obj47;
		t = obj47;
		add(pAsset);
		t.addBeh(new TesterBehLog("Showing chart compare window"));
		t.addBeh(new TesterBehOpenWindow("chart_comparer"));
		t.addBeh(new TesterBehWait(1f));
		t.addBeh(new TesterBehCloseWindows());
		BehaviourTaskTester obj48 = new BehaviourTaskTester
		{
			id = "wait_05"
		};
		pAsset = obj48;
		t = obj48;
		add(pAsset);
		t.addBeh(new TesterBehWait(0.5f));
		BehaviourTaskTester obj49 = new BehaviourTaskTester
		{
			id = "wait_1"
		};
		pAsset = obj49;
		t = obj49;
		add(pAsset);
		t.addBeh(new TesterBehWait(1f));
		BehaviourTaskTester obj50 = new BehaviourTaskTester
		{
			id = "wait_5"
		};
		pAsset = obj50;
		t = obj50;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 5 game seconds..."));
		t.addBeh(new TesterBehWait(5f));
		BehaviourTaskTester obj51 = new BehaviourTaskTester
		{
			id = "wait_10"
		};
		pAsset = obj51;
		t = obj51;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 10 game seconds..."));
		t.addBeh(new TesterBehWait(10f));
		BehaviourTaskTester obj52 = new BehaviourTaskTester
		{
			id = "wait_20"
		};
		pAsset = obj52;
		t = obj52;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 20 game seconds..."));
		t.addBeh(new TesterBehWait(20f));
		BehaviourTaskTester obj53 = new BehaviourTaskTester
		{
			id = "wait_years_1"
		};
		pAsset = obj53;
		t = obj53;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 1 year..."));
		t.addBeh(new TesterBehWaitYears(1));
		t.addBeh(new TesterBehWaitForYear());
		BehaviourTaskTester obj54 = new BehaviourTaskTester
		{
			id = "wait_years_5"
		};
		pAsset = obj54;
		t = obj54;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 5 years..."));
		t.addBeh(new TesterBehWaitYears(5));
		t.addBeh(new TesterBehWaitForYear());
		BehaviourTaskTester obj55 = new BehaviourTaskTester
		{
			id = "wait_years_10"
		};
		pAsset = obj55;
		t = obj55;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 10 years..."));
		t.addBeh(new TesterBehWaitYears(10));
		t.addBeh(new TesterBehWaitForYear());
		BehaviourTaskTester obj56 = new BehaviourTaskTester
		{
			id = "wait_years_20"
		};
		pAsset = obj56;
		t = obj56;
		add(pAsset);
		t.addBeh(new TesterBehLog("Waiting for 20 years..."));
		t.addBeh(new TesterBehWaitYears(20));
		t.addBeh(new TesterBehWaitForYear());
		BehaviourTaskTester obj57 = new BehaviourTaskTester
		{
			id = "spawn_random_power"
		};
		pAsset = obj57;
		t = obj57;
		add(pAsset);
		t.addBeh(new TesterBehSpawnRandomPower());
		t.addBeh(new TesterBehWait(0.1f));
		BehaviourTaskTester obj58 = new BehaviourTaskTester
		{
			id = "randomize_subspecies_traits"
		};
		pAsset = obj58;
		t = obj58;
		add(pAsset);
		t.addBeh(new TesterMutateSubspeciesTraits());
		BehaviourTaskTester obj59 = new BehaviourTaskTester
		{
			id = "randomize_subspecies_genes"
		};
		pAsset = obj59;
		t = obj59;
		add(pAsset);
		t.addBeh(new TesterMutateSubspeciesGenes());
		BehaviourTaskTester obj60 = new BehaviourTaskTester
		{
			id = "randomize_actor_traits"
		};
		pAsset = obj60;
		t = obj60;
		add(pAsset);
		t.addBeh(new TesterMutateActorTraits());
		BehaviourTaskTester obj61 = new BehaviourTaskTester
		{
			id = "randomize_actor_status"
		};
		pAsset = obj61;
		t = obj61;
		add(pAsset);
		t.addBeh(new TesterMutateActorStatus());
		BehaviourTaskTester obj62 = new BehaviourTaskTester
		{
			id = "spawn_infinity_coin"
		};
		pAsset = obj62;
		t = obj62;
		add(pAsset);
		t.addBeh(new TesterBehSpawnPower("infinity_coin"));
		BehaviourTaskTester obj63 = new BehaviourTaskTester
		{
			id = "spawn_random_unit"
		};
		pAsset = obj63;
		t = obj63;
		add(pAsset);
		t.addBeh(new TesterBehSpawnRandomUnit());
		BehaviourTaskTester obj64 = new BehaviourTaskTester
		{
			id = "spawn_random_unit_5"
		};
		pAsset = obj64;
		t = obj64;
		add(pAsset);
		t.addBeh(new TesterBehSpawnRandomUnit(5));
		BehaviourTaskTester obj65 = new BehaviourTaskTester
		{
			id = "spawn_random_unit_10"
		};
		pAsset = obj65;
		t = obj65;
		add(pAsset);
		t.addBeh(new TesterBehSpawnRandomUnit(10));
		BehaviourTaskTester obj66 = new BehaviourTaskTester
		{
			id = "spawn_random_building"
		};
		pAsset = obj66;
		t = obj66;
		add(pAsset);
		t.addBeh(new TesterBehSpawnRandomBuilding());
		BehaviourTaskTester obj67 = new BehaviourTaskTester
		{
			id = "generate_map_random"
		};
		pAsset = obj67;
		t = obj67;
		add(pAsset);
		t.addBeh(new TesterBehGenerateMap());
		BehaviourTaskTester obj68 = new BehaviourTaskTester
		{
			id = "super_damage_units"
		};
		pAsset = obj68;
		t = obj68;
		add(pAsset);
		t.addBeh(new TesterBehSuperDamageToUnits());
	}

	public override void editorDiagnosticLocales()
	{
	}
}
