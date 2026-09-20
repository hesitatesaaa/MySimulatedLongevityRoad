using System;
using Beebyte.Obfuscator;
using GoogleMobileAds.Api;
using UnityEngine;

[ObfuscateLiterals]
public class GoogleMobileAdsLoader : MonoBehaviour
{
	private static GoogleMobileAdsLoader instance;

	internal static bool initialized;

	public static void initAds()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)instance != (Object)null) && shouldLoad())
		{
			GameObject val = new GameObject("GoogleMobileAdsLoader")
			{
				hideFlags = (HideFlags)52
			};
			Object.DontDestroyOnLoad((Object)val);
			val.transform.SetParent(GameObject.Find("Services").transform);
			instance = val.AddComponent<GoogleMobileAdsLoader>();
		}
	}

	public void Start()
	{
		if (DebugConfig.isOn(DebugOption.TestAds))
		{
			Config.testAds = true;
		}
		if (!Config.isMobile || Config.hasPremium)
		{
			return;
		}
		try
		{
			string region = PreciseLocale.GetRegion();
			if (region.ToLower().Contains("us") || region.ToLower().Contains("gb"))
			{
				GoogleInterstitialAd.default_current = 1;
				GoogleRewardAd.default_current = 1;
			}
			string currencyCode = PreciseLocale.GetCurrencyCode();
			if (currencyCode == "USD" || currencyCode == "GBP")
			{
				GoogleInterstitialAd.default_current = 1;
				GoogleRewardAd.default_current = 1;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			log("Initializing");
			MobileAds.DisableMediationInitialization();
			MobileAds.Initialize((Action<InitializationStatus>)delegate
			{
				ThreadHelper.ExecuteInUpdate(delegate
				{
					log("Initialized");
					initialized = true;
					Config.adsInitialized = true;
				});
			});
		}
		catch (Exception ex2)
		{
			log("Could not initialize ads");
			Debug.Log((object)ex2);
		}
	}

	private static void log(string pLog)
	{
		Debug.Log((object)(GetColor() + " <color=#fbbc04>" + pLog + "</color>"));
	}

	public static string GetColor()
	{
		return "[<color=#ea4335>A</color><color=#fbbc04>D</color><color=#4285f4>M</color>]";
	}

	public static bool shouldLoad()
	{
		return false;
	}
}
