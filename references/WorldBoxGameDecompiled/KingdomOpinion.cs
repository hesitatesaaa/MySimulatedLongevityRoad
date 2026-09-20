using System;
using System.Collections.Generic;
using UnityPools;

public class KingdomOpinion : IDisposable
{
	public readonly Dictionary<OpinionAsset, int> results = UnsafeCollectionPool<Dictionary<OpinionAsset, int>, KeyValuePair<OpinionAsset, int>>.Get();

	public int total;

	public Kingdom main;

	public Kingdom target;

	public KingdomOpinion(Kingdom k1, Kingdom k2)
	{
		main = k1;
		target = k2;
	}

	internal void calculate(Kingdom pMain, Kingdom pTarget, DiplomacyRelation pRelation)
	{
		clear();
		foreach (OpinionAsset item in AssetManager.opinion_library.list)
		{
			int num = item.calc(pMain, pTarget);
			total += num;
			if (num != 0)
			{
				results.Add(item, num);
			}
		}
	}

	private void clear()
	{
		total = 0;
		results.Clear();
	}

	public void Dispose()
	{
		clear();
		UnsafeCollectionPool<Dictionary<OpinionAsset, int>, KeyValuePair<OpinionAsset, int>>.Release(results);
		main = null;
		target = null;
	}
}
