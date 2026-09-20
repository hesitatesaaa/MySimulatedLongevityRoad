using System;

[Serializable]
public class CultureColorsLibrary : ColorLibrary
{
	public CultureColorsLibrary()
	{
		file_path = "colors/colors_general";
	}

	public override void init()
	{
		base.init();
		useSameColorsFrom(AssetManager.kingdom_colors_library);
	}

	public override bool isColorUsedInWorld(ColorAsset pAsset)
	{
		foreach (Culture culture in World.world.cultures)
		{
			if (checkColor(pAsset, culture.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
