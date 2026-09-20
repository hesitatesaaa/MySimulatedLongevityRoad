using System;
using System.Collections.Generic;

[Serializable]
public class ReligionTrait : BaseTrait<ReligionTrait>
{
	public string transformation_biome_id;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_traits_religion;

	public override string typed_id => "religion_trait";

	protected override IEnumerable<ITraitsOwner<ReligionTrait>> getRelatedMetaList()
	{
		return World.world.religions;
	}

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.religion_trait_groups.get(group_id);
	}
}
