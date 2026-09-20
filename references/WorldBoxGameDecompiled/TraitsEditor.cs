using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class TraitsEditor<TTrait, TTraitButton, TTraitEditorButton, TTraitGroupAsset, TTraitGroup> : AugmentationsEditor<TTrait, TTraitButton, TTraitEditorButton, TTraitGroupAsset, TTraitGroup, ITraitWindow<TTrait, TTraitButton>, ITraitsEditor<TTrait>>, ITraitsEditor<TTrait>, IAugmentationsEditor where TTrait : BaseTrait<TTrait> where TTraitButton : TraitButton<TTrait> where TTraitEditorButton : TraitEditorButton<TTraitButton, TTrait> where TTraitGroupAsset : BaseTraitGroupAsset where TTraitGroup : TraitGroupElement<TTrait, TTraitButton, TTraitEditorButton>
{
	protected virtual MetaType meta_type
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	protected MetaTypeAsset meta_type_asset => AssetManager.meta_type_library.getAsset(meta_type);

	protected virtual List<string> filter_traits => null;

	protected virtual List<string> filter_trait_groups => null;

	protected override void OnEnable()
	{
		if (!rain_editor && filter_traits != null)
		{
			foreach (TTraitEditorButton all_augmentation_button in all_augmentation_buttons)
			{
				TTrait elementAsset = all_augmentation_button.augmentation_button.getElementAsset();
				bool active = !filter_traits.Contains(elementAsset.id);
				((Component)all_augmentation_button).gameObject.SetActive(active);
			}
		}
		base.OnEnable();
	}

	protected override void metaAugmentationClick(TTraitEditorButton pButton)
	{
		TTraitButton augmentation_button = pButton.augmentation_button;
		TTrait elementAsset = pButton.augmentation_button.getElementAsset();
		bool flag = hasAugmentation(augmentation_button);
		bool flag2 = elementAsset.isAvailable();
		bool unlocked_with_achievement = elementAsset.unlocked_with_achievement;
		if (!flag2 && (!unlocked_with_achievement || (unlocked_with_achievement && !flag)))
		{
			return;
		}
		if (elementAsset.can_be_removed)
		{
			if (flag)
			{
				removeAugmentation(augmentation_button);
			}
			else
			{
				if (elementAsset.hasOppositeTraits())
				{
					foreach (TTrait opposite_trait in elementAsset.opposite_traits)
					{
						removeTrait(opposite_trait);
					}
				}
				addAugmentation(augmentation_button);
			}
			onNanoWasModified();
		}
		base.metaAugmentationClick(pButton);
	}

	protected override void rainAugmentationClick(TTraitEditorButton pButton)
	{
		if (!isAugmentationAvailable(pButton.augmentation_button))
		{
			return;
		}
		TTrait elementAsset = pButton.augmentation_button.getElementAsset();
		if (augmentations_hashset.Contains(elementAsset.id))
		{
			augmentations_hashset.Remove(elementAsset.id);
		}
		else
		{
			if (elementAsset.hasOppositeTraits())
			{
				foreach (TTrait opposite_trait in elementAsset.opposite_traits)
				{
					augmentations_hashset.Remove(opposite_trait.id);
				}
			}
			augmentations_hashset.Add(elementAsset.id);
		}
		base.rainAugmentationClick(pButton);
	}

	protected override void showActiveButtons()
	{
		augmentation_window.reloadTraits(pAnimated: false);
	}

	protected override void createButton(TTrait pElement, TTraitGroup pGroup)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		TTraitEditorButton tEditorButton = Object.Instantiate<TTraitEditorButton>(prefab_editor_augmentation, pGroup.augmentation_buttons_transform);
		tEditorButton.augmentation_button.load(pElement.id);
		tEditorButton.augmentation_button.is_editor_button = true;
		all_augmentation_buttons.Add(tEditorButton);
		pGroup.augmentation_buttons.Add(tEditorButton);
		((UnityEvent)tEditorButton.augmentation_button.button.onClick).AddListener((UnityAction)delegate
		{
			editorButtonClick(tEditorButton);
		});
	}

	protected override void startSignal()
	{
		AchievementLibrary.traits_explorer_40.checkBySignal();
		AchievementLibrary.traits_explorer_60.checkBySignal();
		AchievementLibrary.traits_explorer_90.checkBySignal();
	}

	protected override void onNanoWasModified()
	{
		if (edited_marker_augmentation != null)
		{
			addTrait(edited_marker_augmentation);
		}
	}

	protected override void checkEnabledGroups()
	{
		foreach (KeyValuePair<string, TTraitGroup> dict_group in dict_groups)
		{
			string key = dict_group.Key;
			TTraitGroup value = dict_group.Value;
			if (filter_trait_groups != null && filter_trait_groups.Contains(key))
			{
				((Component)value).gameObject.SetActive(false);
				continue;
			}
			bool active = value.countActiveButtons() > 0;
			((Component)value).gameObject.SetActive(active);
		}
	}

	protected override bool hasAugmentation(TTraitButton pButton)
	{
		return hasTrait(pButton.getElementAsset());
	}

	protected override bool addAugmentation(TTraitButton pButton)
	{
		return addTrait(pButton.getElementAsset());
	}

	protected override bool removeAugmentation(TTraitButton pButton)
	{
		return removeTrait(pButton.getElementAsset());
	}

	public virtual ITraitsOwner<TTrait> getTraitsOwner()
	{
		return (ITraitsOwner<TTrait>)meta_type_asset.get_selected();
	}

	public ActorAsset getActorAsset()
	{
		return getTraitsOwner().getActorAsset();
	}

	protected virtual bool hasTrait(TTrait pTrait)
	{
		return getTraitsOwner().hasTrait(pTrait);
	}

	protected virtual bool addTrait(TTrait pTrait)
	{
		getTraitsOwner().traitModifiedEvent();
		return getTraitsOwner().addTrait(pTrait);
	}

	protected virtual bool removeTrait(TTrait pTrait)
	{
		return getTraitsOwner().removeTrait(pTrait);
	}
}
