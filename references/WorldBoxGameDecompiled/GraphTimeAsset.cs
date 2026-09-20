using System;
using db;

[Serializable]
public class GraphTimeAsset : Asset
{
	public GraphTimeScale scale_id;

	public HistoryInterval interval;

	public int max_time_frame;
}
