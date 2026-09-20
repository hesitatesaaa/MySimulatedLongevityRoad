using System.Collections.Generic;

public class SubspeciesBannerLibrary : GenericBannerLibrary
{
	public override void init()
	{
		base.init();
		main = add(new BannerAsset
		{
			id = "main",
			backgrounds = new List<string>
			{
				"subspecies/background_00", "subspecies/background_01", "subspecies/background_02", "subspecies/background_03", "subspecies/background_04", "subspecies/background_05", "subspecies/background_06", "subspecies/background_07", "subspecies/background_08", "subspecies/background_09",
				"subspecies/background_10", "subspecies/background_11"
			}
		});
	}
}
