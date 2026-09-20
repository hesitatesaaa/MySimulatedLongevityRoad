using UnityEngine;

public class EffectsLibrary : AssetLibrary<EffectAsset>
{
	public override void init()
	{
		base.init();
		add(new EffectAsset
		{
			id = "fx_spores",
			prefab_id = "effects/prefabs/PrefabSpores",
			show_on_mini_map = true,
			limit = 200
		});
		add(new EffectAsset
		{
			id = "fx_fireball_explosion",
			sprite_path = "effects/fx_fireball_explosion",
			sorting_layer_id = "EffectsTop",
			use_basic_prefab = true,
			draw_light_area = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall"
		});
		add(new EffectAsset
		{
			id = "fx_firebomb_explosion",
			sprite_path = "effects/fx_firebomb_explosion",
			sorting_layer_id = "EffectsTop",
			use_basic_prefab = true,
			draw_light_area = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall"
		});
		add(new EffectAsset
		{
			id = "fx_plasma_ball_explosion",
			sprite_path = "effects/fx_plasma_ball_explosion",
			sorting_layer_id = "EffectsTop",
			use_basic_prefab = true,
			draw_light_area = true
		});
		add(new EffectAsset
		{
			id = "fx_cast_ground_blue",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_cast_ground_blue_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_cast_top_blue",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_cast_top_blue_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_cast_ground_red",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_cast_ground_red_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_cast_top_red",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_cast_top_red_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_cast_ground_green",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_cast_ground_green_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_cast_ground_purple",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_cast_ground_purple_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_cast_top_green",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_cast_top_green_t",
			draw_light_area = true,
			draw_light_size = 0.2f,
			limit = 60
		});
		add(new EffectAsset
		{
			id = "fx_create_skeleton",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_create_skeleton_t",
			draw_light_area = true,
			show_on_mini_map = true,
			limit = 0
		});
		add(new EffectAsset
		{
			id = "fx_teleport_blue",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_teleport_blue_t",
			draw_light_area = true,
			limit = 100
		});
		add(new EffectAsset
		{
			id = "fx_teleport_red",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_teleport_red_t",
			draw_light_area = true,
			limit = 100
		});
		add(new EffectAsset
		{
			id = "fx_shield_hit",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_shield_hit_t",
			draw_light_area = true,
			limit = 200
		});
		add(new EffectAsset
		{
			id = "fx_dodge",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/combat_actions/fx_action_dodge_t",
			limit = 100
		});
		add(new EffectAsset
		{
			id = "fx_block",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/combat_actions/fx_action_block_t",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_drowning",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_status_drowning_t",
			limit = 50
		});
		add(new EffectAsset
		{
			id = "fx_water_splash",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_status_drowning_t",
			limit_unload = true,
			limit = 50
		});
		add(new EffectAsset
		{
			id = "fx_grin_reaper",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_grin_reaper_animation",
			show_on_mini_map = false,
			time_between_frames = 0.001f,
			limit = 20
		});
		add(new EffectAsset
		{
			id = "fx_monolith_launch",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_monolith_launch",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_monolith_launch_bottom",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_monolith_launch_bottom",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_monolith_glow_1",
			use_basic_prefab = true,
			sorting_layer_id = "Objects",
			sprite_path = "effects/fx_monolith_glow_1",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_monolith_glow_2",
			use_basic_prefab = true,
			sorting_layer_id = "Objects",
			sprite_path = "effects/fx_monolith_glow_2",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_waypoint_alien_mold_launch_bottom",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_waypoint_alien_mold_launch_bottom",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_waypoint_computer_launch_bottom",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_waypoint_computer_launch_bottom",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_waypoint_golden_egg_launch_bottom",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_waypoint_golden_egg_launch_bottom",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_waypoint_harp_launch_bottom",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_waypoint_harp_launch_bottom",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_bad_place",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_bad_place_t",
			draw_light_area = true,
			limit = 10,
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_debug_tile",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_debug_tile",
			draw_light_area = true,
			time_between_frames = 3f,
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_move",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsBack",
			sprite_path = "effects/fx_move_t",
			draw_light_area = true,
			limit = 30,
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_plasma_trail",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_plasma_trail_t",
			draw_light_area = true,
			show_on_mini_map = true,
			limit = 15
		});
		add(new EffectAsset
		{
			id = "fx_building_sparkle",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_building_sparkle_t",
			limit = 15
		});
		add(new EffectAsset
		{
			id = "fx_fire_smoke",
			prefab_id = "Prefabs/PrefabFireSmoke",
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_boulder_charge",
			prefab_id = "Prefabs/PrefabBoulderCharge",
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_spark",
			prefab_id = "Prefabs/PrefabSpark",
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_lightning_big",
			prefab_id = "effects/prefabs/PrefabLightning",
			show_on_mini_map = true,
			limit = 100,
			draw_light_area = true,
			draw_light_size = 2f,
			draw_light_area_offset_y = 5f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionLightningStrike"
		});
		add(new EffectAsset
		{
			id = "fx_lightning_medium",
			prefab_id = "effects/prefabs/PrefabLightningMedium",
			show_on_mini_map = true,
			limit = 100,
			draw_light_area = true,
			draw_light_size = 2f,
			draw_light_area_offset_y = 5f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionLightningStrike"
		});
		add(new EffectAsset
		{
			id = "fx_lightning_small",
			prefab_id = "effects/prefabs/PrefabLightningSmall",
			show_on_mini_map = true,
			limit = 100,
			draw_light_area = true,
			draw_light_size = 2f,
			draw_light_area_offset_y = 5f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionLightningStrike"
		});
		add(new EffectAsset
		{
			id = "fx_spawn",
			prefab_id = "effects/prefabs/PrefabSpawnSmall",
			show_on_mini_map = false,
			draw_light_area = true,
			spawn_action = showSpawnEffect,
			limit = 100
		});
		add(new EffectAsset
		{
			id = "fx_teleport_singularity",
			use_basic_prefab = true,
			sorting_layer_id = "EffectsTop",
			sprite_path = "effects/fx_teleport_singularity",
			draw_light_area = true,
			limit = 0
		});
		add(new EffectAsset
		{
			id = "fx_spawn_big",
			prefab_id = "effects/prefabs/PrefabSpawnBig",
			show_on_mini_map = true,
			spawn_action = spawnSimpleTile,
			draw_light_area = true,
			draw_light_size = 2f,
			sound_launch = "event:/SFX/UNIQUE/Crabzilla/CrabzillaSpawn"
		});
		add(new EffectAsset
		{
			id = "fx_land_explosion_old",
			prefab_id = "effects/prefabs/PrefabFireballExplosion",
			show_on_mini_map = true,
			draw_light_area = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_crab_bomb",
			prefab_id = "effects/prefabs/PrefabFireballExplosion",
			show_on_mini_map = true,
			draw_light_area = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionCrabBomb"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_tiny",
			prefab_id = "effects/prefabs/PrefabExplosionSmall",
			show_on_mini_map = true,
			draw_light_area = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionTiny"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_small",
			prefab_id = "effects/prefabs/PrefabExplosionSmall",
			show_on_mini_map = true,
			draw_light_area = true,
			draw_light_size = 1f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_ufo",
			prefab_id = "effects/prefabs/PrefabExplosionSmall",
			show_on_mini_map = true,
			draw_light_area = true,
			draw_light_size = 1f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionUFO"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_meteorite",
			prefab_id = "effects/prefabs/PrefabExplosionSmall",
			show_on_mini_map = true,
			draw_light_area = true,
			draw_light_size = 2f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionMeteorite"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_middle",
			prefab_id = "effects/prefabs/PrefabExplosionSmall",
			show_on_mini_map = true,
			draw_light_area = true,
			draw_light_size = 2f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionSmall"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_nuke_atomic",
			show_on_mini_map = true,
			prefab_id = "effects/prefabs/PrefabExplosionBig",
			draw_light_area = true,
			draw_light_size = 5f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionBig"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_huge",
			show_on_mini_map = true,
			prefab_id = "effects/prefabs/PrefabExplosionBig",
			draw_light_area = true,
			draw_light_size = 5f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionHuge"
		});
		add(new EffectAsset
		{
			id = "fx_explosion_wave",
			prefab_id = "effects/prefabs/PrefabExplosionWave",
			show_on_mini_map = true
		});
		add(new EffectAsset
		{
			id = "fx_fireworks",
			prefab_id = "effects/prefabs/PrefabFireworks",
			show_on_mini_map = true,
			spawn_action = spawnFireworks,
			cooldown_interval = 0.20000000298023224,
			draw_light_area = true,
			draw_light_size = 4f,
			draw_light_area_offset_y = 40f,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionFireworks"
		});
		add(new EffectAsset
		{
			id = "fx_hearts",
			prefab_id = "effects/prefabs/PrefabHearts",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_new_border",
			prefab_id = "effects/prefabs/PrefabNewBorder",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_money_got_loot",
			prefab_id = "effects/prefabs/PrefabMoneyGotLoot",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_money_got_money",
			prefab_id = "effects/prefabs/PrefabMoneyGotMoney",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_money_paid_tax",
			prefab_id = "effects/prefabs/PrefabMoneyPaidTax",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_money_paid_tribute",
			prefab_id = "effects/prefabs/PrefabMoneyPaidTribute",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_money_spend_money",
			prefab_id = "effects/prefabs/PrefabMoneySpendMoney",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_conversion_religion",
			load_texture = true,
			prefab_id = "effects/prefabs/PrefabMetaEvent",
			sprite_path = "effects/fx_conversion_religion",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_conversion_culture",
			load_texture = true,
			prefab_id = "effects/prefabs/PrefabMetaEvent",
			sprite_path = "effects/fx_conversion_culture",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_conversion_language",
			load_texture = true,
			prefab_id = "effects/prefabs/PrefabMetaEvent",
			sprite_path = "effects/fx_conversion_language",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_experience_gain",
			load_texture = true,
			prefab_id = "effects/prefabs/PrefabMetaEvent",
			sprite_path = "effects/fx_experience_gain",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_change_happiness_positive",
			load_texture = true,
			prefab_id = "effects/prefabs/PrefabMetaEvent",
			sprite_path = "effects/fx_change_happiness_positive",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_change_happiness_negative",
			load_texture = true,
			prefab_id = "effects/prefabs/PrefabMetaEvent",
			sprite_path = "effects/fx_change_happiness_negative",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_hmm",
			prefab_id = "effects/prefabs/PrefabHmm",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_plot_progress",
			prefab_id = "effects/prefabs/PrefabPlotProgress",
			sorting_layer_id = "EffectsTop",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_nuke_flash",
			prefab_id = "effects/prefabs/PrefabNukeFlash",
			show_on_mini_map = true,
			draw_light_area = true,
			draw_light_size = 3f,
			spawn_action = spawnNukeFlash
		});
		add(new EffectAsset
		{
			id = "fx_napalm_flash",
			show_on_mini_map = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionMiddle",
			prefab_id = "effects/prefabs/PrefabNapalmFlash",
			draw_light_area = true,
			draw_light_size = 2f,
			spawn_action = spawnNapalmFlash
		});
		add(new EffectAsset
		{
			id = "fx_thunder_flash",
			prefab_id = "effects/prefabs/PrefabThunderFlash",
			limit = 3,
			show_on_mini_map = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionLightningStrike",
			spawn_action = spawnThunderFlash
		});
		add(new EffectAsset
		{
			id = "fx_boulder_impact",
			prefab_id = "effects/prefabs/PrefabBoulderImpact",
			show_on_mini_map = true,
			sound_launch = "event:/SFX/DESTRUCTION/DropSimpleImpact"
		});
		add(new EffectAsset
		{
			id = "fx_boulder_impact_water",
			prefab_id = "effects/prefabs/PrefabBoulderImpactWater",
			show_on_mini_map = true,
			sound_launch = "event:/SFX/DESTRUCTION/DropSimpleImpact"
		});
		add(new EffectAsset
		{
			id = "fx_antimatter_effect",
			prefab_id = "effects/prefabs/PrefabAntimatterEffect",
			show_on_mini_map = true,
			sound_launch = "event:/SFX/EXPLOSIONS/ExplosionAntimatterBomb",
			spawn_action = spawnSimpleTile
		});
		add(new EffectAsset
		{
			id = "fx_infinity_coin",
			show_on_mini_map = true,
			prefab_id = "effects/prefabs/PrefabInfinityCoin",
			spawn_action = spawnSimpleTile,
			draw_light_area = true,
			draw_light_size = 1f,
			sound_launch = "event:/SFX/DESTRUCTION/InfinityCoin"
		});
		add(new EffectAsset
		{
			id = "fx_status_particle",
			prefab_id = "effects/prefabs/PrefabStatusParticle",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_weapon_particle",
			prefab_id = "effects/prefabs/PrefabStatusParticle",
			limit = 50
		});
		add(new EffectAsset
		{
			id = "fx_slash",
			prefab_id = "effects/prefabs/PrefabSlash",
			limit = 40
		});
		add(new EffectAsset
		{
			id = "fx_hit",
			prefab_id = "effects/prefabs/PrefabHit",
			limit = 20
		});
		add(new EffectAsset
		{
			id = "fx_miss",
			prefab_id = "effects/prefabs/PrefabMiss",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_jump",
			sorting_layer_id = "EffectsBack",
			load_texture = true,
			sprite_path = "effects/jump",
			use_basic_prefab = true,
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_walk",
			sorting_layer_id = "EffectsBack",
			load_texture = true,
			sprite_path = "effects/walk",
			limit = 15,
			use_basic_prefab = true,
			cooldown_interval = 0.15000000596046448
		});
		add(new EffectAsset
		{
			id = "fx_hit_critical",
			prefab_id = "effects/prefabs/PrefabHitCritical",
			limit = 10
		});
		add(new EffectAsset
		{
			id = "fx_boat_explosion",
			prefab_id = "effects/prefabs/PrefabBoatExplosion",
			draw_light_area = true,
			limit = 20
		});
		add(new EffectAsset
		{
			id = "fx_fishnet",
			prefab_id = "effects/prefabs/PrefabFishnet",
			limit = 20,
			sound_launch = "event:/SFX/CIVILIZATIONS/SpawnFishnet"
		});
		add(new EffectAsset
		{
			id = "fx_tile_effect",
			prefab_id = "effects/prefabs/PrefabTileEffect",
			limit = 20,
			show_on_mini_map = false,
			spawn_action = spawnSimpleTile
		});
		add(new EffectAsset
		{
			id = "fx_cloud",
			prefab_id = "effects/prefabs/PrefabCloud",
			show_on_mini_map = true,
			limit = 200,
			limit_unload = true,
			spawn_action = spawnCloud
		});
		add(new EffectAsset
		{
			id = "fx_meteorite",
			prefab_id = "effects/prefabs/PrefabMeteorite",
			show_on_mini_map = true,
			spawn_action = spawnMeteorite,
			sound_launch = "event:/SFX/DESTRUCTION/FallMeteorite"
		});
		add(new EffectAsset
		{
			id = "fx_boulder",
			prefab_id = "effects/prefabs/PrefabBoulder",
			show_on_mini_map = true,
			spawn_action = spawnBoulder
		});
		add(new EffectAsset
		{
			id = "fx_santa",
			prefab_id = "effects/prefabs/PrefabSanta",
			show_on_mini_map = true,
			spawn_action = spawnSanta,
			sound_loop_idle = "event:/SFX/OTHER/RoboSanta/RoboSantaIdleLoop",
			limit = 100
		});
		add(new EffectAsset
		{
			id = "fx_zone_highlight",
			prefab_id = "effects/prefabs/PrefabZoneFlash",
			show_on_mini_map = true,
			spawn_action = spawnZoneFlash
		});
		add(new EffectAsset
		{
			id = "fx_tornado",
			prefab_id = "effects/prefabs/PrefabTornado",
			show_on_mini_map = true,
			sound_loop_idle = "event:/SFX/NATURE/TornadoIdleLoop"
		});
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		foreach (EffectAsset item in list)
		{
			if (item.use_basic_prefab || item.load_texture)
			{
				if (item.sorting_layer_id == null)
				{
					BaseAssetLibrary.logAssetError("EffectsLibrary: sorting_layer_id is missing", item.id);
				}
				if (item.sprite_path == null)
				{
					BaseAssetLibrary.logAssetError("EffectsLibrary: sprite_path is missing", item.id);
				}
			}
			if (!item.use_basic_prefab && item.prefab_id == null)
			{
				BaseAssetLibrary.logAssetError("EffectsLibrary: prefab_id is missing", item.id);
			}
		}
	}

	private static BaseEffect check(string pID)
	{
		EffectAsset effectAsset = AssetManager.effects_library.get(pID);
		if (effectAsset == null)
		{
			return null;
		}
		if (effectAsset.cooldown_interval > 0.0 && effectAsset.checkIsUnderCooldown())
		{
			return null;
		}
		if (!effectAsset.show_on_mini_map && MapBox.isRenderMiniMap())
		{
			return null;
		}
		return World.world.stack_effects.get(pID).spawnNew();
	}

	public static BaseEffect spawnAtTileRandomScale(string pID, WorldTile pTile, float pScaleMin, float pScaleMax)
	{
		float pScale = Randy.randomFloat(pScaleMin, pScaleMax);
		return spawnAtTile(pID, pTile, pScale);
	}

	public static void spawnDebugTile(WorldTile pTile, Color pColor)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (pTile != null)
		{
			BaseEffect baseEffect = spawnAtTile("fx_debug_tile", pTile, 0.75f);
			if (!((Object)(object)baseEffect == (Object)null))
			{
				pColor.a = 0.7f;
				baseEffect.sprite_renderer.color = pColor;
			}
		}
	}

	public static BaseEffect spawnAtTile(string pID, WorldTile pTile, float pScale)
	{
		BaseEffect baseEffect = spawn(pID, pTile);
		if ((Object)(object)baseEffect == (Object)null)
		{
			return null;
		}
		baseEffect.prepare(pTile, pScale);
		return baseEffect;
	}

	public static BaseEffect spawnAt(string pID, Vector2 pPos, float pScale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = spawn(pID, null, null, null, 0f, pPos.x, pPos.y);
		if ((Object)(object)baseEffect == (Object)null)
		{
			return null;
		}
		baseEffect.prepare(pPos, pScale);
		return baseEffect;
	}

	public static BaseEffect spawnAt(string pID, Vector3 pPos, float pScale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = spawn(pID, null, null, null, 0f, pPos.x, pPos.y);
		if ((Object)(object)baseEffect == (Object)null)
		{
			return null;
		}
		baseEffect.prepare(Vector2.op_Implicit(pPos), pScale);
		return baseEffect;
	}

	public static BaseEffect spawn(string pID, WorldTile pTile = null, string pParam1 = null, string pParam2 = null, float pFloatParam1 = 0f, float pX = -1f, float pY = -1f, Actor pActor = null)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = check(pID);
		if ((Object)(object)baseEffect == (Object)null)
		{
			return null;
		}
		EffectAsset effectAsset = AssetManager.effects_library.get(pID);
		if (effectAsset.spawn_action != null)
		{
			effectAsset.spawn_action(baseEffect, pTile, pParam1, pParam2, pFloatParam1, pActor);
		}
		if (effectAsset.has_sound_launch)
		{
			float num = pX;
			float num2 = pY;
			if (pTile != null && num == -1f && num2 == -1f)
			{
				num = pTile.x;
				num2 = pTile.y;
			}
			MusicBox.playSound(effectAsset.sound_launch, num, num2);
		}
		if (pX != -1f && pY != -1f)
		{
			((Component)baseEffect).transform.position = new Vector3(pX, pY, 0f);
		}
		if (effectAsset.has_sound_loop_idle)
		{
			baseEffect.fmod_instance = MusicBox.attachToObject(effectAsset.sound_loop_idle, ((Component)baseEffect).gameObject, Object.op_Implicit((Object)(object)baseEffect));
		}
		return baseEffect;
	}

	public static void spawnExplosionWave(Vector3 pVec, float pRadius, float pSpeed = 1f)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = spawn("fx_explosion_wave");
		if (!((Object)(object)baseEffect == (Object)null))
		{
			((ExplosionFlash)baseEffect).start(pVec, pRadius, pSpeed);
		}
	}

	public static bool canShowSlashEffect()
	{
		return !World.world.stack_effects.controller_slash_effects.isLimitReached();
	}

	public static void spawnSlash(Vector2 pVec, string pPathSprites, float pAngle, float pScaleMod = 0.1f)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = spawn("fx_slash");
		if (!((Object)(object)baseEffect == (Object)null))
		{
			baseEffect.prepare(pVec, pScaleMod);
			SpriteAnimation component = ((Component)baseEffect).GetComponent<SpriteAnimation>();
			Sprite[] spriteList = SpriteTextureLoader.getSpriteList(pPathSprites);
			component.setFrames(spriteList);
			((Component)baseEffect).transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, pAngle));
		}
	}

	public BaseEffect spawnMeteorite(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((Meteorite)pEffect).spawnOn(pTile, pParam1, pActor);
		return pEffect;
	}

	public BaseEffect spawnSanta(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((Santa)pEffect).spawnOn(pTile);
		return pEffect;
	}

	public BaseEffect spawnBoulder(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Boulder obj = (Boulder)pEffect;
		Vector2 mousePos = World.world.getMousePos();
		obj.spawnOn(mousePos);
		return pEffect;
	}

	public BaseEffect spawnNapalmFlash(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((NapalmFlash)pEffect).spawnFlash(pTile);
		return pEffect;
	}

	public BaseEffect spawnNukeFlash(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((NukeFlash)pEffect).spawnFlash(pTile, pParam1);
		return pEffect;
	}

	public BaseEffect spawnThunderFlash(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((ThunderFlash)pEffect).spawnFlash();
		return pEffect;
	}

	public BaseEffect spawnSimpleTile(BaseEffect pEffect, WorldTile pTile, string pParam1 = null, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		pEffect.spawnOnTile(pTile);
		return pEffect;
	}

	public BaseEffect spawnZoneFlash(BaseEffect pEffect, WorldTile pTile, string pParam1 = null, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((ZoneFlash)pEffect).spawnOnTile(pTile);
		return pEffect;
	}

	public BaseEffect spawnCloud(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((Cloud)pEffect).spawn(pTile, pParam1);
		return pEffect;
	}

	public BaseEffect spawnFireworks(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		((Fireworks)pEffect).spawnOnTile(pTile);
		return pEffect;
	}

	public BaseEffect showSpawnEffect(BaseEffect pEffect, WorldTile pTile, string pParam1, string pParam2 = null, float pFloatParam1 = 0f, Actor pActor = null)
	{
		pEffect.prepare(pTile, 0.2f);
		return pEffect;
	}

	public BaseEffect spawnStatusParticle(BaseEffect pEffect, Vector3 pPos)
	{
		return pEffect;
	}

	public static void highlightKingdomZones(Kingdom pKingdom, Color pColor, float pAlpha = 0.3f)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		foreach (City city in pKingdom.getCities())
		{
			foreach (TileZone zone in city.zones)
			{
				((ZoneFlash)spawn("fx_zone_highlight", zone.centerTile, null, null, pAlpha)).start(pColor, pAlpha);
			}
		}
	}

	public static void showMoneyEffect(string pID, Vector2 pPosition, TileZone pZone, float pScale)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (pZone.visible_main_centered && PlayerConfig.optionBoolEnabled("money_flow"))
		{
			float x = pPosition.x + Randy.randomFloat(-0.3f, 0.3f);
			pPosition.x = x;
			spawnAt(pID, pPosition, pScale);
		}
	}

	public static void showMetaEventEffectConversion(string pID, Actor pActor)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerConfig.optionBoolEnabled("meta_conversions"))
		{
			showMetaEventEffect(pID, pActor.current_position, pActor.current_zone, pActor.actor_scale);
		}
	}

	public static void showMetaEventEffect(string pID, Actor pActor)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		showMetaEventEffect(pID, pActor.current_position, pActor.current_zone, pActor.actor_scale);
	}

	public static void showMetaEventEffect(string pID, Vector2 pPosition, TileZone pZone, float pScale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (pZone.visible_main_centered)
		{
			float x = pPosition.x + Randy.randomFloat(-0.3f, 0.3f);
			pPosition.x = x;
			spawnAt(pID, pPosition, pScale);
		}
	}
}
