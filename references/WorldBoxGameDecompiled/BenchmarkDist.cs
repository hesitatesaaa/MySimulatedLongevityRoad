using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BenchmarkDist
{
	public long result;

	internal string benchmark_group_id;

	internal string benchmark_id;

	internal List<WorldTile> test_tiles;

	internal bool print_to_console;

	private static BenchmarkDist _instance;

	public BenchmarkDist()
	{
		if (_instance == null)
		{
			benchmark_group_id = "dist_test_total";
			benchmark_id = "dist_test";
			test_tiles = new List<WorldTile>();
			_instance = this;
			setup();
		}
	}

	public static void update()
	{
		_instance.run();
	}

	public void setup()
	{
		if (!Config.game_loaded)
		{
			MapBox.on_world_loaded = (Action)Delegate.Combine(MapBox.on_world_loaded, (Action)delegate
			{
				setup();
			});
		}
		else
		{
			test_tiles.AddRange(World.world.tiles_list);
			test_tiles.ShuffleHalf();
			test_tiles.RemoveRange(test_tiles.Count / 2, test_tiles.Count / 2);
		}
	}

	public void run()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0779: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0976: Unknown result type (might be due to invalid IL or missing references)
		//IL_0978: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c49: Unknown result type (might be due to invalid IL or missing references)
		string pGroupID = benchmark_group_id;
		string text = benchmark_id;
		int num = 0;
		double num2 = 0.0;
		int num3 = -1;
		int num4 = int.MaxValue;
		float num5 = float.MaxValue;
		List<WorldTile> list = test_tiles;
		list.Shuffle();
		int2[] array = (int2[])(object)new int2[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = new int2(list[i].x, list[i].y);
		}
		float2[] array2 = (float2[])(object)new float2[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			array2[j] = new float2((float)list[j].x, (float)list[j].y);
		}
		NativeArray<int2> val = default(NativeArray<int2>);
		val._002Ector(array, (Allocator)3);
		NativeArray<float2> val2 = default(NativeArray<float2>);
		val2._002Ector(array2, (Allocator)3);
		WorldTile worldTile = list[0];
		Vector2Int pos = worldTile.pos;
		Vector3 posV = worldTile.posV3;
		int2 val3 = default(int2);
		((int2)(ref val3))._002Ector(worldTile.x, worldTile.y);
		float2 val4 = default(float2);
		((float2)(ref val4))._002Ector((float)worldTile.x, (float)worldTile.y);
		Bench.bench(text, pGroupID);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("DistTile", text);
		for (int k = 1; k < list.Count; k++)
		{
			WorldTile pT = list[k];
			float num6 = Toolbox.DistTile(worldTile, pT);
			if (num6 < num5)
			{
				num5 = num6;
				num3 = k;
			}
			num2 += (double)num6;
			num++;
		}
		Bench.benchEnd("DistTile", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("DistVec2", text);
		for (int l = 1; l < list.Count; l++)
		{
			WorldTile worldTile2 = list[l];
			float num7 = Toolbox.DistVec2(pos, worldTile2.pos);
			if (num7 < num5)
			{
				num5 = num7;
				num3 = l;
			}
			num2 += (double)num7;
			num++;
		}
		Bench.benchEnd("DistVec2", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("DistVec3", text);
		for (int m = 1; m < list.Count; m++)
		{
			WorldTile worldTile3 = list[m];
			float num8 = Toolbox.DistVec3(posV, worldTile3.posV3);
			if (num8 < num5)
			{
				num5 = num8;
				num3 = m;
			}
			num2 += (double)num8;
			num++;
		}
		Bench.benchEnd("DistVec3", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("Dist", text);
		for (int n = 1; n < list.Count; n++)
		{
			WorldTile worldTile4 = list[n];
			float num9 = Toolbox.Dist(worldTile.x, worldTile.y, worldTile4.x, worldTile4.y);
			if (num9 < num5)
			{
				num5 = num9;
				num3 = n;
			}
			num2 += (double)num9;
			num++;
		}
		Bench.benchEnd("Dist", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("DistFloat", text);
		for (int num10 = 1; num10 < list.Count; num10++)
		{
			WorldTile worldTile5 = list[num10];
			float num11 = DistFloat(worldTile.x, worldTile.y, worldTile5.x, worldTile5.y);
			if (num11 < num5)
			{
				num5 = num11;
				num3 = num10;
			}
			num2 += (double)num11;
			num++;
		}
		Bench.benchEnd("DistFloat", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("Dist.pos", text);
		for (int num12 = 1; num12 < list.Count; num12++)
		{
			Vector2Int pos2 = list[num12].pos;
			float num13 = Toolbox.Dist(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, ((Vector2Int)(ref pos2)).x, ((Vector2Int)(ref pos2)).y);
			if (num13 < num5)
			{
				num5 = num13;
				num3 = num12;
			}
			num2 += (double)num13;
			num++;
		}
		Bench.benchEnd("Dist.pos", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("FastDistTile", text);
		for (int num14 = 1; num14 < list.Count; num14++)
		{
			WorldTile pT2 = list[num14];
			int num15 = Toolbox.SquaredDistTile(worldTile, pT2);
			if (num15 < num4)
			{
				num4 = num15;
				num3 = num14;
			}
			num2 += (double)num15;
			num++;
		}
		Bench.benchEnd("FastDistTile", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("FastDist", text);
		for (int num16 = 1; num16 < list.Count; num16++)
		{
			WorldTile worldTile6 = list[num16];
			int num17 = Toolbox.SquaredDist(worldTile.x, worldTile.y, worldTile6.x, worldTile6.y);
			if (num17 < num4)
			{
				num4 = num17;
				num3 = num16;
			}
			num2 += (double)num17;
			num++;
		}
		Bench.benchEnd("FastDist", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("FastDistFloat", text);
		for (int num18 = 1; num18 < list.Count; num18++)
		{
			WorldTile worldTile7 = list[num18];
			float num19 = FastDistFloat(worldTile.x, worldTile.y, worldTile7.x, worldTile7.y);
			if (num19 < num5)
			{
				num5 = num19;
				num3 = num18;
			}
			num2 += (double)num19;
			num++;
		}
		Bench.benchEnd("FastDistFloat", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("FastDistVec2", text);
		for (int num20 = 1; num20 < list.Count; num20++)
		{
			WorldTile worldTile8 = list[num20];
			int num21 = Toolbox.SquaredDistVec2(pos, worldTile8.pos);
			if (num21 < num4)
			{
				num4 = num21;
				num3 = num20;
			}
			num2 += (double)num21;
			num++;
		}
		Bench.benchEnd("FastDistVec2", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("FastDistVec3", text);
		for (int num22 = 1; num22 < list.Count; num22++)
		{
			WorldTile worldTile9 = list[num22];
			float num23 = Toolbox.SquaredDistVec3(posV, worldTile9.posV3);
			if (num23 < num5)
			{
				num5 = num23;
				num3 = num22;
			}
			num2 += (double)num23;
			num++;
		}
		Bench.benchEnd("FastDistVec3", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("FastDist.pos", text);
		for (int num24 = 1; num24 < list.Count; num24++)
		{
			Vector2Int pos3 = list[num24].pos;
			float num25 = Toolbox.SquaredDist(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, ((Vector2Int)(ref pos3)).x, ((Vector2Int)(ref pos3)).y);
			if (num25 < num5)
			{
				num5 = num25;
				num3 = num24;
			}
			num2 += (double)num25;
			num++;
		}
		Bench.benchEnd("FastDist.pos", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("distancesq", text);
		for (int num26 = 1; num26 < list.Count; num26++)
		{
			WorldTile worldTile10 = list[num26];
			float num27 = math.distancesq((float)worldTile.x, (float)worldTile10.x) + math.distancesq((float)worldTile.y, (float)worldTile10.y);
			if (num27 < num5)
			{
				num5 = num27;
				num3 = num26;
			}
			num2 += (double)num27;
			num++;
		}
		Bench.benchEnd("distancesq", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("float2", text);
		float2 val5 = default(float2);
		for (int num28 = 1; num28 < list.Count; num28++)
		{
			WorldTile worldTile11 = list[num28];
			((float2)(ref val5))._002Ector((float)worldTile11.x, (float)worldTile11.y);
			float num29 = math.distancesq(val4, val5);
			if (num29 < num5)
			{
				num5 = num29;
				num3 = num28;
			}
			num2 += (double)num29;
			num++;
		}
		Bench.benchEnd("float2", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("int2", text);
		int2 val6 = default(int2);
		for (int num30 = 1; num30 < list.Count; num30++)
		{
			WorldTile worldTile12 = list[num30];
			((int2)(ref val6))._002Ector(worldTile12.x, worldTile12.y);
			float num31 = math.distancesq(float2.op_Implicit(val3), float2.op_Implicit(val6));
			if (num31 < num5)
			{
				num5 = num31;
				num3 = num30;
			}
			num2 += (double)num31;
			num++;
		}
		Bench.benchEnd("int2", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("int2array", text);
		for (int num32 = 1; num32 < array.Length; num32++)
		{
			float num33 = math.distancesq(float2.op_Implicit(val3), float2.op_Implicit(array[num32]));
			if (num33 < num5)
			{
				num5 = num33;
				num3 = num32;
			}
			num2 += (double)num33;
			num++;
		}
		Bench.benchEnd("int2array", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("nint2array", text);
		for (int num34 = 1; num34 < val2.Length; num34++)
		{
			float num35 = math.distancesq(float2.op_Implicit(val3), val2[num34]);
			if (num35 < num5)
			{
				num5 = num35;
				num3 = num34;
			}
			num2 += (double)num35;
			num++;
		}
		Bench.benchEnd("nint2array", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("float2array", text);
		for (int num36 = 1; num36 < array2.Length; num36++)
		{
			float num37 = math.distancesq(val4, array2[num36]);
			if (num37 < num5)
			{
				num5 = num37;
				num3 = num36;
			}
			num2 += (double)num37;
			num++;
		}
		Bench.benchEnd("float2array", text, pSaveCounter: true, list[num3].tile_id);
		num3 = -1;
		num4 = int.MaxValue;
		num5 = float.MaxValue;
		num2 = 0.0;
		num = 0;
		Bench.bench("nfloat2array", text);
		for (int num38 = 1; num38 < val.Length; num38++)
		{
			float num39 = math.distancesq(val4, float2.op_Implicit(val[num38]));
			if (num39 < num5)
			{
				num5 = num39;
				num3 = num38;
			}
			num2 += (double)num39;
			num++;
		}
		Bench.benchEnd("nfloat2array", text, pSaveCounter: true, list[num3].tile_id);
		val.Dispose();
		val2.Dispose();
		Bench.benchEnd(text, pGroupID, pSaveCounter: false, 0L);
		if (print_to_console)
		{
			Debug.Log((object)("LAST:\n" + Bench.printableBenchResults(text, false, "DistTile", "DistVec2", "DistVec3", "Dist", "DistFloat", "Dist.pos", "FastDistTile", "FastDistVec2", "FastDistVec3", "FastDist", "FastDistFloat", "FastDist.pos", "int2", "int2array", "nint2array", "float2", "float2array", "nfloat2array", "distancesq", "job_new", "job_prefill", "pjob_prefill", "BurstDist", "BurstDistFloat", "BurstFastDistFloat", "BurstDist.pos", "BurstFastDist", "BurstFastDist.pos")));
			Debug.Log((object)("AVG:\n" + Bench.printableBenchResults(text, true, "DistTile", "DistVec2", "DistVec3", "Dist", "DistFloat", "Dist.pos", "FastDistTile", "FastDistVec2", "FastDistVec3", "FastDist", "FastDistFloat", "FastDist.pos", "int2", "int2array", "nint2array", "float2", "float2array", "nfloat2array", "distancesq", "job_new", "job_prefill", "pjob_prefill", "BurstDist", "BurstDistFloat", "BurstFastDistFloat", "BurstDist.pos", "BurstFastDist", "BurstFastDist.pos")));
		}
		result = (long)num2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float DistFloat(float x1, float y1, float x2, float y2)
	{
		return Mathf.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float FastDistFloat(float x1, float y1, float x2, float y2)
	{
		return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
	}
}
