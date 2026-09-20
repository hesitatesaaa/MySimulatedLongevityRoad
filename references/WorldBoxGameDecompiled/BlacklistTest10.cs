using System.Collections.Generic;

public class BlacklistTest10
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

	public static bool checkBlackList(string pName)
	{
		string text = pName.ToLower();
		_unique.Clear();
		_unique.UnionWith(text);
		_unique.RemoveWhere((char pChar) => !char.IsLetter(pChar));
		string text2 = BlacklistTools.cleanString(text);
		bool flag = !(text2 == text);
		Dictionary<char, string[]> profanity = _profanity;
		foreach (char item in _unique)
		{
			if (!profanity.ContainsKey(item))
			{
				continue;
			}
			for (int num = 0; num < profanity[item].Length; num++)
			{
				if (text.Contains(profanity[item][num]))
				{
					return true;
				}
				if (flag && text2.Contains(profanity[item][num]))
				{
					return true;
				}
			}
		}
		return false;
	}
}
