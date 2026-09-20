using UnityEngine;
using UnityEngine.UI;

public class CityBanner : BannerGeneric<City, CityData>
{
	[SerializeField]
	private Sprite _city_sprite;

	[SerializeField]
	private Sprite _capital_sprite;

	private Image _part_city_icon;

	protected override MetaType meta_type => MetaType.City;

	protected override string tooltip_id => "city";

	protected override void setupBanner()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		ColorAsset colorAsset = meta_object.kingdom.getColor();
		part_background.sprite = meta_object.kingdom.getElementBackground();
		part_icon.sprite = meta_object.kingdom.getElementIcon();
		Sprite pSprite = (meta_object.isCapitalCity() ? _capital_sprite : _city_sprite);
		_part_city_icon.sprite = DynamicSprites.getIconWithColors(pSprite, null, colorAsset);
		Color colorMainSecond = colorAsset.getColorMainSecond();
		Color colorBanner = colorAsset.getColorBanner();
		colorMainSecond = Color.Lerp(colorMainSecond, Color.black, 0.05f);
		colorBanner = Color.Lerp(colorBanner, Color.black, 0.05f);
		((Graphic)part_background).color = colorMainSecond;
		((Graphic)part_icon).color = colorBanner;
	}

	protected override void setupParts()
	{
		base.setupParts();
		_part_city_icon = ((Component)((Component)this).transform.FindRecursive("Foundation")).GetComponent<Image>();
	}

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.city = meta_object;
		return tooltipData;
	}
}
