using System;
using UnityEngine;

public class TraitButton<TTrait> : AugmentationButton<TTrait> where TTrait : BaseTrait<TTrait>
{
	public override void load(TTrait pElement)
	{
		create();
		augmentation_asset = pElement;
		image.sprite = augmentation_asset.getSprite();
		((Object)((Component)this).gameObject).name = getElementType() + "_" + augmentation_asset.id;
		loadLegendaryOutline();
	}

	internal virtual void load(string pElementID)
	{
		throw new NotImplementedException();
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

	protected override void fillTooltipData(TTrait pTrait)
	{
		TooltipData tooltipData = tooltipDataBuilder();
		tooltipData.is_editor_augmentation_button = is_editor_button;
		Tooltip.show(this, tooltip_type, tooltipData);
	}

	protected override bool unlockElement()
	{
		return augmentation_asset.unlock();
	}

	protected override string getElementType()
	{
		return "trait";
	}

	protected override void startSignal()
	{
		AchievementLibrary.traits_explorer_40.checkBySignal();
		AchievementLibrary.traits_explorer_60.checkBySignal();
		AchievementLibrary.traits_explorer_90.checkBySignal();
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
