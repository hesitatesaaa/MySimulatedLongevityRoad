using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TweenedColoredText : MonoBehaviour
{
	public Color color1;

	public Color color2;

	public float duration;

	private Text _text;

	private void Awake()
	{
		_text = ((Component)this).GetComponent<Text>();
	}

	private void OnEnable()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ShortcutExtensions.DOKill((Component)(object)_text, true);
		((Graphic)_text).color = color1;
		TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOColor(_text, color2, duration), -1, (LoopType)1), (Ease)4);
	}

	public TweenedColoredText()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		color1 = Color.blue;
		color2 = Color.red;
		duration = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}
