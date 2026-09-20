using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class IconRotationAnimation : MonoBehaviour
{
	public float delay = 5f;

	public bool randomDelay;

	private Vector3 initScale;

	private Vector3 scaleTo;

	internal Tweener curTween;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		initScale = ((Component)this).transform.localScale;
		scaleTo = initScale * 1.1f;
		if (randomDelay)
		{
			delay = Randy.randomFloat(1f, 10f);
		}
	}

	private void checkDestroyTween()
	{
		if (curTween != null && ((Tween)curTween).active)
		{
			TweenExtensions.Complete((Tween)(object)curTween, false);
			TweenExtensions.Kill((Tween)(object)curTween, false);
			curTween = null;
		}
	}

	private void rotate1()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		if (!((Object)(object)((Component)this).transform == (Object)null))
		{
			curTween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, scaleTo, 0.3f), delay), (Ease)28), new TweenCallback(rotate2));
		}
	}

	private void rotate2()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (!((Object)(object)((Component)this).transform == (Object)null))
		{
			if (randomDelay)
			{
				delay = Randy.randomFloat(1f, 10f);
			}
			curTween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, initScale, 0.3f), 0f), (Ease)28), new TweenCallback(rotate1));
		}
	}

	private void OnEnable()
	{
		checkDestroyTween();
		rotate1();
	}

	private void OnDisable()
	{
		checkDestroyTween();
	}

	private void OnDestroy()
	{
		checkDestroyTween();
	}
}
