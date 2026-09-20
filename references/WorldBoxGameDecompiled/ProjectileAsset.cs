using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class ProjectileAsset : Asset
{
	public string texture;

	public bool animated = true;

	public float animation_speed = 6f;

	public float speed = 20f;

	public float speed_random;

	public string terraform_option = string.Empty;

	public int terraform_range;

	public string end_effect;

	public float end_effect_scale = 0.25f;

	public string sound_launch = string.Empty;

	public string sound_impact = string.Empty;

	public bool look_at_target;

	public bool trail_effect_enabled;

	public string trail_effect_id = "fx_fire_smoke";

	public float trail_effect_scale = 0.25f;

	public float trail_effect_timer = 0.2f;

	public bool hit_freeze;

	public bool hit_shake;

	[DefaultValue(0.3f)]
	public float shake_duration = 0.3f;

	[DefaultValue(0.01f)]
	public float shake_interval = 0.01f;

	[DefaultValue(2f)]
	public float shake_intensity = 2f;

	[DefaultValue(false)]
	public bool shake_x;

	[DefaultValue(true)]
	public bool shake_y = true;

	[NonSerialized]
	public Sprite[] frames;

	[DefaultValue(0.1f)]
	public float scale_start = 0.1f;

	[DefaultValue(0.1f)]
	public float scale_target = 0.1f;

	public string texture_shadow = string.Empty;

	public AttackAction world_actions;

	public AttackAction impact_actions;

	public bool draw_light_area;

	public float draw_light_area_offset_x;

	public float draw_light_area_offset_y;

	public float draw_light_size = 0.05f;

	public bool trigger_on_collision;

	public bool can_be_collided = true;

	public bool can_be_left_on_ground;

	public bool can_be_blocked;

	public bool use_min_angle_height = true;

	public float mass = 1f;

	public float size = 0.5f;
}
