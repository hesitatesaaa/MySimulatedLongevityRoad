using System.Collections.Generic;
using UnityEngine;

public class TileLibrary : TileLibraryMain<TileType>
{
	private TileType[] _depth_list_generator = new TileType[256];

	private TileType[] _depth_list_gameplay = new TileType[256];

	private TileType[] _depth_list;

	public static List<TileType> lava_types = new List<TileType>();

	public static TileType summit;

	public static TileType mountains;

	public static TileType hills;

	public static TileType grey_goo;

	public static TileType deep_ocean;

	public static TileType close_ocean;

	public static TileType shallow_waters;

	public static TileType sand;

	public static TileType soil_low;

	public static TileType soil_high;

	public static TileType lava0;

	public static TileType lava1;

	public static TileType lava2;

	public static TileType lava3;

	public static TileType pit_deep_ocean;

	public static TileType pit_close_ocean;

	public static TileType pit_shallow_waters;

	public static TileTypeBase[] array_tiles = new TileTypeBase[256];

	public override void init()
	{
		base.init();
		deep_ocean = add(new TileType
		{
			id = "deep_ocean",
			color_hex = "#3370CC",
			liquid = true,
			ocean = true,
			height_min = 0,
			decrease_to_id = "pit_deep_ocean",
			increase_to_id = "pit_close_ocean",
			walk_multiplier = 0.1f,
			strength = 0,
			layer_type = TileLayerType.Ocean,
			can_be_frozen = false,
			can_errode_to_sand = false
		});
		t.considered_empty_tile = true;
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.deep_ocean);
		t.render_z = 0;
		close_ocean = clone("close_ocean", "deep_ocean");
		t.considered_empty_tile = false;
		t.can_be_frozen = false;
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.close_ocean);
		t.drawPixel = true;
		t.color_hex = "#4084E2";
		t.height_min = 30;
		t.decrease_to_id = "pit_close_ocean";
		t.increase_to_id = "pit_shallow_waters";
		t.strength = 0;
		t.layer_type = TileLayerType.Ocean;
		t.can_errode_to_sand = false;
		shallow_waters = add(new TileType
		{
			id = "shallow_waters",
			drawPixel = true,
			can_be_frozen = true,
			color_hex = "#55AEF0",
			edge_color_hex = "#3F90EA",
			liquid = true,
			ocean = true,
			height_min = 70,
			freeze_to_id = "ice",
			decrease_to_id = "pit_shallow_waters",
			increase_to_id = "sand",
			walk_multiplier = 0.1f,
			strength = 0,
			layer_type = TileLayerType.Ocean,
			can_errode_to_sand = false,
			fast_freeze = true
		});
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.shallow_waters);
		pit_deep_ocean = clone("pit_deep_ocean", "deep_ocean");
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.pit_deep_ocean);
		t.drawPixel = true;
		t.color_hex = "#898989";
		t.liquid = false;
		t.ocean = false;
		t.walk_multiplier = 1f;
		t.can_be_filled_with_ocean = true;
		t.fill_to_ocean = "deep_ocean";
		t.water_fill_sound = "event:/SFX/NATURE/FillWaterTile";
		t.ground = true;
		t.decrease_to_id = string.Empty;
		t.increase_to_id = "pit_close_ocean";
		t.can_be_set_on_fire = true;
		t.layer_type = TileLayerType.Ground;
		t.strength = 2;
		t.considered_empty_tile = true;
		pit_close_ocean = clone("pit_close_ocean", "close_ocean");
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.pit_close_ocean);
		t.drawPixel = true;
		t.color_hex = "#A0A0A0";
		t.liquid = false;
		t.ocean = false;
		t.walk_multiplier = 1f;
		t.can_be_filled_with_ocean = true;
		t.fill_to_ocean = "close_ocean";
		t.water_fill_sound = "event:/SFX/NATURE/FillWaterTile";
		t.decrease_to_id = "pit_deep_ocean";
		t.increase_to_id = "pit_shallow_waters";
		t.can_be_set_on_fire = true;
		t.layer_type = TileLayerType.Ground;
		t.strength = 2;
		t.ground = true;
		pit_shallow_waters = clone("pit_shallow_waters", "shallow_waters");
		t.can_be_frozen = false;
		t.setDrawLayer(TileZIndexes.pit_shallow_waters);
		t.drawPixel = true;
		t.color_hex = "#C1C1C1";
		t.liquid = false;
		t.ocean = false;
		t.walk_multiplier = 1f;
		t.can_be_filled_with_ocean = true;
		t.fill_to_ocean = "shallow_waters";
		t.water_fill_sound = "event:/SFX/NATURE/FillWaterTile";
		t.decrease_to_id = "pit_close_ocean";
		t.increase_to_id = "sand";
		t.freeze_to_id = string.Empty;
		t.can_be_set_on_fire = true;
		t.layer_type = TileLayerType.Ground;
		t.ground = true;
		t.strength = 2;
		add(new TileType
		{
			id = "border_pit",
			layer_type = TileLayerType.Ground,
			can_be_autotested = false
		});
		t.setDrawLayer(TileZIndexes.border_pit);
		add(new TileType
		{
			id = "border_water",
			layer_type = TileLayerType.Ground,
			can_be_autotested = false
		});
		t.setDrawLayer(TileZIndexes.border_water);
		add(new TileType
		{
			id = "border_water_runup",
			layer_type = TileLayerType.Ground,
			can_be_autotested = false
		});
		t.setDrawLayer(TileZIndexes.border_water_runup);
		sand = add(new TileType
		{
			cost = 116,
			biome_build_check = true,
			id = "sand",
			sand = true,
			drawPixel = true,
			color_hex = "#F7E898",
			edge_color_hex = "#D8C08C",
			height_min = 98,
			decrease_to_id = "pit_shallow_waters",
			increase_to_id = "soil_low",
			ground = true,
			walk_multiplier = 0.5f,
			freeze_to_id = "snow_sand",
			creep_rank_type = TileRank.Low,
			can_be_set_on_fire = true,
			can_build_on = true,
			can_be_farm = true
		});
		t.ignore_walk_multiplier_if_tag = "walk_adaptation_sand";
		t.setBiome("biome_sand");
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.sand);
		t.biome_tags = AssetLibrary<TileType>.h<BiomeTag>(BiomeTag.Sand);
		soil_low = add(new TileType
		{
			cost = 115,
			drawPixel = true,
			id = "soil_low",
			color_hex = "#E2934B",
			height_min = 108,
			decrease_to_id = "sand",
			increase_to_id = "soil_high",
			ground = true,
			can_be_biome = true,
			soil = true,
			freeze_to_id = "frozen_low",
			rank_type = TileRank.Low,
			creep_rank_type = TileRank.Low,
			can_be_farm = true,
			can_build_on = true,
			can_be_set_on_fire = true,
			used_in_generator = true,
			food_resource = "worms",
			biome_build_check = true
		});
		t.setDrawLayer(TileZIndexes.soil_low);
		t.biome_tags = AssetLibrary<TileType>.h<BiomeTag>(BiomeTag.Soil);
		soil_high = add(new TileType
		{
			cost = 120,
			drawPixel = true,
			id = "soil_high",
			color_hex = "#B66F3A",
			height_min = 128,
			additional_height = new int[8] { 15, 16, 17, 14, 13, 12, 11, 10 },
			decrease_to_id = "soil_low",
			increase_to_id = "hills",
			ground = true,
			rank_type = TileRank.High,
			creep_rank_type = TileRank.High,
			can_be_biome = true,
			soil = true,
			freeze_to_id = "frozen_high",
			can_be_farm = true,
			can_build_on = true,
			can_be_set_on_fire = true,
			used_in_generator = true,
			food_resource = "worms",
			biome_build_check = true
		});
		t.setDrawLayer(TileZIndexes.soil_high);
		t.biome_tags = AssetLibrary<TileType>.h<BiomeTag>(BiomeTag.Soil);
		lava0 = add(new TileType
		{
			cost = 100,
			drawPixel = true,
			id = "lava0",
			color_hex = "#F62D14",
			decrease_to_id = "sand",
			increase_to_id = "hills",
			liquid = true,
			walk_multiplier = 0.2f,
			damage_units = true,
			damage = 150,
			lava = true,
			lava_level = 0,
			strength = 0,
			layer_type = TileLayerType.Lava,
			can_be_frozen = false,
			material = "mat_world_object_lit"
		});
		t.lava_increase = "lava1";
		t.lava_change_state_after = 30;
		t.step_action = TileActionLibrary.setUnitOnFire;
		t.step_action_chance = 0.9f;
		t.setDrawLayer(TileZIndexes.lava0);
		lava1 = clone("lava1", "lava0");
		t.setDrawLayer(TileZIndexes.lava1);
		t.color_hex = "#FF6700";
		t.step_action = TileActionLibrary.setUnitOnFire;
		t.step_action_chance = 0.9f;
		t.lava_level = 1;
		t.lava_decrease = "lava0";
		t.lava_increase = "lava2";
		t.lava_change_state_after = 10;
		lava2 = clone("lava2", "lava0");
		t.setDrawLayer(TileZIndexes.lava2);
		t.color_hex = "#FFAC00";
		t.step_action = TileActionLibrary.setUnitOnFire;
		t.step_action_chance = 0.9f;
		t.lava_level = 2;
		t.lava_decrease = "lava1";
		t.lava_increase = "lava3";
		t.lava_change_state_after = 10;
		lava3 = clone("lava3", "lava0");
		t.setDrawLayer(TileZIndexes.lava3);
		t.color_hex = "#FFDE00";
		t.step_action = TileActionLibrary.setUnitOnFire;
		t.step_action_chance = 0.9f;
		t.lava_level = 3;
		t.lava_decrease = "lava2";
		t.lava_increase = string.Empty;
		t.lava_change_state_after = 3;
		hills = add(new TileType
		{
			cost = 140,
			drawPixel = true,
			id = "hills",
			color_hex = "#5B5E5C",
			height_min = 199,
			rocks = true,
			ground = true,
			edge_hills = true,
			additional_height = new int[2] { 2, -6 },
			decrease_to_id = "soil_high",
			increase_to_id = "mountains",
			freeze_to_id = "snow_hills",
			can_be_set_on_fire = true
		});
		t.setBiome("biome_hill");
		t.biome_tags = AssetLibrary<TileType>.h<BiomeTag>(BiomeTag.Hills);
		t.hold_lava = true;
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.hills);
		mountains = add(new TileType
		{
			cost = 160,
			drawPixel = true,
			id = "mountains",
			color_hex = "#414545",
			height_min = 210,
			rocks = true,
			mountains = true,
			edge_mountains = true,
			additional_height = new int[2] { 2, 4 },
			decrease_to_id = "hills",
			increase_to_id = "summit",
			walk_multiplier = 0.5f,
			freeze_to_id = "snow_block",
			can_be_set_on_fire = true,
			layer_type = TileLayerType.Block,
			block = true,
			block_height = 3f,
			force_edge_variation = true,
			force_edge_variation_frame = 2
		});
		t.hold_lava = true;
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.mountains);
		summit = add(new TileType
		{
			cost = 160,
			drawPixel = true,
			id = "summit",
			color_hex = "#333333",
			height_min = 230,
			rocks = true,
			mountains = true,
			edge_mountains = true,
			additional_height = new int[2] { 2, 4 },
			decrease_to_id = "mountains",
			walk_multiplier = 0.5f,
			freeze_to_id = "snow_summit",
			can_be_set_on_fire = true,
			layer_type = TileLayerType.Block,
			block = true,
			block_height = 5f,
			force_edge_variation = true,
			force_edge_variation_frame = 2
		});
		t.summit = true;
		t.hold_lava = true;
		t.used_in_generator = true;
		t.setDrawLayer(TileZIndexes.summit);
		grey_goo = add(new TileType
		{
			cost = 10,
			drawPixel = true,
			grey_goo = true,
			id = "grey_goo",
			color_hex = "#5D6191",
			decrease_to_id = "pit_deep_ocean",
			burnable = true,
			ground = false,
			walk_multiplier = 0.1f,
			damage_units = true,
			damage = 200,
			strength = 0,
			life = true,
			can_be_frozen = false,
			layer_type = TileLayerType.Goo
		});
		t.setDrawLayer(TileZIndexes.grey_goo);
		lava_types = new List<TileType> { lava0, lava1, lava2, lava3 };
	}

	private void loadTileSprites()
	{
		foreach (TileType item in list)
		{
			loadSpritesForTile(item);
		}
	}

	private void loadSpritesForTile(TileType pType)
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

	public TileType getGen(string pID)
	{
		if (!dict.ContainsKey(pID))
		{
			return null;
		}
		return dict[pID];
	}

	public override TileType add(TileType pAsset)
	{
		pAsset.index_id = TileTypeBase.last_index_id++;
		array_tiles[pAsset.index_id] = pAsset;
		return base.add(pAsset);
	}

	public override void linkAssets()
	{
		base.linkAssets();
		using ListPool<TileType> listPool = new ListPool<TileType>();
		foreach (TileType item in list)
		{
			if (item.used_in_generator)
			{
				listPool.Add(item);
			}
		}
		setListTo(DepthGeneratorType.Generator);
		for (int i = 0; i < _depth_list_generator.Length; i++)
		{
			_depth_list_generator[i] = getTypeByDepth(i, listPool);
		}
		setListTo(DepthGeneratorType.Gameplay);
		for (int j = 0; j < _depth_list_gameplay.Length; j++)
		{
			_depth_list_gameplay[j] = getTypeByDepth(j, list);
		}
		setListTo(DepthGeneratorType.Generator);
		foreach (TileType item2 in list)
		{
			item2.decrease_to = getGen(item2.decrease_to_id);
			item2.increase_to = getGen(item2.increase_to_id);
		}
		loadTileSprites();
		foreach (TileType item3 in list)
		{
			if (!string.IsNullOrEmpty(item3.biome_id))
			{
				item3.biome_asset = AssetManager.biome_library.get(item3.biome_id);
			}
		}
	}

	public void setListTo(DepthGeneratorType pVal)
	{
		switch (pVal)
		{
		case DepthGeneratorType.Generator:
			_depth_list = _depth_list_generator;
			break;
		case DepthGeneratorType.Gameplay:
			_depth_list = _depth_list_gameplay;
			break;
		}
	}

	public TileType getTypeByDepth(int pHeight, IReadOnlyList<TileType> pList)
	{
		TileType tileType = null;
		for (int i = 0; i < pList.Count; i++)
		{
			TileType tileType2 = pList[i];
			if (tileType2.height_min != -1 && (tileType == null || pHeight >= tileType2.height_min))
			{
				tileType = tileType2;
			}
		}
		return tileType;
	}

	public override TileType clone(string pNew, string pFrom)
	{
		TileType tileType = base.clone(pNew, pFrom);
		tileType.can_be_farm = false;
		tileType.used_in_generator = false;
		return tileType;
	}

	public TileType getTypeByDepth(WorldTile pWorldTile)
	{
		return _depth_list[pWorldTile.Height];
	}
}
