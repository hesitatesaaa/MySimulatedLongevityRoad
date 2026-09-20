using System.Collections.Generic;
using UnityEngine;

internal class PowerTracker : MonoBehaviour
{
	private static int amount = 0;

	private static PowerTracker instance;

	private static bool frameDone = false;

	internal static SteamTracker steamTracker;

	internal static DiscordTracker discordTracker;

	private static bool initiated = false;

	internal static StatisticsAsset activeStat;

	internal static string statValue = "";

	private static float timer = 10f;

	private static float secTimer = 1f;

	private static int currentIndex = 0;

	private static List<StatisticsAsset> rotateStats => StatisticsLibrary.power_tracker_pool;

	private void Start()
	{
		instance = this;
		if (!Config.disable_discord)
		{
			discordTracker = ((Component)this).gameObject.AddComponent<DiscordTracker>();
		}
		if (!Config.disable_steam)
		{
			steamTracker = ((Component)this).gameObject.AddComponent<SteamTracker>();
		}
		initiated = true;
	}

	internal static void PlusOne(GodPower pPower = null)
	{
		if (!((Object)(object)instance == (Object)null) && !MoveCamera.hasFocusUnit() && pPower != null)
		{
			frameDone = true;
			amount++;
			discordTracker?.updateUsing(amount, pPower.getLocaleID());
			steamTracker?.updateUsing(amount, pPower.getLocaleID());
		}
	}

	internal static void trackPower(string pString = "")
	{
		if ((Object)(object)instance == (Object)null || MoveCamera.hasFocusUnit())
		{
			return;
		}
		switch (pString)
		{
		case "ButtonLeader":
			return;
		case "ButtonKingdom":
			return;
		case "ButtonCapital":
			return;
		}
		amount = 0;
		if (!frameDone)
		{
			steamTracker?.trackViewing(pString);
			discordTracker?.trackViewing(pString);
		}
	}

	internal static void setPower(GodPower pPower)
	{
		if ((Object)(object)instance == (Object)null || MoveCamera.hasFocusUnit())
		{
			return;
		}
		frameDone = true;
		if (pPower == null)
		{
			trackWatching();
		}
		else
		{
			if (!pPower.track_activity)
			{
				return;
			}
			if (LocalizedTextManager.stringExists(pPower.getLocaleID()))
			{
				discordTracker?.trackUsing(pPower.getLocaleID());
				steamTracker?.trackUsing(pPower.getLocaleID());
			}
		}
		amount = 0;
	}

	internal static void trackWatching()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			discordTracker?.trackWatching();
			steamTracker?.trackWatching();
		}
	}

	internal static void spectatingUnit(string pUnit)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			frameDone = true;
			steamTracker?.spectatingUnit(pUnit);
			discordTracker?.spectatingUnit(pUnit);
		}
	}

	internal static void trackWindow(string screen_id, ScrollWindow pWindow)
	{
		if ((Object)(object)instance == (Object)null)
		{
			return;
		}
		frameDone = true;
		switch (screen_id)
		{
		case "kingdom":
			steamTracker?.inspectKingdom(SelectedMetas.selected_kingdom.name);
			discordTracker?.inspectKingdom(SelectedMetas.selected_kingdom.name);
			return;
		case "city":
			steamTracker?.inspectVillage(SelectedMetas.selected_city.name);
			discordTracker?.inspectVillage(SelectedMetas.selected_city.name);
			return;
		case "unit":
			steamTracker?.inspectUnit(SelectedUnit.unit.getName());
			discordTracker?.inspectUnit(SelectedUnit.unit.getName());
			return;
		}
		Transform val = ((Component)pWindow).transform.Find("Background/Title");
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)pWindow).transform.FindRecursive("Title");
		}
		if ((Object)(object)val != (Object)null && ((Component)(object)val).HasComponent<LocalizedText>() && ((Component)val).GetComponent<LocalizedText>().key != "??????")
		{
			steamTracker?.trackViewing(((Component)val).GetComponent<LocalizedText>().key);
			discordTracker?.trackViewing(((Component)val).GetComponent<LocalizedText>().key);
		}
		else
		{
			Debug.Log((object)("[PT] Not found " + screen_id));
			steamTracker?.trackViewing(screen_id);
			discordTracker?.trackViewing(screen_id);
		}
	}

	private static void resetTimer()
	{
		timer = 9f;
	}

	private static void nextStat()
	{
		currentIndex = Randy.randomInt(0, rotateStats.Count);
		if (currentIndex >= rotateStats.Count)
		{
			currentIndex = 0;
		}
	}

	private void updateStat()
	{
		if (!((Object)(object)instance == (Object)null))
		{
			StatisticsAsset statisticsAsset = rotateStats[currentIndex];
			string text = statisticsAsset.string_action(statisticsAsset);
			if (text != "0" && !string.IsNullOrEmpty(text))
			{
				statisticsAsset.last_value = text;
				activeStat = statisticsAsset;
			}
			else
			{
				nextStat();
				updateStat();
			}
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Update()
	{
		if (initiated && (Object)(object)discordTracker == (Object)null && (Object)(object)steamTracker == (Object)null)
		{
			Object.Destroy((Object)(object)this);
			Debug.Log((object)"[PT] Destroying...");
			return;
		}
		if (secTimer > 0f)
		{
			secTimer -= Time.deltaTime;
		}
		else
		{
			secTimer = 1f;
			updateStat();
		}
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			return;
		}
		resetTimer();
		nextStat();
	}

	private void LateUpdate()
	{
		frameDone = false;
	}
}
