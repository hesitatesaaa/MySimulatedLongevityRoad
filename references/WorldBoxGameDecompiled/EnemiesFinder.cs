using System.Collections.Generic;
using UnityEngine.Pool;

public static class EnemiesFinder
{
	private static Dictionary<Kingdom, EnemyFinderContainer> _cache_data = new Dictionary<Kingdom, EnemyFinderContainer>();

	public static int counter_reused = 0;

	private static EnemyFinderContainer getCacheContainer(Kingdom pKingdom)
	{
		if (!_cache_data.TryGetValue(pKingdom, out var value))
		{
			value = UnsafeGenericPool<EnemyFinderContainer>.Get();
			value.setKingdom(pKingdom);
			_cache_data.Add(pKingdom, value);
		}
		return value;
	}

	internal static EnemyFinderData findEnemiesFrom(WorldTile pTile, Kingdom pKingdom, int pChunkRange = -1)
	{
		if (pChunkRange == -1)
		{
			pChunkRange = SimGlobals.m.unit_chunk_sight_range;
		}
		EnemyFinderContainer cacheContainer = getCacheContainer(pKingdom);
		MapChunk chunk = pTile.chunk;
		return cacheContainer.getData(chunk, pChunkRange);
	}

	public static void clear()
	{
		counter_reused = 0;
		foreach (EnemyFinderContainer value in _cache_data.Values)
		{
			value.clear();
			UnsafeGenericPool<EnemyFinderContainer>.Release(value);
		}
		_cache_data.Clear();
	}

	public static void disposeAll()
	{
		foreach (EnemyFinderContainer value in _cache_data.Values)
		{
			value.disposeAll();
		}
		clear();
	}
}
