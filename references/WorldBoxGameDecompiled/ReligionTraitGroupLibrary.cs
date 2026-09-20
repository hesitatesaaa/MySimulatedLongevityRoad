public class ReligionTraitGroupLibrary : BaseCategoryLibrary<ReligionTraitGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new ReligionTraitGroupAsset
		{
			id = "harmony",
			name = "trait_group_harmony",
			color = "#FFE8AA"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "creation",
			name = "trait_group_creation",
			color = "#A5FF7F"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "destruction",
			name = "trait_group_destruction",
			color = "#FF4949"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "restoration",
			name = "trait_group_restoration",
			color = "#FFE97C"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "necromancy",
			name = "trait_group_necromancy",
			color = "#ACB6C1"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "protection",
			name = "trait_group_protection",
			color = "#56FFFF"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "the_void",
			name = "trait_group_the_void",
			color = "#FF00D5"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "transformation",
			name = "trait_group_transformation",
			color = "#C2C1FF"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "fate",
			name = "trait_group_fate",
			color = "#ffd82f"
		});
		add(new ReligionTraitGroupAsset
		{
			id = "special",
			name = "trait_group_special",
			color = "#FF8F44"
		});
	}
}
