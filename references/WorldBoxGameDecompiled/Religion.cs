using System;
using System.Collections.Generic;
using UnityEngine;
using db;

public class Religion : MetaObjectWithTraits<ReligionData, ReligionTrait>
{
	public readonly List<City> cities = new List<City>();

	public readonly List<Kingdom> kingdoms = new List<Kingdom>();

	public BooksHandler books = new BooksHandler();

	public List<PlotAsset> possible_rites = new List<PlotAsset>();

	private bool _has_bloodline_bond;

	protected override MetaType meta_type => MetaType.Religion;

	public override BaseSystemManager manager => World.world.religions;

	public bool is_magic_only_clan_members => _has_bloodline_bond;

	protected override List<string> default_traits => getActorAsset().default_religion_traits;

	protected override AssetLibrary<ReligionTrait> trait_library => AssetManager.religion_traits;

	protected override List<string> saved_traits => data.saved_traits;

	protected override string species_id => data.creator_species_id;

	public void newReligion(Actor pActor, WorldTile pTile, bool pAddDefaultTraits)
	{
		data.creator_name = pActor.getName();
		data.creator_id = pActor.getID();
		data.creator_species_id = pActor.asset.id;
		data.creator_subspecies_name = pActor.subspecies.name;
		data.creator_subspecies_id = pActor.subspecies.getID();
		data.creator_clan_id = pActor.clan?.id ?? (-1);
		data.creator_clan_name = pActor.clan?.name;
		data.creator_kingdom_id = pActor.kingdom.getID();
		data.creator_kingdom_name = pActor.kingdom.name;
		data.creator_city_name = pActor.city?.name;
		data.creator_city_id = pActor.city?.getID() ?? (-1);
		generateNewMetaObject(pAddDefaultTraits);
		generateName(pActor);
		books.setMeta(null, null, this);
	}

	public override bool isReadyForRemoval()
	{
		if (books.hasBooks())
		{
			return false;
		}
		return base.isReadyForRemoval();
	}

	public override void loadData(ReligionData pData)
	{
		base.loadData(pData);
		books.setDirty();
		books.setMeta(null, null, this);
	}

	protected override void recalcBaseStats()
	{
		base.recalcBaseStats();
		_has_bloodline_bond = hasTrait("bloodline_bond");
		possible_rites.Clear();
		foreach (ReligionTrait trait in getTraits())
		{
			if (trait.hasPlotAsset())
			{
				possible_rites.Add(trait.plot_asset);
			}
		}
	}

	public void countConversion()
	{
		addRenown(1);
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
	}

	public override void increaseBirths()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public override void save()
	{
		base.save();
		data.saved_traits = getTraitsAsStrings();
	}

	public override void generateBanner()
	{
		data.banner_background_id = AssetManager.religion_banners_library.getNewIndexBackground();
		data.banner_icon_id = AssetManager.religion_banners_library.getNewIndexIcon();
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.religion_colors_library;
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
		string pName = pActor.generateName(MetaType.Religion, getID());
		setName(pName);
		data.name_culture_id = pActor.culture?.getID() ?? (-1);
	}

	public Sprite getBackgroundSprite()
	{
		return AssetManager.religion_banners_library.getSpriteBackground(data.banner_background_id);
	}

	public Sprite getIconSprite()
	{
		return AssetManager.religion_banners_library.getSpriteIcon(data.banner_icon_id);
	}

	public int countCities()
	{
		return cities.Count;
	}

	public int countKingdoms()
	{
		return kingdoms.Count;
	}

	public override void convertSameSpeciesAroundUnit(Actor pActorMain, bool pOverrideExisting = false)
	{
		foreach (Actor item in getUnitFromChunkForConversion(pActorMain))
		{
			if (pOverrideExisting || !item.hasReligion())
			{
				item.setReligion(this);
			}
		}
	}

	public override void forceConvertSameSpeciesAroundUnit(Actor pActorMain)
	{
		convertSameSpeciesAroundUnit(pActorMain, pOverrideExisting: true);
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "religion");
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
