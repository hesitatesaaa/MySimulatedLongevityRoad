using System.Collections.Generic;

public class TileEffectsLibrary : AssetLibrary<TileEffectAsset>
{
	private static Dictionary<string, List<TileEffectAsset>> _dict_effects = new Dictionary<string, List<TileEffectAsset>>();

	public override void init()
	{
		base.init();
		add(new TileEffectAsset
		{
			id = "wave_normal",
			rate = 100,
			chance = 1f,
			path_sprite = "effects/tile_effects/waves/wave_normal"
		});
		t.addTileTypes("close_ocean", "deep_ocean", "shallow_waters");
		add(new TileEffectAsset
		{
			id = "wave_mermaid",
			chance = 0.05f,
			path_sprite = "effects/tile_effects/waves/wave_mermaid"
		});
		t.addTileTypes("close_ocean", "deep_ocean");
		add(new TileEffectAsset
		{
			id = "wave_dolphin",
			chance = 0.05f,
			path_sprite = "effects/tile_effects/waves/wave_dolphin"
		});
		t.addTileTypes("close_ocean", "deep_ocean");
		add(new TileEffectAsset
		{
			id = "wave_tentacle",
			chance = 0.05f,
			path_sprite = "effects/tile_effects/waves/wave_tentacle"
		});
		t.addTileTypes("close_ocean", "deep_ocean");
		add(new TileEffectAsset
		{
			id = "wave_shark_fin",
			chance = 0.05f,
			path_sprite = "effects/tile_effects/waves/wave_shark_fin"
		});
		t.addTileTypes("close_ocean", "deep_ocean");
		add(new TileEffectAsset
		{
			id = "wave_bubbles",
			chance = 0.05f,
			path_sprite = "effects/tile_effects/waves/wave_bubbles"
		});
		t.addTileTypes("close_ocean", "deep_ocean", "shallow_waters");
		add(new TileEffectAsset
		{
			id = "enchanted_sparkle",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/enchanted_sparkle"
		});
		t.addTileTypes("enchanted_high", "enchanted_low");
		add(new TileEffectAsset
		{
			id = "paradox_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/paradox_effect"
		});
		t.addTileTypes("paradox_high", "paradox_low");
		add(new TileEffectAsset
		{
			id = "desert_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/desert_effect"
		});
		t.addTileTypes("desert_high", "desert_low");
		add(new TileEffectAsset
		{
			id = "celestial_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/celestial_effect"
		});
		t.addTileTypes("celestial_low", "celestial_high");
		add(new TileEffectAsset
		{
			id = "singularity_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/singularity_effect"
		});
		t.addTileTypes("singularity_high", "singularity_low");
		add(new TileEffectAsset
		{
			id = "corrupted_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/corrupted_effect"
		});
		t.addTileTypes("corrupted_low", "corrupted_high");
		add(new TileEffectAsset
		{
			id = "infernal_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/infernal_effect"
		});
		t.addTileTypes("infernal_low", "infernal_high");
		add(new TileEffectAsset
		{
			id = "wind_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/wind_effect"
		});
		t.addTileTypes("savanna_high", "savanna_low");
		add(new TileEffectAsset
		{
			id = "birch_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/birch_effect"
		});
		t.addTileTypes("birch_high", "birch_low");
		add(new TileEffectAsset
		{
			id = "maple_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/maple_effect"
		});
		t.addTileTypes("maple_high", "maple_low");
		add(new TileEffectAsset
		{
			id = "lava_effect",
			chance = 0.8f,
			path_sprite = "effects/tile_effects/lava_effect"
		});
		t.addTileTypes("lava3", "lava2");
		add(new TileEffectAsset
		{
			id = "permafrost_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/permafrost_effect"
		});
		t.addTileTypes("permafrost_high", "permafrost_low");
		add(new TileEffectAsset
		{
			id = "swamp_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/swamp_effect"
		});
		t.addTileTypes("swamp_high", "swamp_low");
		add(new TileEffectAsset
		{
			id = "sand_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/desert_effect"
		});
		t.addTileTypes("sand");
		add(new TileEffectAsset
		{
			id = "mountain_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/wind_effect"
		});
		t.addTileTypes("hills", "mountains");
		add(new TileEffectAsset
		{
			id = "soil_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/wind_effect"
		});
		t.addTileTypes("soil_high", "soil_low");
		add(new TileEffectAsset
		{
			id = "clover_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/clover_effect"
		});
		t.addTileTypes("clover_high", "clover_low");
		add(new TileEffectAsset
		{
			id = "garlic_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/infernal_effect"
		});
		t.addTileTypes("garlic_high", "garlic_low");
		add(new TileEffectAsset
		{
			id = "flower_effect",
			chance = 0.1f,
			path_sprite = "effects/tile_effects/flower_effect"
		});
		t.addTileTypes("flower_high", "flower_low");
		add(new TileEffectAsset
		{
			id = "crystal_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/crystal_effect"
		});
		t.addTileTypes("crystal_high", "crystal_low");
		add(new TileEffectAsset
		{
			id = "jungle_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/jungle_effect"
		});
		t.addTileTypes("jungle_high", "jungle_low");
		add(new TileEffectAsset
		{
			id = "mushroom_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/mushroom_effect"
		});
		t.addTileTypes("mushroom_high", "mushroom_low");
		add(new TileEffectAsset
		{
			id = "lemon_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/lemon_effect"
		});
		t.addTileTypes("lemon_high", "lemon_low");
		add(new TileEffectAsset
		{
			id = "candy_effect",
			chance = 0.1f,
			path_sprite = "effects/tile_effects/candy_effect"
		});
		t.addTileTypes("candy_high", "candy_low");
		add(new TileEffectAsset
		{
			id = "grass_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/birch_effect"
		});
		t.addTileTypes("grass_high", "grass_low");
		add(new TileEffectAsset
		{
			id = "wasteland_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/wasteland_effect"
		});
		t.addTileTypes("wasteland_high", "wasteland_low");
		add(new TileEffectAsset
		{
			id = "rocklands_effect",
			chance = 0.3f,
			path_sprite = "effects/tile_effects/rocklands_effect"
		});
		t.addTileTypes("rocklands_high", "rocklands_low");
	}

	public override void linkAssets()
	{
		base.linkAssets();
		fillPool();
	}

	private void fillPool()
	{
		foreach (TileEffectAsset item in list)
		{
			foreach (string tile_type in item.tile_types)
			{
				addToDict(tile_type, item);
			}
		}
	}

	private void addToDict(string pTileTypeID, TileEffectAsset pAsset)
	{
		if (!_dict_effects.TryGetValue(pTileTypeID, out var value))
		{
			value = new List<TileEffectAsset>();
			_dict_effects.Add(pTileTypeID, value);
		}
		value.AddTimes(pAsset.rate, pAsset);
	}

	public static TileEffectAsset getRandomEffect(WorldTile pTile)
	{
		if (!_dict_effects.TryGetValue(pTile.Type.id, out var value))
		{
			return null;
		}
		return value.GetRandom();
	}
}
