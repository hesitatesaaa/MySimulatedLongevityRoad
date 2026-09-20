using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
	public Text textField;

	public RectTransform mask;

	public RectTransform bar;

	private Tweener _bar_tween;

	private Tweener _text_tween;

	private float _val;

	private float _max = 100f;

	private string _ending;

	private bool _float;

	public StatBarUpdated _bar_updated_action;

	private void OnEnable()
	{
		restartBar();
	}

	private void Start()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		restartBar();
		((UnityEvent)((Component)this).gameObject.AddOrGetComponent<Button>().onClick).AddListener(new UnityAction(restartBar));
	}

	private void restartBar()
	{
		setBar(_val, _max, _ending, pReset: true, _float);
	}

	private void resetSize()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = mask.rect;
		float num = Mathf.Floor(0.01f * ((Rect)(ref rect)).width);
		bar.sizeDelta = new Vector2(num, bar.sizeDelta.y);
		textField.text = "0";
	}

	public void resetTween()
	{
		checkDestroyTween(pComplete: false);
		_val = 0f;
		resetSize();
	}

	private void OnRectTransformDimensionsChange()
	{
		restartBar();
	}

	public void setBar(float pVal, float pMax, string pEnding, bool pReset = true, bool pFloat = false, bool pUpdateText = true, float pSpeed = 0.3f)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		if (pMax == 0f)
		{
			resetTween();
			textField.text = "-";
		}
		else
		{
			if (!pReset && pVal == _val && pMax == _max && pEnding == _ending)
			{
				return;
			}
			if (pReset)
			{
				resetTween();
			}
			_max = pMax;
			_ending = pEnding;
			_float = pFloat;
			checkDestroyTween(pComplete: false);
			if (pReset)
			{
				_bar_tween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(bar, new Vector2(0f, bar.sizeDelta.y), 0.005f, false), (TweenCallback)delegate
				{
					//IL_0018: Unknown result type (might be due to invalid IL or missing references)
					//IL_001d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0049: Unknown result type (might be due to invalid IL or missing references)
					//IL_0053: Unknown result type (might be due to invalid IL or missing references)
					float num3 = pVal / pMax;
					Rect rect2 = mask.rect;
					float num4 = Mathf.Floor(num3 * ((Rect)(ref rect2)).width);
					_bar_tween = (Tweener)(object)DOTweenModuleUI.DOSizeDelta(bar, new Vector2(num4, bar.sizeDelta.y), pSpeed, false);
				});
			}
			else
			{
				float num = pVal / pMax;
				Rect rect = mask.rect;
				float num2 = Mathf.Floor(num * ((Rect)(ref rect)).width);
				_bar_tween = (Tweener)(object)DOTweenModuleUI.DOSizeDelta(bar, new Vector2(num2, bar.sizeDelta.y), pSpeed, false);
			}
			if (pUpdateText)
			{
				if (pFloat)
				{
					_text_tween = (Tweener)(object)textField.DOUpCounter(_val, pVal, pSpeed, pEnding);
				}
				else
				{
					_text_tween = (Tweener)(object)textField.DOUpCounter((int)_val, (int)pVal, pSpeed, pEnding);
				}
			}
			_val = pVal;
			_bar_updated_action?.Invoke(pVal, pMax);
		}
	}

	private void OnDisable()
	{
		checkDestroyTween();
	}

	private void checkDestroyTween(bool pComplete = true)
	{
		if (TweenExtensions.IsActive((Tween)(object)_bar_tween))
		{
			TweenExtensions.Complete((Tween)(object)_bar_tween, pComplete);
			TweenExtensions.Kill((Tween)(object)_bar_tween, pComplete);
			_bar_tween = null;
		}
		if (TweenExtensions.IsActive((Tween)(object)_text_tween))
		{
			TweenExtensions.Complete((Tween)(object)_text_tween, pComplete);
			TweenExtensions.Kill((Tween)(object)_text_tween, pComplete);
			_text_tween = null;
		}
	}

	public void addCallback(StatBarUpdated pAction)
	{
		_bar_updated_action = (StatBarUpdated)Delegate.Combine(_bar_updated_action, pAction);
	}

	public void removeCallback(StatBarUpdated pAction)
	{
		_bar_updated_action = (StatBarUpdated)Delegate.Remove(_bar_updated_action, pAction);
	}
}
