using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolGenericMono<T> where T : Component
{
	private readonly List<T> _elements_total = new List<T>();

	private readonly Queue<T> _elements_inactive = new Queue<T>();

	private readonly T _prefab;

	private readonly Transform _parent_transform;

	public ObjectPoolGenericMono(T pPrefab, Transform pParentTransform)
	{
		_prefab = pPrefab;
		_parent_transform = pParentTransform;
	}

	public void clear(bool pDisable = true)
	{
		_elements_inactive.Clear();
		sortElements();
		foreach (T item in _elements_total)
		{
			release(item, pDisable);
		}
	}

	private void sortElements()
	{
		_elements_total.Sort((T a, T b) => ((Component)a).transform.GetSiblingIndex().CompareTo(((Component)b).transform.GetSiblingIndex()));
	}

	public T getFirstActive()
	{
		return _elements_total[0];
	}

	public IReadOnlyList<T> getListTotal()
	{
		return _elements_total;
	}

	public void disableInactive()
	{
		foreach (T item in _elements_inactive)
		{
			if (((Component)item).gameObject.activeSelf)
			{
				((Component)item).gameObject.SetActive(false);
			}
		}
	}

	public T getNext()
	{
		T newOrActivate = getNewOrActivate();
		checkActive(newOrActivate);
		return newOrActivate;
	}

	private T getNewOrActivate()
	{
		T val;
		if (_elements_inactive.Count > 0)
		{
			val = _elements_inactive.Dequeue();
		}
		else
		{
			val = Object.Instantiate<T>(_prefab, _parent_transform);
			_elements_total.Add(val);
			((Object)(object)val).name = typeof(T)?.ToString() + " " + _elements_total.Count + " " + ((Component)val).transform.GetSiblingIndex();
		}
		return val;
	}

	public void release(T pElement, bool pDisable = true)
	{
		if (((Component)_parent_transform).gameObject.activeInHierarchy)
		{
			((Component)pElement).transform.SetAsLastSibling();
		}
		if (!_elements_inactive.Contains(pElement))
		{
			_elements_inactive.Enqueue(pElement);
		}
		if (((Component)pElement).gameObject.activeSelf & pDisable)
		{
			((Component)pElement).gameObject.SetActive(false);
		}
	}

	private void checkActive(T pElement)
	{
		if (!((Component)pElement).gameObject.activeSelf)
		{
			((Component)pElement).gameObject.SetActive(true);
		}
	}

	public int countTotal()
	{
		return _elements_total.Count;
	}

	public int countInactive()
	{
		return _elements_inactive.Count;
	}

	public int countActive()
	{
		return _elements_total.Count - _elements_inactive.Count;
	}

	public void resetParent()
	{
		foreach (T item in _elements_total)
		{
			resetParent(item);
		}
	}

	public void resetParent(T pElement)
	{
		if (((Component)_parent_transform).gameObject.activeInHierarchy)
		{
			((Component)pElement).transform.SetParent(_parent_transform);
		}
	}
}
