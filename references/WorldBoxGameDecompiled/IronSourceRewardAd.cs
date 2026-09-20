using System;
using Beebyte.Obfuscator;
using Unity.Services.LevelPlay;
using UnityEngine;

[ObfuscateLiterals]
public class IronSourceRewardAd : IWorldBoxAd
{
	private const string LEVELPLAY_REWARDED = "none";

	private static int loaded = 0;

	private static int failed = 0;

	private static bool initialized = false;

	private static LevelPlayRewardedAd _rewarded_ad;

	private static bool started = false;

	private static string lastError = "";

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
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		if (Config.isMobile && !Config.hasPremium && IronSourceMobileAdsLoader.initialized && (!HasAd() || started))
		{
			KillAd();
			started = false;
			if (!initialized)
			{
				initialized = true;
				_rewarded_ad = new LevelPlayRewardedAd("none");
				_rewarded_ad.OnAdLoaded += HandleRewardedAdAvailable;
				_rewarded_ad.OnAdLoadFailed += HandleRewardedAdUnavailable;
				_rewarded_ad.OnAdDisplayed += HandleRewardBasedVideoOpened;
				_rewarded_ad.OnAdDisplayFailed += HandleRewardedAdFailedToShow;
				_rewarded_ad.OnAdRewarded += HandleRewardBasedVideoRewarded;
				_rewarded_ad.OnAdClosed += HandleRewardBasedVideoClosed;
				_rewarded_ad.OnAdLoadFailed += HandleRewardedAdFailedToLoad;
			}
			log("Requesting Ad");
			_rewarded_ad.LoadAd();
		}
	}

	public void HandleRewardedAdAvailable(LevelPlayAdInfo pAdInfo)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Available");
		});
	}

	public void HandleRewardedAdUnavailable(LevelPlayAdError pError)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			log("Ad Unavailable");
		});
	}

	public void HandleRewardBasedVideoOpened(LevelPlayAdInfo pAdInfo)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			started = true;
			loaded++;
			failed = 0;
			log("Ad Opened");
			RewardedAds.debug += "h3_";
			if (adStartedCallback != null)
			{
				adStartedCallback();
			}
		});
	}

	public void HandleRewardedAdFailedToShow(LevelPlayAdDisplayInfoError pAdError)
	{
		string tLoadError = "Failed to show ad";
		if (pAdError != null)
		{
			tLoadError = ((object)pAdError).ToString();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			started = true;
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
			RewardedAds.debug += "h4_";
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
		});
	}

	public void HandleRewardedAdFailedToLoad(LevelPlayAdError pAdError)
	{
		string tLoadError = "Failed to load ad";
		if (pAdError != null)
		{
			tLoadError = ((object)pAdError).ToString();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			started = true;
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
			RewardedAds.debug += "h4_";
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
		});
	}

	public void HandleRewardBasedVideoRewarded(LevelPlayAdInfo pAdInfo, LevelPlayReward pReward)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			started = true;
			loaded++;
			failed = 0;
			log("Ad Rewarded");
			if ((Object)(object)World.world != (Object)null)
			{
				log("is worldbox on focus " + World.world.has_focus);
			}
			RewardedAds.instance.handleRewards();
			RewardedAds.debug += "h5_";
		});
	}

	public void HandleRewardBasedVideoClosed(LevelPlayAdInfo pAdInfo)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			started = true;
			loaded++;
			failed = 0;
			log("Ad Closed");
			RewardedAds.debug += "h6_";
			KillAd();
			if (adFinishedCallback != null)
			{
				adFinishedCallback();
			}
		});
	}

	public void KillAd()
	{
		_ = started;
	}

	public bool IsReady()
	{
		if (!IsInitialized())
		{
			return false;
		}
		LevelPlayRewardedAd rewarded_ad = _rewarded_ad;
		if (rewarded_ad == null)
		{
			return false;
		}
		return rewarded_ad.IsAdReady();
	}

	public void ShowAd()
	{
		if (IsReady())
		{
			started = true;
			_rewarded_ad.ShowAd((string)null);
		}
	}

	public bool HasAd()
	{
		if (!IsInitialized())
		{
			return false;
		}
		return IsReady();
	}

	public string GetProviderName()
	{
		return "IronSource Rewarded Ad";
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
