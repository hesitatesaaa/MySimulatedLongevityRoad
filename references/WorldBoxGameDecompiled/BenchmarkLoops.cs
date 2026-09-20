using System;
using System.Collections.Generic;

public class BenchmarkLoops
{
	private List<WorldTile> _test_world_tiles = new List<WorldTile>();

	private ListPool<WorldTile> _test_world_tiles_pool;

	private HashSet<WorldTile> _test_hashset = new HashSet<WorldTile>();

	private WorldTile[] _test_world_tiles_arr;

	private List<WorldTile> _new_tiles = new List<WorldTile>();

	private int _runs;

	private bool _counter;

	private int _max_amount;

	private DebugToolAsset _asset;

	internal static Dictionary<string, BenchmarkLoops> _benchmarks = new Dictionary<string, BenchmarkLoops>();

	public BenchmarkLoops(DebugToolAsset pAsset, int pMaxAmount)
	{
		if (!_benchmarks.ContainsKey(pAsset.benchmark_group_id))
		{
			_benchmarks.Add(pAsset.benchmark_group_id, this);
			_max_amount = pMaxAmount;
			_asset = pAsset;
		}
	}

	public static void update(DebugToolAsset pAsset)
	{
		_benchmarks[pAsset.benchmark_group_id].run();
	}

	public void run()
	{
		string benchmark_group_id = _asset.benchmark_group_id;
		string benchmark_total_group = _asset.benchmark_total_group;
		int count = _test_world_tiles.Count;
		_counter = Randy.randomBool();
		if (_runs++ > 10 || _test_world_tiles_arr == null)
		{
			_runs = 0;
			_test_world_tiles_pool?.Dispose();
			_test_hashset.Clear();
			_test_world_tiles_arr?.Clear();
			_test_world_tiles.Clear();
			foreach (WorldTile new_tile in _new_tiles)
			{
				new_tile.Dispose();
			}
			_new_tiles.Clear();
			for (int i = 0; i < _max_amount; i++)
			{
				_test_world_tiles.Add(World.world.tiles_list.GetRandom());
			}
			_test_hashset.UnionWith(_test_world_tiles);
			_test_world_tiles_pool = new ListPool<WorldTile>(_test_world_tiles);
			_test_world_tiles_arr = _test_world_tiles.ToArray();
		}
		Bench.bench(benchmark_group_id, benchmark_total_group);
		_test_world_tiles.Shuffle();
		_test_world_tiles.Shuffle();
		_test_world_tiles.Shuffle();
		_test_world_tiles.Shuffle();
		_test_world_tiles_arr.Shuffle();
		_test_world_tiles_arr.Shuffle();
		_test_world_tiles_arr.Shuffle();
		_test_world_tiles_arr.Shuffle();
		_test_world_tiles_pool.Shuffle();
		_test_world_tiles_pool.Shuffle();
		_test_world_tiles_pool.Shuffle();
		_test_world_tiles_pool.Shuffle();
		Bench.bench("list_for_field", benchmark_group_id);
		int num = 0;
		count = 0;
		for (int j = 0; j < _test_world_tiles.Count; j++)
		{
			WorldTile worldTile = _test_world_tiles[j];
			num += worldTile.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_for_field", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_for_field", benchmark_group_id);
		num = 0;
		count = 0;
		for (int k = 0; k < _test_world_tiles_pool.Count; k++)
		{
			WorldTile worldTile2 = _test_world_tiles_pool[k];
			num += worldTile2.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_for_field", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_span_for", benchmark_group_id);
		num = 0;
		count = 0;
		Span<WorldTile> span = _test_world_tiles_pool.AsSpan();
		for (int l = 0; l < span.Length; l++)
		{
			WorldTile worldTile3 = span[l];
			num += worldTile3.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_span_for", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_for_local", benchmark_group_id);
		num = 0;
		count = 0;
		List<WorldTile> test_world_tiles = _test_world_tiles;
		for (int m = 0; m < test_world_tiles.Count; m++)
		{
			WorldTile worldTile4 = test_world_tiles[m];
			num += worldTile4.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_for_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_for_local", benchmark_group_id);
		num = 0;
		count = 0;
		ListPool<WorldTile> test_world_tiles_pool = _test_world_tiles_pool;
		for (int n = 0; n < test_world_tiles_pool.Count; n++)
		{
			WorldTile worldTile5 = test_world_tiles_pool[n];
			num += worldTile5.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_for_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_span_for_local", benchmark_group_id);
		num = 0;
		count = 0;
		Span<WorldTile> span2 = _test_world_tiles_pool.AsSpan();
		for (int num2 = 0; num2 < span2.Length; num2++)
		{
			WorldTile worldTile6 = span2[num2];
			num += worldTile6.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_span_for_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_for_local_len", benchmark_group_id);
		num = 0;
		count = 0;
		test_world_tiles = _test_world_tiles;
		int count2 = test_world_tiles.Count;
		for (int num3 = 0; num3 < count2; num3++)
		{
			WorldTile worldTile7 = test_world_tiles[num3];
			num += worldTile7.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_for_local_len", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_for_local_len", benchmark_group_id);
		num = 0;
		count = 0;
		test_world_tiles_pool = _test_world_tiles_pool;
		count2 = test_world_tiles_pool.Count;
		for (int num4 = 0; num4 < count2; num4++)
		{
			WorldTile worldTile8 = test_world_tiles_pool[num4];
			num += worldTile8.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_for_local_len", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_span_for_local_len", benchmark_group_id);
		num = 0;
		count = 0;
		span2 = _test_world_tiles_pool.AsSpan();
		count2 = span2.Length;
		for (int num5 = 0; num5 < count2; num5++)
		{
			WorldTile worldTile9 = span2[num5];
			num += worldTile9.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_span_for_local_len", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_foreach_field", benchmark_group_id);
		num = 0;
		count = 0;
		foreach (WorldTile test_world_tile in _test_world_tiles)
		{
			num += test_world_tile.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_foreach_field", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_foreach_field", benchmark_group_id);
		num = 0;
		count = 0;
		foreach (ref WorldTile item in _test_world_tiles_pool)
		{
			WorldTile current2 = item;
			num += current2.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_foreach_field", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_foreach_local", benchmark_group_id);
		num = 0;
		count = 0;
		test_world_tiles = _test_world_tiles;
		foreach (WorldTile item2 in test_world_tiles)
		{
			num += item2.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_foreach_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_foreach_local", benchmark_group_id);
		num = 0;
		count = 0;
		test_world_tiles_pool = _test_world_tiles_pool;
		foreach (ref WorldTile item3 in test_world_tiles_pool)
		{
			WorldTile current4 = item3;
			num += current4.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_foreach_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_pool.Shuffle();
		Bench.bench("lpool_span_foreach", benchmark_group_id);
		num = 0;
		count = 0;
		span2 = _test_world_tiles_pool.AsSpan();
		Span<WorldTile> span3 = span2;
		for (int num6 = 0; num6 < span3.Length; num6++)
		{
			WorldTile worldTile10 = span3[num6];
			num += worldTile10.data.tile_id;
			count++;
		}
		Bench.benchEnd("lpool_span_foreach", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_span_for", benchmark_group_id);
		num = 0;
		count = 0;
		span2 = _test_world_tiles.AsSpan();
		for (int num7 = 0; num7 < span2.Length; num7++)
		{
			WorldTile worldTile11 = span2[num7];
			num += worldTile11.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_span_for", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_span_for_new", benchmark_group_id);
		num = 0;
		count = 0;
		Span<WorldTile> span4 = _test_world_tiles.AsSpan();
		for (int num8 = 0; num8 < span4.Length; num8++)
		{
			WorldTile worldTile12 = span4[num8];
			num += worldTile12.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_span_for_new", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_span_foreach", benchmark_group_id);
		num = 0;
		count = 0;
		span2 = _test_world_tiles.AsSpan();
		span3 = span2;
		for (int num6 = 0; num6 < span3.Length; num6++)
		{
			WorldTile worldTile13 = span3[num6];
			num += worldTile13.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_span_foreach", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_span_foreach_new", benchmark_group_id);
		num = 0;
		count = 0;
		span3 = _test_world_tiles.AsSpan();
		for (int num6 = 0; num6 < span3.Length; num6++)
		{
			WorldTile worldTile14 = span3[num6];
			num += worldTile14.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_span_foreach_new", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_rspan_for", benchmark_group_id);
		num = 0;
		count = 0;
		ReadOnlySpan<WorldTile> readOnlySpan = _test_world_tiles.AsReadOnlySpan();
		for (int num9 = 0; num9 < readOnlySpan.Length; num9++)
		{
			WorldTile worldTile15 = readOnlySpan[num9];
			num += worldTile15.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_rspan_for", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_rspan_for_new", benchmark_group_id);
		num = 0;
		count = 0;
		ReadOnlySpan<WorldTile> readOnlySpan2 = _test_world_tiles.AsReadOnlySpan();
		for (int num10 = 0; num10 < readOnlySpan2.Length; num10++)
		{
			WorldTile worldTile16 = readOnlySpan2[num10];
			num += worldTile16.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_rspan_for_new", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_rspan_foreach", benchmark_group_id);
		num = 0;
		count = 0;
		readOnlySpan = _test_world_tiles.AsReadOnlySpan();
		ReadOnlySpan<WorldTile> readOnlySpan3 = readOnlySpan;
		for (int num6 = 0; num6 < readOnlySpan3.Length; num6++)
		{
			WorldTile worldTile17 = readOnlySpan3[num6];
			num += worldTile17.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_rspan_foreach", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles.Shuffle();
		Bench.bench("list_rspan_foreach_new", benchmark_group_id);
		num = 0;
		count = 0;
		readOnlySpan3 = _test_world_tiles.AsReadOnlySpan();
		for (int num6 = 0; num6 < readOnlySpan3.Length; num6++)
		{
			WorldTile worldTile18 = readOnlySpan3[num6];
			num += worldTile18.data.tile_id;
			count++;
		}
		Bench.benchEnd("list_rspan_foreach_new", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_for_field", benchmark_group_id);
		num = 0;
		count = 0;
		for (int num11 = 0; num11 < _test_world_tiles_arr.Length; num11++)
		{
			WorldTile worldTile19 = _test_world_tiles_arr[num11];
			num += worldTile19.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_for_field", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_for_local", benchmark_group_id);
		num = 0;
		count = 0;
		WorldTile[] test_world_tiles_arr = _test_world_tiles_arr;
		foreach (WorldTile worldTile20 in test_world_tiles_arr)
		{
			num += worldTile20.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_for_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_for_local_len", benchmark_group_id);
		num = 0;
		count = 0;
		test_world_tiles_arr = _test_world_tiles_arr;
		count2 = test_world_tiles_arr.Length;
		for (int num13 = 0; num13 < count2; num13++)
		{
			WorldTile worldTile21 = test_world_tiles_arr[num13];
			num += worldTile21.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_for_local_len", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_foreach_field", benchmark_group_id);
		num = 0;
		count = 0;
		WorldTile[] test_world_tiles_arr2 = _test_world_tiles_arr;
		foreach (WorldTile worldTile22 in test_world_tiles_arr2)
		{
			num += worldTile22.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_foreach_field", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_foreach_local", benchmark_group_id);
		num = 0;
		count = 0;
		test_world_tiles_arr = _test_world_tiles_arr;
		test_world_tiles_arr2 = test_world_tiles_arr;
		foreach (WorldTile worldTile23 in test_world_tiles_arr2)
		{
			num += worldTile23.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_foreach_local", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_rspan_foreach", benchmark_group_id);
		readOnlySpan = new ReadOnlySpan<WorldTile>(_test_world_tiles_arr);
		num = 0;
		count = 0;
		readOnlySpan3 = readOnlySpan;
		for (int num6 = 0; num6 < readOnlySpan3.Length; num6++)
		{
			WorldTile worldTile24 = readOnlySpan3[num6];
			num += worldTile24.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_rspan_foreach", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_rspan_for", benchmark_group_id);
		readOnlySpan = new ReadOnlySpan<WorldTile>(_test_world_tiles_arr);
		num = 0;
		count = 0;
		for (int num14 = 0; num14 < readOnlySpan.Length; num14++)
		{
			WorldTile worldTile25 = readOnlySpan[num14];
			num += worldTile25.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_rspan_for", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_span_foreach", benchmark_group_id);
		num = 0;
		count = 0;
		span2 = new Span<WorldTile>(_test_world_tiles_arr);
		span3 = span2;
		for (int num6 = 0; num6 < span3.Length; num6++)
		{
			WorldTile worldTile26 = span3[num6];
			num += worldTile26.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_span_foreach", benchmark_group_id, pSaveCounter: true, num);
		_test_world_tiles_arr.Shuffle();
		Bench.bench("arr_span_for", benchmark_group_id);
		num = 0;
		count = 0;
		span2 = new Span<WorldTile>(_test_world_tiles_arr);
		for (int num15 = 0; num15 < span2.Length; num15++)
		{
			WorldTile worldTile27 = span2[num15];
			num += worldTile27.data.tile_id;
			count++;
		}
		Bench.benchEnd("arr_span_for", benchmark_group_id, pSaveCounter: true, num);
		Bench.benchEnd(benchmark_group_id, benchmark_total_group, pSaveCounter: false, 0L);
	}
}
