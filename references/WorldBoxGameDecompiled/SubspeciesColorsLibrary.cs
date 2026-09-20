public class SubspeciesColorsLibrary : ColorLibrary
{
	public SubspeciesColorsLibrary()
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
		foreach (Subspecies subspecy in World.world.subspecies)
		{
			if (checkColor(pAsset, subspecy.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
