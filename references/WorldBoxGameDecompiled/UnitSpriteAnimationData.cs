using System;
using UnityEngine;

[Serializable]
public class UnitSpriteAnimationData
{
	public string name;

	public Vector3 head;

	public Vector3 item;

	public Vector3 backpack;

	public bool showHead;

	public bool showItem;

	public UnitSpriteAnimationData()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		head = default(Vector3);
		head = default(Vector3);
		backpack = default(Vector3);
	}
}
