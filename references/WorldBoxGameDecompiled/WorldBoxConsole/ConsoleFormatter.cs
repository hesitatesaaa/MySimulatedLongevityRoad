using System;
using System.Text.RegularExpressions;
using UnityEngine;
using db;

namespace WorldBoxConsole;

public class ConsoleFormatter
{
	private static string log;

	private static string start;

	private static string end;

	private static string build = "";

	private static Regex _regex = new Regex("[\\d\\.]+");

	public static string logWarning(int pWarningNum, string pLogString)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.AppendLine().Append("<color=yellow>--- warning[").Append(pWarningNum)
			.Append("]: ---" + build + "</color>")
			.AppendLine();
		string[] array = pLogString.Trim().Split('\n');
		foreach (string value in array)
		{
			stringBuilderPool.Append("<b><color=cyan>").Append(value).Append("</color></b>")
				.AppendLine();
		}
		return stringBuilderPool.ToString();
	}

	public static string logError(int pErrorNum, string pLogString, string pStackTrace)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		string text = "";
		try
		{
			text = getShortGameplayStateInfo();
		}
		catch (Exception)
		{
			text = "(gameplay state crashed)";
		}
		string[] array = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		foreach (string value in array)
		{
			stringBuilderPool.Append("<size=7><b><color=#df4ec8>").Append(value).Append("</color></b></size>")
				.AppendLine();
		}
		stringBuilderPool.Append("<color=red>--- error[").Append(pErrorNum).Append("]: ---")
			.Append(build)
			.Append("</color>")
			.AppendLine();
		array = pLogString.Trim().Split('\n');
		foreach (string value2 in array)
		{
			stringBuilderPool.Append("<b><color=cyan>").Append(value2).Append("</color></b>")
				.AppendLine();
		}
		if (!string.IsNullOrEmpty(pStackTrace.Trim('\n', ' ')))
		{
			try
			{
				pStackTrace = formatStacktrace(pStackTrace);
			}
			catch (Exception)
			{
			}
			stringBuilderPool.Append("<color=red>--- stack: ---").Append(build).Append("</color>")
				.AppendLine()
				.Append(pStackTrace)
				.AppendLine();
		}
		return stringBuilderPool.ToString();
	}

	public static string addSystemInfo()
	{
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append("-----------").AppendLine().Append("Game Version: <color=white>")
			.Append(Application.version)
			.Append("</color>");
		build = " " + Application.version;
		if (!string.IsNullOrEmpty(Config.versionCodeText))
		{
			stringBuilderPool.Append(" (<color=white>").Append(Config.versionCodeText);
			if (!string.IsNullOrEmpty(Config.gitCodeText))
			{
				stringBuilderPool.Append("@").Append(Config.gitCodeText);
			}
			stringBuilderPool.Append("</color>)");
			build = build + " (" + Config.versionCodeText;
			if (!string.IsNullOrEmpty(Config.gitCodeText))
			{
				build = build + "@" + Config.gitCodeText;
			}
			build += ")";
		}
		build += " ---";
		stringBuilderPool.AppendLine().Append("Modded: <color=white>").Append(Config.MODDED)
			.Append("</color>")
			.AppendLine()
			.Append("operatingSystemFamily: <color=white>")
			.Append(SystemInfo.operatingSystemFamily)
			.Append("</color>")
			.AppendLine()
			.Append("deviceModel: <color=white>")
			.Append(SystemInfo.deviceModel)
			.Append("</color>")
			.AppendLine()
			.Append("deviceName: <color=white>")
			.Append(SystemInfo.deviceName)
			.Append("</color>")
			.AppendLine()
			.Append("deviceType: <color=white>")
			.Append(SystemInfo.deviceType)
			.Append("</color>")
			.AppendLine()
			.Append("systemMemorySize: <color=white>")
			.Append(SystemInfo.systemMemorySize)
			.Append("</color>")
			.AppendLine()
			.Append("graphicsDeviceID: <color=white>")
			.Append(SystemInfo.graphicsDeviceID)
			.Append("</color>")
			.AppendLine()
			.Append("Graphics.activeTier: <color=white>")
			.Append(((object)Graphics.activeTier/*cast due to constrained. prefix*/).ToString())
			.Append("</color>")
			.AppendLine()
			.Append("GC.GetTotalMemory: <color=white>")
			.Append(GC.GetTotalMemory(forceFullCollection: false) / 1000000 + " mb")
			.Append("</color>")
			.AppendLine()
			.Append("graphicsMemorySize: <color=white>")
			.Append(SystemInfo.graphicsMemorySize)
			.Append("</color>")
			.AppendLine()
			.Append("maxTextureSize: <color=white>")
			.Append(SystemInfo.maxTextureSize)
			.Append("</color>")
			.AppendLine()
			.Append("operatingSystem: <color=white>")
			.Append(SystemInfo.operatingSystem)
			.Append("</color>")
			.AppendLine()
			.Append("processorType: <color=white>")
			.Append(SystemInfo.processorType)
			.Append("</color>")
			.AppendLine()
			.Append("installMode: <color=white>")
			.Append(Application.installMode)
			.Append("</color>")
			.AppendLine()
			.Append("sandboxType: <color=white>")
			.Append(Application.sandboxType)
			.Append("</color>")
			.AppendLine()
			.Append("FPS: <color=white>")
			.Append(FPS.fps)
			.Append("</color>")
			.AppendLine()
			.Append("-----------");
		return stringBuilderPool.ToString();
	}

	public static string logFormatter(string pLogString, string pColor = "white")
	{
		pLogString = pLogString.Trim(' ', '\n');
		if (pLogString != "" && HasDigit(pLogString) && !pLogString.Contains("<color"))
		{
			return _regex.Replace(pLogString, "<color=" + pColor + ">$0</color>");
		}
		return pLogString;
	}

	private static bool HasDigit(string pString)
	{
		for (int i = 0; i < pString.Length; i++)
		{
			if (char.IsDigit(pString[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static string formatStacktrace(string pStackTrace)
	{
		string[] array = pStackTrace.Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Contains("(at "))
			{
				string[] array2 = array[i].Split(new string[1] { " (at " }, StringSplitOptions.None);
				start = array2[0];
				end = array2[1].Substring(0, array2[1].Length - 1);
			}
			else
			{
				start = array[i];
				end = "";
			}
			if (start.Contains("("))
			{
				string[] array3 = start.Split('(');
				string text = array3[0];
				string text2 = array3[1].Substring(0, array3[1].Length - 1);
				char? c = null;
				if (text.Contains(":"))
				{
					c = ':';
				}
				else if (text.Contains("."))
				{
					c = '.';
				}
				if (c.HasValue)
				{
					string[] array4 = text.Split(c.Value);
					array4[^1] = "<b><color=cyan>" + array4[^1] + "</color></b>";
					text = string.Join(c.Value.ToString(), array4);
				}
				if (text2.Trim() != string.Empty)
				{
					string[] array5 = ((!text2.Contains(",")) ? new string[1] { text2 } : text2.Split(','));
					for (int j = 0; j < array5.Length; j++)
					{
						string text3 = array5[j].Trim();
						if (text3.Contains(' '))
						{
							string[] array6 = text3.Split(' ');
							string text4 = array6[0];
							if (text4.Contains("."))
							{
								text4 = text4.Split('.')[^1];
							}
							string text5 = array6[1];
							array5[j] = "<color=#FFCC1C>" + text4 + "</color> <b><color=cyan>" + text5 + "</color></b>";
						}
						else
						{
							array5[j] = "<color=#FFCC1C>" + text3 + "</color>";
						}
						text2 = string.Join(", ", array5);
					}
				}
				start = text + "(" + text2 + ")";
				while (start.Contains("System."))
				{
					start = start.Replace("System.", string.Empty);
				}
			}
			if (end != string.Empty)
			{
				if (end.Contains("BuiltInPackages/"))
				{
					end = end.Split(new string[1] { "BuiltInPackages/" }, StringSplitOptions.None)[1];
				}
				if (end.Contains("unity/build/"))
				{
					end = end.Split(new string[1] { "unity/build/" }, StringSplitOptions.None)[1];
				}
				if (end.Contains("Unity.app/"))
				{
					end = end.Split(new string[1] { "Unity.app/" }, StringSplitOptions.None)[1];
				}
				if (end.Contains("Export/"))
				{
					end = end.Split(new string[1] { "Export/" }, StringSplitOptions.None)[1];
				}
				if (end.Contains("github/workspace/"))
				{
					end = end.Split(new string[1] { "github/workspace/" }, StringSplitOptions.None)[1];
				}
				if (end.Contains(":"))
				{
					string[] array7 = end.Split(':');
					string[] array8 = array7[^2].Split('/');
					array8[^1] = "<size=7><b><color=cyan>" + array8[^1] + "</color></b></size>";
					array7[^2] = string.Join("/", array8);
					array7[^1] = "<size=7><b><color=cyan>" + array7[^1] + "</color></b></size>";
					end = string.Join(":", array7);
				}
				end = "<size=5> (at " + end + ")</size>";
			}
			array[i] = "<size=7>" + start + "</size>" + end;
		}
		pStackTrace = string.Join("\n", array);
		return pStackTrace;
	}

	private static string getShortGameplayStateInfo()
	{
		MapBox instance = MapBox.instance;
		if ((Object)(object)instance == (Object)null)
		{
			return "(world not loaded)";
		}
		WindowStats debug_window_stats = Config.debug_window_stats;
		bool? flag = instance.quality_changer?.isLowRes();
		string text = PowerButtonSelector.instance?.selectedButton?.godPower?.id;
		string debug_last_selected_power_button = Config.debug_last_selected_power_button;
		bool flag2 = SelectedUnit.isSet();
		bool flag3 = ControllableUnit.isControllingUnit();
		string text2 = Config.time_scale_asset?.id ?? "null";
		int debug_worlds_loaded = Config.debug_worlds_loaded;
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append("spd: <H>" + text2 + "</H>");
		if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(debug_last_selected_power_button))
		{
			stringBuilderPool.Append(", ");
			stringBuilderPool.Append("pow: <H>" + (text ?? "none") + "</H>");
			if (debug_last_selected_power_button != text)
			{
				stringBuilderPool.Append(" last: <H>" + (debug_last_selected_power_button ?? "none") + "</H>");
			}
		}
		stringBuilderPool.Append(", ");
		stringBuilderPool.Append("zoom: <H>");
		stringBuilderPool.Append((!flag.HasValue) ? "null" : (flag.Value ? "map" : "full"));
		stringBuilderPool.Append("</H>");
		stringBuilderPool.Append(", ");
		stringBuilderPool.Append("win: <H>" + (debug_window_stats.current ?? "none") + "</H> (<H>" + (debug_window_stats.previous ?? "none") + "</H>)");
		stringBuilderPool.Append($" (o:{debug_window_stats.opens},c:{debug_window_stats.closes},s:{debug_window_stats.shows},h:{debug_window_stats.hides})");
		stringBuilderPool.Append(", ");
		stringBuilderPool.Append($"worlds: {debug_worlds_loaded}");
		stringBuilderPool.Append(", ");
		stringBuilderPool.Append($"modded: <H>{Config.MODDED}</H>");
		stringBuilderPool.Append(", ");
		stringBuilderPool.Append($"db pend: <H>{DBInserter.hasCommands()}</H>");
		stringBuilderPool.AppendLine();
		using StringBuilderPool stringBuilderPool2 = new StringBuilderPool();
		foreach (BaseSystemManager list_all_sim_manager in MapBox.instance.list_all_sim_managers)
		{
			string value = list_all_sim_manager.debugShort();
			if (!string.IsNullOrEmpty(value))
			{
				if (stringBuilderPool2.Length > 0)
				{
					stringBuilderPool2.Append(", ");
				}
				stringBuilderPool2.Append(value);
				if (stringBuilderPool2.Length > 78)
				{
					stringBuilderPool.Append(stringBuilderPool2.ToString());
					stringBuilderPool.AppendLine();
					stringBuilderPool2.Clear();
				}
			}
		}
		if (stringBuilderPool2.Length > 0)
		{
			stringBuilderPool.Append(stringBuilderPool2.ToString());
			stringBuilderPool.AppendLine();
		}
		using StringBuilderPool stringBuilderPool3 = new StringBuilderPool();
		if (flag2)
		{
			string text3 = SelectedUnit.unit?.asset?.id;
			stringBuilderPool3.Append("selected: <H>" + text3 + "</H>");
			if (SelectedUnit.multipleSelected())
			{
				int num = SelectedUnit.countSelected();
				stringBuilderPool3.Append($" ({num})");
			}
		}
		if (flag3)
		{
			if (stringBuilderPool3.Length > 0)
			{
				stringBuilderPool3.Append(", ");
			}
			string text4 = ControllableUnit.getControllableUnit()?.asset?.id;
			int num2 = ControllableUnit.count();
			stringBuilderPool3.Append("controlling: <H>" + text4 + "</H>");
			if (num2 > 1)
			{
				stringBuilderPool3.Append($" ({num2})");
			}
		}
		if (stringBuilderPool3.Length > 0)
		{
			stringBuilderPool.Append(stringBuilderPool3.ToString());
			stringBuilderPool.AppendLine();
		}
		return logFormatter(stringBuilderPool.ToString(), "yellow").Replace("<H>", "<color=yellow>").Replace("</H>", "</color>");
	}

	private static string getWindowInfo()
	{
		if (!ScrollWindow.isWindowActive())
		{
			return Config.debug_last_window;
		}
		return ScrollWindow.getCurrentWindow()?.screen_id;
	}
}
