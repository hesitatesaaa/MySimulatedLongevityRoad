using System;
using UnityEngine;

[Serializable]
public class PersonalityAsset : Asset, ILocalizedAsset
{
	public BaseStats base_stats = new BaseStats();

	public string icon;

	public Sprite sprite;

	public string getTranslatedName()
	{
		return getLocaleID().Localize();
	}

	public string getLocaleID()
	{
		return "personality_" + id;
	}

	public Sprite getSprite()
	{
		if ((Object)(object)sprite == (Object)null)
		{
			sprite = SpriteTextureLoader.getSprite("ui/Icons/" + icon);
		}
		return sprite;
	}
}
