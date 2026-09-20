using UnityEngine.UI;

public class ClanBanner : BannerGeneric<Clan, ClanData>
{
	protected override MetaType meta_type => MetaType.Clan;

	protected override string tooltip_id => "clan";

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.clan = meta_object;
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
	}
}
