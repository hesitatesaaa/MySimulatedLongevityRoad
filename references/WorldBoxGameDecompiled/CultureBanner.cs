using UnityEngine;
using UnityEngine.UI;

public class CultureBanner : BannerGeneric<Culture, CultureData>
{
	protected override MetaType meta_type => MetaType.Culture;

	protected override string tooltip_id => "culture";

	protected override void loadPartBackground()
	{
		part_background = ((Component)((Component)this).transform.FindRecursive("Decor")).GetComponent<Image>();
	}

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.culture = meta_object;
		return tooltipData;
	}

	protected override void setupBanner()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		part_icon.sprite = meta_object.getElementSprite();
		part_background.sprite = meta_object.getDecorSprite();
		ColorAsset colorAsset = meta_object.getColor();
		((Graphic)part_icon).color = colorAsset.getColorBanner();
		((Graphic)part_background).color = colorAsset.getColorMainSecond();
		((Graphic)part_frame).color = colorAsset.getColorMainSecond();
	}
}
