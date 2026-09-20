using System;
using Beebyte.Obfuscator;

[ObfuscateLiterals]
internal static class TestingCB
{
	internal static void init()
	{
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(premiumChecker));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(premiumPossible));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(purpleTextures));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(fireworks));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(tutorial));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(aye));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(language));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(openWindow));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(deleteFile));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(nextCheck));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(valCheck));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(enableSigCheck));
		VersionCallbacks.versionCallbacks = (Action<string>)Delegate.Combine(VersionCallbacks.versionCallbacks, new Action<string>(adChecks));
	}

	private static void premiumChecker(string pVersionCheck)
	{
		if (pVersionCheck.Contains("no_valid"))
		{
			Config.removePremium();
		}
		if (pVersionCheck.Contains("give_prem"))
		{
			Config.givePremium();
		}
	}

	private static void premiumPossible(string pVersionCheck)
	{
		if (pVersionCheck.Contains("dprchk"))
		{
			Config.pCheck(value: false);
		}
		if (pVersionCheck.Contains("eprchk"))
		{
			Config.pCheck(value: true);
		}
	}

	private static void purpleTextures(string pVersionCheck)
	{
		if (pVersionCheck.Contains("everything_magic"))
		{
			Config.magicCheck(pEnabled: true);
		}
		if (pVersionCheck.Contains("nothing_magic"))
		{
			Config.magicCheck(pEnabled: false);
		}
	}

	private static void fireworks(string pVersionCheck)
	{
		if (pVersionCheck.Contains("fireworks"))
		{
			Config.fireworksCheck(pEnabled: true);
		}
		if (pVersionCheck.Contains("firenope"))
		{
			Config.fireworksCheck(pEnabled: false);
		}
	}

	private static void tutorial(string pVersionCheck)
	{
		if (pVersionCheck.Contains("showtut"))
		{
			World.world?.tutorial?.startTutorial();
		}
		if (pVersionCheck.Contains("bear"))
		{
			Tutorial.restartTutorial();
		}
	}

	private static void aye(string pVersionCheck)
	{
		if (pVersionCheck.Contains("aye"))
		{
			MapBox.aye();
		}
	}

	private static void language(string pVersionCheck)
	{
		if (pVersionCheck.Contains("lang_"))
		{
			string text = extractVal(pVersionCheck, "lang_");
			LocalizedTextManager.instance.setLanguage(text);
		}
	}

	private static void openWindow(string pVersionCheck)
	{
		if (pVersionCheck.Contains("window_"))
		{
			ScrollWindow.get(extractVal(pVersionCheck, "window_", pLast: true)).forceShow();
		}
	}

	private static void deleteFile(string pVersionCheck)
	{
		if (pVersionCheck.Contains("del_"))
		{
			CustomTextureAtlas.delete(extractVal(pVersionCheck, "del_"));
		}
	}

	private static void nextCheck(string pVersionCheck)
	{
		if (pVersionCheck.Contains("nxtc_"))
		{
			int num = int.Parse(extractVal(pVersionCheck, "nxtc_"));
			if (num > 0)
			{
				InitStuff.targetSeconds = num;
			}
		}
		else
		{
			InitStuff.targetSeconds = 900f;
		}
	}

	private static void valCheck(string pVersionCheck)
	{
		if (pVersionCheck.Contains("evalchk"))
		{
			Config.valCheck(pEnabled: true);
		}
		if (pVersionCheck.Contains("dvalchk"))
		{
			Config.valCheck(pEnabled: false);
		}
	}

	private static void enableSigCheck(string pVersionCheck)
	{
	}

	private static void adChecks(string pVersionCheck)
	{
	}

	public static string extractVal(string pVersionCheck, string pSplitValue, bool pLast = false)
	{
		string[] array = pVersionCheck.Split(new string[1] { pSplitValue }, StringSplitOptions.RemoveEmptyEntries);
		string text = ((array.Length <= 1) ? array[0] : array[1]);
		if (!pLast && text.Contains("_"))
		{
			text = text.Split(new string[1] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
		}
		return text;
	}
}
