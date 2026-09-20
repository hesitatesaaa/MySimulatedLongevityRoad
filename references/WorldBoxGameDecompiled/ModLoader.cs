using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class ModLoader : MonoBehaviour
{
	private bool initialized;

	private static List<string> modsLoaded = new List<string>();

	private const string MODS_FOLDER = "mods";

	public void Update()
	{
		if (Config.game_loaded && Config.experimental_mode && !initialized)
		{
			initialized = true;
			Initialize();
			((Behaviour)this).enabled = false;
		}
	}

	internal static List<string> getModsLoaded()
	{
		return modsLoaded;
	}

	public void Initialize()
	{
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		string path = Path.Combine(Application.streamingAssetsPath, "mods");
		if (!Directory.Exists(path))
		{
			Debug.LogError((object)"Can not find mod dlls - there is no 'Mods' folder");
			return;
		}
		using ListPool<FileInfo> listPool = new ListPool<FileInfo>(new DirectoryInfo(path).GetFiles());
		listPool.RemoveAll((FileInfo file) => !file.Name.ToLower().EndsWith(".dll"));
		HashSet<string> tCheckFilenames = new HashSet<string>();
		foreach (ref FileInfo item3 in listPool)
		{
			string item = item3.Name.ToLower();
			tCheckFilenames.Add(item);
		}
		listPool.RemoveAll(delegate(FileInfo tFileInfo)
		{
			string text6 = tFileInfo.Name.ToLower();
			if (!text6.EndsWith("_memload.dll"))
			{
				return false;
			}
			string item2 = text6.Replace("_memload.dll", ".dll");
			return tCheckFilenames.Contains(item2) ? true : false;
		});
		listPool.Shuffle();
		foreach (ref FileInfo item4 in listPool)
		{
			FileInfo current = item4;
			bool flag = false;
			if (!current.Name.ToLower().EndsWith(".dll"))
			{
				continue;
			}
			string text = current.FullName;
			string? directoryName = Path.GetDirectoryName(text);
			string text2 = Path.GetFileNameWithoutExtension(current.Name).Replace("_memload", "");
			string text3 = text2;
			string text4 = text2 + "_memload.dll";
			string text5 = Path.Combine(directoryName, text4);
			if (File.Exists(text5))
			{
				flag = true;
				text3 = Path.GetFileNameWithoutExtension(text4);
				text = text5;
				Debug.Log((object)("[" + text2 + "] Loading " + current.Name + " into memory"));
			}
			else
			{
				flag = false;
				Debug.Log((object)("[" + text2 + "] Loading " + current.Name));
			}
			try
			{
				Assembly assembly;
				if (!flag)
				{
					assembly = Assembly.LoadFile(text);
				}
				else
				{
					byte[] rawAssembly = File.ReadAllBytes(text);
					string path2 = Path.Combine(Path.GetDirectoryName(text), text3 + ".pdb");
					if (File.Exists(path2))
					{
						Debug.Log((object)("[" + text2 + "] .pdb symbol file found"));
						try
						{
							byte[] rawSymbolStore = File.ReadAllBytes(path2);
							assembly = Assembly.Load(rawAssembly, rawSymbolStore);
						}
						catch (Exception ex)
						{
							Debug.LogError((object)("[" + text2 + "] Failed to load with .pdb symbol file"));
							Debug.LogError((object)ex.Message);
							assembly = Assembly.Load(rawAssembly);
						}
					}
					else
					{
						assembly = Assembly.Load(rawAssembly);
					}
				}
				Debug.Log((object)("[" + text2 + "] Assembly: " + assembly));
				Debug.Log((object)("[" + text2 + "] classes inside the mod:"));
				Type[] types = assembly.GetTypes();
				for (int num = 0; num < types.Length; num++)
				{
					Debug.Log((object)("[" + text2 + "] " + types[num]));
				}
				Debug.Log((object)("[" + text2 + "] Attempting to load " + text2 + ".WorldBoxMod"));
				Type type = assembly.GetType(text2 + ".WorldBoxMod");
				if (type != null)
				{
					GameObject val = new GameObject(text2);
					val.transform.parent = ((Component)this).transform;
					val.AddComponent(type);
					modsLoaded.Add(text2);
					Config.MODDED = true;
					Debug.Log((object)("[" + text2 + "] Was added"));
				}
				else
				{
					Debug.LogError((object)("[" + text2 + "] Missing className: " + text2 + ".WorldBoxMod"));
				}
			}
			catch (Exception ex2)
			{
				Debug.Log((object)("[" + text2 + "] Failed to load mod from path : "));
				Debug.Log((object)("[" + text2 + "] " + text));
				Debug.LogError((object)ex2.Message);
			}
		}
	}
}
