using System.Collections.Generic;

public class KingdomTraitLibrary : BaseTraitLibrary<KingdomTrait>
{
	protected override string icon_path => "ui/Icons/kingdom_traits/";

	protected override List<string> getDefaultTraitsForMeta(ActorAsset pAsset)
	{
		return pAsset.default_kingdom_traits;
	}

	public override void init()
	{
		base.init();
		add(new KingdomTrait
		{
			id = "tax_rate_local_high",
			group_id = "local_tax",
			is_local_tax_trait = true,
			tax_rate = 0.7f
		});
		t.addOpposite("tax_rate_local_low");
		add(new KingdomTrait
		{
			id = "tax_rate_local_low",
			group_id = "local_tax",
			is_local_tax_trait = true,
			tax_rate = 0.2f
		});
		t.addOpposite("tax_rate_local_high");
		add(new KingdomTrait
		{
			id = "tax_rate_tribute_high",
			group_id = "tribute",
			is_tribute_tax_trait = true,
			tax_rate = 0.7f
		});
		t.addOpposite("tax_rate_tribute_low");
		add(new KingdomTrait
		{
			id = "tax_rate_tribute_low",
			group_id = "tribute",
			is_tribute_tax_trait = true,
			tax_rate = 0.2f
		});
		t.addOpposite("tax_rate_tribute_high");
		add(new KingdomTrait
		{
			id = "grin_mark",
			group_id = "fate",
			spawn_random_trait_allowed = false,
			priority = -100
		});
		t.setTraitInfoToGrinMark();
		t.setUnlockedWithAchievement("achievementCreaturesExplorer");
	}
}
