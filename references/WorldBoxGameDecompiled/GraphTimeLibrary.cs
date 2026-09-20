using db;

public class GraphTimeLibrary : AssetLibrary<GraphTimeAsset>
{
	public override void init()
	{
		base.init();
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_10,
			max_time_frame = 10,
			interval = HistoryInterval.Yearly1
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_25,
			max_time_frame = 25,
			interval = HistoryInterval.Yearly5
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_100,
			max_time_frame = 100,
			interval = HistoryInterval.Yearly10
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_250,
			max_time_frame = 250,
			interval = HistoryInterval.Yearly50
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_500,
			max_time_frame = 500,
			interval = HistoryInterval.Yearly50
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_1000,
			max_time_frame = 1000,
			interval = HistoryInterval.Yearly100
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_2500,
			max_time_frame = 2500,
			interval = HistoryInterval.Yearly500
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_5000,
			max_time_frame = 5000,
			interval = HistoryInterval.Yearly500
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_10000,
			max_time_frame = 10000,
			interval = HistoryInterval.Yearly1000
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_50000,
			max_time_frame = 50000,
			interval = HistoryInterval.Yearly5000
		});
		add(new GraphTimeAsset
		{
			scale_id = GraphTimeScale.year_100000,
			max_time_frame = 100000,
			interval = HistoryInterval.Yearly10000
		});
	}

	public static long getMinTime(GraphTimeAsset pAsset)
	{
		return Date.getYear((float)Date.getYearsSince(0.0) * 60f - 60f * (float)pAsset.max_time_frame);
	}

	public static long getMaxTime(GraphTimeAsset pAsset)
	{
		return Date.getYear((float)Date.getYearsSince(0.0) * 60f);
	}

	public override GraphTimeAsset add(GraphTimeAsset pAsset)
	{
		pAsset.id = pAsset.scale_id.ToString();
		return base.add(pAsset);
	}
}
