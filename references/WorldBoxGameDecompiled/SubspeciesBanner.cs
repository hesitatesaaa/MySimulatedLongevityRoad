using UnityEngine;
using UnityEngine.UI;

public class SubspeciesBanner : BannerGeneric<Subspecies, SubspeciesData>
{
	private Image _part_bookmark_1;

	private Image _part_bookmark_2;

	public Image unit_sprite;

	protected override MetaType meta_type => MetaType.Subspecies;

	protected override string tooltip_id => "subspecies";

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.subspecies = meta_object;
		return tooltipData;
	}

	protected override void setupParts()
	{
		base.setupParts();
		Transform obj = ((Component)this).transform.FindRecursive("Bookmark 1");
		_part_bookmark_1 = ((obj != null) ? ((Component)obj).GetComponent<Image>() : null);
		Transform obj2 = ((Component)this).transform.FindRecursive("Bookmark 2");
		_part_bookmark_2 = ((obj2 != null) ? ((Component)obj2).GetComponent<Image>() : null);
	}

	protected override void setupBanner()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		part_background.sprite = meta_object.getSpriteBackground();
		part_icon.sprite = meta_object.getSpriteIcon();
		ColorAsset colorAsset = meta_object.getColor();
		((Graphic)_part_bookmark_1).color = colorAsset.getColorMainSecond();
		((Graphic)_part_bookmark_2).color = colorAsset.getColorMain();
		Sprite unitSpriteForBanner = meta_object.getUnitSpriteForBanner();
		unit_sprite.sprite = unitSpriteForBanner;
	}
}
