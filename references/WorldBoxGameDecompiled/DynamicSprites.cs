using UnityEngine;

public static class DynamicSprites
{
	public const int NO_COLOR_ID = -900000;

	public static Sprite getIconWithColors(Sprite pSprite, PhenotypeAsset pPhenotype, ColorAsset pKingdomColor)
	{
		DynamicSpritesAsset icons = DynamicSpritesLibrary.icons;
		long num = ((object)pSprite).GetHashCode() * 10000 + (pPhenotype?.GetHashCode() ?? 0) * 100 + (pKingdomColor?.GetHashCode() ?? 0);
		Sprite val = icons.getSprite(num);
		if (val == null)
		{
			val = DynamicSpriteCreator.createNewIcon(icons, pSprite, pKingdomColor, pPhenotype);
			icons.addSprite(num, val);
		}
		return val;
	}

	public static Sprite getRecoloredBuilding(Sprite pBuildingSprite, ColorAsset pColor, DynamicSpritesAsset pAtlasAsset)
	{
		long buildingSpriteID = getBuildingSpriteID(((object)pBuildingSprite).GetHashCode(), pColor);
		Sprite val = pAtlasAsset.getSprite(buildingSpriteID);
		if (val == null)
		{
			val = DynamicSpriteCreator.createNewSpriteBuilding(pAtlasAsset, buildingSpriteID, pBuildingSprite, pColor);
			pAtlasAsset.addSprite(buildingSpriteID, val);
		}
		return val;
	}

	private static long getBuildingSpriteID(int pBaseSpriteID, ColorAsset pColor)
	{
		long num = ((pColor != null) ? (pColor.index_id + 1) : (-1000000));
		return (num + 1) * 10000000 + pBaseSpriteID;
	}

	public static Sprite getBuildingLight(Building pBuilding)
	{
		DynamicSpritesAsset building_lights = DynamicSpritesLibrary.building_lights;
		int hashCode = ((object)pBuilding.last_main_sprite).GetHashCode();
		return building_lights.getSprite(hashCode);
	}

	public static Sprite getIcon(Sprite pSprite, ColorAsset pColorAsset)
	{
		DynamicSpritesAsset icons = DynamicSpritesLibrary.icons;
		long num = ((object)pSprite).GetHashCode() * 10000 + pColorAsset.GetHashCode();
		Sprite val = icons.getSprite(num);
		if (val == null)
		{
			val = DynamicSpriteCreator.createNewIcon(icons, pSprite, pColorAsset);
			icons.addSprite(num, val);
		}
		return val;
	}

	public static Sprite getShadowBuilding(BuildingAsset pAsset, Sprite pSprite)
	{
		if (!pAsset.shadow)
		{
			return null;
		}
		int hashCode = ((object)pSprite).GetHashCode();
		return DynamicSpritesLibrary.building_shadows.getSprite(hashCode);
	}

	public static Sprite getShadowUnit(Sprite pSprite, int pHashCode)
	{
		DynamicSpritesAsset units_shadows = DynamicSpritesLibrary.units_shadows;
		Sprite val = units_shadows.getSprite(pHashCode);
		if (val == null)
		{
			val = DynamicSpriteCreator.createNewUnitShadow(units_shadows, pSprite);
			units_shadows.addSprite(pHashCode, val);
		}
		return val;
	}

	public static void preloadItemSprite(Sprite pSprite, ColorAsset pColorAsset = null)
	{
		long itemSpriteID = getItemSpriteID(pSprite, pColorAsset);
		DynamicSpritesAsset items = DynamicSpritesLibrary.items;
		Sprite pSprite2 = DynamicSpriteCreator.createNewItemSprite(items, pSprite, pColorAsset);
		items.addSprite(itemSpriteID, pSprite2);
	}

	public static long getItemSpriteID(Sprite pSprite, ColorAsset pColor)
	{
		int pColorID = pColor?.GetHashCode() ?? (-900000);
		return getItemSpriteID(pSprite, pColorID);
	}

	public static long getItemSpriteID(Sprite pSprite, int pColorID = -900000)
	{
		return ((object)pSprite).GetHashCode() * 10000 + pColorID;
	}

	public static Sprite getCachedAtlasItemSprite(long pID, Sprite pSpriteSource)
	{
		Sprite sprite = DynamicSpritesLibrary.items.getSprite(pID);
		if (sprite == null)
		{
			Debug.LogError((object)("[getCachedAtlasItemSprite]Dynamic sprite not found: " + pID + " " + (object)pSpriteSource));
			return pSpriteSource;
		}
		return sprite;
	}

	public static Sprite getCachedAtlasItemSprite(long pID, Sprite pSpriteSource, ColorAsset pColorAsset)
	{
		Sprite sprite = DynamicSpritesLibrary.items.getSprite(pID);
		if (sprite == null)
		{
			Debug.LogError((object)("[getCachedAtlasItemSprite]Dynamic sprite not found: " + pID + " " + ((object)pSpriteSource)?.ToString() + " " + ((pColorAsset != null) ? (pColorAsset.index_id + " " + pColorAsset.color_main) : "null")));
			return pSpriteSource;
		}
		return sprite;
	}
}
