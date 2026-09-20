public class LanguageTraitGroupLibrary : BaseCategoryLibrary<LanguageTraitGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new LanguageTraitGroupAsset
		{
			id = "knowledge",
			name = "trait_group_knowledge",
			color = "#BAF0F4"
		});
		add(new LanguageTraitGroupAsset
		{
			id = "spirit",
			name = "trait_group_spirit",
			color = "#BAFFC2"
		});
		add(new LanguageTraitGroupAsset
		{
			id = "harmony",
			name = "trait_group_harmony",
			color = "#FFFAA3"
		});
		add(new LanguageTraitGroupAsset
		{
			id = "chaos",
			name = "trait_group_chaos",
			color = "#FF6B86"
		});
		add(new LanguageTraitGroupAsset
		{
			id = "miscellaneous",
			name = "trait_group_miscellaneous",
			color = "#D8D8D8"
		});
		add(new LanguageTraitGroupAsset
		{
			id = "fate",
			name = "trait_group_fate",
			color = "#ffd82f"
		});
		add(new LanguageTraitGroupAsset
		{
			id = "special",
			name = "trait_group_special",
			color = "#FF8F44"
		});
	}
}
