using System.Collections.Generic;
using UnityEngine;
using db;

public class Clan : MetaObjectWithTraits<ClanData, ClanTrait>
{
	public BaseStats base_stats_male = new BaseStats();

	public BaseStats base_stats_female = new BaseStats();

	protected override MetaType meta_type => MetaType.Clan;

	protected override bool track_death_types => true;

	public override BaseSystemManager manager => World.world.clans;

	protected override AssetLibrary<ClanTrait> trait_library => AssetManager.clan_traits;

	protected override List<string> default_traits => getActorAsset().default_clan_traits;

	protected override List<string> saved_traits => data.saved_traits;

	protected override string species_id => data.original_actor_asset;

	public void newClan(Actor pFounder, bool pAddDefaultTraits)
	{
		data.original_actor_asset = pFounder.asset.id;
		generateNewMetaObject(pAddDefaultTraits);
		if (pFounder.kingdom.isCiv())
		{
			data.founder_kingdom_name = pFounder.kingdom.data.name;
			data.founder_kingdom_id = pFounder.kingdom.getID();
		}
		data.founder_actor_name = pFounder.getName();
		data.founder_actor_id = pFounder.getID();
		data.founder_city_name = pFounder.city?.name;
		data.founder_city_id = pFounder.city?.getID() ?? (-1);
		data.creator_subspecies_name = pFounder.subspecies.name;
		data.creator_subspecies_id = pFounder.subspecies.getID();
		data.creator_species_id = pFounder.asset.id;
		string pName = pFounder.generateName(MetaType.Clan, getID());
		data.name_culture_id = pFounder.culture?.getID() ?? (-1);
		setName(pName);
	}

	protected override void recalcBaseStats()
	{
		base.recalcBaseStats();
		base_stats_female.clear();
		base_stats_male.clear();
		foreach (ClanTrait trait in getTraits())
		{
			base_stats_male.mergeStats(trait.base_stats_male);
			base_stats_female.mergeStats(trait.base_stats_female);
		}
	}

	public override void increaseBirths()
	{
		base.increaseBirths();
		addRenown(1);
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
	}

	public override void listUnit(Actor pActor)
	{
		base.listUnit(pActor);
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.clan_colors_library;
	}

	public override void generateBanner()
	{
		data.banner_background_id = AssetManager.clan_banners_library.getNewIndexBackground();
		data.banner_icon_id = AssetManager.clan_banners_library.getNewIndexIcon();
	}

	public string getMotto()
	{
		if (string.IsNullOrEmpty(data.motto))
		{
			data.motto = NameGenerator.getName("clan_mottos");
		}
		return data.motto;
	}

	public override void save()
	{
		base.save();
		data.saved_traits = getTraitsAsStrings();
	}

	public void checkMembersForNewChief()
	{
		if (base.units.Count != 0 && getChief().isRekt())
		{
			setChief(null);
			Actor nextChief = getNextChief();
			if (nextChief != null)
			{
				setChief(nextChief);
			}
		}
	}

	public void setChief(Actor pActor)
	{
		if (data != null)
		{
			if (pActor.isRekt())
			{
				data.chief_id = -1L;
				chiefLeft();
			}
			else
			{
				data.chief_id = pActor.getID();
				addChief(pActor);
			}
		}
	}

	public void updateChiefs()
	{
		if (data.past_chiefs == null || data.past_chiefs.Count == 0)
		{
			return;
		}
		foreach (LeaderEntry past_chief in data.past_chiefs)
		{
			Actor actor = World.world.units.get(past_chief.id);
			if (!actor.isRekt())
			{
				past_chief.name = actor.name;
			}
		}
	}

	public void addChief(Actor pActor)
	{
		ClanData clanData = data;
		if (clanData.past_chiefs == null)
		{
			clanData.past_chiefs = new List<LeaderEntry>();
		}
		chiefLeft();
		data.past_chiefs.Add(new LeaderEntry
		{
			id = pActor.getID(),
			name = pActor.name,
			color_id = data.color_id,
			timestamp_ago = World.world.getCurWorldTime()
		});
		if (data.past_chiefs.Count > 30)
		{
			data.past_chiefs.Shift();
		}
	}

	public void chiefLeft()
	{
		if (data.past_chiefs != null && data.past_chiefs.Count != 0)
		{
			LeaderEntry leaderEntry = data.past_chiefs.Last();
			if (!(leaderEntry.timestamp_end >= leaderEntry.timestamp_ago))
			{
				leaderEntry.timestamp_end = World.world.getCurWorldTime();
				updateChiefs();
			}
		}
	}

	public void tryForgetChief(Actor pActor)
	{
		if (data.chief_id == pActor.getID())
		{
			setChief(null);
		}
	}

	public Culture getClanCulture()
	{
		Culture culture = getChiefCulture();
		if (culture == null && data.culture_id.hasValue())
		{
			culture = World.world.cultures.get(data.culture_id);
		}
		if (culture == null)
		{
			data.culture_id = -1L;
		}
		else
		{
			data.culture_id = culture.getID();
		}
		return culture;
	}

	private Culture getChiefCulture()
	{
		return getChief()?.culture;
	}

	public Actor getNextChief(Actor pIgnore = null)
	{
		if (isDirtyUnits())
		{
			return null;
		}
		using ListPool<Actor> listPool = new ListPool<Actor>(base.units);
		ListSorters.sortUnitsSortedByAgeAndTraits(listPool, getClanCulture());
		Actor result = null;
		Actor chief = getChief();
		foreach (ref Actor item in listPool)
		{
			Actor current = item;
			if (current != pIgnore && current.isAlive() && chief != current)
			{
				result = current;
				break;
			}
		}
		return result;
	}

	public int getMaxMembers()
	{
		return (int)base_stats_meta["limit_clan_members"];
	}

	public Actor getChief()
	{
		Actor result = null;
		if (data.chief_id.hasValue())
		{
			result = World.world.units.get(data.chief_id);
		}
		return result;
	}

	public bool hasChief()
	{
		return getChief() != null;
	}

	public bool isFull()
	{
		int maxMembers = getMaxMembers();
		if (maxMembers == 0)
		{
			return false;
		}
		return base.units.Count >= maxMembers;
	}

	public bool fitToRule(Actor pActor, Kingdom pKingdom)
	{
		if (pActor.kingdom != pKingdom)
		{
			return false;
		}
		if (!pActor.isUnitFitToRule())
		{
			return false;
		}
		if (pActor.isKing())
		{
			return false;
		}
		return true;
	}

	public Sprite getBackgroundSprite()
	{
		return AssetManager.clan_banners_library.getSpriteBackground(data.banner_background_id);
	}

	public Sprite getIconSprite()
	{
		return AssetManager.clan_banners_library.getSpriteIcon(data.banner_icon_id);
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "clan");
		base.Dispose();
	}

	public string getTextMaxMembers()
	{
		int maxMembers = getMaxMembers();
		if (maxMembers == 0)
		{
			return base.units.Count.ToString();
		}
		return $"{base.units.Count}/{maxMembers}";
	}

	public override bool hasCities()
	{
		foreach (City city in World.world.cities)
		{
			if (city.getRoyalClan() == this)
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<City> getCities()
	{
		foreach (City city in World.world.cities)
		{
			if (city.getRoyalClan() == this)
			{
				yield return city;
			}
		}
	}

	public override bool hasKingdoms()
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.getKingClan() == this)
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<Kingdom> getKingdoms()
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.getKingClan() == this)
			{
				yield return kingdom;
			}
		}
	}
}
