using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlotsEditor : AugmentationsEditor<PlotAsset, PlotButton, PlotEditorButton, PlotCategoryAsset, PlotGroupElement, IPlotsWindow, IPlotsEditor>, IPlotsEditor, IAugmentationsEditor
{
	protected override List<PlotCategoryAsset> augmentation_groups_list => AssetManager.plot_category_library.list;

	protected override PlotAsset edited_marker_augmentation => null;

	protected override List<PlotAsset> all_augmentations_list => AssetManager.plots_library.list;

	protected override void metaAugmentationClick(PlotEditorButton pButton)
	{
		if (isAugmentationAvailable(pButton.augmentation_button))
		{
			PlotButton augmentation_button = pButton.augmentation_button;
			bool num = hasAugmentation(augmentation_button);
			if (getCurrentActor().hasPlot())
			{
				removeAugmentation(augmentation_button);
			}
			if (!num)
			{
				addAugmentation(augmentation_button);
			}
			base.metaAugmentationClick(pButton);
		}
	}

	protected override void createButton(PlotAsset pElement, PlotGroupElement pGroup)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		if (pElement.show_in_meta_editor)
		{
			PlotEditorButton tEditorButton = Object.Instantiate<PlotEditorButton>(prefab_editor_augmentation, pGroup.augmentation_buttons_transform);
			tEditorButton.augmentation_button.is_editor_button = true;
			tEditorButton.augmentation_button.load(pElement);
			all_augmentation_buttons.Add(tEditorButton);
			pGroup.augmentation_buttons.Add(tEditorButton);
			((UnityEventBase)tEditorButton.augmentation_button.button.onClick).RemoveAllListeners();
			((UnityEvent)tEditorButton.augmentation_button.button.onClick).AddListener((UnityAction)delegate
			{
				editorButtonClick(tEditorButton);
			});
			((Component)tEditorButton).gameObject.SetActive(true);
		}
	}

	protected override bool hasAugmentation(PlotButton pButton)
	{
		Actor currentActor = getCurrentActor();
		if (!currentActor.hasPlot())
		{
			return false;
		}
		if (currentActor.plot.getAsset().id != pButton.getElementId())
		{
			return false;
		}
		return true;
	}

	protected override bool addAugmentation(PlotButton pButton)
	{
		Actor currentActor = getCurrentActor();
		currentActor.addStatusEffect("voices_in_my_head");
		if (currentActor.hasPlot())
		{
			World.world.plots.cancelPlot(currentActor.plot);
			currentActor.plot = null;
		}
		PlotAsset elementAsset = pButton.getElementAsset();
		if (!elementAsset.canBeDoneByRole(currentActor))
		{
			WorldTip.showNowTop("plot_force_not_possible");
			return false;
		}
		if (elementAsset.check_can_be_forced != null && !elementAsset.check_can_be_forced(currentActor))
		{
			WorldTip.showNowTop("plot_force_bad_conditions");
			return false;
		}
		if (!World.world.plots.tryStartPlot(currentActor, elementAsset))
		{
			WorldTip.showNowTop("plot_force_failed");
			return false;
		}
		WorldTip.showNowTop("plot_force_started");
		currentActor.cancelAllBeh();
		currentActor.setTask("check_plot");
		return true;
	}

	protected override bool removeAugmentation(PlotButton pButton)
	{
		getCurrentActor().setPlot(null);
		return true;
	}

	protected override void showActiveButtons()
	{
	}

	protected override void startSignal()
	{
		AchievementLibrary.plots_explorer.checkBySignal();
	}

	protected override bool isAugmentationExists(string pId)
	{
		return AssetManager.plots_library.has(pId);
	}

	private Actor getCurrentActor()
	{
		return SelectedUnit.unit;
	}
}
