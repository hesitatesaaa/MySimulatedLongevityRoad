using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopulationPyramidItem : MonoBehaviour
{
	[SerializeField]
	private RectTransform _mask;

	[SerializeField]
	private RectTransform _bar;

	[SerializeField]
	private Image _bar_image;

	[SerializeField]
	private Text _count_text;

	[SerializeField]
	private float _bar_width = 80f;

	[SerializeField]
	private int _count;

	[SerializeField]
	private int _max_count;

	[SerializeField]
	private float _percent;

	[SerializeField]
	private float _calc_percent;

	private Tweener _cur_tween;

	private void Awake()
	{
		resetBar();
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)((Component)this).gameObject.AddOrGetComponent<Button>().onClick).AddListener(new UnityAction(animateBar));
	}

	internal void setCount(int pCount, int pMax)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_count_text.text = pCount.ToString();
		Color color = ((Graphic)_count_text).color;
		if (pCount == 0)
		{
			color.a = 0.5f;
		}
		else
		{
			color.a = 1f;
		}
		((Graphic)_count_text).color = color;
		_count = pCount;
		_max_count = pMax;
		animateBar();
	}

	internal int getCount()
	{
		return _count;
	}

	private void resetBar()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		checkDestroyTween();
		_bar.sizeDelta = new Vector2(0.1f, _bar.sizeDelta.y);
	}

	internal void setOpacity(float pOpacity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)_bar_image).color;
		color.a = pOpacity;
		((Graphic)_bar_image).color = color;
	}

	internal void animateBar()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		resetBar();
		_percent = (float)_count / (float)_max_count;
		if (_count > 0)
		{
			_calc_percent = 4f + Mathf.Floor(_percent * _bar_width);
		}
		else
		{
			_calc_percent = 0f;
		}
		_cur_tween = (Tweener)(object)DOTweenModuleUI.DOSizeDelta(_bar, new Vector2(_calc_percent, _bar.sizeDelta.y), 0.3f, false);
	}

	private void OnDisable()
	{
		checkDestroyTween();
	}

	private void checkDestroyTween()
	{
		if (TweenExtensions.IsActive((Tween)(object)_cur_tween))
		{
			TweenExtensions.Complete((Tween)(object)_cur_tween, false);
			TweenExtensions.Kill((Tween)(object)_cur_tween, false);
		}
		_cur_tween = null;
	}
}
