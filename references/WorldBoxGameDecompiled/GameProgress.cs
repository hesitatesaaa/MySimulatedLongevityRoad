using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GameProgress
{
	public static GameProgress instance;

	private string dataPath;

	internal GameProgressData data;

	public static void init()
	{
		if (instance == null)
		{
			Debug.Log((object)"INIT Progress");
			instance = new GameProgress();
			instance.create();
		}
	}

	public void create()
	{
		setNewDataPath();
		if (File.Exists(dataPath))
		{
			try
			{
				loadData();
				return;
			}
			catch (Exception)
			{
				initNewSave();
				return;
			}
		}
		initNewSave();
	}

	private void setNewDataPath()
	{
		dataPath = Application.persistentDataPath + "/worldboxProgress";
	}

	private void initNewSave()
	{
		data = new GameProgressData();
		saveData();
	}

	public static bool unlockAchievement(string pName)
	{
		if (instance == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(pName))
		{
			return false;
		}
		if (isAchievementUnlocked(pName))
		{
			return false;
		}
		instance.data.achievements.Add(pName);
		saveData();
		return true;
	}

	public static bool isAchievementUnlocked(string pName)
	{
		if (instance == null)
		{
			return false;
		}
		return instance.data.achievements.Contains(pName);
	}

	public static void saveData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		JsonSerializerSettings val = new JsonSerializerSettings
		{
			DefaultValueHandling = (DefaultValueHandling)3,
			Formatting = (Formatting)1
		};
		string pStringData = Toolbox.encode(JsonConvert.SerializeObject((object)instance.data, val));
		Toolbox.WriteSafely("Game Progress", instance.dataPath, ref pStringData);
	}

	private void loadData()
	{
		if (!File.Exists(dataPath))
		{
			return;
		}
		string text = File.ReadAllText(dataPath);
		try
		{
			string text2 = Toolbox.decode(text);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text2;
			}
		}
		catch (Exception)
		{
		}
		try
		{
			data = JsonConvert.DeserializeObject<GameProgressData>(text);
			data.setDefaultValues();
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("Error loading game progress data from " + dataPath));
			Debug.LogError((object)ex2);
			initNewSave();
		}
	}

	public void debugClearAllAchievements()
	{
		data.achievements.Clear();
		saveData();
	}

	public void unlockAllAchievements()
	{
		foreach (Achievement item in AssetManager.achievements.list)
		{
			unlockAchievement(item.id);
		}
	}

	public void debugClearAll()
	{
		data.prepare();
		foreach (HashSet<string> all_hashset in data.all_hashsets)
		{
			all_hashset.Clear();
		}
		saveData();
	}

	public void debugUnlockAll()
	{
		foreach (ActorTrait item in AssetManager.traits.list)
		{
			item.unlock(pSaveData: false);
		}
		foreach (CultureTrait item2 in AssetManager.culture_traits.list)
		{
			item2.unlock(pSaveData: false);
		}
		foreach (LanguageTrait item3 in AssetManager.language_traits.list)
		{
			item3.unlock(pSaveData: false);
		}
		foreach (SubspeciesTrait item4 in AssetManager.subspecies_traits.list)
		{
			item4.unlock(pSaveData: false);
		}
		foreach (ClanTrait item5 in AssetManager.clan_traits.list)
		{
			item5.unlock(pSaveData: false);
		}
		foreach (ReligionTrait item6 in AssetManager.religion_traits.list)
		{
			item6.unlock(pSaveData: false);
		}
		foreach (KingdomTrait item7 in AssetManager.kingdoms_traits.list)
		{
			item7.unlock(pSaveData: false);
		}
		foreach (EquipmentAsset item8 in AssetManager.items.list)
		{
			item8.unlock(pSaveData: false);
		}
		foreach (GeneAsset item9 in AssetManager.gene_library.list)
		{
			item9.unlock(pSaveData: false);
		}
		foreach (ActorAsset item10 in AssetManager.actor_library.list)
		{
			item10.unlock(pSaveData: false);
		}
		foreach (PlotAsset item11 in AssetManager.plots_library.list)
		{
			item11.unlock(pSaveData: false);
		}
		saveData();
	}
}
