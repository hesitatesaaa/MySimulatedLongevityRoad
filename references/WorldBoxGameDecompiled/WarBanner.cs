using UnityEngine;
using UnityEngine.UI;

public class WarBanner : BannerGeneric<War, WarData>
{
	public KingdomBanner banner_kingdom1;

	public KingdomBanner banner_kingdom2;

	public Image war_icon;

	public bool diplo_banner;

	public Image total_war_icon;

	private bool diplo_banner_initiated;

	public bool buttons_enabled;

	protected override MetaType meta_type => MetaType.War;

	protected override string tooltip_id => "war";

	protected override void setupBanner()
	{
		base.setupBanner();
		((Component)banner_kingdom1).gameObject.SetActive(false);
		((Component)banner_kingdom2).gameObject.SetActive(false);
		((Component)total_war_icon).gameObject.SetActive(false);
		Kingdom mainAttacker = meta_object.getMainAttacker();
		if (!mainAttacker.isRekt())
		{
			((Component)banner_kingdom1).gameObject.SetActive(true);
			banner_kingdom1.load(mainAttacker);
		}
		if (meta_object.isTotalWar())
		{
			((Component)total_war_icon).gameObject.SetActive(true);
		}
		else
		{
			Kingdom mainDefender = meta_object.getMainDefender();
			if (!mainDefender.isRekt())
			{
				((Component)banner_kingdom2).gameObject.SetActive(true);
				banner_kingdom2.load(mainDefender);
			}
		}
		switch (meta_object.data.winner)
		{
		case WarWinner.Attackers:
			banner_kingdom1.hasWon();
			if (!meta_object.isTotalWar())
			{
				banner_kingdom2.hasLost();
			}
			break;
		case WarWinner.Defenders:
			banner_kingdom1.hasLost();
			if (!meta_object.isTotalWar())
			{
				banner_kingdom2.hasWon();
			}
			break;
		}
		war_icon.sprite = SpriteTextureLoader.getSprite(meta_object.getAsset().path_icon);
		if (buttons_enabled)
		{
			initDiploBanner();
		}
	}

	private void initDiploBanner()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!diplo_banner_initiated)
		{
			diplo_banner_initiated = true;
			diplo_banner = true;
			((Behaviour)((Component)this).GetComponent<Button>()).enabled = true;
			((Behaviour)((Component)this).GetComponent<TipButton>()).enabled = true;
			UiButtonHoverAnimation component = ((Component)this).GetComponent<UiButtonHoverAnimation>();
			((Behaviour)component).enabled = true;
			component.scale_size = 1.1f;
			component.default_scale = new Vector3(0.8f, 0.8f, 0.8f);
			setupTooltip();
		}
	}

	protected override TooltipData getTooltipData()
	{
		TooltipData tooltipData = base.getTooltipData();
		tooltipData.war = meta_object;
		return tooltipData;
	}
}
