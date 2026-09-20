internal static class StatsHelper
{
	public static string getStatistic(string statName)
	{
		StatisticsAsset statisticsAsset = AssetManager.statistics_library.get(statName);
		if (statisticsAsset != null && statisticsAsset.string_action != null)
		{
			return statisticsAsset.string_action(statisticsAsset);
		}
		return getStat(statName).ToString() ?? "";
	}

	public static long getStat(string statName)
	{
		StatisticsAsset statisticsAsset = AssetManager.statistics_library.get(statName);
		if (statisticsAsset != null && statisticsAsset.long_action != null)
		{
			return statisticsAsset.long_action(statisticsAsset);
		}
		return 0L;
	}
}
