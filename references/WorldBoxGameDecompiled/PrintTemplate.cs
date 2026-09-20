using System;
using UnityEngine;

[Serializable]
public class PrintTemplate
{
	public string name;

	public Texture2D graphics;

	internal PrintStep[] steps;

	internal int steps_per_tick = 1;
}
