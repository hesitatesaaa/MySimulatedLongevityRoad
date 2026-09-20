using System;
using System.Collections.Generic;

public class ResourceLibrary : AssetLibrary<ResourceAsset>
{
	private const string TEMPLATE_FOOD = "$TEMPLATE_FOOD$";

	private const string TEMPLATE_STRATEGIC_MINERAL = "$TEMPLATE_STRATEGIC_MINERAL$";

	public static ResourceAsset herbs;

	public static ResourceAsset mushrooms;

	public static ResourceAsset desert_berries;

	public static ResourceAsset berries;

	public static ResourceAsset peppers;

	public static ResourceAsset bananas;

	public static ResourceAsset crystal_salt;

	public static ResourceAsset coconut;

	public static ResourceAsset evil_beets;

	public static ResourceAsset lemons;

	public static ResourceAsset meat;

	public static ResourceAsset sushi;

	public static ResourceAsset jam;

	public static ResourceAsset burger;

	public static ResourceAsset cider;

	public static ResourceAsset ale;

	public static ResourceAsset honey;

	public static ResourceAsset tea;

	public static ResourceAsset pie;

	public static ResourceAsset wheat;

	public static ResourceAsset bread;

	public static ResourceAsset fish;

	public static ResourceAsset candy;

	public static ResourceAsset worms;

	public static ResourceAsset pine_cones;

	public static ResourceAsset snow_cucumbers;

	public static ResourceAsset celestial_avocado;

	public static ResourceAsset wood;

	public static ResourceAsset stone;

	public static ResourceAsset common_metals;

	public static ResourceAsset gold;

	public static ResourceAsset fertilizer;

	[NonSerialized]
	public List<ResourceAsset> strategic_resource_assets = new List<ResourceAsset>();

	[NonSerialized]
	public Dictionary<string, List<string>> diet_food_pools = new Dictionary<string, List<string>>();

	public override void init()
	{
		base.init();
		initTemplates();
		initOther();
		initStrategic();
		initFoodIngredients();
		initFood();
		initFoodRecipes();
	}

	private void initTemplates()
	{
		add(new ResourceAsset
		{
			id = "$TEMPLATE_FOOD$",
			type = ResType.Food,
			path_gameplay_sprite = "bag_food",
			tooltip = "city_resource_food",
			food = true
		});
		add(new ResourceAsset
		{
			id = "$TEMPLATE_STRATEGIC_MINERAL$",
			type = ResType.Strategic,
			mineral = true
		});
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_minerals");
	}

	private void initFoodIngredients()
	{
		wheat = add(new ResourceAsset
		{
			id = "wheat",
			type = ResType.Ingredient_Food,
			path_gameplay_sprite = "wheat",
			path_icon = "iconResWheat",
			supply_bound_give = 100,
			supply_bound_take = 20,
			restore_nutrition = 50,
			restore_health = 0.05f,
			restore_mana = 5,
			restore_stamina = 5,
			favorite_food_chance = 0.1f,
			tastiness = 0.1f,
			restore_happiness = 5
		});
		t.food = true;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_crops");
	}

	private void initFoodRecipes()
	{
		bread = clone("bread", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResBread";
		t.path_gameplay_sprite = "bread";
		t.ingredients_amount = 1;
		t.restore_nutrition = 100;
		t.restore_health = 0.1f;
		t.restore_mana = 10;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 10;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("wheat");
		t.trade_bound = 50;
		t.produce_min = 50;
		t.maximum = 999;
		t.trade_give = 5;
		t.tastiness = 0.5f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_crops");
		sushi = clone("sushi", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResSushi";
		t.restore_nutrition = 70;
		t.restore_health = 0.2f;
		t.restore_mana = 20;
		t.restore_stamina = 20;
		t.restore_happiness = 20;
		t.give_experience = 5;
		t.ingredients_amount = 1;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("fish", "herbs");
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("fast#5");
		t.give_chance = 0.1f;
		t.tastiness = 0.6f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fish", "diet_meat");
		jam = clone("jam", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResJam";
		t.restore_nutrition = 100;
		t.restore_health = 0.4f;
		t.restore_mana = 40;
		t.restore_stamina = 40;
		t.restore_happiness = 25;
		t.ingredients_amount = 2;
		t.give_experience = 15;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("berries", "herbs");
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("regeneration#3");
		t.give_chance = 0.1f;
		t.tastiness = 0.9f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits", "diet_vegetation");
		cider = clone("cider", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResCider";
		t.restore_nutrition = 55;
		t.restore_health = 0.25f;
		t.restore_mana = 25;
		t.restore_stamina = 25;
		t.restore_happiness = 20;
		t.ingredients_amount = 3;
		t.give_experience = 10;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("berries");
		t.trade_bound = 50;
		t.trade_give = 5;
		t.tastiness = 0.7f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		ale = clone("ale", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResAle";
		t.restore_nutrition = 45;
		t.restore_health = 0.2f;
		t.restore_mana = 20;
		t.restore_stamina = 20;
		t.restore_happiness = 20;
		t.ingredients_amount = 3;
		t.give_experience = 10;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("wheat");
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_status_id = AssetLibrary<ResourceAsset>.a<string>("slowness");
		t.give_trait_id = Toolbox.splitStringIntoArray("poison_immune#1");
		t.give_chance = 0.4f;
		t.tastiness = 0.6f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_crops");
		burger = clone("burger", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResBurger";
		t.restore_nutrition = 100;
		t.restore_health = 0.4f;
		t.restore_mana = 25;
		t.restore_stamina = 35;
		t.restore_happiness = 25;
		t.ingredients_amount = 1;
		t.give_experience = 9;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("meat", "bread", "herbs");
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_status_id = AssetLibrary<ResourceAsset>.a<string>("slowness");
		t.give_trait_id = Toolbox.splitStringIntoArray("fat#1");
		t.give_chance = 0.05f;
		t.tastiness = 0.85f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_meat", "diet_crops", "diet_vegetation");
		pie = clone("pie", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResPie";
		t.restore_nutrition = 100;
		t.restore_health = 0.4f;
		t.restore_mana = 35;
		t.restore_stamina = 25;
		t.restore_happiness = 25;
		t.ingredients_amount = 1;
		t.give_experience = 10;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("meat", "wheat");
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_status_id = AssetLibrary<ResourceAsset>.a<string>("slowness");
		t.give_trait_id = Toolbox.splitStringIntoArray("fat#1");
		t.give_chance = 0.05f;
		t.tastiness = 0.9f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_meat", "diet_crops");
		tea = clone("tea", "$TEMPLATE_FOOD$");
		t.restore_nutrition = 20;
		t.restore_health = 0.25f;
		t.restore_mana = 40;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 10;
		t.path_icon = "iconResTea";
		t.path_gameplay_sprite = "box_tea";
		t.trade_bound = 50;
		t.trade_give = 5;
		t.ingredients = AssetLibrary<ResourceAsset>.a<string>("herbs");
		t.give_status_id = AssetLibrary<ResourceAsset>.a<string>("caffeinated");
		t.give_chance = 0.1f;
		t.tastiness = 0.3f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_vegetation");
	}

	private void initFood()
	{
		berries = clone("berries", "$TEMPLATE_FOOD$");
		t.path_gameplay_sprite = "berries";
		t.path_icon = "iconResBerries";
		t.restore_nutrition = 30;
		t.restore_health = 0.35f;
		t.restore_mana = 15;
		t.restore_stamina = 10;
		t.restore_happiness = 12;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.tastiness = 0.55f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		bananas = clone("bananas", "$TEMPLATE_FOOD$");
		t.path_gameplay_sprite = "bananas";
		t.path_icon = "iconResBanana";
		t.restore_nutrition = 25;
		t.restore_health = 0.4f;
		t.restore_mana = 15;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_trait_id = Toolbox.splitStringIntoArray("unlucky");
		t.give_chance = 0.05f;
		t.tastiness = 0.55f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		coconut = clone("coconut", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResCoconut";
		t.path_gameplay_sprite = "coconut";
		t.restore_nutrition = 30;
		t.restore_health = 0.45f;
		t.restore_mana = 15;
		t.restore_stamina = 10;
		t.restore_happiness = 15;
		t.give_experience = 10;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_status_id = Toolbox.splitStringIntoArray("shield#1");
		t.give_chance = 0.1f;
		t.tastiness = 0.55f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		crystal_salt = clone("crystal_salt", "$TEMPLATE_FOOD$");
		t.drop_per_mass = 150;
		t.path_icon = "iconResCrystalSalt";
		t.path_gameplay_sprite = "crystal_salt";
		t.restore_nutrition = 20;
		t.restore_health = 0.2f;
		t.restore_mana = 20;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_status_id = Toolbox.splitStringIntoArray("caffeinated#3", "slowness#6");
		t.give_trait_id = Toolbox.splitStringIntoArray("madness#10", "strong_minded#1");
		t.give_chance = 0.1f;
		t.tastiness = 0.2f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_minerals");
		desert_berries = clone("desert_berries", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResDesertBerry";
		t.path_gameplay_sprite = "desert_berries";
		t.restore_nutrition = 25;
		t.restore_health = 0.35f;
		t.restore_mana = 15;
		t.restore_stamina = 10;
		t.restore_happiness = 12;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_status_id = Toolbox.splitStringIntoArray("poisoned#2", "slowness#2");
		t.give_trait_id = Toolbox.splitStringIntoArray("poison_immune#1");
		t.give_chance = 0.1f;
		t.tastiness = 0.55f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		evil_beets = clone("evil_beets", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResEvilBeets";
		t.path_gameplay_sprite = "evil_beets";
		t.restore_nutrition = 25;
		t.restore_health = 0.4f;
		t.restore_mana = 15;
		t.restore_stamina = 25;
		t.restore_happiness = 15;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_trait_id = Toolbox.splitStringIntoArray("pyromaniac#1", "evil#3");
		t.give_chance = 0.1f;
		t.tastiness = 0.1f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_vegetation");
		mushrooms = clone("mushrooms", "$TEMPLATE_FOOD$");
		t.drop_per_mass = 40;
		t.path_gameplay_sprite = "mushrooms";
		t.path_icon = "iconResMushrooms";
		t.restore_nutrition = 20;
		t.restore_health = 0.4f;
		t.restore_mana = 15;
		t.restore_stamina = 15;
		t.restore_happiness = 15;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_status_id = Toolbox.splitStringIntoArray("powerup#10", "slowness#3");
		t.give_trait_id = Toolbox.splitStringIntoArray("madness#1", "strong_minded#1", "paranoid#5", "content#10");
		t.give_chance = 0.1f;
		t.tastiness = 0.2f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_vegetation");
		peppers = clone("peppers", "$TEMPLATE_FOOD$");
		t.drop_per_mass = 40;
		t.path_icon = "iconResPeppers";
		t.path_gameplay_sprite = "peppers";
		t.restore_nutrition = 30;
		t.restore_health = 0.4f;
		t.restore_mana = 25;
		t.restore_stamina = 15;
		t.restore_happiness = 20;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.storage_max = 100;
		t.give_status_id = Toolbox.splitStringIntoArray("burning");
		t.give_trait_id = Toolbox.splitStringIntoArray("fire_proof#5", "fire_blood#5");
		t.give_chance = 0.1f;
		t.tastiness = 0.3f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_vegetation");
		herbs = clone("herbs", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResHerbs";
		t.path_gameplay_sprite = "herbs";
		t.restore_nutrition = 25;
		t.restore_health = 0.5f;
		t.restore_mana = 25;
		t.restore_stamina = 10;
		t.restore_happiness = 5;
		t.give_experience = 5;
		t.trade_bound = 30;
		t.trade_give = 10;
		t.storage_max = 100;
		t.tastiness = 0.1f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_vegetation");
		fish = clone("fish", "$TEMPLATE_FOOD$");
		t.path_gameplay_sprite = "fish";
		t.restore_nutrition = 50;
		t.restore_health = 0.3f;
		t.restore_mana = 25;
		t.restore_stamina = 10;
		t.restore_happiness = 15;
		t.path_icon = "iconResFish";
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("tough#5", "strong#5");
		t.give_chance = 0.1f;
		t.tastiness = 0.25f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fish", "diet_meat");
		candy = clone("candy", "$TEMPLATE_FOOD$");
		t.drop_per_mass = 50;
		t.path_icon = "iconResCandy";
		t.path_gameplay_sprite = "candy";
		t.restore_nutrition = 40;
		t.restore_health = 0.4f;
		t.restore_mana = 25;
		t.restore_stamina = 10;
		t.restore_happiness = 40;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("fat#10", "tiny#5", "giant#5", "bloodlust#1");
		t.give_chance = 0.1f;
		t.tastiness = 1f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		worms = clone("worms", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResWorms";
		t.path_gameplay_sprite = "worms";
		t.restore_nutrition = 10;
		t.restore_health = 0.2f;
		t.restore_mana = 10;
		t.restore_stamina = 10;
		t.restore_happiness = -5;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.tastiness = 0.1f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_meat");
		snow_cucumbers = clone("snow_cucumbers", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResSnowCucumbers";
		t.path_gameplay_sprite = "snow_cucumbers";
		t.restore_nutrition = 30;
		t.restore_health = 0.35f;
		t.restore_mana = 25;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_status_id = Toolbox.splitStringIntoArray("frozen");
		t.give_trait_id = Toolbox.splitStringIntoArray("freeze_proof#1");
		t.give_chance = 0.1f;
		t.tastiness = 0.2f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_vegetation");
		celestial_avocado = clone("celestial_avocado", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResCelestialAvocado";
		t.path_gameplay_sprite = "celestial_avocado";
		t.restore_nutrition = 100;
		t.restore_health = 0.55f;
		t.restore_mana = 50;
		t.restore_stamina = 50;
		t.restore_happiness = 50;
		t.give_experience = 10;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("sunblessed#1");
		t.give_chance = 0.1f;
		t.tastiness = 1.1f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		pine_cones = clone("pine_cones", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResPineCones";
		t.path_gameplay_sprite = "pine_cones";
		t.restore_nutrition = 15;
		t.restore_health = 0.15f;
		t.restore_mana = 10;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_status_id = Toolbox.splitStringIntoArray("frozen");
		t.give_trait_id = Toolbox.splitStringIntoArray("freeze_proof#10", "tough#1", "strong#1", "regeneration#1");
		t.give_chance = 0.1f;
		t.tastiness = 0.1f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_crops");
		lemons = clone("lemons", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResLemons";
		t.path_gameplay_sprite = "lemons";
		t.restore_nutrition = 20;
		t.restore_health = 0.3f;
		t.restore_mana = 10;
		t.restore_stamina = 10;
		t.restore_happiness = 10;
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("eagle_eyed#5", "regeneration#3");
		t.give_chance = 0.1f;
		t.tastiness = 0.2f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits");
		meat = clone("meat", "$TEMPLATE_FOOD$");
		t.drop_per_mass = 50;
		t.restore_nutrition = 60;
		t.restore_health = 0.5f;
		t.restore_mana = 10;
		t.restore_stamina = 25;
		t.restore_happiness = 20;
		t.path_icon = "iconResMeat";
		t.path_gameplay_sprite = "meat";
		t.give_experience = 5;
		t.trade_bound = 50;
		t.trade_give = 5;
		t.give_trait_id = Toolbox.splitStringIntoArray("tough#5", "strong#5");
		t.give_chance = 0.1f;
		t.tastiness = 0.5f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_meat");
		honey = clone("honey", "$TEMPLATE_FOOD$");
		t.path_icon = "iconResHoney";
		t.path_gameplay_sprite = "honey";
		t.restore_nutrition = 15;
		t.restore_health = 0.45f;
		t.restore_mana = 10;
		t.restore_stamina = 25;
		t.restore_happiness = 35;
		t.give_experience = 10;
		t.trade_bound = 50;
		t.trade_give = 10;
		t.storage_max = 100;
		t.tastiness = 0.5f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_fruits", "diet_flowers");
	}

	private void initStrategic()
	{
		wood = add(new ResourceAsset
		{
			id = "wood",
			path_icon = "iconResWood",
			path_gameplay_sprite = "wood",
			type = ResType.Strategic,
			wood = true
		});
		t.restore_nutrition = 50;
		t.restore_health = 0.45f;
		t.restore_mana = 15;
		t.restore_stamina = 15;
		t.restore_happiness = 10;
		t.tastiness = 0.2f;
		t.diet = AssetLibrary<ResourceAsset>.a<string>("diet_wood");
		stone = clone("stone", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.drop_per_mass = 150;
		t.path_icon = "iconResStone";
		t.path_gameplay_sprite = "stone";
		t.mine_rate = 25;
		t.restore_nutrition = 20;
		t.restore_health = 0.2f;
		t.restore_mana = 10;
		t.restore_stamina = 10;
		t.restore_happiness = 5;
		t.tastiness = 0.1f;
		clone("silver", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.path_icon = "iconResSilver";
		t.path_gameplay_sprite = "silver";
		t.restore_nutrition = 35;
		t.restore_health = 0.3f;
		t.restore_mana = 25;
		t.restore_stamina = 15;
		t.restore_happiness = 20;
		t.tastiness = 0.4f;
		clone("mythril", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.path_icon = "iconResMythril";
		t.path_gameplay_sprite = "mythril";
		t.restore_nutrition = 45;
		t.restore_health = 0.4f;
		t.restore_mana = 30;
		t.restore_stamina = 25;
		t.restore_happiness = 25;
		t.tastiness = 0.6f;
		clone("adamantine", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.path_icon = "iconResAdamantine";
		t.path_gameplay_sprite = "adamantine";
		t.restore_nutrition = 55;
		t.restore_health = 0.5f;
		t.restore_mana = 45;
		t.restore_stamina = 40;
		t.restore_happiness = 35;
		t.tastiness = 0.8f;
		add(new ResourceAsset
		{
			id = "dragon_scales",
			path_icon = "iconResDragonScales",
			path_gameplay_sprite = "dragon_scales",
			type = ResType.Strategic
		});
		common_metals = clone("common_metals", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.path_icon = "iconResCommonMetals";
		t.path_gameplay_sprite = "common_metals";
		t.storage_max = 100;
		t.restore_nutrition = 35;
		t.restore_health = 0.3f;
		t.restore_mana = 15;
		t.restore_stamina = 15;
		t.restore_happiness = 10;
		t.tastiness = 0.2f;
		clone("bones", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.drop_per_mass = 100;
		t.path_icon = "iconResBones";
		t.path_gameplay_sprite = "bones";
		t.tastiness = 0.3f;
		add(new ResourceAsset
		{
			id = "leather",
			path_icon = "iconResLeather",
			path_gameplay_sprite = "leather",
			type = ResType.Strategic,
			drop_per_mass = 100
		});
		clone("gems", "$TEMPLATE_STRATEGIC_MINERAL$");
		t.drop_per_mass = 200;
		t.path_icon = "iconResGems";
		t.path_gameplay_sprite = "gems";
		t.mine_rate = 1;
		t.restore_nutrition = 35;
		t.restore_health = 0.3f;
		t.restore_mana = 15;
		t.restore_stamina = 25;
		t.restore_happiness = 15;
		t.tastiness = 1f;
		fertilizer = add(new ResourceAsset
		{
			id = "fertilizer",
			path_icon = "iconFertilizer",
			path_gameplay_sprite = "fertilizer",
			type = ResType.Strategic
		});
	}

	private void initOther()
	{
		gold = add(new ResourceAsset
		{
			id = "gold",
			path_icon = "iconResGold",
			path_gameplay_sprite = "gold",
			maximum = 999,
			supply_bound_give = 600,
			supply_bound_take = 10,
			supply_give = 100,
			type = ResType.Currency
		});
	}

	public override void post_init()
	{
		base.post_init();
		int num = 0;
		foreach (ResourceAsset item in list)
		{
			item.order = num++;
			item.full_sprite_path = "items/resources/" + item.path_gameplay_sprite;
		}
	}

	public void loadSprites()
	{
		foreach (ResourceAsset item in list)
		{
			item.gameplay_sprites = SpriteTextureLoader.getSpriteList(item.full_sprite_path);
		}
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (ResourceAsset item in list)
		{
			if (item.type == ResType.Strategic)
			{
				strategic_resource_assets.Add(item);
			}
			if (item.give_trait_id != null)
			{
				item.give_trait = new ActorTrait[item.give_trait_id.Length];
				for (int i = 0; i < item.give_trait_id.Length; i++)
				{
					string pID = item.give_trait_id[i];
					item.give_trait[i] = AssetManager.traits.get(pID);
				}
			}
			if (item.give_status_id != null)
			{
				item.give_status = new StatusAsset[item.give_status_id.Length];
				for (int j = 0; j < item.give_status_id.Length; j++)
				{
					string pID2 = item.give_status_id[j];
					item.give_status[j] = AssetManager.status.get(pID2);
				}
			}
			if (item.diet == null)
			{
				continue;
			}
			foreach (SubspeciesTrait item2 in AssetManager.subspecies_traits.list)
			{
				if (!item2.is_diet_related)
				{
					continue;
				}
				string key = item2.id;
				string[] diet = item.diet;
				foreach (string pTag in diet)
				{
					if (item2.base_stats_meta.hasTag(pTag))
					{
						if (!diet_food_pools.TryGetValue(key, out var value))
						{
							value = new List<string>();
							diet_food_pools.Add(key, value);
						}
						value.Add(item.id);
					}
				}
			}
		}
	}

	public override void editorDiagnostic()
	{
		foreach (ResourceAsset item in list)
		{
			if (!item.isTemplateAsset())
			{
				checkSpriteExists("path_gameplay_sprite", item.full_sprite_path, item);
			}
		}
		base.editorDiagnostic();
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (ResourceAsset item in list)
		{
			if (!item.isTemplateAsset())
			{
				checkLocale(item, item.getLocaleID());
			}
		}
	}
}
