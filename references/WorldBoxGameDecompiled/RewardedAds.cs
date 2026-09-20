using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RewardedAds : MonoBehaviour
{
	internal static RewardedAds instance;

	internal static List<IWorldBoxAd> adProviders = new List<IWorldBoxAd>();

	internal static IWorldBoxAd adProvider;

	public static bool initiated = false;

	private float timeout;

	private int failed;

	private bool should_switch = true;

	private const int AD_TIMEOUT = 8;

	private const int AD_REQUEST_TIMEOUT = 10;

	private const int LOST_FOCUS_TIMEOUT = 3;

	private static string adType;

	public static string _debug = "";

	private static List<PowerButton> rewardPowers = new List<PowerButton>();

	private static List<PowerButton> unlockButtons = new List<PowerButton>();

	internal static bool _isShowing = false;

	public static string debug
	{
		get
		{
			return _debug;
		}
		set
		{
			_debug = ((value != null && value.Length > 50) ? value.Substring(value.Length - 50, 50) : value);
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public void switchProvider()
	{
		if (!should_switch)
		{
			return;
		}
		should_switch = false;
		using ListPool<IWorldBoxAd> listPool = new ListPool<IWorldBoxAd>(adProviders.Count);
		foreach (IWorldBoxAd adProvider in adProviders)
		{
			if (adProvider.IsInitialized() && adProvider != RewardedAds.adProvider)
			{
				listPool.Add(adProvider);
			}
		}
		if (listPool.Count == 0)
		{
			foreach (IWorldBoxAd adProvider2 in adProviders)
			{
				if (adProvider2.IsInitialized())
				{
					listPool.Add(adProvider2);
				}
			}
		}
		RewardedAds.adProvider = listPool.GetRandom();
		RewardedAds.adProvider.Reset();
		log("Switched provider: " + RewardedAds.adProvider.GetProviderName());
	}

	public void unloadAd()
	{
		debug += "u_";
		adProvider.KillAd();
		debug += "u2_";
	}

	private void RequestRewardBasedVideo()
	{
		debug += "h8_";
		timeout = 18f;
		unloadAd();
		switchProvider();
		adProvider.RequestAd();
		debug += "h9_";
	}

	private static void logEvent(string pEvent)
	{
		Analytics.LogEvent(pEvent);
		if (!string.IsNullOrEmpty(adType))
		{
			Analytics.LogEvent(pEvent + "_" + adType);
		}
	}

	private void log(string pString)
	{
		Debug.Log((object)("<color=cyan>[R] " + pString + "</color>"));
	}

	private void adReset()
	{
		failed = 0;
		timeout = 2f;
	}

	private void adStarted()
	{
		failed = 0;
		timeout = 2f;
		logEvent("ad_reward_started");
		_isShowing = true;
	}

	private void adFailed()
	{
		failed++;
		timeout = 8 * failed;
		logEvent("ad_reward_failed");
		_isShowing = false;
		should_switch = failed > 1;
	}

	private void adFinished()
	{
		failed = 0;
		timeout = 2f;
		logEvent("ad_reward_finished");
		_isShowing = false;
	}

	private PowerButton generateRandomReward()
	{
		return null;
	}

	private bool hasRewardAvailable()
	{
		return false;
	}

	private void generateReward()
	{
	}

	internal static bool isReady()
	{
		if (!Config.adsInitialized)
		{
			return false;
		}
		if (!initiated)
		{
			return false;
		}
		if (adProvider == null)
		{
			return false;
		}
		return adProvider.IsReady();
	}

	public static bool hasAd()
	{
		if (!Config.adsInitialized)
		{
			return false;
		}
		if (!initiated)
		{
			return false;
		}
		if (adProvider == null)
		{
			return false;
		}
		return adProvider.HasAd();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool isShowing()
	{
		return _isShowing;
	}

	public void ShowRewardedAd(string pAdType = "")
	{
		adType = pAdType;
		debug += "h10_";
		if (isReady())
		{
			log("Show rewarded video");
			log("Active ad provider: " + adProvider.GetProviderName());
			logEvent("ad_reward_start");
			adProvider.ShowAd();
		}
		else
		{
			ScrollWindow.get("ad_loading_error").clickShow();
			logEvent("ad_reward_loading_error");
		}
	}

	public static void trimTimeout()
	{
		if (!((Object)(object)instance == (Object)null) && instance.timeout > 2f && instance.failed > 0)
		{
			instance.timeout = 2f;
			instance.failed = 0;
		}
	}

	public void handleRewards()
	{
	}
}
