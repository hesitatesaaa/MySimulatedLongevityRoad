public class FamiliesColorsLibrary : ColorLibrary
{
	public FamiliesColorsLibrary()
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
		foreach (Family family in World.world.families)
		{
			if (checkColor(pAsset, family.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
