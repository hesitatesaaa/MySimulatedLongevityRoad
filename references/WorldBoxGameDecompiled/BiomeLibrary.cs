using System.Collections.Generic;

public class BiomeLibrary : AssetLibrary<BiomeAsset>
{
	public static List<BiomeAsset> pool_biomes = new List<BiomeAsset>();

	public override void init()
	{
		base.init();
		addNormalBiomes();
		addCreepBiomes();
		addSpecialBiomes();
	}

	public void addNormalBiomes()
	{
		add(new BiomeAsset
		{
			id = "biome_grass",
			tile_low = "grass_low",
			tile_high = "grass_high",
			localized_key = "Grass",
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 8,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("graminae", "viridis", "pratensis")
		});
		t.addSubspeciesTrait("fast_builders");
		t.addCultureTrait("pep_talks");
		t.addCultureTrait("youth_reverence");
		t.addCultureTrait("gossip_lovers");
		t.addLanguageTrait("strict_spelling");
		t.addUnit("wolf");
		t.addUnit("fox", 2);
		t.addUnit("raccoon");
		t.addUnit("sheep", 3);
		t.addUnit("chicken", 3);
		t.addUnit("rabbit", 3);
		t.addUnit("fly", 4);
		t.addUnit("beetle", 4);
		t.addUnit("grasshopper", 4);
		t.addSapientUnit("human");
		t.addSapientUnit("dwarf");
		t.addSapientUnit("orc");
		t.addSapientUnit("elf");
		t.addSapientUnit("civ_wolf");
		t.addSapientUnit("civ_fox");
		t.addSapientUnit("bandit");
		t.addSapientUnit("civ_sheep");
		t.addSapientUnit("civ_chicken");
		t.addSapientUnit("civ_rabbit");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("tree_green_1");
		t.addTree("tree_green_2");
		t.addTree("tree_green_3");
		t.addPlant("mushroom_red", 2);
		t.addPlant("mushroom_green", 2);
		t.addPlant("mushroom_teal", 2);
		t.addPlant("mushroom_white", 2);
		t.addPlant("mushroom_yellow", 2);
		t.addPlant("green_herb", 4);
		t.addPlant("flower", 4);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_savanna",
			tile_low = "savanna_low",
			tile_high = "savanna_high",
			localized_key = "Savanna",
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 5,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("savannae", "aridus", "praeruptus")
		});
		t.addActorTrait("fast");
		t.addCultureTrait("sword_lovers");
		t.addLanguageTrait("elegant_words");
		t.addUnit("rhino");
		t.addUnit("hyena");
		t.addUnit("buffalo");
		t.addUnit("cat", 3);
		t.addUnit("armadillo");
		t.addUnit("ostrich", 2);
		t.addUnit("fly", 4);
		t.addUnit("beetle", 4);
		t.addSapientUnit("human");
		t.addSapientUnit("dwarf");
		t.addSapientUnit("orc");
		t.addSapientUnit("elf");
		t.addSapientUnit("civ_cat");
		t.addSapientUnit("civ_rhino");
		t.addSapientUnit("civ_hyena");
		t.addSapientUnit("civ_buffalo");
		t.addSapientUnit("civ_armadillo");
		t.addMineral("mineral_stone", 5);
		t.addMineral("mineral_metals", 3);
		t.addTree("savanna_tree_1", 5);
		t.addTree("savanna_tree_2", 5);
		t.addTree("savanna_tree_big_1");
		t.addTree("savanna_tree_big_2");
		t.addPlant("savanna_plant");
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_jungle",
			tile_low = "jungle_low",
			tile_high = "jungle_high",
			localized_key = "Jungle",
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 6,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("junglis", "viriditas", "tropicus")
		});
		t.addActorTrait("tiny");
		t.addActorTrait("poison_immune");
		t.addCultureTrait("spear_lovers");
		t.addCultureTrait("city_layout_raindrops");
		t.addLanguageTrait("beautiful_calligraphy");
		t.addUnit("snake");
		t.addUnit("cat", 2);
		t.addUnit("monkey");
		t.addUnit("frog", 3);
		t.addUnit("scorpion");
		t.addSapientUnit("human");
		t.addSapientUnit("dwarf");
		t.addSapientUnit("orc");
		t.addSapientUnit("elf");
		t.addSapientUnit("civ_snake");
		t.addSapientUnit("civ_cat");
		t.addSapientUnit("civ_monkey");
		t.addSapientUnit("civ_frog");
		t.addSapientUnit("civ_scorpion");
		t.addMineral("mineral_silver", 5);
		t.addMineral("mineral_metals");
		t.addMineral("mineral_gems");
		t.addTree("jungle_tree");
		t.addPlant("jungle_plant");
		t.addPlant("jungle_flower");
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_desert",
			tile_low = "desert_low",
			tile_high = "desert_high",
			localized_key = "Desert",
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 4,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("solantheria", "ergensis", "deserticus")
		});
		t.addActorTrait("eagle_eyed");
		t.addClanTrait("silver_tongues");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("gossip_lovers");
		t.addLanguageTrait("stylish_writing");
		t.addSubspeciesTraitAlways("adaptation_desert");
		t.addUnit("cat");
		t.addUnit("snake");
		t.addSapientUnit("human");
		t.addSapientUnit("dwarf");
		t.addSapientUnit("orc");
		t.addSapientUnit("elf");
		t.addSapientUnit("civ_cat");
		t.addSapientUnit("civ_snake");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_gems");
		t.addMineral("mineral_silver");
		t.addTree("desert_tree");
		t.addPlant("desert_plant");
		t.addUnit("scorpion");
		add(new BiomeAsset
		{
			id = "biome_lemon",
			tile_low = "lemon_low",
			tile_high = "lemon_high",
			localized_key = "Lemon",
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 3,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("aurantium", "acidus", "limonum")
		});
		t.addActorTrait("genius");
		t.addCultureTrait("fast_learners");
		t.addLanguageTrait("melodic");
		t.addLanguageTrait("elegant_words");
		t.addUnit("lemon_snail");
		t.addSapientUnit("civ_lemon_man");
		t.addMineral("mineral_stone", 5);
		t.addMineral("mineral_metals", 3);
		t.addTree("lemon_tree");
		t.addPlant("mushroom_red");
		t.addPlant("mushroom_green");
		t.addPlant("mushroom_teal");
		t.addPlant("mushroom_white");
		t.addPlant("mushroom_yellow");
		t.addPlant("green_herb", 5);
		t.addPlant("flower", 5);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_permafrost",
			tile_low = "permafrost_low",
			tile_high = "permafrost_high",
			localized_key = "Permafrost",
			cold_biome = true,
			spread_biome = true,
			generator_pot_amount = 3,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("albus", "sibericus", "frostis")
		});
		t.addActorTrait("freeze_proof");
		t.addSubspeciesTrait("cold_resistance");
		t.addSubspeciesTraitAlways("adaptation_permafrost");
		t.addCultureTrait("attentive_readers");
		t.addCultureTrait("ancestors_knowledge");
		t.addUnit("penguin", 4);
		t.addUnit("bear");
		t.addUnit("wolf", 2);
		t.addUnit("snowman");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addSapientUnit("human");
		t.addSapientUnit("cold_one");
		t.addSapientUnit("civ_penguin");
		t.addSapientUnit("civ_bear");
		t.addSapientUnit("civ_wolf");
		t.addSapientUnit("snowman");
		t.addTree("pine_tree");
		t.addPlant("snow_plant");
		add(new BiomeAsset
		{
			id = "biome_swamp",
			tile_low = "swamp_low",
			tile_high = "swamp_high",
			localized_key = "Swamp",
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 3,
			generator_max_size = 300,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("paludis", "limosa", "paluster")
		});
		t.addActorTrait("regeneration");
		t.addCultureTrait("bow_lovers");
		t.addLanguageTrait("scribble");
		t.addSubspeciesTraitAlways("adaptation_swamp");
		t.addSubspeciesTraitEvolution("mutation_skin_tentacle_horror");
		t.addUnit("crocodile");
		t.addUnit("snake", 4);
		t.addUnit("frog", 6);
		t.addUnit("fly", 5);
		t.addUnit("turtle", 3);
		t.addUnit("capybara");
		t.addSapientUnit("orc");
		t.addSapientUnit("elf");
		t.addSapientUnit("bandit");
		t.addSapientUnit("civ_crocodile");
		t.addSapientUnit("civ_snake");
		t.addSapientUnit("civ_frog");
		t.addSapientUnit("civ_capybara");
		t.addSapientUnit("civ_piranha");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("swamp_tree");
		t.addPlant("swamp_plant");
		t.addPlant("swamp_plant_big");
		add(new BiomeAsset
		{
			id = "biome_crystal",
			tile_low = "crystal_low",
			tile_high = "crystal_high",
			localized_key = "Crystal",
			loot_generation = 2,
			spread_biome = true,
			generator_pot_amount = 2,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("crystallus", "lucidus", "prismatus")
		});
		t.addActorTrait("strong");
		t.addSubspeciesTrait("bioproduct_gems");
		t.addCultureTrait("tiny_legends");
		t.addCultureTrait("armorsmith_mastery");
		t.addCultureTrait("weaponsmith_mastery");
		t.addLanguageTrait("powerful_words");
		t.addUnit("crystal_sword", 2);
		t.addSapientUnit("dwarf");
		t.addSapientUnit("civ_crystal_golem");
		t.addTree("crystal_tree");
		t.addPlant("crystal_plant");
		add(new BiomeAsset
		{
			id = "biome_enchanted",
			tile_low = "enchanted_low",
			tile_high = "enchanted_high",
			localized_key = "Enchanted",
			spread_biome = true,
			spread_by_drops_blessing = true,
			loot_generation = 1,
			generator_pot_amount = 3,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("incantatum", "enchanta", "herbaeonis")
		});
		t.addActorTrait("attractive");
		t.addSubspeciesTrait("gaia_roots");
		t.addSubspeciesTraitEvolution("mutation_skin_energy");
		t.addCultureTrait("training_potential");
		t.addCultureTrait("diplomatic_ascension");
		t.addCultureTrait("legacy_keepers");
		t.addLanguageTrait("ancient_runes");
		t.addLanguageTrait("enlightening_script");
		t.addUnit("butterfly", 5);
		t.addUnit("fairy");
		t.addSapientUnit("elf");
		t.addSapientUnit("white_mage");
		t.addSapientUnit("fairy");
		t.addMineral("mineral_silver", 5);
		t.addMineral("mineral_metals", 2);
		t.addMineral("mineral_gems");
		t.addTree("enchanted_tree");
		t.addPlant("flower", 5);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_corrupted",
			tile_low = "corrupted_low",
			tile_high = "corrupted_high",
			localized_key = "Corrupted",
			loot_generation = -1,
			dark_biome = true,
			spread_biome = true,
			spread_by_drops_curse = true,
			generator_pot_amount = 1,
			generator_max_size = 20,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("corruptus", "morbida", "tenebris")
		});
		t.addActorTrait("weak");
		t.addSubspeciesTrait("death_grow_mythril");
		t.addSubspeciesTraitAlways("adaptation_corruption");
		t.addCultureTrait("happiness_from_war");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("ethnocentric_guard");
		t.addLanguageTrait("cursed_font");
		t.addReligionTrait("rite_of_restless_dead");
		t.addReligionTrait("cast_curse");
		t.addReligionTrait("spawn_skeleton");
		t.addUnit("jumpy_skull");
		t.addSapientUnit("necromancer");
		t.addSapientUnit("jumpy_skull");
		t.addSapientUnit("plague_doctor");
		t.addMineral("mineral_bones", 7);
		t.addTree("corrupted_tree");
		t.addTree("corrupted_tree_big");
		t.addPlant("corrupted_plant");
		add(new BiomeAsset
		{
			id = "biome_infernal",
			tile_low = "infernal_low",
			tile_high = "infernal_high",
			localized_key = "Infernal",
			spread_biome = true,
			spread_by_drops_fire = true,
			spread_ignore_burned_stages = true,
			generator_pot_amount = 1,
			generator_max_size = 20,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("ignifer", "brasarius", "ardorus")
		});
		t.addActorTrait("fire_blood");
		t.addActorTrait("fire_proof");
		t.addSubspeciesTrait("heat_resistance");
		t.addSubspeciesTrait("hydrophobia");
		t.addSubspeciesTraitAlways("adaptation_infernal");
		t.addCultureTrait("happiness_from_war");
		t.addCultureTrait("xenophobic");
		t.addClanTrait("cursed_blood");
		t.addLanguageTrait("raging_paragraphs");
		t.addReligionTrait("rite_of_the_abyss");
		t.addUnit("fire_skull");
		t.addSapientUnit("evil_mage");
		t.addSapientUnit("demon");
		t.addSapientUnit("fire_skull");
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_adamantine", 3);
		t.addTree("infernal_tree");
		t.addTree("infernal_tree_big");
		t.addTree("infernal_tree_small");
		t.addPlant("flame_flower");
		add(new BiomeAsset
		{
			id = "biome_candy",
			tile_low = "candy_low",
			tile_high = "candy_high",
			localized_key = "Candy",
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 1,
			generator_max_size = 30,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("dulcis", "glucosus", "crispus")
		});
		t.addActorTrait("fat");
		t.addActorTrait("gluttonous");
		t.addSubspeciesTrait("annoying_fireworks");
		t.addSubspeciesTrait("super_positivity");
		t.addCultureTrait("tiny_legends");
		t.addLanguageTrait("foolish_glyphs");
		t.addUnit("smore");
		t.addSapientUnit("civ_candy_man");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("candy_tree");
		t.addPlant("candy_plant");
		add(new BiomeAsset
		{
			id = "biome_mushroom",
			tile_low = "mushroom_low",
			tile_high = "mushroom_high",
			localized_key = "Mushroom",
			spread_biome = true,
			spread_by_drops_water = true,
			spread_by_drops_powerup = true,
			generator_pot_amount = 1,
			generator_max_size = 300,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("sporae", "mycota", "mycetis")
		});
		t.addActorTrait("regeneration");
		t.addSubspeciesTrait("bioproduct_mushrooms");
		t.addSubspeciesTrait("super_positivity");
		t.addCultureTrait("animal_whisperers");
		t.addCultureTrait("pep_talks");
		t.addLanguageTrait("powerful_words");
		t.addUnit("frog", 3);
		t.addUnit("sheep");
		t.addSapientUnit("civ_sheep");
		t.addSapientUnit("civ_frog");
		t.addMineral("mineral_silver", 3);
		t.addMineral("mineral_stone", 5);
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_gems");
		t.addTree("mushroom_tree");
		t.addPlant("flower", 2);
		t.addPlant("mushroom_red", 2);
		t.addPlant("mushroom_green", 2);
		t.addPlant("mushroom_teal", 2);
		t.addPlant("mushroom_white", 2);
		t.addPlant("mushroom_yellow", 2);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_wasteland",
			tile_low = "wasteland_low",
			tile_high = "wasteland_high",
			localized_key = "Wasteland",
			loot_generation = -2,
			dark_biome = true,
			spread_biome = true,
			spread_by_drops_acid = true,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("toxicus", "wastus")
		});
		t.addActorTrait("ugly");
		t.addSubspeciesTrait("bad_genes");
		t.addSubspeciesTrait("bioluminescence");
		t.addSubspeciesTraitAlways("adaptation_wasteland");
		t.addSubspeciesTraitEvolution("mutation_skin_abomination");
		t.addCultureTrait("xenophobic");
		t.addCultureTrait("solitude_seekers");
		t.addCultureTrait("legacy_keepers");
		t.addLanguageTrait("foolish_glyphs");
		t.addUnit("rat", 4);
		t.addUnit("acid_blob");
		t.addSapientUnit("civ_acid_gentleman");
		t.addSapientUnit("civ_rat");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("wasteland_tree");
		t.addPlant("wasteland_flower");
		add(new BiomeAsset
		{
			id = "biome_birch",
			tile_low = "birch_low",
			tile_high = "birch_high",
			localized_key = "Birch",
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 4,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("silvanus", "virentis", "sylvestris")
		});
		t.addSubspeciesTrait("enhanced_strength");
		t.addCultureTrait("axe_lovers");
		t.addCultureTrait("diplomatic_ascension");
		t.addUnit("wolf");
		t.addUnit("sheep");
		t.addUnit("rabbit");
		t.addUnit("fox");
		t.addSapientUnit("civ_wolf");
		t.addSapientUnit("civ_sheep");
		t.addSapientUnit("civ_rabbit");
		t.addSapientUnit("civ_fox");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("birch_tree");
		t.addPlant("birch_plant");
		t.addPlant("mushroom_red");
		t.addPlant("mushroom_green");
		t.addPlant("mushroom_teal");
		t.addPlant("mushroom_white");
		t.addPlant("mushroom_yellow");
		t.addPlant("green_herb", 4);
		t.addPlant("flower", 4);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_maple",
			tile_low = "maple_low",
			tile_high = "maple_high",
			localized_key = "Maple",
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 5,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("mapleleafus", "canadus", "malifolia")
		});
		t.addSubspeciesTrait("death_grow_tree");
		t.addCultureTrait("xenophiles");
		t.addLanguageTrait("nicely_structured_grammar");
		t.addLanguageTrait("melodic");
		t.addUnit("wolf");
		t.addUnit("dog");
		t.addSapientUnit("civ_wolf");
		t.addSapientUnit("civ_dog");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("maple_tree");
		t.addPlant("maple_plant");
		t.addPlant("mushroom_red");
		t.addPlant("mushroom_green");
		t.addPlant("mushroom_teal");
		t.addPlant("mushroom_white");
		t.addPlant("mushroom_yellow");
		t.addPlant("green_herb", 4);
		t.addPlant("flower", 4);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_rocklands",
			tile_low = "rocklands_low",
			tile_high = "rocklands_high",
			localized_key = "Rocklands",
			grow_strength = 5,
			spread_biome = true,
			generator_pot_amount = 2,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("lithos", "petrae", "rupes")
		});
		t.addActorTrait("giant");
		t.addSubspeciesTrait("bioproduct_stone");
		t.addSubspeciesTrait("parental_care");
		t.addSubspeciesTraitEvolution("mutation_skin_living_rock");
		t.addCultureTrait("statue_lovers");
		t.addCultureTrait("training_potential");
		t.addCultureTrait("city_layout_stone_garden");
		t.addClanTrait("blood_of_giants");
		t.addLanguageTrait("powerful_words");
		t.addUnit("goat");
		t.addUnit("alpaca");
		t.addUnit("armadillo");
		t.addSapientUnit("dwarf");
		t.addSapientUnit("civ_goat");
		t.addSapientUnit("civ_alpaca");
		t.addSapientUnit("civ_armadillo");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_gems");
		t.addTree("rocklands_tree");
		t.addPlant("rocklands_plant");
		add(new BiomeAsset
		{
			id = "biome_garlic",
			tile_low = "garlic_low",
			tile_high = "garlic_high",
			localized_key = "Garlic",
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 2,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("aromatica", "vampiris", "odoratus")
		});
		t.addActorTrait("immune");
		t.addSubspeciesTrait("accelerated_healing");
		t.addCultureTrait("attentive_readers");
		t.addClanTrait("masters_of_propaganda");
		t.addLanguageTrait("enlightening_script");
		t.addUnit("garl");
		t.addSapientUnit("civ_garlic_man");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("garlic_tree");
		t.addPlant("garlic_plant");
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_flower",
			tile_low = "flower_low",
			tile_high = "flower_high",
			localized_key = "Flower",
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 3,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("botanicus", "petala", "fragrans")
		});
		t.addActorTrait("attractive");
		t.addSubspeciesTrait("death_grow_plant");
		t.addCultureTrait("matriarchy");
		t.addCultureTrait("gossip_lovers");
		t.addLanguageTrait("elegant_words");
		t.addUnit("flower_bud");
		t.addUnit("butterfly");
		t.addUnit("beetle");
		t.addUnit("bee");
		t.addUnit("grasshopper");
		t.addUnit("fly");
		t.addSapientUnit("civ_liliar");
		t.addSapientUnit("druid");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addTree("flower_tree_1");
		t.addTree("flower_tree_2");
		t.addTree("flower_tree_3");
		t.addPlant("flower_plant", 3);
		t.addPlant("mushroom_red");
		t.addPlant("mushroom_green");
		t.addPlant("mushroom_teal");
		t.addPlant("mushroom_white");
		t.addPlant("mushroom_yellow");
		t.addPlant("green_herb", 5);
		t.addPlant("flower", 20);
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_celestial",
			tile_low = "celestial_low",
			tile_high = "celestial_high",
			localized_key = "Celestial",
			grow_strength = 5,
			spread_biome = true,
			generator_pot_amount = 1,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("caelestis", "stellaris", "fragrans")
		});
		t.addActorTrait("giant");
		t.addActorTrait("genius");
		t.addSubspeciesTrait("hyper_intelligence");
		t.addSubspeciesTraitEvolution("mutation_skin_light_orb");
		t.addClanTrait("gods_chosen");
		t.addCultureTrait("serenity_now");
		t.addCultureTrait("ancestors_knowledge");
		t.addLanguageTrait("font_of_gods");
		t.addLanguageTrait("magic_words");
		t.addReligionTrait("rite_of_falling_stars");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_mythril");
		t.addTree("celestial_tree", 10);
		t.addTree("celestial_tree_small");
		t.addUnit("unicorn");
		t.addSapientUnit("civ_unicorn");
		t.addPlant("celestial_plant");
		add(new BiomeAsset
		{
			id = "biome_clover",
			tile_low = "clover_low",
			tile_high = "clover_high",
			localized_key = "Clover",
			loot_generation = 3,
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_water = true,
			generator_pot_amount = 2,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("luckii", "trifolii", "prosperus")
		});
		t.addActorTrait("lucky");
		t.addSubspeciesTrait("death_grow_plant");
		t.addCultureTrait("elder_reverence");
		t.addLanguageTrait("stylish_writing");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_gems");
		t.addUnit("rabbit");
		t.addUnit("cow");
		t.addUnit("butterfly", 5);
		t.addUnit("bee", 3);
		t.addUnit("fairy");
		t.addSapientUnit("civ_rabbit");
		t.addSapientUnit("civ_cow");
		t.addTree("clover_tree");
		t.addPlant("clover_plant");
		t.addBush("fruit_bush");
		add(new BiomeAsset
		{
			id = "biome_singularity",
			tile_low = "singularity_low",
			tile_high = "singularity_high",
			localized_key = "Singularity Swamp",
			loot_generation = 3,
			dark_biome = true,
			grow_strength = 5,
			spread_biome = true,
			spread_by_drops_coffee = true,
			generator_pot_amount = 0,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("singularis", "infinitus", "quantumus")
		});
		t.addActorTrait("tiny");
		t.addActorTrait("long_liver");
		t.addSubspeciesTrait("big_stomach");
		t.addSubspeciesTraitEvolution("mutation_skin_void");
		t.addCultureTrait("ancestors_knowledge");
		t.addCultureTrait("gossip_lovers");
		t.addLanguageTrait("repeated_sentences");
		t.addReligionTrait("teleport");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addUnit("angle");
		t.addSapientUnit("angle");
		t.addTree("singularity_tree");
		t.addPlant("singularity_plant");
		add(new BiomeAsset
		{
			id = "biome_paradox",
			tile_low = "paradox_low",
			tile_high = "paradox_high",
			localized_key = "Paradox",
			grow_strength = 5,
			spread_biome = true,
			generator_pot_amount = 0,
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_type_selector_trees = TileActionLibrary.getGrowTypeRandomTrees,
			grow_type_selector_plants = TileActionLibrary.getGrowTypeRandomPlants,
			grow_type_selector_bushes = TileActionLibrary.getGrowTypeRandomBushes,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("temporis", "tempus", "chronos")
		});
		t.addActorTrait("long_liver");
		t.addSubspeciesTrait("rapid_aging");
		t.addSubspeciesTrait("long_lifespan");
		t.addSubspeciesTraitEvolution("mutation_skin_fractal");
		t.addClanTrait("blood_of_eons");
		t.addCultureTrait("reading_lovers");
		t.addCultureTrait("youth_reverence");
		t.addCultureTrait("ancestors_knowledge");
		t.addCultureTrait("elder_reverence");
		t.addCultureTrait("expansionists");
		t.addLanguageTrait("eternal_text");
		t.addLanguageTrait("confusing_semantics");
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals", 3);
		t.addMineral("mineral_mythril");
		t.addTree("paradox_tree");
		t.addUnit("scorpion");
		t.addSapientUnit("alien");
		t.addSapientUnit("civ_scorpion");
		t.addPlant("paradox_plant");
	}

	private void addSpecialBiomes()
	{
		add(new BiomeAsset
		{
			id = "biome_sand",
			localized_key = "Sand",
			grow_type_selector_trees = TileActionLibrary.getGrowTypeSand,
			grow_vegetation_auto = true,
			special_biome = true,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("granitius", "arenarius", "sabulum")
		});
		t.addSubspeciesTrait("metamorphosis_crab");
		t.addUnit("crab");
		t.addSapientUnit("civ_crab");
		add(new BiomeAsset
		{
			id = "biome_hill",
			localized_key = "Hills",
			grow_type_selector_minerals = TileActionLibrary.getGrowTypeRandomMineral,
			grow_vegetation_auto = true,
			special_biome = true,
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("hillus", "stonus", "absolutus")
		});
		t.addMineral("mineral_stone", 4);
		t.addMineral("mineral_metals");
	}

	private void addCreepBiomes()
	{
		add(new BiomeAsset
		{
			id = "biome_biomass",
			tile_low = "biomass_low",
			tile_high = "biomass_high",
			localized_key = "Biomass",
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("organicus", "vivum")
		});
		t.addSubspeciesTraitEvolution("mutation_skin_abomination");
		add(new BiomeAsset
		{
			id = "biome_cybertile",
			tile_low = "cybertile_low",
			tile_high = "cybertile_high",
			localized_key = "Cybercore",
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("machinus", "cyberneticus", "technus")
		});
		t.addSubspeciesTraitEvolution("mutation_skin_metalic_orb");
		add(new BiomeAsset
		{
			id = "biome_pumpkin",
			tile_low = "pumpkin_low",
			tile_high = "pumpkin_high",
			localized_key = "Super Pumpkin",
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("maximus", "pepo", "cucurbitae")
		});
		add(new BiomeAsset
		{
			id = "biome_tumor",
			tile_low = "tumor_low",
			tile_high = "tumor_high",
			localized_key = "Tumor",
			subspecies_name_suffix = AssetLibrary<BiomeAsset>.a<string>("neoplasmis", "cancrosus", "tumoralis")
		});
		t.addSubspeciesTraitEvolution("mutation_skin_blood_vortex");
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (BiomeAsset item in list)
		{
			addBiomeToPool(item);
		}
	}

	private void addBiomeToPool(BiomeAsset pAsset)
	{
		for (int i = 0; i < pAsset.generator_pot_amount; i++)
		{
			pool_biomes.Add(pAsset);
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (BiomeAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}
}
