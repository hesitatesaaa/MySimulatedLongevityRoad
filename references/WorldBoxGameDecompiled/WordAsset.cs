using System;
using System.Text.RegularExpressions;

[Serializable]
public class WordAsset : Asset
{
	public string getLocaleID()
	{
		throw new NotImplementedException();
	}

	public string getDescriptionID()
	{
		throw new NotImplementedException();
	}

	public string getDescriptionID2()
	{
		throw new NotImplementedException();
	}

	public string getWordInLanguage(LanguageStructure pStructure, LinguisticsAsset pLinguisticsAsset, int pSeed)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		string wordPattern = getWordPattern(pStructure, pSeed);
		for (int i = 0; i < wordPattern.Length; i++)
		{
			stringBuilderPool.Append(wordPattern[i] switch
			{
				'S' => pStructure.syllables_start.GetRandom(), 
				'M' => pStructure.syllables_mid.GetRandom(), 
				'E' => pStructure.syllables_ends.GetRandom(), 
				_ => "", 
			});
		}
		string text = stringBuilderPool.ToString();
		if (pLinguisticsAsset.word_type != WordType.None)
		{
			int word_type = (int)pLinguisticsAsset.word_type;
			PrefixesSettings settings_prefixes = pStructure.settings_prefixes;
			SuffixesSettings settings_suffixes = pStructure.settings_suffixes;
			if (settings_prefixes.enabled[word_type])
			{
				text = settings_prefixes.sets[word_type].GetRandom() + settings_prefixes.separator[word_type] + text;
			}
			if (settings_suffixes.enabled[word_type])
			{
				text = text + settings_suffixes.separator[word_type] + settings_suffixes.sets[word_type].GetRandom();
			}
		}
		return text;
	}

	private string getWordPattern(LanguageStructure pStructure, int pSeed)
	{
		return selectWeightedPattern(pStructure.word_patterns, pStructure.word_weights);
	}

	private string selectWeightedPattern(string[] pPattern, float[] pWeight)
	{
		float num = Randy.random();
		float num2 = 0f;
		for (int i = 0; i < pPattern.Length; i++)
		{
			num2 += pWeight[i];
			if (num < num2)
			{
				return pPattern[i];
			}
		}
		return pPattern.Last();
	}

	private string fixWordBoundaries(string pWord)
	{
		if (string.IsNullOrEmpty(pWord))
		{
			return pWord;
		}
		return Regex.Replace(Regex.Replace(Regex.Replace(pWord, "([bcdfghjklmnpqrstvwxyz])\\1{2,}", "$1$1"), "([aeiou])\\1+", delegate(Match m)
		{
			string value = m.Value;
			switch (value)
			{
			case "ee":
			case "oo":
			case "aa":
				return value.Substring(0, 2);
			default:
				return m.Groups[1].Value;
			}
		}), "([bdgkpt])([bdgkpt])", "$1").Replace("tst", "st").Replace("ndn", "nd")
			.Replace("ckc", "ck");
	}
}
