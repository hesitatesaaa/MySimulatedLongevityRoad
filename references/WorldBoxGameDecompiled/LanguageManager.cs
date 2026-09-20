using System.Collections.Generic;

public class LanguageManager : MetaSystemManager<Language, LanguageData>
{
	private bool _dirty_kingdoms = true;

	private bool _dirty_cities = true;

	public LanguageManager()
	{
		type_id = "language";
	}

	public Language newLanguage(Actor pActor, bool pAddDefaultTraits)
	{
		World.world.game_stats.data.languagesCreated++;
		World.world.map_stats.languagesCreated++;
		Language language = newObject();
		language.newLanguage(pActor, pAddDefaultTraits);
		MetaHelper.addRandomTrait(language, AssetManager.language_traits);
		addRandomTraitFromBiomeToLanguage(language, pActor.current_tile);
		return language;
	}

	public void addRandomTraitFromBiomeToLanguage(Language pLanguage, WorldTile pTile)
	{
		pLanguage.addRandomTraitFromBiome(pTile, pTile.Type.biome_asset?.spawn_trait_language, AssetManager.language_traits);
	}

	public Language getMainLanguage(List<Actor> pUnitList)
	{
		for (int i = 0; i < pUnitList.Count; i++)
		{
			Actor actor = pUnitList[i];
			if (actor.hasLanguage())
			{
				Language language = actor.language;
				countMetaObject(language);
			}
		}
		return getMostUsedMetaObject();
	}

	public override void removeObject(Language pObject)
	{
		World.world.game_stats.data.languagesForgotten++;
		World.world.map_stats.languagesForgotten++;
		base.removeObject(pObject);
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_alive = World.world.units.units_only_alive;
		for (int i = 0; i < units_only_alive.Count; i++)
		{
			Actor actor = units_only_alive[i];
			Language language = actor.language;
			if (language != null && language.isDirtyUnits())
			{
				language.listUnit(actor);
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
		clearAllKingdomListst();
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			kingdom.getLanguage()?.listKingdom(kingdom);
		}
	}

	private void clearAllKingdomListst()
	{
		using IEnumerator<Language> enumerator = GetEnumerator();
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
			if (city.hasLanguage())
			{
				city.language.listCity(city);
			}
		}
	}

	private void clearAllCitiesListst()
	{
		using IEnumerator<Language> enumerator = GetEnumerator();
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
