public class ListWindowStatistics : StatisticsRows
{
	public MetaType meta_type;

	protected override void init()
	{
		foreach (StatisticsAsset item in AssetManager.statistics_library.list)
		{
			if (!item.list_window_meta_type.isNone() && meta_type == item.list_window_meta_type)
			{
				addStatRow(item);
			}
		}
	}
}
