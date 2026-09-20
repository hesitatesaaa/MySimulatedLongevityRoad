using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomButtonSwitch : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Action click_increase;

	public Action click_decrease;

	private Animator anim;

	private Vector3 defaultScale;

	private Vector3 clickedScale;

	private void Start()
	{
		anim = ((Component)this).gameObject.GetComponent<Animator>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)eventData.button == 1)
		{
			click_decrease?.Invoke();
			SoundBox.click();
			newClickAnimation();
		}
		else
		{
			click_increase?.Invoke();
			SoundBox.click();
			newClickAnimation();
		}
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		defaultScale = ((Component)this).transform.localScale;
		clickedScale = defaultScale * 1.1f;
	}

	public void newClickAnimation()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		((Component)this).transform.localScale = clickedScale;
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, defaultScale, 0.3f), (Ease)28);
	}

	private void OnDestroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
	}
}
