public class LanguagesColorsLibrary : ColorLibrary
{
	public LanguagesColorsLibrary()
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
		foreach (Language language in World.world.languages)
		{
			if (checkColor(pAsset, language.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
