using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine.Pool;

public static class ListPoolExtensions
{
	private static Random rnd => Randy.rnd;

	public static string ToJson(this ListPool<string> list)
	{
		if (list.Count == 0)
		{
			return "[]";
		}
		return "['" + string.Join("','", list) + "']";
	}

	public static void ShuffleHalf<T>(this ListPool<T> list)
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

	public static void ShuffleN<T>(this ListPool<T> list, int pItems)
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
	public static void Shuffle<T>(this ListPool<T> list)
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
	public static void ShuffleOne<T>(this ListPool<T> list)
	{
		if (list.Count >= 2)
		{
			list.Swap(0, rnd.Next(0, list.Count));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ShuffleOne<T>(this ListPool<T> list, int nItem)
	{
		if (list.Count >= 2 && list.Count >= nItem + 1)
		{
			list.Swap(nItem, rnd.Next(nItem, list.Count));
		}
	}

	public static void ShuffleLast<T>(this ListPool<T> list)
	{
		if (list.Count >= 2)
		{
			list.Swap(list.Count - 1, rnd.Next(0, list.Count));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Pop<T>(this ListPool<T> list)
	{
		T result = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Shift<T>(this ListPool<T> list)
	{
		T result = list[0];
		list.RemoveAt(0);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T First<T>(this ListPool<T> list)
	{
		return list[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T Last<T>(this ListPool<T> list)
	{
		return list[list.Count - 1];
	}

	public static void ShuffleRandomOne<T>(this ListPool<T> list)
	{
		if (list.Count >= 2)
		{
			int num = Randy.randomInt(0, list.Count - 1);
			list.Swap(num, rnd.Next(num, list.Count));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Swap<T>(this ListPool<T> list, int i, int j)
	{
		T[] rawBuffer = list.GetRawBuffer();
		T val = rawBuffer[i];
		rawBuffer[i] = rawBuffer[j];
		rawBuffer[j] = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T GetRandom<T>(this ListPool<T> list)
	{
		return list[rnd.Next(0, list.Count)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void RemoveAtSwapBack<T>(this ListPool<T> list, T pObject)
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

	[Pure]
	public static T[] ToArray<T>(this ListPool<T> list)
	{
		T[] array = new T[list.Count];
		list.CopyTo(array, 0);
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static bool Any<T>(this ListPool<T> list)
	{
		if (list == null)
		{
			return false;
		}
		return list.Count > 0;
	}

	[Pure]
	public static bool SetEquals<T>(this ListPool<T> pList, IEnumerable<T> pOther)
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddTimes<T>(this ListPool<T> pList, int pAmount, T pObject)
	{
		for (int i = 0; i < pAmount; i++)
		{
			pList.Add(pObject);
		}
	}

	public static int CountAll<T>(this ListPool<T> pList, Predicate<T> pMatch)
	{
		int num = 0;
		for (int i = 0; i < pList.Count; i++)
		{
			if (pMatch(pList[i]))
			{
				num++;
			}
		}
		return num;
	}

	public static IEnumerable<T> Where<T>(this ListPool<T> pList, Func<T, bool> pPredicate)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			if (pPredicate(pList[i]))
			{
				yield return pList[i];
			}
		}
	}

	[Pure]
	public static bool ValuesEqual<T>(this ListPool<T> pList, ListPool<T> pOther)
	{
		if (pList.Count != pOther.Count)
		{
			return false;
		}
		long longHashCode = pList.GetLongHashCode();
		long longHashCode2 = pOther.GetLongHashCode();
		if (longHashCode != longHashCode2)
		{
			return false;
		}
		return true;
	}

	[Pure]
	public static long GetLongHashCode<T>(this ListPool<T> pList)
	{
		long num = 0L;
		foreach (ref T p in pList)
		{
			T current = p;
			num += current.GetHashCode();
		}
		return num;
	}

	public static string AsString<T>(this ListPool<T> pListPool)
	{
		return pListPool.ToArray().AsString();
	}
}
