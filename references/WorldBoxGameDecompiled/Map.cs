using System;
using System.Collections.Generic;

[Serializable]
public class Map
{
	public string mapId;

	public string language;

	public string timestamp;

	public string userId;

	public string username;

	public string version;

	public string mapName;

	public string mapDescription;

	public int size;

	public int sortIndex;

	public List<MapTagType> mapTags;

	public MapMetaData mapMeta;

	public OnlineStats onlineStats = new OnlineStats
	{
		downloads = 0,
		plays = 0,
		favs = 0,
		reports = 0
	};

	public string formattedMapId
	{
		get
		{
			if (!string.IsNullOrEmpty(mapId) && mapId.Length == 12)
			{
				return "WB-" + mapId.Substring(0, 4) + "-" + mapId.Substring(4, 4) + "-" + mapId.Substring(8, 4);
			}
			return mapId;
		}
	}
}
