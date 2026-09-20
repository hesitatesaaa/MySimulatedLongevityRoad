using System;
using System.Collections.Generic;

public class Blacklist
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

	internal static bool checkBlackList(StringBuilderPool pName)
	{
		pName.ToLowerInvariant();
		Span<char> span = stackalloc char[pName.Length];
		pName.CopyTo(0, span, pName.Length);
		ReadOnlySpan<char> readOnlySpan = span;
		Dictionary<string, string[]> profanity = _profanity;
		for (int i = 0; i < readOnlySpan.Length - 3 + 1; i++)
		{
			string key = readOnlySpan.Slice(i, 3).ToString();
			if (!profanity.TryGetValue(key, out var value))
			{
				continue;
			}
			for (int j = 0; j < value.Length; j++)
			{
				ReadOnlySpan<char> pSearchPattern = value[j].AsSpan();
				if (BlacklistTools.contains(readOnlySpan, pSearchPattern, i))
				{
					return true;
				}
			}
		}
		ReadOnlySpan<char> readOnlySpan2 = BlacklistTools.cleanSpan(readOnlySpan);
		if (readOnlySpan2 == readOnlySpan || readOnlySpan2.Length <= 2)
		{
			return false;
		}
		for (int k = 0; k < readOnlySpan2.Length - 3 + 1; k++)
		{
			string key2 = readOnlySpan2.Slice(k, 3).ToString();
			if (!profanity.TryGetValue(key2, out var value2))
			{
				continue;
			}
			for (int l = 0; l < value2.Length; l++)
			{
				ReadOnlySpan<char> pSearchPattern2 = value2[l].AsSpan();
				if (BlacklistTools.contains(readOnlySpan2, pSearchPattern2, k))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static bool checkBlackList(string pName)
	{
		ReadOnlySpan<char> readOnlySpan = pName.ToLower().AsSpan();
		Dictionary<string, string[]> profanity = _profanity;
		for (int i = 0; i < readOnlySpan.Length - 3 + 1; i++)
		{
			string key = readOnlySpan.Slice(i, 3).ToString();
			if (!profanity.TryGetValue(key, out var value))
			{
				continue;
			}
			for (int j = 0; j < value.Length; j++)
			{
				ReadOnlySpan<char> pSearchPattern = value[j].AsSpan();
				if (BlacklistTools.contains(readOnlySpan, pSearchPattern, i))
				{
					return true;
				}
			}
		}
		ReadOnlySpan<char> readOnlySpan2 = BlacklistTools.cleanSpan(readOnlySpan);
		if (readOnlySpan2 == readOnlySpan || readOnlySpan2.Length <= 2)
		{
			return false;
		}
		for (int k = 0; k < readOnlySpan2.Length - 3 + 1; k++)
		{
			string key2 = readOnlySpan2.Slice(k, 3).ToString();
			if (!profanity.TryGetValue(key2, out var value2))
			{
				continue;
			}
			for (int l = 0; l < value2.Length; l++)
			{
				ReadOnlySpan<char> pSearchPattern2 = value2[l].AsSpan();
				if (BlacklistTools.contains(readOnlySpan2, pSearchPattern2, k))
				{
					return true;
				}
			}
		}
		return false;
	}
}
