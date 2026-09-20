using System.Collections.Generic;
using UnityEngine;

public abstract class GenericBannerLibrary : AssetLibrary<BannerAsset>
{
	public BannerAsset main;

	public int getNewIndexBackground()
	{
		return Randy.randomInt(0, getCurrentAsset().backgrounds.Count);
	}

	public int getNewIndexIcon()
	{
		return Randy.randomInt(0, getCurrentAsset().icons.Count);
	}

	public int getNewIndexFrame()
	{
		return Randy.randomInt(0, getCurrentAsset().frames.Count);
	}

	public Sprite getSpriteBackground(int pIndex)
	{
		BannerAsset currentAsset = getCurrentAsset();
		return loadSpriteFromAsset(currentAsset.backgrounds, pIndex);
	}

	public Sprite getSpriteBackground(int pIndex, string pAssetID)
	{
		BannerAsset bannerAsset = get(pAssetID);
		return loadSpriteFromAsset(bannerAsset.backgrounds, pIndex);
	}

	public Sprite getSpriteIcon(int pIndex)
	{
		BannerAsset currentAsset = getCurrentAsset();
		return loadSpriteFromAsset(currentAsset.icons, pIndex);
	}

	public Sprite getSpriteIcon(int pIndex, string pAssetID)
	{
		BannerAsset bannerAsset = get(pAssetID);
		return loadSpriteFromAsset(bannerAsset.icons, pIndex);
	}

	public Sprite getSpriteFrame(int pID)
	{
		BannerAsset currentAsset = getCurrentAsset();
		if (pID >= currentAsset.frames.Count)
		{
			pID = 0;
		}
		return SpriteTextureLoader.getSprite(currentAsset.frames[pID]);
	}

	private Sprite loadSpriteFromAsset(List<string> pSpriteList, int pIndex)
	{
		if (pIndex >= pSpriteList.Count)
		{
			pIndex = 0;
		}
		return SpriteTextureLoader.getSprite(pSpriteList[pIndex]);
	}

	public BannerAsset getCurrentAsset()
	{
		return main;
	}
}
