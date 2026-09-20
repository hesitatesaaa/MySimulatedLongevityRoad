using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class CornerAye : MonoBehaviour
{
	public static CornerAye instance;

	public Transform sprite;

	private RectTransform _rect;

	private void Awake()
	{
		_rect = ((Component)sprite).GetComponent<RectTransform>();
		reset();
	}

	private void reset()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_rect.anchoredPosition = new Vector2(100f, 0f);
		ShortcutExtensions.DOKill((Component)(object)((Component)sprite).transform, false);
	}

	private void Start()
	{
		instance = this;
	}

	public void startAye()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		reset();
		float num = 0.3f;
		((Tween)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)sprite).transform, default(Vector3), num, false), (Ease)27)).onComplete = new TweenCallback(moveBack);
	}

	private void moveBack()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(100f, 0f);
		float num = 0.3f;
		TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)sprite).transform, val, num, false), (Ease)7), 0.1f);
	}
}
