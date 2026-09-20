using System;
using Proyecto26;
using UnityEngine;

public class GameLoadedEvent : BaseMapObject
{
	private void Awake()
	{
		LogText.log("GameLoadedEvent", "Awake", "st");
		LogText.log("GameLoadedEvent", "Awake", "en");
		setVersionData();
	}

	private void setVersionData()
	{
		Object obj = Resources.Load("texts/build_info");
		TextAsset val = (TextAsset)(object)((obj is TextAsset) ? obj : null);
		try
		{
			Config.versionCodeText = val.text.Split('$')[0];
			Config.versionCodeDate = val.text.Split('$')[1];
		}
		catch (Exception)
		{
			if ((Object)(object)val != (Object)null)
			{
				Config.versionCodeText = val.text;
				Config.versionCodeDate = "";
			}
		}
		try
		{
			RestClient.DefaultRequestHeaders["wb-build"] = Config.versionCodeText ?? "na";
		}
		catch (Exception)
		{
		}
		try
		{
			Object obj2 = Resources.Load("texts/git_info");
			TextAsset val2 = (TextAsset)(object)((obj2 is TextAsset) ? obj2 : null);
			if ((Object)(object)val2 != (Object)null)
			{
				Config.gitCodeText = val2.text;
			}
			try
			{
				RestClient.DefaultRequestHeaders["wb-git"] = Config.gitCodeText ?? "na";
			}
			catch (Exception)
			{
			}
		}
		catch (Exception ex4)
		{
			Debug.Log((object)ex4);
		}
	}

	internal override void create()
	{
		base.create();
		Config.LOAD_TIME_INIT = Time.realtimeSinceStartup;
		LogText.log("GameLoadedEvent", "create");
		LocalizedTextManager.instance.setLanguage(PlayerConfig.dict["language"].stringVal);
		if (Globals.TRAILER_MODE)
		{
			TrailerModeSettings.startEvent();
		}
		World.world.startTheGame();
		GodPower.diagnostic();
		Config.updateCrashMetadata();
	}
}
