using System.Collections.Generic;
using UnityEngine;

public class PremiumWindow : MonoBehaviour
{
	public Transform buttons_transform;

	public void Awake()
	{
		clearButtons();
		addButtons();
	}

	private void addButtons()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, PowerButton> dictionary = new Dictionary<string, PowerButton>();
		foreach (PowerButton premium_button in GodPower.premium_buttons)
		{
			dictionary.TryAdd(premium_button.godPower.id, premium_button);
		}
		foreach (PowerButton value in dictionary.Values)
		{
			((Component)value).gameObject.SetActive(false);
			PowerButton powerButton = Object.Instantiate<PowerButton>(value, buttons_transform);
			((Object)((Component)powerButton).transform).name = ((Object)((Component)value).transform).name;
			powerButton.type = PowerButtonType.Shop;
			powerButton.destroyLockIcon();
			((Component)powerButton).GetComponent<RectTransform>().pivot = ((Component)value).GetComponent<RectTransform>().pivot;
			IconRotationAnimation iconRotationAnimation = ((Component)powerButton).gameObject.AddComponent<IconRotationAnimation>();
			iconRotationAnimation.delay = Randy.randomFloat(1f, 10f);
			iconRotationAnimation.randomDelay = true;
			((Component)value).gameObject.SetActive(true);
			((Component)powerButton).gameObject.SetActive(true);
		}
	}

	private void clearButtons()
	{
		while (buttons_transform.childCount > 0)
		{
			GameObject gameObject = ((Component)buttons_transform.GetChild(0)).gameObject;
			gameObject.transform.SetParent((Transform)null);
			Object.Destroy((Object)(object)gameObject);
		}
	}
}
