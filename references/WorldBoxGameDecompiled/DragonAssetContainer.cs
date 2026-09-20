using System;
using UnityEngine;

[Serializable]
public class DragonAssetContainer
{
	public string name;

	public DragonState id;

	public Sprite[] frames;

	public DragonState[] states;

	public float speed = 0.1f;
}
