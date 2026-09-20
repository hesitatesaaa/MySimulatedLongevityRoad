using System;
using System.Collections.Generic;
using System.IO;
using UnityPools;

public class KingdomLibrary : AssetLibrary<KingdomAsset>
{
	private const string TEMPLATE_MOB = "$TEMPLATE_MOB$";

	private const string TEMPLATE_MOB_GOOD = "$TEMPLATE_MOB_GOOD$";

	private const string TEMPLATE_MOB_VERY_GOOD = "$TEMPLATE_MOB_VERY_GOOD$";

	private const string TEMPLATE_ANIMAL = "$TEMPLATE_ANIMAL$";

	private const string TEMPLATE_ANIMAL_NEUTRAL = "$TEMPLATE_ANIMAL_NEUTRAL$";

	private const string TEMPLATE_ANIMAL_PEACEFUL = "$TEMPLATE_ANIMAL_PEACEFUL$";

	private const string TEMPLATE_CIV = "$TEMPLATE_CIV$";

	private const string TEMPLATE_CIV_GOOD = "$TEMPLATE_CIV_GOOD$";

	private const string TEMPLATE_NOMAD = "$TEMPLATE_NOMAD$";

	private const string TEMPLATE_CIV_NEW = "$TEMPLATE_CIV_NEW$";

	private ColorAsset _shared_default_color;

	public override void init()
	{
		base.init();
		_shared_default_color = ColorAsset.tryMakeNewColorAsset("#888888");
		_shared_default_color.id = "SHARED_COLOR";
		addTemplates();
		addNeutral();
		addNomads();
		addNewCivs();
		addCivs();
		addAnimals();
		addUnique();
		addMobs();
		addAnimalMinicivs();
		addCoolMinicivs();
		addCreeps();
	}

	private void addTemplates()
	{
		add(new KingdomAsset
		{
			id = "$TEMPLATE_MOB$",
			mobs = true
		});
		clone("$TEMPLATE_MOB_GOOD$", "$TEMPLATE_MOB$");
		t.addTag("good");
		t.addFriendlyTag("good");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("civ");
		t.addEnemyTag("orc");
		t.addEnemyTag("bandit");
		clone("$TEMPLATE_MOB_VERY_GOOD$", "$TEMPLATE_MOB_GOOD$");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("living_plants");
		t.addFriendlyTag("snowman");
		t.addEnemyTag("wolf");
		t.addEnemyTag("bear");
		clone("$TEMPLATE_ANIMAL$", "$TEMPLATE_MOB$");
		t.addTag("nature_creature");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("neutral_animals");
		clone("$TEMPLATE_ANIMAL_NEUTRAL$", "$TEMPLATE_MOB$");
		t.count_as_danger = false;
		t.addTag("neutral_animals");
		t.addTag("neutral");
		clone("$TEMPLATE_ANIMAL_PEACEFUL$", "$TEMPLATE_ANIMAL_NEUTRAL$");
		t.addFriendlyTag("good");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("civ");
		add(new KingdomAsset
		{
			id = "$TEMPLATE_CIV$",
			civ = true
		});
		t.addTag("civ");
		t.addEnemyTag("bandit");
		clone("$TEMPLATE_CIV_GOOD$", "$TEMPLATE_CIV$");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("good");
		clone("$TEMPLATE_NOMAD$", "$TEMPLATE_CIV_GOOD$");
		t.addFriendlyTag("neutral");
		t.civ = false;
		t.mobs = true;
		t.nomads = true;
		clone("$TEMPLATE_CIV_NEW$", "$TEMPLATE_CIV_GOOD$");
		t.addFriendlyTag("neutral");
	}

	private void addNomads()
	{
		clone("nomads_human", "$TEMPLATE_NOMAD$");
		t.group_main = true;
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#BACADD");
		t.setIcon("ui/Icons/iconHumans");
		t.addTag("human");
		t.addTag("sliceable");
		t.addFriendlyTag("human");
		clone("nomads_elf", "$TEMPLATE_NOMAD$");
		t.group_main = true;
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#98DB8C");
		t.setIcon("ui/Icons/iconElves");
		t.addTag("elf");
		t.addTag("nature_creature");
		t.addTag("sliceable");
		t.addFriendlyTag("elf");
		t.addFriendlyTag("nature_creature");
		clone("nomads_orc", "$TEMPLATE_NOMAD$");
		t.group_main = true;
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#FFCD70");
		t.setIcon("ui/Icons/iconOrcs");
		t.civ = false;
		t.mobs = true;
		t.addTag("orc");
		t.addTag("sliceable");
		t.addFriendlyTag("orc");
		t.addFriendlyTag("golden_brain");
		t.addFriendlyTag("wolf");
		t.addFriendlyTag("hyena");
		clone("nomads_dwarf", "$TEMPLATE_NOMAD$");
		t.group_main = true;
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#B1A0FF");
		t.setIcon("ui/Icons/iconDwarf");
		t.addTag("dwarf");
		t.addFriendlyTag("dwarf");
		t.addFriendlyTag("civ_crystal_golem");
	}

	private void addCivs()
	{
		clone("human", "nomads_human");
		t.group_main = true;
		t.clearKingdomColor();
		t.setIcon("ui/Icons/iconHumans");
		t.civ = true;
		t.mobs = false;
		clone("elf", "nomads_elf");
		t.group_main = true;
		t.clearKingdomColor();
		t.setIcon("ui/Icons/iconElves");
		t.civ = true;
		t.mobs = false;
		clone("dwarf", "nomads_dwarf");
		t.group_main = true;
		t.clearKingdomColor();
		t.setIcon("ui/Icons/iconDwarf");
		t.civ = true;
		t.mobs = false;
		clone("orc", "nomads_orc");
		t.group_main = true;
		t.clearKingdomColor();
		t.setIcon("ui/Icons/iconOrcs");
		t.civ = true;
		t.mobs = false;
	}

	private void addNewCivs()
	{
		clone("civ_cat", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_cat");
		t.addTag("sliceable");
		clone("civ_dog", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_dog");
		t.addTag("sliceable");
		clone("civ_chicken", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_chicken");
		t.addTag("sliceable");
		clone("civ_rabbit", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_rabbit");
		t.addTag("sliceable");
		clone("civ_monkey", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_monkey");
		t.addTag("sliceable");
		clone("civ_fox", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_fox");
		t.addTag("sliceable");
		clone("civ_sheep", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_sheep");
		t.addTag("sliceable");
		clone("civ_cow", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_cow");
		t.addTag("sliceable");
		clone("civ_armadillo", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_armadillo");
		clone("civ_wolf", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_wolf");
		t.addTag("sliceable");
		clone("civ_bear", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_bear");
		t.addTag("sliceable");
		clone("civ_rhino", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_rhino");
		clone("civ_buffalo", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_buffalo");
		t.addTag("sliceable");
		clone("civ_hyena", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_hyena");
		t.addTag("sliceable");
		clone("civ_rat", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_rat");
		t.addTag("sliceable");
		clone("civ_alpaca", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_alpaca");
		t.addTag("sliceable");
		clone("civ_capybara", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_capybara");
		t.friendship_for_everyone = true;
		t.addFriendlyTag("everyone");
		clone("civ_goat", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_goat");
		t.addTag("sliceable");
		clone("civ_crab", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_crab");
		t.addFriendlyTag("crab");
		clone("civ_scorpion", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_scorpion");
		clone("civ_penguin", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_penguin");
		t.addTag("sliceable");
		clone("civ_turtle", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_turtle");
		clone("civ_crocodile", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_crocodile");
		t.addTag("sliceable");
		clone("civ_snake", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_snake");
		t.addTag("sliceable");
		t.addFriendlyTag("snake");
		t.addFriendlyTag("miniciv_snake");
		clone("civ_frog", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_frog");
		t.addTag("sliceable");
		clone("civ_piranha", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_piranha");
		clone("civ_liliar", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_liliar");
		t.addTag("sliceable");
		clone("civ_garlic_man", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_garlic_man");
		t.addTag("garlic");
		clone("civ_lemon_man", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_lemon_man");
		t.addTag("sliceable");
		clone("civ_acid_gentleman", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_acid_gentleman");
		clone("civ_crystal_golem", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_crystal_golem");
		t.addFriendlyTag("dwarf");
		clone("civ_candy_man", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_candy_man");
		clone("civ_beetle", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_beetle");
		clone("civ_seal", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_seal");
		clone("civ_unicorn", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_unicorn");
		clone("civ_ghost", "$TEMPLATE_CIV_NEW$");
		t.setIcon("ui/Icons/civs/civ_ghost");
	}

	private void addMobs()
	{
		clone("bandit", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconBandit");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#E3362F");
		t.addTag("neutral");
		t.addTag("sliceable");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("miniciv_bandit");
		t.addEnemyTag("civ");
		clone("snowman", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconSnowMan");
		t.addTag("snow");
		t.addFriendlyTag("good");
		t.addFriendlyTag("snow");
		clone("evil_mage", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconEvilMage");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#E3362F");
		t.addTag("evil");
		t.addFriendlyTag("demon");
		clone("white_mage", "$TEMPLATE_MOB_VERY_GOOD$");
		t.setIcon("ui/Icons/iconWhiteMage");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#91E1D6");
		clone("necromancer", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconNecromancer");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#81208B");
		t.addTag("evil");
		t.addFriendlyTag("undead");
		t.addFriendlyTag("miniciv_necromancer");
		t.addFriendlyTag("fly");
		t.addEnemyTag("garlic");
		clone("druid", "$TEMPLATE_MOB_GOOD$");
		t.setIcon("ui/Icons/iconDruid");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#85C32E");
		t.addTag("nature_creature");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("super_pumpkin");
		clone("plague_doctor", "$TEMPLATE_MOB_VERY_GOOD$");
		t.setIcon("ui/Icons/iconPlagueDoctor");
		clone("undead", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconZombie");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#D5D5D5");
		t.addFriendlyTag("necromancer");
		t.addEnemyTag("garlic");
		clone("cold_one", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconWalker");
		t.addTag("snow");
		t.addFriendlyTag("snow");
		clone("demon", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconDemon");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#A30000");
		t.addFriendlyTag("fire_elemental");
		clone("angle", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconAngle");
		t.addTag("good");
		t.addTag("nature_creature");
		t.addFriendlyTag("good");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("civ");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("super_pumpkin");
		t.addFriendlyTag("snowman");
		clone("aliens", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconAlien");
		t.addTag("sliceable");
		t.addFriendlyTag("assimilators");
		clone("mush", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/actor_traits/iconMushSpores");
		t.addTag("sliceable");
		t.addFriendlyTag("living_plants");
		clone("greg", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconGreg");
		t.addTag("sliceable");
		clone("fire_elemental", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconFireElemental");
		t.addFriendlyTag("demon");
		t.addFriendlyTag("dragons");
		t.addFriendlyTag("fire_skull");
		clone("dragons", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconDragon");
		t.addTag("sliceable");
		t.addFriendlyTag("fire_elemental");
		clone("living_plants", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconLivingPlants");
		t.addTag("nature_creature");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("good");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("mush");
		clone("living_houses", "$TEMPLATE_MOB$");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#E53B3B");
		t.setIcon("ui/Icons/iconLivingHouse");
		t.addFriendlyTag("living_houses");
		clone("fire_skull", "$TEMPLATE_MOB$");
		t.addTag("undead");
		t.addTag("demon");
		t.setIcon("ui/Icons/iconFireSkull");
		t.addFriendlyTag("demon");
		t.addFriendlyTag("dragons");
		t.addFriendlyTag("undead");
		t.addFriendlyTag("fire_elemental");
		clone("jumpy_skull", "$TEMPLATE_MOB$");
		t.addTag("undead");
		t.setIcon("ui/Icons/iconJumpySkull");
		t.addFriendlyTag("undead");
		t.addFriendlyTag("fire_skull");
		t.addFriendlyTag("necromancer");
		clone("fairy", "good");
		t.setIcon("ui/Icons/iconFairy");
		t.addTag("good");
	}

	private void addAnimals()
	{
		clone("cat", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconCat");
		t.addTag("small");
		t.addTag("sliceable");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("snowman");
		t.addEnemyTag("snake");
		clone("dog", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconDog");
		t.addTag("sliceable");
		t.addFriendlyTag("wolf");
		t.addFriendlyTag("human");
		t.addEnemyTag("cat");
		clone("chicken", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconChicken");
		t.addTag("small");
		t.addTag("sliceable");
		clone("rabbit", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconRabbit");
		t.addTag("small");
		t.addTag("sliceable");
		clone("monkey", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconMonkey");
		t.addTag("sliceable");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("snowman");
		t.addEnemyTag("snake");
		clone("fox", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconFox");
		t.addTag("sliceable");
		t.addFriendlyTag("wolf");
		t.addFriendlyTag("bear");
		clone("sheep", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconSheep");
		t.addTag("sliceable");
		clone("cow", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconCow");
		t.addTag("sliceable");
		clone("armadillo", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconArmadillo");
		clone("raccoon", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconRaccoon");
		t.addTag("sliceable");
		t.addTag("small");
		t.addFriendlyTag("bandit");
		clone("wolf", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconWolf");
		t.addTag("sliceable");
		t.addFriendlyTag("orc");
		t.addFriendlyTag("dog");
		t.addFriendlyTag("living_houses");
		clone("bear", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconBear");
		t.addTag("sliceable");
		t.addFriendlyTag("living_houses");
		t.addEnemyTag("rhino");
		t.addEnemyTag("crocodile");
		clone("rhino", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconRhino");
		t.addEnemyTag("hyena");
		t.addEnemyTag("snake");
		t.addEnemyTag("bear");
		t.addEnemyTag("wolf");
		t.addEnemyTag("rat");
		clone("buffalo", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconBuffalo");
		t.addTag("sliceable");
		t.addFriendlyTag("rhino");
		t.addEnemyTag("hyena");
		t.addEnemyTag("bear");
		t.addEnemyTag("wolf");
		t.addEnemyTag("crocodile");
		clone("hyena", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconHyena");
		t.addTag("sliceable");
		t.addFriendlyTag("orc");
		t.addFriendlyTag("living_houses");
		t.addEnemyTag("monkey");
		clone("rat", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconRat");
		t.addTag("sliceable");
		t.addTag("small");
		t.addFriendlyTag("civ_acid_gentleman");
		t.addFriendlyTag("miniciv_acid_blob");
		t.addFriendlyTag("acid_blob");
		t.addEnemyTag("cat");
		clone("alpaca", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconAlpaca");
		t.addTag("sliceable");
		clone("capybara", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconCapybara");
		t.friendship_for_everyone = true;
		t.addFriendlyTag("everyone");
		clone("goat", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconGoat");
		t.addFriendlyTag("nomads_dwarf");
		t.addFriendlyTag("dwarf");
		t.addFriendlyTag("civ_crystal_golem");
		t.addFriendlyTag("crystal_sword");
		clone("penguin", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconPenguin");
		t.addTag("sliceable");
		t.addFriendlyTag("bandit");
		t.addFriendlyTag("super_pumpkin");
		clone("ostrich", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconOstrich");
		t.addTag("sliceable");
		clone("crab", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconCrab");
		t.addTag("small");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("snowman");
		t.addFriendlyTag("crabzilla");
		clone("scorpion", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconScorpion");
		clone("turtle", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconTurtle");
		clone("crocodile", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconCrocodile");
		t.addTag("sliceable");
		t.addEnemyTag("chicken");
		t.addEnemyTag("monkey");
		clone("snake", "$TEMPLATE_ANIMAL_NEUTRAL$");
		t.setIcon("ui/Icons/iconSnake");
		t.addTag("small");
		t.addTag("nature_creature");
		t.addFriendlyTag("civ_snake");
		t.addFriendlyTag("elf");
		t.addFriendlyTag("nature_creature");
		clone("frog", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconFrog");
		t.addTag("sliceable");
		clone("piranha", "$TEMPLATE_ANIMAL$");
		t.setIcon("ui/Icons/iconPiranha");
		t.addTag("sliceable");
		t.addTag("small");
		clone("seal", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconSeal");
		t.addTag("sliceable");
		clone("flower_bud", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconFlowerBud");
		t.addTag("sliceable");
		clone("crystal_sword", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconCrystalSword");
		t.addEnemyTag("sliceable");
		clone("lemon_snail", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconLemonSnail");
		t.addTag("sliceable");
		t.addTag("small");
		clone("garl", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconGarl");
		t.addTag("garlic");
		clone("smore", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconSmore");
		t.addTag("sliceable");
		t.addTag("small");
		clone("acid_blob", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconAcidBlob");
		t.addEnemyTag("small");
		clone("unicorn", "$TEMPLATE_ANIMAL_PEACEFUL$");
		t.setIcon("ui/Icons/iconUnicorn");
		t.addEnemyTag("sliceable");
	}

	private void addCoolMinicivs()
	{
		cloneAsMiniciv("civ_aliens", "aliens", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("civ_druid", "druid");
		t.group_minicivs_cool = true;
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#85C32E");
		t.addTag("sliceable");
		cloneAsMiniciv("miniciv_angle", "angle", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_bandit", "bandit", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_cold_one", "cold_one", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_demon", "demon", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_evil_mage", "evil_mage", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_fire_skull", "fire_skull", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_jumpy_skull", "jumpy_skull", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_necromancer", "necromancer", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		t.addFriendlyTag("necromancer");
		t.addFriendlyTag("undead");
		cloneAsMiniciv("miniciv_plague_doctor", "plague_doctor", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_white_mage", "white_mage", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_greg", "greg");
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_fairy", "fairy", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
		cloneAsMiniciv("miniciv_snowman", "snowman", pMakeLoveToNeutrals: true);
		t.group_minicivs_cool = true;
	}

	private void addAnimalMinicivs()
	{
		cloneAsMiniciv("miniciv_cat", "cat");
		cloneAsMiniciv("miniciv_dog", "dog");
		cloneAsMiniciv("miniciv_chicken", "chicken");
		cloneAsMiniciv("miniciv_rabbit", "rabbit");
		cloneAsMiniciv("miniciv_monkey", "monkey");
		cloneAsMiniciv("miniciv_fox", "fox");
		cloneAsMiniciv("miniciv_sheep", "sheep");
		cloneAsMiniciv("miniciv_cow", "cow");
		cloneAsMiniciv("miniciv_armadillo", "armadillo");
		cloneAsMiniciv("miniciv_raccoon", "raccoon");
		cloneAsMiniciv("miniciv_wolf", "wolf");
		cloneAsMiniciv("miniciv_bear", "bear");
		cloneAsMiniciv("miniciv_rhino", "rhino");
		cloneAsMiniciv("miniciv_buffalo", "buffalo");
		cloneAsMiniciv("miniciv_hyena", "hyena");
		cloneAsMiniciv("miniciv_rat", "rat");
		cloneAsMiniciv("miniciv_alpaca", "alpaca");
		cloneAsMiniciv("miniciv_capybara", "capybara");
		t.addFriendlyTag("everyone");
		cloneAsMiniciv("miniciv_goat", "goat");
		cloneAsMiniciv("miniciv_penguin", "penguin");
		cloneAsMiniciv("miniciv_ostrich", "ostrich");
		cloneAsMiniciv("miniciv_crab", "crab");
		t.addFriendlyTag("crabzilla");
		cloneAsMiniciv("miniciv_scorpion", "scorpion");
		cloneAsMiniciv("miniciv_turtle", "turtle");
		cloneAsMiniciv("miniciv_crocodile", "crocodile");
		cloneAsMiniciv("miniciv_snake", "snake");
		cloneAsMiniciv("miniciv_frog", "frog");
		cloneAsMiniciv("miniciv_piranha", "piranha");
		cloneAsMiniciv("miniciv_seal", "seal");
		cloneAsMiniciv("miniciv_flower_bud", "flower_bud");
		cloneAsMiniciv("miniciv_crystal_sword", "crystal_sword");
		cloneAsMiniciv("miniciv_lemon_snail", "lemon_snail");
		cloneAsMiniciv("miniciv_garl", "garl");
		cloneAsMiniciv("miniciv_smore", "smore");
		cloneAsMiniciv("miniciv_acid_blob", "acid_blob");
		cloneAsMiniciv("miniciv_insect", "insect");
		cloneAsMiniciv("miniciv_unicorn", "unicorn");
	}

	private void addCreeps()
	{
		clone("super_pumpkin", "$TEMPLATE_MOB$");
		t.setIcon("ui/Icons/iconSuperPumpkin");
		t.addTag("sliceable");
		t.group_creeps = true;
		t.addFriendlyTag("druid");
		clone("tumor", "$TEMPLATE_MOB$");
		t.group_creeps = true;
		t.setIcon("ui/Icons/iconTumor");
		clone("biomass", "$TEMPLATE_MOB$");
		t.group_creeps = true;
		t.setIcon("ui/Icons/iconBiomass");
		clone("assimilators", "$TEMPLATE_MOB$");
		t.group_creeps = true;
		t.setIcon("ui/Icons/iconAssimilator");
		t.addFriendlyTag("aliens");
	}

	private void addUnique()
	{
		add(new KingdomAsset
		{
			id = "godfinger",
			nature = true,
			count_as_danger = false
		});
		t.setIcon("ui/Icons/iconGodFinger");
		add(new KingdomAsset
		{
			id = "good",
			mobs = true,
			concept = true,
			count_as_danger = false
		});
		t.setIcon("ui/Icons/actor_traits/iconBlessing");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("civ");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("snowman");
		t.addEnemyTag("wolf");
		t.addEnemyTag("bear");
		t.addEnemyTag("orc");
		t.addEnemyTag("bandit");
		add(new KingdomAsset
		{
			id = "mad",
			always_attack_each_other = true,
			force_look_all_chunks = true,
			mobs = true,
			units_always_looking_for_enemies = true,
			is_forced_by_trait = true,
			forced_by_trait_kingdom_id = "madness"
		});
		t.setIcon("ui/Icons/actor_traits/iconMadness");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#E53B3B");
		add(new KingdomAsset
		{
			id = "alien_mold",
			force_look_all_chunks = true,
			mobs = true,
			units_always_looking_for_enemies = true,
			is_forced_by_trait = true,
			forced_by_trait_kingdom_id = "desire_alien_mold",
			building_attractor_id = "waypoint_alien_mold"
		});
		t.setIcon("ui/Icons/iconWaypointAlienMold");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#C342FF");
		t.addFriendlyTag("aliens");
		t.addFriendlyTag("civ_aliens");
		add(new KingdomAsset
		{
			id = "computer",
			force_look_all_chunks = true,
			mobs = true,
			units_always_looking_for_enemies = true,
			is_forced_by_trait = true,
			forced_by_trait_kingdom_id = "desire_computer",
			building_attractor_id = "waypoint_computer"
		});
		t.setIcon("ui/Icons/iconWaypointComputer");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#5DCE2D");
		t.addFriendlyTag("assimilators");
		add(new KingdomAsset
		{
			id = "golden_egg",
			force_look_all_chunks = true,
			mobs = true,
			units_always_looking_for_enemies = true,
			is_forced_by_trait = true,
			forced_by_trait_kingdom_id = "desire_golden_egg",
			building_attractor_id = "waypoint_golden_egg"
		});
		t.setIcon("ui/Icons/iconWaypointGoldenEgg");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#FFEC77");
		t.addFriendlyTag("chicken");
		t.addFriendlyTag("civ_chicken");
		t.addFriendlyTag("miniciv_chicken");
		t.addFriendlyTag("sheep");
		t.addFriendlyTag("civ_sheep");
		t.addFriendlyTag("miniciv_sheep");
		add(new KingdomAsset
		{
			id = "harp",
			force_look_all_chunks = true,
			mobs = true,
			units_always_looking_for_enemies = true,
			is_forced_by_trait = true,
			forced_by_trait_kingdom_id = "desire_harp",
			building_attractor_id = "waypoint_harp"
		});
		t.setIcon("ui/Icons/iconWaypointHarp");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#FF60E9");
		t.addFriendlyTag("crystal_sword");
		t.addFriendlyTag("civ_crystal_golem");
		t.addFriendlyTag("miniciv_crystal_sword");
		add(new KingdomAsset
		{
			id = "possessed",
			force_look_all_chunks = true
		});
		t.setIcon("ui/Icons/iconPossessed2");
		t.addEnemyTag("nature");
		t.addEnemyTag("ruins");
		t.addEnemyTag("abandoned");
		add(new KingdomAsset
		{
			id = "crabzilla",
			mobs = true
		});
		t.setIcon("ui/Icons/iconCrabzilla");
		t.addTag("crab");
		t.addFriendlyTag("crab");
		t.addFriendlyTag("civ_crab");
		t.addFriendlyTag("miniciv_crab");
		add(new KingdomAsset
		{
			id = "ants",
			mobs = true
		});
		t.setIcon("ui/Icons/iconAntRed");
		t.addTag("nature_creature");
		t.addFriendlyTag("good");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("living_houses");
		add(new KingdomAsset
		{
			id = "golden_brain",
			mobs = true,
			brain = true,
			count_as_danger = false
		});
		t.setIcon("ui/Icons/iconGoldBrain");
		t.addTag("neutral");
		t.addFriendlyTag("orc");
		t.addFriendlyTag("bandit");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("civ");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("snowman");
		add(new KingdomAsset
		{
			id = "corrupted_brain",
			mobs = true,
			brain = true
		});
		t.setIcon("ui/Icons/iconCorruptedBrain");
		t.addTag("mad");
	}

	private void addNeutral()
	{
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		add(new KingdomAsset
		{
			id = "neutral",
			civ = true,
			neutral = true,
			default_civ_color_index = 83,
			count_as_danger = false,
			concept = true
		});
		t.setIcon("ui/Icons/worldrules/icon_random_seeds");
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#AAAAAA");
		t.addTag("nature_creature");
		t.addTag("neutral");
		t.addFriendlyTag("good");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("neutral");
		add(new KingdomAsset
		{
			id = "neutral_animals",
			mobs = true,
			count_as_danger = false,
			concept = true
		});
		t.setIcon("ui/Icons/worldrules/icon_animalspawn");
		t.addTag("neutral");
		t.addTag("nature_creature");
		t.addFriendlyTag("good");
		t.addFriendlyTag("neutral");
		t.addFriendlyTag("nature_creature");
		t.addFriendlyTag("living_houses");
		t.addFriendlyTag("snowman");
		t.addFriendlyTag("civ");
		clone("insect", "neutral_animals");
		t.setIcon("ui/Icons/iconBeetle");
		t.concept = true;
		clone("fly", "insect");
		t.setIcon("ui/Icons/iconFly");
		add(new KingdomAsset
		{
			id = "nature",
			nature = true,
			mobs = true,
			count_as_danger = false,
			concept = true
		});
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#888888");
		t.setIcon("ui/Icons/world generation/icon_randomBiomes");
		add(new KingdomAsset
		{
			id = "ruins",
			nature = true,
			mobs = true,
			count_as_danger = false,
			concept = true
		});
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#444444");
		t.setIcon("ui/Icons/iconCityDestroyed");
		t.color_building = Toolbox.color_white;
		add(new KingdomAsset
		{
			id = "abandoned",
			nature = true,
			mobs = true,
			abandoned = true,
			count_as_danger = false,
			concept = true
		});
		t.default_kingdom_color = ColorAsset.tryMakeNewColorAsset("#888888");
		t.setIcon("ui/Icons/iconKingdomDestroyed");
		t.color_building = Toolbox.color_abandoned_building;
	}

	public override void post_init()
	{
		base.post_init();
		using ListPool<string> listPool = new ListPool<string>();
		foreach (KingdomAsset item in list)
		{
			if (item.friendship_for_everyone && !item.brain)
			{
				listPool.Add(item.id);
			}
		}
		foreach (KingdomAsset item2 in list)
		{
			item2.addTag("everyone");
			foreach (ref string item3 in listPool)
			{
				string current3 = item3;
				item2.addFriendlyTag(current3);
			}
			if (item2.default_kingdom_color != null)
			{
				if (string.Equals(item2.default_kingdom_color.id, "ASSET_ID"))
				{
					item2.default_kingdom_color.id = "kingdom_library_color_" + item2.id;
				}
			}
			else
			{
				item2.default_kingdom_color = _shared_default_color;
			}
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (KingdomAsset item in list)
		{
			finish(item);
		}
	}

	private void finish(KingdomAsset pAsset)
	{
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		generateDebugReportFile();
		foreach (KingdomAsset item in list)
		{
			if ((!item.civ && !item.mobs) || item.concept || item.nomads || item.nature || item.neutral || item.brain || item.is_forced_by_trait)
			{
				continue;
			}
			bool flag = false;
			foreach (ActorAsset item2 in AssetManager.actor_library.list)
			{
				if (item.civ && item2.kingdom_id_civilization == item.id)
				{
					flag = true;
					break;
				}
				if (item.mobs && item2.kingdom_id_wild == item.id)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (item.civ)
				{
					BaseAssetLibrary.logAssetError("<b>KingdomLibrary</b>: <e>Civ Kingdom</e> is not used by any <e>kingdom_id_civilization</e>", item.id);
				}
				else
				{
					BaseAssetLibrary.logAssetError("<b>KingdomLibrary</b>: <e>Mob Kingdom</e> is not used by any <e>kingdom_id_wild</e>", item.id);
				}
			}
		}
	}

	public void checkForMissingTags()
	{
		for (int i = 0; i < list.Count - 1; i++)
		{
			KingdomAsset kingdomAsset = list[i];
			for (int j = i + 1; j < list.Count; j++)
			{
				KingdomAsset kingdomAsset2 = list[j];
				if (kingdomAsset == kingdomAsset2 || kingdomAsset.isFoe(kingdomAsset2) == kingdomAsset2.isFoe(kingdomAsset))
				{
					continue;
				}
				KingdomAsset kingdomAsset3 = kingdomAsset;
				if (kingdomAsset3.assets_discrepancies == null)
				{
					kingdomAsset3.assets_discrepancies = new HashSet<string>();
				}
				kingdomAsset3 = kingdomAsset2;
				if (kingdomAsset3.assets_discrepancies == null)
				{
					kingdomAsset3.assets_discrepancies = new HashSet<string>();
				}
				kingdomAsset.assets_discrepancies.Add(kingdomAsset2.id);
				kingdomAsset2.assets_discrepancies.Add(kingdomAsset.id);
				if (kingdomAsset2.id.Contains(kingdomAsset.id) || kingdomAsset.id.Contains(kingdomAsset2.id))
				{
					kingdomAsset3 = kingdomAsset;
					if (kingdomAsset3.assets_discrepancies_bad == null)
					{
						kingdomAsset3.assets_discrepancies_bad = new HashSet<string>();
					}
					kingdomAsset3 = kingdomAsset2;
					if (kingdomAsset3.assets_discrepancies_bad == null)
					{
						kingdomAsset3.assets_discrepancies_bad = new HashSet<string>();
					}
					kingdomAsset.assets_discrepancies_bad.Add(kingdomAsset2.id);
					kingdomAsset2.assets_discrepancies_bad.Add(kingdomAsset.id);
				}
			}
		}
	}

	public void generateDebugReportFile()
	{
		if (!DebugConfig.isOn(DebugOption.GenerateGameplayReport))
		{
			return;
		}
		string path = "GenAssets/wbdiag/kingdom_library.log";
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.AppendLine("# RELATIONS");
		using ListPool<string> listPool = new ListPool<string>();
		using ListPool<string> listPool2 = new ListPool<string>();
		Span<KingdomAsset> span = list.AsSpan();
		Span<KingdomAsset> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			KingdomAsset kingdomAsset = span2[i];
			listPool.Clear();
			listPool2.Clear();
			stringBuilderPool.AppendLine("###" + kingdomAsset.id.ToUpper() + ":");
			Span<KingdomAsset> span3 = span;
			for (int j = 0; j < span3.Length; j++)
			{
				KingdomAsset kingdomAsset2 = span3[j];
				if (kingdomAsset.isFoe(kingdomAsset2))
				{
					listPool2.Add(kingdomAsset2.id);
				}
				else
				{
					listPool.Add(kingdomAsset2.id);
				}
			}
			stringBuilderPool.AppendLine("- FRIENDLY:");
			foreach (ref string item in listPool)
			{
				string current = item;
				stringBuilderPool.AppendLine("   " + current);
			}
			stringBuilderPool.AppendLine("- FOES:");
			foreach (ref string item2 in listPool2)
			{
				string current2 = item2;
				stringBuilderPool.AppendLine("   " + current2);
			}
			stringBuilderPool.AppendLine();
		}
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine();
		stringBuilderPool.AppendLine("# TAGS");
		Dictionary<string, HashSet<string>> dictionary = UnsafeCollectionPool<Dictionary<string, HashSet<string>>, KeyValuePair<string, HashSet<string>>>.Get();
		span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			foreach (string list_tag in span2[i].list_tags)
			{
				dictionary.TryAdd(list_tag, UnsafeCollectionPool<HashSet<string>, string>.Get());
				Span<KingdomAsset> span3 = span;
				for (int j = 0; j < span3.Length; j++)
				{
					KingdomAsset kingdomAsset3 = span3[j];
					if (kingdomAsset3.list_tags.Contains(list_tag))
					{
						dictionary[list_tag].Add(kingdomAsset3.id);
					}
				}
			}
		}
		foreach (KeyValuePair<string, HashSet<string>> item3 in dictionary)
		{
			stringBuilderPool.AppendLine("-" + item3.Key.ToUpper() + ":");
			foreach (string item4 in item3.Value)
			{
				stringBuilderPool.AppendLine("   " + item4);
			}
			UnsafeCollectionPool<HashSet<string>, string>.Release(item3.Value);
		}
		dictionary.Clear();
		UnsafeCollectionPool<Dictionary<string, HashSet<string>>, KeyValuePair<string, HashSet<string>>>.Release(dictionary);
		File.WriteAllTextAsync(path, stringBuilderPool.ToString());
	}

	public void cloneAsMiniciv(string pNew, string pFrom, bool pMakeLoveToNeutrals = false)
	{
		clone(pNew, pFrom);
		t.group_miniciv = true;
		t.mobs = false;
		t.civ = true;
		t.addTag(pFrom);
		t.addFriendlyTag(pFrom);
		get(pFrom).addFriendlyTag(pNew);
		if (pMakeLoveToNeutrals)
		{
			t.addTag("civ");
			t.addFriendlyTag("neutral");
			t.addFriendlyTag("nature_creature");
		}
	}

	public override KingdomAsset clone(string pNew, string pFrom)
	{
		base.clone(pNew, pFrom);
		t.concept = false;
		return t;
	}
}
