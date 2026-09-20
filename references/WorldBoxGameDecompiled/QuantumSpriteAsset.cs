using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class QuantumSpriteAsset : Asset
{
	public string id_prefab;

	public bool turn_off_renderer;

	public float base_scale = 0.2f;

	public bool flag_battle;

	public bool flag_kingdom_color;

	public bool render_arrow_end;

	public bool render_arrow_start;

	public bool arrow_animation;

	public int line_height = 3;

	public int line_width = 3;

	public bool render_gameplay;

	public bool render_map;

	public bool render_space;

	public bool add_camera_zoom_multiplier = true;

	public int add_camera_zoom_multiplier_min = 1;

	public int add_camera_zoom_multiplier_max = 8;

	public string sound_idle;

	public bool selected_city_scale;

	public string path_icon;

	public DebugOption debug_option;

	public int default_amount;

	public Color color;

	public Color color_2;

	[NonSerialized]
	public QuantumSpriteGroupSystem group_system;

	public QuantumSpriteUpdater draw_call;

	public QuantumSpriteCreate create_object;

	[JsonIgnore]
	public bool has_sound_idle => sound_idle != null;
}
