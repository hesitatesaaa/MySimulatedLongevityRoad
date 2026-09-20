using UnityEngine;
using UnityEngine.UI;

public class SpectateUnit : MonoBehaviour
{
	private Actor _actor;

	public Text unitName;

	public UnitAvatarLoader avatarLoader;

	public KingdomBanner kingdomBannerR;

	public KingdomBanner kingdomBannerL;

	public Text text_age;

	public Text text_kills;

	public StatBar health_bar;

	public void updateStats()
	{
		unitName.text = _actor.coloredName;
		text_age.text = Toolbox.formatNumber(_actor.getAge());
		text_kills.text = Toolbox.formatNumber(_actor.data.kills);
		health_bar.setBar(_actor.getHealth(), _actor.getMaxHealth(), "/" + _actor.getMaxHealth().ToText(4), pReset: false);
		if (_actor.hasKingdom() && _actor.isKingdomCiv())
		{
			((Component)kingdomBannerR).gameObject.SetActive(true);
			((Component)kingdomBannerL).gameObject.SetActive(true);
			kingdomBannerR.load(_actor.kingdom);
			kingdomBannerL.load(_actor.kingdom);
		}
		else
		{
			((Component)kingdomBannerR).gameObject.SetActive(false);
			((Component)kingdomBannerL).gameObject.SetActive(false);
		}
	}

	public void clickKingdomElement()
	{
		if (Input.touchCount <= 1)
		{
			SelectedMetas.selected_kingdom = _actor.kingdom;
			ScrollWindow.showWindow("kingdom");
		}
	}

	public void clickLocate()
	{
		if (Input.touchCount <= 1 && !ScrollWindow.isAnimationActive())
		{
			WorldLog.locationFollow(_actor);
		}
	}

	public void clickInspect()
	{
		if (Input.touchCount <= 1 && !ScrollWindow.isAnimationActive() && _actor != null && _actor.isAlive())
		{
			ScrollWindow.moveAllToLeftAndRemove();
			ActionLibrary.openUnitWindow(_actor);
		}
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
		if (MoveCamera.inSpectatorMode())
		{
			if (!MoveCamera.isCameraFollowingUnit(_actor))
			{
				_actor = MoveCamera.getFocusUnit();
			}
			if (_actor != null)
			{
				updateStats();
			}
		}
	}
}
