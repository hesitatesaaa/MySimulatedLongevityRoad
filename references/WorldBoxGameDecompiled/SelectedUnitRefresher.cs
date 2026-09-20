using System.Collections.Generic;

public class SelectedUnitRefresher<T>
{
	private HashSet<T> _temp_set_1 = new HashSet<T>();

	private HashSet<T> _temp_set_2 = new HashSet<T>();

	public bool needsToRefresh(IReadOnlyCollection<T> pCurrent, IReadOnlyCollection<T> pNew)
	{
		foreach (T item in pCurrent)
		{
			_temp_set_1.Add(item);
		}
		foreach (T item2 in pNew)
		{
			_temp_set_2.Add(item2);
		}
		if (_temp_set_1.SetEquals(_temp_set_2))
		{
			return false;
		}
		return true;
	}

	public void addRendered(T pPrev)
	{
		_temp_set_1.Add(pPrev);
	}

	public void addCurrent(T pCurrent)
	{
		_temp_set_2.Add(pCurrent);
	}

	public bool needsToRefresh()
	{
		if (_temp_set_1.SetEquals(_temp_set_2))
		{
			return false;
		}
		return true;
	}

	public void clear()
	{
		_temp_set_1.Clear();
		_temp_set_2.Clear();
	}
}
