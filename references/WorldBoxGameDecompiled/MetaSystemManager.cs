using System;
using System.Collections.Generic;
using db;
using db.tables;

public abstract class MetaSystemManager<TObject, TData> : CoreSystemManager<TObject, TData> where TObject : MetaObject<TData>, new() where TData : MetaObjectData, new()
{
	private bool _dirty_units;

	protected Dictionary<TObject, MetaObjectCounter<TObject, TData>> _counters_dict = new Dictionary<TObject, MetaObjectCounter<TObject, TData>>();

	protected List<MetaObjectCounter<TObject, TData>> _counters_list = new List<MetaObjectCounter<TObject, TData>>();

	private List<TObject> _to_remove = new List<TObject>();

	protected void countMetaObject(TObject pMetaObject)
	{
		if (!_counters_dict.TryGetValue(pMetaObject, out var value))
		{
			value = new MetaObjectCounter<TObject, TData>(pMetaObject);
			_counters_dict.Add(pMetaObject, value);
			_counters_list.Add(value);
		}
		_counters_dict[pMetaObject].amount++;
	}

	protected TObject getMostUsedMetaObject()
	{
		if (!_counters_list.Any())
		{
			return null;
		}
		_counters_list.Sort(sortByAmount);
		TObject meta_object = _counters_list[0].meta_object;
		clearCounters();
		return meta_object;
	}

	private int sortByAmount(MetaObjectCounter<TObject, TData> pO1, MetaObjectCounter<TObject, TData> pO2)
	{
		return pO2.amount.CompareTo(pO1.amount);
	}

	private void clearCounters()
	{
		_counters_dict.Clear();
		_counters_list.Clear();
	}

	public override void parallelDirtyUnitsCheck()
	{
		beginChecksUnits();
	}

	public void beginChecksUnits()
	{
		if (_dirty_units)
		{
			clearAllUnitLists();
			updateDirtyUnits();
			finishDirtyUnits();
		}
	}

	public override void startCollectHistoryData()
	{
		HistoryMetaDataAsset historyMetaDataAsset = AssetManager.history_meta_data_library.get(type_id);
		if (historyMetaDataAsset.collector == null)
		{
			return;
		}
		using IEnumerator<TObject> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TObject current = enumerator.Current;
			Delegate[] invocationList = historyMetaDataAsset.collector.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				HistoryTable obj = ((HistoryDataCollector)invocationList[i])(current);
				obj.timestamp = World.world.map_stats.history_current_year;
				DBInserter.insertData(obj, type_id);
			}
		}
	}

	public override void clearLastYearStats()
	{
		using IEnumerator<TObject> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearLastYearStats();
		}
	}

	public override bool isUnitsDirty()
	{
		return _dirty_units;
	}

	protected virtual void finishDirtyUnits()
	{
		_dirty_units = false;
		using IEnumerator<TObject> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TObject current = enumerator.Current;
			if (current.isDirtyUnits())
			{
				current.unDirty();
			}
		}
	}

	public override void checkDeadObjects()
	{
		base.checkDeadObjects();
		using (IEnumerator<TObject> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				TObject current = enumerator.Current;
				if (current.isReadyForRemoval())
				{
					_to_remove.Add(current);
				}
			}
		}
		foreach (TObject item in _to_remove)
		{
			item.triggerOnRemoveObject();
			removeObject(item);
		}
		_to_remove.Clear();
	}

	protected abstract void updateDirtyUnits();

	public virtual void unitDied(TObject pObject)
	{
		setDirtyUnits(pObject);
	}

	public virtual void unitAdded(TObject pObject)
	{
		setDirtyUnits(pObject);
	}

	protected override void addObject(TObject pObject)
	{
		base.addObject(pObject);
		setDirtyUnits(pObject);
	}

	public void setDirtyUnits(TObject pObject)
	{
		pObject?.setDirty();
		_dirty_units = true;
	}

	private void clearAllUnitLists()
	{
		using IEnumerator<TObject> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TObject current = enumerator.Current;
			if (current.isDirtyUnits())
			{
				current.clearListUnits();
			}
		}
	}
}
