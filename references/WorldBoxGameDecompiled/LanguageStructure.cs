using System;
using System.Text.RegularExpressions;

[Serializable]
public class LanguageStructure
{
	public string[] sets_vowels;

	public string[] sets_consonants;

	public string[] sets_onset_1;

	public string[] sets_onset_2;

	public string[] sets_codas_1;

	public string[] sets_codas_2;

	public string[] sets_diphthongs;

	public string[] syllables_start;

	public string[] syllables_mid;

	public string[] syllables_ends;

	public string[] word_patterns;

	public float[] word_weights;

	public ArticleSettings settings_articles;

	public PrefixesSettings settings_prefixes;

	public SuffixesSettings settings_suffixes;

	public LanguageStructure()
	{
		generateSyllableSets();
	}

	public void generateSyllableSets()
	{
		if (syllables_start == null)
		{
			generateMainParts();
			generatePatterns();
			int pSizeMin = Randy.randomInt(1, 2);
			int pSizeMax = Randy.randomInt(1, 3);
			settings_articles = new ArticleSettings();
			settings_articles.create(this, pSizeMin, pSizeMax);
			settings_prefixes = new PrefixesSettings();
			settings_prefixes.create(this, 0, 4);
			settings_suffixes = new SuffixesSettings();
			settings_suffixes.create(this, 0, 4);
			syllables_start = generateSyllables("syllable_starts", Randy.randomInt(2, 10));
			syllables_mid = generateSyllables("syllable_mids", Randy.randomInt(2, 10));
			syllables_ends = generateSyllables("syllable_ends", Randy.randomInt(2, 10));
		}
	}

	private void generatePatterns()
	{
		int num = Randy.randomInt(3, 10);
		word_patterns = new string[num];
		word_weights = new float[num];
		for (int i = 0; i < num; i++)
		{
			word_patterns[i] = LanguageStructureHelpers.possible_word_patterns.GetRandom();
			word_weights[i] = Randy.randomFloat(0.05f, 1f);
		}
	}

	private void generateMainParts()
	{
		sets_consonants = generateParts("consonant", 5);
		sets_vowels = generateParts("vowel", 5);
		sets_onset_1 = generateParts("onset1", 5);
		sets_onset_2 = generateParts("onset2", 5);
		sets_codas_1 = generateParts("coda1", 5);
		sets_codas_2 = generateParts("coda2", 5);
		sets_diphthongs = generateParts("diphthongs", 5);
	}

	private string[] generateParts(string pID, int pAmount)
	{
		LinguisticsAsset linguisticsAsset = AssetManager.linguistics_library.get(pID);
		string[] array = new string[pAmount];
		for (int i = 0; i < pAmount; i++)
		{
			array[i] = linguisticsAsset.getRandom();
		}
		return array;
	}

	private string[] generateSyllables(string pID, int pAmount)
	{
		string[] array = new string[pAmount];
		LinguisticsAsset linguisticsAsset = AssetManager.linguistics_library.get(pID);
		for (int i = 0; i < pAmount; i++)
		{
			string[] randomPattern = linguisticsAsset.getRandomPattern();
			string text = string.Join("", randomPattern);
			using (new StringBuilderPool())
			{
				string text2 = string.Empty;
				string empty = string.Empty;
				string text3 = string.Empty;
				if (text.StartsWith("CC"))
				{
					text2 = sets_onset_2.GetRandom();
				}
				else if (text.StartsWith("C"))
				{
					text2 = sets_onset_1.GetRandom();
				}
				empty = ((text.Contains("VV") || Randy.randomChance(0.2f)) ? sets_diphthongs.GetRandom() : sets_vowels.GetRandom());
				if (text.EndsWith("CC"))
				{
					text3 = sets_codas_2.GetRandom();
				}
				else if (text.EndsWith("C"))
				{
					text3 = sets_codas_1.GetRandom();
				}
				string text4 = text2 + empty + text3;
				array[i] = text4;
			}
		}
		return array;
	}

	private string fixOrthography(string pSyllable)
	{
		if (string.IsNullOrEmpty(pSyllable))
		{
			return pSyllable;
		}
		string input = pSyllable;
		input = Regex.Replace(input, "([bcdfghjklmnpqrstvwxyz])\\1{2,}", "$1$1");
		input = input.Replace("ck", "ck");
		input = input.Replace("kk", "ck");
		input = input.Replace("cc", "ck");
		input = Regex.Replace(input, "qw|qv", "qu");
		input = input.Replace("q", "qu");
		input = Regex.Replace(input, "aa+", "a");
		input = Regex.Replace(input, "ii+", "i");
		input = Regex.Replace(input, "uu+", "u");
		if (input.StartsWith("x"))
		{
			input = "z" + input.Substring(1);
		}
		input = Regex.Replace(input, "([bcdfghjklmnpqrstvwxyz])\\1\\1+", "$1$1");
		input = input.Replace("tch", "ch");
		input = input.Replace("dge", "ge");
		if (input.Length > 2)
		{
			string text = input.Substring(0, 2).ToLower();
			string[] array = new string[7] { "kg", "pn", "gn", "kn", "wr", "mn", "ps" };
			foreach (string text2 in array)
			{
				if (text == text2)
				{
					input = input.Substring(1);
					break;
				}
			}
		}
		return input;
	}
}
