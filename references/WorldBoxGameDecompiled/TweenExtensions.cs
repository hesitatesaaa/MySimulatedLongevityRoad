using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.UI;

public static class TweenExtensions
{
	public static TweenerCore<int, int, NoOptions> DOUpCounter(this Text target, int endValue)
	{
		if (!int.TryParse(target.text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
		{
			result = 0;
		}
		return target.DOUpCounter(result, endValue, 0.45f);
	}

	public static TweenerCore<int, int, NoOptions> DOUpCounter(this Text target, int endValue, float duration, string pEnding = "", string pColor = "")
	{
		if (!int.TryParse(target.text, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
		{
			result = 0;
		}
		return target.DOUpCounter(result, endValue, duration, pEnding, pColor);
	}

	public static TweenerCore<int, int, NoOptions> DOUpCounter(this Text target, int fromValue, int endValue, float duration, string pEnding = "", string pColor = "")
	{
		TweenerCore<int, int, NoOptions> obj = DOTween.To((DOGetter<int>)(() => fromValue), (DOSetter<int>)delegate(int x)
		{
			fromValue = x;
			if (pColor != "")
			{
				target.text = Toolbox.coloredText(fromValue.ToText(4) + pEnding, pColor);
			}
			else
			{
				target.text = fromValue.ToText(4) + pEnding;
			}
		}, endValue, duration);
		TweenSettingsExtensions.SetEase<TweenerCore<int, int, NoOptions>>(obj, (Ease)12);
		TweenSettingsExtensions.SetTarget<TweenerCore<int, int, NoOptions>>(obj, (object)target);
		return obj;
	}

	public static TweenerCore<float, float, FloatOptions> DOUpCounter(this Text target, float fromValue, float endValue, float duration, string pEnding = "", string pColor = "")
	{
		TweenerCore<float, float, FloatOptions> obj = DOTween.To((DOGetter<float>)(() => fromValue), (DOSetter<float>)delegate(float x)
		{
			fromValue = x;
			if (pColor != "")
			{
				target.text = Toolbox.coloredText(fromValue.ToText() + pEnding, pColor);
			}
			else
			{
				target.text = fromValue.ToText() + pEnding;
			}
		}, endValue, duration);
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(obj, (Ease)14);
		TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(obj, (object)target);
		return obj;
	}

	public static TweenerCore<long, long, NoOptions> DORandomCounter(this Text target, long fromValue, long endValue, float duration)
	{
		long current = fromValue;
		int endLength = endValue.ToString().Length;
		string endVal = endValue.ToString();
		TweenerCore<long, long, NoOptions> obj = DOTween.To((DOGetter<long>)(() => current), (DOSetter<long>)delegate(long x)
		{
			current = x;
			string text = "";
			string text2 = current.ToString();
			bool flag = (float)(current - fromValue) < (float)(endValue - fromValue) * 0.95f;
			for (int i = 0; i < endLength; i++)
			{
				if (flag)
				{
					text += Randy.randomInt((i == 0) ? 1 : 0, 10);
				}
				else if (text2.Length >= endLength && endVal.Substring(i, 1) == text2.Substring(i, 1))
				{
					text += endVal.Substring(i, 1);
				}
				else if (i == 0)
				{
					int num = int.Parse(endVal.Substring(i, 1)) + 1;
					text += ((num < 2) ? 1 : Randy.randomInt(1, (num > 10) ? 10 : num));
				}
				else
				{
					text += Randy.randomInt(0, 10);
				}
			}
			target.text = long.Parse(text).ToText();
		}, endValue, duration);
		TweenSettingsExtensions.SetEase<TweenerCore<long, long, NoOptions>>(obj, (Ease)12);
		TweenSettingsExtensions.SetTarget<TweenerCore<long, long, NoOptions>>(obj, (object)target);
		return obj;
	}

	public static TweenerCore<int, int, NoOptions> DORandomCounter(this Text target, int fromValue, int endValue, float duration)
	{
		int current = fromValue;
		int endLength = endValue.ToString().Length;
		string endVal = endValue.ToString();
		TweenerCore<int, int, NoOptions> obj = DOTween.To((DOGetter<int>)(() => current), (DOSetter<int>)delegate(int x)
		{
			current = x;
			string text = "";
			string text2 = current.ToString();
			bool flag = (float)(current - fromValue) < (float)(endValue - fromValue) * 0.95f;
			for (int i = 0; i < endLength; i++)
			{
				if (flag)
				{
					text += Randy.randomInt((i == 0) ? 1 : 0, 10);
				}
				else if (text2.Length >= endLength && endVal.Substring(i, 1) == text2.Substring(i, 1))
				{
					text += endVal.Substring(i, 1);
				}
				else if (i == 0)
				{
					int num = int.Parse(endVal.Substring(i, 1)) + 1;
					text += ((num < 2) ? 1 : Randy.randomInt(1, (num > 10) ? 10 : num));
				}
				else
				{
					text += Randy.randomInt(0, 10);
				}
			}
			target.text = int.Parse(text).ToText();
		}, endValue, duration);
		TweenSettingsExtensions.SetEase<TweenerCore<int, int, NoOptions>>(obj, (Ease)12);
		TweenSettingsExtensions.SetTarget<TweenerCore<int, int, NoOptions>>(obj, (object)target);
		return obj;
	}

	public static TweenerCore<float, float, FloatOptions> DOMinHeight(this LayoutElement target, float endValue, float duration, bool snapping = false)
	{
		TweenerCore<float, float, FloatOptions> obj = DOTween.To((DOGetter<float>)(() => target.minHeight), (DOSetter<float>)delegate(float x)
		{
			target.minHeight = x;
		}, endValue, duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<float, float, FloatOptions> DOPreferredHeight(this LayoutElement target, float endValue, float duration, bool snapping = false)
	{
		TweenerCore<float, float, FloatOptions> obj = DOTween.To((DOGetter<float>)(() => target.preferredHeight), (DOSetter<float>)delegate(float x)
		{
			target.preferredHeight = x;
		}, endValue, duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, snapping), (object)target);
		return obj;
	}
}
