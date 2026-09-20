public class TesterJobLibrary : AssetLibrary<JobTesterAsset>
{
	private int _last_job;

	public override void init()
	{
		base.init();
		add(new JobTesterAsset
		{
			id = "test_spawn_units"
		});
		for (int i = 0; i < 50; i++)
		{
			t.addTask("spawn_random_unit");
			t.addTask("wait_1");
		}
		t.addTask("super_damage_units");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_spawn_buildings"
		});
		for (int j = 0; j < 50; j++)
		{
			t.addTask("spawn_random_building");
			t.addTask("wait_1");
		}
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "destroy_sim_objects"
		});
		t.addTask("destroy_sim_objects");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_random_power"
		});
		for (int k = 0; k < 15; k++)
		{
			t.addTask("spawn_random_power");
			t.addTask("wait_1");
		}
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_world_change"
		});
		t.addTask("fill_world_water");
		t.addTask("wait_1");
		t.addTask("fill_world_randomly");
		t.addTask("wait_1");
		t.addTask("fill_world_soil");
		t.addTask("wait_1");
		t.addTask("fill_world_randomly");
		t.addTask("wait_1");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_world_pit"
		});
		t.addTask("fill_world_pit");
		t.addTask("wait_10");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_world_lava"
		});
		for (int l = 0; l < 10; l++)
		{
			t.addTask("fill_world_lava");
		}
		t.addTask("wait_5");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_windows"
		});
		t.addTask("end_test");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("open_random_window");
		t.addTask("close_all_windows");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_gene_editor",
			manual_test = true
		});
		t.addTask("unpause");
		t.addTask("setup_laws");
		t.addTask("wait_1");
		t.addTask("pick_random_meta_objects");
		t.addTask("pause");
		t.addTask("open_gene_editor");
		t.addTask("end_test");
		add(new JobTesterAsset
		{
			id = "test_chart_windows",
			manual_test = true
		});
		t.addTask("unpause");
		t.addTask("setup_laws");
		for (int m = 0; m < 6; m++)
		{
			t.addTask("try_new_city");
		}
		t.addTask("wait_for_cities");
		t.addTask("wait_years_10");
		for (int n = 0; n < 3; n++)
		{
			t.addTask("wait_years_5");
			t.addTask("pick_random_meta_objects");
			t.addTask("open_city_chart_window");
			t.addTask("pick_random_meta_objects");
			t.addTask("open_city_chart_window");
			t.addTask("wait_years_5");
			t.addTask("pick_random_meta_objects");
			t.addTask("open_kingdom_chart_window");
			t.addTask("pick_random_meta_objects");
			t.addTask("open_kingdom_chart_window");
		}
		t.addTask("pick_random_meta_objects_graph");
		t.addTask("show_compare_window");
		t.addTask("pause");
		add(new JobTesterAsset
		{
			id = "test_meta_windows",
			manual_test = true
		});
		t.addTask("unpause");
		t.addTask("setup_laws");
		for (int num = 0; num < 6; num++)
		{
			t.addTask("try_new_city");
		}
		t.addTask("wait_for_cities");
		t.addTask("wait_5");
		for (int num2 = 0; num2 < 50; num2++)
		{
			for (int num3 = 0; num3 < 5; num3++)
			{
				t.addTask("pick_random_meta_objects");
			}
			t.addTask("open_random_meta_window");
			for (int num4 = 0; num4 < 20; num4++)
			{
				t.addTask("random_window_tab");
				t.addTask("random_meta_switch");
			}
			t.addTask("close_all_windows");
		}
		t.addTask("pick_random_meta_objects_graph");
		t.addTask("show_compare_window");
		t.addTask("pause");
		add(new JobTesterAsset
		{
			id = "test_unit_selection",
			manual_test = true
		});
		t.addTask("setup_laws");
		for (int num5 = 0; num5 < 10; num5++)
		{
			t.addTask("spawn_random_unit_5");
			for (int num6 = 0; num6 < 15; num6++)
			{
				t.addTask("pick_random_meta_objects");
			}
			t.addTask("wait_1");
		}
		t.addTask("cull_mobs");
		t.addTask("cull_units");
		t.addTask("cull_godfinger");
		t.addTask("fix_water");
		t.addTask("close_all_windows");
		for (int num7 = 0; num7 < 10; num7++)
		{
			t.addTask("spawn_random_unit_5");
			for (int num8 = 0; num8 < 15; num8++)
			{
				t.addTask("pick_random_meta_objects");
			}
			t.addTask("wait_1");
		}
		t.addTask("randomize_subspecies_traits");
		t.addTask("randomize_subspecies_genes");
		t.addTask("randomize_actor_traits");
		t.addTask("randomize_actor_status");
		t.addTask("restart_test");
		add(new JobTesterAsset
		{
			id = "test_multi_chart",
			manual_test = true
		});
		t.addTask("unpause");
		t.addTask("setup_laws");
		for (int num9 = 0; num9 < 6; num9++)
		{
			t.addTask("try_new_city");
		}
		t.addTask("wait_for_cities");
		for (int num10 = 0; num10 < 10; num10++)
		{
			t.addTask("pick_random_meta_objects_graph");
			t.addTask("show_compare_window");
			t.addTask("wait_years_5");
		}
		t.addTask("pause");
		add(new JobTesterAsset
		{
			id = "auto_play",
			manual_test = true
		});
		t.addTask("unpause");
		t.addTask("setup_laws");
		t.addTask("fix_water");
		for (int num11 = 0; num11 < 6; num11++)
		{
			t.addTask("try_new_city");
		}
		t.addTask("wait_for_cities");
		t.addTask("random_bombs_and_kingdoms");
		t.addTask("cull_godfinger");
		t.addTask("restart_test");
		add(new JobTesterAsset
		{
			id = "test_windows_tooltips",
			manual_test = true
		});
		t.addTask("unpause");
		t.addTask("setup_laws");
		t.addTask("fix_water");
		for (int num12 = 0; num12 < 6; num12++)
		{
			t.addTask("try_new_city");
		}
		t.addTask("wait_for_cities");
		for (int num13 = 0; num13 < 5; num13++)
		{
			t.addTask("spawn_random_unit_5");
		}
		t.addTask("cull_mobs");
		t.addTask("cull_units");
		t.addTask("cull_godfinger");
		t.addTask("fix_water");
		for (int num14 = 0; num14 < 5; num14++)
		{
			t.addTask("spawn_random_unit_5");
		}
		t.addTask("randomize_subspecies_traits");
		t.addTask("randomize_subspecies_genes");
		t.addTask("randomize_actor_traits");
		t.addTask("randomize_actor_status");
		t.addTask("reset_tweens");
		t.addTask("close_all_windows");
		t.addTask("prepare_window_testdata");
		t.addTask("open_next_window");
		t.addTask("test_window_and_tooltips");
		t.addTask("clear_window_testdata");
		t.addTask("close_all_windows");
		t.addTask("restart_test");
	}

	public override void linkAssets()
	{
		base.linkAssets();
	}

	public string getNextJob()
	{
		if (list.Count == 0)
		{
			return string.Empty;
		}
		if (_last_job > list.Count - 1)
		{
			_last_job = 0;
			list.Shuffle();
		}
		JobTesterAsset jobTesterAsset = list[_last_job++];
		if (jobTesterAsset.manual_test)
		{
			return getNextJob();
		}
		return jobTesterAsset.id;
	}
}
