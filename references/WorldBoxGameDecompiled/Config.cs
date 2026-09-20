using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Beebyte.Obfuscator;
using UnityEngine;
using UnityEngine.CrashReportHandler;

[ObfuscateLiterals]
public class Config
{
	public static bool parallel_jobs_updater = true;

	public static bool parallel_chunk_manager = true;

	public static string versionCodeText = string.Empty;

	public static string gitCodeText = string.Empty;

	public static string versionCodeDate = string.Empty;

	public static string iname = string.Empty;

	internal static bool? gen = null;

	public static string testStreamingAssets = "test";

	public static bool ui_main_hidden = false;

	public static int WORLD_SAVE_VERSION = 17;

	public static string current_map_template = "continent";

	public static string customMapSize = "standard";

	public static int customZoneX = 0;

	public static int customZoneY = 0;

	public static int customPerlinScale = 10;

	public static int customRandomShapes = 10;

	public static int customWaterLevel = 10;

	public static int ZONE_AMOUNT_X = 4;

	public static int ZONE_AMOUNT_Y = 4;

	public static string customMapSizeDefault = "standard";

	public static string maxMapSize = "iceberg";

	public static int ZONE_AMOUNT_X_DEFAULT = 3;

	public static int ZONE_AMOUNT_Y_DEFAULT = 4;

	public const int MAP_BLOCK_SIZE = 64;

	public const int CHUNK_SIZE = 16;

	public const int TILES_IN_CHUNK = 256;

	public const int CITY_ZONE_SIZE = 8;

	public const int CITY_ZONE_TILES = 64;

	public const int TILES_IN_REGION = 256;

	public const int PREVIEW_MAP_SIZE = 512;

	public const float FOCUS_SCROLL_DELAY_PC = 0.4f;

	public const float FOCUS_SCROLL_DELAY_PHONE = 0.55f;

	public static WorldTimeScaleAsset time_scale_asset = null;

	public static bool fps_lock_30 = false;

	public static bool MODDED = false;

	public static bool EVERYTHING_MAGIC_COLOR = false;

	public static bool EVERYTHING_FIREWORKS = false;

	private static bool _paused = false;

	public static bool lockGameControls = false;

	internal static string steam_name;

	internal static string steam_id;

	internal static bool steam_language_allow_detect = true;

	internal static string discordId;

	internal static string discordName;

	internal static string discordDiscriminator;

	public static bool testAds = false;

	public static bool firebaseInitiating = false;

	public static bool firebaseChecked = false;

	public static bool firebaseEnabled = true;

	public static bool authEnabled = false;

	public const string firebaseDatabaseURL = "https://worldbox-g.firebaseio.com/";

	public const string baseURL = "https://versions.superworldbox.com";

	public const string currencyURL = "https://currency.superworldbox.com";

	public static bool adsInitialized = false;

	public static bool disable_dispose_logs = true;

	public static bool disable_loading_logs = false;

	public static bool disable_discord = false;

	public static bool disable_steam = false;

	public static bool disable_db = false;

	public static bool disable_startup_window = false;

	public static bool disable_tutorial = false;

	public static bool debug_log_meta_ranks = false;

	public static string debug_last_selected_power_button;

	public static string debug_last_window;

	public static int debug_worlds_loaded;

	public static WindowStats debug_window_stats;

	public static bool load_random_test_map = false;

	public static bool load_new_map = false;

	public static bool load_dragon = false;

	public static bool load_save_on_start = false;

	public static bool load_save_from_path = false;

	public static string load_test_save_path = "";

	public static bool load_test_map = false;

	public static int load_save_on_start_slot = 1;

	public static string auto_test_on_start = null;

	public static float LOAD_TIME_INIT = 0f;

	public static float LOAD_TIME_CREATE = 0f;

	public static float LOAD_TIME_GENERATE = 0f;

	public static float LAST_LOAD_TIME = 0f;

	public static bool editor_test_rewards_from_ads = false;

	private static bool _hpr = false;

	public static bool sprite_animations_on = true;

	public static bool shadows_active = false;

	public static bool tooltips_active = true;

	public static bool preload_windows = true;

	public static bool preload_quantum_sprites = true;

	public static bool preload_buildings = true;

	public static bool preload_units = true;

	public static bool autosaves = true;

	public static bool graphs = true;

	public static bool experimental_mode = false;

	public static bool wbb_confirmed = false;

	public static bool full_screen = true;

	public static bool firebase_available = false;

	public static bool upload_available = false;

	public static bool game_loaded = false;

	public static bool show_console_on_start = false;

	public static bool show_console_on_error = true;

	public static bool editor_maxim = false;

	public static bool editor_mastef = false;

	public static bool editor_nikon = false;

	public static bool editor_devs = false;

	public static bool fmod_test_build = false;

	public static bool isEditor = false;

	public static bool isMobile = false;

	public static bool isIos = false;

	public static bool isAndroid = false;

	public static bool isComputer = true;

	public static bool grey_goo_damaged = false;

	public static GodPower power_to_unlock;

	private static string _current_brush = "circ_5";

	public static string selected_trait_editor = string.Empty;

	public static readonly SelectedObjectsGraph selected_objects_graph = new SelectedObjectsGraph();

	private static bool _dragging_item = false;

	public static IDraggable dragging_item_object = null;

	public static Kingdom whisper_A;

	public static Kingdom whisper_B;

	public static Kingdom unity_A;

	public static Kingdom unity_B;

	private static float timer = 30f;

	private static bool skip = false;

	private static List<string> _loggedSelectedPowers = new List<string>();

	private static bool _scheduledGC = false;

	private static bool _scheduledGCUnload = false;

	public static string gv;

	public static bool worldLoading => SmoothLoader.isLoading();

	public static bool paused
	{
		get
		{
			return _paused;
		}
		set
		{
			_paused = value;
		}
	}

	public static bool hasPremium
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (editor_test_rewards_from_ads)
			{
				return false;
			}
			return _hpr;
		}
		set
		{
			_hpr = value;
		}
	}

	public static string current_brush
	{
		get
		{
			return _current_brush;
		}
		set
		{
			_current_brush = value;
			current_brush_data = Brush.get(value);
		}
	}

	public static BrushData current_brush_data { get; private set; }

	public static bool joyControls => false;

	public static string gs { get; } = "";

	public static void setDraggingObject(IDraggable pGameObject)
	{
		dragging_item_object = pGameObject;
		_dragging_item = true;
	}

	public static bool isDraggingObject(IDraggable pGameObject)
	{
		if (dragging_item_object == null)
		{
			return false;
		}
		if (dragging_item_object == pGameObject)
		{
			return true;
		}
		return false;
	}

	public static IDraggable getDraggingObject()
	{
		return dragging_item_object;
	}

	public static void clearDraggingObject()
	{
		_dragging_item = false;
		dragging_item_object = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isDraggingItem()
	{
		return _dragging_item;
	}

	public static void setWorldSpeed(WorldTimeScaleAsset pAsset, bool pUpdateDebug = true)
	{
		if (!pAsset.sonic & pUpdateDebug)
		{
			DebugConfig.setOption(DebugOption.SonicSpeed, pVal: false, pUpdateSpecialSettings: false);
		}
		time_scale_asset = pAsset;
	}

	public static void setWorldSpeed(string pID, bool pUpdateDebug = true)
	{
		setWorldSpeed(AssetManager.time_scales.get(pID), pUpdateDebug);
	}

	public static void nextWorldSpeed(bool pCycle = false)
	{
		setWorldSpeed(time_scale_asset.getNext(pCycle));
	}

	public static void prevWorldSpeed()
	{
		setWorldSpeed(time_scale_asset.getPrevious());
	}

	public static void setPortrait(bool pValue)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (pValue)
		{
			Screen.autorotateToPortrait = true;
			Screen.autorotateToPortraitUpsideDown = true;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;
			if ((int)Input.deviceOrientation == 2)
			{
				Screen.orientation = (ScreenOrientation)2;
			}
			else
			{
				Screen.orientation = (ScreenOrientation)1;
			}
			Screen.orientation = (ScreenOrientation)5;
			World.world.camera.ResetAspect();
		}
		else
		{
			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			if ((int)Input.deviceOrientation == 4)
			{
				Screen.orientation = (ScreenOrientation)4;
			}
			else
			{
				Screen.orientation = (ScreenOrientation)3;
			}
			Screen.orientation = (ScreenOrientation)5;
			World.world.camera.ResetAspect();
		}
	}

	public static void setAutorotation(bool pValue)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected I4, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		OptionAsset optionAsset = AssetManager.options_library.get("portrait");
		if (pValue)
		{
			Screen.autorotateToPortrait = true;
			Screen.autorotateToPortraitUpsideDown = true;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;
			DeviceOrientation deviceOrientation = Input.deviceOrientation;
			Screen.orientation = (ScreenOrientation)((deviceOrientation - 1) switch
			{
				0 => 1, 
				1 => 2, 
				2 => 3, 
				3 => 4, 
				_ => 1, 
			});
			Screen.orientation = (ScreenOrientation)5;
			optionAsset.interactable = false;
			World.world.camera.ResetAspect();
		}
		else
		{
			optionAsset.interactable = true;
			setPortrait(PlayerConfig.optionBoolEnabled("portrait"));
		}
	}

	public static void enableAutoRotation(bool pValue)
	{
	}

	public static bool skipCrashMetadata()
	{
		if (MODDED)
		{
			return true;
		}
		if (experimental_mode)
		{
			return true;
		}
		if ((!gen) ?? false)
		{
			return true;
		}
		return false;
	}

	[Skip]
	[SkipRename]
	[DoNotFake]
	public static void updateCrashMetadata()
	{
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		if (!game_loaded || SmoothLoader.isLoading() || skip)
		{
			return;
		}
		if (timer > 0f)
		{
			timer -= Time.fixedDeltaTime;
			return;
		}
		timer = 30f;
		if (skipCrashMetadata())
		{
			skip = true;
			CrashReportHandler.enableCaptureExceptions = false;
			return;
		}
		CrashReportHandler.enableCaptureExceptions = false;
		try
		{
			if (!string.IsNullOrEmpty(versionCodeText))
			{
				CrashReportHandler.SetUserMetadata("u_versionCodeText", versionCodeText);
			}
			if (!string.IsNullOrEmpty(gitCodeText))
			{
				CrashReportHandler.SetUserMetadata("u_gitCodeText", gitCodeText);
			}
			CrashReportHandler.SetUserMetadata("c_MODDED", MODDED.ToString());
			CrashReportHandler.SetUserMetadata("c_HAVE_PREMIUM", hasPremium.ToString());
			CrashReportHandler.SetUserMetadata("c_game_speed", time_scale_asset.id);
			CrashReportHandler.SetUserMetadata("c_sonic_speed", DebugConfig.isOn(DebugOption.SonicSpeed).ToString());
			CrashReportHandler.SetUserMetadata("c_show_map_names", Zones.showMapNames().ToString());
			if (DebugConfig.instance.debugButton.gameObject.activeSelf)
			{
				CrashReportHandler.SetUserMetadata("c_debug_button", "visible");
			}
			CrashReportHandler.SetUserMetadata("o_camera_lowRes", World.world.quality_changer.isLowRes().ToString());
			CrashReportHandler.SetUserMetadata("o_selected_power", World.world.getSelectedPowerID());
			CrashReportHandler.SetUserMetadata("c_map_mode", World.world.zone_calculator.getCurrentModeDebug().ToString());
			if (ScrollWindow.isWindowActive())
			{
				CrashReportHandler.SetUserMetadata("o_window_open", ScrollWindow.getCurrentWindow().screen_id);
			}
			else
			{
				CrashReportHandler.SetUserMetadata("o_window_open", "false");
			}
			string text = "";
			for (int i = 0; i < _loggedSelectedPowers.Count; i++)
			{
				text = text + _loggedSelectedPowers[i] + ",";
			}
			CrashReportHandler.SetUserMetadata("o_power_history", text);
			CrashReportHandler.SetUserMetadata("map_units", World.world.units.Count.ToString());
			CrashReportHandler.SetUserMetadata("map_buildings", World.world.buildings.Count.ToString());
			CrashReportHandler.SetUserMetadata("map_civ_kingdoms", World.world.kingdoms.Count.ToString());
			CrashReportHandler.SetUserMetadata("map_cultures", World.world.cultures.Count.ToString());
			CrashReportHandler.SetUserMetadata("map_layers", World.world.zone_calculator.zones.Count.ToString());
			CrashReportHandler.SetUserMetadata("map_chunks", World.world.map_chunk_manager.chunks.Length.ToString());
			CrashReportHandler.SetUserMetadata("map_drops_active", World.world.drop_manager.getActiveIndex().ToString());
			CrashReportHandler.SetUserMetadata("map_stackEffects_active", World.world.stack_effects.countActive().ToString());
			try
			{
				CrashReportHandler.SetUserMetadata("u_installMode", ((object)Application.installMode/*cast due to constrained. prefix*/).ToString());
				CrashReportHandler.SetUserMetadata("u_sandboxType", ((object)Application.sandboxType/*cast due to constrained. prefix*/).ToString());
				CrashReportHandler.SetUserMetadata("g_activeTier", ((object)Graphics.activeTier/*cast due to constrained. prefix*/).ToString());
			}
			catch (Exception)
			{
			}
		}
		catch (Exception ex2)
		{
			skip = true;
			Debug.LogError((object)ex2);
			throw;
		}
		CrashReportHandler.enableCaptureExceptions = true;
	}

	public static void logSelectedPower(GodPower pPower)
	{
		if (_loggedSelectedPowers.Count > 5)
		{
			_loggedSelectedPowers.RemoveAt(0);
		}
		_loggedSelectedPowers.Add(pPower.id);
	}

	public static void scheduleGC(string pWhere, bool pUnloadResources = false)
	{
		_scheduledGC = true;
		if (pUnloadResources)
		{
			_scheduledGCUnload = true;
		}
	}

	public static void checkGC()
	{
		if (_scheduledGC)
		{
			forceGC("scheduled", _scheduledGCUnload);
		}
	}

	public static void forceGC(string pWhere, bool pUnloadResources = false)
	{
		if (pUnloadResources)
		{
			Resources.UnloadUnusedAssets();
			GC.Collect(1, GCCollectionMode.Optimized, blocking: false);
		}
		else
		{
			GC.Collect(0, GCCollectionMode.Optimized, blocking: false);
		}
		_scheduledGC = false;
		_scheduledGCUnload = false;
	}

	public static float getScrollToGroupDelay()
	{
		if (!isMobile)
		{
			return 0.4f;
		}
		return 0.55f;
	}

	public static void fireworksCheck(bool pEnabled)
	{
		EVERYTHING_FIREWORKS = pEnabled;
		PlayerConfig.instance.data.fireworksCheck2025 = pEnabled;
		PlayerConfig.saveData();
	}

	public static void valCheck(bool pEnabled)
	{
		PlayerConfig.instance.data.valCheck2025 = pEnabled;
		PlayerConfig.saveData();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void pCheck(bool value)
	{
		PlayerConfig.instance.data.pPossible0507 = value;
		PlayerConfig.saveData();
	}

	public static void magicCheck(bool pEnabled)
	{
		EVERYTHING_MAGIC_COLOR = pEnabled;
		PlayerConfig.instance.data.magicCheck2025 = pEnabled;
		PlayerConfig.saveData();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void givePremium()
	{
		InAppManager.activatePrem();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void removePremium()
	{
		hasPremium = false;
		PlayerConfig.instance.data.premium = false;
		PlayerConfig.saveData();
		PremiumElementsChecker.checkElements();
	}
}
