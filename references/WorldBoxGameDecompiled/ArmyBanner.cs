using UnityEngine;
using UnityEngine.UI;

public class ArmyBanner : BannerGeneric<Army, ArmyData>
{
	[SerializeField]
	private Image _species_icon;

	protected override MetaType meta_type => MetaType.Army;

	protected override string tooltip_id => "army";

	protected override void setupBanner()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		Kingdom kingdom = meta_object.getKingdom();
		part_background.sprite = kingdom.getElementBackground();
		part_icon.sprite = kingdom.getElementIcon();
		ColorAsset colorAsset = kingdom.getColor();
		Color colorMainSecond = colorAsset.getColorMainSecond();
		Color colorBanner = colorAsset.getColorBanner();
		colorMainSecond = Color.Lerp(colorMainSecond, Color.black, 0.05f);
		colorBanner = Color.Lerp(colorBanner, Color.black, 0.05f);
		((Graphic)part_background).color = colorMainSecond;
		((Graphic)part_icon).color = colorBanner;
		((Component)_species_icon).gameObject.SetActive(false);
	}

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.army = meta_object;
		return tooltipData;
	}
}
