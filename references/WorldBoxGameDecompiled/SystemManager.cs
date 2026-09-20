using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public abstract class SystemManager<TObject, TData> : BaseSystemManager where TObject : NanoObject, ILoadable<TData>, new() where TData : BaseSystemData, new()
{
	protected string type_id;

	private int _version;

	protected readonly Dictionary<long, TObject> dict = new Dictionary<long, TObject>();

	private HashSet<TObject> _to_dispose = new HashSet<TObject>();

	protected readonly Stack<TObject> _dead_objects = new Stack<TObject>();

	protected long _counter_recycled;

	public int version => _version;

	public override int Count => dict.Count;

	public override void clear()
	{
		_version++;
		dict.Clear();
		base.clear();
	}

	public virtual void update(float pElapsed)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	[Pure]
	public TObject get(long pID)
	{
		if (!pID.hasValue())
		{
			return null;
		}
		dict.TryGetValue(pID, out var value);
		return value;
	}

	public sealed override void ClearAllDisposed()
	{
		foreach (TObject item in _to_dispose)
		{
			item.Dispose();
			putForRecycle(item);
		}
		_to_dispose.Clear();
	}

	public void scheduleToDisposeObject(TObject pObject)
	{
		_to_dispose.Add(pObject);
	}

	public void scheduleToDisposeHashSet(HashSet<TObject> pHashSet)
	{
		_to_dispose.UnionWith(pHashSet);
	}

	public void scheduleToDisposeList(List<TObject> pList)
	{
		_to_dispose.UnionWith(pList);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void putForRecycle(TObject pObject)
	{
		pObject.exists = false;
		_dead_objects.Push(pObject);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[MustUseReturnValue]
	protected TObject newObject(long pSpecialID)
	{
		return newObjectFromID(pSpecialID);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[MustUseReturnValue]
	protected TObject newObject()
	{
		long nextId = World.world.map_stats.getNextId(type_id);
		return newObjectFromID(nextId);
	}

	[MustUseReturnValue]
	private TObject newObjectFromID(long pID)
	{
		TData data = new TData
		{
			id = pID,
			created_time = World.world.getCurWorldTime()
		};
		TObject nextObject = getNextObject();
		nextObject.setData(data);
		nextObject.created_time_unscaled = Time.time;
		addObject(nextObject);
		return nextObject;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[MustUseReturnValue]
	protected TObject getNextObject()
	{
		TObject val;
		if (_dead_objects.Count > 0)
		{
			val = _dead_objects.Pop();
			revive(val);
		}
		else
		{
			val = new TObject();
			val.setHash(BaseSystemManager._latest_hash++);
		}
		return val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void loadFromSave(List<TData> pList)
	{
		for (int i = 0; i < pList.Count; i++)
		{
			TData val = pList[i];
			if (val.id == -1)
			{
				val.id = World.world.map_stats.getNextId(type_id);
			}
			loadObject(val);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual TObject loadObject(TData pData)
	{
		TObject nextObject = getNextObject();
		nextObject.loadData(pData);
		addObject(nextObject);
		return nextObject;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void addObject(TObject pObject)
	{
		dict.Add(pObject.getID(), pObject);
		somethingChanged();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual void removeObject(TObject pObject)
	{
		pObject.setAlive(pValue: false);
		dict.Remove(pObject.getID());
		scheduleToDisposeObject(pObject);
		somethingChanged();
	}

	public void somethingChanged()
	{
		BaseSystemManager.anything_changed = true;
		_version++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void revive(TObject pObject)
	{
		_counter_recycled++;
		pObject.revive();
	}

	public override void showDebugTool(DebugTool pTool)
	{
		string text = GetType().ToString();
		text = text.Substring(text.LastIndexOf('.') + 1);
		int num = 0;
		foreach (TObject dead_object in _dead_objects)
		{
			if (dead_object.isAlive())
			{
				num++;
			}
		}
		pTool.setText(text, "a: " + Count + " | d: " + _dead_objects.Count + " | r : " + _counter_recycled + " | ad: " + num, 0f, pShowBar: false, 0L);
	}

	public override string debugShort()
	{
		if (Count == 0 && _dead_objects.Count == 0)
		{
			return "";
		}
		if (_dead_objects.Count > 0)
		{
			return $"{type_id} [a:{Count}|d:{_dead_objects.Count}]";
		}
		return $"{type_id} [{Count}]";
	}
}
