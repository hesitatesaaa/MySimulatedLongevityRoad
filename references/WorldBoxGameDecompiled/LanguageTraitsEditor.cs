using System.Collections.Generic;

public class LanguageTraitsEditor : TraitsEditor<LanguageTrait, LanguageTraitButton, LanguageTraitEditorButton, LanguageTraitGroupAsset, LanguageTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Language;

	protected override List<LanguageTraitGroupAsset> augmentation_groups_list => AssetManager.language_trait_groups.list;

	protected override List<LanguageTrait> all_augmentations_list => AssetManager.language_traits.list;

	protected override LanguageTrait edited_marker_augmentation => AssetManager.language_traits.get("divine_encryption");

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_language.checkBySignal();
	}
}
