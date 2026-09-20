using System;

public static class StringExtension
{
	public static int[] AllIndexesOf(this string pString, string pValue)
	{
		int num = 0;
		int length = pValue.Length;
		int num2 = 0;
		while (num < pString.Length)
		{
			int num3 = pString.IndexOf(pValue, num, StringComparison.Ordinal);
			if (num3 == -1)
			{
				break;
			}
			num2++;
			num = num3 + length;
		}
		int[] array = new int[num2];
		num = 0;
		num2 = 0;
		while (num < pString.Length)
		{
			int num4 = pString.IndexOf(pValue, num, StringComparison.Ordinal);
			if (num4 == -1)
			{
				break;
			}
			array[num2] = num4;
			num2++;
			num = num4 + length;
		}
		return array;
	}

	public static char Last(this string pString)
	{
		return pString[pString.Length - 1];
	}

	public static char First(this string pString)
	{
		return pString[0];
	}

	public static string Reverse(this string pString)
	{
		return string.Create(pString.Length, pString, delegate(Span<char> pChars, string pState)
		{
			pState.AsSpan().CopyTo(pChars);
			pChars.Reverse();
		});
	}

	public static string Shuffle(this string pString)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		using ListPool<char> listPool = new ListPool<char>();
		for (int i = 0; i < pString.Length; i++)
		{
			listPool.Add(pString[i]);
		}
		listPool.Shuffle();
		for (int j = 0; j < listPool.Count; j++)
		{
			char value = listPool[j];
			stringBuilderPool.Append(value);
		}
		string text = stringBuilderPool.ToString();
		return char.ToUpper(text[0]) + text.Substring(1).ToLower();
	}

	public static string FirstToUpper(this string pString)
	{
		if (pString.Length == 0)
		{
			return pString;
		}
		string text = pString.Substring(0, 1).ToUpper();
		pString = pString.Substring(1, pString.Length - 1);
		return text + pString;
	}

	public static string ColorHex(this string pString, string pColorHex, bool pLocalize = false)
	{
		return Toolbox.coloredText(pString, pColorHex, pLocalize);
	}

	public static string blue(this string pString)
	{
		if (!string.IsNullOrEmpty(pString))
		{
			return pString.ColorHex("#4CCFFF");
		}
		return "";
	}

	public static string blue(this object pString)
	{
		return pString?.ToString().blue();
	}

	public static string red(this string pString)
	{
		if (!string.IsNullOrEmpty(pString))
		{
			return pString.ColorHex("#FF637D");
		}
		return "";
	}

	public static string red(this object pString)
	{
		return pString?.ToString().red();
	}

	public static string teal(this string pString)
	{
		if (!string.IsNullOrEmpty(pString))
		{
			return pString.ColorHex("#23F3FF");
		}
		return "";
	}

	public static string teal(this object pString)
	{
		return pString?.ToString().teal();
	}

	public static string yellow(this string pString)
	{
		if (!string.IsNullOrEmpty(pString))
		{
			return pString.ColorHex("#FFFF51");
		}
		return "";
	}

	public static string yellow(this object pString)
	{
		return pString?.ToString().yellow();
	}

	public static string Localize(this string pString)
	{
		return LocalizedTextManager.getText(pString.Underscore());
	}

	public static string Description(this string pString)
	{
		return pString + "_description";
	}

	public static bool EndsWithAny(this string pString, string[] pTrimString)
	{
		foreach (string value in pTrimString)
		{
			if (pString.EndsWith(value))
			{
				return true;
			}
		}
		return false;
	}

	public static string TrimEnd(this string pString, string pTrimString)
	{
		if (pString.EndsWith(pTrimString))
		{
			return pString.Substring(0, pString.Length - pTrimString.Length);
		}
		return pString;
	}

	public static bool HasUpperCase(this string pString)
	{
		ReadOnlySpan<char> readOnlySpan = pString.AsSpan();
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (char.IsUpper(readOnlySpan[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ShouldUnderscore(this string pString)
	{
		ReadOnlySpan<char> readOnlySpan = pString.AsSpan();
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (!char.IsLetterOrDigit(readOnlySpan[i]) && readOnlySpan[i] != '_')
			{
				return true;
			}
			if (char.IsWhiteSpace(readOnlySpan[i]) || char.IsUpper(readOnlySpan[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static string Truncate(this string pString, int pMaxLength)
	{
		if (string.IsNullOrEmpty(pString) || pString.Length <= pMaxLength)
		{
			return pString;
		}
		return pString.Substring(0, pMaxLength);
	}

	public static string Underscore(this string pString)
	{
		if (string.IsNullOrEmpty(pString))
		{
			return pString;
		}
		if (!pString.ShouldUnderscore())
		{
			return pString;
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		ReadOnlySpan<char> readOnlySpan = pString.AsSpan();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			if (char.IsLower(readOnlySpan[i]))
			{
				flag3 = true;
				break;
			}
		}
		for (int j = 0; j < readOnlySpan.Length; j++)
		{
			if (char.IsLetter(readOnlySpan[j]))
			{
				if (char.IsUpper(readOnlySpan[j]))
				{
					if (j > 0 && !flag && (!flag2 | flag3))
					{
						stringBuilderPool.Append('_');
					}
					stringBuilderPool.Append(char.ToLower(readOnlySpan[j]));
					flag2 = true;
				}
				else
				{
					stringBuilderPool.Append(readOnlySpan[j]);
					flag2 = false;
				}
				flag = false;
			}
			else if (char.IsDigit(readOnlySpan[j]))
			{
				stringBuilderPool.Append(readOnlySpan[j]);
				flag = false;
				flag2 = false;
			}
			else if (!flag)
			{
				stringBuilderPool.Append('_');
				flag = true;
				flag2 = false;
			}
		}
		if (flag)
		{
			stringBuilderPool.Remove(stringBuilderPool.Length - 1, 1);
		}
		return stringBuilderPool.ToString();
	}
}
