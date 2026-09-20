public class ArticleSettings : StructureSettings
{
	public override void create(LanguageStructure pStructure, int pSizeMin, int pSizeMax)
	{
		WordType[] word_types = LanguageStructureHelpers.word_types;
		foreach (WordType pWord in word_types)
		{
			generate(pStructure, pWord, pSizeMin, pSizeMax);
		}
	}

	public void generate(LanguageStructure pStructure, WordType pWord, int pSizeMin, int pSizeMax)
	{
		bool flag = Randy.randomBool();
		enabled[(int)pWord] = flag;
		if (flag)
		{
			sets[(int)pWord] = generateSets(pStructure, Randy.randomInt(pSizeMin, pSizeMax));
			separator[(int)pWord] = LanguageStructureHelpers.possible_article_separators.GetRandom();
		}
	}

	private string[] generateSets(LanguageStructure pStructure, int pAmount)
	{
		string[] array = new string[pAmount];
		for (int i = 0; i < pAmount; i++)
		{
			array[i] = Randy.randomInt(0, 5) switch
			{
				0 => pStructure.sets_consonants.GetRandom() + pStructure.sets_vowels.GetRandom() + pStructure.sets_consonants.GetRandom(), 
				1 => pStructure.sets_onset_2.GetRandom() + pStructure.sets_vowels.GetRandom(), 
				2 => pStructure.sets_consonants.GetRandom() + pStructure.sets_vowels.GetRandom(), 
				3 => pStructure.sets_vowels.GetRandom() + pStructure.sets_consonants.GetRandom() + pStructure.sets_vowels.GetRandom(), 
				_ => pStructure.sets_vowels.GetRandom() ?? "", 
			};
		}
		return array;
	}
}
