using System.Collections.Generic;

public class ReligionManager : MetaSystemManager<Religion, ReligionData>
{
	private bool _dirty_kingdoms = true;

	private bool _dirty_cities = true;

	public ReligionManager()
	{
		type_id = "religion";
	}

	public Religion newReligion(Actor pFounder, bool pAddDefaultTraits)
	{
		World.world.game_stats.data.religionsCreated++;
		World.world.map_stats.religionsCreated++;
		Religion religion = newObject();
		religion.newReligion(pFounder, pFounder.current_tile, pAddDefaultTraits);
		MetaHelper.addRandomTrait(religion, AssetManager.religion_traits);
		addRandomTraitFromBiomeToReligion(religion, pFounder.current_tile);
		return religion;
	}

	private void addRandomTraitFromBiomeToReligion(Religion pReligion, WorldTile pTile)
	{
		pReligion.addRandomTraitFromBiome(pTile, pTile.Type.biome_asset?.spawn_trait_religion, AssetManager.religion_traits);
	}

	public Religion getMainReligion(List<Actor> pUnitList)
	{
		for (int i = 0; i < pUnitList.Count; i++)
		{
			Actor actor = pUnitList[i];
			if (actor.hasReligion())
			{
				Religion religion = actor.religion;
				countMetaObject(religion);
			}
		}
		return getMostUsedMetaObject();
	}

	public override void removeObject(Religion pObject)
	{
		World.world.game_stats.data.religionsForgotten++;
		World.world.map_stats.religionsForgotten++;
		base.removeObject(pObject);
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Religion religion = actor.religion;
			if (religion != null && religion.isDirtyUnits())
			{
				religion.listUnit(actor);
			}
		}
	}

	public void beginChecksKingdoms()
	{
		if (_dirty_kingdoms)
		{
			updateDirtyKingdoms();
		}
		_dirty_kingdoms = false;
	}

	private void updateDirtyKingdoms()
	{
		clearAllKingdomLists();
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.hasReligion())
			{
				kingdom.religion.listKingdom(kingdom);
			}
		}
	}

	private void clearAllKingdomLists()
	{
		using IEnumerator<Religion> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearListKingdoms();
		}
	}

	public void beginChecksCities()
	{
		if (_dirty_cities)
		{
			updateDirtyCities();
		}
		_dirty_cities = false;
	}

	private void updateDirtyCities()
	{
		clearAllCitiesLists();
		foreach (City city in World.world.cities)
		{
			if (city.hasReligion())
			{
				city.religion.listCity(city);
			}
		}
	}

	private void clearAllCitiesLists()
	{
		using IEnumerator<Religion> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearListCities();
		}
	}

	public void setDirtyKingdoms()
	{
		_dirty_kingdoms = true;
	}

	public void setDirtyCities()
	{
		_dirty_cities = true;
	}

	public override bool isLocked()
	{
		if (isUnitsDirty())
		{
			return true;
		}
		if (_dirty_cities)
		{
			return true;
		}
		if (_dirty_kingdoms)
		{
			return true;
		}
		return false;
	}
}
