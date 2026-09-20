using System.Collections.Generic;
using UnityEngine;

public class MapGenTemplateLibrary : AssetLibrary<MapGenTemplate>
{
	private Dictionary<string, MapGenValues> default_values = new Dictionary<string, MapGenValues>();

	public override void init()
	{
		base.init();
		add(new MapGenTemplate
		{
			id = "continent",
			freeze_mountains = true,
			path_icon = "ui/new_world_templates_icons/template_continent"
		});
		t.values.main_perlin_noise_stage = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.perlin_noise_stage_3 = true;
		t.values.gradient_round_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.ring_effect = true;
		t.values.random_shapes_amount = 5;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "box_world",
			freeze_mountains = true,
			path_icon = "ui/new_world_templates_icons/template_box_world"
		});
		t.values.main_perlin_noise_stage = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.perlin_noise_stage_3 = true;
		t.values.add_mountain_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.ring_effect = true;
		t.values.random_shapes_amount = 5;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "islands",
			freeze_mountains = true,
			path_icon = "ui/new_world_templates_icons/template_islands"
		});
		t.values.main_perlin_noise_stage = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.perlin_noise_stage_3 = true;
		t.values.gradient_round_edges = true;
		t.values.ring_effect = true;
		t.values.random_shapes_amount = 5;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "toast",
			path_icon = "ui/new_world_templates_icons/template_toast"
		});
		t.values.square_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.main_perlin_noise_stage = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.perlin_noise_stage_3 = true;
		t.values.remove_mountains = true;
		t.values.random_shapes_amount = 5;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "pancake",
			path_icon = "ui/new_world_templates_icons/template_pancake"
		});
		t.values.gradient_round_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.main_perlin_noise_stage = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.perlin_noise_stage_3 = true;
		t.values.remove_mountains = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "boring_plains",
			path_icon = "ui/new_world_templates_icons/template_boring_plains"
		});
		t.values.add_center_gradient_land = true;
		t.values.main_perlin_noise_stage = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.perlin_noise_stage_3 = true;
		t.values.add_mountain_edges = true;
		t.values.remove_mountains = true;
		t.allow_edit_low_ground = false;
		t.allow_edit_high_ground = false;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		addReplaceOption(0, "shallow_waters", "soil_low");
		addReplaceOption(0, "close_ocean", "soil_low");
		addReplaceOption(0, "deep_ocean", "soil_high");
		add(new MapGenTemplate
		{
			id = "checkerboard",
			special_checkerboard = true,
			path_icon = "ui/new_world_templates_icons/template_checkerboard"
		});
		t.values.random_biomes = true;
		t.values.add_mountain_edges = true;
		t.values.remove_mountains = true;
		disableNormalSettings(t);
		t.allow_edit_random_biomes = true;
		t.allow_edit_random_resources = true;
		t.allow_edit_random_vegetation = true;
		add(new MapGenTemplate
		{
			id = "cubicles",
			special_cubicles = true,
			allow_edit_cubicles = true,
			path_icon = "ui/new_world_templates_icons/template_cubicles"
		});
		t.values.random_biomes = true;
		t.values.add_mountain_edges = true;
		t.values.remove_mountains = true;
		t.values.cubicle_size = 2;
		disableNormalSettings(t);
		t.allow_edit_cubicles = true;
		t.allow_edit_random_biomes = true;
		t.allow_edit_random_resources = true;
		t.allow_edit_random_vegetation = true;
		add(new MapGenTemplate
		{
			id = "dormant_volcano",
			force_height_to = 125,
			path_icon = "ui/new_world_templates_icons/template_dormant_volcano"
		});
		t.values.perlin_noise_stage_2 = true;
		t.values.random_biomes = false;
		t.values.gradient_round_edges = true;
		t.values.add_center_gradient_land = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "cheese",
			force_height_to = 120,
			path_icon = "ui/new_world_templates_icons/template_cheese"
		});
		t.values.square_edges = true;
		t.values.perlin_noise_stage_2 = true;
		t.values.remove_mountains = true;
		t.values.add_center_gradient_land = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "shallow_waters");
		addReplaceOption(170, "sand", "shallow_waters");
		addReplaceOption(150, "soil_high", "sand");
		addReplaceOption(130, "soil_low", "sand");
		addReplaceOption(80, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "bad_apple",
			path_icon = "ui/new_world_templates_icons/template_bad_apple"
		});
		t.values.gradient_round_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.center_gradient_mountains = true;
		t.values.main_perlin_noise_stage = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "donut",
			force_height_to = 125,
			path_icon = "ui/new_world_templates_icons/template_donut"
		});
		t.values.gradient_round_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.add_center_lake = true;
		t.values.perlin_noise_stage_2 = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "lasagna",
			path_icon = "ui/new_world_templates_icons/template_lasagna"
		});
		t.values.square_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.main_perlin_noise_stage = true;
		t.values.random_shapes_amount = 5;
		t.values.low_ground = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "chaos_pearl",
			path_icon = "ui/new_world_templates_icons/template_chaos_pearl"
		});
		t.values.gradient_round_edges = true;
		t.values.add_center_gradient_land = true;
		t.values.main_perlin_noise_stage = true;
		t.values.low_ground = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "anthill",
			special_anthill = true,
			path_icon = "ui/new_world_templates_icons/template_anthill"
		});
		disableNormalSettings(t);
		t.allow_edit_random_biomes = true;
		t.allow_edit_random_resources = true;
		t.allow_edit_random_vegetation = true;
		newReplaceContainer(15);
		addReplaceOption(170, "soil_high", "soil_low");
		add(new MapGenTemplate
		{
			id = "empty",
			show_reset_button = false,
			path_icon = "ui/new_world_templates_icons/template_empty"
		});
		disableNormalSettings(t);
		createDefaultBackupValues();
	}

	public void createDefaultBackupValues()
	{
		foreach (MapGenTemplate item in list)
		{
			MapGenValues value = JsonUtility.FromJson<MapGenValues>(JsonUtility.ToJson((object)item.values));
			default_values.Add(item.id, value);
		}
	}

	public void resetTemplateValues(MapGenTemplate pAsset)
	{
		MapGenValues values = JsonUtility.FromJson<MapGenValues>(JsonUtility.ToJson((object)default_values[pAsset.id]));
		pAsset.values = values;
	}

	public void disableNormalSettings(MapGenTemplate pAsset)
	{
		pAsset.allow_edit_size = false;
		pAsset.allow_edit_random_shapes = false;
		pAsset.allow_edit_random_biomes = false;
		pAsset.allow_edit_perlin_scale_stage_1 = false;
		pAsset.allow_edit_perlin_scale_stage_2 = false;
		pAsset.allow_edit_perlin_scale_stage_3 = false;
		pAsset.allow_edit_mountain_edges = false;
		pAsset.allow_edit_random_vegetation = false;
		pAsset.allow_edit_random_resources = false;
		pAsset.allow_edit_center_lake = false;
		pAsset.allow_edit_round_edges = false;
		pAsset.allow_edit_square_edges = false;
		pAsset.allow_edit_ring_effect = false;
		pAsset.allow_edit_center_land = false;
		pAsset.allow_edit_low_ground = false;
		pAsset.allow_edit_high_ground = false;
		pAsset.allow_edit_remove_mountains = false;
	}

	private void newReplaceContainer(int pScale = 1)
	{
		t.perlin_replace.Add(new PerlinReplaceContainer
		{
			scale = pScale
		});
	}

	private void addReplaceOption(int pHeight, string pFrom, string pTo)
	{
		t.perlin_replace[t.perlin_replace.Count - 1].options.Add(new PerlinReplaceOption
		{
			replace_height_value = pHeight,
			from = pFrom,
			to = pTo
		});
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (MapGenTemplate item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}
}
