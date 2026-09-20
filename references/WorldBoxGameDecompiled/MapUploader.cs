using System;
using RSG;

public static class MapUploader
{
	public static Promise<string> uploadMap()
	{
		string text = DateTime.UtcNow.ToString("yyyyMMdd");
		return S3Manager.instance.uploadFileToAWS3("wbox/" + text.ToString() + "/" + Auth.userId + "_" + Guid.NewGuid().ToString() + ".wbox", getMapData());
	}

	private static byte[] getMapData()
	{
		return SaveManager.getMapFromPath(SaveManager.currentSavePath).toZip();
	}
}
