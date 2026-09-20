using System;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationButton : MonoBehaviour
{
	public Sprite button_current;

	public Sprite button_normal;

	public Sprite button_highlight;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _bg_image;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Text _text_field;

	private TipButton _tip_button;

	private LocalizedText _localized_text;

	[SerializeField]
	private Text _percent;

	private GameLanguageAsset _asset;

	private bool _initialized;

	public GameLanguageAsset getAsset()
	{
		return _asset;
	}

	private void init()
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_localized_text = ((Component)_text_field).GetComponent<LocalizedText>();
		_tip_button = ((Component)_button).GetComponent<TipButton>();
		_tip_button.hoverAction = delegate
		{
			if (InputHelpers.mouseSupported)
			{
				showTooltip();
			}
		};
		TipButton tip_button = _tip_button;
		tip_button.clickAction = (TooltipAction)Delegate.Combine(tip_button.clickAction, (TooltipAction)delegate
		{
			if (InputHelpers.mouseSupported)
			{
				changeLanguage();
			}
			else if (Tooltip.isShowingFor(this))
			{
				changeLanguage();
			}
			else
			{
				showTooltip();
			}
		});
		((Object)((Component)this).gameObject).name = _asset.id;
		if (_asset.path_icon != null)
		{
			_icon.sprite = SpriteTextureLoader.getSprite(_asset.path_icon);
			((Component)_icon).gameObject.SetActive(true);
			RectTransform component = ((Component)_text_field).GetComponent<RectTransform>();
			component.offsetMin = new Vector2(18.5f, component.offsetMin.y);
			component.offsetMax = new Vector2(-4f, component.offsetMax.y);
		}
		else
		{
			((Component)_icon).gameObject.SetActive(false);
			RectTransform component2 = ((Component)_text_field).GetComponent<RectTransform>();
			component2.offsetMin = new Vector2(4f, component2.offsetMin.y);
			component2.offsetMax = new Vector2(-4f, component2.offsetMax.y);
		}
		_text_field.text = _asset.name;
		_localized_text.checkSpecialLanguages(_asset);
	}

	private void showTooltip()
	{
		TooltipData pData = new TooltipData
		{
			game_language_asset = _asset
		};
		Tooltip.show(this, "game_language", pData);
	}

	private void changeLanguage()
	{
		LocalizedTextManager.instance.setLanguage(_asset.id);
		WorldLanguagesWindow.updateButtons();
	}

	internal void checkSprite()
	{
		if (LocalizedTextManager.current_language == _asset)
		{
			_bg_image.sprite = button_current;
		}
		else if (LocalizedTextManager.getCulture(((Object)((Component)((Component)this).transform).gameObject).name) == LocalizedTextManager.getCurrentCulture())
		{
			_bg_image.sprite = button_highlight;
		}
		else
		{
			_bg_image.sprite = button_normal;
		}
	}

	public void SetAsset(GameLanguageAsset pAsset, int pDone)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		_asset = pAsset;
		if (pDone > 0)
		{
			if (pDone < 40)
			{
				((Graphic)_percent).color = Toolbox.color_negative_RGBA;
			}
			else if (pDone < 60)
			{
				((Graphic)_percent).color = Toolbox.color_log_warning;
			}
			else if (pDone < 80)
			{
				((Graphic)_percent).color = Toolbox.color_text_default;
			}
			else
			{
				((Graphic)_percent).color = Toolbox.color_positive_RGBA;
			}
			_percent.text = pDone + "%";
			((Component)_percent).gameObject.SetActive(true);
		}
		else
		{
			((Component)_percent).gameObject.SetActive(false);
		}
		init();
		checkSprite();
	}
}
