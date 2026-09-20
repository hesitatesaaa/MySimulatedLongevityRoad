using System.Collections.Generic;

public class FamilysBannerLibrary : GenericBannerLibrary
{
	public override void init()
	{
		base.init();
		main = add(new BannerAsset
		{
			id = "main",
			backgrounds = new List<string>
			{
				"families/background_00", "families/background_01", "families/background_02", "families/background_03", "families/background_04", "families/background_05", "families/background_06", "families/background_07", "families/background_08", "families/background_09",
				"families/background_10", "families/background_11", "families/background_12", "families/background_13", "families/background_14", "families/background_15", "families/background_16"
			},
			frames = new List<string>
			{
				"families/frame_00", "families/frame_01", "families/frame_02", "families/frame_03", "families/frame_04", "families/frame_05", "families/frame_06", "families/frame_07", "families/frame_08", "families/frame_09",
				"families/frame_10", "families/frame_11", "families/frame_12", "families/frame_13", "families/frame_14", "families/frame_15", "families/frame_16", "families/frame_17", "families/frame_18", "families/frame_19",
				"families/frame_20", "families/frame_21", "families/frame_22"
			}
		});
	}
}
