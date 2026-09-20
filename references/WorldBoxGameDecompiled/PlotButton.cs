using UnityEngine;
using UnityEngine.UI;

public class PlotButton : AugmentationButton<PlotAsset>
{
	protected override string tooltip_type => "plot_in_editor";

	public override void load(PlotAsset pElement)
	{
		create();
		augmentation_asset = pElement;
		image.sprite = augmentation_asset.getSprite();
		((Object)((Component)this).gameObject).name = getElementType() + "_" + augmentation_asset.id;
		loadLegendaryOutline();
	}

	protected override void Update()
	{
		if (!is_editor_button)
		{
			if (augmentation_asset.unlocked_with_achievement)
			{
				((Component)locked_bg).gameObject.SetActive(false);
				return;
			}
			bool active = !augmentation_asset.isAvailable();
			((Component)locked_bg).gameObject.SetActive(active);
		}
	}

	public override void updateIconColor(bool pSelected)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!is_editor_button)
		{
			return;
		}
		if (!getElementAsset().isAvailable())
		{
			((Graphic)image).color = Toolbox.color_black;
			return;
		}
		if (pSelected)
		{
			((Graphic)image).color = Toolbox.color_augmentation_selected;
			return;
		}
		Actor unit = SelectedUnit.unit;
		if (augmentation_asset.canBeDoneByRole(unit))
		{
			if (augmentation_asset.check_can_be_forced != null && !augmentation_asset.check_can_be_forced(SelectedUnit.unit))
			{
				((Graphic)image).color = Toolbox.color_gray;
			}
			else
			{
				((Graphic)image).color = Toolbox.color_white;
			}
		}
		else
		{
			((Graphic)image).color = Toolbox.color_gray;
		}
	}

	protected override bool unlockElement()
	{
		return augmentation_asset.unlock();
	}

	protected override void startSignal()
	{
		AchievementLibrary.plots_explorer.checkBySignal();
	}

	protected override void fillTooltipData(PlotAsset pElement)
	{
		Tooltip.show(this, tooltip_type, tooltipDataBuilder());
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			plot_asset = augmentation_asset
		};
	}

	protected override string getElementType()
	{
		return "plot";
	}

	public override string getElementId()
	{
		return augmentation_asset.id;
	}

	protected override Rarity getRarity()
	{
		return augmentation_asset.rarity;
	}
}
