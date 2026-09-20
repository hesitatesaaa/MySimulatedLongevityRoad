using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public class ObjectContainer<T> : IEnumerable<T>, IEnumerable
{
	private HashSet<T> _to_remove = new HashSet<T>();

	private HashSet<T> _to_add = new HashSet<T>();

	private HashSet<T> _hashSet = new HashSet<T>();

	private T[] _array;

	private int _array_count;

	private bool _dirty_list;

	private bool _dirty_array;

	private bool _dirty_container;

	private readonly List<T> _simple_list = new List<T>();

	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _hashSet.Count;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator<T> GetEnumerator()
	{
		return _hashSet.GetEnumerator();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(T pObject, bool pCheckToAddRemove = true)
	{
		if (pCheckToAddRemove)
		{
			if (_to_add.Contains(pObject))
			{
				return true;
			}
			if (_to_remove.Contains(pObject))
			{
				return false;
			}
		}
		return _hashSet.Contains(pObject);
	}

	public void Clear()
	{
		_hashSet.Clear();
		_to_add.Clear();
		_to_remove.Clear();
		_simple_list.Clear();
		_dirty_list = false;
		_dirty_array = false;
		_dirty_container = false;
		if (_array_count > 0)
		{
			Array.Clear(_array, 0, _array.Length);
			_array_count = 0;
		}
	}

	public void doChecks()
	{
		checkAddRemove();
		checkSimpleListDirty();
	}

	public bool isDirtyContainer()
	{
		return _dirty_container;
	}

	public void checkAddRemove()
	{
		if (_to_add.Count > 0)
		{
			if (_hashSet.Count == 0)
			{
				HashSet<T> hashSet = _hashSet;
				_hashSet = _to_add;
				_to_add = hashSet;
			}
			else
			{
				_hashSet.UnionWith(_to_add);
				_to_add.Clear();
			}
			_dirty_list = true;
			_dirty_array = true;
		}
		if (_to_remove.Count > 0)
		{
			_hashSet.ExceptWith(_to_remove);
			_to_remove.Clear();
			_dirty_list = true;
			_dirty_array = true;
		}
		_dirty_container = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public List<T> getSimpleList()
	{
		checkSimpleListDirty();
		return _simple_list;
	}

	public T[] getFastSimpleArray()
	{
		if (_dirty_array)
		{
			_dirty_array = false;
			_hashSet.CopyTo(_array, 0);
			_array_count = _hashSet.Count;
		}
		return _array;
	}

	public T[] getSimpleArray()
	{
		if (_dirty_array)
		{
			_dirty_array = false;
			prepareArray(_hashSet.Count);
			_hashSet.CopyTo(_array, 0);
			_array_count = _hashSet.Count;
		}
		return _array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void checkSimpleListDirty()
	{
		if (_dirty_list)
		{
			List<T> simple_list = _simple_list;
			simple_list.Clear();
			simple_list.AddRange(_hashSet);
			_dirty_list = false;
		}
	}

	public void prepareArray(int pSize)
	{
		_array = Toolbox.checkArraySize(_array, pSize);
	}

	[CanBeNull]
	public T GetRandom()
	{
		checkAddRemove();
		List<T> simpleList = getSimpleList();
		if (simpleList.Count == 0)
		{
			return default(T);
		}
		return simpleList.GetRandom();
	}

	public string debug()
	{
		return _hashSet.Count + "/" + _to_add.Count + "/" + _to_remove.Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T pObject)
	{
		_to_add.Add(pObject);
		_to_remove.Remove(pObject);
		_dirty_container = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Remove(T pObject)
	{
		_to_remove.Add(pObject);
		_to_add.Remove(pObject);
		_dirty_container = true;
	}
}
