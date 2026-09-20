using UnityEngine;

public class WorldStats : StatisticsRows
{
	[SerializeField]
	private WorldStatsTabs _tab_type;

	protected override void init()
	{
		bool flag = _tab_type != WorldStatsTabs.Everything;
		foreach (StatisticsAsset item in AssetManager.statistics_library.list)
		{
			if (item.is_world_statistics && (!flag || item.world_stats_tabs.HasFlag(_tab_type)))
			{
				addStatRow(item);
			}
		}
	}
}
