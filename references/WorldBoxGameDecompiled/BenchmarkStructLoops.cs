using System;
using System.Collections.Generic;

public static class BenchmarkStructLoops
{
	private static List<WorldTileDataStruct> _test_world_tiles = new List<WorldTileDataStruct>();

	private static ListPool<WorldTileDataStruct> _test_world_tiles_pool;

	private static HashSet<WorldTileDataStruct> _test_hashset = new HashSet<WorldTileDataStruct>();

	private static WorldTileDataStruct[] _test_world_tiles_arr;

	private static int _runs = 0;

	public static void start()
	{
		int count = _test_world_tiles.Count;
		if (_runs++ > 30 || _test_world_tiles_arr == null)
		{
			_runs = 0;
			_test_world_tiles_pool?.Dispose();
			_test_world_tiles.Clear();
			_test_hashset.Clear();
			int num = Randy.randomInt(1, 5);
			int num2 = World.world.tiles_list.Length;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					WorldTile worldTile = World.world.tiles_list[j];
					int pTileID = worldTile.data.tile_id + i * num2;
					_test_world_tiles.Add(new WorldTileDataStruct(worldTile, pTileID));
				}
				_test_world_tiles.Shuffle();
			}
			_test_hashset.UnionWith(_test_world_tiles);
			_test_world_tiles_pool = new ListPool<WorldTileDataStruct>(_test_world_tiles);
			_test_world_tiles_arr = _test_world_tiles.ToArray();
		}
		Bench.bench("loops_struct_test", "loops_struct_test_total");
		Bench.bench("list_for", "loops_struct_test");
		int num3 = 0;
		count = 0;
		for (int k = 0; k < _test_world_tiles.Count; k++)
		{
			num3 += _test_world_tiles[k].tile_id;
			count++;
		}
		Bench.benchEnd("list_for", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_for", "loops_struct_test");
		num3 = 0;
		count = 0;
		for (int l = 0; l < _test_world_tiles_pool.Count; l++)
		{
			num3 += _test_world_tiles_pool[l].tile_id;
			count++;
		}
		Bench.benchEnd("listpool_for", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_span_for", "loops_struct_test");
		num3 = 0;
		count = 0;
		Span<WorldTileDataStruct> span = _test_world_tiles_pool.AsSpan();
		for (int m = 0; m < span.Length; m++)
		{
			WorldTileDataStruct worldTileDataStruct = span[m];
			num3 += worldTileDataStruct.tile_id;
			count++;
		}
		Bench.benchEnd("listpool_span_for", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("hashset_foreach", "loops_struct_test");
		num3 = 0;
		count = 0;
		foreach (WorldTileDataStruct item in _test_hashset)
		{
			num3 += item.tile_id;
			count++;
		}
		Bench.benchEnd("hashset_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("list_for_local", "loops_struct_test");
		num3 = 0;
		count = 0;
		List<WorldTileDataStruct> test_world_tiles = _test_world_tiles;
		for (int n = 0; n < test_world_tiles.Count; n++)
		{
			num3 += test_world_tiles[n].tile_id;
			count++;
		}
		Bench.benchEnd("list_for_local", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_for_local", "loops_struct_test");
		num3 = 0;
		count = 0;
		ListPool<WorldTileDataStruct> test_world_tiles_pool = _test_world_tiles_pool;
		for (int num4 = 0; num4 < test_world_tiles_pool.Count; num4++)
		{
			num3 += test_world_tiles_pool[num4].tile_id;
			count++;
		}
		Bench.benchEnd("listpool_for_local", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_span_for_local", "loops_struct_test");
		num3 = 0;
		count = 0;
		Span<WorldTileDataStruct> span2 = _test_world_tiles_pool.AsSpan();
		for (int num5 = 0; num5 < span2.Length; num5++)
		{
			WorldTileDataStruct worldTileDataStruct2 = span2[num5];
			num3 += worldTileDataStruct2.tile_id;
			count++;
		}
		Bench.benchEnd("listpool_span_for_local", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("list_for_local_len", "loops_struct_test");
		num3 = 0;
		count = 0;
		test_world_tiles = _test_world_tiles;
		int count2 = test_world_tiles.Count;
		for (int num6 = 0; num6 < count2; num6++)
		{
			num3 += test_world_tiles[num6].tile_id;
			count++;
		}
		Bench.benchEnd("list_for_local_len", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_for_local_len", "loops_struct_test");
		num3 = 0;
		count = 0;
		test_world_tiles_pool = _test_world_tiles_pool;
		count2 = test_world_tiles_pool.Count;
		for (int num7 = 0; num7 < count2; num7++)
		{
			num3 += test_world_tiles_pool[num7].tile_id;
			count++;
		}
		Bench.benchEnd("listpool_for_local_len", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_span_for_local_len", "loops_struct_test");
		num3 = 0;
		count = 0;
		span2 = _test_world_tiles_pool.AsSpan();
		count2 = span2.Length;
		for (int num8 = 0; num8 < count2; num8++)
		{
			WorldTileDataStruct worldTileDataStruct3 = span2[num8];
			num3 += worldTileDataStruct3.tile_id;
			count++;
		}
		Bench.benchEnd("listpool_span_for_local_len", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("list_foreach", "loops_struct_test");
		num3 = 0;
		count = 0;
		foreach (WorldTileDataStruct test_world_tile in _test_world_tiles)
		{
			num3 += test_world_tile.tile_id;
			count++;
		}
		Bench.benchEnd("list_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_foreach", "loops_struct_test");
		num3 = 0;
		count = 0;
		foreach (ref WorldTileDataStruct item2 in _test_world_tiles_pool)
		{
			WorldTileDataStruct current = item2;
			num3 += current.tile_id;
			count++;
		}
		Bench.benchEnd("listpool_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("listpool_span_foreach", "loops_struct_test");
		num3 = 0;
		count = 0;
		span2 = _test_world_tiles_pool.AsSpan();
		Span<WorldTileDataStruct> span3 = span2;
		for (int num9 = 0; num9 < span3.Length; num9++)
		{
			WorldTileDataStruct worldTileDataStruct4 = span3[num9];
			num3 += worldTileDataStruct4.tile_id;
			count++;
		}
		Bench.benchEnd("listpool_span_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("array_for", "loops_struct_test");
		num3 = 0;
		count = 0;
		for (int num10 = 0; num10 < _test_world_tiles_arr.Length; num10++)
		{
			WorldTileDataStruct worldTileDataStruct5 = _test_world_tiles_arr[num10];
			num3 += worldTileDataStruct5.tile_id;
			count++;
		}
		Bench.benchEnd("array_for", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("array_for_local", "loops_struct_test");
		num3 = 0;
		count = 0;
		WorldTileDataStruct[] test_world_tiles_arr = _test_world_tiles_arr;
		for (int num11 = 0; num11 < test_world_tiles_arr.Length; num11++)
		{
			WorldTileDataStruct worldTileDataStruct6 = test_world_tiles_arr[num11];
			num3 += worldTileDataStruct6.tile_id;
			count++;
		}
		Bench.benchEnd("array_for_local", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("array_for_local_len", "loops_struct_test");
		num3 = 0;
		count = 0;
		test_world_tiles_arr = _test_world_tiles_arr;
		count2 = _test_world_tiles_arr.Length;
		for (int num12 = 0; num12 < count2; num12++)
		{
			WorldTileDataStruct worldTileDataStruct7 = test_world_tiles_arr[num12];
			num3 += worldTileDataStruct7.tile_id;
			count++;
		}
		Bench.benchEnd("array_for_local_len", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("array_foreach", "loops_struct_test");
		num3 = 0;
		count = 0;
		WorldTileDataStruct[] test_world_tiles_arr2 = _test_world_tiles_arr;
		for (int num9 = 0; num9 < test_world_tiles_arr2.Length; num9++)
		{
			WorldTileDataStruct worldTileDataStruct8 = test_world_tiles_arr2[num9];
			num3 += worldTileDataStruct8.tile_id;
			count++;
		}
		Bench.benchEnd("array_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("ro_span_foreach", "loops_struct_test");
		ReadOnlySpan<WorldTileDataStruct> readOnlySpan = new ReadOnlySpan<WorldTileDataStruct>(_test_world_tiles_arr);
		num3 = 0;
		count = 0;
		ReadOnlySpan<WorldTileDataStruct> readOnlySpan2 = readOnlySpan;
		for (int num9 = 0; num9 < readOnlySpan2.Length; num9++)
		{
			WorldTileDataStruct worldTileDataStruct9 = readOnlySpan2[num9];
			num3 += worldTileDataStruct9.tile_id;
			count++;
		}
		Bench.benchEnd("ro_span_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("ro_span_for", "loops_struct_test");
		readOnlySpan = new ReadOnlySpan<WorldTileDataStruct>(_test_world_tiles_arr);
		num3 = 0;
		count = 0;
		for (int num13 = 0; num13 < readOnlySpan.Length; num13++)
		{
			WorldTileDataStruct worldTileDataStruct10 = readOnlySpan[num13];
			num3 += worldTileDataStruct10.tile_id;
			count++;
		}
		Bench.benchEnd("ro_span_for", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("span_foreach", "loops_struct_test");
		span2 = new Span<WorldTileDataStruct>(_test_world_tiles_arr);
		num3 = 0;
		count = 0;
		span3 = span2;
		for (int num9 = 0; num9 < span3.Length; num9++)
		{
			WorldTileDataStruct worldTileDataStruct11 = span3[num9];
			num3 += worldTileDataStruct11.tile_id;
			count++;
		}
		Bench.benchEnd("span_foreach", "loops_struct_test", pSaveCounter: true, count);
		Bench.bench("span_for", "loops_struct_test");
		span2 = new Span<WorldTileDataStruct>(_test_world_tiles_arr);
		num3 = 0;
		count = 0;
		for (int num14 = 0; num14 < span2.Length; num14++)
		{
			WorldTileDataStruct worldTileDataStruct12 = span2[num14];
			num3 += worldTileDataStruct12.tile_id;
			count++;
		}
		Bench.benchEnd("span_for", "loops_struct_test", pSaveCounter: true, count);
		Bench.benchEnd("loops_struct_test", "loops_struct_test_total", pSaveCounter: false, 0L);
	}
}
