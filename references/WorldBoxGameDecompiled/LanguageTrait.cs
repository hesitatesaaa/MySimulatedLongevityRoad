using System;
using System.Collections.Generic;

[Serializable]
public class LanguageTrait : BaseTrait<LanguageTrait>
{
	public BookTraitAction read_book_trait_action;

	protected override HashSet<string> progress_elements => base._progress_data?.unlocked_traits_language;

	public override string typed_id => "language_trait";

	protected override IEnumerable<ITraitsOwner<LanguageTrait>> getRelatedMetaList()
	{
		return World.world.languages;
	}

	public override BaseCategoryAsset getGroup()
	{
		return AssetManager.language_trait_groups.get(group_id);
	}
}
