public class KingdomTraitGroupLibrary : BaseCategoryLibrary<KingdomTraitGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new KingdomTraitGroupAsset
		{
			id = "tribute",
			name = "trait_group_tribute",
			color = "#BAFFC2"
		});
		add(new KingdomTraitGroupAsset
		{
			id = "local_tax",
			name = "trait_group_local_tax",
			color = "#BAFFC2"
		});
		add(new KingdomTraitGroupAsset
		{
			id = "miscellaneous",
			name = "trait_group_miscellaneous",
			color = "#D8D8D8"
		});
		add(new KingdomTraitGroupAsset
		{
			id = "fate",
			name = "trait_group_fate",
			color = "#ffd82f"
		});
	}
}
