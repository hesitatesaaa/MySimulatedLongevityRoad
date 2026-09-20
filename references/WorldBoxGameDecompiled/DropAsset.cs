using System;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class DropAsset : Asset
{
	[DefaultValue(DropType.DropGeneric)]
	public DropType type;

	public bool random_frame;

	public bool random_flip;

	public bool animated;

	[DefaultValue(0.1f)]
	public float animation_speed;

	[DefaultValue(0.1f)]
	public float animation_speed_random;

	public bool animation_rotation;

	[DefaultValue(1f)]
	public float animation_rotation_speed_min;

	[DefaultValue(1f)]
	public float animation_rotation_speed_max;

	public string sound_drop;

	public string sound_launch;

	public DropsAction action_launch;

	public DropsAction action_landed;

	public DropsLandedAction action_landed_drop;

	public string building_asset;

	[DefaultValue(3.2f)]
	public float falling_speed;

	[DefaultValue(0.5f)]
	public float falling_speed_random;

	public Vector3 falling_height;

	public bool falling_random_x_move;

	public float particle_interval;

	[DefaultValue("mat_world_object")]
	public string material;

	[DefaultValue("drops/drop_pixel")]
	public string path_texture;

	[DefaultValue(1f)]
	public float default_scale;

	public bool surprises_units;

	public string drop_type_low;

	public string drop_type_high;

	[NonSerialized]
	public TopTileType cached_drop_type_low;

	[NonSerialized]
	public TopTileType cached_drop_type_high;

	[NonSerialized]
	public Sprite[] cached_sprites;

	private string[] _building_asset_split;

	public string getRandomBuildingAsset()
	{
		if (_building_asset_split == null)
		{
			_building_asset_split = building_asset.Split(',');
		}
		return _building_asset_split.GetRandom();
	}

	public DropAsset()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		type = DropType.DropGeneric;
		animation_speed = 0.1f;
		animation_speed_random = 0.1f;
		animation_rotation_speed_min = 1f;
		animation_rotation_speed_max = 1f;
		falling_speed = 3.2f;
		falling_speed_random = 0.5f;
		falling_height = Vector2.op_Implicit(new Vector2(15f, 20f));
		material = "mat_world_object";
		path_texture = "drops/drop_pixel";
		default_scale = 1f;
		base._002Ector();
	}
}
