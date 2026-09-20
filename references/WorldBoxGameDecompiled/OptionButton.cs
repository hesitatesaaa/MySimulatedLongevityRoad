using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionButton : MonoBehaviour
{
	private const float DISABLED_ALPHA = 0.8f;

	public static bool player_config_dirty;

	public Image icon;

	public Text counter;

	public Text text;

	public PowerButton button_switch;

	public SliderExtended slider;

	public GameObject sliderArea;

	public GameObject optionArea;

	public Action eventLeft;

	public Action eventRight;

	public Color color_enabled;

	public Color color_disabled;

	[SerializeField]
	private Sprite _button_sprite_interactable;

	[SerializeField]
	private Sprite _button_sprite_not_interactable;

	public SettingsWindow settings_window;

	public OptionAsset option_asset => AssetManager.options_library.get(((Object)((Component)this).transform).name);

	private void Start()
	{
		settings_window = ((Component)((Component)this).transform.parent).GetComponentInParent<SettingsWindow>();
		settings_window.buttons.Add(this);
	}

	private void OnDestroy()
	{
		if ((Object)(object)settings_window != (Object)null)
		{
			settings_window.buttons.Remove(this);
		}
	}

	private void OnEnable()
	{
		if (option_asset == null)
		{
			Debug.LogError((object)("Missing Option - " + ((Object)((Component)this).transform).name));
			return;
		}
		string localeID = option_asset.getLocaleID();
		((Component)text).GetComponent<LocalizedText>().setKeyAndUpdate(localeID);
		TipButton tipButton = ((option_asset.type != OptionType.Bool) ? ((Component)slider).GetComponent<TipButton>() : ((Component)button_switch).GetComponent<TipButton>());
		tipButton.setHoverAction(delegate
		{
			if (InputHelpers.mouseSupported)
			{
				showTooltip();
			}
		});
		if (!string.IsNullOrEmpty(option_asset.getDescriptionID2()))
		{
			((Component)slider).GetComponent<TipButton>().text_description_2 = option_asset.getDescriptionID2();
			((Component)button_switch).GetComponent<TipButton>().text_description_2 = option_asset.getDescriptionID2();
		}
		if (option_asset.type == OptionType.Bool)
		{
			((Component)counter).gameObject.SetActive(false);
			((Component)button_switch).gameObject.SetActive(true);
			sliderArea.SetActive(false);
		}
		else if (option_asset.type == OptionType.Int)
		{
			((Component)counter).gameObject.SetActive(true);
			((Component)button_switch).gameObject.SetActive(false);
			sliderArea.SetActive(true);
			updateSlider();
			((UnityEvent<float>)(object)((Slider)slider).onValueChanged).AddListener((UnityAction<float>)sliderChanged);
			slider.addCallbackPointerDown(checkShowSliderTooltip);
		}
		updateElements();
	}

	public void showTooltip()
	{
		Tooltip.show(this, "tip", getTooltipData());
	}

	public void checkShowSliderTooltip()
	{
		if (!InputHelpers.mouseSupported)
		{
			if (Input.touchCount >= 2)
			{
				Tooltip.hideTooltip();
			}
			else
			{
				showTooltip();
			}
		}
	}

	private TooltipData getTooltipData()
	{
		TooltipData tooltipData = new TooltipData
		{
			tip_name = option_asset.getLocaleID(),
			tip_description = option_asset.getDescriptionID()
		};
		string descriptionID = option_asset.getDescriptionID2();
		if (!string.IsNullOrEmpty(descriptionID))
		{
			tooltipData.tip_description_2 = descriptionID;
		}
		return tooltipData;
	}

	public void switchBoolOption()
	{
		bool flag = PlayerConfig.optionBoolEnabled(option_asset.id);
		PlayerConfig.setOptionBool(option_asset.id, !flag);
		player_config_dirty = true;
	}

	public void clickSwitch()
	{
		if (option_asset == null)
		{
			Debug.LogError((object)("Missing Option - " + ((Object)((Component)this).transform).name));
			return;
		}
		if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this))
		{
			showTooltip();
			return;
		}
		switchBoolOption();
		option_asset.action?.Invoke(option_asset);
		if (option_asset.update_all_elements_after_click)
		{
			settings_window.updateAllElements();
		}
		else
		{
			updateElements();
		}
	}

	private int clampAssetValue(int pValue)
	{
		return Mathf.Clamp(pValue, option_asset.min_value, option_asset.max_value);
	}

	private void sliderChanged(float pValue)
	{
		int pValue2 = (int)pValue;
		pValue2 = clampAssetValue(pValue2);
		PlayerConfig.setOptionInt(option_asset.id, pValue2);
		player_config_dirty = true;
		option_asset.action?.Invoke(option_asset);
		updateElements();
		checkShowSliderTooltip();
	}

	public void updateElements(bool pCallCallbacks = false)
	{
		updateCounter();
		updateSwitchButton();
		updateColors();
		updateSlider(pCallCallbacks);
	}

	private void updateSlider(bool pCallCallbacks = false)
	{
		if (option_asset.type == OptionType.Int)
		{
			((Slider)slider).minValue = option_asset.min_value;
			((Slider)slider).maxValue = option_asset.max_value;
			if (!pCallCallbacks)
			{
				((Slider)slider).SetValueWithoutNotify((float)PlayerConfig.getIntValue(option_asset.id));
			}
			else
			{
				((Slider)slider).value = PlayerConfig.getIntValue(option_asset.id);
			}
			((Slider)slider).wholeNumbers = false;
		}
	}

	private void updateColors()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (option_asset.type == OptionType.Bool)
		{
			if (!option_asset.interactable)
			{
				((Graphic)text).color = color_disabled;
				((Graphic)icon).color = color_disabled;
			}
			else if (PlayerConfig.optionBoolEnabled(option_asset.id))
			{
				((Graphic)text).color = color_enabled;
				((Graphic)icon).color = color_enabled;
			}
			else
			{
				((Graphic)text).color = color_disabled;
				((Graphic)icon).color = color_disabled;
			}
		}
	}

	private void updateSwitchButton()
	{
		if (option_asset.type != OptionType.Bool)
		{
			return;
		}
		Image component = ((Component)button_switch).GetComponent<Image>();
		CanvasGroup component2 = ((Component)button_switch).GetComponent<CanvasGroup>();
		if (!option_asset.interactable)
		{
			component2.alpha = 0.8f;
			component2.interactable = false;
			component.sprite = _button_sprite_not_interactable;
			return;
		}
		component2.interactable = true;
		component.sprite = _button_sprite_interactable;
		if (PlayerConfig.optionBoolEnabled(option_asset.id))
		{
			component2.alpha = 1f;
			((Component)((Component)button_switch).transform.Find("Text")).GetComponent<LocalizedText>().setKeyAndUpdate("short_on");
			button_switch.icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconOn");
		}
		else
		{
			component2.alpha = 0.8f;
			((Component)((Component)button_switch).transform.Find("Text")).GetComponent<LocalizedText>().setKeyAndUpdate("short_off");
			button_switch.icon.sprite = SpriteTextureLoader.getSprite("ui/icons/IconOff");
		}
	}

	public void updateCounter()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (option_asset.type == OptionType.Int)
		{
			int intValue = PlayerConfig.getIntValue(option_asset.id);
			string text = intValue.ToString();
			if (option_asset.counter_format != null)
			{
				text = option_asset.counter_format(option_asset);
			}
			else if (option_asset.counter_percent)
			{
				text += "%";
			}
			counter.text = text;
			if (intValue == 0)
			{
				((Graphic)this.text).color = color_disabled;
				((Graphic)counter).color = color_disabled;
				((Graphic)icon).color = color_disabled;
			}
			else
			{
				((Graphic)this.text).color = color_enabled;
				((Graphic)counter).color = color_enabled;
				((Graphic)icon).color = color_enabled;
			}
		}
	}

	private void sliderDragEnded()
	{
		if (!InputHelpers.mouseSupported)
		{
			Tooltip.hideTooltip();
		}
	}
}
