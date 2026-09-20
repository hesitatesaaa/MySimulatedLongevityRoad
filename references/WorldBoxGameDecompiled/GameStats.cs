using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GameStats : MonoBehaviour
{
	internal GameStatsData data;

	private string dataPath;

	private WorldTimer saveTimer;

	private void Start()
	{
		dataPath = Application.persistentDataPath + "/stats.json";
		loadData();
		if (data == null)
		{
			data = new GameStatsData();
		}
		else
		{
			checkDataForErrors();
		}
		saveTimer = new WorldTimer(30f, saveData);
		data.gameLaunches++;
	}

	internal bool goodForAds()
	{
		return true;
	}

	private void saveData()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		string text = "Stats";
		bool flag = false;
		string pNewPath = dataPath;
		string text2 = dataPath + ".tmp";
		try
		{
			if (!Directory.Exists(Application.persistentDataPath))
			{
				Directory.CreateDirectory(Application.persistentDataPath);
			}
		}
		catch (Exception ex)
		{
			WorldTip.showNow("Error creating directory to save stats in! Check console for details", pTranslate: false, "top");
			Debug.Log((object)("Error creating directory: " + Application.persistentDataPath));
			Debug.Log((object)ex);
		}
		try
		{
			using FileStream stream = new FileStream(text2, FileMode.Create, FileAccess.Write);
			using StreamWriter streamWriter = new StreamWriter(stream);
			JsonWriter val = (JsonWriter)new JsonTextWriter((TextWriter)streamWriter);
			try
			{
				new JsonSerializer().Serialize(val, (object)data);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (IOException ex2)
		{
			if (Toolbox.IsDiskFull(ex2))
			{
				WorldTip.showNow("Error saving " + text + " : Disk full!", pTranslate: false, "top");
			}
			else
			{
				Debug.Log((object)("Could not save " + text + " due to hard drive / IO Error : "));
				Debug.Log((object)ex2);
				WorldTip.showNow("Error saving " + text + " due to IOError! Check console for details", pTranslate: false, "top");
			}
			flag = true;
		}
		catch (Exception ex3)
		{
			Debug.Log((object)("Could not save " + text + " due to error : "));
			Debug.Log((object)ex3);
			WorldTip.showNow("Error saving " + text + "! Check console for errors", pTranslate: false, "top");
			flag = true;
		}
		if (flag)
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
		}
		else
		{
			Toolbox.MoveSafely(text2, pNewPath);
		}
		AchievementLibrary.life_is_a_sim.check();
	}

	private void checkDataForErrors()
	{
		if (double.IsNaN(data.gameTime) || double.IsInfinity(data.gameTime) || data.gameTime < 0.0)
		{
			Debug.Log((object)data.gameTime);
			Debug.LogError((object)"Game time is NaN or Infinity! Resetting to 0");
			data.gameTime = 0.0;
		}
		if (data.creaturesBorn < 0)
		{
			data.creaturesBorn = Math.Max(0L, data.creaturesDied - data.creaturesCreated);
		}
	}

	private void loadData()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		if (!File.Exists(dataPath))
		{
			return;
		}
		try
		{
			using FileStream stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read);
			using StreamReader streamReader = new StreamReader(stream);
			JsonReader val = (JsonReader)new JsonTextReader((TextReader)streamReader);
			try
			{
				JsonSerializer val2 = new JsonSerializer();
				data = val2.Deserialize<GameStatsData>(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"exception caught when loading stats");
			Debug.LogError((object)ex);
		}
		if (data == null)
		{
			Debug.LogError((object)"(!) stats not has been loaded");
		}
	}

	public void updateStats(float pTime)
	{
		data.gameTime += pTime;
		saveTimer.update();
	}
}
