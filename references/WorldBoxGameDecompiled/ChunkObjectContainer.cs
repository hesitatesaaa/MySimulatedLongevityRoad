using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ChunkObjectContainer : IDisposable
{
	public readonly List<long> kingdoms = new List<long>();

	public readonly List<Actor> units_all = new List<Actor>();

	public readonly List<Building> buildings_all = new List<Building>();

	private readonly HashSet<long> _hash_kingdoms = new HashSet<long>();

	private readonly Dictionary<long, List<Actor>> _dict_units = new Dictionary<long, List<Actor>>();

	private readonly Dictionary<long, List<Building>> _dict_buildings = new Dictionary<long, List<Building>>();

	private int _total_units;

	private int _total_buildings;

	public int total_units => _total_units;

	public int total_buildings => _total_buildings;

	public void reset(bool pClearBuildings)
	{
		if ((_total_units == 0 && _total_buildings == 0) || (_total_units == 0 && !pClearBuildings))
		{
			return;
		}
		foreach (List<Actor> value in _dict_units.Values)
		{
			value.Clear();
		}
		units_all.Clear();
		_total_units = 0;
		kingdoms.Clear();
		_hash_kingdoms.Clear();
		if (pClearBuildings)
		{
			buildings_all.Clear();
			_total_buildings = 0;
			{
				foreach (List<Building> value2 in _dict_buildings.Values)
				{
					value2.Clear();
				}
				return;
			}
		}
		if (_dict_buildings.Count <= 0)
		{
			return;
		}
		foreach (long key in _dict_buildings.Keys)
		{
			kingdoms.Add(key);
		}
		_hash_kingdoms.UnionWith(kingdoms);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public List<Building> getBuildings(long pKingdom)
	{
		return _dict_buildings[pKingdom];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public List<Actor> getUnits(long pKingdom)
	{
		return _dict_units[pKingdom];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isEmpty()
	{
		return kingdoms.Count == 0;
	}

	public void addActor(Actor pActor)
	{
		long id = pActor.kingdom.id;
		if (_hash_kingdoms.Add(id))
		{
			if (!_dict_units.TryGetValue(id, out var value))
			{
				value = new List<Actor>();
				_dict_units[id] = value;
				_dict_buildings[id] = new List<Building>();
			}
			value.Add(pActor);
			kingdoms.Add(id);
			_total_units++;
		}
		else
		{
			_dict_units[id].Add(pActor);
			_total_units++;
		}
		units_all.Add(pActor);
	}

	public void addBuilding(Building pBuilding)
	{
		long id = pBuilding.kingdom.id;
		if (_hash_kingdoms.Add(id))
		{
			if (!_dict_buildings.TryGetValue(id, out var value))
			{
				value = new List<Building>();
				_dict_buildings[id] = value;
				_dict_units[id] = new List<Actor>();
			}
			value.Add(pBuilding);
			_total_buildings++;
			kingdoms.Add(id);
		}
		else
		{
			_dict_buildings[id].Add(pBuilding);
			_total_buildings++;
		}
		buildings_all.Add(pBuilding);
	}

	public void Dispose()
	{
		reset(pClearBuildings: true);
		_dict_units.Clear();
		_dict_buildings.Clear();
		units_all.Clear();
		buildings_all.Clear();
		_total_units = 0;
		_total_buildings = 0;
	}

	public Dictionary<long, List<Building>>.ValueCollection getDebugBuildings()
	{
		return _dict_buildings.Values;
	}

	public Dictionary<long, List<Actor>>.ValueCollection getDebugUnits()
	{
		return _dict_units.Values;
	}
}
