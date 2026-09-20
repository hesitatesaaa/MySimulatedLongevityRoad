using System;
using System.Collections.Generic;

[Serializable]
public class MapGenTemplate : Asset, IDescriptionAsset, ILocalizedAsset
{
	public MapGenValues values = new MapGenValues();

	public int force_height_to;

	public bool freeze_mountains;

	public bool special_anthill;

	public bool special_checkerboard;

	public bool special_cubicles;

	public List<PerlinReplaceContainer> perlin_replace = new List<PerlinReplaceContainer>();

	public string path_icon = "ui/template_icon_1";

	public bool allow_edit_size = true;

	public bool allow_edit_random_shapes = true;

	public bool allow_edit_random_biomes = true;

	public bool allow_edit_perlin_scale_stage_1 = true;

	public bool allow_edit_perlin_scale_stage_2 = true;

	public bool allow_edit_perlin_scale_stage_3 = true;

	public bool allow_edit_mountain_edges = true;

	public bool allow_edit_random_vegetation = true;

	public bool allow_edit_random_resources = true;

	public bool allow_edit_center_lake = true;

	public bool allow_edit_round_edges = true;

	public bool allow_edit_square_edges = true;

	public bool allow_edit_ring_effect = true;

	public bool allow_edit_center_land = true;

	public bool allow_edit_low_ground = true;

	public bool allow_edit_high_ground = true;

	public bool allow_edit_remove_mountains = true;

	public bool allow_edit_cubicles;

	public bool show_reset_button = true;

	public string getLocaleID()
	{
		return "template_" + id;
	}

	public string getDescriptionID()
	{
		return "template_" + id + "_info";
	}
}
