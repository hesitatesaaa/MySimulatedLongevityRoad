using System.Collections.Generic;

public class SubspeciesTraitsEditor : TraitsEditor<SubspeciesTrait, SubspeciesTraitButton, SubspeciesTraitEditorButton, SubspeciesTraitGroupAsset, SubspeciesTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Subspecies;

	protected override List<SubspeciesTraitGroupAsset> augmentation_groups_list => AssetManager.subspecies_trait_groups.list;

	protected override List<SubspeciesTrait> all_augmentations_list => AssetManager.subspecies_traits.list;

	protected override SubspeciesTrait edited_marker_augmentation => AssetManager.subspecies_traits.get("gmo");

	protected override List<string> filter_traits => getActorAsset().trait_filter_subspecies;

	protected override List<string> filter_trait_groups => getActorAsset().trait_group_filter_subspecies;

	protected override void onNanoWasModified()
	{
		((Subspecies)getTraitsOwner()).eventGMO();
		base.onNanoWasModified();
	}

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_subspecies.checkBySignal();
		AchievementLibrary.swarm.checkBySignal(getTraitsOwner());
	}
}
