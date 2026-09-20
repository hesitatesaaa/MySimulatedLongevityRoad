using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Pool;

public static class ListExtensions
{
	private static Random rnd => Randy.rnd;

	public static string ToJson(this List<string> list)
	{
		if (list.Count == 0)
		{
			return "[]";
		}
		return "['" + string.Join("','", list) + "']";
	}

	public static void ShuffleHalf<T>(this List<T> list)
	{
		if (list.Count >= 2)
		{
			int count = list.Count;
			int num = count / 2 + 1;
			for (int i = 0; i < num && i < count; i += 2)
			{
				list.Swap(i, rnd.Next(i, count));
			}
		}
	}

	public static void ShuffleN<T>(this List<T> list, int pItems)
	{
		if (list.Count >= 2)
		{
			int num = ((list.Count < pItems) ? list.Count : pItems);
			for (int i = 0; i < num; i++)
			{
				list.Swap(i, rnd.Next(i, num));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Shuffle<T>(this List<T> list)
	{
		if (list.Count >= 2)
		{
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				list.Swap(i, rnd.Next(i, count));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ShuffleOne<T>(this List<T> list)
	{
		if (list.Count >= 2)
		{
			list.Swap(0, rnd.Next(0, list.Count));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ShuffleOne<T>(this List<T> list, int nItem)
	{
		if (list.Count >= 2 && list.Count >= nItem + 1)
		{
			list.Swap(nItem, rnd.Next(nItem, list.Count));
		}
	}

	public static void ShuffleLast<T>(this List<T> list)
	{
		if (list.Count >= 2)
		{
			list.Swap(list.Count - 1, rnd.Next(0, list.Count));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Pop<T>(this List<T> list)
	{
		T result = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Shift<T>(this List<T> list)
	{
		T result = list[0];
		list.RemoveAt(0);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T First<T>(this List<T> list)
	{
		return list[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T Last<T>(this List<T> list)
	{
		return list[list.Count - 1];
	}

	public static void ShuffleRandomOne<T>(this List<T> list)
	{
		if (list.Count >= 2)
		{
			int num = Randy.randomInt(0, list.Count - 1);
			list.Swap(num, rnd.Next(num, list.Count));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Swap<T>(this List<T> list, int i, int j)
	{
		T value = list[i];
		list[i] = list[j];
		list[j] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T GetRandom<T>(this List<T> list)
	{
		return list[rnd.Next(0, list.Count)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RemoveAtSwapBack<T>(this List<T> list, T pObject)
	{
		int num = list.IndexOf(pObject);
		if (num != -1)
		{
			int index = list.Count - 1;
			list[num] = list[index];
			list[index] = pObject;
			list.RemoveAt(index);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Any<T>(this List<T> list)
	{
		if (list == null)
		{
			return false;
		}
		return list.Count > 0;
	}

	[Pure]
	public static bool SetEquals<T>(this List<T> pList, IEnumerable<T> pOther)
	{
		if (pList == null || pOther == null)
		{
			return false;
		}
		HashSet<T> hashSet = CollectionPool<HashSet<T>, T>.Get();
		HashSet<T> hashSet2 = CollectionPool<HashSet<T>, T>.Get();
		hashSet.UnionWith(pList);
		hashSet2.UnionWith(pOther);
		bool result = hashSet.SetEquals(hashSet2);
		hashSet2.Clear();
		hashSet.Clear();
		CollectionPool<HashSet<T>, T>.Release(hashSet);
		CollectionPool<HashSet<T>, T>.Release(hashSet2);
		return result;
	}

	public static string ToLineString<T>(this List<T> pList, string pSeparator = ",")
	{
		if (pList == null)
		{
			return string.Empty;
		}
		return string.Join(pSeparator, pList);
	}

	public static void PrintToConsole<T>(this List<T> pList)
	{
		if (pList != null)
		{
			Debug.Log((object)pList.ToLineString());
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddTimes<T>(this List<T> pList, int pAmount, T pObject)
	{
		for (int i = 0; i < pAmount; i++)
		{
			pList.Add(pObject);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T LoopNext<T>(this List<T> pList, T pObject)
	{
		int num = pList.IndexOf(pObject);
		if (num == -1)
		{
			return pObject;
		}
		num++;
		if (num >= pList.Count)
		{
			num = 0;
		}
		return pList[num];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this List<T> pList)
	{
		ListAccessHelper.ListDataHelper<T> obj = UnsafeUtility.As<List<T>, ListAccessHelper.ListDataHelper<T>>(ref pList);
		int size = obj._size;
		T[] items = obj._items;
		if ((uint)size > (uint)items.Length)
		{
			throw new InvalidOperationException("Concurrent operations are not supported.");
		}
		return new Span<T>(items, 0, size);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> pList)
	{
		ListAccessHelper.ListDataHelper<T> obj = UnsafeUtility.As<List<T>, ListAccessHelper.ListDataHelper<T>>(ref pList);
		int size = obj._size;
		T[] items = obj._items;
		if ((uint)size > (uint)items.Length)
		{
			throw new InvalidOperationException("Concurrent operations are not supported.");
		}
		return new ReadOnlySpan<T>(items, 0, size);
	}

	public static string AsString<T>(this List<T> pList)
	{
		return pList.ToArray().AsString();
	}
}
