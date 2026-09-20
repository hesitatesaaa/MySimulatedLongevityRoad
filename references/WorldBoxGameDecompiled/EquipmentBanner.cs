using UnityEngine;

public class EquipmentBanner : BannerGeneric<Item, ItemData>
{
	[SerializeField]
	private IconOutline _outline;

	[SerializeField]
	private Sprite _frame_sprite_legendary;

	[SerializeField]
	private Sprite _frame_sprite_epic;

	protected override MetaType meta_type => MetaType.Item;

	protected override string tooltip_id => "equipment";

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.item = meta_object;
		return tooltipData;
	}

	protected override void setupBanner()
	{
		base.setupBanner();
		Item item = meta_object;
		Rarity quality = item.getQuality();
		part_icon.sprite = item.getSprite();
		bool active = true;
		switch (quality)
		{
		case Rarity.R3_Legendary:
			part_frame.sprite = _frame_sprite_legendary;
			break;
		case Rarity.R2_Epic:
			part_frame.sprite = _frame_sprite_epic;
			break;
		default:
			active = false;
			break;
		}
		((Component)part_frame).gameObject.SetActive(active);
		if (quality == Rarity.R3_Legendary)
		{
			showOutline();
		}
		else
		{
			((Component)_outline).gameObject.SetActive(false);
		}
	}

	private void showOutline()
	{
		_outline.show(RarityLibrary.legendary.color_container);
	}
}
