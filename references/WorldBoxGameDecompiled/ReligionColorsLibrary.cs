using System;

[Serializable]
public class ReligionColorsLibrary : ColorLibrary
{
	public ReligionColorsLibrary()
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
		foreach (Religion religion in World.world.religions)
		{
			if (checkColor(pAsset, religion.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
