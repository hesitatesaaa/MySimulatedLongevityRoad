using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BuildingAsset : Asset
{
	[NonSerialized]
	public bool sprites_are_initiated;

	public Vector3 scale_base;

	[DefaultValue("")]
	public string kingdom;

	[DefaultValue("")]
	public string civ_kingdom;

	public BuildingFundament fundament;

	[DefaultValue("building")]
	public string material;

	[DefaultValue("buildings")]
	public string atlas_id;

	[DefaultValue("buildings")]
	public string atlas_id_fallback_when_not_wobbly;

	[NonSerialized]
	public DynamicSpritesAsset atlas_asset;

	public bool prevent_freeze;

	public float bonus_z;

	public bool removed_by_sponge;

	[DefaultValue("")]
	public string sprite_path;

	[DefaultValue("buildings/")]
	public string main_path;

	public bool grow_creep;

	[DefaultValue(CreepWorkerMovementType.RandomNeighbourAll)]
	public CreepWorkerMovementType grow_creep_movement_type;

	[DefaultValue("")]
	public string grow_creep_type;

	public bool draw_light_area;

	public float draw_light_area_offset_x;

	public float draw_light_area_offset_y;

	[DefaultValue(0.5f)]
	public float draw_light_size;

	public int grow_creep_steps_max;

	public float grow_creep_step_interval;

	[DefaultValue(1)]
	public int grow_creep_workers;

	public bool grow_creep_direction_random_position;

	public bool grow_creep_random_new_direction;

	public bool grow_creep_flash;

	public int construction_progress_needed;

	public bool grow_creep_redraw_tile;

	[DefaultValue(7)]
	public int grow_creep_steps_before_new_direction;

	[DefaultValue(true)]
	public bool has_ruins_graphics;

	public bool has_special_animation_state;

	[DefaultValue(6f)]
	public float animation_speed;

	[DefaultValue(BuildingType.Building_None)]
	public BuildingType building_type;

	public bool sparkle_effect;

	public List<ResourceContainer> resources_given;

	public bool can_be_grown;

	[DefaultValue(0.5f)]
	public float vegetation_random_chance;

	public bool has_kingdom_color;

	public bool city_building;

	public bool can_be_abandoned;

	public bool mini_civ_auto_load;

	public bool destroy_on_liquid;

	public bool can_be_upgraded;

	[DefaultValue("")]
	public string upgrade_to;

	[DefaultValue("")]
	public string upgraded_from;

	public int upgrade_level;

	[DefaultValue("")]
	public string type;

	public bool gatherable;

	public bool wheat;

	public bool produce_biome_food;

	public float growth_time;

	public int loot_generation;

	public string[] boat_types;

	public string boat_type_fishing;

	public string boat_type_trading;

	public string boat_type_transport;

	public bool waypoint;

	public int priority;

	public BuildingStepAction step_action;

	[NonSerialized]
	public bool has_step_action;

	[DefaultValue(true)]
	public bool shadow;

	public Vector2 shadow_bound;

	[DefaultValue(0.2f)]
	public float shadow_distortion;

	public bool auto_remove_ruin;

	public bool ice_tower;

	public bool spawn_units;

	public bool beehive;

	[DefaultValue("-")]
	public string spawn_units_asset;

	public bool tower;

	[DefaultValue("")]
	public string tower_projectile;

	public float tower_projectile_offset;

	[DefaultValue(false)]
	public bool tower_attack_buildings;

	[DefaultValue(3f)]
	public float tower_projectile_reload;

	[DefaultValue(1)]
	public int tower_projectile_amount;

	public bool ignore_other_buildings_for_upgrade;

	public bool random_flip;

	public ConstructionCost cost;

	public BaseStats base_stats;

	public bool ignored_by_cities;

	public bool remove_buildings_when_dropped;

	public bool remove_civ_buildings;

	public bool ignore_same_building_id;

	public bool build_road_to;

	public bool can_be_damaged_by_tornado;

	public bool can_be_placed_on_liquid;

	public bool can_be_placed_on_blocks;

	public bool damaged_by_rain;

	public bool only_build_tiles;

	public bool build_place_borders;

	public bool build_place_single;

	public bool build_place_center;

	public bool needs_farms_ground;

	public bool build_place_batch;

	public bool build_prefer_replace_house;

	public bool check_for_close_building;

	public bool ignore_buildings;

	public bool can_be_demolished;

	public bool burnable;

	public bool affected_by_lava;

	public bool affected_by_acid;

	public bool can_units_live_here;

	public int housing_slots;

	public int housing_happiness;

	public int max_houses;

	public bool storage;

	public bool storage_only_food;

	[DefaultValue(true)]
	public bool can_be_living_house;

	[DefaultValue(true)]
	public bool can_be_living_plant;

	[DefaultValue(true)]
	public bool remove_ruins;

	[DefaultValue(true)]
	public bool has_ruin_state;

	public bool has_resources_to_collect;

	public bool has_resources_grown_to_collect;

	public bool has_resources_grown_to_collect_on_spawn;

	public bool can_be_chopped_down;

	public int book_slots;

	public BuildingOverrideMainSprites get_override_sprites_main;

	public BuildingOverrideMainSprite get_override_sprite_main;

	public bool is_vegetation;

	public bool is_stockpile;

	public Vector2 stockpile_top_left_offset;

	public Vector2 stockpile_center_offset;

	public int limit_per_zone;

	public bool become_alive_when_chopped;

	public int limit_in_radius;

	public int limit_global;

	public bool docks;

	[NonSerialized]
	public bool has_biome_tags;

	public HashSet<BiomeTag> biome_tags_growth;

	[NonSerialized]
	public bool has_biome_tags_spread;

	public HashSet<BiomeTag> biome_tags_spread;

	public bool spread_biome;

	public string spread_biome_id;

	[DefaultValue("")]
	public string group;

	public bool affected_by_drought;

	public bool affected_by_cold_temperature;

	public bool smoke;

	[DefaultValue(0.5f)]
	public float smoke_interval;

	public Vector2Int smoke_offset;

	public bool spawn_drops;

	[DefaultValue("")]
	public string spawn_drop_id;

	public float spawn_drop_interval;

	public float spawn_drop_start_height;

	public float spawn_drop_min_height;

	public float spawn_drop_max_height;

	public float spawn_drop_min_radius;

	public float spawn_drop_max_radius;

	public string transform_tiles_to_tile_type;

	public string transform_tiles_to_top_tiles;

	public string sound_spawn;

	public string sound_idle;

	public string sound_hit;

	public string sound_built;

	public string sound_destroyed;

	public int nutrition_restore;

	public bool spawn_rats;

	public bool flora;

	public FloraSize flora_size;

	public bool spread;

	public float spread_chance;

	public float spread_steps;

	public FloraType flora_type;

	public string[] spread_ids;

	public bool has_sprites_spawn;

	public bool has_sprites_main;

	public bool has_sprites_main_disabled;

	public bool has_sprites_ruin;

	public bool has_sprites_special;

	public bool has_sprite_construction;

	public bool check_for_adaptation_tags;

	public GetColorForMapIcon get_map_icon_color;

	public bool has_get_map_icon_color;

	[NonSerialized]
	public BuildingSprites building_sprites;

	[NonSerialized]
	public HashSet<Building> buildings;

	[JsonIgnore]
	public bool has_sound_spawn => sound_spawn != null;

	[JsonIgnore]
	public bool has_sound_idle => sound_idle != null;

	[JsonIgnore]
	public bool has_sound_hit => sound_hit != null;

	[JsonIgnore]
	public bool has_sound_built => sound_built != null;

	[JsonIgnore]
	public bool has_sound_destroyed => sound_destroyed != null;

	public BuildingAsset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		scale_base = new Vector3(0.25f, 0.25f, 0.25f);
		kingdom = string.Empty;
		civ_kingdom = string.Empty;
		material = "building";
		atlas_id = "buildings";
		atlas_id_fallback_when_not_wobbly = "buildings";
		sprite_path = string.Empty;
		main_path = "buildings/";
		grow_creep_type = string.Empty;
		draw_light_size = 0.5f;
		grow_creep_workers = 1;
		grow_creep_steps_before_new_direction = 7;
		has_ruins_graphics = true;
		animation_speed = 6f;
		vegetation_random_chance = 0.5f;
		upgrade_to = string.Empty;
		upgraded_from = string.Empty;
		type = string.Empty;
		shadow = true;
		shadow_bound = new Vector2(0.5f, 0.8f);
		shadow_distortion = 0.2f;
		spawn_units_asset = "-";
		tower_projectile = string.Empty;
		tower_projectile_reload = 3f;
		tower_projectile_amount = 1;
		can_be_living_house = true;
		can_be_living_plant = true;
		remove_ruins = true;
		has_ruin_state = true;
		group = string.Empty;
		smoke_interval = 0.5f;
		spawn_drop_id = "";
		buildings = new HashSet<Building>();
		base._002Ector();
	}

	public bool setSpread(FloraType pType, int pSpreadSteps = 1, float pSpreadChance = 1f)
	{
		if (pType == FloraType.None)
		{
			spread = false;
			return false;
		}
		spread = true;
		flora_type = pType;
		spread_steps = pSpreadSteps;
		spread_chance = pSpreadChance;
		return true;
	}

	public void setAtlasID(string pAtlasID, string pFallbackID = null)
	{
		if (pFallbackID == null)
		{
			pFallbackID = pAtlasID;
		}
		atlas_id = pAtlasID;
		atlas_id_fallback_when_not_wobbly = pFallbackID;
	}

	public void setShadow(float pBoundX, float pBoundY, float pDistortion)
	{
		shadow = true;
		shadow_bound.x = pBoundX;
		shadow_bound.y = pBoundY;
		shadow_distortion = pDistortion;
	}

	public bool isOverlaysBiomeTags(TileTypeBase pTileType)
	{
		if (!has_biome_tags)
		{
			return true;
		}
		return pTileType.overlapsBiomeTags(biome_tags_growth);
	}

	public bool isOverlaysBiomeSpreadTags(TileTypeBase pTileType)
	{
		if (!has_biome_tags_spread)
		{
			return false;
		}
		return pTileType.overlapsBiomeTags(biome_tags_spread);
	}

	public void checkLimits(Building pBuildingToIgnore = null)
	{
		if (limit_global == 0 || buildings.Count < limit_global)
		{
			return;
		}
		int num = buildings.Count - limit_global;
		foreach (Building building in buildings)
		{
			if (num == 0)
			{
				break;
			}
			if ((pBuildingToIgnore == null || pBuildingToIgnore != building) && building.isAlive())
			{
				building.startDestroyBuilding();
				num--;
			}
		}
	}

	public bool canBeOccupied()
	{
		if (!hasHousingSlots() && !docks)
		{
			return spawn_units;
		}
		return true;
	}

	public void addResource(string pID, int pAmount, bool pNewList = false)
	{
		if ((resources_given == null) | pNewList)
		{
			resources_given = new List<ResourceContainer>();
		}
		resources_given.Add(new ResourceContainer(pID, pAmount));
	}

	public bool hasResourceGiven(string pID)
	{
		if (resources_given == null)
		{
			return false;
		}
		foreach (ResourceContainer item in resources_given)
		{
			if (item.id == pID)
			{
				return true;
			}
		}
		return false;
	}

	public ActorAsset getRandomBoatAssetToBuild(City pCity)
	{
		string random = boat_types.GetRandom();
		string boatAssetIDFromType = getBoatAssetIDFromType(random, pCity);
		if (string.IsNullOrEmpty(boatAssetIDFromType))
		{
			return null;
		}
		return AssetManager.actor_library.get(boatAssetIDFromType);
	}

	public void setHousingSlots(int pValue)
	{
		can_units_live_here = true;
		housing_slots = pValue;
	}

	public bool hasHousingSlots()
	{
		return housing_slots > 0;
	}

	private string getBoatAssetIDFromType(string pSpeciesBoat, City pCity)
	{
		if (pCity == null)
		{
			return "boat_fishing";
		}
		ArchitectureAsset architecture_asset = pCity.getActorAsset().architecture_asset;
		return pSpeciesBoat switch
		{
			"boat_type_fishing" => architecture_asset.actor_asset_id_boat_fishing, 
			"boat_type_trading" => architecture_asset.actor_asset_id_trading, 
			"boat_type_transport" => architecture_asset.actor_asset_id_transport, 
			_ => architecture_asset.actor_asset_id_boat_fishing, 
		};
	}

	public void checkSpritesAreLoaded()
	{
		if (!sprites_are_initiated)
		{
			sprites_are_initiated = true;
			loadBuildingSprites();
		}
	}

	public void loadBuildingSprites()
	{
		Sprite[] array = loadBuildingSpriteList();
		PreloadHelpers.total_building_sprites += array.Length;
		PreloadHelpers.all_preloaded_sprites_buildings.AddRange(array);
		BuildingSprites buildingSprites = (building_sprites = new BuildingSprites());
		PreloadHelpers.total_building_sprite_containers++;
		foreach (Sprite val in array)
		{
			string[] array2 = ((Object)val).name.Split('_');
			string text = array2[0];
			int num = int.Parse(array2[1]);
			while (buildingSprites.animation_data.Count < num + 1)
			{
				buildingSprites.animation_data.Add(null);
			}
			if (buildingSprites.animation_data[num] == null)
			{
				buildingSprites.animation_data[num] = new BuildingAnimationData();
			}
			BuildingAnimationData buildingAnimationData = building_sprites.animation_data[num];
			bool pIsContructionSprite = false;
			switch (text)
			{
			case "main":
			{
				BuildingAnimationData buildingAnimationData2 = buildingAnimationData;
				if (buildingAnimationData2.list_main == null)
				{
					buildingAnimationData2.list_main = new ListPool<Sprite>();
				}
				buildingAnimationData.list_main.Add(val);
				if (buildingAnimationData.list_main.Count > 1)
				{
					buildingAnimationData.animated = true;
				}
				break;
			}
			case "disabled":
			{
				BuildingAnimationData buildingAnimationData2 = buildingAnimationData;
				if (buildingAnimationData2.list_main_disabled == null)
				{
					buildingAnimationData2.list_main_disabled = new ListPool<Sprite>();
				}
				buildingAnimationData.list_main_disabled.Add(val);
				if (buildingAnimationData.list_main_disabled.Count > 1)
				{
					buildingAnimationData.animated = true;
				}
				break;
			}
			case "spawn":
			{
				BuildingAnimationData buildingAnimationData2 = buildingAnimationData;
				if (buildingAnimationData2.list_spawn == null)
				{
					buildingAnimationData2.list_spawn = new ListPool<Sprite>();
				}
				buildingAnimationData.list_spawn.Add(val);
				if (buildingAnimationData.list_spawn.Count > 1)
				{
					buildingAnimationData.animated = true;
				}
				break;
			}
			case "ruin":
			{
				BuildingAnimationData buildingAnimationData2 = buildingAnimationData;
				if (buildingAnimationData2.list_ruins == null)
				{
					buildingAnimationData2.list_ruins = new ListPool<Sprite>();
				}
				buildingAnimationData.list_ruins.Add(val);
				break;
			}
			case "construction":
				building_sprites.construction = val;
				pIsContructionSprite = true;
				break;
			case "special":
			{
				BuildingAnimationData buildingAnimationData2 = buildingAnimationData;
				if (buildingAnimationData2.list_special == null)
				{
					buildingAnimationData2.list_special = new ListPool<Sprite>();
				}
				buildingAnimationData.list_special.Add(val);
				break;
			}
			case "mini":
				building_sprites.map_icon = new BuildingMapIcon(val);
				break;
			}
			if (shadow)
			{
				DynamicSpriteCreator.createBuildingShadow(this, val, pIsContructionSprite);
			}
		}
		foreach (BuildingAnimationData animation_datum in building_sprites.animation_data)
		{
			animation_datum.main = animation_datum.list_main?.ToArray();
			animation_datum.spawn = animation_datum.list_spawn?.ToArray();
			animation_datum.main_disabled = animation_datum.list_main_disabled?.ToArray();
			animation_datum.ruins = animation_datum.list_ruins?.ToArray();
			animation_datum.special = animation_datum.list_special?.ToArray();
			animation_datum.list_main?.Dispose();
			animation_datum.list_spawn?.Dispose();
			animation_datum.list_main_disabled?.Dispose();
			animation_datum.list_ruins?.Dispose();
			animation_datum.list_special?.Dispose();
			animation_datum.list_main = null;
			animation_datum.list_spawn = null;
			animation_datum.list_main_disabled = null;
			animation_datum.list_ruins = null;
			animation_datum.list_special = null;
		}
	}

	public Sprite[] loadBuildingSpriteList()
	{
		string text = sprite_path;
		if (string.IsNullOrEmpty(text))
		{
			text = main_path + id;
		}
		return SpriteTextureLoader.getSpriteList(text);
	}
}
