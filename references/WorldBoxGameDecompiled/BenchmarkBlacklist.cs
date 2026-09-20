using System.Collections.Generic;
using UnityEngine;

public static class BenchmarkBlacklist
{
	private static List<WorldTile> _test_world_tiles = new List<WorldTile>();

	private static HashSet<WorldTile> _test_hashset = new HashSet<WorldTile>();

	private static WorldTile[] _test_world_tiles_arr;

	private static List<string> _names = new List<string>();

	private static HashSet<string> _names_set = new HashSet<string>();

	private static int _runs = 0;

	private static int _max = 250;

	private static HashSet<string> _good_words = new HashSet<string>();

	private static HashSet<string> _bad_words = new HashSet<string>();

	private static HashSet<string> _result_good_words = new HashSet<string>();

	private static HashSet<string> _result_bad_words = new HashSet<string>();

	public static void start()
	{
		if (_runs-- <= 0)
		{
			_names.Clear();
			_names_set.Clear();
			_max = Randy.randomInt(50, 255);
			_runs = 5;
		}
		if (_names.Count == 0)
		{
			_good_words.Clear();
			_bad_words.Clear();
			_names_set.Clear();
			Blacklist.init();
			BlacklistTest.init();
			BlacklistTest2.init();
			BlacklistTest3.init();
			BlacklistTest4.init();
			BlacklistTest5.init();
			BlacklistTest6.init();
			BlacklistTest7.init();
			BlacklistTest8.init();
			BlacklistTest9.init();
			BlacklistTest10.init();
			BlacklistTest11.init();
			BlacklistTest12.init();
			BlacklistTest13.init();
			AssetManager.name_generator.list.Shuffle();
			foreach (NameGeneratorAsset item in AssetManager.name_generator.list)
			{
				if (_names_set.Count > _max)
				{
					break;
				}
				for (int i = 0; i < 150; i++)
				{
					string text = NameGenerator.generateNameFromTemplate(item);
					if (string.IsNullOrEmpty(text))
					{
						Debug.LogError((object)("name generator returned null or empty string " + item.id));
						continue;
					}
					_names_set.Add(text);
					if (_names_set.Count > _max)
					{
						break;
					}
				}
			}
			_names.AddRange(_names_set);
			bool flag = false;
			bool flag2 = false;
			for (int j = 0; j < _names.Count; j++)
			{
				if (!Blacklist.checkBlackList(_names[j]))
				{
					flag = true;
					_good_words.Add(_names[j]);
				}
				else
				{
					flag2 = true;
					_bad_words.Add(_names[j]);
				}
			}
			if (!flag || !flag2)
			{
				_runs = 0;
				start();
			}
			Debug.Log((object)("Unique names for test " + _names.Count + " / " + _max + " => " + _good_words.Count + " / " + _bad_words.Count));
		}
		_result_good_words.Clear();
		_result_bad_words.Clear();
		Bench.bench("blacklist_test", "blacklist_test_total");
		Bench.bench("current_blacklist_good", "blacklist_test");
		int num = 0;
		for (int k = 0; k < _names.Count; k++)
		{
			if (!Blacklist.checkBlackList(_names[k]))
			{
				num++;
				_result_good_words.Add(_names[k]);
			}
		}
		Bench.benchEnd("current_blacklist_good", "blacklist_test", pSaveCounter: true, num);
		Bench.bench("current_blacklist_bad", "blacklist_test");
		int num2 = 0;
		for (int l = 0; l < _names.Count; l++)
		{
			if (Blacklist.checkBlackList(_names[l]))
			{
				num2++;
				_result_bad_words.Add(_names[l]);
			}
		}
		Bench.benchEnd("current_blacklist_bad", "blacklist_test", pSaveCounter: true, num2);
		checkResult("current_blacklist_bad");
		Bench.bench("three_blacklist_good_9", "blacklist_test");
		int num3 = 0;
		for (int m = 0; m < _names.Count; m++)
		{
			if (!BlacklistTest9.checkBlackList(_names[m]))
			{
				num3++;
				_result_good_words.Add(_names[m]);
			}
		}
		Bench.benchEnd("three_blacklist_good_9", "blacklist_test", pSaveCounter: true, num3);
		Bench.bench("three_blacklist_bad_9", "blacklist_test");
		int num4 = 0;
		for (int n = 0; n < _names.Count; n++)
		{
			if (BlacklistTest9.checkBlackList(_names[n]))
			{
				num4++;
				_result_bad_words.Add(_names[n]);
			}
		}
		Bench.benchEnd("three_blacklist_bad_9", "blacklist_test", pSaveCounter: true, num4);
		checkResult("three_blacklist_bad_9");
		Bench.bench("old_blacklist_good_10", "blacklist_test");
		int num5 = 0;
		for (int num6 = 0; num6 < _names.Count; num6++)
		{
			if (!BlacklistTest10.checkBlackList(_names[num6]))
			{
				num5++;
				_result_good_words.Add(_names[num6]);
			}
		}
		Bench.benchEnd("old_blacklist_good_10", "blacklist_test", pSaveCounter: true, num5);
		Bench.bench("old_blacklist_bad_10", "blacklist_test");
		int num7 = 0;
		for (int num8 = 0; num8 < _names.Count; num8++)
		{
			if (BlacklistTest10.checkBlackList(_names[num8]))
			{
				num7++;
				_result_bad_words.Add(_names[num8]);
			}
		}
		Bench.benchEnd("old_blacklist_bad_10", "blacklist_test", pSaveCounter: true, num7);
		checkResult("old_blacklist_bad_10");
		Bench.bench("slice_blacklist_good_11", "blacklist_test");
		int num9 = 0;
		for (int num10 = 0; num10 < _names.Count; num10++)
		{
			if (!BlacklistTest11.checkBlackList(_names[num10]))
			{
				num9++;
				_result_good_words.Add(_names[num10]);
			}
		}
		Bench.benchEnd("slice_blacklist_good_11", "blacklist_test", pSaveCounter: true, num9);
		Bench.bench("slice_blacklist_bad_11", "blacklist_test");
		int num11 = 0;
		for (int num12 = 0; num12 < _names.Count; num12++)
		{
			if (BlacklistTest11.checkBlackList(_names[num12]))
			{
				num11++;
				_result_bad_words.Add(_names[num12]);
			}
		}
		Bench.benchEnd("slice_blacklist_bad_11", "blacklist_test", pSaveCounter: true, num11);
		checkResult("slice_blacklist_bad_11");
		Bench.bench("ref_blacklist_good_12", "blacklist_test");
		int num13 = 0;
		for (int num14 = 0; num14 < _names.Count; num14++)
		{
			if (!BlacklistTest12.checkBlackList(_names[num14]))
			{
				num13++;
				_result_good_words.Add(_names[num14]);
			}
		}
		Bench.benchEnd("ref_blacklist_good_12", "blacklist_test", pSaveCounter: true, num13);
		Bench.bench("ref_blacklist_bad_12", "blacklist_test");
		int num15 = 0;
		for (int num16 = 0; num16 < _names.Count; num16++)
		{
			if (BlacklistTest12.checkBlackList(_names[num16]))
			{
				num15++;
				_result_bad_words.Add(_names[num16]);
			}
		}
		Bench.benchEnd("ref_blacklist_bad_12", "blacklist_test", pSaveCounter: true, num15);
		checkResult("ref_blacklist_bad_12");
		Bench.bench("idx_blacklist_good_13", "blacklist_test");
		int num17 = 0;
		for (int num18 = 0; num18 < _names.Count; num18++)
		{
			if (!BlacklistTest13.checkBlackList(_names[num18]))
			{
				num17++;
				_result_good_words.Add(_names[num18]);
			}
		}
		Bench.benchEnd("idx_blacklist_good_13", "blacklist_test", pSaveCounter: true, num17);
		Bench.bench("idx_blacklist_bad_13", "blacklist_test");
		int num19 = 0;
		for (int num20 = 0; num20 < _names.Count; num20++)
		{
			if (BlacklistTest13.checkBlackList(_names[num20]))
			{
				num19++;
				_result_bad_words.Add(_names[num20]);
			}
		}
		Bench.benchEnd("idx_blacklist_bad_13", "blacklist_test", pSaveCounter: true, num19);
		checkResult("idx_blacklist_bad_13");
		Bench.benchEnd("blacklist_test", "blacklist_test_total", pSaveCounter: false, 0L);
	}

	public static void checkResult(string pBenchmarkName)
	{
		if (_result_good_words.Count != _good_words.Count || _result_bad_words.Count != _bad_words.Count)
		{
			Debug.LogError((object)(pBenchmarkName + ": Blacklist check failed " + _result_good_words.Count + " " + _good_words.Count + " " + _result_bad_words.Count + " " + _bad_words.Count));
			foreach (string result_good_word in _result_good_words)
			{
				if (!_good_words.Contains(result_good_word))
				{
					Debug.LogError((object)(pBenchmarkName + ": Missing good word: " + result_good_word));
				}
			}
			foreach (string result_bad_word in _result_bad_words)
			{
				if (!_bad_words.Contains(result_bad_word))
				{
					Debug.LogError((object)(pBenchmarkName + ": Missing bad word: " + result_bad_word));
				}
			}
			foreach (string good_word in _good_words)
			{
				if (!_result_good_words.Contains(good_word))
				{
					Debug.LogError((object)(pBenchmarkName + ": Extra good word: " + good_word));
				}
			}
			foreach (string bad_word in _bad_words)
			{
				if (!_result_bad_words.Contains(bad_word))
				{
					Debug.LogError((object)(pBenchmarkName + ": Extra bad word: " + bad_word));
				}
			}
		}
		_result_good_words.Clear();
		_result_bad_words.Clear();
	}
}
