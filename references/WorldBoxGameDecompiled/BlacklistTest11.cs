using System;
using System.Collections.Generic;

public class BlacklistTest11
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
		ReadOnlySpan<char> readOnlySpan = pName.ToLower().AsSpan();
		int length = readOnlySpan.Length;
		Dictionary<string, string[]> profanity = _profanity;
		for (int i = 0; i < length - 3 + 1; i++)
		{
			string key = readOnlySpan.Slice(i, 3).ToString();
			if (!profanity.TryGetValue(key, out var value))
			{
				continue;
			}
			for (int j = 0; j < value.Length; j++)
			{
				ReadOnlySpan<char> pSearchPattern = value[j].AsSpan();
				if (BlacklistTools.contains(readOnlySpan.Slice(i), pSearchPattern))
				{
					return true;
				}
			}
		}
		ReadOnlySpan<char> readOnlySpan2 = BlacklistTools.cleanSpan(readOnlySpan);
		int length2 = readOnlySpan2.Length;
		if (readOnlySpan2 == readOnlySpan || length2 <= 2)
		{
			return false;
		}
		for (int k = 0; k < length2 - 3 + 1; k++)
		{
			string key2 = readOnlySpan2.Slice(k, 3).ToString();
			if (!profanity.TryGetValue(key2, out var value2))
			{
				continue;
			}
			for (int l = 0; l < value2.Length; l++)
			{
				ReadOnlySpan<char> pSearchPattern2 = value2[l].AsSpan();
				if (BlacklistTools.contains(readOnlySpan2.Slice(k), pSearchPattern2))
				{
					return true;
				}
			}
		}
		return false;
	}
}
