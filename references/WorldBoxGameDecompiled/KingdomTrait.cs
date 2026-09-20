using System;
using System.Collections.Generic;

[Serializable]
public class KingdomTrait : BaseTrait<KingdomTrait>
{
	public bool is_local_tax_trait;

	public bool is_tribute_tax_trait;

	public float tax_rate;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_traits_kingdom;

	public override string typed_id => "kingdom_trait";

	protected override IEnumerable<ITraitsOwner<KingdomTrait>> getRelatedMetaList()
	{
		return World.world.kingdoms;
	}

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.kingdoms_traits_groups.get(group_id);
	}
}
