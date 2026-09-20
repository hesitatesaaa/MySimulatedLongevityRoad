public class ClanColorsLibrary : ColorLibrary
{
	public ClanColorsLibrary()
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
		foreach (Clan clan in World.world.clans)
		{
			if (checkColor(pAsset, clan.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
