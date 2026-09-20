using UnityEngine.UI;

public class FamilyBanner : BannerGeneric<Family, FamilyData>
{
	protected override MetaType meta_type => MetaType.Family;

	protected override string tooltip_id => "family";

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.family = meta_object;
		return tooltipData;
	}

	protected override void setupBanner()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		part_background.sprite = meta_object.getSpriteBackground();
		part_icon.sprite = meta_object.getSpriteIcon();
		part_frame.sprite = meta_object.getSpriteFrame();
		ColorAsset colorAsset = meta_object.getColor();
		((Graphic)part_background).color = colorAsset.getColorMainSecond();
	}
}
