using System;
using System.Collections.Generic;
using UnityEngine;

public class Sfx : MonoBehaviour
{
	private static Dictionary<string, List<SoundController>> dict;

	private static List<SoundController> listAll;

	[Obsolete("Sound system moved to MusicBox")]
	public static void timeout(string pName)
	{
		Debug.LogWarning((object)"Don't call SFX.timeout");
	}

	[Obsolete("Check out MusicBox.playSound instead")]
	public static void play(string pName, bool pRestart = true, float pX = -1f, float pY = -1f)
	{
		Debug.LogWarning((object)"Don't call SFX.play");
	}

	[Obsolete("Sound system moved to MusicBox")]
	public static void fadeOut(string pName)
	{
		_ = PlayerConfig.dict["sound"].boolVal;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
