using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DebugConfig : MonoBehaviour
{
	public GameObject debugButton;

	public static DebugConfig instance;

	private static Dictionary<DebugOption, bool> _dictionary;

	private static int _pos_x = 20;

	public static List<string> default_debug_tools = new List<string>();

	public static bool debug_enabled => instance.debugButton.gameObject.activeSelf;

	private void Start()
	{
		instance = this;
		if (isOn(DebugOption.DisablePremium) || isOn(DebugOption.TestAds))
		{
			debugButton.SetActive(true);
			if (isOn(DebugOption.DisablePremium))
			{
				Debug.LogError((object)"[DEBUG] Premium is disabled via debug menu!");
				Debug.Log((object)"This option is only for beta testers. If you purchased premium, you won't have it until you disable it.");
				Debug.Log((object)"Disable this option in the debug menu and restart the game.");
			}
			if (isOn(DebugOption.TestAds))
			{
				Debug.LogError((object)"[DEBUG] Test Ads are enabled via debug menu!");
				Debug.Log((object)"This option is only for beta testers. You most likely don't want to play with it.");
				Debug.Log((object)"Disable this option in the debug menu and restart the game.");
			}
		}
	}

	private static void enableDefaultOptions()
	{
		setOption(DebugOption.GenerateNewMapOnMapLoadingError, pVal: true);
		setOption(DebugOption.LavaGlow, pVal: true);
		setOption(DebugOption.UseCameraAspect, pVal: true);
		setOption(DebugOption.UseGlobalPathLock, pVal: true);
		setOption(DebugOption.SystemBuildTick, pVal: true);
		setOption(DebugOption.SystemCityPlaceFinder, pVal: true);
		setOption(DebugOption.SystemProduceNewCitizens, pVal: true);
		setOption(DebugOption.SystemUnitPathfinding, pVal: true);
		setOption(DebugOption.SystemWorldBehaviours, pVal: true);
		setOption(DebugOption.SystemZoneGrowth, pVal: true);
		setOption(DebugOption.SystemCheckUnitAction, pVal: true);
		setOption(DebugOption.SystemUpdateUnitAnimation, pVal: true);
		setOption(DebugOption.SystemUpdateBuildings, pVal: true);
		setOption(DebugOption.SystemUpdateUnits, pVal: true);
		setOption(DebugOption.SystemUpdateCities, pVal: true);
		setOption(DebugOption.SystemRedrawMap, pVal: true);
		setOption(DebugOption.SystemUpdateDirtyChunks, pVal: true);
		setOption(DebugOption.SystemCheckGoodForBoat, pVal: false);
		setOption(DebugOption.SystemCityTasks, pVal: true);
		setOption(DebugOption.InspectObjectsOnClick, pVal: true);
		setOption(DebugOption.SystemMusic, pVal: false);
		setOption(DebugOption.Greg, pVal: false);
		setOption(DebugOption.MakeUnitsFollowCursor, pVal: false);
		setOption(DebugOption.SystemSplitAstar, pVal: true);
		setOption(DebugOption.UseCacheForRegionPath, pVal: true);
		setOption(DebugOption.ParallelJobsUpdater, pVal: true);
		setOption(DebugOption.ParallelChunks, pVal: true);
		setOption(DebugOption.ScaleEffectEnabled, pVal: true);
		if (Config.editor_devs && !Config.editor_nikon)
		{
			setOption(DebugOption.ExportAssetLibraries, pVal: true);
			setOption(DebugOption.GenerateGameplayReport, pVal: true);
			setOption(DebugOption.TesterLibs, pVal: true);
		}
		if (Config.isEditor)
		{
			setOption(DebugOption.DebugButton, pVal: true);
		}
	}

	public static void initDict()
	{
		_dictionary = new Dictionary<DebugOption, bool>();
		foreach (DebugOption value in Enum.GetValues(typeof(DebugOption)))
		{
			setOption(value, pVal: false);
		}
	}

	public static void init()
	{
		if (_dictionary != null)
		{
			return;
		}
		initDict();
		enableDefaultOptions();
		if (PlayerConfig.instance != null)
		{
			if (PlayerConfig.instance.data.premiumDisabled)
			{
				setOption(DebugOption.DisablePremium, pVal: true);
			}
			if (PlayerConfig.instance.data.testAds)
			{
				setOption(DebugOption.TestAds, pVal: true);
			}
		}
		if (Config.fmod_test_build)
		{
			Config.disable_discord = true;
			Config.disable_steam = true;
		}
		if (Config.isEditor)
		{
			Config.show_console_on_error = false;
			Config.customMapSizeDefault = "standard";
			setOption(DebugOption.Graphy, pVal: true);
		}
		if (Config.editor_maxim)
		{
			editorMaximOptions();
		}
		if (Config.editor_mastef)
		{
			editorMastefOptions();
		}
		if (Config.editor_nikon)
		{
			editorNikonOptions();
		}
	}

	private static void editorNikonOptions()
	{
		Config.show_console_on_error = false;
		Config.disable_discord = true;
		Config.disable_steam = true;
		Config.load_save_on_start = false;
		Config.load_save_on_start_slot = 8;
		Config.disable_startup_window = false;
		Config.disable_tutorial = true;
		MusicBox.debug_sounds = false;
		setOption(DebugOption.DebugButton, pVal: true);
		setOption(DebugOption.Graphy, pVal: false);
		setOption(DebugOption.PauseOnStart, pVal: true);
		setOption(DebugOption.ShowHiddenStats, pVal: true);
		setOption(DebugOption.DebugWindowHotkeys, pVal: true);
		setOption(DebugOption.DebugUnitHotkeys, pVal: true);
	}

	internal static void debugToolMastefDefaults(DebugTool pTool)
	{
		pTool.sort_order_reversed = false;
		pTool.sort_by_values = true;
		pTool.show_averages = false;
		pTool.hide_zeroes = false;
		pTool.show_counter = true;
		pTool.show_max = false;
		pTool.paused = false;
		pTool.hide_zeroes = true;
		pTool.state = DebugToolState.Values;
	}

	private static void editorMastefOptions()
	{
		Plot.DEBUG_PLOTS = false;
		Config.show_console_on_error = false;
		Config.disable_loading_logs = true;
		Config.disable_discord = true;
		Config.disable_steam = true;
		MusicBox.debug_sounds = false;
		Config.disable_startup_window = true;
		Config.disable_dispose_logs = true;
		setOption(DebugOption.OverlaySoundsActive, pVal: false);
		setOption(DebugOption.ShowLayoutGroupGrid, pVal: false);
		setOption(DebugOption.BenchAiEnabled, pVal: false);
		setOption(DebugOption.DebugTooltipUI, pVal: false);
		setOption(DebugOption.ParallelJobsUpdater, pVal: false);
		setOption(DebugOption.ParallelChunks, pVal: false);
		setOption(DebugOption.SonicSpeed, pVal: false);
		setOption(DebugOption.Graphy, pVal: false);
		MapBox.on_world_loaded = (Action)Delegate.Combine(MapBox.on_world_loaded, (Action)delegate
		{
		});
	}

	private static void debugBoats()
	{
		setOption(DebugOption.OverlayBoatTransport, pVal: true);
		setOption(DebugOption.BoatPassengerLines, pVal: true);
		setOption(DebugOption.ActorGizmosBoatTaxiRequestTargets, pVal: true);
		setOption(DebugOption.ActorGizmosBoatTaxiTarget, pVal: true);
		setOption(DebugOption.RenderIslandsTileCorners, pVal: true);
		setOption(DebugOption.RegionNeighbours, pVal: true);
		setOption(DebugOption.Region, pVal: true);
		addDebugTool("boat");
		addDebugTool("taxi");
		addDebugTool("tile_info");
	}

	private static void debugReproduction()
	{
		addDebugTool("reproduction_diagnostic_cursor");
		addDebugTool("reproduction_diagnostic_total");
		addDebugTool("city_jobs");
		addDebugTool("city_storage");
	}

	private static void debugMapChunks()
	{
		addDebugTool("map_chunks");
		addDebugTool("benchmark_chunks");
	}

	private static void editorMaximOptions()
	{
		setOption(DebugOption.GenerateNewMapOnMapLoadingError, pVal: false);
		Config.disable_discord = true;
		Config.disable_steam = true;
		Config.disable_startup_window = true;
		Config.show_console_on_error = false;
		Config.disable_tutorial = true;
		setOption(DebugOption.ControlledUnitsAttackRaycast, pVal: true);
		setOption(DebugOption.DebugWindowHotkeys, pVal: true);
		setOption(DebugOption.CivDrawCityClaimZone, pVal: true);
		setOption(DebugOption.PauseOnStart, pVal: true);
		setOption(DebugOption.ShowHiddenStats, pVal: true);
		setOption(DebugOption.DebugUnitHotkeys, pVal: true);
		setOption(DebugOption.ShowWarriorsCityText, pVal: true);
		setOption(DebugOption.ShowCityWeaponsText, pVal: true);
		setOption(DebugOption.ShowFoodCityText, pVal: true);
		setOption(DebugOption.CursorUnitAttackRange, pVal: true);
		setOption(DebugOption.ShowAmountNearArmy, pVal: true);
		for (int i = 0; i < 100; i++)
		{
			Debug.Log((object)"remember the cant");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isOn(DebugOption pOption)
	{
		return _dictionary[pOption];
	}

	public static void switchOption(DebugOption pOption)
	{
		setOption(pOption, !isOn(pOption));
		if (pOption == DebugOption.DisablePremium && PlayerConfig.instance != null)
		{
			PlayerConfig.instance.data.premiumDisabled = isOn(pOption);
			PlayerConfig.instance.data.clearDebugOnStart = false;
			PlayerConfig.saveData();
			PremiumElementsChecker.checkElements();
			disablePremiumNotify();
		}
		if (pOption == DebugOption.TestAds && PlayerConfig.instance != null)
		{
			PlayerConfig.instance.data.testAds = isOn(pOption);
			PlayerConfig.instance.data.clearDebugOnStart = false;
			PlayerConfig.saveData();
			PremiumElementsChecker.checkElements();
			testAdsNotify();
		}
	}

	public static void disablePremiumNotify()
	{
		if (PlayerConfig.instance != null)
		{
			if (isOn(DebugOption.DisablePremium))
			{
				WorldTip.showNow("Premium is blocked! Even after restart!", pTranslate: false, "top", 10f);
			}
			else
			{
				WorldTip.showNow("Premium is unblocked", pTranslate: false, "top");
			}
		}
	}

	public static void testAdsNotify()
	{
		if (PlayerConfig.instance != null)
		{
			if (isOn(DebugOption.TestAds))
			{
				WorldTip.showNow("Test Ads are enabled! Even after restart!", pTranslate: false, "top", 10f);
			}
			else
			{
				WorldTip.showNow("Test Ads disabled", pTranslate: false, "top");
			}
		}
	}

	public static void setOption(DebugOption pOption, bool pVal, bool pUpdateSpecialSettings = true)
	{
		if (_dictionary == null)
		{
			init();
		}
		_dictionary[pOption] = pVal;
		if (!pUpdateSpecialSettings)
		{
			return;
		}
		if (pOption == DebugOption.Graphy)
		{
			checkGraphy();
		}
		if (pOption == DebugOption.SonicSpeed && Config.game_loaded)
		{
			if (pVal)
			{
				Config.setWorldSpeed("x40", pUpdateDebug: false);
			}
			else
			{
				Config.setWorldSpeed("x1", pUpdateDebug: false);
			}
		}
	}

	public static void checkSonicTimeScales()
	{
		if (isOn(DebugOption.SonicSpeed))
		{
			Config.setWorldSpeed("x40", pUpdateDebug: false);
		}
		else
		{
			Config.setWorldSpeed("x1", pUpdateDebug: false);
		}
	}

	public static void checkGraphy()
	{
		if ((Object)(object)PrefabLibrary.instance != (Object)null)
		{
			PrefabLibrary.instance.graphy.gameObject.SetActive(isOn(DebugOption.Graphy));
		}
	}

	public static void createTool(string pID, int pX = 80, int pY = -10, int pWidth = -1)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		Bench.bench_enabled = true;
		DebugToolAsset asset = AssetManager.debug_tool_library.get(pID);
		DebugTool debugTool = Object.Instantiate<DebugTool>(PrefabLibrary.instance.debugTool, ((Component)instance).transform);
		debugTool.setAsset(asset);
		debugTool.populateOptions();
		for (int i = 0; i < debugTool.dropdown.options.Count; i++)
		{
			if (debugTool.dropdown.options[i].text == pID)
			{
				debugTool.dropdown.value = i;
				debugTool.dropdown.captionText.text = pID;
				break;
			}
		}
		((Component)debugTool).transform.localPosition = new Vector3((float)pX, (float)pY);
		_pos_x += 128;
	}

	private static void addDebugTool(string pID)
	{
		default_debug_tools.Add(pID);
	}
}
