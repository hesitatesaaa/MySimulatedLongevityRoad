using System;
using Steamworks;
using UnityEngine;

public class SteamTracker : MonoBehaviour, IRichTracker
{
	private static bool steam_initialized = false;

	private SteamTracker instance;

	private static float timer = 10f;

	private void Start()
	{
		instance = this;
		SteamSDK.steamInitialized.Then((Action)delegate
		{
			init();
		}).Catch((Action<Exception>)delegate
		{
			Object.Destroy((Object)(object)instance);
		});
	}

	private void OnDestroy()
	{
		instance = null;
		PowerTracker.steamTracker = null;
	}

	private static bool init()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (SteamSDK.steamInitialized != null && (int)SteamSDK.steamInitialized.CurState == 2)
		{
			steam_initialized = true;
		}
		return steam_initialized;
	}

	public void trackViewing(string pText)
	{
		if (steam_initialized || init())
		{
			if (pText == "" || !LocalizedTextManager.stringExists(pText))
			{
				trackActivity("#Status_browsing");
				return;
			}
			SteamFriends.SetRichPresence("window", LocalizedTextManager.getText(pText));
			trackActivity("#Status_viewing");
		}
	}

	public void trackWatching()
	{
		trackActivity("#Status_watching");
	}

	public void trackUsing(string pPower)
	{
		if (steam_initialized || init())
		{
			SteamFriends.SetRichPresence("power", LocalizedTextManager.getText(pPower));
			trackActivity("#Status_using");
		}
	}

	public void updateUsing(int pAmount, string pPower = "")
	{
		if (steam_initialized || init())
		{
			if (pPower != "")
			{
				SteamFriends.SetRichPresence("power", LocalizedTextManager.getText(pPower));
			}
			SteamFriends.SetRichPresence("amount", pAmount.ToString());
			trackActivity("#Status_using_num");
		}
	}

	public void inspectKingdom(string pKingdom)
	{
		if (steam_initialized || init())
		{
			SteamFriends.SetRichPresence("kingdom", pKingdom);
			trackActivity("#Status_kingdom");
		}
	}

	public void inspectVillage(string pVillage)
	{
		if (steam_initialized || init())
		{
			SteamFriends.SetRichPresence("village", pVillage);
			trackActivity("#Status_village");
		}
	}

	public void inspectUnit(string pUnit)
	{
		if (steam_initialized || init())
		{
			SteamFriends.SetRichPresence("unit", pUnit);
			trackActivity("#Status_unit");
		}
	}

	public void spectatingUnit(string pUnit)
	{
		if (steam_initialized || init())
		{
			SteamFriends.SetRichPresence("unit", pUnit);
			trackActivity("#Status_spectating");
		}
	}

	public void updateDetails(StatisticsAsset pStat)
	{
		if (steam_initialized || init())
		{
			string localeID = pStat.getLocaleID();
			if (!string.IsNullOrEmpty(localeID))
			{
				SteamFriends.SetRichPresence("stat", localeID);
			}
			SteamFriends.SetRichPresence("value", pStat.last_value);
			trackActivity(pStat.steam_activity);
		}
	}

	public void trackActivity(string pText)
	{
		if (!steam_initialized && !init())
		{
			return;
		}
		timer = 10f;
		try
		{
			if (pText.Substring(0, 1) != "#")
			{
				Debug.LogError((object)pText);
			}
			else
			{
				SteamFriends.SetRichPresence("steam_display", pText);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)"Could not set Steam Rich Presence (Steam not running, or game not run as Administrator)");
			Debug.LogError((object)ex);
		}
	}

	private void Update()
	{
		if (!steam_initialized)
		{
			return;
		}
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			return;
		}
		timer = 10f;
		try
		{
			updateDetails(PowerTracker.activeStat);
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Steam Failed or Disabled");
			Debug.Log((object)ex);
			Object.Destroy((Object)(object)instance);
		}
	}
}
