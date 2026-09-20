using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class LogoButton : MonoBehaviour
{
	private List<UiCreature> listLetters;

	private float initScale = 1f;

	private Tweener tweener;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		initScale = ((Component)this).transform.localScale.x;
		loadLetters();
	}

	private void loadLetters()
	{
		listLetters = new List<UiCreature>();
		Transform transform = ((Component)((Component)this).transform.FindRecursive("Letters")).transform;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UiCreature component = ((Component)transform.GetChild(i)).GetComponent<UiCreature>();
			if (component.dropped)
			{
				component.resetPosition();
			}
			listLetters.Add(component);
		}
	}

	private void letterFall()
	{
		if (listLetters.Count == 0)
		{
			loadLetters();
			AchievementLibrary.destroy_worldbox.check();
			return;
		}
		listLetters.ShuffleOne();
		UiCreature uiCreature = listLetters[0];
		listLetters.RemoveAt(0);
		uiCreature.click();
	}

	public void clickLogo()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		MusicBox.playSound("event:/SFX/EXPLOSIONS/ExplosionHuge");
		if (tweener != null && ((Tween)tweener).active)
		{
			TweenExtensions.Kill((Tween)(object)tweener, false);
		}
		float num = initScale * 1.2f;
		if (listLetters.Count == 0)
		{
			num = 1.6f;
			((Component)this).transform.localScale = new Vector3(num, num, num);
			tweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, new Vector3(initScale, initScale, initScale), 0.3f), (Ease)27);
		}
		else
		{
			((Component)this).transform.localScale = new Vector3(num, num, num);
			tweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, new Vector3(initScale, initScale, initScale), 0.3f), (Ease)27);
		}
		letterFall();
	}
}
