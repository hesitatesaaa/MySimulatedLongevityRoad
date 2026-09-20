using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public static class Benchmark
{
	public static void benchHashsetStart()
	{
		for (int i = 0; i < 1000; i++)
		{
			benchObjHashsetCreateVsAdd(5000);
		}
		Debug.Log((object)("- BenchTest - list:" + Bench.getBenchResult("BenchTest - list")));
		Debug.Log((object)("- BenchTest - hashset:" + Bench.getBenchResult("BenchTest - hashset")));
	}

	public static void benchObjHashsetCreateVsAdd(int pAmount = 3000)
	{
		Bench.bench_enabled = true;
		List<BenchObject> list = new List<BenchObject>();
		for (int i = 0; i < pAmount; i++)
		{
			list.Add(new BenchObject());
		}
		int num = 10;
		List<BenchObject> list2 = new List<BenchObject>();
		list2.AddRange(list);
		Bench.bench("BenchTest - list");
		for (int j = 0; j < num; j++)
		{
			BenchObject random = list2.GetRandom();
			list2.Remove(random);
		}
		Bench.benchEnd("BenchTest - list", "main", pSaveCounter: false, 0L);
		HashSet<BenchObject> hashSet = new HashSet<BenchObject>();
		hashSet.UnionWith(list);
		Bench.bench("BenchTest - hashset");
		for (int k = 0; k < num; k++)
		{
			BenchObject random2 = list.GetRandom();
			hashSet.Remove(random2);
		}
		list2.Clear();
		list2.AddRange(hashSet);
		Bench.benchEnd("BenchTest - hashset", "main", pSaveCounter: false, 0L);
	}

	public static void benchObjectsVsData(int pObjects)
	{
		double num = 1000.0;
		Bench.bench_enabled = true;
		int num2 = pObjects;
		Debug.Log((object)"----");
		Debug.Log((object)("NEW BENCH - " + pObjects));
		BenchObject[] array = new BenchObject[num2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new BenchObject();
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int j = 0; (double)j < num; j++)
		{
			for (int k = 0; k < array.Length; k++)
			{
				array[k].update(0f);
			}
		}
		stopwatch.Stop();
		array = new BenchObject[num2];
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = new BenchObject();
		}
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		for (int m = 0; (double)m < num; m++)
		{
			for (int n = 0; n < array.Length; n++)
			{
				array[n].updateMove(0f);
				array[n].updateMove(0f);
				array[n].updateMove(0f);
				array[n].updateMove(0f);
				array[n].updateMove(0f);
			}
		}
		stopwatch2.Stop();
		array = new BenchObject[num2];
		for (int num3 = 0; num3 < array.Length; num3++)
		{
			array[num3] = new BenchObject();
		}
		Stopwatch stopwatch3 = new Stopwatch();
		stopwatch3.Start();
		for (int num4 = 0; (double)num4 < num; num4++)
		{
			foreach (BenchObject obj in array)
			{
				obj.updateMove(0f);
				obj.updateMove(0f);
				obj.updateMove(0f);
				obj.updateMove(0f);
				obj.updateMove(0f);
			}
		}
		stopwatch3.Stop();
		array = new BenchObject[num2];
		for (int num6 = 0; num6 < array.Length; num6++)
		{
			array[num6] = new BenchObject();
		}
		Stopwatch stopwatch4 = new Stopwatch();
		stopwatch4.Start();
		for (int num7 = 0; (double)num7 < num; num7++)
		{
			Parallel.ForEach(array, World.world.parallel_options, delegate(BenchObject pObject)
			{
				pObject.updateMove(0f);
				pObject.updateMove(0f);
				pObject.updateMove(0f);
				pObject.updateMove(0f);
				pObject.updateMove(0f);
			});
		}
		stopwatch4.Stop();
		array = new BenchObject[num2];
		for (int num8 = 0; num8 < array.Length; num8++)
		{
			array[num8] = new BenchObject();
		}
		Stopwatch stopwatch5 = new Stopwatch();
		stopwatch5.Start();
		for (int num9 = 0; (double)num9 < num; num9++)
		{
			for (int num10 = 0; num10 < array.Length; num10++)
			{
				array[num10].derp += 22;
				if (array[num10].derp == 1000)
				{
					array[num10].derp += 10;
					if (array[num10].derp < 10)
					{
						array[num10].derp += 5;
					}
					else
					{
						array[num10].derp -= 5;
					}
				}
			}
			for (int num11 = 0; num11 < array.Length; num11++)
			{
				array[num11].derp += 22;
				if (array[num11].derp == 1000)
				{
					array[num11].derp += 10;
					if (array[num11].derp < 10)
					{
						array[num11].derp += 5;
					}
					else
					{
						array[num11].derp -= 5;
					}
				}
			}
			for (int num12 = 0; num12 < array.Length; num12++)
			{
				array[num12].derp += 22;
				if (array[num12].derp == 1000)
				{
					array[num12].derp += 10;
					if (array[num12].derp < 10)
					{
						array[num12].derp += 5;
					}
					else
					{
						array[num12].derp -= 5;
					}
				}
			}
			for (int num13 = 0; num13 < array.Length; num13++)
			{
				array[num13].derp += 22;
				if (array[num13].derp == 1000)
				{
					array[num13].derp += 10;
					if (array[num13].derp < 10)
					{
						array[num13].derp += 5;
					}
					else
					{
						array[num13].derp -= 5;
					}
				}
			}
			for (int num14 = 0; num14 < array.Length; num14++)
			{
				array[num14].derp += 22;
				if (array[num14].derp == 1000)
				{
					array[num14].derp += 10;
					if (array[num14].derp < 10)
					{
						array[num14].derp += 5;
					}
					else
					{
						array[num14].derp -= 5;
					}
				}
			}
		}
		stopwatch5.Stop();
		array = new BenchObject[num2];
		for (int num15 = 0; num15 < array.Length; num15++)
		{
			array[num15] = new BenchObject();
		}
		Stopwatch stopwatch6 = new Stopwatch();
		stopwatch6.Start();
		for (int num16 = 0; (double)num16 < num; num16++)
		{
			foreach (BenchObject benchObject in array)
			{
				benchObject.derp += 22;
				if (benchObject.derp == 1000)
				{
					benchObject.derp += 10;
					if (benchObject.derp < 10)
					{
						benchObject.derp += 5;
					}
					else
					{
						benchObject.derp -= 5;
					}
				}
			}
			foreach (BenchObject benchObject2 in array)
			{
				benchObject2.derp += 22;
				if (benchObject2.derp == 1000)
				{
					benchObject2.derp += 10;
					if (benchObject2.derp < 10)
					{
						benchObject2.derp += 5;
					}
					else
					{
						benchObject2.derp -= 5;
					}
				}
			}
			foreach (BenchObject benchObject3 in array)
			{
				benchObject3.derp += 22;
				if (benchObject3.derp == 1000)
				{
					benchObject3.derp += 10;
					if (benchObject3.derp < 10)
					{
						benchObject3.derp += 5;
					}
					else
					{
						benchObject3.derp -= 5;
					}
				}
			}
			foreach (BenchObject benchObject4 in array)
			{
				benchObject4.derp += 22;
				if (benchObject4.derp == 1000)
				{
					benchObject4.derp += 10;
					if (benchObject4.derp < 10)
					{
						benchObject4.derp += 5;
					}
					else
					{
						benchObject4.derp -= 5;
					}
				}
			}
			foreach (BenchObject benchObject5 in array)
			{
				benchObject5.derp += 22;
				if (benchObject5.derp == 1000)
				{
					benchObject5.derp += 10;
					if (benchObject5.derp < 10)
					{
						benchObject5.derp += 5;
					}
					else
					{
						benchObject5.derp -= 5;
					}
				}
			}
		}
		stopwatch6.Stop();
		Debug.Log((object)("bench_objects " + (double)stopwatch.ElapsedTicks / num + " 100%"));
		Debug.Log((object)("bench_data " + getResult(stopwatch, stopwatch2, num)));
		Debug.Log((object)("bench_data_index " + getResult(stopwatch, stopwatch5, num)));
		Debug.Log((object)("bench_data_temp " + getResult(stopwatch, stopwatch6, num)));
		Debug.Log((object)("stopwatch_parallel " + getResult(stopwatch, stopwatch4, num)));
		Debug.Log((object)("stopwatch_data_optimized " + getResult(stopwatch, stopwatch3, num)));
	}

	private static string getResult(Stopwatch p1, Stopwatch p2, double pTries)
	{
		double num = (double)p1.ElapsedTicks / pTries;
		double num2 = (double)p2.ElapsedTicks / pTries;
		double num3 = num / num2 * 100.0 - 100.0;
		return num2 + ", " + num3 + "%";
	}

	public static void benchNativeECSAndOOP()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Bench.bench_enabled = true;
		int num = 200000;
		NativeArray<Vector3> val = default(NativeArray<Vector3>);
		val._002Ector(num, (Allocator)3, (NativeArrayOptions)1);
		NativeArray<int> val2 = default(NativeArray<int>);
		val2._002Ector(num, (Allocator)3, (NativeArrayOptions)1);
		NativeArray<int> val3 = default(NativeArray<int>);
		val3._002Ector(num, (Allocator)3, (NativeArrayOptions)1);
		NativeArray<int> val4 = default(NativeArray<int>);
		val4._002Ector(num, (Allocator)3, (NativeArrayOptions)1);
		ActorData[] array = new ActorData[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new ActorData();
		}
		Bench.bench("test_native_vectors");
		for (int j = 0; j < num; j++)
		{
			Vector3 val5 = val[j];
			val5.x = j;
			val5.y = j;
			val[j] = val5;
		}
		for (int k = 0; k < num; k++)
		{
			val4[k] = k;
		}
		Bench.benchEnd("test_native_vectors", "main", pSaveCounter: false, 0L);
		Bench.bench("test_native_x_y");
		for (int l = 0; l < num; l++)
		{
			val2[l] = l;
			val3[l] = l;
		}
		for (int m = 0; m < num; m++)
		{
			val4[m] = m;
		}
		Bench.benchEnd("test_native_x_y", "main", pSaveCounter: false, 0L);
		Bench.bench("test_normal_temp_var");
		for (int n = 0; n < num; n++)
		{
			ActorData obj = array[n];
			obj.x = n;
			obj.y = n;
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			array[num2].health = num2;
		}
		Bench.benchEnd("test_normal_temp_var", "main", pSaveCounter: false, 0L);
		Bench.bench("test_normal_direct");
		for (int num3 = 0; num3 < num; num3++)
		{
			array[num3].x = num3;
			array[num3].y = num3;
		}
		for (int num4 = 0; num4 < num; num4++)
		{
			array[num4].health = num4;
		}
		Bench.benchEnd("test_normal_direct", "main", pSaveCounter: false, 0L);
		Debug.Log((object)"-  - - - - - - ");
		Debug.Log((object)("- BenchTest - test_native_vectors: " + Bench.getBenchResult("test_native_vectors", "main", pAverage: false)));
		Debug.Log((object)("- BenchTest - test_native_x_y: " + Bench.getBenchResult("test_native_x_y", "main", pAverage: false)));
		Debug.Log((object)("- BenchTest - test_normal_temp_var: " + Bench.getBenchResult("test_normal_temp_var", "main", pAverage: false)));
		Debug.Log((object)("- BenchTest - test_normal_direct: " + Bench.getBenchResult("test_normal_direct", "main", pAverage: false)));
		Debug.Log((object)("- BenchTest - test_job_native_vectors: " + Bench.getBenchResult("test_job_native_vectors", "main", pAverage: false)));
		Debug.Log((object)("- BenchTest - test_job_native_xy: " + Bench.getBenchResult("test_job_native_xy", "main", pAverage: false)));
		val.Dispose();
		val2.Dispose();
		val3.Dispose();
		val4.Dispose();
	}

	public static void benchReferenceVsDict()
	{
	}

	public static void testVirtual()
	{
		int num = 1000;
		BenchTest1 benchTest = new BenchTest1();
		BenchTest2 benchTest2 = new BenchTest2();
		Bench.bench("BenchTest - normal");
		for (int i = 0; i < num; i++)
		{
			benchTest.test();
		}
		Bench.benchEnd("BenchTest - normal", "main", pSaveCounter: false, 0L);
		Bench.bench("BenchTest - virtual");
		for (int j = 0; j < num; j++)
		{
			benchTest2.testVirtual();
		}
		Bench.benchEnd("BenchTest - virtual", "main", pSaveCounter: false, 0L);
		Debug.Log((object)"Benchmark:");
		Debug.Log((object)("- BenchTest - normal:" + Bench.getBenchResult("BenchTest - normal")));
		Debug.Log((object)("- BenchTest - virtual:" + Bench.getBenchResult("BenchTest - virtual")));
	}

	public static void testQueue()
	{
		int num = 10000;
		List<TileType> list = new List<TileType>();
		Queue<TileType> queue = new Queue<TileType>();
		LinkedList<TileType> linkedList = new LinkedList<TileType>();
		for (int i = 0; i < num; i++)
		{
			list.Add(new TileType());
			queue.Enqueue(new TileType());
			linkedList.AddLast(new TileType());
		}
		Bench.bench("list");
		for (int j = 0; j < list.Count; j++)
		{
			_ = list[0];
			list.RemoveAt(0);
		}
		Bench.benchEnd("list", "main", pSaveCounter: false, 0L);
		Bench.bench("queue");
		for (int k = 0; k < queue.Count; k++)
		{
			queue.Dequeue();
		}
		Bench.benchEnd("queue", "main", pSaveCounter: false, 0L);
		Bench.bench("linked");
		for (int l = 0; l < linkedList.Count; l++)
		{
			_ = linkedList.First;
			linkedList.RemoveFirst();
		}
		Bench.benchEnd("linked", "main", pSaveCounter: false, 0L);
		Debug.Log((object)("!!!BENCH REMOVE AT 0 " + num));
		Bench.printBenchResult("list");
		Bench.printBenchResult("queue");
		Bench.printBenchResult("linked");
	}

	public static void testRemoveStructs()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		int num = 100;
		int num2 = 500;
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Vector3> list4 = new List<Vector3>();
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		for (int i = 0; i < num2; i++)
		{
			list.Add(new Vector3
			{
				x = Randy.randomInt(0, 1000),
				y = Randy.randomInt(0, 1000),
				z = Randy.randomInt(0, 1000)
			});
		}
		list.Shuffle();
		for (int j = 0; j < num; j++)
		{
			list2.Add(list.GetRandom());
		}
		Bench.bench("remove");
		foreach (Vector3 item in list)
		{
			list3.Add(item);
		}
		for (int k = 0; k < num; k++)
		{
			list3.Remove(list2[k]);
		}
		Bench.benchEnd("remove", "main", pSaveCounter: false, 0L);
		Bench.bench("RemoveAtSwapBack");
		foreach (Vector3 item2 in list)
		{
			list4.Add(item2);
		}
		for (int l = 0; l < num; l++)
		{
			list4.RemoveAtSwapBack(list2[l]);
		}
		Bench.benchEnd("RemoveAtSwapBack", "main", pSaveCounter: false, 0L);
		Bench.benchEnd("remove_native", "main", pSaveCounter: false, 0L);
		Bench.bench("remove_hashset");
		foreach (Vector3 item3 in list)
		{
			hashSet.Add(item3);
		}
		for (int m = 0; m < num; m++)
		{
			hashSet.Remove(list2[m]);
		}
		Bench.benchEnd("remove_hashset", "main", pSaveCounter: false, 0L);
		Debug.Log((object)"Benchmark:");
		Debug.Log((object)("- built-in remove:" + Bench.getBenchResult("remove")));
		Debug.Log((object)("- own RemoveAtSwapBack: " + Bench.getBenchResult("RemoveAtSwapBack")));
		Debug.Log((object)("- native RemoveAtSwapBack: " + Bench.getBenchResult("remove_native")));
		Debug.Log((object)("- remove hashset: " + Bench.getBenchResult("remove_hashset")));
	}

	public static void testCapacity()
	{
		int num = 100;
		int num2 = 100000;
		Bench.bench("new_list");
		List<List<int>> list = new List<List<int>>(num);
		for (int i = 0; i < num; i++)
		{
			List<int> list2 = new List<int>();
			list.Add(list2);
			for (int j = 0; j < num2; j++)
			{
				list2.Add(j);
			}
		}
		Bench.benchEnd("new_list", "main", pSaveCounter: false, 0L);
		Bench.bench("new_list_reused");
		for (int k = 0; k < list.Count; k++)
		{
			List<int> list3 = list[k];
			list3.Clear();
			for (int l = 0; l < num2; l++)
			{
				list3.Add(l);
			}
		}
		Bench.benchEnd("new_list_reused", "main", pSaveCounter: false, 0L);
		Bench.bench("new_list_set_capacity");
		list = new List<List<int>>(num);
		for (int m = 0; m < num; m++)
		{
			List<int> list4 = new List<int>(num2);
			list.Add(list4);
			for (int n = 0; n < num2; n++)
			{
				list4.Add(n);
			}
		}
		Bench.benchEnd("new_list_set_capacity", "main", pSaveCounter: false, 0L);
		Bench.bench("new_list_set_capacity_reused");
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			List<int> list5 = list[num3];
			list5.Clear();
			for (int num4 = 0; num4 < num2; num4++)
			{
				list5.Add(num4);
			}
		}
		Bench.benchEnd("new_list_set_capacity_reused", "main", pSaveCounter: false, 0L);
		Bench.printBenchResult("new_list");
		Bench.printBenchResult("new_list_set_capacity");
		Bench.printBenchResult("new_list_reused");
		Bench.printBenchResult("new_list_set_capacity_reused");
	}
}
