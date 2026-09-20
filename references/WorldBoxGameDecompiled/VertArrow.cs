using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VertArrow : MonoBehaviour
{
	public Image arrow;

	private Transform _arrow_transform;

	public Vector3 hidPos;

	public bool isLeft = true;

	public ScrollRectExtended scrollRect;

	public RectTransform contentContainer;

	private float timer;

	private bool shouldShow = true;

	private Button button;

	private Tweener _tweener;

	private void Awake()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		_arrow_transform = ((Component)arrow).transform;
		((UnityEvent<Vector2>)scrollRect.onValueChanged).AddListener((UnityAction<Vector2>)onScroll);
		button = ((Component)arrow).GetComponent<Button>();
		((UnityEvent)button.onClick).AddListener(new UnityAction(scrollTab));
	}

	private void onScroll(Vector2 pVal)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		shouldShow = true;
		Rect rect = contentContainer.rect;
		float width = ((Rect)(ref rect)).width;
		rect = scrollRect.rectTransform.rect;
		if (width < ((Rect)(ref rect)).width)
		{
			shouldShow = false;
		}
		else if (isLeft)
		{
			if (scrollRect.horizontalNormalizedPosition > 0.1f)
			{
				shouldShow = true;
			}
			else
			{
				shouldShow = false;
			}
		}
		else if (scrollRect.horizontalNormalizedPosition == 1f)
		{
			shouldShow = false;
		}
		else if (scrollRect.horizontalNormalizedPosition < 0.98f)
		{
			shouldShow = true;
		}
		else
		{
			shouldShow = false;
		}
	}

	private void Update()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!shouldShow)
		{
			timer += Time.deltaTime * 2f;
		}
		else
		{
			timer -= Time.deltaTime * 2f;
		}
		timer = Mathf.Clamp(timer, 0f, 1f);
		float num = iTween.easeInOutCirc(0f, hidPos.x, timer);
		if (_arrow_transform.localPosition.x != num)
		{
			_arrow_transform.localPosition = new Vector3(num, 0f);
		}
	}

	private void scrollTab()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		float horizontalNormalizedPosition = scrollRect.horizontalNormalizedPosition;
		Rect rect = scrollRect.rectTransform.rect;
		float width = ((Rect)(ref rect)).width;
		rect = scrollRect.content.rect;
		float num = width / ((Rect)(ref rect)).width;
		horizontalNormalizedPosition = ((!isLeft) ? (horizontalNormalizedPosition + Mathf.Min(num, 0.5f)) : (horizontalNormalizedPosition - Mathf.Min(num, 0.5f)));
		TweenExtensions.Kill((Tween)(object)_tweener, false);
		_tweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => scrollRect.horizontalNormalizedPosition), (DOSetter<float>)delegate(float pPos)
		{
			scrollRect.horizontalNormalizedPosition = pPos;
		}, horizontalNormalizedPosition, 0.3f), (Ease)22);
	}

	private void OnDisable()
	{
		TweenExtensions.Kill((Tween)(object)_tweener, true);
	}
}
