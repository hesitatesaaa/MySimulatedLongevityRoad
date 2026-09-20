using System;

public class HindiCorrector
{
	private static string[] hindi_letters = new string[178]
	{
		"‘", "’", "“", "”", "(", ")", "{", "}", "=", "।",
		"?", "-", "µ", "॰", ",", ".", "\u094d ", "०", "१", "२",
		"३", "४", "५", "६", "७", "८", "९", "x", ":", "ल\u094dम",
		"ङ", "ऩ", "ऱ", "य\u093c", "ग़", "ड़", "ढ़", "ख़\u094dय", "ख़\u094d", "ख़",
		"क़\u094dय", "क़\u094d", "क़", "फ\u093c\u094d", "फ़", "ज़\u094dय", "ज़\u094d", "ज़", "त\u094dत\u094d", "त\u094dत",
		"क\u094dत", "द\u0943", "क\u0943", "ह\u094dन", "ह\u094dय", "ह\u0943", "ह\u094dम", "ह\u094dर", "ह\u094d", "द\u094dद",
		"क\u094dष\u094d", "क\u094dष", "त\u094dर\u094d", "त\u094dर", "ज\u094dञ", "छ\u094dय", "ट\u094dय", "ठ\u094dय", "ड\u094dय", "ढ\u094dय",
		"द\u094dय", "द\u094dव", "श\u094dर", "ट\u094dर", "ड\u094dर", "ढ\u094dर", "छ\u094dर", "क\u094dर", "फ\u094dर", "द\u094dर",
		"प\u094dर", "ग\u094dर", "र\u0941", "र\u0942", "\u094dर", "ओ", "औ", "आ", "अ", "ई",
		"इ", "उ", "ऊ", "ऐ", "ए", "ऋ", "क\u094d", "क", "क\u094dक", "ख\u094d",
		"ख", "ग\u094d", "ग", "घ\u094d", "घ", "ङ", "च\u0948", "च\u094d", "च", "छ",
		"ज\u094d", "ज", "झ\u094d", "झ", "ञ", "ट\u094dट", "ट\u094dठ", "ट", "ठ", "ड\u094dड",
		"ड\u094dढ", "ड", "ढ", "ण\u094d", "ण", "त\u094d", "त", "थ\u094d", "थ", "द\u094dध",
		"द", "ध\u094d", "ध", "न\u094d", "न", "प\u094d", "प", "फ\u094d", "फ", "ब\u094d",
		"ब", "भ\u094d", "भ", "म\u094d", "म", "य\u094d", "य", "र", "ल\u094d", "ल",
		"ळ", "व\u094d", "व", "श\u094d", "श", "ष\u094d", "ष", "स\u094d", "स", "ह",
		"ऑ", "\u0949", "\u094b", "\u094c", "\u093e", "\u0940", "\u0941", "\u0942", "\u0943", "\u0947",
		"\u0948", "\u0902", "\u0901", "\u0903", "\u0945", "ऽ", "\u094d ", "\u094d"
	};

	private static string[] replace_letters = new string[178]
	{
		"^", "*", "Þ", "ß", "¼", "½", "¿", "À", "¾", "A",
		"\\", "&", "&", "Œ", "]", "-", "~ ", "å", "ƒ", "„",
		"…", "†", "‡", "ˆ", "‰", "Š", "‹", "Û", "%", "Ye",
		"³", "u+", "j+", ";+", "x+", "M+", "<+", "[+;", "[+", "[k+",
		"D+;", "D+", "d+", "¶+", "Q+", "T+;", "T+", "t+", "Ù", "Ùk",
		"Dr", "–", "—", "à", "á", "â", "ã", "ºz", "º", "í",
		"{", "{k", "«", "=", "K", "Nî", "Vî", "Bî", "Mî", "<î",
		"|", "}", "J", "Vª", "Mª", "<ªª", "Nª", "Ø", "Ý", "æ",
		"ç", "xz", "#", ":", "z", "vks", "vkS", "vk", "v", "bZ",
		"b", "m", "Å", ",s", ",", "_", "D", "d", "ô", "[",
		"[k", "X", "x", "?", "?k", "³", "pkS", "P", "p", "N",
		"T", "t", "÷", ">", "¥", "ê", "ë", "V", "B", "ì",
		"ï", "M", "<", ".", ".k", "R", "r", "F", "Fk", ")",
		"n", "/", "/k", "U", "u", "I", "i", "¶", "Q", "C",
		"c", "H", "Hk", "E", "e", "\u00b8", ";", "j", "Y", "y",
		"G", "O", "o", "'", "'k", "\"", "\"k", "L", "l", "g",
		"v‚", "‚", "ks", "kS", "k", "h", "q", "w", "`", "s",
		"S", "a", "¡", "%", "W", "·", "~ ", "~"
	};

	public static string GetCorrectedHindiText(string unicode_substring)
	{
		int num = hindi_letters.Length;
		string text = unicode_substring;
		for (int num2 = text.IndexOf("'", StringComparison.Ordinal); num2 >= 0; num2 = text.IndexOf("'", StringComparison.Ordinal))
		{
			text = ReplaceFirstOccurrence(text, "'", "^");
			text = ReplaceFirstOccurrence(text, "'", "*");
		}
		for (int num3 = text.IndexOf("\"", StringComparison.Ordinal); num3 >= 0; num3 = text.IndexOf("\"", StringComparison.Ordinal))
		{
			text = ReplaceFirstOccurrence(text, "\"", "ß");
			text = ReplaceFirstOccurrence(text, "\"", "Þ");
		}
		for (int num4 = text.IndexOf("\u093f", StringComparison.Ordinal); num4 != -1; num4 = text.IndexOf("\u093f", num4 + 1, StringComparison.Ordinal))
		{
			char c = text[num4 - 1];
			text = text.Replace(c + "\u093f", "f" + c);
			while (text.Contains("\u094df" + c))
			{
				int num5 = text.IndexOf("\u094df" + c, StringComparison.Ordinal);
				text = text.Replace(text[num5 - 1] + "\u094df" + c, "f" + text[num5 - 1] + "\u094d" + c);
			}
		}
		string text2 = "\u093e\u093f\u0940\u0941\u0942\u0943\u0947\u0948\u094b\u094c\u0902:\u0901\u0945";
		text += "  ";
		for (int num6 = text.IndexOf("र\u094d", StringComparison.Ordinal); num6 > 0; num6 = text.IndexOf("र\u094d", StringComparison.Ordinal))
		{
			int num7 = num6 + 2;
			if (text[num7 + 1] == '\u094d')
			{
				num7 += 2;
			}
			char value = text[num7 + 1];
			while (text2.IndexOf(value) != -1)
			{
				num7++;
				value = text[num7 + 1];
			}
			string text3 = text.Substring(num6 + 2, num7 - num6 - 1);
			text = text.Replace("र\u094d" + text3, text3 + "Z");
		}
		text = text.Substring(0, text.Length - 2);
		for (int i = 0; i < num; i++)
		{
			int num8 = 0;
			if (text.Contains(hindi_letters[i]))
			{
				while (num8 != -1)
				{
					text = text.Replace(hindi_letters[i], replace_letters[i]);
					num8 = text.IndexOf(hindi_letters[i], StringComparison.Ordinal);
				}
			}
		}
		return text;
	}

	public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
	{
		int num = Source.IndexOf(Find, StringComparison.Ordinal);
		if (num < 0)
		{
			return Source;
		}
		return Source.Remove(num, Find.Length).Insert(num, Replace);
	}
}
