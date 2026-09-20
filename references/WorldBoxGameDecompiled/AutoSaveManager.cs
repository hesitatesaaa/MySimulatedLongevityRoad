using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityPools;

public static class AutoSaveManager
{
	private static float _time = 300f;

	private static float _interval = 300f;

	private static bool lowMemory = false;

	private static int lastClear = 0;

	private static int low_mem_count = 0;

	public static void update()
	{
		if (!lowMemory && Config.autosaves)
		{
			if (_time > 0f)
			{
				_time -= Time.deltaTime;
			}
			else if (ScrollWindow.isWindowActive() || ControllableUnit.isControllingUnit())
			{
				_time += 10f;
			}
			else
			{
				autoSave(pSkipDelete: false, pForce: true);
			}
		}
	}

	public static void autoSave(bool pSkipDelete = false, bool pForce = false)
	{
		if (!pForce && (_time > 240f || Time.realtimeSinceStartup - Config.LAST_LOAD_TIME < 120f))
		{
			return;
		}
		string text = SaveManager.generateAutosavesPath(Math.Truncate(Epoch.Current()).ToString());
		try
		{
			using (getAutoSaves())
			{
				text = SaveManager.generateAutosavesPath(Math.Truncate(Epoch.Current()).ToString());
				SaveManager.saveWorldToDirectory(text, pCompress: false);
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"error while auto saving");
			Debug.LogError((object)ex);
			SaveManager.deleteSavePath(text);
		}
		try
		{
			if (!pSkipDelete)
			{
				checkClearSaves();
			}
		}
		catch (Exception ex2)
		{
			Debug.Log((object)"Error while clearing saves");
			Debug.LogError((object)ex2);
		}
		resetAutoSaveTimer();
	}

	private static void checkClearSaves()
	{
		using ListPool<AutoSaveData> listPool = getAutoSaves();
		Dictionary<string, ListPool<AutoSaveData>> autoSavesPerMap = getAutoSavesPerMap(listPool);
		foreach (ListPool<AutoSaveData> value in autoSavesPerMap.Values)
		{
			while (value.Count > 5)
			{
				SaveManager.deleteSavePath(value.Pop().path);
			}
			value.Dispose();
		}
		UnsafeCollectionPool<Dictionary<string, ListPool<AutoSaveData>>, KeyValuePair<string, ListPool<AutoSaveData>>>.Release(autoSavesPerMap);
		if (listPool.Count <= 30)
		{
			return;
		}
		using ListPool<AutoSaveData> listPool2 = getAutoSaves();
		if (listPool2.Count > 30)
		{
			for (int i = 30; i < listPool2.Count; i++)
			{
				SaveManager.deleteSavePath(listPool2[i].path);
			}
		}
	}

	public static void resetAutoSaveTimer()
	{
		_time = _interval;
	}

	public static ListPool<AutoSaveData> getAutoSaves()
	{
		string text = SaveManager.generateAutosavesPath();
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		using ListPool<string> listPool = Toolbox.getDirectories(text);
		ListPool<AutoSaveData> listPool2 = new ListPool<AutoSaveData>(listPool.Count);
		foreach (ref string item in listPool)
		{
			string current = item;
			MapMetaData metaFor = SaveManager.getMetaFor(current);
			if (metaFor == null)
			{
				SaveManager.deleteSavePath(current);
				continue;
			}
			listPool2.Add(new AutoSaveData
			{
				name = metaFor.mapStats.name,
				path = current,
				timestamp = metaFor.timestamp
			});
		}
		listPool2.Sort(sorter);
		return listPool2;
	}

	public static Dictionary<string, ListPool<AutoSaveData>> getAutoSavesPerMap(ListPool<AutoSaveData> pDatas)
	{
		Dictionary<string, ListPool<AutoSaveData>> dictionary = UnsafeCollectionPool<Dictionary<string, ListPool<AutoSaveData>>, KeyValuePair<string, ListPool<AutoSaveData>>>.Get();
		for (int i = 0; i < pDatas.Count; i++)
		{
			AutoSaveData autoSaveData = pDatas[i];
			if (!dictionary.ContainsKey(autoSaveData.name))
			{
				dictionary[autoSaveData.name] = new ListPool<AutoSaveData>();
			}
			dictionary[autoSaveData.name].Add(autoSaveData);
		}
		return dictionary;
	}

	public static int sorter(AutoSaveData o1, AutoSaveData o2)
	{
		return o2.timestamp.CompareTo(o1.timestamp);
	}

	internal static void OnLowMemory()
	{
		if (!Config.game_loaded || SmoothLoader.isLoading())
		{
			return;
		}
		low_mem_count++;
		if (low_mem_count < 3)
		{
			return;
		}
		resetAutoSaveTimer();
		int num = (int)Epoch.Current();
		if (!lowMemory || lastClear - num >= 30)
		{
			lastClear = num;
			if (!lowMemory)
			{
				Debug.Log((object)"Running out of memory!");
				WorldTip.showNow("Low on memory(RAM)! Disabling auto-saves", pTranslate: false, "top");
			}
			else
			{
				Debug.Log((object)"Running out of memory!");
				WorldTip.showNow("Your device is low on memory(RAM)", pTranslate: false, "top");
			}
			lowMemory = true;
			Config.forceGC("low memory");
		}
	}
}
