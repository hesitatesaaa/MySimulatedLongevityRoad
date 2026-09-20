using System.Collections.Generic;
using UnityEngine;

public class SubspeciesBirthTraitsEditor : TraitsEditor<ActorTrait, ActorTraitButton, ActorTraitEditorButton, ActorTraitGroupAsset, ActorTraitGroupElement>
{
	private SubspeciesWindow _subspecies_window;

	protected override MetaType meta_type => MetaType.Subspecies;

	protected override List<ActorTraitGroupAsset> augmentation_groups_list => AssetManager.trait_groups.list;

	protected override ActorTrait edited_marker_augmentation => AssetManager.traits.get("scar_of_divinity");

	protected override List<ActorTrait> all_augmentations_list => AssetManager.traits.list;

	public override ITraitsOwner<ActorTrait> getTraitsOwner()
	{
		return getTraitsContainer();
	}

	private SubspeciesActorBirthTraits getTraitsContainer()
	{
		return getSelectedSubspecies().getActorBirthTraits();
	}

	protected override void create()
	{
		base.create();
		_subspecies_window = ((Component)this).GetComponentInParent<SubspeciesWindow>();
		selected_editor_buttons = new ObjectPoolGenericMono<ActorTraitButton>(prefab_augmentation, ((Component)selected_editor_augmentations_grid).transform);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		augmentations_list_link = getSelectedSubspecies().getActorBirthTraits().getTraitsAsStrings();
		augmentations_hashset.Clear();
		augmentations_hashset.UnionWith(augmentations_list_link);
		loadEditorSelectedAugmentations();
	}

	protected override void onNanoWasModified()
	{
		getSelectedSubspecies().eventGMO();
		base.onNanoWasModified();
	}

	protected override void loadEditorSelectedButton(ActorTraitButton pButton, string pAugmentationId)
	{
		base.loadEditorSelectedButton(pButton, pAugmentationId);
		pButton.load(pAugmentationId);
	}

	protected override bool isAugmentationExists(string pId)
	{
		return AssetManager.traits.has(pId);
	}

	protected override void metaAugmentationClick(ActorTraitEditorButton pButton)
	{
		base.metaAugmentationClick(pButton);
		augmentations_hashset.Clear();
		augmentations_hashset.UnionWith(getTraitsContainer().getTraitsAsStrings());
		loadEditorSelectedAugmentations();
	}

	protected override void refreshAugmentationWindow()
	{
		_subspecies_window.updateStats();
		_subspecies_window.reloadBanner();
	}

	protected override void showActiveButtons()
	{
		loadEditorSelectedAugmentations();
	}

	private Subspecies getSelectedSubspecies()
	{
		return (Subspecies)base.meta_type_asset.get_selected();
	}
}
