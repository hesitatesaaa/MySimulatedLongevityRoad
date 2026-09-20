using System;
using System.Collections.Generic;
using System.Reflection;
using Beebyte.Obfuscator;
using UnityEngine;
using strings;

[Serializable]
[ObfuscateLiterals]
public class ActorAssetLibrary : BaseLibraryWithUnlockables<ActorAsset>
{
	[NonSerialized]
	public List<ActorAsset> list_only_boat_assets;

	private int _humanoids_amount;

	private const string TEMPLATE_BASIC_UNIT = "$basic_unit$";

	private const string TEMPLATE_BASIC_UNIT_COLORED = "$basic_unit_colored$";

	private const string TEMPLATE_ANIMAL_BASE = "$animal_base$";

	private const string TEMPLATE_ANIMAL = "$animal$";

	private const string TEMPLATE_ANIMAL_FUR = "$animal_fur$";

	private const string TEMPLATE_ANIMAL_SKIN = "$animal_skin$";

	private const string TEMPLATE_PEACEFUL_ANIMAL = "$peaceful_animal$";

	private const string TEMPLATE_CARNIVORE = "$carnivore$";

	private const string TEMPLATE_HERBIVORE = "$herbivore$";

	private const string TEMPLATE_OMNIVORE = "$omnivore$";

	private const string TEMPLATE_CIV_UNIT = "$civ_unit$";

	private const string TEMPLATE_CIV_ADVANCED_UNIT = "$civ_advanced_unit$";

	private const string TEMPLATE_BOAT = "$boat$";

	private const string TEMPLATE_BOAT_TRADING = "$boat_trading$";

	private const string TEMPLATE_BOAT_TRANSPORT = "$boat_transport$";

	private const string TEMPLATE_MOB_NO_GENES = "$mob_no_genes$";

	private const string TEMPLATE_MOB = "$mob$";

	private const string TEMPLATE_CREEP_MOB = "$creep_mob$";

	private const string TEMPLATE_ANIMAL_CIV = "$animal_civ$";

	private const string TEMPLATE_INSECT = "$insect$";

	private const string TEMPLATE_FLYING_INSECT = "$flying_insect$";

	public int getHumanoidsAmount()
	{
		return _humanoids_amount;
	}

	public override void init()
	{
		Debug.Log((object)"INIT ActorStats");
		base.init();
		initTemplates();
		initCivsClassic();
		initAnimalsNormal();
		initAnimalsWeird();
		initInsects();
		initMobsOther();
		initCivsNew();
		initSpecial();
		initAnts();
		initCreepMobs();
		initBoats();
		Debug.Log((object)("GENERATE ACTOR STATS " + list.Count));
	}

	private void initSpecial()
	{
		clone("greg", "$mob$");
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("greg_set");
		t.kingdom_id_wild = "greg";
		t.kingdom_id_civilization = "miniciv_greg";
		t.name_taxonomic_kingdom = "gregalia";
		t.name_taxonomic_phylum = "gregata";
		t.name_taxonomic_class = "gregia";
		t.name_taxonomic_order = "greges";
		t.name_taxonomic_family = "gregae";
		t.name_taxonomic_genus = "greg";
		t.name_taxonomic_species = "greg";
		t.collective_term = "group_grex";
		t.addSubspeciesTrait("hydrophobia");
		t.addSubspeciesTrait("enhanced_strength");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("big_stomach");
		t.addSubspeciesTrait("diet_geophagy");
		t.addSubspeciesTrait("diet_xylophagy");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("exoskeleton");
		t.addSubspeciesTrait("fenix_born");
		t.addSubspeciesTrait("parental_care");
		t.addSubspeciesTrait("reproduction_parthenogenesis");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("gestation_extremely_long");
		t.addSubspeciesTrait("voracious");
		t.addSubspeciesTrait("egg_face");
		t.addReligionTrait("echo_of_the_void");
		t.addTrait("giant");
		t.addTrait("strong");
		t.addTrait("regeneration");
		addPhenotype("bright_yellow");
		t.special = true;
		t.unit_other = true;
		t.name_locale = "Greg";
		t.actor_size = ActorSize.S15_Bear;
		t.shadow_texture = "unitShadow_6";
		t.animation_walk = ActorAnimationSequences.walk_0;
		t.animation_swim = ActorAnimationSequences.walk_0;
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.architecture_id = "civ_greg";
		t.banner_id = "human";
		t.addGenome(("health", 500f), ("stamina", 200f), ("mutation", 2f), ("damage", 100f), ("armor", 10f), ("speed", 40f));
		t.icon = "iconGreg";
		t.color_hex = "#24803E";
		t.rotating_animation = true;
		t.has_soul = true;
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.addResource("worms", 2, pNewList: true);
		t.addResource("evil_beets", 1);
		clone("living_plants", "$mob_no_genes$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("living_plant_set");
		t.use_phenotypes = false;
		t.special = true;
		t.has_baby_form = false;
		t.name_locale = "living_plant";
		t.actor_size = ActorSize.S15_Bear;
		t.shadow_texture = "unitShadow_6";
		t.kingdom_id_wild = "living_plants";
		t.base_stats["health"] = 300f;
		t.base_stats["speed"] = 10f;
		t.base_stats["armor"] = 0f;
		t.base_stats["attack_speed"] = 70f;
		t.base_stats["damage"] = 30f;
		t.base_stats["knockback"] = 1.2f;
		t.base_stats["mass"] = 50f;
		t.base_stats["mass_2"] = 500f;
		t.base_stats["targets"] = 3f;
		t.damaged_by_ocean = true;
		t.icon = "iconLivingPlants";
		t.show_icon_inspect_window = true;
		t.show_icon_inspect_window_id = "iconLivingPlants";
		t.color_hex = "#115D11";
		t.rotating_animation = true;
		t.disable_jump_animation = false;
		t.inspect_avatar_scale = 1f;
		t.base_stats["scale"] = 0.25f;
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.action_on_load = delegate(Actor pActor)
		{
			pActor.data.get("special_sprite_id", out var pResult, null);
			if (!string.IsNullOrEmpty(pResult))
			{
				AssetManager.buildings.get(pResult).checkSpritesAreLoaded();
			}
		};
		t.get_override_sprite = delegate(Actor pActor)
		{
			pActor.data.get("special_sprite_id", out var pResult, null);
			pActor.data.get("special_sprite_index", out var pResult2, 0);
			if (string.IsNullOrEmpty(pResult))
			{
				pResult = "ui/Icons/iconLivingPlants";
				return SpriteTextureLoader.getSprite(pResult);
			}
			return AssetManager.buildings.get(pResult).building_sprites.animation_data[pResult2].main[0];
		};
		addTrait("regeneration");
		t.music_theme = "Units_LivingPlants";
		t.sound_hit = "event:/SFX/HIT/HitGeneric";
		t.show_in_knowledge_window = false;
		t.use_items = false;
		t.take_items = false;
		t.can_edit_equipment = false;
		t.use_tool_items = false;
		t.addResource("herbs", 1, pNewList: true);
		clone("living_house", "$mob_no_genes$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("living_house_set");
		t.use_phenotypes = false;
		t.has_baby_form = false;
		t.special = true;
		t.name_locale = "living_house";
		t.actor_size = ActorSize.S15_Bear;
		t.shadow_texture = "unitShadow_6";
		t.kingdom_id_wild = "living_houses";
		t.base_stats["health"] = 500f;
		t.base_stats["armor"] = 1f;
		t.base_stats["speed"] = 10f;
		t.base_stats["attack_speed"] = 60f;
		t.base_stats["damage"] = 50f;
		t.base_stats["knockback"] = 1.4f;
		t.base_stats["mass"] = 50f;
		t.base_stats["mass_2"] = 10000f;
		t.base_stats["targets"] = 3f;
		t.damaged_by_ocean = true;
		t.icon = "iconLivingHouse";
		t.show_icon_inspect_window = true;
		t.show_icon_inspect_window_id = "iconLivingHouse";
		t.color_hex = "#24803E";
		t.rotating_animation = true;
		t.disable_jump_animation = false;
		t.inspect_avatar_scale = 1f;
		t.base_stats["scale"] = 0.25f;
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.get_override_sprite = get("living_plants").get_override_sprite;
		t.sound_hit = "event:/SFX/HIT/HitWood";
		t.show_in_knowledge_window = false;
		t.use_items = false;
		t.take_items = false;
		t.can_edit_equipment = false;
		t.use_tool_items = false;
		t.addResource("wood", 1, pNewList: true);
		add(t = new ActorAsset
		{
			id = "dragon",
			kingdom_id_wild = "dragons",
			special = true,
			can_be_killed_by_stuff = true,
			can_be_killed_by_life_eraser = true,
			ignore_tile_speed_multiplier = true,
			skip_fight_logic = true,
			job = AssetLibrary<ActorAsset>.a<string>("dragon_job"),
			can_be_moved_by_powers = true,
			can_be_hurt_by_powers = true,
			update_z = true,
			default_height = 0f,
			effect_damage = true,
			can_flip = true,
			can_turn_into_zombie = true,
			actor_size = ActorSize.S17_Dragon,
			shadow_texture = "unitShadow_7",
			can_be_inspected = true,
			hide_favorite_icon = true,
			icon = "iconDragon",
			inspect_avatar_scale = 1.1f,
			inspect_avatar_offset_y = -22f,
			avatar_prefab = "p_dragon",
			visible_on_minimap = true,
			die_on_blocks = false,
			ignore_blocks = true,
			move_from_block = false,
			run_to_water_when_on_fire = false,
			split_ai_update = false,
			experience_given = 100,
			can_be_surprised = false,
			allow_possession = false,
			show_task_icon = false,
			can_talk_with = false,
			control_can_backstep = false,
			control_can_dash = false,
			control_can_jump = false,
			control_can_kick = false,
			control_can_talk = false,
			control_can_swear = false,
			control_can_steal = false,
			inspect_mind = false
		});
		t.setCanTurnIntoZombieAsset("zombie_dragon", pAutoZombieAsset: false);
		t.get_override_sprite = (Actor pActor) => pActor.getSpriteAnimation().currentSpriteGraphic;
		t.get_override_avatar_frames = (Actor pActor) => PrefabLibrary.instance.dragonAsset.getAsset(DragonState.Fly).frames;
		t.allowed_status_tiers = StatusTier.Basic;
		t.render_status_effects = false;
		t.name_locale = "Dragon";
		t.die_in_lava = false;
		t.cancel_beh_on_land = false;
		t.base_stats["health"] = 1000f;
		t.base_stats["damage"] = 100f;
		t.base_stats["speed"] = 40f;
		t.base_stats["scale"] = 0.3f;
		t.base_stats["size"] = 2f;
		t.base_stats["mass"] = 4f;
		t.base_stats["mass_2"] = 2500f;
		t.base_stats["targets"] = 20f;
		t.base_stats["lifespan"] = 10000f;
		addTrait("regeneration");
		addTrait("strong_minded");
		addTrait("fire_proof");
		addTrait("fire_blood");
		t.addResource("dragon_scales", 10, pNewList: true);
		t.addResource("meat", 10);
		t.addResource("bones", 10);
		t.music_theme = "Units_Dragon";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		ActorAsset actorAsset = t;
		actorAsset.action_click = (WorldAction)Delegate.Combine(actorAsset.action_click, new WorldAction(Dragon.clickToWakeup));
		t.action_dead_animation = Dragon.dragonFall;
		ActorAsset actorAsset2 = t;
		actorAsset2.action_death = (WorldAction)Delegate.Combine(actorAsset2.action_death, new WorldAction(ActionLibrary.dragonSlayer));
		ActorAsset actorAsset3 = t;
		actorAsset3.action_get_hit = (GetHitAction)Delegate.Combine(actorAsset3.action_get_hit, new GetHitAction(Dragon.getHit));
		t.check_flip = Dragon.canFlip;
		t.animation_speed_based_on_walk_speed = false;
		t.needs_to_be_explored = false;
		t.use_tool_items = false;
		clone("zombie_dragon", "dragon");
		t.special = true;
		t.name_locale = "Zombie";
		t.avatar_prefab = "p_zombie_dragon";
		t.visible_on_minimap = true;
		t.die_in_lava = true;
		t.show_in_knowledge_window = false;
		t.setZombie(pAnimal: false);
		t.job = AssetLibrary<ActorAsset>.a<string>("dragon_job");
		removeTrait("fire_proof");
		removeTrait("fire_blood");
		addTrait("acid_blood");
		addTrait("acid_proof");
		add(t = new ActorAsset
		{
			id = "UFO",
			kingdom_id_wild = "aliens",
			special = true,
			can_be_killed_by_stuff = true,
			can_be_killed_by_life_eraser = true,
			ignore_tile_speed_multiplier = true,
			skip_fight_logic = true,
			job = AssetLibrary<ActorAsset>.a<string>("ufo_job"),
			flying = true,
			very_high_flyer = true,
			can_be_moved_by_powers = true,
			can_be_hurt_by_powers = true,
			effect_damage = true,
			special_dead_animation = true,
			actor_size = ActorSize.S17_Dragon,
			shadow_texture = "unitShadow_7",
			can_be_inspected = true,
			icon = "iconUfo",
			hide_favorite_icon = true,
			die_by_lightning = true,
			avatar_prefab = "p_ufo",
			visible_on_minimap = true,
			die_on_blocks = false,
			ignore_blocks = true,
			move_from_block = false,
			run_to_water_when_on_fire = false,
			has_skin = false,
			flag_ufo = true,
			split_ai_update = false,
			allow_possession = false,
			show_task_icon = false,
			can_talk_with = false,
			control_can_backstep = false,
			control_can_dash = false,
			control_can_jump = false,
			control_can_kick = false,
			control_can_talk = false,
			control_can_swear = false,
			control_can_steal = false
		});
		t.get_override_sprite = (Actor pActor) => pActor.getSpriteAnimation().currentSpriteGraphic;
		t.allowed_status_tiers = StatusTier.Basic;
		t.render_status_effects = false;
		t.inspect_avatar_scale = 1.45f;
		t.inspect_avatar_offset_y = 8f;
		t.name_locale = "UFO";
		t.name_template_unit = "ufo_name";
		t.base_stats["health"] = 1000f;
		t.base_stats["armor"] = 5f;
		t.base_stats["scale"] = 0.3f;
		t.base_stats["speed"] = 20f;
		t.base_stats["damage"] = 80f;
		t.base_stats["lifespan"] = 0f;
		t.base_stats["size"] = 2f;
		ActorAsset actorAsset4 = t;
		actorAsset4.action_death = (WorldAction)Delegate.Combine(actorAsset4.action_death, new WorldAction(ActionLibrary.spawnAliens));
		t.base_stats["mass"] = 5f;
		t.base_stats["mass_2"] = 2500f;
		t.action_dead_animation = UFO.ufoFall;
		ActorAsset actorAsset5 = t;
		actorAsset5.action_click = (WorldAction)Delegate.Combine(actorAsset5.action_click, new WorldAction(UFO.click));
		ActorAsset actorAsset6 = t;
		actorAsset6.action_get_hit = (GetHitAction)Delegate.Combine(actorAsset6.action_get_hit, new GetHitAction(UFO.getHit));
		t.prevent_unconscious_rotation = true;
		addTrait("strong_minded");
		addTrait("fire_proof");
		addTrait("light_lamp");
		t.music_theme = "Units_UFO";
		t.sound_hit = "event:/SFX/HIT/HitMetal";
		t.needs_to_be_explored = false;
		t.can_be_surprised = false;
		t.use_tool_items = false;
		t.default_height = 8f;
		add(t = new ActorAsset
		{
			id = "crabzilla",
			kingdom_id_wild = "crabzilla",
			special = true,
			can_be_killed_by_stuff = true,
			ignore_tile_speed_multiplier = true,
			skip_fight_logic = true,
			flying = false,
			can_be_moved_by_powers = false,
			can_be_hurt_by_powers = true,
			update_z = true,
			can_flip = false,
			skip_save = true,
			avatar_prefab = "p_crabzilla",
			visible_on_minimap = true,
			ignore_generic_render = true,
			die_on_blocks = false,
			ignore_blocks = true,
			move_from_block = false,
			run_to_water_when_on_fire = false,
			ignored_by_infinity_coin = true,
			split_ai_update = false,
			has_ai_system = false,
			show_in_knowledge_window = false,
			show_task_icon = false,
			can_talk_with = false,
			control_can_backstep = false,
			control_can_dash = false,
			control_can_jump = false,
			control_can_kick = false,
			control_can_talk = false,
			control_can_swear = false,
			control_can_steal = false,
			show_controllable_tip = false
		});
		t.allowed_status_tiers = StatusTier.None;
		t.has_sprite_renderer = false;
		t.name_locale = "Crabzilla";
		t.icon = "iconCrabzilla";
		t.base_stats["scale"] = 0.25f;
		t.actor_size = ActorSize.S17_Dragon;
		t.base_stats["health"] = 10000f;
		t.base_stats["speed"] = 50f;
		t.base_stats["damage"] = 10f;
		t.base_stats["size"] = 8f;
		t.base_stats["mass_2"] = 99999f;
		t.can_level_up = false;
		t.shadow = false;
		t.hit_fx_alternative_offset = false;
		ActorAsset actorAsset7 = t;
		actorAsset7.action_death = (WorldAction)Delegate.Combine(actorAsset7.action_death, new WorldAction(ActionLibrary.clearCrabzilla));
		ActorAsset actorAsset8 = t;
		actorAsset8.action_death = (WorldAction)Delegate.Combine(actorAsset8.action_death, new WorldAction(ActionLibrary.startCrabzillaNuke));
		t.action_dead_animation = delegate(BaseSimObject pTarget, WorldTile _, float _)
		{
			pTarget.a.dieAndDestroy(AttackType.None);
			return true;
		};
		ActorAsset actorAsset9 = t;
		actorAsset9.action_get_hit = (GetHitAction)Delegate.Combine(actorAsset9.action_get_hit, new GetHitAction(Crabzilla.getHit));
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		addTrait("strong_minded");
		t.needs_to_be_explored = false;
		t.can_be_surprised = false;
		t.use_tool_items = false;
		add(t = new ActorAsset
		{
			id = "god_finger",
			special = true,
			actor_size = ActorSize.S17_Dragon,
			shadow_texture = "unitShadow_7",
			kingdom_id_wild = "godfinger",
			can_be_killed_by_stuff = true,
			ignore_tile_speed_multiplier = true,
			can_be_killed_by_life_eraser = true,
			skip_fight_logic = true,
			can_be_moved_by_powers = true,
			can_be_hurt_by_powers = true,
			update_z = false,
			effect_damage = true,
			skip_save = true,
			avatar_prefab = "p_god_finger",
			visible_on_minimap = true,
			die_on_blocks = false,
			ignore_blocks = true,
			move_from_block = false,
			run_to_water_when_on_fire = false,
			split_ai_update = false,
			has_ai_system = true,
			job = AssetLibrary<ActorAsset>.a<string>("godfinger_job"),
			flying = true,
			very_high_flyer = true,
			die_by_lightning = true,
			show_in_knowledge_window = false,
			allow_possession = false,
			show_task_icon = false,
			can_talk_with = false,
			control_can_backstep = false,
			control_can_dash = false,
			control_can_jump = false,
			control_can_kick = false,
			control_can_talk = false,
			control_can_swear = false,
			control_can_steal = false,
			show_in_taxonomy_tooltip = false
		});
		t.get_override_sprite = (Actor pActor) => pActor.getSpriteAnimation().currentSpriteGraphic;
		t.allowed_status_tiers = StatusTier.Basic;
		t.render_status_effects = false;
		t.flag_finger = true;
		t.name_locale = "God Finger";
		t.base_stats["scale"] = 0.3f;
		t.base_stats["mass"] = 5f;
		t.base_stats["mass_2"] = 99999f;
		t.base_stats["speed"] = 50f;
		t.base_stats["damage"] = 80f;
		t.base_stats["lifespan"] = 10000f;
		t.base_stats["size"] = 2f;
		t.base_stats["health"] = 100f;
		t.base_stats["armor"] = 5f;
		t.die_in_lava = false;
		addTrait("light_lamp");
		addTrait("strong_minded");
		addTrait("fire_proof");
		t.music_theme = "Units_GodFinger";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.needs_to_be_explored = false;
		ActorAsset actorAsset10 = t;
		actorAsset10.action_dead_animation = (DeadAnimation)Delegate.Combine(actorAsset10.action_dead_animation, new DeadAnimation(GodFinger.deathFlip));
		t.can_be_surprised = false;
		t.use_tool_items = false;
	}

	private void initAnts()
	{
		clone("ant_black", "$basic_unit$");
		t.allow_possession = false;
		t.can_be_cloned = false;
		t.show_task_icon = false;
		t.kingdom_id_wild = "ants";
		t.can_be_killed_by_stuff = true;
		t.can_be_killed_by_life_eraser = true;
		t.ignore_tile_speed_multiplier = true;
		t.skip_fight_logic = true;
		t.can_be_moved_by_powers = true;
		t.can_be_hurt_by_powers = true;
		t.update_z = true;
		t.effect_damage = true;
		t.can_flip = true;
		t.actor_size = ActorSize.S13_Human;
		t.color_hex = "#000000";
		t.die_on_blocks = false;
		t.ignore_blocks = true;
		t.move_from_block = false;
		t.run_to_water_when_on_fire = false;
		t.force_land_creature = true;
		t.force_ocean_creature = true;
		t.split_ai_update = false;
		t.can_be_inspected = true;
		t.name_locale = "Black Ant";
		t.name_template_unit = "ant_name";
		t.unit_other = true;
		t.job = AssetLibrary<ActorAsset>.a<string>("ant_black");
		t.animation_walk = ActorAnimationSequences.walk_0;
		t.base_stats["speed"] = 20f;
		t.base_stats["mass_2"] = 0.01f;
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.icon = "iconAntBlack";
		addTrait("strong_minded");
		t.generateFmodPaths("ant_black");
		t.can_be_surprised = false;
		t.use_tool_items = false;
		t.inspect_mind = false;
		t.inspect_genealogy = false;
		t.can_talk_with = false;
		clone("ant_green", "ant_black");
		t.name_locale = "Green Ant";
		t.icon = "iconAntGreen";
		t.job = AssetLibrary<ActorAsset>.a<string>("ant_green");
		t.color_hex = "#007F0E";
		clone("ant_blue", "ant_black");
		t.name_locale = "Blue Ant";
		t.icon = "iconAntBlue";
		t.job = AssetLibrary<ActorAsset>.a<string>("ant_blue");
		t.color_hex = "#0094FF";
		clone("ant_red", "ant_black");
		t.name_locale = "Red Ant";
		t.icon = "iconAntRed";
		t.job = AssetLibrary<ActorAsset>.a<string>("ant_red");
		t.color_hex = "#FF2511";
		clone("sand_spider", "ant_black");
		t.name_locale = "Sand Spider";
		t.icon = "iconSandSpider";
		t.job = AssetLibrary<ActorAsset>.a<string>("sandspider_job");
		t.color_hex = "#2D2D2D";
		t.base_stats["speed"] = 100f;
		clone("worm", "ant_black");
		t.unit_other = true;
		t.can_be_inspected = false;
		t.name_template_unit = "bug_name";
		t.name_locale = "Worm";
		t.job = AssetLibrary<ActorAsset>.a<string>("worm_job");
		t.animation_walk = ActorAnimationSequences.walk_0;
		t.can_be_moved_by_powers = false;
		t.can_be_hurt_by_powers = true;
		t.can_be_killed_by_stuff = false;
		t.kingdom_id_wild = "nature";
		t.base_stats["speed"] = 100f;
		t.base_stats["mass_2"] = 0.05f;
		t.color_hex = null;
		t.shadow = false;
		addTrait("fire_proof");
		t.music_theme = "Units_Worm";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		clone("printer", "ant_black");
		t.kingdom_id_wild = "nature";
		t.job = AssetLibrary<ActorAsset>.a<string>("printer_job");
		t.can_be_moved_by_powers = false;
		t.can_be_hurt_by_powers = true;
		t.update_z = true;
		t.effect_damage = true;
		t.can_flip = true;
		t.skip_save = true;
		t.die_on_blocks = false;
		t.ignore_blocks = true;
		t.move_from_block = false;
		t.run_to_water_when_on_fire = false;
		t.ignored_by_infinity_coin = true;
		t.split_ai_update = false;
		t.unit_other = true;
		t.name_locale = "Printer";
		t.base_stats["health"] = 1f;
		t.base_stats["speed"] = 10000f;
		t.base_stats["mass"] = 10f;
		t.animation_walk = ActorAnimationSequences.walk_0_2;
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
	}

	private void initCreepMobs()
	{
		clone("$creep_mob$", "$mob$");
		t.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain", "phenotypes");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("default_set");
		t.base_stats["speed"] = 10f;
		t.has_advanced_textures = false;
		t.can_turn_into_ice_one = false;
		t.has_baby_form = false;
		t.kingdom_id_civilization = string.Empty;
		t.build_order_template_id = string.Empty;
		clone("mush_unit", "$creep_mob$");
		t.can_edit_equipment = true;
		t.take_items = true;
		t.use_items = true;
		t.name_taxonomic_kingdom = "fungi";
		t.name_taxonomic_phylum = "ascomycota";
		t.name_taxonomic_class = "sordariomycetes";
		t.name_taxonomic_order = "hypocreales";
		t.name_taxonomic_family = "cordycipitaceae";
		t.name_taxonomic_genus = "cordyceps";
		t.name_taxonomic_species = "puppetus";
		t.collective_term = "group_mycelium";
		addPhenotype("dark_green");
		t.base_stats["mass_2"] = 60f;
		t.addGenome(("health", 300f), ("stamina", 50f), ("lifespan", 100f), ("mutation", 5f), ("damage", 25f), ("attack_speed", 30f), ("speed", 30f));
		t.unit_other = true;
		t.name_taxonomic_kingdom = "fungi";
		t.collective_term = "group_mycelium";
		t.name_locale = "Mush";
		t.body_separate_part_hands = true;
		t.kingdom_id_wild = "mush";
		t.can_be_killed_by_divine_light = true;
		t.icon = "actor_traits/iconMushSpores";
		t.color_hex = "#FF49CB";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.can_turn_into_zombie = false;
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		addTrait("weightless");
		addTrait("mush_spores");
		addTrait("regeneration");
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		clone("mush_animal", "mush_unit");
		t.name_taxonomic_species = "pippitus";
		t.show_in_taxonomy_tooltip = false;
		t.icon = "actor_traits/iconMushSpores";
		t.unit_other = true;
		t.base_stats["health"] = 200f;
		t.base_stats["mass_2"] = 45f;
		t.body_separate_part_hands = false;
		t.use_items = false;
		t.take_items = false;
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		clone("tumor_monster_unit", "$mob$");
		t.needs_to_be_explored = true;
		t.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("default_set");
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "neoplasia";
		t.name_taxonomic_class = "malignomorpha";
		t.name_taxonomic_order = "oncovorales";
		t.name_taxonomic_family = "tumoridae";
		t.name_taxonomic_genus = "neoplasmus";
		t.name_taxonomic_species = "carcinomus";
		t.collective_term = "group_cancer";
		addPhenotype("dark_salmon");
		t.base_stats["mass_2"] = 75f;
		t.addGenome(("health", 100f), ("stamina", 50f), ("lifespan", 100f), ("mutation", 5f), ("damage", 15f), ("speed", 6f));
		t.icon = "iconTumor";
		t.unit_other = true;
		t.name_locale = "Tumor Monster";
		t.immune_to_tumor = true;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.can_turn_into_ice_one = false;
		t.body_separate_part_hands = true;
		t.kingdom_id_wild = "tumor";
		t.icon = "iconTumorMonster";
		t.color_hex = "#FF49CB";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.kingdom_id_civilization = string.Empty;
		t.build_order_template_id = string.Empty;
		addTrait("weightless");
		addTrait("ugly");
		addTraitIgnore("bomberman");
		addTraitIgnore("pyromaniac");
		t.music_theme = "Buildings_Tumor";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		clone("tumor_monster_animal", "tumor_monster_unit");
		t.show_in_taxonomy_tooltip = false;
		t.show_in_knowledge_window = false;
		t.base_asset_id = "tumor_monster_unit";
		t.icon = "iconTumorMonster";
		t.unit_other = true;
		t.body_separate_part_hands = false;
		t.use_items = false;
		t.take_items = false;
		t.mush_id = "mush_animal";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.base_stats["mass_2"] = 55f;
		clone("lil_pumpkin", "tumor_monster_animal");
		t.show_in_knowledge_window = true;
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("pumpkin_set");
		t.show_in_taxonomy_tooltip = true;
		t.name_taxonomic_kingdom = "plantae";
		t.name_taxonomic_phylum = "angiospermae";
		t.name_taxonomic_class = "dicotyledonae";
		t.name_taxonomic_order = "cucurbitales";
		t.name_taxonomic_family = "cucurbitaceae";
		t.name_taxonomic_genus = "worldboxus";
		t.name_taxonomic_species = "maximus";
		t.collective_term = "group_squash";
		t.addSubspeciesTrait("egg_pumpkin");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		clearPhenotypes();
		addPhenotype("dark_orange");
		t.base_stats["mass_2"] = 9f;
		t.addGenome(("health", 1000f), ("stamina", 150f), ("lifespan", 100f), ("mutation", 5f), ("damage", 15f), ("speed", 6f));
		t.icon = "iconLilPumpkin";
		t.unit_other = true;
		t.name_locale = "Lil Pumpkin";
		t.kingdom_id_wild = "super_pumpkin";
		t.immune_to_slowness = true;
		t.mush_id = "mush_animal";
		t.clearTraits();
		addTrait("attractive");
		addTrait("fat");
		addTrait("bloodlust");
		addTrait("thorns");
		addTraitIgnore("bomberman");
		addTraitIgnore("pyromaniac");
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.base_asset_id = null;
		t.addResource("herbs", 1, pNewList: true);
		t.addResource("tea", 1);
		clone("assimilator", "tumor_monster_animal");
		t.show_in_knowledge_window = true;
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("assimilator_set");
		t.show_in_taxonomy_tooltip = true;
		clearPhenotypes();
		addPhenotype("black_blue");
		t.addSubspeciesTrait("hydrophobia");
		t.addSubspeciesTrait("egg_metal_box");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.name_taxonomic_kingdom = "machina";
		t.name_taxonomic_phylum = "cybernetica";
		t.name_taxonomic_class = "slowupdata";
		t.name_taxonomic_order = "noupdates";
		t.name_taxonomic_family = "assimiladae";
		t.name_taxonomic_genus = "assimilatus";
		t.name_taxonomic_species = "perfectus";
		t.collective_term = "group_network";
		t.addGenome(("health", 1000f), ("stamina", 300f), ("lifespan", 100f), ("mutation", 5f), ("damage", 15f), ("armor", 20f), ("speed", 6f));
		t.icon = "iconAssimilator";
		t.unit_other = true;
		t.name_locale = "Assimilator";
		t.inspect_avatar_scale = 2.1f;
		t.can_turn_into_mush = false;
		t.can_turn_into_zombie = false;
		t.kingdom_id_wild = "assimilators";
		t.base_stats["mass_2"] = 15f;
		t.base_stats["damage"] = 0f;
		t.body_separate_part_hands = true;
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("shotgun");
		addTrait("fire_proof");
		addTrait("bubble_defense");
		removeTrait("ugly");
		removeTrait("weightless");
		addTraitIgnore("bomberman");
		addTraitIgnore("pyromaniac");
		t.sound_hit = "event:/SFX/HIT/HitMetal";
		t.use_items = true;
		t.base_asset_id = null;
		t.addResource("adamantine", 1, pNewList: true);
		t.addResource("common_metals", 1);
		clone("bioblob", "tumor_monster_animal");
		t.show_in_knowledge_window = true;
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("bioblob_set");
		t.show_in_taxonomy_tooltip = true;
		t.name_taxonomic_kingdom = "protista";
		t.name_taxonomic_phylum = "amoebozoa";
		t.name_taxonomic_class = "myxogastria";
		t.name_taxonomic_order = "physarales";
		t.name_taxonomic_family = "blobidae";
		t.name_taxonomic_genus = "blobus";
		t.name_taxonomic_species = "opticus";
		t.collective_term = "group_blob";
		t.addSubspeciesTrait("egg_eyeball");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		addTrait("strong_minded");
		addTrait("ugly");
		addTrait("fragile_health");
		clearPhenotypes();
		addPhenotype("toxic_green");
		t.base_stats["mass_2"] = 15f;
		t.addGenome(("health", 200f), ("stamina", 20f), ("lifespan", 100f), ("mutation", 5f), ("damage", 15f), ("armor", 20f), ("speed", 6f));
		t.icon = "iconBioblob";
		t.unit_other = true;
		t.name_locale = "Bioblob";
		t.kingdom_id_wild = "biomass";
		t.immune_to_slowness = true;
		t.mush_id = "mush_animal";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.prevent_unconscious_rotation = true;
		t.base_asset_id = null;
	}

	public void clearPhenotypes()
	{
		t.phenotypes_dict = null;
		t.phenotypes_list = null;
	}

	public void addPhenotype(string pID, string pType = "default_color")
	{
		if (t.phenotypes_dict == null)
		{
			t.phenotypes_dict = new Dictionary<string, List<string>>();
			t.phenotypes_list = new List<string>();
		}
		if (!t.phenotypes_dict.ContainsKey(pType))
		{
			t.phenotypes_dict[pType] = new List<string>();
		}
		if (!t.phenotypes_dict[pType].Contains(pID))
		{
			t.phenotypes_dict[pType].Add(pID);
			t.phenotypes_list.Add(pID);
		}
	}

	public void clear()
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].units.Clear();
		}
	}

	internal void addTrait(string pTrait)
	{
		t.addTrait(pTrait);
	}

	internal void addTraitIgnore(string pTrait)
	{
		t.addTraitIgnore(pTrait);
	}

	internal void removeTrait(string pTrait)
	{
		t.removeTrait(pTrait);
	}

	public override void post_init()
	{
		loadAutoTextures(list);
		generateZombieAssets();
		loadShadows();
		base.post_init();
		generateFmodPaths();
		foreach (ActorAsset item in list)
		{
			if (item.action_dead_animation != null)
			{
				item.special_dead_animation = true;
			}
			if (!string.IsNullOrEmpty(item.base_asset_id))
			{
				ActorAsset actorAsset = get(item.base_asset_id);
				item.units = actorAsset.units;
			}
			if (item.is_humanoid && !item.unit_zombie)
			{
				_humanoids_amount++;
			}
		}
	}

	private void linkSpells()
	{
		foreach (ActorAsset item in list)
		{
			if (item.spell_ids != null && item.spell_ids.Count != 0)
			{
				item.spells = new SpellHolder();
				item.spells.mergeWith(item.spell_ids);
			}
		}
	}

	public override void linkAssets()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		base.linkAssets();
		setupBoolsAvatarPrefabs();
		setupBoolSpriteOverrides();
		linkArchitectures();
		linkSpells();
		fillOnlyBoatsList();
		foreach (ActorAsset item in list)
		{
			if (item.color_hex != null)
			{
				item.color = Color32.op_Implicit(Toolbox.makeColor(item.color_hex));
			}
			if (item.check_flip == null)
			{
				item.check_flip = (BaseSimObject _, WorldTile _) => true;
			}
		}
	}

	private void linkArchitectures()
	{
		foreach (ActorAsset item in list)
		{
			if (!string.IsNullOrEmpty(item.architecture_id))
			{
				item.architecture_asset = AssetManager.architecture_library.get(item.architecture_id);
			}
		}
	}

	public override ActorAsset add(ActorAsset pAsset)
	{
		ActorAsset actorAsset = base.add(pAsset);
		if (actorAsset.base_stats == null)
		{
			actorAsset.base_stats = new BaseStats();
		}
		return actorAsset;
	}

	private void fillOnlyBoatsList()
	{
		list_only_boat_assets = list.FindAll((ActorAsset pAsset) => pAsset.is_boat);
	}

	private void setupBoolSpriteOverrides()
	{
		foreach (ActorAsset item in list)
		{
			if (item.get_override_sprite != null)
			{
				item.has_override_sprite = true;
			}
			if (item.get_override_avatar_frames != null)
			{
				item.has_override_avatar_frames = true;
			}
		}
	}

	private void setupBoolsAvatarPrefabs()
	{
		foreach (ActorAsset item in list)
		{
			if (item.avatar_prefab != string.Empty)
			{
				item.has_avatar_prefab = true;
			}
		}
	}

	private void loadAutoTextures(IEnumerable<ActorAsset> pAssetsList)
	{
		foreach (ActorAsset pAssets in pAssetsList)
		{
			loadTexturesAndSprites(pAssets);
		}
	}

	private void generateFmodPaths()
	{
		foreach (ActorAsset item in list)
		{
			if (!item.is_boat)
			{
				item.generateFmodPaths(item.id);
			}
		}
	}

	private void loadTexturesAndSprites(ActorAsset pAsset)
	{
		string text = "actors/species/";
		string texture_id = pAsset.id;
		if (pAsset.texture_id != string.Empty)
		{
			texture_id = pAsset.texture_id;
		}
		if (pAsset.default_animal)
		{
			text = text + "animals/" + texture_id + "/";
		}
		else if (pAsset.civ)
		{
			text = text + "civs/" + texture_id + "/";
		}
		else if (pAsset.unit_other)
		{
			text = text + "other/" + texture_id + "/";
		}
		ActorTextureSubAsset actorTextureSubAsset = pAsset.texture_asset;
		if (actorTextureSubAsset == null)
		{
			actorTextureSubAsset = (pAsset.texture_asset = new ActorTextureSubAsset(text, pAsset.has_advanced_textures));
			actorTextureSubAsset.prevent_unconscious_rotation = pAsset.prevent_unconscious_rotation;
			actorTextureSubAsset.render_heads_for_children = pAsset.render_heads_for_babies;
			if (pAsset.shadow)
			{
				actorTextureSubAsset.shadow = true;
				actorTextureSubAsset.shadow_texture = pAsset.shadow_texture;
				actorTextureSubAsset.shadow_texture_egg = pAsset.shadow_texture_egg;
				actorTextureSubAsset.shadow_texture_baby = pAsset.shadow_texture_baby;
			}
		}
		if (pAsset.can_turn_into_zombie)
		{
			pAsset.texture_path_zombie_for_auto_loader_main = text + "zombie";
			pAsset.texture_path_zombie_for_auto_loader_heads = text + "heads_zombie";
		}
		if (pAsset.has_baby_form)
		{
			bool flag = hasSpriteInResources(actorTextureSubAsset.texture_path_baby);
			if (!flag)
			{
				Sprite[] spriteList = SpriteTextureLoader.getSpriteList(actorTextureSubAsset.texture_path_baby);
				for (int i = 0; i < spriteList.Length; i++)
				{
					if (!(((Object)spriteList[i]).name != "walk_0_head"))
					{
						flag = true;
						break;
					}
				}
			}
			if (pAsset.render_heads_for_babies && !flag)
			{
				Debug.LogError((object)("ActorAssetLibrary: Actor Asset " + pAsset.id + " does not have head sprite for baby, but supposed to render them!"));
			}
		}
		else
		{
			actorTextureSubAsset.texture_path_baby = null;
		}
	}

	private void generateZombieAssets()
	{
		using ListPool<ActorAsset> listPool = new ListPool<ActorAsset>(list);
		using ListPool<ActorAsset> listPool2 = new ListPool<ActorAsset>(128);
		createDefaultZombieAsset();
		foreach (ref ActorAsset item2 in listPool)
		{
			ActorAsset current = item2;
			if (!current.isTemplateAsset() && current.zombie_auto_asset && current.can_turn_into_zombie)
			{
				string zombieID = current.getZombieID();
				ActorAsset item = clone(zombieID, current.id);
				listPool2.Add(item);
				setDefaultZombieFields(t, current, current.default_animal);
				ActorTextureSubAsset actorTextureSubAsset = new ActorTextureSubAsset(current.texture_path_zombie_for_auto_loader_main, t.has_advanced_textures);
				t.texture_asset = actorTextureSubAsset;
				ActorTextureSubAsset texture_asset = current.texture_asset;
				actorTextureSubAsset.shadow = texture_asset.shadow;
				actorTextureSubAsset.shadow_texture = texture_asset.shadow_texture;
				actorTextureSubAsset.shadow_texture_egg = texture_asset.shadow_texture_egg;
				actorTextureSubAsset.shadow_texture_baby = texture_asset.shadow_texture_baby;
				if (hasSpriteInResources(current.texture_path_zombie_for_auto_loader_main))
				{
					actorTextureSubAsset.texture_path_main = current.texture_path_zombie_for_auto_loader_main;
					actorTextureSubAsset.texture_heads = current.texture_path_zombie_for_auto_loader_heads;
				}
				else
				{
					actorTextureSubAsset.texture_path_main = current.texture_asset.texture_path_main;
					actorTextureSubAsset.texture_heads = current.texture_asset.texture_heads;
					t.dynamic_sprite_zombie = true;
				}
				if (current.animation_swim == null)
				{
					t.animation_swim = null;
				}
			}
		}
		loadAutoTextures(listPool2);
	}

	private void loadShadows()
	{
		foreach (ActorAsset item in list)
		{
			if (item.shadow)
			{
				item.texture_asset.loadShadow();
			}
		}
	}

	private void createDefaultZombieAsset()
	{
		ActorAsset actorAsset = clone("zombie", "human");
		actorAsset.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain", "phenotypes");
		setDefaultZombieFields(actorAsset, get("human"));
		loadTexturesAndSprites(actorAsset);
	}

	private void setDefaultZombieFields(ActorAsset pAsset, ActorAsset pDefaultCreatureAsset, bool pAnimal = false)
	{
		pAsset.has_advanced_textures = false;
		pAsset.show_in_knowledge_window = false;
		pAsset.civ = false;
		pAsset.can_have_subspecies = true;
		pAsset.name_locale = "Zombie";
		pAsset.body_separate_part_hands = true;
		pAsset.icon = "iconZombie";
		pAsset.use_items = true;
		pAsset.can_edit_equipment = true;
		pAsset.banner_id = string.Empty;
		pAsset.color_hex = "#24803E";
		pAsset.job = AssetLibrary<ActorAsset>.a<string>("decision");
		pAsset.can_attack_buildings = false;
		pAsset.can_attack_brains = true;
		pAsset.disable_jump_animation = true;
		pAsset.animation_walk = pDefaultCreatureAsset.animation_walk;
		pAsset.animation_swim = pDefaultCreatureAsset.animation_swim;
		pAsset.only_melee_attack = true;
		pAsset.setZombie(pAnimal);
		pAsset.name_taxonomic_species = "zombus";
		pAsset.can_be_surprised = false;
	}

	public override void editorDiagnostic()
	{
		editorErrorChecks();
		editorNameSetChecks();
		phenotypeChecks();
		base.editorDiagnostic();
	}

	private void phenotypeChecks()
	{
		using ListPool<string> listPool = new ListPool<string>();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ActorAsset item in list)
		{
			if (item.use_phenotypes && (item.phenotypes_list == null || item.phenotypes_list.Count == 0))
			{
				BaseAssetLibrary.logAssetError("<b>ActorAssetLibrary</b>: Unit is set to use phenotypes, but no phenotypes are used", item.id);
			}
			if (item.phenotypes_list != null)
			{
				List<string> trait_group_filter_subspecies = item.trait_group_filter_subspecies;
				if (trait_group_filter_subspecies != null && trait_group_filter_subspecies.Contains("phenotypes"))
				{
					hashSet.UnionWith(item.phenotypes_list);
				}
				else
				{
					listPool.AddRange(item.phenotypes_list);
				}
			}
		}
		hashSet.RemoveAll(listPool);
		foreach (ref string item2 in listPool)
		{
			string current2 = item2;
			if (!AssetManager.phenotype_library.has(current2))
			{
				BaseAssetLibrary.logAssetError("<b>ActorAssetLibrary</b>: Phenotype <e>" + current2 + "</e> not found", current2);
			}
		}
		foreach (PhenotypeAsset item3 in AssetManager.phenotype_library.list)
		{
			if (!listPool.Contains(item3.id) && !hashSet.Contains(item3.id))
			{
				BaseAssetLibrary.logAssetError($"<b>ActorAssetLibrary</b>: Phenotype <e>{item3}</e> not findable, because not used by any units", item3.id);
			}
		}
		foreach (string item4 in hashSet)
		{
			using ListPool<string> listPool2 = new ListPool<string>();
			foreach (ActorAsset item5 in list)
			{
				if (item5.phenotypes_list != null && item5.phenotypes_list.Contains(item4))
				{
					listPool2.Add(item5.id);
				}
			}
			BaseAssetLibrary.logAssetError("<b>ActorAssetLibrary</b>: Phenotype <e>" + item4 + "</e> not reachable, because used by units with hidden phenotypes : <e>" + string.Join(",", listPool2) + "</e>", item4);
		}
	}

	private void editorNameSetChecks()
	{
		foreach (ActorAsset item in list)
		{
			if (item.canBecomeSapient() && !item.unit_zombie && item.name_template_sets == null)
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Name Templates Not Set!!", item.id);
			}
		}
	}

	private void editorErrorChecks()
	{
		foreach (ActorAsset item in list)
		{
			if (item.can_evolve_into_new_species && string.IsNullOrEmpty(item.evolution_id))
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset missing evolution_id", item.id);
			}
			if (item.kingdom_id_wild != string.Empty && !AssetManager.kingdoms.has(item.kingdom_id_wild))
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset has <e>invalid kingdom_id_wild</e> " + item.kingdom_id_wild, item.id);
			}
			if (item.kingdom_id_civilization != string.Empty)
			{
				if (!AssetManager.kingdoms.has(item.kingdom_id_civilization))
				{
					BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset has <e>invalid kingdom_id_civilization</e> " + item.kingdom_id_civilization, item.id);
				}
				else if (!AssetManager.kingdoms.get(item.kingdom_id_civilization).civ)
				{
					BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset has <e>invalid kingdom_id_civilization</e> that is not a .civ " + item.kingdom_id_civilization, item.id);
				}
			}
			if (item.architecture_id != string.Empty && !AssetManager.architecture_library.has(item.architecture_id))
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset has <e>invalid architecture id</e> " + item.architecture_id, item.id);
			}
			if (!item.zombie_auto_asset && !item.unit_zombie && typeof(SA).GetField(item.id, BindingFlags.Static | BindingFlags.Public) == null)
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset does not have <e>SA property</e>", item.id);
			}
			if (item.use_phenotypes && (item.phenotypes_dict == null || item.phenotypes_dict.Count == 0))
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset does not have <e>colors set</e>", item.id);
			}
			if (item.can_have_subspecies && (item.phenotypes_dict == null || item.phenotypes_dict.Count == 0) && item.use_phenotypes)
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset can have subspecies, but no default color sets", item.id);
			}
			if (item.can_have_subspecies && item.genome_parts.Count == 0)
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset does not have <e>genes set</e>", item.id);
			}
			if (string.IsNullOrEmpty(item.icon))
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset does not have <e>icon set</e>", item.id);
			}
			else if ((Object)(object)item.getSpriteIcon() == (Object)null)
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: Actor Asset <e>sprite is missing</e> on path <e>" + item.icon + "</e>", item.id);
			}
			if (!string.IsNullOrEmpty(item.banner_id) && SpriteTextureLoader.getSpriteList(KingdomBannerLibrary.getFullPathBackground(item.banner_id)).Length == 0)
			{
				BaseAssetLibrary.logAssetError("ActorAssetLibrary: there's <e>no folder for banners</e> for", item.id + " with banner id " + item.banner_id);
			}
		}
	}

	public void preloadMainUnitSprites()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.preload_units)
		{
			return;
		}
		foreach (ActorAsset item in list)
		{
			if (item.has_override_sprite || !item.has_sprite_renderer)
			{
				continue;
			}
			item.texture_asset.preloadSprites(item.civ, item.has_baby_form, item);
			if (item.shadow)
			{
				ActorTextureSubAsset texture_asset = item.texture_asset;
				if (texture_asset.shadow_size.x < 1f || texture_asset.shadow_size.y < 1f)
				{
					BaseAssetLibrary.logAssetError($"ActorAssetLibrary: Shadow size is too small : <e>{texture_asset.shadow_size}</e>", item.id);
				}
				if (texture_asset.shadow_size_egg.x < 1f || texture_asset.shadow_size_egg.y < 1f)
				{
					BaseAssetLibrary.logAssetError($"ActorAssetLibrary: Egg shadow size is too small : <e>{texture_asset.shadow_size_egg}</e>", item.id);
				}
				if (texture_asset.shadow_size_baby.x < 1f || texture_asset.shadow_size_baby.y < 1f)
				{
					BaseAssetLibrary.logAssetError($"ActorAssetLibrary: Baby shadow size is too small : <e>{texture_asset.shadow_size_baby}</e>", item.id);
				}
			}
		}
	}

	public override void editorDiagnosticLocales()
	{
		foreach (ActorAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			if (item.can_have_subspecies)
			{
				checkLocale(item, item.getCollectiveTermID());
			}
		}
	}

	private void initAnimalsNormal()
	{
		clone("fox", "$carnivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("fox_set");
		t.kingdom_id_wild = "fox";
		t.kingdom_id_civilization = "miniciv_fox";
		t.base_stats["mass_2"] = 7f;
		t.addGenome(("health", 80f), ("stamina", 80f), ("mutation", 1f), ("lifespan", 90f), ("damage", 18f), ("speed", 7f), ("armor", 5f), ("offspring", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.name_locale = "Fox";
		t.setSocialStructure("group_skulk", 12);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "canidae";
		t.name_taxonomic_genus = "vulpes";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S8_Fox;
		t.shadow_texture = "unitShadow_4";
		t.icon = "iconFox";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("bright_orange");
		t.color_hex = "#C2974E";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_fox";
		t.architecture_id = "civ_fox";
		t.banner_id = "civ_fox";
		t.clearTraits();
		addTrait("genius");
		addTrait("fast");
		t.addResource("leather", 1);
		clone("buffalo", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("buffalo_set");
		t.kingdom_id_wild = "buffalo";
		t.kingdom_id_civilization = "miniciv_buffalo";
		t.base_stats["mass_2"] = 650f;
		t.addGenome(("health", 180f), ("stamina", 200f), ("mutation", 1f), ("speed", 11f), ("lifespan", 80f), ("damage", 20f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_graminivore");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("dense_dwellings");
		t.name_locale = "Buffalo";
		t.setSocialStructure("group_herd", 200);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "artiodactyla";
		t.name_taxonomic_family = "bovidae";
		t.name_taxonomic_genus = "syncerus";
		t.name_taxonomic_species = "caffer";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S16_Buffalo;
		t.shadow_texture = "unitShadow_7";
		t.icon = "iconBuffalo";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("soil");
		t.color_hex = "#C2974E";
		t.max_random_amount = 3;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_buffalo";
		t.architecture_id = "civ_buffalo";
		t.banner_id = "civ_buffalo";
		t.clearTraits();
		addTrait("strong");
		addTrait("tough");
		t.addResource("leather", 2);
		clone("hyena", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("hyena_set");
		t.kingdom_id_wild = "hyena";
		t.kingdom_id_civilization = "miniciv_hyena";
		t.base_stats["mass_2"] = 63f;
		t.addGenome(("health", 130f), ("stamina", 150f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 18f), ("armor", 5f), ("offspring", 6f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("super_positivity");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("conscription_female_only");
		t.addCultureTrait("city_layout_tile_wobbly_pattern");
		t.name_locale = "Hyena";
		t.setSocialStructure("group_cackle", 30);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "hyaenidae";
		t.name_taxonomic_genus = "crocuta";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S10_Dog;
		t.shadow_texture = "unitShadow_5";
		t.icon = "iconHyena";
		t.color_hex = "#C2974E";
		t.max_random_amount = 2;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_hyena";
		t.architecture_id = "civ_hyena";
		t.banner_id = "civ_hyena";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("savanna");
		t.addResource("leather", 1);
		clone("crocodile", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
		t.kingdom_id_wild = "crocodile";
		t.kingdom_id_civilization = "miniciv_crocodile";
		t.base_stats["mass_2"] = 450f;
		t.addGenome(("health", 180f), ("stamina", 40f), ("mutation", 1f), ("speed", 5f), ("lifespan", 70f), ("damage", 30f), ("armor", 15f), ("offspring", 10f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addSubspeciesTrait("aquatic");
		t.name_locale = "Crocodile";
		t.setSocialStructure("group_bask", 20);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "reptilia";
		t.name_taxonomic_order = "crocodilia";
		t.name_taxonomic_family = "crocodylidae";
		t.name_taxonomic_genus = "crocodylus";
		t.inspect_avatar_scale = 1.3f;
		t.base_stats["mass"] = 20f;
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S15_Bear;
		t.shadow_texture = "unitShadow_21";
		t.icon = "iconCrocodile";
		t.color_hex = "#C2974E";
		t.prevent_unconscious_rotation = true;
		t.immune_to_slowness = true;
		t.max_random_amount = 2;
		t.force_land_creature = true;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_crocodile";
		t.architecture_id = "civ_crocodile";
		t.banner_id = "civ_crocodile";
		addTrait("tough");
		t.clonePhenotype("$animal_skin$");
		addPhenotype("dark_green");
		t.addResource("leather", 2);
		clone("monkey", "$herbivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("monkey_set");
		t.kingdom_id_wild = "monkey";
		t.kingdom_id_civilization = "miniciv_monkey";
		t.base_stats["mass_2"] = 50f;
		t.addGenome(("health", 80f), ("stamina", 150f), ("mutation", 1f), ("speed", 14f), ("lifespan", 80f), ("damage", 12f), ("armor", 5f), ("offspring", 8f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("nimble");
		t.addSubspeciesTrait("shiny_love");
		t.name_locale = "Monkey";
		t.setSocialStructure("group_troop", 50);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "primates";
		t.name_taxonomic_family = "cercopithecidae";
		t.name_taxonomic_genus = "macaca";
		t.name_taxonomic_species = "mulatta";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S9_Monkey;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconMonkey";
		t.color_hex = "#C2974E";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_monkey";
		t.architecture_id = "civ_monkey";
		t.banner_id = "civ_monkey";
		t.clearTraits();
		addTrait("genius");
		addTrait("agile");
		t.clonePhenotype("$animal_fur$");
		addPhenotype("dark_orange");
		t.default_attack = "rocks";
		t.addResource("leather", 1);
		clone("rhino", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("rhino_set");
		t.kingdom_id_wild = "rhino";
		t.kingdom_id_civilization = "miniciv_rhino";
		t.base_stats["mass_2"] = 700f;
		t.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("speed", 12f), ("lifespan", 80f), ("damage", 20f), ("armor", 15f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("aggressive");
		t.name_locale = "Rhino";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "perissodactyla";
		t.name_taxonomic_family = "rhinocerotidae";
		t.name_taxonomic_genus = "rhinoceros";
		t.collective_term = "group_crash";
		t.base_stats["mass"] = 20f;
		t.base_stats["targets"] = 3f;
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S16_Buffalo;
		t.shadow_texture = "unitShadow_11";
		t.icon = "iconRhino";
		t.color_hex = "#C2974E";
		t.max_random_amount = 1;
		t.animal_breeding_close_units_limit = 4;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_rhino";
		t.architecture_id = "civ_rhino";
		t.banner_id = "civ_rhino";
		t.clearTraits();
		t.addTrait("strong");
		t.addTrait("fat");
		t.addTrait("dash");
		t.addTrait("hard_skin");
		addPhenotype("mid_gray");
		addPhenotype("skin_medium");
		t.addResource("meat", 2);
		t.addResource("leather", 4);
		clone("frog", "$peaceful_animal$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("frog_set");
		t.kingdom_id_wild = "frog";
		t.kingdom_id_civilization = "miniciv_frog";
		t.base_stats["mass_2"] = 1.5f;
		t.addGenome(("health", 40f), ("stamina", 20f), ("mutation", 2f), ("speed", 7f), ("lifespan", 90f), ("damage", 12f), ("birth_rate", 3f), ("offspring", 30f));
		t.name_locale = "Frog";
		t.setSocialStructure("group_army", 100);
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_bubble");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_insectivore");
		t.addSubspeciesTrait("adaptation_swamp");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "amphibia";
		t.name_taxonomic_order = "anura";
		t.name_taxonomic_family = "ranidae";
		t.name_taxonomic_genus = "rana";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S2_Crab;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconFrog";
		t.color_hex = "#C2974E";
		t.immune_to_slowness = true;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_frog";
		t.architecture_id = "civ_frog";
		t.banner_id = "civ_frog";
		t.clearTraits();
		addTrait("poisonous");
		addTrait("weightless");
		addPhenotype("bright_green");
		addPhenotype("infernal", "biome_jungle");
		addPhenotype("skin_medium", "biome_savanna");
		addPhenotype("pink_yellow_mushroom", "biome_mushroom");
		addPhenotype("aqua", "biome_corrupted");
		addPhenotype("aqua", "biome_swamp");
		addPhenotype("desert", "biome_desert");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("lemon", "biome_lemon");
		t.addResource("leather", 1);
		clone("snake", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("snake_set");
		t.kingdom_id_wild = "snake";
		t.kingdom_id_civilization = "miniciv_snake";
		t.base_stats["mass_2"] = 15f;
		t.addGenome(("health", 40f), ("stamina", 10f), ("mutation", 2f), ("speed", 7f), ("lifespan", 150f), ("damage", 20f), ("birth_rate", 3f), ("offspring", 20f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addSubspeciesTrait("circadian_drift");
		addTrait("poison_immune");
		addTrait("venomous");
		addTrait("weightless");
		t.name_locale = "Snake";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "reptilia";
		t.name_taxonomic_order = "squamata";
		t.name_taxonomic_family = "elapidae";
		t.name_taxonomic_genus = "naja";
		t.collective_term = "group_den";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S5_Snake;
		t.shadow_texture = "unitShadow_4";
		t.icon = "iconSnake";
		t.color_hex = "#C2974E";
		t.immune_to_slowness = true;
		t.can_attack_buildings = false;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_snake";
		t.architecture_id = "civ_snake";
		t.banner_id = "civ_snake";
		addPhenotype("dark_green");
		addPhenotype("dark_orange");
		addPhenotype("bright_green", "biome_jungle");
		addPhenotype("savanna", "biome_savanna");
		addPhenotype("aqua", "biome_swamp");
		addPhenotype("corrupted", "biome_corrupted");
		addPhenotype("desert", "biome_desert");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("lemon", "biome_lemon");
		t.default_attack = "bite";
		t.addResource("leather", 1);
		clone("dog", "$peaceful_animal$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("wolf_set");
		t.base_stats["mass_2"] = 45f;
		t.kingdom_id_wild = "dog";
		t.kingdom_id_civilization = "miniciv_dog";
		t.addGenome(("health", 80f), ("stamina", 120f), ("mutation", 1f), ("lifespan", 20f), ("damage", 18f), ("speed", 15f), ("armor", 5f), ("offspring", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("super_positivity");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_omnivore");
		t.name_locale = "Dog";
		t.setSocialStructure("group_pack", 20);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "canidae";
		t.name_taxonomic_genus = "canis";
		t.name_taxonomic_species = "lupus";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S10_Dog;
		t.shadow_texture = "unitShadow_5";
		t.icon = "iconDog";
		t.color_hex = "#393939";
		t.default_attack = "jaws";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_dog";
		t.architecture_id = "civ_dog";
		t.banner_id = "civ_dog";
		addPhenotype("white_gray");
		addPhenotype("gray_black");
		addPhenotype("wood");
		addPhenotype("polar", "biome_permafrost");
		addPhenotype("skin_black", "biome_corrupted");
		addPhenotype("desert", "biome_desert");
		t.clearTraits();
		addTrait("fast");
		addTrait("dash");
		t.addResource("leather", 1);
		clone("wolf", "$carnivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("wolf_set");
		t.base_stats["mass_2"] = 55f;
		t.kingdom_id_wild = "wolf";
		t.kingdom_id_civilization = "miniciv_wolf";
		t.addGenome(("health", 120f), ("stamina", 150f), ("mutation", 1f), ("lifespan", 20f), ("damage", 22f), ("speed", 15f), ("armor", 3f), ("offspring", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_carnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addClanTrait("combat_instincts");
		t.name_locale = "Wolfs";
		t.setSocialStructure("group_pack", 20);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "canidae";
		t.name_taxonomic_genus = "canis";
		t.name_taxonomic_species = "lupus";
		t.can_attack_buildings = false;
		t.actor_size = ActorSize.S12_Wolf;
		t.shadow_texture = "unitShadow_7";
		t.icon = "iconWolf";
		t.color_hex = "#393939";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_wolf";
		t.architecture_id = "civ_wolf";
		t.banner_id = "civ_wolf";
		addTrait("nightchild");
		t.clonePhenotype("$animal_fur$");
		addPhenotype("wood");
		addPhenotype("white_gray");
		addPhenotype("gray_black");
		addPhenotype("dark_red", "biome_infernal");
		t.music_theme = "Units_Wolf";
		t.addResource("leather", 1);
		clone("bear", "$carnivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("bear_set");
		t.base_stats["mass_2"] = 175f;
		t.kingdom_id_wild = "bear";
		t.kingdom_id_civilization = "miniciv_bear";
		t.addGenome(("health", 200f), ("stamina", 200f), ("mutation", 1f), ("lifespan", 35f), ("damage", 30f), ("speed", 15f), ("armor", 8f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("big_stomach");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_carnivore");
		t.addSubspeciesTrait("diet_frugivore");
		t.addSubspeciesTrait("winter_slumberers");
		t.addSubspeciesTrait("energy_preserver");
		t.addSubspeciesTrait("aggressive");
		t.name_locale = "Bear";
		t.setSocialStructure("group_sleuth", 4);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "ursidae";
		t.name_taxonomic_genus = "ursus";
		t.base_stats["mass"] = 2f;
		t.base_stats["targets"] = 2f;
		t.can_attack_buildings = false;
		t.actor_size = ActorSize.S15_Bear;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconBear";
		t.color_hex = "#6C522D";
		addTrait("strong");
		t.default_attack = "claws";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_bear";
		t.architecture_id = "civ_bear";
		t.banner_id = "civ_bear";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("wood");
		addPhenotype("skin_dark");
		t.music_theme = "Units_Bear";
		t.addResource("leather", 3);
		clone("piranha", "$animal$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("piranha_set");
		t.kingdom_id_wild = "piranha";
		t.kingdom_id_civilization = "miniciv_piranha";
		t.base_stats["mass_2"] = 10f;
		t.addGenome(("health", 40f), ("stamina", 50f), ("mutation", 1f), ("lifespan", 10f), ("damage", 25f), ("speed", 13f), ("armor", 2f), ("offspring", 20f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_roe");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("aggressive");
		t.addSubspeciesTrait("diet_carnivore");
		t.addSubspeciesTrait("diet_piscivore");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addSubspeciesTrait("aquatic");
		t.addSubspeciesTrait("fins");
		t.name_locale = "Piranha";
		t.setSocialStructure("group_shoal", 30);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "actinopterygii";
		t.name_taxonomic_order = "characiformes";
		t.name_taxonomic_family = "serrasalmidae";
		t.name_taxonomic_genus = "pygocentrus";
		t.name_taxonomic_species = "nattereri";
		t.can_attack_buildings = false;
		t.actor_size = ActorSize.S4_Piranha;
		t.icon = "iconPiranha";
		t.color_hex = "#3483B6";
		t.immune_to_slowness = true;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_piranha";
		t.architecture_id = "civ_piranha";
		t.banner_id = "civ_piranha";
		ActorAsset actorAsset = t;
		actorAsset.action_death = (WorldAction)Delegate.Combine(actorAsset.action_death, new WorldAction(ActionLibrary.checkPiranhaAchievement));
		t.clonePhenotype("$animal_fur$");
		addPhenotype("aqua");
		addPhenotype("bright_salmon");
		t.music_theme = "Units_Piranha";
		t.force_land_creature = false;
		t.addResource("sushi", 1, pNewList: true);
		clone("rabbit", "$herbivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("rabbit_set");
		t.kingdom_id_wild = "rabbit";
		t.kingdom_id_civilization = "miniciv_rabbit";
		t.base_stats["mass_2"] = 4.5f;
		t.addGenome(("health", 50f), ("stamina", 140f), ("mutation", 1f), ("lifespan", 15f), ("damage", 5f), ("armor", 1f), ("speed", 15f), ("offspring", 12f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_short");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("cautious_instincts");
		t.addClanTrait("we_are_legion");
		t.name_locale = "Rabbit";
		t.setSocialStructure("group_colony", 30);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "lagomorpha";
		t.name_taxonomic_family = "leporidae";
		t.name_taxonomic_genus = "oryctolagus";
		t.source_meat = true;
		t.actor_size = ActorSize.S6_Chicken;
		t.shadow_texture = "unitShadow_2";
		t.icon = "iconRabbit";
		t.color_hex = "#D3D6D1";
		addTrait("weightless");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_rabbit";
		t.architecture_id = "civ_rabbit";
		t.banner_id = "civ_rabbit";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("white_gray");
		addPhenotype("wood");
		addTrait("fast");
		t.music_theme = "Units_Rabbit";
		t.addResource("leather", 1);
		clone("cat", "$carnivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("cat_set");
		t.kingdom_id_wild = "cat";
		t.kingdom_id_civilization = "miniciv_cat";
		t.base_stats["mass_2"] = 5f;
		t.addGenome(("health", 85f), ("stamina", 150f), ("mutation", 1f), ("lifespan", 45f), ("damage", 20f), ("speed", 15f), ("armor", 3f), ("offspring", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.name_locale = "Cat";
		t.setSocialStructure("group_clowder", 15);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "felidae";
		t.name_taxonomic_genus = "felis";
		t.name_taxonomic_species = "catus";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S7_Cat;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconCat";
		t.color_hex = "#C2974E";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_cat";
		t.architecture_id = "civ_cat";
		t.banner_id = "civ_cat";
		addTrait("weightless");
		t.default_attack = "claws";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("white_gray");
		addPhenotype("wood");
		addPhenotype("bright_orange");
		t.music_theme = "Units_Cat";
		t.addResource("leather", 1);
		clone("raccoon", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("raccoon_set");
		t.kingdom_id_wild = "raccoon";
		t.kingdom_id_civilization = "miniciv_raccoon";
		t.base_stats["mass_2"] = 9f;
		t.addGenome(("health", 85f), ("stamina", 80f), ("mutation", 1f), ("lifespan", 45f), ("damage", 20f), ("speed", 15f), ("armor", 3f), ("offspring", 4f));
		t.addSubspeciesTrait("nimble");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("shiny_love");
		t.addSubspeciesTrait("circadian_drift");
		t.addReligionTrait("rite_of_dissent");
		t.addClanTrait("silver_tongues");
		t.name_locale = "Raccoon";
		t.setSocialStructure("group_gaze", 15);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "procyonidae";
		t.name_taxonomic_genus = "procyon";
		t.name_taxonomic_species = "lotor";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S7_Cat;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconRaccoon";
		t.color_hex = "#C2974E";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "bandit";
		t.architecture_id = "civ_bandit";
		t.banner_id = "civ_bandit";
		t.default_attack = "claws";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("gray_black");
		addPhenotype("black_blue");
		t.music_theme = "Units_Cat";
		t.disable_jump_animation = true;
		t.addResource("leather", 1);
		clone("seal", "$carnivore$");
		t.needs_to_be_explored = false;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("seal_set");
		t.kingdom_id_wild = "seal";
		t.kingdom_id_civilization = "miniciv_seal";
		t.base_stats["mass_2"] = 90f;
		t.addGenome(("health", 200f), ("stamina", 30f), ("mutation", 1f), ("lifespan", 30f), ("damage", 20f), ("speed", 4f), ("armor", 10f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("diet_piscivore");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("fins");
		t.addReligionTrait("cast_blood_rain");
		t.addCultureTrait("dense_dwellings");
		t.addClanTrait("blood_of_sea");
		t.addTrait("agile");
		t.name_locale = "Seal";
		t.setSocialStructure("group_colony", 50);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "carnivora";
		t.name_taxonomic_family = "phocidae";
		t.name_taxonomic_genus = "phoca";
		t.name_taxonomic_species = "vitulina";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S13_Human;
		t.shadow_texture = "unitShadow_5";
		t.icon = "iconSeal";
		t.color_hex = "#C2974E";
		t.evolution_id = "civ_seal";
		t.can_evolve_into_new_species = true;
		t.architecture_id = "civ_piranha";
		t.banner_id = "civ_seal";
		t.default_attack = "bite";
		t.clonePhenotype("$animal_skin$");
		addPhenotype("black_blue");
		addPhenotype("polar", "biome_permafrost");
		t.addResource("meat", 1);
		t.addResource("leather", 2);
		clone("ostrich", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("ostrich_set");
		t.kingdom_id_wild = "ostrich";
		t.kingdom_id_civilization = "miniciv_ostrich";
		t.base_stats["mass_2"] = 117f;
		t.addGenome(("health", 100f), ("stamina", 150f), ("mutation", 1f), ("lifespan", 40f), ("damage", 20f), ("speed", 15f), ("armor", 0f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addTrait("dash");
		t.addTrait("fast");
		t.name_locale = "Ostrich";
		t.setSocialStructure("group_flock", 10);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "aves";
		t.name_taxonomic_order = "struthioniformes";
		t.name_taxonomic_family = "struthionidae";
		t.name_taxonomic_genus = "struthio";
		t.name_taxonomic_species = "camelus";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S13_Human;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconOstrich";
		t.color_hex = "#C2974E";
		t.can_evolve_into_new_species = false;
		t.architecture_id = "civ_piranha";
		t.banner_id = "civ_druid";
		t.default_attack = "bite";
		t.clonePhenotype("$animal_skin$");
		addPhenotype("black_blue");
		addPhenotype("wood");
		t.addResource("leather", 1);
		clone("unicorn", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("unicorn_set");
		t.kingdom_id_wild = "unicorn";
		t.kingdom_id_civilization = "miniciv_unicorn";
		t.base_stats["mass_2"] = 500f;
		t.addGenome(("health", 500f), ("stamina", 120f), ("mutation", 1f), ("lifespan", 500f), ("damage", 20f), ("speed", 15f), ("armor", 0f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_rainbow");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("bioproduct_gems");
		t.addCultureTrait("city_layout_royal_checkers");
		t.addCultureTrait("fames_crown");
		t.addClanTrait("magic_blood");
		t.addClanTrait("witchs_vein");
		t.addClanTrait("warlocks_vein");
		t.addTrait("heart_of_wizard");
		t.addTrait("healing_aura");
		t.name_locale = "Unicorn";
		t.setSocialStructure("group_herd", 15);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "perissodactyla";
		t.name_taxonomic_family = "equidae";
		t.name_taxonomic_genus = "unicornis";
		t.name_taxonomic_species = "fabulosus";
		t.skip_fight_logic = false;
		t.source_meat = true;
		t.actor_size = ActorSize.S14_Cow;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconUnicorn";
		t.color_hex = "#C2974E";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_unicorn";
		t.architecture_id = "civ_unicorn";
		t.banner_id = "civ_unicorn";
		t.default_attack = "bite";
		t.clonePhenotype("$animal_skin$");
		addPhenotype("skin_pale");
		addPhenotype("polar");
		addPhenotype("candy");
		t.addTrait("blessed");
		t.addResource("gems", 1);
		t.addResource("leather", 2);
		clone("rat", "$omnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("rat_set");
		t.kingdom_id_wild = "rat";
		t.kingdom_id_civilization = "miniciv_rat";
		t.base_stats["mass_2"] = 0.5f;
		t.addGenome(("health", 30f), ("stamina", 30f), ("mutation", 1f), ("lifespan", 30f), ("damage", 8f), ("armor", 1f), ("speed", 15f), ("offspring", 15f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_short");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("nimble");
		t.addSubspeciesTrait("shiny_love");
		t.addClanTrait("we_are_legion");
		t.name_locale = "Rat";
		t.setSocialStructure("group_colony", 100);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "rodentia";
		t.name_taxonomic_family = "muridae";
		t.name_taxonomic_genus = "rattus";
		t.actor_size = ActorSize.S3_Rat;
		t.shadow_texture = "unitShadow_2";
		t.kingdom_id_wild = "rat";
		t.shadow = true;
		t.source_meat = true;
		t.max_random_amount = 5;
		t.can_attack_buildings = false;
		t.color_hex = "#2D2D2D";
		t.icon = "iconRat";
		addTrait("contagious");
		t.clonePhenotype("$animal_fur$");
		addPhenotype("gray_black");
		addPhenotype("white_gray");
		addPhenotype("wood");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_rat";
		t.architecture_id = "civ_rat";
		t.banner_id = "civ_rat";
		t.music_theme = "Units_Rat";
		t.disable_jump_animation = true;
		clone("chicken", "$herbivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("chicken_set");
		t.kingdom_id_wild = "chicken";
		t.kingdom_id_civilization = "miniciv_chicken";
		t.base_stats["mass_2"] = 4f;
		t.addGenome(("health", 35f), ("stamina", 30f), ("mutation", 1f), ("speed", 7f), ("lifespan", 30f), ("damage", 5f), ("armor", 1f), ("birth_rate", 3f), ("offspring", 12f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("cautious_instincts");
		t.addCultureTrait("dense_dwellings");
		addTrait("peaceful");
		addTrait("weightless");
		addTrait("content");
		t.name_locale = "Chicken";
		t.setSocialStructure("group_flock", 20);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "aves";
		t.name_taxonomic_order = "galliformes";
		t.name_taxonomic_family = "phasianidae";
		t.name_taxonomic_genus = "gallus";
		t.source_meat = true;
		t.actor_size = ActorSize.S6_Chicken;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconChicken";
		t.color_hex = "#DEDAC4";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_chicken";
		t.architecture_id = "civ_chicken";
		t.banner_id = "civ_chicken";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("wood");
		addPhenotype("white_gray");
		addPhenotype("dark_orange");
		t.music_theme = "Units_Chicken";
		clone("sheep", "$herbivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("sheep_set");
		t.kingdom_id_wild = "sheep";
		t.kingdom_id_civilization = "miniciv_sheep";
		t.base_stats["mass_2"] = 65f;
		t.addGenome(("health", 90f), ("stamina", 10f), ("mutation", 1f), ("lifespan", 100f), ("damage", 10f), ("speed", 6f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_graminivore");
		t.addSubspeciesTrait("cautious_instincts");
		t.name_locale = "Sheep";
		t.setSocialStructure("group_flock", 100);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "artiodactyla";
		t.name_taxonomic_family = "bovidae";
		t.name_taxonomic_genus = "ovis";
		t.name_taxonomic_species = "aries";
		t.source_meat = true;
		t.actor_size = ActorSize.S11_Sheep;
		t.icon = "iconSheep";
		t.color_hex = "#D7D7D7";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_sheep";
		t.architecture_id = "civ_sheep";
		t.banner_id = "civ_sheep";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("white_gray");
		addPhenotype("skin_mixed");
		addPhenotype("polar");
		t.music_theme = "Units_Sheep";
		t.addResource("leather", 2);
		clone("cow", "$herbivore$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("cow_set");
		t.kingdom_id_wild = "cow";
		t.kingdom_id_civilization = "miniciv_cow";
		t.base_stats["mass_2"] = 550f;
		t.addGenome(("health", 120f), ("stamina", 20f), ("mutation", 1f), ("lifespan", 100f), ("damage", 10f), ("speed", 6f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_graminivore");
		t.name_locale = "Cow";
		t.setSocialStructure("group_herd", 50);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "artiodactyla";
		t.name_taxonomic_family = "bovidae";
		t.name_taxonomic_genus = "bos";
		t.name_taxonomic_species = "taurus";
		t.source_meat = true;
		t.actor_size = ActorSize.S14_Cow;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconCow";
		t.color_hex = "#D7D7D7";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_cow";
		t.architecture_id = "civ_cow";
		t.banner_id = "civ_cow";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("white_gray");
		t.addResource("leather", 3);
		clone("penguin", "$animal$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("penguin_set");
		t.kingdom_id_wild = "penguin";
		t.kingdom_id_civilization = "miniciv_penguin";
		t.base_stats["mass_2"] = 35f;
		t.addGenome(("health", 70f), ("stamina", 100f), ("mutation", 1f), ("speed", 5f), ("lifespan", 20f), ("damage", 7f), ("armor", 2f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_spotted");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_piscivore");
		t.addSubspeciesTrait("adaptation_permafrost");
		t.addCultureTrait("matriarchy");
		t.name_locale = "Penguin";
		t.setSocialStructure("group_colony", 200);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "aves";
		t.name_taxonomic_order = "sphenisciformes";
		t.name_taxonomic_family = "spheniscidae";
		t.name_taxonomic_genus = "aptenodytes";
		t.source_meat = true;
		t.actor_size = ActorSize.S10_Dog;
		t.shadow_texture = "unitShadow_3";
		t.icon = "iconPenguin";
		t.color_hex = "#D7D7D7";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_penguin";
		t.architecture_id = "civ_penguin";
		t.banner_id = "civ_penguin";
		addTrait("weightless");
		addPhenotype("black_blue");
		clone("armadillo", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("armadillo_set");
		t.kingdom_id_wild = "armadillo";
		t.kingdom_id_civilization = "miniciv_armadillo";
		t.base_stats["mass_2"] = 5f;
		t.addGenome(("health", 200f), ("stamina", 50f), ("mutation", 1f), ("lifespan", 100f), ("damage", 20f), ("armor", 20f), ("speed", 8f), ("offspring", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addTrait("hard_skin");
		t.addTrait("block");
		t.name_locale = "Armadillo";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "cingulata";
		t.name_taxonomic_family = "dasypodidae";
		t.name_taxonomic_genus = "dasypus";
		t.collective_term = "group_roll";
		t.source_meat = true;
		t.actor_size = ActorSize.S6_Chicken;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconArmadillo";
		t.color_hex = "#D7D7D7";
		t.can_evolve_into_new_species = true;
		t.clonePhenotype("$animal_fur$");
		addPhenotype("dark_orange");
		addPhenotype("wood");
		t.evolution_id = "civ_armadillo";
		t.architecture_id = "civ_armadillo";
		t.banner_id = "civ_armadillo";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		clone("alpaca", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("alpaca_set");
		t.kingdom_id_wild = "alpaca";
		t.kingdom_id_civilization = "miniciv_alpaca";
		t.base_stats["mass_2"] = 67f;
		t.addGenome(("health", 100f), ("stamina", 30f), ("mutation", 1f), ("lifespan", 20f), ("damage", 9f), ("speed", 8f), ("armor", 2f), ("offspring", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addTrait("soft_skin");
		t.addReligionTrait("path_of_unity");
		t.name_locale = "Alpaca";
		t.setSocialStructure("group_herd", 50);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "artiodactyla";
		t.name_taxonomic_family = "camelidae";
		t.name_taxonomic_genus = "lama";
		t.name_taxonomic_species = "pacos";
		t.source_meat = true;
		t.actor_size = ActorSize.S12_Wolf;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconAlpaca";
		t.color_hex = "#D7D7D7";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("skin_dark");
		addPhenotype("white_gray");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_alpaca";
		t.architecture_id = "civ_alpaca";
		t.banner_id = "civ_alpaca";
		t.addResource("leather", 2);
		clone("capybara", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("capybara_set");
		t.kingdom_id_wild = "capybara";
		t.kingdom_id_civilization = "miniciv_capybara";
		t.base_stats["mass_2"] = 50f;
		t.addGenome(("health", 80f), ("stamina", 10f), ("mutation", 1f), ("lifespan", 10f), ("damage", 9f), ("speed", 5f), ("armor", 2f), ("offspring", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addTrait("peaceful");
		t.addTrait("content");
		t.name_locale = "Capybara";
		t.setSocialStructure("group_herd", 20);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "rodentia";
		t.name_taxonomic_family = "caviidae";
		t.name_taxonomic_genus = "hydrochoerus";
		t.source_meat = true;
		t.actor_size = ActorSize.S12_Wolf;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconCapybara";
		t.color_hex = "#D7D7D7";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("wood");
		addPhenotype("dark_orange");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_capybara";
		t.architecture_id = "civ_capybara";
		t.banner_id = "civ_capybara";
		t.addResource("leather", 1);
		clone("goat", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("goat_set");
		t.kingdom_id_wild = "goat";
		t.kingdom_id_civilization = "miniciv_goat";
		t.base_stats["mass_2"] = 30f;
		t.addGenome(("health", 90f), ("stamina", 100f), ("mutation", 1f), ("lifespan", 20f), ("damage", 11f), ("armor", 2f), ("speed", 12f), ("offspring", 3f));
		t.addTrait("dash");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.name_locale = "Goat";
		t.setSocialStructure("group_flock", 100);
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "artiodactyla";
		t.name_taxonomic_family = "bovidae";
		t.name_taxonomic_genus = "capra";
		t.name_taxonomic_species = "hircus";
		t.source_meat = true;
		t.actor_size = ActorSize.S12_Wolf;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconGoat";
		t.color_hex = "#D7D7D7";
		t.clonePhenotype("$animal_fur$");
		addPhenotype("gray_black");
		addPhenotype("white_gray");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_goat";
		t.architecture_id = "civ_goat";
		t.banner_id = "civ_goat";
		t.addResource("leather", 1);
		clone("scorpion", "$carnivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("scorpion_set");
		t.kingdom_id_wild = "scorpion";
		t.kingdom_id_civilization = "miniciv_scorpion";
		t.base_stats["mass_2"] = 0.25f;
		t.addGenome(("health", 40f), ("stamina", 10f), ("mutation", 1f), ("lifespan", 10f), ("damage", 30f), ("armor", 5f), ("speed", 5f), ("offspring", 25f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("adaptation_desert");
		t.addSubspeciesTrait("diet_insectivore");
		t.addCultureTrait("conscription_female_only");
		t.name_locale = "Scorpion";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "arachnida";
		t.name_taxonomic_order = "scorpiones";
		t.name_taxonomic_family = "scorpionidae";
		t.name_taxonomic_genus = "pandinus";
		t.collective_term = "group_bed";
		t.source_meat = true;
		t.actor_size = ActorSize.S12_Wolf;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconScorpion";
		t.color_hex = "#D7D7D7";
		t.disable_jump_animation = true;
		addPhenotype("dark_red");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("bright_yellow", "biome_desert");
		addPhenotype("bright_purple", "biome_corrupted");
		addPhenotype("soil", "biome_savanna");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_scorpion";
		t.architecture_id = "civ_scorpion";
		t.banner_id = "civ_scorpion";
		clone("turtle", "$animal$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("turtle_set");
		t.kingdom_id_wild = "turtle";
		t.kingdom_id_civilization = "miniciv_turtle";
		t.base_stats["mass_2"] = 50f;
		t.addGenome(("health", 150f), ("stamina", 10f), ("mutation", 1f), ("lifespan", 400f), ("damage", 15f), ("speed", 5f), ("armor", 25f), ("birth_rate", 3f), ("offspring", 20f));
		t.name_locale = "Turtle";
		t.setSocialStructure("group_bale", 10);
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_colored");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_algivore");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addSubspeciesTrait("cautious_instincts");
		t.addCultureTrait("matriarchy");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "reptilia";
		t.name_taxonomic_order = "testudines";
		t.name_taxonomic_family = "emydidae";
		t.name_taxonomic_genus = "trachemys";
		t.source_meat = true;
		t.actor_size = ActorSize.S7_Cat;
		t.shadow_texture = "unitShadow_7";
		t.icon = "iconTurtle";
		t.color_hex = "#D7D7D7";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_2;
		t.immune_to_slowness = true;
		t.flag_turtle = true;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_turtle";
		t.architecture_id = "civ_turtle";
		t.banner_id = "civ_turtle";
		t.prevent_unconscious_rotation = true;
		addPhenotype("dark_green");
		addPhenotype("swamp");
		addPhenotype("corrupted");
		addPhenotype("desert");
		addPhenotype("aqua");
		addTrait("slow");
		addTrait("weightless");
		addTrait("genius");
		t.addResource("bones", 2);
		clone("crab", "$animal$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crab_set");
		t.kingdom_id_wild = "crab";
		t.kingdom_id_civilization = "miniciv_crab";
		t.base_stats["mass_2"] = 2f;
		t.addGenome(("health", 60f), ("stamina", 10f), ("mutation", 1f), ("lifespan", 10f), ("damage", 20f), ("armor", 15f), ("speed", 5f), ("birth_rate", 3f), ("offspring", 30f));
		t.name_locale = "Crab";
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("exoskeleton");
		t.addSubspeciesTrait("egg_roe");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_algivore");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("population_minimal");
		t.addTrait("weightless");
		t.addTrait("hard_skin");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "malacostraca";
		t.name_taxonomic_order = "decapoda";
		t.name_taxonomic_family = "portunidae";
		t.name_taxonomic_genus = "carcinus";
		t.collective_term = "group_cast";
		t.disable_jump_animation = true;
		t.source_meat = true;
		t.actor_size = ActorSize.S2_Crab;
		t.shadow_texture = "unitShadow_2";
		t.icon = "iconCrab";
		t.color_hex = "#D7D7D7";
		addPhenotype("bright_salmon");
		addPhenotype("swamp", "biome_swamp");
		addPhenotype("corrupted", "biome_corrupted");
		addPhenotype("desert", "biome_desert");
		addPhenotype("infernal", "biome_infernal");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_crab";
		t.architecture_id = "civ_crab";
		t.banner_id = "civ_crab";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
	}

	private void initAnimalsWeird()
	{
		clone("crystal_sword", "$animal$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crystal_sword_set");
		t.kingdom_id_wild = "crystal_sword";
		t.kingdom_id_civilization = "miniciv_crystal_sword";
		t.base_stats["mass_2"] = 10f;
		t.addGenome(("health", 100f), ("stamina", 50f), ("mutation", 1f), ("lifespan", 1000f), ("damage", 50f), ("armor", 20f), ("speed", 10f), ("offspring", 2f));
		t.setSocialStructure("group_guild", 30);
		t.addClanTrait("blood_of_eons");
		t.addClanTrait("combat_instincts");
		t.addCultureTrait("sword_lovers");
		t.addCultureTrait("city_layout_diamond");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_crystal");
		t.addSubspeciesTrait("reproduction_budding");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("bioluminescence");
		t.addSubspeciesTrait("heat_resistance");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("death_grow_mythril");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_lithotroph");
		t.addSubspeciesTrait("bioproduct_gems");
		t.addReligionTrait("rite_of_infinite_edges");
		t.addTrait("deflect_projectile");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "cnidaria";
		t.name_taxonomic_class = "anthozoa";
		t.name_taxonomic_order = "crystalliformes";
		t.name_taxonomic_family = "crystallidae";
		t.name_taxonomic_genus = "gladii";
		t.name_taxonomic_species = "volans";
		t.name_locale = "Crystal Sword";
		t.body_separate_part_hands = false;
		t.has_skin = false;
		t.mush_id = "mush_animal";
		t.icon = "iconCrystalSword";
		t.color_hex = "#75D0F4";
		t.disable_jump_animation = true;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.walk_0_3;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.sound_hit = "event:/SFX/HIT/HitStone";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_crystal_golem";
		t.architecture_id = "civ_crystal_golem";
		t.banner_id = "civ_crystal_golem";
		t.prevent_unconscious_rotation = true;
		addPhenotype("crystal");
		addPhenotype("bright_purple");
		addPhenotype("bright_pink");
		addPhenotype("bright_teal");
		addPhenotype("bright_yellow");
		addPhenotype("bright_red");
		addPhenotype("dark_violet", "biome_corrupted");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("bright_green", "biome_swamp");
		addTrait("shiny");
		addTrait("fire_proof");
		addTrait("freeze_proof");
		addTrait("light_lamp");
		t.addResource("gems", 1, pNewList: true);
		t.addResource("crystal_salt", 1);
		clone("smore", "$animal$");
		t.needs_to_be_explored = true;
		t.render_heads_for_babies = false;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("smore_set");
		t.kingdom_id_wild = "smore";
		t.kingdom_id_civilization = "miniciv_smore";
		t.base_stats["mass_2"] = 4f;
		t.addGenome(("health", 300f), ("stamina", 10f), ("mutation", 3f), ("lifespan", 400f), ("speed", 12f), ("damage", 30f), ("armor", 5f), ("offspring", 3f));
		t.addSubspeciesTrait("reproduction_vegetative");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_candy");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("diet_xylophagy");
		t.addSubspeciesTrait("diet_hematophagy");
		t.setSocialStructure("group_diabetes", 10);
		t.name_taxonomic_kingdom = "plantae";
		t.name_taxonomic_phylum = "tracheophyta";
		t.name_taxonomic_class = "magnoliopsida";
		t.name_taxonomic_order = "poales";
		t.name_taxonomic_family = "poaceae";
		t.name_taxonomic_genus = "saccharum";
		t.name_taxonomic_species = "smorex";
		t.name_locale = "Smore";
		t.body_separate_part_hands = false;
		t.has_skin = false;
		t.icon = "iconSmore";
		t.color_hex = "#F74AA6";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_candy_man";
		t.architecture_id = "civ_candy_man";
		t.banner_id = "civ_candy_man";
		addPhenotype("skin_medium");
		addPhenotype("skin_dark");
		addPhenotype("skin_mixed");
		addPhenotype("swamp", "biome_swamp");
		addTrait("flesh_eater");
		addTrait("evil");
		addTrait("gluttonous");
		t.max_random_amount = 3;
		t.addResource("candy", 1, pNewList: true);
		t.addResource("evil_beets", 1);
		clone("acid_blob", "$animal$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("acid_blob_set");
		t.kingdom_id_wild = "acid_blob";
		t.kingdom_id_civilization = "miniciv_acid_blob";
		t.base_stats["mass_2"] = 66f;
		t.addGenome(("health", 120f), ("stamina", 50f), ("mutation", 5f), ("speed", 4f), ("lifespan", 50f), ("damage", 35f), ("offspring", 10f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_blob");
		t.addSubspeciesTrait("reproduction_fission");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("unstable_genome");
		t.addSubspeciesTrait("adaptation_wasteland");
		t.addReligionTrait("cosmic_radiation");
		t.addCultureTrait("happiness_from_war");
		t.name_locale = "Acid Blob";
		t.setSocialStructure("group_legion", 100);
		t.name_taxonomic_kingdom = "protista";
		t.name_taxonomic_phylum = "amoebozoa";
		t.name_taxonomic_class = "myxogastria";
		t.name_taxonomic_order = "liceales";
		t.name_taxonomic_family = "reticulariaceae";
		t.name_taxonomic_genus = "blobicus";
		t.name_taxonomic_species = "slimus";
		t.body_separate_part_hands = false;
		t.has_skin = false;
		t.mush_id = "mush_animal";
		t.icon = "iconAcidBlob";
		t.color_hex = "#008800";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.max_random_amount = 10;
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_acid_gentleman";
		t.architecture_id = "civ_acid_gentleman";
		t.banner_id = "civ_acid_gentleman";
		t.prevent_unconscious_rotation = true;
		addTrait("acid_blood");
		addTrait("acid_proof");
		addTrait("acid_touch");
		addTrait("lustful");
		addTrait("fat");
		addPhenotype("toxic_green");
		addPhenotype("bright_pink", "biome_corrupted");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("swamp", "biome_swamp");
		t.addResource("jam", 1, pNewList: true);
		clone("flower_bud", "$peaceful_animal$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("flower_set");
		t.kingdom_id_wild = "flower_bud";
		t.kingdom_id_civilization = "miniciv_flower_bud";
		t.base_stats["mass_2"] = 5f;
		t.addGenome(("health", 50f), ("stamina", 50f), ("mutation", 3f), ("lifespan", 50f), ("damage", 3f), ("speed", 7f), ("armor", 5f), ("offspring", 5f));
		t.addSubspeciesTrait("reproduction_vegetative");
		t.addSubspeciesTrait("death_grow_plant");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("genetic_mirror");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("conscription_female_only");
		t.addTrait("sunblessed");
		t.name_locale = "Flower Bud";
		t.setSocialStructure("group_platoon", 30);
		t.name_taxonomic_kingdom = "plantae";
		t.name_taxonomic_phylum = "tracheophyta";
		t.name_taxonomic_class = "liliopsida";
		t.name_taxonomic_order = "liliales";
		t.name_taxonomic_family = "liliaceae";
		t.name_taxonomic_genus = "ambulilium";
		t.name_taxonomic_species = "mobilens";
		t.source_meat = true;
		t.actor_size = ActorSize.S12_Wolf;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconFlowerBud";
		t.color_hex = "#D7D7D7";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_liliar";
		t.architecture_id = "civ_liliar";
		t.banner_id = "civ_liliar";
		t.clonePhenotype("$animal_skin$");
		addPhenotype("bright_purple");
		addPhenotype("bright_pink");
		addPhenotype("bright_blue");
		addPhenotype("bright_yellow");
		addPhenotype("bright_red");
		addPhenotype("jungle", "biome_jungle");
		addPhenotype("swamp", "biome_swamp");
		addPhenotype("desert", "biome_desert");
		addPhenotype("polar", "biome_permafrost");
		addPhenotype("bright_orange", "biome_maple");
		t.addResource("herbs", 1, pNewList: true);
		clone("lemon_snail", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("lemon_man_set");
		t.kingdom_id_wild = "lemon_snail";
		t.kingdom_id_civilization = "miniciv_lemon_snail";
		t.base_stats["mass_2"] = 5f;
		t.addGenome(("health", 50f), ("stamina", 10f), ("mutation", 3f), ("lifespan", 55f), ("damage", 12f), ("speed", 4f), ("armor", 10f), ("offspring", 15f));
		t.addSubspeciesTrait("reproduction_hermaphroditic");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_colored");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("slow_builders");
		t.addReligionTrait("cast_cure");
		t.addCultureTrait("city_layout_raindrops");
		t.addTrait("slow");
		t.addTrait("regeneration");
		t.name_locale = "Bitba";
		t.setSocialStructure("group_caravan", 10);
		t.name_taxonomic_kingdom = "plantae";
		t.name_taxonomic_phylum = "tracheophyta";
		t.name_taxonomic_class = "magnoliopsida";
		t.name_taxonomic_order = "sapindales";
		t.name_taxonomic_family = "rutaceae";
		t.name_taxonomic_genus = "citruslimax";
		t.name_taxonomic_species = "nicedrinkus";
		t.source_meat = true;
		t.actor_size = ActorSize.S6_Chicken;
		t.shadow_texture = "unitShadow_6";
		t.disable_jump_animation = true;
		t.icon = "iconLemonSnail";
		t.color_hex = "#D7D7D7";
		addPhenotype("lemon");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_lemon_man";
		t.architecture_id = "civ_lemon_man";
		t.banner_id = "civ_lemon_man";
		t.addResource("lemons", 1, pNewList: true);
		clone("garl", "$herbivore$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("garlic_man_set");
		t.kingdom_id_wild = "garl";
		t.kingdom_id_civilization = "miniciv_garl";
		t.base_stats["mass_2"] = 25f;
		t.addGenome(("health", 80f), ("stamina", 110f), ("mutation", 2f), ("lifespan", 5f), ("damage", 18f), ("armor", 3f), ("speed", 8f), ("offspring", 4f));
		t.addSubspeciesTrait("reproduction_vegetative");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("genetic_mirror");
		t.addTrait("poison_immune");
		t.addTrait("regeneration");
		t.name_locale = "Garl";
		t.setSocialStructure("group_pack", 20);
		t.name_taxonomic_kingdom = "plantae";
		t.name_taxonomic_phylum = "tracheophyta";
		t.name_taxonomic_class = "liliopsida";
		t.name_taxonomic_order = "asparagales";
		t.name_taxonomic_family = "amaryllidaceae";
		t.name_taxonomic_genus = "allium";
		t.name_taxonomic_species = "walkus";
		t.source_meat = true;
		t.actor_size = ActorSize.S6_Chicken;
		t.shadow_texture = "unitShadow_6";
		t.icon = "iconGarl";
		t.color_hex = "#D7D7D7";
		addPhenotype("mid_gray");
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_garlic_man";
		t.architecture_id = "civ_garlic_man";
		t.banner_id = "civ_garlic_man";
		t.addResource("herbs", 1, pNewList: true);
	}

	private void initInsects()
	{
		clone("bee", "$flying_insect$");
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.has_advanced_textures = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
		t.architecture_id = "civ_bee";
		t.banner_id = "civ_bee";
		addPhenotype("bright_yellow");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("city_layout_honeycomb");
		t.addCultureTrait("hive_society");
		t.addGenome(("health", 1f), ("mutation", 1f), ("damage", 10f), ("speed", 5f), ("offspring", 20f));
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "insecta";
		t.name_taxonomic_order = "hymenoptera";
		t.name_taxonomic_family = "apidae";
		t.name_taxonomic_genus = "apis";
		t.name_taxonomic_species = "mellifera";
		t.collective_term = "group_colony";
		t.name_locale = "Bee";
		t.hovering_max = 1f;
		t.icon = "iconBee";
		t.color_hex = "#23F3FF";
		t.addDecision("bee_find_hive");
		t.addDecision("bee_create_hive");
		t.music_theme = "Units_BeeHive";
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("diet_nectarivore");
		t.addSubspeciesTrait("pollinating");
		t.addSubspeciesTrait("reproduction_parthenogenesis");
		clone("fly", "$flying_insect$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
		t.kingdom_id_wild = "fly";
		t.architecture_id = "civ_druid";
		t.banner_id = "civ_druid";
		addPhenotype("black_blue");
		t.addGenome(("health", 1f), ("mutation", 1f), ("damage", 1f), ("speed", 5f), ("offspring", 15f));
		t.icon = "iconFly";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "insecta";
		t.name_taxonomic_order = "diptera";
		t.name_taxonomic_family = "muscidae";
		t.name_taxonomic_genus = "musca";
		t.name_taxonomic_species = "domestica";
		t.collective_term = "group_business";
		t.name_locale = "Fly";
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("diet_nectarivore");
		t.addSubspeciesTrait("egg_bubble");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("reproduction_sexual");
		clone("butterfly", "$flying_insect$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
		t.architecture_id = "civ_druid";
		t.banner_id = "civ_druid";
		addPhenotype("bright_yellow");
		addPhenotype("bright_red");
		addPhenotype("bright_violet");
		addPhenotype("bright_pink");
		addPhenotype("bright_teal");
		t.addGenome(("lifespan", 3f), ("health", 1f), ("mutation", 1f), ("damage", 1f), ("speed", 5f), ("offspring", 10f));
		t.icon = "iconButterfly";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "insecta";
		t.name_taxonomic_order = "lepidoptera";
		t.name_taxonomic_family = "nymphalidae";
		t.name_taxonomic_genus = "danaus";
		t.name_taxonomic_species = "plexippus";
		t.collective_term = "group_kaleidoscope";
		t.name_locale = "Butterfly";
		t.icon = "iconButterfly";
		t.max_random_amount = 6;
		t.color_hex = "#23F3FF";
		ActorAsset actorAsset = t;
		actorAsset.action_death = (WorldAction)Delegate.Combine(actorAsset.action_death, new WorldAction(ActionLibrary.tryToCreatePlants));
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("diet_nectarivore");
		t.addSubspeciesTrait("pollinating");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("egg_cocoon");
		clone("grasshopper", "$insect$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
		t.architecture_id = "civ_druid";
		t.banner_id = "civ_druid";
		addPhenotype("bright_green");
		t.addGenome(("health", 1f), ("mutation", 1f), ("damage", 1f), ("speed", 5f), ("offspring", 10f));
		t.icon = "iconGrasshopper";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "insecta";
		t.name_taxonomic_order = "orthoptera";
		t.name_taxonomic_family = "acrididae";
		t.name_taxonomic_genus = "omocestus";
		t.name_taxonomic_species = "viridulus";
		t.collective_term = "group_cloud";
		t.name_locale = "Grasshopper";
		t.shadow = false;
		clone("beetle", "$insect$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
		t.architecture_id = "civ_beetle";
		t.banner_id = "civ_beetle";
		t.kingdom_id_wild = "insect";
		t.kingdom_id_civilization = "miniciv_insect";
		t.can_evolve_into_new_species = true;
		t.evolution_id = "civ_beetle";
		t.addSubspeciesTrait("diet_xylophagy");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("exoskeleton");
		t.addSubspeciesTrait("population_minimal");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_cocoon");
		addPhenotype("aqua");
		addPhenotype("black_blue");
		addPhenotype("swamp");
		addPhenotype("soil");
		t.addTrait("hard_skin");
		t.addTrait("slow");
		t.addGenome(("health", 1f), ("mutation", 1f), ("damage", 1f), ("speed", 5f), ("offspring", 10f));
		t.icon = "iconBeetle";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "insecta";
		t.name_taxonomic_order = "coleoptera";
		t.name_taxonomic_family = "scarabaeidae";
		t.name_taxonomic_genus = "sisyphus";
		t.name_taxonomic_species = "schaefferi";
		t.collective_term = "group_swarm";
		t.name_locale = "Beetle";
		t.disable_jump_animation = true;
		t.shadow = true;
	}

	private void initBoats()
	{
		ActorAsset obj = new ActorAsset
		{
			id = "$boat$",
			can_be_killed_by_stuff = true,
			can_be_killed_by_life_eraser = true,
			can_attack_buildings = true,
			can_be_moved_by_powers = true,
			can_be_hurt_by_powers = true,
			update_z = true,
			effect_damage = true,
			force_ocean_creature = true,
			shadow = false,
			is_boat = true,
			kingdom_id_wild = "neutral_animals",
			can_be_inspected = true,
			inspect_children = false,
			inspect_sex = false,
			inspect_show_species = false,
			inspect_generation = false,
			inspect_home = true,
			immune_to_injuries = true,
			path_movement_timeout = 0.1f,
			split_ai_update = false,
			show_on_meta_layer = true,
			show_in_knowledge_window = false,
			show_in_taxonomy_tooltip = false,
			force_hide_stamina = true,
			force_hide_mana = true,
			can_talk_with = false,
			control_can_backstep = false,
			control_can_jump = false,
			control_can_kick = false,
			control_can_dash = false,
			control_can_talk = false,
			control_can_swear = false,
			control_can_steal = false,
			needs_to_be_explored = false,
			show_controllable_tip = false
		};
		ActorAsset pAsset = obj;
		t = obj;
		add(pAsset);
		t.inspect_genealogy = false;
		t.need_colored_sprite = true;
		t.allowed_status_tiers = StatusTier.Basic;
		t.render_status_effects = false;
		t.texture_atlas = UnitTextureAtlasID.Boats;
		t.name_locale = "Boats";
		t.inspect_avatar_scale = 1f;
		t.color_hex = "#000000";
		t.base_stats["scale"] = 0.25f;
		t.base_stats["mass"] = 1000f;
		t.base_stats["size"] = 1f;
		t.can_edit_traits = false;
		addTrait("boat");
		t.sound_hit = "event:/SFX/HIT/HitWood";
		t.can_be_surprised = false;
		t.icon = "iconBoat";
		t.job = AssetLibrary<ActorAsset>.a<string>("decision");
		t.sound_attack = null;
		t.sound_spawn = null;
		t.sound_idle = null;
		t.sound_death = null;
		t.addDecision("boat_check_existence");
		t.addDecision("boat_danger_check");
		t.addDecision("boat_idle");
		t.addDecision("boat_check_limits");
		t.prevent_unconscious_rotation = true;
		t.animation_speed_based_on_walk_speed = false;
		t.get_override_sprite = delegate(Actor pActor)
		{
			Boat simpleComponent = pActor.getSimpleComponent<Boat>();
			AnimationDataBoat animationDataBoat = simpleComponent.getAnimationDataBoat();
			ActorAnimation value = animationDataBoat.normal;
			if (!pActor.isAlive())
			{
				value = animationDataBoat.broken;
			}
			else if (pActor.position_height != 0f || pActor.isInMagnet())
			{
				value = animationDataBoat.normal;
			}
			else if (!animationDataBoat.dict.TryGetValue(simpleComponent.last_movement_angle, out value))
			{
				int closestAngle = Toolbox.getClosestAngle(simpleComponent.last_movement_angle, animationDataBoat);
				animationDataBoat.dict.TryGetValue(closestAngle, out value);
			}
			if (value == null)
			{
				value = animationDataBoat.normal;
			}
			Sprite result = value.frames[0];
			if (value.frames.Length != 0)
			{
				result = AnimationHelper.getSpriteFromList(0, value.frames, pActor.asset.animation_swim_speed);
			}
			return result;
		};
		t.use_tool_items = false;
		clone("$boat_trading$", "$boat$");
		t.default_attack = "boat_cannonball";
		t.boat_type = "boat_type_trading";
		t.base_stats["health"] = 200f;
		t.base_stats["speed"] = 30f;
		t.base_stats["mass_2"] = 3000f;
		t.base_stats["attack_speed"] = 0.1f;
		t.draw_boat_mark = true;
		t.cost = new ConstructionCost(10, 0, 0, 10);
		t.actor_size = ActorSize.S16_Buffalo;
		addTrait("light_lamp");
		t.addDecision("boat_trading");
		clone("$boat_transport$", "$boat$");
		t.default_attack = "boat_cannonball";
		t.boat_type = "boat_type_transport";
		t.base_stats["health"] = 1000f;
		t.base_stats["speed"] = 25f;
		t.base_stats["mass_2"] = 2000f;
		t.base_stats["attack_speed"] = 0.5f;
		t.draw_boat_mark = true;
		t.draw_boat_mark_big = true;
		t.is_boat_transport = true;
		t.cost = new ConstructionCost(5, 0, 2, 20);
		t.actor_size = ActorSize.S17_Dragon;
		addTrait("light_lamp");
		t.addDecision("boat_transport_check");
		clone("boat_fishing", "$boat$");
		t.skip_fight_logic = true;
		t.boat_type = "boat_type_fishing";
		t.base_stats["speed"] = 10f;
		t.base_stats["health"] = 100f;
		t.base_stats["mass_2"] = 100f;
		t.cost = new ConstructionCost(5, 0, 0, 5);
		t.actor_size = ActorSize.S15_Bear;
		t.addDecision("boat_fishing");
		clone("boat_transport_human", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_orc", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_elf", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_dwarf", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_acid_gentleman", "$boat_transport$");
		t.default_attack = "boat_acid_ball";
		clone("boat_transport_alpaca", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_angle", "$boat_transport$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_transport_armadillo", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_bear", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_buffalo", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_candy_man", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_capybara", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_cat", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_chicken", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_cow", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_crab", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_crocodile", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_crystal_golem", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_dog", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_fox", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_frog", "$boat_transport$");
		t.default_attack = "boat_acid_ball";
		clone("boat_transport_garlic_man", "$boat_transport$");
		t.default_attack = "boat_acid_ball";
		clone("boat_transport_goat", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_hyena", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_lemon_man", "$boat_transport$");
		t.default_attack = "boat_acid_ball";
		clone("boat_transport_liliar", "$boat_transport$");
		t.default_attack = "boat_acid_ball";
		clone("boat_transport_monkey", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_penguin", "$boat_transport$");
		t.default_attack = "boat_snowball";
		clone("boat_transport_piranha", "$boat_transport$");
		t.default_attack = "boat_necro_ball";
		clone("boat_transport_rabbit", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_rat", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_rhino", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_scorpion", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_sheep", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_snake", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_turtle", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_wolf", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_white_mage", "$boat_transport$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_transport_snowman", "$boat_transport$");
		t.default_attack = "boat_snowball";
		clone("boat_transport_necromancer", "$boat_transport$");
		t.default_attack = "boat_necro_ball";
		clone("boat_transport_evil_mage", "$boat_transport$");
		t.default_attack = "boat_fireball";
		clone("boat_transport_druid", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_bee", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_beetle", "$boat_transport$");
		t.default_attack = "rocks";
		clone("boat_transport_seal", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_unicorn", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_ghost", "$boat_transport$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_transport_fairy", "$boat_transport$");
		t.default_attack = "boat_arrow";
		clone("boat_transport_demon", "$boat_transport$");
		t.default_attack = "boat_fireball";
		clone("boat_transport_cold_one", "$boat_transport$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_transport_bandit", "$boat_transport$");
		t.default_attack = "boat_cannonball";
		clone("boat_transport_alien", "$boat_transport$");
		t.default_attack = "boat_plasma_ball";
		clone("boat_transport_greg", "$boat_transport$");
		t.default_attack = "boat_plasma_ball";
		clone("boat_trading_human", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_orc", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_elf", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_dwarf", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_acid_gentleman", "$boat_trading$");
		t.default_attack = "boat_acid_ball";
		clone("boat_trading_alpaca", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_angle", "$boat_trading$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_trading_armadillo", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_bear", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_buffalo", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_candy_man", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_capybara", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_cat", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_chicken", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_cow", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_crab", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_crocodile", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_crystal_golem", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_dog", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_fox", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_frog", "$boat_trading$");
		t.default_attack = "boat_acid_ball";
		clone("boat_trading_garlic_man", "$boat_trading$");
		t.default_attack = "boat_acid_ball";
		clone("boat_trading_goat", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_hyena", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_lemon_man", "$boat_trading$");
		t.default_attack = "boat_acid_ball";
		clone("boat_trading_liliar", "$boat_trading$");
		t.default_attack = "boat_acid_ball";
		clone("boat_trading_monkey", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_penguin", "$boat_trading$");
		t.default_attack = "boat_snowball";
		clone("boat_trading_piranha", "$boat_trading$");
		t.default_attack = "boat_necro_ball";
		clone("boat_trading_rabbit", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_rat", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_rhino", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_scorpion", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_sheep", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_snake", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_turtle", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_wolf", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_white_mage", "$boat_trading$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_trading_snowman", "$boat_trading$");
		t.default_attack = "boat_snowball";
		clone("boat_trading_necromancer", "$boat_trading$");
		t.default_attack = "boat_necro_ball";
		clone("boat_trading_evil_mage", "$boat_trading$");
		t.default_attack = "boat_fireball";
		clone("boat_trading_druid", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_bee", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_beetle", "$boat_trading$");
		t.default_attack = "rocks";
		clone("boat_trading_seal", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_unicorn", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_ghost", "$boat_trading$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_trading_fairy", "$boat_trading$");
		t.default_attack = "boat_arrow";
		clone("boat_trading_demon", "$boat_trading$");
		t.default_attack = "boat_fireball";
		clone("boat_trading_cold_one", "$boat_trading$");
		t.default_attack = "boat_freeze_ball";
		clone("boat_trading_bandit", "$boat_trading$");
		t.default_attack = "boat_cannonball";
		clone("boat_trading_alien", "$boat_trading$");
		t.default_attack = "boat_plasma_ball";
		clone("boat_trading_greg", "$boat_trading$");
		t.default_attack = "boat_plasma_ball";
	}

	private void initCivsClassic()
	{
		clone("human", "$civ_advanced_unit$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("human_default_set", "human_slavic_set", "human_germanic_set", "human_rus_set", "human_posh_set", "human_folk_set", "human_pomeranian_set", "human_frankish_set", "human_rome_set", "human_iberian_set", "human_monolux_set");
		t.addPreferredColors("blue", "navy", "teal", "cyan");
		t.build_order_template_id = "build_order_advanced";
		t.music_theme = "Humans_Neutral";
		t.kingdom_id_wild = "nomads_human";
		t.kingdom_id_civilization = "human";
		t.banner_id = "human";
		t.architecture_id = "human";
		t.name_locale = "Human";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "primates";
		t.name_taxonomic_family = "hominidae";
		t.name_taxonomic_genus = "homo";
		t.name_taxonomic_species = "sapiens";
		t.icon = "iconHumans";
		t.color_hex = "#005E72";
		t.zombie_color_hex = "#00AD2C";
		t.disable_jump_animation = true;
		t.base_stats["mass_2"] = 65f;
		t.addGenome(("health", 100f), ("stamina", 100f), ("mutation", 1f), ("bonus_sex_random", 2f), ("bad", 2f), ("lifespan", 70f), ("damage", 15f), ("speed", 15f), ("offspring", 5f), ("diplomacy", 3f), ("warfare", 3f), ("stewardship", 3f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("polyphasic_sleep");
		t.addSubspeciesTrait("nocturnal_dormancy");
		t.addClanTrait("divine_dozen");
		t.addCultureTrait("city_layout_the_grand_arrangement");
		t.addCultureTrait("city_layout_stone_garden");
		t.addCultureTrait("roads");
		t.addCultureTrait("statue_lovers");
		t.addCultureTrait("pep_talks");
		t.addCultureTrait("youth_reverence");
		t.addCultureTrait("expansionists");
		t.addLanguageTrait("nicely_structured_grammar");
		t.addReligionTrait("bloodline_bond");
		t.addReligionTrait("rite_of_roaring_skies");
		t.addReligionTrait("cast_shield");
		t.production = new string[2] { "bread", "pie" };
		addPhenotype("skin_light");
		addPhenotype("skin_dark");
		addPhenotype("skin_mixed");
		clone("elf", "$civ_advanced_unit$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("elf_default_set");
		t.addPreferredColors("green", "lime", "lavender");
		t.kingdom_id_wild = "nomads_elf";
		t.kingdom_id_civilization = "elf";
		t.banner_id = "elf";
		t.architecture_id = "elf";
		t.build_order_template_id = "build_order_advanced";
		t.music_theme = "Elves_Neutral";
		t.name_locale = "Elf";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "primates";
		t.name_taxonomic_family = "hominidae";
		t.name_taxonomic_genus = "elvus";
		t.name_taxonomic_species = "elegance";
		t.collective_term = "group_quiver";
		t.icon = "iconElves";
		t.color_hex = "#005D00";
		t.zombie_color_hex = "#2C8D98";
		t.civ_base_cities = 3;
		t.family_limit = 20;
		t.base_stats["mass_2"] = 25f;
		t.addGenome(("health", 70f), ("bonus_sex_random", 1f), ("stamina", 200f), ("lifespan", 500f), ("mutation", 2f), ("damage", 10f), ("speed", 20f), ("offspring", 2f), ("diplomacy", 5f), ("warfare", 2f), ("stewardship", 2f), ("intelligence", 6f));
		t.addCultureTrait("bow_lovers");
		t.addCultureTrait("spear_lovers");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("youth_reverence");
		t.addCultureTrait("reading_lovers");
		t.addCultureTrait("attentive_readers");
		t.addCultureTrait("animal_whisperers");
		t.addCultureTrait("true_roots");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("unbroken_chain");
		t.addCultureTrait("city_layout_pillars");
		t.addClanTrait("blood_pact");
		t.addClanTrait("divine_dozen");
		t.addClanTrait("witchs_vein");
		t.addLanguageTrait("melodic");
		t.addLanguageTrait("magic_words");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("death_grow_tree");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_frugivore");
		t.addSubspeciesTrait("diet_granivore");
		t.addSubspeciesTrait("diet_florivore");
		t.addSubspeciesTrait("diet_folivore");
		t.addSubspeciesTrait("pure");
		t.addKingdomTrait("tax_rate_local_low");
		t.addKingdomTrait("tax_rate_tribute_low");
		t.addReligionTrait("rite_of_living_harvest");
		t.addReligionTrait("rite_of_entanglement");
		t.addReligionTrait("cast_grass_seeds");
		addTrait("weightless");
		addTrait("moonchild");
		addTrait("soft_skin");
		t.disable_jump_animation = true;
		t.production = new string[4] { "bread", "jam", "sushi", "cider" };
		addPhenotype("skin_light");
		addPhenotype("skin_mixed");
		addPhenotype("mid_gray", "biome_corrupted");
		addPhenotype("skin_purple", "biome_celestial");
		t.addResource("meat", 1, pNewList: true);
		t.addResource("bones", 1);
		clone("orc", "$civ_advanced_unit$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("orc_default_set");
		t.addPreferredColors("red", "orange", "brown", "maroon", "black");
		t.kingdom_id_wild = "nomads_orc";
		t.kingdom_id_civilization = "orc";
		t.banner_id = "orc";
		t.architecture_id = "orc";
		t.build_order_template_id = "build_order_advanced";
		t.music_theme = "Orcs_Neutral";
		t.family_limit = 50;
		t.name_locale = "Orc";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "primates";
		t.name_taxonomic_family = "hominidae";
		t.name_taxonomic_genus = "orcus";
		t.name_taxonomic_species = "bellicus";
		t.collective_term = "group_horde";
		t.base_stats["mass_2"] = 85f;
		t.icon = "iconOrcs";
		t.color_hex = "#2F5225";
		t.zombie_color_hex = "#7C5280";
		t.civ_base_cities = 4;
		t.addGenome(("health", 150f), ("bonus_sex_random", 1f), ("stamina", 130f), ("lifespan", 50f), ("mutation", 2f), ("damage", 20f), ("speed", 14f), ("offspring", 10f), ("diplomacy", 2f), ("warfare", 5f), ("birth_rate", 5f), ("stewardship", 3f), ("intelligence", 2f));
		addTrait("regeneration");
		addTrait("savage");
		addTrait("nightchild");
		addTrait("bloodlust");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("slow_builders");
		t.addSubspeciesTrait("polyphasic_sleep");
		t.addSubspeciesTrait("prolonged_rest");
		t.addSubspeciesTrait("aggressive");
		t.addClanTrait("warlocks_vein");
		t.addClanTrait("combat_instincts");
		t.addCultureTrait("buildings_spread");
		t.addCultureTrait("training_potential");
		t.addCultureTrait("fast_learners");
		t.addCultureTrait("happiness_from_war");
		t.addCultureTrait("dense_dwellings");
		t.addCultureTrait("tiny_legends");
		t.addCultureTrait("warriors_ascension");
		t.addCultureTrait("shattered_crown");
		t.addLanguageTrait("scribble");
		t.addLanguageTrait("raging_paragraphs");
		t.addLanguageTrait("confusing_semantics");
		t.addLanguageTrait("foolish_glyphs");
		t.addReligionTrait("rite_of_falling_stars");
		t.addReligionTrait("zeal_of_conquest");
		t.addReligionTrait("cast_fire");
		t.production = new string[3] { "bread", "burger", "tea" };
		t.disable_jump_animation = true;
		addPhenotype("skin_green");
		addPhenotype("skin_pale", "biome_permafrost");
		addPhenotype("mid_gray", "biome_corrupted");
		addPhenotype("skin_red", "biome_infernal");
		t.addResource("meat", 1, pNewList: true);
		t.addResource("bones", 2);
		clone("dwarf", "$civ_advanced_unit$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("dwarf_default_set", "dwarf_nordic_set");
		t.addPreferredColors("yellow", "orange", "brown");
		t.kingdom_id_wild = "nomads_dwarf";
		t.kingdom_id_civilization = "dwarf";
		t.banner_id = "dwarf";
		t.architecture_id = "dwarf";
		t.build_order_template_id = "build_order_advanced";
		t.music_theme = "Dwarves_Neutral";
		t.name_locale = "Dwarf";
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "chordata";
		t.name_taxonomic_class = "mammalia";
		t.name_taxonomic_order = "primates";
		t.name_taxonomic_family = "hominidae";
		t.name_taxonomic_genus = "dworfus";
		t.name_taxonomic_species = "fortis";
		t.collective_term = "group_beard";
		t.base_stats["mass_2"] = 75f;
		t.family_limit = 30;
		t.item_making_skill = 3;
		t.icon = "iconDwarf";
		t.color_hex = "#828282";
		t.zombie_color_hex = "#7C5280";
		t.civ_base_cities = 3;
		t.addGenome(("health", 150f), ("bonus_sex_random", 1f), ("stamina", 40f), ("lifespan", 220f), ("mutation", 2f), ("damage", 18f), ("speed", 12f), ("offspring", 3f), ("diplomacy", 2f), ("warfare", 3f), ("stewardship", 5f), ("intelligence", 2f));
		addTrait("miner");
		addTrait("deflect_projectile");
		addTrait("block");
		addTrait("fat");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("fast_builders");
		t.addSubspeciesTrait("monophasic_sleep");
		t.addCultureTrait("roads");
		t.addCultureTrait("city_layout_the_grand_arrangement");
		t.addCultureTrait("hammer_lovers");
		t.addCultureTrait("tower_lovers");
		t.addCultureTrait("conscription_male_only");
		t.addCultureTrait("elder_reverence");
		t.addCultureTrait("gossip_lovers");
		t.addCultureTrait("weaponsmith_mastery");
		t.addCultureTrait("armorsmith_mastery");
		t.addLanguageTrait("powerful_words");
		t.addLanguageTrait("ancient_runes");
		t.addClanTrait("divine_dozen");
		t.addClanTrait("iron_will");
		t.addReligionTrait("rite_of_unbroken_shield");
		t.addReligionTrait("rite_of_shattered_earth");
		t.production = new string[2] { "bread", "ale" };
		t.disable_jump_animation = true;
		addPhenotype("skin_light");
		addPhenotype("skin_medium");
		addPhenotype("mid_gray");
		t.addResource("meat", 2, pNewList: true);
		t.addResource("bones", 1);
		t.addResource("stone", 1);
	}

	private void initCivsNew()
	{
		clone("civ_cat", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("cat_set");
		t.base_stats["mass_2"] = 35f;
		t.addGenome(("health", 80f), ("stamina", 150f), ("lifespan", 100f), ("mutation", 2f), ("damage", 20f), ("speed", 17f), ("offspring", 5f), ("diplomacy", 4f), ("warfare", 4f), ("stewardship", 2f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("polyphasic_sleep");
		t.addSubspeciesTrait("inquisitive_nature");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("true_roots");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("sword_lovers");
		t.addClanTrait("silver_tongues");
		t.addClanTrait("best_five");
		t.addClanTrait("endurance_of_titans");
		t.addClanTrait("deathbound");
		t.addReligionTrait("path_of_unity");
		t.addReligionTrait("cast_silence");
		t.addReligionTrait("rite_of_eternal_brew");
		t.addReligionTrait("summon_lightning");
		t.addReligionTrait("rite_of_dissent");
		t.addLanguageTrait("melodic");
		addTrait("dodge");
		addTrait("battle_reflexes");
		t.kingdom_id_civilization = "civ_cat";
		t.architecture_id = "civ_cat";
		t.banner_id = "civ_cat";
		t.cloneTaxonomyFromForSapiens("cat");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("cat");
		t.color_hex = "#005E72";
		clone("civ_dog", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("wolf_set");
		t.base_stats["mass_2"] = 70f;
		t.addGenome(("health", 120f), ("stamina", 130f), ("lifespan", 80f), ("mutation", 2f), ("damage", 20f), ("speed", 14f), ("offspring", 5f), ("diplomacy", 4f), ("warfare", 4f), ("stewardship", 2f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("super_positivity");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("polyphasic_sleep");
		t.addCultureTrait("xenophiles");
		t.addCultureTrait("dense_dwellings");
		t.addCultureTrait("expansionists");
		t.addCultureTrait("training_potential");
		t.addCultureTrait("patriarchy");
		t.addClanTrait("we_are_legion");
		t.addClanTrait("stonefists");
		t.addClanTrait("blood_of_giants");
		t.addClanTrait("gaia_shield");
		t.addClanTrait("blood_pact");
		t.addCultureTrait("join_or_die");
		t.addReligionTrait("cast_shield");
		t.addReligionTrait("path_of_unity");
		t.addReligionTrait("hand_of_order");
		t.addReligionTrait("rite_of_roaring_skies");
		t.addLanguageTrait("melodic");
		t.addLanguageTrait("scribble");
		t.addTrait("dash");
		t.kingdom_id_civilization = "civ_dog";
		t.architecture_id = "civ_dog";
		t.banner_id = "civ_dog";
		t.cloneTaxonomyFromForSapiens("dog");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("dog");
		t.color_hex = "#005E72";
		clone("civ_chicken", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("chicken_set");
		t.base_stats["mass_2"] = 35f;
		t.addGenome(("health", 60f), ("stamina", 30f), ("lifespan", 80f), ("mutation", 3f), ("damage", 10f), ("speed", 7f), ("offspring", 12f), ("diplomacy", 4f), ("warfare", 2f), ("stewardship", 4f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("metamorphosis_chicken");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addSubspeciesTrait("monophasic_sleep");
		t.addSubspeciesTrait("cautious_instincts");
		t.addLanguageTrait("strict_spelling");
		t.addReligionTrait("rite_of_living_harvest");
		t.addReligionTrait("cast_silence");
		t.addReligionTrait("summon_lightning");
		t.addReligionTrait("rite_of_change");
		t.addReligionTrait("path_of_unity");
		t.kingdom_id_civilization = "civ_chicken";
		t.render_heads_for_babies = false;
		t.architecture_id = "civ_chicken";
		t.banner_id = "civ_chicken";
		t.cloneTaxonomyFromForSapiens("chicken");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("chicken");
		t.color_hex = "#005E72";
		t.addResource("meat", 1);
		t.addResource("bones", 1);
		clone("civ_rabbit", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("rabbit_set");
		t.base_stats["mass_2"] = 30f;
		t.addGenome(("health", 60f), ("stamina", 150f), ("lifespan", 90f), ("mutation", 3f), ("damage", 10f), ("speed", 12f), ("offspring", 12f), ("diplomacy", 4f), ("warfare", 1f), ("stewardship", 4f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_short");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addSubspeciesTrait("polyphasic_sleep");
		t.addSubspeciesTrait("cautious_instincts");
		t.addClanTrait("we_are_legion");
		t.addCultureTrait("dense_dwellings");
		t.addLanguageTrait("beautiful_calligraphy");
		t.addReligionTrait("cast_grass_seeds");
		t.addReligionTrait("rite_of_eternal_brew");
		t.addReligionTrait("summon_tornado");
		t.addReligionTrait("path_of_unity");
		t.addReligionTrait("spawn_vegetation");
		t.kingdom_id_civilization = "civ_rabbit";
		t.architecture_id = "civ_rabbit";
		t.banner_id = "civ_rabbit";
		t.cloneTaxonomyFromForSapiens("rabbit");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("rabbit");
		t.color_hex = "#005E72";
		clone("civ_monkey", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("monkey_set");
		t.base_stats["mass_2"] = 65f;
		t.addGenome(("health", 90f), ("stamina", 140f), ("lifespan", 80f), ("mutation", 2f), ("damage", 20f), ("speed", 14f), ("offspring", 8f), ("diplomacy", 4f), ("warfare", 3f), ("stewardship", 2f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("good_throwers");
		t.addSubspeciesTrait("super_positivity");
		t.addSubspeciesTrait("polyphasic_sleep");
		t.addSubspeciesTrait("shiny_love");
		t.addCultureTrait("patriarchy");
		t.addCultureTrait("training_potential");
		t.addCultureTrait("fast_learners");
		t.addCultureTrait("buildings_spread");
		t.addCultureTrait("expertise_exchange");
		t.addCultureTrait("fames_crown");
		t.addClanTrait("we_are_legion");
		t.addClanTrait("silver_tongues");
		t.addClanTrait("bonebreakers");
		t.addClanTrait("combat_instincts");
		t.addReligionTrait("path_of_unity");
		t.addReligionTrait("rite_of_dissent");
		t.addReligionTrait("summon_tornado");
		t.addReligionTrait("minds_awakening");
		t.addReligionTrait("cast_curse");
		t.addLanguageTrait("scribble");
		t.kingdom_id_civilization = "civ_monkey";
		t.architecture_id = "civ_monkey";
		t.banner_id = "civ_monkey";
		t.cloneTaxonomyFromForSapiens("monkey");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("monkey");
		t.color_hex = "#005E72";
		clone("civ_fox", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("fox_set");
		t.base_stats["mass_2"] = 60f;
		t.addGenome(("health", 80f), ("stamina", 120f), ("lifespan", 120f), ("mutation", 2f), ("damage", 20f), ("speed", 12f), ("offspring", 4f), ("diplomacy", 4f), ("warfare", 4f), ("stewardship", 3f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("monophasic_sleep");
		t.addSubspeciesTrait("nimble");
		t.addLanguageTrait("elegant_words");
		t.addLanguageTrait("stylish_writing");
		t.addReligionTrait("cast_curse");
		t.addReligionTrait("rite_of_dissent");
		t.addReligionTrait("summon_lightning");
		t.addReligionTrait("path_of_unity");
		t.addReligionTrait("path_of_unity");
		t.addCultureTrait("reading_lovers");
		t.addCultureTrait("fames_crown");
		t.kingdom_id_civilization = "civ_fox";
		t.architecture_id = "civ_fox";
		t.banner_id = "civ_fox";
		t.cloneTaxonomyFromForSapiens("fox");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("fox");
		t.color_hex = "#005E72";
		clone("civ_sheep", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("sheep_set");
		t.base_stats["mass_2"] = 80f;
		t.addGenome(("health", 90f), ("stamina", 30f), ("lifespan", 100f), ("mutation", 2f), ("damage", 10f), ("speed", 12f), ("offspring", 2f), ("diplomacy", 4f), ("warfare", 1f), ("stewardship", 4f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("monophasic_sleep");
		t.addSubspeciesTrait("cautious_instincts");
		t.addLanguageTrait("nicely_structured_grammar");
		t.addLanguageTrait("melodic");
		t.addKingdomTrait("tax_rate_tribute_high");
		t.addKingdomTrait("tax_rate_local_high");
		t.addCultureTrait("golden_rule");
		t.addCultureTrait("city_layout_diamond");
		t.addReligionTrait("cast_grass_seeds");
		t.kingdom_id_civilization = "civ_sheep";
		t.architecture_id = "civ_sheep";
		t.banner_id = "civ_sheep";
		t.cloneTaxonomyFromForSapiens("sheep");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("sheep");
		t.color_hex = "#005E72";
		t.addTrait("greedy");
		t.addResource("meat", 2, pNewList: true);
		t.addResource("leather", 1);
		t.addResource("bones", 1);
		clone("civ_cow", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("cow_set");
		t.base_stats["mass_2"] = 175f;
		t.addGenome(("health", 120f), ("stamina", 20f), ("lifespan", 100f), ("mutation", 2f), ("damage", 10f), ("speed", 11f), ("offspring", 2f), ("diplomacy", 4f), ("warfare", 1f), ("stewardship", 4f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addSubspeciesTrait("monophasic_sleep");
		t.addSubspeciesTrait("prolonged_rest");
		t.addLanguageTrait("melodic");
		t.addLanguageTrait("stylish_writing");
		t.addCultureTrait("dense_dwellings");
		t.addTrait("tough");
		t.render_heads_for_babies = false;
		t.kingdom_id_civilization = "civ_cow";
		t.architecture_id = "civ_cow";
		t.banner_id = "civ_cow";
		t.cloneTaxonomyFromForSapiens("cow");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("cow");
		t.color_hex = "#005E72";
		t.addResource("meat", 2);
		clone("civ_armadillo", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("armadillo_set");
		t.base_stats["mass_2"] = 45f;
		t.addGenome(("health", 200f), ("stamina", 50f), ("lifespan", 100f), ("mutation", 1f), ("damage", 20f), ("armor", 20f), ("speed", 17f), ("offspring", 2f), ("diplomacy", 2f), ("warfare", 4f), ("stewardship", 4f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addCultureTrait("armorsmith_mastery");
		t.addLanguageTrait("powerful_words");
		t.kingdom_id_civilization = "civ_armadillo";
		t.architecture_id = "civ_armadillo";
		t.banner_id = "civ_armadillo";
		t.cloneTaxonomyFromForSapiens("armadillo");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("armadillo");
		t.color_hex = "#005E72";
		t.addResource("meat", 1);
		t.addResource("bones", 2);
		clone("civ_wolf", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("wolf_set");
		t.base_stats["mass_2"] = 85f;
		t.addGenome(("health", 130f), ("stamina", 140f), ("lifespan", 80f), ("mutation", 1f), ("damage", 18f), ("speed", 20f), ("offspring", 4f), ("diplomacy", 3f), ("warfare", 4f), ("stewardship", 2f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_short");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("metamorphosis_wolf");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("diet_folivore");
		t.addCultureTrait("city_layout_claws");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("ethnocentric_guard");
		t.addCultureTrait("happiness_from_war");
		t.addClanTrait("blood_pact");
		t.addClanTrait("iron_will");
		t.addClanTrait("combat_instincts");
		t.addLanguageTrait("powerful_words");
		t.addLanguageTrait("scribble");
		t.addReligionTrait("rite_of_restless_dead");
		t.kingdom_id_civilization = "civ_wolf";
		t.architecture_id = "civ_wolf";
		t.banner_id = "civ_wolf";
		t.cloneTaxonomyFromForSapiens("wolf");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("wolf");
		t.color_hex = "#005E72";
		t.addResource("bones", 1);
		t.addResource("leather", 1);
		clone("civ_bear", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("bear_set");
		t.base_stats["mass_2"] = 140f;
		t.addGenome(("health", 180f), ("stamina", 150f), ("lifespan", 80f), ("mutation", 3f), ("damage", 30f), ("armor", 5f), ("speed", 14f), ("offspring", 2f), ("diplomacy", 2f), ("warfare", 4f), ("stewardship", 3f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("big_stomach");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("winter_slumberers");
		t.addSubspeciesTrait("energy_preserver");
		t.addSubspeciesTrait("aggressive");
		t.addCultureTrait("city_layout_claws");
		t.addCultureTrait("patriarchy");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("ethnocentric_guard");
		t.addCultureTrait("conscription_male_only");
		t.addCultureTrait("axe_lovers");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("training_potential");
		t.addLanguageTrait("foolish_glyphs");
		t.addClanTrait("blood_of_giants");
		t.addClanTrait("iron_will");
		t.addReligionTrait("rite_of_shattered_earth");
		t.kingdom_id_civilization = "civ_bear";
		t.render_heads_for_babies = false;
		t.architecture_id = "civ_bear";
		t.banner_id = "civ_bear";
		t.cloneTaxonomyFromForSapiens("bear");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("bear");
		t.color_hex = "#005E72";
		t.addResource("meat", 1);
		clone("civ_rhino", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("rhino_set");
		t.base_stats["mass_2"] = 295f;
		t.addGenome(("health", 230f), ("stamina", 110f), ("lifespan", 80f), ("mutation", 1f), ("damage", 35f), ("armor", 15f), ("speed", 16f), ("offspring", 2f), ("diplomacy", 1f), ("warfare", 6f), ("stewardship", 2f), ("intelligence", 2f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("bioproduct_stone");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("aggressive");
		t.addClanTrait("bonebreakers");
		t.addClanTrait("blood_of_giants");
		t.addClanTrait("iron_will");
		t.addClanTrait("void_ban");
		t.addCultureTrait("patriarchy");
		t.addCultureTrait("axe_lovers");
		t.addCultureTrait("city_layout_bricks");
		t.addLanguageTrait("raging_paragraphs");
		t.addLanguageTrait("powerful_words");
		t.addReligionTrait("zeal_of_conquest");
		t.addTrait("dash");
		t.addTrait("block");
		t.addTrait("hard_skin");
		t.addTrait("tough");
		t.addTrait("strong");
		t.render_heads_for_babies = false;
		t.kingdom_id_civilization = "civ_rhino";
		t.architecture_id = "civ_rhino";
		t.banner_id = "civ_rhino";
		t.cloneTaxonomyFromForSapiens("rhino");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("rhino");
		t.color_hex = "#005E72";
		t.addResource("meat", 2);
		clone("civ_buffalo", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("buffalo_set");
		t.base_stats["mass_2"] = 290f;
		t.addGenome(("health", 160f), ("stamina", 120f), ("lifespan", 80f), ("mutation", 1f), ("damage", 18f), ("speed", 16f), ("offspring", 2f), ("diplomacy", 3f), ("warfare", 4f), ("stewardship", 3f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("city_layout_monolith_mesh");
		t.addCultureTrait("dense_dwellings");
		t.addLanguageTrait("raging_paragraphs");
		t.addLanguageTrait("powerful_words");
		t.addClanTrait("stonefists");
		t.addReligionTrait("rite_of_roaring_skies");
		t.addTrait("tough");
		t.addTrait("dash");
		t.render_heads_for_babies = false;
		t.kingdom_id_civilization = "civ_buffalo";
		t.architecture_id = "civ_buffalo";
		t.banner_id = "civ_buffalo";
		t.cloneTaxonomyFromForSapiens("buffalo");
		t.name_taxonomic_genus = "jecodespecus";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("buffalo");
		t.color_hex = "#005E72";
		t.addResource("meat", 2);
		t.addResource("bones", 1);
		clone("civ_hyena", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("hyena_set");
		t.base_stats["mass_2"] = 60f;
		t.addGenome(("health", 130f), ("stamina", 140f), ("lifespan", 80f), ("mutation", 1f), ("damage", 18f), ("armor", 5f), ("speed", 20f), ("offspring", 6f), ("diplomacy", 3f), ("warfare", 4f), ("stewardship", 3f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("super_positivity");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addClanTrait("combat_instincts");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("conscription_female_only");
		t.addCultureTrait("axe_lovers");
		t.addCultureTrait("city_layout_tile_wobbly_pattern");
		t.addLanguageTrait("raging_paragraphs");
		t.addLanguageTrait("confusing_semantics");
		t.addReligionTrait("rite_of_dissent");
		t.kingdom_id_civilization = "civ_hyena";
		t.architecture_id = "civ_hyena";
		t.banner_id = "civ_hyena";
		t.cloneTaxonomyFromForSapiens("hyena");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("hyena");
		t.color_hex = "#005E72";
		clone("civ_rat", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("rat_set");
		t.base_stats["mass_2"] = 20f;
		t.addGenome(("health", 80f), ("stamina", 50f), ("lifespan", 80f), ("mutation", 4f), ("damage", 12f), ("speed", 16f), ("armor", 5f), ("offspring", 15f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 3f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_short");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("nimble");
		t.addClanTrait("we_are_legion");
		t.addCultureTrait("ethnocentric_guard");
		t.addCultureTrait("hive_society");
		t.addCultureTrait("expansionists");
		t.addLanguageTrait("scribble");
		t.kingdom_id_civilization = "civ_rat";
		t.render_heads_for_babies = false;
		t.architecture_id = "civ_rat";
		t.banner_id = "civ_rat";
		t.cloneTaxonomyFromForSapiens("rat");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("rat");
		t.color_hex = "#005E72";
		clone("civ_alpaca", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("alpaca_set");
		t.base_stats["mass_2"] = 70f;
		t.addGenome(("health", 110f), ("stamina", 50f), ("lifespan", 150f), ("mutation", 1f), ("damage", 18f), ("speed", 12f), ("offspring", 3f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 5f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addCultureTrait("xenophiles");
		t.addCultureTrait("diplomatic_ascension");
		t.addLanguageTrait("elegant_words");
		t.addTrait("soft_skin");
		t.addReligionTrait("path_of_unity");
		t.kingdom_id_civilization = "civ_alpaca";
		t.architecture_id = "civ_alpaca";
		t.banner_id = "civ_alpaca";
		t.cloneTaxonomyFromForSapiens("alpaca");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("alpaca");
		t.color_hex = "#005E72";
		t.addResource("meat", 2);
		t.addResource("leather", 1);
		clone("civ_capybara", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("capybara_set");
		t.base_stats["mass_2"] = 70f;
		t.addGenome(("health", 130f), ("stamina", 30f), ("lifespan", 90f), ("mutation", 1f), ("damage", 18f), ("speed", 10f), ("offspring", 4f), ("diplomacy", 4f), ("warfare", 2f), ("stewardship", 5f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("telepathic_link");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addSubspeciesTrait("slow_builders");
		t.addCultureTrait("serenity_now");
		t.addCultureTrait("xenophiles");
		t.addLanguageTrait("melodic");
		t.kingdom_id_civilization = "civ_capybara";
		t.architecture_id = "civ_capybara";
		t.banner_id = "civ_capybara";
		t.cloneTaxonomyFromForSapiens("capybara");
		t.name_taxonomic_genus = "mastefus";
		t.name_taxonomic_species = "yourmomus";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("capybara");
		t.color_hex = "#005E72";
		t.addTrait("peaceful");
		t.addTrait("content");
		t.addTrait("arcane_reflexes");
		clone("civ_goat", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("goat_set");
		t.base_stats["mass_2"] = 50f;
		t.addGenome(("health", 110f), ("stamina", 100f), ("lifespan", 80f), ("mutation", 1f), ("damage", 15f), ("speed", 16f), ("offspring", 3f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 5f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_herbivore");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("reading_lovers");
		t.addCultureTrait("expertise_exchange");
		t.addCultureTrait("ancestors_knowledge");
		t.addLanguageTrait("enlightening_script");
		t.render_heads_for_babies = false;
		t.kingdom_id_civilization = "civ_goat";
		t.architecture_id = "civ_goat";
		t.banner_id = "civ_goat";
		t.cloneTaxonomyFromForSapiens("goat");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("goat");
		t.color_hex = "#005E72";
		clone("civ_scorpion", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("scorpion_set");
		t.base_stats["mass_2"] = 25f;
		t.addGenome(("health", 130f), ("stamina", 30f), ("lifespan", 150f), ("mutation", 3f), ("damage", 30f), ("speed", 11f), ("armor", 25f), ("offspring", 25f), ("diplomacy", 1f), ("warfare", 5f), ("stewardship", 2f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_very_long");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("adaptation_desert");
		t.addClanTrait("combat_instincts");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("patriarchy");
		t.addCultureTrait("conscription_female_only");
		t.addCultureTrait("solitude_seekers");
		t.addLanguageTrait("elegant_words");
		t.addReligionTrait("sands_of_ruin");
		t.addReligionTrait("cast_fire");
		addTrait("venomous");
		addTrait("poison_immune");
		t.kingdom_id_civilization = "civ_scorpion";
		t.architecture_id = "civ_scorpion";
		t.banner_id = "civ_scorpion";
		t.cloneTaxonomyFromForSapiens("scorpion");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("scorpion");
		t.color_hex = "#005E72";
		t.addResource("meat", 2, pNewList: true);
		t.addResource("bones", 1);
		clone("civ_crab", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crab_set");
		t.base_stats["mass_2"] = 30f;
		t.addGenome(("health", 130f), ("stamina", 20f), ("lifespan", 80f), ("mutation", 1f), ("damage", 18f), ("speed", 12f), ("armor", 25f), ("offspring", 30f), ("diplomacy", 2f), ("warfare", 4f), ("stewardship", 3f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_roe");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("metamorphosis_crab");
		t.addSubspeciesTrait("exoskeleton");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("diet_algivore");
		t.addSubspeciesTrait("fins");
		t.addCultureTrait("ethnocentric_guard");
		t.addCultureTrait("city_layout_pebbles");
		t.addLanguageTrait("powerful_words");
		t.addCultureTrait("dense_dwellings");
		t.addClanTrait("blood_of_sea");
		t.addTrait("hard_skin");
		t.kingdom_id_civilization = "civ_crab";
		t.architecture_id = "civ_crab";
		t.banner_id = "civ_crab";
		t.cloneTaxonomyFromForSapiens("crab");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("crab");
		t.color_hex = "#005E72";
		t.addResource("bones", 2);
		clone("civ_penguin", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("penguin_set");
		t.base_stats["mass_2"] = 35f;
		t.addGenome(("health", 110f), ("stamina", 100f), ("lifespan", 80f), ("mutation", 1f), ("damage", 12f), ("speed", 10f), ("offspring", 2f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 3f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("adaptation_permafrost");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("xenophiles");
		t.addClanTrait("blood_of_sea");
		t.addLanguageTrait("beautiful_calligraphy");
		t.addReligionTrait("rite_of_tempest_call");
		addTrait("freeze_proof");
		t.kingdom_id_civilization = "civ_penguin";
		t.architecture_id = "civ_penguin";
		t.banner_id = "civ_penguin";
		t.cloneTaxonomyFromForSapiens("penguin");
		t.name_taxonomic_genus = "hugovazus";
		t.name_taxonomic_species = "pingus";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("penguin");
		t.color_hex = "#005E72";
		clone("civ_turtle", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("turtle_set");
		t.base_stats["mass_2"] = 110f;
		t.addGenome(("health", 180f), ("stamina", 50f), ("lifespan", 500f), ("mutation", 1f), ("damage", 12f), ("speed", 8f), ("armor", 25f), ("offspring", 20f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 5f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_colored");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addSubspeciesTrait("cautious_instincts");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("sword_lovers");
		t.addCultureTrait("solitude_seekers");
		t.addLanguageTrait("enlightening_script");
		t.addLanguageTrait("strict_spelling");
		t.addLanguageTrait("eternal_text");
		t.addCultureTrait("dense_dwellings");
		t.addClanTrait("blood_of_sea");
		t.kingdom_id_civilization = "civ_turtle";
		t.architecture_id = "civ_turtle";
		t.banner_id = "civ_turtle";
		t.cloneTaxonomyFromForSapiens("turtle");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("turtle");
		t.color_hex = "#005E72";
		t.addResource("bones", 1);
		clone("civ_crocodile", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crocodile_set");
		t.base_stats["mass_2"] = 200f;
		t.addGenome(("health", 130f), ("stamina", 60f), ("lifespan", 90f), ("mutation", 1f), ("damage", 20f), ("speed", 10f), ("armor", 10f), ("offspring", 10f), ("diplomacy", 2f), ("warfare", 4f), ("stewardship", 2f), ("intelligence", 3f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addClanTrait("masters_of_propaganda");
		t.addClanTrait("blood_of_sea");
		t.addClanTrait("combat_instincts");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("patriarchy");
		t.addCultureTrait("warriors_ascension");
		t.addCultureTrait("city_layout_parallels");
		t.addLanguageTrait("scribble");
		t.addReligionTrait("cast_silence");
		t.kingdom_id_civilization = "civ_crocodile";
		t.render_heads_for_babies = false;
		t.architecture_id = "civ_crocodile";
		t.banner_id = "civ_crocodile";
		t.cloneTaxonomyFromForSapiens("crocodile");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("crocodile");
		t.color_hex = "#005E72";
		t.addResource("meat", 1);
		t.addResource("leather", 1);
		clone("civ_snake", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("snake_set");
		t.base_stats["mass_2"] = 15f;
		t.addGenome(("health", 80f), ("stamina", 40f), ("lifespan", 150f), ("mutation", 1f), ("damage", 18f), ("speed", 10f), ("armor", 5f), ("offspring", 10f), ("diplomacy", 3f), ("warfare", 4f), ("stewardship", 2f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_shell_plain");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addSubspeciesTrait("circadian_drift");
		t.addClanTrait("silver_tongues");
		t.addClanTrait("blood_of_sea");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("ethnocentric_guard");
		t.addCultureTrait("spear_lovers");
		t.addCultureTrait("solitude_seekers");
		t.addLanguageTrait("strict_spelling");
		t.addReligionTrait("rite_of_dissent");
		t.addReligionTrait("cast_curse");
		t.addReligionTrait("cast_silence");
		addTrait("venomous");
		t.kingdom_id_civilization = "civ_snake";
		t.architecture_id = "civ_snake";
		t.banner_id = "civ_snake";
		t.cloneTaxonomyFromForSapiens("snake");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("snake");
		t.color_hex = "#005E72";
		t.addResource("meat", 2, pNewList: true);
		t.addResource("leather", 2);
		clone("civ_frog", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("frog_set");
		t.base_stats["mass_2"] = 15f;
		t.addGenome(("health", 80f), ("stamina", 50f), ("lifespan", 90f), ("mutation", 3f), ("damage", 12f), ("speed", 7f), ("offspring", 15f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 3f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_bubble");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("dreamweavers");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addLanguageTrait("melodic");
		t.addCultureTrait("dense_dwellings");
		t.addReligionTrait("cast_blood_rain");
		t.addClanTrait("masters_of_propaganda");
		t.addClanTrait("blood_of_sea");
		t.render_heads_for_babies = false;
		t.kingdom_id_civilization = "civ_frog";
		t.architecture_id = "civ_frog";
		t.banner_id = "civ_frog";
		t.cloneTaxonomyFromForSapiens("frog");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("frog");
		t.color_hex = "#005E72";
		t.addResource("meat", 1, pNewList: true);
		t.addResource("leather", 1);
		clone("civ_piranha", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("piranha_set");
		t.base_stats["mass_2"] = 19f;
		t.addGenome(("health", 40f), ("stamina", 50f), ("lifespan", 30f), ("mutation", 1f), ("damage", 30f), ("speed", 7f), ("armor", 5f), ("offspring", 20f), ("diplomacy", 1f), ("warfare", 6f), ("stewardship", 1f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_roe");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("aggressive");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_carnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("ethnocentric_guard");
		t.addLanguageTrait("scribble");
		t.addClanTrait("we_are_legion");
		t.addClanTrait("combat_instincts");
		t.addClanTrait("blood_of_sea");
		t.addReligionTrait("cast_blood_rain");
		t.addTrait("battle_reflexes");
		t.kingdom_id_civilization = "civ_piranha";
		t.architecture_id = "civ_piranha";
		t.banner_id = "civ_piranha";
		t.cloneTaxonomyFromForSapiens("piranha");
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("piranha");
		t.color_hex = "#005E72";
		t.addResource("sushi", 2, pNewList: true);
		clone("civ_liliar", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("flower_set");
		t.base_stats["mass_2"] = 90f;
		t.addGenome(("health", 40f), ("stamina", 100f), ("lifespan", 400f), ("mutation", 2f), ("damage", 5f), ("speed", 10f), ("offspring", 10f), ("diplomacy", 6f), ("warfare", 1f), ("stewardship", 6f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_vegetative");
		t.addSubspeciesTrait("death_grow_plant");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("gaia_roots");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("genetic_mirror");
		t.addCultureTrait("xenophiles");
		t.addCultureTrait("conscription_female_only");
		t.addClanTrait("gaia_blood");
		t.addClanTrait("gaia_shield");
		t.addClanTrait("flesh_weavers");
		t.addLanguageTrait("melodic");
		t.addLanguageTrait("nicely_structured_grammar");
		t.addLanguageTrait("beautiful_calligraphy");
		t.addTrait("regeneration");
		t.kingdom_id_civilization = "civ_liliar";
		t.architecture_id = "civ_liliar";
		t.banner_id = "civ_liliar";
		t.cloneTaxonomyFromForSapiens("flower_bud");
		t.name_taxonomic_genus = "luulia";
		t.name_taxonomic_species = "jubkoza";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("flower_bud");
		t.color_hex = "#005E72";
		t.addResource("herbs", 2, pNewList: true);
		clone("civ_garlic_man", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("garlic_man_set");
		t.base_stats["mass_2"] = 30f;
		t.addGenome(("health", 80f), ("stamina", 100f), ("lifespan", 250f), ("mutation", 2f), ("damage", 12f), ("speed", 20f), ("armor", 5f), ("offspring", 9f), ("diplomacy", 3f), ("warfare", 2f), ("stewardship", 5f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_vegetative");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("genetic_mirror");
		t.addCultureTrait("legacy_keepers");
		t.addLanguageTrait("confusing_semantics");
		t.addClanTrait("flesh_weavers");
		t.addCultureTrait("hive_society");
		t.addReligionTrait("cast_cure");
		t.addTrait("poison_immune");
		t.addTrait("regeneration");
		t.render_heads_for_babies = false;
		t.kingdom_id_civilization = "civ_garlic_man";
		t.architecture_id = "civ_garlic_man";
		t.banner_id = "civ_garlic_man";
		t.cloneTaxonomyFromForSapiens("garl");
		t.name_taxonomic_species = "lumneskatus";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("garl");
		t.color_hex = "#005E72";
		t.addResource("herbs", 2, pNewList: true);
		clone("civ_lemon_man", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("lemon_man_set");
		t.base_stats["mass_2"] = 45f;
		t.addGenome(("health", 80f), ("stamina", 100f), ("lifespan", 150f), ("mutation", 4f), ("damage", 12f), ("speed", 14f), ("offspring", 5f), ("diplomacy", 6f), ("warfare", 2f), ("stewardship", 5f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_hermaphroditic");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("population_large");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addCultureTrait("hive_society");
		t.addCultureTrait("expertise_exchange");
		t.addLanguageTrait("confusing_semantics");
		t.addLanguageTrait("doomed_glyphs");
		t.addReligionTrait("cast_cure");
		t.kingdom_id_civilization = "civ_lemon_man";
		t.architecture_id = "civ_lemon_man";
		t.banner_id = "civ_lemon_man";
		t.cloneTaxonomyFromForSapiens("lemon_snail");
		t.name_taxonomic_genus = "soursapiens";
		t.name_taxonomic_species = "misbehavius";
		t.icon = "civs/" + t.id;
		t.name_locale = "civ_lemon_man";
		t.clonePhenotype("lemon_snail");
		t.color_hex = "#005E72";
		addTrait("poison_immune");
		addTrait("paranoid");
		addTrait("attractive");
		addTrait("lucky");
		t.addTrait("regeneration");
		t.addResource("lemons", 3, pNewList: true);
		clone("civ_acid_gentleman", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("acid_blob_set");
		t.base_stats["mass_2"] = 99f;
		t.addGenome(("health", 80f), ("stamina", 30f), ("lifespan", 150f), ("mutation", 10f), ("damage", 18f), ("speed", 9f), ("armor", 5f), ("offspring", 10f), ("diplomacy", 1f), ("warfare", 4f), ("stewardship", 2f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_blob");
		t.addSubspeciesTrait("reproduction_fission");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("unstable_genome");
		t.addSubspeciesTrait("adaptation_wasteland");
		t.addCultureTrait("pep_talks");
		t.addCultureTrait("city_layout_madman_labyrinth");
		t.addLanguageTrait("elegant_words");
		t.kingdom_id_civilization = "civ_acid_gentleman";
		t.architecture_id = "civ_acid_gentleman";
		t.render_heads_for_babies = false;
		t.banner_id = "civ_acid_gentleman";
		t.cloneTaxonomyFromForSapiens("acid_blob");
		t.name_taxonomic_genus = "gentlemanus";
		t.name_taxonomic_species = "jumpus";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.clonePhenotype("acid_blob");
		t.color_hex = "#005E72";
		t.clonePhenotype("acid_blob");
		t.prevent_unconscious_rotation = true;
		addTrait("acid_blood");
		addTrait("acid_proof");
		addTrait("acid_touch");
		addTrait("poison_immune");
		addTrait("paranoid");
		addTrait("attractive");
		t.addResource("jam", 1, pNewList: true);
		clone("civ_crystal_golem", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("crystal_golem_set");
		t.base_stats["mass_2"] = 455f;
		t.addGenome(("health", 250f), ("stamina", 60f), ("lifespan", 1000f), ("mutation", 1f), ("damage", 30f), ("speed", 12f), ("armor", 30f), ("offspring", 2f), ("diplomacy", 1f), ("warfare", 6f), ("stewardship", 2f), ("intelligence", 4f));
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_crystal");
		t.addSubspeciesTrait("reproduction_hermaphroditic");
		t.addSubspeciesTrait("gestation_extremely_long");
		t.addSubspeciesTrait("bioluminescence");
		t.addSubspeciesTrait("heat_resistance");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("metamorphosis_sword");
		t.addSubspeciesTrait("bioproduct_gems");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_lithotroph");
		t.addSubspeciesTrait("slow_builders");
		t.addSubspeciesTrait("bioproduct_gems");
		t.addClanTrait("endurance_of_titans");
		t.addClanTrait("best_five");
		t.addClanTrait("blood_of_giants");
		t.addClanTrait("iron_will");
		t.addCultureTrait("fames_crown");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("sword_lovers");
		t.addCultureTrait("city_layout_titan_footprints");
		t.addLanguageTrait("words_of_madness");
		t.addClanTrait("blood_of_eons");
		t.name_locale = "Crystal Golem";
		t.cloneTaxonomyFromForSapiens("crystal_sword");
		t.name_taxonomic_genus = "bigus";
		t.name_taxonomic_species = "crystallus";
		t.kingdom_id_civilization = "civ_crystal_golem";
		t.architecture_id = "civ_crystal_golem";
		t.banner_id = "civ_crystal_golem";
		t.icon = "civs/" + t.id;
		t.has_skin = false;
		t.mush_id = "mush_unit";
		t.clonePhenotype("crystal_sword");
		t.color_hex = "#75D0F4";
		t.sound_hit = "event:/SFX/HIT/HitStone";
		addTrait("shiny");
		addTrait("strong_minded");
		t.addResource("gems", 1, pNewList: true);
		t.addResource("stone", 1);
		t.addResource("crystal_salt", 1);
		clone("civ_candy_man", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("candy_man_set");
		t.base_stats["mass_2"] = 70f;
		t.addGenome(("health", 85f), ("stamina", 60f), ("lifespan", 150f), ("mutation", 5f), ("damage", 12f), ("speed", 10f), ("offspring", 4f), ("diplomacy", 6f), ("warfare", 2f), ("stewardship", 3f), ("intelligence", 5f));
		t.addSubspeciesTrait("reproduction_vegetative");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_candy");
		t.addSubspeciesTrait("annoying_fireworks");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("diet_cannibalism");
		t.addSubspeciesTrait("diet_carnivore");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addSubspeciesTrait("genetic_mirror");
		t.addCultureTrait("xenophobic");
		t.addLanguageTrait("words_of_madness");
		t.name_locale = "Candy Man";
		t.kingdom_id_civilization = "civ_candy_man";
		t.architecture_id = "civ_candy_man";
		t.banner_id = "civ_candy_man";
		t.cloneTaxonomyFromForSapiens("smore");
		t.name_taxonomic_genus = "zucker";
		t.name_taxonomic_species = "daddies";
		t.icon = "civs/" + t.id;
		t.name_locale = t.id;
		t.has_skin = false;
		t.mush_id = "mush_unit";
		t.clonePhenotype("smore");
		t.color_hex = "#75D0F4";
		t.sound_hit = "event:/SFX/HIT/HitStone";
		addTrait("flesh_eater");
		addTrait("evil");
		addTrait("gluttonous");
		addTrait("strong_minded");
		t.addResource("candy", 4, pNewList: true);
		t.addResource("evil_beets", 1);
		clone("civ_beetle", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("insect_set");
		t.architecture_id = "civ_beetle";
		t.banner_id = "civ_beetle";
		t.kingdom_id_civilization = "civ_beetle";
		t.default_attack = "rocks";
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_xylophagy");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("fast_builders");
		t.addSubspeciesTrait("high_fecundity");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("exoskeleton");
		t.addSubspeciesTrait("egg_cocoon");
		t.addClanTrait("bonebreakers");
		t.addCultureTrait("city_layout_bricks");
		t.addCultureTrait("dense_dwellings");
		t.clonePhenotype("beetle");
		t.addTrait("hard_skin");
		t.addTrait("slow");
		t.addTrait("strong");
		t.addGenome(("health", 100f), ("stamina", 70f), ("lifespan", 50f), ("mutation", 1f), ("damage", 10f), ("speed", 12f), ("offspring", 10f), ("diplomacy", 1f), ("warfare", 6f), ("stewardship", 4f), ("intelligence", 4f));
		t.icon = "civs/" + t.id;
		t.cloneTaxonomyFromForSapiens("beetle");
		t.name_taxonomic_genus = "hollonus";
		t.name_taxonomic_species = "silkus";
		t.name_locale = t.id;
		t.disable_jump_animation = true;
		t.render_heads_for_babies = false;
		t.shadow = true;
		t.base_stats["mass_2"] = 10f;
		t.addResource("fertilizer", 1);
		clone("civ_seal", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("seal_set");
		t.architecture_id = "civ_seal";
		t.banner_id = "civ_seal";
		t.kingdom_id_civilization = "civ_seal";
		t.default_attack = "jaws";
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("adaptation_corruption");
		t.addSubspeciesTrait("adaptation_desert");
		t.addSubspeciesTrait("adaptation_infernal");
		t.addSubspeciesTrait("adaptation_permafrost");
		t.addSubspeciesTrait("adaptation_swamp");
		t.addSubspeciesTrait("adaptation_wasteland");
		t.addClanTrait("blood_of_sea");
		t.addClanTrait("combat_instincts");
		t.addLanguageTrait("strict_spelling");
		t.addCultureTrait("city_layout_bricks");
		t.addCultureTrait("armorsmith_mastery");
		t.addCultureTrait("weaponsmith_mastery");
		t.addCultureTrait("craft_shotgun");
		t.clonePhenotype("seal");
		t.addTrait("agile");
		t.addTrait("strong");
		t.addTrait("fat");
		t.addTrait("backstep");
		t.addTrait("deflect_projectile");
		t.addTrait("dodge");
		t.addTrait("block");
		t.addTrait("dash");
		t.addGenome(("health", 100f), ("stamina", 70f), ("lifespan", 50f), ("mutation", 1f), ("damage", 10f), ("speed", 12f), ("offspring", 10f), ("diplomacy", 1f), ("warfare", 6f), ("stewardship", 4f), ("intelligence", 4f));
		t.icon = "civs/" + t.id;
		t.cloneTaxonomyFromForSapiens("seal");
		t.name_taxonomic_genus = "phocanavus";
		t.name_taxonomic_species = "militaris";
		t.name_locale = t.id;
		t.disable_jump_animation = true;
		t.render_heads_for_babies = true;
		t.shadow = true;
		clone("civ_unicorn", "$animal_civ$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("unicorn_set");
		t.architecture_id = "civ_unicorn";
		t.banner_id = "civ_unicorn";
		t.kingdom_id_civilization = "civ_unicorn";
		t.default_attack = "hands";
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("egg_rainbow");
		t.addSubspeciesTrait("gestation_moderate");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("bioproduct_gems");
		t.addCultureTrait("city_layout_royal_checkers");
		t.addCultureTrait("fames_crown");
		t.addClanTrait("magic_blood");
		t.addClanTrait("witchs_vein");
		t.addClanTrait("warlocks_vein");
		t.addTrait("heart_of_wizard");
		t.addTrait("healing_aura");
		t.clonePhenotype("unicorn");
		t.base_stats["mass_2"] = 300f;
		t.addGenome(("health", 500f), ("stamina", 120f), ("mutation", 1f), ("lifespan", 500f), ("damage", 20f), ("speed", 15f), ("armor", 0f), ("offspring", 2f));
		t.icon = "civs/" + t.id;
		t.cloneTaxonomyFromForSapiens("unicorn");
		t.name_taxonomic_genus = "pankus";
		t.name_taxonomic_species = "veryloudus";
		t.name_locale = t.id;
		t.disable_jump_animation = true;
		t.render_heads_for_babies = true;
		t.shadow = true;
	}

	private void initMobsOther()
	{
		clone("cold_one", "$mob$");
		t.render_heads_for_babies = true;
		t.is_humanoid = true;
		t.can_turn_into_ice_one = false;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("cold_one_set");
		t.architecture_id = "civ_cold_one";
		t.kingdom_id_wild = "cold_one";
		t.kingdom_id_civilization = "miniciv_cold_one";
		t.build_order_template_id = "build_order_basic_2";
		t.setSocialStructure("group_blizzard", 40, pCreateOnSpawn: true, pFollowHerd: true, FamilyParentsMode.None);
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("adaptation_permafrost");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_metamorph");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("egg_ice");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("dense_dwellings");
		t.addCultureTrait("true_roots");
		t.addCultureTrait("craft_ice_weapon");
		t.addCultureTrait("city_layout_silk_web");
		t.addLanguageTrait("chilly_font");
		t.addClanTrait("deathbound");
		t.addClanTrait("we_are_legion");
		t.addClanTrait("flesh_weavers");
		t.addReligionTrait("cast_silence");
		t.addReligionTrait("path_of_unity");
		t.addReligionTrait("minds_awakening");
		t.addReligionTrait("rite_of_shattered_earth");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "cnidaria";
		t.name_taxonomic_class = "anthozoa";
		t.name_taxonomic_order = "cryonata";
		t.name_taxonomic_family = "gelididae";
		t.name_taxonomic_genus = "colda";
		t.name_taxonomic_species = "asice";
		t.base_stats["mass_2"] = 85f;
		t.addGenome(("health", 250f), ("stamina", 150f), ("mutation", 1f), ("lifespan", 1000f), ("damage", 40f), ("speed", 30f), ("armor", 15f), ("offspring", 4f));
		t.unit_other = true;
		t.name_locale = "Cold One";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("ice_hammer");
		t.banner_id = "civ_cold_one";
		t.icon = "iconWalker";
		t.color_hex = "#90D2D4";
		t.skeleton_id = "skeleton";
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.has_soul = true;
		t.addDecision("attack_golden_brain");
		addPhenotype("bright_blue");
		addTrait("regeneration");
		addTrait("cold_aura");
		addTrait("weightless");
		addTrait("freeze_proof");
		t.music_theme = "Units_ColdOne";
		t.sound_hit = "event:/SFX/HIT/HitGeneric";
		t.addResource("bones", 1, pNewList: true);
		t.addResource("snow_cucumbers", 2);
		clone("necromancer", "$mob$");
		t.is_humanoid = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("necromancer_set");
		t.kingdom_id_wild = "necromancer";
		t.kingdom_id_civilization = "miniciv_necromancer";
		t.architecture_id = "civ_necromancer";
		t.banner_id = "civ_necromancer";
		t.build_order_template_id = "build_order_basic_2";
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_spores");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("egg_face");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("gift_of_death");
		t.addSubspeciesTrait("gift_of_blood");
		t.addSubspeciesTrait("adaptation_corruption");
		t.addSubspeciesTrait("circadian_drift");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addLanguageTrait("spooky_language");
		t.addLanguageTrait("ancient_runes");
		t.addLanguageTrait("cursed_font");
		t.addLanguageTrait("mortal_tongue");
		t.addReligionTrait("shadowroot");
		t.addReligionTrait("cast_silence");
		t.addReligionTrait("rite_of_change");
		t.addReligionTrait("spawn_skeleton");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("true_roots");
		t.addCultureTrait("happiness_from_war");
		t.addCultureTrait("craft_necro_staff");
		t.addClanTrait("deathbound");
		t.addClanTrait("flesh_weavers");
		t.addClanTrait("blood_of_eons");
		t.addClanTrait("witchs_vein");
		t.addClanTrait("iron_will");
		t.name_taxonomic_kingdom = "fungi";
		t.name_taxonomic_phylum = "basidiomycota";
		t.name_taxonomic_class = "pucciniomycetes";
		t.name_taxonomic_order = "pucciniales";
		t.name_taxonomic_family = "umbramagusaceae";
		t.name_taxonomic_genus = "necromagus";
		t.name_taxonomic_species = "boneys";
		t.collective_term = "group_mycelium";
		t.base_stats["mass_2"] = 50f;
		t.addGenome(("health", 300f), ("stamina", 50f), ("mutation", 1f), ("lifespan", 550f), ("damage", 15f), ("speed", 10f), ("armor", 5f), ("offspring", 3f), ("diplomacy", 6f), ("warfare", 8f), ("stewardship", 6f), ("intelligence", 7f));
		t.unit_other = true;
		t.name_locale = "Necromancer";
		t.body_separate_part_hands = true;
		t.icon = "iconNecromancer";
		t.color_hex = "#EE3A42";
		t.skeleton_id = "skeleton";
		t.effect_cast_top = "fx_cast_top_green";
		t.effect_cast_ground = "fx_cast_ground_green";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("necromancer_staff");
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.has_soul = true;
		t.addDecision("attack_golden_brain");
		addPhenotype("skin_mixed");
		addTrait("regeneration");
		addTrait("evil");
		addTrait("fragile_health");
		t.addResource("mushrooms", 1, pNewList: true);
		t.addResource("bones", 1);
		clone("druid", "$mob$");
		t.is_humanoid = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("druid_set");
		t.architecture_id = "civ_druid";
		t.kingdom_id_wild = "druid";
		t.banner_id = "civ_druid";
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("egg_cocoon");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("bioproduct_mushrooms");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("gift_of_blood");
		t.addSubspeciesTrait("gift_of_harmony");
		t.addSubspeciesTrait("gift_of_life");
		t.addCultureTrait("ancestors_knowledge");
		t.addCultureTrait("true_roots");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("animal_whisperers");
		t.addCultureTrait("conscription_female_only");
		t.addCultureTrait("city_layout_pillars");
		t.addCultureTrait("craft_druid_staff");
		t.addCultureTrait("spear_lovers");
		t.addCultureTrait("xenophiles");
		t.addCultureTrait("ultimogeniture");
		t.addClanTrait("gaia_blood");
		t.addLanguageTrait("melodic");
		t.addReligionTrait("spawn_vegetation");
		t.addReligionTrait("rite_of_entanglement");
		t.name_taxonomic_kingdom = "fungi";
		t.name_taxonomic_phylum = "basidiomycota";
		t.name_taxonomic_class = "agaricomycetes";
		t.name_taxonomic_order = "agaricales";
		t.name_taxonomic_family = "luminomagusaceae";
		t.name_taxonomic_genus = "druidus";
		t.name_taxonomic_species = "greenus";
		t.collective_term = "group_mycelium";
		t.base_stats["mass_2"] = 80f;
		t.addGenome(("health", 100f), ("stamina", 100f), ("lifespan", 200f), ("mutation", 2f), ("damage", 12f), ("speed", 12f), ("armor", 4f), ("offspring", 8f), ("diplomacy", 7f), ("warfare", 5f), ("stewardship", 8f), ("intelligence", 6f));
		t.addSubspeciesTrait("death_grow_tree");
		t.addSubspeciesTrait("death_grow_plant");
		t.unit_other = true;
		t.name_locale = "Druid";
		t.body_separate_part_hands = true;
		t.kingdom_id_wild = "druid";
		t.kingdom_id_civilization = "civ_druid";
		t.icon = "iconDruid";
		t.color_hex = "#4CDB75";
		t.skeleton_id = "skeleton";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("druid_staff");
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.has_soul = true;
		addPhenotype("skin_mixed");
		addTrait("regeneration");
		addTrait("flower_prints");
		addTrait("healing_aura");
		t.addResource("mushrooms", 1, pNewList: true);
		t.addResource("herbs", 2);
		t.addResource("bones", 1);
		clone("plague_doctor", "$mob$");
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("plague_doctor_set");
		t.kingdom_id_wild = "plague_doctor";
		t.kingdom_id_civilization = "miniciv_plague_doctor";
		t.architecture_id = "civ_bandit";
		t.banner_id = "civ_bandit";
		t.build_order_template_id = "build_order_basic_2";
		addPhenotype("gray_black");
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("population_moderate");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_spores");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("bioproduct_mushrooms");
		t.addSubspeciesTrait("egg_face");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("gift_of_harmony");
		t.addCultureTrait("legacy_keepers");
		t.addCultureTrait("xenophiles");
		t.addCultureTrait("conscription_female_only");
		t.addCultureTrait("animal_whisperers");
		t.addCultureTrait("training_potential");
		t.addCultureTrait("craft_doctor_staff");
		t.addCultureTrait("city_layout_royal_checkers");
		t.addClanTrait("flesh_weavers");
		t.addClanTrait("gaia_blood");
		t.addClanTrait("best_five");
		t.addClanTrait("stonefists");
		t.addLanguageTrait("beautiful_calligraphy");
		t.addLanguageTrait("enlightening_script");
		t.addLanguageTrait("magic_words");
		t.addLanguageTrait("cursed_font");
		t.addReligionTrait("cast_cure");
		t.addReligionTrait("rite_of_change");
		t.name_taxonomic_kingdom = "fungi";
		t.name_taxonomic_phylum = "ascomycota";
		t.name_taxonomic_class = "eurotiomycetes";
		t.name_taxonomic_order = "eurotiales";
		t.name_taxonomic_family = "aspergillaceae";
		t.name_taxonomic_genus = "antiplagus";
		t.name_taxonomic_species = "medicus";
		t.collective_term = "group_mycelium";
		t.base_stats["mass_2"] = 80f;
		t.addGenome(("health", 500f), ("stamina", 100f), ("lifespan", 100f), ("mutation", 2f), ("damage", 1f), ("speed", 20f), ("armor", 4f), ("offspring", 6f), ("diplomacy", 7f), ("warfare", 5f), ("stewardship", 8f), ("intelligence", 6f));
		t.unit_other = true;
		t.name_locale = "Plague Doctor";
		t.immune_to_tumor = true;
		t.body_separate_part_hands = true;
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.addDecision("random_move_towards_civ_building");
		t.icon = "iconPlagueDoctor";
		t.color_hex = "#EE3A42";
		t.skeleton_id = "skeleton";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("plague_doctor_staff");
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.has_soul = true;
		addTrait("regeneration");
		addTrait("immune");
		addTrait("fire_proof");
		t.addResource("mushrooms", 1, pNewList: true);
		clone("white_mage", "$mob$");
		t.is_humanoid = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("white_mage_set");
		t.kingdom_id_wild = "white_mage";
		t.kingdom_id_civilization = "miniciv_white_mage";
		t.architecture_id = "civ_white_mage";
		t.banner_id = "civ_white_mage";
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_budding");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("egg_orb");
		t.addSubspeciesTrait("bioproduct_mushrooms");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("metamorphosis_butterfly");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("gift_of_void");
		t.addSubspeciesTrait("gift_of_blood");
		t.addSubspeciesTrait("gift_of_water");
		t.addClanTrait("witchs_vein");
		t.addLanguageTrait("ancient_runes");
		t.addLanguageTrait("enlightening_script");
		t.addLanguageTrait("strict_spelling");
		t.addLanguageTrait("beautiful_calligraphy");
		t.addLanguageTrait("magic_words");
		t.addLanguageTrait("melodic");
		t.addCultureTrait("craft_white_staff");
		t.addCultureTrait("animal_whisperers");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("city_layout_rings");
		t.addCultureTrait("city_layout_the_grand_arrangement");
		t.addReligionTrait("cast_cure");
		t.addCultureTrait("ancestors_knowledge");
		addTrait("regeneration");
		addTrait("freeze_proof");
		addTrait("wise");
		t.name_taxonomic_kingdom = "fungi";
		t.name_taxonomic_phylum = "basidiomycota";
		t.name_taxonomic_class = "agaricomycetes";
		t.name_taxonomic_order = "agaricales";
		t.name_taxonomic_family = "luminomagusaceae";
		t.name_taxonomic_genus = "goodmagus";
		t.name_taxonomic_species = "staffus";
		t.collective_term = "group_mycelium";
		addPhenotype("skin_mixed");
		t.addGenome(("health", 300f), ("stamina", 50f), ("lifespan", 500f), ("mutation", 1f), ("damage", 5f), ("speed", 10f), ("armor", 3f), ("offspring", 2f), ("diplomacy", 8f), ("warfare", 4f), ("stewardship", 7f), ("intelligence", 7f));
		t.unit_other = true;
		t.name_locale = "White Mage";
		t.body_separate_part_hands = true;
		t.base_stats["targets"] = 1f;
		t.base_stats["mass_2"] = 75f;
		t.icon = "iconWhiteMage";
		t.color_hex = "#EE3A42";
		t.skeleton_id = "skeleton";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("white_staff");
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.has_soul = true;
		t.addResource("mushrooms", 2, pNewList: true);
		clone("evil_mage", "$mob$");
		t.is_humanoid = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("evil_mage_set");
		t.kingdom_id_wild = "evil_mage";
		t.kingdom_id_civilization = "miniciv_evil_mage";
		t.architecture_id = "civ_evil_mage";
		t.banner_id = "civ_evil_mage";
		t.build_order_template_id = "build_order_basic_2";
		t.addSubspeciesTrait("photosynthetic_skin");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_budding");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("egg_flames");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("bioproduct_mushrooms");
		t.addSubspeciesTrait("fire_elemental_form");
		t.addSubspeciesTrait("population_small");
		t.addSubspeciesTrait("spicy_kids");
		t.addSubspeciesTrait("gift_of_void");
		t.addSubspeciesTrait("gift_of_fire");
		t.addSubspeciesTrait("gift_of_thunder");
		t.addSubspeciesTrait("gift_of_air");
		t.addSubspeciesTrait("gift_of_blood");
		t.addSubspeciesTrait("adaptation_infernal");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addCultureTrait("ancestors_knowledge");
		t.addCultureTrait("craft_evil_staff");
		t.addCultureTrait("city_layout_architects_eye");
		t.addCultureTrait("city_layout_the_grand_arrangement");
		t.addClanTrait("deathbound");
		t.addClanTrait("warlocks_vein");
		t.addLanguageTrait("scorching_words");
		t.addLanguageTrait("ancient_runes");
		t.addLanguageTrait("enlightening_script");
		t.addLanguageTrait("strict_spelling");
		t.addLanguageTrait("magic_words");
		t.addLanguageTrait("confusing_semantics");
		t.addReligionTrait("rite_of_infernal_wrath");
		t.name_taxonomic_kingdom = "fungi";
		t.name_taxonomic_phylum = "basidiomycota";
		t.name_taxonomic_class = "pucciniomycetes";
		t.name_taxonomic_order = "pucciniales";
		t.name_taxonomic_family = "umbramagusaceae";
		t.name_taxonomic_genus = "evilmagus";
		t.name_taxonomic_species = "burnus";
		t.collective_term = "group_mycelium";
		addPhenotype("gray_black");
		t.addGenome(("health", 500f), ("stamina", 60f), ("lifespan", 450f), ("mutation", 1f), ("damage", 1f), ("armor", 4f), ("speed", 20f), ("offspring", 2f), ("diplomacy", 5f), ("warfare", 7f), ("stewardship", 5f), ("intelligence", 8f));
		t.unit_other = true;
		t.name_locale = "Evil Mage";
		t.body_separate_part_hands = true;
		t.base_stats["targets"] = 1f;
		t.base_stats["mass_2"] = 75f;
		t.icon = "iconEvilMage";
		t.color_hex = "#EE3A42";
		t.skeleton_id = "skeleton";
		t.effect_teleport = "fx_teleport_red";
		t.effect_cast_top = "fx_cast_top_red";
		t.effect_cast_ground = "fx_cast_ground_red";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("evil_staff");
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.has_soul = true;
		addTrait("evil");
		addTrait("fire_proof");
		addTrait("regeneration");
		addTrait("hotheaded");
		t.addResource("mushrooms", 2, pNewList: true);
		t.addResource("bones", 1);
		clone("skeleton", "$mob$");
		t.species_spawn_radius = 60;
		t.is_humanoid = true;
		t.can_have_subspecies = true;
		t.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("phenotypes");
		t.kingdom_id_civilization = "miniciv_jumpy_skull";
		t.architecture_id = "civ_necromancer";
		t.banner_id = "civ_necromancer";
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("skeleton_set");
		t.use_phenotypes = false;
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.unit_other = true;
		t.name_locale = "Skeleton";
		t.collective_term = "group_stack";
		t.body_separate_part_hands = true;
		t.has_skin = false;
		t.kingdom_id_wild = "undead";
		t.can_be_killed_by_divine_light = true;
		t.icon = "iconSkeleton";
		t.color_hex = "#ffffff";
		t.job = AssetLibrary<ActorAsset>.a<string>("skeleton_job");
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "ossiphora";
		t.name_taxonomic_class = "calciata";
		t.name_taxonomic_order = "rattlers";
		t.name_taxonomic_family = "osteus";
		t.name_taxonomic_genus = "bonelords";
		t.name_taxonomic_species = "calcius";
		t.addSubspeciesNamePrefix("calcius");
		t.addSubspeciesNamePrefix("bonelords");
		t.addSubspeciesNamePrefix("boney");
		t.addGenome(("health", 100f), ("damage", 10f), ("speed", 10f), ("stamina", 100f), ("lifespan", 100f));
		t.base_stats["mass_2"] = 15f;
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("bow_bronze", "bow_steel", "bow_iron", "sword_steel", "spear_steel", "sword_iron");
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.addDecision("attack_golden_brain");
		t.die_from_dispel = true;
		addTrait("weightless");
		addTrait("backstep");
		addTrait("dodge");
		addTrait("dash");
		addTrait("block");
		t.music_theme = "Units_Skeleton";
		t.sound_hit = "event:/SFX/HIT/HitBone";
		t.can_be_surprised = false;
		t.addResource("bones", 2, pNewList: true);
		clone("jumpy_skull", "$mob$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("jumpy_skull_set");
		t.kingdom_id_wild = "jumpy_skull";
		t.kingdom_id_civilization = "miniciv_jumpy_skull";
		t.architecture_id = "civ_necromancer";
		t.banner_id = "civ_necromancer";
		t.addSubspeciesTrait("reproduction_soulborne");
		t.addSubspeciesTrait("aggressive");
		t.addSubspeciesTrait("population_small");
		addPhenotype("white_gray");
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "infernalia";
		t.name_taxonomic_class = "daemonica";
		t.name_taxonomic_order = "skullus";
		t.name_taxonomic_family = "hoppidae";
		t.name_taxonomic_genus = "chere";
		t.name_taxonomic_species = "pushka";
		t.collective_term = "group_stack";
		t.base_stats["mass_2"] = 3.5f;
		t.addDecision("check_swearing");
		t.addGenome(("health", 1f), ("lifespan", 500f), ("damage", 10f), ("speed", 5f));
		t.unit_other = true;
		t.name_locale = "Rude Skull";
		t.can_turn_into_mush = false;
		t.can_turn_into_zombie = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_ice_one = false;
		t.body_separate_part_hands = false;
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.has_skin = false;
		t.can_be_killed_by_divine_light = true;
		t.die_from_dispel = true;
		t.icon = "iconJumpySkull";
		t.color_hex = "#ffffff";
		t.disable_jump_animation = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.max_random_amount = 4;
		addTrait("weightless");
		addTrait("block");
		addTrait("paranoid");
		addTrait("hotheaded");
		t.actor_size = ActorSize.S7_Cat;
		t.music_theme = "Units_Skeleton";
		t.sound_hit = "event:/SFX/HIT/HitBone";
		t.addResource("bones", 2, pNewList: true);
		clone("fire_elemental", "$mob$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("fire_elemental_set");
		t.use_phenotypes = false;
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "infernalia";
		t.name_taxonomic_class = "daemonica";
		t.name_taxonomic_order = "elementales";
		t.name_taxonomic_family = "ignisidae";
		t.name_taxonomic_genus = "ignis";
		t.name_taxonomic_species = "blazarus";
		t.trait_group_filter_subspecies = AssetLibrary<ActorAsset>.l<string>("advanced_brain", "phenotypes");
		t.collective_term = "group_blaze";
		t.addSubspeciesTrait("heat_resistance");
		t.addSubspeciesTrait("hydrophobia");
		t.addSubspeciesTrait("fire_elemental_form");
		t.addSubspeciesTrait("fenix_born");
		t.addSubspeciesTrait("egg_flames");
		t.addSubspeciesTrait("gestation_long");
		t.addSubspeciesTrait("rapid_aging");
		t.addSubspeciesTrait("reproduction_soulborne");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addCultureTrait("matriarchy");
		t.addLanguageTrait("scorching_words");
		t.base_stats["mass_2"] = 50f;
		t.addGenome(("health", 5f), ("lifespan", 450f), ("damage", 10f), ("speed", 10f));
		t.unit_other = true;
		t.name_locale = "Fire Elemental";
		t.can_turn_into_mush = false;
		t.can_turn_into_zombie = false;
		t.can_turn_into_ice_one = false;
		t.can_turn_into_tumor = false;
		t.body_separate_part_hands = false;
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.use_items = false;
		t.take_items = false;
		t.can_edit_equipment = false;
		t.has_skin = false;
		t.has_soul = false;
		t.die_from_dispel = true;
		t.kingdom_id_wild = "fire_elemental";
		t.architecture_id = "civ_demon";
		t.banner_id = "civ_demon";
		t.die_in_lava = false;
		t.default_attack = "fire_hands";
		t.allowed_status_tiers = StatusTier.Basic;
		t.icon = "iconFireElemental";
		t.color_hex = "#ff0000";
		t.disable_jump_animation = true;
		t.animation_idle = ActorAnimationSequences.idle_0_3;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.walk_0_3;
		t.shadow = false;
		t.max_random_amount = 4;
		addTrait("weightless");
		addTrait("light_lamp");
		addTrait("fire_proof");
		addTrait("fire_blood");
		addTrait("burning_feet");
		t.music_theme = "Units_Skeleton";
		t.sound_hit = null;
		t.sound_spawn = null;
		t.sound_attack = null;
		t.sound_idle = null;
		t.sound_death = null;
		t.generateFmodPaths("fire_elemental");
		t.addResource("peppers", 1, pNewList: true);
		clone("fire_elemental_blob", "fire_elemental");
		t.base_asset_id = "fire_elemental";
		t.show_in_taxonomy_tooltip = false;
		t.show_in_knowledge_window = false;
		t.base_stats["speed"] = 3f;
		t.base_stats["mass_2"] = 66f;
		clone("fire_elemental_horse", "fire_elemental");
		t.base_asset_id = "fire_elemental";
		t.show_in_taxonomy_tooltip = false;
		t.show_in_knowledge_window = false;
		t.base_stats["speed"] = 20f;
		t.base_stats["mass_2"] = 450f;
		clone("fire_elemental_slug", "fire_elemental");
		t.base_asset_id = "fire_elemental";
		t.show_in_taxonomy_tooltip = false;
		t.show_in_knowledge_window = false;
		t.prevent_unconscious_rotation = true;
		t.base_stats["speed"] = 2f;
		t.base_stats["mass_2"] = 30f;
		clone("fire_elemental_snake", "fire_elemental");
		t.base_asset_id = "fire_elemental";
		t.show_in_taxonomy_tooltip = false;
		t.show_in_knowledge_window = false;
		t.base_stats["speed"] = 5f;
		t.base_stats["mass_2"] = 15f;
		clone("ghost", "$mob$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("necromancer_set");
		t.use_phenotypes = false;
		t.unit_other = true;
		t.name_locale = "Ghost";
		t.body_separate_part_hands = false;
		t.has_advanced_textures = true;
		t.has_baby_form = false;
		t.has_skin = false;
		t.shadow = false;
		t.can_turn_into_zombie = false;
		t.kingdom_id_wild = "undead";
		t.kingdom_id_civilization = "civ_ghost";
		t.can_be_killed_by_divine_light = true;
		t.base_stats["mass_2"] = 0f;
		t.addGenome(("health", 200f), ("damage", 10f), ("speed", 15f), ("lifespan", 1000f));
		t.icon = "iconGhost";
		t.color_hex = "#ffffff";
		t.job = AssetLibrary<ActorAsset>.a<string>("skeleton_job");
		t.disable_jump_animation = true;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.max_random_amount = 1;
		t.can_turn_into_mush = false;
		t.mush_id = string.Empty;
		t.can_turn_into_tumor = false;
		t.tumor_id = string.Empty;
		t.has_skin = false;
		t.immune_to_injuries = true;
		t.die_on_blocks = false;
		t.sound_hit = "event:/SFX/HIT/HitGeneric";
		t.prevent_unconscious_rotation = true;
		t.architecture_id = "civ_ghost";
		t.banner_id = "civ_ghost";
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "otherworldlia";
		t.name_taxonomic_class = "ectoplasmica";
		t.name_taxonomic_order = "soulus";
		t.name_taxonomic_family = "transparencia";
		t.name_taxonomic_genus = "spectrum";
		t.name_taxonomic_species = "umbra";
		t.collective_term = "group_cloud";
		addTrait("weightless");
		addTrait("fire_proof");
		addTrait("freeze_proof");
		t.addLanguageTrait("spooky_language");
		t.addSubspeciesTrait("bioluminescence");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("hovering");
		t.addSubspeciesTrait("reproduction_soulborne");
		t.resources_given = null;
		clone("fire_skull", "$mob$");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("fire_skull_set");
		t.kingdom_id_wild = "fire_skull";
		t.kingdom_id_civilization = "miniciv_fire_skull";
		t.architecture_id = "civ_demon";
		t.banner_id = "civ_demon";
		t.build_order_template_id = "build_order_basic_2";
		t.use_phenotypes = false;
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "infernalia";
		t.name_taxonomic_class = "daemonica";
		t.name_taxonomic_order = "skullus";
		t.name_taxonomic_family = "pyropidae";
		t.name_taxonomic_genus = "gorit";
		t.name_taxonomic_species = "dumkus";
		t.collective_term = "group_stack";
		t.addLanguageTrait("scorching_words");
		t.addSubspeciesTrait("reproduction_soulborne");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("adaptation_infernal");
		t.addSubspeciesTrait("egg_flames");
		t.base_stats["mass_2"] = 3.5f;
		t.addGenome(("health", 5f), ("damage", 15f), ("speed", 5f));
		t.unit_other = true;
		t.name_locale = "Fire Skull";
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.can_turn_into_ice_one = false;
		t.body_separate_part_hands = false;
		t.has_advanced_textures = false;
		t.has_baby_form = false;
		t.has_skin = false;
		t.icon = "iconFireSkull";
		t.color_hex = "#EE3A42";
		t.disable_jump_animation = true;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.max_random_amount = 4;
		addTrait("evil");
		addTrait("weightless");
		addTrait("fire_blood");
		addTrait("fire_proof");
		t.addResource("peppers", 1, pNewList: true);
		t.addResource("bones", 2);
		clone("demon", "$mob$");
		t.is_humanoid = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("demon_set");
		t.kingdom_id_wild = "demon";
		t.kingdom_id_civilization = "miniciv_demon";
		t.architecture_id = "civ_demon";
		t.banner_id = "civ_demon";
		t.build_order_template_id = "build_order_basic_2";
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("reproduction_soulborne");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("egg_flames");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("spicy_kids");
		t.addSubspeciesTrait("heat_resistance");
		t.addSubspeciesTrait("hydrophobia");
		t.addSubspeciesTrait("chaos_driven");
		t.addSubspeciesTrait("adaptation_infernal");
		t.addSubspeciesTrait("diet_hematophagy");
		t.addSubspeciesTrait("circadian_drift");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("happiness_from_war");
		t.addCultureTrait("craft_flame_weapon");
		t.addLanguageTrait("scorching_words");
		t.addReligionTrait("rite_of_the_abyss");
		t.addReligionTrait("rite_of_infernal_wrath");
		t.addReligionTrait("infernal_rot");
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "infernalia";
		t.name_taxonomic_class = "daemonica";
		t.name_taxonomic_order = "diabolus";
		t.name_taxonomic_family = "maleficidae";
		t.name_taxonomic_genus = "daemorior";
		t.name_taxonomic_species = "maleficus";
		t.setSocialStructure("group_blaze", 40, pCreateOnSpawn: true, pFollowHerd: true, FamilyParentsMode.None);
		t.base_stats["mass_2"] = 66.6f;
		t.addGenome(("health", 200f), ("stamina", 150f), ("mutation", 2f), ("lifespan", 1000f), ("damage", 35f), ("speed", 15f), ("armor", 10f), ("offspring", 6f));
		t.unit_other = true;
		t.name_locale = "Demon";
		t.body_separate_part_hands = true;
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("flame_sword");
		t.can_be_killed_by_divine_light = true;
		t.die_in_lava = false;
		t.icon = "iconDemon";
		t.color_hex = "#EE3A42";
		t.skeleton_id = "skeleton";
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.disable_jump_animation = true;
		t.addDecision("attack_golden_brain");
		t.actor_size = ActorSize.S14_Cow;
		addPhenotype("skin_red");
		addTrait("regeneration");
		addTrait("burning_feet");
		addTrait("fire_blood");
		addTrait("evil");
		t.music_theme = "Units_Demon";
		t.addResource("bones", 1, pNewList: true);
		t.addResource("peppers", 2);
		t.addResource("meat", 1);
		clone("angle", "$mob$");
		t.setUnlockedWithAchievement("achievementAncientWarOfGeometryAndEvil");
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("angle_set");
		t.kingdom_id_wild = "angle";
		t.kingdom_id_civilization = "miniciv_angle";
		t.architecture_id = "civ_angle";
		t.banner_id = "civ_angle";
		t.build_order_template_id = "build_order_basic_2";
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTrait("egg_orb");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("heat_resistance");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("reproduction_divine");
		t.addSubspeciesTrait("pure");
		t.addSubspeciesTrait("gift_of_harmony");
		t.addSubspeciesTrait("hovering");
		t.addClanTrait("gods_chosen");
		t.addClanTrait("iron_will");
		t.addCultureTrait("city_layout_cross");
		t.addCultureTrait("xenophiles");
		t.addCultureTrait("fames_crown");
		t.addLanguageTrait("font_of_gods");
		t.addLanguageTrait("repeated_sentences");
		t.addLanguageTrait("eternal_text");
		t.addReligionTrait("rite_of_infinite_edges");
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "mathematica";
		t.name_taxonomic_class = "geometrica";
		t.name_taxonomic_order = "polygones";
		t.name_taxonomic_family = "holidae";
		t.name_taxonomic_genus = "anglo";
		t.name_taxonomic_species = "holliens";
		t.setSocialStructure("group_polygon", 40, pCreateOnSpawn: true, pFollowHerd: true, FamilyParentsMode.None);
		t.base_stats["mass_2"] = 7.77f;
		t.addGenome(("health", 200f), ("stamina", 200f), ("mutation", 2f), ("lifespan", 1000f), ("damage", 35f), ("speed", 15f), ("armor", 10f), ("offspring", 7f));
		t.unit_other = true;
		t.name_locale = "Angle";
		t.body_separate_part_hands = true;
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("ice_hammer");
		t.die_in_lava = false;
		t.icon = "iconAngle";
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.can_turn_into_ice_one = false;
		t.color_hex = "#EE3A42";
		t.animation_idle = ActorAnimationSequences.walk_0_4;
		t.animation_walk = ActorAnimationSequences.walk_0_4;
		t.animation_swim = null;
		t.disable_jump_animation = true;
		t.actor_size = ActorSize.S14_Cow;
		t.prevent_unconscious_rotation = true;
		addPhenotype("bright_yellow");
		addTrait("regeneration");
		addTrait("blessed");
		addTrait("light_lamp");
		addTrait("psychopath");
		t.music_theme = "Units_Demon";
		t.can_be_surprised = false;
		clone("fairy", "$peaceful_animal$");
		t.needs_to_be_explored = true;
		t.has_advanced_textures = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("fairy_set");
		t.kingdom_id_wild = "fairy";
		t.kingdom_id_civilization = "miniciv_fairy";
		t.architecture_id = "civ_fairy";
		t.banner_id = "civ_fairy";
		t.addSubspeciesTrait("gift_of_life");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("bioluminescence");
		t.addSubspeciesTrait("hyper_intelligence");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("diet_frugivore");
		t.addSubspeciesTrait("diet_florivore");
		t.addSubspeciesTrait("bioproduct_gold");
		t.addSubspeciesTrait("egg_rainbow");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("metamorphosis_butterfly");
		t.addSubspeciesTrait("death_grow_plant");
		t.addSubspeciesTrait("gift_of_harmony");
		t.addSubspeciesTrait("gift_of_thunder");
		t.addSubspeciesTrait("hovering");
		t.addCultureTrait("ultimogeniture");
		t.addCultureTrait("tiny_legends");
		t.addCultureTrait("fames_crown");
		t.addReligionTrait("cast_cure");
		t.addLanguageTrait("melodic");
		t.addClanTrait("magic_blood");
		t.name_taxonomic_kingdom = "mythoria";
		t.name_taxonomic_phylum = "arthropoda";
		t.name_taxonomic_class = "insecta";
		t.name_taxonomic_order = "diptera";
		t.name_taxonomic_family = "fabulidae";
		t.name_taxonomic_genus = "faerina";
		t.name_taxonomic_species = "glitterbug";
		t.collective_term = "group_flutter";
		addPhenotype("bright_pink");
		t.base_stats["mass_2"] = 0.01f;
		t.addGenome(("health", 40f), ("stamina", 100f), ("lifespan", 300f), ("mutation", 2f), ("damage", 8f), ("speed", 15f), ("armor", 2f), ("offspring", 5f));
		t.unit_other = true;
		t.default_animal = false;
		t.name_locale = "Fairy";
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_idle = ActorAnimationSequences.walk_0_3;
		t.animation_swim = null;
		t.source_meat = false;
		t.source_meat_insect = false;
		t.actor_size = ActorSize.S3_Rat;
		t.shadow_texture = "unitShadow_2";
		t.icon = "iconFairy";
		t.color_hex = "#23F3FF";
		t.disable_jump_animation = true;
		t.has_soul = true;
		t.move_from_block = true;
		t.die_on_blocks = false;
		t.prevent_unconscious_rotation = true;
		t.animation_speed_based_on_walk_speed = false;
		addTrait("weightless");
		addTrait("healing_aura");
		addTrait("immune");
		addTrait("light_lamp");
		addTrait("moonchild");
		t.music_theme = "Units_Fairy";
		clone("bandit", "$mob$");
		t.render_heads_for_babies = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("bandit_set");
		t.is_humanoid = true;
		t.architecture_id = "civ_bandit";
		t.kingdom_id_wild = "bandit";
		t.kingdom_id_civilization = "miniciv_bandit";
		t.banner_id = "civ_bandit";
		t.addSubspeciesTrait("reproduction_sexual");
		t.addSubspeciesTrait("reproduction_strategy_viviparity");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("nimble");
		t.addSubspeciesTrait("shiny_love");
		t.addSubspeciesTrait("circadian_drift");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("stomach");
		t.addCultureTrait("join_or_die");
		t.addCultureTrait("fames_crown");
		t.addClanTrait("nitroglycerin_blood");
		t.addClanTrait("combat_instincts");
		t.addReligionTrait("rite_of_dissent");
		t.cloneTaxonomyFromForSapiens("raccoon");
		t.name_taxonomic_genus = "banditus";
		t.name_taxonomic_species = "nikonis";
		t.collective_term = "group_gang";
		t.base_stats["mass_2"] = 67f;
		t.addGenome(("health", 100f), ("stamina", 100f), ("lifespan", 60f), ("mutation", 2f), ("damage", 18f), ("speed", 10f), ("armor", 5f), ("offspring", 5f), ("diplomacy", 4f), ("warfare", 6f), ("stewardship", 3f), ("intelligence", 4f));
		t.unit_other = true;
		t.name_locale = "Bandit";
		t.body_separate_part_hands = true;
		t.kingdom_id_wild = "bandit";
		t.icon = "iconBandit";
		t.color_hex = "#4A3F35";
		t.skeleton_id = "skeleton";
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("spear_bronze", "spear_steel", "spear_iron", "sword_bronze", "sword_steel", "sword_iron", "bow_bronze", "bow_steel", "bow_iron");
		t.has_soul = true;
		addPhenotype("skin_mixed");
		addTrait("bomberman");
		addTrait("thief");
		t.disable_jump_animation = true;
		t.music_theme = "Units_Bandits";
		t.addResource("cider", 1);
		clone("snowman", "$mob$");
		t.render_heads_for_babies = true;
		t.needs_to_be_explored = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("snowman_set");
		t.kingdom_id_wild = "snowman";
		t.kingdom_id_civilization = "miniciv_snowman";
		t.architecture_id = "civ_snowman";
		t.banner_id = "civ_snowman";
		t.build_order_template_id = "build_order_basic_2";
		addPhenotype("white_gray");
		t.addSubspeciesTrait("reproduction_fission");
		t.addSubspeciesTrait("genetic_mirror");
		t.addSubspeciesTrait("egg_ice");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("genetic_psychosis");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("hydrophobia");
		t.addSubspeciesTrait("good_throwers");
		t.addSubspeciesTrait("adaptation_permafrost");
		t.addLanguageTrait("chilly_font");
		t.addCultureTrait("solitude_seekers");
		t.addReligionTrait("hand_of_order");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "cnidaria";
		t.name_taxonomic_class = "anthozoa";
		t.name_taxonomic_order = "cryonata";
		t.name_taxonomic_family = "niveidae";
		t.name_taxonomic_genus = "snowda";
		t.name_taxonomic_species = "frosti";
		t.collective_term = "group_melt";
		t.base_stats["mass_2"] = 67f;
		t.addGenome(("health", 100f), ("stamina", 50f), ("lifespan", 60f), ("mutation", 5f), ("damage", 10f), ("speed", 10f), ("armor", 5f), ("offspring", 3f), ("diplomacy", 2f), ("warfare", 2f), ("stewardship", 2f), ("intelligence", 2f));
		t.unit_other = true;
		t.name_locale = "Snowman";
		t.default_attack = "snowball";
		t.icon = "iconSnowman";
		t.color_hex = "#FFFFFF";
		t.can_turn_into_mush = false;
		t.can_turn_into_tumor = false;
		t.can_turn_into_zombie = false;
		t.can_turn_into_ice_one = false;
		addTrait("heliophobia");
		addTrait("regeneration");
		addTrait("cold_aura");
		addTrait("fat");
		addTrait("freeze_proof");
		t.disable_jump_animation = true;
		t.music_theme = "Units_Snowman";
		t.addResource("pine_cones", 1, pNewList: true);
		clone("alien", "$mob$");
		t.render_heads_for_babies = true;
		t.needs_to_be_explored = true;
		t.is_humanoid = true;
		t.name_template_sets = AssetLibrary<ActorAsset>.a<string>("alien_set");
		t.architecture_id = "civ_alien";
		t.kingdom_id_wild = "aliens";
		t.kingdom_id_civilization = "civ_aliens";
		t.banner_id = "civ_alien";
		t.build_order_template_id = "build_order_basic_2";
		addPhenotype("bright_green");
		t.addSubspeciesTrait("prefrontal_cortex");
		t.addSubspeciesTrait("advanced_hippocampus");
		t.addSubspeciesTrait("amygdala");
		t.addSubspeciesTrait("wernicke_area");
		t.addSubspeciesTrait("stomach");
		t.addSubspeciesTrait("diet_omnivore");
		t.addSubspeciesTrait("accelerated_healing");
		t.addSubspeciesTrait("bioluminescence");
		t.addSubspeciesTrait("fins");
		t.addSubspeciesTrait("hyper_intelligence");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTrait("egg_alien");
		t.addSubspeciesTrait("reproduction_strategy_oviparity");
		t.addSubspeciesTrait("reproduction_sexual");
		t.addClanTrait("best_five");
		t.addCultureTrait("city_layout_tile_moonsteps");
		t.addCultureTrait("city_layout_iron_weave");
		t.addCultureTrait("craft_blaster");
		t.addReligionTrait("rite_of_fractured_minds");
		t.addReligionTrait("minds_awakening");
		t.name_taxonomic_kingdom = "animalia";
		t.name_taxonomic_phylum = "tardigrada";
		t.name_taxonomic_class = "eutardigrada";
		t.name_taxonomic_order = "apochela";
		t.name_taxonomic_family = "milnesiidae";
		t.name_taxonomic_genus = "abugus";
		t.name_taxonomic_species = "abobicus";
		t.collective_term = "group_topology";
		t.addGenome(("health", 200f), ("stamina", 150f), ("lifespan", 60f), ("mutation", 2f), ("damage", 18f), ("speed", 10f), ("armor", 5f), ("offspring", 3f), ("diplomacy", 7f), ("warfare", 7f), ("stewardship", 7f), ("intelligence", 7f));
		t.unit_other = true;
		t.body_separate_part_hands = true;
		t.name_locale = "Alien";
		t.base_stats["lifespan"] = 1000f;
		t.base_stats["mass_2"] = 32.5f;
		t.default_weapons = AssetLibrary<ActorAsset>.a<string>("alien_blaster");
		t.icon = "iconAlien";
		t.color_hex = "#00FF00";
		t.can_turn_into_tumor = true;
		t.can_turn_into_mush = true;
		t.mush_id = "mush_unit";
		t.tumor_id = "tumor_monster_unit";
		t.has_soul = true;
		t.family_banner_frame_generation_inclusion = "families/frame_11";
		t.family_banner_frame_only_inclusion = true;
		addTrait("regeneration");
		addTrait("fat");
		addTrait("acid_blood");
		addTrait("acid_proof");
		addTrait("strong_minded");
		t.disable_jump_animation = true;
	}

	private void initTemplates()
	{
		ActorAsset obj = new ActorAsset
		{
			id = "$basic_unit$"
		};
		ActorAsset pAsset = obj;
		t = obj;
		add(pAsset);
		t.base_stats["attack_speed"] = 1f;
		t.base_stats["accuracy"] = 1f;
		t.base_stats["mass"] = 1f;
		t.base_stats["knockback"] = 1.5f;
		t.base_stats["targets"] = 1f;
		t.base_stats["area_of_effect"] = 0.1f;
		t.base_stats["size"] = 0.5f;
		t.base_stats["range"] = 1f;
		t.base_stats["critical_damage_multiplier"] = 2f;
		t.base_stats["scale"] = 0.1f;
		t.base_stats["multiplier_supply_timer"] = 1f;
		t.base_throwing_range = 7f;
		t.affected_by_dust = true;
		t.needs_to_be_explored = false;
		t.job = AssetLibrary<ActorAsset>.a<string>("decision");
		clone("$basic_unit_colored$", "$basic_unit$");
		t.has_advanced_textures = true;
		t.has_baby_form = true;
		t.setSimpleCivSettings();
		t.kingdom_id_wild = "neutral_animals";
		t.can_edit_equipment = true;
		t.use_items = true;
		t.take_items = true;
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.need_colored_sprite = true;
		t.update_z = true;
		t.can_be_killed_by_stuff = true;
		t.can_be_killed_by_life_eraser = true;
		t.can_attack_buildings = true;
		t.can_be_moved_by_powers = true;
		t.can_be_hurt_by_powers = true;
		t.effect_damage = true;
		t.can_flip = true;
		t.death_animation_angle = true;
		t.can_be_inspected = true;
		t.can_have_subspecies = true;
		t.use_phenotypes = true;
		t.addResource("meat", 1);
		t.addResource("bones", 1);
		clone("$animal_base$", "$basic_unit_colored$");
		t.build_order_template_id = "build_order_basic";
		t.has_advanced_textures = false;
		t.default_animal = true;
		clone("$animal_fur$", "$animal_base$");
		addPhenotype("savanna", "biome_savanna");
		addPhenotype("dark_teal", "biome_crystal");
		addPhenotype("dark_blue", "biome_crystal");
		addPhenotype("dark_orange", "biome_savanna");
		addPhenotype("swamp", "biome_swamp");
		addPhenotype("skin_blue", "biome_swamp");
		addPhenotype("corrupted", "biome_corrupted");
		addPhenotype("desert", "biome_desert");
		addPhenotype("skin_yellow", "biome_desert");
		addPhenotype("dark_yellow", "biome_desert");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("lemon", "biome_lemon");
		addPhenotype("pink_yellow_mushroom", "biome_mushroom");
		addPhenotype("dark_orange", "biome_sand");
		addPhenotype("wood", "biome_sand");
		addPhenotype("bright_violet", "biome_singularity");
		addPhenotype("mid_gray", "biome_garlic");
		addPhenotype("dark_orange", "biome_maple");
		addPhenotype("polar", "biome_permafrost");
		addPhenotype("gray_black", "biome_rocklands");
		addPhenotype("bright_purple", "biome_celestial");
		addPhenotype("magical", "biome_celestial");
		addPhenotype("magical", "biome_mushroom");
		addPhenotype("dark_purple", "biome_mushroom");
		addPhenotype("skin_pink", "biome_candy");
		addPhenotype("dark_pink", "biome_candy");
		clone("$animal_skin$", "$animal_base$");
		addPhenotype("corrupted", "biome_corrupted");
		addPhenotype("infernal", "biome_infernal");
		addPhenotype("lemon", "biome_lemon");
		clone("$civ_unit$", "$basic_unit_colored$");
		t.render_heads_for_babies = true;
		t.chromosomes_first = AssetLibrary<ActorAsset>.l<string>("chromosome_big", "chromosome_medium");
		t.setCanTurnIntoZombieAsset("zombie", pAutoZombieAsset: true);
		t.addSubspeciesTrait("prefrontal_cortex");
		t.genome_size = 20;
		t.civ = true;
		t.actor_size = ActorSize.S13_Human;
		t.inspect_home = true;
		t.body_separate_part_hands = true;
		t.has_soul = true;
		t.setSocialStructure("group_family", 10, pCreateOnSpawn: true, pFollowHerd: false, FamilyParentsMode.Normal);
		t.name_taxonomic_species = "sapiens";
		t.civ_base_cities = 5;
		t.can_turn_into_demon_in_age_of_chaos = true;
		t.can_turn_into_mush = true;
		t.can_turn_into_ice_one = true;
		t.mush_id = "mush_unit";
		t.can_turn_into_tumor = true;
		t.tumor_id = "tumor_monster_unit";
		t.animation_walk = ActorAnimationSequences.walk_0_3;
		t.animation_swim = ActorAnimationSequences.swim_0_3;
		t.default_attack = "hands";
		t.skeleton_id = "skeleton";
		t.disable_jump_animation = true;
		t.needs_to_be_explored = false;
		clone("$civ_advanced_unit$", "$civ_unit$");
		t.skin_citizen_male = AssetLibrary<ActorAsset>.a<string>("male_1", "male_2", "male_3", "male_4", "male_5", "male_6", "male_7", "male_8", "male_9", "male_10");
		t.skin_citizen_female = AssetLibrary<ActorAsset>.a<string>("female_1", "female_2", "female_3", "female_4", "female_5", "female_6", "female_7", "female_8", "female_9", "female_10");
		t.skin_warrior = AssetLibrary<ActorAsset>.a<string>("warrior_1", "warrior_2", "warrior_3", "warrior_4", "warrior_5", "warrior_6", "warrior_7", "warrior_8", "warrior_9", "warrior_10");
		t.is_humanoid = true;
		clone("$mob_no_genes$", "$basic_unit_colored$");
		t.inspect_children = false;
		t.default_attack = "base_attack";
		t.kingdom_id_civilization = string.Empty;
		t.build_order_template_id = string.Empty;
		t.disable_jump_animation = true;
		t.can_have_subspecies = false;
		clone("$mob$", "$basic_unit_colored$");
		t.default_attack = "base_attack";
		t.can_have_subspecies = true;
		t.disable_jump_animation = true;
		t.can_turn_into_mush = true;
		t.can_turn_into_ice_one = true;
		t.mush_id = "mush_unit";
		t.can_turn_into_tumor = true;
		t.tumor_id = "tumor_monster_unit";
		t.setCanTurnIntoZombieAsset("zombie", pAutoZombieAsset: true);
		clone("$animal$", "$animal_base$");
		t.setCanTurnIntoZombieAsset("zombie_animal", pAutoZombieAsset: true);
		t.can_turn_into_mush = true;
		t.mush_id = "mush_animal";
		t.can_turn_into_tumor = true;
		t.tumor_id = "tumor_monster_animal";
		t.source_meat = true;
		t.default_attack = "jaws";
		clone("$peaceful_animal$", "$animal_base$");
		t.setCanTurnIntoZombieAsset("zombie_animal", pAutoZombieAsset: true);
		t.can_turn_into_mush = true;
		t.mush_id = "mush_animal";
		t.can_turn_into_tumor = true;
		t.tumor_id = "tumor_monster_animal";
		t.base_stats["damage"] = 1f;
		addTrait("peaceful");
		clone("$carnivore$", "$animal$");
		t.addSubspeciesTrait("diet_carnivore");
		clone("$herbivore$", "$animal_base$");
		t.can_turn_into_mush = true;
		t.mush_id = "mush_animal";
		t.setCanTurnIntoZombieAsset("zombie_animal", pAutoZombieAsset: true);
		t.addSubspeciesTrait("diet_herbivore");
		clone("$omnivore$", "$animal$");
		t.addSubspeciesTrait("diet_omnivore");
		clone("$insect$", "$animal_base$");
		t.chromosomes_first = AssetLibrary<ActorAsset>.l<string>("chromosome_tiny");
		t.has_baby_form = false;
		t.has_advanced_textures = false;
		t.setCanTurnIntoZombieAsset("zombie", pAutoZombieAsset: true);
		t.kingdom_id_wild = "insect";
		t.kingdom_id_civilization = "miniciv_insect";
		t.source_meat_insect = true;
		t.actor_size = ActorSize.S0_Bug;
		t.shadow_texture = "unitShadow_2";
		t.color_hex = "#23F3FF";
		t.animation_idle = ActorAnimationSequences.walk_0_2;
		t.animation_walk = ActorAnimationSequences.walk_0_2;
		t.animation_swim = null;
		t.base_stats["speed"] = 5f;
		t.base_stats["health"] = 1f;
		t.base_stats["damage"] = 1f;
		t.base_stats["mass_2"] = 0.015f;
		addTrait("peaceful");
		t.max_random_amount = 5;
		t.addResource("jam", 1, pNewList: true);
		clone("$flying_insect$", "$insect$");
		t.animation_idle = ActorAnimationSequences.walk_0_1;
		t.animation_walk = ActorAnimationSequences.walk_0_1;
		t.animation_swim = null;
		t.disable_jump_animation = true;
		t.move_from_block = true;
		addTrait("weightless");
		t.addSubspeciesTrait("hovering");
		clone("$animal_civ$", "$civ_unit$");
		t.render_heads_for_babies = true;
		t.name_locale = "Greg";
		t.icon = "iconHumans";
		t.setCanTurnIntoZombieAsset("zombie", pAutoZombieAsset: true);
		t.color_hex = "#005E72";
		t.disable_jump_animation = true;
		t.addGenome(("diplomacy", 3f), ("warfare", 3f), ("stewardship", 3f), ("intelligence", 3f));
		t.needs_to_be_explored = true;
		t.is_humanoid = true;
	}
}
