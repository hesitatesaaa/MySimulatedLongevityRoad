using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MetaObject<TData> : CoreSystemObject<TData>, IMetaObject, ICoreObject where TData : MetaObjectData
{
	private bool _units_dirty;

	protected static readonly HashSet<Family> _family_counter = new HashSet<Family>();

	private ColorAsset _cached_color;

	private bool _force_preserve_alive;

	private int _cursor_over;

	private double _timestamp_last_visible = -1.0;

	private Actor _cached_visible_unit;

	private long _cached_visible_unit_id;

	protected virtual bool track_death_types => false;

	public List<Actor> units { get; } = new List<Actor>();

	public MetaTypeAsset meta_type_asset => AssetManager.meta_type_library.getAsset(meta_type);

	public void preserveAlive()
	{
		_force_preserve_alive = true;
	}

	protected override void setDefaultValues()
	{
		base.setDefaultValues();
		_units_dirty = true;
		_force_preserve_alive = true;
	}

	public virtual bool isReadyForRemoval()
	{
		if (_force_preserve_alive)
		{
			return false;
		}
		if (units.Count > 0)
		{
			return false;
		}
		return true;
	}

	internal virtual void clearListUnits()
	{
		_force_preserve_alive = false;
		units.Clear();
	}

	public virtual void listUnit(Actor pActor)
	{
		units.Add(pActor);
	}

	public bool isLocked()
	{
		return isDirtyUnits();
	}

	public bool isDirtyUnits()
	{
		return _units_dirty;
	}

	public void unDirty()
	{
		stats_dirty_version++;
		_units_dirty = false;
	}

	public void setDirty()
	{
		_units_dirty = true;
	}

	public virtual void updateDirty()
	{
	}

	public override void Dispose()
	{
		if (!Config.disable_dispose_logs)
		{
			Debug.Log((object)("MetaObject::Dispose " + data.id + " " + data.name));
		}
		clearListUnits();
		_cached_color = null;
		clearCachedVisibleUnit();
		base.Dispose();
	}

	protected virtual ColorLibrary getColorLibrary()
	{
		throw new NotImplementedException(GetType().Name);
	}

	public override bool updateColor(ColorAsset pColor)
	{
		if (getColor() == pColor)
		{
			return false;
		}
		data.setColorID(getColorLibrary().list.IndexOf(pColor));
		_cached_color = null;
		return true;
	}

	public bool isCursorOver()
	{
		return _cursor_over > 0;
	}

	public void setCursorOver()
	{
		_cursor_over = 3;
	}

	public void clearCursorOver()
	{
		if (_cursor_over > 0)
		{
			_cursor_over--;
		}
	}

	public override ColorAsset getColor()
	{
		if (_cached_color == null)
		{
			_cached_color = getColorLibrary().list[data.color_id];
		}
		return _cached_color;
	}

	public override void trackName(bool pPostChange = false)
	{
		if (!string.IsNullOrEmpty(data.name) && (!pPostChange || (data.past_names != null && data.past_names.Count != 0)))
		{
			BaseSystemData baseSystemData = data;
			if (baseSystemData.past_names == null)
			{
				baseSystemData.past_names = new List<NameEntry>();
			}
			if (data.past_names.Count == 0)
			{
				NameEntry item = new NameEntry(data.name, pCustom: false, data.original_color_id, data.created_time);
				data.past_names.Add(item);
			}
			else if (!(data.past_names.Last().name == data.name))
			{
				NameEntry item2 = new NameEntry(data.name, data.custom_name, data.color_id);
				data.past_names.Add(item2);
			}
		}
	}

	protected virtual void generateNewMetaObject(bool pAddDefaultTraits)
	{
		generateNewMetaObject();
	}

	protected virtual void generateNewMetaObject()
	{
		generateColor();
		generateBanner();
	}

	public virtual void generateBanner()
	{
		throw new NotImplementedException(GetType().Name);
	}

	protected virtual void generateColor()
	{
		ActorAsset actorAsset = getActorAsset();
		int nextColorIndex = getColorLibrary().getNextColorIndex(actorAsset);
		data.setColorID(nextColorIndex);
	}

	public bool isSelected()
	{
		return SelectedObjects.isNanoObjectSelected(this);
	}

	public virtual int countUnits()
	{
		return units.Count;
	}

	public virtual IEnumerable<Actor> getUnits()
	{
		return units;
	}

	public virtual Actor getRandomUnit()
	{
		return Randy.getRandom(units);
	}

	public Actor getRandomActorForReaper()
	{
		foreach (Actor item in units.LoopRandom())
		{
			if (item.isAlive())
			{
				return item;
			}
		}
		return null;
	}

	public virtual int countHappyUnits()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (!unit.asset.is_boat && unit.isHappy())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countUnhappyUnits()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (!unit.asset.is_boat && unit.isUnhappy())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countSingleMales()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isBreedingAge() && unit.isSexMale() && !unit.hasLover())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countCouples()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.hasLover())
			{
				num++;
			}
		}
		return num / 2;
	}

	public virtual int countSingleFemales()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isBreedingAge() && unit.isSexFemale() && !unit.hasLover())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countHoused()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (!unit.asset.is_boat && unit.hasHouse())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countHomeless()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (!unit.asset.is_boat && !unit.hasHouse())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countStarving()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isStarving())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countHungry()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isHungry())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countSick()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isSick())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countAdults()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAlive() && !unit.asset.is_boat && unit.isAdult())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countTotalMoney()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAlive())
			{
				num += unit.money;
			}
		}
		return num;
	}

	public int countPotentialParents(ActorSex pSex)
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAlive() && !unit.asset.is_boat && unit.data.sex == pSex && unit.canBreed() && !unit.hasReachedOffspringLimit())
			{
				num++;
			}
		}
		return num;
	}

	public int countUnitsWithStatus(string pStatusID)
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAlive() && unit.hasStatus(pStatusID))
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countChildren()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (!unit.asset.is_boat && unit.isAlive() && unit.isBaby())
			{
				num++;
			}
		}
		return num;
	}

	public virtual IEnumerable<Family> getFamilies()
	{
		_family_counter.Clear();
		try
		{
			foreach (Actor unit in getUnits())
			{
				if (unit.hasFamily() && _family_counter.Add(unit.family))
				{
					yield return unit.family;
				}
			}
		}
		finally
		{
			_family_counter.Clear();
		}
	}

	public virtual bool hasFamilies()
	{
		foreach (Actor unit in getUnits())
		{
			if (unit.hasFamily())
			{
				return true;
			}
		}
		return false;
	}

	public virtual int countFamilies()
	{
		int num = 0;
		foreach (Family family in getFamilies())
		{
			_ = family;
			num++;
		}
		return num;
	}

	public int countKings()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isKing())
			{
				num++;
			}
		}
		return num;
	}

	public int countLeaders()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isCityLeader())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countMales()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAlive() && unit.isSexMale())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countFemales()
	{
		int num = 0;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAlive() && unit.isSexFemale())
			{
				num++;
			}
		}
		return num;
	}

	public virtual int countPopulationPercentage()
	{
		int num = countUnits();
		int count = World.world.units.Count;
		return (int)((float)num / (float)count * 100f);
	}

	public virtual void increaseDeaths(AttackType pType)
	{
		if (!isAlive())
		{
			return;
		}
		ref TData reference = ref data;
		long total_deaths = reference.total_deaths;
		reference.total_deaths = total_deaths + 1;
		if (track_death_types)
		{
			switch (pType)
			{
			case AttackType.Plague:
			{
				ref TData reference18 = ref data;
				total_deaths = reference18.deaths_plague;
				reference18.deaths_plague = total_deaths + 1;
				break;
			}
			case AttackType.Starvation:
			{
				ref TData reference17 = ref data;
				total_deaths = reference17.deaths_hunger;
				reference17.deaths_hunger = total_deaths + 1;
				break;
			}
			case AttackType.Eaten:
			{
				ref TData reference16 = ref data;
				total_deaths = reference16.deaths_eaten;
				reference16.deaths_eaten = total_deaths + 1;
				break;
			}
			case AttackType.Age:
			{
				ref TData reference15 = ref data;
				total_deaths = reference15.deaths_natural;
				reference15.deaths_natural = total_deaths + 1;
				break;
			}
			case AttackType.Poison:
			{
				ref TData reference14 = ref data;
				total_deaths = reference14.deaths_poison;
				reference14.deaths_poison = total_deaths + 1;
				break;
			}
			case AttackType.Infection:
			{
				ref TData reference13 = ref data;
				total_deaths = reference13.deaths_infection;
				reference13.deaths_infection = total_deaths + 1;
				break;
			}
			case AttackType.Tumor:
			{
				ref TData reference12 = ref data;
				total_deaths = reference12.deaths_tumor;
				reference12.deaths_tumor = total_deaths + 1;
				break;
			}
			case AttackType.Acid:
			{
				ref TData reference11 = ref data;
				total_deaths = reference11.deaths_acid;
				reference11.deaths_acid = total_deaths + 1;
				break;
			}
			case AttackType.Fire:
			{
				ref TData reference10 = ref data;
				total_deaths = reference10.deaths_fire;
				reference10.deaths_fire = total_deaths + 1;
				break;
			}
			case AttackType.Divine:
			{
				ref TData reference9 = ref data;
				total_deaths = reference9.deaths_divine;
				reference9.deaths_divine = total_deaths + 1;
				break;
			}
			case AttackType.Metamorphosis:
			{
				ref TData reference8 = ref data;
				total_deaths = reference8.metamorphosis;
				reference8.metamorphosis = total_deaths + 1;
				break;
			}
			case AttackType.Weapon:
			{
				ref TData reference7 = ref data;
				total_deaths = reference7.deaths_weapon;
				reference7.deaths_weapon = total_deaths + 1;
				break;
			}
			case AttackType.Gravity:
			{
				ref TData reference6 = ref data;
				total_deaths = reference6.deaths_gravity;
				reference6.deaths_gravity = total_deaths + 1;
				break;
			}
			case AttackType.Drowning:
			{
				ref TData reference5 = ref data;
				total_deaths = reference5.deaths_drowning;
				reference5.deaths_drowning = total_deaths + 1;
				break;
			}
			case AttackType.Water:
			{
				ref TData reference4 = ref data;
				total_deaths = reference4.deaths_water;
				reference4.deaths_water = total_deaths + 1;
				break;
			}
			case AttackType.Explosion:
			{
				ref TData reference3 = ref data;
				total_deaths = reference3.deaths_explosion;
				reference3.deaths_explosion = total_deaths + 1;
				break;
			}
			default:
			{
				ref TData reference2 = ref data;
				total_deaths = reference2.deaths_other;
				reference2.deaths_other = total_deaths + 1;
				break;
			}
			}
		}
	}

	public virtual void increaseBirths()
	{
		if (isAlive())
		{
			ref TData reference = ref data;
			long total_births = reference.total_births;
			reference.total_births = total_births + 1;
		}
	}

	public virtual void increaseKills()
	{
		if (isAlive())
		{
			ref TData reference = ref data;
			long total_kills = reference.total_kills;
			reference.total_kills = total_kills + 1;
		}
	}

	private void clearCachedVisibleUnit()
	{
		_cached_visible_unit = null;
		_cached_visible_unit_id = -1L;
		_timestamp_last_visible = -1.0;
	}

	public Actor getOldestVisibleUnitForNameplatesCached()
	{
		if (World.world.getWorldTimeElapsedSince(_timestamp_last_visible) > 5f)
		{
			_cached_visible_unit = null;
		}
		if (!_cached_visible_unit.isRekt() && (!_cached_visible_unit.current_zone.visible_main_centered || _cached_visible_unit.id != _cached_visible_unit_id))
		{
			clearCachedVisibleUnit();
		}
		if (_cached_visible_unit != null)
		{
			return _cached_visible_unit;
		}
		_timestamp_last_visible = World.world.getCurWorldTime();
		_cached_visible_unit = getOldestVisibleUnit();
		if (_cached_visible_unit != null)
		{
			_cached_visible_unit_id = _cached_visible_unit.data.id;
		}
		else
		{
			clearCachedVisibleUnit();
		}
		return _cached_visible_unit;
	}

	public Actor getOldestVisibleUnit()
	{
		Actor actor = null;
		foreach (Actor unit in units)
		{
			if (!unit.asset.is_boat && unit.isAlive() && unit.current_zone.visible_main_centered && (actor == null || unit.data.created_time < actor.data.created_time))
			{
				actor = unit;
			}
		}
		return actor;
	}

	public virtual Sprite getTopicSprite()
	{
		throw new NotImplementedException();
	}

	public long getTotalDeaths()
	{
		return data.total_deaths;
	}

	public long getTotalBirths()
	{
		return data.total_births;
	}

	public long getTotalKills()
	{
		return data.total_kills;
	}

	public long getEvolutions()
	{
		return data.evolutions;
	}

	public void increaseEvolutions()
	{
		ref TData reference = ref data;
		long evolutions = reference.evolutions;
		reference.evolutions = evolutions + 1;
	}

	public long getDeaths(AttackType pType)
	{
		switch (pType)
		{
		case AttackType.Plague:
			return data.deaths_plague;
		case AttackType.Starvation:
			return data.deaths_hunger;
		case AttackType.Eaten:
			return data.deaths_eaten;
		case AttackType.Age:
			return data.deaths_natural;
		case AttackType.Poison:
			return data.deaths_poison;
		case AttackType.Infection:
			return data.deaths_infection;
		case AttackType.Tumor:
			return data.deaths_tumor;
		case AttackType.Acid:
			return data.deaths_acid;
		case AttackType.Fire:
			return data.deaths_fire;
		case AttackType.Divine:
			return data.deaths_divine;
		case AttackType.Metamorphosis:
			return data.metamorphosis;
		case AttackType.Weapon:
			return data.deaths_weapon;
		case AttackType.Gravity:
			return data.deaths_gravity;
		case AttackType.Drowning:
			return data.deaths_drowning;
		case AttackType.Water:
			return data.deaths_water;
		case AttackType.Explosion:
			return data.deaths_explosion;
		case AttackType.Other:
		case AttackType.AshFever:
		case AttackType.None:
			return data.deaths_other;
		default:
			throw new ArgumentOutOfRangeException($"Unknown attack type: {pType}");
		}
	}

	public void addRenown(int pAmount)
	{
		ref TData reference = ref data;
		int renown = reference.renown + pAmount;
		reference.renown = renown;
	}

	public void addRenown(int pAmount, float pPercent)
	{
		int pAmount2 = (int)((float)pAmount * pPercent);
		addRenown(pAmount2);
	}

	public virtual void clearLastYearStats()
	{
	}

	public virtual void convertSameSpeciesAroundUnit(Actor pActorMain, bool pOverrideExisting = false)
	{
		throw new NotImplementedException(GetType().Name);
	}

	public virtual void forceConvertSameSpeciesAroundUnit(Actor pActorMain)
	{
		throw new NotImplementedException(GetType().Name);
	}

	public virtual ActorAsset getActorAsset()
	{
		return null;
	}

	public IEnumerable<Actor> getUnitFromChunkForConversion(Actor pActorMain)
	{
		foreach (Actor item in Finder.getUnitsFromChunk(pActorMain.current_tile, 1))
		{
			if (item.isSameSpecies(pActorMain) && (!item.hasCity() || item.hasSameCity(pActorMain)))
			{
				yield return item;
			}
		}
	}

	public Sprite getSpriteIcon()
	{
		return getActorAsset().getSpriteIcon();
	}

	public void allAngryAt(Actor pActorTarget, float pDistance)
	{
		float num = pDistance * pDistance;
		WorldTile current_tile = pActorTarget.current_tile;
		bool flag = pActorTarget.hasStatus("possessed");
		foreach (Actor unit in getUnits())
		{
			if (unit != pActorTarget && !unit.isRekt() && !((float)Toolbox.SquaredDistTile(unit.current_tile, current_tile) > num) && (!flag || !unit.hasStatus("possessed_follower")))
			{
				unit.addAggro(pActorTarget);
			}
		}
	}

	public virtual bool hasUnits()
	{
		foreach (Actor unit in getUnits())
		{
			if (!unit.isRekt() && !unit.asset.is_boat)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void triggerOnRemoveObject()
	{
	}

	public MetaObjectData getMetaData()
	{
		return data;
	}

	public int getRenown()
	{
		return data.renown;
	}

	public virtual int getPopulationPeople()
	{
		return units.Count;
	}

	public virtual bool hasCities()
	{
		throw new NotImplementedException();
	}

	public virtual IEnumerable<City> getCities()
	{
		throw new NotImplementedException();
	}

	public virtual bool hasKingdoms()
	{
		throw new NotImplementedException();
	}

	public virtual IEnumerable<Kingdom> getKingdoms()
	{
		throw new NotImplementedException();
	}
}
