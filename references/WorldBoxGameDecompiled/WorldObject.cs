using System;
using System.Collections.Generic;
using UnityEngine;
using db;
using db.tables;

public class WorldObject : NanoObject, IMetaObject, ICoreObject
{
	protected static readonly HashSet<Family> _family_counter = new HashSet<Family>();

	private HistoryMetaDataAsset _history_meta_data_asset => AssetManager.history_meta_data_library.get("world");

	protected override MetaType meta_type => MetaType.World;

	public override string name
	{
		get
		{
			return World.world.map_stats.name;
		}
		protected set
		{
			World.world.map_stats.name = value;
		}
	}

	public List<Actor> units => World.world.units.getSimpleList();

	public override long getID()
	{
		return 1L;
	}

	public int countUnits()
	{
		return World.world.units.Count;
	}

	public IEnumerable<Actor> getUnits()
	{
		return World.world.units;
	}

	public bool hasUnits()
	{
		return World.world.units.Count > 0;
	}

	public Actor getRandomUnit()
	{
		return World.world.units.GetRandom();
	}

	public Actor getRandomActorForReaper()
	{
		return null;
	}

	public IEnumerable<Family> getFamilies()
	{
		return World.world.families;
	}

	public int countFamilies()
	{
		return World.world.families.Count;
	}

	public bool hasFamilies()
	{
		return World.world.families.Count > 0;
	}

	public override ColorAsset getColor()
	{
		return AssetManager.kingdom_colors_library.list.GetRandom();
	}

	public MetaObjectData getMetaData()
	{
		throw new NotImplementedException();
	}

	public int getRenown()
	{
		throw new NotImplementedException();
	}

	public int getPopulationPeople()
	{
		throw new NotImplementedException();
	}

	public long getTotalKills()
	{
		throw new NotImplementedException();
	}

	public long getTotalDeaths()
	{
		throw new NotImplementedException();
	}

	public bool isSelected()
	{
		throw new NotImplementedException();
	}

	public Actor getOldestVisibleUnit()
	{
		throw new NotImplementedException();
	}

	public Actor getOldestVisibleUnitForNameplatesCached()
	{
		throw new NotImplementedException();
	}

	public void startCollectHistoryData()
	{
		Delegate[] invocationList = _history_meta_data_asset.collector.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			HistoryTable obj = ((HistoryDataCollector)invocationList[i])(this);
			obj.timestamp = World.world.map_stats.history_current_year;
			DBInserter.insertData(obj, "world");
		}
	}

	public void clearLastYearStats()
	{
	}

	public ActorAsset getActorAsset()
	{
		throw new NotImplementedException();
	}

	public Sprite getSpriteIcon()
	{
		throw new NotImplementedException();
	}

	public bool isCursorOver()
	{
		throw new NotImplementedException();
	}

	public void setCursorOver()
	{
		throw new NotImplementedException();
	}

	public int getAge()
	{
		throw new NotImplementedException();
	}

	public bool isFavorite()
	{
		throw new NotImplementedException();
	}

	public void switchFavorite()
	{
		throw new NotImplementedException();
	}

	public bool hasCities()
	{
		return World.world.cities.Count > 0;
	}

	public IEnumerable<City> getCities()
	{
		return World.world.cities;
	}

	public bool hasKingdoms()
	{
		return World.world.kingdoms.Count > 0;
	}

	public IEnumerable<Kingdom> getKingdoms()
	{
		return World.world.kingdoms;
	}
}
