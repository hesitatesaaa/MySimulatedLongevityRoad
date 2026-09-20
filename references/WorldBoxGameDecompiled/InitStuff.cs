using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Proyecto26;
using UnityEngine;
using db;

public class InitStuff : MonoBehaviour
{
	private static bool initiated = false;

	private static bool restinitiated = false;

	private float elapsedSeconds;

	public static float targetSeconds = 900f;

	private float checkInitTimeOut = 1f;

	private float servicesInitTimeOut = 3f;

	private float adsInitTimeOut = 8f;

	private void Start()
	{
		ThreadHelper.Initialize();
		initDB();
		initSteam();
	}

	public static void initRest()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (restinitiated)
		{
			return;
		}
		restinitiated = true;
		try
		{
			RestClient.DefaultRequestHeaders["salt"] = RequestHelper.salt ?? "na";
			RestClient.DefaultRequestHeaders["wb-version"] = Application.version ?? "na";
			RestClient.DefaultRequestHeaders["wb-identifier"] = Application.identifier ?? "na";
			RestClient.DefaultRequestHeaders["wb-platform"] = ((object)Application.platform/*cast due to constrained. prefix*/).ToString() ?? "na";
			RestClient.DefaultRequestHeaders["wb-language"] = LocalizedTextManager.instance.language ?? "na";
			RestClient.DefaultRequestHeaders["wb-prem"] = (Config.hasPremium ? "y" : "n");
			RestClient.DefaultRequestHeaders["wb-build"] = Config.versionCodeText ?? "na";
			RestClient.DefaultRequestHeaders["wb-gen"] = ((!Config.gen.HasValue) ? "na" : (Config.gen.Value ? "y" : "n"));
			RestClient.DefaultRequestHeaders["wb-git"] = Config.gitCodeText ?? "na";
		}
		catch (Exception ex)
		{
			Debug.Log((object)"RestClient initialization Error");
			Debug.Log((object)ex.Message);
		}
	}

	public static void initOnlineServices()
	{
		if (initiated)
		{
			return;
		}
		initiated = true;
		if (Config.isEditor)
		{
			return;
		}
		try
		{
			if (Config.firebaseEnabled)
			{
				initFirebase();
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Firebase Init Error");
			Debug.Log((object)ex.Message);
		}
		try
		{
			VersionCheck.checkVersion();
		}
		catch (Exception ex2)
		{
			Debug.Log((object)"Version Error");
			Debug.Log((object)ex2.Message);
		}
		initRichPresence();
	}

	private void Update()
	{
		if (checkInitTimeOut > 0f)
		{
			checkInitTimeOut -= Time.fixedDeltaTime;
			if (checkInitTimeOut < 0f)
			{
				if (Config.firebaseEnabled)
				{
					if (!Config.firebaseChecked)
					{
						try
						{
							checkFirebase();
						}
						catch (Exception ex)
						{
							Debug.Log((object)"Firebase Check Error");
							Debug.Log((object)ex.Message);
						}
					}
				}
				else
				{
					Config.firebaseChecked = true;
				}
			}
		}
		if (!Config.firebaseChecked)
		{
			return;
		}
		if (!restinitiated)
		{
			initRest();
		}
		if (servicesInitTimeOut > 0f)
		{
			servicesInitTimeOut -= Time.fixedDeltaTime;
			if (servicesInitTimeOut < 0f)
			{
				initOnlineServices();
			}
		}
		if (Config.firebaseInitiating)
		{
			return;
		}
		if (adsInitTimeOut > 0f)
		{
			adsInitTimeOut -= Time.fixedDeltaTime;
			if (adsInitTimeOut < 0f)
			{
				InitAds.initAdProviders();
			}
		}
		if (elapsedSeconds <= targetSeconds)
		{
			elapsedSeconds += Time.deltaTime;
			return;
		}
		elapsedSeconds = 0f;
		try
		{
			if (Config.hasPremium || targetSeconds != 900f)
			{
				VersionCheck.checkVersion();
			}
		}
		catch (Exception)
		{
		}
	}

	private static void checkFirebase()
	{
		Debug.Log((object)"Firebase Starting Check");
		Config.firebaseChecked = false;
		TaskExtension.ContinueWithOnMainThread<DependencyStatus>(FirebaseApp.CheckDependenciesAsync(), (Action<Task<DependencyStatus>>)delegate(Task<DependencyStatus> pTask)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Debug.Log((object)"Firebase check continuing on thread");
			DependencyStatus dependencyStatus = pTask.Result;
			ThreadHelper.ExecuteInUpdate(delegate
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				Debug.Log((object)"Firebase check status");
				if ((int)dependencyStatus != 0)
				{
					Debug.Log((object)"Firebase is not available");
					Debug.Log((object)dependencyStatus);
				}
				else
				{
					Debug.Log((object)"Firebase is available");
					Debug.Log((object)dependencyStatus);
				}
				Config.firebaseChecked = true;
			});
		});
	}

	private static void initFirebase()
	{
		Debug.Log((object)"Firebase init");
		Config.firebaseInitiating = true;
		TaskExtension.ContinueWithOnMainThread<DependencyStatus>(FirebaseApp.CheckAndFixDependenciesAsync(), (Action<Task<DependencyStatus>>)delegate(Task<DependencyStatus> pTask)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Debug.Log((object)"Firebase init continuing on thread");
			DependencyStatus dependencyStatus = pTask.Result;
			Debug.Log((object)dependencyStatus);
			ThreadHelper.ExecuteInUpdate(delegate
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				Debug.Log((object)"Firebase task status");
				if ((int)dependencyStatus == 0)
				{
					try
					{
						Config.firebaseInitiating = false;
						Config.firebase_available = true;
						FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
						Debug.Log((object)"Firebase loaded");
						FirebaseAnalytics.LogEvent("data", "installerName", Config.iname ?? "");
						return;
					}
					catch (Exception ex)
					{
						Debug.Log((object)"Firebase Error");
						Debug.Log((object)ex.Message);
						Config.authEnabled = false;
						Config.firebase_available = false;
						Config.firebaseInitiating = false;
						return;
					}
				}
				Debug.Log((object)$"Could not resolve all Firebase dependencies: {dependencyStatus}");
				Config.authEnabled = false;
				Config.firebase_available = false;
			});
		});
	}

	private static void initDB()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("DB")
		{
			hideFlags = (HideFlags)52
		};
		Object.DontDestroyOnLoad((Object)val);
		val.AddComponent<DBManager>();
		val.transform.SetParent(GameObject.Find("Services").transform);
	}

	private static void initRichPresence()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.disable_steam || !Config.disable_discord)
		{
			GameObject val = new GameObject("PowerTracker")
			{
				hideFlags = (HideFlags)52
			};
			Object.DontDestroyOnLoad((Object)val);
			val.AddComponent<PowerTracker>();
			val.transform.SetParent(GameObject.Find("Services").transform);
		}
	}

	internal static void initSteam()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.disable_steam)
		{
			GameObject val = new GameObject("Steam")
			{
				hideFlags = (HideFlags)52
			};
			Object.DontDestroyOnLoad((Object)val);
			val.AddComponent<SteamSDK>();
			val.transform.SetParent(GameObject.Find("Services").transform);
			SteamAchievements.InitAchievements();
		}
	}
}
