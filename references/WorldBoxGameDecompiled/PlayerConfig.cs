using System;
using System.Collections.Generic;
using System.IO;
using Firebase.Analytics;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerConfig
{
	public static PlayerConfig instance;

	public static Dictionary<string, PlayerOptionData> dict = new Dictionary<string, PlayerOptionData>();

	private string dataPath;

	internal PlayerConfigData data;

	private float rewardCheckTimer = 10f;

	private float rewardCheckTimerInterval = 60f;

	private static bool _memory_check_done = false;

	public static void init()
	{
		if (instance == null)
		{
			Debug.Log((object)"INIT PlayerConfig");
			instance = new PlayerConfig();
			instance.create();
		}
	}

	public void create()
	{
		rewardCheckTimer = rewardCheckTimerInterval;
		setNewDataPath();
		Debug.Log((object)"Init PlayerConfig");
		if (File.Exists(dataPath))
		{
			try
			{
				loadData();
				return;
			}
			catch (Exception)
			{
				initNewSave();
				return;
			}
		}
		initNewSave();
	}

	internal void start()
	{
		AdButtonTimer.setAdTimer();
	}

	internal void update()
	{
	}

	private void setNewDataPath()
	{
		dataPath = Application.persistentDataPath + "/worldboxData";
	}

	private void initNewSave()
	{
		data = new PlayerConfigData();
		data.initData();
		dict["language"].stringVal = detectLanguage();
		Config.steam_language_allow_detect = true;
		if (Globals.specialAbstudio)
		{
			dict["language"].stringVal = "fa";
		}
		saveData();
	}

	public static void setFirebaseProp(string pName, string pProp)
	{
		if (!Config.isMobile || !Config.firebase_available)
		{
			return;
		}
		try
		{
			FirebaseAnalytics.SetUserProperty(pName, pProp);
		}
		catch (Exception)
		{
		}
	}

	public static void toggleFullScreen()
	{
		setFullScreen(!dict["fullscreen"].boolVal);
	}

	public static void setFullScreen(bool pFullScreen, bool pSwitchScreen = true)
	{
		dict["fullscreen"].boolVal = pFullScreen;
		saveData();
		OptionAsset optionAsset = AssetManager.options_library.get("fullscreen");
		optionAsset.action(optionAsset);
	}

	public static string detectLanguage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected I4, but got Unknown
		string text = "en";
		SystemLanguage systemLanguage = Application.systemLanguage;
		return (systemLanguage - 1) switch
		{
			38 => "vn", 
			35 => "th", 
			25 => "no", 
			24 => "lt", 
			34 => "sv", 
			16 => "he", 
			12 => "fn", 
			8 => "nl", 
			19 => "id", 
			28 => "ro", 
			17 => "hu", 
			6 => "cs", 
			40 => "ch", 
			39 => "cz", 
			9 => "en", 
			13 => "fr", 
			14 => "de", 
			26 => "pl", 
			37 => "ua", 
			29 => "ru", 
			31 => "sk", 
			33 => "es", 
			36 => "tr", 
			21 => "ja", 
			22 => "ko", 
			27 => "pt", 
			20 => "it", 
			7 => "da", 
			15 => "gr", 
			30 => "hr", 
			0 => "ar", 
			_ => "en", 
		};
	}

	public static void saveData()
	{
		string pStringData = Toolbox.encode(instance.data.toJson());
		Toolbox.WriteSafely("Player Config", instance.dataPath, ref pStringData);
		foreach (PlayerOptionData value in dict.Values)
		{
			if (value.boolVal)
			{
				setFirebaseProp("option_" + value.name, value.boolVal ? "on" : "off");
			}
		}
		setFirebaseProp("option_language", dict["language"].stringVal);
	}

	private void loadData()
	{
		if (!File.Exists(dataPath))
		{
			return;
		}
		string text = File.ReadAllText(dataPath);
		string text2 = "";
		try
		{
			text2 = Toolbox.decode(text);
		}
		catch (Exception)
		{
			text2 = "";
		}
		if (string.IsNullOrEmpty(text2))
		{
			try
			{
				text2 = Toolbox.decodeMobile(text);
			}
			catch (Exception)
			{
				text2 = "";
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			text = text2;
		}
		if (text.Contains("list"))
		{
			data = JsonConvert.DeserializeObject<PlayerConfigData>(text);
			data.initData();
			string stringVal = data.get("language").stringVal;
			if (stringVal == "boat" || stringVal == "keys")
			{
				data.get("language").stringVal = detectLanguage();
			}
		}
		else
		{
			initNewSave();
		}
		if (data.fireworksCheck2025)
		{
			Config.EVERYTHING_FIREWORKS = true;
		}
		if (data.magicCheck2025)
		{
			Config.EVERYTHING_MAGIC_COLOR = true;
		}
		if (Config.isEditor && Config.editor_test_rewards_from_ads)
		{
			data.rewardedPowers.Clear();
		}
		if (data.premium)
		{
			Config.hasPremium = true;
		}
		bool flag = false;
		if (moveTraitsAndAchievements())
		{
			flag = true;
		}
		if (handleDebugOptions())
		{
			flag = true;
		}
		if (flag)
		{
			saveData();
		}
	}

	internal static bool optionEnabled(string gameOptionName, OptionType pType)
	{
		foreach (PlayerOptionData item in instance.data.list)
		{
			if (!(item.name != gameOptionName) && pType == OptionType.Bool)
			{
				return item.boolVal;
			}
		}
		return false;
	}

	public static int getIntValue(string pID)
	{
		PlayerOptionData playerOptionData = dict[pID];
		OptionAsset optionAsset = AssetManager.options_library.get(pID);
		if (playerOptionData.intVal != Mathf.Clamp(playerOptionData.intVal, optionAsset.min_value, optionAsset.max_value))
		{
			return optionAsset.default_int;
		}
		return playerOptionData.intVal;
	}

	public static bool optionBoolEnabled(string pName)
	{
		return dict[pName].boolVal;
	}

	public static int getOptionInt(string pName)
	{
		return dict[pName].intVal;
	}

	public static string getOptionString(string pName)
	{
		return dict[pName].stringVal;
	}

	public static void setOptionBool(string pName, bool pVal)
	{
		dict[pName].boolVal = pVal;
	}

	public static void setOptionInt(string pName, int pVal)
	{
		dict[pName].intVal = pVal;
	}

	public static void setOptionString(string pName, string pVal)
	{
		dict[pName].stringVal = pVal;
	}

	[Obsolete]
	internal static void switchOption(string gameOptionName, OptionType pType)
	{
		foreach (PlayerOptionData item in instance.data.list)
		{
			if (!(item.name != gameOptionName) && pType == OptionType.Bool)
			{
				item.boolVal = !item.boolVal;
			}
		}
		checkSettings();
	}

	public static void setVsync(bool vsyncEnabled)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (vsyncEnabled)
		{
			Resolution currentResolution = Screen.currentResolution;
			RefreshRate refreshRateRatio = ((Resolution)(ref currentResolution)).refreshRateRatio;
			if (((RefreshRate)(ref refreshRateRatio)).value < 61.0)
			{
				QualitySettings.vSyncCount = 1;
				return;
			}
			currentResolution = Screen.currentResolution;
			refreshRateRatio = ((Resolution)(ref currentResolution)).refreshRateRatio;
			if (((RefreshRate)(ref refreshRateRatio)).value < 121.0)
			{
				QualitySettings.vSyncCount = 2;
				return;
			}
			currentResolution = Screen.currentResolution;
			refreshRateRatio = ((Resolution)(ref currentResolution)).refreshRateRatio;
			if (((RefreshRate)(ref refreshRateRatio)).value < 181.0)
			{
				QualitySettings.vSyncCount = 3;
			}
			else
			{
				QualitySettings.vSyncCount = 4;
			}
			return;
		}
		QualitySettings.vSyncCount = 0;
		if (Config.fps_lock_30)
		{
			if (Application.targetFrameRate != 30)
			{
				Application.targetFrameRate = 30;
			}
		}
		else if (Application.targetFrameRate != 60)
		{
			Application.targetFrameRate = 60;
		}
	}

	public static void turnOffAssetsPreloading()
	{
		setOptionBool("preload_units", pVal: false);
		setOptionBool("preload_buildings", pVal: false);
		setOptionBool("preload_quantum_sprites", pVal: false);
		setOptionBool("preload_windows", pVal: false);
	}

	internal static void checkSettings()
	{
		if (SystemInfo.systemMemorySize < 3000 && !_memory_check_done)
		{
			_memory_check_done = true;
			Debug.Log((object)("[RAM is MEH] SystemInfo.systemMemorySize: " + SystemInfo.systemMemorySize));
			turnOffAssetsPreloading();
		}
		foreach (OptionAsset item in AssetManager.options_library.list)
		{
			if (!item.computer_only || Config.isComputer)
			{
				if (item.reset_to_default_on_launch)
				{
					setOptionBool(item.id, item.default_bool);
					setOptionInt(item.id, item.default_int);
					setOptionString(item.id, item.default_string);
				}
				item.action?.Invoke(item);
			}
		}
	}

	public static int countRewards()
	{
		if (instance?.data?.rewardedPowers != null)
		{
			return instance.data.rewardedPowers.Count;
		}
		return 0;
	}

	public static void clearRewards()
	{
		instance?.data?.rewardedPowers?.Clear();
	}

	private bool moveTraitsAndAchievements()
	{
		bool result = false;
		List<string> achievements = data.achievements;
		if (achievements != null && achievements.Count > 0)
		{
			foreach (string achievement in data.achievements)
			{
				GameProgress.unlockAchievement(achievement);
			}
			data.achievements.Clear();
			result = true;
		}
		List<string> unlocked_traits = data.unlocked_traits;
		if (unlocked_traits != null && unlocked_traits.Count > 0)
		{
			foreach (string unlocked_trait in data.unlocked_traits)
			{
				AssetManager.traits.get(unlocked_trait)?.unlock();
			}
			data.unlocked_traits.Clear();
			result = true;
		}
		return result;
	}

	private bool handleDebugOptions()
	{
		bool result = false;
		if (data.clearDebugOnStart)
		{
			DebugConfig.setOption(DebugOption.DisablePremium, pVal: false);
			DebugConfig.setOption(DebugOption.TestAds, pVal: false);
			data.clearDebugOnStart = false;
			data.premiumDisabled = false;
			data.testAds = false;
			result = true;
		}
		else
		{
			if (data.premiumDisabled)
			{
				DebugConfig.setOption(DebugOption.DisablePremium, pVal: true);
				data.clearDebugOnStart = true;
				result = true;
			}
			if (data.testAds)
			{
				DebugConfig.setOption(DebugOption.TestAds, pVal: true);
				data.clearDebugOnStart = true;
				result = true;
			}
		}
		return result;
	}
}
