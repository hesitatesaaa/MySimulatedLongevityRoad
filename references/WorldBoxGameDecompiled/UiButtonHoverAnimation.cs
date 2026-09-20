using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UiButtonHoverAnimation : MonoBehaviour
{
	private Button button;

	public Vector3 default_scale;

	public float scale_size = 1.1f;

	public static float scaleTime = 0.1f;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		button = ((Component)this).GetComponent<Button>();
		default_scale = ((Component)this).gameObject.transform.localScale;
	}

	private void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		button.OnHover(new UnityAction(startAnim));
	}

	private void startAnim()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = default_scale.x * scale_size;
		((Component)this).transform.localScale = new Vector3(num, num, num);
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, default_scale, scaleTime), (Ease)26);
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
	}
}
