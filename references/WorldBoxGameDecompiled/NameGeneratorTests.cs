using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPools;

public static class NameGeneratorTests
{
	private static string _test_string;

	public static void runTests()
	{
	}

	public static string testAllNamesForUniqueness()
	{
		string text = "";
		foreach (NameGeneratorAsset item2 in AssetManager.name_generator.list)
		{
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < 1000; i++)
			{
				string item = NameGenerator.generateNameFromTemplate(item2);
				if (!hashSet.Contains(item))
				{
					hashSet.Add(item);
				}
			}
			text = text + "Unique names for asset " + item2.id + ": " + hashSet.Count + "\n";
		}
		return writeResults("name_test3_uniq", text);
	}

	public static string testAllNamesOutput()
	{
		string text = "";
		foreach (NameGeneratorAsset item in AssetManager.name_generator.list)
		{
			text = text + "\n--- asset name: " + item.id + " ---\n";
			text = text + NameGenerator.generateNamesFromTemplate(20, item, null, null, pForceLegacy: false, pTestReplacer: true) + "\n";
		}
		return writeResults("name_test3", text);
	}

	public static string testNamesAlliances()
	{
		testNameStart();
		testName("alliance_name");
		return testNameEnd();
	}

	public static string testNamesWars()
	{
		testNameStart();
		testName("war_conquest");
		testName("war_rebellion");
		testName("war_spite");
		testName("war_inspire");
		testName("war_whisper");
		return testNameEnd();
	}

	public static string testNamesItems()
	{
		testNameStart();
		testName("boots_name");
		testName("armor_name");
		testName("helmet_name");
		testName("ring_name");
		testName("amulet_name");
		return testNameEnd();
	}

	public static string testNamesWeapons()
	{
		testNameStart();
		testName("sword_name");
		testName("axe_name");
		testName("hammer_name");
		testName("stick_name");
		testName("blaster_name");
		testName("spear_name");
		testName("bow_name");
		testName("flame_sword_name");
		testName("necromancer_staff_name");
		testName("evil_staff_name");
		testName("white_staff_name");
		testName("plague_doctor_staff_name");
		testName("druid_staff_name");
		return testNameEnd();
	}

	public static void testNameStart()
	{
		_test_string = "";
	}

	public static string testNameEnd()
	{
		return writeResults("name_test2", _test_string);
	}

	public static void testName(string pID, int pAmount = 20)
	{
		_test_string = _test_string + "\n--- " + pID + ":\n";
		NameGeneratorAsset pAsset = AssetManager.name_generator.get(pID);
		_test_string = _test_string + NameGenerator.generateNamesFromTemplate(100, pAsset, null, null, pForceLegacy: false, pTestReplacer: true) + "\n";
	}

	public static string testNamesBooks()
	{
		testNameStart();
		using ListPool<string> listPool = new ListPool<string> { "book_name_fable", "book_name_biology", "book_name_math", "book_name_diplomacy_manual", "book_name_love_story", "book_name_bad_story", "book_name_warfare_manual", "book_name_economy_manual", "book_name_stewardship_manual", "book_name_history" };
		listPool.Shuffle();
		foreach (ref string item in listPool)
		{
			testName(item);
		}
		return testNameEnd();
	}

	public static string testNamesDefault()
	{
		string text = "";
		text += "\n--- default - legacy:\n";
		for (int i = 0; i < 100; i++)
		{
			text = text + NameGenerator.getName("orc_unit", ActorSex.Male, pForceLegacy: true) + "\n";
		}
		text += "\n--- default_name - onomastics:\n";
		for (int j = 0; j < 100; j++)
		{
			text = text + NameGenerator.getName("orc_unit") + "\n";
		}
		return writeResults("name_test_default", text);
	}

	public static string testNamesClans()
	{
		string text = "";
		text += "\n--- human_clan name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_clan")) + "\n";
		text += "\n--- elf_clan name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_clan")) + "\n";
		text += "\n--- dwarf_clan name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_clan")) + "\n";
		text += "\n--- orc_clan name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_clan")) + "\n";
		return writeResults("name_test2", text);
	}

	public static string testNamesKingdoms()
	{
		string text = "";
		text += "\n--- human_kingdom name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_kingdom")) + "\n";
		text += "\n--- elf_kingdom name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_kingdom")) + "\n";
		text += "\n--- dwarf_kingdom name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_kingdom")) + "\n";
		text += "\n--- orc_kingdom name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_kingdom")) + "\n";
		return writeResults("name_test2", text);
	}

	public static string testNamesCities()
	{
		string text = "";
		text += "\n--- human_city name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_city")) + "\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_city"), null, null, pForceLegacy: true) + "\n";
		text += "\n--- elf_city name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_city")) + "\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_city"), null, null, pForceLegacy: true) + "\n";
		text += "\n--- dwarf_city name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_city")) + "\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_city"), null, null, pForceLegacy: true) + "\n";
		text += "\n--- orc_city name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_city")) + "\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_city"), null, null, pForceLegacy: true) + "\n";
		return writeResults("name_test2", text);
	}

	public static string testNamesCulture()
	{
		string text = "";
		text += "\n--- elf_culture name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_culture")) + "\n";
		text += "\n--- dwarf_culture name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_culture")) + "\n";
		text += "\n--- orc_culture name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_culture")) + "\n";
		text += "\n--- human_culture name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_culture")) + "\n";
		return writeResults("name_test2", text);
	}

	public static string testMottos()
	{
		string text = "";
		text += "\n--- Mottos:\n";
		text = text + NameGenerator.generateNamesFromTemplate(100, AssetManager.name_generator.get("clan_mottos")) + "\n";
		return writeResults("name_test_mottos", text);
	}

	public static string testNames()
	{
		string text = "";
		text += "\n--- elf name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_unit")) + "\n";
		text += "\n--- elf City:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_city")) + "\n";
		text += "\n--- elf Kingdom:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("elf_kingdom")) + "\n";
		text += "\n--- dwarf name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_unit")) + "\n";
		text += "\n--- dwarf City:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_city")) + "\n";
		text += "\n--- dwarf Kingdom:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("dwarf_kingdom")) + "\n";
		text += "\n--- orc name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_unit")) + "\n";
		text += "\n--- orc City:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_city")) + "\n";
		text += "\n--- orc Kingdom:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("orc_kingdom")) + "\n";
		text += "\n--- Human name:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_unit")) + "\n";
		text += "\n--- Human City:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_city")) + "\n";
		text += "\n--- Human Kingdom:\n";
		text = text + NameGenerator.generateNamesFromTemplate(20, AssetManager.name_generator.get("human_kingdom")) + "\n";
		return writeResults("name_test2", text);
	}

	public static string testShowOnomasticsVsLegacy()
	{
		string text = "";
		string text2 = "[<color=green>ONO</color>]";
		string text3 = "[<color=orange>LEG</color>]";
		string text4 = "[<color=red>---</color>]";
		string text5 = "[<color=yellow>DIC</color>]";
		foreach (NameGeneratorAsset item in AssetManager.name_generator.list)
		{
			if ((!string.IsNullOrEmpty("") && !item.id.Contains("")) || "".Contains(item.id))
			{
				continue;
			}
			string text6 = text4;
			string text7 = text4;
			string text8 = text4;
			string text9 = " ";
			string text10 = " ";
			if (item.hasOnomastics())
			{
				text9 = "+";
				text7 = text2;
			}
			if (item.use_dictionary)
			{
				text6 = text5;
			}
			List<string[]> templates = item.templates;
			if (templates != null && templates.Count > 0)
			{
				text10 = "-";
				text8 = text3;
			}
			text = text + text9 + text10 + " " + text6 + " " + text7 + " " + text8 + " " + item.id + "\n";
			if (item.hasOnomastics())
			{
				List<string[]> templates2 = item.templates;
				if (templates2 != null && templates2.Count > 0)
				{
					text += compareOnomasticsVsLegacy(item.id, 15000);
				}
			}
		}
		return writeResults("name_test_ono", text);
	}

	public static string compareOnomasticsVsLegacy(string pNameAssetID, int pRuns)
	{
		string text = "";
		Randy.resetSeed(Randy.randomInt(1, 500));
		NameGeneratorAsset pAsset = AssetManager.name_generator.get(pNameAssetID);
		HashSet<string> hashSet = UnsafeCollectionPool<HashSet<string>, string>.Get();
		HashSet<string> hashSet2 = UnsafeCollectionPool<HashSet<string>, string>.Get();
		HashSet<string> hashSet3 = UnsafeCollectionPool<HashSet<string>, string>.Get();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		for (int i = 0; i < pRuns; i++)
		{
			hashSet.Add(NameGenerator.generateNameFromTemplate(pAsset, null, null, pForceLegacy: true).ToLowerInvariant());
		}
		float realtimeSinceStartup2 = Time.realtimeSinceStartup;
		float realtimeSinceStartup3 = Time.realtimeSinceStartup;
		for (int j = 0; j < pRuns; j++)
		{
			hashSet2.Add(NameGenerator.generateNameFromTemplate(pAsset).ToLowerInvariant());
		}
		float realtimeSinceStartup4 = Time.realtimeSinceStartup;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (string item10 in hashSet)
		{
			if (hashSet2.Contains(item10))
			{
				num3++;
				hashSet3.Add(item10);
			}
			else
			{
				num++;
			}
		}
		foreach (string item11 in hashSet2)
		{
			if (!hashSet.Contains(item11))
			{
				num2++;
			}
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num4 = int.MaxValue;
		int num5 = 0;
		foreach (string item12 in hashSet)
		{
			int length = item12.Length;
			if (length < num4)
			{
				num4 = length;
			}
			if (length > num5)
			{
				num5 = length;
			}
		}
		int num6 = int.MaxValue;
		int num7 = 0;
		foreach (string item13 in hashSet2)
		{
			int length2 = item13.Length;
			if (length2 < num6)
			{
				num6 = length2;
			}
			if (length2 > num7)
			{
				num7 = length2;
			}
		}
		int num8 = Mathf.Min(num4, num6);
		int num9 = Mathf.Max(num5, num7);
		for (int k = num8; k <= num9; k++)
		{
			dictionary[k] = 0;
			dictionary2[k] = 0;
		}
		foreach (string item14 in hashSet)
		{
			dictionary[item14.Length]++;
		}
		foreach (string item15 in hashSet2)
		{
			dictionary2[item15.Length]++;
		}
		float num10 = 100f * (float)num / (float)hashSet.Count;
		float num11 = 100f * (float)num2 / (float)hashSet2.Count;
		float num12 = 100f * (float)num3 / (float)hashSet.Count;
		string text2 = ((num10 < 25f) ? ("<color=green>" + num10.ToString("F2") + "%</color>") : ((num10 < 70f) ? ("<color=orange>" + num10.ToString("F2") + "%</color>") : ("<color=red>" + num10.ToString("F2") + "%</color>")));
		string text3 = ((num11 < 25f) ? ("<color=green>" + num11.ToString("F2") + "%</color>") : ((num11 < 70f) ? ("<color=orange>" + num11.ToString("F2") + "%</color>") : ("<color=red>" + num11.ToString("F2") + "%</color>")));
		string text4 = ((num12 < 25f) ? ("<color=red>" + num12.ToString("F2") + "%</color>") : ((num12 < 70f) ? ("<color=orange>" + num12.ToString("F2") + "%</color>") : ("<color=green>" + num12.ToString("F2") + "%</color>")));
		using ListPool<string[]> listPool = new ListPool<string[]>();
		listPool.Add(new string[2]
		{
			"Unique " + pNameAssetID + " :",
			pRuns + " runs"
		});
		listPool.Add(new string[4]
		{
			"Legacy :",
			hashSet.Count.ToString() ?? "",
			100 * hashSet.Count / hashSet2.Count + "%",
			(realtimeSinceStartup2 - realtimeSinceStartup).ToString("F2") + "s"
		});
		listPool.Add(new string[4]
		{
			"Ono :",
			hashSet2.Count.ToString() ?? "",
			100 * hashSet2.Count / hashSet.Count + "%",
			(realtimeSinceStartup4 - realtimeSinceStartup3).ToString("F2") + "s"
		});
		listPool.Add(new string[3]
		{
			"names only in legacy :",
			num.ToString() ?? "",
			text2
		});
		listPool.Add(new string[3]
		{
			"names only in ono :",
			num2.ToString() ?? "",
			text3
		});
		listPool.Add(new string[3]
		{
			"names in both :",
			num3.ToString() ?? "",
			text4
		});
		string text5 = ((num4 < num6) ? ("<color=red>" + num4 + "</color>") : num4.ToString());
		string text6 = ((num6 < num4) ? ("<color=red>" + num6 + "</color>") : num6.ToString());
		string text7 = ((num5 > num7) ? ("<color=red>" + num5 + "</color>") : num5.ToString());
		string text8 = ((num7 > num5) ? ("<color=red>" + num7 + "</color>") : num7.ToString());
		listPool.Add(new string[2]
		{
			"min/max len legacy :",
			text5 + "-" + text7
		});
		listPool.Add(new string[2]
		{
			"min/max len ono :",
			text6 + "-" + text8
		});
		text = text + "\n" + Toolbox.printRows(listPool);
		listPool.Clear();
		string[] second = dictionary.Select((KeyValuePair<int, int> p) => p.Key.ToString()).ToArray();
		string[] second2 = dictionary.Select((KeyValuePair<int, int> p) => p.Value.ToString()).ToArray();
		string[] second3 = dictionary2.Select((KeyValuePair<int, int> p) => p.Value.ToString()).ToArray();
		string[] item = new string[1] { "len dist" }.Concat(second).ToArray();
		string[] item2 = new string[1] { "legacy :" }.Concat(second2).ToArray();
		string[] item3 = new string[1] { "ono :" }.Concat(second3).ToArray();
		listPool.Add(item);
		listPool.Add(item2);
		listPool.Add(item3);
		text = text + "\n" + Toolbox.printRows(listPool);
		HashSet<string> hashSet4 = UnsafeCollectionPool<HashSet<string>, string>.Get();
		hashSet4.UnionWith(hashSet);
		hashSet4.ExceptWith(hashSet2);
		using ListPool<string> listPool2 = new ListPool<string>(hashSet4);
		listPool2.Sort();
		using ListPool<string> listPool3 = new ListPool<string>(91);
		using ListPool<string> listPool4 = new ListPool<string>(91);
		using ListPool<string> listPool5 = new ListPool<string>(91);
		if (listPool2.Count > 0)
		{
			listPool3.Add("Legacy");
			(string, string) tuple = findShortestLongest(listPool2);
			string item4 = tuple.Item1;
			string item5 = tuple.Item2;
			for (int num13 = 0; num13 < Mathf.Min(listPool2.Count, 30); num13++)
			{
				listPool3.Add(listPool2.Shift());
			}
			for (int num14 = 0; num14 < Mathf.Min(listPool2.Count, 30); num14++)
			{
				listPool3.Insert(Mathf.Min(31, listPool3.Count), listPool2.Pop());
			}
			int num15 = Mathf.Max(listPool2.Count / 2 - 15, 0);
			for (int num16 = 0; num16 < Mathf.Min(listPool2.Count, 30); num16++)
			{
				listPool3.Insert(Mathf.Min(30 + num16 + 1, listPool3.Count), listPool2[num16 + num15]);
			}
			listPool3.Add(Toolbox.fillLeft("", item5.Length, '='));
			listPool3.Add("Min/Max");
			listPool3.Add(Toolbox.fillLeft("", item5.Length, '='));
			listPool3.Add(item5);
			listPool3.Add(item4);
		}
		HashSet<string> hashSet5 = UnsafeCollectionPool<HashSet<string>, string>.Get();
		hashSet5.UnionWith(hashSet2);
		hashSet5.ExceptWith(hashSet);
		using ListPool<string> listPool6 = new ListPool<string>(hashSet5);
		listPool6.Sort();
		if (listPool6.Count > 0)
		{
			listPool4.Add("Ono");
			(string, string) tuple2 = findShortestLongest(listPool6);
			string item6 = tuple2.Item1;
			string item7 = tuple2.Item2;
			for (int num17 = 0; num17 < Mathf.Min(listPool6.Count, 30); num17++)
			{
				listPool4.Add(listPool6.Shift());
			}
			for (int num18 = 0; num18 < Mathf.Min(listPool6.Count, 30); num18++)
			{
				listPool4.Insert(Mathf.Min(31, listPool4.Count), listPool6.Pop());
			}
			int num19 = Mathf.Max(listPool6.Count / 2 - 15, 0);
			for (int num20 = 0; num20 < Mathf.Min(listPool6.Count, 30); num20++)
			{
				listPool4.Insert(Mathf.Min(30 + num20 + 1, listPool4.Count), listPool6[num20 + num19]);
			}
			listPool4.Add(Toolbox.fillLeft("", item7.Length, '='));
			listPool4.Add("Min/Max");
			listPool4.Add(Toolbox.fillLeft("", item7.Length, '='));
			listPool4.Add(item7);
			listPool4.Add(item6);
		}
		using ListPool<string> listPool7 = new ListPool<string>(hashSet3);
		listPool7.Sort();
		if (listPool7.Count > 0)
		{
			listPool5.Add("Both");
			(string, string) tuple3 = findShortestLongest(listPool7);
			string item8 = tuple3.Item1;
			string item9 = tuple3.Item2;
			for (int num21 = 0; num21 < Mathf.Min(listPool7.Count, 30); num21++)
			{
				listPool5.Add(listPool7.Shift());
			}
			for (int num22 = 0; num22 < Mathf.Min(listPool7.Count, 30); num22++)
			{
				listPool5.Insert(Mathf.Min(31, listPool7.Count), listPool7.Pop());
			}
			int num23 = Mathf.Max(listPool7.Count / 2 - 15, 0);
			for (int num24 = 0; num24 < Mathf.Min(listPool7.Count, 30); num24++)
			{
				listPool5.Insert(Mathf.Min(30 + num24 + 1, listPool7.Count), listPool7[num24 + num23]);
			}
			listPool5.Add(Toolbox.fillLeft("", item9.Length, '='));
			listPool5.Add("Min/Max");
			listPool5.Add(Toolbox.fillLeft("", item9.Length, '='));
			listPool5.Add(item9);
			listPool5.Add(item8);
		}
		text = text + "\n" + Toolbox.printColumns(listPool3, listPool4, listPool5);
		UnsafeCollectionPool<HashSet<string>, string>.Release(hashSet4);
		UnsafeCollectionPool<HashSet<string>, string>.Release(hashSet5);
		UnsafeCollectionPool<HashSet<string>, string>.Release(hashSet);
		UnsafeCollectionPool<HashSet<string>, string>.Release(hashSet2);
		UnsafeCollectionPool<HashSet<string>, string>.Release(hashSet3);
		return text;
	}

	private static (string, string) findShortestLongest(ListPool<string> pHashSet)
	{
		string item = null;
		string item2 = null;
		int num = int.MinValue;
		int num2 = int.MaxValue;
		foreach (ref string item3 in pHashSet)
		{
			string current = item3;
			int length = current.Length;
			if (length > num)
			{
				num = length;
				item = current;
			}
			if (length < num2)
			{
				num2 = length;
				item2 = current;
			}
		}
		return (item2, item);
	}

	public static string writeResults(string pFilename, string pResults)
	{
		File.WriteAllText(Application.persistentDataPath + "/" + pFilename, pResults);
		Debug.Log((object)("Written result to " + pFilename + " in " + Application.persistentDataPath));
		return pResults;
	}
}
