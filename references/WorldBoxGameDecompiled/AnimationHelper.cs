using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class AnimationHelper
{
	private static float animationTimeMax = 100f;

	private static float _time_simulation;

	private static float _time_session;

	public static float getTime()
	{
		return _time_simulation;
	}

	public static void updateTime(float pElapsedScaled, float pElapsedSession)
	{
		updateTimeSimulation(pElapsedScaled);
		updateTimeSession(pElapsedSession);
	}

	private static void updateTimeSimulation(float pElapsed)
	{
		if (!World.world.isPaused())
		{
			_time_simulation += pElapsed;
			if (_time_simulation >= animationTimeMax)
			{
				_time_simulation -= animationTimeMax;
			}
		}
	}

	private static void updateTimeSession(float pElapsed)
	{
		_time_session += pElapsed;
		if (_time_session >= animationTimeMax)
		{
			_time_session -= animationTimeMax;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float getAnimationGlobalTime(float pAnimationSpeed)
	{
		return _time_simulation * pAnimationSpeed;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Sprite getSpriteFromListSessionTime(int pHashCodeOffset, IList<Sprite> pFrames, float pAnimationSpeed)
	{
		return getSpriteFromList(_time_session * pAnimationSpeed, pHashCodeOffset, pFrames);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Sprite getSpriteFromList(int pHashCodeOffset, IList<Sprite> pFrames, float pAnimationSpeed)
	{
		return getSpriteFromList(getAnimationGlobalTime(pAnimationSpeed), pHashCodeOffset, pFrames);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Sprite getSpriteFromList(float pTime, int pHashCodeOffset, IList<Sprite> pFrames)
	{
		int spriteIndex = getSpriteIndex(pTime, pHashCodeOffset, pFrames.Count);
		return pFrames[spriteIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getSpriteIndex(float pTime, int pHashCodeOffset, int pFrameCount)
	{
		if (pHashCodeOffset < 0)
		{
			pHashCodeOffset = -pHashCodeOffset;
		}
		return (int)(Mathf.Abs(pTime + (float)(pHashCodeOffset * 100)) % (float)pFrameCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getSpriteIndex(long pHashCodeOffset, int pFrameCount)
	{
		if (pHashCodeOffset < 0)
		{
			pHashCodeOffset = -pHashCodeOffset;
		}
		return (int)(Mathf.Abs((float)(1 + pHashCodeOffset * 100)) % (float)pFrameCount);
	}
}
