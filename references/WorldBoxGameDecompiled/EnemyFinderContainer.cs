using System;
using System.Collections.Generic;
using UnityEngine.Pool;

public class EnemyFinderContainer
{
	public Dictionary<int, EnemyFinderData> dict_data = new Dictionary<int, EnemyFinderData>((int)Math.Pow(9.0, SimGlobals.m.unit_chunk_sight_range));

	private Kingdom _kingdom;

	public void setKingdom(Kingdom pKingdom)
	{
		_kingdom = pKingdom;
	}

	public EnemyFinderData getData(MapChunk pChunk, int pRange)
	{
		int key = pChunk.id * 10000 + pRange;
		if (!dict_data.TryGetValue(key, out var value))
		{
			value = UnsafeGenericPool<EnemyFinderData>.Get();
			dict_data.Add(key, value);
			if (!_kingdom.asset.force_look_all_chunks)
			{
				if (pRange == 0)
				{
					findEnemiesOfKingdomInChunk(value, pChunk, _kingdom);
					return value;
				}
				if (Randy.randomChance(0.8f))
				{
					findEnemiesOfKingdomInChunk(value, pChunk, _kingdom);
				}
			}
			if (value.isEmpty())
			{
				for (int i = 0; i <= pRange; i++)
				{
					checkRange(value, pChunk, i, i);
					if (!value.isEmpty() && !_kingdom.asset.force_look_all_chunks)
					{
						break;
					}
				}
			}
			return value;
		}
		EnemiesFinder.counter_reused++;
		return value;
	}

	private void checkRange(EnemyFinderData pData, MapChunk pChunk, int pRange, int pSkipLessThan = -1)
	{
		if (pRange == 0)
		{
			findEnemiesOfKingdomInChunk(pData, pChunk, _kingdom);
			return;
		}
		int x = pChunk.x;
		int y = pChunk.y;
		bool flag = pSkipLessThan > 0;
		int num = pSkipLessThan * -1;
		for (int i = -pRange; i <= pRange; i++)
		{
			for (int j = -pRange; j <= pRange; j++)
			{
				if (!flag || i <= num || i >= pSkipLessThan || j <= num || j >= pSkipLessThan)
				{
					int pX = x + i;
					int pY = y + j;
					MapChunk mapChunk = World.world.map_chunk_manager.get(pX, pY);
					if (mapChunk != null)
					{
						findEnemiesOfKingdomInChunk(pData, mapChunk, _kingdom);
					}
				}
			}
		}
	}

	private static void findEnemiesOfKingdomInChunk(EnemyFinderData pData, MapChunk pChunk, Kingdom pMainKingdom)
	{
		if (pChunk.objects.kingdoms.Count == 0)
		{
			return;
		}
		List<long> kingdoms = pChunk.objects.kingdoms;
		bool flag = WorldLawLibrary.world_law_peaceful_monsters.isEnabled();
		if (pMainKingdom.asset.mobs & flag)
		{
			return;
		}
		for (int i = 0; i < kingdoms.Count; i++)
		{
			long num = kingdoms[i];
			Kingdom civOrWildViaID = World.world.kingdoms.getCivOrWildViaID(num);
			if (civOrWildViaID != null && (!flag || !civOrWildViaID.asset.mobs) && pMainKingdom.isEnemy(civOrWildViaID))
			{
				pData.addEnemyList(pChunk.objects.getUnits(num));
				pData.addEnemyList(pChunk.objects.getBuildings(num));
			}
		}
	}

	public void clear()
	{
		foreach (EnemyFinderData value in dict_data.Values)
		{
			value.reset();
			UnsafeGenericPool<EnemyFinderData>.Release(value);
		}
		dict_data.Clear();
	}

	public void disposeAll()
	{
		foreach (EnemyFinderData value in dict_data.Values)
		{
			value.reset();
		}
		_kingdom = null;
	}
}
