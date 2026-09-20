using System;
using System.Collections.Generic;

public static class DictionaryExtensions
{
	public static int RemoveByValue<TKey, TValue>(this IDictionary<TKey, TValue> pDict, Predicate<TValue> pPredicate)
	{
		using ListPool<TKey> listPool = new ListPool<TKey>(pDict.Count);
		foreach (KeyValuePair<TKey, TValue> item in pDict)
		{
			if (pPredicate(item.Value))
			{
				listPool.Add(item.Key);
			}
		}
		foreach (ref TKey item2 in listPool)
		{
			TKey current2 = item2;
			pDict.Remove(current2);
		}
		return listPool.Count;
	}

	public static int RemoveByKey<TKey, TValue>(this IDictionary<TKey, TValue> pDict, Predicate<TKey> pPredicate)
	{
		using ListPool<TKey> listPool = new ListPool<TKey>(pDict.Count);
		foreach (TKey key in pDict.Keys)
		{
			if (pPredicate(key))
			{
				listPool.Add(key);
			}
		}
		foreach (ref TKey item in listPool)
		{
			TKey current2 = item;
			pDict.Remove(current2);
		}
		return listPool.Count;
	}
}
