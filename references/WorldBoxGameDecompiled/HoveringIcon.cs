using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class HoveringIcon : MonoBehaviour
{
	private Vector3 _original_pos;

	private float _random_timer;

	public Image image;

	public float min = -2f;

	public float max = 2f;

	public float timer_mod = 1f;

	private Tweener _tweener;

	internal RectTransform rect;

	private void Awake()
	{
		rect = ((Component)this).GetComponent<RectTransform>();
		image = ((Component)this).GetComponent<Image>();
	}

	internal void clear()
	{
		TweenExtensions.Kill((Tween)(object)_tweener, false);
	}

	internal void init()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_original_pos = ((Component)this).transform.localPosition;
		_random_timer = Randy.randomFloat(1f * timer_mod, 1.5f * timer_mod);
		startAnimation();
	}

	private void OnDisable()
	{
		clear();
	}

	private void startAnimation()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		TweenExtensions.Kill((Tween)(object)_tweener, false);
		((Component)this).transform.localPosition = new Vector3(_original_pos.x, _original_pos.y += Randy.randomFloat(min, max));
		if (Randy.randomBool())
		{
			moveStageOne();
		}
		else
		{
			moveStageTwo();
		}
	}

	private void moveStageTwo()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		_tweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, _original_pos, _random_timer, false), (Ease)7), new TweenCallback(moveStageOne));
	}

	private void moveStageOne()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(_original_pos.x, _original_pos.y, 1f);
		val.y += 3f;
		_tweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, val, _random_timer, false), (Ease)7), new TweenCallback(moveStageTwo));
	}
}
