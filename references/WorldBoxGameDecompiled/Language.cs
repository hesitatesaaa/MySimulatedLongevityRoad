using System;
using System.Collections.Generic;
using UnityEngine;
using db;

public class Language : MetaObjectWithTraits<LanguageData, LanguageTrait>
{
	public readonly List<City> cities = new List<City>();

	public readonly List<Kingdom> kingdoms = new List<Kingdom>();

	public readonly BooksHandler books = new BooksHandler();

	protected override MetaType meta_type => MetaType.Language;

	public override BaseSystemManager manager => World.world.languages;

	protected override List<string> default_traits => getActorAsset().default_language_traits;

	protected override AssetLibrary<LanguageTrait> trait_library => AssetManager.language_traits;

	protected override List<string> saved_traits => data.saved_traits;

	protected override string species_id => data.creator_species_id;

	public void newLanguage(Actor pActor, bool pAddDefaultTraits)
	{
		generateName(pActor);
		data.creator_name = pActor.getName();
		data.creator_id = pActor.getID();
		data.creator_species_id = pActor.asset.id;
		data.creator_subspecies_name = pActor.subspecies.name;
		data.creator_subspecies_id = pActor.subspecies.getID();
		data.creator_kingdom_id = pActor.kingdom.getID();
		data.creator_kingdom_name = pActor.kingdom.data.name;
		data.creator_clan_id = pActor.clan?.getID() ?? (-1);
		data.creator_clan_name = pActor.clan?.data.name;
		data.creator_city_id = pActor.city?.getID() ?? (-1);
		data.creator_city_name = pActor.city?.name;
		generateNewMetaObject(pAddDefaultTraits);
		books.setMeta(null, this);
	}

	public void countNewSpeaker()
	{
		increaseNewSpeakers();
		addRenown(1);
	}

	public void countConversion()
	{
		increaseConvertedSpeakers();
		addRenown(2);
	}

	public override void save()
	{
		base.save();
		data.saved_traits = getTraitsAsStrings();
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
	}

	public override void increaseBirths()
	{
		new NotImplementedException(GetType().Name);
	}

	public override void generateBanner()
	{
		data.banner_icon_id = AssetManager.language_banners_library.getNewIndexIcon();
		data.banner_background_id = AssetManager.language_banners_library.getNewIndexBackground();
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.languages_colors_library;
	}

	public void listCity(City pCity)
	{
		cities.Add(pCity);
	}

	public void listKingdom(Kingdom pKingdom)
	{
		kingdoms.Add(pKingdom);
	}

	public void clearListCities()
	{
		cities.Clear();
	}

	public void clearListKingdoms()
	{
		kingdoms.Clear();
	}

	private void generateName(Actor pActor)
	{
		string pName = pActor.generateName(MetaType.Language, getID());
		setName(pName);
		data.name_culture_id = pActor.culture?.getID() ?? (-1);
	}

	public Sprite getBackgroundSprite()
	{
		return AssetManager.language_banners_library.getSpriteBackground(data.banner_background_id);
	}

	public Sprite getIconSprite()
	{
		return AssetManager.language_banners_library.getSpriteIcon(data.banner_icon_id);
	}

	public override void loadData(LanguageData pData)
	{
		base.loadData(pData);
		books.setMeta(null, this);
	}

	public int countWrittenBooks()
	{
		return data.books_written;
	}

	public int countCities()
	{
		return cities.Count;
	}

	public int countKingdoms()
	{
		return kingdoms.Count;
	}

	public int getSpeakersNew()
	{
		return data.speakers_new;
	}

	public int getSpeakersLost()
	{
		return data.speakers_lost;
	}

	public int getSpeakersConverted()
	{
		return data.speakers_converted;
	}

	public void increaseConvertedSpeakers()
	{
		data.speakers_converted++;
	}

	public void increaseNewSpeakers()
	{
		data.speakers_new++;
	}

	public void increaseSpeakersLost()
	{
		data.speakers_lost++;
	}

	public override bool isReadyForRemoval()
	{
		if (books.hasBooks())
		{
			return false;
		}
		return base.isReadyForRemoval();
	}

	public override void convertSameSpeciesAroundUnit(Actor pActorMain, bool pOverrideExisting = false)
	{
		foreach (Actor item in getUnitFromChunkForConversion(pActorMain))
		{
			if (pOverrideExisting || !item.hasLanguage())
			{
				item.joinLanguage(this);
			}
		}
	}

	public override void forceConvertSameSpeciesAroundUnit(Actor pActorMain)
	{
		convertSameSpeciesAroundUnit(pActorMain, pOverrideExisting: true);
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "language");
		books.clear();
		cities.Clear();
		kingdoms.Clear();
		base.Dispose();
	}

	public override bool hasCities()
	{
		return cities.Count > 0;
	}

	public override IEnumerable<City> getCities()
	{
		return cities;
	}

	public override bool hasKingdoms()
	{
		return kingdoms.Count > 0;
	}

	public override IEnumerable<Kingdom> getKingdoms()
	{
		return kingdoms;
	}
}
