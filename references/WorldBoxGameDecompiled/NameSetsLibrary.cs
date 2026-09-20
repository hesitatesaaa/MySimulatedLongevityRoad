using System;
using System.Collections.Generic;

[Serializable]
public class NameSetsLibrary : AssetLibrary<NameSetAsset>
{
	public override void init()
	{
		base.init();
		addHumanLike();
		addFantasyRaces();
		addAnimalRaces();
		addMobRaces();
	}

	private void addMobRaces()
	{
		add(new NameSetAsset
		{
			id = "default_set",
			city = "default_name",
			clan = "default_name",
			culture = "default_name",
			family = "default_name",
			kingdom = "default_name",
			language = "default_name",
			unit = "default_name",
			religion = "default_name"
		});
		add(new NameSetAsset
		{
			id = "cold_one_set",
			city = "cold_one_name",
			clan = "cold_one_name",
			culture = "cold_one_name",
			family = "cold_one_name",
			kingdom = "cold_one_name",
			language = "cold_one_name",
			unit = "cold_one_name",
			religion = "cold_one_name"
		});
		add(new NameSetAsset
		{
			id = "necromancer_set",
			city = "necromancer_name",
			clan = "necromancer_name",
			culture = "necromancer_name",
			family = "necromancer_name",
			kingdom = "necromancer_name",
			language = "necromancer_name",
			unit = "necromancer_name",
			religion = "necromancer_name"
		});
		add(new NameSetAsset
		{
			id = "druid_set",
			city = "flower_name",
			clan = "flower_name",
			culture = "flower_name",
			family = "flower_name",
			kingdom = "flower_name",
			language = "flower_name",
			unit = "flower_name",
			religion = "flower_name"
		});
		add(new NameSetAsset
		{
			id = "plague_doctor_set",
			city = "white_mage_name",
			clan = "white_mage_name",
			culture = "white_mage_name",
			family = "white_mage_name",
			kingdom = "white_mage_name",
			language = "white_mage_name",
			unit = "white_mage_name",
			religion = "white_mage_name"
		});
		add(new NameSetAsset
		{
			id = "white_mage_set",
			city = "white_mage_name",
			clan = "white_mage_name",
			culture = "white_mage_name",
			family = "white_mage_name",
			kingdom = "white_mage_name",
			language = "white_mage_name",
			unit = "white_mage_name",
			religion = "white_mage_name"
		});
		add(new NameSetAsset
		{
			id = "evil_mage_set",
			city = "evil_mage_name",
			clan = "evil_mage_name",
			culture = "evil_mage_name",
			family = "evil_mage_name",
			kingdom = "evil_mage_name",
			language = "evil_mage_name",
			unit = "evil_mage_name",
			religion = "evil_mage_name"
		});
		add(new NameSetAsset
		{
			id = "skeleton_set",
			city = "skeleton_name",
			clan = "skeleton_name",
			culture = "skeleton_name",
			family = "skeleton_name",
			kingdom = "skeleton_name",
			language = "skeleton_name",
			unit = "skeleton_name",
			religion = "skeleton_name"
		});
		add(new NameSetAsset
		{
			id = "jumpy_skull_set",
			city = "jumpy_skull_name",
			clan = "jumpy_skull_name",
			culture = "jumpy_skull_name",
			family = "jumpy_skull_name",
			kingdom = "jumpy_skull_name",
			language = "jumpy_skull_name",
			unit = "jumpy_skull_name",
			religion = "jumpy_skull_name"
		});
		add(new NameSetAsset
		{
			id = "fire_skull_set",
			city = "fire_skull_name",
			clan = "fire_skull_name",
			culture = "fire_skull_name",
			family = "fire_skull_name",
			kingdom = "fire_skull_name",
			language = "fire_skull_name",
			unit = "fire_skull_name",
			religion = "fire_skull_name"
		});
		add(new NameSetAsset
		{
			id = "demon_set",
			city = "demon_name",
			clan = "demon_name",
			culture = "demon_name",
			family = "demon_name",
			kingdom = "demon_name",
			language = "demon_name",
			unit = "demon_name",
			religion = "demon_name"
		});
		add(new NameSetAsset
		{
			id = "angle_set",
			city = "angle_name",
			clan = "angle_name",
			culture = "angle_name",
			family = "angle_name",
			kingdom = "angle_name",
			language = "angle_name",
			unit = "angle_name",
			religion = "angle_name"
		});
		add(new NameSetAsset
		{
			id = "fairy_set",
			city = "fairy_name",
			clan = "fairy_name",
			culture = "fairy_name",
			family = "fairy_name",
			kingdom = "fairy_name",
			language = "fairy_name",
			unit = "fairy_name",
			religion = "fairy_name"
		});
		add(new NameSetAsset
		{
			id = "bandit_set",
			city = "bandit_name",
			clan = "bandit_name",
			culture = "bandit_name",
			family = "bandit_name",
			kingdom = "bandit_name",
			language = "bandit_name",
			unit = "bandit_name",
			religion = "bandit_name"
		});
		add(new NameSetAsset
		{
			id = "snowman_set",
			city = "snowman_name",
			clan = "snowman_name",
			culture = "snowman_name",
			family = "snowman_name",
			kingdom = "snowman_name",
			language = "snowman_name",
			unit = "snowman_name",
			religion = "snowman_name"
		});
		add(new NameSetAsset
		{
			id = "alien_set",
			city = "alien_name",
			clan = "alien_name",
			culture = "alien_name",
			family = "alien_name",
			kingdom = "alien_name",
			language = "alien_name",
			unit = "alien_name",
			religion = "alien_name"
		});
		add(new NameSetAsset
		{
			id = "fire_elemental_set",
			city = "fire_elemental_name",
			clan = "fire_elemental_name",
			culture = "fire_elemental_name",
			family = "fire_elemental_name",
			kingdom = "fire_elemental_name",
			language = "fire_elemental_name",
			unit = "fire_elemental_name",
			religion = "fire_elemental_name"
		});
		add(new NameSetAsset
		{
			id = "pumpkin_set",
			city = "lil_pumpkin_name",
			clan = "lil_pumpkin_name",
			culture = "lil_pumpkin_name",
			family = "lil_pumpkin_name",
			kingdom = "lil_pumpkin_name",
			language = "lil_pumpkin_name",
			unit = "lil_pumpkin_name",
			religion = "lil_pumpkin_name"
		});
		add(new NameSetAsset
		{
			id = "assimilator_set",
			city = "assimilator_name",
			clan = "assimilator_name",
			culture = "assimilator_name",
			family = "assimilator_name",
			kingdom = "assimilator_name",
			language = "assimilator_name",
			unit = "assimilator_name",
			religion = "assimilator_name"
		});
		add(new NameSetAsset
		{
			id = "bioblob_set",
			city = "bioblob_name",
			clan = "bioblob_name",
			culture = "bioblob_name",
			family = "bioblob_name",
			kingdom = "bioblob_name",
			language = "bioblob_name",
			unit = "bioblob_name",
			religion = "bioblob_name"
		});
		add(new NameSetAsset
		{
			id = "greg_set",
			city = "greg_name",
			clan = "greg_name",
			culture = "greg_name",
			family = "greg_name",
			kingdom = "greg_name",
			language = "greg_name",
			unit = "greg_name",
			religion = "greg_name"
		});
		add(new NameSetAsset
		{
			id = "living_plant_set",
			city = "living_plant_name",
			clan = "living_plant_name",
			culture = "living_plant_name",
			family = "living_plant_name",
			kingdom = "living_plant_name",
			language = "living_plant_name",
			unit = "living_plant_name",
			religion = "living_plant_name"
		});
		add(new NameSetAsset
		{
			id = "living_house_set",
			city = "living_house_name",
			clan = "living_house_name",
			culture = "living_house_name",
			family = "living_house_name",
			kingdom = "living_house_name",
			language = "living_house_name",
			unit = "living_house_name",
			religion = "living_house_name"
		});
	}

	private void addAnimalRaces()
	{
		add(new NameSetAsset
		{
			id = "cat_set",
			city = "cat_name",
			clan = "cat_name",
			culture = "cat_name",
			family = "cat_name",
			kingdom = "cat_name",
			language = "cat_name",
			unit = "cat_name",
			religion = "cat_name"
		});
		add(new NameSetAsset
		{
			id = "wolf_set",
			city = "wolf_name",
			clan = "wolf_name",
			culture = "wolf_name",
			family = "wolf_name",
			kingdom = "wolf_name",
			language = "wolf_name",
			unit = "wolf_name",
			religion = "wolf_name"
		});
		add(new NameSetAsset
		{
			id = "fox_set",
			city = "fox_name",
			clan = "fox_name",
			culture = "fox_name",
			family = "fox_name",
			kingdom = "fox_name",
			language = "fox_name",
			unit = "fox_name",
			religion = "fox_name"
		});
		add(new NameSetAsset
		{
			id = "chicken_set",
			city = "chicken_name",
			clan = "chicken_name",
			culture = "chicken_name",
			family = "chicken_name",
			kingdom = "chicken_name",
			language = "chicken_name",
			unit = "chicken_name",
			religion = "chicken_name"
		});
		add(new NameSetAsset
		{
			id = "rabbit_set",
			city = "rabbit_name",
			clan = "rabbit_name",
			culture = "rabbit_name",
			family = "rabbit_name",
			kingdom = "rabbit_name",
			language = "rabbit_name",
			unit = "rabbit_name",
			religion = "rabbit_name"
		});
		add(new NameSetAsset
		{
			id = "monkey_set",
			city = "monkey_name",
			clan = "monkey_name",
			culture = "monkey_name",
			family = "monkey_name",
			kingdom = "monkey_name",
			language = "monkey_name",
			unit = "monkey_name",
			religion = "monkey_name"
		});
		add(new NameSetAsset
		{
			id = "sheep_set",
			city = "sheep_name",
			clan = "sheep_name",
			culture = "sheep_name",
			family = "sheep_name",
			kingdom = "sheep_name",
			language = "sheep_name",
			unit = "sheep_name",
			religion = "sheep_name"
		});
		add(new NameSetAsset
		{
			id = "cow_set",
			city = "cow_name",
			clan = "cow_name",
			culture = "cow_name",
			family = "cow_name",
			kingdom = "cow_name",
			language = "cow_name",
			unit = "cow_name",
			religion = "cow_name"
		});
		add(new NameSetAsset
		{
			id = "armadillo_set",
			city = "armadillo_name",
			clan = "armadillo_name",
			culture = "armadillo_name",
			family = "armadillo_name",
			kingdom = "armadillo_name",
			language = "armadillo_name",
			unit = "armadillo_name",
			religion = "armadillo_name"
		});
		add(new NameSetAsset
		{
			id = "bear_set",
			city = "bear_name",
			clan = "bear_name",
			culture = "bear_name",
			family = "bear_name",
			kingdom = "bear_name",
			language = "bear_name",
			unit = "bear_name",
			religion = "bear_name"
		});
		add(new NameSetAsset
		{
			id = "rhino_set",
			city = "rhino_name",
			clan = "rhino_name",
			culture = "rhino_name",
			family = "rhino_name",
			kingdom = "rhino_name",
			language = "rhino_name",
			unit = "rhino_name",
			religion = "rhino_name"
		});
		add(new NameSetAsset
		{
			id = "buffalo_set",
			city = "buffalo_name",
			clan = "buffalo_name",
			culture = "buffalo_name",
			family = "buffalo_name",
			kingdom = "buffalo_name",
			language = "buffalo_name",
			unit = "buffalo_name",
			religion = "buffalo_name"
		});
		add(new NameSetAsset
		{
			id = "rat_set",
			city = "rat_name",
			clan = "rat_name",
			culture = "rat_name",
			family = "rat_name",
			kingdom = "rat_name",
			language = "rat_name",
			unit = "rat_name",
			religion = "rat_name"
		});
		add(new NameSetAsset
		{
			id = "capybara_set",
			city = "capybara_name",
			clan = "capybara_name",
			culture = "capybara_name",
			family = "capybara_name",
			kingdom = "capybara_name",
			language = "capybara_name",
			unit = "capybara_name",
			religion = "capybara_name"
		});
		add(new NameSetAsset
		{
			id = "goat_set",
			city = "goat_name",
			clan = "goat_name",
			culture = "goat_name",
			family = "goat_name",
			kingdom = "goat_name",
			language = "goat_name",
			unit = "goat_name",
			religion = "goat_name"
		});
		add(new NameSetAsset
		{
			id = "raccoon_set",
			city = "raccoon_name",
			clan = "raccoon_name",
			culture = "raccoon_name",
			family = "raccoon_name",
			kingdom = "raccoon_name",
			language = "raccoon_name",
			unit = "raccoon_name",
			religion = "raccoon_name"
		});
		add(new NameSetAsset
		{
			id = "unicorn_set",
			city = "white_mage_name",
			clan = "white_mage_name",
			culture = "white_mage_name",
			family = "white_mage_name",
			kingdom = "white_mage_name",
			language = "white_mage_name",
			unit = "white_mage_name",
			religion = "white_mage_name"
		});
		add(new NameSetAsset
		{
			id = "seal_set",
			city = "default_name",
			clan = "default_name",
			culture = "default_name",
			family = "default_name",
			kingdom = "default_name",
			language = "default_name",
			unit = "default_name",
			religion = "default_name"
		});
		add(new NameSetAsset
		{
			id = "ostrich_set",
			city = "default_name",
			clan = "default_name",
			culture = "default_name",
			family = "default_name",
			kingdom = "default_name",
			language = "default_name",
			unit = "default_name",
			religion = "default_name"
		});
		add(new NameSetAsset
		{
			id = "scorpion_set",
			city = "scorpion_name",
			clan = "scorpion_name",
			culture = "scorpion_name",
			family = "scorpion_name",
			kingdom = "scorpion_name",
			language = "scorpion_name",
			unit = "scorpion_name",
			religion = "scorpion_name"
		});
		add(new NameSetAsset
		{
			id = "crab_set",
			city = "crab_name",
			clan = "crab_name",
			culture = "crab_name",
			family = "crab_name",
			kingdom = "crab_name",
			language = "crab_name",
			unit = "crab_name",
			religion = "crab_name"
		});
		add(new NameSetAsset
		{
			id = "penguin_set",
			city = "penguin_name",
			clan = "penguin_name",
			culture = "penguin_name",
			family = "penguin_name",
			kingdom = "penguin_name",
			language = "penguin_name",
			unit = "penguin_name",
			religion = "penguin_name"
		});
		add(new NameSetAsset
		{
			id = "turtle_set",
			city = "turtle_name",
			clan = "turtle_name",
			culture = "turtle_name",
			family = "turtle_name",
			kingdom = "turtle_name",
			language = "turtle_name",
			unit = "turtle_name",
			religion = "turtle_name"
		});
		add(new NameSetAsset
		{
			id = "crocodile_set",
			city = "crocodile_name",
			clan = "crocodile_name",
			culture = "crocodile_name",
			family = "crocodile_name",
			kingdom = "crocodile_name",
			language = "crocodile_name",
			unit = "crocodile_name",
			religion = "crocodile_name"
		});
		add(new NameSetAsset
		{
			id = "frog_set",
			city = "frog_name",
			clan = "frog_name",
			culture = "frog_name",
			family = "frog_name",
			kingdom = "frog_name",
			language = "frog_name",
			unit = "frog_name",
			religion = "frog_name"
		});
		add(new NameSetAsset
		{
			id = "piranha_set",
			city = "piranha_name",
			clan = "piranha_name",
			culture = "piranha_name",
			family = "piranha_name",
			kingdom = "piranha_name",
			language = "piranha_name",
			unit = "piranha_name",
			religion = "piranha_name"
		});
		add(new NameSetAsset
		{
			id = "flower_set",
			city = "flower_name",
			clan = "flower_name",
			culture = "flower_name",
			family = "flower_name",
			kingdom = "flower_name",
			language = "flower_name",
			unit = "flower_name",
			religion = "flower_name"
		});
		add(new NameSetAsset
		{
			id = "garlic_man_set",
			city = "garlic_name",
			clan = "garlic_name",
			culture = "garlic_name",
			family = "garlic_name",
			kingdom = "garlic_name",
			language = "garlic_name",
			unit = "garlic_name",
			religion = "garlic_name"
		});
		add(new NameSetAsset
		{
			id = "lemon_man_set",
			city = "lemon_name",
			clan = "lemon_name",
			culture = "lemon_name",
			family = "lemon_name",
			kingdom = "lemon_name",
			language = "lemon_name",
			unit = "lemon_name",
			religion = "lemon_name"
		});
		add(new NameSetAsset
		{
			id = "acid_blob_set",
			city = "acid_blob_name",
			clan = "acid_blob_name",
			culture = "acid_blob_name",
			family = "acid_blob_name",
			kingdom = "acid_blob_name",
			language = "acid_blob_name",
			unit = "acid_blob_name",
			religion = "acid_blob_name"
		});
		add(new NameSetAsset
		{
			id = "crystal_golem_set",
			city = "crystal_sword_name",
			clan = "crystal_sword_name",
			culture = "crystal_sword_name",
			family = "crystal_sword_name",
			kingdom = "crystal_sword_name",
			language = "crystal_sword_name",
			unit = "crystal_sword_name",
			religion = "crystal_sword_name"
		});
		add(new NameSetAsset
		{
			id = "crystal_sword_set",
			city = "crystal_sword_name",
			clan = "crystal_sword_name",
			culture = "crystal_sword_name",
			family = "crystal_sword_name",
			kingdom = "crystal_sword_name",
			language = "crystal_sword_name",
			unit = "crystal_sword_name",
			religion = "crystal_sword_name"
		});
		add(new NameSetAsset
		{
			id = "smore_set",
			city = "candy_name",
			clan = "candy_name",
			culture = "candy_name",
			family = "candy_name",
			kingdom = "candy_name",
			language = "candy_name",
			unit = "candy_name",
			religion = "candy_name"
		});
		add(new NameSetAsset
		{
			id = "insect_set",
			city = "bug_name",
			clan = "bug_name",
			culture = "bug_name",
			family = "bug_name",
			kingdom = "bug_name",
			language = "bug_name",
			unit = "bug_name",
			religion = "bug_name"
		});
		add(new NameSetAsset
		{
			id = "candy_man_set",
			city = "candy_name",
			clan = "candy_name",
			culture = "candy_name",
			family = "candy_name",
			kingdom = "candy_name",
			language = "candy_name",
			unit = "candy_name",
			religion = "candy_name"
		});
		add(new NameSetAsset
		{
			id = "alpaca_set",
			city = "alpaca_name",
			clan = "alpaca_name",
			culture = "alpaca_name",
			family = "alpaca_name",
			kingdom = "alpaca_name",
			language = "alpaca_name",
			unit = "alpaca_name",
			religion = "alpaca_name"
		});
		add(new NameSetAsset
		{
			id = "hyena_set",
			city = "hyena_city",
			clan = "hyena_name",
			culture = "hyena_name",
			family = "hyena_name",
			kingdom = "hyena_name",
			language = "hyena_name",
			unit = "hyena_name",
			religion = "hyena_name"
		});
		add(new NameSetAsset
		{
			id = "snake_set",
			city = "snake_city",
			clan = "snake_name",
			culture = "snake_name",
			family = "snake_name",
			kingdom = "snake_name",
			language = "snake_name",
			unit = "snake_name",
			religion = "snake_name"
		});
	}

	private void addFantasyRaces()
	{
		add(new NameSetAsset
		{
			id = "dwarf_default_set",
			city = "dwarf_city",
			clan = "dwarf_clan",
			culture = "dwarf_culture",
			family = "dwarf_clan",
			kingdom = "dwarf_kingdom",
			language = "dwarf_language",
			unit = "dwarf_unit",
			religion = "dwarf_religion"
		});
		add(new NameSetAsset
		{
			id = "dwarf_nordic_set",
			city = "nordic_city",
			clan = "nordic_clan",
			culture = "dwarf_culture",
			family = "nordic_clan",
			kingdom = "nordic_kingdom",
			language = "dwarf_language",
			unit = "nordic_unit",
			religion = "dwarf_religion"
		});
		add(new NameSetAsset
		{
			id = "elf_default_set",
			city = "elf_city",
			clan = "elf_clan",
			culture = "elf_culture",
			family = "elf_clan",
			kingdom = "elf_kingdom",
			language = "elf_language",
			unit = "elf_unit",
			religion = "elf_religion"
		});
		add(new NameSetAsset
		{
			id = "orc_default_set",
			city = "orc_city",
			clan = "orc_clan",
			culture = "orc_culture",
			family = "orc_clan",
			kingdom = "orc_kingdom",
			language = "orc_language",
			unit = "orc_unit",
			religion = "orc_religion"
		});
	}

	private void addHumanLike()
	{
		add(new NameSetAsset
		{
			id = "human_default_set",
			city = "human_city",
			clan = "human_clan",
			culture = "human_culture",
			family = "human_clan",
			kingdom = "human_kingdom",
			language = "human_language",
			unit = "human_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_slavic_set",
			city = "slavic_city",
			clan = "slavic_clan",
			culture = "human_culture",
			family = "slavic_clan",
			kingdom = "slavic_kingdom",
			language = "human_language",
			unit = "slavic_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_germanic_set",
			city = "germanic_city",
			clan = "germanic_clan",
			culture = "human_culture",
			family = "germanic_clan",
			kingdom = "germanic_kingdom",
			language = "human_language",
			unit = "germanic_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_rus_set",
			city = "rus_city",
			clan = "rus_clan",
			culture = "human_culture",
			family = "rus_clan",
			kingdom = "rus_kingdom",
			language = "human_language",
			unit = "rus_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_posh_set",
			city = "posh_city",
			clan = "posh_clan",
			culture = "human_culture",
			family = "posh_clan",
			kingdom = "posh_kingdom",
			language = "human_language",
			unit = "posh_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_folk_set",
			city = "folk_city",
			clan = "folk_clan",
			culture = "human_culture",
			family = "folk_clan",
			kingdom = "folk_kingdom",
			language = "human_language",
			unit = "folk_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_pomeranian_set",
			city = "pomeranian_city",
			clan = "pomeranian_clan",
			culture = "human_culture",
			family = "pomeranian_clan",
			kingdom = "pomeranian_kingdom",
			language = "human_language",
			unit = "pomeranian_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_frankish_set",
			city = "frankish_city",
			clan = "frankish_clan",
			culture = "human_culture",
			family = "frankish_clan",
			kingdom = "frankish_kingdom",
			language = "human_language",
			unit = "frankish_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_rome_set",
			city = "rome_city",
			clan = "rome_clan",
			culture = "human_culture",
			family = "rome_clan",
			kingdom = "rome_kingdom",
			language = "human_language",
			unit = "rome_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_iberian_set",
			city = "iberian_city",
			clan = "iberian_clan",
			culture = "human_culture",
			family = "iberian_clan",
			kingdom = "iberian_kingdom",
			language = "human_language",
			unit = "iberian_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "human_monolux_set",
			city = "monolux_city",
			clan = "monolux_clan",
			culture = "human_culture",
			family = "monolux_clan",
			kingdom = "monolux_kingdom",
			language = "human_language",
			unit = "monolux_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "polynesian_set",
			city = "poly_city",
			clan = "poly_clan",
			culture = "human_culture",
			family = "poly_clan",
			kingdom = "poly_kingdom",
			language = "human_language",
			unit = "poly_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "nomads_set",
			city = "nomad_city",
			clan = "nomad_clan",
			culture = "human_culture",
			family = "nomad_clan",
			kingdom = "nomad_kingdom",
			language = "human_language",
			unit = "nomad_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "sino_set",
			city = "sino_city",
			clan = "sino_clan",
			culture = "human_culture",
			family = "sino_clan",
			kingdom = "sino_kingdom",
			language = "human_language",
			unit = "sino_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "siam_set",
			city = "siam_city",
			clan = "siam_clan",
			culture = "human_culture",
			family = "siam_clan",
			kingdom = "siam_kingdom",
			language = "human_language",
			unit = "siam_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "vishna_set",
			city = "vishna_city",
			clan = "vishna_clan",
			culture = "human_culture",
			family = "vishna_family",
			kingdom = "vishna_kingdom",
			language = "human_language",
			unit = "vishna_unit",
			religion = "human_religion"
		});
		add(new NameSetAsset
		{
			id = "nihon_set",
			city = "nihon_city",
			clan = "nihon_clan",
			culture = "human_culture",
			family = "nihon_clan",
			kingdom = "nihon_kingdom",
			language = "human_language",
			unit = "nihon_unit",
			religion = "human_religion"
		});
	}

	public override void editorDiagnostic()
	{
		using ListPool<string> listPool = new ListPool<string>(8);
		using ListPool<string> listPool2 = new ListPool<string>(32);
		foreach (NameSetAsset item in list)
		{
			listPool.Clear();
			(AssetManager.name_generator.has(item.city) ? listPool2 : listPool).Add(item.city);
			(AssetManager.name_generator.has(item.clan) ? listPool2 : listPool).Add(item.clan);
			(AssetManager.name_generator.has(item.culture) ? listPool2 : listPool).Add(item.culture);
			(AssetManager.name_generator.has(item.family) ? listPool2 : listPool).Add(item.family);
			(AssetManager.name_generator.has(item.kingdom) ? listPool2 : listPool).Add(item.kingdom);
			(AssetManager.name_generator.has(item.language) ? listPool2 : listPool).Add(item.language);
			(AssetManager.name_generator.has(item.unit) ? listPool2 : listPool).Add(item.unit);
			(AssetManager.name_generator.has(item.religion) ? listPool2 : listPool).Add(item.religion);
			foreach (ref string item2 in listPool)
			{
				string current2 = item2;
				BaseAssetLibrary.logAssetError("<e>" + item.id + "</e>: Missing name generator", current2);
			}
		}
		foreach (string item3 in new HashSet<string>(listPool2))
		{
			if (!AssetManager.name_generator.get(item3).hasOnomastics())
			{
				BaseAssetLibrary.logAssetError("Name generator is missing onomastics", item3);
			}
		}
		base.editorDiagnostic();
	}
}
