using System;
using System.Collections.Generic;

public class BlacklistTest12
{
	private static readonly Dictionary<string, string[]> _profanity = new Dictionary<string, string[]>();

	private const int INDEX_LENGTH = 3;

	private static bool _initiated = false;

	public static void init()
	{
		if (!_initiated)
		{
			_initiated = true;
			BlacklistTools.loadProfanityFilter(_profanity);
		}
	}

	internal static bool checkBlackList(string pName)
	{
		ReadOnlySpan<char> pText = pName.ToLower().AsSpan();
		Dictionary<string, string[]> profanity = _profanity;
		for (int i = 0; i < pText.Length - 3 + 1; i++)
		{
			string key = pText.Slice(i, 3).ToString();
			if (!profanity.TryGetValue(key, out var value))
			{
				continue;
			}
			for (int j = 0; j < value.Length; j++)
			{
				ReadOnlySpan<char> pSearchPattern = value[j].AsSpan();
				if (BlacklistTools.contains(ref pText, ref pSearchPattern))
				{
					return true;
				}
			}
		}
		ReadOnlySpan<char> pText2 = BlacklistTools.cleanSpan(pText);
		if (pText2 == pText || pText2.Length <= 2)
		{
			return false;
		}
		for (int k = 0; k < pText2.Length - 3 + 1; k++)
		{
			string key2 = pText2.Slice(k, 3).ToString();
			if (!profanity.TryGetValue(key2, out var value2))
			{
				continue;
			}
			for (int l = 0; l < value2.Length; l++)
			{
				ReadOnlySpan<char> pSearchPattern2 = value2[l].AsSpan();
				if (BlacklistTools.contains(ref pText2, ref pSearchPattern2))
				{
					return true;
				}
			}
		}
		return false;
	}
}
