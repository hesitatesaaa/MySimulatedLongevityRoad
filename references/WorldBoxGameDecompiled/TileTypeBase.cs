using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class TileTypeBase : Asset
{
	public const int BURNED_STAGES = 15;

	public const int EXPLOSION_STAGES = 60;

	public const int MAX_HEIGHT = 255;

	[NonSerialized]
	public HashSetWorldTile hashset = new HashSetWorldTile();

	[NonSerialized]
	public List<MusicAsset> music_assets;

	[NonSerialized]
	private bool _hashset_dirty;

	[NonSerialized]
	private List<WorldTile> _current_tiles = new List<WorldTile>();

	public static Color32 edge_color_ocean;

	public static Color32 edge_color_no_ocean;

	public static Color32 edge_color_hills;

	public static Color32 edge_color_mountain;

	[NonSerialized]
	public TileType increase_to;

	[NonSerialized]
	public TileType decrease_to;

	public WorldAction unit_death_action;

	public TileStepAction step_action;

	[DefaultValue(0.05f)]
	public float step_action_chance = 0.05f;

	public bool force_edge_variation;

	public int force_edge_variation_frame;

	[DefaultValue("")]
	public string increase_to_id = string.Empty;

	[DefaultValue("")]
	public string decrease_to_id = string.Empty;

	[DefaultValue("")]
	public string freeze_to_id = string.Empty;

	public bool creep;

	public bool wasteland;

	public int index_id;

	public static int last_index_id;

	[DefaultValue(1f)]
	public float walk_multiplier = 1f;

	[DefaultValue("")]
	public string ignore_walk_multiplier_if_tag = string.Empty;

	[DefaultValue(true)]
	public bool allowed_to_be_finger_copied = true;

	[DefaultValue(TileRank.Nothing)]
	public TileRank rank_type;

	[DefaultValue(TileRank.Nothing)]
	public TileRank creep_rank_type;

	[DefaultValue("")]
	public string biome_id = string.Empty;

	[NonSerialized]
	public BiomeAsset biome_asset;

	public bool can_be_removed_with_spade;

	public bool can_be_removed_with_bucket;

	public bool can_be_removed_with_demolish;

	public bool can_be_removed_with_pickaxe;

	public bool can_be_removed_with_axe;

	public bool can_be_removed_with_sickle;

	public bool is_biome;

	public bool block;

	public float block_height;

	public bool animated_wall;

	[NonSerialized]
	public TileSprites sprites;

	[DefaultValue(TileLayerType.Ground)]
	public TileLayerType layer_type = TileLayerType.Ground;

	public int render_z;

	public string draw_layer_name;

	public static int last_z;

	public bool can_be_set_on_fire;

	public bool burnable;

	public bool can_be_set_on_fire_by_burning_feet = true;

	[DefaultValue(10)]
	public int burn_rate = 10;

	public bool hold_lava;

	public bool can_be_filled_with_ocean;

	public string fill_to_ocean;

	public string water_fill_sound;

	public bool liquid;

	public bool ocean;

	public bool ground;

	public bool forever_frozen;

	public bool road;

	public bool life;

	public bool damaged_when_walked;

	public bool remove_on_heat;

	public bool remove_on_freeze;

	public bool chunk_dirty_when_temperature;

	public bool fast_freeze;

	[DefaultValue(true)]
	public bool can_be_frozen = true;

	public bool can_be_unfrozen;

	[DefaultValue(true)]
	public bool can_be_autotested = true;

	public bool grey_goo;

	public bool grass;

	public bool sand;

	public bool rocks;

	public bool mountains;

	public bool summit;

	public bool soil;

	public bool terraform_after_fire;

	public bool explodable;

	public bool explodable_delayed;

	public bool explodable_timed;

	public bool explodable_by_ocean;

	public bool ignore_ocean_edge_rendering;

	public int explode_range;

	public bool damage_units;

	[DefaultValue(1)]
	public int damage = 1;

	public bool lava;

	[DefaultValue(-1)]
	public int lava_level = -1;

	public int lava_change_state_after;

	[DefaultValue("")]
	public string lava_decrease = string.Empty;

	[DefaultValue("")]
	public string lava_increase = string.Empty;

	public bool edge_hills;

	public bool edge_mountains;

	public bool wall;

	[DefaultValue(true)]
	public bool check_edge = true;

	public bool can_be_biome;

	[DefaultValue(true)]
	public bool can_errode_to_sand = true;

	public int explodeTimer;

	[DefaultValue(1)]
	public int cost = 1;

	public bool can_be_farm;

	public bool farm_field;

	public bool can_build_on;

	public bool biome_build_check;

	[DefaultValue(1f)]
	public float fire_chance = 1f;

	[DefaultValue("")]
	public string food_resource = string.Empty;

	public int nutrition;

	public HashSet<BiomeTag> biome_tags;

	[NonSerialized]
	public bool has_biome_tags;

	public string color_hex;

	[NonSerialized]
	public Color32 color;

	public string edge_color_hex;

	[NonSerialized]
	public Color32 edge_color;

	public bool considered_empty_tile;

	public bool drawPixel;

	[DefaultValue(3)]
	public int strength = 3;

	[DefaultValue("mat_world_object")]
	public string material = "mat_world_object";

	[DefaultValue(-1)]
	public int height_min = -1;

	public int[] additional_height;

	public string only_allowed_to_build_with_tag;

	public bool used_in_generator;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsType(string v)
	{
		return id == v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsType(TileTypeBase pType)
	{
		return pType.index_id == index_id;
	}

	public void setBiome(string pType)
	{
		if (pType == null)
		{
			biome_id = string.Empty;
			is_biome = false;
		}
		else
		{
			is_biome = true;
			biome_id = pType;
		}
	}

	public void setDrawLayer(TileZIndexes pForceZ, string pForceOtherName = null)
	{
		if (pForceZ == TileZIndexes.nothing)
		{
			render_z = last_z;
			last_z++;
		}
		else
		{
			render_z = (int)pForceZ;
		}
		if (!string.IsNullOrEmpty(pForceOtherName))
		{
			draw_layer_name = pForceOtherName;
		}
		else
		{
			draw_layer_name = id;
		}
	}

	public void hashsetAdd(WorldTile pTile)
	{
		_hashset_dirty = true;
		hashset.Add(pTile);
	}

	public void hashsetRemove(WorldTile pTile)
	{
		_hashset_dirty = true;
		hashset.Remove(pTile);
	}

	public List<WorldTile> getCurrentTiles()
	{
		if (_hashset_dirty)
		{
			_hashset_dirty = false;
			_current_tiles.Clear();
			_current_tiles.AddRange(hashset);
		}
		return _current_tiles;
	}

	public void hashsetClear()
	{
		_current_tiles.Clear();
		hashset.Clear();
		_hashset_dirty = false;
	}

	public bool canBeEatenByGeophag()
	{
		if (liquid)
		{
			return false;
		}
		if (can_be_filled_with_ocean)
		{
			return false;
		}
		return true;
	}

	public bool overlapsBiomeTags(HashSet<BiomeTag> pBiomeTagsGrowth)
	{
		if (!has_biome_tags)
		{
			return false;
		}
		return biome_tags.Overlaps(pBiomeTagsGrowth);
	}

	static TileTypeBase()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		edge_color_ocean = Color32.op_Implicit(Toolbox.makeColor("#2D61AF"));
		edge_color_no_ocean = Color32.op_Implicit(Toolbox.makeColor("#494949"));
		edge_color_hills = Color32.op_Implicit(Toolbox.makeColor("#313333"));
		edge_color_mountain = Color32.op_Implicit(Toolbox.makeColor("#2C3032"));
		last_index_id = 0;
	}
}
