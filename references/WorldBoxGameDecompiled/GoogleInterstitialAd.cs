using System;
using System.Collections.Generic;
using Beebyte.Obfuscator;
using GoogleMobileAds.Api;
using UnityEngine;

[ObfuscateLiterals]
public class GoogleInterstitialAd : IWorldBoxAd
{
	private InterstitialAd interstitial;

	private static string _last_error = "";

	private static string _last_id = "";

	private static int _loaded = 0;

	private static int _failed = 0;

	internal static int default_current = 2;

	private static int _current = 2;

	private const int max_current = 3;

	private static string _admob_id = string.Empty;

	public Action<string> logger { get; set; }

	public Action adResetCallback { get; set; }

	public Action adFailedCallback { get; set; }

	public Action adFinishedCallback { get; set; }

	public Action adStartedCallback { get; set; }

	public void Reset()
	{
		log("reset to " + default_current);
		_current = default_current;
		_failed = 0;
		_loaded = 0;
		_last_error = "";
		_last_id = "";
	}

	private string getInterstitialAdUnitID()
	{
		log("[preint] " + _current);
		if (_failed > 1 && _loaded == 0)
		{
			_failed = 0;
			_current++;
			_current = Mathf.Clamp(_current, 0, 3);
			log("Level " + _current);
		}
		else if (_loaded > 2 && _current > 0)
		{
			_current--;
			_current = Mathf.Clamp(_current, 0, 3);
			log("Level " + _current);
		}
		return "unexpected_platform";
	}

	public void RequestAd()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		if (!Config.isMobile || Config.hasPremium)
		{
			return;
		}
		_admob_id = getInterstitialAdUnitID();
		KillAd();
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
		InterstitialAd.Load(_admob_id, val, (Action<InterstitialAd, LoadAdError>)delegate(InterstitialAd ad, LoadAdError error)
		{
			ThreadHelper.ExecuteInUpdate(delegate
			{
				if (error != null || ad == null)
				{
					log("Callback error");
					HandleOnAdFailedToLoad(error);
				}
				else
				{
					HandleOnAdLoaded();
					interstitial = ad;
					interstitial.OnAdFullScreenContentOpened += HandleOnAdOpened;
					interstitial.OnAdFullScreenContentClosed += HandleOnAdClosed;
					interstitial.OnAdFullScreenContentFailed += HandleOnAdFailed;
					interstitial.OnAdPaid += HandleOnPaidEvent;
				}
			});
		});
	}

	public void HandleOnAdLoaded()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			_loaded++;
			_failed = 0;
			log("Ad Loaded");
		});
	}

	public void HandleOnAdFailedToLoad(LoadAdError pLoadAdError = null)
	{
		string tLoadError = "Failed to load ad";
		if (pLoadAdError != null)
		{
			tLoadError = ((AdError)pLoadAdError).GetMessage();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			_loaded = 0;
			_failed++;
			if (_last_error != tLoadError || _admob_id != _last_id)
			{
				log(_current + " " + _admob_id);
				log("<color=red>Failed Load: " + tLoadError + "</color>");
				_last_error = tLoadError;
				_last_id = _admob_id;
			}
			else
			{
				log("<color=red>Failed Load</color>");
			}
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
			if (tLoadError.Contains("floor") || tLoadError.Contains("fill") || tLoadError.Contains("configured"))
			{
				_failed++;
				if (_current < 3 && adResetCallback != null)
				{
					adResetCallback();
				}
			}
		});
	}

	public void HandleOnAdFailed(AdError pLoadAdError)
	{
		string tLoadError = "Failed to show ad";
		if (pLoadAdError != null)
		{
			tLoadError = pLoadAdError.GetMessage();
		}
		ThreadHelper.ExecuteInUpdate(delegate
		{
			_loaded = 0;
			_failed++;
			if (_last_error != tLoadError || _admob_id != _last_id)
			{
				log(_current + " " + _admob_id);
				log("<color=red>Ad Failed: " + tLoadError + "</color>");
				_last_error = tLoadError;
				_last_id = _admob_id;
			}
			else
			{
				log("<color=red>Ad Failed</color>");
			}
			KillAd();
			if (adFailedCallback != null)
			{
				adFailedCallback();
			}
			if (tLoadError.Contains("floor") || tLoadError.Contains("fill") || tLoadError.Contains("configured"))
			{
				_failed++;
				if (_current < 3 && adResetCallback != null)
				{
					adResetCallback();
				}
			}
		});
	}

	public void HandleOnAdOpened()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			_loaded++;
			_failed = 0;
			log("Ad Opened");
			if (adStartedCallback != null)
			{
				adStartedCallback();
			}
		});
	}

	public void HandleOnAdClosed()
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			_loaded++;
			_failed = 0;
			log("Ad Closed");
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
			_loaded++;
			_failed = 0;
			log(tLog1);
			log(tLog2);
		});
	}

	public void KillAd()
	{
		if (interstitial != null)
		{
			interstitial.OnAdFullScreenContentOpened -= HandleOnAdOpened;
			interstitial.OnAdFullScreenContentClosed -= HandleOnAdClosed;
			interstitial.OnAdPaid -= HandleOnPaidEvent;
			interstitial.Destroy();
			interstitial = null;
		}
	}

	public bool IsReady()
	{
		if (interstitial != null)
		{
			return interstitial.CanShowAd();
		}
		return false;
	}

	public void ShowAd()
	{
		if (IsReady())
		{
			interstitial.Show();
		}
	}

	public bool HasAd()
	{
		if (!IsInitialized())
		{
			return false;
		}
		return interstitial != null;
	}

	public string GetProviderName()
	{
		return "AdMob Interstitial Ad";
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
