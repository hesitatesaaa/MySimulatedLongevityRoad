using UnityEngine;

public class StoryLibrary : AssetLibrary<StoryAsset>
{
	public override void init()
	{
		add(new StoryAsset
		{
			id = "story_1"
		});
		t.addTemplate("pron_obj", "word_concept", "comma", "word_concept", "word_action", "word_concept", "word_creature", "period", "pron_obj", "word_concept", "pron_poss_adj", "word_place", "question_mark", "pron_obj", "word_concept", "pron_poss_adj", "word_place", "exclamation_mark");
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		if (Config.editor_maxim)
		{
			test();
		}
	}

	private void test()
	{
		string[] randomTemplate = get("story_1").getRandomTemplate();
		for (int i = 0; i < 10; i++)
		{
			Language language = new Language();
			LanguageStructure languageStructure = new LanguageStructure();
			language.data = new LanguageData();
			language.data.structure = languageStructure;
			string text = generateExample(language, randomTemplate);
			string text2 = generateExample(language, randomTemplate);
			string text3 = generateExample(language, randomTemplate);
			string text4 = "S:" + languageStructure.syllables_start.AsString() + ", |M:" + languageStructure.syllables_mid.AsString() + ", |E:" + languageStructure.syllables_ends.AsString();
			Debug.Log((object)("Example Language " + $"{i} : " + text4));
			Debug.Log((object)("Example " + 1 + ": " + text));
			Debug.Log((object)("Example " + 2 + ": " + text2));
			Debug.Log((object)("Example " + 3 + ": " + text3));
		}
	}

	public static string getTestText(Language pLanguage)
	{
		string[] randomTemplate = AssetManager.story_library.get("story_1").getRandomTemplate();
		if (pLanguage.data.structure == null)
		{
			LanguageStructure structure = new LanguageStructure();
			pLanguage.data.structure = structure;
		}
		return generateExample(pLanguage, randomTemplate);
	}

	private static string generateExample(Language pLanguage, string[] pTemplate)
	{
		LanguageStructure structure = pLanguage.data.structure;
		using ListPool<string> listPool = new ListPool<string>();
		LinguisticsAsset linguisticsAsset = null;
		for (int i = 0; i < pTemplate.Length; i++)
		{
			string pID = pTemplate[i];
			LinguisticsAsset simple = AssetManager.linguistics_library.getSimple(pID);
			if (simple == null)
			{
				continue;
			}
			if (simple.word_group)
			{
				if (i > 0 && simple.add_space)
				{
					listPool.Add(" ");
				}
				string[] array = null;
				int word_type = (int)simple.word_type;
				if (simple.word_type != WordType.None && structure.settings_articles.enabled[word_type])
				{
					array = structure.settings_articles.sets[word_type];
				}
				if (array != null)
				{
					listPool.Add(array.GetRandom());
					listPool.Add(structure.settings_articles.separator[word_type]);
				}
				string random = simple.array.GetRandom();
				string text = AssetManager.words_library.getSimple(random).getWordInLanguage(structure, simple, 0);
				if (linguisticsAsset != null && linguisticsAsset.next_uppercase)
				{
					text = text.FirstToUpper();
				}
				listPool.Add(text);
			}
			else if (!string.IsNullOrEmpty(simple.simple_text))
			{
				listPool.Add(simple.simple_text);
			}
			else if (simple.symbols_around)
			{
				listPool.Insert(listPool.Count - 1, simple.symbols_around_left);
				listPool.Insert(listPool.Count, simple.symbols_around_right);
			}
			linguisticsAsset = simple;
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		foreach (ref string item in listPool)
		{
			string current = item;
			stringBuilderPool.Append(current);
		}
		return stringBuilderPool.ToString().FirstToUpper();
	}
}
