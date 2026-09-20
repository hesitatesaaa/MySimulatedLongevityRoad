public class ActorTraitGroupLibrary : BaseCategoryLibrary<ActorTraitGroupAsset>
{
	public override void init()
	{
		base.init();
		add(new ActorTraitGroupAsset
		{
			id = "cognitive",
			name = "trait_group_cognitive",
			color = "#5EFFFF"
		});
		add(new ActorTraitGroupAsset
		{
			id = "mind",
			name = "trait_group_mind",
			color = "#BAF0F4"
		});
		add(new ActorTraitGroupAsset
		{
			id = "spirit",
			name = "trait_group_spirit",
			color = "#BC42FF"
		});
		add(new ActorTraitGroupAsset
		{
			id = "physique",
			name = "trait_group_physique",
			color = "#FF6145"
		});
		add(new ActorTraitGroupAsset
		{
			id = "health",
			name = "trait_group_health",
			color = "#89FF56"
		});
		add(new ActorTraitGroupAsset
		{
			id = "body",
			name = "trait_group_body",
			color = "#FF6B86"
		});
		add(new ActorTraitGroupAsset
		{
			id = "appearance",
			name = "trait_group_appearance",
			color = "#FF6DEB"
		});
		add(new ActorTraitGroupAsset
		{
			id = "protection",
			name = "trait_group_protection",
			color = "#FF6B86"
		});
		add(new ActorTraitGroupAsset
		{
			id = "skills",
			name = "trait_group_skills",
			color = "#BCBCBC"
		});
		add(new ActorTraitGroupAsset
		{
			id = "merits",
			name = "trait_group_merits",
			color = "#FFDA23"
		});
		add(new ActorTraitGroupAsset
		{
			id = "acquired",
			name = "trait_group_acquired",
			color = "#A3AFFF"
		});
		add(new ActorTraitGroupAsset
		{
			id = "fun",
			name = "trait_group_fun",
			color = "#FFFAA3"
		});
		add(new ActorTraitGroupAsset
		{
			id = "fate",
			name = "trait_group_fate",
			color = "#ffd82f"
		});
		add(new ActorTraitGroupAsset
		{
			id = "miscellaneous",
			name = "trait_group_miscellaneous",
			color = "#D8D8D8"
		});
		add(new ActorTraitGroupAsset
		{
			id = "special",
			name = "trait_group_special",
			color = "#FF8F44"
		});
	}
}
