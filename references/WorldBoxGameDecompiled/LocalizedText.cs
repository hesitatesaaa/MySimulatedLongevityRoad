using System.Collections.Generic;
using System.Text.RegularExpressions;
using UPersian.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LocalizedText : UIBehaviour
{
	public const string DEFAULT_KEY = "??????";

	protected const char LINE_ENDING = '\n';

	public bool convertToUppercase;

	public bool autoField = true;

	public bool specialTags;

	public string key = "??????";

	private FontStyle? _font_style_before;

	private bool? _shadow_before = false;

	private bool _has_shadow;

	internal Text text;

	private TextAnchor? _text_alignment_before;

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		text = ((Component)this).GetComponent<Text>();
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
		if (autoField)
		{
			LocalizedTextManager.addTextField(this);
			updateText();
		}
	}

	public void setKeyAndUpdate(string pKey)
	{
		key = pKey;
		updateText();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		GameLanguageAsset current_language = LocalizedTextManager.current_language;
		if ((current_language == null || current_language.isRTL()) && !string.IsNullOrEmpty(key) && !(key == "??????") && !((Object)(object)text == (Object)null))
		{
			updateText();
			((UIBehaviour)this).OnRectTransformDimensionsChange();
		}
	}

	internal virtual void updateText(bool pCheckText = true)
	{
		if ((Object)(object)this.text == (Object)null || LocalizedTextManager.instance == null || !LocalizedTextManager.instance.initiated)
		{
			return;
		}
		if ((Object)(object)LocalizedTextManager.current_font != (Object)null)
		{
			this.text.font = LocalizedTextManager.current_font;
		}
		string text = LocalizedTextManager.getText(key, this.text);
		if (convertToUppercase)
		{
			text = text.ToUpper();
		}
		if (specialTags && text.Contains("$"))
		{
			if (text.Contains("$total_prem_powers$"))
			{
				text = text.Replace("$total_prem_powers$", GodPower.premium_powers.Count.ToString() ?? "");
			}
			if (text.Contains("$minutes$"))
			{
				text = text.Replace("$minutes$", 30.ToString() ?? "");
			}
			if (text.Contains("$minutes_clock$"))
			{
				text = text.Replace("$minutes_clock$", 720.ToString() ?? "");
			}
			if (text.Contains("$hours_clock$"))
			{
				text = text.Replace("$hours_clock$", 12.ToString() ?? "");
			}
			if (text.Contains("$power$") && Config.power_to_unlock != null)
			{
				text = text.Replace("$power$", Config.power_to_unlock.getLocaleID().Localize() ?? "");
			}
			if (text.Contains("$hours$"))
			{
				text = text.Replace("$hours$", 3.ToString() ?? "");
			}
			if (text.Contains("$number$"))
			{
				text = text.Replace("$number$", 3.ToString() ?? "");
			}
			if (text.Contains("$discord_count$"))
			{
				text = text.Replace("$discord_count$", 560000.ToText() ?? "");
			}
			if (text.Contains("$wbcode$"))
			{
				text = text.Replace("$wbcode$", "<color=cyan>WB-5555-1166-5555</color>");
			}
			if (text.Contains("$lifeissimhours$"))
			{
				text = text.Replace("$lifeissimhours$", 24f.ToText());
			}
			if (text.Contains("$current_era_year"))
			{
				text = text.Replace("$current_era_year$", Date.getCurrentYear().ToText());
			}
			if (text.Contains("$era_moons_left"))
			{
				int pInt = World.world.era_manager.calculateMoonsLeft();
				text = text.Replace("$era_moons_left$", pInt.ToText());
			}
		}
		this.text.text = text;
		checkTextFont();
		if (pCheckText)
		{
			checkSpecialLanguages();
		}
	}

	internal void checkTextFont(GameLanguageAsset pLanguage = null)
	{
		if (!((Object)(object)text == (Object)null))
		{
			if (pLanguage == null)
			{
				pLanguage = LocalizedTextManager.current_language;
			}
			Font val = pLanguage.font();
			if (!((Object)(object)val == (Object)null))
			{
				text.font = val;
			}
		}
	}

	internal void checkSpecialLanguages(GameLanguageAsset pLanguage = null)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)text == (Object)null)
		{
			return;
		}
		if (pLanguage == null)
		{
			pLanguage = LocalizedTextManager.current_language;
		}
		checkTextFont(pLanguage);
		if (!_text_alignment_before.HasValue)
		{
			_text_alignment_before = text.alignment;
		}
		if (!_font_style_before.HasValue)
		{
			_font_style_before = text.fontStyle;
		}
		if (!_shadow_before.HasValue)
		{
			_shadow_before = (_has_shadow = ((Component)(object)text).HasComponent<Shadow>());
		}
		if (pLanguage.hasForcedStyle())
		{
			text.fontStyle = pLanguage.force_style.style;
			if (text.fontSize < 9 && pLanguage.force_style.shadow && !_has_shadow)
			{
				((Component)text).gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 160f);
				_has_shadow = true;
			}
		}
		else
		{
			text.fontStyle = _font_style_before.Value;
			if (_has_shadow && _shadow_before == false)
			{
				Shadow val = default(Shadow);
				if (((Component)text).TryGetComponent<Shadow>(ref val))
				{
					Object.Destroy((Object)(object)val);
				}
				_has_shadow = false;
			}
		}
		if (pLanguage.isRTL())
		{
			text.text = getRTLText(text, text.text);
			text.alignment = getRTLAlignment(_text_alignment_before.Value);
		}
		else
		{
			text.alignment = _text_alignment_before.Value;
		}
		if (pLanguage.isHindi() && !Regex.IsMatch(text.text, "[a-zA-Z]"))
		{
			text.SetHindiText(text.text);
		}
	}

	internal static string getRTLText(Text pText, string pString)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		TextGenerator cachedTextGenerator = pText.cachedTextGenerator;
		Rect rect = ((Graphic)pText).rectTransform.rect;
		cachedTextGenerator.Populate(pString, pText.GetGenerationSettings(((Rect)(ref rect)).size));
		if (!(pText.cachedTextGenerator.lines is List<UILineInfo> list))
		{
			return null;
		}
		string text = "";
		if (list.Count == 0)
		{
			text = pString;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (i < list.Count - 1)
			{
				int startCharIdx = list[i].startCharIdx;
				int length = list[i + 1].startCharIdx - list[i].startCharIdx;
				text += pString.Substring(startCharIdx, length);
				if (text.Length > 0 && text[text.Length - 1] != '\n' && text[text.Length - 1] != '\r')
				{
					text += "\n";
				}
			}
			else
			{
				text += pString.Substring(list[i].startCharIdx);
			}
		}
		UPersianUtils.RtlFix(ref text);
		return text;
	}

	internal TextAnchor getRTLAlignment(TextAnchor pTextAlignment)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((int)pTextAlignment == 0)
		{
			return (TextAnchor)2;
		}
		if ((int)pTextAlignment == 2)
		{
			return (TextAnchor)0;
		}
		if ((int)pTextAlignment == 3)
		{
			return (TextAnchor)5;
		}
		if ((int)pTextAlignment == 5)
		{
			return (TextAnchor)3;
		}
		if ((int)pTextAlignment == 6)
		{
			return (TextAnchor)8;
		}
		if ((int)pTextAlignment == 8)
		{
			return (TextAnchor)6;
		}
		return pTextAlignment;
	}
}
