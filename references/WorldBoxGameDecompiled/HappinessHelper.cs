using UnityEngine;

public static class HappinessHelper
{
	public static Sprite getSpriteBasedOnHappinessValue(int pValue)
	{
		if (pValue < -80)
		{
			return SpriteTextureLoader.getSprite("ui/Icons/iconHappiness_1");
		}
		if (pValue < -20)
		{
			return SpriteTextureLoader.getSprite("ui/Icons/iconHappiness_2");
		}
		if (pValue < 20)
		{
			return SpriteTextureLoader.getSprite("ui/Icons/iconHappiness_3");
		}
		if (pValue < 40)
		{
			return SpriteTextureLoader.getSprite("ui/Icons/iconHappiness_4");
		}
		if (pValue < 80)
		{
			return SpriteTextureLoader.getSprite("ui/Icons/iconHappiness_5");
		}
		return SpriteTextureLoader.getSprite("ui/Icons/iconHappiness_6");
	}
}
