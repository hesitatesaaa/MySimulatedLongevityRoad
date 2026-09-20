public class MapGenSettingsLibrary : AssetLibrary<MapGenSettingsAsset>
{
	private MapGenValues gen_values => AssetManager.map_gen_templates.get(Config.current_map_template).values;

	public override void init()
	{
		base.init();
		add(new MapGenSettingsAsset
		{
			id = "gen_perlin_scale_stage_1",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_perlin_scale_stage_1,
			max_value = 30,
			action_get = () => gen_values.perlin_scale_stage_1,
			action_set = delegate(int pValue)
			{
				gen_values.perlin_scale_stage_1 = pValue;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_perlin_scale_stage_2",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_perlin_scale_stage_1,
			max_value = 30,
			action_get = () => gen_values.perlin_scale_stage_2,
			action_set = delegate(int pValue)
			{
				gen_values.perlin_scale_stage_2 = pValue;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_perlin_scale_stage_3",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_perlin_scale_stage_1,
			max_value = 30,
			action_get = () => gen_values.perlin_scale_stage_3,
			action_set = delegate(int pValue)
			{
				gen_values.perlin_scale_stage_3 = pValue;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_random_shapes",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_random_shapes,
			max_value = 40,
			action_get = () => gen_values.random_shapes_amount,
			action_set = delegate(int pValue)
			{
				gen_values.random_shapes_amount = pValue;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_cubicles_sizes",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_cubicles,
			min_value = 2,
			max_value = 15,
			action_get = () => gen_values.cubicle_size,
			action_set = delegate(int pValue)
			{
				gen_values.cubicle_size = pValue;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_random_biomes",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_random_biomes,
			is_switch = true,
			action_get = () => gen_values.random_biomes ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.random_biomes = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_mountain_edges",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_mountain_edges,
			is_switch = true,
			action_get = () => gen_values.add_mountain_edges ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.add_mountain_edges = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_add_vegetation",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_random_vegetation,
			is_switch = true,
			action_get = () => gen_values.add_vegetation ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.add_vegetation = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_add_resources",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_random_resources,
			is_switch = true,
			action_get = () => gen_values.add_resources ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.add_resources = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_add_center_lake",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_center_lake,
			is_switch = true,
			action_get = () => gen_values.add_center_lake ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.add_center_lake = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_add_center_land",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_center_land,
			is_switch = true,
			action_get = () => gen_values.add_center_gradient_land ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.add_center_gradient_land = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_round_edges",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_round_edges,
			is_switch = true,
			action_get = () => gen_values.gradient_round_edges ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.gradient_round_edges = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_square_edges",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_square_edges,
			is_switch = true,
			action_get = () => gen_values.square_edges ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.square_edges = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_ring_effect",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_ring_effect,
			is_switch = true,
			action_get = () => gen_values.ring_effect ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.ring_effect = pValue == 1;
			}
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_low_ground",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_low_ground,
			is_switch = true,
			action_set = delegate(int pValue)
			{
				gen_values.low_ground = pValue == 1;
			},
			action_get = () => gen_values.low_ground ? 1 : 0
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_high_ground",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_high_ground,
			is_switch = true,
			action_set = delegate(int pValue)
			{
				gen_values.high_ground = pValue == 1;
			},
			action_get = () => gen_values.high_ground ? 1 : 0
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_remove_mountains",
			allowed_check = (MapGenTemplate pAsset) => pAsset.allow_edit_remove_mountains,
			is_switch = true,
			action_set = delegate(int pValue)
			{
				gen_values.remove_mountains = pValue == 1;
			},
			action_get = () => gen_values.remove_mountains ? 1 : 0
		});
		add(new MapGenSettingsAsset
		{
			id = "gen_forbidden_knowledge",
			allowed_check = (MapGenTemplate _) => Config.hasPremium && AchievementLibrary.cursed_world.isUnlocked(),
			is_switch = true,
			action_get = () => gen_values.forbidden_knowledge_start ? 1 : 0,
			action_set = delegate(int pValue)
			{
				gen_values.forbidden_knowledge_start = pValue == 1;
			}
		});
	}

	public override MapGenSettingsAsset add(MapGenSettingsAsset pAsset)
	{
		pAsset.increase = delegate(MapGenSettingsAsset mapGenSettingsAsset)
		{
			int num = mapGenSettingsAsset.action_get();
			int num2 = 1;
			if (HotkeyLibrary.many_mod.isHolding())
			{
				num2 = 5;
			}
			num += num2;
			if (num > mapGenSettingsAsset.max_value)
			{
				num = mapGenSettingsAsset.min_value;
			}
			mapGenSettingsAsset.action_set(num);
		};
		pAsset.decrease = delegate(MapGenSettingsAsset mapGenSettingsAsset)
		{
			int num = mapGenSettingsAsset.action_get();
			int num2 = 1;
			if (HotkeyLibrary.many_mod.isHolding())
			{
				num2 = 5;
			}
			num -= num2;
			if (num < mapGenSettingsAsset.min_value)
			{
				num = mapGenSettingsAsset.max_value;
			}
			mapGenSettingsAsset.action_set(num);
		};
		pAsset.action_switch = delegate(MapGenSettingsAsset mapGenSettingsAsset)
		{
			int pValue = ((mapGenSettingsAsset.action_get() == 0) ? 1 : 0);
			mapGenSettingsAsset.action_set(pValue);
		};
		return base.add(pAsset);
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (MapGenSettingsAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
