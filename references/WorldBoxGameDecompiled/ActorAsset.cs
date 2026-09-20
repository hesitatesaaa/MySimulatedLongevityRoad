using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ActorAsset : BaseUnlockableAsset, IDescriptionAsset, ILocalizedAsset, IAnimationFrames
{
	[DefaultValue(true)]
	public bool split_ai_update = true;

	[DefaultValue(true)]
	public bool has_ai_system = true;

	[DefaultValue(1)]
	public int item_making_skill = 1;

	public bool affected_by_dust;

	public string sound_idle;

	public string sound_idle_loop;

	public string sound_spawn;

	public string sound_death;

	public string sound_attack;

	[DefaultValue("event:/SFX/HIT/HitGeneric")]
	public string sound_hit = "event:/SFX/HIT/HitGeneric";

	public bool show_controllable_tip = true;

	public bool show_task_icon = true;

	public string music_theme;

	public string music_theme_civ;

	[DefaultValue(UnitTextureAtlasID.Units)]
	public UnitTextureAtlasID texture_atlas;

	public bool ignored_by_infinity_coin;

	[DefaultValue("")]
	public string name_taxonomic_kingdom = "";

	[DefaultValue("")]
	public string name_taxonomic_phylum = "";

	[DefaultValue("")]
	public string name_taxonomic_class = "";

	[DefaultValue("")]
	public string name_taxonomic_order = "";

	[DefaultValue("")]
	public string name_taxonomic_family = "";

	[DefaultValue("")]
	public string name_taxonomic_genus = "";

	[DefaultValue("")]
	public string name_taxonomic_species = "";

	[DefaultValue(true)]
	public bool name_subspecies_add_biome_suffix = true;

	public bool auto_civ;

	[DefaultValue("")]
	public string name_locale = "";

	[DefaultValue(StatusTier.Advanced)]
	public StatusTier allowed_status_tiers = StatusTier.Advanced;

	[DefaultValue(true)]
	public bool render_status_effects = true;

	[DefaultValue(ActorSize.S13_Human)]
	public ActorSize actor_size = ActorSize.S13_Human;

	public string[] animation_walk;

	[DefaultValue(10f)]
	public float animation_walk_speed = 10f;

	public string[] animation_swim;

	[DefaultValue(8f)]
	public float animation_swim_speed = 8f;

	public string[] animation_idle = ActorAnimationSequences.walk_0;

	[DefaultValue(10f)]
	public float animation_idle_speed = 10f;

	[DefaultValue(10f)]
	public float max_shake_timer = 10f;

	[DefaultValue(true)]
	public bool animation_speed_based_on_walk_speed = true;

	[DefaultValue("base_attack")]
	public string default_attack = "base_attack";

	public bool immune_to_tumor;

	public bool immune_to_slowness;

	public int aggression;

	[DefaultValue(true)]
	public bool shadow = true;

	[DefaultValue("unitShadow_5")]
	public string shadow_texture = "unitShadow_5";

	[DefaultValue("unitShadow_2")]
	public string shadow_texture_egg = "unitShadow_2";

	[DefaultValue("unitShadow_4")]
	public string shadow_texture_baby = "unitShadow_4";

	[DefaultValue(true)]
	public bool hit_fx_alternative_offset = true;

	[DefaultValue(true)]
	public bool can_level_up = true;

	[DefaultValue(true)]
	public bool can_talk_with = true;

	public float base_throwing_range;

	[DefaultValue(true)]
	public bool use_tool_items = true;

	public bool use_items;

	public bool take_items;

	public bool control_can_jump = true;

	public bool control_can_talk = true;

	public bool control_can_dash = true;

	public bool control_can_backstep = true;

	public bool control_can_steal = true;

	public bool control_can_swear = true;

	public bool control_can_kick = true;

	public bool use_phenotypes;

	[JsonIgnore]
	public Dictionary<string, List<string>> phenotypes_dict;

	public List<string> phenotypes_list;

	public List<string> generated_subspecies_names_prefixes;

	public bool can_be_killed_by_stuff;

	public bool can_be_killed_by_life_eraser;

	public bool can_be_killed_by_divine_light;

	[DefaultValue(true)]
	public bool show_on_meta_layer = true;

	public bool ignore_tile_speed_multiplier;

	public bool skip_fight_logic;

	public bool can_attack_buildings;

	public bool can_attack_brains;

	[DefaultValue(true)]
	public bool count_as_unit = true;

	public bool only_melee_attack;

	public bool flag_ufo;

	public bool flag_finger;

	public bool flag_turtle;

	public bool default_animal;

	public bool civ;

	public bool unit_other;

	[DefaultValue("")]
	public string kingdom_id_wild = "";

	[DefaultValue("")]
	public string kingdom_id_civilization = "";

	public bool special;

	[DefaultValue(true)]
	public bool show_in_taxonomy_tooltip = true;

	[DefaultValue(true)]
	public bool render_budding = true;

	public string family_banner_frame_generation_exclusion = "families/frame_11";

	public string family_banner_frame_generation_inclusion;

	public bool family_banner_frame_only_inclusion;

	[DefaultValue("")]
	public string texture_id = "";

	[DefaultValue("")]
	public string architecture_id = "";

	public string texture_path_zombie_for_auto_loader_main;

	public string texture_path_zombie_for_auto_loader_heads;

	public ActorTextureSubAsset texture_asset;

	public bool prevent_unconscious_rotation;

	public bool render_heads_for_babies;

	private string _debug_phenotype_color = "";

	public bool body_separate_part_hands;

	public bool has_baby_form;

	public bool has_advanced_textures;

	public List<string> decision_ids;

	private DecisionAsset[] _cached_assets_decisions;

	private int _cached_assets_decisions_counter;

	private HashSet<SubspeciesTrait> _cached_assets_subspecies_traits;

	private Sprite _cached_sprite;

	private BaseStats _cached_overview_stats;

	[DefaultValue(0.5f)]
	public float hovering_min = 0.5f;

	[DefaultValue(1.2f)]
	public float hovering_max = 1.2f;

	public bool hovering;

	public bool flying;

	public bool very_high_flyer;

	public bool disable_jump_animation;

	public bool rotating_animation;

	[DefaultValue(true)]
	public bool die_on_blocks = true;

	public bool ignore_blocks;

	[DefaultValue(true)]
	public bool move_from_block = true;

	[DefaultValue(true)]
	public bool run_to_water_when_on_fire = true;

	public bool damaged_by_ocean;

	[DefaultValue(true)]
	public bool cancel_beh_on_land = true;

	public bool force_ocean_creature;

	public bool force_land_creature;

	public bool is_humanoid;

	public bool is_boat;

	public bool is_boat_transport;

	public bool draw_boat_mark;

	public bool draw_boat_mark_big;

	[DefaultValue("")]
	public string boat_type = "";

	[DefaultValue(6)]
	public int animal_breeding_close_units_limit = 6;

	[DefaultValue("")]
	public string avatar_prefab = "";

	[NonSerialized]
	public bool has_avatar_prefab;

	public bool ignore_generic_render;

	public bool need_colored_sprite;

	public bool die_from_dispel;

	[DefaultValue(true)]
	public bool die_in_lava = true;

	public bool can_be_moved_by_powers;

	public bool can_be_hurt_by_powers;

	public bool can_turn_into_ice_one;

	public bool can_turn_into_mush;

	public bool can_turn_into_tumor;

	public bool can_evolve_into_new_species;

	public bool has_soul;

	[DefaultValue(true)]
	public bool can_receive_traits = true;

	public string base_asset_id;

	public string power_id;

	public bool zombie_auto_asset;

	public bool can_turn_into_zombie;

	[DefaultValue("")]
	public string zombie_id_internal = "";

	public string zombie_color_hex = "#3B8130";

	public bool unit_zombie;

	public bool dynamic_sprite_zombie;

	[DefaultValue("")]
	public string skeleton_id = "";

	[DefaultValue("")]
	public string mush_id = "";

	[DefaultValue("")]
	public string tumor_id = "";

	[DefaultValue("")]
	public string evolution_id = "";

	public bool can_turn_into_demon_in_age_of_chaos;

	public bool show_icon_inspect_window;

	[DefaultValue("")]
	public string show_icon_inspect_window_id = "";

	public bool hide_favorite_icon;

	[DefaultValue(true)]
	public bool can_be_favorited = true;

	public bool can_be_inspected;

	[DefaultValue(2.5f)]
	public float inspect_avatar_scale = 2.5f;

	public float inspect_avatar_offset_x;

	public float inspect_avatar_offset_y;

	[DefaultValue(100)]
	public int nutrition_max = 100;

	[DefaultValue(3)]
	public int months_breeding_timeout = 3;

	[DefaultValue(18)]
	public int age_spawn = 18;

	[DefaultValue(true)]
	public bool can_edit_traits = true;

	[DefaultValue(false)]
	public bool can_edit_equipment;

	[DefaultValue(true)]
	public bool finish_scale_on_creation = true;

	[DefaultValue(2f)]
	public float path_movement_timeout = 2f;

	public bool source_meat;

	public bool source_meat_insect;

	[DefaultValue(0.3f)]
	public float default_height = 0.3f;

	public bool update_z;

	public bool visible_on_minimap;

	public bool follow_herd;

	[DefaultValue(true)]
	public bool inspect_stats = true;

	[DefaultValue(true)]
	public bool inspect_children = true;

	[DefaultValue(true)]
	public bool inspect_generation = true;

	[DefaultValue(true)]
	public bool inspect_sex = true;

	[DefaultValue(true)]
	public bool inspect_kills = true;

	[DefaultValue(true)]
	public bool inspect_experience = true;

	[DefaultValue(true)]
	public bool inspect_show_species = true;

	[DefaultValue(true)]
	public bool inspect_mind = true;

	[DefaultValue(true)]
	public bool inspect_genealogy = true;

	[DefaultValue(true)]
	public bool allow_possession = true;

	[DefaultValue(true)]
	public bool allow_strange_urge_movement = true;

	public bool inspect_home;

	public bool immune_to_injuries;

	[DefaultValue(true)]
	public bool can_be_cloned = true;

	[DefaultValue(10)]
	public int experience_given = 10;

	public string[] job;

	public string[] job_citizen;

	public string[] job_kingdom;

	public string[] job_baby;

	public string[] job_attacker;

	public string effect_cast_top = "fx_cast_top_blue";

	public string effect_cast_ground = "fx_cast_ground_blue";

	public string effect_teleport = "fx_teleport_blue";

	public List<string> spell_ids;

	public bool effect_damage;

	public bool can_flip;

	public bool special_dead_animation;

	public bool death_animation_angle;

	[DefaultValue(StatusTier.Advanced)]
	public StatusTier status_tiers = StatusTier.Advanced;

	[DefaultValue(true)]
	public bool has_sprite_renderer = true;

	public bool die_by_lightning;

	[DefaultValue(true)]
	public bool has_skin = true;

	[DefaultValue("")]
	public string grow_into_id = "";

	[DefaultValue("iconQuestionMark")]
	public string icon = "iconQuestionMark";

	public bool skip_save;

	public string color_hex;

	[NonSerialized]
	public Color32? color;

	public ConstructionCost cost;

	[DefaultValue(40)]
	public int species_spawn_radius = 40;

	public bool can_have_subspecies;

	public int genome_size;

	[DefaultValue(30)]
	public int family_spawn_radius = 30;

	[DefaultValue(20)]
	public int family_limit = 20;

	public bool create_family_at_spawn;

	[DefaultValue(FamilyParentsMode.Normal)]
	public FamilyParentsMode family_show_parents;

	[DefaultValue("COLLECTIVE_NAME")]
	public string collective_term = "COLLECTIVE_NAME";

	[DefaultValue(50)]
	public int language_spawn_radius = 50;

	public List<string> traits;

	public HashSet<string> traits_ignore;

	public List<string> preferred_attribute;

	[DefaultValue(null)]
	public HashSet<string> preferred_colors;

	public string[] production;

	[DefaultValue(null)]
	public string[] name_template_sets;

	[DefaultValue("default_name")]
	public string name_template_unit = "default_name";

	[DefaultValue("")]
	public string banner_id = "";

	[DefaultValue("")]
	public string build_order_template_id = "";

	[DefaultValue(4)]
	public int civ_base_cities = 4;

	[DefaultValue(0.35f)]
	public float civ_base_army_multiplier = 0.35f;

	public List<string> default_subspecies_traits;

	public List<string> default_clan_traits;

	public List<string> default_culture_traits;

	public List<string> default_language_traits;

	public List<string> default_kingdom_traits;

	public List<string> default_religion_traits;

	public List<string> trait_filter_subspecies;

	public List<string> trait_group_filter_subspecies;

	public List<ResourceContainer> resources_given;

	public string[] skin_citizen_male;

	public string[] skin_citizen_female;

	public string[] skin_warrior;

	public string[] default_weapons;

	public ActorGetSprite get_override_sprite;

	[NonSerialized]
	public bool has_override_sprite;

	public ActorGetSprites get_override_avatar_frames;

	[NonSerialized]
	public bool has_override_avatar_frames;

	public List<string> chromosomes_first;

	public HashSet<GenomePart> genome_parts = new HashSet<GenomePart>();

	[DefaultValue(3)]
	public int max_random_amount = 3;

	[DefaultValue(true)]
	public bool can_be_surprised = true;

	[NonSerialized]
	public HashSet<Actor> units = new HashSet<Actor>();

	[NonSerialized]
	public ArchitectureAsset architecture_asset;

	[NonSerialized]
	public SpellHolder spells;

	public BaseActionActor action_on_load;

	public WorldAction action_click;

	public WorldAction action_death;

	public DeadAnimation action_dead_animation;

	public GetHitAction action_get_hit;

	public WorldAction check_flip;

	public bool force_hide_stamina;

	public bool force_hide_mana;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_actors;

	[JsonIgnore]
	public bool has_sound_idle => sound_idle != null;

	[JsonIgnore]
	public bool has_sound_idle_loop => sound_idle_loop != null;

	[JsonIgnore]
	public bool has_sound_spawn => sound_spawn != null;

	[JsonIgnore]
	public bool has_sound_death => sound_death != null;

	[JsonIgnore]
	public bool has_sound_attack => sound_attack != null;

	[JsonIgnore]
	public bool has_sound_hit => sound_hit != null;

	[JsonIgnore]
	public bool has_music_theme => music_theme != null;

	[JsonIgnore]
	public bool has_music_theme_civ => music_theme_civ != null;

	[JsonIgnore]
	public string debug_phenotype_colors
	{
		get
		{
			if (string.IsNullOrEmpty(_debug_phenotype_color))
			{
				_debug_phenotype_color = phenotypes_list?.GetRandom();
			}
			return _debug_phenotype_color;
		}
		set
		{
			_debug_phenotype_color = value;
		}
	}

	[DefaultValue("male_1")]
	public string skin_civ_default_male => "male_1";

	[DefaultValue("female_1")]
	public string skin_civ_default_female => "female_1";

	public int decisions_counter => _cached_assets_decisions_counter;

	[JsonIgnore]
	public string boat_texture_id => id;

	public void addSubspeciesNamePrefix(string pName)
	{
		if (generated_subspecies_names_prefixes == null)
		{
			generated_subspecies_names_prefixes = new List<string>();
		}
		generated_subspecies_names_prefixes.Add(pName);
	}

	public bool hasDefaultEggForm()
	{
		return default_subspecies_traits?.Contains("reproduction_strategy_oviparity") ?? false;
	}

	public string getDefaultEggID()
	{
		string result = "egg_shell_plain";
		foreach (string default_subspecies_trait in default_subspecies_traits)
		{
			SubspeciesTrait subspeciesTrait = AssetManager.subspecies_traits.get(default_subspecies_trait);
			if (subspeciesTrait.phenotype_egg)
			{
				result = subspeciesTrait.id_egg;
				break;
			}
		}
		return result;
	}

	public void setZombie(bool pAnimal = true)
	{
		if (id != "zombie")
		{
			base_asset_id = "zombie";
		}
		needs_to_be_explored = false;
		use_phenotypes = false;
		name_locale = "Zombie";
		can_attack_brains = true;
		kingdom_id_wild = "undead";
		collective_term = "group_swarm";
		kingdom_id_civilization = string.Empty;
		architecture_id = string.Empty;
		build_order_template_id = string.Empty;
		take_items = false;
		follow_herd = false;
		can_be_killed_by_divine_light = true;
		unit_zombie = true;
		has_baby_form = false;
		has_advanced_textures = false;
		unlocked_with_achievement = false;
		achievement_id = null;
		addTrait("zombie");
		addTrait("stupid");
		can_turn_into_zombie = false;
		can_turn_into_mush = false;
		base_stats["lifespan"] = 0f;
		zombie_id_internal = string.Empty;
		job = Toolbox.a<string>("decision");
		can_evolve_into_new_species = false;
		addDecision("attack_golden_brain");
		if (traits != null)
		{
			traits = traits.FindAll((string pTrait) => !AssetManager.traits.get(pTrait).remove_for_zombie_actor_asset);
		}
		if (default_subspecies_traits != null)
		{
			default_subspecies_traits = default_subspecies_traits.FindAll((string pTrait) => !AssetManager.subspecies_traits.get(pTrait).remove_for_zombies);
		}
		default_kingdom_traits = null;
		default_culture_traits = null;
		default_clan_traits = null;
		default_language_traits = null;
		default_religion_traits = null;
		addTraitGroupFilter("advanced_brain");
		addTraitGroupFilter("reproduction_strategy");
		addTraitGroupFilter("reproductive_methods");
		addTraitGroupFilter("eggs");
		addTraitGroupFilter("harmony");
		if (pAnimal)
		{
			generateFmodPaths("zombie_animal");
		}
		else
		{
			generateFmodPaths("zombie");
		}
		music_theme = "Units_Zombie";
		sound_hit = "event:/SFX/HIT/HitFlesh";
	}

	public void setCanTurnIntoZombieAsset(string pZombieID, bool pAutoZombieAsset)
	{
		can_turn_into_zombie = true;
		zombie_auto_asset = pAutoZombieAsset;
		zombie_id_internal = pZombieID;
	}

	public string getZombieID()
	{
		if (zombie_auto_asset)
		{
			return zombie_id_internal + "_" + id;
		}
		return zombie_id_internal;
	}

	public void cloneTaxonomyFromForSapiens(string pFrom)
	{
		ActorAsset actorAsset = AssetManager.actor_library.get(pFrom);
		name_taxonomic_kingdom = actorAsset.name_taxonomic_kingdom;
		name_taxonomic_phylum = actorAsset.name_taxonomic_phylum;
		name_taxonomic_class = actorAsset.name_taxonomic_class;
		name_taxonomic_order = actorAsset.name_taxonomic_order;
		name_taxonomic_family = actorAsset.name_taxonomic_family;
		name_taxonomic_genus = actorAsset.name_taxonomic_genus;
		name_taxonomic_species = "sapiens";
	}

	public string getTaxonomyRank(string pType)
	{
		return pType switch
		{
			"taxonomy_kingdom" => name_taxonomic_kingdom, 
			"taxonomy_phylum" => name_taxonomic_phylum, 
			"taxonomy_class" => name_taxonomic_class, 
			"taxonomy_order" => name_taxonomic_order, 
			"taxonomy_family" => name_taxonomic_family, 
			"taxonomy_genus" => name_taxonomic_genus, 
			"taxonomy_species" => name_taxonomic_species, 
			_ => string.Empty, 
		};
	}

	public bool isTaxonomyRank(string pType, string pID)
	{
		return getTaxonomyRank(pType) == pID;
	}

	public void setSocialStructure(string pTerm, int pLimit, bool pCreateOnSpawn = true, bool pFollowHerd = true, FamilyParentsMode pShowParents = FamilyParentsMode.Alpha)
	{
		collective_term = pTerm;
		family_limit = pLimit;
		create_family_at_spawn = pCreateOnSpawn;
		family_show_parents = pShowParents;
		follow_herd = pFollowHerd;
	}

	public void generateFmodPaths(string pID)
	{
		string text = "event:/SFX/UNITS/" + pID;
		if (!has_sound_attack)
		{
			sound_attack = text + "/attack";
		}
		if (!has_sound_death)
		{
			sound_death = text + "/death";
		}
		if (!has_sound_idle)
		{
			sound_idle = text + "/idle";
		}
		if (!has_sound_spawn)
		{
			sound_spawn = text + "/spawn";
		}
	}

	public void clonePhenotype(string pFrom)
	{
		ActorAsset actorAsset = AssetManager.actor_library.get(pFrom);
		if (actorAsset.phenotypes_dict != null)
		{
			phenotypes_dict = new Dictionary<string, List<string>>(actorAsset.phenotypes_dict);
			phenotypes_list = new List<string>(actorAsset.phenotypes_list);
		}
	}

	public PhenotypeAsset getDefaultPhenotypeAsset()
	{
		string pID = phenotypes_list[0];
		return AssetManager.phenotype_library.get(pID);
	}

	public void clearTraits()
	{
		if (traits != null)
		{
			traits.Clear();
		}
	}

	public string getCollectiveTermID()
	{
		return collective_term;
	}

	public override string getLocaleID()
	{
		return name_locale.Underscore();
	}

	public string getDescriptionID()
	{
		return getGodPower()?.getDescriptionID();
	}

	public string getLocalizedName()
	{
		if (!isAvailable())
		{
			return LocalizedTextManager.getText("achievement_tip_hidden");
		}
		return LocalizedTextManager.getText(getLocaleID());
	}

	public string getLocalizedDescription()
	{
		if (!isAvailable())
		{
			if (unlocked_with_achievement)
			{
				string text = LocalizedTextManager.getText("actor_locked_tooltip_text_achievement");
				string newValue = "<color=#00ffffff>" + getAchievementLocaleID().Localize() + "</color>";
				return text.Replace("$achievement_id$", newValue);
			}
			return LocalizedTextManager.getText("actor_locked_tooltip_text_exploration");
		}
		return LocalizedTextManager.getText(getDescriptionID());
	}

	public void addPreferredColors(params string[] pColors)
	{
		preferred_colors = new HashSet<string>(pColors);
	}

	public string getTranslatedName()
	{
		return getLocaleID().Localize();
	}

	public void addGenome(params (string, float)[] pListGenomePartsIDs)
	{
		for (int i = 0; i < pListGenomePartsIDs.Length; i++)
		{
			(string, float) tuple = pListGenomePartsIDs[i];
			string item = tuple.Item1;
			float item2 = tuple.Item2;
			GenomePart genomePart = new GenomePart(item, item2);
			if (!genome_parts.Add(genomePart))
			{
				genome_parts.TryGetValue(genomePart, out var actualValue);
				GenomePart item3 = new GenomePart(item, actualValue.value + item2);
				genome_parts.Remove(actualValue);
				genome_parts.Add(item3);
			}
		}
	}

	public string getIconPath()
	{
		return "ui/Icons/" + icon;
	}

	public Sprite getSpriteIcon()
	{
		if (_cached_sprite == null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(getIconPath());
		}
		return _cached_sprite;
	}

	public override Sprite getSprite()
	{
		return getSpriteIcon();
	}

	public bool hasBiomePhenotype(string pBiomeID)
	{
		if (phenotypes_dict == null || phenotypes_dict.Count == 0)
		{
			return false;
		}
		return phenotypes_dict.ContainsKey(pBiomeID);
	}

	public BaseStats getStatsForOverview()
	{
		if (_cached_overview_stats == null)
		{
			_cached_overview_stats = new BaseStats();
			_cached_overview_stats["health"] = base_stats["health"];
			_cached_overview_stats["lifespan"] = base_stats["lifespan"];
			_cached_overview_stats["damage"] = base_stats["damage"];
			_cached_overview_stats["speed"] = base_stats["speed"];
			foreach (GenomePart genome_part in genome_parts)
			{
				switch (genome_part.id)
				{
				case "health":
				case "lifespan":
				case "damage":
				case "speed":
					_cached_overview_stats[genome_part.id] += genome_part.value;
					break;
				}
			}
		}
		return _cached_overview_stats;
	}

	public bool hasDecisions()
	{
		List<string> list = decision_ids;
		if (list == null)
		{
			return false;
		}
		return list.Count > 0;
	}

	public DecisionAsset[] getDecisions()
	{
		if (hasDecisions() && _cached_assets_decisions == null)
		{
			_cached_assets_decisions = new DecisionAsset[64];
			foreach (string decision_id in decision_ids)
			{
				DecisionAsset decisionAsset = AssetManager.decisions_library.get(decision_id);
				if (decisionAsset != null)
				{
					_cached_assets_decisions[_cached_assets_decisions_counter++] = decisionAsset;
				}
			}
		}
		return _cached_assets_decisions;
	}

	public string getDefaultKingdom()
	{
		return kingdom_id_wild;
	}

	public HashSet<SubspeciesTrait> getDefaultSubspeciesTraits()
	{
		if (default_subspecies_traits == null)
		{
			return null;
		}
		if (_cached_assets_subspecies_traits == null)
		{
			_cached_assets_subspecies_traits = new HashSet<SubspeciesTrait>();
			default_subspecies_traits.Sort((string pS1, string pS2) => string.Compare(pS1, pS2, StringComparison.Ordinal));
			foreach (string default_subspecies_trait in default_subspecies_traits)
			{
				SubspeciesTrait subspeciesTrait = AssetManager.subspecies_traits.get(default_subspecies_trait);
				if (subspeciesTrait != null)
				{
					_cached_assets_subspecies_traits.Add(subspeciesTrait);
				}
			}
		}
		return _cached_assets_subspecies_traits;
	}

	public int countPopulation()
	{
		return units.Count;
	}

	public int countSubspecies()
	{
		int num = 0;
		foreach (Subspecies subspecy in World.world.subspecies)
		{
			if (subspecy.getActorAsset() == this)
			{
				num++;
			}
		}
		return num;
	}

	public int countFamilies()
	{
		int num = 0;
		foreach (Family family in World.world.families)
		{
			if (family.getActorAsset() == this)
			{
				num++;
			}
		}
		return num;
	}

	public void addSpell(string pID)
	{
		if (spell_ids == null)
		{
			spell_ids = new List<string>();
		}
		spell_ids.Add(pID);
	}

	public void addTraitGroupFilter(string pTrait)
	{
		if (trait_group_filter_subspecies == null)
		{
			trait_group_filter_subspecies = new List<string>();
		}
		if (!trait_group_filter_subspecies.Contains(pTrait))
		{
			trait_group_filter_subspecies.Add(pTrait);
		}
	}

	public void addTrait(string pTraitID)
	{
		if (traits == null)
		{
			traits = new List<string>();
		}
		if (!traits.Contains(pTraitID))
		{
			traits.Add(pTraitID);
		}
	}

	public void addTraitIgnore(string pTraitID)
	{
		if (traits_ignore == null)
		{
			traits_ignore = new HashSet<string>();
		}
		traits_ignore.Add(pTraitID);
	}

	public void removeTrait(string pTrait)
	{
		traits?.Remove(pTrait);
		List<string> list = traits;
		if (list != null && list.Count == 0)
		{
			traits = null;
		}
	}

	public void addSubspeciesTrait(string pTrait)
	{
		if (default_subspecies_traits == null)
		{
			default_subspecies_traits = new List<string>();
		}
		if (!default_subspecies_traits.Contains(pTrait))
		{
			default_subspecies_traits.Add(pTrait);
		}
	}

	public void addCultureTrait(string pTrait)
	{
		if (default_culture_traits == null)
		{
			default_culture_traits = new List<string>();
		}
		if (!default_culture_traits.Contains(pTrait))
		{
			default_culture_traits.Add(pTrait);
		}
	}

	public void addLanguageTrait(string pTrait)
	{
		if (default_language_traits == null)
		{
			default_language_traits = new List<string>();
		}
		if (!default_language_traits.Contains(pTrait))
		{
			default_language_traits.Add(pTrait);
		}
	}

	public void addKingdomTrait(string pTrait)
	{
		if (default_kingdom_traits == null)
		{
			default_kingdom_traits = new List<string>();
		}
		if (!default_kingdom_traits.Contains(pTrait))
		{
			default_kingdom_traits.Add(pTrait);
		}
	}

	public void addClanTrait(string pTrait)
	{
		if (default_clan_traits == null)
		{
			default_clan_traits = new List<string>();
		}
		if (!default_clan_traits.Contains(pTrait))
		{
			default_clan_traits.Add(pTrait);
		}
	}

	public void addReligionTrait(string pTrait)
	{
		if (default_religion_traits == null)
		{
			default_religion_traits = new List<string>();
		}
		if (!default_religion_traits.Contains(pTrait))
		{
			default_religion_traits.Add(pTrait);
		}
	}

	public void addDecision(string pDecision)
	{
		if (decision_ids == null)
		{
			decision_ids = new List<string>();
		}
		if (!decision_ids.Contains(pDecision))
		{
			decision_ids.Add(pDecision);
		}
	}

	public override bool unlock(bool pSaveData = true)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!base.unlock(pSaveData))
		{
			return false;
		}
		string text = (string.IsNullOrEmpty(base_asset_id) ? id : base_asset_id);
		ActorAsset actorAsset;
		if (text != id)
		{
			actorAsset = AssetManager.actor_library.get(text);
			if (!actorAsset.unlock())
			{
				return false;
			}
		}
		else
		{
			actorAsset = this;
		}
		if (PowerButton.actor_spawn_buttons.TryGetValue(actorAsset, out var value))
		{
			((Graphic)value.icon).color = Toolbox.color_white;
		}
		return true;
	}

	protected override bool isDebugUnlockedAll()
	{
		return DebugConfig.isOn(DebugOption.UnlockAllActors);
	}

	public bool canEditItem(EquipmentAsset pItem)
	{
		if (!pItem.is_pool_weapon && can_edit_equipment)
		{
			return true;
		}
		if (pItem.is_pool_weapon && can_edit_equipment)
		{
			return true;
		}
		return false;
	}

	public void addResource(string pID, int pAmount, bool pNewList = false)
	{
		if ((resources_given == null) | pNewList)
		{
			resources_given = new List<ResourceContainer>();
		}
		if (resources_given.Count > 0)
		{
			for (int i = 0; i < resources_given.Count; i++)
			{
				ResourceContainer value = resources_given[i];
				if (value.id == pID)
				{
					value.amount += pAmount;
					resources_given[i] = value;
					return;
				}
			}
		}
		resources_given.Add(new ResourceContainer(pID, pAmount));
	}

	public BuildingAsset getBuildingDockAsset()
	{
		string pID = "docks_" + architecture_id;
		return AssetManager.buildings.get(pID);
	}

	public void setSimpleCivSettings()
	{
		skin_citizen_male = Toolbox.a<string>("male_1");
		skin_citizen_female = Toolbox.a<string>("female_1");
		skin_warrior = Toolbox.a<string>("warrior_1");
		production = new string[1] { "bread" };
		build_order_template_id = "build_order_basic";
		name_template_sets = Toolbox.a<string>("default_name");
		job = Toolbox.a<string>("decision");
		job_citizen = Toolbox.a<string>("unit_citizen");
		job_kingdom = Toolbox.a<string>("decision");
		job_baby = Toolbox.a<string>("decision");
		job_attacker = Toolbox.a<string>("attacker");
		kingdom_id_wild = "neutral_animals";
	}

	public bool canBecomeSapient()
	{
		return !string.IsNullOrEmpty(kingdom_id_civilization);
	}

	public bool hasDefaultSpells()
	{
		if (spells != null)
		{
			return spells.hasAny();
		}
		return false;
	}

	public TooltipData getTooltip()
	{
		GodPower godPower = getGodPower();
		return new TooltipData
		{
			tip_name = getLocaleID(),
			tip_description = getDescriptionID(),
			power = godPower
		};
	}

	public GodPower getGodPower()
	{
		string pID = power_id ?? base_asset_id ?? id;
		if (!AssetManager.powers.has(pID))
		{
			return null;
		}
		return AssetManager.powers.get(pID);
	}

	public string getNameTemplate(MetaType pType)
	{
		string text = name_template_sets?.GetRandom();
		if (!string.IsNullOrEmpty(text))
		{
			return AssetManager.name_sets.get(text).get(pType);
		}
		if (pType == MetaType.Unit)
		{
			string.IsNullOrEmpty(name_template_unit);
			return name_template_unit;
		}
		return name_template_unit;
	}

	public string[] getWalk()
	{
		return animation_walk;
	}

	public string[] getIdle()
	{
		return animation_idle;
	}

	public string[] getSwim()
	{
		return animation_swim;
	}
}
