using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TipButton : MonoBehaviour
{
	private Vector3 _default_scale;

	private float _default_click_scale_increase = 1.1f;

	public const float SCALE_DURATION = 0.1f;

	public string textOnClick;

	public string textOnClickDescription;

	public string text_description_2;

	public string text_override_non_steam = string.Empty;

	public string description_override_non_steam = string.Empty;

	public TooltipAction hoverAction;

	public TooltipAction clickAction;

	public bool return_if_same_object;

	public string type = "tip";

	public bool showOnClick = true;

	public bool override_click_scale_animation;

	public float overridden_click_scale_animation = 1f;

	private Tweener _scale_anim;

	private void Awake()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (hoverAction == null)
		{
			setHoverAction(showTooltipDefault);
		}
		_default_scale = ((Component)this).gameObject.transform.localScale;
	}

	private void Start()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			if (showOnClick)
			{
				((UnityEvent)val.onClick).AddListener(new UnityAction(showTooltipOnClick));
			}
			val.OnHover(new UnityAction(showHoverTooltip));
			val.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
		}
		else
		{
			Slider component = ((Component)this).GetComponent<Slider>();
			if ((Object)(object)component != (Object)null)
			{
				component.OnHover(new UnityAction(showHoverTooltip));
				component.OnHoverOut(new UnityAction(Tooltip.hideTooltip));
			}
		}
	}

	public void setDefaultScale(Vector3 pScale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_default_scale = pScale;
	}

	public void setHoverAction(TooltipAction pAction, bool pAddAnimation = true)
	{
		hoverAction = pAction;
		if (pAddAnimation)
		{
			hoverAction = (TooltipAction)Delegate.Combine(hoverAction, new TooltipAction(clickAnimation));
		}
	}

	private void showTooltipOnClick()
	{
		if (clickAction != null)
		{
			clickAction();
		}
		else
		{
			hoverAction?.Invoke();
		}
	}

	private void showHoverTooltip()
	{
		if (Config.tooltips_active)
		{
			hoverAction?.Invoke();
		}
	}

	public void showTooltipDefault()
	{
		if (Config.isMobile)
		{
			if (!string.IsNullOrEmpty(text_override_non_steam))
			{
				textOnClick = text_override_non_steam;
			}
			if (!string.IsNullOrEmpty(description_override_non_steam))
			{
				textOnClickDescription = description_override_non_steam;
			}
		}
		if (!(textOnClick == "") || !(textOnClickDescription == ""))
		{
			TooltipData pData = new TooltipData
			{
				tip_name = textOnClick,
				tip_description = textOnClickDescription,
				tip_description_2 = text_description_2
			};
			Tooltip.show(((Component)this).gameObject, type, pData);
		}
	}

	public void clickAnimation()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float default_click_scale_increase = _default_click_scale_increase;
		if (override_click_scale_animation)
		{
			default_click_scale_increase = overridden_click_scale_animation;
		}
		float num = _default_scale.x * default_click_scale_increase;
		((Component)this).transform.localScale = new Vector3(num, num, num);
		TweenExtensions.Kill((Tween)(object)_scale_anim, false);
		_scale_anim = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, _default_scale, 0.1f), (Ease)26);
	}

	private void OnEnable()
	{
		resetAnimation();
	}

	private void resetAnimation()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Tweener scale_anim = _scale_anim;
		if (scale_anim != null)
		{
			TweenExtensions.Kill((Tween)(object)scale_anim, false);
		}
		((Component)this).transform.localScale = _default_scale;
	}

	private void OnDestroy()
	{
		TweenExtensions.Kill((Tween)(object)_scale_anim, false);
	}
}
