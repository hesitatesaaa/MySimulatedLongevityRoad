using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayInterstitialAd : MonoBehaviour
{
	public static PlayInterstitialAd instance;

	internal static List<IWorldBoxAd> adProviders = new List<IWorldBoxAd>();

	public static IWorldBoxAd adProvider;

	public static bool initiated = false;

	public float timeout;

	private int failed;

	private bool should_switch = true;

	private const int AD_TIMEOUT = 8;

	private const int LOST_FOCUS_TIMEOUT = 3;

	internal static bool _isShowing = false;

	private void Awake()
	{
		instance = this;
	}

	private void Update()
	{
	}

	public void initAds()
	{
	}

	public void realInit()
	{
		initiated = true;
		adProviders.Add(new IronSourceInterstitialAd());
		adProviders.Add(new GoogleInterstitialAd());
		foreach (IWorldBoxAd adProvider in adProviders)
		{
			adProvider.adResetCallback = adReset;
			adProvider.adFinishedCallback = adFinished;
			adProvider.adFailedCallback = adFailed;
			adProvider.adStartedCallback = adStarted;
			adProvider.logger = log;
		}
		switchProvider();
		scheduleAd(8f);
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
			if (adProvider.IsInitialized() && adProvider != PlayInterstitialAd.adProvider)
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
		PlayInterstitialAd.adProvider = listPool.GetRandom();
		PlayInterstitialAd.adProvider.Reset();
		log("Switched provider: " + PlayInterstitialAd.adProvider.GetProviderName());
	}

	internal bool isReady()
	{
		return false;
	}

	public static bool hasAd()
	{
		return adProvider.HasAd();
	}

	internal void showAd()
	{
		log("Show interstitial ad");
		log("Active ad provider: " + adProvider.GetProviderName());
		MonoBehaviour.print((object)("- Show interstitial ad " + isReady()));
		logEvent("interstitial_ad_show");
		adProvider.ShowAd();
	}

	public static void forceShowAd()
	{
		try
		{
			logEvent("interstitial_ad_force_show");
			if (!initiated)
			{
				instance.realInit();
				adProvider.RequestAd();
			}
			if (adProvider.IsReady())
			{
				adProvider.ShowAd();
			}
			else
			{
				adProvider.RequestAd();
			}
		}
		catch (Exception)
		{
		}
	}

	private void scheduleAd(float pTimer = 60f)
	{
		if (!(timeout > 0f))
		{
			adProvider.KillAd();
			timeout = pTimer;
			switchProvider();
		}
	}

	private static void logEvent(string pEvent)
	{
		Analytics.LogEvent(pEvent);
	}

	private void log(string pString)
	{
		Debug.Log((object)("<color=yellow>[I] " + pString + "</color>"));
	}

	private void adReset()
	{
		failed = 0;
		timeout = 2f;
	}

	private void adStarted()
	{
		failed = 0;
		timeout = 8f;
		logEvent("interstitial_ad_started");
		_isShowing = true;
	}

	private void adFailed()
	{
		failed++;
		timeout = 8 * failed;
		logEvent("interstitial_ad_failed");
		_isShowing = false;
		should_switch = failed > 1;
	}

	private void adFinished()
	{
		failed = 0;
		timeout = 8f;
		logEvent("interstitial_ad_finished");
		_isShowing = false;
	}

	internal static bool isShowing()
	{
		return _isShowing;
	}

	internal static void setActive(bool pActive = false)
	{
		if ((Object)(object)instance == (Object)null)
		{
			instance = GameObject.Find("Services").GetComponentInChildren<PlayInterstitialAd>(true);
		}
		((Component)instance).gameObject.SetActive(pActive);
	}
}
