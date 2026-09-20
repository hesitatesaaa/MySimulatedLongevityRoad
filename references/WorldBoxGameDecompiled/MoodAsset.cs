using System;
using Beebyte.Obfuscator;
using UnityEngine;

[Serializable]
[ObfuscateLiterals]
public class MoodAsset : Asset
{
	public BaseStats base_stats = new BaseStats();

	public string icon;

	[NonSerialized]
	public Sprite sprite;

	public Sprite getSprite()
	{
		if ((Object)(object)sprite == (Object)null)
		{
			sprite = SpriteTextureLoader.getSprite("ui/Icons/" + icon);
		}
		return sprite;
	}
}
