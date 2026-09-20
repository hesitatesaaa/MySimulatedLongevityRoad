using System;
using System.Collections.Generic;

[Serializable]
public class ClanTrait : BaseTrait<ClanTrait>
{
	public BaseStats base_stats_male = new BaseStats();

	public BaseStats base_stats_female = new BaseStats();

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_traits_clan;

	public override string typed_id => "clan_trait";

	protected override IEnumerable<ITraitsOwner<ClanTrait>> getRelatedMetaList()
	{
		return World.world.clans;
	}

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.clan_trait_groups.get(group_id);
	}
}
