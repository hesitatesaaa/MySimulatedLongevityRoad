using System;
using UnityEngine;

public static class Epoch
{
	private static DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static double Current()
	{
		return (DateTime.UtcNow - epochStart).TotalSeconds;
	}

	public static double SecondsElapsed(double t1)
	{
		return Mathf.Abs((float)(Current() - t1));
	}

	public static int SecondsElapsed(int t1, int t2)
	{
		return Mathf.Abs(t1 - t2);
	}

	internal static DateTime toDateTime(double epoch)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
	}
}
