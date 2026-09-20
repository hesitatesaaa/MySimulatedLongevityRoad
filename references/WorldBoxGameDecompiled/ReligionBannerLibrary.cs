using System.Collections.Generic;

public class ReligionBannerLibrary : GenericBannerLibrary
{
	public override void init()
	{
		base.init();
		main = add(new BannerAsset
		{
			id = "main",
			backgrounds = new List<string> { "religions/background_00", "religions/background_01", "religions/background_02", "religions/background_03", "religions/background_04" },
			icons = new List<string>
			{
				"religions/icon_00", "religions/icon_01", "religions/icon_02", "religions/icon_03", "religions/icon_04", "religions/icon_05", "religions/icon_06", "religions/icon_07", "religions/icon_08", "religions/icon_09",
				"religions/icon_10", "religions/icon_11", "religions/icon_12", "religions/icon_13", "religions/icon_14", "religions/icon_15", "religions/icon_16", "religions/icon_17", "religions/icon_18", "religions/icon_19",
				"religions/icon_20", "religions/icon_21", "religions/icon_22", "religions/icon_23", "religions/icon_24"
			}
		});
	}
}
