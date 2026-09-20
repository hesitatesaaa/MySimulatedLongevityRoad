using System;
using Beebyte.Obfuscator;
using Unity.Services.LevelPlay;

[ObfuscateLiterals]
public class IronSourceInterstitialAd : IWorldBoxAd
{
	private const string LEVELPLAY_INTERSTITIAL = "none";

	private static string lastError = "";

	private static int loaded = 0;

	private static int failed = 0;

	private static bool _initialized = false;

	private static LevelPlayInterstitialAd _interstitial_ad;

	public Action<string> logger { get; set; }

	public Action adResetCallback { get; set; }

	public Action adFailedCallback { get; set; }

	public Action adFinishedCallback { get; set; }

	public Action adStartedCallback { get; set; }

	public void Reset()
	{
		failed = 0;
		loaded = 0;
		lastError = "";
	}

	public void RequestAd()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (Config.isMobile && !Config.hasPremium && IronSourceMobileAdsLoader.initialized)
		{
			if (!_initialized)
			{
				_initialized = true;
				_interstitial_ad = new LevelPlayInterstitialAd("none");
				_interstitial_ad.OnAdLoaded += HandleOnAdReady;
				_interstitial_ad.OnAdLoadFailed += HandleOnAdFailedToLoad;
				_interstitial_ad.OnAdDisplayed += HandleOnAdOpened;
				_interstitial_ad.OnAdDisplayFailed += HandleOnAdFailedToShow;
				_interstitial_ad.OnAdClosed += HandleOnAdClosed;
			}
			KillAd();
			log("Requesting Ad");
			_interstitial_ad.LoadAd();
		}
	}

	public void HandleOnAdLoaded()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Loaded");
		});
	}

	public void HandleOnAdFailedToLoad(LevelPlayAdError pLoadAdError)
	{
		string tLoadError = "Failed to load ad";
		if (pLoadAdError != null)
		{
			tLoadError = ((object)pLoadAdError).ToString();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded = 0;
			failed++;
			if (lastError != tLoadError)
			{
				log("<color=red>Ad Failed to Load: " + tLoadError + "</color>");
				lastError = tLoadError;
			}
			else
			{
				log("<color=red>Ad Failed to Load</color>");
			}
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
		});
	}

	public void HandleOnAdFailedToShow(LevelPlayAdDisplayInfoError pLoadAdInfoError)
	{
		_ = pLoadAdInfoError.DisplayLevelPlayAdInfo;
		LevelPlayAdError levelPlayError = pLoadAdInfoError.LevelPlayError;
		string tLoadError = "Failed to show ad";
		if (levelPlayError != null)
		{
			tLoadError = ((object)levelPlayError).ToString();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded = 0;
			failed++;
			if (lastError != tLoadError)
			{
				log("<color=red>Ad Failed to Show: " + tLoadError + "</color>");
				lastError = tLoadError;
			}
			else
			{
				log("<color=red>Ad Failed to Show</color>");
			}
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
		});
	}

	public void HandleOnAdReady(LevelPlayAdInfo pAdInfo)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Ready");
		});
	}

	public void HandleOnAdOpened(LevelPlayAdInfo pAdInfo)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Opened");
			if (adStartedCallback != null)
			{
				adStartedCallback();
			}
		});
	}

	public void HandleOnAdClosed(LevelPlayAdInfo pAdInfo)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Closed");
			KillAd();
			if (adFinishedCallback != null)
			{
				adFinishedCallback();
			}
		});
	}

	public void KillAd()
	{
	}

	public bool IsReady()
	{
		if (!IronSourceMobileAdsLoader.initialized)
		{
			return false;
		}
		LevelPlayInterstitialAd interstitial_ad = _interstitial_ad;
		if (interstitial_ad == null)
		{
			return false;
		}
		return interstitial_ad.IsAdReady();
	}

	public void ShowAd()
	{
		if (IsReady())
		{
			_interstitial_ad.ShowAd((string)null);
		}
	}

	public bool HasAd()
	{
		if (!IsInitialized())
		{
			return false;
		}
		LevelPlayInterstitialAd interstitial_ad = _interstitial_ad;
		if (interstitial_ad == null)
		{
			return false;
		}
		return interstitial_ad.IsAdReady();
	}

	public string GetProviderName()
	{
		return "IronSource Interstitial Ad";
	}

	public string GetColor()
	{
		return IronSourceMobileAdsLoader.GetColor();
	}

	private void log(string pLog)
	{
		logger(GetColor() + " " + pLog);
	}

	public bool IsInitialized()
	{
		return IronSourceMobileAdsLoader.initialized;
	}

	public void showAdInfo(LevelPlayAdInfo pAdInfo)
	{
	}
}
