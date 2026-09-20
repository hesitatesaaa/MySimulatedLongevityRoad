using System;
using System.Collections.Generic;

public class MultiStackPool<T> where T : new()
{
	private Dictionary<Type, StackPool<T>> _pools = new Dictionary<Type, StackPool<T>>();

	public U get<U>() where U : T, new()
	{
		Type typeFromHandle = typeof(U);
		if (!_pools.TryGetValue(typeFromHandle, out var value))
		{
			value = new StackPool<T>();
			_pools.Add(typeFromHandle, value);
		}
		return value.get<U>();
	}

	public void release(T pObject)
	{
		Type type = pObject.GetType();
		if (_pools.TryGetValue(type, out var value))
		{
			value.release(pObject);
		}
	}

	public void clear()
	{
		foreach (StackPool<T> value in _pools.Values)
		{
			value.clear();
		}
		_pools.Clear();
	}
}
