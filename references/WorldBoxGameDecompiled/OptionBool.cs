using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Obsolete]
public class OptionBool : MonoBehaviour
{
	public bool optionEnabled = true;

	public bool invokeCallbackOnStart = true;

	public SpriteRenderer spriteRenderer;

	public Image icon;

	private Button button;

	public Sprite spriteOn;

	public Sprite spriteOff;

	public UnityEvent callback;

	public UnityEvent<bool> boolCallback;

	public bool gameOption;

	public OptionType gameOptionType;

	public string gameOptionName = "-";

	private void Start()
	{
		updateSprite();
		if (invokeCallbackOnStart)
		{
			if (callback != null)
			{
				callback.Invoke();
			}
			if (boolCallback != null)
			{
				boolCallback.Invoke(optionEnabled);
			}
		}
	}

	public void checkGameOption(bool pSwitch = false)
	{
		if (pSwitch)
		{
			PlayerConfig.switchOption(gameOptionName, gameOptionType);
		}
		optionEnabled = PlayerConfig.optionEnabled(gameOptionName, gameOptionType);
		updateSprite();
		OptionAsset optionAsset = AssetManager.options_library.get(gameOptionName);
		optionAsset.action?.Invoke(optionAsset);
	}

	private void OnEnable()
	{
		if (!((Object)(object)World.world == (Object)null) && gameOption)
		{
			updateSprite();
			checkGameOption();
			updateSprite();
		}
	}

	public void clickButton()
	{
		if (gameOption)
		{
			checkGameOption(pSwitch: true);
			PlayerConfig.saveData();
			return;
		}
		optionEnabled = !optionEnabled;
		updateSprite();
		if (callback != null)
		{
			callback.Invoke();
		}
		if (boolCallback != null)
		{
			boolCallback.Invoke(optionEnabled);
		}
	}

	private void updateSprite()
	{
		if (optionEnabled)
		{
			icon.sprite = spriteOn;
		}
		else
		{
			icon.sprite = spriteOff;
		}
	}

	public void optionSpriteAnimation()
	{
		Config.sprite_animations_on = !Config.sprite_animations_on;
	}

	public void optionShowWORLD()
	{
		((Component)World.world).gameObject.SetActive(!((Component)World.world).gameObject.activeSelf);
	}

	public void optionRemovePremuium()
	{
		Config.hasPremium = false;
		PlayerConfig.instance.data.premium = false;
		PlayerConfig.saveData();
		PremiumElementsChecker.checkElements();
		if (Config.isMobile)
		{
			InAppManager.consumePremium();
		}
	}

	public void clearRewards()
	{
		PlayerConfig.instance.data.rewardedPowers.Clear();
		PlayerConfig.saveData();
		PremiumElementsChecker.checkElements();
	}

	public void optionShowCanvas()
	{
		((Behaviour)World.world.canvas).enabled = false;
	}

	public void optionRenderer()
	{
		((Renderer)spriteRenderer).enabled = optionEnabled;
		updateSprite();
	}
}
