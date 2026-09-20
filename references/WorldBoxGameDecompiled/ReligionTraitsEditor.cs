using System.Collections.Generic;

public class ReligionTraitsEditor : TraitsEditor<ReligionTrait, ReligionTraitButton, ReligionTraitEditorButton, ReligionTraitGroupAsset, ReligionTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Religion;

	protected override List<ReligionTraitGroupAsset> augmentation_groups_list => AssetManager.religion_trait_groups.list;

	protected override List<ReligionTrait> all_augmentations_list => AssetManager.religion_traits.list;

	protected override ReligionTrait edited_marker_augmentation => AssetManager.religion_traits.get("divine_insight");

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_religion.checkBySignal();
	}
}
