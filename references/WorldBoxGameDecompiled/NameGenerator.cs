using System;
using System.Collections.Generic;
using UnityPools;

public class NameGenerator
{
	private static int _current_consonants = 0;

	private static int _current_vowels = 0;

	private static readonly char[] vowels_all = new char[6] { 'a', 'e', 'i', 'o', 'u', 'y' };

	private static readonly char[] consonants_all = new char[20]
	{
		'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm',
		'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z'
	};

	private static bool _initiated = false;

	[ThreadStatic]
	private static Dictionary<string, ListPool<string>> _dict_splitted_items;

	public static void init()
	{
		if (!_initiated)
		{
			_initiated = true;
			Blacklist.init();
		}
	}

	public static string generateName(Actor pActor, MetaType pType, long pSeed, ActorSex pSex = ActorSex.None)
	{
		string text = null;
		int hashCode = pType.GetHashCode();
		pSeed += hashCode;
		if (pActor.hasCulture())
		{
			OnomasticsData onomasticData = pActor.culture.getOnomasticData(pType);
			if (onomasticData != null)
			{
				return onomasticData.generateName(pSex, 0, pSeed);
			}
			text = pActor.culture.getNameTemplate(pType);
		}
		else
		{
			foreach (Actor parent in pActor.getParents())
			{
				if (parent.hasCulture())
				{
					OnomasticsData onomasticData2 = parent.culture.getOnomasticData(pType);
					if (onomasticData2 != null)
					{
						return onomasticData2.generateName(pSex, 0, pSeed);
					}
					text = parent.culture.getNameTemplate(pType);
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = pActor.asset.getNameTemplate(pType);
		}
		return getName(text, pSex, pForceLegacy: false, null, pSeed);
	}

	public static string getName(string pAssetID, ActorSex pSex = ActorSex.Male, bool pForceLegacy = false, string pTemplate = null, long? pSeed = null, bool pIgnoreBlackList = false)
	{
		init();
		NameGeneratorAsset nameGeneratorAsset = AssetManager.name_generator.get(pAssetID);
		_current_consonants = 0;
		_current_vowels = 0;
		string text = generateNameFromTemplate(nameGeneratorAsset, null, null, pForceLegacy, 0, pTemplate, null, pTestReplacer: false, pSeed, pSex, pIgnoreBlackList);
		if (!nameGeneratorAsset.hasOnomastics() && pSex == ActorSex.Female)
		{
			string strB = text.Substring(text.Length - 1, 1);
			bool flag = false;
			string[] vowels = nameGeneratorAsset.vowels;
			for (int i = 0; i < vowels.Length; i++)
			{
				if (vowels[i].CompareTo(strB) == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				text += Randy.getRandom(nameGeneratorAsset.vowels);
			}
		}
		return text;
	}

	private static string firstToUpper(string pString)
	{
		return pString.FirstToUpper();
	}

	private static string addVowel(string[] pList, bool pUppercase = false)
	{
		_current_consonants = 0;
		_current_vowels++;
		if (pUppercase)
		{
			return firstToUpper(Randy.getRandom(pList));
		}
		return Randy.getRandom(pList);
	}

	private static string addEnding(NameGeneratorAsset pTemplate, string pName)
	{
		string text = Randy.getRandom(pTemplate.parts);
		if (isConsonant(text[0]) && isConsonant(pName[pName.Length - 1]))
		{
			text = addVowel(pTemplate.vowels) + text;
		}
		else if (!isConsonant(text[0]) && !isConsonant(pName[pName.Length - 1]))
		{
			text = addConsonant(pTemplate.consonants) + text;
		}
		return text;
	}

	private static string addConsonant(string[] pList, bool pUppercase = false)
	{
		_current_consonants++;
		_current_vowels = 0;
		if (pUppercase)
		{
			return firstToUpper(Randy.getRandom(pList));
		}
		return Randy.getRandom(pList);
	}

	private static string addPart(string[] pArray, bool pUppercase = false)
	{
		string text = Randy.getRandom(pArray);
		if (isConsonant(text[text.Length - 1]))
		{
			_current_consonants++;
			_current_vowels = 0;
		}
		else
		{
			_current_consonants = 0;
			_current_vowels++;
		}
		if (pUppercase)
		{
			text = firstToUpper(text);
		}
		return text;
	}

	private static bool isConsonant(char pChar)
	{
		return consonants_all.IndexOf(pChar) > -1;
	}

	private static bool isVowel(char pChar)
	{
		return vowels_all.IndexOf(pChar) > -1;
	}

	public static string generateNameFromTemplate(string pAssetID, Actor pActor = null, Kingdom pKingdom = null, bool pForceLegacy = false)
	{
		return generateNameFromTemplate(AssetManager.name_generator.get(pAssetID), pActor, pKingdom, pForceLegacy);
	}

	public static string generateNameFromOnomastics(NameGeneratorAsset pAsset, string pTemplate = null, Actor pActor = null, long? pSeed = null, ActorSex pSex = ActorSex.None)
	{
		OnomasticsData onomasticsData = (string.IsNullOrEmpty(pTemplate) ? OnomasticsCache.getOriginalData(pAsset.onomastics_templates.GetRandom()) : OnomasticsCache.getOriginalData(pTemplate));
		ActorSex pSex2 = pSex;
		if (pActor != null)
		{
			pSex2 = pActor.data.sex;
		}
		return onomasticsData.generateName(pSex2, 0, pSeed);
	}

	public static string generateNamesFromTemplate(int pAmount, NameGeneratorAsset pAsset, Actor pActor = null, Kingdom pKingdom = null, bool pForceLegacy = false, bool pTestReplacer = false)
	{
		string text = "";
		HashSet<string> hashSet = new HashSet<string>();
		List<string> list = new List<string>();
		if (pAsset.hasOnomastics() && !pForceLegacy)
		{
			foreach (string onomastics_template in pAsset.onomastics_templates)
			{
				hashSet.Clear();
				list.Clear();
				for (int i = 0; i < 100; i++)
				{
					hashSet.Add(generateNameFromTemplate(pAsset, pActor, pKingdom, pForceLegacy: false, 0, onomastics_template, null, pTestReplacer));
				}
				text = text + "\n--- " + onomastics_template;
				text = text + "\n -- (" + hashSet.Count + " / " + 100 + ") \n";
				list.AddRange(hashSet);
				list.Shuffle();
				if (list.Count - pAmount > 0)
				{
					list.RemoveRange(pAmount, list.Count - pAmount);
				}
				list.Sort();
				foreach (string item in list)
				{
					text = text + item + "\n";
				}
			}
		}
		else
		{
			for (int j = 0; j < 100; j++)
			{
				hashSet.Add(generateNameFromTemplate(pAsset, pActor, pKingdom, pForceLegacy, 0, null, null, pTestReplacer));
			}
			text += "\n--- Legacy";
			text = text + "\n -- (" + hashSet.Count + " / " + 100 + ") \n";
			list.AddRange(hashSet);
			list.Shuffle();
			if (list.Count - pAmount > 0)
			{
				list.RemoveRange(pAmount, list.Count - pAmount);
			}
			list.Sort();
			foreach (string item2 in list)
			{
				text = text + item2 + "\n";
			}
		}
		return text;
	}

	public static string generateNameFromTemplate(NameGeneratorAsset pAsset, Actor pActor = null, Kingdom pKingdom = null, bool pForceLegacy = false, int pCalls = 0, string pOnomasticsTemplate = null, string[] pClassicTemplate = null, bool pTestReplacer = false, long? pSeed = null, ActorSex pSex = ActorSex.None, bool pIgnoreBlacklist = false)
	{
		if (pCalls > 50)
		{
			return string.Empty;
		}
		if (pAsset.hasOnomastics() && !pForceLegacy)
		{
			return generateNameFromOnomastics(pAsset, pOnomasticsTemplate, pActor, pSeed, pSex);
		}
		_current_consonants = 0;
		_current_vowels = 0;
		string pName = "";
		string[] obj = pClassicTemplate ?? pAsset.templates.GetRandom();
		bool flag = false;
		bool flag2 = false;
		string[] array = obj;
		foreach (string text in array)
		{
			string text2;
			string text3;
			if (text.Contains('#'))
			{
				string[] array2 = text.Split('#');
				text2 = array2[0];
				text3 = array2[1];
			}
			else
			{
				text2 = text;
				text3 = "";
			}
			if (pAsset.use_dictionary)
			{
				if (text2 == "$comma$")
				{
					pName += ", ";
					continue;
				}
				if (text2.Contains(';'))
				{
					text2 = text2.Split(';').GetRandom();
				}
				Dictionary<string, ListPool<string>> dict_splitted_items = _dict_splitted_items;
				if (dict_splitted_items == null || !dict_splitted_items.ContainsKey(text2))
				{
					if (_dict_splitted_items == null)
					{
						_dict_splitted_items = UnsafeCollectionPool<Dictionary<string, ListPool<string>>, KeyValuePair<string, ListPool<string>>>.Get();
					}
					ListPool<string> value = new ListPool<string>(pAsset.dict_parts[text2].Split(','));
					_dict_splitted_items.Add(text2, value);
				}
				_dict_splitted_items[text2].ShuffleLast();
				string text4 = _dict_splitted_items[text2].Last();
				if (_dict_splitted_items[text2].Count > 1)
				{
					_dict_splitted_items[text2].Pop();
				}
				pName += text4;
				continue;
			}
			switch (text2)
			{
			case "RANDOM_LETTER":
				pName = ((!Randy.randomBool()) ? (pName + addConsonant(pAsset.consonants, pUppercase: true)) : (pName + addVowel(pAsset.vowels, pUppercase: true)));
				break;
			case "space":
			case " ":
				pName += " ";
				break;
			case "letters":
			{
				string[] array4 = text3.Split('-');
				pName += addWord(pAsset, int.Parse(array4[0]), int.Parse(array4[1]));
				break;
			}
			case "Letters":
			{
				string[] array3 = text3.Split('-');
				pName += addWord(pAsset, int.Parse(array3[0]), int.Parse(array3[1]), pToUpperFirst: true);
				break;
			}
			case "part":
				pName += pAsset.parts.GetRandom();
				break;
			case "consonant":
				pName += addConsonant(pAsset.consonants);
				break;
			case "CONSONANT":
				pName += addConsonant(pAsset.consonants, pUppercase: true);
				break;
			case "vowel":
				pName += addVowel(pAsset.vowels);
				break;
			case "vowelchance":
				if (Randy.randomBool())
				{
					pName += addVowel(pAsset.vowels);
				}
				break;
			case "removalchance":
				if (Randy.randomBool())
				{
					pName.Remove(pName.Length - 1);
				}
				break;
			case "VOWEL":
				pName += addVowel(pAsset.vowels, pUppercase: true);
				break;
			case "special1":
				pName += pAsset.special1.GetRandom();
				break;
			case "special2":
				pName += pAsset.special2.GetRandom();
				break;
			case "Part":
			{
				string random = pAsset.parts.GetRandom();
				random = firstToUpper(random);
				pName += random;
				break;
			}
			case "number":
				pName += Randy.randomInt(0, 10);
				break;
			case "addition_start":
				if (!flag2 && Randy.randomChance(pAsset.add_addition_chance))
				{
					pName = pName + pAsset.addition_start.GetRandom() + " ";
					flag2 = true;
				}
				break;
			case "addition_ending":
				if (!flag2 && Randy.randomChance(pAsset.add_addition_chance))
				{
					pName = pName + " " + pAsset.addition_ending.GetRandom();
					flag2 = true;
				}
				break;
			case "part_group":
				foreach (string part_group in pAsset.part_groups)
				{
					string[] pArray6 = part_group.Split(',');
					pName += pArray6.GetRandom();
				}
				break;
			case "part_group2":
				foreach (string item in pAsset.part_groups2)
				{
					string[] pArray5 = item.Split(',');
					pName += pArray5.GetRandom();
				}
				break;
			case "part_group3":
				foreach (string item2 in pAsset.part_groups3)
				{
					string[] pArray4 = item2.Split(',');
					pName += pArray4.GetRandom();
				}
				break;
			case "Part_group":
				flag = true;
				foreach (string part_group2 in pAsset.part_groups)
				{
					string[] pArray3 = part_group2.Split(',');
					if (flag)
					{
						pName += firstToUpper(pArray3.GetRandom());
						flag = false;
					}
					else
					{
						pName += pArray3.GetRandom();
					}
				}
				break;
			case "Part_group2":
				flag = true;
				foreach (string item3 in pAsset.part_groups2)
				{
					string[] pArray2 = item3.Split(',');
					if (flag)
					{
						pName += firstToUpper(pArray2.GetRandom());
						flag = false;
					}
					else
					{
						pName += pArray2.GetRandom();
					}
				}
				break;
			case "Part_group3":
				flag = true;
				foreach (string item4 in pAsset.part_groups3)
				{
					string[] pArray = item4.Split(',');
					if (flag)
					{
						pName += firstToUpper(pArray.GetRandom());
						flag = false;
					}
					else
					{
						pName += pArray.GetRandom();
					}
				}
				break;
			}
		}
		if (pName.Contains('$'))
		{
			if (pTestReplacer)
			{
				NameGeneratorReplacers.replacer_debug(ref pName);
			}
			else
			{
				if (pAsset.replacer != null)
				{
					pAsset.replacer(ref pName, pActor);
				}
				if (pAsset.replacer_kingdom != null)
				{
					pAsset.replacer_kingdom(ref pName, pKingdom);
				}
			}
		}
		bool flag3 = false;
		if (string.IsNullOrEmpty(pName))
		{
			flag3 = true;
		}
		else if (!pAsset.use_dictionary && !pIgnoreBlacklist && Blacklist.checkBlackList(pName))
		{
			flag3 = true;
		}
		if (flag3)
		{
			return generateNameFromTemplate(pAsset, pActor, pKingdom, pForceLegacy, ++pCalls, pOnomasticsTemplate, pClassicTemplate, pTestReplacer);
		}
		pName = firstToUpper(pName);
		if (pAsset.finalizer != null)
		{
			pName = pAsset.finalizer(pName);
		}
		if (_dict_splitted_items != null)
		{
			foreach (ListPool<string> value2 in _dict_splitted_items.Values)
			{
				value2.Dispose();
			}
			_dict_splitted_items.Clear();
			UnsafeCollectionPool<Dictionary<string, ListPool<string>>, KeyValuePair<string, ListPool<string>>>.Release(_dict_splitted_items);
			_dict_splitted_items = null;
		}
		return pName;
	}

	private static string addWord(NameGeneratorAsset pAsset, int pMin, int pMax, bool pToUpperFirst = false)
	{
		string text = "";
		int num = Randy.randomInt(pMin, pMax);
		for (int i = 0; i < num; i++)
		{
			if (_current_consonants >= pAsset.max_consonants_in_row)
			{
				text += addVowel(pAsset.vowels, pToUpperFirst);
				pToUpperFirst = false;
			}
			else if (_current_vowels >= pAsset.max_vowels_in_row)
			{
				text += addConsonant(pAsset.consonants, pToUpperFirst);
				pToUpperFirst = false;
			}
			else if (Randy.randomBool())
			{
				text += addVowel(pAsset.vowels, pToUpperFirst);
				pToUpperFirst = false;
			}
			else
			{
				text += addConsonant(pAsset.consonants, pToUpperFirst);
				pToUpperFirst = false;
			}
		}
		return text;
	}
}
