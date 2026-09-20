using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class WorldAgeAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	[DefaultValue(35)]
	public int years_min;

	[DefaultValue(55)]
	public int years_max;

	[DefaultValue(0.2f)]
	public float era_effect_overlay_alpha;

	[DefaultValue(1f)]
	public float era_effect_light_alpha_game;

	[DefaultValue(1f)]
	public float era_effect_light_alpha_minimap;

	public bool overlay_darkness;

	public bool particles_snow;

	public bool particles_rain;

	public bool particles_magic;

	public bool particles_ash;

	public bool particles_sun;

	public bool global_freeze_world;

	public bool global_unfreeze_world;

	public bool global_unfreeze_world_mountains;

	public bool overlay_magic;

	public bool overlay_rain_darkness;

	public bool overlay_winter;

	public bool overlay_chaos;

	public bool overlay_moon;

	public bool overlay_sun;

	public bool overlay_ash;

	public bool overlay_night;

	public bool overlay_rain;

	public List<string> clouds;

	public HashSet<string> biomes;

	[DefaultValue(15f)]
	public float cloud_interval;

	[DefaultValue(1f)]
	public float range_weapons_multiplier;

	[DefaultValue(1)]
	public int temperature_damage_bonus;

	public string[] conditions;

	[DefaultValue("")]
	public string force_next;

	[DefaultValue(true)]
	public bool flag_crops_grow;

	public bool era_disaster_snow_turns_babies_into_ice_ones;

	public bool era_disaster_fire_elemental_spawn_on_fire;

	public float fire_elemental_spawn_chance;

	public bool era_disaster_rage_brings_demons;

	public bool flag_light_age;

	public bool flag_chaos;

	public bool flag_winter;

	public bool flag_moon;

	public bool flag_night;

	public bool flag_light_damage;

	public string path_icon;

	public string path_background;

	[NonSerialized]
	private Sprite _cached_sprite;

	[NonSerialized]
	private Sprite _cached_background;

	public Color light_color;

	public Color title_color;

	public int bonus_loyalty;

	public int bonus_opinion;

	public int bonus_biomes_growth;

	[DefaultValue(true)]
	public bool grow_vegetation;

	[DefaultValue(1f)]
	public float fire_spread_rate_bonus;

	[DefaultValue(1)]
	public int rate;

	[DefaultValue(10f)]
	public float special_effect_interval;

	public List<int> default_slots;

	public bool link_default_slots;

	public WorldAgeAction special_effect_action;

	internal Color pie_selection_color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return title_color;
		}
	}

	public Sprite getSprite()
	{
		if ((Object)(object)_cached_sprite == (Object)null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return _cached_sprite;
	}

	public Sprite getBackground()
	{
		if ((Object)(object)_cached_background == (Object)null)
		{
			_cached_background = SpriteTextureLoader.getSprite(path_background);
		}
		return _cached_background;
	}

	public string getLocaleID()
	{
		return id + "_title";
	}

	public string getDescriptionID()
	{
		return id + "_description";
	}

	public WorldAgeAsset()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		years_min = 35;
		years_max = 55;
		era_effect_overlay_alpha = 0.2f;
		era_effect_light_alpha_game = 1f;
		era_effect_light_alpha_minimap = 1f;
		cloud_interval = 15f;
		range_weapons_multiplier = 1f;
		temperature_damage_bonus = 1;
		force_next = string.Empty;
		flag_crops_grow = true;
		light_color = Toolbox.makeColor("#FFCE61");
		title_color = Toolbox.makeColor("#FFFFFF");
		grow_vegetation = true;
		fire_spread_rate_bonus = 1f;
		rate = 1;
		special_effect_interval = 10f;
		default_slots = new List<int>();
		base._002Ector();
	}
}
