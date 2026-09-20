using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IllustrationFadeIn : MonoBehaviour
{
	public float scale_start;

	public float scale_end;

	public float duration;

	public Ease ease_type;

	private void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		Button val = default(Button);
		if (!((Component)this).TryGetComponent<Button>(ref val))
		{
			val = ((Component)this).gameObject.AddComponent<Button>();
		}
		((UnityEvent)val.onClick).AddListener(new UnityAction(onCLick));
		((Graphic)((Component)this).GetComponent<Image>()).raycastTarget = true;
	}

	private void OnEnable()
	{
		startTween();
	}

	public void startTween()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(scale_start, scale_start, scale_start);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(scale_end, scale_end, scale_end);
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.From<Vector3, Vector3, VectorOptions>(ShortcutExtensions.DOScale(((Component)this).transform, val2, duration), val, true, false), ease_type);
	}

	public void onCLick()
	{
		startTween();
	}

	public IllustrationFadeIn()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		scale_start = 1.5f;
		scale_end = 1f;
		duration = 1f;
		ease_type = (Ease)12;
		((MonoBehaviour)this)._002Ector();
	}
}
