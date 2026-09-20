using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EpPathFinding.cs;

public class Heuristic
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Manhattan(int iDx, int iDy)
	{
		return iDx + iDy;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Euclidean(int iDx, int iDy)
	{
		float num = iDx;
		float num2 = iDy;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Chebyshev(int iDx, int iDy)
	{
		return Mathf.Max(iDx, iDy);
	}
}
