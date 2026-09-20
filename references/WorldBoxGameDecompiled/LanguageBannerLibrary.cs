using System.Collections.Generic;

public class LanguageBannerLibrary : GenericBannerLibrary
{
	public override void init()
	{
		base.init();
		main = add(new BannerAsset
		{
			id = "main",
			backgrounds = new List<string> { "languages/background_00", "languages/background_01", "languages/background_02", "languages/background_03", "languages/background_04", "languages/background_05", "languages/background_06", "languages/background_07", "languages/background_08", "languages/background_09" },
			icons = new List<string>
			{
				"languages/icon_00", "languages/icon_01", "languages/icon_02", "languages/icon_03", "languages/icon_04", "languages/icon_05", "languages/icon_06", "languages/icon_07", "languages/icon_08", "languages/icon_09",
				"languages/icon_10", "languages/icon_11", "languages/icon_12", "languages/icon_13", "languages/icon_14", "languages/icon_15", "languages/icon_16", "languages/icon_17", "languages/icon_18", "languages/icon_19",
				"languages/icon_20"
			}
		});
	}
}
