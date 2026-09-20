using DG.Tweening;
using UnityEngine;

public interface IShakable
{
	float shake_duration { get; }

	float shake_strength { get; }

	Tweener shake_tween { get; set; }

	Transform transform { get; }

	void shake()
	{
		killShakeTween();
		shake_tween = ShortcutExtensions.DOShakePosition(transform, shake_duration, shake_strength, 10, 90f, false, true, (ShakeRandomnessMode)0);
	}

	void killShakeTween()
	{
		TweenExtensions.Kill((Tween)(object)shake_tween, true);
	}
}
