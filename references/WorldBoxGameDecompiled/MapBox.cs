using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DG.Tweening;
using EpPathFinding.cs;
using UnityEngine;
using WorldBoxConsole;
using ai;
using ai.behaviours;
using db;
using life.taxi;
using tools.debug;

public class MapBox : MonoBehaviour
{
	internal GameObject joys;

	public const float TRANSITION_EFFECT_ALPHA = 0.1f;

	public const float TRANSITION_EFFECT_ALPHA_SPEED = 0.1f;

	internal SaveManager save_manager;

	internal ResourceThrowManager resource_throw_manager;

	internal GameStats game_stats;

	internal WorldObject world_object = new WorldObject();

	internal MapStats map_stats = new MapStats();

	internal WorldLaws world_laws;

	internal HotkeyTabsData hotkey_tabs_data;

	internal Canvas canvas;

	public static MapBox instance;

	internal PowerButtonSelector selected_buttons;

	internal ParallelOptions parallel_options;

	public Transform drag_parent;

	public static int width;

	public static int height;

	public static int current_world_seed_id;

	internal WorldTile[,] tiles_map;

	internal WorldTile[] tiles_list;

	public readonly List<BaseSystemManager> list_all_sim_managers = new List<BaseSystemManager>();

	private readonly List<BaseSystemManager> _list_meta_other_managers = new List<BaseSystemManager>();

	private readonly List<BaseSystemManager> _list_meta_main_managers = new List<BaseSystemManager>();

	private readonly List<BaseSystemManager> _list_sim_objects_managers = new List<BaseSystemManager>();

	public ProjectileManager projectiles;

	public StatusManager statuses;

	public CityManager cities;

	public WarManager wars;

	public PlotManager plots;

	public AllianceManager alliances;

	public ClanManager clans;

	public KingdomManager kingdoms;

	public WildKingdomsManager kingdoms_wild;

	public CultureManager cultures;

	public BookManager books;

	public SubspeciesManager subspecies;

	public ReligionManager religions;

	public LanguageManager languages;

	public FamilyManager families;

	public ArmyManager armies;

	public ItemManager items;

	public DiplomacyManager diplomacy;

	public BuildingManager buildings;

	public ActorManager units;

	public TileManager tile_manager;

	internal WorldTilemap tilemap;

	private float _redraw_timer;

	private bool _initiated;

	private DebugLayer _debug_layer;

	internal readonly RegionPathFinder region_path_finder = new RegionPathFinder();

	internal LoadingScreen transition_screen;

	internal StackEffects stack_effects;

	public DropManager drop_manager;

	internal PathFindingVisualiser path_finding_visualiser;

	internal WorldLayer world_layer;

	internal SpriteRenderer _world_layer_switch_effect;

	internal WorldLayerEdges world_layer_edges;

	internal UnitLayer unit_layer;

	internal GreyGooLayer grey_goo_layer;

	internal FireLayer fire_layer;

	private LavaLayer _lava_layer;

	internal PixelFlashEffects flash_effects;

	internal IslandsCalculator islands_calculator;

	internal ZoneCalculator zone_calculator;

	internal RoadsCalculator roads_calculator;

	internal BurnedTilesLayer burned_layer;

	internal ExplosionsEffects explosion_layer;

	internal ConwayLife conway_layer;

	internal MapChunkManager map_chunk_manager;

	internal AutoCivilization civilization_maker;

	private List<MapLayer> _map_layers;

	private List<BaseModule> _map_modules;

	internal Earthquake earthquake_manager;

	public Vector2 wind_direction;

	private StaticGrid _search_grid_ground;

	internal HashSet<WorldTile> tiles_dirty;

	internal GlowParticles particles_fire;

	internal GlowParticles particles_smoke;

	internal NameplateManager nameplate_manager;

	internal WorldBoxConsole.Console console;

	internal bool has_focus = true;

	internal Heat heat;

	internal HeatRayEffect heat_ray_fx;

	internal EffectDivineLight fx_divine_light;

	private MapBorder _map_border;

	internal QualityChanger quality_changer;

	internal Transform transform_units;

	internal SimObjectsZones sim_object_zones;

	internal Tutorial tutorial;

	private WorldLog _world_log;

	internal Magnet magnet;

	internal float timer_nutrition_decay;

	internal AutoTesterBot auto_tester;

	private UnitSelectionEffect _unit_select_effect;

	private readonly List<SpriteGroupSystem<GroupSpriteObject>> _list_systems = new List<SpriteGroupSystem<GroupSpriteObject>>();

	public static CursorSpeed cursor_speed;

	private DebugTextGroupSystem _debug_text_group_system;

	private SignalManager _signal_manager;

	internal ExplosionChecker explosion_checker;

	internal WorldAgeManager era_manager;

	public DelayedActionsManager delayed_actions_manager;

	public PlayerControl player_control;

	private bool _first_gen = true;

	private int _load_counter;

	private float _shake_timer;

	private float _shake_interval_timer;

	private float _shake_intensity = 1f;

	private float _shake_interval = 0.1f;

	private bool _shake_x = true;

	private bool _shake_y = true;

	private Transform _shake_camera;

	internal float elapsed;

	internal float delta_time;

	internal float fixed_delta_time;

	public readonly CityZoneHelper city_zone_helper = new CityZoneHelper();

	internal static Action on_world_loaded;

	private static int _tile_id;

	internal readonly AStarParam pathfinding_param = new AStarParam();

	internal int dirty_tiles_last;

	private bool _is_paused;

	private int _render_skip;

	private bool _meta_skip = true;

	private MetaTypeAsset _cached_map_meta_asset;

	private ArchitectMood _cached_architect_mood;

	internal LibraryMaterials library_materials => LibraryMaterials.instance;

	internal Camera camera { get; private set; }

	internal MoveCamera move_camera { get; private set; }

	internal ZoneCamera zone_camera { get; private set; }

	public GodPower selected_power => selected_buttons.selectedButton?.godPower;

	private void Awake()
	{
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		instance = this;
		player_control = new PlayerControl();
		parallel_options = new ParallelOptions
		{
			CancellationToken = ((MonoBehaviour)this).destroyCancellationToken
		};
		auto_tester = Object.FindFirstObjectByType<AutoTesterBot>((FindObjectsInactive)1);
		save_manager = ((Component)this).GetComponentInChildren<SaveManager>();
		game_stats = ((Component)this).GetComponentInChildren<GameStats>();
		tilemap = ((Component)this).GetComponentInChildren<WorldTilemap>();
		_map_border = ((Component)this).GetComponentInChildren<MapBorder>();
		stack_effects = ((Component)this).GetComponentInChildren<StackEffects>();
		resource_throw_manager = new ResourceThrowManager();
		heat_ray_fx = ((Component)this).GetComponentInChildren<HeatRayEffect>();
		fx_divine_light = ((Component)this).GetComponentInChildren<EffectDivineLight>();
		particles_fire = ((Component)((Component)this).transform.Find("Particles Fire")).GetComponent<GlowParticles>();
		particles_smoke = ((Component)((Component)this).transform.Find("Particles Smoke")).GetComponent<GlowParticles>();
		_shake_camera = GameObject.Find("CameraShake").transform;
		Transform transform = GameObject.Find("Canvas Container Main").transform;
		canvas = ((Component)transform.FindRecursive("Canvas - UI/General")).GetComponent<Canvas>();
		transition_screen = ((Component)transform).GetComponentInChildren<LoadingScreen>(true);
		console = ((Component)transform).GetComponentInChildren<WorldBoxConsole.Console>(true);
		nameplate_manager = ((Component)transform).GetComponentInChildren<NameplateManager>(true);
		tutorial = ((Component)transform).GetComponentInChildren<Tutorial>(true);
		selected_buttons = ((Component)transform).GetComponentInChildren<PowerButtonSelector>();
		cursor_speed = new CursorSpeed();
		_signal_manager = new SignalManager();
		joys = GameObject.Find("Joys");
		joys.gameObject.SetActive(false);
		magnet = new Magnet();
		islands_calculator = new IslandsCalculator();
		sim_object_zones = new SimObjectsZones();
		_world_log = new WorldLog();
		quality_changer = ((Component)this).GetComponent<QualityChanger>();
		transform_units = ((Component)this).transform.FindRecursive("Units");
		stack_effects.create();
		tiles_dirty = new HashSet<WorldTile>();
		tiles_list = new WorldTile[0];
		tile_manager = new TileManager();
		drop_manager = new DropManager(((Component)this).transform.Find("Drops"));
		_list_meta_main_managers.Add(subspecies = new SubspeciesManager());
		_list_meta_main_managers.Add(families = new FamilyManager());
		_list_meta_main_managers.Add(armies = new ArmyManager());
		_list_meta_main_managers.Add(languages = new LanguageManager());
		_list_meta_main_managers.Add(religions = new ReligionManager());
		_list_meta_main_managers.Add(cities = new CityManager());
		_list_meta_main_managers.Add(clans = new ClanManager());
		_list_meta_main_managers.Add(alliances = new AllianceManager());
		_list_meta_main_managers.Add(kingdoms = new KingdomManager());
		_list_meta_main_managers.Add(kingdoms_wild = new WildKingdomsManager());
		_list_meta_main_managers.Add(cultures = new CultureManager());
		_list_meta_main_managers.Add(plots = new PlotManager());
		_list_meta_main_managers.Add(wars = new WarManager());
		_list_meta_main_managers.Add(items = new ItemManager());
		_list_meta_other_managers.Add(books = new BookManager());
		_list_meta_other_managers.Add(diplomacy = new DiplomacyManager());
		_list_meta_other_managers.Add(projectiles = new ProjectileManager());
		_list_meta_other_managers.Add(statuses = new StatusManager());
		_list_sim_objects_managers.Add(units = new ActorManager());
		_list_sim_objects_managers.Add(buildings = new BuildingManager());
		list_all_sim_managers.AddRange(_list_sim_objects_managers);
		list_all_sim_managers.AddRange(_list_meta_main_managers);
		list_all_sim_managers.AddRange(_list_meta_other_managers);
		heat = new Heat();
		wind_direction = new Vector2(-0.1f, 0.2f);
		era_manager = new WorldAgeManager();
		delayed_actions_manager = new DelayedActionsManager();
		AssetManager.world_behaviours.createManagers();
		((Component)this).gameObject.AddOrGetComponent<MusicBox>();
		DOTween.SetTweensCapacity(1000, 100);
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		Application.lowMemory += new LowMemoryCallback(AutoSaveManager.OnLowMemory);
		Application.lowMemory += new LowMemoryCallback(PlayerConfig.turnOffAssetsPreloading);
		PlayerConfig.instance.start();
		explosion_checker = new ExplosionChecker();
		camera = Camera.main;
		move_camera = ((Component)camera).GetComponent<MoveCamera>();
		_initiated = true;
		_map_layers = new List<MapLayer>();
		_map_modules = new List<BaseModule>();
		_map_layers.Add(world_layer = ((Component)this).GetComponentInChildren<WorldLayer>());
		_map_layers.Add(world_layer_edges = ((Component)this).GetComponentInChildren<WorldLayerEdges>());
		_world_layer_switch_effect = ((Component)((Component)this).gameObject.transform.Find("world_layer_back")).GetComponent<SpriteRenderer>();
		_map_layers.Add(unit_layer = ((Component)this).GetComponentInChildren<UnitLayer>());
		_map_layers.Add(zone_calculator = ((Component)this).GetComponentInChildren<ZoneCalculator>());
		_map_layers.Add(burned_layer = ((Component)this).GetComponentInChildren<BurnedTilesLayer>());
		_map_layers.Add(explosion_layer = ((Component)this).GetComponentInChildren<ExplosionsEffects>());
		_map_layers.Add(conway_layer = ((Component)this).GetComponentInChildren<ConwayLife>());
		_map_layers.Add(fire_layer = ((Component)this).GetComponentInChildren<FireLayer>());
		_map_layers.Add(_lava_layer = ((Component)this).GetComponentInChildren<LavaLayer>());
		_map_layers.Add(_debug_layer = ((Component)this).GetComponentInChildren<DebugLayer>());
		_map_layers.Add(((Component)this).GetComponentInChildren<DebugLayerCursor>());
		_map_layers.Add(path_finding_visualiser = ((Component)this).GetComponentInChildren<PathFindingVisualiser>());
		_map_layers.Add(flash_effects = ((Component)this).GetComponentInChildren<PixelFlashEffects>());
		_map_modules.Add(roads_calculator = ((Component)this).GetComponentInChildren<RoadsCalculator>());
		_map_modules.Add(grey_goo_layer = ((Component)this).GetComponentInChildren<GreyGooLayer>());
		map_chunk_manager = new MapChunkManager();
		zone_camera = new ZoneCamera();
		if (Config.isComputer || Config.isEditor)
		{
			GameObject val = (GameObject)Resources.Load("effects/PrefabUnitSelectionEffect");
			_unit_select_effect = Object.Instantiate<GameObject>(val, ((Component)this).gameObject.transform).GetComponent<UnitSelectionEffect>();
			_unit_select_effect.create();
		}
		addNewSystem(_debug_text_group_system = new GameObject().AddComponent<DebugTextGroupSystem>());
		foreach (SpriteGroupSystem<GroupSpriteObject> list_system in _list_systems)
		{
			list_system.create();
		}
	}

	private void addNewSystem(SpriteGroupSystem<GroupSpriteObject> pSystem)
	{
		_list_systems.Add(pSystem);
		((Component)pSystem).transform.parent = ((Component)this).transform;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isGameplayControlsLocked()
	{
		if (!ScrollWindow.isWindowActive() && !ScrollWindow.isAnimationActive())
		{
			return RewardedAds.isShowing();
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isWindowOnScreen()
	{
		if (!ScrollWindow.isWindowActive())
		{
			return ScrollWindow.isAnimationActive();
		}
		return true;
	}

	internal bool calcPath(WorldTile pFrom, WorldTile pTargetTile, List<WorldTile> pSavePath)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		pSavePath.Clear();
		StaticGrid search_grid_ground = _search_grid_ground;
		HeuristicMode heuristic = HeuristicMode.MANHATTAN;
		float weight = 2f;
		DiagonalMovement diagonalMovement;
		int num;
		if (pathfinding_param.ocean)
		{
			num = 50;
			diagonalMovement = DiagonalMovement.OnlyWhenNoObstacles;
		}
		else
		{
			if (pathfinding_param.limit)
			{
				num = 500;
			}
			else
			{
				num = -1;
			}
			diagonalMovement = DiagonalMovement.Always;
		}
		num = -1;
		if (pathfinding_param.roads)
		{
			weight = 1f;
			diagonalMovement = DiagonalMovement.Never;
			heuristic = HeuristicMode.EUCLIDEAN;
		}
		search_grid_ground.Reset();
		if (!pFrom.isSameIsland(pTargetTile) && !pathfinding_param.ocean)
		{
			pSavePath.Add(pFrom);
			pSavePath.Add(pTargetTile);
			path_finding_visualiser.showPath(search_grid_ground, pSavePath);
			return true;
		}
		Vector2Int pos = pFrom.pos;
		int x = ((Vector2Int)(ref pos)).x;
		pos = pFrom.pos;
		GridPos iStartPos = new GridPos(x, ((Vector2Int)(ref pos)).y);
		pos = pTargetTile.pos;
		int x2 = ((Vector2Int)(ref pos)).x;
		pos = pTargetTile.pos;
		GridPos iEndPos = new GridPos(x2, ((Vector2Int)(ref pos)).y);
		pathfinding_param.setGrid(search_grid_ground, iStartPos, iEndPos);
		pathfinding_param.DiagonalMovement = diagonalMovement;
		pathfinding_param.SetHeuristic(heuristic);
		pathfinding_param.max_open_list = num;
		pathfinding_param.weight = weight;
		AStarFinder.FindPath(pathfinding_param, pSavePath);
		path_finding_visualiser.showPath(search_grid_ground, pSavePath);
		if (pSavePath.Count == 0)
		{
			return false;
		}
		return true;
	}

	public void startTheGame(bool pForceGenerate = false)
	{
		LogText.log("MapBox", "startTheGame", "st");
		Randy.fullReset();
		Config.game_loaded = true;
		Config.current_brush = "circ_5";
		if (Config.isMobile)
		{
			PlayInterstitialAd.setActive(pActive: true);
		}
		Config.LOAD_TIME_CREATE = Time.realtimeSinceStartup;
		if (pForceGenerate || Config.load_new_map)
		{
			generateNewMap();
		}
		else if (Config.load_random_test_map)
		{
			TestMaps.loadNextMap();
		}
		else if (Config.load_dragon)
		{
			SaveManager.loadMapFromResources("mapTemplates/map_dragon");
		}
		else if (Config.load_save_on_start)
		{
			_first_gen = false;
			string slotSavePath = SaveManager.getSlotSavePath(Config.load_save_on_start_slot);
			save_manager.loadWorld(slotSavePath);
		}
		else if (Config.load_save_from_path)
		{
			SaveManager.loadMapFromResources(Config.load_test_save_path);
		}
		else if (Config.load_test_map)
		{
			DebugMap.makeDebugMap();
		}
		else
		{
			string text = "";
			try
			{
				text = DateTime.Now.ToString("MM/dd");
			}
			catch (Exception)
			{
				text = "";
			}
			if (text == "04/01")
			{
				SaveManager.loadMapFromResources("mapTemplates/map_april_fools");
			}
			else if (FavoriteWorld.hasFavoriteWorldSet())
			{
				int favorite_world = PlayerConfig.instance.data.favorite_world;
				FavoriteWorld.cacheSaveSlotID(favorite_world);
				FavoriteWorld.clearFavoriteWorld();
				_first_gen = false;
				string slotSavePath2 = SaveManager.getSlotSavePath(favorite_world);
				save_manager.loadWorld(slotSavePath2);
			}
			else if (game_stats.data.gameLaunches <= 3)
			{
				SaveManager.loadMapFromResources("mapTemplates/map_dragon");
			}
			else
			{
				generateNewMap();
				SmoothLoader.add(delegate
				{
					buildings.addBuilding("volcano", GetTile(width / 2, height / 2));
				}, "add_volcano");
				SmoothLoader.add(delegate
				{
					WorldTile tile = GetTile(0, height - 1);
					WorldTile tile2 = GetTile(width - 1, height - 1);
					WorldTile tile3 = GetTile(0, 0);
					WorldTile tile4 = GetTile(width - 1, 0);
					units.spawnNewUnit("angle", tile, pSpawnSound: false, pMiracleSpawn: true).setName("DAB", pTrack: false);
					units.spawnNewUnit("angle", tile2, pSpawnSound: false, pMiracleSpawn: true).setName("ABC", pTrack: false);
					units.spawnNewUnit("angle", tile3, pSpawnSound: false, pMiracleSpawn: true).setName("CDA", pTrack: false);
					units.spawnNewUnit("angle", tile4, pSpawnSound: false, pMiracleSpawn: true).setName("BCD", pTrack: false);
				}, "spawn_angles");
			}
		}
		SmoothLoader.add(addLastStep, "Prepare Game Launch");
	}

	private void addLastStep()
	{
		SmoothLoader.add(delegate
		{
			Config.LOAD_TIME_GENERATE = Time.realtimeSinceStartup;
			((Renderer)((Component)this).GetComponent<SpriteRenderer>()).enabled = true;
			((Component)nameplate_manager).gameObject.SetActive(true);
			FavoriteWorld.restoreCachedFavoriteWorldOnSuccess();
			if (!Config.disable_startup_window)
			{
				if (PlayerConfig.instance.data.tutorialFinished || Config.disable_tutorial)
				{
					ScrollWindow.get("welcome").forceShow();
				}
				else
				{
					tutorial.startTutorial();
				}
			}
			PremiumElementsChecker.checkElements();
			MonoBehaviour.print((object)("LOAD TIME INIT: " + Config.LOAD_TIME_INIT));
			MonoBehaviour.print((object)("LOAD TIME CREATE: " + (Config.LOAD_TIME_CREATE - Config.LOAD_TIME_INIT)));
			MonoBehaviour.print((object)("LOAD TIME GENERATE: " + (Config.LOAD_TIME_GENERATE - Config.LOAD_TIME_CREATE)));
			LogText.log("MapBox", "startTheGame", "en");
		}, "Start the Game", pSkipFrame: false, 0.001f, pToEnd: true);
	}

	private void afterLoadEvent()
	{
		Debug.Log((object)"afterLoadEvent--------------------------");
		PremiumElementsChecker.checkElements();
	}

	internal void centerCamera()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)camera).transform.position;
		position.x = width / 2;
		position.y = height / 2;
		((Component)camera).transform.position = position;
		move_camera.resetZoom();
	}

	private void resetTiles()
	{
		_search_grid_ground?.Reset();
		WorldTile[] array = tiles_list;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].clear();
		}
		tiles_dirty.Clear();
		tilemap.clear();
	}

	private void clearTiles()
	{
		_search_grid_ground?.Dispose();
		_search_grid_ground = null;
		zone_calculator.clean();
		map_chunk_manager.clean();
		WorldTile[] array = tiles_list;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
		tiles_list = new WorldTile[0];
		for (int j = 0; j < width; j++)
		{
			for (int k = 0; k < height; k++)
			{
				tiles_map[j, k] = null;
			}
		}
		tiles_map = null;
		tiles_dirty.Clear();
		tilemap.clear();
	}

	private void createTiles()
	{
		SmoothLoader.add(delegate
		{
			tiles_list = new WorldTile[width * height];
			tiles_map = new WorldTile[width, height];
			GeneratorTool.Setup(tiles_map);
		}, "Prepare Tiles");
		SmoothLoader.add(delegate
		{
			_tile_id = 0;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					WorldTile worldTile = new WorldTile(j, i, _tile_id);
					_search_grid_ground.SetTileNode(j, i, worldTile);
					tiles_map[j, i] = worldTile;
					tiles_list[_tile_id] = worldTile;
					_tile_id++;
				}
			}
		}, "Create Tiles (" + height * width + ")", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			_ = tiles_list.Length;
		}, "Create Neighbours [" + height * width + "] (1/3)", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			int num = tiles_list.Length;
			for (int i = 0; i < num; i++)
			{
				tiles_list[i].resetNeighbourLists();
			}
		}, "Create Neighbours [" + height * width + "] (2/3)", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			GeneratorTool.GenerateTileNeighbours(tiles_list);
		}, "Create Neighbours [" + height * width + "] (3/3)", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			zone_calculator.generate();
			WorldBehaviourActionFire.prepare();
		}, "Create Zones", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			map_chunk_manager.prepare();
		}, "Create Chunks", pSkipFrame: true);
	}

	public static AttackDataResult newAttack(AttackData pData)
	{
		if (pData.hit_tile == null)
		{
			return new AttackDataResult(ApplyAttackState.Continue, -1L);
		}
		int num = pData.targets;
		AttackDataResult result = new AttackDataResult(ApplyAttackState.Continue, -1L);
		if (pData.target != null)
		{
			result = checkAttackFor(pData, pData.target);
			switch (result.state)
			{
			case ApplyAttackState.Hit:
				num--;
				break;
			case ApplyAttackState.Block:
			case ApplyAttackState.Deflect:
				return result;
			}
			if (num == 0)
			{
				return result;
			}
		}
		if (num == 0)
		{
			return result;
		}
		List<BaseSimObject> list = EnemiesFinder.findEnemiesFrom(pData.hit_tile, pData.kingdom, 0).list;
		if (list == null)
		{
			return result;
		}
		foreach (BaseSimObject item in list.LoopRandom())
		{
			if (item != pData.target)
			{
				if (num == 0)
				{
					break;
				}
				result = checkAttackFor(pData, item);
				switch (result.state)
				{
				case ApplyAttackState.Hit:
					num--;
					break;
				case ApplyAttackState.Block:
				case ApplyAttackState.Deflect:
					return result;
				}
			}
		}
		return result;
	}

	public static AttackDataResult checkAttackFor(AttackData pData, BaseSimObject pTargetToCheck)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (pTargetToCheck.isRekt())
		{
			return AttackDataResult.Continue;
		}
		if (pData.initiator.isRekt())
		{
			return AttackDataResult.Continue;
		}
		if (pTargetToCheck == pData.initiator)
		{
			return AttackDataResult.Continue;
		}
		if (!pData.initiator.canAttackTarget(pTargetToCheck))
		{
			return AttackDataResult.Continue;
		}
		if (pTargetToCheck.isActor() && pTargetToCheck.hasStatus("dodge"))
		{
			return AttackDataResult.Continue;
		}
		Vector3 val = Vector2.op_Implicit(pTargetToCheck.current_position);
		float num = Toolbox.SquaredDist(val.x, val.y + pTargetToCheck.getHeight(), pData.hit_position.x, pData.hit_position.y + pData.hit_position.z);
		float num2 = pData.area_of_effect + pTargetToCheck.stats["size"];
		num2 *= num2;
		if (num < num2)
		{
			Vector3 val2 = Vector3.MoveTowards(pData.hit_position, val, pTargetToCheck.stats["size"] * 0.9f);
			val2.y += pTargetToCheck.getHeight();
			AttackDataResult result = applyAttack(pData, pTargetToCheck);
			if (result.state == ApplyAttackState.Hit)
			{
				Vector3 hit_position = pData.hit_position;
				hit_position.y += hit_position.z;
				if (pData.critical)
				{
					EffectsLibrary.spawnAt("fx_hit_critical", hit_position, 0.1f);
					return result;
				}
				EffectsLibrary.spawnAt("fx_hit", hit_position, 0.1f);
			}
			return result;
		}
		return AttackDataResult.Miss;
	}

	private static AttackDataResult applyAttack(AttackData pData, BaseSimObject pTargetToCheck)
	{
		bool flag = pTargetToCheck.isActor();
		Actor a = pTargetToCheck.a;
		ProjectileAsset projectileAsset = null;
		if (pData.is_projectile)
		{
			projectileAsset = AssetManager.projectiles.get(pData.projectile_id);
		}
		if (flag && ControllableUnit.isControllingUnit(a) && a.hasMeleeAttack() && a.isJustAttacked())
		{
			CombatActionLibrary.combat_action_deflect.action_actor(pTargetToCheck.a, pData);
			return new AttackDataResult(ApplyAttackState.Deflect, pTargetToCheck.a.data.id);
		}
		if (flag && a.tryToUseAdvancedCombatAction(a.getCombatActionPool(CombatActionPool.BEFORE_HIT_DEFLECT), null, out var pResultCombatAsset))
		{
			pResultCombatAsset.action_actor(pTargetToCheck.a, pData);
			return new AttackDataResult(ApplyAttackState.Deflect, pTargetToCheck.a.data.id);
		}
		bool flag2 = false;
		if (projectileAsset != null && projectileAsset.can_be_blocked)
		{
			flag2 = true;
		}
		if (flag2 && flag && a.tryToUseAdvancedCombatAction(a.getCombatActionPool(CombatActionPool.BEFORE_HIT_BLOCK), null, out pResultCombatAsset))
		{
			pResultCombatAsset.action_actor(pTargetToCheck.a, pData);
			return AttackDataResult.Block;
		}
		if (flag && a.tryToUseAdvancedCombatAction(a.getCombatActionPool(CombatActionPool.BEFORE_HIT), null, out pResultCombatAsset))
		{
			pResultCombatAsset.action_actor(a, pData);
			return AttackDataResult.Continue;
		}
		int num = (int)Randy.randomFloat(pData.damage_range * (float)pData.damage, pData.damage);
		if (pData.critical)
		{
			num *= pData.critical_damage_multiplier;
		}
		if (pData.initiator.isActor() && pTargetToCheck.isAlive())
		{
			pData.initiator.a.addExperience(2);
		}
		pTargetToCheck.getHit(num, pFlash: true, pData.attack_type, pData.initiator, pMetallicWeapon: pData.metallic_weapon, pSkipIfShake: pData.skip_shake);
		if (!pTargetToCheck.hasHealth())
		{
			ActorTool.applyForceToUnit(pData, pTargetToCheck);
		}
		else
		{
			ActorTool.applyForceToUnit(pData, pTargetToCheck, 0.5f, pCheckCancelJobOnLand: true);
		}
		if (pData.initiator.isActor())
		{
			pData.initiator.a.attackTargetActions(pTargetToCheck, pData.hit_tile);
		}
		if (flag && pData.initiator.isActor() && !pTargetToCheck.hasHealth() && pData.initiator.a.needsFood() && pData.initiator.a.subspecies.diet_meat && a.asset.source_meat)
		{
			pData.initiator.a.addNutritionFromEating(70, pSetMaxNutrition: true);
			pData.initiator.a.countConsumed();
		}
		return AttackDataResult.Hit;
	}

	public void clearArchitectMood()
	{
		_cached_architect_mood = null;
	}

	public void clearWorld()
	{
		clearArchitectMood();
		current_world_seed_id++;
		DBInserter.Lock();
		tile_manager.clear();
		CursedSacrifice.reset();
		LogText.log("MapBox", "clearWorld", "st");
		auto_tester?.clearWorld();
		Analytics.worldLoading();
		SelectedUnit.clear();
		ControllableUnit.clear();
		DBManager.clearAndClose();
		explosion_checker.clear();
		BattleKeeperManager.clear();
		ZoneMetaDataVisualizer.clearAll();
		Finder.clear();
		_debug_layer.clear();
		selected_buttons.unselectAll();
		player_control.clear();
		MusicBox.clearAllSounds();
		clearFrameCaches();
		EnemiesFinder.disposeAll();
		TaxiManager.clear();
		islands_calculator.clear();
		RegionLinkHashes.clear();
		nameplate_manager.clearAll();
		map_chunk_manager.clearAll();
		islands_calculator.clear();
		quality_changer.reset();
		tilemap.clear();
		zone_camera.clear();
		Config.paused = false;
		if (DebugConfig.isOn(DebugOption.PauseOnStart))
		{
			Config.paused = true;
		}
		selected_buttons.checkToggleIcons();
		heat.clear();
		era_manager.clear();
		delayed_actions_manager.clear();
		foreach (TileType item in AssetManager.tiles.list)
		{
			item.hashsetClear();
		}
		foreach (TopTileType item2 in AssetManager.top_tiles.list)
		{
			item2.hashsetClear();
		}
		foreach (WorldBehaviourAsset item3 in AssetManager.world_behaviours.list)
		{
			item3.manager.clear();
		}
		WildKingdomsManager.neutral.clearListCities();
		AutoSaveManager.resetAutoSaveTimer();
		AssetManager.actor_library.clear();
		AssetManager.buildings.clear();
		BehaviourActionActor.clear();
		city_zone_helper.clear();
		region_path_finder.clear();
		Toolbox.clearAll();
		drop_manager.clear();
		armies.clear();
		foreach (BaseSystemManager list_all_sim_manager in list_all_sim_managers)
		{
			list_all_sim_manager.clear();
		}
		particles_fire.clear();
		particles_smoke.clear();
		stack_effects.clear();
		TornadoEffect.Clear();
		resource_throw_manager.clear();
		foreach (MapLayer map_layer in _map_layers)
		{
			map_layer.clear();
		}
		foreach (BaseModule map_module in _map_modules)
		{
			map_module.clear();
		}
		sim_object_zones.fullClear();
		resetTiles();
		zone_camera.fullClear();
		world_layer_edges.clear();
		WorldBehaviourActionFire.clearFires();
		HistoryHud.instance.Clear();
		DBInserter.Unlock();
		DBManager.clearAndClose();
		LogText.log("MapBox", "clearWorld", "en");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WorldTile GetTile(int pX, int pY)
	{
		if (pX < 0 || pX >= width || pY < 0 || pY >= height)
		{
			return null;
		}
		return tiles_map[pX, pY];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public WorldTile GetTileSimple(int pX, int pY)
	{
		return tiles_map[pX, pY];
	}

	public void setMapSize(int pWidth, int pHeight)
	{
		Config.ZONE_AMOUNT_X = pWidth;
		Config.ZONE_AMOUNT_Y = pHeight;
		width = Config.ZONE_AMOUNT_X * 64;
		height = Config.ZONE_AMOUNT_Y * 64;
		int num = width * height;
		if (tiles_list.Length != num)
		{
			recreateSizes();
		}
	}

	private void afterTransitionGeneration()
	{
		generateNewMap();
	}

	public void clickGenerateNewMap()
	{
		transition_screen.startTransition(afterTransitionGeneration);
	}

	public void generateNewMap()
	{
		if (!_initiated)
		{
			return;
		}
		if (Config.show_console_on_start)
		{
			console.Toggle();
		}
		SmoothLoader.prepare();
		SmoothLoader.add(delegate
		{
			LogText.log("MapBox", "generateNewMap", "st");
			Analytics.worldLoading();
			if (_first_gen)
			{
				Config.customMapSize = Config.customMapSizeDefault;
			}
			if (!_first_gen)
			{
				AchievementLibrary.custom_world.check();
			}
			_first_gen = false;
			int size = MapSizeLibrary.getSize(Config.customMapSize);
			addClearWorld(size, size);
		}, "Generate New Map (1/3)");
		SmoothLoader.add(delegate
		{
			Config.ZONE_AMOUNT_Y = (Config.ZONE_AMOUNT_X = MapSizeLibrary.getSize(Config.customMapSize));
		}, "Generate New Map (2/3)");
		SmoothLoader.add(delegate
		{
			setMapSize(Config.ZONE_AMOUNT_X, Config.ZONE_AMOUNT_Y);
		}, "Generate New Map (3/3)");
		SmoothLoader.add(delegate
		{
			LogText.log("MapBox", "GenerateMap", "st");
			AssetManager.tiles.setListTo(DepthGeneratorType.Generator);
			world_laws = new WorldLaws();
			world_laws.init();
			hotkey_tabs_data = new HotkeyTabsData();
		}, "gen: World Laws");
		SmoothLoader.add(delegate
		{
			map_stats = new MapStats();
			map_stats.initNewWorld();
			Randy.resetSeed(Randy.randomInt(1, 555555555));
		}, "gen: Generating Name");
		if (!Config.disable_db)
		{
			SmoothLoader.add(delegate
			{
				DBManager.createDB();
			}, "Creating Stats DB");
			DBTables.createOrMigrateTablesLoader();
		}
		WindowPreloader.addWaitForWindowResources();
		SmoothLoader.add(delegate
		{
			era_manager.setDefaultAges();
		}, "gen: World Ages");
		SmoothLoader.add(delegate
		{
			MapGenerator.prepare();
			LogText.log("MapBox", "GenerateMap", "en");
		}, "Preparing Map", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			cleanUpWorld();
		}, "Cleaning Up The World", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			redrawTiles();
		}, "Drawing Up The World", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			preloadRenderedSprites();
		}, "Preload rendered sprites...", pSkipFrame: false, 0.2f);
		SmoothLoader.add(delegate
		{
			finishMakingWorld();
			LogText.log("MapBox", "generateNewMap", "en");
		}, "Tidying Up The World", pSkipFrame: true);
		SmoothLoader.add(delegate
		{
			lastGC();
		}, "Rewriting The World", pSkipFrame: true);
		addLoadAutoTester();
		addKillAllUnits();
		addLoadWorldCallbacks();
		SmoothLoader.add(delegate
		{
			finishingUpLoading();
		}, "Finishing up...", pSkipFrame: false, 0.2f);
	}

	public void finishingUpLoading()
	{
		CanvasMain.instance.setMainUiEnabled(pEnabled: true);
	}

	public void preloadRenderedSprites()
	{
		foreach (Actor unit in units)
		{
			unit.checkSpriteToRender();
		}
		foreach (Building building in buildings)
		{
			building.checkSpriteToRender();
		}
	}

	public void addUnloadResources()
	{
		_load_counter++;
		if (_load_counter > 5)
		{
			_load_counter = 0;
			SmoothLoader.add(delegate
			{
				Resources.UnloadUnusedAssets();
			}, "UnloadUnusedAssets", pSkipFrame: true);
		}
	}

	public void addClearWorld(int pNextWidth, int pNextHeight)
	{
		SmoothLoader.add(delegate
		{
			LogText.log("MapBox", "clearWorld", "st");
			clearWorld();
			LogText.log("MapBox", "clearWorld", "en");
		}, "Clearing World", pSkipFrame: true);
		DebugMemory.addMemorySnapshot("clearWorld");
		int num = pNextWidth * 64;
		int num2 = pNextHeight * 64;
		int num3 = num * num2;
		if (tiles_list.Length != num3)
		{
			SmoothLoader.add(delegate
			{
				clearTiles();
			}, "Clean old Tiles");
			DebugMemory.addMemorySnapshot("clearTiles");
		}
	}

	public void addKillAllUnits()
	{
		if (!DebugConfig.isOn(DebugOption.KillAllUnitsOnLoad))
		{
			return;
		}
		SmoothLoader.add(delegate
		{
			foreach (Actor unit in units)
			{
				unit.dieAndDestroy(AttackType.None);
			}
		}, "Killing All Units", pSkipFrame: true);
	}

	public void addLoadAutoTester()
	{
		if (!DebugConfig.isOn(DebugOption.TesterLibs))
		{
			return;
		}
		SmoothLoader.add(delegate
		{
			if (!string.IsNullOrEmpty(Config.auto_test_on_start))
			{
				auto_tester.create(Config.auto_test_on_start);
				((Component)auto_tester).gameObject.SetActive(true);
			}
		}, "Loading Auto Tester", pSkipFrame: true);
	}

	public void addLoadWorldCallbacks()
	{
		SmoothLoader.add(delegate
		{
			Config.debug_worlds_loaded++;
			on_world_loaded?.Invoke();
		}, "World Loaded", pSkipFrame: true);
	}

	private void generateMap(string pType = "islands")
	{
	}

	public void cleanUpWorld(bool pSetChunksDirty = true)
	{
		MapGenerator.clear();
		updateDirtyMetaContainersAndCleanup();
		era_manager.prepare();
		if (pSetChunksDirty)
		{
			map_chunk_manager.allDirty();
			map_chunk_manager.update(0f, pForce: true);
		}
		foreach (City city in cities)
		{
			city.forceDoChecks();
		}
		foreach (City city2 in cities)
		{
			city2.executeAllActionsForCity();
		}
		allTilesDirty();
		centerCamera();
	}

	public void redrawTiles()
	{
		_meta_skip = true;
		if (MusicBox.new_world_on_start_played)
		{
			MusicBox.reserveFlag("new_world");
		}
		tilemap.redrawTiles(pForceAll: true);
	}

	public void finishMakingWorld()
	{
		ToolbarButtons.instance?.resetBar();
		game_stats.data.mapsCreated++;
		AchievementLibrary.gen_5_worlds.check();
		AchievementLibrary.gen_50_worlds.check();
		AchievementLibrary.gen_100_worlds.check();
		Analytics.worldLoaded();
		Config.LAST_LOAD_TIME = Time.realtimeSinceStartup;
	}

	public void lastGC()
	{
		Config.forceGC("finish making world");
	}

	private void recreateSizes()
	{
		SmoothLoader.add(delegate
		{
			_search_grid_ground = new StaticGrid(width, height);
		}, "Recreate Sizes (1/4)");
		SmoothLoader.add(delegate
		{
			createTiles();
		}, "Recreate Sizes (2/4)");
		SmoothLoader.add(delegate
		{
			tile_manager.setup(width, height, tiles_map);
		}, "Tile Manager", pSkipFrame: true);
		for (int num = 0; num < _map_layers.Count; num++)
		{
			int j = num;
			SmoothLoader.add(delegate
			{
				_map_layers[j].createTextureNew();
			}, "Recreate Sizes (3/4) (" + (num + 1) + "/" + _map_layers.Count + ")");
		}
		SmoothLoader.add(delegate
		{
			if (Globals.TRAILER_MODE)
			{
				Object.Destroy((Object)(object)((Component)_map_border).gameObject);
			}
			else
			{
				_map_border.generateTexture();
			}
		}, "Recreate Sizes (4/4)");
	}

	public Actor getActorNearCursor()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return ActionLibrary.getActorNearPos(instance.getMousePos());
	}

	public WorldTile getMouseTilePosCachedFrame()
	{
		return player_control.getMouseTilePosCachedFrame();
	}

	public Vector2 getMousePos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return player_control.getMousePos();
	}

	public WorldTile getMouseTilePos()
	{
		return player_control.getMouseTilePos();
	}

	public bool isPointerInGame()
	{
		return player_control.isPointerInGame();
	}

	public bool isPointerInsideMapBounds()
	{
		return getMouseTilePos() != null;
	}

	public bool isOverUI()
	{
		return player_control.isOverUI();
	}

	public bool isTouchOverUI(Touch pTouch)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return player_control.isTouchOverUI(pTouch);
	}

	public static bool controlsLocked()
	{
		return PlayerControl.controlsLocked();
	}

	public static bool isControllingUnit()
	{
		return PlayerControl.isControllingUnit();
	}

	public bool isBusyWithUI()
	{
		return player_control.isBusyWithUI();
	}

	public bool isActionHappening()
	{
		return player_control.isActionHappening();
	}

	public bool isOverUiButton()
	{
		return player_control?.isPointerOverUIButton() ?? false;
	}

	public void loopWithBrush(WorldTile pCenterTile, BrushData pBrush, PowerAction pAction, GodPower pPower = null)
	{
		BrushPixelData[] pos = pBrush.pos;
		int num = pos.Length;
		for (int i = 0; i < num; i++)
		{
			BrushPixelData brushPixelData = pos[i];
			int num2 = pCenterTile.x + brushPixelData.x;
			int num3 = pCenterTile.y + brushPixelData.y;
			if (num2 >= 0 && num2 < width && num3 >= 0 && num3 < height)
			{
				WorldTile tileSimple = instance.GetTileSimple(num2, num3);
				pAction(tileSimple, pPower);
			}
		}
	}

	public void highlightTilesBrush(WorldTile pCenterTile, BrushData pBrush, PowerAction pAction, GodPower pPower = null)
	{
		loopWithBrush(pCenterTile, pBrush, pAction, pPower);
	}

	public void loopWithBrushPowerForDropsFull(WorldTile pCenterTile, BrushData pBrush, PowerAction pAction, GodPower pPower = null)
	{
		loopWithBrush(pCenterTile, pBrush, pAction, pPower);
	}

	public void loopWithBrushPowerForDropsRandom(WorldTile pCenterTile, BrushData pBrush, PowerAction pAction, GodPower pPower = null)
	{
		BrushPixelData[] pos = pBrush.pos;
		int num = pos.Length;
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>();
		for (int i = 0; i < num; i++)
		{
			BrushPixelData brushPixelData = pos[i];
			int num2 = pCenterTile.x + brushPixelData.x;
			int num3 = pCenterTile.y + brushPixelData.y;
			if (num2 >= 0 && num2 < width && num3 >= 0 && num3 < height)
			{
				WorldTile tileSimple = instance.GetTileSimple(num2, num3);
				listPool.Add(tileSimple);
			}
		}
		int drops = pBrush.drops;
		listPool.Shuffle();
		for (int j = 0; j < drops; j++)
		{
			if (listPool.Count == 0)
			{
				break;
			}
			WorldTile pTile = listPool.Pop();
			pAction(pTile, pPower);
		}
	}

	public void loopWithBrush(WorldTile pCenterTile, BrushData pBrush, PowerActionWithID pAction, string pPowerID = null)
	{
		BrushPixelData[] pos = pBrush.pos;
		int num = pos.Length;
		for (int i = 0; i < num; i++)
		{
			BrushPixelData brushPixelData = pos[i];
			int num2 = pCenterTile.x + brushPixelData.x;
			int num3 = pCenterTile.y + brushPixelData.y;
			if (num2 >= 0 && num2 < width && num3 >= 0 && num3 < height)
			{
				WorldTile tileSimple = instance.GetTileSimple(num2, num3);
				pAction(tileSimple, pPowerID);
			}
		}
	}

	public void checkCityZone(WorldTile pTile)
	{
		if (pTile.zone.city == null)
		{
			return;
		}
		bool flag = false;
		HashSet<Building> hashset = pTile.zone.getHashset(BuildingList.Civs);
		if (hashset != null)
		{
			foreach (Building item in hashset)
			{
				if (item.city == pTile.zone.city)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			pTile.zone.city.removeZone(pTile.zone);
		}
	}

	public static void spawnLightningBig(WorldTile pTile, float pScale = 0.25f, Actor pActor = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = EffectsLibrary.spawnAtTile("fx_lightning_big", pTile, pScale);
		if (!((Object)(object)baseEffect == (Object)null))
		{
			int pRad = (int)(pScale * 25f);
			MapAction.checkLightningAction(pTile.pos, pRad, pActor, pCheckForImmortal: true, pCheckMayIInterrupt: true);
			MapAction.damageWorld(pTile, pRad, AssetManager.terraform.get("lightning_power"), pActor);
			baseEffect.sprite_renderer.flipX = Randy.randomBool();
			MapAction.checkSantaHit(pTile.pos, pRad);
			MapAction.checkUFOHit(pTile.pos, pRad, pActor);
			MapAction.checkTornadoHit(pTile.pos, pRad);
		}
	}

	public static void spawnLightningMedium(WorldTile pTile, float pScale = 0.25f, Actor pActor = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = EffectsLibrary.spawnAtTile("fx_lightning_medium", pTile, pScale);
		if (!((Object)(object)baseEffect == (Object)null))
		{
			int pRad = (int)(pScale * 15f);
			MapAction.checkLightningAction(pTile.pos, pRad, pActor);
			MapAction.damageWorld(pTile, pRad, AssetManager.terraform.get("lightning_normal"), pActor);
			baseEffect.sprite_renderer.flipX = Randy.randomBool();
			MapAction.checkTornadoHit(pTile.pos, pRad);
		}
	}

	public static void spawnLightningSmall(WorldTile pTile, float pScale = 0.25f, Actor pActor = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		BaseEffect baseEffect = EffectsLibrary.spawnAtTile("fx_lightning_small", pTile, pScale);
		if (!((Object)(object)baseEffect == (Object)null))
		{
			int pRad = (int)(pScale * 10f);
			MapAction.checkLightningAction(pTile.pos, pRad, pActor);
			MapAction.damageWorld(pTile, pRad, AssetManager.terraform.get("lightning_normal"), pActor);
			baseEffect.sprite_renderer.flipX = Randy.randomBool();
			MapAction.checkTornadoHit(pTile.pos, pRad);
		}
	}

	public void applyForceOnTile(WorldTile pTile, int pRad = 10, float pForceAmount = 1.5f, bool pForceOut = true, int pDamage = 0, string[] pIgnoreKingdoms = null, BaseSimObject pByWho = null, TerraformOptions pOptions = null, bool pChangeHappiness = false)
	{
		int num = pRad * pRad;
		foreach (Actor item in Finder.getUnitsFromChunk(pTile, 1))
		{
			if (item == pByWho?.a || (float)Toolbox.SquaredDistTile(item.current_tile, pTile) > (float)num || (pOptions != null && item.asset.very_high_flyer && !pOptions.applies_to_high_flyers))
			{
				continue;
			}
			if (pIgnoreKingdoms != null)
			{
				bool flag = false;
				for (int i = 0; i < pIgnoreKingdoms.Length; i++)
				{
					Kingdom kingdom = kingdoms_wild.get(pIgnoreKingdoms[i]);
					if (item.kingdom == kingdom)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			item.makeStunned(4f);
			if (pChangeHappiness)
			{
				item.changeHappiness("just_forced_power");
			}
			if (item.asset.can_be_hurt_by_powers && pDamage > 0)
			{
				AttackType pAttackType = AttackType.Other;
				if (pOptions != null)
				{
					pAttackType = pOptions.attack_type;
				}
				item.getHit(pDamage, pFlash: true, pAttackType, pByWho);
			}
			if (pForceAmount > 0f)
			{
				if (pForceOut)
				{
					item.calculateForce(item.current_tile.x, item.current_tile.y, pTile.x, pTile.y, pForceAmount, 0f, pCheckCancelJobOnLand: true);
				}
				else
				{
					item.calculateForce(pTile.x, pTile.y, item.current_tile.x, item.current_tile.y, pForceAmount, 0f, pCheckCancelJobOnLand: true);
				}
			}
		}
	}

	internal void stopAttacksFor(bool pMonsters)
	{
		foreach (Actor unit in units)
		{
			if (unit.has_attack_target && unit.isEnemyTargetAlive() && (unit.kingdom.asset.mobs || unit.attack_target.kingdom.asset.mobs) == pMonsters)
			{
				unit.cancelAllBeh();
			}
		}
	}

	public void allDirty()
	{
		for (int i = 0; i < tiles_list.Length; i++)
		{
			WorldTile worldTile = tiles_list[i];
			tiles_dirty.Add(worldTile);
			tilemap.addToQueueToRedraw(worldTile);
		}
	}

	private void OnApplicationFocus(bool pFocus)
	{
		has_focus = pFocus;
	}

	private void OnApplicationPause(bool pPause)
	{
		has_focus = !pPause;
	}

	private void OnApplicationQuit()
	{
		DOTween.KillAll(false);
	}

	private void updateShake(float pElapsed)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (_shake_timer == 0f)
		{
			return;
		}
		if (_shake_timer > 0f)
		{
			_shake_timer -= pElapsed;
		}
		if (_shake_timer <= 0f)
		{
			_shake_timer = 0f;
			_shake_camera.position = new Vector3(0f, 0f);
			return;
		}
		if (_shake_interval_timer > 0f)
		{
			_shake_interval_timer -= pElapsed;
			return;
		}
		_shake_interval_timer = _shake_interval;
		Vector3 position = default(Vector3);
		if (_shake_x)
		{
			position.x = Randy.randomFloat(0f - _shake_intensity, _shake_intensity);
		}
		if (_shake_y)
		{
			position.y = Randy.randomFloat(0f - _shake_intensity, _shake_intensity);
		}
		_shake_camera.position = position;
	}

	public void startShake(float pDuration = 0.3f, float pInterval = 0.01f, float pIntensity = 2f, bool pShakeX = false, bool pShakeY = true)
	{
		_shake_timer = pDuration;
		_shake_interval = pInterval;
		_shake_intensity = pIntensity;
		_shake_x = pShakeX;
		_shake_y = pShakeY;
	}

	private void updateMapLayers(float pElapsed)
	{
		Bench.bench("heat", "game_total");
		heat.update(pElapsed);
		Bench.benchEnd("heat", "game_total", pSaveCounter: false, 0L);
		Bench.bench("map_chunk_manager", "game_total");
		map_chunk_manager.update(pElapsed);
		Bench.benchEnd("map_chunk_manager", "game_total", pSaveCounter: false, 0L);
		Bench.bench("map_layers", "game_total");
		for (int i = 0; i < _map_layers.Count; i++)
		{
			_map_layers[i].update(pElapsed);
		}
		Bench.benchEnd("map_layers", "game_total", pSaveCounter: false, 0L);
		Bench.bench("map_layers_draw", "game_total");
		for (int j = 0; j < _map_layers.Count; j++)
		{
			_map_layers[j].draw(pElapsed);
		}
		Bench.benchEnd("map_layers_draw", "game_total", pSaveCounter: false, 0L);
		Bench.bench("map_modules", "game_total");
		for (int k = 0; k < _map_modules.Count; k++)
		{
			_map_modules[k].update(pElapsed);
		}
		Bench.benchEnd("map_modules", "game_total", pSaveCounter: false, 0L);
	}

	public float calculateCurElapsed()
	{
		return Time.fixedDeltaTime * Config.time_scale_asset.multiplier;
	}

	private void clearFrameCaches()
	{
	}

	private void LateUpdate()
	{
		player_control.clearLateUpdate();
	}

	private void Update()
	{
		FPS.update();
		if (!Config.game_loaded)
		{
			return;
		}
		Config.parallel_jobs_updater = DebugConfig.isOn(DebugOption.ParallelJobsUpdater);
		Bench.bench_ai_enabled = DebugConfig.isOn(DebugOption.BenchAiEnabled);
		if (SmoothLoader.isLoading())
		{
			if (DebugConfig.isOn(DebugOption.GenerateNewMapOnMapLoadingError))
			{
				try
				{
					SmoothLoader.update(Time.deltaTime);
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
					generateNewMap();
					return;
				}
			}
			SmoothLoader.update(Time.deltaTime);
			return;
		}
		Randy.nextSeed();
		Bench.bench("game_total");
		ScrollingHelper.update();
		Bench.bench("move_camera", "game_total");
		move_camera.update();
		Bench.benchEnd("move_camera", "game_total", pSaveCounter: false, 0L);
		Bench.bench("mapbox_update_1", "game_total");
		stack_effects.light_blobs.Clear();
		_signal_manager.update();
		clearFrameCaches();
		Config.updateCrashMetadata();
		PlayerConfig.instance.update();
		Tooltip.checkClearAll();
		CursorTooltipHelper.update();
		delta_time = Time.fixedDeltaTime;
		fixed_delta_time = Time.fixedDeltaTime;
		game_stats.updateStats(Time.deltaTime);
		_is_paused = Config.paused || ScrollWindow.isWindowActive() || RewardedAds.isShowing();
		_cached_map_meta_asset = Zones.getMapMetaAsset();
		Bench.benchEnd("mapbox_update_1", "game_total", pSaveCounter: false, 0L);
		Bench.bench("battle_keeper", "game_total");
		BattleKeeperManager.update(delta_time);
		Bench.benchEnd("battle_keeper", "game_total", pSaveCounter: false, 0L);
		elapsed = calculateCurElapsed();
		if (Config.fps_lock_30)
		{
			elapsed *= 2f;
		}
		cursor_speed.update();
		Bench.bench("music_box", "game_total");
		MusicBox.inst.update(delta_time);
		Bench.benchEnd("music_box", "game_total", pSaveCounter: false, 0L);
		Bench.bench("auto_tester", "game_total");
		auto_tester.update(elapsed);
		Bench.benchEnd("auto_tester", "game_total", pSaveCounter: false, 0L);
		if (Config.isMobile && (RewardedAds.isShowing() || PlayInterstitialAd.isShowing()))
		{
			return;
		}
		Bench.bench("auto_save", "game_total");
		AutoSaveManager.update();
		Bench.benchEnd("auto_save", "game_total", pSaveCounter: false, 0L);
		Bench.bench("send_to_sql", "game_total");
		DBInserter.executeCommandsAsync();
		Bench.benchEnd("send_to_sql", "game_total", pSaveCounter: false, 0L);
		checkMainSimulationUpdate();
		delayed_actions_manager.update(elapsed, delta_time);
		tilemap.update(elapsed);
		Bench.bench("update_shake", "game_total");
		updateShake(elapsed);
		Bench.benchEnd("update_shake", "game_total", pSaveCounter: false, 0L);
		Bench.bench("update_panel", "game_total");
		map_stats.updateStatsForPanel(delta_time);
		Bench.benchEnd("update_panel", "game_total", pSaveCounter: false, 0L);
		Bench.bench("quality_changer", "game_total");
		quality_changer.update();
		Bench.benchEnd("quality_changer", "game_total", pSaveCounter: false, 0L);
		updateTransitionEffect();
		Bench.bench("update_controls", "game_total");
		player_control.updateControls();
		Bench.benchEnd("update_controls", "game_total", pSaveCounter: false, 0L);
		Bench.bench("zone_camera", "game_total");
		zone_camera.update();
		Bench.benchEnd("zone_camera", "game_total", pSaveCounter: false, 0L);
		Bench.bench("unit_select_effect", "game_total");
		if ((Object)(object)_unit_select_effect != (Object)null)
		{
			_unit_select_effect.update(elapsed);
		}
		Bench.benchEnd("unit_select_effect", "game_total", pSaveCounter: false, 0L);
		Bench.bench("zone_selection_effect", "game_total");
		zone_calculator.updateAnimationsAndSelections();
		Bench.benchEnd("zone_selection_effect", "game_total", pSaveCounter: false, 0L);
		Bench.bench("nameplates", "game_total");
		nameplate_manager.update();
		Bench.benchEnd("nameplates", "game_total", pSaveCounter: false, 0L);
		if (Config.time_scale_asset.render_skip)
		{
			if (_render_skip < 2)
			{
				_render_skip++;
			}
			else
			{
				_render_skip = 0;
				calculateVisibleObjects();
				renderStuff();
			}
		}
		else
		{
			calculateVisibleObjects();
			renderStuff();
		}
		Bench.bench("update_sprite_constructor", "game_total");
		updateDynamicSprites();
		Bench.benchEnd("update_sprite_constructor", "game_total", pSaveCounter: false, 0L);
		Bench.bench("light_renderer", "game_total");
		LightRenderer.instance.update(delta_time);
		Bench.benchEnd("light_renderer", "game_total", pSaveCounter: false, 0L);
		Bench.bench("update_finish", "game_total");
		updateFinish();
		Bench.benchEnd("update_finish", "game_total", pSaveCounter: false, 0L);
		Bench.bench("end_checks", "game_total");
		checkMinWindowSize();
		checkVersionCallbacks();
		Bench.update();
		Bench.benchEnd("end_checks", "game_total", pSaveCounter: false, 0L);
		Bench.benchEnd("game_total", "main", pSaveCounter: false, 0L);
	}

	private void checkMainSimulationUpdate()
	{
		int num = (ScrollWindow.isWindowActive() ? 1 : Config.time_scale_asset.ticks);
		for (int i = 0; i < num; i++)
		{
			updateSimulation(elapsed);
		}
	}

	private void updateTransitionEffect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (_world_layer_switch_effect.color.a != 0f)
		{
			Color color = _world_layer_switch_effect.color;
			color.a -= delta_time * 0.1f;
			if (color.a < 0f)
			{
				color.a = 0f;
			}
			_world_layer_switch_effect.color = color;
		}
	}

	private void updateSimulation(float pElapsed)
	{
		updateDirtyMetaContainersAndCleanup();
		explosion_checker.update(pElapsed);
		city_zone_helper.update(pElapsed);
		if (!isPaused())
		{
			updateTimerNutrition(pElapsed);
			Bench.bench("update_age", "game_total");
			map_stats.updateWorldTime(pElapsed);
			Bench.benchEnd("update_age", "game_total", pSaveCounter: false, 0L);
			Bench.bench("taxi", "game_total");
			TaxiManager.update(pElapsed);
			Bench.benchEnd("taxi", "game_total", pSaveCounter: false, 0L);
			Bench.bench("update_meta_history", "game_total");
			updateMetaHistory();
			Bench.benchEnd("update_meta_history", "game_total", pSaveCounter: false, 0L);
		}
		AnimationHelper.updateTime(pElapsed, delta_time);
		EnemiesFinder.clear();
		ControllableUnit.updateControllableUnit();
		updateMapLayers(pElapsed);
		updateCities(pElapsed);
		updateActors(pElapsed);
		updateBuildings(pElapsed);
		drop_manager.update(pElapsed);
		cultures.update(pElapsed);
		stack_effects.update(pElapsed);
		resource_throw_manager.update(pElapsed);
		updateWorldBehaviours(pElapsed);
		Bench.bench("army_manager", "game_total");
		armies.update(pElapsed);
		Bench.benchEnd("army_manager", "game_total", pSaveCounter: false, 0L);
		Bench.bench("kingdoms", "game_total");
		kingdoms.update(pElapsed);
		Bench.benchEnd("kingdoms", "game_total", pSaveCounter: false, 0L);
		Bench.bench("diplomacy", "game_total");
		diplomacy.update(pElapsed);
		Bench.benchEnd("diplomacy", "game_total", pSaveCounter: false, 0L);
		Bench.bench("subspecies", "game_total");
		subspecies.update(pElapsed);
		Bench.benchEnd("subspecies", "game_total", pSaveCounter: false, 0L);
		Bench.bench("plots", "game_total");
		plots.update(pElapsed);
		Bench.benchEnd("plots", "game_total", pSaveCounter: false, 0L);
		Bench.bench("clans", "game_total");
		clans.update(pElapsed);
		Bench.benchEnd("clans", "game_total", pSaveCounter: false, 0L);
		Bench.bench("alliances", "game_total");
		alliances.update(pElapsed);
		Bench.benchEnd("alliances", "game_total", pSaveCounter: false, 0L);
		Bench.bench("wars", "game_total");
		wars.update(pElapsed);
		Bench.benchEnd("wars", "game_total", pSaveCounter: false, 0L);
		Bench.bench("languages", "game_total");
		languages.update(pElapsed);
		Bench.benchEnd("languages", "game_total", pSaveCounter: false, 0L);
		Bench.bench("religions", "game_total");
		religions.update(pElapsed);
		Bench.benchEnd("religions", "game_total", pSaveCounter: false, 0L);
		Bench.bench("projectiles", "game_total");
		projectiles.update(pElapsed);
		Bench.benchEnd("projectiles", "game_total", pSaveCounter: false, 0L);
		Bench.bench("stasuses", "game_total");
		statuses.update(pElapsed);
		Bench.benchEnd("stasuses", "game_total", pSaveCounter: false, 0L);
		Bench.bench("era_manager", "game_total");
		era_manager.update(pElapsed);
		Bench.benchEnd("era_manager", "game_total", pSaveCounter: false, 0L);
	}

	private void updateMetaHistory()
	{
		if (!Config.graphs || Config.disable_db || Date.getCurrentMonth() != 12)
		{
			return;
		}
		int currentYear = Date.getCurrentYear();
		if (currentYear == map_stats.history_current_year)
		{
			return;
		}
		map_stats.history_current_year = currentYear;
		foreach (BaseSystemManager list_all_sim_manager in list_all_sim_managers)
		{
			list_all_sim_manager.startCollectHistoryData();
		}
		world_object.startCollectHistoryData();
		foreach (BaseSystemManager list_all_sim_manager2 in list_all_sim_managers)
		{
			list_all_sim_manager2.clearLastYearStats();
		}
		world_object.clearLastYearStats();
	}

	private void updateDirtyMetaContainersAndCleanup()
	{
		BuildingZonesSystem.update();
		checkSimManagerLists();
		units.checkContainer();
		buildings.checkContainer();
		sim_object_zones.update();
		Bench.bench("prepare_for_meta_checks", "game_total");
		units.prepareForMetaChecks();
		Bench.benchEnd("prepare_for_meta_checks", "game_total", pSaveCounter: false, 0L);
		Bench.bench("check_dirty_meta_units", "game_total");
		checkDirtyUnits();
		Bench.benchEnd("check_dirty_meta_units", "game_total", pSaveCounter: false, 0L);
		Bench.bench("check_dirty_meta_objects", "game_total");
		checkDirtyMetaObjects();
		Bench.benchEnd("check_dirty_meta_objects", "game_total", pSaveCounter: false, 0L);
		if (!isWindowOnScreen())
		{
			Bench.bench("check_meta_obj_destroy", "game_total");
			checkMetaObjectsDestroy();
			Bench.benchEnd("check_meta_obj_destroy", "game_total", pSaveCounter: false, 0L);
			Bench.bench("check_obj_destroy", "game_total");
			checkObjectsToDestroy();
			Bench.benchEnd("check_obj_destroy", "game_total", pSaveCounter: false, 0L);
		}
		checkSimManagerLists();
		Bench.bench("check_references_units", "game_total");
		checkEventUnitsDestroy();
		Bench.benchEnd("check_references_units", "game_total", pSaveCounter: false, 0L);
		Bench.bench("check_references_buildings", "game_total");
		checkEventBuildingsDestroy();
		Bench.benchEnd("check_references_buildings", "game_total", pSaveCounter: false, 0L);
		Bench.bench("check_references_houses", "game_total");
		checkEventHouses();
		Bench.benchEnd("check_references_houses", "game_total", pSaveCounter: false, 0L);
		Bench.bench("check_dirty_meta_objects_2", "game_total");
		checkDirtyMetaObjects();
		Bench.benchEnd("check_dirty_meta_objects_2", "game_total", pSaveCounter: false, 0L);
		Bench.bench("check_anything_changed", "game_total");
		checkAnyMetaAddedRemoved();
		Bench.benchEnd("check_anything_changed", "game_total", pSaveCounter: false, 0L);
	}

	private void checkEventUnitsDestroy()
	{
		if (!units.event_destroy)
		{
			return;
		}
		units.event_destroy = false;
		foreach (Actor unit in units)
		{
			if (unit.beh_actor_target != null && !unit.beh_actor_target.isAlive())
			{
				unit.beh_actor_target = null;
			}
			if (unit.attackedBy != null && !unit.attackedBy.isAlive())
			{
				unit.attackedBy = null;
			}
			if (unit.hasLover() && !unit.lover.isAlive())
			{
				unit.lover.lover = null;
				unit.lover = null;
			}
		}
		TaxiManager.removeDeadUnits();
	}

	private void checkEventBuildingsDestroy()
	{
		if (!buildings.event_destroy)
		{
			return;
		}
		List<Actor> simpleList = units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			if (actor.beh_building_target != null && !actor.beh_building_target.isAlive())
			{
				actor.beh_building_target = null;
			}
			if (actor.attackedBy != null && !actor.attackedBy.isAlive())
			{
				actor.attackedBy = null;
			}
		}
		buildings.event_destroy = false;
	}

	private void checkEventHouses()
	{
		if (!buildings.event_houses)
		{
			return;
		}
		foreach (Building occupied_building in buildings.occupied_buildings)
		{
			occupied_building.residents.Clear();
			if (occupied_building.asset.docks)
			{
				occupied_building.component_docks.clearBoatCounter();
			}
		}
		List<Actor> simpleList = units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			Actor actor = simpleList[i];
			actor.checkHomeBuilding();
			Building home_building = actor.home_building;
			if (home_building != null)
			{
				if (home_building.asset.docks)
				{
					home_building.component_docks.increaseBoatCounter(actor);
				}
				else
				{
					home_building.residents.Add(actor.data.id);
				}
			}
			Building inside_building = actor.inside_building;
			if (inside_building != null && (!inside_building.isUsable() || inside_building.isAbandoned()))
			{
				actor.exitBuilding();
				actor.cancelAllBeh();
			}
		}
		buildings.event_houses = false;
	}

	private void debugHouses()
	{
		foreach (Building building in buildings)
		{
			if (!building.isUsable() && building.countResidents() > 0)
			{
				Debug.LogError((object)("Building " + building.data.id + " has residents but is not usable"));
			}
			if (!building.asset.docks && building.countResidents() > building.asset.housing_slots)
			{
				Debug.LogError((object)(building.asset.id + " has more residents than housing " + building.countResidents() + "/" + building.asset.housing_slots));
			}
		}
	}

	public void checkSimManagerLists()
	{
		for (int i = 0; i < list_all_sim_managers.Count; i++)
		{
			list_all_sim_managers[i].checkLists();
		}
	}

	private void checkDirtyUnits()
	{
		bool flag = false;
		int num = 0;
		for (int i = 0; i < _list_meta_main_managers.Count; i++)
		{
			if (_list_meta_main_managers[i].isUnitsDirty())
			{
				flag = true;
				num++;
			}
		}
		if (!flag)
		{
			return;
		}
		if (num < 3)
		{
			subspecies.beginChecksUnits();
			families.beginChecksUnits();
			armies.beginChecksUnits();
			clans.beginChecksUnits();
			plots.beginChecksUnits();
			languages.beginChecksUnits();
			cultures.beginChecksUnits();
			religions.beginChecksUnits();
			cities.beginChecksUnits();
			kingdoms.beginChecksUnits();
			kingdoms_wild.beginChecksUnits();
		}
		else
		{
			Parallel.ForEach(_list_meta_main_managers, parallel_options, delegate(BaseSystemManager pSystem)
			{
				pSystem.parallelDirtyUnitsCheck();
			});
		}
	}

	private void checkDirtyMetaObjects()
	{
		kingdoms_wild.beginChecksBuildings();
		kingdoms.beginChecksBuildings();
		cities.beginChecksBuildings();
		kingdoms.beginChecksCities();
		religions.beginChecksKingdoms();
		religions.beginChecksCities();
		cultures.beginChecksKingdoms();
		cultures.beginChecksCities();
		languages.beginChecksKingdoms();
		languages.beginChecksCities();
	}

	private void checkAnyMetaAddedRemoved()
	{
		if (!BaseSystemManager.anything_changed)
		{
			return;
		}
		Config.selected_objects_graph.RemoveWhere((NanoObject pNanoObject) => pNanoObject.isRekt());
		if (ScrollWindow.isWindowActive())
		{
			ScrollWindow.checkElements();
			if (!MetaSwitchManager.isAnimationActive())
			{
				MetaSwitchManager.checkAndRefresh();
			}
		}
		SpriteSwitcher.checkAllStates();
		BaseSystemManager.anything_changed = false;
	}

	private void checkMetaObjectsDestroy()
	{
		if (_meta_skip)
		{
			_meta_skip = false;
			return;
		}
		foreach (BaseSystemManager list_meta_main_manager in _list_meta_main_managers)
		{
			list_meta_main_manager.checkDeadObjects();
		}
	}

	private void calculateVisibleObjects()
	{
		buildings.calculateVisibleBuildings();
		units.calculateVisibleActors();
	}

	public void resetRedrawTimer()
	{
		_redraw_timer = -1f;
	}

	private void renderStuff()
	{
		QuantumSpriteManager.update();
		Bench.bench("redraw_mini_map", "game_total");
		if (_redraw_timer > 0f)
		{
			_redraw_timer -= Time.deltaTime;
		}
		else
		{
			_redraw_timer = 0.001f;
			if (tiles_dirty.Count > 0)
			{
				redrawMiniMap();
			}
		}
		Bench.benchEnd("redraw_mini_map", "game_total", pSaveCounter: false, 0L);
		Bench.bench("redraw_tiles", "game_total");
		tilemap.redrawTiles();
		Bench.benchEnd("redraw_tiles", "game_total", pSaveCounter: false, 0L);
		Bench.bench("update_debug_texts", "game_total");
		updateDebugGroupSystem();
		Bench.benchEnd("update_debug_texts", "game_total", pSaveCounter: false, 0L);
	}

	private void updateFinish()
	{
		if (timer_nutrition_decay <= 0f)
		{
			timer_nutrition_decay = SimGlobals.m.interval_nutrition_decay;
		}
	}

	private void checkVersionCallbacks()
	{
		if (VersionCallbacks.timer > 0f)
		{
			VersionCallbacks.updateVC(elapsed);
		}
		if (Config.EVERYTHING_FIREWORKS)
		{
			spawnForcedFireworks();
		}
	}

	private void checkMinWindowSize()
	{
		if (!Screen.fullScreen)
		{
			if (Screen.width < 720)
			{
				Screen.SetResolution(720, Screen.height, false);
			}
			else if (Screen.height < 480)
			{
				Screen.SetResolution(Screen.width, 480, false);
			}
		}
	}

	private void checkObjectsToDestroy()
	{
		buildings.checkObjectsToDestroy();
		units.checkObjectsToDestroy();
	}

	private void updateWorldBehaviours(float pElapsed)
	{
		if (!DebugConfig.isOn(DebugOption.SystemWorldBehaviours))
		{
			return;
		}
		Bench.bench("world_beh", "game_total");
		List<WorldBehaviourAsset> list = AssetManager.world_behaviours.list;
		for (int i = 0; i < list.Count; i++)
		{
			WorldBehaviourAsset worldBehaviourAsset = list[i];
			if (worldBehaviourAsset.enabled)
			{
				Bench.bench(worldBehaviourAsset.id, "world_beh");
				worldBehaviourAsset.manager.update(pElapsed);
				Bench.benchEnd(worldBehaviourAsset.id, "world_beh", pSaveCounter: false, 0L);
			}
		}
		Bench.benchEnd("world_beh", "game_total", pSaveCounter: false, 0L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float getWorldTimeElapsedSince(double pTime)
	{
		return (float)(map_stats.world_time - pTime);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float getRealTimeElapsedSince(double pTime)
	{
		return (float)(getCurSessionTime() - pTime);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double getCurWorldTime()
	{
		return map_stats.world_time;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double getCurSessionTime()
	{
		return game_stats.data.gameTime;
	}

	public void updateDynamicSprites()
	{
		AssetManager.dynamic_sprites_library.checkDirty();
	}

	public void updateDebugGroupSystem()
	{
		_debug_text_group_system.update(elapsed);
	}

	internal void updateTimerNutrition(float pElapsed)
	{
		if (timer_nutrition_decay > 0f)
		{
			timer_nutrition_decay -= pElapsed;
		}
	}

	internal void updateObjectAge()
	{
		foreach (Actor unit in units)
		{
			unit.updateAge();
		}
		cities.updateAge();
		kingdoms.updateAge();
	}

	private void updateCities(float pElapsed)
	{
		if (DebugConfig.isOn(DebugOption.SystemUpdateCities))
		{
			Bench.bench("cities", "game_total");
			cities.update(pElapsed);
			Bench.benchEnd("cities", "game_total", pSaveCounter: false, 0L);
		}
	}

	private void updateBuildings(float pElapsed)
	{
		if (DebugConfig.isOn(DebugOption.SystemUpdateBuildings))
		{
			buildings.update(pElapsed);
		}
	}

	private void updateActors(float pElapsed)
	{
		if (DebugConfig.isOn(DebugOption.SystemUpdateUnits))
		{
			units.update(pElapsed);
		}
	}

	private void allTilesDirty()
	{
		tiles_dirty.Clear();
		tilemap.clear();
		for (int i = 0; i < tiles_list.Length; i++)
		{
			WorldTile tileDirty = tiles_list[i];
			setTileDirty(tileDirty);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void redrawRenderedTile(WorldTile pTile)
	{
		pTile.last_rendered_tile_type = null;
		setTileDirty(pTile);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void setTileDirty(WorldTile pTile)
	{
		pTile.updateStats();
		tiles_dirty.Add(pTile);
		if (tilemap.needsRedraw(pTile))
		{
			tilemap.addToQueueToRedraw(pTile);
			if (pTile.has_tile_up)
			{
				tilemap.addToQueueToRedraw(pTile.tile_up);
			}
			if (pTile.has_tile_down)
			{
				tilemap.addToQueueToRedraw(pTile.tile_down);
			}
			if (pTile.has_tile_left)
			{
				tilemap.addToQueueToRedraw(pTile.tile_left);
			}
			if (pTile.has_tile_right)
			{
				tilemap.addToQueueToRedraw(pTile.tile_right);
			}
		}
		world_layer_edges.addDirtyChunk(pTile.chunk);
		checkBehaviours(pTile);
	}

	internal void setZoomOrthographic(float pZoom)
	{
		quality_changer.setZoomOrthographic(pZoom);
	}

	public void redrawMiniMap(bool pForce = false)
	{
		if (!DebugConfig.isOn(DebugOption.SystemRedrawMap) || !(isRenderMiniMap() | pForce))
		{
			return;
		}
		dirty_tiles_last = tiles_dirty.Count;
		foreach (WorldTile item in tiles_dirty)
		{
			updateDirtyTile(item);
		}
		world_layer_edges.redraw();
		tiles_dirty.Clear();
		world_layer.updatePixels();
	}

	internal void checkBehaviours(WorldTile pTile)
	{
		if (pTile.Type.explodable_timed)
		{
			explosion_layer.addTimedTnt(pTile);
		}
		if (pTile.Type.can_be_filled_with_ocean)
		{
			WorldBehaviourOcean.tiles.Add(pTile);
		}
		else
		{
			WorldBehaviourOcean.tiles.Remove(pTile);
		}
	}

	private void updateDirtyTile(WorldTile pTile)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (pTile.hasBuilding())
		{
			_ = Color.grey;
			if (!(Color32.op_Implicit(pTile.building.getColorForMinimap(pTile)) == Color32.op_Implicit(Toolbox.clear)))
			{
				world_layer.pixels[pTile.data.tile_id] = pTile.building.getColorForMinimap(pTile);
				return;
			}
		}
		world_layer.pixels[pTile.data.tile_id] = pTile.getColor();
	}

	public void followUnit(Actor pActor)
	{
		SelectedUnit.clear();
		move_camera.focusOnAndFollow(pActor, null, null);
	}

	public void locateSelectedVillage()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		City selected_city = SelectedMetas.selected_city;
		ScrollWindow.hideAllEvent();
		move_camera.focusOn(Vector2.op_Implicit(selected_city.city_center));
	}

	public void locatePosition(Vector3 pVector)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (isGameplayControlsLocked())
		{
			ScrollWindow.hideAllEvent();
		}
		move_camera.focusOn(pVector);
	}

	public void locatePosition(Vector3 pVector, Action pFocusReachedCallback, Action pFocusCancelCallback)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (isGameplayControlsLocked())
		{
			ScrollWindow.hideAllEvent();
		}
		move_camera.focusOn(pVector, pFocusReachedCallback, pFocusCancelCallback);
	}

	public void locateAndFollow(Actor pActor, Action pFocusReachedCallback, Action pFocusCancelCallback)
	{
		if (isGameplayControlsLocked())
		{
			ScrollWindow.hideAllEvent();
		}
		move_camera.focusOnAndFollow(pActor, pFocusReachedCallback, pFocusCancelCallback);
	}

	public bool isSelectedPower(string pPower)
	{
		if (!isAnyPowerSelected())
		{
			return false;
		}
		if (selected_power.id == pPower)
		{
			return true;
		}
		return false;
	}

	public string getSelectedPowerID()
	{
		if (!isAnyPowerSelected())
		{
			return string.Empty;
		}
		return selected_power.id;
	}

	public MouseHoldAnimation getSelectedPowerHoldAnimation()
	{
		if (!isAnyPowerSelected())
		{
			return MouseHoldAnimation.Default;
		}
		return getSelectedPowerAsset().mouse_hold_animation;
	}

	public bool canDragMap()
	{
		if (!isAnyPowerSelected())
		{
			return true;
		}
		return getSelectedPowerAsset().can_drag_map;
	}

	public GodPower getSelectedPowerAsset()
	{
		if (!isAnyPowerSelected())
		{
			return null;
		}
		return AssetManager.powers.get(getSelectedPowerID());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isAnyPowerSelected()
	{
		return (Object)(object)selected_buttons.selectedButton != (Object)null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isPaused()
	{
		return _is_paused;
	}

	internal void spawnCongratulationFireworks()
	{
		City random = cities.getRandom();
		if (random != null)
		{
			Building random2 = Randy.getRandom(random.buildings);
			if (random2 != null && !random2.isUnderConstruction())
			{
				EffectsLibrary.spawn("fx_fireworks", random2.current_tile);
			}
		}
	}

	internal void spawnForcedFireworks()
	{
		WorldTile random = Randy.getRandom(tiles_list);
		PlayerConfig.dict["sound"].boolVal = true;
		EffectsLibrary.spawn("fx_fireworks", random);
	}

	public int getCivWorldPopulation()
	{
		int num = 0;
		foreach (Actor unit in units)
		{
			if (unit.isSapient())
			{
				num++;
			}
			if (unit.asset.is_boat)
			{
				num++;
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isRenderMiniMap()
	{
		return instance.quality_changer.isLowRes();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isRenderGameplay()
	{
		return !instance.quality_changer.isLowRes();
	}

	internal static void aye()
	{
		CornerAye.instance.startAye();
	}

	public MetaTypeAsset getCachedMapMetaAsset()
	{
		return _cached_map_meta_asset;
	}

	public ArchitectMood getArchitectMood()
	{
		if (_cached_architect_mood == null)
		{
			_cached_architect_mood = map_stats.getArchitectMood();
		}
		return _cached_architect_mood;
	}

	public Color getArchitectColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return getArchitectMood().getColor();
	}
}
