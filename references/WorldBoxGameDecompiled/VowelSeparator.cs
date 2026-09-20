using System.Collections.Generic;

public static class VowelSeparator
{
	private const string VOWELS = "aeiouy";

	private const string VOWELS_SPECIAL = "àáâãäåæèéêëìíîïòóôõöøùúûüýÿāăąēĕėęěĩīĭįĳōŏőœũūŭůűųŷǎǐǒǔǖǘǚǜǟǡǣǫǭǻǽǿȁȃȅȇȉȋȍȏȕȗȧȩȫȭȯȱȳеийоуыэюяѐёєіїѝў";

	private static HashSet<char> _vowels = new HashSet<char>("aeiouy".ToCharArray());

	private static HashSet<char> _special_vowels = new HashSet<char>("àáâãäåæèéêëìíîïòóôõöøùúûüýÿāăąēĕėęěĩīĭįĳōŏőœũūŭůűųŷǎǐǒǔǖǘǚǜǟǡǣǫǭǻǽǿȁȃȅȇȉȋȍȏȕȗȧȩȫȭȯȱȳеийоуыэюяѐёєіїѝў".ToCharArray());

	public static void addRandomConsonants(StringBuilderPool pString, string[] pPartsToInsert)
	{
		if (pString.Length < 2)
		{
			return;
		}
		pString.ToLowerInvariant();
		int num = pString.LastIndexOfAny(' ', ',') + 2;
		using ListPool<int> listPool = new ListPool<int>(pString.Length);
		for (int i = num; i < pString.Length; i++)
		{
			if (isVowel(pString[i - 1]) && isVowel(pString[i]))
			{
				listPool.Add(i);
			}
		}
		if (listPool.Count != 0)
		{
			int random = OnomasticsLibrary.GetRandom(listPool);
			string random2 = OnomasticsLibrary.GetRandom(pPartsToInsert);
			pString.Insert(random, random2);
		}
	}

	public static ListPool<int> findAllVowels(StringBuilderPool pString, int pStart, int pLength)
	{
		ListPool<int> listPool = new ListPool<int>(pLength);
		for (int i = pStart; i < pStart + pLength; i++)
		{
			if (isVowel(pString[i]))
			{
				listPool.Add(i);
			}
		}
		return listPool;
	}

	public static ListPool<int> findAllSingleVowels(StringBuilderPool pString, int pStart, int pLength)
	{
		pString.ToLowerInvariant();
		ListPool<int> listPool = new ListPool<int>(pLength);
		for (int i = pStart; i < pStart + pLength; i++)
		{
			if (isVowel(pString[i]) && (i <= 0 || !isVowel(pString[i - 1])) && (i >= pString.Length - 1 || !isVowel(pString[i + 1])))
			{
				listPool.Add(i);
			}
		}
		return listPool;
	}

	public static bool isVowel(char pChar)
	{
		pChar = char.ToLowerInvariant(pChar);
		if (_vowels.Contains(pChar))
		{
			return true;
		}
		if (!char.IsLetter(pChar))
		{
			return false;
		}
		return _special_vowels.Contains(pChar);
	}
}
