using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class UiMover : MonoBehaviour
{
	public bool onVisible;

	public Vector3 initPos;

	public Vector3 hidePos;

	public bool visible;

	public bool initInitPos = true;

	private Tweener _tweener;

	private void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (initInitPos)
		{
			initPos = ((Component)this).gameObject.transform.localPosition;
		}
	}

	public void setVisible(bool pVisible, bool pNow = false, TweenCallback pCompleteCallback = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		visible = pVisible;
		if (pNow)
		{
			if (pVisible)
			{
				((Component)this).gameObject.transform.localPosition = initPos;
			}
			else
			{
				((Component)this).gameObject.transform.localPosition = hidePos;
			}
		}
		else if (visible)
		{
			if (!onVisible)
			{
				onVisible = true;
				moveTween(initPos, pCompleteCallback);
			}
		}
		else if (onVisible)
		{
			onVisible = false;
			moveTween(hidePos, pCompleteCallback);
		}
	}

	protected void moveTween(Vector3 pVecPos, TweenCallback pCompleteCallback = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.35f;
		TweenExtensions.Kill((Tween)(object)_tweener, true);
		_tweener = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, pVecPos, num, false), 0.02f), (Ease)10), pCompleteCallback);
	}
}
