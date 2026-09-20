using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ParallelHelper
{
	public static int DEBUG_BATCH_SIZE = 128;

	private const int MAX_BATCH_SIZE = 256;

	private static readonly int[] _batch_sizes = new int[11]
	{
		4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048,
		4096
	};

	public static int getDynamicBatchSize(int pCount)
	{
		if (pCount <= 32)
		{
			return 4;
		}
		if (pCount <= 64)
		{
			return 8;
		}
		if (pCount <= 128)
		{
			return 16;
		}
		if (pCount <= 256)
		{
			return 32;
		}
		if (pCount <= 512)
		{
			return 64;
		}
		if (pCount <= 2048)
		{
			return 128;
		}
		return 256;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int calcTotalBatches(int pItemsTotal, int pBatchSize)
	{
		return Mathf.CeilToInt((float)pItemsTotal / (float)pBatchSize);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int calculateBatchEnd(int pItemsIndexStart, int pBatchSize, int pItemsTotal)
	{
		return Mathf.Min(pItemsIndexStart + pBatchSize, pItemsTotal);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int calculateBatchBeg(int pBatchIndex, int pBatchSize)
	{
		return pBatchIndex * pBatchSize;
	}

	public static void moveDebugBatchSize()
	{
		int num = Array.IndexOf(_batch_sizes, DEBUG_BATCH_SIZE);
		if (num == -1)
		{
			num = 0;
		}
		num = (num + 1) % _batch_sizes.Length;
		DEBUG_BATCH_SIZE = _batch_sizes[num];
	}
}
