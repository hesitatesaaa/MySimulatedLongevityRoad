using System.Collections.Generic;

public class ClanBannerLibrary : GenericBannerLibrary
{
	public override void init()
	{
		base.init();
		main = add(new BannerAsset
		{
			id = "main",
			backgrounds = new List<string>
			{
				"clans/clan_background_00", "clans/clan_background_01", "clans/clan_background_02", "clans/clan_background_03", "clans/clan_background_04", "clans/clan_background_05", "clans/clan_background_06", "clans/clan_background_07", "clans/clan_background_08", "clans/clan_background_09",
				"clans/clan_background_10", "clans/clan_background_11", "clans/clan_background_12", "clans/clan_background_13", "clans/clan_background_14", "clans/clan_background_15", "clans/clan_background_16"
			},
			icons = new List<string>
			{
				"clans/clan_icon_00", "clans/clan_icon_01", "clans/clan_icon_02", "clans/clan_icon_03", "clans/clan_icon_04", "clans/clan_icon_05", "clans/clan_icon_06", "clans/clan_icon_07", "clans/clan_icon_08", "clans/clan_icon_09",
				"clans/clan_icon_10", "clans/clan_icon_11", "clans/clan_icon_12", "clans/clan_icon_13", "clans/clan_icon_14", "clans/clan_icon_15", "clans/clan_icon_16", "clans/clan_icon_17", "clans/clan_icon_18", "clans/clan_icon_19",
				"clans/clan_icon_20", "clans/clan_icon_21"
			}
		});
	}
}
