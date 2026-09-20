using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class Building : BaseSimObject, IEquatable<Building>, IComparable<Building>, ILoadable<BuildingData>
{
	public BatchBuildings batch;

	internal bool positionDirty;

	internal bool sprite_dirty;

	internal bool tiles_dirty;

	private Sprite _last_colored_sprite;

	private ColorAsset _last_color_asset;

	internal Sprite last_main_sprite;

	internal BuildingData data;

	internal BuildingAsset asset;

	public bool flip_x;

	internal readonly List<WorldTile> tiles;

	public BuildingAnimationData animData;

	public int animData_index;

	private float _shake_timer;

	private float _shake_intensity_x;

	private float _shake_intensity_y;

	internal float lastAngle;

	private Vector2 _shake_offset;

	internal readonly List<TileZone> zones;

	internal BuildingAnimationState animation_state;

	internal BuildingOwnershipState state_ownership;

	internal ListPool<BaseBuildingComponent> components_list;

	internal Docks component_docks;

	internal Wheat component_wheat;

	internal BuildingFruitGrowth component_fruit_growth;

	internal UnitSpawner component_unit_spawner;

	internal BuildingSpreadBiome component_biome_spreader;

	internal BuildingMonolith component_monolith;

	internal BuildingWaypoint component_waypoint;

	internal BuildingBiomeFoodProducer component_food_producer;

	internal Beehive component_beehive;

	internal readonly BuildingTweenScaleHelper scale_helper;

	internal bool chopped;

	internal bool is_visible;

	internal bool check_spawn_animation;

	private float _timer_shake_resource;

	private float _auto_remove_timer;

	public HashSet<long> residents;

	private Vector3 _last_scale;

	public Material material;

	protected override MetaType meta_type => MetaType.Building;

	internal WorldTile door_tile
	{
		get
		{
			if (!current_tile.has_tile_down)
			{
				return current_tile;
			}
			return current_tile.tile_down;
		}
	}

	public City city => current_tile.zone.city;

	public CityResources resources => data.resources;

	internal bool isBurnable()
	{
		if (!hasHealth())
		{
			return false;
		}
		if (hasCity())
		{
			City city = getCity();
			if (city.hasReligion() && city.getReligion().hasMetaTag("building_immunity_fire"))
			{
				return false;
			}
		}
		return asset.burnable;
	}

	public float getExistenceTime()
	{
		return World.world.getWorldTimeElapsedSince(data.created_time);
	}

	public float getExistenceMonths()
	{
		return getExistenceTime() / 5f;
	}

	public void setAnimData(int pIndex)
	{
		if (pIndex >= asset.building_sprites.animation_data.Count || pIndex < 0)
		{
			pIndex = 0;
		}
		animData = asset.building_sprites.animation_data[pIndex];
		animData_index = pIndex;
	}

	internal void stopFire()
	{
		finishStatusEffect("burning");
	}

	internal override void create()
	{
		base.create();
		setObjectType(MapObjectType.Building);
		startShake(0.3f);
	}

	protected sealed override void setDefaultValues()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.setDefaultValues();
		flip_x = false;
		positionDirty = false;
		sprite_dirty = false;
		tiles_dirty = false;
		_last_colored_sprite = null;
		_last_color_asset = null;
		_shake_timer = 0f;
		lastAngle = 0f;
		residents.Clear();
		_shake_offset = Vector2.zero;
		animation_state = BuildingAnimationState.Normal;
		state_ownership = BuildingOwnershipState.None;
		chopped = false;
		is_visible = false;
		check_spawn_animation = false;
	}

	private T addComponent<T>() where T : BaseBuildingComponent, new()
	{
		T val = World.world.buildings.component_pool.get<T>();
		if (components_list == null)
		{
			components_list = new ListPool<BaseBuildingComponent>();
		}
		components_list.Add(val);
		val.create(this);
		batch.c_components.Add(this);
		return val;
	}

	public bool hasBooks()
	{
		if (data.books == null)
		{
			return false;
		}
		return data.books.hasAny();
	}

	public bool hasFreeBookSlot()
	{
		if (asset.book_slots == 0)
		{
			return false;
		}
		return asset.book_slots > data.books.totalBooks();
	}

	public void addBook(Book pBook)
	{
		data.books.addBook(pBook);
	}

	public bool isState(BuildingState pState)
	{
		return data.state == pState;
	}

	internal void setBuilding(WorldTile pTile, BuildingAsset pAsset, BuildingData pData)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		current_tile = pTile;
		current_tile.zone.addBuildingMain(this);
		if (pData == null)
		{
			setTemplate(pAsset);
			BuildingData buildingData = data;
			Vector2Int pos = pTile.pos;
			buildingData.mainX = ((Vector2Int)(ref pos)).x;
			BuildingData buildingData2 = data;
			pos = pTile.pos;
			buildingData2.mainY = ((Vector2Int)(ref pos)).y;
			setState(BuildingState.Normal);
			updateStats();
			setMaxHealth();
			if (asset.has_resources_grown_to_collect)
			{
				setHaveResourcesToCollect(asset.has_resources_grown_to_collect_on_spawn);
			}
		}
		else
		{
			setData(pData);
			setTemplate(pAsset);
		}
		setStatsDirty();
		current_position = Vector2Int.op_Implicit(current_tile.pos);
		current_scale.x = asset.scale_base.x;
		current_scale.y = asset.scale_base.y;
		fillTiles();
		if (!string.IsNullOrEmpty(asset.kingdom))
		{
			Kingdom kingdom = World.world.kingdoms_wild.get(asset.kingdom);
			setKingdom(kingdom);
		}
		if (!isUnderConstruction())
		{
			int num = -1;
			if (pData != null)
			{
				num = pData.frameID;
			}
			initAnimationData();
			if (num != -1)
			{
				setAnimData(num);
			}
		}
		checkMaterial();
		setPositionDirty();
		updatePosition();
		if (pAsset.storage && data.resources == null)
		{
			data.resources = new CityResources();
		}
		if (pAsset.book_slots > 0 && data.books == null)
		{
			data.books = new StorageBooks();
		}
		if (pAsset.smoke)
		{
			addComponent<BuildingSmokeEffect>();
		}
		if (pAsset.building_type == BuildingType.Building_Poops)
		{
			batch.c_poop.Add(this);
		}
		if (pAsset.spread)
		{
			switch (pAsset.flora_type)
			{
			case FloraType.Fungi:
				batch.c_spread_fungi.Add(this);
				break;
			case FloraType.Plant:
				batch.c_spread_plants.Add(this);
				break;
			case FloraType.Tree:
				batch.c_spread_trees.Add(this);
				break;
			}
		}
		if (pAsset.produce_biome_food)
		{
			component_food_producer = addComponent<BuildingBiomeFoodProducer>();
		}
		if (pAsset.spawn_drops)
		{
			addComponent<BuildingEffectSpawnDrop>();
		}
		if (pAsset.id == "monolith")
		{
			component_monolith = addComponent<BuildingMonolith>();
		}
		if (pAsset.waypoint)
		{
			component_waypoint = pAsset.kingdom switch
			{
				"alien_mold" => addComponent<BuildingWaypointAlienMold>(), 
				"computer" => addComponent<BuildingWaypointComputer>(), 
				"golden_egg" => addComponent<BuildingWaypointGoldenEgg>(), 
				"harp" => addComponent<BuildingWaypointHarp>(), 
				_ => throw new ArgumentOutOfRangeException(pAsset.kingdom + " is not a valid kingdom for a waypoint"), 
			};
		}
		if (pAsset.grow_creep)
		{
			addComponent<BuildingCreepHUB>();
		}
		if (pAsset.wheat)
		{
			component_wheat = addComponent<Wheat>();
		}
		if (pAsset.building_type == BuildingType.Building_Fruits)
		{
			component_fruit_growth = addComponent<BuildingFruitGrowth>();
		}
		if (pAsset.ice_tower)
		{
			addComponent<IceTower>();
		}
		if (pAsset.id == "poop")
		{
			addComponent<Poop>();
		}
		if (pAsset.spawn_units)
		{
			component_unit_spawner = addComponent<UnitSpawner>();
		}
		if (pAsset.spread_biome)
		{
			component_biome_spreader = addComponent<BuildingSpreadBiome>();
		}
		if (pAsset.beehive)
		{
			component_beehive = addComponent<Beehive>();
		}
		if (pAsset.docks)
		{
			component_docks = addComponent<Docks>();
		}
		if (pAsset.tower)
		{
			addComponent<BuildingTower>();
		}
		if (pData == null && !pAsset.city_building)
		{
			setAnimationState(BuildingAnimationState.Normal);
			this.setScaleTween();
		}
		if (isRuin())
		{
			makeRuins();
		}
		else if (asset.city_building && hasCity())
		{
			setKingdom(current_tile.zone_city.kingdom);
		}
		else if (asset.city_building && !hasCity() && isAbandoned())
		{
			makeAbandoned();
		}
	}

	private void debugCheckResourcesOnSpawn(BuildingAsset pAsset)
	{
	}

	public override void setStatsDirty()
	{
		base.setStatsDirty();
		if (isAlive())
		{
			batch.c_stats_dirty.Add(this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void setPositionDirty()
	{
		positionDirty = true;
		batch.c_position_dirty.Add(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override BaseObjectData getData()
	{
		return data;
	}

	public void setData(BuildingData pData)
	{
		data = pData;
	}

	public void loadData(BuildingData pData)
	{
		setData(pData);
		pData.load();
	}

	public void loadBuilding(BuildingData pData)
	{
		if (!isUnderConstruction())
		{
			setAnimData(pData.frameID);
		}
		if (data.resources != null)
		{
			resources.loadFromSave();
		}
	}

	internal void setHaveResourcesToCollect(bool pValue)
	{
		if (pValue)
		{
			data.addFlag("has_resources");
		}
		else
		{
			data.removeFlag("has_resources");
		}
	}

	public bool hasResourcesToCollect()
	{
		if (asset.has_resources_grown_to_collect)
		{
			return data.hasFlag("has_resources");
		}
		if (chopped)
		{
			return false;
		}
		return asset.has_resources_to_collect;
	}

	internal bool canBeUpgraded()
	{
		if (isUnderConstruction())
		{
			return false;
		}
		if (asset.city_building && !isCiv())
		{
			return false;
		}
		return asset.can_be_upgraded;
	}

	internal bool upgradeBuilding()
	{
		if (!canBeUpgraded())
		{
			return false;
		}
		BuildingAsset buildingAsset = AssetManager.buildings.get(asset.upgrade_to);
		if ((buildingAsset.fundament.left != asset.fundament.left || buildingAsset.fundament.right != asset.fundament.right || buildingAsset.fundament.top != asset.fundament.top || buildingAsset.fundament.bottom != asset.fundament.bottom) && !checkTilesForUpgrade(current_tile, buildingAsset))
		{
			return false;
		}
		makeZoneDirty();
		setTemplate(buildingAsset);
		initAnimationData();
		updateStats();
		setMaxHealth();
		fillTiles();
		return true;
	}

	private void setTemplate(BuildingAsset pTemplate)
	{
		asset = pTemplate;
		data.asset_id = asset.id;
		asset.buildings.Add(this);
		if (asset.canBeOccupied())
		{
			World.world.buildings.occupied_buildings.Add(this);
		}
		asset.checkSpritesAreLoaded();
	}

	internal void setMaterial(string pMaterialID)
	{
		material = LibraryMaterials.instance.dict[pMaterialID];
	}

	internal void setKingdomCiv(Kingdom pKingdom)
	{
		if (kingdom != pKingdom || !hasKingdom())
		{
			setKingdom(pKingdom);
		}
	}

	internal void makeRuins()
	{
		setKingdom(World.world.kingdoms_wild.get("ruins"));
		setState(BuildingState.Ruins);
	}

	public void makeAbandoned()
	{
		setKingdom(WildKingdomsManager.abandoned);
		if (isUnderConstruction())
		{
			startDestroyBuilding();
		}
		else if (!asset.can_be_abandoned)
		{
			if (asset.has_ruin_state)
			{
				startMakingRuins();
			}
			else
			{
				startDestroyBuilding();
			}
		}
	}

	public void setKingdom(Kingdom pKingdom)
	{
		if (kingdom != pKingdom)
		{
			if (kingdom != pKingdom)
			{
				makeZoneDirty();
			}
			checkKingdom();
			kingdom = pKingdom;
			checkKingdom();
			if (isKingdomCiv())
			{
				setOwnershipState(BuildingOwnershipState.Civilization);
			}
			else
			{
				setOwnershipState(BuildingOwnershipState.World);
			}
			setTilesDirty();
			World.world.sim_object_zones.setBuildingsDirty(base.chunk);
		}
	}

	private void checkKingdom()
	{
		if (hasKingdom())
		{
			if (kingdom.wild)
			{
				World.world.kingdoms_wild.setDirtyBuildings();
			}
			else
			{
				World.world.kingdoms.setDirtyBuildings();
			}
		}
	}

	public bool hasHousingLogic()
	{
		if (asset.canBeOccupied())
		{
			return true;
		}
		return false;
	}

	private void setState(BuildingState pState)
	{
		if (hasHousingLogic())
		{
			World.world.buildings.event_houses = true;
		}
		if (isRemoved())
		{
			return;
		}
		if (pState == BuildingState.Ruins && !isRuin())
		{
			bool flag = false;
			if (flag)
			{
				foreach (WorldTile tile in tiles)
				{
					if (tile.Type.lava)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				setHealth(getMaxHealthPercent(0.5f));
			}
			else
			{
				setMaxHealth();
			}
			stats["health"] = getHealth();
		}
		data.state = pState;
		checkAutoRemove();
		checkMaterial();
		clearZones();
		if (!isRemoved())
		{
			fillTiles();
		}
		setTilesDirty();
		World.world.sim_object_zones.setBuildingsDirty(base.chunk);
	}

	public void checkMaterial()
	{
		if (data.state == BuildingState.Ruins)
		{
			setMaterial(BuildingRendererSettings.cur_default_material);
		}
		else if (BuildingRendererSettings.wobbly_material_enabled)
		{
			setMaterial(asset.material);
		}
		else
		{
			setMaterial(BuildingRendererSettings.cur_default_material);
		}
	}

	internal void updateKingdomColors()
	{
		setTilesDirty();
	}

	internal bool checkTilesForUpgrade(WorldTile pTile, BuildingAsset pTemplate)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		WorldTile worldTile = null;
		Vector2Int pos = pTile.pos;
		int num = ((Vector2Int)(ref pos)).x - pTemplate.fundament.left;
		pos = pTile.pos;
		int num2 = ((Vector2Int)(ref pos)).y - pTemplate.fundament.bottom;
		int num3 = pTemplate.fundament.right + pTemplate.fundament.left + 1;
		int num4 = pTemplate.fundament.top + pTemplate.fundament.bottom + 1;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				worldTile = World.world.GetTile(num + i, num2 + j);
				if (worldTile == null)
				{
					return false;
				}
				if (!worldTile.Type.can_build_on)
				{
					return false;
				}
				if (worldTile.zone.city != city)
				{
					return false;
				}
				Building building = worldTile.building;
				if (building != null && building != this)
				{
					if (building.asset.priority >= asset.priority)
					{
						return false;
					}
					if (building.asset.upgrade_level >= asset.upgrade_level)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	internal void debugConstructions()
	{
		if (!((Object)(object)asset.building_sprites.construction == (Object)null))
		{
			setUnderConstruction();
		}
	}

	private void initAnimationData()
	{
		asset.checkSpritesAreLoaded();
		int num = Randy.randomInt(0, asset.building_sprites.animation_data.Count);
		setAnimData(num);
		if (asset.random_flip && !asset.shadow)
		{
			flip_x = Randy.randomBool();
		}
		this.setScaleTween();
	}

	private void fillTiles()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (tiles.Count != 0)
		{
			clearTiles();
		}
		Vector2Int pos = current_tile.pos;
		int num = ((Vector2Int)(ref pos)).x - asset.fundament.left;
		pos = current_tile.pos;
		int num2 = ((Vector2Int)(ref pos)).y - asset.fundament.bottom;
		int num3 = asset.fundament.right + asset.fundament.left + 1;
		int num4 = asset.fundament.top + asset.fundament.bottom + 1;
		int num5 = 0;
		for (int i = 0; i < num3; i++)
		{
			for (int j = num5; j < num4; j++)
			{
				WorldTile tile = World.world.GetTile(num + i, num2 + j);
				if (tile != null)
				{
					setBuildingTile(tile, i, j);
				}
			}
		}
		setTilesDirty();
	}

	internal void checkDirtyTiles()
	{
		if (tiles_dirty)
		{
			tiles_dirty = false;
			for (int i = 0; i < tiles.Count; i++)
			{
				WorldTile tileDirty = tiles[i];
				World.world.setTileDirty(tileDirty);
			}
			batch?.c_tiles_dirty.Remove(this);
		}
	}

	private void setTilesDirty()
	{
		tiles_dirty = true;
		batch?.c_tiles_dirty.Add(this);
	}

	private void forceUpdateTilesDirty()
	{
		setTilesDirty();
		checkDirtyTiles();
	}

	private void setBuildingTile(WorldTile pTile, int pX, int pY)
	{
		if (pTile.hasBuilding() && pTile.building != this)
		{
			pTile.building.startDestroyBuilding();
		}
		pTile.building = this;
		pTile.minimap_building_x = pX;
		pTile.minimap_building_y = pY;
		if (!tiles.Contains(pTile))
		{
			tiles.Add(pTile);
			if (!zones.Contains(pTile.zone))
			{
				zones.Add(pTile.zone);
			}
		}
		TileType tileType = null;
		TopTileType topTileType = null;
		if (asset.transform_tiles_to_tile_type != null)
		{
			tileType = AssetManager.tiles.get(asset.transform_tiles_to_tile_type);
		}
		if (asset.transform_tiles_to_top_tiles != null)
		{
			topTileType = AssetManager.top_tiles.get(asset.transform_tiles_to_top_tiles);
		}
		if (tileType != null || topTileType != null)
		{
			if (tileType == null)
			{
				tileType = pTile.main_type;
			}
			if (tileType.can_be_biome)
			{
				MapAction.terraformTile(pTile, tileType, topTileType, TerraformLibrary.nothing);
			}
		}
	}

	public void setOwnershipState(BuildingOwnershipState pState)
	{
		if (state_ownership != pState)
		{
			makeZoneDirty();
		}
		state_ownership = pState;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isRuin()
	{
		return data.state == BuildingState.Ruins;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isRemoved()
	{
		return data.state == BuildingState.Removed;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isNormal()
	{
		return data.state == BuildingState.Normal;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isAbandoned()
	{
		if (state_ownership == BuildingOwnershipState.World)
		{
			return asset.city_building;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isCiv()
	{
		return state_ownership == BuildingOwnershipState.Civilization;
	}

	public void prepareForSave()
	{
		if (hasCity())
		{
			data.cityID = city.data.id;
		}
		else
		{
			data.cityID = -1L;
		}
		resources?.save();
		data.frameID = animData_index;
		data.save();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isUsable()
	{
		if (!isAlive())
		{
			return false;
		}
		if (isRuin())
		{
			return false;
		}
		if (isOnRemove())
		{
			return false;
		}
		if (isRemoved())
		{
			return false;
		}
		return true;
	}

	internal void startDestroyBuilding()
	{
		if (!isOnRemove())
		{
			if (asset.has_ruins_graphics && !isUnderConstruction())
			{
				setState(BuildingState.Ruins);
			}
			startRemove();
		}
	}

	private void clearZones()
	{
		zones.Clear();
	}

	internal void kill()
	{
		if (!isAlive())
		{
			return;
		}
		clearZones();
		setAlive(pValue: false);
		if (asset.city_building)
		{
			World.world.map_stats.housesDestroyed++;
		}
		if (!hasBooks())
		{
			return;
		}
		foreach (long list_book in data.books.list_books)
		{
			Book pBook = World.world.books.get(list_book);
			World.world.books.burnBook(pBook);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override City getCity()
	{
		return city;
	}

	internal override void updateStats()
	{
		base.updateStats();
		stats.clear();
		stats.mergeStats(asset.base_stats);
		if (getHealth() > getMaxHealth())
		{
			setMaxHealth();
		}
		batch.c_stats_dirty.Remove(this);
	}

	internal void chopTree()
	{
		if (!chopped && ((!asset.become_alive_when_chopped && !WorldLawLibrary.world_law_bark_bites_back.isEnabled()) || !Randy.randomChance(0.2f) || !ActionLibrary.tryToMakeFloraAlive(this)))
		{
			finishAllStatusEffects();
			MusicBox.playSound("event:/SFX/NATURE/TreeFall", current_tile, pGameViewOnly: true, pVisibleOnly: true);
			chopped = true;
			setHaveResourcesToCollect(pValue: false);
			float pTargetAngle = (Randy.randomBool() ? 90 : (-90));
			scale_helper.doRotateTween(pTargetAngle, 1f, finishChop);
			batch.c_angle.Add(this);
		}
	}

	private void finishChop()
	{
		startRemove();
	}

	private void startRemove()
	{
		if (!isOnRemove())
		{
			if (!isUnderConstruction() && asset.has_sound_destroyed)
			{
				MusicBox.playSound(asset.sound_destroyed, current_tile, pGameViewOnly: true, pVisibleOnly: true);
			}
			setAnimationState(BuildingAnimationState.OnRemove);
			clearTiles();
			clearComponents();
			setHaveResourcesToCollect(pValue: false);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isAnimationState(BuildingAnimationState pState)
	{
		return animation_state == pState;
	}

	internal void startMakingRuins()
	{
		if (!asset.has_ruin_state)
		{
			startRemove();
		}
		else if (!isAnimationState(BuildingAnimationState.OnRuin) && data.state != BuildingState.Ruins)
		{
			setAnimationState(BuildingAnimationState.OnRuin);
			makeRuins();
		}
	}

	internal void removeBuildingFinal()
	{
		setState(BuildingState.Removed);
		clearZones();
		clearTiles();
		kill();
		current_tile.zone.removeBuildingMain(this);
		World.world.buildings.scheduleDestroyOnPlay(this);
	}

	internal void clearTiles()
	{
		forceUpdateTilesDirty();
		for (int i = 0; i < tiles.Count; i++)
		{
			tiles[i].building = null;
		}
		tiles.Clear();
	}

	private void clearComponents()
	{
		if (asset.flora_type == FloraType.Tree)
		{
			batch.c_spread_trees.Remove(this);
		}
		if (asset.flora_type == FloraType.Fungi)
		{
			batch.c_spread_fungi.Remove(this);
		}
		if (asset.flora_type == FloraType.Plant)
		{
			batch.c_spread_plants.Remove(this);
		}
		if (asset.building_type == BuildingType.Building_Poops)
		{
			batch.c_poop.Remove(this);
		}
		if (components_list != null)
		{
			batch.c_components.Remove(this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isOnRemove()
	{
		return animation_state == BuildingAnimationState.OnRemove;
	}

	internal void setAnimationState(BuildingAnimationState pState)
	{
		if (!isOnRemove())
		{
			animation_state = pState;
			this.checkTweens();
		}
	}

	internal void completeMakingRuin()
	{
		setState(BuildingState.Ruins);
		setAnimationState(BuildingAnimationState.Normal);
		this.setScaleTween();
	}

	private void checkAutoRemove()
	{
		if (batch != null)
		{
			if (asset.auto_remove_ruin && isRuin() && !isCiv())
			{
				batch.c_auto_remove.Add(this);
			}
			else
			{
				batch.c_auto_remove.Remove(this);
			}
		}
	}

	internal void updateAutoRemove(float pElapsed)
	{
		if (_auto_remove_timer < 300f)
		{
			_auto_remove_timer += pElapsed;
			return;
		}
		_auto_remove_timer = 0f;
		batch.c_auto_remove.Remove(this);
		startDestroyBuilding();
	}

	internal void updateTimerShakeResources(float pElapsed)
	{
		if (_timer_shake_resource > 0f)
		{
			_timer_shake_resource -= pElapsed;
			if (_timer_shake_resource <= 0f)
			{
				batch.c_resource_shaker.Remove(this);
			}
		}
	}

	internal void updateComponents(float pElapsed)
	{
		for (int i = 0; i < components_list.Count; i++)
		{
			components_list[i].update(pElapsed);
		}
	}

	public void updatePosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (positionDirty)
		{
			positionDirty = false;
			batch.c_position_dirty.Remove(this);
			cur_transform_position = current_tile.posV3;
			if (cur_transform_position.z < 0f)
			{
				cur_transform_position.z = 0f;
			}
			cur_transform_position.x += _shake_offset.x;
			cur_transform_position.y += _shake_offset.y;
			cur_transform_position.z = -0.2f + asset.bonus_z;
		}
	}

	internal void spawnBurstSpecial(int pAmount = 1)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		MapBox world = World.world;
		Vector2Int pos = current_tile.pos;
		int x = ((Vector2Int)(ref pos)).x;
		pos = current_tile.pos;
		WorldTile tile = world.GetTile(x, ((Vector2Int)(ref pos)).y);
		if (tile == null)
		{
			tile = current_tile;
		}
		for (int i = 0; i < pAmount; i++)
		{
			World.world.drop_manager.spawnParabolicDrop(tile, asset.spawn_drop_id, asset.spawn_drop_start_height, asset.spawn_drop_min_height, asset.spawn_drop_max_height, asset.spawn_drop_min_radius, asset.spawn_drop_max_radius);
		}
	}

	internal bool updateBuild(int pProgress = 1)
	{
		data.change("construction_progress", pProgress);
		startShake(0.3f);
		bool result = false;
		if (getConstructionProgress() > asset.construction_progress_needed)
		{
			result = true;
			completeConstruction();
			if (asset.has_sound_built)
			{
				MusicBox.playSound(asset.sound_built, current_tile, pGameViewOnly: true, pVisibleOnly: true);
			}
			initAnimationData();
			this.setScaleTween(0.25f);
		}
		else
		{
			this.setScaleTween(0.75f);
		}
		return result;
	}

	private void makeZoneDirty()
	{
		current_tile.zone.setDirty(pValue: true);
		if (hasHousingLogic())
		{
			World.world.buildings.event_houses = true;
		}
	}

	public bool hasResidentSlots()
	{
		if (!asset.hasHousingSlots())
		{
			return false;
		}
		if (asset.housing_slots > countResidents())
		{
			return true;
		}
		return false;
	}

	public int countResidents()
	{
		return residents.Count;
	}

	public bool hasResidents()
	{
		return countResidents() > 0;
	}

	public void startShake(float pDuration, float pIntensityX = 0.1f, float pIntensityY = 0.1f)
	{
		_shake_timer = pDuration;
		_shake_intensity_x = pIntensityX;
		_shake_intensity_y = pIntensityY;
		batch?.c_shake.Add(this);
	}

	internal void resourceGathering(float pElapsed)
	{
		if (!(_timer_shake_resource > 0f))
		{
			batch.c_resource_shaker.Add(this);
			startShake(0.3f);
			_timer_shake_resource = 1f;
		}
	}

	public void updateShake(float pElapsed)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_shake_timer > 0f)
		{
			_shake_timer -= pElapsed;
			if (_shake_timer < 0f)
			{
				_shake_offset = Vector2.zero;
				batch.c_shake.Remove(this);
			}
			else
			{
				_shake_offset.x = ((Random)(ref batch.rnd)).NextFloat(0f - _shake_intensity_x, _shake_intensity_x);
				_shake_offset.y = ((Random)(ref batch.rnd)).NextFloat(0f - _shake_intensity_y, _shake_intensity_y);
			}
			setPositionDirty();
		}
	}

	internal override void getHitFullHealth(AttackType pAttackType)
	{
		getHit(getHealth(), pFlash: false, pAttackType, null, pSkipIfShake: false, pMetallicWeapon: false, pCheckDamageReduction: false);
	}

	internal override void getHit(float pDamage, bool pFlash = true, AttackType pAttackType = AttackType.Other, BaseSimObject pAttacker = null, bool pSkipIfShake = true, bool pMetallicWeapon = false, bool pCheckDamageReduction = true)
	{
		if (!isAnimationState(BuildingAnimationState.Normal))
		{
			return;
		}
		changeHealth((int)(0f - pDamage));
		if (pAttackType == AttackType.Weapon && asset.has_sound_hit)
		{
			MusicBox.playSound(asset.sound_hit, current_tile, pGameViewOnly: true, pVisibleOnly: true);
		}
		startShake(0.3f);
		if (!hasHealth())
		{
			if (data.state == BuildingState.Ruins)
			{
				startDestroyBuilding();
			}
			else
			{
				startMakingRuins();
			}
		}
		else
		{
			this.setScaleTween(0.75f);
		}
	}

	internal void extractResources(Actor pBy)
	{
		this.setScaleTween(0.75f);
		switch (asset.building_type)
		{
		case BuildingType.Building_Wheat:
		case BuildingType.Building_Plant:
			startDestroyBuilding();
			break;
		case BuildingType.Building_Tree:
			chopTree();
			break;
		case BuildingType.Building_Poops:
		case BuildingType.Building_Mineral:
			startRemove();
			break;
		case BuildingType.Building_Fruits:
			component_fruit_growth.reset();
			setHaveResourcesToCollect(pValue: false);
			if (Randy.randomChance(0.2f))
			{
				startDestroyBuilding();
			}
			break;
		case BuildingType.Building_Hives:
			component_beehive.honey = 0;
			setHaveResourcesToCollect(pValue: false);
			break;
		}
	}

	internal Color32 getColorForMinimap(WorldTile pTile)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (Config.EVERYTHING_MAGIC_COLOR)
		{
			return Toolbox.EVERYTHING_MAGIC_COLOR32;
		}
		return asset.building_sprites.map_icon.getColor(pTile.minimap_building_x, pTile.minimap_building_y, this);
	}

	public WorldTile getConstructionTile()
	{
		if (asset.docks)
		{
			var (array, pLength) = Toolbox.getAllZonesFromTile(current_tile);
			foreach (TileZone item in array.LoopRandom(pLength))
			{
				using IEnumerator<WorldTile> enumerator2 = checkZoneForDockConstruction(item).GetEnumerator();
				if (enumerator2.MoveNext())
				{
					return enumerator2.Current;
				}
			}
		}
		return Randy.getRandom(tiles);
	}

	public int getConstructionProgress()
	{
		data.get("construction_progress", out var pResult, 0);
		return pResult;
	}

	public void completeConstruction()
	{
		data.removeInt("construction_progress");
		data.removeFlag("under_construction");
		makeZoneDirty();
	}

	public bool isUnderConstruction()
	{
		if (!asset.has_sprite_construction)
		{
			return false;
		}
		return data.hasFlag("under_construction");
	}

	public void setUnderConstruction()
	{
		if (asset.has_sprite_construction)
		{
			data.addFlag("under_construction");
		}
	}

	public bool canRemoveForFarms()
	{
		return asset.flora;
	}

	internal IEnumerable<WorldTile> checkZoneForDockConstruction(TileZone pZone)
	{
		if (pZone.city == null || pZone.city != city)
		{
			yield break;
		}
		foreach (WorldTile item in pZone.tiles.LoopRandom())
		{
			if (item.Type.ground && Toolbox.SquaredDistTile(current_tile, item) <= 49)
			{
				yield return item;
			}
		}
	}

	internal void checkStartSpawnAnimation()
	{
		Sprite[] spawn = animData.spawn;
		if (spawn != null && spawn.Length != 0)
		{
			check_spawn_animation = true;
		}
	}

	public Sprite calculateMainSprite()
	{
		bool flag = true;
		Sprite[] array = null;
		bool flag2 = isRuin();
		if (flag2)
		{
			flag = false;
		}
		if (isUnderConstruction())
		{
			last_main_sprite = asset.building_sprites.construction;
			return last_main_sprite;
		}
		if (asset.has_special_animation_state)
		{
			array = ((!hasResourcesToCollect()) ? animData.special : animData.main);
		}
		else if (flag2 && asset.has_ruins_graphics)
		{
			flag = false;
			array = animData.ruins;
		}
		else if (asset.spawn_drops && data.hasFlag("stop_spawn_drops"))
		{
			array = animData.main_disabled;
		}
		else if (asset.can_be_abandoned && isAbandoned())
		{
			Sprite[] main_disabled = animData.main_disabled;
			array = ((main_disabled == null || main_disabled.Length == 0) ? animData.main : animData.main_disabled);
			flag = false;
		}
		else
		{
			array = animData.main;
			if (asset.get_override_sprites_main != null)
			{
				Sprite[] array2 = asset.get_override_sprites_main(this);
				if (array2 != null)
				{
					array = array2;
				}
			}
		}
		Sprite val = null;
		if (check_spawn_animation)
		{
			return getSpawnFrameSprite();
		}
		if (!flag || array.Length == 1)
		{
			return array[0];
		}
		return AnimationHelper.getSpriteFromList(GetHashCode(), array, asset.animation_speed);
	}

	public bool isColoredSpriteNeedsCheck(Sprite pMainSprite)
	{
		if (last_main_sprite == null || ((object)last_main_sprite).GetHashCode() != ((object)pMainSprite).GetHashCode() || _last_color_asset != kingdom.getColor())
		{
			return true;
		}
		return false;
	}

	public Sprite calculateColoredSprite(Sprite pMainSprite)
	{
		if (isColoredSpriteNeedsCheck(pMainSprite))
		{
			_last_colored_sprite = DynamicSprites.getRecoloredBuilding(pMainSprite, kingdom.getColor(), asset.atlas_asset);
			last_main_sprite = pMainSprite;
			_last_color_asset = kingdom.getColor();
		}
		return _last_colored_sprite;
	}

	public Sprite getLastColoredSprite()
	{
		return _last_colored_sprite;
	}

	public void clearSprites()
	{
		last_main_sprite = null;
		_last_colored_sprite = null;
		_last_color_asset = null;
	}

	public Sprite checkSpriteToRender()
	{
		Sprite pMainSprite = calculateMainSprite();
		return calculateColoredSprite(pMainSprite);
	}

	public Vector3 getCurrentScale()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		float tweenBuildingsValue = World.world.quality_changer.getTweenBuildingsValue();
		float num = current_scale.y * tweenBuildingsValue;
		float num2 = current_scale.x * tweenBuildingsValue;
		if (_last_scale.y != num || _last_scale.x != num2)
		{
			((Vector3)(ref _last_scale)).Set(num2, num, 1f);
		}
		return _last_scale;
	}

	public bool isFullyGrown()
	{
		if (!asset.can_be_grown)
		{
			return true;
		}
		if (asset.wheat)
		{
			return component_wheat.isMaxLevel();
		}
		return false;
	}

	private Sprite getSpawnFrameSprite()
	{
		Sprite[] spawn = animData.spawn;
		float worldTimeElapsedSince = World.world.getWorldTimeElapsedSince(data.created_time);
		float num = (float)spawn.Length * asset.animation_speed / 60f;
		Sprite result;
		if (num > worldTimeElapsedSince)
		{
			int num2 = (int)(worldTimeElapsedSince / num * (float)spawn.Length);
			result = spawn[num2];
		}
		else
		{
			result = spawn.Last();
			check_spawn_animation = false;
		}
		return result;
	}

	public int takeResource(string pResourceID, int pAmount)
	{
		return resources.change(pResourceID, -pAmount);
	}

	public int getResourcesAmount(string pResourceID)
	{
		return resources.get(pResourceID);
	}

	public int addResources(string pResourceID, int pAmount)
	{
		return resources.change(pResourceID, pAmount);
	}

	public bool hasSpaceForResource(ResourceAsset pResourceAsset)
	{
		return resources.hasSpaceForResource(pResourceAsset);
	}

	public bool hasResourcesForNewItems()
	{
		return resources.hasResourcesForNewItems();
	}

	public int countFood()
	{
		return resources.countFood();
	}

	public ResourceAsset getRandomSuitableFood(Subspecies pSubspecies, string pFavoriteFood = null)
	{
		return resources.getRandomSuitableFood(pSubspecies, pFavoriteFood);
	}

	public override void Dispose()
	{
		kingdom = null;
		_last_colored_sprite = null;
		_last_color_asset = null;
		last_main_sprite = null;
		batch = null;
		data = null;
		asset = null;
		tiles.Clear();
		animData = null;
		zones.Clear();
		if (components_list != null)
		{
			for (int i = 0; i < components_list.Count; i++)
			{
				BaseBuildingComponent baseBuildingComponent = components_list[i];
				baseBuildingComponent.Dispose();
				World.world.buildings.component_pool.release(baseBuildingComponent);
			}
			components_list.Clear();
			components_list.Dispose();
			components_list = null;
		}
		component_docks = null;
		component_wheat = null;
		component_fruit_growth = null;
		component_unit_spawner = null;
		component_biome_spreader = null;
		component_monolith = null;
		component_waypoint = null;
		component_food_producer = null;
		component_beehive = null;
		scale_helper.reset();
		base.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Building pObject)
	{
		return GetHashCode() == pObject.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(Building pTarget)
	{
		return GetHashCode().CompareTo(pTarget.GetHashCode());
	}

	public void checkVegetationSpread(float pElapsed)
	{
		BuildingAsset buildingAsset = asset;
		if (Randy.randomChance(buildingAsset.spread_chance))
		{
			WorldTile random = current_tile.neighboursAll.GetRandom();
			for (int i = 0; (float)i < buildingAsset.spread_steps; i++)
			{
				random = random.neighboursAll.GetRandom();
			}
			string random2 = buildingAsset.spread_ids.GetRandom();
			BuildingAsset pAsset = AssetManager.buildings.get(random2);
			tryToGrowOnTile(random, pAsset);
		}
	}

	private bool tryToGrowOnTile(WorldTile pTile, BuildingAsset pAsset, bool pCheckLimit = true)
	{
		if (pCheckLimit && pTile.zone.hasReachedBuildingLimit(pTile, pAsset))
		{
			return false;
		}
		if (!World.world.buildings.canBuildFrom(pTile, pAsset, null, BuildPlacingType.New, pFloraGrowth: true))
		{
			return false;
		}
		World.world.buildings.addBuilding(pAsset, pTile);
		if (pAsset.flora_type == FloraType.Tree)
		{
			World.world.game_stats.data.treesGrown++;
		}
		else if (pAsset.flora_type == FloraType.Plant || pAsset.flora_type == FloraType.Fungi)
		{
			World.world.game_stats.data.floraGrown++;
		}
		return true;
	}

	public Building()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		tiles = new List<WorldTile>();
		zones = new List<TileZone>();
		scale_helper = new BuildingTweenScaleHelper();
		residents = new HashSet<long>();
		_last_scale = Vector3.zero;
		base._002Ector();
	}
}
