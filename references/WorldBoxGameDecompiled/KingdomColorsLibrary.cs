public class KingdomColorsLibrary : ColorLibrary
{
	public KingdomColorsLibrary()
	{
		file_path = "colors/colors_general";
		must_be_global = true;
	}

	public override void init()
	{
		base.init();
		loadFromFile<KingdomColorsLibrary>();
	}

	public override bool isColorUsedInWorld(ColorAsset pAsset)
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (checkColor(pAsset, kingdom.data.color_id))
			{
				return true;
			}
		}
		foreach (Alliance alliance in World.world.alliances)
		{
			if (checkColor(pAsset, alliance.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
