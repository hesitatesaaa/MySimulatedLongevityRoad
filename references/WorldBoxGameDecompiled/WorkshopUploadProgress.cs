using System;
using UnityEngine;

internal class WorkshopUploadProgress : IProgress<float>
{
	internal float lastvalue;

	public void Report(float value)
	{
		if (!(lastvalue >= value))
		{
			lastvalue = value;
			WorkshopMaps.uploadProgress = lastvalue;
			Debug.Log((object)value);
		}
	}
}
