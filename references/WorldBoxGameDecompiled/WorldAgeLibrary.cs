using System;
using System.Collections.Generic;

public class WorldAgeLibrary : AssetLibrary<WorldAgeAsset>
{
	[NonSerialized]
	public List<WorldAgeAsset> list_only_normal;

	[NonSerialized]
	public Dictionary<int, List<WorldAgeAsset>> pool_by_slots = new Dictionary<int, List<WorldAgeAsset>>();

	public static WorldAgeAsset hope;

	public override void init()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0610: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		//IL_090a: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afb: Unknown result type (might be due to invalid IL or missing references)
		base.init();
		hope = add(new WorldAgeAsset
		{
			id = "age_hope",
			path_icon = "ui/Icons/ages/iconAgeHope",
			rate = 10,
			flag_light_age = true,
			global_unfreeze_world = true,
			cloud_interval = 25f,
			temperature_damage_bonus = 7,
			bonus_loyalty = 15,
			bonus_opinion = 15,
			title_color = Toolbox.makeColor("#86BC4E"),
			flag_light_damage = true
		});
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_grass");
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_normal");
		t.default_slots = AssetLibrary<WorldAgeAsset>.l<int>(1, 3, 5, 6);
		add(new WorldAgeAsset
		{
			id = "age_sun",
			path_icon = "ui/Icons/ages/iconAgeSun",
			rate = 5,
			overlay_sun = true,
			era_disaster_fire_elemental_spawn_on_fire = true,
			fire_elemental_spawn_chance = 0.01f,
			era_effect_overlay_alpha = 0.1f,
			temperature_damage_bonus = 15,
			global_unfreeze_world = true,
			global_unfreeze_world_mountains = true,
			particles_sun = true,
			title_color = Toolbox.makeColor("#F7A42A"),
			bonus_loyalty = 5,
			bonus_opinion = 5,
			bonus_biomes_growth = 3,
			grow_vegetation = false,
			fire_spread_rate_bonus = 2f,
			flag_light_damage = true,
			special_effect_interval = 7f
		});
		t.special_effect_action = droughtAction;
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_desert", "biome_savanna");
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_normal");
		add(new WorldAgeAsset
		{
			id = "age_dark",
			overlay_darkness = true,
			path_icon = "ui/Icons/ages/iconAgeDark",
			era_effect_overlay_alpha = 0.3f,
			overlay_night = true,
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_hope", "age_sun", "age_wonders", "age_tears"),
			rate = 3,
			global_unfreeze_world = true,
			temperature_damage_bonus = -4,
			flag_crops_grow = false,
			flag_night = true,
			title_color = Toolbox.makeColor("#A4A9B2"),
			range_weapons_multiplier = 0.5f,
			bonus_loyalty = -5,
			bonus_opinion = -5,
			bonus_biomes_growth = -2
		});
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_corrupted");
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_normal");
		t.default_slots = AssetLibrary<WorldAgeAsset>.l<int>(2, 7);
		t.link_default_slots = true;
		add(new WorldAgeAsset
		{
			id = "age_tears",
			particles_rain = true,
			path_icon = "ui/Icons/ages/iconAgeTears",
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_hope", "age_sun", "age_wonders"),
			rate = 3,
			global_unfreeze_world = true,
			overlay_rain = true,
			era_effect_overlay_alpha = 0.3f,
			cloud_interval = 20f,
			special_effect_interval = 5f,
			title_color = Toolbox.makeColor("#6C97D8"),
			bonus_biomes_growth = 5
		});
		t.special_effect_action = globalRainAction;
		WorldAgeAsset worldAgeAsset = t;
		worldAgeAsset.special_effect_action = (WorldAgeAction)Delegate.Combine(worldAgeAsset.special_effect_action, new WorldAgeAction(trySpawnThunder));
		WorldAgeAsset worldAgeAsset2 = t;
		worldAgeAsset2.special_effect_action = (WorldAgeAction)Delegate.Combine(worldAgeAsset2.special_effect_action, (WorldAgeAction)delegate
		{
			damageHydrophobicUnits();
		});
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_swamp", "biome_mushroom", "biome_jungle");
		t.clouds = Toolbox.splitStringIntoList("cloud_rain#5", "cloud_lightning#1", "cloud_normal#2");
		t.default_slots = AssetLibrary<WorldAgeAsset>.l<int>(2, 7);
		t.link_default_slots = true;
		add(new WorldAgeAsset
		{
			id = "age_moon",
			path_icon = "ui/Icons/ages/iconAgeMoon",
			overlay_darkness = true,
			flag_moon = true,
			overlay_moon = true,
			global_unfreeze_world = true,
			temperature_damage_bonus = -3,
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_hope", "age_sun"),
			rate = 3,
			light_color = Toolbox.makeColor("#8DFFF3"),
			title_color = Toolbox.makeColor("#B5FAFF")
		});
		t.default_slots = AssetLibrary<WorldAgeAsset>.l<int>(2, 7);
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_crystal");
		t.clouds = Toolbox.splitStringIntoList("cloud_rain#5", "cloud_normal#5");
		add(new WorldAgeAsset
		{
			id = "age_chaos",
			overlay_chaos = true,
			flag_chaos = true,
			era_disaster_rage_brings_demons = true,
			path_icon = "ui/Icons/ages/iconAgeChaos",
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_hope", "age_sun", "age_wonders"),
			era_effect_overlay_alpha = 0.35f,
			rate = 2,
			global_unfreeze_world = true,
			global_unfreeze_world_mountains = true,
			title_color = Toolbox.makeColor("#E6503A"),
			bonus_loyalty = -55,
			bonus_opinion = -35,
			flag_light_damage = true
		});
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_infernal");
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_rage");
		add(new WorldAgeAsset
		{
			id = "age_wonders",
			path_icon = "ui/Icons/ages/iconAgeWonders",
			rate = 2,
			flag_light_age = true,
			particles_magic = true,
			overlay_magic = true,
			era_effect_overlay_alpha = 0.15f,
			global_unfreeze_world = true,
			global_unfreeze_world_mountains = true,
			title_color = Toolbox.makeColor("#D6559C")
		});
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_enchanted", "biome_candy");
		t.clouds = Toolbox.splitStringIntoList("cloud_magic#5", "cloud_normal#1");
		t.default_slots = AssetLibrary<WorldAgeAsset>.l<int>(2, 7);
		t.link_default_slots = true;
		add(new WorldAgeAsset
		{
			id = "age_ice",
			particles_snow = true,
			path_icon = "ui/Icons/ages/iconAgeIce",
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_hope", "age_sun", "age_wonders"),
			rate = 2,
			global_freeze_world = true,
			overlay_winter = true,
			flag_winter = true,
			era_effect_overlay_alpha = 0.2f,
			temperature_damage_bonus = 5,
			flag_crops_grow = false,
			cloud_interval = 20f,
			title_color = Toolbox.makeColor("#C1FAFF"),
			bonus_biomes_growth = -20,
			years_min = 30,
			years_max = 40
		});
		t.special_effect_action = delegate
		{
			damageHydrophobicUnits(pFrost: true);
		};
		WorldAgeAsset worldAgeAsset3 = t;
		worldAgeAsset3.special_effect_action = (WorldAgeAction)Delegate.Combine(worldAgeAsset3.special_effect_action, new WorldAgeAction(frostingAction));
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_permafrost");
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_snow");
		add(new WorldAgeAsset
		{
			id = "age_ash",
			path_icon = "ui/Icons/ages/iconAgeAsh",
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_hope", "age_sun", "age_wonders"),
			rate = 2,
			overlay_ash = true,
			particles_ash = true,
			era_effect_overlay_alpha = 0.4f,
			global_unfreeze_world = true,
			cloud_interval = 15f,
			title_color = Toolbox.makeColor("#DDC49D"),
			bonus_loyalty = -25,
			bonus_opinion = -10,
			bonus_biomes_growth = -4
		});
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_ash");
		add(new WorldAgeAsset
		{
			id = "age_despair",
			overlay_darkness = true,
			particles_snow = true,
			path_icon = "ui/Icons/ages/iconAgeDespair",
			conditions = AssetLibrary<WorldAgeAsset>.a<string>("age_dark", "age_ice"),
			force_next = "age_hope",
			rate = 4,
			overlay_winter = true,
			flag_winter = true,
			overlay_night = true,
			era_effect_overlay_alpha = 0.25f,
			global_freeze_world = true,
			temperature_damage_bonus = 3,
			flag_crops_grow = false,
			flag_night = true,
			cloud_interval = 16f,
			title_color = Toolbox.makeColor("#728599"),
			era_disaster_snow_turns_babies_into_ice_ones = true,
			range_weapons_multiplier = 0.5f,
			bonus_loyalty = -10,
			bonus_opinion = -10,
			bonus_biomes_growth = -2,
			years_min = 30,
			years_max = 40
		});
		WorldAgeAsset worldAgeAsset4 = t;
		worldAgeAsset4.special_effect_action = (WorldAgeAction)Delegate.Combine(worldAgeAsset4.special_effect_action, (WorldAgeAction)delegate
		{
			damageHydrophobicUnits(pFrost: true);
		});
		WorldAgeAsset worldAgeAsset5 = t;
		worldAgeAsset5.special_effect_action = (WorldAgeAction)Delegate.Combine(worldAgeAsset5.special_effect_action, new WorldAgeAction(frostingAction));
		t.biomes = AssetLibrary<WorldAgeAsset>.h<string>("biome_permafrost", "biome_corrupted");
		t.clouds = AssetLibrary<WorldAgeAsset>.l<string>("cloud_snow");
		add(new WorldAgeAsset
		{
			id = "age_unknown",
			path_icon = "ui/Icons/ages/iconAgeUnknown",
			title_color = Toolbox.makeColor("#AAAAAA")
		});
		t.default_slots = AssetLibrary<WorldAgeAsset>.l<int>(4, 8);
	}

	public override void post_init()
	{
		base.post_init();
		foreach (WorldAgeAsset item in list)
		{
			item.path_background = "ui/AgeWheel/backgrounds/" + item.id + "_background";
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (WorldAgeAsset item in list)
		{
			foreach (int default_slot in item.default_slots)
			{
				if (!pool_by_slots.ContainsKey(default_slot))
				{
					pool_by_slots.Add(default_slot, new List<WorldAgeAsset>());
				}
				pool_by_slots[default_slot].Add(item);
			}
		}
		list_only_normal = list.FindAll((WorldAgeAsset pAsset) => pAsset.id != "age_unknown");
	}

	public void damageHydrophobicUnits(bool pFrost = false)
	{
		foreach (Subspecies item in World.world.subspecies.list)
		{
			if (!item.is_damaged_by_water)
			{
				continue;
			}
			foreach (Actor unit in item.units)
			{
				if (unit.isAlive() && !unit.isInsideSomething() && (!pFrost || !unit.has_tag_immunity_cold) && Randy.randomChance(0.5f))
				{
					unit.getHit(unit.getWaterDamage(), pFlash: true, AttackType.Water);
				}
			}
		}
	}

	public void droughtAction()
	{
		extremeEnvironmentAction(droughtCheck);
	}

	private bool droughtCheck(BuildingAsset pAsset)
	{
		if (!pAsset.affected_by_drought)
		{
			return false;
		}
		if (pAsset.type != "type_tree")
		{
			return false;
		}
		return true;
	}

	public void frostingAction()
	{
		extremeEnvironmentAction(frostCheck);
	}

	private bool frostCheck(BuildingAsset pAsset)
	{
		if (!pAsset.affected_by_cold_temperature)
		{
			return false;
		}
		if (pAsset.type != "type_tree" && pAsset.type != "type_vegetation")
		{
			return false;
		}
		return true;
	}

	private void extremeEnvironmentAction(EnvironmentAction pCheckAsset)
	{
		foreach (Building item in World.world.kingdoms_wild.get("nature").buildings.LoopRandom(5))
		{
			if (item.isAlive() && !item.isRuin() && pCheckAsset(item.asset))
			{
				item.startMakingRuins();
			}
		}
	}

	public void globalRainAction()
	{
		if (WorldBehaviourActionFire.hasFires())
		{
			return;
		}
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.isAlive() && actor.hasStatus("burning") && Randy.randomChance(0.9f))
			{
				actor.finishStatusEffect("burning");
			}
		}
		List<Building> simpleList2 = World.world.buildings.getSimpleList();
		for (int j = 0; j < simpleList2.Count; j++)
		{
			Building building = simpleList2[j];
			if (building.isAlive() && Randy.randomChance(0.9f))
			{
				building.stopFire();
			}
		}
	}

	public void trySpawnThunder()
	{
		if (MapBox.isRenderGameplay() && Randy.randomChance(0.1f))
		{
			float worldTimeElapsedSince = World.world.getWorldTimeElapsedSince(StackEffects.last_thunder_timestamp);
			float num = 100f * Config.time_scale_asset.multiplier;
			if (num > 600f)
			{
				num = 600f;
			}
			if (worldTimeElapsedSince > num || StackEffects.last_thunder_timestamp == 0.0)
			{
				EffectsLibrary.spawn("fx_thunder_flash");
				StackEffects.last_thunder_timestamp = World.world.getCurWorldTime();
			}
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (WorldAgeAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}
}
