using System.Globalization;

namespace System.Text;

public static class StringBuilderExtensions
{
	public static StringBuilder Remove(this StringBuilder sb, params char[] removeChars)
	{
		int num = 0;
		while (num < sb.Length)
		{
			if (ArrayExtensions.IndexOf(removeChars, sb[num]) > -1)
			{
				sb.Remove(num, 1);
			}
			else
			{
				num++;
			}
		}
		return sb;
	}

	public static StringBuilder Remove(this StringBuilder sb, int startIndex)
	{
		if (startIndex >= sb.Length)
		{
			return sb;
		}
		return sb.Remove(startIndex, sb.Length - startIndex);
	}

	private static bool IsBOMWhitespace(char c)
	{
		return false;
	}

	private static StringBuilder TrimHelper(this StringBuilder sb, int trimType)
	{
		int num = sb.Length - 1;
		int i = 0;
		if (trimType != 1)
		{
			for (i = 0; i < sb.Length && (char.IsWhiteSpace(sb[i]) || IsBOMWhitespace(sb[i])); i++)
			{
			}
		}
		if (trimType != 0)
		{
			num = sb.Length - 1;
			while (num >= i && (char.IsWhiteSpace(sb[num]) || IsBOMWhitespace(sb[i])))
			{
				num--;
			}
		}
		return sb.CreateTrimmedString(i, num);
	}

	internal static StringBuilder CreateTrimmedString(this StringBuilder sb, int start, int end)
	{
		int num = end - start + 1;
		if (num == sb.Length)
		{
			return sb;
		}
		if (num == 0)
		{
			sb.Length = 0;
			return sb;
		}
		return sb.InternalSubstring(start, end);
	}

	private static StringBuilder InternalSubstring(this StringBuilder sb, int startIndex, int end)
	{
		sb.Length = end + 1;
		if (startIndex > 0)
		{
			sb.Remove(0, startIndex);
		}
		return sb;
	}

	private static StringBuilder TrimHelper(this StringBuilder sb, char[] trimChars, int trimType)
	{
		int num = sb.Length - 1;
		int i = 0;
		if (trimType != 1)
		{
			for (i = 0; i < sb.Length; i++)
			{
				int j = 0;
				for (char c = sb[i]; j < trimChars.Length && trimChars[j] != c; j++)
				{
				}
				if (j == trimChars.Length)
				{
					break;
				}
			}
		}
		if (trimType != 0)
		{
			for (num = sb.Length - 1; num >= i; num--)
			{
				int k = 0;
				for (char c2 = sb[num]; k < trimChars.Length && trimChars[k] != c2; k++)
				{
				}
				if (k == trimChars.Length)
				{
					break;
				}
			}
		}
		return sb.CreateTrimmedString(i, num);
	}

	public static StringBuilder TrimStart(this StringBuilder sb, params char[] trimChars)
	{
		if (trimChars != null && trimChars.Length != 0)
		{
			return sb.TrimHelper(trimChars, 0);
		}
		return sb.TrimHelper(0);
	}

	public static StringBuilder TrimEnd(this StringBuilder sb, params char[] trimChars)
	{
		if (trimChars != null && trimChars.Length != 0)
		{
			return sb.TrimHelper(trimChars, 1);
		}
		return sb.TrimHelper(1);
	}

	public static StringBuilder Trim(this StringBuilder sb)
	{
		return sb.TrimHelper(2);
	}

	public static StringBuilder Trim(this StringBuilder sb, params char[] trimChars)
	{
		if (trimChars != null && trimChars.Length != 0)
		{
			return sb.TrimHelper(trimChars, 2);
		}
		return sb.TrimHelper(2);
	}

	public static int IndexOf(this StringBuilder sb, char value)
	{
		return sb.IndexOf(value, 0, sb.Length);
	}

	public static int IndexOf(this StringBuilder sb, char value, int startIndex)
	{
		return sb.IndexOf(value, startIndex, sb.Length - startIndex);
	}

	public static int IndexOf(this StringBuilder sb, char value, int startIndex, int count)
	{
		if (sb.Length == 0 || count == 0)
		{
			return -1;
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			if (sb[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf(this StringBuilder sb, string value, bool ignoreCase = false)
	{
		if (value == string.Empty)
		{
			return 0;
		}
		return IndexOfInternal(sb, value, 0, sb.Length, ignoreCase);
	}

	public static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase = false)
	{
		return IndexOfInternal(sb, value, startIndex, sb.Length - startIndex, ignoreCase);
	}

	public static int IndexOf(this StringBuilder sb, string value, int startIndex, int count, bool ignoreCase = false)
	{
		return IndexOfInternal(sb, value, startIndex, count, ignoreCase);
	}

	private static int IndexOfInternal(StringBuilder sb, string value, int startIndex, int count, bool ignoreCase)
	{
		if (value == string.Empty)
		{
			return startIndex;
		}
		if (sb.Length == 0 || count == 0 || startIndex + 1 + value.Length > sb.Length)
		{
			return -1;
		}
		int length = value.Length;
		int num = startIndex + count - value.Length;
		if (!ignoreCase)
		{
			for (int i = startIndex; i <= num; i++)
			{
				if (sb[i] == value[0])
				{
					int j;
					for (j = 1; j < length && sb[i + j] == value[j]; j++)
					{
					}
					if (j == length)
					{
						return i;
					}
				}
			}
		}
		else
		{
			for (int k = startIndex; k <= num; k++)
			{
				if (char.ToLower(sb[k]) == char.ToLower(value[0]))
				{
					int j;
					for (j = 1; j < length && char.ToLower(sb[k + j]) == char.ToLower(value[j]); j++)
					{
					}
					if (j == length)
					{
						return k;
					}
				}
			}
		}
		return -1;
	}

	public static int IndexOfAny(this StringBuilder sb, char[] anyOf)
	{
		return sb.IndexOfAny(anyOf, 0, sb.Length);
	}

	public static int IndexOfAny(this StringBuilder sb, char[] anyOf, int startIndex)
	{
		return sb.IndexOfAny(anyOf, startIndex, sb.Length - startIndex);
	}

	public static int IndexOfAny(this StringBuilder sb, char[] anyOf, int startIndex, int count)
	{
		if (sb.Length == 0 || count == 0)
		{
			return -1;
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			if (ArrayExtensions.IndexOf(anyOf, sb[i]) > -1)
			{
				return i;
			}
		}
		return -1;
	}

	public static int LastIndexOf(this StringBuilder sb, char value)
	{
		return sb.LastIndexOf(value, sb.Length - 1, sb.Length);
	}

	public static int LastIndexOf(this StringBuilder sb, char value, int startIndex)
	{
		return sb.LastIndexOf(value, startIndex, startIndex + 1);
	}

	public static int LastIndexOf(this StringBuilder sb, char value, int startIndex, int count)
	{
		if (sb.Length == 0 || count == 0)
		{
			return -1;
		}
		for (int num = startIndex; num > startIndex - count; num--)
		{
			if (sb[num] == value)
			{
				return num;
			}
		}
		return -1;
	}

	public static int LastIndexOf(this StringBuilder sb, string value, bool ignoreCase = false)
	{
		if (value == string.Empty)
		{
			if (sb.Length == 0)
			{
				return 0;
			}
			return sb.Length - 1;
		}
		if (sb.Length == 0)
		{
			return -1;
		}
		return LastIndexOfInternal(sb, value, sb.Length - 1, sb.Length, ignoreCase);
	}

	public static int LastIndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase = false)
	{
		return LastIndexOfInternal(sb, value, startIndex, startIndex + 1, ignoreCase);
	}

	public static int LastIndexOf(this StringBuilder sb, string value, int startIndex, int count, bool ignoreCase = false)
	{
		return LastIndexOfInternal(sb, value, startIndex, count, ignoreCase);
	}

	private static int LastIndexOfInternal(StringBuilder sb, string value, int startIndex, int count, bool ignoreCase)
	{
		if (value == string.Empty)
		{
			return startIndex;
		}
		if (sb.Length == 0 || count == 0 || startIndex + 1 - count + value.Length > sb.Length)
		{
			return -1;
		}
		int length = value.Length;
		int num = length - 1;
		int num2 = startIndex - count + value.Length;
		if (!ignoreCase)
		{
			for (int num3 = startIndex; num3 >= num2; num3--)
			{
				if (sb[num3] == value[num])
				{
					int i;
					for (i = 1; i < length && sb[num3 - i] == value[num - i]; i++)
					{
					}
					if (i == length)
					{
						return num3 - i + 1;
					}
				}
			}
		}
		else
		{
			for (int num4 = startIndex; num4 >= num2; num4--)
			{
				if (char.ToLower(sb[num4]) == char.ToLower(value[num]))
				{
					int i;
					for (i = 1; i < length && char.ToLower(sb[num4 - i]) == char.ToLower(value[num - i]); i++)
					{
					}
					if (i == length)
					{
						return num4 - i + 1;
					}
				}
			}
		}
		return -1;
	}

	public static int LastIndexOfAny(this StringBuilder sb, char[] anyOf)
	{
		return sb.LastIndexOfAny(anyOf, sb.Length - 1, sb.Length);
	}

	public static int LastIndexOfAny(this StringBuilder sb, char[] anyOf, int startIndex)
	{
		return sb.LastIndexOfAny(anyOf, startIndex, startIndex + 1);
	}

	public static int LastIndexOfAny(this StringBuilder sb, char[] anyOf, int startIndex, int count)
	{
		if (sb.Length == 0 || count == 0)
		{
			return -1;
		}
		for (int num = startIndex; num > startIndex - count; num--)
		{
			if (ArrayExtensions.IndexOf(anyOf, sb[num]) > -1)
			{
				return num;
			}
		}
		return -1;
	}

	public static bool StartsWith(this StringBuilder sb, string value, bool ignoreCase = false)
	{
		int length = value.Length;
		if (length > sb.Length)
		{
			return false;
		}
		if (!ignoreCase)
		{
			for (int i = 0; i < length; i++)
			{
				if (sb[i] != value[i])
				{
					return false;
				}
			}
		}
		else
		{
			for (int j = 0; j < length; j++)
			{
				if (char.ToLower(sb[j]) != char.ToLower(value[j]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool EndsWith(this StringBuilder sb, string value, bool ignoreCase = false)
	{
		int length = value.Length;
		int num = sb.Length - 1;
		int num2 = length - 1;
		if (length > sb.Length)
		{
			return false;
		}
		if (!ignoreCase)
		{
			for (int i = 0; i < length; i++)
			{
				if (sb[num - i] != value[num2 - i])
				{
					return false;
				}
			}
		}
		else
		{
			for (int num3 = length - 1; num3 >= 0; num3--)
			{
				if (char.ToLower(sb[num - num3]) != char.ToLower(value[num2 - num3]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static StringBuilder ToLower(this StringBuilder sb)
	{
		for (int i = 0; i < sb.Length; i++)
		{
			sb[i] = char.ToLower(sb[i]);
		}
		return sb;
	}

	public static StringBuilder Reverse(this StringBuilder sb)
	{
		int length = sb.Length;
		for (int i = 0; i < length / 2; i++)
		{
			char value = sb[i];
			sb[i] = sb[length - i - 1];
			sb[length - i - 1] = value;
		}
		return sb;
	}

	public static StringBuilder ToLower(this StringBuilder sb, CultureInfo culture)
	{
		for (int i = 0; i < sb.Length; i++)
		{
			sb[i] = char.ToLower(sb[i], culture);
		}
		return sb;
	}

	public static StringBuilder ToLowerInvariant(this StringBuilder sb)
	{
		return sb.ToLower(CultureInfo.InvariantCulture);
	}

	public static StringBuilder ToUpper(this StringBuilder sb)
	{
		for (int i = 0; i < sb.Length; i++)
		{
			sb[i] = char.ToUpper(sb[i]);
		}
		return sb;
	}

	public static StringBuilder ToUpper(this StringBuilder sb, CultureInfo culture)
	{
		for (int i = 0; i < sb.Length; i++)
		{
			sb[i] = char.ToUpper(sb[i], culture);
		}
		return sb;
	}

	public static StringBuilder ToUpperInvariant(this StringBuilder sb)
	{
		return sb.ToUpper(CultureInfo.InvariantCulture);
	}

	public static StringBuilder ToTitleCase(this StringBuilder sb)
	{
		return sb.ToTitleCase(CultureInfo.InvariantCulture);
	}

	public static StringBuilder ToTitleCase(this StringBuilder sb, CultureInfo culture)
	{
		bool flag = true;
		for (int i = 0; i < sb.Length; i++)
		{
			if (flag)
			{
				sb[i] = char.ToUpper(sb[i], culture);
				flag = false;
			}
			else
			{
				sb[i] = char.ToLower(sb[i], culture);
			}
			if (char.IsWhiteSpace(sb[i]) || char.IsPunctuation(sb[i]) || sb[i] == '\'')
			{
				flag = true;
			}
		}
		return sb;
	}
}
