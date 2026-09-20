using UnityEngine;
using UnityEngine.UI;

public class RewardPowerWindow : MonoBehaviour
{
	[SerializeField]
	private LocalizedText _description;

	[SerializeField]
	private Image[] _icons;

	[SerializeField]
	private bool _auto_gift_key;

	private void OnEnable()
	{
		if ((Object)(object)_description != (Object)null && _auto_gift_key)
		{
			string keyAndUpdate = ((!(Config.power_to_unlock?.id == "clock")) ? "unlock_powers_description_any" : "unlock_powers_description_clock_hours");
			_description.setKeyAndUpdate(keyAndUpdate);
		}
		InitAds.initAdProviders();
		updateButtonIcons();
	}

	private void updateButtonIcons()
	{
		if (Config.power_to_unlock == null || _icons.Length == 0)
		{
			return;
		}
		PowerButton powerButton = PowerButton.get(Config.power_to_unlock.id);
		if ((Object)(object)powerButton != (Object)null)
		{
			Sprite sprite = powerButton.icon.sprite;
			Image[] icons = _icons;
			for (int i = 0; i < icons.Length; i++)
			{
				icons[i].sprite = sprite;
			}
		}
	}

	public void showRewardedAd()
	{
		if (Config.power_to_unlock?.id == "clock")
		{
			PlayerConfig.instance.data.powerReward = "clock";
			if (Config.isMobile || Config.isEditor)
			{
				RewardedAds.instance.ShowRewardedAd("clock");
			}
		}
		else
		{
			PlayerConfig.instance.data.powerReward = Config.power_to_unlock.id;
			if (Config.isMobile || Config.isEditor)
			{
				RewardedAds.instance.ShowRewardedAd("power");
			}
		}
	}
}
