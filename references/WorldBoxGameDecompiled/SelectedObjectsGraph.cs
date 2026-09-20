using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedObjectsGraph : IEnumerable<NanoObject>, IEnumerable
{
	private NanoObject[] _selected_objects = new NanoObject[3];

	private int _selected_count;

	private bool _dirty;

	public int Count
	{
		get
		{
			if (_dirty)
			{
				_selected_count = 0;
				for (int i = 0; i < _selected_objects.Length; i++)
				{
					if (_selected_objects[i] != null)
					{
						_selected_count++;
					}
				}
				_dirty = false;
			}
			return _selected_count;
		}
	}

	public NanoObject this[int index] => _selected_objects[index];

	public void Clear()
	{
		Array.Clear(_selected_objects, 0, _selected_objects.Length);
		_dirty = true;
	}

	public void Add(NanoObject pObject)
	{
		if (pObject == null)
		{
			return;
		}
		for (int i = 0; i < _selected_objects.Length; i++)
		{
			if (_selected_objects[i] == null)
			{
				_selected_objects[i] = pObject;
				_dirty = true;
				return;
			}
		}
		Debug.LogWarning((object)"SelectedObjects is full, cannot add more objects.");
	}

	public void Remove(NanoObject pObject)
	{
		if (pObject == null)
		{
			return;
		}
		for (int i = 0; i < _selected_objects.Length; i++)
		{
			if (_selected_objects[i] == pObject)
			{
				_selected_objects[i] = null;
				_dirty = true;
				return;
			}
		}
		Debug.LogWarning((object)"SelectedObjects does not contain the object to remove.");
	}

	public IEnumerator<NanoObject> GetEnumerator()
	{
		for (int i = 0; i < _selected_objects.Length; i++)
		{
			yield return _selected_objects[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public NanoObject First()
	{
		using (IEnumerator<NanoObject> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				NanoObject current = enumerator.Current;
				if (current != null)
				{
					return current;
				}
			}
		}
		return null;
	}

	public bool Contains(NanoObject pObject)
	{
		using (IEnumerator<NanoObject> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == pObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveWhere(Func<NanoObject, bool> predicate)
	{
		for (int i = 0; i < _selected_objects.Length; i++)
		{
			if (_selected_objects[i] != null && predicate(_selected_objects[i]))
			{
				_selected_objects[i] = null;
			}
		}
		_dirty = true;
	}
}
