using System;
using System.IO;
using RSG;

public static class PreviewUploader
{
	public static Promise<string> uploadImagePreview()
	{
		string text = DateTime.UtcNow.ToString("yyyyMMdd");
		return S3Manager.instance.uploadFileToAWS3("png/" + text.ToString() + "/" + Auth.userId + "_" + Guid.NewGuid().ToString() + ".png", getImagePreview());
	}

	private static byte[] getImagePreview()
	{
		return File.ReadAllBytes(SaveManager.getPngSlotPath());
	}
}
