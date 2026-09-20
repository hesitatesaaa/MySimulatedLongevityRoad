using System.Collections.Generic;
using UnityEngine;

public static class PixelBagManager
{
	private static readonly Dictionary<Sprite, PixelBag> _dict = new Dictionary<Sprite, PixelBag>();

	public static int total => _dict.Count;

	public static PixelBag getPixelBag(Sprite pSpriteSource, bool pCheckPhenotypes = false, bool pCheckLights = false)
	{
		_dict.TryGetValue(pSpriteSource, out var value);
		if (value == null)
		{
			createPixelBag(pSpriteSource, pCheckPhenotypes, pCheckLights);
		}
		return _dict[pSpriteSource];
	}

	private static void createPixelBag(Sprite pSpriteSource, bool pCheckPhenotypes, bool pCheckLights)
	{
		PixelBag value = new PixelBag(pSpriteSource, pCheckPhenotypes, pCheckLights);
		_dict.Add(pSpriteSource, value);
	}

	public static void preloadPixelBagUnit(Sprite pSpriteSource)
	{
		getPixelBag(pSpriteSource, pCheckPhenotypes: true);
	}
}
