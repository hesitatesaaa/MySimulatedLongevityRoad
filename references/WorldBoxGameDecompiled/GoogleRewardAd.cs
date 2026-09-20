using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using GoogleMobileAds.Api;
using UnityEngine;

[ObfuscateLiterals]
public class GoogleRewardAd : IWorldBoxAd
{
	private RewardedAd rewardBasedVideo;

	private static int loaded = 0;

	private static int failed = 0;

	internal static int default_current = 2;

	private static int current = 2;

	private const int max_current = 3;

	private static string _admob_id = string.Empty;

	private bool started;

	private static string lastError = "";

	private static string lastID = "";

	public Action<string> logger { get; set; }

	public Action adResetCallback { get; set; }

	public Action adFailedCallback { get; set; }

	public Action adFinishedCallback { get; set; }

	public Action adStartedCallback { get; set; }

	public void Reset()
	{
		log("reset to " + default_current);
		current = default_current;
		failed = 0;
		loaded = 0;
		lastError = "";
		lastID = "";
	}

	private string getRewardAdUnitID()
	{
		log("[prerew] " + current);
		if (failed > 1 && loaded == 0)
		{
			failed = 0;
			current++;
			current = Mathf.Clamp(current, 0, 3);
			log("Level " + current);
		}
		else if (loaded > 2 && current > 0)
		{
			current--;
			current = Mathf.Clamp(current, 0, 3);
			log("Level " + current);
		}
		return "unexpected_platform";
	}

	public void RequestAd()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if (!Config.isMobile || Config.hasPremium || (rewardBasedVideo != null && !started))
		{
			return;
		}
		KillAd();
		started = false;
		_admob_id = getRewardAdUnitID();
		AdRequest val;
		if (Config.testAds)
		{
			log("Requesting Test Ad");
			val = new AdRequest();
			List<string> testDeviceIds = new List<string> { "38469EF1320047F75C548E8477B3583B", "6b80482efcca7c0f3f07a95f8be98fe6" };
			MobileAds.SetRequestConfiguration(new RequestConfiguration
			{
				TestDeviceIds = testDeviceIds
			});
		}
		else
		{
			log("Requesting Ad");
			val = new AdRequest();
		}
		RewardedAd.Load(_admob_id, val, (Action<RewardedAd, LoadAdError>)delegate(RewardedAd ad, LoadAdError error)
		{
			ThreadHelper.ExecuteInUpdate(delegate
			{
				if (error != null || ad == null)
				{
					log("Callback error");
					HandleRewardBasedVideoFailedToLoad(error);
				}
				else
				{
					HandleRewardBasedVideoLoaded();
					rewardBasedVideo = ad;
					rewardBasedVideo.OnAdFullScreenContentOpened += HandleRewardBasedVideoOpened;
					rewardBasedVideo.OnAdFullScreenContentFailed += HandleRewardedAdFailedToShow;
					rewardBasedVideo.OnAdPaid += HandleOnPaidEvent;
					rewardBasedVideo.OnAdFullScreenContentClosed += HandleRewardBasedVideoClosed;
				}
			});
		});
	}

	public void HandleRewardBasedVideoLoaded()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Loaded");
			RewardedAds.debug += "h1_";
		});
	}

	public void HandleRewardBasedVideoFailedToLoad(LoadAdError pLoadAdError)
	{
		string tLoadError = "Failed to load ad";
		if (pLoadAdError != null)
		{
			tLoadError = ((AdError)pLoadAdError).GetMessage();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded = 0;
			failed++;
			if (lastError != tLoadError || _admob_id != lastID)
			{
				log(current + " " + _admob_id);
				log("<color=red>Ad Failed to Load: " + tLoadError + "</color>");
				lastError = tLoadError;
				lastID = _admob_id;
			}
			else
			{
				log("<color=red>Ad Failed to Load</color>");
			}
			started = true;
			RewardedAds.debug += "h2_";
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
			if (tLoadError.Contains("floor") || tLoadError.Contains("fill") || tLoadError.Contains("configured"))
			{
				failed++;
				if (current < 3 && adResetCallback != null)
				{
					adResetCallback();
				}
			}
		});
	}

	public void HandleRewardBasedVideoOpened()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Opened");
			started = true;
			RewardedAds.debug += "h3_";
			if (adStartedCallback != null)
			{
				adStartedCallback();
			}
		});
	}

	public void HandleRewardedAdFailedToShow(AdError pAdError)
	{
		string tLoadError = "Failed to show ad";
		if (pAdError != null)
		{
			tLoadError = pAdError.GetMessage();
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
			started = true;
			RewardedAds.debug += "h4_";
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
		});
	}

	public void HandleRewardBasedVideoRewarded(Reward pAdReward)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Rewarded");
			started = true;
			if ((Object)(object)World.world != (Object)null)
			{
				log("is worldbox on focus " + World.world.has_focus);
			}
			RewardedAds.instance.handleRewards();
			RewardedAds.debug += "h5_";
		});
	}

	public void HandleRewardBasedVideoClosed()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log("Ad Closed");
			started = true;
			RewardedAds.debug += "h6_";
			KillAd();
			if (adFinishedCallback != null)
			{
				adFinishedCallback();
			}
		});
	}

	public void HandleOnPaidEvent(AdValue pAdValue)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		string tLog1 = "Rewarded interstitial ad has received a paid event. " + ((object)pAdValue).ToString();
		string tLog2 = "Values: " + ((object)pAdValue.Precision/*cast due to constrained. prefix*/).ToString() + " " + pAdValue.Value + " " + pAdValue.CurrencyCode;
		ThreadHelper.ExecuteInUpdate(delegate
		{
			loaded++;
			failed = 0;
			log(tLog1);
			log(tLog2);
			started = true;
			RewardedAds.debug += "h7_";
		});
	}

	public void KillAd()
	{
		if (rewardBasedVideo != null && started)
		{
			rewardBasedVideo.OnAdFullScreenContentOpened -= HandleRewardBasedVideoOpened;
			rewardBasedVideo.OnAdFullScreenContentFailed -= HandleRewardedAdFailedToShow;
			rewardBasedVideo.OnAdPaid -= HandleOnPaidEvent;
			rewardBasedVideo.OnAdFullScreenContentClosed -= HandleRewardBasedVideoClosed;
			rewardBasedVideo.Destroy();
			rewardBasedVideo = null;
		}
	}

	public bool IsReady()
	{
		if (rewardBasedVideo != null)
		{
			return rewardBasedVideo.CanShowAd();
		}
		return false;
	}

	public void ShowAd()
	{
		if (IsReady())
		{
			started = true;
			rewardBasedVideo.Show((Action<Reward>)HandleRewardBasedVideoRewarded);
		}
	}

	public bool HasAd()
	{
		if (!IsInitialized())
		{
			return false;
		}
		return rewardBasedVideo != null;
	}

	public string GetProviderName()
	{
		return "AdMob Rewarded Ad";
	}

	public string GetColor()
	{
		return GoogleMobileAdsLoader.GetColor();
	}

	private void log(string pLog)
	{
		logger(GetColor() + " " + pLog);
	}

	public bool IsInitialized()
	{
		return GoogleMobileAdsLoader.initialized;
	}
}
