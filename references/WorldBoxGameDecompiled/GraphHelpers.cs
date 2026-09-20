using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPools;

public static class GraphHelpers
{
	public static string getCategoryName(string pCategory)
	{
		if (!pCategory.Contains('|'))
		{
			return pCategory;
		}
		return pCategory.Split('|')[0];
	}

	public static ListPool<string> bestCategories(Dictionary<string, MinMax> pCategoryStats)
	{
		Dictionary<string, AvgStats> dictionary = UnsafeCollectionPool<Dictionary<string, AvgStats>, KeyValuePair<string, AvgStats>>.Get();
		foreach (KeyValuePair<string, MinMax> pCategoryStat in pCategoryStats)
		{
			string categoryName = getCategoryName(pCategoryStat.Key);
			MinMax value = pCategoryStat.Value;
			if (!dictionary.TryGetValue(categoryName, out var value2))
			{
				value2 = new AvgStats(0.0, 0, categoryName);
			}
			dictionary[categoryName] = value2.add(value.max);
		}
		using ListPool<AvgStats> listPool = new ListPool<AvgStats>(dictionary.Values);
		UnsafeCollectionPool<Dictionary<string, AvgStats>, KeyValuePair<string, AvgStats>>.Release(dictionary);
		listPool.Sort(delegate(AvgStats a, AvgStats b)
		{
			int num3 = b.count.CompareTo(a.count);
			return (num3 == 0) ? b.avg.CompareTo(a.avg) : num3;
		});
		int num = Math.Min(3, listPool.Count);
		ListPool<string> listPool2 = new ListPool<string>(num);
		for (int num2 = 0; num2 < num; num2++)
		{
			if (num2 <= 0 || (!(listPool[num2].avg <= 3.0) && listPool[num2].count >= listPool[0].count))
			{
				listPool2.Add(listPool[num2].name);
			}
		}
		return listPool2;
	}

	public static string horizontalFormatYears(double pValue, int pDigits)
	{
		return Toolbox.formatNumber((long)(pValue - (double)Date.getCurrentYear()) * -1) + "\n" + pValue.ToText();
	}

	public static string verticalFormat(double pValue, int pDigits)
	{
		MinMax min_max = GraphController.min_max;
		double num = Math.Abs(pValue);
		string text = null;
		text = ((!(num < 1000.0)) ? Toolbox.formatNumber((long)pValue) : pValue.ToString("N" + pDigits));
		if (pValue == 0.0)
		{
			return Toolbox.coloredText(text, "#FFBC66");
		}
		if (pValue < 0.0)
		{
			string pColor = Toolbox.colorBetween(pValue, min_max.min, 0.0, "#FF637D", "#FFBC66");
			return Toolbox.coloredText(text, pColor);
		}
		string pColor2 = Toolbox.colorBetween(pValue, 0.0, min_max.max, "#FFBC66", "#F3961F");
		return Toolbox.coloredText(text, pColor2);
	}

	public static long calculateNiceMaxAxisSize(double pLargestValue)
	{
		if (pLargestValue < 5.0)
		{
			return 5L;
		}
		if (pLargestValue < 8.0)
		{
			return 8L;
		}
		if (pLargestValue < 10.0)
		{
			return 10L;
		}
		if (pLargestValue < 20.0)
		{
			return 20L;
		}
		if (pLargestValue < 30.0)
		{
			return 30L;
		}
		if (pLargestValue < 40.0)
		{
			return 40L;
		}
		if (pLargestValue < 50.0)
		{
			return 50L;
		}
		if (pLargestValue < 60.0)
		{
			return 60L;
		}
		if (pLargestValue < 80.0)
		{
			return 80L;
		}
		if (pLargestValue < 100.0)
		{
			return 100L;
		}
		if (pLargestValue < 120.0)
		{
			return 120L;
		}
		if (pLargestValue < 140.0)
		{
			return 140L;
		}
		if (pLargestValue < 160.0)
		{
			return 160L;
		}
		if (pLargestValue < 180.0)
		{
			return 180L;
		}
		if (pLargestValue < 200.0)
		{
			return 200L;
		}
		if (pLargestValue < 240.0)
		{
			return 240L;
		}
		if (pLargestValue < 280.0)
		{
			return 280L;
		}
		if (pLargestValue < 300.0)
		{
			return 300L;
		}
		if (pLargestValue < 340.0)
		{
			return 340L;
		}
		if (pLargestValue < 380.0)
		{
			return 380L;
		}
		if (pLargestValue < 400.0)
		{
			return 400L;
		}
		if (pLargestValue < 500.0)
		{
			return 500L;
		}
		if (pLargestValue < 600.0)
		{
			return 600L;
		}
		if (pLargestValue < 700.0)
		{
			return 700L;
		}
		if (pLargestValue < 800.0)
		{
			return 800L;
		}
		if (pLargestValue < 900.0)
		{
			return 900L;
		}
		if (pLargestValue < 1000.0)
		{
			return 1000L;
		}
		double num = Mathf.Pow(10f, Mathf.Floor(Mathf.Log10((float)pLargestValue)));
		double num2 = pLargestValue / num;
		double num3 = ((num2 <= 1.5) ? 1.5 : ((num2 <= 2.0) ? 2.0 : ((num2 <= 3.0) ? 3.0 : ((!(num2 <= 5.0)) ? 10.0 : 5.0))));
		return (long)(num3 * num);
	}

	public static int findVerticalDivision(long pValue)
	{
		if (canDivideIntoWholeNumbers(pValue, 4))
		{
			return 4;
		}
		if (canDivideIntoWholeNumbers(pValue, 5))
		{
			return 5;
		}
		if (canDivideIntoWholeNumbers(pValue, 3))
		{
			return 3;
		}
		if (canDivideIntoWholeNumbers(pValue, 6))
		{
			return 6;
		}
		if (canDivideIntoWholeNumbers(pValue, 2))
		{
			return 2;
		}
		return 4;
	}

	private static bool canDivideIntoWholeNumbers(long pTotalValue, int pSegments)
	{
		for (int i = 1; i <= pSegments; i++)
		{
			if ((double)pTotalValue / (double)pSegments * (double)i % 1.0 > 0.0)
			{
				return false;
			}
		}
		return true;
	}
}
