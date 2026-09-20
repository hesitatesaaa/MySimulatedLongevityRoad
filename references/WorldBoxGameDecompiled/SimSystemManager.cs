using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class SimSystemManager<TObject, TData> : SystemManager<TObject, TData>, IEnumerable<TObject>, IEnumerable where TObject : BaseSimObject, ILoadable<TData>, new() where TData : BaseObjectData, new()
{
	private readonly ObjectContainer<TObject> _container = new ObjectContainer<TObject>();

	private HashSet<TObject> _to_destroy_objects = new HashSet<TObject>();

	public bool event_destroy;

	public bool event_houses;

	public SimSystemManager()
	{
	}

	public void prepareArray()
	{
		_container.prepareArray(Count);
	}

	public override void loadFromSave(List<TData> pList)
	{
		base.loadFromSave(pList);
		_container.checkAddRemove();
	}

	protected override void addObject(TObject pObject)
	{
		base.addObject(pObject);
		_container.Add(pObject);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override void removeObject(TObject pObject)
	{
		base.removeObject(pObject);
		_container.Remove(pObject);
	}

	public override void clear()
	{
		base.clear();
		_container.Clear();
	}

	public void checkContainer()
	{
		_container.checkAddRemove();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator<TObject> GetEnumerator()
	{
		return _container.GetEnumerator();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public string debugContainer()
	{
		return _container.debug();
	}

	public TObject GetRandom()
	{
		return _container.GetRandom();
	}

	public List<TObject> getSimpleList()
	{
		return _container.getSimpleList();
	}

	public TObject[] getSimpleArray()
	{
		return _container.getSimpleArray();
	}

	internal virtual void scheduleDestroyOnPlay(TObject pObject)
	{
		_to_destroy_objects.Add(pObject);
	}

	internal void scheduleDestroyAllOnWorldClear()
	{
		using IEnumerator<TObject> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TObject current = enumerator.Current;
			_to_destroy_objects.Add(current);
		}
	}

	internal bool checkObjectsToDestroy()
	{
		if (_to_destroy_objects.Count <= 0)
		{
			return false;
		}
		foreach (TObject to_destroy_object in _to_destroy_objects)
		{
			destroyObject(to_destroy_object);
		}
		_to_destroy_objects.Clear();
		checkContainer();
		event_destroy = true;
		return true;
	}

	protected virtual void destroyObject(TObject pObject)
	{
	}
}
