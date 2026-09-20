using UnityEngine;

public static class HandRendererTexturePreloader
{
	private static int _preloaded_items_counter;

	public static void launch()
	{
		AssetManager.items.loadSprites();
		AssetManager.unit_hand_tools.loadSprites();
		AssetManager.resources.loadSprites();
		preloadItemsIntoAtlas();
	}

	private static void preloadItemsIntoAtlas()
	{
		foreach (UnitHandToolAsset item in AssetManager.unit_hand_tools.list)
		{
			preloadSpritesUnitHands(pUseColors: item.is_colored, pSprites: item.getSprites());
		}
		foreach (EquipmentAsset item2 in AssetManager.items.list)
		{
			preloadSpritesUnitHands(pUseColors: item2.is_colored, pSprites: item2.getSprites());
		}
		foreach (ResourceAsset item3 in AssetManager.resources.list)
		{
			preloadSpritesUnitHands(pUseColors: item3.is_colored, pSprites: item3.getSprites());
		}
		Debug.Log((object)("Total Preloaded Hand Renderer Sprites : " + _preloaded_items_counter + " with colors " + ColorAsset.getAllColorsList().Count));
	}

	private static void preloadSpritesUnitHands(Sprite[] pSprites, bool pUseColors)
	{
		if (pSprites == null)
		{
			return;
		}
		Sprite[] array;
		if (pUseColors)
		{
			foreach (ColorAsset allColors in ColorAsset.getAllColorsList())
			{
				array = pSprites;
				for (int i = 0; i < array.Length; i++)
				{
					DynamicSprites.preloadItemSprite(array[i], allColors);
					_preloaded_items_counter++;
				}
			}
			return;
		}
		array = pSprites;
		for (int i = 0; i < array.Length; i++)
		{
			DynamicSprites.preloadItemSprite(array[i]);
			_preloaded_items_counter++;
		}
	}

	public static int getTotal()
	{
		return _preloaded_items_counter;
	}
}
