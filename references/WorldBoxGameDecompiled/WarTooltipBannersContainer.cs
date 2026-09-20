using UnityEngine;
using UnityEngine.UI;

public class WarTooltipBannersContainer : MonoBehaviour
{
	[SerializeField]
	private KingdomBanner _banner_left;

	[SerializeField]
	private KingdomBanner _banner_right;

	[SerializeField]
	private Image _total_war;

	public void load(War pWar)
	{
		((Component)_banner_right).gameObject.SetActive(false);
		((Component)_banner_left).gameObject.SetActive(false);
		((Component)_total_war).gameObject.SetActive(false);
		Kingdom main_attacker = pWar.main_attacker;
		if (!main_attacker.isRekt())
		{
			((Component)_banner_left).gameObject.SetActive(true);
			_banner_left.load(main_attacker);
		}
		if (pWar.isTotalWar())
		{
			((Component)_total_war).gameObject.SetActive(true);
			return;
		}
		Kingdom mainDefender = pWar.getMainDefender();
		if (!mainDefender.isRekt())
		{
			((Component)_banner_right).gameObject.SetActive(true);
			_banner_right.load(mainDefender);
		}
	}
}
