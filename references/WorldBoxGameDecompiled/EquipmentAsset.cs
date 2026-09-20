using System;
using UnityEngine;

[Serializable]
public class EquipmentAsset : ItemAsset, IHandRenderer
{
	public bool is_colored => colored;

	public bool is_animated => animated;

	public Sprite[] getSprites()
	{
		return gameplay_sprites;
	}
}
