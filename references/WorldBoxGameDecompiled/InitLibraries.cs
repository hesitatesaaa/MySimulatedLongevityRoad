using System;
using UnityEngine;

public class InitLibraries : MonoBehaviour
{
	public static bool initiated;

	public static bool initiated_main;

	private void Awake()
	{
		initLibs();
	}

	public static void initMainLibs()
	{
		if (initiated_main)
		{
			return;
		}
		initiated_main = true;
		try
		{
			Config.gv = Application.version;
			Config.iname = Application.installerName;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
		try
		{
			if (Application.genuineCheckAvailable)
			{
				Config.gen = Application.genuine;
			}
		}
		catch (Exception)
		{
			Config.gen = null;
		}
		LogHandler.init();
		AssetManager.initMain();
		GameProgress.init();
		PlayerConfig.init();
		LocalizedTextManager.init();
	}

	private void initLibs()
	{
		if (!initiated)
		{
			initiated = true;
			LogText.log("InitLibraries " + Config.gv, "initLibs", "st");
			DebugConfig.init();
			initMainLibs();
			AssetManager.init();
			DebugConfig.checkSonicTimeScales();
			NameGenerator.init();
			LogText.log("InitLibraries", "initLibs", "en");
		}
	}
}
