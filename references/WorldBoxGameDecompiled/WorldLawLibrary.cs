public class WorldLawLibrary : BaseLibraryWithUnlockables<WorldLawAsset>
{
	public static WorldLawAsset world_law_diplomacy;

	public static WorldLawAsset world_law_rites;

	public static WorldLawAsset world_law_peaceful_monsters;

	public static WorldLawAsset world_law_hunger;

	public static WorldLawAsset world_law_vegetation_random_seeds;

	public static WorldLawAsset world_law_roots_without_borders;

	public static WorldLawAsset world_law_spread_trees;

	public static WorldLawAsset world_law_spread_fungi;

	public static WorldLawAsset world_law_spread_plants;

	public static WorldLawAsset world_law_spread_fast_trees;

	public static WorldLawAsset world_law_spread_fast_fungi;

	public static WorldLawAsset world_law_spread_fast_plants;

	public static WorldLawAsset world_law_exploding_mushrooms;

	public static WorldLawAsset world_law_entanglewood;

	public static WorldLawAsset world_law_bark_bites_back;

	public static WorldLawAsset world_law_plants_tickles;

	public static WorldLawAsset world_law_root_pranks;

	public static WorldLawAsset world_law_nectar_nap;

	public static WorldLawAsset world_law_spread_density_high;

	public static WorldLawAsset world_law_grow_minerals;

	public static WorldLawAsset world_law_grow_grass;

	public static WorldLawAsset world_law_biome_overgrowth;

	public static WorldLawAsset world_law_kingdom_expansion;

	public static WorldLawAsset world_law_old_age;

	public static WorldLawAsset world_law_animals_spawn;

	public static WorldLawAsset world_law_animals_babies;

	public static WorldLawAsset world_law_rebellions;

	public static WorldLawAsset world_law_border_stealing;

	public static WorldLawAsset world_law_erosion;

	public static WorldLawAsset world_law_forever_lava;

	public static WorldLawAsset world_law_forever_cold;

	public static WorldLawAsset world_law_disasters_nature;

	public static WorldLawAsset world_law_disasters_other;

	public static WorldLawAsset world_law_rat_plague;

	public static WorldLawAsset world_law_angry_civilians;

	public static WorldLawAsset world_law_civ_babies;

	public static WorldLawAsset world_law_civ_migrants;

	public static WorldLawAsset world_law_forever_tumor_creep;

	public static WorldLawAsset world_law_civ_army;

	public static WorldLawAsset world_law_civ_limit_population_100;

	public static WorldLawAsset world_law_gaias_covenant;

	public static WorldLawAsset world_law_clouds;

	public static WorldLawAsset world_law_evolution_events;

	public static WorldLawAsset world_law_terramorphing;

	public static WorldLawAsset world_law_gene_spaghetti;

	public static WorldLawAsset world_law_mutant_box;

	public static WorldLawAsset world_law_glitched_noosphere;

	public static WorldLawAsset world_law_drop_of_thoughts;

	public static WorldLawAsset world_law_cursed_world;

	public override void init()
	{
		base.init();
		addFlora();
		world_law_gene_spaghetti = add(new WorldLawAsset
		{
			id = "world_law_gene_spaghetti",
			group_id = "units",
			icon_path = "ui/Icons/worldrules/icon_gene_spaghetti",
			default_state = false
		});
		world_law_mutant_box = add(new WorldLawAsset
		{
			id = "world_law_mutant_box",
			group_id = "units",
			icon_path = "ui/Icons/worldrules/icon_mutant_box",
			default_state = false
		});
		world_law_glitched_noosphere = add(new WorldLawAsset
		{
			id = "world_law_glitched_noosphere",
			group_id = "civilizations",
			icon_path = "ui/Icons/worldrules/icon_glitched_noosphere",
			default_state = false
		});
		world_law_drop_of_thoughts = add(new WorldLawAsset
		{
			id = "world_law_drop_of_thoughts",
			group_id = "spawn",
			icon_path = "ui/Icons/worldrules/icon_drop_of_thoughts",
			default_state = false
		});
		world_law_diplomacy = add(new WorldLawAsset
		{
			id = "world_law_diplomacy",
			group_id = "diplomacy",
			icon_path = "ui/Icons/worldrules/icon_diplomacy",
			default_state = true,
			on_state_change = checkDiplomacy
		});
		world_law_rites = add(new WorldLawAsset
		{
			id = "world_law_rites",
			group_id = "diplomacy",
			icon_path = "ui/Icons/worldrules/icon_rites",
			default_state = true
		});
		world_law_peaceful_monsters = add(new WorldLawAsset
		{
			id = "world_law_peaceful_monsters",
			group_id = "mobs",
			icon_path = "ui/Icons/worldrules/icon_peacefulanimals",
			default_state = false,
			on_state_change = checkPeacefulMonsters
		});
		world_law_hunger = add(new WorldLawAsset
		{
			id = "world_law_hunger",
			group_id = "units",
			icon_path = "ui/Icons/worldrules/icon_hunger",
			default_state = true
		});
		world_law_vegetation_random_seeds = add(new WorldLawAsset
		{
			id = "world_law_vegetation_random_seeds",
			group_id = "nature",
			icon_path = "ui/Icons/worldrules/icon_random_seeds",
			default_state = true
		});
		world_law_roots_without_borders = add(new WorldLawAsset
		{
			id = "world_law_roots_without_borders",
			group_id = "nature",
			icon_path = "ui/Icons/worldrules/icon_roots_without_borders",
			default_state = false
		});
		world_law_grow_minerals = add(new WorldLawAsset
		{
			id = "world_law_grow_minerals",
			group_id = "nature",
			icon_path = "ui/Icons/iconStone",
			default_state = true
		});
		world_law_grow_grass = add(new WorldLawAsset
		{
			id = "world_law_grow_grass",
			group_id = "biomes",
			icon_path = "ui/Icons/worldrules/icon_growgrass",
			default_state = true
		});
		world_law_biome_overgrowth = add(new WorldLawAsset
		{
			id = "world_law_biome_overgrowth",
			group_id = "biomes",
			icon_path = "ui/Icons/worldrules/icon_overgrowth",
			default_state = true
		});
		world_law_terramorphing = add(new WorldLawAsset
		{
			id = "world_law_terramorphing",
			group_id = "civilizations",
			icon_path = "ui/Icons/worldrules/icon_terramorphing",
			default_state = true
		});
		world_law_kingdom_expansion = add(new WorldLawAsset
		{
			id = "world_law_kingdom_expansion",
			group_id = "civilizations",
			icon_path = "ui/Icons/worldrules/icon_kingdomexpansion",
			default_state = true
		});
		world_law_old_age = add(new WorldLawAsset
		{
			id = "world_law_old_age",
			group_id = "units",
			icon_path = "ui/Icons/worldrules/icon_oldage",
			default_state = true
		});
		world_law_animals_spawn = add(new WorldLawAsset
		{
			id = "world_law_animals_spawn",
			group_id = "spawn",
			icon_path = "ui/Icons/worldrules/icon_animalspawn",
			default_state = true
		});
		world_law_animals_babies = add(new WorldLawAsset
		{
			id = "world_law_animals_babies",
			group_id = "mobs",
			icon_path = "ui/Icons/iconChicken",
			default_state = true
		});
		world_law_rebellions = add(new WorldLawAsset
		{
			id = "world_law_rebellions",
			group_id = "diplomacy",
			icon_path = "ui/Icons/worldrules/icon_rebellion",
			default_state = true
		});
		world_law_border_stealing = add(new WorldLawAsset
		{
			id = "world_law_border_stealing",
			group_id = "diplomacy",
			icon_path = "ui/Icons/worldrules/icon_borderstealing",
			default_state = true
		});
		world_law_erosion = add(new WorldLawAsset
		{
			id = "world_law_erosion",
			group_id = "nature",
			icon_path = "ui/Icons/worldrules/icon_erosion",
			default_state = true
		});
		world_law_forever_lava = add(new WorldLawAsset
		{
			id = "world_law_forever_lava",
			group_id = "weather",
			icon_path = "ui/Icons/worldrules/icon_foreverlava",
			default_state = false
		});
		world_law_forever_cold = add(new WorldLawAsset
		{
			id = "world_law_forever_cold",
			group_id = "weather",
			icon_path = "ui/Icons/iconSnow",
			default_state = false
		});
		world_law_disasters_nature = add(new WorldLawAsset
		{
			id = "world_law_disasters_nature",
			group_id = "disasters",
			icon_path = "ui/Icons/worldrules/icon_disasters",
			default_state = true
		});
		world_law_clouds = add(new WorldLawAsset
		{
			id = "world_law_clouds",
			group_id = "spawn",
			icon_path = "ui/Icons/iconCloud",
			default_state = true
		});
		world_law_evolution_events = add(new WorldLawAsset
		{
			id = "world_law_evolution_events",
			group_id = "other",
			icon_path = "ui/Icons/iconMonolith",
			default_state = true
		});
		world_law_disasters_other = add(new WorldLawAsset
		{
			id = "world_law_disasters_other",
			group_id = "disasters",
			icon_path = "ui/Icons/iconEvilMage",
			default_state = true
		});
		world_law_rat_plague = add(new WorldLawAsset
		{
			id = "world_law_rat_plague",
			group_id = "disasters",
			icon_path = "ui/Icons/iconRatKing",
			default_state = false
		});
		world_law_angry_civilians = add(new WorldLawAsset
		{
			id = "world_law_angry_civilians",
			group_id = "civilizations",
			icon_path = "ui/Icons/worldrules/icon_angryvillagers",
			default_state = false
		});
		world_law_civ_babies = add(new WorldLawAsset
		{
			id = "world_law_civ_babies",
			group_id = "civilizations",
			icon_path = "ui/Icons/worldrules/icon_lastofus",
			default_state = true
		});
		world_law_civ_migrants = add(new WorldLawAsset
		{
			id = "world_law_civ_migrants",
			group_id = "civilizations",
			icon_path = "ui/Icons/worldrules/icon_migrants",
			default_state = true
		});
		world_law_forever_tumor_creep = add(new WorldLawAsset
		{
			id = "world_law_forever_tumor_creep",
			group_id = "mobs",
			icon_path = "ui/Icons/iconTumor",
			default_state = false
		});
		world_law_civ_army = add(new WorldLawAsset
		{
			id = "world_law_civ_army",
			group_id = "civilizations",
			icon_path = "ui/Icons/iconArmyList",
			default_state = true
		});
		world_law_civ_limit_population_100 = add(new WorldLawAsset
		{
			id = "world_law_civ_limit_population_100",
			group_id = "harmony",
			icon_path = "ui/Icons/iconPopulation100",
			default_state = false
		});
		world_law_gaias_covenant = add(new WorldLawAsset
		{
			id = "world_law_gaias_covenant",
			group_id = "harmony",
			icon_path = "ui/Icons/worldrules/icon_gaias_covenant",
			default_state = false
		});
		world_law_cursed_world = add(new WorldLawAsset
		{
			id = "world_law_cursed_world",
			icon_path = "ui/Icons/worldrules/icon_cursed_world",
			default_state = false,
			can_turn_off = false,
			requires_premium = true,
			on_state_change = checkCursedWorld,
			on_state_enabled = turnOnCursedWorld,
			on_world_load = checkAlreadyCursed
		});
	}

	private void addFlora()
	{
		world_law_spread_trees = add(new WorldLawAsset
		{
			id = "world_law_spread_trees",
			group_id = "trees",
			icon_path = "ui/Icons/worldrules/icon_grow_trees",
			default_state = true
		});
		world_law_spread_fungi = add(new WorldLawAsset
		{
			id = "world_law_spread_fungi",
			group_id = "fungi",
			icon_path = "ui/Icons/worldrules/icon_grow_fungi",
			default_state = true
		});
		world_law_spread_plants = add(new WorldLawAsset
		{
			id = "world_law_spread_plants",
			group_id = "plants",
			icon_path = "ui/Icons/worldrules/icon_grow_plants",
			default_state = true
		});
		world_law_spread_fast_trees = add(new WorldLawAsset
		{
			id = "world_law_spread_fast_trees",
			group_id = "trees",
			icon_path = "ui/Icons/worldrules/icon_grow_trees_fast",
			default_state = false
		});
		world_law_spread_fast_fungi = add(new WorldLawAsset
		{
			id = "world_law_spread_fast_fungi",
			group_id = "fungi",
			icon_path = "ui/Icons/worldrules/icon_grow_fungi_fast",
			default_state = false
		});
		world_law_spread_fast_plants = add(new WorldLawAsset
		{
			id = "world_law_spread_fast_plants",
			group_id = "plants",
			icon_path = "ui/Icons/worldrules/icon_grow_plants_fast",
			default_state = false
		});
		world_law_spread_density_high = add(new WorldLawAsset
		{
			id = "world_law_spread_density_high",
			group_id = "nature",
			icon_path = "ui/Icons/worldrules/icon_flora_density_high",
			default_state = false
		});
		world_law_exploding_mushrooms = add(new WorldLawAsset
		{
			id = "world_law_exploding_mushrooms",
			group_id = "fungi",
			icon_path = "ui/Icons/worldrules/icon_exploding_mushrooms",
			default_state = false,
			on_state_change = delegate(PlayerOptionData pOption)
			{
				if (pOption.boolVal)
				{
					World.world.map_stats.exploding_mushrooms_enabled_at = World.world.getCurWorldTime();
				}
			}
		});
		world_law_entanglewood = add(new WorldLawAsset
		{
			id = "world_law_entanglewood",
			group_id = "trees",
			icon_path = "ui/Icons/worldrules/icon_entanglewood",
			default_state = true
		});
		world_law_bark_bites_back = add(new WorldLawAsset
		{
			id = "world_law_bark_bites_back",
			group_id = "trees",
			icon_path = "ui/Icons/worldrules/icon_bark_bites_back",
			default_state = false
		});
		world_law_plants_tickles = add(new WorldLawAsset
		{
			id = "world_law_plants_tickles",
			group_id = "plants",
			icon_path = "ui/Icons/worldrules/icon_plants_tickles",
			default_state = false
		});
		world_law_root_pranks = add(new WorldLawAsset
		{
			id = "world_law_root_pranks",
			group_id = "plants",
			icon_path = "ui/Icons/worldrules/icon_root_pranks",
			default_state = false
		});
		world_law_nectar_nap = add(new WorldLawAsset
		{
			id = "world_law_nectar_nap",
			group_id = "plants",
			icon_path = "ui/Icons/worldrules/icon_nectar_nap",
			default_state = false
		});
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		foreach (WorldLawAsset item in list)
		{
			checkSpriteExists("icon_path", item.icon_path, item);
		}
	}

	public override void editorDiagnosticLocales()
	{
		foreach (WorldLawAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
			checkLocale(item, item.getDescriptionID2());
		}
		base.editorDiagnosticLocales();
	}

	private void checkDiplomacy(PlayerOptionData pOption)
	{
		if (!world_law_diplomacy.isEnabled())
		{
			World.world.stopAttacksFor(pMonsters: false);
			World.world.wars.stopAllWars();
		}
	}

	private void checkPeacefulMonsters(PlayerOptionData pOption)
	{
		if (world_law_peaceful_monsters.isEnabled())
		{
			World.world.stopAttacksFor(pMonsters: true);
		}
	}

	private void turnOnCursedWorld(PlayerOptionData pOption)
	{
		CursedSacrifice.justCursedWorld();
	}

	private void checkCursedWorld(PlayerOptionData pOption)
	{
		PowerButton.checkActorSpawnButtons();
	}

	private void checkAlreadyCursed()
	{
		if (world_law_cursed_world.isEnabled())
		{
			CursedSacrifice.loadAlreadyCursedState();
		}
	}

	public static float getIntervalSpreadTrees()
	{
		if (world_law_spread_fast_trees.isEnabled())
		{
			return 10f;
		}
		return 50f;
	}

	public static float getIntervalSpreadPlants()
	{
		if (world_law_spread_fast_plants.isEnabled())
		{
			return 10f;
		}
		return 40f;
	}

	public static float getIntervalSpreadFungi()
	{
		if (world_law_spread_fast_fungi.isEnabled())
		{
			return 10f;
		}
		return 30f;
	}

	public string addToGameplayReport(string pWhatFor)
	{
		string empty = string.Empty;
		empty = empty + pWhatFor + "\n";
		foreach (WorldLawAsset item in list)
		{
			string translatedName = item.getTranslatedName();
			string translatedDescription = item.getTranslatedDescription();
			string translatedDescription2 = item.getTranslatedDescription2();
			string text = "\n" + translatedName;
			text += "\n";
			if (!string.IsNullOrEmpty(translatedDescription))
			{
				text = text + "1: " + translatedDescription;
			}
			if (!string.IsNullOrEmpty(translatedDescription2))
			{
				text = text + "\n2: " + translatedDescription2;
			}
			empty += text;
		}
		return empty + "\n\n";
	}
}
