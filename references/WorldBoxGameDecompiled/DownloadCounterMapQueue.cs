using System;

[Serializable]
public class DownloadCounterMapQueue : QueueItem
{
	public string username;

	public string userId;

	public string reason;

	public string error;

	public string status;

	public string mapId;
}
