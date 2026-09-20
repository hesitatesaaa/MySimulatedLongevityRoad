using System;
using System.Collections.Concurrent;
using System.IO;
using UnityEngine;
using WorldBoxConsole;

public class LogHandler : MonoBehaviour
{
	private static string folder_base = "/logs";

	private static string dataName = "/error";

	public static string log = "";

	internal static int errorNum = 0;

	private static string lastError = "";

	private static int errorRepeated = 0;

	private static bool _init_handler = false;

	private static bool _init_instance = false;

	private static bool toggledConsole = false;

	private static string _filename = null;

	private static ConcurrentQueue<LogItem> log_queue = new ConcurrentQueue<LogItem>();

	private static LogHandler _instance;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	public static void init()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (!_init_handler)
		{
			_init_handler = true;
			if (!Application.isEditor)
			{
				Application.SetStackTraceLogType((LogType)3, (StackTraceLogType)0);
				Application.SetStackTraceLogType((LogType)2, (StackTraceLogType)1);
				Application.SetStackTraceLogType((LogType)0, (StackTraceLogType)1);
			}
			Application.logMessageReceivedThreaded += new LogCallback(HandleLog);
			Application.logMessageReceivedThreaded += new LogCallback(WorldBoxConsole.Console.HandleLog);
			if (!Directory.Exists(getDirPath()))
			{
				Directory.CreateDirectory(getDirPath());
			}
		}
	}

	[RuntimeInitializeOnLoadMethod]
	public static void initInstance()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (!_init_instance)
		{
			_init_instance = true;
			if ((Object)(object)_instance == (Object)null)
			{
				GameObject val = new GameObject("[LogHandler]");
				_instance = val.AddComponent<LogHandler>();
				Object.DontDestroyOnLoad((Object)val);
				((Object)val).hideFlags = (HideFlags)52;
			}
		}
	}

	private void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		LogItem result;
		while (log_queue.TryDequeue(out result))
		{
			ProcessLog(result.log, result.stack_trace, result.type);
		}
	}

	private static void HandleLog(string pLogString, string pStackTrace, LogType pLogType)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ThreadHelper.isMainThread())
		{
			ProcessLog(pLogString, pStackTrace, pLogType);
		}
		else
		{
			log_queue.Enqueue(new LogItem(pLogString, pStackTrace, pLogType));
		}
	}

	private static void ProcessLog(string pLogString, string pStackTrace, LogType pLogType)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		pLogString = pLogString.Trim(' ', '\n');
		if ((int)pLogType == 0 || (int)pLogType == 4 || (int)pLogType == 1)
		{
			if (errorNum > 100)
			{
				return;
			}
			log = "";
			if (errorNum == 0)
			{
				log = log + "Game Version: " + Application.version;
				if (!string.IsNullOrEmpty(Config.versionCodeText))
				{
					log = log + " (" + Config.versionCodeText;
					if (!string.IsNullOrEmpty(Config.gitCodeText))
					{
						log = log + "@" + Config.gitCodeText;
					}
					log += ")";
				}
				log = log + "\nModded: " + Config.MODDED;
				log = log + "\noperatingSystemFamily: " + ((object)SystemInfo.operatingSystemFamily/*cast due to constrained. prefix*/).ToString();
				log = log + "\ndeviceModel: " + SystemInfo.deviceModel;
				log = log + "\ndeviceName: " + SystemInfo.deviceName;
				log = log + "\ndeviceType: " + ((object)SystemInfo.deviceType/*cast due to constrained. prefix*/).ToString();
				log = log + "\nsystemMemorySize: " + SystemInfo.systemMemorySize;
				log = log + "\ngraphicsDeviceID: " + SystemInfo.graphicsDeviceID;
				log = log + "\ngraphicsActiveTier: " + ((object)Graphics.activeTier/*cast due to constrained. prefix*/).ToString();
				log = log + "\nGC.GetTotalMemory: " + GC.GetTotalMemory(forceFullCollection: false) / 1000000 + " mb";
				log = log + "\ngraphicsMemorySize: " + SystemInfo.graphicsMemorySize;
				log = log + "\nmaxTextureSize: " + SystemInfo.maxTextureSize;
				log = log + "\noperatingSystem: " + SystemInfo.operatingSystem;
				log = log + "\nprocessorType: " + SystemInfo.processorType;
				log = log + "\ninstallMode: " + ((object)Application.installMode/*cast due to constrained. prefix*/).ToString();
				log = log + "\nsandboxType: " + ((object)Application.sandboxType/*cast due to constrained. prefix*/).ToString();
				try
				{
					if (Input.anyKey)
					{
						string text = "";
						if (HotkeyLibrary.isHoldingAlt())
						{
							text += "Alt ";
						}
						if (HotkeyLibrary.isHoldingControlForSelection())
						{
							text += "Ctrl ";
						}
						if (HotkeyLibrary.isHoldingAnyMod())
						{
							text += "Mod ";
						}
						log = log + "\nkeyboard: " + Input.anyKey + " " + Input.anyKeyDown + " " + Input.inputString + " " + text;
						if (Input.mousePresent)
						{
							string text2 = (Input.GetMouseButton(0) ? "press0" : (Input.GetMouseButtonDown(0) ? "down0" : (Input.GetMouseButtonUp(0) ? "up0" : "none1")));
							string text3 = (Input.GetMouseButton(1) ? "press1" : (Input.GetMouseButtonDown(1) ? "down1" : (Input.GetMouseButtonUp(1) ? "up1" : "none1")));
							string text4 = (Input.GetMouseButton(2) ? "press2" : (Input.GetMouseButtonDown(2) ? "down2" : (Input.GetMouseButtonUp(2) ? "up2" : "none2")));
							string text5 = ((object)Input.mousePosition/*cast due to constrained. prefix*/).ToString();
							log = log + "\nmouse: " + text5 + " " + text2 + " " + text3 + " " + text4;
						}
					}
				}
				catch (Exception)
				{
				}
				log = log + "\nFPS: " + FPS.fps;
				log += "\n-----------\n\n";
			}
			if (!pStackTrace.AsSpan().Trim().IsEmpty && pStackTrace == lastError)
			{
				errorRepeated++;
				return;
			}
			if (pStackTrace.AsSpan().Trim().IsEmpty && pLogString == lastError)
			{
				errorRepeated++;
				return;
			}
			clearRepeat();
			log = log + "- error[" + errorNum + "]: " + pLogString + "\n";
			log = log + "- stack:\n" + pStackTrace + "\n";
			lastError = pStackTrace;
			File.AppendAllText(getPath(), log);
			errorNum++;
			openConsole();
		}
		else
		{
			clearRepeat();
			log = log + "- trace: " + pLogString + "\n";
		}
	}

	private static void openConsole()
	{
		if (Config.show_console_on_error && (Object)(object)World.world != (Object)null && (Object)(object)World.world.console != (Object)null && !toggledConsole)
		{
			toggledConsole = true;
			World.world.console.Show();
		}
	}

	private static void clearRepeat()
	{
		if (errorRepeated > 0)
		{
			log = log + "- last error repeated " + errorRepeated + " times\n";
			lastError = "";
			errorRepeated = 0;
		}
	}

	public static string getDirPath()
	{
		return Application.persistentDataPath + folder_base;
	}

	private static string getPath()
	{
		if (_filename == null)
		{
			_filename = getFileName();
		}
		return _filename;
	}

	private static string getFileName()
	{
		string text = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
		return getDirPath() + dataName + "_" + text + ".log";
	}
}
