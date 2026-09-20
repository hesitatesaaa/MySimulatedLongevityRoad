using System;

[Serializable]
public class PrefixesSettings : StructureSettings
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
		int num = Randy.randomInt(pSizeMin, pSizeMax);
		bool flag = num != 0 && Randy.randomBool();
		enabled[(int)pWord] = flag;
		if (flag)
		{
			sets[(int)pWord] = generateSets(pStructure, num);
			separator[(int)pWord] = "";
		}
	}

	private string[] generateSets(LanguageStructure pStructure, int pAmount)
	{
		string[] array = new string[pAmount];
		for (int i = 0; i < pAmount; i++)
		{
			array[i] = Randy.randomInt(0, 5) switch
			{
				0 => pStructure.sets_consonants.GetRandom() + pStructure.sets_vowels.GetRandom(), 
				1 => pStructure.sets_onset_2.GetRandom() + pStructure.sets_vowels.GetRandom(), 
				2 => pStructure.sets_consonants.GetRandom() + pStructure.sets_vowels.GetRandom(), 
				_ => pStructure.sets_vowels.GetRandom() + pStructure.sets_consonants.GetRandom(), 
			};
		}
		return array;
	}
}
