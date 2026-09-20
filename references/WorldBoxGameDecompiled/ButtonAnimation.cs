using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
	public static float scaleTime = 0.1f;

	private IEnumerator newAnim()
	{
		((Component)this).gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
		yield return CoroutineHelper.wait_for_0_01_s;
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).gameObject.transform, 1f, scaleTime), (Ease)28);
	}

	public void clickAnimation()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			((MonoBehaviour)this).StartCoroutine(newAnim());
		}
	}
}
