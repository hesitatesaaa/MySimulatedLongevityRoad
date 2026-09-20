using System;
using System.Collections.Generic;
using UnityPools;

public class CategoryData : IDisposable
{
	private LinkedList<Dictionary<string, long>> _data = new LinkedList<Dictionary<string, long>>();

	internal ListPool<object> db_list;

	public LinkedListNode<Dictionary<string, long>> Last => _data.Last;

	public int Count => _data.Count;

	public LinkedListNode<Dictionary<string, long>> AddLast(Dictionary<string, long> pDict)
	{
		return _data.AddLast(pDict);
	}

	public void Clear()
	{
		foreach (Dictionary<string, long> datum in _data)
		{
			UnsafeCollectionPool<Dictionary<string, long>, KeyValuePair<string, long>>.Release(datum);
		}
		_data.Clear();
		db_list?.Dispose();
		db_list = null;
	}

	public void Dispose()
	{
		Clear();
		_data = null;
	}
}
