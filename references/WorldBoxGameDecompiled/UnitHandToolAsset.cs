using System;
using UnityEngine;

[Serializable]
public class UnitHandToolAsset : Asset, IHandRenderer
{
	public bool animated;

	public string path_gameplay_sprite;

	public bool colored;

	[NonSerialized]
	public Sprite[] gameplay_sprites;

	public bool is_colored => colored;

	public bool is_animated => animated;

	public Sprite[] getSprites()
	{
		return gameplay_sprites;
	}
}
