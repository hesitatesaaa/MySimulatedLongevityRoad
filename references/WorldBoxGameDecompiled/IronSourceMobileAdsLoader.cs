using System;
using Beebyte.Obfuscator;
using Unity.Services.LevelPlay;
using UnityEngine;
using com.unity3d.mediation;

[ObfuscateLiterals]
public class IronSourceMobileAdsLoader : MonoBehaviour
{
	private const string APP_KEY = "unexpected_platform";

	private static IronSourceMobileAdsLoader instance;

	internal static bool initialized;

	public static void initAds()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)instance != (Object)null))
		{
			GameObject val = new GameObject("IronSourceMobileAdsLoader")
			{
				hideFlags = (HideFlags)52
			};
			Object.DontDestroyOnLoad((Object)val);
			val.transform.SetParent(GameObject.Find("Services").transform);
			instance = val.AddComponent<IronSourceMobileAdsLoader>();
		}
	}

	internal void Start()
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
			log("Initializing");
			LevelPlayAdFormat[] array = (LevelPlayAdFormat[])(object)new LevelPlayAdFormat[1] { (LevelPlayAdFormat)2 };
			LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
			LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
			LevelPlay.Init("unexpected_platform", (string)null, array);
			log("Version " + LevelPlay.UnityVersion);
		}
		catch (Exception ex)
		{
			log("Could not initialize ads");
			Debug.Log((object)ex);
		}
	}

	private void OnApplicationPause(bool isPaused)
	{
		log("OnApplicationPause = " + isPaused);
	}

	private void SdkInitializationCompletedEvent(LevelPlayConfiguration pConfig)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			log("Initialized");
			initialized = true;
			Config.adsInitialized = true;
		});
	}

	private void SdkInitializationFailedEvent(LevelPlayInitError pConfig)
	{
		ThreadHelper.ExecuteInUpdate(delegate
		{
			log("Failed to initialize ads");
			initialized = false;
		});
	}

	private static void log(string pLog)
	{
		Debug.Log((object)(GetColor() + " <color=#abe0c3>" + pLog + "</color>"));
	}

	public static string GetColor()
	{
		return "[<color=#abe0c3>IS</color>]";
	}
}
