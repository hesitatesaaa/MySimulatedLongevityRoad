using System;

namespace db;

public static class HistoryIntervalExtensions
{
	public static int getIntervalStep(this HistoryInterval pInterval)
	{
		return pInterval switch
		{
			HistoryInterval.Yearly1 => 1, 
			HistoryInterval.Yearly5 => 5, 
			HistoryInterval.Yearly10 => 10, 
			HistoryInterval.Yearly50 => 50, 
			HistoryInterval.Yearly100 => 100, 
			HistoryInterval.Yearly500 => 500, 
			HistoryInterval.Yearly1000 => 1000, 
			HistoryInterval.Yearly5000 => 5000, 
			HistoryInterval.Yearly10000 => 10000, 
			_ => throw new NotImplementedException("interval step not defined"), 
		};
	}

	public static int getMaxTimeFrame(this HistoryInterval pInterval)
	{
		int num = 0;
		foreach (GraphTimeAsset item in AssetManager.graph_time_library.list)
		{
			if (item.interval == pInterval && num < item.max_time_frame)
			{
				num = item.max_time_frame;
			}
		}
		return num;
	}

	public static (int tEveryYears, HistoryInterval tFromInterval) fillFrom(this HistoryInterval pInterval)
	{
		return pInterval switch
		{
			HistoryInterval.Yearly1 => (tEveryYears: 0, tFromInterval: HistoryInterval.None), 
			HistoryInterval.Yearly5 => (tEveryYears: 5, tFromInterval: HistoryInterval.Yearly1), 
			HistoryInterval.Yearly10 => (tEveryYears: 10, tFromInterval: HistoryInterval.Yearly1), 
			HistoryInterval.Yearly50 => (tEveryYears: 50, tFromInterval: HistoryInterval.Yearly10), 
			HistoryInterval.Yearly100 => (tEveryYears: 100, tFromInterval: HistoryInterval.Yearly10), 
			HistoryInterval.Yearly500 => (tEveryYears: 500, tFromInterval: HistoryInterval.Yearly50), 
			HistoryInterval.Yearly1000 => (tEveryYears: 1000, tFromInterval: HistoryInterval.Yearly100), 
			HistoryInterval.Yearly5000 => (tEveryYears: 5000, tFromInterval: HistoryInterval.Yearly500), 
			HistoryInterval.Yearly10000 => (tEveryYears: 10000, tFromInterval: HistoryInterval.Yearly1000), 
			_ => throw new NotImplementedException("interval step not defined"), 
		};
	}
}
