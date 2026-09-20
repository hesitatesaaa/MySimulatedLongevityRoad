public class ClanTraitGroupLibrary : BaseCategoryLibrary<ClanTraitGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new ClanTraitGroupAsset
		{
			id = "spirit",
			name = "trait_group_spirit",
			color = "#BAFFC2"
		});
		add(new ClanTraitGroupAsset
		{
			id = "mind",
			name = "trait_group_mind",
			color = "#BAF0F4"
		});
		add(new ClanTraitGroupAsset
		{
			id = "body",
			name = "trait_group_body",
			color = "#FF6B86"
		});
		add(new ClanTraitGroupAsset
		{
			id = "chaos",
			name = "trait_group_chaos",
			color = "#FF6A00"
		});
		add(new ClanTraitGroupAsset
		{
			id = "harmony",
			name = "trait_group_harmony",
			color = "#DD96FF"
		});
		add(new ClanTraitGroupAsset
		{
			id = "fate",
			name = "trait_group_fate",
			color = "#ffd82f"
		});
		add(new ClanTraitGroupAsset
		{
			id = "special",
			name = "trait_group_special",
			color = "#FF8F44"
		});
	}
}
