using System;
using System.Collections.Generic;

public class BlacklistTest3
{
	private static readonly Dictionary<char, string[]> _profanity = new Dictionary<char, string[]>();

	private static readonly HashSet<char> _unique = new HashSet<char>();

	private static bool _initiated = false;

	public static void init()
	{
		if (!_initiated)
		{
			_initiated = true;
			BlacklistTools.loadProfanityFilter(_profanity, _unique);
		}
	}

	internal static bool checkBlackList(string pName)
	{
		string text = pName.ToLower();
		_unique.Clear();
		_unique.UnionWith(text);
		_unique.RemoveWhere((char pChar) => !char.IsLetter(pChar));
		Dictionary<char, string[]> profanity = _profanity;
		ReadOnlySpan<char> readOnlySpan = text.AsSpan();
		ReadOnlySpan<char> readOnlySpan2 = BlacklistTools.cleanSpan(readOnlySpan);
		bool flag = !(readOnlySpan2 == readOnlySpan);
		foreach (char item in _unique)
		{
			if (!profanity.TryGetValue(item, out var value))
			{
				continue;
			}
			for (int num = 0; num < value.Length; num++)
			{
				ReadOnlySpan<char> pSearchPattern = value[num].AsSpan();
				if (BlacklistTools.contains(readOnlySpan, pSearchPattern))
				{
					return true;
				}
				if (flag && BlacklistTools.contains(readOnlySpan2, pSearchPattern))
				{
					return true;
				}
			}
		}
		return false;
	}
}
