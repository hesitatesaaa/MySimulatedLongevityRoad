using System;
using System.Collections.Generic;

public class BlacklistTest6
{
	private static readonly Dictionary<char, char[][]> _profanity = new Dictionary<char, char[][]>();

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
		string text2 = BlacklistTools.cleanStringAsSpan(text);
		bool flag = !(text2 == text);
		Dictionary<char, char[][]> profanity = _profanity;
		ReadOnlySpan<char> pText = text.AsSpan();
		ReadOnlySpan<char> pText2 = (flag ? text2.AsSpan() : ((ReadOnlySpan<char>)null));
		foreach (char item in _unique)
		{
			if (!profanity.TryGetValue(item, out var value))
			{
				continue;
			}
			for (int num = 0; num < value.Length; num++)
			{
				ReadOnlySpan<char> pSearchPattern = value[num].AsSpan();
				if (BlacklistTools.contains(pText, pSearchPattern))
				{
					return true;
				}
				if (flag && BlacklistTools.contains(pText2, pSearchPattern))
				{
					return true;
				}
			}
		}
		return false;
	}
}
