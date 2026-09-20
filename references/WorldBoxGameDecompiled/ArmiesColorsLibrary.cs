public class ArmiesColorsLibrary : ColorLibrary
{
	public ArmiesColorsLibrary()
	{
		file_path = "colors/colors_general";
	}

	public override void init()
	{
		base.init();
		loadFromFile<ArmiesColorsLibrary>();
	}

	public override bool isColorUsedInWorld(ColorAsset pAsset)
	{
		foreach (Army army in World.world.armies)
		{
			if (checkColor(pAsset, army.data.color_id))
			{
				return true;
			}
		}
		return base.isColorUsedInWorld(pAsset);
	}
}
