using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UnitTextPhrases : MonoBehaviour
{
	[SerializeField]
	private RectTransform _size_parent;

	[SerializeField]
	private Text _text;

	private Tweener _text_tweener;

	private void Awake()
	{
		finish();
	}

	public void startNewTween(string pText, Transform pFollowObject)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		((Component)this).gameObject.SetActive(true);
		killTweens();
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f, 0f, Randy.randomFloat(-30f, 30f));
		((Transform)_size_parent).localRotation = Quaternion.Euler(val);
		_text.text = pText;
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(0f, (float)Randy.randomInt(8, 12), 0f);
		if ((Object)(object)pFollowObject == (Object)null)
		{
			((Component)_text).transform.localPosition = val2;
		}
		else
		{
			((Component)_text).transform.position = pFollowObject.position + val2;
		}
		_text.fontSize = Randy.randomInt(7, 9);
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(0f, Randy.randomFloat(30f, 60f), 0f);
		TweenExtensions.Kill((Tween)(object)_text_tweener, false);
		if ((Object)(object)pFollowObject == (Object)null)
		{
			_text_tweener = (Tweener)(object)ShortcutExtensions.DOLocalMove(((Component)_text).transform, val3, 3f, false);
		}
		else
		{
			_text_tweener = (Tweener)(object)ShortcutExtensions.DOMove(((Component)_text).transform, val3 + pFollowObject.position, 3f, false);
		}
		TweenSettingsExtensions.SetEase<Tweener>(_text_tweener, (Ease)9);
		((Tween)DOTweenModuleUI.DOColor(_text, Color.white, 1.25f)).onComplete = new TweenCallback(doTextFade);
	}

	private void doTextFade()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		((Tween)DOTweenModuleUI.DOFade(_text, 0f, 2f)).onComplete = new TweenCallback(finish);
	}

	public bool isTweening()
	{
		return TweenExtensions.IsActive((Tween)(object)_text_tweener);
	}

	private void finish()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		killTweens();
		((Graphic)_text).color = Toolbox.color_white_transparent;
		((Component)this).gameObject.SetActive(false);
	}

	private void killTweens()
	{
		Tweener text_tweener = _text_tweener;
		if (text_tweener != null)
		{
			TweenExtensions.Kill((Tween)(object)text_tweener, false);
		}
		ShortcutExtensions.DOKill((Component)(object)_text, false);
	}
}
