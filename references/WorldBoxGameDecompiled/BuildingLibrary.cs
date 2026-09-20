using System.Collections.Generic;
using System.Reflection;
using Beebyte.Obfuscator;
using UnityEngine;
using strings;

[ObfuscateLiterals]
public class BuildingLibrary : AssetLibrary<BuildingAsset>
{
	public static readonly Vector2 shadow_under_construction_bound;

	public static readonly float shadow_under_construction_distortion;

	private const string TEMPLATE_CREEP = "$building_creep$";

	private const string TEMPLATE_RESOURCE = "$resource$";

	private const string TEMPLATE_MINERAL = "$mineral$";

	private const string TEMPLATE_FLORA_SMALL = "$flora_small$";

	private const string TEMPLATE_BUILDING = "$building$";

	private const string TEMPLATE_WAYPOINT = "$waypoint$";

	private const string TEMPLATE_DROP_SPREADER = "$drop_spreader$";

	private const string TEMPLATE_CITY_BUILDING = "$city_building$";

	private const string TEMPLATE_CITY_COLORED_BUILDING = "$city_colored_building$";

	private const string TEMPLATE_WINDMILL_BASE = "$windmill_base$";

	private const string TEMPLATE_WINDMILL_0 = "$windmill_0$";

	private const string TEMPLATE_WINDMILL_1 = "$windmill_1$";

	private const string TEMPLATE_BUILDING_CIV_HUMAN = "$building_civ_human$";

	private const string TEMPLATE_BUILDING_CIV_ORC = "$building_civ_orc$";

	private const string TEMPLATE_BUILDING_CIV_DWARF = "$building_civ_dwarf$";

	private const string TEMPLATE_BUILDING_CIV_ELF = "$building_civ_elf$";

	public override void init()
	{
		base.init();
		addTrees();
		addVegetation();
		addMinerals();
		addPoop();
		addGrownResources();
		addGeneralCityBuildings();
		addNatureBuildings();
		addMobBuildings();
		addCreeps();
		addHumans();
		addOrcs();
		addElves();
		addDwarves();
	}

	public override void post_init()
	{
		base.post_init();
		initBuildingsFromArchitectures();
	}

	public override void linkAssets()
	{
		base.linkAssets();
		checkAtlasLink(pWobbleTreesSettingIsActive: true);
		foreach (BuildingAsset item in list)
		{
			if (item.step_action != null)
			{
				item.has_step_action = true;
			}
			if (item.get_map_icon_color != null)
			{
				item.has_get_map_icon_color = true;
			}
			HashSet<BiomeTag> biome_tags_growth = item.biome_tags_growth;
			item.has_biome_tags = biome_tags_growth != null && biome_tags_growth.Count > 0;
			HashSet<BiomeTag> biome_tags_spread = item.biome_tags_spread;
			item.has_biome_tags_spread = biome_tags_spread != null && biome_tags_spread.Count > 0;
		}
	}

	public void checkAtlasLink(bool pWobbleTreesSettingIsActive)
	{
		foreach (BuildingAsset item in list)
		{
			if (!pWobbleTreesSettingIsActive)
			{
				item.atlas_asset = AssetManager.dynamic_sprites_library.get(item.atlas_id_fallback_when_not_wobbly);
			}
			else
			{
				item.atlas_asset = AssetManager.dynamic_sprites_library.get(item.atlas_id);
			}
		}
	}

	private void initBuildingsFromArchitectures()
	{
		foreach (ArchitectureAsset item in AssetManager.architecture_library.list)
		{
			if (item.isTemplateAsset() || !item.generate_buildings)
			{
				continue;
			}
			string text = item.id;
			string[] styled_building_orders = item.styled_building_orders;
			foreach (string text2 in styled_building_orders)
			{
				string pNew = item.building_ids_for_construction[text2];
				string generation_target = item.generation_target;
				BuildingAsset building = AssetManager.architecture_library.get(generation_target).getBuilding(text2);
				BuildingAsset buildingAsset = clone(pNew, building.id);
				buildingAsset.group = "civ_building";
				buildingAsset.mini_civ_auto_load = true;
				buildingAsset.civ_kingdom = text;
				buildingAsset.main_path = "buildings/civ_main/" + text + "/";
				buildingAsset.can_be_upgraded = false;
				buildingAsset.has_sprite_construction = true;
				if (item.spread_biome)
				{
					buildingAsset.spread_biome = true;
					buildingAsset.spread_biome_id = item.spread_biome_id;
				}
				buildingAsset.material = item.material;
				if (buildingAsset.material == "jelly")
				{
					buildingAsset.setAtlasID("buildings_wobbly", "buildings");
				}
				buildingAsset.shadow = item.has_shadows;
				buildingAsset.burnable = item.burnable_buildings;
				buildingAsset.affected_by_acid = item.acid_affected_buildings;
				switch (text2)
				{
				case "order_tent":
					buildingAsset.fundament = new BuildingFundament(2, 2, 2, 0);
					break;
				case "order_hall_0":
					buildingAsset.fundament = new BuildingFundament(3, 3, 4, 0);
					break;
				case "order_temple":
					buildingAsset.fundament = new BuildingFundament(2, 2, 3, 0);
					break;
				case "order_watch_tower":
					buildingAsset.fundament = new BuildingFundament(1, 1, 1, 0);
					if (!string.IsNullOrEmpty(item.projectile_id))
					{
						buildingAsset.tower_projectile = item.projectile_id;
					}
					break;
				case "order_library":
					buildingAsset.fundament = new BuildingFundament(2, 2, 2, 0);
					break;
				case "order_docks_0":
				{
					string upgrade_to = "docks_" + text;
					buildingAsset.upgrade_to = upgrade_to;
					buildingAsset.can_be_upgraded = true;
					break;
				}
				case "order_docks_1":
				{
					string upgraded_from = "fishing_docks_" + text;
					buildingAsset.upgraded_from = upgraded_from;
					buildingAsset.has_sprites_main_disabled = false;
					break;
				}
				case "order_windmill_0":
					buildingAsset.fundament = new BuildingFundament(2, 2, 2, 0);
					if (buildingAsset.shadow)
					{
						buildingAsset.setShadow(0.4f, 0.38f, 0.47f);
					}
					break;
				}
			}
		}
	}

	private void addTrees()
	{
		add(new BuildingAsset
		{
			id = "tree_green_1",
			fundament = new BuildingFundament(1, 1, 1, 0),
			building_type = BuildingType.Building_Tree,
			type = "type_tree",
			destroy_on_liquid = true,
			random_flip = true,
			ignored_by_cities = true,
			burnable = true,
			affected_by_acid = true,
			affected_by_lava = true,
			flora = true,
			flora_size = FloraSize.Big,
			can_be_damaged_by_tornado = true,
			group = "nature",
			kingdom = "nature",
			check_for_close_building = false,
			biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(default(BiomeTag)),
			material = "tree",
			affected_by_drought = true,
			affected_by_cold_temperature = true,
			main_path = "buildings/trees/",
			can_be_chopped_down = true,
			has_resources_to_collect = true,
			is_vegetation = true
		});
		t.setAtlasID("buildings_trees", "buildings");
		t.nutrition_restore = 40;
		t.sound_spawn = "event:/SFX/NATURE/BaseFloraSpawn";
		t.remove_ruins = false;
		t.setSpread(FloraType.Tree, 10, 0.5f);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("tree_green_1");
		t.setShadow(0.5f, 0.14f, 0.08f);
		t.limit_per_zone = 3;
		t.can_be_living_plant = true;
		t.base_stats["health"] = 10f;
		t.addResource("wood", 5);
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("tree_green_2", "tree_green_1");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("tree_green_2");
		clone("tree_green_3", "tree_green_1");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("tree_green_3");
		clone("corrupted_tree", "tree_green_1");
		t.become_alive_when_chopped = true;
		t.limit_per_zone = 4;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Corrupted);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("corrupted_tree", "corrupted_tree_big");
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleCorruptedTree";
		t.affected_by_cold_temperature = false;
		clone("corrupted_tree_big", "corrupted_tree");
		t.become_alive_when_chopped = true;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Corrupted);
		t.fundament = new BuildingFundament(2, 2, 1, 0);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("corrupted_tree", "corrupted_tree_big");
		clone("enchanted_tree", "tree_green_1");
		t.limit_per_zone = 4;
		t.draw_light_area = true;
		t.draw_light_size = 0.2f;
		t.draw_light_area_offset_y = 2f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Enchanted);
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Green, BiomeTag.Clover, BiomeTag.Flower, BiomeTag.Garlic, BiomeTag.Maple, BiomeTag.Birch, BiomeTag.Enchanted);
		t.setShadow(0.5f, 0.03f, 0.12f);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("enchanted_tree");
		clone("swamp_tree", "tree_green_1");
		t.fundament = new BuildingFundament(1, 1, 1, 0);
		t.limit_per_zone = 2;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Swamp);
		t.can_be_placed_on_liquid = true;
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("swamp_tree");
		clone("savanna_tree_1", "tree_green_1");
		t.limit_per_zone = 3;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Savanna);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("savanna_tree_1", "savanna_tree_big_1");
		clone("savanna_tree_2", "savanna_tree_1");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("savanna_tree_2", "savanna_tree_big_2");
		clone("savanna_tree_big_1", "savanna_tree_1");
		t.fundament = new BuildingFundament(2, 2, 1, 0);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("savanna_tree_1", "savanna_tree_big_1");
		clone("savanna_tree_big_2", "savanna_tree_big_1");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("savanna_tree_2", "savanna_tree_big_2");
		clone("mushroom_tree", "tree_green_1");
		t.limit_per_zone = 2;
		t.setSpread(FloraType.Fungi, 10, 0.45f);
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Mushroom, BiomeTag.Green);
		t.addResource("mushrooms", 1);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("mushroom_tree");
		clone("jungle_tree", "tree_green_1");
		t.limit_per_zone = 8;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Jungle);
		t.addResource("bananas", 1);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("jungle_tree");
		clone("infernal_tree", "tree_green_1");
		t.draw_light_area = true;
		t.draw_light_size = 0.05f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Infernal);
		t.burnable = false;
		t.affected_by_drought = false;
		t.setShadow(0.1f, 0.31f, 0.33f);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("infernal_tree", "infernal_tree_small", "infernal_tree_big");
		t.affected_by_cold_temperature = false;
		clone("infernal_tree_small", "infernal_tree");
		t.fundament = new BuildingFundament(0, 0, 1, 0);
		t.setShadow(0.5f, 0.31f, 0.33f);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("infernal_tree", "infernal_tree_small", "infernal_tree_big");
		clone("infernal_tree_big", "infernal_tree");
		t.fundament = new BuildingFundament(2, 2, 1, 0);
		t.draw_light_area = true;
		t.draw_light_size = 0.1f;
		t.setShadow(0.37f, 0.16f, 0.2f);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("infernal_tree", "infernal_tree_small", "infernal_tree_big");
		clone("cacti_tree", "tree_green_1");
		t.affected_by_drought = false;
		t.vegetation_random_chance = 0.2f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Sand);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("cacti_tree");
		clone("palm_tree", "tree_green_1");
		t.vegetation_random_chance = 0.1f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Sand);
		t.setShadow(0.37f, 0.16f, 0f);
		t.addResource("coconut", 1);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("palm_tree");
		clone("desert_tree", "tree_green_1");
		t.affected_by_drought = false;
		t.limit_per_zone = 1;
		t.vegetation_random_chance = 0.1f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Desert);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("desert_tree");
		clone("crystal_tree", "tree_green_1");
		t.affected_by_drought = false;
		t.burnable = false;
		t.draw_light_area = true;
		t.draw_light_size = 0.1f;
		t.limit_per_zone = 1;
		t.vegetation_random_chance = 0.1f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Crystal, BiomeTag.Rocklands);
		t.material = "building";
		t.setAtlasID("buildings");
		t.sparkle_effect = true;
		t.addResource("wood", 5, pNewList: true);
		t.addResource("stone", 1);
		t.addResource("gems", 1);
		t.addResource("common_metals", 1);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("crystal_tree");
		t.affected_by_cold_temperature = false;
		clone("wasteland_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Soil, BiomeTag.Wasteland);
		t.affected_by_acid = false;
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("wasteland_tree");
		clone("candy_tree", "tree_green_1");
		t.limit_per_zone = 1;
		t.vegetation_random_chance = 0.1f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Candy);
		t.addResource("candy", 3);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("candy_tree");
		t.affected_by_cold_temperature = false;
		clone("lemon_tree", "tree_green_1");
		t.limit_per_zone = 1;
		t.vegetation_random_chance = 0.1f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Lemon, BiomeTag.Green);
		t.addResource("lemons", 3);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("lemon_tree");
		t.affected_by_cold_temperature = false;
		clone("pine_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Soil, BiomeTag.Green, BiomeTag.Permafrost);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("pine_tree");
		t.addResource("pine_cones", 3);
		t.affected_by_cold_temperature = false;
		clone("birch_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Green, BiomeTag.Birch);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("birch_tree");
		clone("maple_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Green, BiomeTag.Maple);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("maple_tree");
		clone("garlic_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Garlic);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("garlic_tree");
		clone("flower_tree_1", "tree_green_1");
		t.setSpread(FloraType.Plant, 10, 0.3f);
		t.vegetation_random_chance = 0.5f;
		t.flora_size = FloraSize.Big;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Flower);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("flower_tree_1");
		clone("flower_tree_2", "flower_tree_1");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("flower_tree_2");
		clone("flower_tree_3", "flower_tree_1");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("flower_tree_3");
		clone("rocklands_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.material = "building";
		t.setAtlasID("buildings");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Rocklands);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("rocklands_tree");
		t.limit_in_radius = 6;
		t.affected_by_cold_temperature = false;
		clone("celestial_tree", "tree_green_1");
		t.can_be_living_plant = false;
		t.ignored_by_cities = false;
		t.can_be_chopped_down = false;
		t.material = "tree_celestial";
		t.setAtlasID("buildings_trees_big");
		t.vegetation_random_chance = 0.5f;
		t.setShadow(0.5f, 0.03f, 0.05f);
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Celestial);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("celestial_tree");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.limit_per_zone = 1;
		t.limit_in_radius = 30;
		t.affected_by_cold_temperature = false;
		t.draw_light_area = true;
		t.draw_light_size = 1f;
		t.addResource("celestial_avocado", 3);
		t.addResource("wood", 100);
		clone("celestial_tree_small", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.material = "building";
		t.setAtlasID("buildings_trees", "buildings");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Celestial);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("celestial_tree_small");
		t.limit_in_radius = 6;
		t.limit_per_zone = 15;
		t.affected_by_cold_temperature = false;
		t.draw_light_area = true;
		t.draw_light_size = 0.2f;
		t.addResource("celestial_avocado", 1);
		clone("singularity_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.limit_per_zone = 1;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Singularity);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("singularity_tree");
		t.affected_by_cold_temperature = false;
		clone("clover_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Clover);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("clover_tree");
		clone("paradox_tree", "tree_green_1");
		t.vegetation_random_chance = 0.5f;
		t.limit_per_zone = 1;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Paradox);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("paradox_tree");
		t.affected_by_cold_temperature = false;
	}

	private void addVegetation()
	{
		add(new BuildingAsset
		{
			id = "$flora_small$",
			fundament = new BuildingFundament(0, 0, 0, 0),
			has_ruins_graphics = false,
			destroy_on_liquid = true,
			random_flip = true,
			ignored_by_cities = true,
			burnable = true,
			affected_by_acid = true,
			affected_by_lava = true,
			flora = true,
			flora_size = FloraSize.Tiny,
			affected_by_cold_temperature = true,
			group = "nature",
			kingdom = "nature",
			building_type = BuildingType.Building_Plant,
			material = "tree",
			main_path = "buildings/vegetation/",
			is_vegetation = true
		});
		t.setAtlasID("buildings_trees", "buildings");
		t.has_ruin_state = false;
		t.remove_ruins = false;
		t.setSpread(FloraType.Plant, 5, 0.3f);
		t.type = "type_vegetation";
		t.nutrition_restore = 10;
		t.limit_per_zone = 5;
		t.priority = -1;
		t.can_be_placed_on_blocks = false;
		t.base_stats["health"] = 10f;
		t.sound_spawn = "event:/SFX/NATURE/BaseFloraSpawn";
		t.shadow = false;
		t.addResource("herbs", 1);
		t.has_sprites_main = true;
		clone("desert_plant", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Desert);
		t.limit_per_zone = 3;
		t.addResource("desert_berries", 1, pNewList: true);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("desert_plant");
		clone("crystal_plant", "$flora_small$");
		t.limit_per_zone = 2;
		t.setShadow(0.19f, 0.03f, 0.09f);
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Crystal);
		t.material = "building";
		t.setAtlasID("buildings");
		t.burnable = false;
		t.sparkle_effect = true;
		t.addResource("gems", 1, pNewList: true);
		t.addResource("crystal_salt", 1);
		t.addResource("common_metals", 2);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("crystal_plant");
		t.affected_by_cold_temperature = false;
		clone("candy_plant", "$flora_small$");
		t.setShadow(0.19f, 0.03f, 0.09f);
		t.fundament = new BuildingFundament(1, 1, 1, 0);
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Candy);
		t.addResource("candy", 1, pNewList: true);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("candy_plant");
		t.affected_by_cold_temperature = false;
		clone("snow_plant", "$flora_small$");
		t.limit_per_zone = 4;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Permafrost);
		t.addResource("snow_cucumbers", 1);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("snow_plant");
		t.affected_by_cold_temperature = false;
		clone("green_herb", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Green, BiomeTag.Lemon, BiomeTag.Jungle);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("green_herb");
		clone("corrupted_plant", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Corrupted);
		t.addResource("evil_beets", 1, pNewList: true);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("corrupted_plant");
		t.affected_by_cold_temperature = false;
		clone("jungle_plant", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Jungle);
		t.limit_per_zone = 6;
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("jungle_plant");
		clone("savanna_plant", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Savanna);
		t.addResource("wheat", 1);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("savanna_plant");
		clone("mushroom_red", "$flora_small$");
		t.fundament = new BuildingFundament(1, 1, 1, 0);
		t.limit_per_zone = 9;
		t.setSpread(FloraType.Fungi, 4, 0.5f);
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Lemon, BiomeTag.Green, BiomeTag.Mushroom);
		t.biome_tags_spread = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Hills);
		t.addResource("mushrooms", 1, pNewList: true);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("mushroom_red");
		clone("mushroom_green", "mushroom_red");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("mushroom_green");
		clone("mushroom_teal", "mushroom_red");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("mushroom_teal");
		clone("mushroom_white", "mushroom_red");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("mushroom_white");
		clone("mushroom_yellow", "mushroom_red");
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("mushroom_yellow");
		clone("flower", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Lemon, BiomeTag.Green, BiomeTag.Mushroom, BiomeTag.Enchanted);
		t.type = "type_flower";
		t.nutrition_restore = 15;
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("flower");
		clone("flame_flower", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Infernal);
		t.burnable = false;
		t.addResource("peppers", 1, pNewList: true);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("flame_flower");
		t.affected_by_cold_temperature = false;
		clone("jungle_flower", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Jungle);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("jungle_flower");
		clone("wasteland_flower", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Wasteland);
		t.affected_by_acid = false;
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("wasteland_flower");
		clone("swamp_plant", "$flora_small$");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Swamp);
		t.fundament = new BuildingFundament(0, 0, 0, 0);
		t.can_be_placed_on_liquid = true;
		t.limit_per_zone = 4;
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("swamp_plant", "swamp_plant_big");
		clone("swamp_plant_big", "swamp_plant");
		t.limit_per_zone = 4;
		t.fundament = new BuildingFundament(1, 1, 1, 0);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("swamp_plant", "swamp_plant_big");
		clone("birch_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Birch, BiomeTag.Green);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("birch_plant");
		clone("maple_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Maple, BiomeTag.Green);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("maple_plant");
		clone("flower_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Flower, BiomeTag.Green);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("flower_plant");
		clone("garlic_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Garlic, BiomeTag.Green);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("garlic_plant");
		clone("rocklands_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Rocklands);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("rocklands_plant");
		t.affected_by_cold_temperature = false;
		clone("celestial_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Celestial);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("celestial_plant");
		t.affected_by_cold_temperature = false;
		clone("singularity_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Singularity);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("singularity_plant");
		t.affected_by_cold_temperature = false;
		clone("clover_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Clover, BiomeTag.Green);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("clover_plant");
		clone("paradox_plant", "flower");
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Paradox);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("paradox_plant");
		t.affected_by_cold_temperature = false;
	}

	private void addMinerals()
	{
		add(new BuildingAsset
		{
			id = "$resource$",
			fundament = new BuildingFundament(1, 1, 1, 0),
			has_resources_to_collect = true,
			has_ruins_graphics = false,
			destroy_on_liquid = true,
			random_flip = true,
			ignored_by_cities = false,
			burnable = false,
			affected_by_acid = true,
			affected_by_lava = true,
			group = "nature",
			kingdom = "nature",
			main_path = "buildings/minerals/"
		});
		t.setAtlasID("buildings");
		t.remove_ruins = false;
		t.can_be_placed_on_blocks = false;
		t.base_stats["health"] = 10f;
		clone("$mineral$", "$resource$");
		t.type = "type_mineral";
		t.has_ruin_state = false;
		t.remove_ruins = true;
		t.ignore_buildings = false;
		t.ignored_by_cities = true;
		t.ignore_same_building_id = true;
		t.building_type = BuildingType.Building_Mineral;
		t.vegetation_random_chance = 0.1f;
		t.limit_per_zone = 1;
		t.setShadow(0.19f, 0.03f, 0.09f);
		t.has_sprites_main = true;
		t.nutrition_restore = 30;
		clone("mineral_bones", "$mineral$");
		t.addResource("bones", 3);
		t.addResource("stone", 1);
		clone("mineral_adamantine", "$mineral$");
		t.draw_light_area = true;
		t.draw_light_size = 0.15f;
		t.sparkle_effect = true;
		t.nutrition_restore = 60;
		t.addResource("adamantine", 1);
		t.addResource("stone", 1);
		clone("mineral_mythril", "$mineral$");
		t.draw_light_area = true;
		t.draw_light_size = 0.1f;
		t.sparkle_effect = true;
		t.nutrition_restore = 40;
		t.addResource("mythril", 1);
		t.addResource("stone", 1);
		clone("mineral_gems", "$mineral$");
		t.sparkle_effect = true;
		t.nutrition_restore = 70;
		t.addResource("gems", 1);
		t.addResource("stone", 1);
		clone("mineral_stone", "$mineral$");
		t.addResource("stone", 3);
		clone("mineral_metals", "$mineral$");
		t.sparkle_effect = true;
		t.addResource("common_metals", 2);
		t.addResource("stone", 1);
		clone("mineral_gold", "$mineral$");
		t.sparkle_effect = true;
		t.addResource("gold", 20);
		t.addResource("stone", 20);
		clone("mineral_silver", "$mineral$");
		t.sparkle_effect = true;
		t.addResource("silver", 1);
		t.addResource("stone", 1);
	}

	private void addPoop()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		add(new BuildingAsset
		{
			id = "poop",
			building_type = BuildingType.Building_Poops,
			fundament = new BuildingFundament(0, 0, 0, 0),
			has_ruins_graphics = false,
			has_ruin_state = false,
			destroy_on_liquid = true,
			random_flip = true,
			ignored_by_cities = true,
			burnable = true,
			affected_by_acid = true,
			affected_by_lava = true,
			flora = true,
			flora_size = FloraSize.Tiny,
			group = "nature",
			kingdom = "nature",
			main_path = "buildings/nature/",
			removed_by_sponge = true
		});
		t.scale_base = new Vector3(0.1f, 0.1f, 0.1f);
		t.type = "type_poop";
		t.remove_ruins = true;
		t.addResource("fertilizer", 1);
		t.base_stats["health"] = 10f;
	}

	private void addGrownResources()
	{
		clone("fruit_bush", "$resource$");
		t.main_path = "buildings/nature/";
		t.has_ruin_state = false;
		t.can_be_living_plant = true;
		t.building_type = BuildingType.Building_Fruits;
		t.is_vegetation = true;
		t.has_special_animation_state = true;
		t.addResource("berries", 3);
		t.nutrition_restore = 30;
		t.type = "type_fruits";
		t.burnable = true;
		t.flora = true;
		t.can_be_damaged_by_tornado = true;
		t.ignored_by_cities = true;
		t.vegetation_random_chance = 0.2f;
		t.limit_per_zone = 1;
		t.biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Lemon, BiomeTag.Green, BiomeTag.Mushroom, BiomeTag.Enchanted, BiomeTag.Jungle, BiomeTag.Savanna, BiomeTag.Maple, BiomeTag.Birch, BiomeTag.Flower, BiomeTag.Garlic, BiomeTag.Clover);
		t.setSpread(FloraType.Plant, 10);
		t.spread_ids = AssetLibrary<BuildingAsset>.a<string>("fruit_bush");
		t.material = "tree";
		t.setAtlasID("buildings_trees", "buildings");
		t.setShadow(0.19f, 0.03f, 0.09f);
		t.has_sprites_main = true;
		t.has_sprites_special = true;
		t.gatherable = true;
		t.has_resources_grown_to_collect = true;
		t.has_resources_grown_to_collect_on_spawn = true;
		add(new BuildingAsset
		{
			id = "wheat",
			fundament = new BuildingFundament(0, 0, 0, 0),
			type = "type_crops",
			building_type = BuildingType.Building_Wheat,
			destroy_on_liquid = true,
			random_flip = true,
			ignored_by_cities = true,
			burnable = true,
			affected_by_acid = true,
			affected_by_lava = true,
			flora = true,
			can_be_damaged_by_tornado = true,
			group = "nature",
			kingdom = "nature",
			shadow = false,
			biome_tags_growth = AssetLibrary<BuildingAsset>.h<BiomeTag>(BiomeTag.Field),
			has_ruins_graphics = false,
			material = "tree",
			wheat = true,
			growth_time = 50f,
			main_path = "buildings/nature/",
			can_be_living_plant = true,
			can_be_grown = true
		});
		t.setAtlasID("buildings_trees", "buildings");
		t.nutrition_restore = 20;
		t.has_ruin_state = false;
		t.addResource("wheat", 1);
		t.base_stats["health"] = 10f;
		t.has_sprites_main = true;
		t.get_map_icon_color = delegate(Building pBuilding)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			int animData_index = pBuilding.animData_index;
			return Color32.op_Implicit(Toolbox.colors_wheat[animData_index]);
		};
	}

	private void addGeneralCityBuildings()
	{
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		add(new BuildingAsset
		{
			id = "$building$",
			fundament = new BuildingFundament(3, 3, 2, 0),
			burnable = true,
			destroy_on_liquid = true,
			build_road_to = true,
			affected_by_acid = true,
			affected_by_lava = true,
			can_be_damaged_by_tornado = true,
			only_build_tiles = true,
			check_for_close_building = true,
			sound_hit = "event:/SFX/HIT/HitGeneric",
			main_path = "buildings/nature/",
			can_be_demolished = true
		});
		t.base_stats["health"] = 1500f;
		t.setShadow(0.5f, 0.35f, 0.53f);
		clone("$city_building$", "$building$");
		t.building_type = BuildingType.Building_Civ;
		t.has_sprite_construction = true;
		t.main_path = "buildings/civ_general/";
		t.construction_progress_needed = 50;
		t.city_building = true;
		t.can_be_abandoned = true;
		t.build_place_batch = true;
		t.setShadow(0.5f, 0.37f, 0.28f);
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		t.check_for_adaptation_tags = true;
		clone("$city_colored_building$", "$city_building$");
		t.has_kingdom_color = true;
		clone("bonfire", "$city_building$");
		t.burnable = false;
		t.draw_light_area = true;
		t.draw_light_size = 0.8f;
		t.can_be_abandoned = false;
		t.priority = 120;
		t.type = "type_bonfire";
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.construction_progress_needed = 30;
		t.cost = new ConstructionCost();
		t.smoke = true;
		t.smoke_interval = 2.5f;
		t.smoke_offset = new Vector2Int(2, 3);
		t.can_be_living_house = false;
		t.build_place_batch = false;
		t.build_prefer_replace_house = true;
		t.check_for_close_building = false;
		t.max_houses = 3;
		t.produce_biome_food = true;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBonfire";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
		t.setShadow(0.19f, 0.5f, 0.27f);
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		t.check_for_adaptation_tags = false;
		clone("well", "$city_building$");
		t.priority = 21;
		t.type = "type_well";
		t.fundament = new BuildingFundament(2, 2, 1, 0);
		t.cost = new ConstructionCost(0, 20, 1, 5);
		t.construction_progress_needed = 200;
		t.burnable = false;
		t.max_houses = 3;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWell";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("training_dummy", "$city_building$");
		t.priority = 23;
		t.type = "type_training_dummies";
		t.fundament = new BuildingFundament(0, 0, 0, 0);
		t.cost = new ConstructionCost(5, 0, 0, 5);
		t.construction_progress_needed = 100;
		t.burnable = true;
		t.max_houses = 3;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
		t.has_sprite_construction = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		t.setShadow(0.6f, 0.27f, 0.23f);
		clone("stockpile", "$city_building$");
		t.priority = 100;
		t.is_stockpile = true;
		t.shadow = false;
		t.stockpile_top_left_offset = new Vector2(-2f, 3.5f);
		t.stockpile_center_offset = new Vector2(0f, 1.5f);
		t.storage = true;
		t.type = "type_stockpile";
		t.fundament = new BuildingFundament(3, 3, 5, 0);
		t.cost = new ConstructionCost();
		t.bonus_z = -5f;
		t.construction_progress_needed = 10;
		t.burnable = true;
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("stockpile_fireproof", "stockpile");
		t.burnable = false;
		clone("stockpile_acidproof", "stockpile");
		t.affected_by_acid = false;
		clone("statue", "$city_building$");
		t.priority = 27;
		t.type = "type_statue";
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.cost = new ConstructionCost(0, 5, 0, 25);
		t.burnable = false;
		t.max_houses = 3;
		t.setShadow(0.5f, 0.17f, 0.26f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleStatue";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("mine", "$city_building$");
		t.priority = 50;
		t.type = "type_mine";
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.cost = new ConstructionCost(0, 0, 0, 15);
		t.construction_progress_needed = 300;
		t.burnable = false;
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.build_place_single = true;
		t.build_place_batch = false;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleMine";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("$windmill_base$", "$city_colored_building$");
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.priority = 23;
		t.burnable = false;
		t.storage = true;
		t.storage_only_food = true;
		t.type = "type_windmill";
		t.needs_farms_ground = true;
		t.build_place_center = true;
		t.build_place_single = true;
		t.build_place_batch = false;
		t.setShadow(0.5f, 0.23f, 0.27f);
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWindmill";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
		clone("$windmill_0$", "$windmill_base$");
		t.cost = new ConstructionCost(5, 0, 0, 5);
		t.can_be_upgraded = true;
		t.sound_hit = "event:/SFX/HIT/HitWood";
		clone("$windmill_1$", "$windmill_base$");
		t.cost = new ConstructionCost(0, 5, 5, 30);
		t.can_be_upgraded = false;
		t.has_sprite_construction = false;
	}

	private void addNatureBuildings()
	{
		//IL_084f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0854: Unknown result type (might be due to invalid IL or missing references)
		clone("golden_brain", "$building$");
		t.building_type = BuildingType.Building_Nature;
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.base_stats["health"] = 10000f;
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.group = "golden_brain";
		t.kingdom = "golden_brain";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleGoldenBrain";
		t.setShadow(0.56f, 0.23f, 0.28f);
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("$waypoint$", "$building$");
		t.waypoint = true;
		t.building_type = BuildingType.Building_Nature;
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.base_stats["health"] = 10000f;
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.group = "nature";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.limit_global = 1;
		t.setShadow(0.56f, 0.23f, 0.28f);
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("waypoint_alien_mold", "$waypoint$");
		t.kingdom = "alien_mold";
		clone("waypoint_computer", "$waypoint$");
		t.kingdom = "computer";
		clone("waypoint_golden_egg", "$waypoint$");
		t.kingdom = "golden_egg";
		clone("waypoint_harp", "$waypoint$");
		t.kingdom = "harp";
		clone("corrupted_brain", "$building$");
		t.building_type = BuildingType.Building_Nature;
		t.draw_light_area = true;
		t.draw_light_size = 0.5f;
		t.base_stats["health"] = 10000f;
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.group = "corrupted_brain";
		t.kingdom = "corrupted_brain";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.tower = true;
		t.tower_attack_buildings = false;
		t.tower_projectile = "madness_ball";
		t.tower_projectile_offset = 6f;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleCorruptedBrain";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.setShadow(0.44f, 0.38f, 0.37f);
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingFlesh";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingFlesh";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("monolith", "$building$");
		t.building_type = BuildingType.Building_Nature;
		t.draw_light_area = true;
		t.ignored_by_cities = false;
		t.draw_light_size = 1f;
		t.base_stats["health"] = 50000f;
		t.fundament = new BuildingFundament(2, 2, 3, 0);
		t.group = "nature";
		t.kingdom = "nature";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.setShadow(0.56f, 0.23f, 0.28f);
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		t.has_sprites_special = true;
		clone("beehive", "$building$");
		t.building_type = BuildingType.Building_Hives;
		t.base_stats["health"] = 100f;
		t.fundament = new BuildingFundament(1, 0, 1, 0);
		t.group = "nature";
		t.kingdom = "neutral_animals";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = true;
		t.housing_slots = 5;
		t.beehive = true;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBeehive";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingGeneric";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		t.has_special_animation_state = true;
		t.addResource("honey", 1);
		t.type = "type_hive";
		t.gatherable = true;
		t.has_resources_grown_to_collect = true;
		t.has_resources_grown_to_collect_on_spawn = false;
		clone("$drop_spreader$", "$building$");
		t.building_type = BuildingType.Building_Nature;
		t.group = "nature";
		t.kingdom = "nature";
		t.burnable = false;
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.can_be_placed_on_blocks = true;
		t.destroy_on_liquid = false;
		t.ignored_by_cities = false;
		t.affected_by_lava = false;
		t.can_be_placed_on_liquid = true;
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		t.has_sprites_main_disabled = true;
		t.can_be_damaged_by_tornado = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = false;
		t.spawn_drops = true;
		clone("volcano", "$drop_spreader$");
		t.draw_light_area = true;
		t.draw_light_size = 0.8f;
		t.transform_tiles_to_tile_type = "lava3";
		t.smoke = true;
		t.smoke_interval = 1.5f;
		t.smoke_offset = new Vector2Int(2, 2);
		t.spawn_drop_id = "lava";
		t.spawn_drop_start_height = 1.8f;
		t.spawn_drop_min_height = 5f;
		t.spawn_drop_max_height = 30f;
		t.spawn_drop_interval = 0.1f;
		t.spawn_drop_min_radius = 2f;
		t.spawn_drop_max_radius = 8f;
		t.step_action = delegate(Actor pActor, Building pBuilding)
		{
			if (pActor.asset.die_in_lava && !pActor.isUnderDamageCooldown() && !pBuilding.isRuin())
			{
				pActor.getHit(200f, pFlash: true, AttackType.Fire);
				if (!pActor.isAlive())
				{
					CursedSacrifice.checkGoodForSacrifice(pActor);
					pActor.skipUpdates();
				}
			}
		};
		t.setShadow(0.4f, 0f, 0.7f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleVolcano";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("geyser", "$drop_spreader$");
		t.smoke = true;
		t.smoke_interval = 2.5f;
		t.spawn_drop_id = "rain";
		t.spawn_drop_start_height = 2.5f;
		t.spawn_drop_min_height = 10f;
		t.spawn_drop_max_height = 40f;
		t.spawn_drop_min_radius = 2f;
		t.spawn_drop_max_radius = 17f;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleGeyser";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("geyser_acid", "$drop_spreader$");
		t.smoke = true;
		t.smoke_interval = 3.5f;
		t.spawn_drop_id = "acid";
		t.affected_by_acid = false;
		t.spawn_drop_start_height = 2f;
		t.spawn_drop_min_height = 5f;
		t.spawn_drop_max_height = 36f;
		t.spawn_drop_min_radius = 2f;
		t.spawn_drop_max_radius = 15f;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleAcidGeyser";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
	}

	private void addMobBuildings()
	{
		clone("flame_tower", "$building$");
		t.building_type = BuildingType.Building_Mob;
		t.main_path = "buildings/mobs/";
		t.draw_light_area = true;
		t.draw_light_size = 0.5f;
		t.draw_light_area_offset_y = 8f;
		t.base_stats["health"] = 1000f;
		t.fundament = new BuildingFundament(2, 2, 3, 0);
		t.group = "demon";
		t.kingdom = "demon";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.spawn_units = true;
		t.spawn_units_asset = "demon";
		t.housing_slots = 5;
		t.tower = true;
		t.tower_attack_buildings = true;
		t.tower_projectile = "fireball";
		t.tower_projectile_offset = 10f;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleFlameTower";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("ice_tower", "$building$");
		t.building_type = BuildingType.Building_Mob;
		t.main_path = "buildings/mobs/";
		t.base_stats["health"] = 1000f;
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.group = "cold_one";
		t.kingdom = "cold_one";
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.ice_tower = true;
		t.spawn_units = true;
		t.spawn_units_asset = "cold_one";
		t.housing_slots = 5;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleIceTower";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("angle_tower", "$building$");
		t.building_type = BuildingType.Building_Mob;
		t.main_path = "buildings/mobs/";
		t.base_stats["health"] = 1000f;
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.group = "angle";
		t.kingdom = "angle";
		t.housing_slots = 5;
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = true;
		t.burnable = false;
		t.spawn_units = true;
		t.spawn_units_asset = "angle";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
	}

	private void addCreeps()
	{
		clone("$building_creep$", "$building$");
		t.main_path = "buildings/creeps/";
		t.building_type = BuildingType.Building_Creep;
		t.has_sprites_spawn = true;
		t.has_sprites_main = true;
		t.has_sprites_ruin = true;
		clone("tumor", "$building_creep$");
		t.material = "jelly";
		t.setAtlasID("buildings_wobbly", "buildings");
		t.transform_tiles_to_top_tiles = "tumor_low";
		t.fundament = new BuildingFundament(1, 1, 1, 0);
		t.group = "tumor";
		t.kingdom = "tumor";
		t.can_be_placed_on_blocks = false;
		t.can_be_placed_on_liquid = false;
		t.ignore_buildings = true;
		t.check_for_close_building = false;
		t.can_be_living_house = false;
		t.spawn_units = true;
		t.spawn_units_asset = "tumor_monster_animal";
		t.housing_slots = 5;
		setGrowBiomeAround("biome_tumor", 5, 2, 0.1f, CreepWorkerMovementType.Direction);
		t.grow_creep_direction_random_position = true;
		t.grow_creep_flash = true;
		t.grow_creep_redraw_tile = true;
		t.setShadow(0.2f, 0.08f, 0.66f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTumor";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingFlesh";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingFlesh";
		clone("biomass", "tumor");
		t.group = "biomass";
		t.kingdom = "biomass";
		t.spawn_units_asset = "bioblob";
		t.housing_slots = 5;
		t.transform_tiles_to_top_tiles = "biomass_low";
		setGrowBiomeAround("biome_biomass", 10, 4, 0.7f, CreepWorkerMovementType.RandomNeighbourAll);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBiomass";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingFlesh";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingFlesh";
		clone("super_pumpkin", "tumor");
		t.group = "super_pumpkin";
		t.kingdom = "super_pumpkin";
		t.spawn_units_asset = "lil_pumpkin";
		t.housing_slots = 5;
		t.transform_tiles_to_top_tiles = "pumpkin_low";
		setGrowBiomeAround("biome_pumpkin", 10, 3, 0.2f, CreepWorkerMovementType.Direction);
		t.grow_creep_direction_random_position = true;
		t.grow_creep_random_new_direction = true;
		t.grow_creep_steps_before_new_direction = 20;
		t.grow_creep_flash = true;
		t.grow_creep_redraw_tile = true;
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleSuperPumpkin";
		t.sound_hit = "event:/SFX/HIT/HitFlesh";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingFlesh";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingFlesh";
		clone("cybercore", "tumor");
		t.group = "assimilators";
		t.draw_light_area = true;
		t.draw_light_size = 0.2f;
		t.draw_light_area_offset_y = 2f;
		t.kingdom = "assimilators";
		t.spawn_units_asset = "assimilator";
		t.housing_slots = 5;
		t.transform_tiles_to_top_tiles = "cybertile_low";
		setGrowBiomeAround("biome_cybertile", 20, 6, 2f, CreepWorkerMovementType.Direction);
		t.grow_creep_steps_before_new_direction = 7;
		t.grow_creep_direction_random_position = false;
		t.grow_creep_random_new_direction = true;
		t.damaged_by_rain = true;
		t.burnable = false;
		t.material = "building";
		t.setAtlasID("buildings");
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleCybercore";
		t.sound_hit = "event:/SFX/HIT/HitMetal";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingRobotic";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingRobotic";
	}

	private void addHumans()
	{
		clone("$building_civ_human$", "$city_colored_building$");
		t.main_path = "buildings/civ_main/human/";
		t.group = "human";
		t.civ_kingdom = "human";
		clone("fishing_docks_human", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.2f;
		t.draw_light_area_offset_y = 2f;
		t.sprite_path = "buildings/civ_general/fishing_dock";
		t.priority = 20;
		t.type = "type_docks";
		t.fundament = new BuildingFundament(2, 2, 4, 0);
		t.cost = new ConstructionCost(10);
		t.burnable = false;
		t.docks = true;
		t.can_be_placed_on_liquid = true;
		t.destroy_on_liquid = false;
		t.build_road_to = false;
		t.only_build_tiles = false;
		t.auto_remove_ruin = true;
		t.max_houses = 1;
		t.can_be_upgraded = true;
		t.upgrade_level = 1;
		t.upgrade_to = "docks_human";
		t.boat_types = new string[1] { "boat_type_fishing" };
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleFishingDocks";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
		t.setShadow(0.5f, 0.55f, 0.63f);
		clone("watch_tower_human", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.5f;
		t.base_stats["health"] = 3000f;
		t.base_stats["targets"] = 1f;
		t.base_stats["area_of_effect"] = 1f;
		t.base_stats["damage"] = 50f;
		t.base_stats["knockback"] = 1f;
		t.priority = 22;
		t.type = "type_watch_tower";
		t.fundament = new BuildingFundament(1, 1, 1, 0);
		t.cost = new ConstructionCost(0, 20, 1, 5);
		t.burnable = false;
		t.tower = true;
		t.tower_attack_buildings = true;
		t.tower_projectile = "arrow";
		t.tower_projectile_offset = 4f;
		t.tower_projectile_amount = 6;
		t.build_place_borders = true;
		t.build_place_batch = false;
		t.build_place_single = true;
		t.setShadow(0.5f, 0.23f, 0.27f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleWatchTower";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("docks_human", "fishing_docks_human");
		t.sprite_path = string.Empty;
		t.cost = new ConstructionCost(10, 6);
		t.draw_light_area = true;
		t.draw_light_size = 0.5f;
		t.draw_light_area_offset_y = 8f;
		t.can_be_upgraded = false;
		t.upgraded_from = "fishing_docks_human";
		t.boat_types = new string[3] { "boat_type_fishing", "boat_type_trading", "boat_type_transport" };
		t.setShadow(0.5f, 0.55f, 0.63f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleDocks";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprites_main_disabled = true;
		clone("barracks_human", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.5f;
		t.priority = 22;
		t.burnable = false;
		t.type = "type_barracks";
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.cost = new ConstructionCost(0, 5, 2, 15);
		t.setShadow(0.56f, 0.41f, 0.43f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleBarracks";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("temple_human", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.draw_light_area_offset_y = 3f;
		t.priority = 26;
		t.type = "type_temple";
		t.fundament = new BuildingFundament(2, 2, 3, 0);
		t.cost = new ConstructionCost(0, 10, 2, 30);
		t.burnable = false;
		t.group = "human";
		t.max_houses = 2;
		t.setShadow(0.56f, 0.41f, 0.43f);
		t.sound_idle = "event:/SFX/BUILDINGS_IDLE/IdleTemple";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("library_human", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.draw_light_area_offset_y = 3f;
		t.priority = 26;
		t.type = "type_library";
		t.fundament = new BuildingFundament(2, 2, 3, 0);
		t.cost = new ConstructionCost(0, 10, 2, 30);
		t.burnable = false;
		t.group = "human";
		t.book_slots = 5;
		t.setShadow(0.56f, 0.41f, 0.43f);
		clone("market_human", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.3f;
		t.draw_light_area_offset_y = 3f;
		t.priority = 26;
		t.type = "type_market";
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.cost = new ConstructionCost(10, 5, 2, 100);
		t.burnable = true;
		t.group = "human";
		t.setShadow(0.56f, 0.41f, 0.43f);
		clone("windmill_human_0", "$windmill_0$");
		t.group = "human";
		t.main_path = "buildings/civ_main/human/";
		t.upgrade_to = "windmill_human_1";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.civ_kingdom = "human";
		clone("windmill_human_1", "$windmill_1$");
		t.group = "human";
		t.main_path = "buildings/civ_main/human/";
		t.upgraded_from = "windmill_human_0";
		t.civ_kingdom = "human";
		clone("tent_human", "$building_civ_human$");
		t.type = "type_house";
		t.cost = new ConstructionCost(1);
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.can_be_upgraded = true;
		t.setHousingSlots(3);
		t.loot_generation = 1;
		t.housing_happiness = 5;
		t.burnable = true;
		t.upgrade_to = "house_human_0";
		t.base_stats["health"] = 50f;
		t.build_place_batch = true;
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingGeneric";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingGeneric";
		clone("house_human_0", "$building_civ_human$");
		t.draw_light_area = true;
		t.draw_light_size = 0.2f;
		t.type = "type_house";
		t.cost = new ConstructionCost(5);
		t.setHousingSlots(3);
		t.loot_generation = 1;
		t.housing_happiness = 6;
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.can_be_upgraded = true;
		t.burnable = true;
		t.upgrade_to = "house_human_1";
		t.upgraded_from = "tent_human";
		t.base_stats["health"] = 100f;
		t.has_sprite_construction = false;
		t.group = "human";
		t.sound_hit = "event:/SFX/HIT/HitWood";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
		clone("house_human_1", "house_human_0");
		t.cost = new ConstructionCost(4);
		t.setHousingSlots(4);
		t.loot_generation = 2;
		t.housing_happiness = 7;
		t.upgrade_level = 1;
		t.upgrade_to = "house_human_2";
		t.upgraded_from = "house_human_0";
		t.base_stats["health"] = 150f;
		t.group = "human";
		t.sound_hit = "event:/SFX/HIT/HitWood";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
		clone("house_human_2", "house_human_1");
		t.cost = new ConstructionCost(0, 5);
		t.upgrade_level = 2;
		t.loot_generation = 3;
		t.burnable = false;
		t.upgrade_to = "house_human_3";
		t.upgraded_from = "house_human_1";
		t.base_stats["health"] = 200f;
		t.group = "human";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("house_human_3", "house_human_2");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.cost = new ConstructionCost(0, 10);
		t.setHousingSlots(5);
		t.loot_generation = 4;
		t.housing_happiness = 9;
		t.upgrade_level = 3;
		t.upgrade_to = "house_human_4";
		t.upgraded_from = "house_human_2";
		t.base_stats["health"] = 250f;
		t.group = "human";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("house_human_4", "house_human_3");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.cost = new ConstructionCost(0, 15);
		t.setHousingSlots(6);
		t.loot_generation = 5;
		t.housing_happiness = 10;
		t.upgrade_level = 4;
		t.upgrade_to = "house_human_5";
		t.upgraded_from = "house_human_3";
		t.base_stats["health"] = 350f;
		t.group = "human";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("house_human_5", "house_human_4");
		t.cost = new ConstructionCost(0, 20, 2, 10);
		t.setHousingSlots(7);
		t.loot_generation = 6;
		t.housing_happiness = 11;
		t.upgrade_level = 5;
		t.can_be_upgraded = false;
		t.upgrade_to = string.Empty;
		t.upgraded_from = "house_human_4";
		t.base_stats["health"] = 400f;
		t.group = "human";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		clone("hall_human_0", "house_human_0");
		t.sound_hit = "event:/SFX/HIT/HitWood";
		t.priority = 100;
		t.storage = true;
		t.type = "type_hall";
		t.cost = new ConstructionCost(10, 5);
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.can_be_upgraded = true;
		t.base_stats["health"] = 200f;
		t.burnable = true;
		t.setHousingSlots(5);
		t.housing_happiness = 10;
		t.loot_generation = 3;
		t.upgrade_to = "hall_human_1";
		t.ignore_other_buildings_for_upgrade = true;
		t.group = "human";
		t.build_place_batch = true;
		t.max_houses = 2;
		t.produce_biome_food = true;
		t.setShadow(0.56f, 0.41f, 0.43f);
		t.draw_light_size = 0.3f;
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingWood";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingWood";
		t.book_slots = 3;
		t.has_sprite_construction = true;
		clone("hall_human_1", "hall_human_0");
		t.cost = new ConstructionCost(0, 10, 1, 20);
		t.setHousingSlots(8);
		t.loot_generation = 5;
		t.housing_happiness = 15;
		t.upgrade_level = 1;
		t.burnable = false;
		t.upgrade_to = "hall_human_2";
		t.upgraded_from = "hall_human_0";
		t.base_stats["health"] = 400f;
		t.group = "human";
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.draw_light_size = 0.4f;
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
		t.has_sprite_construction = false;
		clone("hall_human_2", "hall_human_1");
		t.cost = new ConstructionCost(0, 15, 1, 100);
		t.setHousingSlots(12);
		t.loot_generation = 10;
		t.housing_happiness = 20;
		t.upgrade_level = 2;
		t.can_be_upgraded = false;
		t.upgraded_from = "hall_human_1";
		t.upgrade_to = string.Empty;
		t.base_stats["health"] = 600f;
		t.group = "human";
		t.draw_light_size = 0.5f;
		t.sound_built = "event:/SFX/BUILDINGS/SpawnBuildingStone";
		t.sound_destroyed = "event:/SFX/BUILDINGS/DestroyBuildingStone";
	}

	private void addOrcs()
	{
		clone("$building_civ_orc$", "$city_colored_building$");
		t.main_path = "buildings/civ_main/orc/";
		t.group = "orc";
		t.civ_kingdom = "orc";
		clone("watch_tower_orc", "watch_tower_human");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("fishing_docks_orc", "fishing_docks_human");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.upgrade_to = "docks_orc";
		t.civ_kingdom = "orc";
		clone("docks_orc", "docks_human");
		t.main_path = "buildings/civ_main/orc/";
		t.group = "orc";
		t.draw_light_area_offset_y = 8f;
		t.draw_light_area_offset_x = -1f;
		t.upgraded_from = "fishing_docks_orc";
		t.civ_kingdom = "orc";
		clone("barracks_orc", "barracks_human");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("temple_orc", "temple_human");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("library_orc", "library_human");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("market_orc", "market_human");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("windmill_orc_0", "$windmill_0$");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.upgrade_to = "windmill_orc_1";
		t.civ_kingdom = "orc";
		clone("windmill_orc_1", "$windmill_1$");
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.upgraded_from = "windmill_orc_0";
		t.civ_kingdom = "orc";
		clone("tent_orc", "tent_human");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.upgrade_to = "house_orc_0";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("house_orc_0", "house_human_0");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_orc_1";
		t.upgraded_from = "tent_orc";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("house_orc_1", "house_human_1");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_orc_2";
		t.upgraded_from = "house_orc_0";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("house_orc_2", "house_human_2");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_orc_3";
		t.upgraded_from = "house_orc_1";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("house_orc_3", "house_human_3");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.upgrade_to = "house_orc_4";
		t.upgraded_from = "house_orc_2";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("house_orc_4", "house_human_4");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.upgrade_to = "house_orc_5";
		t.upgraded_from = "house_orc_3";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("house_orc_5", "house_human_5");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.upgraded_from = "house_orc_4";
		t.civ_kingdom = "orc";
		clone("hall_orc_0", "hall_human_0");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.upgrade_to = "hall_orc_1";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("hall_orc_1", "hall_human_1");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.upgrade_to = "hall_orc_2";
		t.upgraded_from = "hall_orc_0";
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.civ_kingdom = "orc";
		clone("hall_orc_2", "hall_human_2");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.group = "orc";
		t.main_path = "buildings/civ_main/orc/";
		t.upgraded_from = "hall_orc_1";
		t.civ_kingdom = "orc";
	}

	private void addElves()
	{
		clone("$building_civ_elf$", "$city_colored_building$");
		t.main_path = "buildings/civ_main/elf/";
		t.group = "elf";
		t.civ_kingdom = "elf";
		clone("watch_tower_elf", "watch_tower_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("fishing_docks_elf", "fishing_docks_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.upgrade_to = "docks_elf";
		t.civ_kingdom = "elf";
		clone("docks_elf", "docks_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.draw_light_area_offset_y = 6f;
		t.draw_light_area_offset_x = -2f;
		t.upgraded_from = "fishing_docks_elf";
		t.civ_kingdom = "elf";
		clone("barracks_elf", "barracks_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("temple_elf", "temple_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("library_elf", "library_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("market_elf", "market_human");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("windmill_elf_0", "$windmill_0$");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.upgrade_to = "windmill_elf_1";
		t.civ_kingdom = "elf";
		clone("windmill_elf_1", "$windmill_1$");
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.upgraded_from = "windmill_elf_0";
		t.civ_kingdom = "elf";
		clone("tent_elf", "tent_human");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.upgrade_to = "house_elf_0";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("house_elf_0", "house_human_0");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_elf_1";
		t.upgraded_from = "tent_human";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("house_elf_1", "house_human_1");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_elf_2";
		t.upgraded_from = "house_elf_0";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("house_elf_2", "house_human_2");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_elf_3";
		t.upgraded_from = "house_elf_1";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("house_elf_3", "house_human_3");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.upgrade_to = "house_elf_4";
		t.upgraded_from = "house_elf_2";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("house_elf_4", "house_human_4");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.upgrade_to = "house_elf_5";
		t.upgraded_from = "house_elf_3";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("house_elf_5", "house_human_5");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.upgraded_from = "house_elf_4";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("hall_elf_0", "hall_human_0");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.upgrade_to = "hall_elf_1";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("hall_elf_1", "hall_human_1");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.upgrade_to = "hall_elf_2";
		t.upgraded_from = "hall_elf_0";
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.civ_kingdom = "elf";
		clone("hall_elf_2", "hall_human_2");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.group = "elf";
		t.main_path = "buildings/civ_main/elf/";
		t.upgraded_from = "hall_elf_1";
		t.civ_kingdom = "elf";
	}

	private void addDwarves()
	{
		clone("$building_civ_dwarf$", "$city_colored_building$");
		t.main_path = "buildings/civ_main/dwarf/";
		t.group = "dwarf";
		t.civ_kingdom = "dwarf";
		clone("watch_tower_dwarf", "watch_tower_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("fishing_docks_dwarf", "fishing_docks_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.upgrade_to = "docks_dwarf";
		t.civ_kingdom = "dwarf";
		clone("docks_dwarf", "docks_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.draw_light_area_offset_y = 10f;
		t.upgraded_from = "fishing_docks_dwarf";
		t.civ_kingdom = "dwarf";
		clone("barracks_dwarf", "barracks_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("temple_dwarf", "temple_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("library_dwarf", "library_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("market_dwarf", "market_human");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("windmill_dwarf_0", "$windmill_0$");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.upgrade_to = "windmill_dwarf_1";
		t.civ_kingdom = "dwarf";
		clone("windmill_dwarf_1", "$windmill_1$");
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.upgraded_from = "windmill_dwarf_0";
		t.civ_kingdom = "dwarf";
		clone("tent_dwarf", "tent_human");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.upgrade_to = "house_dwarf_0";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("house_dwarf_0", "house_human_0");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_dwarf_1";
		t.upgraded_from = "tent_dwarf";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("house_dwarf_1", "house_human_1");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_dwarf_2";
		t.upgraded_from = "house_dwarf_0";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("house_dwarf_2", "house_human_2");
		t.fundament = new BuildingFundament(1, 1, 2, 0);
		t.upgrade_to = "house_dwarf_3";
		t.upgraded_from = "house_dwarf_1";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("house_dwarf_3", "house_human_3");
		t.fundament = new BuildingFundament(2, 2, 2, 0);
		t.upgrade_to = "house_dwarf_4";
		t.upgraded_from = "house_dwarf_2";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.setHousingSlots(6);
		t.civ_kingdom = "dwarf";
		clone("house_dwarf_4", "house_human_4");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.upgrade_to = "house_dwarf_5";
		t.upgraded_from = "house_dwarf_3";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.setHousingSlots(8);
		t.civ_kingdom = "dwarf";
		clone("house_dwarf_5", "house_human_5");
		t.fundament = new BuildingFundament(3, 3, 2, 0);
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.upgraded_from = "house_dwarf_4";
		t.setHousingSlots(10);
		t.civ_kingdom = "dwarf";
		clone("hall_dwarf_0", "hall_human_0");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.upgrade_to = "hall_dwarf_1";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("hall_dwarf_1", "hall_human_1");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.upgrade_to = "hall_dwarf_2";
		t.upgraded_from = "hall_dwarf_0";
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.civ_kingdom = "dwarf";
		clone("hall_dwarf_2", "hall_human_2");
		t.fundament = new BuildingFundament(3, 3, 4, 0);
		t.group = "dwarf";
		t.main_path = "buildings/civ_main/dwarf/";
		t.upgraded_from = "1hall_dwarf";
		t.civ_kingdom = "dwarf";
	}

	public void setGrowBiomeAround(string pID, int pMaxSteps, int pWorkers, float pStepInterval, CreepWorkerMovementType pMovementType)
	{
		t.grow_creep = true;
		t.grow_creep_type = pID;
		t.grow_creep_steps_max = pMaxSteps;
		t.grow_creep_workers = pWorkers;
		t.grow_creep_step_interval = pStepInterval;
		t.grow_creep_movement_type = pMovementType;
	}

	public override void editorDiagnostic()
	{
		foreach (BuildingAsset item in list)
		{
			if (!item.mini_civ_auto_load && typeof(SB).GetField(item.id, BindingFlags.Static | BindingFlags.Public) == null)
			{
				BaseAssetLibrary.logAssetError("BuildingLibrary: SB class does not have property", item.id);
			}
			if (!(item.type == "") && typeof(S_BuildingType).GetField(item.type, BindingFlags.Static | BindingFlags.Public) == null)
			{
				BaseAssetLibrary.logAssetError("BuildingLibrary: SB class does not have type property", item.type);
			}
		}
		base.editorDiagnostic();
	}

	public void clear()
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].buildings.Clear();
		}
	}

	public override BuildingAsset add(BuildingAsset pAsset)
	{
		BuildingAsset buildingAsset = base.add(pAsset);
		if (buildingAsset.base_stats == null)
		{
			buildingAsset.base_stats = new BaseStats();
			buildingAsset.base_stats["health"] = 100f;
			buildingAsset.base_stats["size"] = 2f;
		}
		return buildingAsset;
	}

	public string addToGameplayReport()
	{
		string text = "##### Buildings: \n\n";
		text += "\nAsset ID                           | type                             | building_type                    | health                           | size                             | city_building                    | can_be_upgraded                  | upgrade_from                     | upgrade_to\n";
		int num = 35;
		int num2 = 35;
		foreach (BuildingAsset item in list)
		{
			int num3 = 0;
			string text2 = "> " + item.id;
			string type = item.type;
			string pText = item.building_type.ToString();
			string pText2 = item.base_stats["health"].ToString();
			string pText3 = item.base_stats["size"].ToString();
			string pText4 = item.city_building.ToString();
			string pText5 = item.can_be_upgraded.ToString();
			string upgraded_from = item.upgraded_from;
			string upgrade_to = item.upgrade_to;
			string pLineInfo = text2;
			addLine(ref pLineInfo, type, num + num2 * num3++);
			addLine(ref pLineInfo, pText, num + num2 * num3++);
			addLine(ref pLineInfo, pText2, num + num2 * num3++);
			addLine(ref pLineInfo, pText3, num + num2 * num3++);
			addLine(ref pLineInfo, pText4, num + num2 * num3++);
			addLine(ref pLineInfo, pText5, num + num2 * num3++);
			addLine(ref pLineInfo, upgraded_from, num + num2 * num3++);
			addLine(ref pLineInfo, upgrade_to, num + num2 * num3++);
			pLineInfo += "\n";
			text += pLineInfo;
		}
		text += "\n## END OF BUILDINGS REPORT\n";
		text = text + Toolbox.getRepeatedString('=', 100) + "\n\n";
		return text + "\n\n";
	}

	private void addLine(ref string pLineInfo, string pText, int pSize)
	{
		pLineInfo = Toolbox.fillRight(pLineInfo, pSize);
		pLineInfo = pLineInfo + "| " + pText;
	}

	static BuildingLibrary()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		shadow_under_construction_bound = new Vector2(0f, 0.61f);
		shadow_under_construction_distortion = 0.19f;
	}
}
