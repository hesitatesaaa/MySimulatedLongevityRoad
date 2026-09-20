using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

public static class Randy
{
	internal static Random rnd;

	internal static Random rand;

	internal static Random debug_rand;

	private static long _seed;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void OnBeforeSplashScreen()
	{
		fullReset();
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void OnBeforeSceneLoad()
	{
		fullReset();
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void OnAfterSceneLoad()
	{
		fullReset();
	}

	internal static void fullReset()
	{
		int year = DateTime.Now.Year;
		int month = DateTime.Now.Month;
		int day = DateTime.Now.Day;
		int hour = DateTime.Now.Hour;
		_seed = year * 100000 + month * 1000 + day * 10 + hour / 3;
		nextSeed();
	}

	internal static void nextSeed()
	{
		_seed += 543L;
		resetSeed(_seed);
	}

	public static void resetSeed(long pLongValue)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (pLongValue == 0L)
		{
			pLongValue = 1L;
		}
		int num = (int)pLongValue;
		uint num2 = (uint)pLongValue;
		Random.InitState(num);
		rnd = new Random(num);
		rand = new Random(num2);
		((Random)(ref rand)).NextBool();
	}

	public static void resetSeed(int pIntValue)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (pIntValue == 0)
		{
			pIntValue = 1;
		}
		Random.InitState(pIntValue);
		uint num = (uint)pIntValue;
		rnd = new Random(pIntValue);
		rand = new Random(num);
		((Random)(ref rand)).NextBool();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static int randomInt(int pMinInclusive, int pMaxExclusive)
	{
		return ((Random)(ref rand)).NextInt(pMinInclusive, pMaxExclusive);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static bool randomBool()
	{
		return ((Random)(ref rand)).NextBool();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static bool randomChance(float pVal)
	{
		float num = random();
		return pVal >= num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static float random()
	{
		return ((Random)(ref rand)).NextFloat();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static float randomFloat(float pMinInclusive, float pMaxExclusive)
	{
		return ((Random)(ref rand)).NextFloat(pMinInclusive, pMaxExclusive);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static Vector2 randomPointOnCircle(float pRadiusMinInclusive, float pRadiusMaxExclusive)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector2 randomPointInUnitCircle = getRandomPointInUnitCircle();
		return ((Vector2)(ref randomPointInUnitCircle)).normalized * randomFloat(pRadiusMinInclusive, pRadiusMaxExclusive);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static Vector2 getRandomPointInUnitCircle()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sqrt(random());
		float num2 = random() * 2f * MathF.PI;
		return new Vector2(num * Mathf.Cos(num2), num * Mathf.Sin(num2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public static T getRandom<T>(T[] pArray)
	{
		return pArray.GetRandom();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	[CanBeNull]
	public static T getRandom<T>(List<T> pList)
	{
		if (pList.Count == 0)
		{
			return default(T);
		}
		return pList.GetRandom();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	[CanBeNull]
	public static T getRandom<T>(ListPool<T> pList)
	{
		if (pList.Count == 0)
		{
			return default(T);
		}
		return pList.GetRandom();
	}

	[Pure]
	public static T RandomEnumValue<T>() where T : Enum
	{
		Array values = Enum.GetValues(typeof(T));
		return (T)values.GetValue(randomInt(0, values.Length));
	}

	[Pure]
	public static Color getRandomColor()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new Color(random(), random(), random(), 1f);
	}

	[Pure]
	public static Color ColorHSV()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);
	}

	[Pure]
	public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax, float alphaMin, float alphaMax)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Lerp(hueMin, hueMax, ((Random)(ref debug_rand)).NextFloat());
		float num2 = Mathf.Lerp(saturationMin, saturationMax, ((Random)(ref debug_rand)).NextFloat());
		float num3 = Mathf.Lerp(valueMin, valueMax, ((Random)(ref debug_rand)).NextFloat());
		Color result = Color.HSVToRGB(num, num2, num3, true);
		result.a = Mathf.Lerp(alphaMin, alphaMax, ((Random)(ref debug_rand)).NextFloat());
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this List<T> list)
	{
		return new RandomListEnumerator<T>(list);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this List<T> list, int pMax)
	{
		return new RandomListEnumerator<T>(list, list.Count, pMax);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this T[] array)
	{
		return new RandomArrayEnumerator<T>(array);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this T[] array, int pLength)
	{
		return new RandomArrayEnumerator<T>(array, pLength);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this T[] array, int pLength, int pMax)
	{
		return new RandomArrayEnumerator<T>(array, pLength, pMax);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this ListPool<T> list)
	{
		return new RandomArrayEnumerator<T>(list.GetRawBuffer(), list.Count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this ListPool<T> list, int pMax)
	{
		return new RandomArrayEnumerator<T>(list.GetRawBuffer(), list.Count, pMax);
	}

	public static IEnumerable<T> LoopRandom<T>(this IEnumerable<T> pEnumerable)
	{
		ListPool<T> tPool = null;
		IEnumerable<T> enumerable;
		if (!(pEnumerable is List<T> list))
		{
			if (!(pEnumerable is T[] array))
			{
				if (pEnumerable is ListPool<T> list2)
				{
					enumerable = list2.LoopRandom();
				}
				else
				{
					tPool = new ListPool<T>(pEnumerable);
					enumerable = tPool.LoopRandom();
				}
			}
			else
			{
				enumerable = array.LoopRandom(array.Length);
			}
		}
		else
		{
			enumerable = list.LoopRandom();
		}
		try
		{
			foreach (T item in enumerable)
			{
				yield return item;
			}
		}
		finally
		{
			tPool?.Dispose();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IEnumerable<T> LoopRandom<T>(this IEnumerable<T> pEnumerable, int pMax)
	{
		ListPool<T> tPool = null;
		IEnumerable<T> enumerable;
		if (!(pEnumerable is List<T> list))
		{
			if (!(pEnumerable is T[] array))
			{
				if (pEnumerable is ListPool<T> list2)
				{
					enumerable = list2.LoopRandom(pMax);
				}
				else
				{
					tPool = new ListPool<T>(pEnumerable);
					enumerable = tPool.LoopRandom(pMax);
				}
			}
			else
			{
				enumerable = array.LoopRandom(array.Length, pMax);
			}
		}
		else
		{
			enumerable = list.LoopRandom(pMax);
		}
		try
		{
			foreach (T item in enumerable)
			{
				yield return item;
			}
		}
		finally
		{
			tPool?.Dispose();
		}
	}

	static Randy()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		rnd = new Random(123);
		rand = new Random(123u);
		debug_rand = new Random(123u);
		_seed = 1L;
	}
}
