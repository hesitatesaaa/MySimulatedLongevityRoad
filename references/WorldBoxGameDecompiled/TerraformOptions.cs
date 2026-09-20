using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class TerraformOptions : Asset
{
	public bool remove_trees_fully;

	public bool destroy_buildings;

	public bool make_ruins;

	public bool remove_burned;

	public bool remove_tornado;

	public bool add_burned;

	public int add_heat;

	public bool flash;

	public bool remove_borders;

	public bool remove_top_tile;

	public bool remove_roads;

	public bool remove_frozen;

	public bool remove_fire;

	public bool remove_water;

	public bool remove_ruins;

	public bool lightning_effect;

	public bool remove_lava;

	public bool damage_buildings;

	public int damage;

	public WorldAction bomb_action;

	public bool set_fire;

	public bool transform_to_wasteland;

	public bool apply_force;

	[DefaultValue(1.5f)]
	public float force_power = 1.5f;

	public bool explode_tile;

	public bool explosion_pixel_effect = true;

	public bool explode_and_set_random_fire;

	public int explode_strength;

	public bool applies_to_high_flyers;

	public bool shake;

	[DefaultValue(0.3f)]
	public float shake_duration = 0.3f;

	[DefaultValue(0.01f)]
	public float shake_interval = 0.01f;

	[DefaultValue(2f)]
	public float shake_intensity = 2f;

	public string add_trait;

	[DefaultValue(AttackType.Other)]
	public AttackType attack_type = AttackType.Other;

	public string[] ignore_kingdoms;

	public List<string> destroy_only;

	public List<string> ignore_buildings;
}
