public class UiGameStats : StatisticsRows
{
	protected override void init()
	{
		foreach (StatisticsAsset item in AssetManager.statistics_library.list)
		{
			if (item.is_game_statistics)
			{
				addStatRow(item);
			}
		}
	}
}
