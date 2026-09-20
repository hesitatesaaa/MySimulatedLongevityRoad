using UnityEngine;
using UnityEngine.UI;

public class AllianceBanner : BannerGeneric<Alliance, AllianceData>
{
	public Sprite frame_normal;

	public Sprite frame_forced;

	protected override MetaType meta_type => MetaType.Alliance;

	protected override string tooltip_id => "alliance";

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.alliance = meta_object;
		return tooltipData;
	}

	protected override void setupBanner()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		part_background.sprite = meta_object.getBackgroundSprite();
		part_icon.sprite = meta_object.getIconSprite();
		ColorAsset colorAsset = meta_object.getColor();
		((Graphic)part_background).color = colorAsset.getColorMainSecond();
		((Graphic)part_icon).color = colorAsset.getColorBanner();
		if (meta_object.isNormalType())
		{
			part_frame.sprite = frame_normal;
		}
		else
		{
			part_frame.sprite = frame_forced;
		}
	}
}
