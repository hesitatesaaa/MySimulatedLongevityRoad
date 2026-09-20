using System.Collections.Generic;
using Firebase.Analytics;

public class Analytics
{
	private static Dictionary<string, string> _event_slugs = new Dictionary<string, string>();

	public static void trackWindow(string pName)
	{
		if (!Config.isEditor && !Config.isComputer)
		{
			string text = slugify(pName);
			if (Config.firebase_available)
			{
				FirebaseAnalytics.LogEvent("open_window", "window_id", text);
				logScreen("ScrollWindow", text);
			}
		}
	}

	public static void hideWindow()
	{
		logScreen("GamePlay", "gameplay");
	}

	public static void worldLoaded()
	{
		logScreen("GamePlay", "gameplay");
	}

	public static void worldLoading()
	{
		logScreen("LoadingScreen", "loading");
	}

	private static void logScreen(string pClass, string pName)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		if (Config.firebase_available)
		{
			Parameter[] array = (Parameter[])(object)new Parameter[2]
			{
				new Parameter(FirebaseAnalytics.ParameterScreenClass, pClass),
				new Parameter(FirebaseAnalytics.ParameterScreenName, pName)
			};
			FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView, array);
		}
	}

	public static void LogEvent(string pName, bool pFirebase = true, bool pFacebook = true)
	{
		if (Config.isEditor || Config.isComputer)
		{
			return;
		}
		MapBox world = World.world;
		if (world == null || world.auto_tester?.active != true)
		{
			string text = slugify(pName);
			if (Config.firebase_available & pFirebase)
			{
				FirebaseAnalytics.LogEvent(text);
			}
		}
	}

	public static void LogEvent(string pName, string parameterName, string parameterValue)
	{
		if (Config.isEditor || Config.isComputer)
		{
			return;
		}
		MapBox world = World.world;
		if (world == null || world.auto_tester?.active != true)
		{
			string text = slugify(pName);
			if (Config.firebase_available)
			{
				FirebaseAnalytics.LogEvent(text, parameterName, parameterValue);
			}
		}
	}

	public static string slugify(string pPhrase)
	{
		if (!_event_slugs.TryGetValue(pPhrase, out var value))
		{
			value = pPhrase.Trim().Replace(" ", "_").ToLower();
			_event_slugs[pPhrase] = value;
		}
		return value;
	}
}
