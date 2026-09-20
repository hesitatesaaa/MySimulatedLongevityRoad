using System.Collections.Generic;
using UnityEngine;

public class KingdomBannerLibrary : GenericBannerLibrary
{
	public const string PATH_BANNER_KINGDOMS = "banners_kingdoms/";

	public const string PATH_BACKGROUND = "/background";

	public const string PATH_ICON = "/icon";

	public override void init()
	{
		base.init();
	}

	public override BannerAsset get(string pID)
	{
		if (dict.ContainsKey(pID))
		{
			return base.get(pID);
		}
		loadNewAssetRuntime(pID);
		return base.get(pID);
	}

	public static string getFullPathBackground(string pID)
	{
		return "banners_kingdoms/" + pID + "/background";
	}

	public static string getFullPathIcon(string pID)
	{
		return "banners_kingdoms/" + pID + "/icon";
	}

	private BannerAsset loadNewAssetRuntime(string pID)
	{
		string fullPathBackground = getFullPathBackground(pID);
		string fullPathIcon = getFullPathIcon(pID);
		Sprite[] spriteList = SpriteTextureLoader.getSpriteList(fullPathBackground);
		Sprite[] spriteList2 = SpriteTextureLoader.getSpriteList(fullPathIcon);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		Sprite[] array = spriteList;
		foreach (Sprite val in array)
		{
			string item = fullPathBackground + "/" + ((Object)val).name;
			list.Add(item);
		}
		array = spriteList2;
		foreach (Sprite val2 in array)
		{
			string item2 = fullPathIcon + "/" + ((Object)val2).name;
			list2.Add(item2);
		}
		BannerAsset bannerAsset = new BannerAsset
		{
			id = pID,
			backgrounds = list,
			icons = list2
		};
		add(bannerAsset);
		return bannerAsset;
	}
}
