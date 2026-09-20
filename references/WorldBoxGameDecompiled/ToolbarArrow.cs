using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToolbarArrow : MonoBehaviour
{
	protected const float POSITION_SCROLL_BOUND_BEGIN = 0.1f;

	protected const float POSITION_SCROLL_BOUND_END = 0.98f;

	protected const float POSITION_SCROLL_END = 1f;

	protected const float POSITION_SCROLL_MIN = 0.5f;

	private const float POSITION_UPDATE_SPEED = 2f;

	private const float TWEEN_DURATION = 0.3f;

	[SerializeField]
	private Image arrow;

	[SerializeField]
	protected Vector3 hide_position;

	[SerializeField]
	protected ScrollRectExtended scroll_rect;

	protected float timer;

	protected bool should_show = true;

	protected Transform arrow_transform;

	private Button _button;

	private Tweener _tweener;

	private void Awake()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		arrow_transform = ((Component)arrow).transform;
		((UnityEvent<Vector2>)scroll_rect.onValueChanged).AddListener((UnityAction<Vector2>)onScroll);
		_button = ((Component)arrow).GetComponent<Button>();
		((UnityEvent)_button.onClick).AddListener(new UnityAction(scrollTab));
	}

	protected virtual void onScroll(Vector2 pVal)
	{
		throw new NotImplementedException();
	}

	protected virtual float getEndPosition()
	{
		throw new NotImplementedException();
	}

	protected virtual float getScrollPosition()
	{
		throw new NotImplementedException();
	}

	protected virtual void setScrollPosition(float pValue)
	{
		throw new NotImplementedException();
	}

	protected virtual void Update()
	{
		if (!should_show)
		{
			timer += Time.deltaTime * 2f;
		}
		else
		{
			timer -= Time.deltaTime * 2f;
		}
		timer = Mathf.Clamp(timer, 0f, 1f);
	}

	private void scrollTab()
	{
		float endPosition = getEndPosition();
		TweenExtensions.Kill((Tween)(object)_tweener, false);
		_tweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => getScrollPosition()), (DOSetter<float>)delegate(float pPos)
		{
			setScrollPosition(pPos);
		}, endPosition, 0.3f), (Ease)22);
	}

	private void OnDisable()
	{
		TweenExtensions.Kill((Tween)(object)_tweener, true);
	}
}
