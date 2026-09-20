using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public static class ArrayExtensions
{
	private static Random rnd => Randy.rnd;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T First<T>(this T[] pArray)
	{
		return pArray[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T Last<T>(this T[] pArray)
	{
		return pArray[^1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static int IndexOf<T>(this T[] pArray, T pValue)
	{
		return Array.IndexOf(pArray, pValue);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static bool Contains<T>(this T[] pArray, T pValue)
	{
		return Array.IndexOf(pArray, pValue) > -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static int FreeIndex<T>(this T[] pArray)
	{
		return Array.IndexOf(pArray, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T GetRandom<T>(this T[] pArray)
	{
		return pArray[rnd.Next(0, pArray.Length)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T GetRandom<T>(this T[] pArray, int pLength)
	{
		return pArray[rnd.Next(0, pLength)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Swap<T>(this T[] pArray, int pIndex1, int pIndex2)
	{
		T val = pArray[pIndex1];
		pArray[pIndex1] = pArray[pIndex2];
		pArray[pIndex2] = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Shuffle<T>(this T[] pArray)
	{
		if (pArray.Length >= 2)
		{
			int num = pArray.Length;
			for (int i = 0; i < num; i++)
			{
				pArray.Swap(i, rnd.Next(i, num));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Shuffle<T>(this T[] pArray, int pCount)
	{
		if (pCount >= 2)
		{
			for (int i = 0; i < pCount; i++)
			{
				pArray.Swap(i, rnd.Next(i, pCount));
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ShuffleOne<T>(this T[] pArray)
	{
		if (pArray.Length >= 2)
		{
			pArray.Swap(0, rnd.Next(0, pArray.Length));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ShuffleOne<T>(this T[] pArray, int pItem)
	{
		if (pArray.Length >= 2 && pArray.Length >= pItem + 1)
		{
			pArray.Swap(pItem, rnd.Next(pItem, pArray.Length));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ShuffleOne<T>(this T[] pArray, int pItem, int pCount)
	{
		if (pCount >= 2 && pCount >= pItem + 1)
		{
			pArray.Swap(pItem, rnd.Next(pItem, pCount));
		}
	}

	public static void Clear<T>(this T[] pArray)
	{
		Array.Clear(pArray, 0, pArray.Length);
	}

	public static void Clear<T>(this T[] pArray, int pCount)
	{
		Array.Clear(pArray, 0, pCount);
	}

	[Pure]
	public static bool AnyTrue(this bool[] pArray)
	{
		for (int i = 0; i < pArray.Length; i++)
		{
			if (pArray[i])
			{
				return true;
			}
		}
		return false;
	}

	[Pure]
	public static bool AnyFalse(this bool[] pArray)
	{
		for (int i = 0; i < pArray.Length; i++)
		{
			if (!pArray[i])
			{
				return true;
			}
		}
		return false;
	}

	public static string AsString<T>(this T[] pArray)
	{
		if (pArray == null)
		{
			return "";
		}
		using ListPool<string> listPool = new ListPool<string>(pArray.Length);
		for (int i = 0; i < pArray.Length; i++)
		{
			T val = pArray[i];
			listPool.Add(val?.ToString() ?? "null");
		}
		return string.Join(", ", listPool.ToArray());
	}

	public static void PrintToConsole<T>(this T[] pArray, string pMessage = null)
	{
		if (pArray != null)
		{
			string text = "";
			for (int i = 0; i < pArray.Length; i++)
			{
				T val = pArray[i];
				text = text + val.ToString() + ",";
			}
			if (text.Length > 0)
			{
				text = text.TrimEnd(',');
			}
			if (pMessage != null)
			{
				Debug.Log((object)(pMessage + ": [" + text + "]"));
			}
			else
			{
				Debug.Log((object)text);
			}
		}
	}

	public static bool AllTrue(this bool[] pArray)
	{
		return !pArray.AnyFalse();
	}

	public static bool AllFalse(this bool[] pArray)
	{
		return !pArray.AnyTrue();
	}
}
