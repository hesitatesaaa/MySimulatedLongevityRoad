using UnityEngine;
using UnityEngine.UI;

public class AdLoadingButton : MonoBehaviour
{
	public Text button_text;

	public LocalizedText button_localized_text;

	public Button button;

	private Image button_image;

	public Sprite spriteOn;

	public Sprite spriteOff;

	private AdLoadingButtonState state;

	private void Awake()
	{
		button_image = ((Component)button).GetComponent<Image>();
		button_localized_text = ((Component)button_text).GetComponent<LocalizedText>();
		state = AdLoadingButtonState.None;
	}

	private void Update()
	{
		AdLoadingButtonState adLoadingButtonState = AdLoadingButtonState.None;
		if (Config.isEditor && Config.editor_test_rewards_from_ads)
		{
			adLoadingButtonState = AdLoadingButtonState.AdReady;
			state = adLoadingButtonState;
			toggleState();
			return;
		}
		if (RewardedAds.isReady())
		{
			adLoadingButtonState = AdLoadingButtonState.AdReady;
		}
		else if (!Config.adsInitialized)
		{
			adLoadingButtonState = AdLoadingButtonState.Initializing;
		}
		else
		{
			adLoadingButtonState = AdLoadingButtonState.AdLoading;
			RewardedAds.trimTimeout();
		}
		if (adLoadingButtonState != state)
		{
			state = adLoadingButtonState;
			toggleState();
		}
	}

	private void toggleState()
	{
		switch (state)
		{
		case AdLoadingButtonState.Initializing:
			((Selectable)button).interactable = false;
			button_localized_text.setKeyAndUpdate("waiting_for_ad");
			button_image.sprite = spriteOff;
			break;
		case AdLoadingButtonState.AdLoading:
			((Selectable)button).interactable = false;
			button_localized_text.setKeyAndUpdate("loading_ads");
			button_image.sprite = spriteOff;
			break;
		case AdLoadingButtonState.AdReady:
			((Selectable)button).interactable = true;
			button_localized_text.setKeyAndUpdate("watch_ad");
			button_image.sprite = spriteOn;
			break;
		}
	}
}
