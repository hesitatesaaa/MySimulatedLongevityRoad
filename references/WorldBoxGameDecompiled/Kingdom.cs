using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using db;

public class Kingdom : MetaObjectWithTraits<KingdomData, KingdomTrait>
{
	public static KingdomCheckCache cache_enemy_check = new KingdomCheckCache();

	public KingdomAsset asset;

	public bool wild;

	public float timer_action;

	public Actor king;

	public City capital;

	public Culture culture;

	public Language language;

	public Religion religion;

	public readonly List<Building> buildings = new List<Building>();

	public readonly List<City> cities = new List<City>();

	public int power;

	public AiSystemKingdom ai;

	public Vector3 location;

	private float _cached_tax_local;

	private float _cached_tax_tribute;

	private bool _has_boats;

	protected override MetaType meta_type => MetaType.Kingdom;

	public override BaseSystemManager manager => World.world.kingdoms;

	protected override bool track_death_types => true;

	protected override AssetLibrary<KingdomTrait> trait_library => AssetManager.kingdoms_traits;

	protected override List<string> default_traits => getActorAsset().default_kingdom_traits;

	protected override List<string> saved_traits => data.saved_traits;

	[Obsolete("use .getColor() instead", false)]
	public ColorAsset kingdomColor => getColor();

	protected override void recalcBaseStats()
	{
		base.recalcBaseStats();
		_cached_tax_local = SimGlobals.m.base_tax_rate_local;
		_cached_tax_tribute = SimGlobals.m.base_tax_rate_tribute;
		foreach (KingdomTrait trait in getTraits())
		{
			if (trait.is_local_tax_trait)
			{
				_cached_tax_local = trait.tax_rate;
			}
			if (trait.is_tribute_tax_trait)
			{
				_cached_tax_tribute = trait.tax_rate;
			}
		}
	}

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
		power = 1;
		timer_action = 5f;
	}

	protected override ColorLibrary getColorLibrary()
	{
		return AssetManager.kingdom_colors_library;
	}

	public void clearListCities()
	{
		cities.Clear();
	}

	public void clearBuildingList()
	{
		buildings.Clear();
	}

	public override void increaseDeaths(AttackType pType)
	{
		if (isAlive())
		{
			base.increaseDeaths(pType);
			if (hasAlliance())
			{
				getAlliance().increaseDeaths(pType);
			}
		}
	}

	public override void increaseKills()
	{
		if (isAlive())
		{
			base.increaseKills();
			if (hasAlliance())
			{
				getAlliance().increaseKills();
			}
		}
	}

	public override void increaseBirths()
	{
		if (isAlive())
		{
			base.increaseBirths();
			if (hasAlliance())
			{
				getAlliance().increaseBirths();
			}
			addRenown(1);
		}
	}

	public void increaseLeft()
	{
		if (isAlive())
		{
			data.left++;
		}
	}

	public void increaseJoined()
	{
		if (isAlive())
		{
			data.joined++;
			addRenown(1);
		}
	}

	public void increaseMoved()
	{
		if (isAlive())
		{
			data.moved++;
			addRenown(2);
		}
	}

	public void increaseMigrants()
	{
		if (isAlive())
		{
			data.migrated++;
		}
	}

	public long getTotalLeft()
	{
		return data.left;
	}

	public long getTotalJoined()
	{
		return data.joined;
	}

	public long getTotalMoved()
	{
		return data.moved;
	}

	public long getTotalMigrated()
	{
		return data.migrated;
	}

	public override bool isReadyForRemoval()
	{
		if (buildings.Count > 0)
		{
			return false;
		}
		if (getPopulationTotal() > 0)
		{
			return false;
		}
		if (hasCities())
		{
			return false;
		}
		if (World.world.projectiles.hasActiveProjectiles(this))
		{
			return false;
		}
		if (!base.isReadyForRemoval())
		{
			return false;
		}
		return true;
	}

	public bool hasBuildings()
	{
		return buildings.Count > 0;
	}

	public void addBuildings(List<Building> pListBuildings)
	{
		buildings.AddRange(pListBuildings);
	}

	public void listCity(City pCity)
	{
		cities.Add(pCity);
	}

	public void listBuilding(Building pBuilding)
	{
		buildings.Add(pBuilding);
	}

	public Subspecies getMainSubspecies()
	{
		if (hasKing())
		{
			return king.subspecies;
		}
		if (base.units.Count == 0)
		{
			return null;
		}
		return base.units[0].subspecies;
	}

	public void createWildKingdom()
	{
		asset.default_kingdom_color.initColor();
		wild = true;
	}

	public void createAI()
	{
		if (Globals.AI_TEST_ACTIVE)
		{
			if (ai == null)
			{
				ai = new AiSystemKingdom(this);
			}
			ai.next_job_delegate = getNextJob;
			ai.jobs_library = AssetManager.job_kingdom;
			ai.task_library = AssetManager.tasks_kingdom;
		}
	}

	public bool isOpinionTowardsKingdomGood(Kingdom pKingdom)
	{
		if (this == pKingdom)
		{
			return true;
		}
		if (World.world.diplomacy.getOpinion(this, pKingdom).total >= 0)
		{
			return true;
		}
		return false;
	}

	public string getNextJob()
	{
		return "kingdom";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isCiv()
	{
		return asset.civ;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isMobs()
	{
		return asset.mobs;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isNeutral()
	{
		return asset.neutral;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isNature()
	{
		return asset.nature;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isNomads()
	{
		return asset.nomads;
	}

	public override void save()
	{
		base.save();
		if (hasCulture())
		{
			data.id_culture = culture.id;
		}
		if (hasReligion())
		{
			data.id_religion = religion.id;
		}
		if (hasLanguage())
		{
			data.id_language = language.id;
		}
		if (hasKing())
		{
			data.kingID = king.data.id;
		}
		else
		{
			data.kingID = -1L;
		}
		data.saved_traits = getTraitsAsStrings();
	}

	public IEnumerable<War> getWars(bool pRandom = false)
	{
		return World.world.wars.getWars(this, pRandom);
	}

	public bool isAttacker()
	{
		foreach (War war in getWars())
		{
			if (war.isAttacker(this))
			{
				return true;
			}
		}
		return false;
	}

	public bool isDefender()
	{
		foreach (War war in getWars())
		{
			if (war.isDefender(this))
			{
				return true;
			}
		}
		return false;
	}

	public bool isInWarWith(Kingdom pKingdom)
	{
		return World.world.wars.isInWarWith(this, pKingdom);
	}

	public bool isInWarOnSameSide(Kingdom pKingdom)
	{
		foreach (War war in getWars())
		{
			if (war.onTheSameSide(pKingdom, this))
			{
				return true;
			}
		}
		return false;
	}

	public bool isEnemy(Kingdom pKingdomTarget)
	{
		if (pKingdomTarget == null)
		{
			return true;
		}
		long hash = cache_enemy_check.getHash(this, pKingdomTarget);
		if (cache_enemy_check.dict.TryGetValue(hash, out var value))
		{
			return value;
		}
		if (isCiv() && pKingdomTarget.isCiv())
		{
			if (pKingdomTarget == this)
			{
				cache_enemy_check.dict[hash] = false;
				return false;
			}
			if (World.world.wars.isInWarWith(this, pKingdomTarget))
			{
				cache_enemy_check.dict[hash] = true;
				return true;
			}
			cache_enemy_check.dict[hash] = false;
			return false;
		}
		if (asset.isFoe(pKingdomTarget.asset))
		{
			cache_enemy_check.dict[hash] = true;
			return true;
		}
		cache_enemy_check.dict[hash] = false;
		return false;
	}

	public bool isGettingCaptured()
	{
		foreach (City city in getCities())
		{
			if (city.isGettingCaptured())
			{
				return true;
			}
		}
		return false;
	}

	public override ColorAsset getColor()
	{
		if (isCiv())
		{
			return base.getColor();
		}
		return asset.default_kingdom_color;
	}

	internal void newCivKingdom(Actor pActor)
	{
		asset = AssetManager.kingdoms.get(pActor.asset.kingdom_id_civilization);
		data.original_actor_asset = pActor.asset.id;
		string pName = pActor.generateName(MetaType.Kingdom, getID());
		setName(pName);
		data.name_culture_id = culture?.id ?? (-1);
		generateNewMetaObject();
	}

	public override ActorAsset getActorAsset()
	{
		if (hasKing())
		{
			return king.getActorAsset();
		}
		return getFounderSpecies();
	}

	public ActorAsset getFounderSpecies()
	{
		return AssetManager.actor_library.get(data.original_actor_asset);
	}

	public string getSpecies()
	{
		if (string.IsNullOrEmpty(data.original_actor_asset))
		{
			return null;
		}
		return getActorAsset()?.id;
	}

	public void trySetRoyalClan()
	{
		if (hasKing() && king.hasClan() && king.clan.id != data.royal_clan_id)
		{
			long royal_clan_id = data.royal_clan_id;
			Clan clan = World.world.clans.get(royal_clan_id);
			if (clan != null && clan.isAlive())
			{
				logNewRoyalClanChanged(clan, king.clan);
			}
			else if (king.clan.getRenown() >= 10)
			{
				logNewRoyalClan(king.clan);
			}
			data.royal_clan_id = king.clan.id;
		}
	}

	public void checkEndWar()
	{
		data.timestamp_last_war = World.world.getCurWorldTime();
	}

	public void madePeace(War pWar)
	{
		int pAmount = (int)((float)pWar.getRenown() * 0.25f);
		addRenown(pAmount);
		foreach (Actor unit in getUnits())
		{
			unit.madePeace(pWar);
		}
		if (hasAlliance())
		{
			getAlliance().addRenown(pAmount);
		}
	}

	public void wonWar(War pWar)
	{
		addRenown(pWar.getRenown());
		foreach (Actor unit in getUnits())
		{
			unit.warWon(pWar);
		}
		if (hasAlliance())
		{
			getAlliance().addRenown(pWar.getRenown());
		}
	}

	public void lostWar(War pWar)
	{
		int pAmount = (int)((float)pWar.getRenown() * 0.1f);
		addRenown(pAmount);
		foreach (Actor unit in getUnits())
		{
			unit.warLost(pWar);
		}
		if (hasAlliance())
		{
			getAlliance().addRenown(pAmount);
		}
	}

	internal void updateCiv(float pElapsed)
	{
		if (data.timer_new_king > 0f)
		{
			data.timer_new_king -= pElapsed;
		}
		if (ai != null)
		{
			if (timer_action > 0f)
			{
				timer_action -= pElapsed;
			}
			else
			{
				ai.update();
			}
		}
	}

	public void setCapital(City pCity)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		capital = pCity;
		if (capital != null && capital.isAlive())
		{
			KingdomData kingdomData = data;
			long capitalID = (data.last_capital_id = pCity.data.id);
			kingdomData.capitalID = capitalID;
			location = Vector2.op_Implicit(capital.city_center);
		}
		else
		{
			data.capitalID = -1L;
		}
	}

	public void setKing(Actor pActor, bool pFromLoad = false)
	{
		king = pActor;
		king.setProfession(UnitProfession.King);
		if (!pFromLoad)
		{
			data.total_kings++;
			addRuler(pActor);
			data.timestamp_king_rule = World.world.getCurWorldTime();
			king.changeHappiness("become_king");
		}
		trySetRoyalClan();
	}

	internal void kingLeftEvent()
	{
		if (hasKing())
		{
			if (king.isAlive())
			{
				king.changeHappiness("lost_crown");
			}
			logKingLeft(king);
			removeKing();
		}
	}

	internal void kingFledCity()
	{
		if (hasKing())
		{
			if (king.city.isCapitalCity())
			{
				logKingFledCapital(king);
			}
			else
			{
				logKingFledCity(king);
			}
			king.setCity(null);
		}
	}

	internal void removeKing()
	{
		if (!king.isRekt())
		{
			king.setProfession(UnitProfession.Unit);
		}
		rulerLeft();
		king = null;
		data.timer_new_king = Randy.randomFloat(5f, 20f);
	}

	public void updateRulers()
	{
		if (data.past_rulers == null || data.past_rulers.Count == 0)
		{
			return;
		}
		foreach (LeaderEntry past_ruler in data.past_rulers)
		{
			Actor actor = World.world.units.get(past_ruler.id);
			if (!actor.isRekt())
			{
				past_ruler.name = actor.name;
			}
		}
	}

	public void addRuler(Actor pActor)
	{
		KingdomData kingdomData = data;
		if (kingdomData.past_rulers == null)
		{
			kingdomData.past_rulers = new List<LeaderEntry>();
		}
		rulerLeft();
		data.past_rulers.Add(new LeaderEntry
		{
			id = pActor.getID(),
			name = pActor.name,
			color_id = data.color_id,
			timestamp_ago = World.world.getCurWorldTime()
		});
		if (data.past_rulers.Count > 30)
		{
			data.past_rulers.Shift();
		}
	}

	public void rulerLeft()
	{
		if (data.past_rulers != null && data.past_rulers.Count != 0)
		{
			LeaderEntry leaderEntry = data.past_rulers.Last();
			if (!(leaderEntry.timestamp_end >= leaderEntry.timestamp_ago))
			{
				leaderEntry.timestamp_end = World.world.getCurWorldTime();
				updateRulers();
			}
		}
	}

	public void logKingDead(Actor pActor)
	{
		if (!pActor.attackedBy.isRekt() && pActor.attackedBy.isActor())
		{
			WorldLog.logKingMurder(this, pActor, pActor.attackedBy.a);
		}
		else
		{
			WorldLog.logKingDead(this, pActor);
		}
	}

	public void logKingFledCapital(Actor pActor)
	{
		WorldLog.logKingFledCapital(this, pActor);
	}

	public void logKingFledCity(Actor pActor)
	{
		WorldLog.logKingFledCity(this, pActor);
	}

	public void logKingLeft(Actor pActor)
	{
		WorldLog.logKingLeft(this, pActor);
	}

	public void logNewRoyalClanChanged(Clan pOldClan, Clan pNewClan)
	{
		WorldLog.logRoyalClanChanged(this, pOldClan, pNewClan);
	}

	public void logNewRoyalClan(Clan pClan)
	{
		WorldLog.logRoyalClanNew(this, pClan);
	}

	public void logRoyalClanLost(Clan pClan)
	{
		WorldLog.logRoyalClanNoMore(this, pClan);
	}

	internal void checkClearCapital(City pCity)
	{
		if (pCity.isCapitalCity())
		{
			clearCapital();
		}
	}

	public void clearCapital()
	{
		data.capitalID = -1L;
		capital = null;
	}

	public bool hasNearbyKingdoms()
	{
		foreach (City city in getCities())
		{
			if (city.neighbours_kingdoms.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public void capturedFrom(Kingdom pKingdom)
	{
		World.world.diplomacy.getRelation(this, pKingdom);
	}

	public virtual string getMotto()
	{
		if (string.IsNullOrEmpty(data.motto))
		{
			data.motto = NameGenerator.getName("kingdom_mottos");
		}
		return data.motto;
	}

	public override void generateBanner()
	{
		BannerAsset bannerAsset = AssetManager.kingdom_banners_library.get(getActorAsset().banner_id);
		data.banner_icon_id = Randy.randomInt(0, bannerAsset.icons.Count);
		data.banner_background_id = Randy.randomInt(0, bannerAsset.backgrounds.Count);
	}

	public override void loadData(KingdomData pData)
	{
		base.loadData(pData);
		if (data.id_culture.hasValue())
		{
			setCulture(World.world.cultures.get(data.id_culture));
		}
		if (data.id_language.hasValue())
		{
			setLanguage(World.world.languages.get(data.id_language));
		}
		if (data.id_religion.hasValue())
		{
			setReligion(World.world.religions.get(data.id_religion));
		}
		ActorAsset actorAsset = getActorAsset();
		asset = AssetManager.kingdoms.get(actorAsset.kingdom_id_civilization);
	}

	internal void load2()
	{
		City city = World.world.cities.get(data.capitalID);
		if (city != null)
		{
			setCapital(city);
		}
		if (data.kingID.hasValue())
		{
			Actor actor = World.world.units.get(data.kingID);
			if (actor != null)
			{
				setKing(actor, pFromLoad: true);
				actor.setProfession(UnitProfession.King);
			}
		}
	}

	public override bool updateColor(ColorAsset pColor)
	{
		bool flag = base.updateColor(pColor);
		if (flag)
		{
			foreach (Building building in buildings)
			{
				building.updateKingdomColors();
			}
		}
		return flag;
	}

	public static float distanceBetweenKingdom(Kingdom pKingdom, Kingdom pTarget)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!pKingdom.hasCities() || !pTarget.hasCities())
		{
			return -1f;
		}
		float num = float.MaxValue;
		using ListPool<Vector2> listPool = new ListPool<Vector2>();
		using ListPool<Vector2> listPool2 = new ListPool<Vector2>();
		foreach (City city in pKingdom.getCities())
		{
			listPool.Add(city.city_center);
		}
		foreach (City city2 in pTarget.getCities())
		{
			listPool2.Add(city2.city_center);
		}
		foreach (ref Vector2 item in listPool)
		{
			Vector2 current3 = item;
			foreach (ref Vector2 item2 in listPool2)
			{
				Vector2 current4 = item2;
				float num2 = Toolbox.SquaredDistVec2Float(current3, current4);
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public override IEnumerable<City> getCities()
	{
		if (World.world.kingdoms.hasDirtyCities())
		{
			foreach (City city in World.world.cities)
			{
				if (!city.isRekt() && city.kingdom == this)
				{
					yield return city;
				}
			}
			yield break;
		}
		foreach (City city2 in cities)
		{
			if (!city2.isRekt())
			{
				yield return city2;
			}
		}
	}

	public void clear()
	{
		buildings.Clear();
		cities.Clear();
		base.units.Clear();
		cache_enemy_check.clear();
		clearCapital();
	}

	public override void Dispose()
	{
		DBInserter.deleteData(getID(), "kingdom");
		clear();
		asset = null;
		king = null;
		capital = null;
		culture = null;
		language = null;
		religion = null;
		ai?.reset();
		base.Dispose();
	}

	public bool hasEnemies()
	{
		return World.world.wars.hasWars(this);
	}

	public ListPool<Kingdom> getEnemiesKingdoms()
	{
		return World.world.wars.getEnemiesOf(this);
	}

	public void makeSurvivorsToNomads()
	{
		if (base.units.Count == 0)
		{
			return;
		}
		for (int i = 0; i < base.units.Count; i++)
		{
			Actor actor = base.units[i];
			if (actor.isAlive())
			{
				if (actor.asset.is_boat)
				{
					actor.getHitFullHealth(AttackType.None);
					continue;
				}
				actor.cancelAllBeh();
				actor.removeFromPreviousFaction();
				actor.joinKingdom(World.world.kingdoms_wild.get(actor.asset.kingdom_id_wild));
			}
		}
		base.units.Clear();
	}

	public void clearKingData()
	{
		king = null;
	}

	public void updateAge()
	{
		if (hasKing() && king.hasClan())
		{
			king.clan.addRenown(1);
		}
	}

	public override int countCouples()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countCouples();
		}
		return num;
	}

	public override int countSingleMales()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countSingleMales();
		}
		return num;
	}

	public override int countSingleFemales()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countSingleFemales();
		}
		return num;
	}

	public int countZones()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countZones();
		}
		return num;
	}

	public int countBuildings()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countBuildings();
		}
		return num;
	}

	public int countCities()
	{
		if (!World.world.kingdoms.hasDirtyCities())
		{
			return cities.Count;
		}
		int num = 0;
		foreach (City city in getCities())
		{
			_ = city;
			num++;
		}
		return num;
	}

	public override int getPopulationPeople()
	{
		if (!_has_boats)
		{
			return base.units.Count;
		}
		int num = 0;
		int num2 = 0;
		foreach (City city in getCities())
		{
			num += city.getPopulationPeople();
			num2 += city.countBoats();
		}
		if (num + num2 == base.units.Count)
		{
			return num;
		}
		num = 0;
		foreach (Actor unit in getUnits())
		{
			if (!unit.asset.is_boat)
			{
				num++;
			}
		}
		return num;
	}

	public override int countUnits()
	{
		return getPopulationPeople();
	}

	public override IEnumerable<Actor> getUnits()
	{
		foreach (Actor unit in base.units)
		{
			if (unit.isAlive() && !unit.asset.is_boat && unit.kingdom == this)
			{
				yield return unit;
			}
		}
	}

	public override Actor getRandomUnit()
	{
		foreach (Actor item in base.units.LoopRandom())
		{
			if (item.isAlive() && !item.asset.is_boat && item.kingdom == this)
			{
				return item;
			}
		}
		return null;
	}

	public int getPopulationTotal()
	{
		return base.units.Count;
	}

	public int countBoats()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countBoats();
		}
		return num;
	}

	public int getPopulationTotalPossible()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.getPopulationMaximum();
		}
		return num;
	}

	public int countWeapons()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countWeapons();
		}
		return num;
	}

	public int countTotalFood()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.getTotalFood();
		}
		return num;
	}

	public int countTotalWarriors()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.countWarriors();
		}
		return num;
	}

	public int countWarriorsMax()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			num += city.getMaxWarriors();
		}
		return num;
	}

	public int getMaxCities()
	{
		int num = getActorAsset().civ_base_cities;
		if (hasKing())
		{
			num += (int)king.stats["cities"];
		}
		if (num < 1)
		{
			num = 1;
		}
		return num;
	}

	public bool diceAgressionSuccess()
	{
		if (!hasKing())
		{
			return false;
		}
		int num = countCities();
		if (num < getMaxCities())
		{
			return true;
		}
		if (num >= getMaxCities() && Randy.randomChance(king.stats["personality_aggression"]))
		{
			return true;
		}
		return false;
	}

	public bool isSupreme()
	{
		return DiplomacyManager.kingdom_supreme == this;
	}

	public bool isSecondBest()
	{
		return DiplomacyManager.kingdom_second == this;
	}

	public bool hasAlliance()
	{
		return getAlliance() != null;
	}

	public Alliance getAlliance()
	{
		if (!data.allianceID.hasValue())
		{
			return null;
		}
		Alliance alliance = World.world.alliances.get(data.allianceID);
		if (alliance == null)
		{
			data.allianceID = -1L;
		}
		return alliance;
	}

	public void allianceLeave(Alliance pAlliance)
	{
		data.allianceID = -1L;
		data.timestamp_alliance = World.world.getCurWorldTime();
	}

	public void allianceJoin(Alliance pAlliance)
	{
		data.allianceID = pAlliance.data.id;
		data.timestamp_alliance = World.world.getCurWorldTime();
	}

	public void calculateNeighbourCities()
	{
		foreach (City city in getCities())
		{
			city.recalculateNeighbourCities();
		}
	}

	public Culture getCulture()
	{
		return culture;
	}

	public void setCulture(Culture pCulture)
	{
		if (culture != pCulture)
		{
			culture = pCulture;
			World.world.cultures.setDirtyKingdoms();
		}
	}

	public bool hasCulture()
	{
		if (culture != null && !culture.isAlive())
		{
			setCulture(null);
		}
		return culture != null;
	}

	public void setLanguage(Language pLanguage)
	{
		language = pLanguage;
		World.world.languages.setDirtyKingdoms();
	}

	public Language getLanguage()
	{
		return language;
	}

	public bool hasLanguage()
	{
		if (language != null && !language.isAlive())
		{
			setLanguage(null);
		}
		return language != null;
	}

	public void setReligion(Religion pReligion)
	{
		if (religion != pReligion)
		{
			religion = pReligion;
			World.world.religions.setDirtyKingdoms();
		}
	}

	public Religion getReligion()
	{
		return religion;
	}

	public bool hasReligion()
	{
		if (religion != null && !religion.isAlive())
		{
			setReligion(null);
		}
		return religion != null;
	}

	public bool isEnemyAroundZone(TileZone pZone)
	{
		TileZone[] neighbours = pZone.neighbours;
		foreach (TileZone tileZone in neighbours)
		{
			if (tileZone.city == null)
			{
				return true;
			}
			Kingdom kingdom = tileZone.city.kingdom;
			if (kingdom != this)
			{
				return true;
			}
			if (kingdom != this && kingdom.isEnemy(this))
			{
				return true;
			}
		}
		return false;
	}

	public override bool hasCities()
	{
		using (IEnumerator<City> enumerator = getCities().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	public bool hasCapital()
	{
		return capital != null;
	}

	public bool hasKing()
	{
		if (king == null)
		{
			return false;
		}
		if (!king.isAlive())
		{
			removeKing();
			return false;
		}
		return true;
	}

	public void affectKingByPowers()
	{
		if (hasKing())
		{
			king.addStatusEffect("voices_in_my_head");
		}
	}

	public int countUnhappyCities()
	{
		int num = 0;
		foreach (City city in getCities())
		{
			if (!city.isHappy())
			{
				num++;
			}
		}
		return num;
	}

	public Sprite getSpeciesIcon()
	{
		return getActorAsset().getSpriteIcon();
	}

	public Sprite getElementIcon()
	{
		return AssetManager.kingdom_banners_library.getSpriteIcon(data.banner_icon_id, getActorAsset().banner_id);
	}

	public Sprite getElementBackground()
	{
		return AssetManager.kingdom_banners_library.getSpriteBackground(data.banner_background_id, getActorAsset().banner_id);
	}

	public void increaseHappinessFromNewCityCapture()
	{
		foreach (Actor unit in getUnits())
		{
			if (!unit.hasHappinessEntry("was_conquered", 400f))
			{
				unit.changeHappiness("conquered_city");
			}
		}
	}

	public void increaseHappinessFromDestroyingCity()
	{
		foreach (Actor unit in getUnits())
		{
			if (!unit.hasHappinessEntry("was_conquered", 400f))
			{
				unit.changeHappiness("destroyed_city");
			}
		}
	}

	public void decreaseHappinessFromLostCityCapture(City pCity)
	{
		foreach (Actor unit in base.units)
		{
			if (!unit.hasHappinessEntry("was_conquered", 400f))
			{
				if (pCity.isCapitalCity())
				{
					unit.changeHappiness("lost_capital");
				}
				else
				{
					unit.changeHappiness("lost_city");
				}
			}
		}
	}

	public void decreaseHappinessFromRazedCity(City pCity)
	{
		foreach (Actor unit in base.units)
		{
			if (!unit.hasHappinessEntry("was_conquered", 400f))
			{
				if (pCity.isCapitalCity())
				{
					unit.changeHappiness("razed_capital");
				}
				else
				{
					unit.changeHappiness("razed_city");
				}
			}
		}
	}

	public int getLootMin()
	{
		return 5;
	}

	public float getTaxRateTribute()
	{
		return _cached_tax_tribute;
	}

	public float getTaxRateLocal()
	{
		return _cached_tax_local;
	}

	public void copyMetasFromOtherKingdom(Kingdom pKingdom)
	{
		if (pKingdom.hasCulture())
		{
			setCulture(pKingdom.culture);
		}
		if (pKingdom.hasLanguage())
		{
			setLanguage(pKingdom.language);
		}
		if (pKingdom.hasReligion())
		{
			setReligion(pKingdom.religion);
		}
	}

	public void setUnitMetas(Actor pActor)
	{
		if (pActor.hasCulture())
		{
			setCulture(pActor.culture);
		}
		if (pActor.hasLanguage())
		{
			setLanguage(pActor.language);
		}
		if (pActor.hasReligion())
		{
			setReligion(pActor.religion);
		}
	}

	public void setCityMetas(City pCity)
	{
		if (pCity.hasCulture())
		{
			setCulture(pCity.culture);
		}
		if (pCity.hasLanguage())
		{
			setLanguage(pCity.language);
		}
		if (pCity.hasReligion())
		{
			setReligion(pCity.religion);
		}
	}

	public Clan getKingClan()
	{
		if (hasKing() && king.hasClan())
		{
			return king.clan;
		}
		return null;
	}

	public override void listUnit(Actor pActor)
	{
		if (pActor.asset.is_boat)
		{
			_has_boats = true;
		}
		base.listUnit(pActor);
	}

	internal override void clearListUnits()
	{
		_has_boats = false;
		base.clearListUnits();
	}

	public override string ToString()
	{
		if (data == null)
		{
			return "[Kingdom is null]";
		}
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		stringBuilderPool.Append($"[Kingdom:{base.id} ");
		if (!isAlive())
		{
			stringBuilderPool.Append("[DEAD] ");
		}
		stringBuilderPool.Append("\"" + name + "\" ");
		stringBuilderPool.Append($"Cities:{cities.Count} ");
		if (World.world.kingdoms.hasDirtyCities())
		{
			stringBuilderPool.Append($" [Dirty:{countCities()}] ");
		}
		stringBuilderPool.Append($"Units:{base.units.Count} ");
		if (isDirtyUnits())
		{
			stringBuilderPool.Append("[Dirty] ");
		}
		if (hasKing())
		{
			stringBuilderPool.Append($"King:{king.id} ");
		}
		return stringBuilderPool.ToString().Trim() + "]";
	}
}
