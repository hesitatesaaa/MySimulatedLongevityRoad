using System.Collections.Generic;

public class CultureManager : MetaSystemManager<Culture, CultureData>
{
	private bool _dirty_kingdoms = true;

	private bool _dirty_cities = true;

	public CultureManager()
	{
		type_id = "culture";
	}

	public Culture newCulture(Actor pFounder, bool pAddDefaultTraits)
	{
		World.world.game_stats.data.culturesCreated++;
		World.world.map_stats.culturesCreated++;
		Culture culture = newObject();
		culture.createCulture(pFounder, pAddDefaultTraits);
		addRandomTraitFromBiomeToCulture(culture, pFounder.current_tile);
		MetaHelper.addRandomTrait(culture, AssetManager.culture_traits);
		return culture;
	}

	public void addRandomTraitFromBiomeToCulture(Culture pCulture, WorldTile pTile)
	{
		pCulture.addRandomTraitFromBiome(pTile, pTile.Type.biome_asset?.spawn_trait_culture, AssetManager.culture_traits);
	}

	public override void removeObject(Culture pObject)
	{
		World.world.game_stats.data.culturesForgotten++;
		World.world.map_stats.culturesForgotten++;
		base.removeObject(pObject);
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Culture culture = actor.culture;
			if (culture != null && culture.isDirtyUnits())
			{
				culture.listUnit(actor);
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
			if (kingdom.hasCulture())
			{
				kingdom.culture.listKingdom(kingdom);
			}
		}
	}

	private void clearAllKingdomLists()
	{
		using IEnumerator<Culture> enumerator = GetEnumerator();
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
		clearAllCitiesListst();
		foreach (City city in World.world.cities)
		{
			if (city.hasCulture())
			{
				city.culture.listCity(city);
			}
		}
	}

	private void clearAllCitiesListst()
	{
		using IEnumerator<Culture> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearListCities();
		}
	}

	public void setDirtyCities()
	{
		_dirty_cities = true;
	}

	public void setDirtyKingdoms()
	{
		_dirty_kingdoms = true;
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

	public Culture getMainCulture(List<Actor> pUnitList)
	{
		for (int i = 0; i < pUnitList.Count; i++)
		{
			Actor actor = pUnitList[i];
			if (actor.hasCulture())
			{
				Culture culture = actor.culture;
				countMetaObject(culture);
			}
		}
		return getMostUsedMetaObject();
	}

	public override void clear()
	{
		base.clear();
	}
}
