using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPools;

public class DropsLibrary : AssetLibrary<DropAsset>
{
	private const string TEMPLATE_BIOME_SEEDS = "$biome_seeds$";

	private const string TEMPLATE_SPAWN_BUILDING = "$spawn_building$";

	private const string TEMPLATE_SPAWN_MINERAL = "$spawn_mineral$";

	private const string TEMPLATE_SPAWN_CREEP = "$spawn_creep$";

	private static HashSet<TileZone> _paint_zones_hashset = new HashSet<TileZone>();

	public override void init()
	{
		//IL_0d87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eaf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f50: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f55: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1057: Unknown result type (might be due to invalid IL or missing references)
		//IL_105c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1061: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_10fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_1182: Unknown result type (might be due to invalid IL or missing references)
		//IL_1187: Unknown result type (might be due to invalid IL or missing references)
		//IL_118c: Unknown result type (might be due to invalid IL or missing references)
		//IL_121b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1220: Unknown result type (might be due to invalid IL or missing references)
		//IL_1225: Unknown result type (might be due to invalid IL or missing references)
		//IL_1294: Unknown result type (might be due to invalid IL or missing references)
		//IL_1299: Unknown result type (might be due to invalid IL or missing references)
		//IL_129e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1311: Unknown result type (might be due to invalid IL or missing references)
		//IL_1316: Unknown result type (might be due to invalid IL or missing references)
		//IL_131b: Unknown result type (might be due to invalid IL or missing references)
		//IL_138e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1393: Unknown result type (might be due to invalid IL or missing references)
		//IL_1398: Unknown result type (might be due to invalid IL or missing references)
		//IL_140b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1410: Unknown result type (might be due to invalid IL or missing references)
		//IL_1415: Unknown result type (might be due to invalid IL or missing references)
		//IL_1488: Unknown result type (might be due to invalid IL or missing references)
		//IL_148d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1492: Unknown result type (might be due to invalid IL or missing references)
		//IL_1505: Unknown result type (might be due to invalid IL or missing references)
		//IL_150a: Unknown result type (might be due to invalid IL or missing references)
		//IL_150f: Unknown result type (might be due to invalid IL or missing references)
		//IL_158d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1592: Unknown result type (might be due to invalid IL or missing references)
		//IL_1597: Unknown result type (might be due to invalid IL or missing references)
		//IL_1640: Unknown result type (might be due to invalid IL or missing references)
		//IL_1645: Unknown result type (might be due to invalid IL or missing references)
		//IL_164a: Unknown result type (might be due to invalid IL or missing references)
		//IL_16cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_175e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1763: Unknown result type (might be due to invalid IL or missing references)
		//IL_1768: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_17f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_17f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_187c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1881: Unknown result type (might be due to invalid IL or missing references)
		//IL_1886: Unknown result type (might be due to invalid IL or missing references)
		//IL_1901: Unknown result type (might be due to invalid IL or missing references)
		//IL_1906: Unknown result type (might be due to invalid IL or missing references)
		//IL_190b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1989: Unknown result type (might be due to invalid IL or missing references)
		//IL_198e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1993: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a65: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6f: Unknown result type (might be due to invalid IL or missing references)
		base.init();
		add(new DropAsset
		{
			id = "paint",
			path_texture = "drops/drop_paint",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_paint,
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "dust_black",
			path_texture = "drops/drop_dust_black",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		DropAsset dropAsset = t;
		dropAsset.action_landed = (DropsAction)Delegate.Combine(dropAsset.action_landed, new DropsAction(action_dust_white));
		DropAsset dropAsset2 = t;
		dropAsset2.action_landed = (DropsAction)Delegate.Combine(dropAsset2.action_landed, new DropsAction(action_dust_red));
		DropAsset dropAsset3 = t;
		dropAsset3.action_landed = (DropsAction)Delegate.Combine(dropAsset3.action_landed, new DropsAction(action_dust_blue));
		DropAsset dropAsset4 = t;
		dropAsset4.action_landed = (DropsAction)Delegate.Combine(dropAsset4.action_landed, new DropsAction(action_dust_gold));
		DropAsset dropAsset5 = t;
		dropAsset5.action_landed = (DropsAction)Delegate.Combine(dropAsset5.action_landed, new DropsAction(action_dust_purple));
		DropAsset dropAsset6 = t;
		dropAsset6.action_landed = (DropsAction)Delegate.Combine(dropAsset6.action_landed, new DropsAction(action_dust_black));
		add(new DropAsset
		{
			id = "dust_white",
			path_texture = "drops/drop_dust_white",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_dust_white,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "dust_red",
			path_texture = "drops/drop_dust_red",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_dust_red,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "dust_blue",
			path_texture = "drops/drop_dust_blue",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_dust_blue,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "dust_gold",
			path_texture = "drops/drop_dust_gold",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_dust_gold,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "dust_purple",
			path_texture = "drops/drop_dust_purple",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_dust_purple,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "gamma_rain",
			path_texture = "drops/drop_gamma_rain",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_gamma_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropRainGamma",
			type = DropType.DropTraitRain
		});
		add(new DropAsset
		{
			id = "delta_rain",
			path_texture = "drops/drop_delta_rain",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_delta_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropRainDelta",
			type = DropType.DropTraitRain
		});
		add(new DropAsset
		{
			id = "omega_rain",
			path_texture = "drops/drop_omega_rain",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_omega_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropRainOmeaga",
			type = DropType.DropTraitRain
		});
		add(new DropAsset
		{
			id = "loot_rain",
			path_texture = "drops/drop_loot_rain",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_equipment_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropRainGamma",
			type = DropType.DropEquipmentRain
		});
		add(new DropAsset
		{
			id = "tnt",
			animated = true,
			path_texture = "drops/drop_tnt",
			animation_speed = 0.03f,
			default_scale = 0.2f,
			action_landed = action_tnt,
			sound_drop = "event:/SFX/DROPS/DropTnt",
			type = DropType.DropTile
		});
		add(new DropAsset
		{
			id = "tnt_timed",
			path_texture = "drops/drop_tnttimed",
			default_scale = 0.2f,
			action_landed = action_tnt_timed,
			sound_drop = "event:/SFX/DROPS/DropTnt",
			type = DropType.DropTile
		});
		add(new DropAsset
		{
			id = "water_bomb",
			path_texture = "drops/drop_waterbomb",
			default_scale = 0.2f,
			action_landed = action_water_bomb,
			sound_drop = "event:/SFX/DROPS/DropWaterBomb",
			type = DropType.DropTile
		});
		add(new DropAsset
		{
			id = "landmine",
			path_texture = "drops/drop_landmine",
			default_scale = 0.2f,
			action_landed = action_landmine,
			sound_drop = "event:/SFX/DROPS/DropLandmine",
			type = DropType.DropTile
		});
		add(new DropAsset
		{
			id = "fireworks",
			path_texture = "drops/drop_fireworks",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_fireworks,
			sound_drop = "event:/SFX/DROPS/DropFireworks",
			type = DropType.DropTile
		});
		add(new DropAsset
		{
			id = "inspiration",
			path_texture = "drops/drop_inspiration",
			default_scale = 0.2f,
			action_landed = action_inspiration,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropInspiration",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "discord",
			path_texture = "drops/drop_discord",
			default_scale = 0.2f,
			action_landed = action_discord,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropInspiration",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "friendship",
			path_texture = "drops/drop_friendship",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_friendship,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropFriendship",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "spite",
			path_texture = "drops/drop_spite",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_spite,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropSpite",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "madness",
			path_texture = "drops/drop_madness",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_madness,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropMadness",
			type = DropType.DropTrait
		});
		add(new DropAsset
		{
			id = "blessing",
			path_texture = "drops/drop_blessing",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.1f,
			action_landed = action_blessing,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		DropAsset dropAsset7 = t;
		dropAsset7.action_landed = (DropsAction)Delegate.Combine(dropAsset7.action_landed, new DropsAction(ActionLibrary.action_shrinkTornadoes));
		add(new DropAsset
		{
			id = "shield",
			path_texture = "drops/drop_shield",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_shield,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropShield",
			type = DropType.DropStatus
		});
		add(new DropAsset
		{
			id = "coffee",
			path_texture = "drops/drop_coffee",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_coffee,
			sound_drop = "event:/SFX/DROPS/DropCoffee",
			type = DropType.DropStatus
		});
		add(new DropAsset
		{
			id = "powerup",
			path_texture = "drops/drop_mushroom_powerup",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_powerup,
			sound_drop = "event:/SFX/DROPS/DropPowerup",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "curse",
			path_texture = "drops/drop_curse",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_curse,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropCurse",
			type = DropType.DropTrait
		});
		DropAsset dropAsset8 = t;
		dropAsset8.action_landed = (DropsAction)Delegate.Combine(dropAsset8.action_landed, new DropsAction(ActionLibrary.action_growTornadoes));
		add(new DropAsset
		{
			id = "spell_silence",
			path_texture = "drops/drop_spell_silence",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_spell_silence,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropCurse",
			type = DropType.DropTrait
		});
		add(new DropAsset
		{
			id = "zombie_infection",
			path_texture = "drops/drop_zombieinfection",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_zombie_infection,
			sound_drop = "event:/SFX/DROPS/DropZombieInfection",
			type = DropType.DropTrait
		});
		add(new DropAsset
		{
			id = "mush_spores",
			path_texture = "drops/drop_mushSpores",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_mush_spore,
			sound_drop = "event:/SFX/DROPS/DropMushSpores",
			type = DropType.DropTrait
		});
		add(new DropAsset
		{
			id = "plague",
			path_texture = "drops/drop_plague",
			random_frame = true,
			default_scale = 0.1f,
			action_landed = action_plague,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropPlague",
			type = DropType.DropTrait
		});
		add(new DropAsset
		{
			id = "living_plants",
			path_texture = "drops/drop_blessing",
			animated = true,
			default_scale = 0.1f,
			action_landed = action_living_plants,
			sound_drop = "event:/SFX/DROPS/DropLivingPlants",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "living_house",
			path_texture = "drops/drop_blessing",
			animated = true,
			default_scale = 0.1f,
			action_landed = action_living_house,
			sound_drop = "event:/SFX/DROPS/DropLivingHouse",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "bomb",
			path_texture = "drops/drop_bomb",
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			sound_launch = "event:/SFX/DROPS/DropLaunchBombSmall",
			action_landed = action_bomb,
			type = DropType.DropBomb
		});
		DropAsset dropAsset9 = t;
		dropAsset9.action_launch = (DropsAction)Delegate.Combine(dropAsset9.action_launch, new DropsAction(ActionLibrary.increaseDroppedBombsCounter));
		add(new DropAsset
		{
			id = "grenade",
			path_texture = "drops/drop_grenade",
			animated = true,
			default_scale = 0.2f,
			animation_speed = 0.03f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_grenade,
			random_flip = true,
			sound_launch = "event:/SFX/DROPS/DropLaunchGrenade",
			type = DropType.DropBomb
		});
		add(new DropAsset
		{
			id = "crab_bomb",
			path_texture = "drops/drop_crab_bomb_parachute",
			animated = true,
			default_scale = 0.1f,
			animation_speed = 0.05f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_crab_bomb_impact,
			random_flip = true,
			sound_launch = "event:/SFX/DROPS/DropLaunchCrabBomb",
			type = DropType.DropBomb
		});
		add(new DropAsset
		{
			id = "crab_bomb_shrapnel",
			path_texture = "drops/drop_crab_bomb_shrapnel",
			animated = true,
			animation_rotation = true,
			animation_rotation_speed_min = 50f,
			animation_rotation_speed_max = 200f,
			default_scale = 0.175f,
			animation_speed = 0.05f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_crab_bomb_shrapnel,
			random_flip = true,
			sound_launch = "event:/SFX/DROPS/DropLaunchCrabBomb",
			type = DropType.DropBomb,
			surprises_units = true
		});
		add(new DropAsset
		{
			id = "napalm_bomb",
			path_texture = "drops/drop_napalmbomb",
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_napalm_bomb,
			random_flip = true,
			type = DropType.DropBomb
		});
		DropAsset dropAsset10 = t;
		dropAsset10.action_launch = (DropsAction)Delegate.Combine(dropAsset10.action_launch, new DropsAction(ActionLibrary.increaseDroppedBombsCounter));
		add(new DropAsset
		{
			id = "atomic_bomb",
			path_texture = "drops/drop_atomicbomb",
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_atomic_bomb,
			random_flip = true,
			sound_launch = "event:/SFX/DROPS/DropLaunchGrenadeHuge",
			type = DropType.DropBomb
		});
		DropAsset dropAsset11 = t;
		dropAsset11.action_launch = (DropsAction)Delegate.Combine(dropAsset11.action_launch, new DropsAction(ActionLibrary.increaseDroppedBombsCounter));
		add(new DropAsset
		{
			id = "antimatter_bomb",
			path_texture = "drops/drop_antimatterbomb",
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_antimatter_bomb,
			sound_launch = "event:/SFX/DROPS/DropLaunchGrenadeHuge",
			type = DropType.DropBomb
		});
		DropAsset dropAsset12 = t;
		dropAsset12.action_launch = (DropsAction)Delegate.Combine(dropAsset12.action_launch, new DropsAction(ActionLibrary.increaseDroppedBombsCounter));
		add(new DropAsset
		{
			id = "czar_bomba",
			path_texture = "drops/drop_czarbomba",
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(60f, 70f)),
			action_landed = action_czar_bomba,
			sound_launch = "event:/SFX/DROPS/DropLaunchGrenadeHuge",
			type = DropType.DropBomb
		});
		DropAsset dropAsset13 = t;
		dropAsset13.action_launch = (DropsAction)Delegate.Combine(dropAsset13.action_launch, new DropsAction(ActionLibrary.increaseDroppedBombsCounter));
		add(new DropAsset
		{
			id = "rain",
			path_texture = "drops/drop_rain",
			random_frame = true,
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_rain,
			sound_drop = "event:/SFX/DROPS/DropRain",
			type = DropType.DropGeneric,
			surprises_units = false
		});
		add(new DropAsset
		{
			id = "blood_rain",
			path_texture = "drops/drop_blood",
			random_frame = true,
			default_scale = 0.1f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed_drop = action_blood_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBloodRain",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "clone_rain",
			path_texture = "drops/drop_clone",
			random_frame = true,
			default_scale = 0.1f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_clone_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBloodRain",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "jazz",
			path_texture = "drops/drop_jazz",
			random_frame = true,
			default_scale = 0.1f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_jazz_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBloodRain",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "dispel",
			path_texture = "drops/drop_dispel",
			random_frame = true,
			default_scale = 0.1f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_dispel_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBloodRain",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "sleep",
			path_texture = "drops/drop_sleep",
			random_frame = true,
			default_scale = 0.1f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_sleep_rain,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBloodRain",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "cure",
			path_texture = "drops/drop_cure",
			random_frame = true,
			default_scale = 0.1f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_cure,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropCure",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "fire",
			path_texture = "drops/drop_fire",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			falling_random_x_move = true,
			particle_interval = 0.3f,
			action_landed = action_fire,
			animation_speed_random = 0.08f,
			random_frame = true,
			random_flip = true,
			sound_drop = "event:/SFX/DROPS/DropFire",
			material = "mat_world_object_lit",
			type = DropType.DropGeneric
		});
		add(new DropAsset
		{
			id = "snow",
			path_texture = "drops/drop_snow",
			random_frame = true,
			default_scale = 0.2f,
			falling_speed = 0.3f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			falling_random_x_move = true,
			particle_interval = 0.15f,
			sound_drop = "event:/SFX/DROPS/DropSnow",
			action_landed = action_snow,
			type = DropType.DropGeneric
		});
		add(new DropAsset
		{
			id = "life_seed",
			path_texture = "drops/drop_life_seed",
			random_frame = true,
			default_scale = 0.2f,
			falling_speed = 0.3f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			falling_random_x_move = true,
			particle_interval = 0.15f,
			sound_drop = "event:/SFX/DROPS/DropSeedGrass",
			action_landed = action_life_seed,
			type = DropType.DropGeneric
		});
		add(new DropAsset
		{
			id = "ash",
			path_texture = "drops/drop_ash",
			random_frame = true,
			default_scale = 0.2f,
			falling_speed = 0.3f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			falling_random_x_move = true,
			particle_interval = 0.15f,
			sound_drop = "event:/SFX/DROPS/DropAsh",
			action_landed = action_ash,
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "magic_rain",
			path_texture = "drops/drop_magic_rain",
			random_frame = true,
			default_scale = 0.2f,
			falling_speed = 0.3f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			falling_random_x_move = true,
			particle_interval = 0.15f,
			sound_drop = "event:/SFX/DROPS/DropMagicRain",
			action_landed = action_magic_rain,
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "rage",
			path_texture = "drops/drop_rage",
			random_frame = true,
			default_scale = 0.2f,
			falling_speed = 0.3f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			falling_random_x_move = true,
			particle_interval = 0.15f,
			sound_drop = "event:/SFX/DROPS/DropRage",
			action_landed = action_rage,
			type = DropType.DropStatus
		});
		add(new DropAsset
		{
			id = "acid",
			path_texture = "drops/drop_acid",
			random_frame = true,
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_acid,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropAcid",
			type = DropType.DropMagic
		});
		add(new DropAsset
		{
			id = "lava",
			path_texture = "drops/drop_lava",
			animated = true,
			animation_speed = 0.03f,
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(30f, 45f)),
			action_landed = action_lava,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropLava",
			type = DropType.DropGeneric
		});
		add(new DropAsset
		{
			id = "santa_bomb",
			path_texture = "drops/drop_santabomb",
			random_frame = true,
			default_scale = 0.2f,
			sound_launch = "event:/SFX/DROPS/DropLaunchSantaBomb",
			action_landed = action_santa_bomb,
			type = DropType.DropBomb,
			surprises_units = true
		});
		add(new DropAsset
		{
			id = "$spawn_building$",
			path_texture = "drops/drop_stone",
			random_frame = true,
			default_scale = 0.2f,
			falling_height = Vector2.op_Implicit(new Vector2(10f, 15f)),
			falling_speed = 5f,
			type = DropType.DropBuilding
		});
		t.action_landed = action_spawn_building;
		clone("$biome_seeds$", "$spawn_building$");
		t.type = DropType.DropSeed;
		t.action_landed = null;
		clone("seeds_grass", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_grass";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "grass_low";
		t.drop_type_high = "grass_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_enchanted", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_enchanted";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "enchanted_low";
		t.drop_type_high = "enchanted_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedEnchanted";
		clone("seeds_savanna", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_savanna";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "savanna_low";
		t.drop_type_high = "savanna_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedSavanna";
		clone("seeds_corrupted", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_corrupted";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "corrupted_low";
		t.drop_type_high = "corrupted_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedCorrupted";
		clone("seeds_mushroom", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_mushroom";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "mushroom_low";
		t.drop_type_high = "mushroom_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedMushroom";
		clone("seeds_jungle", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_jungle";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "jungle_low";
		t.drop_type_high = "jungle_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedJungle";
		clone("seeds_desert", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_desert";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "desert_low";
		t.drop_type_high = "desert_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedDesert";
		clone("seeds_lemon", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_lemon";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "lemon_low";
		t.drop_type_high = "lemon_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedLemon";
		clone("seeds_permafrost", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_permafrost";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "permafrost_low";
		t.drop_type_high = "permafrost_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedPermafrost";
		clone("seeds_candy", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_candy";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "candy_low";
		t.drop_type_high = "candy_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedCandy";
		clone("seeds_crystal", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_crystal";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "crystal_low";
		t.drop_type_high = "crystal_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedCrystal";
		clone("seeds_swamp", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_swamp";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "swamp_low";
		t.drop_type_high = "swamp_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedSwamp";
		clone("seeds_infernal", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_infernal";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "infernal_low";
		t.drop_type_high = "infernal_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedInfernal";
		clone("seeds_birch", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_birch";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "birch_low";
		t.drop_type_high = "birch_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_maple", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_maple";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "maple_low";
		t.drop_type_high = "maple_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_rocklands", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_rocklands";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "rocklands_low";
		t.drop_type_high = "rocklands_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_garlic", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_garlic";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "garlic_low";
		t.drop_type_high = "garlic_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_flower", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_flower";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "flower_low";
		t.drop_type_high = "flower_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_celestial", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_celestial";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "celestial_low";
		t.drop_type_high = "celestial_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_singularity", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_singularity";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "singularity_low";
		t.drop_type_high = "singularity_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_clover", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_clover";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "clover_low";
		t.drop_type_high = "clover_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("seeds_paradox", "$biome_seeds$");
		t.path_texture = "drops/drop_seed_paradox";
		t.falling_speed = 3f;
		t.action_landed = action_drop_seeds;
		t.drop_type_low = "paradox_low";
		t.drop_type_high = "paradox_high";
		t.sound_drop = "event:/SFX/DROPS/DropSeedGrass";
		clone("fruit_bush", "$spawn_building$");
		t.path_texture = "drops/drop_seed";
		t.falling_speed = 3f;
		t.action_landed = action_fruit_bush;
		t.sound_drop = "event:/SFX/DROPS/DropBush";
		clone("fertilizer_plants", "$biome_seeds$");
		t.surprises_units = false;
		t.path_texture = "drops/drop_fertilizer";
		t.falling_speed = 5f;
		DropAsset dropAsset14 = t;
		dropAsset14.action_landed = (DropsAction)Delegate.Combine(dropAsset14.action_landed, new DropsAction(action_fertilizer_plants));
		DropAsset dropAsset15 = t;
		dropAsset15.action_landed = (DropsAction)Delegate.Combine(dropAsset15.action_landed, new DropsAction(tryToGrowWheat));
		DropAsset dropAsset16 = t;
		dropAsset16.action_landed = (DropsAction)Delegate.Combine(dropAsset16.action_landed, new DropsAction(flash));
		t.sound_drop = "event:/SFX/DROPS/DropFertilizerPlants";
		clone("fertilizer_trees", "$biome_seeds$");
		t.path_texture = "drops/drop_fertilizer";
		t.falling_speed = 5f;
		t.action_landed = action_fertilizer_trees;
		DropAsset dropAsset17 = t;
		dropAsset17.action_landed = (DropsAction)Delegate.Combine(dropAsset17.action_landed, new DropsAction(flash));
		t.sound_drop = "event:/SFX/DROPS/DropFertilizerPlants";
		clone("$spawn_mineral$", "$spawn_building$");
		t.falling_speed = 6f;
		t.type = DropType.DropMineral;
		clone("stone", "$spawn_mineral$");
		t.path_texture = "drops/drop_stone";
		t.default_scale = 0.2f;
		t.building_asset = "mineral_stone";
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropStone";
		clone("metals", "$spawn_mineral$");
		t.path_texture = "drops/drop_metal";
		t.default_scale = 0.2f;
		t.building_asset = "mineral_metals";
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropMineral";
		clone("gold", "$spawn_mineral$");
		t.path_texture = "drops/drop_gold";
		t.default_scale = 0.2f;
		t.building_asset = "mineral_gold";
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropGold";
		clone("silver", "$spawn_mineral$");
		t.path_texture = "drops/drop_stone";
		t.default_scale = 0.2f;
		t.building_asset = "mineral_silver";
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropMineral";
		clone("mythril", "$spawn_mineral$");
		t.path_texture = "drops/drop_stone";
		t.default_scale = 0.2f;
		t.building_asset = "mineral_mythril";
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropMineral";
		clone("adamantine", "$spawn_mineral$");
		t.path_texture = "drops/drop_stone";
		t.default_scale = 0.2f;
		t.building_asset = "mineral_adamantine";
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropMineral";
		clone("$spawn_creep$", "$spawn_building$");
		t.type = DropType.DropCreep;
		clone("tumor", "$spawn_creep$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropTumor";
		clone("biomass", "$spawn_creep$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropBiomass";
		clone("cybercore", "$spawn_creep$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropCybercore";
		clone("super_pumpkin", "$spawn_creep$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropSuperPumpkin";
		clone("geyser", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropGeyser";
		clone("geyser_acid", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropGeyser";
		clone("volcano", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropVolcano";
		clone("golden_brain", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropGoldenBrain";
		clone("monolith", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		DropAsset dropAsset18 = t;
		dropAsset18.action_landed = (DropsAction)Delegate.Combine(dropAsset18.action_landed, (DropsAction)delegate
		{
			AchievementLibrary.cant_be_too_much.checkBySignal();
		});
		t.sound_drop = "event:/SFX/DROPS/DropMonolith";
		clone("corrupted_brain", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropCorruptedBrain";
		clone("ice_tower", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropIceTower";
		clone("angle_tower", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropIceTower";
		clone("beehive", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropBeehive";
		clone("flame_tower", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropFlameTower";
		addWaypointDrops();
	}

	public override void linkAssets()
	{
		foreach (DropAsset item in list)
		{
			if (!string.IsNullOrEmpty(item.drop_type_high))
			{
				item.cached_drop_type_high = AssetManager.top_tiles.get(item.drop_type_high);
			}
			if (!string.IsNullOrEmpty(item.drop_type_low))
			{
				item.cached_drop_type_low = AssetManager.top_tiles.get(item.drop_type_low);
			}
		}
		base.linkAssets();
	}

	private void addWaypointDrops()
	{
		add(new DropAsset
		{
			id = "desire_alien_mold",
			path_texture = "drops/drop_alien_mold",
			animated = false,
			default_scale = 0.1f,
			material = "mat_world_object_lit",
			sound_drop = "event:/SFX/DROPS/DropBlessing",
			type = DropType.DropMagic
		});
		t.action_landed = action_alien_mold;
		t.surprises_units = true;
		clone("desire_computer", "desire_alien_mold");
		t.path_texture = "drops/drop_computer";
		t.action_landed = action_drop_computer;
		clone("desire_golden_egg", "desire_alien_mold");
		t.path_texture = "drops/drop_golden_egg";
		t.action_landed = action_drop_golden_egg;
		clone("desire_harp", "desire_alien_mold");
		t.path_texture = "drops/drop_harp";
		t.action_landed = action_drop_harp;
		clone("waypoint_alien_mold", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropCorruptedBrain";
		clone("waypoint_computer", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropCorruptedBrain";
		clone("waypoint_golden_egg", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropCorruptedBrain";
		clone("waypoint_harp", "$spawn_building$");
		t.building_asset = t.id;
		t.action_landed = action_spawn_building;
		t.sound_drop = "event:/SFX/DROPS/DropCorruptedBrain";
	}

	public static void action_drop_seeds(WorldTile pTile = null, string pDropID = null)
	{
		DropAsset dropAsset = AssetManager.drops.get(pDropID);
		useDropSeedOn(pTile, dropAsset.cached_drop_type_low, dropAsset.cached_drop_type_high);
	}

	public static void useDropSeedOn(WorldTile pTile, TopTileType pTypeLow, TopTileType pHigh)
	{
		useSeedOn(pTile, pTypeLow, pHigh);
		for (int i = 0; i < pTile.neighbours.Length; i++)
		{
			useSeedOn(pTile.neighbours[i], pTypeLow, pHigh);
		}
	}

	public static void tryToGrowWheat(WorldTile pTile = null, string pDropID = null)
	{
		if (pTile.Type.farm_field && !pTile.hasBuilding())
		{
			World.world.buildings.addBuilding("wheat", pTile);
		}
	}

	public static void useSeedOn(WorldTile pTile, TopTileType pTypeLow, TopTileType pHigh)
	{
		pTile.unfreeze();
		if (!pTile.Type.can_be_biome)
		{
			return;
		}
		if (pTile.isTileRank(TileRank.Low))
		{
			MapAction.growGreens(pTile, pTypeLow);
		}
		else if (pTile.isTileRank(TileRank.High))
		{
			MapAction.growGreens(pTile, pHigh);
		}
		BiomeAsset tBiome = pTile.getBiome();
		if (tBiome == null)
		{
			return;
		}
		pTile.doUnits(delegate(Actor tActor)
		{
			if (!tActor.hasSubspecies() || tBiome.spawn_trait_subspecies_always == null)
			{
				return;
			}
			foreach (string spawn_trait_subspecies_alway in tBiome.spawn_trait_subspecies_always)
			{
				tActor.subspecies.addTrait(spawn_trait_subspecies_alway);
			}
		});
	}

	public static void action_rain(WorldTile pTile = null, string pDropID = null)
	{
		useRainOn(pTile);
		for (int i = 0; i < pTile.neighbours.Length; i++)
		{
			useRainOn(pTile.neighbours[i]);
		}
		for (int j = 0; j < pTile.neighbours.Length; j++)
		{
			WorldTile worldTile = pTile.neighbours[j];
			if (worldTile.isOnFire())
			{
				worldTile.stopFire();
			}
		}
	}

	private static void useRainOn(WorldTile pTile)
	{
		pTile.stopFire();
		pTile.doUnits(delegate(Actor tActor)
		{
			tActor.finishStatusEffect("burning");
			tActor.finishAngryStatus();
			if (tActor.isDamagedByRain())
			{
				tActor.getHit(tActor.getWaterDamage(), pFlash: true, AttackType.Water);
			}
			else
			{
				tActor.addStamina((int)((float)tActor.getMaxStamina() * 0.1f));
			}
		});
		if (pTile.hasBuilding())
		{
			pTile.building.stopFire();
			if (pTile.building.asset.wheat)
			{
				pTile.building.component_wheat.grow();
			}
		}
		if (pTile.hasBuilding() && pTile.building.asset.damaged_by_rain)
		{
			pTile.building.getHit(20f);
		}
		pTile.removeBurn();
		if (pTile.Type.can_be_filled_with_ocean)
		{
			MapAction.setOcean(pTile);
		}
		if (pTile.Type.lava)
		{
			LavaHelper.putOut(pTile);
		}
		if (pTile.Type.explodable_by_ocean)
		{
			World.world.explosion_layer.explodeBomb(pTile);
		}
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_water)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile);
		}
	}

	public static void action_gamma_rain(WorldTile pTile = null, string pDropID = null)
	{
		List<string> trait_editor_gamma = PlayerConfig.instance.data.trait_editor_gamma;
		useTraitRain(pTile, trait_editor_gamma, PlayerConfig.instance.data.trait_editor_gamma_state);
	}

	public static void action_delta_rain(WorldTile pTile = null, string pDropID = null)
	{
		List<string> trait_editor_delta = PlayerConfig.instance.data.trait_editor_delta;
		useTraitRain(pTile, trait_editor_delta, PlayerConfig.instance.data.trait_editor_delta_state);
	}

	public static void action_omega_rain(WorldTile pTile = null, string pDropID = null)
	{
		List<string> trait_editor_omega = PlayerConfig.instance.data.trait_editor_omega;
		useTraitRain(pTile, trait_editor_omega, PlayerConfig.instance.data.trait_editor_omega_state);
	}

	private static void useTraitRain(WorldTile pTile, List<string> pList, RainState pRainState)
	{
		if (pList.Count == 0)
		{
			return;
		}
		using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>(pList.Count);
		foreach (string p in pList)
		{
			ActorTrait actorTrait = AssetManager.traits.get(p);
			if (actorTrait != null && actorTrait.isAvailable() && (pRainState != RainState.Add || actorTrait.can_be_given) && (pRainState != RainState.Remove || actorTrait.can_be_removed))
			{
				listPool.Add(actorTrait);
			}
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (!item.asset.can_edit_traits)
			{
				continue;
			}
			if (pRainState == RainState.Remove)
			{
				item.removeTraits(listPool);
			}
			else
			{
				foreach (ref ActorTrait item2 in listPool)
				{
					ActorTrait current3 = item2;
					item.addTrait(current3, pRemoveOpposites: true);
				}
			}
			item.addTrait("scar_of_divinity");
			item.startShake();
			item.makeConfused(-1f, pColorEffect: true);
		}
	}

	public static void action_equipment_rain(WorldTile pTile = null, string pDropID = null)
	{
		List<string> equipment_editor = PlayerConfig.instance.data.equipment_editor;
		useEquipmentRain(pTile, equipment_editor, PlayerConfig.instance.data.equipment_editor_state);
	}

	private static void useEquipmentRain(WorldTile pTile, List<string> pItems, RainState pRainState)
	{
		if (pItems.Count == 0)
		{
			return;
		}
		pItems.Shuffle();
		using ListPool<EquipmentAsset> listPool = new ListPool<EquipmentAsset>(pItems.Count);
		HashSet<EquipmentType> hashSet = UnsafeCollectionPool<HashSet<EquipmentType>, EquipmentType>.Get();
		for (int i = 0; i < pItems.Count; i++)
		{
			string pID = pItems[i];
			EquipmentAsset equipmentAsset = AssetManager.items.get(pID);
			if (equipmentAsset != null && (pRainState != RainState.Add || !hashSet.Contains(equipmentAsset.equipment_type)) && equipmentAsset.isAvailable() && (pRainState != RainState.Add || equipmentAsset.can_be_given) && (pRainState != RainState.Remove || equipmentAsset.can_be_removed))
			{
				hashSet.Add(equipmentAsset.equipment_type);
				listPool.Add(equipmentAsset);
			}
		}
		UnsafeCollectionPool<HashSet<EquipmentType>, EquipmentType>.Release(hashSet);
		foreach (Actor item3 in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (!item3.canEditEquipment())
			{
				continue;
			}
			for (int j = 0; j < listPool.Count; j++)
			{
				EquipmentAsset equipmentAsset2 = listPool[j];
				if (!item3.asset.canEditItem(equipmentAsset2))
				{
					continue;
				}
				ActorEquipmentSlot slot = item3.equipment.getSlot(equipmentAsset2.equipment_type);
				Item item = slot.getItem();
				if (pRainState == RainState.Remove)
				{
					if (slot.isEmpty() || item.asset.id != equipmentAsset2.id)
					{
						continue;
					}
				}
				else if (!slot.isEmpty() && (item.asset.id == equipmentAsset2.id || item.isFavorite() || item.isCursed()))
				{
					continue;
				}
				if (pRainState == RainState.Remove)
				{
					item.data.favorite = false;
					item.removeMod("eternal");
					slot.takeAwayItem();
				}
				else
				{
					Item item2 = World.world.items.generateItem(equipmentAsset2, item3.kingdom, World.world.map_stats.player_name, 1, item3, 0, pByPlayer: true);
					item2.addMod("divine_rune");
					item3.equipment.setItem(item2, item3);
				}
			}
			item3.startShake();
			item3.makeConfused(-1f, pColorEffect: true);
		}
	}

	public static void action_acid(WorldTile pTile = null, string pDropID = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		MapAction.checkAcidTerraform(pTile);
		if (Randy.randomChance(0.2f))
		{
			World.world.particles_smoke.spawn(pTile.posV3);
		}
		if (pTile.hasBuilding() && pTile.building.asset.affected_by_acid && pTile.building.isAlive())
		{
			pTile.building.getHit(20f);
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (!Randy.randomChance(0.6f) && !item.hasTrait("acid_proof") && !item.hasTrait("acid_blood"))
			{
				item.getHit(20f, pFlash: true, AttackType.Acid);
			}
		}
		World.world.conway_layer.checkKillRange(pTile.pos, 2);
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_acid)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile, pCheckRoad: false, pCheckBonuses: false, pForce: true);
		}
	}

	public static void action_fire(WorldTile pTile = null, string pDropID = null)
	{
		ActionLibrary.burnTile(null, null, pTile);
		ActionLibrary.startBurningObjects(null, null, pTile);
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_fire)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile, pCheckRoad: false, pCheckBonuses: false, pForce: true);
		}
	}

	public static void action_fireworks(WorldTile pTile = null, string pDropID = null)
	{
		MapAction.terraformTop(pTile, TopTileLibrary.fireworks, TerraformLibrary.remove);
	}

	public static void action_tnt(WorldTile pTile = null, string pDropID = null)
	{
		if (pTile.Type.lava || pTile.isOnFire())
		{
			MapAction.terraformTop(pTile, TopTileLibrary.tnt, TerraformLibrary.remove);
			World.world.explosion_layer.explodeBomb(pTile);
		}
		else
		{
			MapAction.terraformTop(pTile, TopTileLibrary.tnt, TerraformLibrary.remove);
		}
	}

	public static void action_tnt_timed(WorldTile pTile = null, string pDropID = null)
	{
		if (pTile.Type.lava || pTile.isOnFire())
		{
			MapAction.terraformTop(pTile, TopTileLibrary.tnt_timed, TerraformLibrary.remove);
			World.world.explosion_layer.explodeBomb(pTile);
		}
		else
		{
			MapAction.terraformTop(pTile, TopTileLibrary.tnt_timed, TerraformLibrary.remove);
		}
	}

	public static void action_czar_bomba(WorldTile pTile = null, string pDropID = null)
	{
		EffectsLibrary.spawn("fx_nuke_flash", pTile, "czar_bomba");
		World.world.startShake(0.3f, 0.01f, 2.5f, pShakeX: true);
	}

	public static void action_atomic_bomb(WorldTile pTile = null, string pDropID = null)
	{
		World.world.startShake(0.3f, 0.01f, 2f, pShakeX: true);
		EffectsLibrary.spawn("fx_nuke_flash", pTile, "atomic_bomb");
	}

	public static void action_antimatter_bomb(WorldTile pTile = null, string pDropID = null)
	{
		World.world.startShake(0.3f, 0.01f, 0.03f);
		EffectsLibrary.spawn("fx_antimatter_effect", pTile);
	}

	public static void action_napalm_bomb(WorldTile pTile = null, string pDropID = null)
	{
		World.world.startShake(0.3f, 0.01f, 0.5f, pShakeX: true);
		EffectsLibrary.spawn("fx_napalm_flash", pTile);
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_tiny", pTile, 0.15f, 0.3f);
	}

	public static void action_crab_bomb_impact(WorldTile pTile = null, string pDropID = null)
	{
		MusicBox.playSound("event:/SFX/DESTRUCTION/CrabBombImpact", pTile);
		int num = Randy.randomInt(1, 4);
		for (int i = 0; i < num; i++)
		{
			World.world.drop_manager.spawnParabolicDrop(pTile, "crab_bomb_shrapnel", 1f, 15f, 40f, 4f, 16f);
		}
	}

	public static void action_crab_bomb_shrapnel(WorldTile pTile = null, string pDropID = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		EffectsLibrary.spawnAt("fx_explosion_crab_bomb", pTile.posV, 0.25f);
		World.world.startShake(0.3f, 0.01f, 0.5f, pShakeX: true);
		MapAction.damageWorld(pTile, 2, AssetManager.terraform.get("crab_bomb"));
		if (Randy.randomChance(0.05f))
		{
			action_crab_bomb_impact(pTile, "crab_bomb_shrapnel");
		}
	}

	public static void action_grenade(WorldTile pTile = null, string pDropID = null)
	{
		MapAction.damageWorld(pTile, 5, AssetManager.terraform.get("grenade"));
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_small", pTile, 0.1f, 0.15f);
	}

	public static void action_bomb(WorldTile pTile = null, string pDropID = null)
	{
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_middle", pTile, 0.45f, 0.6f);
		if (!World.world.explosion_checker.checkNearby(pTile, 10))
		{
			MapAction.damageWorld(pTile, 10, AssetManager.terraform.get("bomb"));
		}
	}

	public static void action_santa_bomb(WorldTile pTile = null, string pDropID = null)
	{
		MapAction.damageWorld(pTile, 10, AssetManager.terraform.get("santa_bomb"));
		EffectsLibrary.spawnAtTileRandomScale("fx_explosion_small", pTile, 0.45f, 0.6f);
	}

	public static void action_water_bomb(WorldTile pTile = null, string pDropID = null)
	{
		if (pTile.Type.liquid || pTile.Type.lava || pTile.isOnFire())
		{
			MapAction.terraformTop(pTile, TopTileLibrary.water_bomb, TerraformLibrary.remove);
			World.world.explosion_layer.explodeBomb(pTile);
		}
		else
		{
			MapAction.terraformTop(pTile, TopTileLibrary.water_bomb, TerraformLibrary.remove);
		}
	}

	public static void action_lava(WorldTile pTile = null, string pDropID = null)
	{
		LavaHelper.addLava(pTile);
	}

	public static void action_rage(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (Randy.randomChance(0.2f))
			{
				item.addStatusEffect("rage");
			}
		}
	}

	public static void action_magic_rain(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (Randy.randomChance(0.2f))
			{
				item.addStatusEffect("powerup");
			}
			if (Randy.randomChance(0.2f))
			{
				item.addStatusEffect("spell_boost");
			}
			if (Randy.randomChance(0.2f))
			{
				item.addStatusEffect("shield");
			}
			if (Randy.randomChance(0.2f))
			{
				item.addStatusEffect("caffeinated");
			}
			item.addMana((int)((float)item.getMaxMana() * 0.1f));
		}
	}

	public static void action_ash(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (Randy.randomChance(0.3f))
			{
				item.addStatusEffect("cough");
			}
			if (Randy.randomChance(0.1f))
			{
				item.addStatusEffect("ash_fever");
			}
		}
	}

	public static void action_life_seed(WorldTile pTile = null, string pDropID = null)
	{
		if (WorldLawLibrary.world_law_animals_spawn.isEnabled())
		{
			trySpawnUnit(pTile);
		}
		if (WorldLawLibrary.world_law_vegetation_random_seeds.isEnabled())
		{
			trySpawnVegetation(pTile);
		}
	}

	private void action_jazz_rain(WorldTile pTile, string pDropID)
	{
		Actor actor = null;
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f, pRandom: true))
		{
			if (item.hasSubspecies() && item.isBreedingAge())
			{
				actor = item;
				break;
			}
		}
		if (actor != null)
		{
			BabyMaker.makeBabyFromMiracle(actor, ActorSex.None, pAddToFamily: true);
		}
	}

	private static void trySpawnUnit(WorldTile pTile)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset == null || !biome_asset.pot_spawn_units_auto)
		{
			return;
		}
		string pID = biome_asset.pot_units_spawn.GetRandom();
		bool flag = false;
		if (WorldLawLibrary.world_law_drop_of_thoughts.isEnabled() && Randy.randomBool() && biome_asset.pot_sapient_units_spawn != null)
		{
			foreach (string item in biome_asset.pot_sapient_units_spawn.LoopRandom())
			{
				ActorAsset actorAsset = AssetManager.actor_library.get(item);
				if (actorAsset.isAvailable())
				{
					GodPower godPower = actorAsset.getGodPower();
					if (godPower == null || godPower.isAvailable())
					{
						pID = item;
						flag = true;
						break;
					}
				}
			}
		}
		ActorAsset actorAsset2 = AssetManager.actor_library.get(pID);
		if (actorAsset2 == null || actorAsset2.units.Count > actorAsset2.max_random_amount)
		{
			return;
		}
		int num = 0;
		foreach (Actor item2 in Finder.getUnitsFromChunk(pTile, 1))
		{
			_ = item2;
			if (num++ > 3)
			{
				return;
			}
		}
		Actor actor = World.world.units.spawnNewUnit(actorAsset2.id, pTile);
		if (flag && actor != null && actor.subspecies.isJustCreated())
		{
			actor.subspecies.makeSapient();
		}
	}

	private static void trySpawnVegetation(WorldTile pTile)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		if (biome_asset != null && biome_asset.grow_vegetation_auto)
		{
			ActionLibrary.growRandomVegetation(pTile, biome_asset);
		}
	}

	public static void action_snow(WorldTile pTile = null, string pDropID = null)
	{
		if (pTile.canBeFrozen())
		{
			pTile.freeze();
		}
		for (int i = 0; i < 10; i++)
		{
			WorldTile random = pTile.chunk.tiles.GetRandom();
			if (random.canBeFrozen())
			{
				if (Toolbox.DistTile(pTile, random) < 11f)
				{
					break;
				}
				random.freeze();
			}
		}
		if (pTile.Type.lava)
		{
			return;
		}
		if (Randy.randomBool())
		{
			foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
			{
				ActionLibrary.addFrozenEffectOnTarget(item, item);
			}
		}
		checkColdOneBabies(pTile);
	}

	public static void checkColdOneBabies(WorldTile pTile)
	{
		if (!WorldLawLibrary.world_law_disasters_other.isEnabled() || !World.world_era.era_disaster_snow_turns_babies_into_ice_ones)
		{
			return;
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.canTurnIntoColdOne())
			{
				ActionLibrary.turnIntoIceOne(item);
			}
		}
	}

	private static void action_cure(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 4f))
		{
			item.removeTrait("plague");
			item.removeTrait("tumor_infection");
			item.removeTrait("mush_spores");
			item.removeTrait("infected");
			item.finishStatusEffect("ash_fever");
			item.finishStatusEffect("cursed");
			item.startShake();
			item.startColorEffect();
		}
	}

	private static void action_clone_rain(WorldTile pTile, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 1f, pRandom: true))
		{
			WorldTile worldTile = null;
			foreach (WorldTile item2 in item.current_tile.neighboursAll.LoopRandom())
			{
				if (!item2.hasUnits())
				{
					worldTile = item2;
					break;
				}
			}
			if (worldTile != null && World.world.units.cloneUnit(item, worldTile))
			{
				break;
			}
		}
	}

	private void action_sleep_rain(WorldTile pTile, string pDropID)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.makeSleep(60f) && !item.isLying())
			{
				item.applyRandomForce();
			}
		}
	}

	private void action_dispel_rain(WorldTile pTile, string pDropID)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.finishStatusEffect("powerup");
			item.finishStatusEffect("enchanted");
			item.finishStatusEffect("slowness");
			item.finishStatusEffect("shield");
			item.finishStatusEffect("invincible");
			item.finishStatusEffect("spell_boost");
			if (item.asset.die_from_dispel)
			{
				item.getHit(item.getMaxHealthPercent(0.5f), pFlash: true, AttackType.Other, null, pSkipIfShake: true, pMetallicWeapon: false, pCheckDamageReduction: true);
			}
		}
	}

	public static void action_blood_rain(Drop pDrop, WorldTile pTile = null, string pDropID = null)
	{
		long casterId = pDrop.getCasterId();
		Actor actor = World.world.units.get(casterId);
		bool flag = !actor.isRekt();
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (!flag || item.id == casterId || !actor.kingdom.isEnemy(item.kingdom))
			{
				item.finishStatusEffect("burning");
				item.restoreHealth(item.getMaxHealthPercent(0.2f));
				item.startShake();
				item.startColorEffect();
			}
		}
	}

	public static void action_plague(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 4f))
		{
			if (item.hasTrait("plague"))
			{
				item.startShake();
				item.startColorEffect();
			}
			else
			{
				item.addTrait("plague");
			}
		}
	}

	public static void action_zombie_infection(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.can_turn_into_zombie && !item.hasTrait("zombie"))
			{
				item.addTrait("infected");
				item.startShake();
				item.startColorEffect();
			}
		}
	}

	public static void action_mush_spore(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.can_turn_into_mush && !item.hasTrait("mush_spores"))
			{
				item.addTrait("mush_spores");
				item.startShake();
				item.startColorEffect();
			}
		}
	}

	private static void action_curse(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.addStatusEffect("cursed"))
			{
				item.setStatsDirty();
				item.removeTrait("blessed");
				item.startShake();
				item.startColorEffect();
			}
		}
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_curse)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile, pCheckRoad: false, pCheckBonuses: false, pForce: true);
		}
	}

	private static void action_spell_silence(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addStatusEffect("spell_silence");
		}
	}

	private static void action_shield(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addStatusEffect("shield");
		}
	}

	private static void action_powerup(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addStatusEffect("powerup");
			if (item.isSameSpecies("mush_unit") || item.isSameSpecies("mush_animal"))
			{
				AchievementLibrary.super_mushroom.check();
			}
		}
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_powerup)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile, pCheckRoad: false, pCheckBonuses: false, pForce: true);
		}
	}

	private static void action_paint(WorldTile pTile = null, string pDropID = null)
	{
		TileZone zone = pTile.zone;
		if (!zone.hasCity())
		{
			return;
		}
		City city = zone.city;
		World.world.city_zone_helper.city_growth.getZoneToClaim(null, city, pDebug: true, _paint_zones_hashset, 1);
		using ListPool<TileZone> listPool = new ListPool<TileZone>();
		foreach (TileZone item in _paint_zones_hashset)
		{
			if (item.hasCity())
			{
				continue;
			}
			TileZone[] neighbours = item.neighbours;
			for (int i = 0; i < neighbours.Length; i++)
			{
				if (neighbours[i].city == city)
				{
					listPool.Add(item);
				}
			}
		}
		if (listPool.Count > 0)
		{
			TileZone random = listPool.GetRandom();
			city.addZone(random);
			city.setAbandonedZonesDirty();
		}
		_paint_zones_hashset.Clear();
	}

	public static void action_dust_black(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.affected_by_dust)
			{
				item.makeConfused(-1f, pColorEffect: true);
			}
		}
	}

	public static void action_dust_white(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.affected_by_dust)
			{
				item.forgetLanguage();
			}
		}
	}

	public static void action_dust_red(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.affected_by_dust)
			{
				item.makeConfused(-1f, pColorEffect: true);
				if (item.hasFamily())
				{
					item.setFamily(null);
				}
				if (item.hasClan())
				{
					item.forgetClan();
				}
				if (item.hasLover())
				{
					Actor lover = item.lover;
					item.setLover(null);
					lover.setLover(null);
				}
			}
		}
	}

	public static void action_dust_blue(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.affected_by_dust)
			{
				item.forgetCulture();
			}
		}
	}

	public static void action_dust_gold(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.affected_by_dust)
			{
				item.forgetKingdomAndCity();
			}
		}
	}

	public static void action_dust_purple(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.asset.affected_by_dust)
			{
				item.forgetReligion();
			}
		}
	}

	public static void action_coffee(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addStatusEffect("caffeinated");
		}
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_coffee)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile, pCheckRoad: false, pCheckBonuses: false, pForce: true);
		}
	}

	public static void action_blessing(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			if (item.addTrait("blessed"))
			{
				item.setStatsDirty();
				item.event_full_stats = true;
			}
			item.finishStatusEffect("cursed");
			item.startShake();
			if (item.isSameSpecies("frog"))
			{
				AchievementLibrary.the_princess.check();
			}
			item.startColorEffect();
		}
		BiomeAsset biome = pTile.getBiome();
		if (biome != null && biome.spread_by_drops_blessing)
		{
			WorldBehaviourActionBiomes.trySpreadBiomeAround(pTile, pTile, pCheckRoad: false, pCheckBonuses: false, pForce: true);
		}
	}

	public static void action_alien_mold(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addTrait("desire_alien_mold");
		}
	}

	public static void action_drop_computer(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addTrait("desire_computer");
		}
	}

	public static void action_drop_golden_egg(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addTrait("desire_golden_egg");
		}
	}

	public static void action_drop_harp(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addTrait("desire_harp");
		}
	}

	public static void action_madness(WorldTile pTile = null, string pDropID = null)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1, 3f))
		{
			item.addTrait("madness");
		}
	}

	public static void action_inspiration(WorldTile pTile, string pDropID = null)
	{
		if (pTile.zone.hasCity() && !World.world.cities.isLocked())
		{
			City zone_city = pTile.zone_city;
			if (!zone_city.isNeutral() && zone_city.kingdom.countCities() != 1 && !zone_city.isCapitalCity() && zone_city.hasLeader())
			{
				zone_city.leader.addStatusEffect("voices_in_my_head");
				zone_city.useInspire(zone_city.leader);
			}
		}
	}

	public static void action_discord(WorldTile pTile, string pDropID = null)
	{
		if (!pTile.zone.hasCity())
		{
			return;
		}
		City zone_city = pTile.zone_city;
		if (zone_city != null && !zone_city.isNeutral())
		{
			Alliance alliance = zone_city.kingdom.getAlliance();
			if (alliance != null)
			{
				World.world.alliances.useDiscordPower(alliance, zone_city);
			}
		}
	}

	public static void action_spite(WorldTile pTile, string pDropID = null)
	{
		if (pTile.zone.hasCity())
		{
			Kingdom kingdom = pTile.zone.city.kingdom;
			if (!kingdom.isNeutral())
			{
				World.world.diplomacy.eventSpite(kingdom);
			}
		}
	}

	public static void action_friendship(WorldTile pTile, string pDropID = null)
	{
		if (pTile.zone.hasCity())
		{
			Kingdom kingdom = pTile.zone.city.kingdom;
			if (!kingdom.isNeutral())
			{
				World.world.diplomacy.eventFriendship(kingdom);
			}
		}
	}

	public static void action_spawn_building(WorldTile pTile = null, string pDropID = null)
	{
		string randomBuildingAsset = AssetManager.drops.get(pDropID).getRandomBuildingAsset();
		BuildingAsset buildingAsset = AssetManager.buildings.get(randomBuildingAsset);
		Building building = World.world.buildings.addBuilding(randomBuildingAsset, pTile, pCheckForBuild: true);
		if (building == null)
		{
			EffectsLibrary.spawnAtTile("fx_bad_place", pTile, 0.25f);
		}
		else
		{
			buildingAsset.checkLimits(building);
		}
	}

	public static void flash(WorldTile pTile, string pDropID)
	{
		World.world.flash_effects.flashPixel(pTile, 20);
	}

	public static void action_fertilizer_plants(WorldTile pTile = null, string pDropID = null)
	{
		BuildingActions.tryGrowVegetationRandom(pTile, VegetationType.Plants, pOnStart: false, pCheckLimit: false, pCheckRandom: false);
		if (pTile.Type.biome_asset != null && pTile.Type.biome_asset.grow_type_selector_plants == null)
		{
			EffectsLibrary.spawnAtTile("fx_bad_place", pTile, 0.25f);
		}
	}

	public static void action_fertilizer_trees(WorldTile pTile = null, string pDropID = null)
	{
		BiomeAsset biome_asset = pTile.Type.biome_asset;
		BuildingActions.tryGrowVegetationRandom(pTile, VegetationType.Trees, pOnStart: false, pCheckLimit: false, pCheckRandom: false);
		if (biome_asset != null && biome_asset.grow_type_selector_trees == null)
		{
			EffectsLibrary.spawnAtTile("fx_bad_place", pTile, 0.25f);
		}
	}

	public static void action_fruit_bush(WorldTile pTile = null, string pDropID = null)
	{
		BuildingAsset buildingAsset = AssetManager.buildings.get("fruit_bush");
		BuildingActions.tryGrowVegetation(pTile, buildingAsset.id, pSfx: true, pCheckLimit: false);
		if (!buildingAsset.isOverlaysBiomeTags(pTile.Type))
		{
			EffectsLibrary.spawnAtTile("fx_bad_place", pTile, 0.25f);
		}
	}

	public static void action_landmine(WorldTile pTile = null, string pDropID = null)
	{
		if (pTile.Type.lava)
		{
			World.world.explosion_layer.explodeBomb(pTile);
		}
		else
		{
			MapAction.terraformTop(pTile, TopTileLibrary.landmine, TerraformLibrary.remove);
		}
	}

	public static void action_living_house(WorldTile pTile = null, string pDropID = null)
	{
		TileZone zone = pTile.zone;
		if (!zone.hasAnyBuildings())
		{
			return;
		}
		using ListPool<Building> listPool = new ListPool<Building>();
		if (zone.hasAnyBuildingsInSet(BuildingList.Civs))
		{
			listPool.AddRange(zone.getHashset(BuildingList.Civs));
		}
		if (zone.hasAnyBuildingsInSet(BuildingList.Ruins))
		{
			listPool.AddRange(zone.getHashset(BuildingList.Ruins));
		}
		if (zone.hasAnyBuildingsInSet(BuildingList.Abandoned))
		{
			listPool.AddRange(zone.getHashset(BuildingList.Abandoned));
		}
		for (int i = 0; i < listPool.Count; i++)
		{
			ActionLibrary.tryToMakeBuildingAlive(listPool[i]);
		}
	}

	public static void action_living_plants(WorldTile pTile = null, string pDropID = null)
	{
		TileZone zone = pTile.zone;
		if (!zone.hasAnyBuildings())
		{
			return;
		}
		using ListPool<Building> listPool = new ListPool<Building>();
		if (zone.hasAnyBuildingsInSet(BuildingList.Food))
		{
			listPool.AddRange(zone.getHashset(BuildingList.Food));
		}
		if (zone.hasAnyBuildingsInSet(BuildingList.Trees))
		{
			listPool.AddRange(zone.getHashset(BuildingList.Trees));
		}
		if (zone.hasAnyBuildingsInSet(BuildingList.Wheat))
		{
			listPool.AddRange(zone.getHashset(BuildingList.Wheat));
		}
		for (int i = 0; i < listPool.Count; i++)
		{
			ActionLibrary.tryToMakeFloraAlive(listPool[i]);
		}
	}
}
