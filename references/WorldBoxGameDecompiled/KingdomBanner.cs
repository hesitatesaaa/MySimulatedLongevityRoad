using UnityEngine;
using UnityEngine.UI;

public class KingdomBanner : BannerGeneric<Kingdom, KingdomData>
{
	public bool diplo_banner;

	[SerializeField]
	protected Image background;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected Image dead_image;

	[SerializeField]
	protected Image left_image;

	[SerializeField]
	protected Image winner_image;

	[SerializeField]
	protected Image loser_image;

	private Color _bgcolor;

	private Color _iconcolor;

	protected override MetaType meta_type => MetaType.Kingdom;

	protected override string tooltip_id => "kingdom";

	protected override void setupTooltip()
	{
		base.setupTooltip();
	}

	protected override void setupBanner()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		base.setupBanner();
		((Component)dead_image).gameObject.SetActive(false);
		((Component)left_image).gameObject.SetActive(false);
		((Component)winner_image).gameObject.SetActive(false);
		((Component)loser_image).gameObject.SetActive(false);
		part_background.sprite = meta_object.getElementBackground();
		part_icon.sprite = meta_object.getElementIcon();
		ColorAsset colorAsset = meta_object.getColor();
		((Graphic)part_background).color = colorAsset.getColorMainSecond();
		((Graphic)part_icon).color = colorAsset.getColorBanner();
	}

	public override void load(NanoObject pObject)
	{
		base.load(pObject);
		if (meta_object.hasDied())
		{
			showAsDead();
		}
	}

	private void showAsDead()
	{
		((Component)dead_image).gameObject.SetActive(true);
	}

	public void hasLeftWar()
	{
		((Component)left_image).gameObject.SetActive(true);
	}

	public void hasWon()
	{
		((Component)winner_image).gameObject.SetActive(true);
	}

	public void hasLost()
	{
		((Component)loser_image).gameObject.SetActive(true);
	}

	protected override void tooltipAction()
	{
		if (meta_object != null)
		{
			string pType = (meta_object.hasDied() ? "kingdom_dead" : "kingdom");
			string tip_name = string.Empty;
			if (diplo_banner)
			{
				pType = "kingdom_diplo";
				tip_name = "kingdom_diplo";
			}
			TooltipData tooltipData = getTooltipData();
			tooltipData.tip_name = tip_name;
			Tooltip.show(this, pType, tooltipData);
		}
	}

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.kingdom = meta_object;
		return tooltipData;
	}
}
