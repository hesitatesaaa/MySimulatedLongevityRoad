using System;
using System.Collections.Generic;

public class BlacklistTest8
{
	private static readonly Dictionary<int, HashSet<int>> _profanity = new Dictionary<int, HashSet<int>>();

	private static int _min_length = int.MaxValue;

	private static int _max_length = int.MinValue;

	private static readonly Dictionary<int, char[]> _char_arrays = new Dictionary<int, char[]>();

	private static bool _initiated = false;

	public static void init()
	{
		if (!_initiated)
		{
			_initiated = true;
			BlacklistTools.loadProfanityFilter(_profanity, ref _min_length, ref _max_length);
			for (int i = _min_length; i <= _max_length; i++)
			{
				_char_arrays[i] = new char[i];
			}
		}
	}

	private static int getCharHashCode(char[] pChar)
	{
		return BlacklistTools.getCharHashCode(pChar);
	}

	internal static bool checkBlackList(string pName)
	{
		ReadOnlySpan<char> readOnlySpan = pName.ToLower().AsSpan();
		ReadOnlySpan<char> readOnlySpan2 = BlacklistTools.cleanSpan(readOnlySpan);
		bool flag = !(readOnlySpan2 == readOnlySpan);
		for (int i = _min_length; i <= _max_length; i++)
		{
			char[] array = _char_arrays[i];
			HashSet<int> hashSet = _profanity[i];
			for (int j = 0; j < readOnlySpan.Length - i + 1; j++)
			{
				readOnlySpan.Slice(j, i).CopyTo(array);
				int charHashCode = getCharHashCode(array);
				if (hashSet.Contains(charHashCode))
				{
					return true;
				}
				if (flag && readOnlySpan2.Length >= j + i)
				{
					readOnlySpan2.Slice(j, i).CopyTo(array);
					charHashCode = getCharHashCode(array);
					if (hashSet.Contains(charHashCode))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
