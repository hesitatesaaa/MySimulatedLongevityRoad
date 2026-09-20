using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EquipmentEditor : AugmentationsEditor<EquipmentAsset, EquipmentButton, EquipmentEditorButton, ItemGroupAsset, EquipmentGroupElement, IEquipmentWindow, IEquipmentEditor>, IEquipmentEditor, IAugmentationsEditor
{
	[SerializeField]
	protected Sprite sprite_art;

	[SerializeField]
	protected Sprite sprite_art_void;

	protected override List<ItemGroupAsset> augmentation_groups_list => AssetManager.item_groups.list;

	protected override EquipmentAsset edited_marker_augmentation => null;

	protected override List<EquipmentAsset> all_augmentations_list => AssetManager.items.list;

	protected override void onEnableRain()
	{
		rain_editor_state = PlayerConfig.instance.data.equipment_editor_state;
		augmentations_list_link = PlayerConfig.instance.data.equipment_editor;
		validateRainData();
		augmentations_hashset.Clear();
		augmentations_hashset.UnionWith(augmentations_list_link);
		loadEditorSelectedAugmentations();
		rain_state_toggle_action = delegate
		{
			toggleRainState(ref PlayerConfig.instance.data.equipment_editor_state);
		};
		rain_state_switcher.toggleState(rain_editor_state == RainState.Remove);
	}

	protected override void OnEnable()
	{
		if (!rain_editor)
		{
			Actor currentActor = getCurrentActor();
			if (!currentActor.canEditEquipment())
			{
				return;
			}
			foreach (EquipmentEditorButton all_augmentation_button in all_augmentation_buttons)
			{
				EquipmentAsset elementAsset = all_augmentation_button.augmentation_button.getElementAsset();
				bool active = true;
				if (!currentActor.asset.canEditItem(elementAsset))
				{
					active = false;
				}
				((Component)all_augmentation_button).gameObject.SetActive(active);
			}
		}
		base.OnEnable();
	}

	protected override void metaAugmentationClick(EquipmentEditorButton pButton)
	{
		if (!isAugmentationAvailable(pButton.augmentation_button))
		{
			return;
		}
		EquipmentButton augmentation_button = pButton.augmentation_button;
		EquipmentAsset elementAsset = pButton.augmentation_button.getElementAsset();
		if (canChangeSlot(elementAsset))
		{
			bool num = hasAugmentation(augmentation_button);
			if (!isSlotEmpty(augmentation_button))
			{
				removeAugmentation(augmentation_button);
			}
			if (!num)
			{
				addAugmentation(augmentation_button);
			}
		}
		augmentation_window.checkEquipmentTabIcon();
		base.metaAugmentationClick(pButton);
	}

	protected override void rainAugmentationClick(EquipmentEditorButton pButton)
	{
		if (isAugmentationAvailable(pButton.augmentation_button))
		{
			string id = pButton.augmentation_button.getElementAsset().id;
			if (!augmentations_hashset.Contains(id))
			{
				augmentations_hashset.Add(id);
			}
			else
			{
				augmentations_hashset.Remove(id);
			}
			base.rainAugmentationClick(pButton);
		}
	}

	protected override void showActiveButtons()
	{
		augmentation_window.reloadEquipment();
	}

	protected override ListPool<EquipmentAsset> getOrderedAugmentationsList()
	{
		return new ListPool<EquipmentAsset>(all_augmentations_list);
	}

	protected override void createButton(EquipmentAsset pElement, EquipmentGroupElement pGroup)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		if (pElement.show_in_meta_editor)
		{
			bool active = true;
			if (!rain_editor && !getCurrentActor().asset.canEditItem(pElement))
			{
				active = false;
			}
			EquipmentEditorButton tEditorButton = Object.Instantiate<EquipmentEditorButton>(prefab_editor_augmentation, pGroup.augmentation_buttons_transform);
			tEditorButton.augmentation_button.is_editor_button = true;
			tEditorButton.augmentation_button.load(pElement);
			all_augmentation_buttons.Add(tEditorButton);
			pGroup.augmentation_buttons.Add(tEditorButton);
			((UnityEventBase)tEditorButton.augmentation_button.button.onClick).RemoveAllListeners();
			((UnityEvent)tEditorButton.augmentation_button.button.onClick).AddListener((UnityAction)delegate
			{
				editorButtonClick(tEditorButton);
			});
			((Component)tEditorButton).gameObject.SetActive(active);
		}
	}

	protected override void startSignal()
	{
		AchievementLibrary.equipment_explorer.checkBySignal();
	}

	private bool canChangeSlot(EquipmentAsset pAsset)
	{
		if (!pAsset.can_be_given)
		{
			return false;
		}
		return getSlotFromCurrentActor(pAsset.equipment_type).canChangeSlot();
	}

	private bool isSlotEmpty(EquipmentButton pButton)
	{
		EquipmentAsset elementAsset = pButton.getElementAsset();
		return getSlotFromCurrentActor(elementAsset.equipment_type).isEmpty();
	}

	protected override bool hasAugmentation(EquipmentButton pButton)
	{
		EquipmentAsset elementAsset = pButton.getElementAsset();
		ActorEquipmentSlot slotFromCurrentActor = getSlotFromCurrentActor(elementAsset.equipment_type);
		if (slotFromCurrentActor.isEmpty())
		{
			return false;
		}
		Item item = slotFromCurrentActor.getItem();
		string id = pButton.getElementAsset().id;
		if (item.getAsset().id == id)
		{
			return true;
		}
		return false;
	}

	protected override bool addAugmentation(EquipmentButton pButton)
	{
		Actor currentActor = getCurrentActor();
		EquipmentAsset elementAsset = pButton.getElementAsset();
		Item item = World.world.items.generateItem(elementAsset, currentActor.kingdom, World.world.map_stats.player_name, 1, currentActor, 0, pByPlayer: true);
		item.addMod("divine_rune");
		currentActor.equipment.setItem(item, currentActor);
		return true;
	}

	protected override bool removeAugmentation(EquipmentButton pButton)
	{
		Actor currentActor = getCurrentActor();
		EquipmentType equipment_type = pButton.getElementAsset().equipment_type;
		currentActor.equipment.getSlot(equipment_type).takeAwayItem();
		currentActor.setStatsDirty();
		return true;
	}

	private ActorEquipmentSlot getSlotFromCurrentActor(EquipmentType pType)
	{
		return getCurrentActor().equipment.getSlot(pType);
	}

	private Actor getCurrentActor()
	{
		return SelectedUnit.unit;
	}

	protected override void loadEditorSelectedButton(EquipmentButton pButton, string pAugmentationId)
	{
		base.loadEditorSelectedButton(pButton, pAugmentationId);
		EquipmentAsset pElement = AssetManager.items.get(pAugmentationId);
		pButton.load(pElement);
	}

	protected override bool isAugmentationExists(string pId)
	{
		return AssetManager.items.has(pId);
	}

	protected override void toggleRainState(ref RainState pState)
	{
		base.toggleRainState(ref pState);
		art.sprite = ((pState == RainState.Add) ? sprite_art : sprite_art_void);
		if (pState == RainState.Add)
		{
			augmentations_hashset.Clear();
			augmentations_hashset.UnionWith(augmentations_list_link);
			reloadButtons();
		}
	}
}
