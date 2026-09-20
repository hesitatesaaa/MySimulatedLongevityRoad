using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using UnityEngine;
using strings;

[ObfuscateLiterals]
public class PowerLibrary : AssetLibrary<GodPower>
{
	private const string TEMPLATE_EXPLOSIVE_TILES = "$template_explosive_tiles$";

	private const string TEMPLATE_BOMBS = "$template_bombs$";

	private const string TEMPLATE_DROPS = "$template_drops$";

	private const string TEMPLATE_SEEDS = "$template_seeds$";

	private const string TEMPLATE_PLANTS = "$template_plants$";

	private const string TEMPLATE_DROP_MINERALS = "$template_minerals$";

	private const string TEMPLATE_DROP_BUILDING = "$template_drop_building$";

	private const string TEMPLATE_PRINTER = "$template_printer$";

	private const string TEMPLATE_SPAWN_SPECIAL = "$template_spawn_special$";

	private const string TEMPLATE_SPAWN_ACTOR = "$template_spawn_actor$";

	private const string TEMPLATE_TERRAFORM_TILES = "$template_terraform_tiles$";

	private const string TEMPLATE_WALL = "$template_wall$";

	private const string TEMPLATE_DRAW = "$template_draw$";

	private const string TEMPLATE_ERASER = "$template_eraser$";

	public static GodPower traits_gamma_rain_edit;

	public static GodPower traits_delta_rain_edit;

	public static GodPower traits_omega_rain_edit;

	public static GodPower equipment_rain_edit;

	public static GodPower inspect_unit;

	public override void init()
	{
		base.init();
		addCivsClassic();
		addCivsAnimals();
		addMobs();
		addSpecial();
		addTerraformTiles();
		addDestruction();
		addClouds();
		addPrinters();
		addDrops();
		addWaypoints();
	}

	private void addWaypoints()
	{
		clone("desire_alien_mold", "$template_drops$");
		t.name = "Alien Mold Desire";
		t.drop_id = t.id;
		clone("desire_computer", "$template_drops$");
		t.name = "Evil Computer Desire";
		t.drop_id = t.id;
		clone("desire_golden_egg", "$template_drops$");
		t.name = "Golden Egg Desire";
		t.drop_id = t.id;
		clone("desire_harp", "$template_drops$");
		t.name = "Ethereal Harp Desire";
		t.drop_id = t.id;
		clone("waypoint_alien_mold", "$template_drop_building$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Alien Mold";
		t.drop_id = t.id;
		clone("waypoint_computer", "$template_drop_building$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Evil Computer";
		t.drop_id = t.id;
		clone("waypoint_golden_egg", "$template_drop_building$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Golden Egg";
		t.drop_id = t.id;
		clone("waypoint_harp", "$template_drop_building$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Ethereal Harp";
		t.drop_id = t.id;
	}

	private void addTerraformTiles()
	{
		add(new GodPower
		{
			id = "$template_terraform_tiles$",
			draw_lines = true,
			terraform = true,
			type = PowerActionType.PowerDrawTile,
			mouse_hold_animation = MouseHoldAnimation.Draw,
			rank = PowerRank.Rank0_free,
			show_tool_sizes = true,
			unselect_when_window = true,
			hold_action = true,
			click_interval = 0f
		});
		t.click_action = cleanBurnedTile;
		GodPower godPower = t;
		godPower.click_action = (PowerActionWithID)Delegate.Combine(godPower.click_action, new PowerActionWithID(stopFire));
		GodPower godPower2 = t;
		godPower2.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower2.click_brush_action, new PowerActionWithID(fmodDrawingSound));
		GodPower godPower3 = t;
		godPower3.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower3.click_brush_action, new PowerActionWithID(loopWithCurrentBrush));
		GodPower godPower4 = t;
		godPower4.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower4.click_brush_action, new PowerActionWithID(drawingCursorEffect));
		clone("$template_draw$", "$template_terraform_tiles$");
		GodPower godPower5 = t;
		godPower5.click_action = (PowerActionWithID)Delegate.Combine(godPower5.click_action, new PowerActionWithID(drawTiles));
		clone("$template_wall$", "$template_draw$");
		t.make_buildings_transparent = true;
		t.force_brush = "sqr_0";
		t.show_tool_sizes = false;
		t.click_action = cleanBurnedTile;
		GodPower godPower6 = t;
		godPower6.click_action = (PowerActionWithID)Delegate.Combine(godPower6.click_action, new PowerActionWithID(stopFire));
		GodPower godPower7 = t;
		godPower7.click_action = (PowerActionWithID)Delegate.Combine(godPower7.click_action, new PowerActionWithID(destroyBuildings));
		GodPower godPower8 = t;
		godPower8.click_action = (PowerActionWithID)Delegate.Combine(godPower8.click_action, new PowerActionWithID(drawLifeEraser));
		GodPower godPower9 = t;
		godPower9.click_action = (PowerActionWithID)Delegate.Combine(godPower9.click_action, new PowerActionWithID(drawTiles));
		t.sound_drawing = "event:/SFX/POWERS/Mountains";
		clone("$template_eraser$", "$template_terraform_tiles$");
		t.click_action = flashPixel;
		clone("fuse", "$template_draw$");
		t.name = "Fuse";
		t.top_tile_type = "fuse";
		GodPower godPower10 = t;
		godPower10.click_action = (PowerActionWithID)Delegate.Combine(godPower10.click_action, new PowerActionWithID(destroyBuildings));
		GodPower godPower11 = t;
		godPower11.click_action = (PowerActionWithID)Delegate.Combine(godPower11.click_action, new PowerActionWithID(flashPixel));
		t.sound_drawing = "event:/SFX/POWERS/Fuse";
		clone("tile_deep_ocean", "$template_draw$");
		t.name = "Deep Ocean";
		t.tile_type = "pit_deep_ocean";
		t.path_icon = "iconTileDeepOcean";
		t.sound_drawing = "event:/SFX/POWERS/Pit";
		GodPower godPower12 = t;
		godPower12.click_action = (PowerActionWithID)Delegate.Combine(godPower12.click_action, new PowerActionWithID(destroyBuildings));
		clone("tile_close_ocean", "$template_draw$");
		t.name = "Close Ocean";
		t.tile_type = "pit_close_ocean";
		t.path_icon = "iconTileCloseOcean";
		t.sound_drawing = "event:/SFX/POWERS/Pit";
		GodPower godPower13 = t;
		godPower13.click_action = (PowerActionWithID)Delegate.Combine(godPower13.click_action, new PowerActionWithID(destroyBuildings));
		clone("tile_shallow_waters", "$template_draw$");
		t.name = "Shallow Waters";
		t.tile_type = "pit_shallow_waters";
		t.path_icon = "iconTileShallowWater";
		t.sound_drawing = "event:/SFX/POWERS/Pit";
		GodPower godPower14 = t;
		godPower14.click_action = (PowerActionWithID)Delegate.Combine(godPower14.click_action, new PowerActionWithID(destroyBuildings));
		clone("tile_sand", "$template_draw$");
		t.name = "Sand";
		t.tile_type = "sand";
		t.path_icon = "iconTileSand";
		t.sound_drawing = "event:/SFX/POWERS/Sand";
		clone("tile_soil", "$template_draw$");
		t.name = "Soil";
		t.tile_type = "soil_low";
		t.path_icon = "iconTileSoil";
		t.sound_drawing = "event:/SFX/POWERS/SoilLow";
		clone("tile_high_soil", "$template_draw$");
		t.name = "Soil High";
		t.tile_type = "soil_high";
		t.path_icon = "iconTileHighSoil";
		t.sound_drawing = "event:/SFX/POWERS/SoilHigh";
		clone("tile_hills", "$template_draw$");
		t.name = "Hills";
		t.tile_type = "hills";
		t.path_icon = "iconTileHills";
		GodPower godPower15 = t;
		godPower15.click_action = (PowerActionWithID)Delegate.Combine(godPower15.click_action, new PowerActionWithID(destroyBuildings));
		t.sound_drawing = "event:/SFX/POWERS/Hills";
		clone("tile_mountains", "$template_draw$");
		t.name = "Mountains";
		t.tile_type = "mountains";
		t.path_icon = "iconTileMountains";
		GodPower godPower16 = t;
		godPower16.click_action = (PowerActionWithID)Delegate.Combine(godPower16.click_action, new PowerActionWithID(destroyBuildings));
		t.sound_drawing = "event:/SFX/POWERS/Mountains";
		clone("tile_summit", "$template_draw$");
		t.name = "Summit";
		t.tile_type = "summit";
		t.path_icon = "iconTileSummit";
		GodPower godPower17 = t;
		godPower17.click_action = (PowerActionWithID)Delegate.Combine(godPower17.click_action, new PowerActionWithID(destroyBuildings));
		t.sound_drawing = "event:/SFX/POWERS/Mountains";
		clone("wall_order", "$template_wall$");
		t.name = "Stone Wall";
		t.top_tile_type = "wall_order";
		t.path_icon = "iconWallOrder";
		clone("wall_evil", "$template_wall$");
		t.name = "Wall of Evil";
		t.top_tile_type = "wall_evil";
		t.path_icon = "iconWallEvil";
		clone("wall_ancient", "$template_wall$");
		t.name = "Ancient Wall";
		t.top_tile_type = "wall_ancient";
		t.path_icon = "iconWallAncient";
		clone("wall_wild", "$template_wall$");
		t.name = "Wooden Wall";
		t.top_tile_type = "wall_wild";
		t.path_icon = "iconWallWild";
		clone("wall_green", "$template_wall$");
		t.name = "Green Wall";
		t.top_tile_type = "wall_green";
		t.path_icon = "iconWallGreen";
		clone("wall_iron", "$template_wall$");
		t.name = "Iron Wall";
		t.top_tile_type = "wall_iron";
		t.path_icon = "iconWallIron";
		clone("wall_light", "$template_wall$");
		t.name = "Wall of Light";
		t.top_tile_type = "wall_light";
		t.path_icon = "iconWallLight";
		clone("shovel_plus", "$template_terraform_tiles$");
		t.name = "Shovel Plus";
		t.path_icon = "iconShovelPlus";
		GodPower godPower18 = t;
		godPower18.click_action = (PowerActionWithID)Delegate.Combine(godPower18.click_action, new PowerActionWithID(drawShovelPlus));
		GodPower godPower19 = t;
		godPower19.click_action = (PowerActionWithID)Delegate.Combine(godPower19.click_action, new PowerActionWithID(destroyBuildings));
		GodPower godPower20 = t;
		godPower20.click_action = (PowerActionWithID)Delegate.Combine(godPower20.click_action, new PowerActionWithID(flashPixel));
		t.sound_drawing = "event:/SFX/POWERS/ShovelPlus";
		clone("shovel_minus", "$template_terraform_tiles$");
		t.name = "Shovel Minus";
		t.path_icon = "iconShovelMinus";
		GodPower godPower21 = t;
		godPower21.click_action = (PowerActionWithID)Delegate.Combine(godPower21.click_action, new PowerActionWithID(drawShovelMinus));
		GodPower godPower22 = t;
		godPower22.click_action = (PowerActionWithID)Delegate.Combine(godPower22.click_action, new PowerActionWithID(destroyBuildings));
		GodPower godPower23 = t;
		godPower23.click_action = (PowerActionWithID)Delegate.Combine(godPower23.click_action, new PowerActionWithID(flashPixel));
		t.sound_drawing = "event:/SFX/POWERS/ShovelMinus";
		clone("vortex", "$template_terraform_tiles$");
		t.name = "Vortex";
		t.path_icon = "iconVertex2";
		t.click_action = stopFire;
		t.sound_drawing = "event:/SFX/POWERS/Vortex";
		GodPower godPower24 = t;
		godPower24.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower24.click_brush_action, new PowerActionWithID(useVortex));
		clone("grey_goo", "$template_eraser$");
		t.name = "Grey Goo";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		GodPower godPower25 = t;
		godPower25.click_action = (PowerActionWithID)Delegate.Combine(godPower25.click_action, new PowerActionWithID(drawGreyGoo));
		GodPower godPower26 = t;
		godPower26.click_action = (PowerActionWithID)Delegate.Combine(godPower26.click_action, new PowerActionWithID(stopFire));
		t.sound_drawing = "event:/SFX/POWERS/GreyGoo";
		t.tester_enabled = false;
		clone("conway", "$template_eraser$");
		t.name = "Conway game of Life1";
		GodPower godPower27 = t;
		godPower27.click_action = (PowerActionWithID)Delegate.Combine(godPower27.click_action, new PowerActionWithID(drawConway));
		GodPower godPower28 = t;
		godPower28.click_action = (PowerActionWithID)Delegate.Combine(godPower28.click_action, new PowerActionWithID(stopFire));
		t.sound_drawing = "event:/SFX/POWERS/Conway";
		clone("conway_inverse", "$template_eraser$");
		t.name = "Conway game of Life2";
		GodPower godPower29 = t;
		godPower29.click_action = (PowerActionWithID)Delegate.Combine(godPower29.click_action, new PowerActionWithID(drawConwayInverse));
		GodPower godPower30 = t;
		godPower30.click_action = (PowerActionWithID)Delegate.Combine(godPower30.click_action, new PowerActionWithID(stopFire));
		t.sound_drawing = "event:/SFX/POWERS/Conway";
		clone("finger", "$template_eraser$");
		t.name = "Finger";
		t.path_icon = "iconTileFinger";
		GodPower godPower31 = t;
		godPower31.click_action = (PowerActionWithID)Delegate.Combine(godPower31.click_action, new PowerActionWithID(drawFinger));
		GodPower godPower32 = t;
		godPower32.click_action = (PowerActionWithID)Delegate.Combine(godPower32.click_action, new PowerActionWithID(stopFire));
		GodPower godPower33 = t;
		godPower33.click_action = (PowerActionWithID)Delegate.Combine(godPower33.click_action, new PowerActionWithID(cleanBurnedTile));
		t.sound_drawing = "event:/SFX/POWERS/Finger";
		clone("life_eraser", "$template_eraser$");
		GodPower godPower34 = t;
		godPower34.click_action = (PowerActionWithID)Delegate.Combine(godPower34.click_action, new PowerActionWithID(drawLifeEraser));
		t.name = "Life Eraser";
		t.sound_drawing = "event:/SFX/POWERS/LifeEraser";
		clone("demolish", "$template_eraser$");
		GodPower godPower35 = t;
		godPower35.click_action = (PowerActionWithID)Delegate.Combine(godPower35.click_action, new PowerActionWithID(drawDemolish));
		t.name = "Demolish";
		t.sound_drawing = "event:/SFX/POWERS/Demolish";
		clone("scissors", "$template_eraser$");
		t.path_icon = "iconScissors";
		t.force_map_mode = MetaType.City;
		GodPower godPower36 = t;
		godPower36.click_action = (PowerActionWithID)Delegate.Combine(godPower36.click_action, new PowerActionWithID(drawScissors));
		t.name = "Scissors";
		t.sound_drawing = "event:/SFX/POWERS/Demolish";
		clone("border_brush", "$template_eraser$");
		t.path_icon = "iconBorderBrush";
		t.force_map_mode = MetaType.City;
		GodPower godPower37 = t;
		godPower37.click_action = (PowerActionWithID)Delegate.Combine(godPower37.click_action, new PowerActionWithID(drawBorderBrush));
		t.name = "Border Brush";
		t.sound_drawing = "event:/SFX/POWERS/Demolish";
		clone("sponge", "$template_eraser$");
		t.path_icon = "iconSponge";
		GodPower godPower38 = t;
		godPower38.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower38.click_brush_action, new PowerActionWithID(removeClouds));
		GodPower godPower39 = t;
		godPower39.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower39.click_brush_action, new PowerActionWithID(removeTornadoes));
		GodPower godPower40 = t;
		godPower40.click_action = (PowerActionWithID)Delegate.Combine(godPower40.click_action, new PowerActionWithID(removeBuildingsBySponge));
		GodPower godPower41 = t;
		godPower41.click_action = (PowerActionWithID)Delegate.Combine(godPower41.click_action, new PowerActionWithID(removeGoo));
		GodPower godPower42 = t;
		godPower42.click_action = (PowerActionWithID)Delegate.Combine(godPower42.click_action, new PowerActionWithID(cleanBurnedTile));
		GodPower godPower43 = t;
		godPower43.click_action = (PowerActionWithID)Delegate.Combine(godPower43.click_action, new PowerActionWithID(stopFire));
		t.name = "Sponge";
		t.sound_drawing = "event:/SFX/POWERS/Sponge";
		clone("sickle", "$template_eraser$");
		t.path_icon = "iconSickle";
		GodPower godPower44 = t;
		godPower44.click_action = (PowerActionWithID)Delegate.Combine(godPower44.click_action, new PowerActionWithID(drawSickle));
		t.name = "Sickle";
		t.sound_event = "event:/SFX/POWERS/Sickle";
		t.sound_drawing = "event:/SFX/POWERS/Sickle";
		clone("spade", "$template_eraser$");
		t.path_icon = "iconSpade";
		GodPower godPower45 = t;
		godPower45.click_action = (PowerActionWithID)Delegate.Combine(godPower45.click_action, new PowerActionWithID(drawSpade));
		t.name = "Spade";
		t.sound_drawing = "event:/SFX/POWERS/Spade";
		clone("axe", "$template_eraser$");
		t.path_icon = "iconAxe";
		GodPower godPower46 = t;
		godPower46.click_action = (PowerActionWithID)Delegate.Combine(godPower46.click_action, new PowerActionWithID(drawAxe));
		t.name = "Axe";
		t.sound_drawing = "event:/SFX/POWERS/Axe";
		clone("bucket", "$template_eraser$");
		t.path_icon = "iconBucket";
		GodPower godPower47 = t;
		godPower47.click_action = (PowerActionWithID)Delegate.Combine(godPower47.click_action, new PowerActionWithID(drawBucket));
		t.name = "Bucket";
		t.sound_drawing = "event:/SFX/POWERS/Bucket";
		clone("pickaxe", "$template_eraser$");
		t.path_icon = "iconPickaxe";
		GodPower godPower48 = t;
		godPower48.click_action = (PowerActionWithID)Delegate.Combine(godPower48.click_action, new PowerActionWithID(drawPickaxe));
		t.name = "Pickaxe";
		t.sound_drawing = "event:/SFX/POWERS/Pickaxe";
		clone("divine_light", "$template_eraser$");
		t.path_icon = "iconDivineLight";
		GodPower godPower49 = t;
		godPower49.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower49.click_brush_action, new PowerActionWithID(divineLightFX));
		t.click_action = drawDivineLight;
		t.name = "Divine Light";
		t.show_tool_sizes = true;
		t.sound_drawing = "event:/SFX/POWERS/DivineLight";
	}

	private void addDrops()
	{
		add(new GodPower
		{
			id = "$template_drops$",
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true,
			falling_chance = 0.05f,
			type = PowerActionType.PowerSpawnDrops,
			mouse_hold_animation = MouseHoldAnimation.Sprinkle
		});
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower = t;
		godPower.click_power_brush_action = (PowerAction)Delegate.Combine(godPower.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		GodPower godPower2 = t;
		godPower2.click_power_action = (PowerAction)Delegate.Combine(godPower2.click_power_action, new PowerAction(fmodDrawingSound));
		t.surprises_units = false;
		clone("paint", "$template_drops$");
		t.name = "Paint";
		t.force_map_mode = MetaType.City;
		t.drop_id = t.id;
		clone("dust_white", "$template_drops$");
		t.name = "White Dust";
		t.drop_id = t.id;
		clone("dust_black", "$template_drops$");
		t.name = "Black Dust";
		t.drop_id = t.id;
		clone("dust_red", "$template_drops$");
		t.name = "Red Dust";
		t.drop_id = t.id;
		clone("dust_blue", "$template_drops$");
		t.name = "Blue Dust";
		t.drop_id = t.id;
		clone("dust_gold", "$template_drops$");
		t.name = "Gold Dust";
		t.drop_id = t.id;
		clone("dust_purple", "$template_drops$");
		t.name = "Purple Dust";
		t.drop_id = t.id;
		clone("$template_explosive_tiles$", "$template_drops$");
		t.falling_chance = 1f;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsRandom;
		GodPower godPower3 = t;
		godPower3.click_power_brush_action = (PowerAction)Delegate.Combine(godPower3.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		clone("tnt", "$template_explosive_tiles$");
		t.name = "tnt";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/Tnt";
		clone("tnt_timed", "$template_explosive_tiles$");
		t.name = "tnt_timed";
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/TntTimed";
		clone("water_bomb", "$template_explosive_tiles$");
		t.name = "Water Bomb";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/WaterBomb";
		clone("landmine", "$template_explosive_tiles$");
		t.name = "Landmine";
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/LandMine";
		clone("fireworks", "$template_explosive_tiles$");
		t.name = "Fireworks";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/Fireworks";
		clone("inspiration", "$template_drops$");
		t.force_map_mode = MetaType.City;
		t.name = "Inspiration";
		t.drop_id = t.id;
		t.path_icon = "iconInspiration";
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Inspiration";
		clone("discord", "$template_drops$");
		t.force_map_mode = MetaType.Alliance;
		t.name = "Discord";
		t.drop_id = t.id;
		t.path_icon = "iconDiscord";
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Inspiration";
		clone("friendship", "$template_drops$");
		t.force_map_mode = MetaType.Kingdom;
		t.name = "Friendship";
		t.path_icon = "iconFriendship";
		t.drop_id = t.id;
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Friendship";
		clone("spite", "$template_drops$");
		t.force_map_mode = MetaType.Kingdom;
		t.name = "Spite";
		t.path_icon = "iconSprite";
		t.drop_id = t.id;
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Spite";
		clone("madness", "$template_drops$");
		t.name = "Madness";
		t.falling_chance = 0.01f;
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/Madness";
		clone("blessing", "$template_drops$");
		t.name = "Blessing";
		t.drop_id = t.id;
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Blessing";
		clone("shield", "$template_drops$");
		t.name = "Shield";
		t.drop_id = t.id;
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Shield";
		clone("curse", "$template_drops$");
		t.name = "Curse";
		t.rank = PowerRank.Rank0_free;
		t.drop_id = t.id;
		t.falling_chance = 0.01f;
		t.sound_drawing = "event:/SFX/POWERS/Curse";
		clone("zombie_infection", "$template_drops$");
		t.name = "Zombie Infection";
		t.falling_chance = 0.01f;
		t.rank = PowerRank.Rank3_good;
		t.drop_id = t.id;
		t.requires_premium = true;
		t.sound_drawing = "event:/SFX/POWERS/ZombieInfection";
		clone("mush_spores", "$template_drops$");
		t.name = "Mush Spores";
		t.falling_chance = 0.01f;
		t.rank = PowerRank.Rank2_normal;
		t.drop_id = t.id;
		t.requires_premium = true;
		t.sound_drawing = "event:/SFX/POWERS/MushSpores";
		clone("coffee", "$template_drops$");
		t.name = "Coffee";
		t.falling_chance = 0.01f;
		t.rank = PowerRank.Rank1_common;
		t.drop_id = t.id;
		t.requires_premium = true;
		t.sound_drawing = "event:/SFX/POWERS/Coffee";
		clone("powerup", "$template_drops$");
		t.name = "Powerup";
		t.falling_chance = 0.01f;
		t.rank = PowerRank.Rank1_common;
		t.drop_id = t.id;
		t.requires_premium = true;
		t.sound_drawing = "event:/SFX/POWERS/Powerup";
		clone("plague", "$template_drops$");
		t.name = "Plague";
		t.drop_id = t.id;
		t.falling_chance = 0.01f;
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.sound_drawing = "event:/SFX/POWERS/Plague";
		clone("living_plants", "$template_drops$");
		t.name = "Living Plants";
		t.drop_id = t.id;
		t.actor_asset_id = "living_plants";
		t.falling_chance = 0.01f;
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		t.sound_drawing = "event:/SFX/POWERS/LivingPlants";
		t.surprises_units = true;
		clone("living_house", "$template_drops$");
		t.name = "Living Houses";
		t.drop_id = t.id;
		t.actor_asset_id = "living_house";
		t.falling_chance = 0.01f;
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		t.sound_drawing = "event:/SFX/POWERS/LivingHouses";
		t.surprises_units = true;
		clone("$template_bombs$", "$template_drops$");
		t.falling_chance = 1f;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsRandom;
		GodPower godPower4 = t;
		godPower4.click_power_brush_action = (PowerAction)Delegate.Combine(godPower4.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		clone("bomb", "$template_bombs$");
		t.name = "Bomb";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/Bomb";
		t.surprises_units = true;
		clone("grenade", "bomb");
		t.name = "Grenade";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/Grenade";
		clone("napalm_bomb", "bomb");
		t.name = "Napalm Bomb";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/NapalmBomb";
		clone("atomic_bomb", "$template_bombs$");
		t.name = "Atomic Bomb";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.sound_drawing = "event:/SFX/POWERS/AtomicBomb";
		t.surprises_units = true;
		clone("antimatter_bomb", "$template_bombs$");
		t.name = "Antimatter Bomb";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/AntimatterBomb";
		t.surprises_units = true;
		clone("czar_bomba", "$template_bombs$");
		t.name = "Tsar Bomba";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		t.sound_drawing = "event:/SFX/POWERS/TsarBomb";
		t.surprises_units = true;
		clone("crab_bomb", "bomb");
		t.name = "Crab Bomb";
		t.drop_id = t.id;
		t.sound_drawing = "event:/SFX/POWERS/CrabBomb";
		clone("rain", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Rain";
		t.falling_chance = 0.02f;
		t.sound_drawing = "event:/SFX/POWERS/Rain";
		clone("blood_rain", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Blood Rain";
		t.falling_chance = 0.02f;
		t.sound_drawing = "event:/SFX/POWERS/BloodRain";
		t.surprises_units = true;
		clone("clone_rain", "$template_drops$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		t.drop_id = t.id;
		t.name = "Clone Rain";
		t.falling_chance = 0.02f;
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower5 = t;
		godPower5.click_power_brush_action = (PowerAction)Delegate.Combine(godPower5.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/BloodRain";
		GodPower godPower6 = t;
		godPower6.click_power_action = (PowerAction)Delegate.Combine(godPower6.click_power_action, new PowerAction(fmodDrawingSound));
		clone("dispel", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Dispel";
		t.falling_chance = 0.02f;
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower7 = t;
		godPower7.click_power_brush_action = (PowerAction)Delegate.Combine(godPower7.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/BloodRain";
		GodPower godPower8 = t;
		godPower8.click_power_action = (PowerAction)Delegate.Combine(godPower8.click_power_action, new PowerAction(fmodDrawingSound));
		clone("sleep", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Sleep";
		t.falling_chance = 0.02f;
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower9 = t;
		godPower9.click_power_brush_action = (PowerAction)Delegate.Combine(godPower9.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/BloodRain";
		GodPower godPower10 = t;
		godPower10.click_power_action = (PowerAction)Delegate.Combine(godPower10.click_power_action, new PowerAction(fmodDrawingSound));
		clone("jazz", "$template_drops$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		t.drop_id = t.id;
		t.name = "Smooth Jazz";
		t.falling_chance = 0.02f;
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower11 = t;
		godPower11.click_power_brush_action = (PowerAction)Delegate.Combine(godPower11.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/BloodRain";
		GodPower godPower12 = t;
		godPower12.click_power_action = (PowerAction)Delegate.Combine(godPower12.click_power_action, new PowerAction(fmodDrawingSound));
		clone("fire", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Fire";
		t.falling_chance = 0.01f;
		t.particle_interval = 0.3f;
		t.sound_drawing = "event:/SFX/POWERS/Fire";
		t.surprises_units = true;
		clone("acid", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Acid";
		t.falling_chance = 0.02f;
		t.sound_drawing = "event:/SFX/POWERS/Acid";
		t.surprises_units = true;
		clone("lava", "$template_drops$");
		t.drop_id = t.id;
		t.name = "Lava";
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		t.falling_chance = 0.03f;
		t.sound_drawing = "event:/SFX/POWERS/Lava";
		t.surprises_units = true;
		add(new GodPower
		{
			id = "$template_seeds$",
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true,
			falling_chance = 0.05f,
			type = PowerActionType.PowerSpawnSeeds,
			mouse_hold_animation = MouseHoldAnimation.Sprinkle
		});
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower13 = t;
		godPower13.click_power_brush_action = (PowerAction)Delegate.Combine(godPower13.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		GodPower godPower14 = t;
		godPower14.click_power_action = (PowerAction)Delegate.Combine(godPower14.click_power_action, new PowerAction(fmodDrawingSound));
		t.surprises_units = false;
		clone("seeds_grass", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Grass Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_savanna", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Savanna Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsSavanna";
		clone("seeds_enchanted", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Enchanted Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsEnchanted";
		clone("seeds_corrupted", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Corrupted Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsCorrupted";
		clone("seeds_mushroom", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Mushroom Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsMushroom";
		clone("seeds_swamp", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Swamp Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsSwamp";
		clone("seeds_infernal", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Infernal Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsInfernal";
		clone("seeds_jungle", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Jungle Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsJungle";
		clone("seeds_desert", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Desert Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsDesert";
		clone("seeds_lemon", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Lemon Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsLemon";
		clone("seeds_permafrost", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Permafrost Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsPermafrost";
		clone("seeds_candy", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Candy Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsCandy";
		clone("seeds_crystal", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Crystal Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsCrystal";
		clone("seeds_birch", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Birch Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_maple", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Maple Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_rocklands", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Rocklands Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_garlic", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Garlic Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_flower", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Flower Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_celestial", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Celestial Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_singularity", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Singularity Swamp Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_clover", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Clover Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("seeds_paradox", "$template_seeds$");
		t.drop_id = t.id;
		t.name = "Paradox Seeds";
		t.sound_drawing = "event:/SFX/POWERS/SeedsGrass";
		clone("$template_plants$", "$template_seeds$");
		t.type = PowerActionType.PowerSpawnDrops;
		clone("fruit_bush", "$template_plants$");
		t.type = PowerActionType.PowerSpawnDrops;
		t.falling_chance = 1f;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsRandom;
		GodPower godPower15 = t;
		godPower15.click_power_brush_action = (PowerAction)Delegate.Combine(godPower15.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.drop_id = t.id;
		t.name = "Fruit Bush";
		t.sound_drawing = "event:/SFX/POWERS/FruitBush";
		clone("fertilizer_plants", "$template_plants$");
		t.falling_chance = 1f;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsRandom;
		GodPower godPower16 = t;
		godPower16.click_power_brush_action = (PowerAction)Delegate.Combine(godPower16.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.drop_id = t.id;
		t.name = "Plants Fertilizer";
		t.sound_drawing = "event:/SFX/POWERS/FertilizerPlants";
		clone("fertilizer_trees", "$template_plants$");
		t.falling_chance = 1f;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsRandom;
		GodPower godPower17 = t;
		godPower17.click_power_brush_action = (PowerAction)Delegate.Combine(godPower17.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.drop_id = t.id;
		t.name = "Trees Fertilizer";
		t.sound_drawing = "event:/SFX/POWERS/FertilizerTrees";
		add(new GodPower
		{
			id = "$template_drop_building$",
			unselect_when_window = true,
			type = PowerActionType.PowerSpawnBuilding,
			mouse_hold_animation = MouseHoldAnimation.Sprinkle,
			force_brush = "circ_1",
			set_used_camera_drag_on_long_move = true
		});
		t.click_power_action = spawnDrops;
		GodPower godPower18 = t;
		godPower18.click_power_action = (PowerAction)Delegate.Combine(godPower18.click_power_action, new PowerAction(flashPixel));
		clone("$template_minerals$", "$template_drops$");
		t.falling_chance = 1f;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsRandom;
		GodPower godPower19 = t;
		godPower19.click_power_brush_action = (PowerAction)Delegate.Combine(godPower19.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		clone("stone", "$template_minerals$");
		t.drop_id = t.id;
		t.name = "Stone";
		t.sound_drawing = "event:/SFX/POWERS/Minerals";
		clone("metals", "$template_minerals$");
		t.drop_id = t.id;
		t.name = "Ore Deposit";
		t.sound_drawing = "event:/SFX/POWERS/Minerals";
		clone("gold", "$template_minerals$");
		t.drop_id = t.id;
		t.name = "Gold";
		t.sound_drawing = "event:/SFX/POWERS/Minerals";
		clone("silver", "$template_minerals$");
		t.drop_id = t.id;
		t.name = "Silver";
		t.sound_drawing = "event:/SFX/POWERS/Minerals";
		clone("mythril", "$template_minerals$");
		t.drop_id = t.id;
		t.name = "Mythril";
		t.sound_drawing = "event:/SFX/POWERS/Minerals";
		clone("adamantine", "$template_minerals$");
		t.drop_id = t.id;
		t.name = "Adamantine";
		t.sound_drawing = "event:/SFX/POWERS/Minerals";
		clone("tumor", "$template_drop_building$");
		t.name = "Tumor";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		clone("biomass", "$template_drop_building$");
		t.name = "Biomass";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		clone("super_pumpkin", "$template_drop_building$");
		t.name = "Super Pumpkin";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		clone("cybercore", "$template_drop_building$");
		t.name = "Cybercore";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		clone("geyser", "$template_drop_building$");
		t.name = "Geyser";
		t.drop_id = t.id;
		clone("geyser_acid", "$template_drop_building$");
		t.name = "Acid Geyser";
		t.drop_id = t.id;
		clone("volcano", "$template_drop_building$");
		t.name = "Volcano";
		t.drop_id = t.id;
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		clone("golden_brain", "$template_drop_building$");
		t.name = "Golden Brain";
		t.drop_id = t.id;
		clone("monolith", "$template_drop_building$");
		t.name = "Monolith";
		t.requires_premium = true;
		t.rank = PowerRank.Rank5_noAwards;
		t.drop_id = t.id;
		clone("corrupted_brain", "$template_drop_building$");
		t.name = "Corrupted Brain";
		t.drop_id = t.id;
		clone("ice_tower", "$template_drop_building$");
		t.name = "Ice Tower";
		t.requires_premium = true;
		t.drop_id = t.id;
		t.rank = PowerRank.Rank3_good;
		clone("beehive", "$template_drop_building$");
		t.name = "Beehive";
		t.drop_id = t.id;
		t.rank = PowerRank.Rank1_common;
		clone("flame_tower", "$template_drop_building$");
		t.name = "Flame Tower";
		t.requires_premium = true;
		t.drop_id = t.id;
		t.rank = PowerRank.Rank3_good;
		clone("angle_tower", "$template_drop_building$");
		t.name = "Angle Tower";
		t.requires_premium = true;
		t.drop_id = t.id;
		t.rank = PowerRank.Rank5_noAwards;
	}

	private void addPrinters()
	{
		add(new GodPower
		{
			id = "$template_printer$",
			name = "Printer",
			unselect_when_window = true,
			actor_spawn_height = 3f,
			show_spawn_effect = true,
			actor_asset_id = "printer"
		});
		t.click_action = spawnPrinter;
		clone("printer_hexagon", "$template_printer$");
		t.printers_print = "hexagon";
		clone("printer_skull", "$template_printer$");
		t.printers_print = "skull";
		clone("printer_squares", "$template_printer$");
		t.printers_print = "squares";
		clone("printer_yinyang", "$template_printer$");
		t.printers_print = "yinyang";
		clone("printer_island1", "$template_printer$");
		t.printers_print = "island1";
		clone("printer_star", "$template_printer$");
		t.printers_print = "star";
		clone("printer_heart", "$template_printer$");
		t.printers_print = "heart";
		clone("printer_diamond", "$template_printer$");
		t.printers_print = "diamond";
		clone("printer_alien_drawing", "$template_printer$");
		t.printers_print = "aliendrawing";
		clone("printer_crater", "$template_printer$");
		t.printers_print = "crater";
		clone("printer_labyrinth", "$template_printer$");
		t.printers_print = "labyrinth";
		clone("printer_spiral", "$template_printer$");
		t.printers_print = "spiral";
		clone("printer_star_fort", "$template_printer$");
		t.printers_print = "starfort";
		clone("printer_code", "$template_printer$");
		t.printers_print = "code";
	}

	private void addClouds()
	{
		clone("cloud", "$template_spawn_special$");
		t.name = "Cloud of Life";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudOfLife;
		clone("cloud_rain", "$template_spawn_special$");
		t.name = "Rain Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudRain;
		clone("cloud_fire", "$template_spawn_special$");
		t.name = "Fire Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudFire;
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		clone("cloud_lightning", "$template_spawn_special$");
		t.name = "Thunder Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudLightning;
		clone("cloud_ash", "$template_spawn_special$");
		t.name = "Ash Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudAsh;
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		clone("cloud_magic", "$template_spawn_special$");
		t.name = "Magic Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudMagic;
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		clone("cloud_rage", "$template_spawn_special$");
		t.name = "Rage Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudRage;
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		clone("cloud_acid", "$template_spawn_special$");
		t.name = "Acid Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudAcid;
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		clone("cloud_lava", "$template_spawn_special$");
		t.name = "Lava Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudLava;
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		clone("cloud_snow", "$template_spawn_special$");
		t.name = "Snow Cloud";
		t.multiple_spawn_tip = true;
		t.click_action = spawnCloudSnow;
	}

	private void addDestruction()
	{
		add(new GodPower
		{
			id = "$template_spawn_special$",
			name = "$template_spawn_special$",
			unselect_when_window = true,
			set_used_camera_drag_on_long_move = true
		});
		clone("force", "$template_spawn_special$");
		t.name = "Force";
		GodPower godPower = t;
		godPower.click_action = (PowerActionWithID)Delegate.Combine(godPower.click_action, new PowerActionWithID(spawnForce));
		clone("finger_flick", "$template_spawn_special$");
		t.show_close_actor = true;
		t.name = "finger_flick";
		GodPower godPower2 = t;
		godPower2.click_action = (PowerActionWithID)Delegate.Combine(godPower2.click_action, new PowerActionWithID(fingerFlick));
		add(new GodPower
		{
			id = "infinity_coin",
			name = "Infinity Coin",
			multiple_spawn_tip = true,
			set_used_camera_drag_on_long_move = true
		});
		GodPower godPower3 = t;
		godPower3.click_action = (PowerActionWithID)Delegate.Combine(godPower3.click_action, new PowerActionWithID(spawnInfinityCoin));
		add(new GodPower
		{
			id = "heatray",
			name = "Heatray",
			requires_premium = true,
			rank = PowerRank.Rank2_normal,
			force_brush = "circ_10",
			show_tool_sizes = false,
			unselect_when_window = true,
			hold_action = true
		});
		GodPower godPower4 = t;
		godPower4.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower4.click_brush_action, new PowerActionWithID(heatrayFX));
		GodPower godPower5 = t;
		godPower5.click_action = (PowerActionWithID)Delegate.Combine(godPower5.click_action, new PowerActionWithID(drawHeatray));
		GodPower godPower6 = t;
		godPower6.click_action = (PowerActionWithID)Delegate.Combine(godPower6.click_action, new PowerActionWithID(flashPixel));
		add(new GodPower
		{
			id = "meteorite",
			name = "Meteorite",
			requires_premium = true,
			rank = PowerRank.Rank3_good,
			unselect_when_window = true,
			set_used_camera_drag_on_long_move = true,
			show_spawn_effect = true,
			multiple_spawn_tip = true
		});
		GodPower godPower7 = t;
		godPower7.click_action = (PowerActionWithID)Delegate.Combine(godPower7.click_action, new PowerActionWithID(spawnMeteorite));
		add(new GodPower
		{
			id = "bowling_ball",
			name = "Bowling Ball",
			requires_premium = true,
			rank = PowerRank.Rank2_normal,
			unselect_when_window = true,
			show_spawn_effect = true,
			hold_action = true,
			sound_drawing = "event:/SFX/POWERS/DivineMagnet",
			multiple_spawn_tip = true
		});
		GodPower godPower8 = t;
		godPower8.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower8.click_brush_action, new PowerActionWithID(prepareBoulder));
		GodPower godPower9 = t;
		godPower9.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower9.click_brush_action, new PowerActionWithID(fmodDrawingSound));
		add(new GodPower
		{
			id = "robot_santa",
			name = "Robot Santa",
			requires_premium = false,
			rank = PowerRank.Rank0_free,
			unselect_when_window = true,
			set_used_camera_drag_on_long_move = true,
			show_spawn_effect = true,
			multiple_spawn_tip = true
		});
		GodPower godPower10 = t;
		godPower10.click_action = (PowerActionWithID)Delegate.Combine(godPower10.click_action, new PowerActionWithID(spawnSanta));
		add(new GodPower
		{
			id = "lightning",
			name = "Lightning",
			unselect_when_window = true,
			set_used_camera_drag_on_long_move = true
		});
		GodPower godPower11 = t;
		godPower11.click_action = (PowerActionWithID)Delegate.Combine(godPower11.click_action, new PowerActionWithID(spawnLightning));
		add(new GodPower
		{
			id = "earthquake",
			name = "Earthquake",
			unselect_when_window = true,
			set_used_camera_drag_on_long_move = true
		});
		GodPower godPower12 = t;
		godPower12.click_action = (PowerActionWithID)Delegate.Combine(godPower12.click_action, new PowerActionWithID(spawnEarthquake));
		add(new GodPower
		{
			id = "tornado",
			name = "Tornado",
			requires_premium = true,
			rank = PowerRank.Rank3_good,
			unselect_when_window = true,
			set_used_camera_drag_on_long_move = true,
			show_spawn_effect = true,
			multiple_spawn_tip = true
		});
		GodPower godPower13 = t;
		godPower13.click_action = (PowerActionWithID)Delegate.Combine(godPower13.click_action, new PowerActionWithID(spawnTornado));
	}

	private void addSpecial()
	{
		add(new GodPower
		{
			id = "temperature_plus",
			name = "Temperature",
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true
		});
		t.click_action = drawTemperaturePlus;
		GodPower godPower = t;
		godPower.click_action = (PowerActionWithID)Delegate.Combine(godPower.click_action, new PowerActionWithID(flashPixel));
		t.click_brush_action = loopWithCurrentBrush;
		GodPower godPower2 = t;
		godPower2.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower2.click_brush_action, new PowerActionWithID(fmodDrawingSound));
		t.sound_drawing = "event:/SFX/POWERS/IncreaseTemperature";
		clone("temperature_minus", "temperature_plus");
		t.click_action = drawTemperatureMinus;
		GodPower godPower3 = t;
		godPower3.click_action = (PowerActionWithID)Delegate.Combine(godPower3.click_action, new PowerActionWithID(flashPixel));
		t.sound_drawing = "event:/SFX/POWERS/DecreaseTemperature";
		add(new GodPower
		{
			id = "magnet",
			name = "Magnet",
			show_tool_sizes = true,
			hold_action = true,
			highlight = true,
			sound_drawing = "event:/SFX/POWERS/DivineMagnet",
			unselect_when_window = true
		});
		t.click_brush_action = useMagnet;
		GodPower godPower4 = t;
		godPower4.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower4.click_brush_action, new PowerActionWithID(flashBrushPixelsDuringClick));
		GodPower godPower5 = t;
		godPower5.click_brush_action = (PowerActionWithID)Delegate.Combine(godPower5.click_brush_action, new PowerActionWithID(fmodDrawingSound));
		add(new GodPower
		{
			id = "hide_ui",
			name = "hide_ui",
			path_icon = "iconHideUI",
			rank = PowerRank.Rank0_free
		});
		t.select_button_action = clickHideUI;
		t.tester_enabled = false;
		t.activate_on_hotkey_select = false;
		traits_gamma_rain_edit = add(new GodPower
		{
			id = "traits_gamma_rain_edit",
			name = "Gamma Rain",
			path_icon = "iconRainGammaEdit",
			requires_premium = true,
			rank = PowerRank.Rank5_noAwards
		});
		t.select_button_action = clickTraitEditorRainButton;
		t.tester_enabled = false;
		t.activate_on_hotkey_select = false;
		traits_delta_rain_edit = add(new GodPower
		{
			id = "traits_delta_rain_edit",
			name = "Delta Rain",
			path_icon = "iconRainDeltaEdit",
			requires_premium = true,
			rank = PowerRank.Rank5_noAwards
		});
		t.select_button_action = clickTraitEditorRainButton;
		t.tester_enabled = false;
		t.activate_on_hotkey_select = false;
		traits_omega_rain_edit = add(new GodPower
		{
			id = "traits_omega_rain_edit",
			name = "Omega Rain",
			path_icon = "iconRainOmegaEdit",
			requires_premium = true,
			rank = PowerRank.Rank5_noAwards
		});
		t.select_button_action = clickTraitEditorRainButton;
		t.tester_enabled = false;
		t.activate_on_hotkey_select = false;
		add(new GodPower
		{
			id = "traits_gamma_rain",
			name = "Gamma Rain",
			path_icon = "iconRainGammaEdit",
			requires_premium = true,
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true,
			falling_chance = 0.05f,
			rank = PowerRank.Rank5_noAwards
		});
		t.drop_id = "gamma_rain";
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower6 = t;
		godPower6.click_power_brush_action = (PowerAction)Delegate.Combine(godPower6.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/GammaRain";
		GodPower godPower7 = t;
		godPower7.click_power_action = (PowerAction)Delegate.Combine(godPower7.click_power_action, new PowerAction(fmodDrawingSound));
		add(new GodPower
		{
			id = "traits_delta_rain",
			name = "Delta Rain",
			path_icon = "iconRainDeltaEdit",
			requires_premium = true,
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true,
			falling_chance = 0.05f,
			rank = PowerRank.Rank5_noAwards
		});
		t.drop_id = "delta_rain";
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower8 = t;
		godPower8.click_power_brush_action = (PowerAction)Delegate.Combine(godPower8.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/DeltaRain";
		GodPower godPower9 = t;
		godPower9.click_power_action = (PowerAction)Delegate.Combine(godPower9.click_power_action, new PowerAction(fmodDrawingSound));
		add(new GodPower
		{
			id = "traits_omega_rain",
			name = "Omega Rain",
			path_icon = "iconRainOmegaEdit",
			requires_premium = true,
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true,
			falling_chance = 0.05f,
			rank = PowerRank.Rank5_noAwards
		});
		t.drop_id = "omega_rain";
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower10 = t;
		godPower10.click_power_brush_action = (PowerAction)Delegate.Combine(godPower10.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/OmegaRain";
		GodPower godPower11 = t;
		godPower11.click_power_action = (PowerAction)Delegate.Combine(godPower11.click_power_action, new PowerAction(fmodDrawingSound));
		equipment_rain_edit = add(new GodPower
		{
			id = "equipment_rain_edit",
			name = "Loot Rain",
			path_icon = "iconRainGammaEdit",
			requires_premium = true,
			rank = PowerRank.Rank5_noAwards
		});
		t.select_button_action = clickEquipmentEditorRainButton;
		t.tester_enabled = false;
		t.activate_on_hotkey_select = false;
		add(new GodPower
		{
			id = "equipment_rain",
			name = "Loot Rain",
			path_icon = "iconRainLootEdit",
			requires_premium = true,
			hold_action = true,
			show_tool_sizes = true,
			unselect_when_window = true,
			falling_chance = 0.05f,
			rank = PowerRank.Rank5_noAwards
		});
		t.drop_id = "loot_rain";
		t.click_power_action = spawnDrops;
		t.click_power_brush_action = loopWithCurrentBrushPowerForDropsFull;
		GodPower godPower12 = t;
		godPower12.click_power_brush_action = (PowerAction)Delegate.Combine(godPower12.click_power_brush_action, new PowerAction(flashBrushPixelsDuringClick));
		t.sound_drawing = "event:/SFX/POWERS/GammaRain";
		GodPower godPower13 = t;
		godPower13.click_power_action = (PowerAction)Delegate.Combine(godPower13.click_power_action, new PowerAction(fmodDrawingSound));
		add(new GodPower
		{
			id = "city_select",
			name = "Select City",
			force_map_mode = MetaType.City,
			path_icon = "iconCityInspect",
			can_drag_map = true
		});
		t.tester_enabled = false;
		t.track_activity = false;
		GodPower godPower14 = t;
		godPower14.click_action = (PowerActionWithID)Delegate.Combine(godPower14.click_action, new PowerActionWithID(ActionLibrary.inspectCity));
		add(new GodPower
		{
			id = "relations",
			name = "Relations",
			force_map_mode = MetaType.Kingdom,
			path_icon = "iconDiplomacy",
			can_drag_map = true
		});
		GodPower godPower15 = t;
		godPower15.select_button_action = (PowerButtonClickAction)Delegate.Combine(godPower15.select_button_action, new PowerButtonClickAction(ActionLibrary.selectRelations));
		GodPower godPower16 = t;
		godPower16.click_special_action = (PowerActionWithID)Delegate.Combine(godPower16.click_special_action, new PowerActionWithID(ActionLibrary.clickRelations));
		t.tester_enabled = false;
		add(new GodPower
		{
			id = "whisper_of_war",
			name = "Whisper of War",
			force_map_mode = MetaType.Kingdom,
			path_icon = "iconWhisperOfWar",
			can_drag_map = true
		});
		GodPower godPower17 = t;
		godPower17.select_button_action = (PowerButtonClickAction)Delegate.Combine(godPower17.select_button_action, new PowerButtonClickAction(ActionLibrary.selectWhisperOfWar));
		GodPower godPower18 = t;
		godPower18.click_special_action = (PowerActionWithID)Delegate.Combine(godPower18.click_special_action, new PowerActionWithID(ActionLibrary.clickWhisperOfWar));
		t.tester_enabled = false;
		add(new GodPower
		{
			id = "unity",
			name = "unity",
			force_map_mode = MetaType.Alliance,
			path_icon = "iconUnity",
			can_drag_map = true
		});
		GodPower godPower19 = t;
		godPower19.select_button_action = (PowerButtonClickAction)Delegate.Combine(godPower19.select_button_action, new PowerButtonClickAction(ActionLibrary.selectUnity));
		GodPower godPower20 = t;
		godPower20.click_special_action = (PowerActionWithID)Delegate.Combine(godPower20.click_special_action, new PowerActionWithID(ActionLibrary.clickUnity));
		t.tester_enabled = false;
		inspect_unit = add(new GodPower
		{
			id = "inspect",
			name = "inspect",
			can_drag_map = true,
			set_used_camera_drag_on_long_move = true
		});
		t.tester_enabled = false;
		GodPower godPower21 = t;
		godPower21.click_action = (PowerActionWithID)Delegate.Combine(godPower21.click_action, new PowerActionWithID(ActionLibrary.inspectUnit));
		t.allow_unit_selection = true;
		add(new GodPower
		{
			id = "map_names",
			name = "Map Names",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.toggle_name = "map_names";
		GodPower godPower22 = t;
		godPower22.toggle_action = (PowerToggleAction)Delegate.Combine(godPower22.toggle_action, new PowerToggleAction(toggleMultiOption));
		add(new GodPower
		{
			id = "map_layers",
			name = "map_layers",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "map_layers";
		GodPower godPower23 = t;
		godPower23.toggle_action = (PowerToggleAction)Delegate.Combine(godPower23.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "map_species_families",
			name = "map_species_families",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "map_species_families";
		GodPower godPower24 = t;
		godPower24.toggle_action = (PowerToggleAction)Delegate.Combine(godPower24.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "city_layer",
			name = "city_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_city_layer";
		GodPower godPower25 = t;
		godPower25.toggle_action = (PowerToggleAction)Delegate.Combine(godPower25.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "culture_layer",
			name = "culture_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_culture_layer";
		GodPower godPower26 = t;
		godPower26.toggle_action = (PowerToggleAction)Delegate.Combine(godPower26.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "subspecies_layer",
			name = "subspecies_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_subspecies_layer";
		GodPower godPower27 = t;
		godPower27.toggle_action = (PowerToggleAction)Delegate.Combine(godPower27.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "family_layer",
			name = "family_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_family_layer";
		GodPower godPower28 = t;
		godPower28.toggle_action = (PowerToggleAction)Delegate.Combine(godPower28.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "language_layer",
			name = "language_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_language_layer";
		GodPower godPower29 = t;
		godPower29.toggle_action = (PowerToggleAction)Delegate.Combine(godPower29.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "religion_layer",
			name = "religion_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_religion_layer";
		GodPower godPower30 = t;
		godPower30.toggle_action = (PowerToggleAction)Delegate.Combine(godPower30.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "clan_layer",
			name = "clan_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_clan_layer";
		GodPower godPower31 = t;
		godPower31.toggle_action = (PowerToggleAction)Delegate.Combine(godPower31.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "kingdom_layer",
			name = "kingdom_layer",
			unselect_when_window = true,
			multi_toggle = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_kingdom_layer";
		GodPower godPower32 = t;
		godPower32.toggle_action = (PowerToggleAction)Delegate.Combine(godPower32.toggle_action, new PowerToggleAction(toggleOptionZone));
		add(new GodPower
		{
			id = "alliance_layer",
			name = "alliance_layer",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_alliance_layer";
		GodPower godPower33 = t;
		godPower33.toggle_action = (PowerToggleAction)Delegate.Combine(godPower33.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "army_layer",
			name = "army_layer",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.map_modes_switch = true;
		t.toggle_name = "map_army_layer";
		GodPower godPower34 = t;
		godPower34.toggle_action = (PowerToggleAction)Delegate.Combine(godPower34.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "map_kings_leaders",
			name = "map_kings_leaders",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "map_kings_leaders";
		GodPower godPower35 = t;
		godPower35.toggle_action = (PowerToggleAction)Delegate.Combine(godPower35.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_favorites",
			name = "marks_favorites",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_favorites";
		GodPower godPower36 = t;
		godPower36.toggle_action = (PowerToggleAction)Delegate.Combine(godPower36.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_favorite_items",
			name = "marks_favorite_items",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_favorite_items";
		GodPower godPower37 = t;
		godPower37.toggle_action = (PowerToggleAction)Delegate.Combine(godPower37.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_armies",
			name = "Show Armies",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_armies";
		GodPower godPower38 = t;
		godPower38.toggle_action = (PowerToggleAction)Delegate.Combine(godPower38.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_battles",
			name = "Show Battles",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_battles";
		GodPower godPower39 = t;
		godPower39.toggle_action = (PowerToggleAction)Delegate.Combine(godPower39.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_plots",
			name = "Plot Icons",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_plots";
		GodPower godPower40 = t;
		godPower40.toggle_action = (PowerToggleAction)Delegate.Combine(godPower40.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_wars",
			name = "War Icons",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_wars";
		GodPower godPower41 = t;
		godPower41.toggle_action = (PowerToggleAction)Delegate.Combine(godPower41.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "highlight_kingdom_enemies",
			name = "Highlight Kingdom Enemies",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "highlight_kingdom_enemies";
		GodPower godPower42 = t;
		godPower42.toggle_action = (PowerToggleAction)Delegate.Combine(godPower42.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "only_favorited_meta",
			name = "only_favorited_meta",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "only_favorited_meta";
		GodPower godPower43 = t;
		godPower43.toggle_action = (PowerToggleAction)Delegate.Combine(godPower43.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "unit_metas",
			name = "unit_metas",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "unit_metas";
		GodPower godPower44 = t;
		godPower44.toggle_action = (PowerToggleAction)Delegate.Combine(godPower44.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "money_flow",
			name = "money_flow",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "money_flow";
		GodPower godPower45 = t;
		godPower45.toggle_action = (PowerToggleAction)Delegate.Combine(godPower45.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "meta_conversions",
			name = "meta_conversions",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "meta_conversions";
		GodPower godPower46 = t;
		godPower46.toggle_action = (PowerToggleAction)Delegate.Combine(godPower46.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "talk_bubbles",
			name = "talk_bubbles",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "talk_bubbles";
		GodPower godPower47 = t;
		godPower47.toggle_action = (PowerToggleAction)Delegate.Combine(godPower47.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "icons_happiness",
			name = "icons_happiness",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "icons_happiness";
		GodPower godPower48 = t;
		godPower48.toggle_action = (PowerToggleAction)Delegate.Combine(godPower48.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "icons_tasks",
			name = "icons_tasks",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "icons_tasks";
		GodPower godPower49 = t;
		godPower49.toggle_action = (PowerToggleAction)Delegate.Combine(godPower49.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "army_targets",
			name = "Army Targets",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "army_targets";
		GodPower godPower50 = t;
		godPower50.toggle_action = (PowerToggleAction)Delegate.Combine(godPower50.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "tooltip_zones",
			name = "Tooltip Zones",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "tooltip_zones";
		GodPower godPower51 = t;
		godPower51.toggle_action = (PowerToggleAction)Delegate.Combine(godPower51.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "tooltip_units",
			name = "Tooltip Units",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "tooltip_units";
		GodPower godPower52 = t;
		godPower52.toggle_action = (PowerToggleAction)Delegate.Combine(godPower52.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_destination",
			name = "cursor_arrow_destination",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_destination";
		GodPower godPower53 = t;
		godPower53.toggle_action = (PowerToggleAction)Delegate.Combine(godPower53.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_lover",
			name = "cursor_arrow_lover",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_lover";
		GodPower godPower54 = t;
		godPower54.toggle_action = (PowerToggleAction)Delegate.Combine(godPower54.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_house",
			name = "cursor_arrow_house",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_house";
		GodPower godPower55 = t;
		godPower55.toggle_action = (PowerToggleAction)Delegate.Combine(godPower55.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_family",
			name = "cursor_arrow_family",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_family";
		GodPower godPower56 = t;
		godPower56.toggle_action = (PowerToggleAction)Delegate.Combine(godPower56.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_parents",
			name = "cursor_arrow_parents",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_parents";
		GodPower godPower57 = t;
		godPower57.toggle_action = (PowerToggleAction)Delegate.Combine(godPower57.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_kids",
			name = "cursor_arrow_kids",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_kids";
		GodPower godPower58 = t;
		godPower58.toggle_action = (PowerToggleAction)Delegate.Combine(godPower58.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "cursor_arrow_attack_target",
			name = "cursor_arrow_attack_target",
			unselect_when_window = true
		});
		t.disabled_on_mobile = true;
		t.tester_enabled = false;
		t.toggle_name = "cursor_arrow_attack_target";
		GodPower godPower59 = t;
		godPower59.toggle_action = (PowerToggleAction)Delegate.Combine(godPower59.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "marks_boats",
			name = "marks_boats",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "marks_boats";
		GodPower godPower60 = t;
		godPower60.toggle_action = (PowerToggleAction)Delegate.Combine(godPower60.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "history_log",
			name = "History Log",
			unselect_when_window = true
		});
		t.tester_enabled = false;
		t.toggle_name = "history_log";
		GodPower godPower61 = t;
		godPower61.toggle_action = (PowerToggleAction)Delegate.Combine(godPower61.toggle_action, new PowerToggleAction(toggleOption));
		add(new GodPower
		{
			id = "pause",
			name = "Pause",
			unselect_when_window = true,
			can_drag_map = true
		});
		t.tester_enabled = false;
		t.activate_on_hotkey_select = false;
		add(new GodPower
		{
			id = "clock",
			name = "Clock",
			unselect_when_window = true,
			requires_premium = true,
			ignore_cursor_icon = true,
			rank = PowerRank.Rank0_free,
			can_drag_map = true
		});
		t.tester_enabled = false;
		t.allow_unit_selection = true;
		add(new GodPower
		{
			id = "follow_unit",
			name = "follow_unit",
			unselect_when_window = true,
			can_drag_map = true
		});
		t.tester_enabled = false;
		t.allow_unit_selection = true;
	}

	private void addCivsAnimals()
	{
		clone("civ_cat", "$template_spawn_actor$");
		t.name = "civ_cat";
		t.actor_asset_id = "civ_cat";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_dog", "$template_spawn_actor$");
		t.name = "civ_dog";
		t.actor_asset_id = "civ_dog";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_chicken", "$template_spawn_actor$");
		t.name = "civ_chicken";
		t.actor_asset_id = "civ_chicken";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_rabbit", "$template_spawn_actor$");
		t.name = "civ_rabbit";
		t.actor_asset_id = "civ_rabbit";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_monkey", "$template_spawn_actor$");
		t.name = "civ_monkey";
		t.actor_asset_id = "civ_monkey";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_fox", "$template_spawn_actor$");
		t.name = "civ_fox";
		t.actor_asset_id = "civ_fox";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_sheep", "$template_spawn_actor$");
		t.name = "civ_sheep";
		t.actor_asset_id = "civ_sheep";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_cow", "$template_spawn_actor$");
		t.name = "civ_cow";
		t.actor_asset_id = "civ_cow";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_armadillo", "$template_spawn_actor$");
		t.name = "civ_armadillo";
		t.actor_asset_id = "civ_armadillo";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_wolf", "$template_spawn_actor$");
		t.name = "civ_wolf";
		t.actor_asset_id = "civ_wolf";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_bear", "$template_spawn_actor$");
		t.name = "civ_bear";
		t.actor_asset_id = "civ_bear";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_rhino", "$template_spawn_actor$");
		t.name = "civ_rhino";
		t.actor_asset_id = "civ_rhino";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_buffalo", "$template_spawn_actor$");
		t.name = "civ_buffalo";
		t.actor_asset_id = "civ_buffalo";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_hyena", "$template_spawn_actor$");
		t.name = "civ_hyena";
		t.actor_asset_id = "civ_hyena";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_rat", "$template_spawn_actor$");
		t.name = "civ_rat";
		t.actor_asset_id = "civ_rat";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_alpaca", "$template_spawn_actor$");
		t.name = "civ_alpaca";
		t.actor_asset_id = "civ_alpaca";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_capybara", "$template_spawn_actor$");
		t.name = "civ_capybara";
		t.actor_asset_id = "civ_capybara";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_goat", "$template_spawn_actor$");
		t.name = "civ_goat";
		t.actor_asset_id = "civ_goat";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_crab", "$template_spawn_actor$");
		t.name = "civ_crab";
		t.actor_asset_id = "civ_crab";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_penguin", "$template_spawn_actor$");
		t.name = "civ_penguin";
		t.actor_asset_id = "civ_penguin";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_turtle", "$template_spawn_actor$");
		t.name = "civ_turtle";
		t.actor_asset_id = "civ_turtle";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_crocodile", "$template_spawn_actor$");
		t.name = "civ_crocodile";
		t.actor_asset_id = "civ_crocodile";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_snake", "$template_spawn_actor$");
		t.name = "civ_snake";
		t.actor_asset_id = "civ_snake";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_frog", "$template_spawn_actor$");
		t.name = "civ_frog";
		t.actor_asset_id = "civ_frog";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_piranha", "$template_spawn_actor$");
		t.name = "civ_piranha";
		t.actor_asset_id = "civ_piranha";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_scorpion", "$template_spawn_actor$");
		t.name = "civ_scorpion";
		t.actor_asset_id = "civ_scorpion";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_candy_man", "$template_spawn_actor$");
		t.name = "civ_candy_man";
		t.actor_asset_id = "civ_candy_man";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_crystal_golem", "$template_spawn_actor$");
		t.name = "civ_crystal_golem";
		t.actor_asset_id = "civ_crystal_golem";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_liliar", "$template_spawn_actor$");
		t.name = "civ_liliar";
		t.actor_asset_id = "civ_liliar";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_garlic_man", "$template_spawn_actor$");
		t.name = "civ_garlic_man";
		t.actor_asset_id = "civ_garlic_man";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_lemon_man", "$template_spawn_actor$");
		t.name = "civ_lemon_man";
		t.actor_asset_id = "civ_lemon_man";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_acid_gentleman", "$template_spawn_actor$");
		t.name = "civ_acid_gentleman";
		t.actor_asset_id = "civ_acid_gentleman";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_beetle", "$template_spawn_actor$");
		t.name = "civ_beetle";
		t.actor_asset_id = "civ_beetle";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_seal", "$template_spawn_actor$");
		t.name = "civ_seal";
		t.actor_asset_id = "civ_seal";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		clone("civ_unicorn", "$template_spawn_actor$");
		t.name = "civ_unicorn";
		t.actor_asset_id = "civ_unicorn";
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
	}

	private void addMobs()
	{
		clone("cold_one", "$template_spawn_actor$");
		t.name = "Cold Ones";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "cold_one";
		clone("demon", "$template_spawn_actor$");
		t.name = "Demon";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "demon";
		clone("angle", "$template_spawn_actor$");
		t.name = "Angle";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "angle";
		clone("tumor_monster_unit", "$template_spawn_actor$");
		t.name = "Tumor Monster";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_ids = AssetLibrary<GodPower>.a<string>("tumor_monster_unit", "tumor_monster_animal");
		clone("mush_unit", "$template_spawn_actor$");
		t.name = "Mush";
		t.requires_premium = false;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_ids = AssetLibrary<GodPower>.a<string>("mush_unit", "mush_animal");
		clone("bioblob", "$template_spawn_actor$");
		t.name = "Bioblob";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "bioblob";
		clone("lil_pumpkin", "$template_spawn_actor$");
		t.name = "Lil Pumpkin";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "lil_pumpkin";
		clone("assimilator", "$template_spawn_actor$");
		t.name = "Assimilator";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "assimilator";
		clone("necromancer", "$template_spawn_actor$");
		t.name = "Necromancer";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "necromancer";
		clone("druid", "$template_spawn_actor$");
		t.name = "Druid";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "druid";
		clone("plague_doctor", "$template_spawn_actor$");
		t.name = "Plague Doctor";
		t.actor_asset_id = "plague_doctor";
		clone("evil_mage", "$template_spawn_actor$");
		t.name = "Evil Mage";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "evil_mage";
		clone("white_mage", "$template_spawn_actor$");
		t.name = "White Mage";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_id = "white_mage";
		clone("bandit", "$template_spawn_actor$");
		t.name = "Bandits";
		t.actor_asset_id = "bandit";
		clone("snowman", "$template_spawn_actor$");
		t.name = "Snowman";
		t.actor_asset_id = "snowman";
		clone("zombie", "$template_spawn_actor$");
		t.name = "Zombie";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		t.actor_asset_ids = AssetLibrary<GodPower>.a<string>("zombie_human", "zombie_orc", "zombie_dwarf", "zombie_elf");
		clone("skeleton", "$template_spawn_actor$");
		t.name = "Skeleton";
		t.rank = PowerRank.Rank0_free;
		t.actor_asset_id = "skeleton";
		clone("sheep", "$template_spawn_actor$");
		t.name = "Sheeps";
		t.actor_asset_id = "sheep";
		clone("rhino", "$template_spawn_actor$");
		t.name = "Rhino";
		t.actor_asset_id = "rhino";
		clone("monkey", "$template_spawn_actor$");
		t.name = "Monkey";
		t.actor_asset_id = "monkey";
		clone("buffalo", "$template_spawn_actor$");
		t.name = "Buffalo";
		t.actor_asset_id = "buffalo";
		clone("fox", "$template_spawn_actor$");
		t.name = "Fox";
		t.actor_asset_id = "fox";
		clone("hyena", "$template_spawn_actor$");
		t.name = "Hyena";
		t.actor_asset_id = "hyena";
		clone("dog", "$template_spawn_actor$");
		t.name = "Dog";
		t.actor_asset_id = "dog";
		clone("cow", "$template_spawn_actor$");
		t.name = "Cow";
		t.actor_asset_id = "cow";
		clone("frog", "$template_spawn_actor$");
		t.name = "Frog";
		t.actor_asset_id = "frog";
		clone("crocodile", "$template_spawn_actor$");
		t.name = "Crocodile";
		t.actor_asset_id = "crocodile";
		clone("snake", "$template_spawn_actor$");
		t.name = "Snake";
		t.actor_asset_id = "snake";
		clone("turtle", "$template_spawn_actor$");
		t.name = "Turtle";
		t.actor_asset_id = "turtle";
		clone("penguin", "$template_spawn_actor$");
		t.name = "Penguin";
		t.actor_asset_id = "penguin";
		clone("crab", "$template_spawn_actor$");
		t.name = "Crab";
		t.actor_asset_id = "crab";
		clone("rabbit", "$template_spawn_actor$");
		t.name = "Rabbit";
		t.actor_asset_id = "rabbit";
		clone("cat", "$template_spawn_actor$");
		t.name = "Cat";
		t.actor_asset_id = "cat";
		clone("chicken", "$template_spawn_actor$");
		t.name = "Chicken";
		t.actor_asset_id = "chicken";
		clone("wolf", "$template_spawn_actor$");
		t.name = "Wolfs";
		t.actor_asset_id = "wolf";
		clone("armadillo", "$template_spawn_actor$");
		t.name = "Armadillo";
		t.actor_asset_id = "armadillo";
		clone("raccoon", "$template_spawn_actor$");
		t.name = "Raccoon";
		t.actor_asset_id = "raccoon";
		clone("seal", "$template_spawn_actor$");
		t.name = "Seal";
		t.actor_asset_id = "seal";
		clone("ostrich", "$template_spawn_actor$");
		t.name = "Ostrich";
		t.actor_asset_id = "ostrich";
		clone("unicorn", "$template_spawn_actor$");
		t.name = "Unicorn";
		t.actor_asset_id = "unicorn";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		clone("alpaca", "$template_spawn_actor$");
		t.name = "Alpaca";
		t.actor_asset_id = "alpaca";
		clone("capybara", "$template_spawn_actor$");
		t.name = "Capybara";
		t.actor_asset_id = "capybara";
		clone("scorpion", "$template_spawn_actor$");
		t.name = "Scorpion";
		t.actor_asset_id = "scorpion";
		clone("flower_bud", "$template_spawn_actor$");
		t.name = "Flower Bud";
		t.actor_asset_id = "flower_bud";
		clone("lemon_snail", "$template_spawn_actor$");
		t.name = "Bitba";
		t.actor_asset_id = "lemon_snail";
		clone("garl", "$template_spawn_actor$");
		t.name = "Garl";
		t.actor_asset_id = "garl";
		clone("bear", "$template_spawn_actor$");
		t.name = "Bear";
		t.actor_asset_id = "bear";
		clone("piranha", "$template_spawn_actor$");
		t.name = "Piranha";
		t.actor_asset_id = "piranha";
		clone("worm", "$template_spawn_actor$");
		t.name = "Worm";
		t.actor_asset_id = "worm";
		clone("crystal_sword", "$template_spawn_actor$");
		t.name = "Crystal Sword";
		t.actor_asset_id = "crystal_sword";
		clone("jumpy_skull", "$template_spawn_actor$");
		t.name = "Rude Skull";
		t.actor_asset_id = "jumpy_skull";
		clone("fire_skull", "$template_spawn_actor$");
		t.name = "Fire Skull";
		t.actor_asset_id = "fire_skull";
		clone("fire_elemental", "$template_spawn_actor$");
		t.name = "Fire Elemental";
		t.actor_asset_ids = SA.fire_elementals;
		clone("ghost", "$template_spawn_actor$");
		t.name = "Ghost";
		t.actor_asset_id = "ghost";
		clone("alien", "$template_spawn_actor$");
		t.name = "Alien";
		t.actor_asset_id = "alien";
		t.requires_premium = true;
		t.rank = PowerRank.Rank3_good;
		clone("greg", "$template_spawn_actor$");
		t.name = "Greg";
		t.actor_asset_id = "greg";
		t.requires_premium = true;
		t.rank = PowerRank.Rank5_noAwards;
		clone("smore", "$template_spawn_actor$");
		t.name = "Smore";
		t.actor_asset_id = "smore";
		clone("sand_spider", "$template_spawn_actor$");
		t.name = "Sand Spider";
		t.actor_asset_id = "sand_spider";
		t.hold_action = true;
		clone("goat", "$template_spawn_actor$");
		t.name = "Goat";
		t.actor_asset_id = "goat";
		clone("acid_blob", "$template_spawn_actor$");
		t.name = "Acid Blob";
		t.actor_asset_id = "acid_blob";
		clone("god_finger", "$template_spawn_actor$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank2_normal;
		t.name = "God Finger";
		t.actor_asset_id = "god_finger";
		clone("UFO", "$template_spawn_actor$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		t.name = "UFO";
		t.actor_asset_id = "UFO";
		clone("dragon", "$template_spawn_actor$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank4_awesome;
		t.name = "Dragon";
		t.actor_asset_id = "dragon";
		clone("fairy", "$template_spawn_actor$");
		t.rank = PowerRank.Rank2_normal;
		t.name = "Fairy";
		t.actor_asset_id = "fairy";
		t.requires_premium = true;
		clone("butterfly", "$template_spawn_actor$");
		t.rank = PowerRank.Rank1_common;
		t.name = "Butterfly";
		t.actor_asset_id = "butterfly";
		clone("bee", "$template_spawn_actor$");
		t.rank = PowerRank.Rank1_common;
		t.name = "Bee";
		t.actor_asset_id = "bee";
		t.click_action = spawnUnit;
		clone("grasshopper", "$template_spawn_actor$");
		t.rank = PowerRank.Rank1_common;
		t.name = "Grasshopper";
		t.actor_asset_id = "grasshopper";
		clone("fly", "$template_spawn_actor$");
		t.rank = PowerRank.Rank1_common;
		t.name = "Fly";
		t.actor_asset_id = "fly";
		clone("beetle", "$template_spawn_actor$");
		t.rank = PowerRank.Rank1_common;
		t.name = "Beetle";
		t.actor_asset_id = "beetle";
		clone("rat", "$template_spawn_actor$");
		t.name = "Rat";
		t.actor_asset_id = "rat";
		clone("ant_blue", "$template_spawn_actor$");
		t.name = "Blue Ant";
		t.actor_asset_id = "ant_blue";
		clone("ant_green", "$template_spawn_actor$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Green Ant";
		t.actor_asset_id = "ant_green";
		clone("ant_black", "$template_spawn_actor$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Black Ant";
		t.actor_asset_id = "ant_black";
		clone("ant_red", "$template_spawn_actor$");
		t.requires_premium = true;
		t.rank = PowerRank.Rank1_common;
		t.name = "Red Ant";
		t.actor_asset_id = "ant_red";
		clone("crabzilla", "$template_spawn_actor$");
		t.name = "Crabzilla";
		t.rank = PowerRank.Rank4_awesome;
		t.requires_premium = true;
		t.actor_asset_id = "crabzilla";
		t.actor_spawn_height = 0f;
		t.ignore_fast_spawn = true;
		t.tester_enabled = false;
		t.multiple_spawn_tip = false;
		t.click_action = spawnCrabzilla;
	}

	private void addCivsClassic()
	{
		add(new GodPower
		{
			id = "$template_spawn_actor$",
			type = PowerActionType.PowerSpawnActor,
			rank = PowerRank.Rank0_free,
			unselect_when_window = true,
			show_spawn_effect = true,
			actor_spawn_height = 3f,
			multiple_spawn_tip = true,
			show_unit_stats_overview = true,
			set_used_camera_drag_on_long_move = true
		});
		t.click_action = spawnUnit;
		clone("human", "$template_spawn_actor$");
		t.name = "Human";
		t.actor_asset_id = "human";
		clone("orc", "$template_spawn_actor$");
		t.rank = PowerRank.Rank4_awesome;
		t.requires_premium = true;
		t.name = "Orc";
		t.actor_asset_id = "orc";
		clone("elf", "$template_spawn_actor$");
		t.rank = PowerRank.Rank4_awesome;
		t.requires_premium = true;
		t.name = "Elf";
		t.actor_asset_id = "elf";
		clone("dwarf", "$template_spawn_actor$");
		t.rank = PowerRank.Rank4_awesome;
		t.requires_premium = true;
		t.name = "Dwarf";
		t.actor_asset_id = "dwarf";
	}

	public override void linkAssets()
	{
		foreach (GodPower item in list)
		{
			if (!string.IsNullOrEmpty(item.drop_id))
			{
				item.cached_drop_asset = AssetManager.drops.get(item.drop_id);
			}
			if (!string.IsNullOrEmpty(item.tile_type))
			{
				item.cached_tile_type_asset = AssetManager.tiles.get(item.tile_type);
			}
			if (!string.IsNullOrEmpty(item.top_tile_type))
			{
				item.cached_top_tile_type_asset = AssetManager.top_tiles.get(item.top_tile_type);
			}
			if (item.actor_asset_id != null)
			{
				ActorAsset actorAsset = AssetManager.actor_library.get(item.actor_asset_id);
				if (actorAsset.power_id == null)
				{
					actorAsset.power_id = item.id;
				}
			}
			string[] actor_asset_ids = item.actor_asset_ids;
			if (actor_asset_ids == null || actor_asset_ids.Length == 0)
			{
				continue;
			}
			string[] actor_asset_ids2 = item.actor_asset_ids;
			foreach (string pID in actor_asset_ids2)
			{
				ActorAsset actorAsset2 = AssetManager.actor_library.get(pID);
				if (actorAsset2.power_id == null)
				{
					actorAsset2.power_id = item.id;
				}
			}
		}
	}

	private void traceRanks(PowerButton pTarget)
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		for (int i = 0; i < list.Count; i++)
		{
			GodPower godPower = list[i];
			switch (godPower.rank)
			{
			case PowerRank.Rank0_free:
				text = text + godPower.name + ", ";
				break;
			case PowerRank.Rank1_common:
				text2 = text2 + godPower.name + ", ";
				break;
			case PowerRank.Rank2_normal:
				text3 = text3 + godPower.name + ", ";
				break;
			case PowerRank.Rank3_good:
				text4 = text4 + godPower.name + ", ";
				break;
			case PowerRank.Rank4_awesome:
				text5 = text5 + godPower.name + ", ";
				break;
			}
		}
		Debug.Log((object)("rank 0: " + text));
		Debug.Log((object)("rank 1: " + text2));
		Debug.Log((object)("rank 2: " + text3));
		Debug.Log((object)("rank 3: " + text4));
		Debug.Log((object)("rank 4: " + text5));
	}

	private bool spawnDrops(WorldTile tTile, GodPower pPower)
	{
		BrushData current_brush_data = Config.current_brush_data;
		bool flag = false;
		if (current_brush_data.size == 0 && current_brush_data.fast_spawn)
		{
			if (World.world.player_control.timer_spawn_pixels <= 0f)
			{
				World.world.player_control.timer_spawn_pixels = 0.5f;
				flag = true;
			}
		}
		else if (current_brush_data.fast_spawn && Randy.randomBool())
		{
			if (World.world.player_control.timer_spawn_pixels <= 0f)
			{
				World.world.player_control.timer_spawn_pixels = 0.3f;
				flag = true;
			}
		}
		else
		{
			flag = Randy.randomChance(pPower.falling_chance);
		}
		if (World.world.player_control.first_click)
		{
			World.world.player_control.first_click = false;
			flag = true;
			World.world.player_control.timer_spawn_pixels = 0.3f;
		}
		if (flag)
		{
			World.world.drop_manager.spawn(tTile, pPower.cached_drop_asset, -1f, -1f, pForceSurprise: true, -1L).soundOn = true;
		}
		return true;
	}

	private bool spawnPrinter(WorldTile pTile, string pPower)
	{
		GodPower godPower = get(pPower);
		EffectsLibrary.spawn("fx_spawn", pTile);
		World.world.units.spawnNewUnitByPlayer("printer", pTile, pSpawnSound: true).data.set("template", godPower.printers_print);
		AchievementLibrary.print_heart.check(godPower);
		return true;
	}

	private bool useMagnet(WorldTile pTile, string pPower)
	{
		World.world.magnet.magnetAction(pFromUpdate: false, pTile);
		return true;
	}

	private bool spawnCloudSnow(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_snow");
		return true;
	}

	private bool spawnCloudLava(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_lava");
		return true;
	}

	private bool spawnCloudAcid(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_acid");
		return true;
	}

	private bool spawnCloudOfLife(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_normal");
		return true;
	}

	private bool spawnCloudRain(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_rain");
		return true;
	}

	private bool spawnCloudFire(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_fire");
		return true;
	}

	private bool spawnCloudLightning(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_lightning");
		return true;
	}

	private bool spawnCloudMagic(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_magic");
		return true;
	}

	private bool spawnCloudRage(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_rage");
		return true;
	}

	private bool spawnCloudAsh(WorldTile pTile, string pPower)
	{
		spawnCloud(pTile, "cloud_ash");
		return true;
	}

	private void spawnCloud(WorldTile pTile, string pCloudID)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		EffectsLibrary.spawn("fx_cloud", pTile, pCloudID);
		Vector2Int pos = pTile.pos;
		float pX = ((Vector2Int)(ref pos)).x;
		pos = pTile.pos;
		MusicBox.playSound("event:/SFX/UNIQUE/SpawnCloud", pX, ((Vector2Int)(ref pos)).y);
	}

	private bool spawnCrabzilla(WorldTile pTile, string pPower)
	{
		World.world.player_control.already_used_power = false;
		World.world.selected_buttons.unselectAll();
		((SpawnEffect)EffectsLibrary.spawn("fx_spawn_big", pTile)).setEvent("crabzilla", pTile);
		return true;
	}

	private bool spawnLightning(WorldTile pTile, string pPower)
	{
		MapBox.spawnLightningBig(pTile);
		return true;
	}

	private bool spawnForce(WorldTile pTile, string pPower)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionForce", pTile);
		World.world.applyForceOnTile(pTile, 10, 3f, pForceOut: true, 0, null, null, null, pChangeHappiness: true);
		EffectsLibrary.spawnExplosionWave(pTile.posV3, 10f);
		return true;
	}

	private bool fingerFlick(WorldTile pTile, string pPower)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Actor actorNearCursor = World.world.getActorNearCursor();
		if (actorNearCursor == null)
		{
			return false;
		}
		Vector2 mousePos = World.world.getMousePos();
		Vector2 current_position = actorNearCursor.current_position;
		float pForceAmountDirection = Randy.randomFloat(2.5f, 5f);
		float pForceHeight = Randy.randomFloat(2.5f, 3f);
		actorNearCursor.calculateForce(current_position.x, current_position.y, mousePos.x, mousePos.y, pForceAmountDirection, pForceHeight, pCheckCancelJobOnLand: true);
		actorNearCursor.addStatusEffect("flicked");
		actorNearCursor.makeStunned();
		return true;
	}

	private bool spawnInfinityCoin(WorldTile pTile, string pPower)
	{
		EffectsLibrary.spawn("fx_infinity_coin", pTile);
		return true;
	}

	private bool spawnEarthquake(WorldTile pTile, string pPower)
	{
		Earthquake.startQuake(pTile);
		return true;
	}

	private bool spawnMeteorite(WorldTile pTile, string pPower)
	{
		Meteorite.spawnMeteorite(pTile);
		return true;
	}

	private bool spawnTornado(WorldTile pTile, string pPower)
	{
		EffectsLibrary.spawnAtTile("fx_tornado", pTile, 0.5f);
		return true;
	}

	private bool prepareBoulder(WorldTile pTile, string pPower)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Touch pTouch = default(Touch);
		Vector2 pPosition;
		if (InputHelpers.mouseSupported)
		{
			pPosition = World.world.getMousePos();
		}
		else
		{
			if (!World.world.player_control.getTouchPos(out pTouch, pOnlyGameplay: true))
			{
				return false;
			}
			pPosition = Vector2.op_Implicit(World.world.camera.ScreenToWorldPoint(Vector2.op_Implicit(((Touch)(ref pTouch)).position)));
		}
		Boulder.chargeBoulder(pPosition, pTouch);
		return true;
	}

	private bool spawnSanta(WorldTile pTile, string pPower)
	{
		EffectsLibrary.spawn("fx_santa", pTile, "santa");
		return true;
	}

	private void toggleOptionZone(string pPower)
	{
		GodPower pPower2 = AssetManager.powers.get(pPower);
		MetaTypeAsset fromPower = AssetManager.meta_type_library.getFromPower(pPower2);
		if (InputHelpers.GetMouseButtonUp(1))
		{
			fromPower.toggleOptionZone(pPower2, -1);
		}
		else
		{
			fromPower.toggleOptionZone(pPower2);
		}
	}

	internal void toggleMultiOption(string pPower)
	{
		GodPower godPower = AssetManager.powers.get(pPower);
		string toggle_name = godPower.toggle_name;
		OptionAsset optionAsset = AssetManager.options_library.get(toggle_name);
		int num = 1;
		num = ((!InputHelpers.GetMouseButtonUp(1)) ? 1 : (-1));
		PlayerOptionData data = optionAsset.data;
		if (data.boolVal)
		{
			data.intVal += num;
			if (data.intVal > optionAsset.max_value)
			{
				data.intVal = 0;
				data.boolVal = false;
			}
			if (data.intVal < 0)
			{
				data.intVal = optionAsset.max_value;
			}
		}
		else
		{
			data.boolVal = true;
		}
		PlayerConfig.saveData();
		string translatedName = godPower.getTranslatedName();
		string translatedDescription = godPower.getTranslatedDescription();
		string translatedOption = optionAsset.getTranslatedOption();
		if (data.boolVal)
		{
			WorldTip.instance.showToolbarText(translatedName + " - " + translatedOption, translatedDescription);
		}
	}

	private void toggleOption(string pPower)
	{
		GodPower godPower = AssetManager.powers.get(pPower);
		WorldTip.instance.showToolbarText(godPower);
		PlayerOptionData playerOptionData = PlayerConfig.dict[godPower.toggle_name];
		playerOptionData.boolVal = !playerOptionData.boolVal;
		if (godPower.map_modes_switch)
		{
			if (playerOptionData.boolVal)
			{
				disableAllOtherMapModes(pPower);
			}
			else
			{
				WorldTip.instance.startHide();
			}
		}
		PlayerConfig.saveData();
	}

	internal static void disableAllOtherMapModes(string pMainPower)
	{
		for (int i = 0; i < AssetManager.powers.list.Count; i++)
		{
			GodPower godPower = AssetManager.powers.list[i];
			if (godPower.map_modes_switch && !(godPower.id == pMainPower))
			{
				PlayerConfig.dict[godPower.toggle_name].boolVal = false;
			}
		}
	}

	private bool useVortex(WorldTile pTile, string pPower)
	{
		if (pTile.isTemporaryFrozen())
		{
			pTile.unfreeze(99);
		}
		VortexAction.moveTiles(pTile, Config.current_brush_data);
		return true;
	}

	private bool drawTiles(WorldTile pTile, string pPowerID)
	{
		GodPower godPower = get(pPowerID);
		TileType cached_tile_type_asset = godPower.cached_tile_type_asset;
		TopTileType cached_top_tile_type_asset = godPower.cached_top_tile_type_asset;
		World.world.flash_effects.flashPixel(pTile, 25);
		if (cached_top_tile_type_asset != null && cached_top_tile_type_asset.wall && pTile.Type.id != cached_top_tile_type_asset.id)
		{
			World.world.game_stats.data.wallsPlaced++;
			AchievementLibrary.segregator.check();
		}
		MapAction.terraformTile(pTile, cached_tile_type_asset, cached_top_tile_type_asset, TerraformLibrary.draw);
		return true;
	}

	private bool flashPixel(WorldTile pTile, string pPowerID = null)
	{
		World.world.flash_effects.flashPixel(pTile, 10);
		return true;
	}

	private bool flashPixel(WorldTile pTile, GodPower pPower)
	{
		World.world.flash_effects.flashPixel(pTile, 10);
		return true;
	}

	private bool drawTemperaturePlus(WorldTile pTile, string pPower)
	{
		if (pTile.isTemporaryFrozen() && Randy.randomBool())
		{
			pTile.unfreeze();
		}
		WorldBehaviourUnitTemperatures.checkTile(pTile, 5);
		if (pTile.Type.lava)
		{
			LavaHelper.heatUpLava(pTile);
		}
		if (pTile.hasBuilding() && pTile.building.asset.spawn_drops)
		{
			pTile.building.data.removeFlag("stop_spawn_drops");
		}
		return true;
	}

	public bool clickHideUI(string pPowerId)
	{
		if (ScrollWindow.isWindowActive())
		{
			return true;
		}
		Config.ui_main_hidden = true;
		return true;
	}

	public bool clickTraitEditorRainButton(string pPowerId)
	{
		Config.selected_trait_editor = pPowerId;
		ScrollWindow.showWindow("trait_rain_editor");
		return true;
	}

	public bool clickEquipmentEditorRainButton(string pPowerId)
	{
		ScrollWindow.showWindow("equipment_rain_editor");
		return true;
	}

	public static bool drawTemperatureMinus(WorldTile pTile, string pPower)
	{
		if (pTile.Type.lava)
		{
			LavaHelper.coolDownLava(pTile);
		}
		if (pTile.isOnFire())
		{
			pTile.stopFire();
		}
		if (pTile.canBeFrozen() && Randy.randomBool())
		{
			if (pTile.health > 0)
			{
				pTile.health--;
			}
			else
			{
				pTile.freeze();
			}
		}
		WorldBehaviourUnitTemperatures.checkTile(pTile, -5);
		if (pTile.hasBuilding())
		{
			ActionLibrary.addFrozenEffectOnTarget(null, pTile.building);
		}
		if (pTile.hasBuilding() && pTile.building.asset.spawn_drops)
		{
			pTile.building.data.addFlag("stop_spawn_drops");
		}
		return true;
	}

	private bool drawShovelPlus(WorldTile pTile, string pPower)
	{
		if (pTile.health > 0)
		{
			pTile.health--;
		}
		else
		{
			MapAction.increaseTile(pTile, pDamage: false, "destroy");
		}
		return false;
	}

	private bool drawShovelMinus(WorldTile pTile, string pPower)
	{
		if (pTile.health > 0)
		{
			pTile.health--;
		}
		else
		{
			MapAction.decreaseTile(pTile, pDamage: false, "destroy");
		}
		return false;
	}

	private bool drawGreyGoo(WorldTile pTile, string pPower)
	{
		World.world.grey_goo_layer.add(pTile);
		return false;
	}

	private bool drawConway(WorldTile pTile, string pPower)
	{
		if (Randy.randomBool())
		{
			World.world.conway_layer.add(pTile, "conway");
		}
		return false;
	}

	private bool drawConwayInverse(WorldTile pTile, string pPower)
	{
		if (Randy.randomBool())
		{
			World.world.conway_layer.add(pTile, "conway_inverse");
		}
		return false;
	}

	private bool drawFinger(WorldTile pTile, string pPower)
	{
		TileType first_pressed_type = World.world.player_control.first_pressed_type;
		TopTileType topTileType = World.world.player_control.first_pressed_top_type;
		if (topTileType != null && !topTileType.allowed_to_be_finger_copied)
		{
			topTileType = null;
		}
		if (first_pressed_type.ground && (topTileType == null || topTileType.ground))
		{
			MapAction.terraformTile(pTile, first_pressed_type, topTileType, TerraformLibrary.draw);
		}
		else
		{
			destroyBuildings(pTile, pPower);
			MapAction.terraformTile(pTile, first_pressed_type, topTileType, TerraformLibrary.destroy_no_flash);
		}
		if (pTile.Type.grey_goo)
		{
			World.world.grey_goo_layer.add(pTile);
		}
		if (topTileType != null && topTileType.biome_id == "biome_grass")
		{
			AchievementLibrary.touch_the_grass.check();
		}
		return false;
	}

	private bool drawBorderBrush(WorldTile pTile, string pPower)
	{
		WorldTile first_pressed_tile = World.world.player_control.first_pressed_tile;
		if (first_pressed_tile == null)
		{
			return false;
		}
		City zone_city = first_pressed_tile.zone_city;
		if (zone_city == null)
		{
			return false;
		}
		zone_city.addZone(pTile.zone);
		World.world.city_zone_helper.city_place_finder.setDirty();
		zone_city.setAbandonedZonesDirty();
		return false;
	}

	private bool spawnUnit(WorldTile pTile, string pPowerID)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		GodPower godPower = get(pPowerID);
		Vector2Int pos = pTile.pos;
		float pX = ((Vector2Int)(ref pos)).x;
		pos = pTile.pos;
		MusicBox.playSound("event:/SFX/UNIQUE/SpawnWhoosh", pX, ((Vector2Int)(ref pos)).y);
		if (godPower.id == "sheep" && pTile.Type.lava)
		{
			AchievementLibrary.sacrifice.check();
		}
		EffectsLibrary.spawn("fx_spawn", pTile);
		string[] actor_asset_ids = godPower.actor_asset_ids;
		string pStatsID = ((actor_asset_ids == null || actor_asset_ids.Length == 0) ? godPower.actor_asset_id : godPower.actor_asset_ids.GetRandom());
		Actor pCheckData = World.world.units.spawnNewUnitByPlayer(pStatsID, pTile, pSpawnSound: true, pMiracleSpawn: true, godPower.actor_spawn_height);
		AchievementLibrary.back_to_beta_testing.check(pCheckData);
		return true;
	}

	private bool divineLightFX(WorldTile pCenterTile, string pPowerID)
	{
		World.world.fx_divine_light.playOn(pCenterTile);
		return true;
	}

	private bool drawDivineLight(WorldTile pCenterTile, string pPowerID)
	{
		pCenterTile.doUnits(delegate(Actor pActor)
		{
			clearBadTraitsFrom(pActor);
			if (pActor.asset.can_be_killed_by_divine_light)
			{
				pActor.getHit(pActor.getMaxHealthPercent(0.4f), pFlash: true, AttackType.Divine);
			}
			else
			{
				pActor.startColorEffect();
			}
			pActor.finishStatusEffect("ash_fever");
			pActor.finishAngryStatus();
			if (!pActor.isInLiquid())
			{
				pActor.cancelAllBeh();
			}
			if (pActor.hasPlot())
			{
				World.world.plots.cancelPlot(pActor.plot);
			}
		});
		return true;
	}

	private void clearBadTraitsFrom(Actor pActor)
	{
		using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>();
		foreach (ActorTrait trait in pActor.getTraits())
		{
			if (trait.can_be_removed_by_divine_light)
			{
				listPool.Add(trait);
			}
		}
		if (listPool.Count > 0)
		{
			pActor.removeTraits(listPool);
			pActor.setStatsDirty();
			pActor.changeHappiness("just_felt_the_divine");
		}
	}

	private bool cleanBurnedTile(WorldTile pTile, string pPowerID)
	{
		pTile.removeBurn();
		return true;
	}

	private bool removeTornadoes(WorldTile pTile, string pPowerID)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		using ListPool<BaseEffect> listPool = new ListPool<BaseEffect>(World.world.stack_effects.get("fx_tornado").getList());
		if (listPool.Count == 0)
		{
			return false;
		}
		float num = 2 * (Config.current_brush_data.size + 1);
		num *= num;
		foreach (ref BaseEffect item in listPool)
		{
			BaseEffect current = item;
			if (current.active)
			{
				Vector3 localPosition = ((Component)current).transform.localPosition;
				if (!(Toolbox.SquaredDist(localPosition.x, localPosition.y, pTile.x, pTile.y) > num))
				{
					((TornadoEffect)current).die();
				}
			}
		}
		return true;
	}

	private bool drawPickaxe(WorldTile pTile, string pPowerID)
	{
		if (pTile.hasBuilding() && pTile.building.asset.building_type == BuildingType.Building_Mineral)
		{
			pTile.building.startDestroyBuilding();
		}
		if (pTile.Type.can_be_removed_with_pickaxe)
		{
			MapAction.decreaseTile(pTile, pDamage: false, "remove");
		}
		return true;
	}

	private bool drawBucket(WorldTile pTile, string pPowerID)
	{
		MapAction.removeLiquid(pTile);
		if (pTile.Type.lava)
		{
			MapAction.decreaseTile(pTile, pDamage: false);
		}
		if (pTile.Type.can_be_removed_with_bucket)
		{
			MapAction.decreaseTile(pTile, pDamage: false);
		}
		return true;
	}

	private bool drawAxe(WorldTile pTile, string pPowerID)
	{
		if (pTile.hasBuilding())
		{
			Building building = pTile.building;
			BuildingAsset asset = building.asset;
			if (asset.building_type == BuildingType.Building_Tree && !building.chopped)
			{
				if (asset.resources_given != null && pTile.hasCity())
				{
					foreach (ResourceContainer item in asset.resources_given)
					{
						pTile.zone_city.addResourcesToRandomStockpile(item.id, item.amount);
					}
				}
				building.chopTree();
			}
		}
		foreach (Actor item2 in Finder.getUnitsFromChunk(pTile, 0))
		{
			if (!(item2.kingdom.name != "living_plants"))
			{
				item2.a.getHitFullHealth(AttackType.Divine);
			}
		}
		if (pTile.Type.can_be_removed_with_axe)
		{
			MapAction.decreaseTile(pTile, pDamage: false, "remove");
		}
		return true;
	}

	private bool drawSpade(WorldTile pTile, string pPowerID)
	{
		if (pTile.Type.can_be_removed_with_spade)
		{
			MapAction.removeGreens(pTile);
		}
		return true;
	}

	private bool drawSickle(WorldTile pTile, string pPowerID)
	{
		if (pTile.hasBuilding())
		{
			BuildingType building_type = pTile.building.asset.building_type;
			if (building_type == BuildingType.Building_Fruits || (uint)(building_type - 5) <= 1u)
			{
				pTile.building.startDestroyBuilding();
			}
		}
		if (pTile.Type.can_be_removed_with_sickle)
		{
			MapAction.decreaseTile(pTile, pDamage: false, "remove");
		}
		return true;
	}

	private bool drawDemolish(WorldTile pTile, string pPowerID)
	{
		if (pTile.hasBuilding() && pTile.building.asset.can_be_demolished)
		{
			pTile.building.startDestroyBuilding();
		}
		if (pTile.Type.can_be_removed_with_demolish)
		{
			MapAction.decreaseTile(pTile, pDamage: false);
		}
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 0))
		{
			if (!(item.kingdom.name != "living_houses"))
			{
				item.a.getHitFullHealth(AttackType.Divine);
			}
		}
		return true;
	}

	private bool drawScissors(WorldTile pTile, string pPowerID)
	{
		if (pTile.zone.hasCity())
		{
			pTile.zone.city.removeZone(pTile.zone);
		}
		return true;
	}

	private bool drawLifeEraser(WorldTile pTile, string pPowerID)
	{
		MapAction.removeLifeFromTile(pTile);
		return true;
	}

	private bool drawHeatray(WorldTile pTile, string pPowerID)
	{
		if (World.world.heat_ray_fx.isReady())
		{
			World.world.heat.addTile(pTile, Randy.randomInt(1, 3));
		}
		return true;
	}

	[ClickActionCaller]
	private bool heatrayFX(WorldTile pTile, string pPowerID)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.heat_ray_fx.isReady())
		{
			MusicBox.inst.playDrawingSound("event:/SFX/POWERS/HeatRayMelts", pTile.x, pTile.y);
		}
		World.world.heat_ray_fx.play(Vector2Int.op_Implicit(pTile.pos), 10);
		loopWithBrush(pTile, pPowerID);
		return true;
	}

	[ClickActionCaller]
	private bool loopWithCurrentBrush(WorldTile pCenterTile, string pPowerID)
	{
		GodPower godPower = get(pPowerID);
		loopWithBrush(pCenterTile, godPower);
		if (godPower.surprises_units)
		{
			ActionLibrary.suprisedByArchitector(null, pCenterTile);
		}
		return true;
	}

	[ClickActionCaller]
	private bool drawingCursorEffect(WorldTile pTile, string pPowerID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		EffectsLibrary.spawnAt("fx_spark", pTile.posV3, 0.2f);
		return true;
	}

	private bool flashBrushPixelsDuringClick(WorldTile pCenterTile, string pPower)
	{
		BrushData current_brush_data = Config.current_brush_data;
		World.world.highlightTilesBrush(pCenterTile, current_brush_data, flashPixel);
		return true;
	}

	private bool flashBrushPixelsDuringClick(WorldTile pCenterTile, GodPower pPower)
	{
		BrushData current_brush_data = Config.current_brush_data;
		World.world.highlightTilesBrush(pCenterTile, current_brush_data, flashPixel, pPower);
		return true;
	}

	[ClickPowerActionCaller]
	private bool loopWithCurrentBrushPowerForDropsFull(WorldTile pCenterTile, GodPower pPower)
	{
		BrushData current_brush_data = Config.current_brush_data;
		WorldBehaviourTileEffects.checkTileForEffectKill(pCenterTile, current_brush_data.size);
		World.world.loopWithBrushPowerForDropsFull(pCenterTile, current_brush_data, pPower.click_power_action, pPower);
		return true;
	}

	[ClickPowerActionCaller]
	private bool loopWithCurrentBrushPowerForDropsRandom(WorldTile pCenterTile, GodPower pPower)
	{
		BrushData current_brush_data = Config.current_brush_data;
		WorldBehaviourTileEffects.checkTileForEffectKill(pCenterTile, current_brush_data.size);
		World.world.loopWithBrushPowerForDropsRandom(pCenterTile, current_brush_data, pPower.click_power_action, pPower);
		return true;
	}

	[ClickActionCaller]
	private bool loopWithBrush(WorldTile pCenterTile, string pPowerID)
	{
		GodPower pPower = get(pPowerID);
		return loopWithBrush(pCenterTile, pPower);
	}

	[ClickActionCaller]
	private bool loopWithBrush(WorldTile pCenterTile, GodPower pPower)
	{
		string pID = Config.current_brush;
		if (!string.IsNullOrEmpty(pPower.force_brush))
		{
			pID = pPower.force_brush;
		}
		BrushData brushData = Brush.get(pID);
		WorldBehaviourTileEffects.checkTileForEffectKill(pCenterTile, brushData.size);
		World.world.loopWithBrush(pCenterTile, brushData, pPower.click_action, pPower.id);
		return true;
	}

	private bool stopFire(WorldTile pTile, string pPowerID)
	{
		pTile.stopFire();
		if (pTile.hasBuilding() && pTile.building.hasStatus("burning"))
		{
			pTile.building.stopFire();
		}
		return true;
	}

	private bool fmodDrawingSound(WorldTile pTile, GodPower pPower)
	{
		if (pPower.has_sound_drawing)
		{
			MusicBox.inst.playDrawingSound(pPower.sound_drawing, pTile.x, pTile.y);
		}
		return true;
	}

	private bool fmodDrawingSound(WorldTile pTile, string pPowerID)
	{
		GodPower pPower = get(pPowerID);
		fmodDrawingSound(pTile, pPower);
		return true;
	}

	private bool destroyBuildings(WorldTile pTile, string pPowerID)
	{
		if (!pTile.hasBuilding())
		{
			return false;
		}
		pTile.building.startDestroyBuilding();
		return true;
	}

	private bool removeClouds(WorldTile pTile, string pPowerID)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEffect> list = World.world.stack_effects.get("fx_cloud").getList();
		float num = 10 * (Config.current_brush_data.size + 1);
		num *= num;
		for (int i = 0; i < list.Count; i++)
		{
			BaseEffect baseEffect = list[i];
			if (baseEffect.active)
			{
				Vector3 localPosition = ((Component)baseEffect).transform.localPosition;
				if (!(Toolbox.SquaredDist(localPosition.x, localPosition.y, pTile.x, pTile.y) > num))
				{
					((Cloud)baseEffect).startToDie();
				}
			}
		}
		return true;
	}

	private bool removeGoo(WorldTile pTile, string pPowerID)
	{
		if (pTile.Type.grey_goo)
		{
			MapAction.decreaseTile(pTile, pDamage: false);
		}
		return true;
	}

	private bool removeBuildingsBySponge(WorldTile pTile, string pPowerID)
	{
		if (!pTile.hasBuilding())
		{
			return false;
		}
		bool flag = false;
		if (pTile.building.isRuin() || pTile.building.asset.removed_by_sponge)
		{
			flag = true;
		}
		if (flag)
		{
			pTile.building.startDestroyBuilding();
		}
		return true;
	}

	public override void editorDiagnosticLocales()
	{
		foreach (GodPower item in list)
		{
			if (item.show_tool_sizes && !string.IsNullOrEmpty(item.force_brush))
			{
				BaseAssetLibrary.logAssetError("<e>PowerLibrary</e>: <b>show_tool_sizes</b> is enabled - but <b>force_brush</b> is set to <b>" + item.force_brush + "</b> - making the tool sizes useless", item.id);
			}
			if (item.show_tool_sizes && item.click_brush_action == null && item.click_power_brush_action == null)
			{
				BaseAssetLibrary.logAssetError("<e>PowerLibrary</e>: <b>show_tool_sizes</b> is enabled - but <b>click_brush_action</b> and <b>click_power_brush_action</b> are not set - making the tool sizes useless", item.id);
			}
		}
		localeChecks();
		callbackChecks();
		base.editorDiagnosticLocales();
	}

	private void localeChecks()
	{
		foreach (GodPower item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}

	private void callbackChecks()
	{
		foreach (GodPower item in list)
		{
			Delegate[] invocationList;
			if (item.click_action != null)
			{
				if (item.click_brush_action != null)
				{
					bool flag = false;
					invocationList = item.click_brush_action.GetInvocationList();
					for (int i = 0; i < invocationList.Length; i++)
					{
						if (invocationList[i].Method.GetCustomAttributes(typeof(ClickActionCallerAttribute), inherit: true).Length != 0)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						string text = item.click_action.AsString();
						string text2 = item.click_brush_action.AsString();
						BaseAssetLibrary.logAssetError("<e>PowerLibrary</e>: <b>click_brush_action</b> (" + text2 + ") overrides <b>click_action</b> (" + text + ") - either add <b>loopWithBrush</b> which will call them - or mark a similar caller method with [ClickActionCaller] attribute", item.id);
					}
				}
				invocationList = item.click_action.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (invocationList[i].Method.GetCustomAttributes(typeof(ClickActionCallerAttribute), inherit: true).Length != 0)
					{
						BaseAssetLibrary.logAssetError("<e>PowerLibrary</e>: <b>click_action</b> (" + item.click_action.AsString() + ") has [ClickActionCaller] attribute - it should be used only in <b>click_brush_action</b>", item.id);
					}
				}
			}
			if (item.click_power_action == null)
			{
				continue;
			}
			if (item.click_power_brush_action != null)
			{
				bool flag2 = false;
				invocationList = item.click_power_brush_action.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					if (invocationList[i].Method.GetCustomAttributes(typeof(ClickPowerActionCallerAttribute), inherit: true).Length != 0)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					string text3 = item.click_power_action.AsString();
					string text4 = item.click_power_brush_action.AsString();
					BaseAssetLibrary.logAssetError("<e>PowerLibrary</e>: <b>click_power_brush_action</b> (" + text4 + ") overrides <b>click_power_action</b> (" + text3 + ") - either add <b>loopWithCurrentBrushPower</b> which will call them - or mark a similar caller method with [ClickPowerActionCaller] attribute", item.id);
				}
			}
			invocationList = item.click_power_action.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i].Method.GetCustomAttributes(typeof(ClickPowerActionCallerAttribute), inherit: true).Length != 0)
				{
					BaseAssetLibrary.logAssetError("<e>PowerLibrary</e>: <b>click_power_action</b> (" + item.click_power_action.AsString() + ") has [ClickPowerActionCaller] attribute - it should be used only in <b>click_power_brush_action</b>", item.id);
				}
			}
		}
	}

	public string addToGameplayReport(string pWhat)
	{
		string empty = string.Empty;
		empty = empty + pWhat + "\n";
		foreach (GodPower item in list)
		{
			string translatedName = item.getTranslatedName();
			string translatedDescription = item.getTranslatedDescription();
			string text = "\n" + translatedName;
			text += "\n";
			if (!string.IsNullOrEmpty(translatedDescription))
			{
				text = text + "1: " + translatedDescription;
			}
			empty += text;
		}
		return empty + "\n\n";
	}
}
