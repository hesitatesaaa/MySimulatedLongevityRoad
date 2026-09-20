using System.Collections.Generic;
using UnityEngine;

public class TrailerMonolith : MonoBehaviour
{
	public static readonly bool enable_trailer_stuff;

	public bool enabled_auto = true;

	public AudioSource audio_source;

	private GameObject camera_object;

	private string[] _biomes = new string[21]
	{
		"biome_savanna", "biome_grass", "biome_infernal", "biome_crystal", "biome_lemon", "biome_singularity", "biome_garlic", "biome_clover", "biome_candy", "biome_permafrost",
		"biome_desert", "biome_swamp", "biome_maple", "biome_birch", "biome_flower", "biome_paradox", "biome_mushroom", "biome_rocklands", "biome_enchanted", "biome_corrupted",
		"biome_jungle"
	};

	private string[] _unit_assets_to_spawn = new string[9] { "demon", "cold_one", "sheep", "angle", "skeleton", "evil_mage", "white_mage", "alien", "necromancer" };

	private int[] _keys = new int[16]
	{
		0, 4, 8, 10, 11, 13, 16, 24, 32, 36,
		40, 42, 43, 45, 48, 56
	};

	private double[] _keys_timings;

	private double[] _drums;

	private int _current_biome;

	private const double INTERVAL_DANCING_TREES = 0.46153125166893005;

	private double _timer_dancing_trees;

	private const double INTERVAL_LOOP = 29.538000106811523;

	private double _timer_song;

	private double offset_timings = -0.15000000596046448;

	private double _last_offset;

	public double track_time;

	private HashSet<int> _processed_keys = new HashSet<int>();

	private HashSet<int> _processed_drums = new HashSet<int>();

	private HashSet<Building> _processed_buildings = new HashSet<Building>();

	public bool reset;

	public bool transition;

	public static int harp_frame_index;

	private const int HARP_MAX_FRAMES = 19;

	private bool _camera_go_zoom = true;

	private float _camera_switch_timer = 10f;

	private const int MAX_WAVE = 5;

	private int _tree_wave;

	public void Start()
	{
		camera_object = ((Component)Camera.main).gameObject;
		calculateTimings();
		camera_object.AddComponent<AudioListener>();
		DebugConfig.setOption(DebugOption.ArrowsUnitsAttackTargets, pVal: false);
		_drums = new double[64];
		for (int i = 0; i < _drums.Length; i++)
		{
			_drums[i] = (double)i * 29.538000106811523 / 64.0;
		}
	}

	private void newLoop()
	{
		_processed_keys.Clear();
		_processed_drums.Clear();
		_processed_buildings.Clear();
		resetDancingTrees();
		double num = 0.0;
		if (_timer_song > 29.538000106811523)
		{
			num = _timer_song - 29.538000106811523;
		}
		_timer_song = 0.0 + num;
	}

	private void resetTrack()
	{
		audio_source.Stop();
		audio_source.time = 0f;
		audio_source.Play();
	}

	private void calculateTimings()
	{
		_last_offset = offset_timings;
		_keys_timings = new double[_keys.Length];
		for (int i = 0; i < _keys.Length; i++)
		{
			_keys_timings[i] = (double)_keys[i] * 29.538000106811523 / 64.0 + offset_timings;
		}
	}

	private void resetDancingTrees()
	{
		double num = ((!(_timer_dancing_trees > 0.46153125166893005)) ? 0.0 : (_timer_dancing_trees - 0.46153125166893005));
		_timer_dancing_trees = 0.0 + num;
	}

	public void Update()
	{
		if (Config.worldLoading || !enabled_auto)
		{
			return;
		}
		if (Input.GetKeyDown((KeyCode)114))
		{
			reset = true;
		}
		if (World.world.isPaused())
		{
			return;
		}
		if (_last_offset != offset_timings)
		{
			calculateTimings();
		}
		if (Time.frameCount % 5 == 0 && harp_frame_index < 19)
		{
			harp_frame_index++;
		}
		track_time = _timer_song;
		if (reset)
		{
			reset = false;
			resetTrack();
			newLoop();
			World.world.move_camera.forceZoom(30f);
			_camera_switch_timer = 2f;
		}
		updateCamera();
		if (_timer_song < 29.538000106811523)
		{
			for (int i = 0; i < _drums.Length; i++)
			{
				if (!_processed_drums.Contains(i) && _timer_song >= _drums[i])
				{
					_processed_drums.Add(i);
					dancingTrees();
				}
			}
			for (int j = 0; j < _keys_timings.Length; j++)
			{
				if (!_processed_keys.Contains(j) && _timer_song >= _keys_timings[j])
				{
					glowMonolith(j);
					spawnRandomUnit();
					spawnRandomUnit();
					spawnRandomUnit();
					spawnRandomLightning();
					spawnRandomLightning();
					spawnRandomLightning();
					spawnRandomLightning();
					_processed_keys.Add(j);
					if (j == 8 || j == 14 || j == 6)
					{
						doMonolithAction();
						switchBiome();
						spawnRandoMTornado();
					}
				}
			}
		}
		if (_timer_song < 29.538000106811523 && !transition)
		{
			_timer_song += Time.deltaTime;
		}
		else
		{
			newLoop();
			doMonolithAction();
			switchBiome();
		}
		if (transition)
		{
			transition = false;
		}
	}

	private void updateCamera()
	{
		World.world.move_camera.camera_zoom_speed = 0.2f;
		if (_camera_switch_timer > 0f)
		{
			_camera_switch_timer -= Time.deltaTime;
		}
		else
		{
			_camera_switch_timer = 10f;
			_camera_go_zoom = !_camera_go_zoom;
		}
		if (_camera_go_zoom)
		{
			World.world.move_camera.setTargetZoom(30f);
		}
		else
		{
			World.world.move_camera.setTargetZoom(60f);
		}
	}

	private void spawnRandoMTornado()
	{
		WorldTile random = TopTileLibrary.wall_ancient.getCurrentTiles().GetRandom();
		EffectsLibrary.spawnAtTile("fx_tornado", random, 0.125f);
	}

	private void spawnRandomLightning()
	{
		MapBox.spawnLightningSmall(World.world.islands_calculator.tryGetRandomGround());
	}

	private void doMonolithAction()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Building building = findMonolith();
		EffectsLibrary.spawnAt("fx_monolith_launch_bottom", building.current_tile.posV3, building.current_scale.y);
		EffectsLibrary.spawnAt("fx_monolith_launch", building.current_tile.posV3, building.current_scale.y);
	}

	private void spawnRandomUnit()
	{
		string random = _unit_assets_to_spawn.GetRandom();
		WorldTile random2 = TileLibrary.hills.getCurrentTiles().GetRandom();
		bool pMiracleSpawn = Randy.randomChance(0.8f);
		World.world.units.spawnNewUnit(random, random2, pSpawnSound: false, pMiracleSpawn);
		EffectsLibrary.spawn("fx_spawn", random2);
	}

	private void glowMonolith(int pIndex)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		foreach (Actor unit in World.world.units)
		{
			unit.makeStunned(1f);
			unit.applyRandomForce();
		}
		Building building = findMonolith();
		if (building == null)
		{
			return;
		}
		if (pIndex == 5 || pIndex == 9 || pIndex == 13)
		{
			EffectsLibrary.spawnAt("fx_monolith_glow_1", building.current_tile.posV3, building.current_scale.y);
		}
		else
		{
			EffectsLibrary.spawnAt("fx_monolith_glow_2", building.current_tile.posV3, building.current_scale.y);
		}
		harp_frame_index = 11;
		foreach (Building building2 in World.world.buildings)
		{
			if (!(building2.asset.id == "monolith") && !(building2.asset.id == "waypoint_harp"))
			{
				building2.startShake(0.5f);
			}
		}
	}

	private Building findMonolith()
	{
		foreach (Building building in World.world.buildings)
		{
			if (!(building.asset.id != "monolith"))
			{
				building.setMaxHealth();
				return building;
			}
		}
		return null;
	}

	private void dancingTrees()
	{
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		Building building = findMonolith();
		if (building == null)
		{
			return;
		}
		foreach (Building building2 in World.world.buildings)
		{
			if (!(building2.asset.id == "monolith") && !(building2.asset.id == "waypoint_harp"))
			{
				building2.setScaleTween(0.9f);
			}
		}
		foreach (Building building3 in World.world.buildings)
		{
			if (!(building3.asset.id != "waypoint_harp"))
			{
				building3.setScaleTween(0.8f);
			}
		}
		foreach (Building building4 in World.world.buildings)
		{
			if (!(building4.asset.id == "monolith") && !(building4.asset.id == "waypoint_harp") && building4.asset.building_type == BuildingType.Building_Tree)
			{
				building4.setScaleTween(0.9f);
			}
		}
		foreach (Building building5 in World.world.buildings)
		{
			if (!(building5.asset.id == "monolith") && !(building5.asset.id == "waypoint_harp") && building5.asset.building_type == BuildingType.Building_Tree && !_processed_buildings.Contains(building5))
			{
				float num = Vector3.Distance(building5.current_tile.posV3, Vector2.op_Implicit(building.current_position));
				float num2 = 1f;
				switch (_tree_wave)
				{
				case 0:
					num2 = 10f;
					break;
				case 1:
					num2 = 15f;
					break;
				case 2:
					num2 = 25f;
					break;
				case 3:
					num2 = 35f;
					break;
				case 4:
					num2 = 50f;
					break;
				}
				if (!(num > num2))
				{
					_processed_buildings.Add(building5);
					float pDuration = 0.3f * (float)(5 - _tree_wave) + Randy.randomFloat(0f, 0.1f);
					building5.setScaleTween(0.3f, pDuration);
				}
			}
		}
		_tree_wave++;
		if (_tree_wave >= 5)
		{
			_tree_wave = 0;
			_processed_buildings.Clear();
		}
	}

	private void switchBiome()
	{
		_current_biome++;
		if (_current_biome >= _biomes.Length)
		{
			_current_biome = 0;
		}
		_tree_wave = 0;
		World.world.era_manager.startNextAge();
		BiomeAsset biomeAsset = AssetManager.biome_library.get(_biomes[_current_biome]);
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			if (worldTile.main_type.soil && worldTile.top_type != null && worldTile.top_type.is_biome)
			{
				if (worldTile.main_type.rank_type == TileRank.High)
				{
					worldTile.setTopTileType(biomeAsset.getTileHigh());
				}
				else
				{
					worldTile.setTopTileType(biomeAsset.getTileLow());
				}
			}
		}
		foreach (Building building in World.world.buildings)
		{
			if (!building.asset.flora)
			{
				continue;
			}
			if (building.asset.building_type == BuildingType.Building_Tree)
			{
				if (!(building.asset.id == "palm_tree"))
				{
					string random = biomeAsset.pot_trees_spawn.GetRandom();
					BuildingAsset asset = AssetManager.buildings.get(random);
					building.asset = asset;
					building.clearSprites();
				}
			}
			else
			{
				if (building.asset.building_type != BuildingType.Building_Plant)
				{
					continue;
				}
				string text = biomeAsset.pot_plants_spawn.GetRandom();
				if (text == "fruit_bush")
				{
					for (int j = 0; j < biomeAsset.pot_plants_spawn.Count; j++)
					{
						if (!(biomeAsset.pot_plants_spawn[j] == "fruit_bush"))
						{
							text = biomeAsset.pot_plants_spawn[j];
							break;
						}
					}
				}
				BuildingAsset asset2 = AssetManager.buildings.get(text);
				building.asset = asset2;
				building.clearSprites();
			}
		}
	}
}
