using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class MusicBoxDebug
{
	internal List<DebugMusicBoxData> list = new List<DebugMusicBoxData>();

	public void add(string pPath, float pX, float pY, EventInstance pInstance)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		pX += Randy.randomFloat(-0.5f, 0.5f);
		pY += Randy.randomFloat(-0.5f, 0.5f);
		list.Add(new DebugMusicBoxData
		{
			timer = 3f,
			path = pPath,
			x = pX,
			y = pY,
			instance = pInstance
		});
	}

	public void update()
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			DebugMusicBoxData debugMusicBoxData = list[num];
			debugMusicBoxData.timer -= Time.deltaTime;
			if (debugMusicBoxData.timer <= 0f)
			{
				list.RemoveAt(num);
			}
		}
	}

	public void clear()
	{
		list.Clear();
	}
}
