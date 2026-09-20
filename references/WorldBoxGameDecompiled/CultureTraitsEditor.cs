using System.Collections.Generic;

public class CultureTraitsEditor : TraitsEditor<CultureTrait, CultureTraitButton, CultureTraitEditorButton, CultureTraitGroupAsset, CultureTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Culture;

	protected override List<CultureTraitGroupAsset> augmentation_groups_list => AssetManager.culture_trait_groups.list;

	protected override List<CultureTrait> all_augmentations_list => AssetManager.culture_traits.list;

	protected override CultureTrait edited_marker_augmentation => AssetManager.culture_traits.get("ethno_sculpted");

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_culture.checkBySignal();
	}

	protected override void metaAugmentationClick(CultureTraitEditorButton pButton)
	{
		base.metaAugmentationClick(pButton);
		if (!(pButton.augmentation_button.getElementAsset().group_id != "succession"))
		{
			AchievementLibrary.succession.check();
		}
	}
}
