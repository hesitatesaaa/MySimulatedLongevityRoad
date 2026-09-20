using System;
using System.Collections.Generic;

namespace db;

[Serializable]
public class HistoryMetaDataAsset : Asset
{
	[NonSerialized]
	public List<HistoryDataAsset> categories = new List<HistoryDataAsset>();

	public HistoryDataCollector collector;

	public MetaType meta_type;

	public Type table_type;

	public Dictionary<HistoryInterval, Type> table_types;

	public Type getTableType(HistoryInterval pInterval)
	{
		return table_types[pInterval];
	}
}
