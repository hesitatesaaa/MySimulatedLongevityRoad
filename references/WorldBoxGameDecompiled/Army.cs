using System.Collections.Generic;
using JetBrains.Annotations;

public class Army : MetaObject<ArmyData>
{
	private Actor _captain;

	private WorldTile _prev_captain_position;

	private City _city;

	private Kingdom _kingdom;

	protected override MetaType meta_type => MetaType.Army;

	public override BaseSystemManager manager => World.world.armies;

	public override ActorAsset getActorAsset()
	{
		return getKingdom().getActorAsset();
	}

	public void createArmy(Actor pActor, City pCity)
	{
		_city = pCity;
		_kingdom = _city.kingdom;
		setCaptain(pActor);
		generateNewMetaObject();
		generateName();
	}

	public void checkCity()
	{
		if (_city.kingdom != _kingdom)
		{
			_kingdom = _city.kingdom;
			updateColor(_kingdom?.getColor());
			generateName(_kingdom);
		}
	}

	public void onKingdomNameChange()
	{
		generateName();
	}

	protected override void generateColor()
	{
		if (isAlive())
		{
			Kingdom kingdom = getKingdom();
			if (!kingdom.isRekt())
			{
				data.setColorID(kingdom.data.color_id);
			}
		}
	}

	public override void trackName(bool pPostChange = false)
	{
		if (!string.IsNullOrEmpty(data.name) && (!pPostChange || (data.past_names != null && data.past_names.Count != 0)))
		{
			ArmyData armyData = data;
			if (armyData.past_names == null)
			{
				armyData.past_names = new List<NameEntry>();
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

	private void generateName(Kingdom pKingdom = null)
	{
		if (data.custom_name && !string.IsNullOrEmpty(data.name))
		{
			return;
		}
		Kingdom kingdom = null;
		kingdom = ((pKingdom == null) ? getKingdom() : pKingdom);
		if (kingdom == null)
		{
			setName("Forgotten Army");
			return;
		}
		string text = kingdom.name ?? "";
		string text2 = data.name;
		if (text2 != null && text2.StartsWith(text + " "))
		{
			return;
		}
		using ListPool<string> listPool = new ListPool<string>(World.world.armies.Count);
		foreach (Army army in World.world.armies)
		{
			if (army != this && army.getKingdom() == kingdom)
			{
				listPool.Add(army.name);
			}
		}
		int num = 1;
		string text4;
		while (true)
		{
			string text3 = num.ToRoman();
			text4 = text + " " + text3;
			if (!listPool.Contains(text4))
			{
				break;
			}
			num++;
		}
		setName(text4);
	}

	public Actor getCaptain()
	{
		return _captain;
	}

	public override void save()
	{
		base.save();
		data.id_city = _city?.id ?? (-1);
		data.id_kingdom = _city?.kingdom?.id ?? _kingdom?.id ?? (-1);
		data.id_captain = (hasCaptain() ? _captain.data.id : (-1));
	}

	public override void loadData(ArmyData pData)
	{
		base.loadData(pData);
		_city = World.world.cities.get(pData.id_city);
		if (_city != null)
		{
			_city.setArmy(this);
		}
		_kingdom = World.world.kingdoms.get(pData.id_kingdom);
		if (string.IsNullOrEmpty(name))
		{
			generateName();
		}
	}

	public void loadDataCaptains()
	{
		Actor actor = World.world.units.get(data.id_captain);
		if (actor != null && actor.army == this)
		{
			setCaptain(actor, pFromLoad: true);
		}
		updateColor(getColor());
	}

	public override void generateBanner()
	{
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.kingdom_colors_library;
	}

	public override ColorAsset getColor()
	{
		return getKingdom().getColor();
	}

	public void clearCity()
	{
		_city = null;
		data.id_city = -1L;
	}

	public void disband()
	{
		for (int i = 0; i < base.units.Count; i++)
		{
			base.units[i].stopBeingWarrior();
		}
		setCaptain(null);
	}

	public void updateCaptains()
	{
		if (data.past_captains == null || data.past_captains.Count == 0)
		{
			return;
		}
		foreach (LeaderEntry past_captain in data.past_captains)
		{
			Actor actor = World.world.units.get(past_captain.id);
			if (!actor.isRekt())
			{
				past_captain.name = actor.name;
			}
		}
	}

	public void addCaptain(Actor pActor)
	{
		ArmyData armyData = data;
		if (armyData.past_captains == null)
		{
			armyData.past_captains = new List<LeaderEntry>();
		}
		captainLeft();
		data.past_captains.Add(new LeaderEntry
		{
			id = pActor.getID(),
			name = pActor.name,
			color_id = (getKingdom()?.data.color_id ?? data.color_id),
			timestamp_ago = World.world.getCurWorldTime()
		});
		if (data.past_captains.Count > 30)
		{
			data.past_captains.Shift();
		}
	}

	public void captainLeft()
	{
		if (data.past_captains != null && data.past_captains.Count != 0)
		{
			LeaderEntry leaderEntry = data.past_captains.Last();
			if (!(leaderEntry.timestamp_end >= leaderEntry.timestamp_ago))
			{
				leaderEntry.timestamp_end = World.world.getCurWorldTime();
				updateCaptains();
			}
		}
	}

	public void setCaptain(Actor pActor, bool pFromLoad = false)
	{
		_captain = pActor;
		if (data == null)
		{
			return;
		}
		if (pActor.isRekt())
		{
			data.id_captain = -1L;
			if (!pFromLoad)
			{
				captainLeft();
			}
		}
		else
		{
			data.id_captain = pActor.getID();
			if (!pFromLoad)
			{
				addCaptain(pActor);
			}
		}
	}

	public void checkCaptainExistence()
	{
		Actor captain = getCaptain();
		if (!captain.isRekt() && captain.current_tile != null)
		{
			_prev_captain_position = captain.current_tile;
		}
		if (captain.isRekt())
		{
			setCaptain(null);
		}
		findCaptain();
	}

	public void checkCaptainRemoval(Actor pActor)
	{
		if (_captain == pActor)
		{
			setCaptain(null);
		}
	}

	public int countMelee()
	{
		int num = 0;
		for (int i = 0; i < base.units.Count; i++)
		{
			Actor actor = base.units[i];
			if (actor.isAlive())
			{
				if (!actor.hasWeapon())
				{
					num++;
				}
				else if (actor.getWeaponAsset().attack_type == WeaponType.Melee)
				{
					num++;
				}
			}
		}
		return num;
	}

	public int countRange()
	{
		int num = 0;
		for (int i = 0; i < base.units.Count; i++)
		{
			Actor actor = base.units[i];
			if (actor.isAlive() && actor.hasWeapon() && actor.getWeaponAsset().attack_type == WeaponType.Range)
			{
				num++;
			}
		}
		return num;
	}

	public bool isGroupInCityAndHaveLeader()
	{
		if (!isAlive())
		{
			return false;
		}
		if (base.units.Count == 0)
		{
			return true;
		}
		if (!hasCaptain())
		{
			return false;
		}
		Actor captain = getCaptain();
		if (captain.isInsideSomething())
		{
			return false;
		}
		if (!captain.current_zone.isSameCityHere(_city))
		{
			return false;
		}
		return true;
	}

	private void findCaptain()
	{
		if (isLocked())
		{
			return;
		}
		if (hasCaptain())
		{
			if (getCaptain().isKingdomCiv())
			{
				return;
			}
			setCaptain(null);
		}
		if (base.units.Count != 0)
		{
			Actor actor = null;
			actor = ((_prev_captain_position != null) ? getNearbyUnitForCaptain(_prev_captain_position) : getRandomActorForCaptain());
			if (actor != null)
			{
				setCaptain(actor);
			}
		}
	}

	private Actor getRandomActorForCaptain()
	{
		foreach (Actor item in base.units.LoopRandom())
		{
			if (!item.isRekt() && item.army == this)
			{
				return item;
			}
		}
		return null;
	}

	private Actor getNearbyUnitForCaptain(WorldTile pLastPosition)
	{
		Actor result = null;
		int num = int.MaxValue;
		List<Actor> list = base.units;
		for (int i = 0; i < list.Count; i++)
		{
			Actor actor = list[i];
			if (actor.army == this && !actor.isRekt())
			{
				int num2 = Toolbox.SquaredDistTile(actor.current_tile, pLastPosition);
				if (num2 < num)
				{
					result = actor;
					num = num2;
				}
			}
		}
		return result;
	}

	public string getDebug()
	{
		string text = base.units.Count.ToString() ?? "";
		if (_captain != null)
		{
			text = text + " " + _captain.getName() + "(" + _captain.getAge() + ")";
		}
		return text;
	}

	[CanBeNull]
	public Kingdom getKingdom()
	{
		Kingdom kingdom = null;
		if (hasCaptain())
		{
			kingdom = getCaptain().kingdom;
		}
		if (kingdom == null)
		{
			kingdom = (_city.isRekt() ? _kingdom : _city.kingdom);
		}
		return kingdom;
	}

	public bool hasKingdom()
	{
		return !_kingdom.isRekt();
	}

	public bool hasCaptain()
	{
		return !_captain.isRekt();
	}

	public City getCity()
	{
		return _city;
	}

	public bool hasCity()
	{
		return !_city.isRekt();
	}

	public override bool isReadyForRemoval()
	{
		if (base.units.Count > 0)
		{
			return false;
		}
		if (hasCaptain())
		{
			return false;
		}
		if (hasCity())
		{
			return false;
		}
		if (!base.isReadyForRemoval())
		{
			return false;
		}
		return true;
	}

	public override void Dispose()
	{
		base.Dispose();
		base.units.Clear();
		_captain = null;
		_prev_captain_position = null;
		_city = null;
		_kingdom = null;
	}

	public override string ToString()
	{
		if (data == null)
		{
			return "[Army is null]";
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append($"[Army:{base.id} ");
		if (!isAlive())
		{
			stringBuilderPool.Append("[DEAD] ");
		}
		stringBuilderPool.Append("\"" + name + "\" ");
		Kingdom kingdom = getKingdom();
		stringBuilderPool.Append($"Kingdom:{kingdom?.id ?? (-1)} ");
		if (hasCity())
		{
			stringBuilderPool.Append($"{_city} ");
		}
		if (kingdom != _kingdom)
		{
			stringBuilderPool.Append($"_kingdom:{_kingdom} ");
		}
		if (hasCaptain())
		{
			stringBuilderPool.Append($"Captain:{_captain?.id ?? (-1)} ");
			if (_captain?.kingdom != kingdom)
			{
				stringBuilderPool.Append($"CaptainKingdom:{_captain?.kingdom?.id ?? (-1)} ");
			}
		}
		stringBuilderPool.Append($"Units:{base.units.Count} ");
		if (manager.isUnitsDirty())
		{
			stringBuilderPool.Append("[Dirty] ");
		}
		return stringBuilderPool.ToString().Trim() + "]";
	}
}
