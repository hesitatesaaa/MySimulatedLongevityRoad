using System.Collections.Generic;

public class ClanTraitsEditor : TraitsEditor<ClanTrait, ClanTraitButton, ClanTraitEditorButton, ClanTraitGroupAsset, ClanTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Clan;

	protected override List<ClanTraitGroupAsset> augmentation_groups_list => AssetManager.clan_trait_groups.list;

	protected override List<ClanTrait> all_augmentations_list => AssetManager.clan_traits.list;

	protected override ClanTrait edited_marker_augmentation => AssetManager.clan_traits.get("geb");

	protected override void startSignal()
	{
		AchievementLibrary.trait_explorer_clan.checkBySignal();
	}
}
