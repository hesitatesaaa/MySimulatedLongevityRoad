using System;
using System.Collections.Generic;

[Serializable]
public class UploadMapQueue : QueueItem
{
	public string username;

	public string userId;

	public string reason;

	public string error;

	public string status;

	public string mapName;

	public string mapDescription;

	public List<string> mapTags;

	public string mapFileName;

	public string mapPreviewName;

	public string mapId;

	public MapMetaData mapMeta;
}
