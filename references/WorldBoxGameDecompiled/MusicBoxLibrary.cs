using System.Collections.Generic;

public class MusicBoxLibrary : AssetLibrary<MusicAsset>
{
	internal readonly List<MusicBoxContainerTiles> c_list_params = new List<MusicBoxContainerTiles>();

	internal readonly Dictionary<string, MusicBoxContainerCivs> c_dict_civs = new Dictionary<string, MusicBoxContainerCivs>();

	internal readonly List<MusicBoxContainerTiles> c_list_environments = new List<MusicBoxContainerTiles>();

	internal readonly Dictionary<string, MusicBoxContainerUnits> c_dict_units = new Dictionary<string, MusicBoxContainerUnits>();

	public static MusicAsset New_World;

	public static MusicAsset Menu;

	public static MusicAsset Neutral_001;

	public static MusicAsset Locations_Desert;

	public override void init()
	{
		base.init();
		Menu = add(new MusicAsset
		{
			id = "Menu",
			disable_param_after_start = true
		});
		Neutral_001 = add(new MusicAsset
		{
			id = "Neutral_001",
			disable_param_after_start = true
		});
		New_World = add(new MusicAsset
		{
			id = "New_World",
			disable_param_after_start = true
		});
		add(new MusicAsset
		{
			id = "wea_rain"
		});
		add(new MusicAsset
		{
			id = "wea_snow"
		});
		addUnits();
		addUnique();
		addLoc();
		addEnv();
	}

	private void addEnv()
	{
		add(new MusicAsset
		{
			id = "LavaEnvironment",
			fmod_path = "event:/SFX/ENVIRONMENT/LavaEnvironment",
			is_environment = true,
			is_param = true,
			min_tiles_to_play = 30
		});
		t.setTileTypes("lava0", "lava1", "lava2", "lava3");
		add(new MusicAsset
		{
			id = "MapEnvironment",
			fmod_path = "event:/SFX/ENVIRONMENT/MapEnvironment",
			is_environment = true,
			is_param = true,
			mini_map_only = true
		});
		add(new MusicAsset
		{
			id = "Ocean",
			fmod_path = "event:/SFX/ENVIRONMENT/Ocean",
			is_environment = true,
			is_param = true,
			min_tiles_to_play = 100
		});
		t.setTileTypes("deep_ocean");
		add(new MusicAsset
		{
			id = "Sea",
			fmod_path = "event:/SFX/ENVIRONMENT/Sea",
			is_param = true,
			is_environment = true,
			min_tiles_to_play = 100
		});
		t.setTileTypes("close_ocean");
	}

	private void addLoc()
	{
		add(new MusicAsset
		{
			id = "Locations_Forest",
			is_param = true
		});
		t.setTileTypes("grass_high", "grass_low", "enchanted_low", "enchanted_high");
		Locations_Desert = add(new MusicAsset
		{
			id = "Locations_Desert",
			is_param = true
		});
		add(new MusicAsset
		{
			id = "Locations_Mountains",
			is_param = true
		});
		t.setTileTypes("hills", "mountains");
		add(new MusicAsset
		{
			id = "Locations_Ocean",
			is_param = true
		});
		t.setTileTypes("deep_ocean", "close_ocean");
		add(new MusicAsset
		{
			id = "Locations_Snow",
			is_param = true
		});
		t.setTileTypes("snow_block", "permafrost_high", "snow_hills", "permafrost_low", "snow_sand");
	}

	private void addUnique()
	{
		add(new MusicAsset
		{
			id = "Crabzilla",
			is_unit_param = true,
			special_delegate_units = delegate(MusicBoxContainerUnits pContainer)
			{
				if (ControllableUnit.isControllingCrabzilla())
				{
					pContainer.units = 1;
				}
			},
			priority = MusicLayerPriority.High
		});
		add(new MusicAsset
		{
			id = "GreyGoo",
			is_unit_param = true,
			special_delegate_units = delegate(MusicBoxContainerUnits pContainer)
			{
				if (World.world.grey_goo_layer.isActive())
				{
					if (!WorldLawLibrary.world_law_gaias_covenant.isEnabled() || MapBox.isRenderMiniMap())
					{
						pContainer.units = 1;
					}
					else
					{
						List<TileZone> visibleZones = World.world.zone_camera.getVisibleZones();
						for (int i = 0; i < visibleZones.Count; i++)
						{
							if (visibleZones[i].hasTilesOfType(TileLibrary.grey_goo))
							{
								pContainer.units = 1;
								break;
							}
						}
					}
				}
			},
			priority = MusicLayerPriority.Medium
		});
		add(new MusicAsset
		{
			id = "Unique_ZombieInfection",
			is_unit_param = true,
			special_delegate_units = delegate
			{
			},
			priority = MusicLayerPriority.Medium
		});
	}

	private void addUnits()
	{
		add(new MusicAsset
		{
			id = "_units_param",
			is_unit_param = true
		});
		clone("Units_Bandits", "_units_param");
		clone("Units_Bear", "_units_param");
		clone("Units_BeeHive", "_units_param");
		clone("Units_Cat", "_units_param");
		clone("Units_Chicken", "_units_param");
		clone("Units_ColdOne", "_units_param");
		clone("Units_Demon", "_units_param");
		clone("Units_Fairy", "_units_param");
		clone("Units_LivingPlants", "_units_param");
		clone("Units_Piranha", "_units_param");
		clone("Units_Rabbit", "_units_param");
		clone("Units_Rat", "_units_param");
		clone("Units_Skeleton", "_units_param");
		clone("Units_Sheep", "_units_param");
		clone("Units_Snowman", "_units_param");
		clone("Units_Wolf", "_units_param");
		clone("Units_Worm", "_units_param");
		clone("Units_Zombie", "_units_param");
		clone("Buildings_Tumor", "_units_param");
		clone("Humans_Neutral", "_units_param");
		clone("Elves_Neutral", "_units_param");
		clone("Orcs_Neutral", "_units_param");
		clone("Dwarves_Neutral", "_units_param");
		add(new MusicAsset
		{
			id = "Units_GodFinger",
			is_unit_param = true,
			special_delegate_units = special_god_finger
		});
		add(new MusicAsset
		{
			id = "Units_Dragon",
			is_unit_param = true,
			special_delegate_units = special_dragon
		});
		add(new MusicAsset
		{
			id = "Units_Santa",
			is_unit_param = true
		});
		add(new MusicAsset
		{
			id = "Units_UFO",
			is_unit_param = true,
			special_delegate_units = special_ufo
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		foreach (MusicAsset item in list)
		{
			if (item.civilization)
			{
				addCivContainer(item);
			}
			if (item.is_param)
			{
				addTileContainer(item);
			}
			if (item.is_unit_param)
			{
				addUnitContainer(item);
			}
			string[] tile_type_strings = item.tile_type_strings;
			if (tile_type_strings == null || tile_type_strings.Length == 0)
			{
				continue;
			}
			using ListPool<TileTypeBase> listPool = new ListPool<TileTypeBase>(item.tile_type_strings.Length);
			string[] tile_type_strings2 = item.tile_type_strings;
			foreach (string text in tile_type_strings2)
			{
				if (AssetManager.top_tiles.has(text))
				{
					listPool.Add(AssetManager.top_tiles.get(text));
				}
				else if (AssetManager.tiles.has(text))
				{
					listPool.Add(AssetManager.tiles.get(text));
				}
				else
				{
					BaseAssetLibrary.logAssetError("MusicBoxLibrary: No matching Tile Type found for", text);
				}
			}
			item.tile_types = listPool.ToArray();
		}
		foreach (MusicAsset item2 in list)
		{
			if (item2.tile_types == null)
			{
				continue;
			}
			TileTypeBase[] tile_types = item2.tile_types;
			for (int i = 0; i < tile_types.Length; i++)
			{
				TileTypeBase tileTypeBase2;
				TileTypeBase tileTypeBase = (tileTypeBase2 = tile_types[i]);
				if (tileTypeBase2.music_assets == null)
				{
					tileTypeBase2.music_assets = new List<MusicAsset>();
				}
				tileTypeBase.music_assets.Add(item2);
			}
		}
	}

	private MusicBoxContainerUnits addUnitContainer(MusicAsset pAsset)
	{
		MusicBoxContainerUnits musicBoxContainerUnits = new MusicBoxContainerUnits();
		c_dict_units.Add(pAsset.id, musicBoxContainerUnits);
		musicBoxContainerUnits.asset = pAsset;
		return musicBoxContainerUnits;
	}

	private MusicBoxContainerCivs addCivContainer(MusicAsset pAsset)
	{
		MusicBoxContainerCivs musicBoxContainerCivs = new MusicBoxContainerCivs();
		c_dict_civs.Add(pAsset.id, musicBoxContainerCivs);
		musicBoxContainerCivs.asset = pAsset;
		return musicBoxContainerCivs;
	}

	private MusicBoxContainerTiles addTileContainer(MusicAsset pAsset)
	{
		MusicBoxContainerTiles musicBoxContainerTiles = new MusicBoxContainerTiles();
		musicBoxContainerTiles.asset = pAsset;
		pAsset.container_tiles = musicBoxContainerTiles;
		c_list_params.Add(musicBoxContainerTiles);
		if (pAsset.is_environment)
		{
			c_list_environments.Add(musicBoxContainerTiles);
		}
		return musicBoxContainerTiles;
	}

	private void special_god_finger(MusicBoxContainerUnits pContainer)
	{
		Kingdom kingdom = World.world.kingdoms_wild.get("godfinger");
		pContainer.units += kingdom.units.Count;
	}

	private void special_dragon(MusicBoxContainerUnits pContainer)
	{
		Kingdom kingdom = World.world.kingdoms_wild.get("dragons");
		pContainer.units += kingdom.units.Count;
	}

	private void special_ufo(MusicBoxContainerUnits pContainer)
	{
		ActorAsset actorAsset = AssetManager.actor_library.get("UFO");
		pContainer.units += actorAsset.units.Count;
	}
}
