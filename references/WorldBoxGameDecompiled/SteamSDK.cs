using System;
using Proyecto26;
using RSG;
using Steamworks;
using UnityEngine;

public class SteamSDK : MonoBehaviour
{
	public const uint STEAM_APP_ID = 1206560u;

	internal static Promise steamInitialized;

	private bool _initiated;

	private static SteamSDK _instance;

	private static bool _should_quit;

	private static readonly string[] _supported_steam_languages;

	private void Start()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (_initiated)
		{
			return;
		}
		_initiated = true;
		bool flag = false;
		try
		{
			_instance = this;
			SteamClient.Init(1206560u, true);
			RestClient.DefaultRequestHeaders["wb-stmc"] = "true";
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Disabling Steam Integration");
			Debug.LogWarning((object)ex);
			RestClient.DefaultRequestHeaders["wb-stmc"] = "na";
			flag = true;
			_should_quit = true;
		}
		try
		{
			string text = ((object)SteamClient.SteamId/*cast due to constrained. prefix*/).ToString();
			if (!string.IsNullOrEmpty(text))
			{
				Config.steam_id = text;
				RestClient.DefaultRequestHeaders["wb-stm"] = text;
				Debug.Log((object)("S:" + Config.steam_id));
			}
			else
			{
				Debug.Log((object)"S:nf");
			}
		}
		catch (Exception)
		{
		}
		try
		{
			if (Config.steam_language_allow_detect)
			{
				Debug.Log((object)"s:Detect - Steam detecting language");
				string steamLanguage = getSteamLanguage();
				if (!string.IsNullOrEmpty(steamLanguage))
				{
					string language = LocalizedTextManager.instance.language;
					if (steamLanguage == "en" && language != "en")
					{
						Debug.Log((object)"s:Detect - Already have a language, not falling back to english");
					}
					else
					{
						LocalizedTextManager.instance.setLanguage(steamLanguage);
					}
				}
				Debug.Log((object)("s:Detect - language " + steamLanguage));
			}
		}
		catch (Exception)
		{
		}
		try
		{
			string name = SteamClient.Name;
			if (!string.IsNullOrEmpty(name))
			{
				Config.steam_name = name;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			if (SteamClient.RestartAppIfNecessary(1206560u))
			{
				Debug.Log((object)"Restart App from Steam launcher");
				_should_quit = true;
				flag = true;
			}
		}
		catch (Exception ex5)
		{
			Debug.Log((object)ex5);
		}
		if (_should_quit && !Config.disable_steam)
		{
			Application.Quit();
		}
		if (flag)
		{
			Debug.Log((object)"Steam is not available");
			steamInitialized.Reject(new Exception("Steam is not available"));
			Object.Destroy((Object)(object)_instance);
		}
		else
		{
			steamInitialized.Resolve();
		}
	}

	private static string getSteamLanguage()
	{
		switch (SteamApps.GameLanguage)
		{
		case "arabic":
			return "ar";
		case "schinese":
			return "cz";
		case "tchinese":
			return "ch";
		case "czech":
			return "cs";
		case "danish":
			return "da";
		case "dutch":
			return "nl";
		case "english":
			return "en";
		case "finnish":
			return "fn";
		case "french":
			return "fr";
		case "german":
			return "de";
		case "greek":
			return "gr";
		case "hungarian":
			return "hu";
		case "indonesian":
			return "id";
		case "italian":
			return "it";
		case "japanese":
			return "ja";
		case "korean":
		case "koreana":
			return "ko";
		case "norwegian":
			return "no";
		case "polish":
			return "pl";
		case "portuguese":
			return "pt";
		case "brazilian":
			return "br";
		case "romanian":
			return "ro";
		case "russian":
			return "ru";
		case "spanish":
			return "es";
		case "latam":
			return "es";
		case "swedish":
			return "sv";
		case "thai":
			return "th";
		case "turkish":
			return "tr";
		case "ukrainian":
			return "ua";
		case "vietnamese":
			return "vn";
		default:
			return string.Empty;
		}
	}

	private void OnDisable()
	{
		try
		{
			SteamClient.Shutdown();
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)ex);
			Object.Destroy((Object)(object)_instance);
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	static SteamSDK()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		steamInitialized = new Promise();
		_should_quit = false;
		_supported_steam_languages = new string[28]
		{
			"ar", "cz", "ch", "cs", "da", "nl", "en", "fn", "fr", "de",
			"gr", "hu", "it", "ja", "ko", "no", "pl", "pt", "br", "ro",
			"ru", "es", "es", "sv", "th", "tr", "ua", "vn"
		};
	}
}
