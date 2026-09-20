using System;
using System.Diagnostics;
using System.IO;
using Beebyte.Obfuscator;
using Proyecto26;
using RSG;
using SimpleJSON;
using UnityEngine;

[ObfuscateLiterals]
internal static class VersionCheck
{
	private static string platform = "";

	public static string onlineVersion = "";

	public static WorldNetVersion wnVersion;

	public static Promise wnPromise;

	private static bool shownVersion = false;

	internal static string _vsCheck;

	private static string versionCheck
	{
		get
		{
			return _vsCheck;
		}
		set
		{
			_vsCheck = value;
			VersionCallbacks.timer = Randy.randomFloat(300f, 600f);
		}
	}

	internal static void checkVersion()
	{
		checkPlatform();
		checkDLLs();
		getOnlineVersion();
	}

	internal static bool isOutdated()
	{
		if (onlineVersion != "" && Config.gv != onlineVersion)
		{
			if (onlineVersion.Split('.').Length != 3)
			{
				return false;
			}
			if (Config.gv.Split('.').Length != 3)
			{
				return false;
			}
			SemanticVersion semanticVersion = new SemanticVersion(onlineVersion);
			SemanticVersion other = new SemanticVersion(Config.gv);
			int num = semanticVersion.CompareTo(other);
			if (num > 0)
			{
				return true;
			}
			_ = 0;
			return false;
		}
		return false;
	}

	internal static void checkDLLs()
	{
		try
		{
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				string text = module.FileName.ToLower();
				if (text.Contains("steam") && !text.Contains("punch") && module.ModuleMemorySize > 0)
				{
					RestClient.DefaultRequestHeaders["wb-stms"] = module.ModuleMemorySize.ToString();
					break;
				}
			}
		}
		catch (Exception)
		{
		}
		int num = 0;
		try
		{
			foreach (string item in Directory.EnumerateFiles(Application.dataPath, "*team*.*", SearchOption.AllDirectories))
			{
				num++;
				try
				{
					string fileName = Path.GetFileName(item);
					fileName = fileName + "," + new FileInfo(item).Length;
					RestClient.DefaultRequestHeaders["wb-stf" + num] = fileName;
				}
				catch (Exception)
				{
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static void getOnlineVersion()
	{
		if (platform.Length < 2)
		{
			return;
		}
		string text = "https://versions.superworldbox.com/versions/" + platform + ".json?" + Toolbox.cacheBuster();
		try
		{
			RestClient.DefaultRequestHeaders["wb-type"] = "vercheck";
			RestClient.DefaultRequestHeaders["wb-prem"] = (Config.hasPremium ? "y" : "n");
		}
		catch (Exception)
		{
		}
		RestClient.Get(text).Then((Action<ResponseHelper>)delegate(ResponseHelper response)
		{
			versionCheck = JSONNode.op_Implicit(JSON.Parse(response.Text));
			if (!(versionCheck == ""))
			{
				if (versionCheck.Split('.').Length != 3)
				{
					try
					{
						if (versionCheck.Contains("no_valid"))
						{
							Config.removePremium();
						}
						if (versionCheck.Contains("give_prem"))
						{
							Config.givePremium();
						}
						if (versionCheck.Contains("dprchk"))
						{
							Config.pCheck(value: false);
						}
						if (versionCheck.Contains("eprchk"))
						{
							Config.pCheck(value: true);
						}
						if (versionCheck.Contains("everything_magic"))
						{
							Config.magicCheck(pEnabled: true);
						}
						if (versionCheck.Contains("nothing_magic"))
						{
							Config.magicCheck(pEnabled: false);
						}
						if (versionCheck.Contains("fireworks"))
						{
							Config.fireworksCheck(pEnabled: true);
						}
						if (versionCheck.Contains("firenope"))
						{
							Config.fireworksCheck(pEnabled: false);
						}
						if (versionCheck.Contains("showtut"))
						{
							World.world?.tutorial?.startTutorial();
						}
						if (versionCheck.Contains("aye"))
						{
							MapBox.aye();
						}
						if (versionCheck.Contains("bear"))
						{
							Tutorial.restartTutorial();
						}
						if (versionCheck.Contains("lang_"))
						{
							string language = extractVal(versionCheck, "lang_");
							LocalizedTextManager.instance.setLanguage(language);
						}
						if (versionCheck.Contains("window_"))
						{
							ScrollWindow.get(extractVal(versionCheck, "window_", pLast: true)).forceShow();
						}
						if (versionCheck.Contains("del_"))
						{
							CustomTextureAtlas.delete(extractVal(versionCheck, "del_"));
						}
						if (versionCheck.Contains("nxtc_"))
						{
							int num = int.Parse(extractVal(versionCheck, "nxtc_"));
							if (num > 0)
							{
								InitStuff.targetSeconds = num;
							}
						}
						else
						{
							InitStuff.targetSeconds = 900f;
						}
						return;
					}
					catch (Exception)
					{
						return;
					}
				}
				onlineVersion = versionCheck;
				if (!shownVersion)
				{
					shownVersion = true;
					Debug.Log((object)("Ver " + onlineVersion + " " + Application.version));
					if (isOutdated())
					{
						Debug.Log((object)"Current version is outdated");
					}
				}
			}
		}).Catch((Action<Exception>)delegate(Exception err)
		{
			Debug.Log((object)"Some error happened during version check");
			Debug.Log((object)err.Message);
		});
	}

	public static bool isWNOutdated()
	{
		if (string.IsNullOrEmpty(wnVersion.version))
		{
			return true;
		}
		if (string.IsNullOrEmpty(wnVersion.build))
		{
			return true;
		}
		if (Config.gv != wnVersion.version)
		{
			if (wnVersion.version.Split('.').Length != 3)
			{
				return false;
			}
			if (Config.gv.Split('.').Length != 3)
			{
				return false;
			}
			SemanticVersion semanticVersion = new SemanticVersion(wnVersion.version);
			SemanticVersion other = new SemanticVersion(Config.gv);
			int num = semanticVersion.CompareTo(other);
			if (num > 0)
			{
				return true;
			}
			_ = 0;
			return false;
		}
		if (Config.versionCodeText != wnVersion.build)
		{
			int num2 = int.Parse(wnVersion.build);
			int value = int.Parse(Config.versionCodeText);
			int num3 = num2.CompareTo(value);
			if (num3 > 0)
			{
				return true;
			}
			_ = 0;
			return false;
		}
		return false;
	}

	private static string extractVal(string versionCheck, string pSplitValue, bool pLast = false)
	{
		string[] array = versionCheck.Split(new string[1] { pSplitValue }, StringSplitOptions.RemoveEmptyEntries);
		string text = ((array.Length <= 1) ? array[0] : array[1]);
		if (!pLast && text.Contains("_"))
		{
			text = text.Split(new string[1] { "_" }, StringSplitOptions.RemoveEmptyEntries)[0];
		}
		return text;
	}

	private static void checkPlatform()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		RuntimePlatform val = Application.platform;
		if ((int)val <= 11)
		{
			switch ((int)val)
			{
			default:
				if ((int)val != 11)
				{
					break;
				}
				platform = "android";
				return;
			case 2:
				platform = "pc";
				return;
			case 7:
				platform = "pc";
				return;
			case 0:
				platform = "mac";
				return;
			case 1:
				platform = "mac";
				return;
			case 8:
				platform = "ios";
				return;
			case 3:
			case 4:
			case 5:
			case 6:
				break;
			}
		}
		else
		{
			if ((int)val == 13)
			{
				platform = "linux";
				return;
			}
			if ((int)val == 16)
			{
				platform = "linux";
				return;
			}
		}
		platform = "unknown";
	}
}
