using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class CoreSystemManager<TObject, TData> : SystemManager<TObject, TData>, IEnumerable<TObject>, IEnumerable where TObject : CoreSystemObject<TData>, new() where TData : BaseSystemData, new()
{
	public readonly List<TObject> list = new List<TObject>(512);

	private readonly HashSet<TObject> _hashset = new HashSet<TObject>(512);

	private bool _dirty_list;

	public override int Count => _hashset.Count;

	private void setListDirty()
	{
		_dirty_list = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator<TObject> GetEnumerator()
	{
		return _hashset.GetEnumerator();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override void checkLists()
	{
		if (_dirty_list)
		{
			_dirty_list = false;
			list.Clear();
			list.AddRange(_hashset);
		}
	}

	public override void loadFromSave(List<TData> pList)
	{
		base.loadFromSave(pList);
		setListDirty();
	}

	protected override void addObject(TObject pObject)
	{
		base.addObject(pObject);
		setListDirty();
		_hashset.Add(pObject);
	}

	public virtual List<TData> save(List<TObject> pList = null)
	{
		if (pList == null)
		{
			pList = this.list;
		}
		List<TData> list = new List<TData>();
		foreach (TObject p in pList)
		{
			if (p.isAlive())
			{
				p.save();
				list.Add(p.data);
			}
		}
		return list;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void removeObject(TObject pObject)
	{
		base.removeObject(pObject);
		_hashset.Remove(pObject);
		setListDirty();
	}

	public TObject getRandom()
	{
		foreach (TObject item in list.LoopRandom())
		{
			if (_hashset.Contains(item))
			{
				return item;
			}
		}
		return null;
	}

	public override void clear()
	{
		foreach (TObject item in _hashset)
		{
			item.setAlive(pValue: false);
		}
		scheduleToDisposeHashSet(_hashset);
		_hashset.Clear();
		list.Clear();
		setListDirty();
		base.clear();
	}
}
