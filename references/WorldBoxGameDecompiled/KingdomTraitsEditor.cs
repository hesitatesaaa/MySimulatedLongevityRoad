using System.Collections.Generic;

public class KingdomTraitsEditor : TraitsEditor<KingdomTrait, KingdomTraitButton, KingdomTraitEditorButton, KingdomTraitGroupAsset, KingdomTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Kingdom;

	protected override List<KingdomTraitGroupAsset> augmentation_groups_list => AssetManager.kingdoms_traits_groups.list;

	protected override List<KingdomTrait> all_augmentations_list => AssetManager.kingdoms_traits.list;
}
