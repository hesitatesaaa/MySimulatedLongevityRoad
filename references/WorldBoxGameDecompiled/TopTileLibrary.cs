using System;
using UnityEngine;

public class TopTileLibrary : TileLibraryMain<TopTileType>
{
	public static TopTileType snow_sand;

	public static TopTileType permafrost_low;

	public static TopTileType permafrost_high;

	public static TopTileType frozen_low;

	public static TopTileType frozen_high;

	public static TopTileType snow_hills;

	public static TopTileType snow_block;

	public static TopTileType snow_summit;

	public static TopTileType ice;

	public static TopTileType landmine;

	public static TopTileType water_bomb;

	public static TopTileType tnt_timed;

	public static TopTileType tnt;

	public static TopTileType fireworks;

	public static TopTileType road;

	public static TopTileType field;

	public static TopTileType fuse;

	public static TopTileType wall_evil;

	public static TopTileType wall_order;

	public static TopTileType wall_ancient;

	public static TopTileType wall_wild;

	public static TopTileType wall_green;

	public static TopTileType wall_iron;

	public static TopTileType wall_light;

	public static TopTileType tumor_low;

	public static TopTileType tumor_high;

	public static TopTileType biomass_low;

	public static TopTileType biomass_high;

	public static TopTileType pumpkin_low;

	public static TopTileType pumpkin_high;

	public static TopTileType cybertile_low;

	public static TopTileType cybertile_high;

	public static TopTileType grass_low;

	public static TopTileType grass_high;

	public static TopTileType corruption_low;

	public static TopTileType corruption_high;

	public static TopTileType enchanted_low;

	public static TopTileType enchanted_high;

	public static TopTileType mushroom_low;

	public static TopTileType mushroom_high;

	public static TopTileType savanna_low;

	public static TopTileType savanna_high;

	public static TopTileType jungle_low;

	public static TopTileType jungle_high;

	public static TopTileType infernal_low;

	public static TopTileType infernal_high;

	public static TopTileType swamp_low;

	public static TopTileType swamp_high;

	public static TopTileType wasteland_low;

	public static TopTileType wasteland_high;

	public static TopTileType desert_low;

	public static TopTileType desert_high;

	public static TopTileType lemon_low;

	public static TopTileType lemon_high;

	public static TopTileType candy_low;

	public static TopTileType candy_high;

	public static TopTileType crystal_low;

	public static TopTileType crystal_high;

	public static TopTileType birch_low;

	public static TopTileType birch_high;

	public static TopTileType maple_low;

	public static TopTileType maple_high;

	public static TopTileType rocklands_low;

	public static TopTileType rocklands_high;

	public static TopTileType garlic_low;

	public static TopTileType garlic_high;

	public static TopTileType flower_low;

	public static TopTileType flower_high;

	public static TopTileType celestial_low;

	public static TopTileType celestial_high;

	public static TopTileType singularity_low;

	public static TopTileType singularity_high;

	public static TopTileType clover_low;

	public static TopTileType clover_high;

	public static TopTileType paradox_low;

	public static TopTileType paradox_high;

	private const string TEMPLATE_WALL = "$wall$";

	public override void init()
	{
		base.init();
		grass_low = add(new TopTileType
		{
			drawPixel = true,
			biome_build_check = true,
			id = "grass_low",
			color_hex = "#7EAF46",
			height_min = 108,
			grass = true,
			nutrition = 30,
			ground = true,
			can_be_farm = true,
			can_build_on = true,
			can_be_set_on_fire = true,
			burnable = true,
			burn_rate = 5,
			strength = 0,
			fire_chance = 0.05f,
			remove_on_freeze = true,
			remove_on_heat = true,
			can_be_biome = true,
			food_resource = "herbs"
		});
		t.can_be_removed_with_spade = true;
		t.setBiome("biome_grass");
		t.setDrawLayer(TileZIndexes.grass_low);
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(default(BiomeTag));
		grass_high = add(new TopTileType
		{
			cost = 120,
			biome_build_check = true,
			drawPixel = true,
			id = "grass_high",
			color_hex = "#5F833C",
			height_min = 128,
			grass = true,
			nutrition = 30,
			can_be_set_on_fire = true,
			burnable = true,
			burn_rate = 5,
			additional_height = new int[8] { 15, 16, 17, 14, 13, 12, 11, 10 },
			ground = true,
			can_build_on = true,
			can_be_farm = true,
			fire_chance = 0.06f,
			strength = 0,
			remove_on_freeze = true,
			remove_on_heat = true,
			can_be_biome = true,
			food_resource = "herbs"
		});
		t.can_be_removed_with_spade = true;
		t.setBiome("biome_grass");
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.grass_high);
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(default(BiomeTag));
		savanna_low = clone("savanna_low", "grass_low");
		t.color_hex = "#F0B121";
		t.setBiome("biome_savanna");
		t.setDrawLayer(TileZIndexes.savanna_low);
		t.fire_chance = 0.06f;
		t.burn_rate = 5;
		t.food_resource = "wheat";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Savanna);
		savanna_high = clone("savanna_high", "grass_high");
		t.color_hex = "#CF931B";
		t.setBiome("biome_savanna");
		t.setDrawLayer(TileZIndexes.savanna_high);
		t.fire_chance = 0.06f;
		t.burn_rate = 5;
		t.food_resource = "wheat";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Savanna);
		enchanted_low = clone("enchanted_low", "grass_low");
		t.color_hex = "#8CDC6A";
		t.setBiome("biome_enchanted");
		t.setDrawLayer(TileZIndexes.enchanted_low);
		t.step_action = ActionLibrary.giveEnchanted;
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Enchanted);
		enchanted_high = clone("enchanted_high", "grass_low");
		t.color_hex = "#76B153";
		t.setBiome("biome_enchanted");
		t.setDrawLayer(TileZIndexes.enchanted_high);
		t.step_action = ActionLibrary.giveEnchanted;
		t.fire_chance = 0.04f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Enchanted);
		mushroom_low = clone("mushroom_low", "grass_low");
		t.color_hex = "#677642";
		t.setBiome("biome_mushroom");
		t.setDrawLayer(TileZIndexes.mushroom_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "mushrooms";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Mushroom);
		mushroom_high = clone("mushroom_high", "grass_high");
		t.color_hex = "#556338";
		t.setBiome("biome_mushroom");
		t.setDrawLayer(TileZIndexes.mushroom_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "mushrooms";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Mushroom);
		birch_low = clone("birch_low", "grass_low");
		t.color_hex = "#93CF3A";
		t.setBiome("biome_birch");
		t.setDrawLayer(TileZIndexes.birch_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Birch);
		birch_high = clone("birch_high", "grass_high");
		t.color_hex = "#76AC2B";
		t.setBiome("biome_birch");
		t.setDrawLayer(TileZIndexes.birch_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Birch);
		maple_low = clone("maple_low", "grass_low");
		t.color_hex = "#B57F22";
		t.setBiome("biome_maple");
		t.setDrawLayer(TileZIndexes.maple_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Maple);
		maple_high = clone("maple_high", "grass_high");
		t.color_hex = "#A75924";
		t.setBiome("biome_maple");
		t.setDrawLayer(TileZIndexes.maple_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Maple);
		rocklands_low = clone("rocklands_low", "grass_low");
		t.color_hex = "#8D8D8D";
		t.setBiome("biome_rocklands");
		t.setDrawLayer(TileZIndexes.rocklands_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 3;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Rocklands);
		rocklands_high = clone("rocklands_high", "grass_high");
		t.color_hex = "#7A7A7A";
		t.setBiome("biome_rocklands");
		t.setDrawLayer(TileZIndexes.rocklands_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 3;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Rocklands);
		garlic_low = clone("garlic_low", "grass_low");
		t.color_hex = "#ACA979";
		t.setBiome("biome_garlic");
		t.setDrawLayer(TileZIndexes.garlic_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 5;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Garlic);
		garlic_high = clone("garlic_high", "grass_high");
		t.color_hex = "#8C8E65";
		t.setBiome("biome_garlic");
		t.setDrawLayer(TileZIndexes.garlic_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 5;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Garlic);
		flower_low = clone("flower_low", "grass_low");
		t.color_hex = "#54CC3A";
		t.setBiome("biome_flower");
		t.setDrawLayer(TileZIndexes.flower_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 5;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Flower);
		flower_high = clone("flower_high", "grass_high");
		t.color_hex = "#39A334";
		t.setBiome("biome_flower");
		t.setDrawLayer(TileZIndexes.flower_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Flower);
		celestial_low = clone("celestial_low", "grass_low");
		t.color_hex = "#B573DA";
		t.setBiome("biome_celestial");
		t.setDrawLayer(TileZIndexes.celestial_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 2;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Celestial);
		celestial_high = clone("celestial_high", "grass_high");
		t.color_hex = "#7F5DA2";
		t.setBiome("biome_celestial");
		t.setDrawLayer(TileZIndexes.celestial_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 2;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Celestial);
		singularity_low = clone("singularity_low", "grass_low");
		t.used_in_generator = false;
		t.color_hex = "#623079";
		t.setBiome("biome_singularity");
		t.setDrawLayer(TileZIndexes.singularity_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 1;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Singularity);
		t.burnable = false;
		t.step_action_chance = 0.1f;
		t.step_action = ActionLibrary.singularityTeleportation;
		singularity_high = clone("singularity_high", "grass_high");
		t.used_in_generator = false;
		t.color_hex = "#502263";
		t.setBiome("biome_singularity");
		t.setDrawLayer(TileZIndexes.singularity_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 1;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Singularity);
		t.burnable = false;
		t.step_action_chance = 0.1f;
		t.step_action = ActionLibrary.singularityTeleportation;
		clover_low = clone("clover_low", "grass_low");
		t.color_hex = "#49AB87";
		t.setBiome("biome_clover");
		t.setDrawLayer(TileZIndexes.clover_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Clover);
		clover_high = clone("clover_high", "grass_high");
		t.color_hex = "#3E8C7B";
		t.setBiome("biome_clover");
		t.setDrawLayer(TileZIndexes.clover_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 4;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Clover);
		paradox_low = clone("paradox_low", "grass_low");
		t.used_in_generator = false;
		t.color_hex = "#896955";
		t.setBiome("biome_paradox");
		t.setDrawLayer(TileZIndexes.paradox_low);
		t.fire_chance = 0.03f;
		t.burn_rate = 2;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Paradox);
		t.step_action = ActionLibrary.timeParadox;
		t.step_action_chance = 0.1f;
		paradox_high = clone("paradox_high", "grass_high");
		t.used_in_generator = false;
		t.color_hex = "#745844";
		t.setBiome("biome_paradox");
		t.setDrawLayer(TileZIndexes.paradox_high);
		t.fire_chance = 0.03f;
		t.burn_rate = 2;
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Paradox);
		t.step_action = ActionLibrary.timeParadox;
		t.step_action_chance = 0.1f;
		corruption_low = clone("corrupted_low", "grass_low");
		t.color_hex = "#6F556C";
		t.setBiome("biome_corrupted");
		t.setDrawLayer(TileZIndexes.corruption_low);
		t.unit_death_action = ActionLibrary.spawnGhost;
		t.step_action = ActionLibrary.giveCursed;
		t.step_action_chance = 0.05f;
		t.fire_chance = 0.02f;
		t.burn_rate = 1;
		t.food_resource = "evil_beets";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Corrupted);
		t.only_allowed_to_build_with_tag = "can_build_in_biome_corruption";
		corruption_high = clone("corrupted_high", "grass_high");
		t.color_hex = "#533F51";
		t.setBiome("biome_corrupted");
		t.setDrawLayer(TileZIndexes.corruption_high);
		t.unit_death_action = ActionLibrary.spawnGhost;
		t.step_action = ActionLibrary.giveCursed;
		t.step_action_chance = 0.05f;
		t.fire_chance = 0.02f;
		t.burn_rate = 1;
		t.food_resource = "evil_beets";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Corrupted);
		t.only_allowed_to_build_with_tag = "can_build_in_biome_corruption";
		infernal_low = clone("infernal_low", "grass_low");
		t.can_be_set_on_fire_by_burning_feet = false;
		t.color_hex = "#9C3626";
		t.setBiome("biome_infernal");
		t.burnable = false;
		t.setDrawLayer(TileZIndexes.infernal_low);
		t.food_resource = "peppers";
		t.hold_lava = true;
		t.can_be_frozen = false;
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Infernal);
		t.only_allowed_to_build_with_tag = "can_build_in_biome_infernal";
		infernal_high = clone("infernal_high", "grass_high");
		t.can_be_set_on_fire_by_burning_feet = false;
		t.color_hex = "#68372D";
		t.setBiome("biome_infernal");
		t.burnable = false;
		t.setDrawLayer(TileZIndexes.infernal_high);
		t.food_resource = "peppers";
		t.hold_lava = true;
		t.can_be_frozen = false;
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Infernal);
		t.only_allowed_to_build_with_tag = "can_build_in_biome_infernal";
		jungle_low = clone("jungle_low", "grass_low");
		t.color_hex = "#46A052";
		t.setBiome("biome_jungle");
		t.setDrawLayer(TileZIndexes.jungle_low);
		t.fire_chance = 0.04f;
		t.burn_rate = 5;
		t.food_resource = "bananas";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Jungle);
		jungle_high = clone("jungle_high", "grass_high");
		t.color_hex = "#1F7020";
		t.setBiome("biome_jungle");
		t.setDrawLayer(TileZIndexes.jungle_high);
		t.fire_chance = 0.05f;
		t.burn_rate = 5;
		t.food_resource = "bananas";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Jungle);
		swamp_low = clone("swamp_low", "grass_low");
		t.color_hex = "#4D483E";
		t.setBiome("biome_swamp");
		t.setDrawLayer(TileZIndexes.swamp_low);
		t.walk_multiplier = 0.6f;
		t.food_resource = "herbs";
		t.burnable = false;
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Swamp);
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_swamp";
		t.only_allowed_to_build_with_tag = "can_build_in_biome_swamp";
		swamp_high = clone("swamp_high", "grass_high");
		t.color_hex = "#453E34";
		t.setBiome("biome_swamp");
		t.setDrawLayer(TileZIndexes.swamp_high);
		t.walk_multiplier = 0.6f;
		t.food_resource = "herbs";
		t.burnable = false;
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Swamp);
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_swamp";
		t.only_allowed_to_build_with_tag = "can_build_in_biome_swamp";
		wasteland_low = clone("wasteland_low", "grass_low");
		t.color_hex = "#849371";
		t.setBiome("biome_wasteland");
		t.grass = false;
		t.wasteland = true;
		t.burnable = false;
		t.can_be_biome = true;
		t.setDrawLayer(TileZIndexes.wasteland_low);
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Wasteland);
		t.only_allowed_to_build_with_tag = "can_build_in_biome_wasteland";
		wasteland_high = clone("wasteland_high", "grass_high");
		t.color_hex = "#6C7759";
		t.setBiome("biome_wasteland");
		t.grass = false;
		t.wasteland = true;
		t.burnable = false;
		t.can_be_biome = true;
		t.setDrawLayer(TileZIndexes.wasteland_high);
		t.food_resource = "herbs";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Wasteland);
		t.only_allowed_to_build_with_tag = "can_build_in_biome_wasteland";
		desert_low = clone("desert_low", "grass_low");
		t.color_hex = "#E8C76E";
		t.setBiome("biome_desert");
		t.grass = false;
		t.burnable = false;
		t.setDrawLayer(TileZIndexes.desert_low);
		t.walk_multiplier = 0.7f;
		t.food_resource = "desert_berries";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Desert);
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_sand";
		t.only_allowed_to_build_with_tag = "can_build_in_biome_desert";
		t.step_action = ActionLibrary.restoreMana;
		desert_high = clone("desert_high", "grass_high");
		t.color_hex = "#E1BA5A";
		t.setBiome("biome_desert");
		t.grass = false;
		t.burnable = false;
		t.setDrawLayer(TileZIndexes.desert_high);
		t.walk_multiplier = 0.7f;
		t.food_resource = "desert_berries";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Desert);
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_sand";
		t.only_allowed_to_build_with_tag = "can_build_in_biome_desert";
		t.step_action = ActionLibrary.restoreMana;
		crystal_low = clone("crystal_low", "grass_low");
		t.color_hex = "#68EADE";
		t.setBiome("biome_crystal");
		t.grass = false;
		t.burnable = false;
		t.setDrawLayer(TileZIndexes.crystal_low);
		t.food_resource = "crystal_salt";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Crystal);
		crystal_high = clone("crystal_high", "grass_high");
		t.color_hex = "#5FD6CB";
		t.setBiome("biome_crystal");
		t.grass = false;
		t.burnable = false;
		t.setDrawLayer(TileZIndexes.crystal_high);
		t.food_resource = "crystal_salt";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Crystal);
		candy_low = clone("candy_low", "grass_low");
		t.color_hex = "#FF96B0";
		t.setBiome("biome_candy");
		t.setDrawLayer(TileZIndexes.candy_low);
		t.food_resource = "candy";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Candy);
		t.burn_rate = 4;
		candy_high = clone("candy_high", "grass_high");
		t.color_hex = "#FB87A4";
		t.setBiome("biome_candy");
		t.setDrawLayer(TileZIndexes.candy_high);
		t.food_resource = "candy";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Candy);
		t.burn_rate = 4;
		lemon_low = clone("lemon_low", "grass_low");
		t.color_hex = "#D1E771";
		t.setBiome("biome_lemon");
		t.setDrawLayer(TileZIndexes.lemon_low);
		t.food_resource = "lemons";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Lemon, BiomeTag.Green);
		t.burn_rate = 2;
		t.step_action = ActionLibrary.restoreStamina;
		lemon_high = clone("lemon_high", "grass_high");
		t.color_hex = "#8ACF55";
		t.setBiome("biome_lemon");
		t.setDrawLayer(TileZIndexes.lemon_high);
		t.food_resource = "lemons";
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Lemon, BiomeTag.Green);
		t.burn_rate = 2;
		t.step_action = ActionLibrary.restoreStamina;
		water_bomb = add(new TopTileType
		{
			cost = 10,
			drawPixel = true,
			id = "water_bomb",
			color_hex = "#6D00CD",
			burnable = false,
			explodable = true,
			explodable_delayed = true,
			explode_range = 9,
			explodable_by_ocean = true,
			ignore_ocean_edge_rendering = true,
			ground = true,
			can_be_filled_with_ocean = true,
			strength = 0,
			can_build_on = true
		});
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.water_bomb);
		t.can_be_removed_with_demolish = true;
		tumor_low = add(new TopTileType
		{
			drawPixel = true,
			id = "tumor_low",
			creep = true,
			color_hex = "#EE5183",
			height_min = 108,
			ground = true,
			walk_multiplier = 0.8f,
			burnable = true,
			life = true,
			fire_chance = 1f,
			strength = 0,
			can_build_on = false,
			remove_on_freeze = true
		});
		t.can_be_frozen = false;
		t.step_action = TileActionLibrary.giveTumorTrait;
		t.setDrawLayer(TileZIndexes.tumor_low);
		t.setBiome("biome_tumor");
		tumor_high = clone("tumor_high", "tumor_low");
		t.can_be_frozen = false;
		t.color_hex = "#FE1864";
		t.setDrawLayer(TileZIndexes.tumor_high);
		t.setBiome("biome_tumor");
		biomass_low = clone("biomass_low", "tumor_low");
		t.can_be_frozen = false;
		t.color_hex = "#45C842";
		t.step_action = TileActionLibrary.giveSlownessStatus;
		TopTileType topTileType = t;
		topTileType.step_action = (TileStepAction)Delegate.Combine(topTileType.step_action, new TileStepAction(TileActionLibrary.giveMadnessTrait));
		t.setDrawLayer(TileZIndexes.biomass_low);
		t.setBiome("biome_biomass");
		t.fire_chance = 0.06f;
		biomass_high = clone("biomass_high", "tumor_high");
		t.can_be_frozen = false;
		t.color_hex = "#41A840";
		t.step_action = TileActionLibrary.giveSlownessStatus;
		TopTileType topTileType2 = t;
		topTileType2.step_action = (TileStepAction)Delegate.Combine(topTileType2.step_action, new TileStepAction(TileActionLibrary.giveMadnessTrait));
		t.setDrawLayer(TileZIndexes.biomass_high);
		t.setBiome("biome_biomass");
		t.fire_chance = 0.06f;
		pumpkin_low = clone("pumpkin_low", "tumor_low");
		t.can_be_frozen = false;
		t.color_hex = "#8F9339";
		t.step_action = TileActionLibrary.giveSlownessStatus;
		t.step_action_chance = 0.2f;
		t.setDrawLayer(TileZIndexes.pumpkin_low);
		t.setBiome("biome_pumpkin");
		t.fire_chance = 0.06f;
		pumpkin_high = clone("pumpkin_high", "tumor_high");
		t.can_be_frozen = false;
		t.color_hex = "#696C02";
		t.step_action = TileActionLibrary.giveSlownessStatus;
		t.step_action_chance = 0.2f;
		t.setDrawLayer(TileZIndexes.pumpkin_high);
		t.setBiome("biome_pumpkin");
		t.fire_chance = 0.06f;
		cybertile_low = clone("cybertile_low", "tumor_low");
		t.can_be_frozen = false;
		t.life = false;
		t.color_hex = "#9EA6A3";
		t.setDrawLayer(TileZIndexes.cybertile_low);
		t.setBiome("biome_cybertile");
		t.burnable = false;
		t.can_be_removed_with_demolish = true;
		t.step_action = null;
		cybertile_high = clone("cybertile_high", "tumor_high");
		t.can_be_frozen = false;
		t.life = false;
		t.color_hex = "#858886";
		t.setDrawLayer(TileZIndexes.cybertile_high);
		t.setBiome("biome_cybertile");
		t.burnable = false;
		t.can_be_removed_with_demolish = true;
		t.step_action = null;
		road = add(new TopTileType
		{
			cost = 116,
			drawPixel = true,
			id = "road",
			color_hex = "#C1997C",
			road = true,
			ground = true,
			walk_multiplier = 1.5f,
			can_be_set_on_fire = true,
			can_build_on = true,
			can_be_frozen = false,
			strength = 0,
			check_edge = false
		});
		t.setDrawLayer(TileZIndexes.road);
		t.can_be_removed_with_demolish = true;
		fuse = add(new TopTileType
		{
			cost = 10,
			drawPixel = true,
			id = "fuse",
			color_hex = "#834C4C",
			burnable = true,
			burn_rate = 4,
			terraform_after_fire = true,
			ground = true,
			can_build_on = true,
			check_edge = false,
			strength = 0
		});
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.fuse);
		t.can_be_removed_with_demolish = true;
		field = add(new TopTileType
		{
			cost = 115,
			drawPixel = true,
			id = "field",
			color_hex = "#A8663A",
			height_min = 108,
			ground = true,
			farm_field = true,
			can_be_frozen = false,
			can_be_removed_with_demolish = true,
			can_be_removed_with_spade = true,
			can_build_on = false,
			can_be_set_on_fire = true,
			burnable = true,
			burn_rate = 4,
			strength = 0,
			fire_chance = 0.4f,
			remove_on_freeze = true,
			check_edge = false,
			remove_on_heat = true
		});
		t.setDrawLayer(TileZIndexes.field);
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Field);
		tnt = add(new TopTileType
		{
			cost = 10,
			drawPixel = true,
			id = "tnt",
			color_hex = "#A30000",
			burnable = true,
			explodable = true,
			explodable_delayed = true,
			explode_range = 10,
			ground = true,
			can_build_on = true,
			strength = 0,
			can_errode_to_sand = false
		});
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.tnt);
		t.can_be_removed_with_demolish = true;
		fireworks = add(new TopTileType
		{
			cost = 10,
			drawPixel = true,
			id = "fireworks",
			color_hex = "#B43DCC",
			burnable = true,
			burn_rate = 5,
			terraform_after_fire = true,
			ground = true,
			can_build_on = true,
			strength = 0,
			can_errode_to_sand = false
		});
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.fireworks);
		t.can_be_removed_with_demolish = true;
		tnt_timed = add(new TopTileType
		{
			cost = 10,
			drawPixel = true,
			id = "tnt_timed",
			color_hex = "#7F0000",
			burnable = true,
			explodable = true,
			explodable_timed = true,
			explodeTimer = 5,
			explode_range = 8,
			ground = true,
			can_build_on = true,
			strength = 0,
			can_errode_to_sand = false
		});
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.tnt_timed);
		t.can_be_removed_with_demolish = true;
		landmine = add(new TopTileType
		{
			cost = 10,
			drawPixel = true,
			id = "landmine",
			color_hex = "#990000",
			burnable = true,
			explodable = true,
			explode_range = 3,
			ground = true,
			strength = 0,
			can_errode_to_sand = false
		});
		t.can_be_frozen = false;
		t.step_action = TileActionLibrary.landmine;
		t.step_action_chance = 0.9f;
		t.setDrawLayer(TileZIndexes.landmine);
		t.can_be_removed_with_demolish = true;
		frozen_low = add(new TopTileType
		{
			id = "frozen_low",
			cost = 10,
			drawPixel = true,
			color_hex = "#BAD5D3",
			edge_color_hex = "#F5F7F6",
			ground = true,
			strength = 0,
			burnable = false,
			can_be_removed_with_spade = true,
			can_be_autotested = false
		});
		t.walk_multiplier = 0.8f;
		t.can_be_farm = true;
		t.can_build_on = true;
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		t.setDrawLayer(TileZIndexes.permafrost_low);
		setSnow();
		frozen_high = clone("frozen_high", "frozen_low");
		t.walk_multiplier = 0.8f;
		t.can_build_on = true;
		t.color_hex = "#D3E4E3";
		t.edge_color_hex = "#F5F7F6";
		t.can_be_autotested = false;
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		t.setDrawLayer(TileZIndexes.permafrost_high);
		setSnow();
		permafrost_low = clone("permafrost_low", "grass_low");
		t.walk_multiplier = 0.8f;
		t.setBiome("biome_permafrost");
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Permafrost);
		t.color_hex = "#99BCDB";
		t.setDrawLayer(TileZIndexes.permafrost_low);
		t.food_resource = string.Empty;
		t.can_be_frozen = false;
		t.forever_frozen = true;
		t.freeze_to_id = string.Empty;
		t.burnable = false;
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		t.only_allowed_to_build_with_tag = "can_build_in_biome_permafrost";
		permafrost_high = clone("permafrost_high", "grass_high");
		t.walk_multiplier = 0.8f;
		t.setBiome("biome_permafrost");
		t.biome_tags = AssetLibrary<TopTileType>.h<BiomeTag>(BiomeTag.Permafrost);
		t.food_resource = "pine_cones";
		t.color_hex = "#B4CFE5";
		t.edge_color_hex = "#F5F7F6";
		t.forever_frozen = true;
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.permafrost_high);
		t.freeze_to_id = string.Empty;
		t.burnable = false;
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		t.only_allowed_to_build_with_tag = "can_build_in_biome_permafrost";
		snow_sand = clone("snow_sand", "permafrost_low");
		t.walk_multiplier = 0.6f;
		t.can_build_on = true;
		t.setDrawLayer(TileZIndexes.snow_sand);
		t.color_hex = "#AFF5F1";
		t.ground = true;
		t.can_be_autotested = false;
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		setSnow();
		ice = clone("ice", "permafrost_low");
		t.setDrawLayer(TileZIndexes.ice);
		t.color_hex = "#A7D6F4";
		t.can_build_on = false;
		t.can_errode_to_sand = false;
		t.damaged_when_walked = true;
		t.layer_type = TileLayerType.Ocean;
		t.ground = false;
		t.liquid = true;
		t.ocean = true;
		t.can_be_autotested = false;
		setSnow();
		snow_hills = clone("snow_hills", "permafrost_low");
		t.walk_multiplier = 0.6f;
		t.color_hex = "#E2EDEC";
		t.setDrawLayer(TileZIndexes.snow_hills);
		t.can_build_on = false;
		t.can_be_farm = false;
		t.can_be_autotested = false;
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		setSnow();
		snow_block = clone("snow_block", "permafrost_low");
		t.walk_multiplier = 0.6f;
		t.layer_type = TileLayerType.Block;
		t.color_hex = "#FCFDFD";
		t.block = true;
		t.block_height = 3f;
		t.rocks = true;
		t.edge_mountains = true;
		t.mountains = true;
		t.ground = false;
		t.can_build_on = false;
		t.can_be_autotested = false;
		t.setDrawLayer(TileZIndexes.snow_block);
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_snow";
		setSnow();
		snow_summit = clone("snow_summit", "permafrost_low");
		t.layer_type = TileLayerType.Block;
		t.color_hex = "#FCFDFD";
		t.block = true;
		t.block_height = 4f;
		t.rocks = true;
		t.edge_mountains = true;
		t.mountains = true;
		t.summit = true;
		t.ground = false;
		t.can_build_on = false;
		t.can_be_autotested = false;
		t.setDrawLayer(TileZIndexes.snow_summit);
		setSnow();
		addWalls();
		loadTileSprites();
	}

	private void addWalls()
	{
		add(new TopTileType
		{
			id = "$wall$",
			layer_type = TileLayerType.Block,
			cost = 10,
			drawPixel = true,
			color_hex = "#ffffff",
			edge_color_hex = "#5A1C32",
			wall = true,
			block = true,
			block_height = 6f,
			can_be_set_on_fire = false,
			strength = 0,
			burnable = false,
			can_be_removed_with_demolish = true,
			allowed_to_be_finger_copied = false,
			can_be_frozen = false
		});
		wall_evil = clone("wall_evil", "$wall$");
		t.strength = 3;
		t.color_hex = "#9B3C4D";
		t.setDrawLayer(TileZIndexes.wall_evil);
		t.life = true;
		wall_order = clone("wall_order", "$wall$");
		t.strength = 3;
		t.setDrawLayer(TileZIndexes.wall_order);
		t.color_hex = "#A1B1A2";
		t.edge_color_hex = "#787F87";
		t.can_be_removed_with_pickaxe = true;
		wall_ancient = clone("wall_ancient", "$wall$");
		t.strength = 4;
		t.setDrawLayer(TileZIndexes.wall_ancient);
		t.color_hex = "#425B78";
		t.edge_color_hex = "#3A485E";
		wall_wild = clone("wall_wild", "$wall$");
		t.strength = 1;
		t.setDrawLayer(TileZIndexes.wall_wild);
		t.color_hex = "#C67D49";
		t.edge_color_hex = "#4B2E1B";
		t.can_be_removed_with_axe = true;
		t.burnable = true;
		t.burn_rate = 5;
		wall_iron = clone("wall_iron", "$wall$");
		t.strength = 2;
		t.setDrawLayer(TileZIndexes.wall_iron);
		t.color_hex = "#597D94";
		t.edge_color_hex = "#2E363E";
		wall_green = clone("wall_green", "$wall$");
		t.strength = 1;
		t.setDrawLayer(TileZIndexes.wall_green);
		t.color_hex = "#65A13F";
		t.edge_color_hex = "#3F6B3F";
		t.can_be_set_on_fire = true;
		t.burnable = true;
		t.burn_rate = 5;
		t.can_be_removed_with_axe = true;
		t.can_be_removed_with_sickle = true;
		wall_light = clone("wall_light", "$wall$");
		t.strength = 10;
		t.setDrawLayer(TileZIndexes.wall_light);
		t.color_hex = "#FFF349";
		t.edge_color_hex = "#E7DC42";
		t.animated_wall = true;
		t.block_height = 20f;
	}

	private void setSnow()
	{
		t.can_be_biome = false;
		t.setBiome(null);
		t.forever_frozen = false;
		t.can_be_frozen = false;
		t.can_be_unfrozen = true;
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (TopTileType item in list)
		{
			if (!string.IsNullOrEmpty(item.biome_id))
			{
				item.biome_asset = AssetManager.biome_library.get(item.biome_id);
			}
		}
	}

	public override TopTileType add(TopTileType pAsset)
	{
		if (!pAsset.isTemplateAsset())
		{
			pAsset.index_id = TileTypeBase.last_index_id++;
			TileLibrary.array_tiles[pAsset.index_id] = pAsset;
		}
		return base.add(pAsset);
	}

	private void loadTileSprites()
	{
		foreach (TopTileType item in list)
		{
			loadSpritesForTile(item);
		}
	}

	private void loadSpritesForTile(TopTileType pType)
	{
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList("tiles/" + pType.id);
		if (spriteList != null && spriteList.Length != 0)
		{
			pType.sprites = new TileSprites();
			Sprite[] array = spriteList;
			foreach (Sprite pSprite in array)
			{
				pType.sprites.addVariation(pSprite, pType.id);
			}
		}
	}
}
