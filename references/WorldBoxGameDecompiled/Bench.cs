using System;
using System.Collections.Generic;
using UnityEngine;

public class Bench
{
	public static bool bench_enabled = false;

	public static bool bench_ai_enabled = false;

	private static Dictionary<string, BenchmarkGroup> dict = new Dictionary<string, BenchmarkGroup>();

	private static float _timer_flatten = 0f;

	public static void update()
	{
		if (bench_enabled)
		{
			finishSplitBenchmarkGroupAI();
			finishSplitBenchmarkGroup("effects_traits");
			finishSplitBenchmarkGroup("effects_items");
			if (_timer_flatten > 0f)
			{
				_timer_flatten -= Time.deltaTime;
				return;
			}
			_timer_flatten = 0.05f;
			flatten("effects_traits");
			flatten("effects_items");
		}
	}

	private static void flatten(string pID)
	{
		if (dict.TryGetValue(pID, out var value))
		{
			value.flatten();
		}
	}

	private static void finishSplitBenchmarkGroupAI()
	{
		DebugConfig.isOn(DebugOption.BenchAiEnabled);
	}

	private static void finishSplitBenchmarkGroup(string pID)
	{
		if (!dict.TryGetValue(pID, out var value))
		{
			return;
		}
		double num = 0.0;
		foreach (ToolBenchmarkData value2 in value.dict_data.Values)
		{
			num += value2.latest_result;
			value2.saveAverageCounter();
		}
		benchSaveSplit(pID, num, 1, "game_total");
	}

	public static void saveAverageCounter(string pID, string pGroup)
	{
		get(pID, pGroup).saveAverageCounter();
	}

	public static BenchmarkGroup getGroup(string pID)
	{
		if (dict.ContainsKey(pID))
		{
			return dict[pID];
		}
		BenchmarkGroup benchmarkGroup = new BenchmarkGroup();
		benchmarkGroup.id = pID;
		dict.Add(pID, benchmarkGroup);
		return benchmarkGroup;
	}

	private static ToolBenchmarkData get(string pID, string pGroupID = "main", bool pNew = true)
	{
		if (!dict.TryGetValue(pGroupID, out var value))
		{
			value = new BenchmarkGroup();
			value.id = pGroupID;
			dict.Add(pGroupID, value);
		}
		if (!value.dict_data.TryGetValue(pID, out var value2) & pNew)
		{
			value2 = new ToolBenchmarkData();
			value2.id = pID;
			value.dict_data.Add(pID, value2);
		}
		return value2;
	}

	public static void clearBenchmarkEntrySkipMultiple(string pGroupID = "main", params string[] pEntries)
	{
		foreach (string pID in pEntries)
		{
			bench(pID, pGroupID);
			benchEnd(pID, pGroupID, pSaveCounter: false, 0L);
		}
	}

	public static void clearBenchmarkEntrySkip(string pID, string pGroupID = "main")
	{
		bench(pID, pGroupID);
		benchEnd(pID, pGroupID, pSaveCounter: false, 0L);
	}

	public static double bench(string pID, string pGroupID = "main", bool pForce = false)
	{
		if (!(bench_enabled | pForce))
		{
			return 0.0;
		}
		ToolBenchmarkData toolBenchmarkData = get(pID, pGroupID);
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		toolBenchmarkData.start(realtimeSinceStartupAsDouble);
		return realtimeSinceStartupAsDouble;
	}

	public static double benchEnd(string pID, string pGroupID = "main", bool pSaveCounter = false, long pCounter = 0L, bool pForce = false)
	{
		if (!(bench_enabled | pForce))
		{
			return 0.0;
		}
		ToolBenchmarkData toolBenchmarkData = get(pID, pGroupID);
		double num = Time.realtimeSinceStartupAsDouble - toolBenchmarkData.latest_time;
		toolBenchmarkData.end(num);
		if (pSaveCounter)
		{
			toolBenchmarkData.newCount(pCounter);
			toolBenchmarkData.saveAverageCounter();
		}
		return num;
	}

	public static void benchSet(string pID, double pVal, int pCounter, string pGroupID = "main")
	{
		if (bench_enabled)
		{
			benchSave(pID, pVal, pCounter, pGroupID);
			saveAverageCounter(pID, pGroupID);
		}
	}

	public static void benchSetValue(string pID, int pValue, string pGroupID = "main")
	{
		if (bench_enabled)
		{
			get(pID, pGroupID).newValue(pValue);
		}
	}

	public static int getBenchValue(string pID, string pGroupID = "main")
	{
		if (!bench_enabled)
		{
			return 0;
		}
		return (int)get(pID, pGroupID).debug_value;
	}

	public static double benchSave(string pID, double pValue, int pCounter, string pGroupID = "main")
	{
		if (!bench_enabled)
		{
			return 0.0;
		}
		ToolBenchmarkData toolBenchmarkData = get(pID, pGroupID);
		toolBenchmarkData.end(pValue);
		toolBenchmarkData.newCount(pCounter);
		return pValue;
	}

	public static double benchSaveSplit(string pID, double pValue, int pCounter, string pGroupID = "main")
	{
		if (!bench_enabled)
		{
			return 0.0;
		}
		ToolBenchmarkData toolBenchmarkData = get(pID, pGroupID);
		toolBenchmarkData.end(pValue);
		toolBenchmarkData.newCount(pCounter);
		return pValue;
	}

	public static string getBenchResult(string pID, string pGroupID = "main", bool pAverage = true)
	{
		return getBenchResultAsDouble(pID, pGroupID, pAverage).ToString("##,0.#######");
	}

	public static double getBenchResultAsDouble(string pID, string pGroupID = "main", bool pAverage = true)
	{
		ToolBenchmarkData toolBenchmarkData = get(pID, pGroupID, pNew: false);
		if (toolBenchmarkData == null)
		{
			return -1.0;
		}
		if (pAverage)
		{
			return toolBenchmarkData.getAverage();
		}
		return toolBenchmarkData.latest_result;
	}

	public static string printableBenchResults(string pGroupID = "main", bool pAverage = false, params string[] pID)
	{
		double[] array = new double[pID.Length];
		double num = 0.0;
		double num2 = double.MaxValue;
		for (int i = 0; i < pID.Length; i++)
		{
			array[i] = getBenchResultAsDouble(pID[i], pGroupID, pAverage);
			if (array[i] > num)
			{
				num = array[i];
			}
			if (array[i] < num2)
			{
				num2 = array[i];
			}
		}
		Array.Sort(array, pID);
		using ListPool<string[]> listPool = new ListPool<string[]>();
		listPool.Add(new string[5] { "ID", "TIME", "PERCENT", "WINNER", "BAR" });
		listPool.Add(new string[0]);
		for (int j = 0; j < pID.Length; j++)
		{
			double num3 = array[j] / num;
			bool flag = array[j].Equals(num2);
			bool flag2 = array[j].Equals(num);
			string text = "";
			string text2 = "";
			string text3 = "";
			int num4 = (int)(num3 * 10.0);
			for (int k = 0; k < num4; k++)
			{
				text3 += "■";
			}
			text3 = Toolbox.fillRight(text3, 10);
			if (flag | flag2)
			{
				if (flag)
				{
					text = "<color=green>";
				}
				if (flag2)
				{
					text = "<color=red>";
				}
				text2 = "</color>";
			}
			string text4 = text + pID[j] + text2;
			string text5 = text + array[j].ToString("F7") + text2;
			string text6 = text + num3.ToString("P0") + text2;
			string text7 = text + (flag ? "WINNER" : (flag2 ? "SLOWEST" : "")) + text2;
			string text8 = text + text3 + text2;
			listPool.Add(new string[5] { text4, text5, text6, text7, text8 });
		}
		return Toolbox.printRows(listPool);
	}

	public static void printBenchResult(string pID, string pGroupID = "main", bool pAverage = false)
	{
		double benchResultAsDouble = getBenchResultAsDouble(pID, pGroupID, pAverage);
		string text = benchResultAsDouble.ToString("##,0.##########");
		if (benchResultAsDouble > 0.3)
		{
			text = "<color=red>" + text + "</color>";
		}
		else if (benchResultAsDouble > 0.1)
		{
			text = "<color=yellow>" + text + "</color>";
		}
		Debug.Log((object)("#benchmark: <color=white>" + pID + "</color>: " + text));
	}
}
