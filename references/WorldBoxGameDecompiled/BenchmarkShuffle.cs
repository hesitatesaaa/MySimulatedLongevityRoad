using System;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkShuffle
{
	public int result;

	internal int max_tiles;

	internal int amount;

	internal string benchmark_total_group_id;

	internal string benchmark_group_id;

	internal List<WorldTile> test_tiles;

	internal bool print_to_console;

	internal static Dictionary<string, BenchmarkShuffle> _benchmarks = new Dictionary<string, BenchmarkShuffle>();

	public BenchmarkShuffle(DebugToolAsset pAsset, int pAmount, int pMaxTiles)
	{
		if (!_benchmarks.ContainsKey(pAsset.benchmark_group_id))
		{
			amount = pAmount;
			max_tiles = pMaxTiles;
			benchmark_total_group_id = pAsset.benchmark_total_group;
			benchmark_group_id = pAsset.benchmark_group_id;
			test_tiles = new List<WorldTile>();
			_benchmarks.Add(pAsset.benchmark_group_id, this);
			setup();
		}
	}

	public static void update(DebugToolAsset pAsset)
	{
		_benchmarks[pAsset.benchmark_group_id].run();
	}

	public void setup()
	{
		if (!Config.game_loaded)
		{
			MapBox.on_world_loaded = (Action)Delegate.Combine(MapBox.on_world_loaded, (Action)delegate
			{
				setup();
			});
			return;
		}
		int num = max_tiles;
		test_tiles.Clear();
		int num2 = Mathf.CeilToInt(Mathf.Sqrt((float)num));
		num2 *= num2;
		using ListPool<WorldTile> list = new ListPool<WorldTile>(World.world.tiles_list);
		list.Shuffle();
		for (int num3 = 0; num3 < num2; num3++)
		{
			test_tiles.Add(list.Pop());
		}
		test_tiles.Shuffle();
	}

	public void run()
	{
		int num = amount;
		string pGroupID = benchmark_total_group_id;
		string text = benchmark_group_id;
		int num2 = 0;
		int num3 = 0;
		List<WorldTile> list = test_tiles;
		for (int num4 = num - 1; num4 >= 0; num4--)
		{
			WorldTile worldTile = list[num4];
			num3 += worldTile.data.tile_id;
			num2++;
		}
		Bench.bench(text, pGroupID);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"no_shuffle_for_{num}", text);
		for (int i = 0; i < num; i++)
		{
			WorldTile worldTile2 = list[i];
			num3 += worldTile2.data.tile_id;
			num2++;
		}
		Bench.benchEnd($"no_shuffle_for_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_all_{num}", text);
		list.Shuffle();
		for (int j = 0; j < num; j++)
		{
			WorldTile worldTile3 = list[j];
			num3 += worldTile3.data.tile_id;
			num2++;
		}
		Bench.benchEnd($"shuffle_all_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_one_new_list_{num}", text);
		ListPool<WorldTile> listPool = new ListPool<WorldTile>(list);
		for (int k = 0; k < num; k++)
		{
			listPool.ShuffleOne(k);
			WorldTile worldTile4 = listPool[k];
			num3 += worldTile4.data.tile_id;
			num2++;
		}
		listPool.Dispose();
		Bench.benchEnd($"shuffle_one_new_list_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_one_{num}", text);
		for (int l = 0; l < num; l++)
		{
			list.ShuffleOne(l);
			WorldTile worldTile5 = list[l];
			num3 += worldTile5.data.tile_id;
			num2++;
		}
		Bench.benchEnd($"shuffle_one_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_for_{num}", text);
		int num5 = Randy.randomInt(0, num);
		int num6 = num + num5;
		for (int m = num5; m < num6; m++)
		{
			int index = m % num;
			WorldTile worldTile6 = list[index];
			num3 += worldTile6.data.tile_id;
			num2++;
		}
		Bench.benchEnd($"shuffle_for_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_2for_{num}", text);
		num5 = Randy.randomInt(0, num);
		for (int n = num5; n < num; n++)
		{
			WorldTile worldTile7 = list[n];
			num3 += worldTile7.data.tile_id;
			num2++;
		}
		for (int num7 = 0; num7 < num5; num7++)
		{
			WorldTile worldTile8 = list[num7];
			num3 += worldTile8.data.tile_id;
			num2++;
		}
		Bench.benchEnd($"shuffle_2for_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_iterator_{num}", text);
		foreach (WorldTile item in list.LoopRandom())
		{
			num3 += item.data.tile_id;
			num2++;
			if (num2 == num)
			{
				break;
			}
		}
		Bench.benchEnd($"shuffle_iterator_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"shuffle_iterator_limit_{num}", text);
		foreach (WorldTile item2 in list.LoopRandom(num))
		{
			num3 += item2.data.tile_id;
			num2++;
		}
		Bench.benchEnd($"shuffle_iterator_limit_{num}", text, pSaveCounter: true, num2);
		test_tiles.Shuffle();
		num3 = 0;
		num2 = 0;
		Bench.bench($"no_shuffle_iterator_{num}", text);
		foreach (WorldTile item3 in list)
		{
			num3 += item3.data.tile_id;
			num2++;
			if (num2 == num)
			{
				break;
			}
		}
		Bench.benchEnd($"no_shuffle_iterator_{num}", text, pSaveCounter: true, num2);
		Bench.benchEnd(text, pGroupID, pSaveCounter: false, 0L);
		if (print_to_console)
		{
			Debug.Log((object)("LAST:\n" + Bench.printableBenchResults(text, false, $"no_shuffle_for_{num}", $"no_shuffle_iterator_{num}", $"shuffle_iterator_{num}", $"shuffle_iterator_limit_{num}", $"shuffle_for_{num}", $"shuffle_2for_{num}", $"shuffle_one_{num}", $"shuffle_one_new_list_{num}", $"shuffle_all_{num}")));
			Debug.Log((object)("AVG:\n" + Bench.printableBenchResults(text, true, $"no_shuffle_for_{num}", $"no_shuffle_iterator_{num}", $"shuffle_iterator_{num}", $"shuffle_iterator_limit_{num}", $"shuffle_for_{num}", $"shuffle_2for_{num}", $"shuffle_one_{num}", $"shuffle_one_new_list_{num}", $"shuffle_all_{num}")));
		}
		result = num3;
	}
}
