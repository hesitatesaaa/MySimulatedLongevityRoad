using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorTraitsEditor : TraitsEditor<ActorTrait, ActorTraitButton, ActorTraitEditorButton, ActorTraitGroupAsset, ActorTraitGroupElement>
{
	protected override MetaType meta_type => MetaType.Unit;

	protected override List<ActorTraitGroupAsset> augmentation_groups_list => AssetManager.trait_groups.list;

	protected override ActorTrait edited_marker_augmentation => AssetManager.traits.get("scar_of_divinity");

	protected override List<ActorTrait> all_augmentations_list => AssetManager.traits.list;

	protected override void onEnableRain()
	{
		TraitRainAsset tAsset = AssetManager.trait_rains.get(Config.selected_trait_editor);
		augmentations_list_link = tAsset.get_list();
		rain_editor_state = tAsset.get_state();
		rain_state_toggle_action = delegate
		{
			toggleRainState(tAsset);
		};
		art.sprite = ((rain_editor_state == RainState.Add) ? tAsset.getSpriteArt() : tAsset.getSpriteArtVoid());
		validateRainData();
		rain_state_switcher.toggleState(rain_editor_state == RainState.Remove);
		augmentations_hashset.Clear();
		augmentations_hashset.UnionWith(augmentations_list_link);
		saveRainValues();
		loadEditorSelectedAugmentations();
		GodPower godPower = AssetManager.powers.get(Config.selected_trait_editor);
		window_title_text.key = godPower.getLocaleID();
		window_title_text.updateText();
		power_icon.sprite = godPower.getIconSprite();
		for (int num = 0; num < powers_icons.childCount; num++)
		{
			((Component)((Component)powers_icons).transform.GetChild(num)).GetComponent<Image>().sprite = godPower.getIconSprite();
		}
	}

	protected override bool addTrait(ActorTrait pTrait)
	{
		return base.addTrait(pTrait);
	}

	protected override void onNanoWasModified()
	{
		base.onNanoWasModified();
		Actor obj = (Actor)getTraitsOwner();
		obj.makeStunnedFromUI();
		obj.updateStats();
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

	public override void scrollToGroupStarter(GameObject pButton, bool pIgnoreTooltipCheck)
	{
		if (rain_editor || getActorAsset().can_edit_traits)
		{
			base.scrollToGroupStarter(pButton, pIgnoreTooltipCheck);
		}
	}

	protected void toggleRainState(TraitRainAsset pAsset)
	{
		RainState pState;
		if (pAsset.get_state() == RainState.Add)
		{
			pState = RainState.Remove;
			rain_state_switcher.toggleState(pState: true);
			art.sprite = pAsset.getSpriteArtVoid();
		}
		else
		{
			pState = RainState.Add;
			rain_state_switcher.toggleState(pState: false);
			art.sprite = pAsset.getSpriteArt();
		}
		IllustrationFadeIn component = ((Component)art).GetComponent<IllustrationFadeIn>();
		if ((Object)(object)component != (Object)null)
		{
			component.startTween();
		}
		pAsset.set_state(pState);
		rain_editor_state = pState;
		reloadButtons();
	}
}
