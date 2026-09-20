using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityPools;

public class WorldTile : IEquatable<WorldTile>, IDisposable
{
	[CanBeNull]
	public TopTileType top_type;

	[CanBeNull]
	public TileType main_type;

	private TileTypeBase cur_tile_type;

	public bool obstacle_is_around;

	internal TileBase current_rendered_tile_graphics;

	public int burned_stages;

	internal WorldTileZoneBorder world_tile_zone_border;

	public const int DEFAULT_HEALTH = 10;

	public int health;

	public Vector3Int last_rendered_border_pos_ocean;

	public Vector3Int last_rendered_pos_tile;

	public TileTypeBase last_rendered_tile_type;

	public float delayed_timer_bomb;

	public string delayed_bomb_type;

	public double timestamp_type_changed;

	public WorldTileData data;

	public int heat;

	internal int explosion_wave;

	internal int explosion_power;

	private Actor _targeted_by;

	public bool world_edge;

	public WorldTile tile_up;

	public WorldTile tile_down;

	public WorldTile tile_left;

	public WorldTile tile_right;

	public WorldTile[] neighbours;

	public WorldTile[] neighboursAll;

	public TileIsland road_island;

	public int pollinated;

	public readonly int x;

	public readonly int y;

	public readonly Vector2Int pos;

	public readonly Vector3 posV3;

	public readonly Vector3 posV;

	internal int minimap_building_x;

	internal int minimap_building_y;

	internal int flash_state;

	internal ColorArray color_array;

	public MapRegion region;

	public TileZone zone;

	public MapChunk chunk;

	public Building building;

	private List<Actor> _units;

	internal int explosion_fx_stage;

	internal bool is_checked_tile;

	internal int score;

	public bool wall_check_dirty;

	private bool _has_walls_around;

	public bool is_liquid => Type.liquid;

	public TileTypeBase Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return cur_tile_type;
		}
	}

	public int Height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return data.height;
		}
		set
		{
			data.height = value;
			if (data.height < 0)
			{
				data.height = 0;
			}
			else if (data.height > 255)
			{
				data.height = 255;
			}
		}
	}

	public City zone_city => zone.city;

	public bool has_tile_up => tile_up != null;

	public bool has_tile_down => tile_down != null;

	public bool has_tile_left => tile_left != null;

	public bool has_tile_right => tile_right != null;

	public int random_animation_seed => World.world.tile_manager.random_seeds[data.tile_id];

	public int tile_id => data.tile_id;

	public WorldTile(int pX, int pY, int pTileID)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		world_tile_zone_border = new WorldTileZoneBorder();
		health = 10;
		delayed_bomb_type = "";
		score = -1;
		base._002Ector();
		last_rendered_pos_tile = WorldTilemap.EMPTY_TILE_POS;
		_units = UnsafeCollectionPool<List<Actor>, Actor>.Get();
		data = new WorldTileData(pTileID);
		pos = new Vector2Int(pX, pY);
		posV3 = new Vector3((float)pX, (float)pY);
		posV = new Vector3((float)pX, (float)pY);
		posV3.x += Actor.sprite_offset.x;
		posV3.y += Actor.sprite_offset.y;
		x = pX;
		y = pY;
	}

	public bool hasWallsAround()
	{
		if (wall_check_dirty)
		{
			wall_check_dirty = false;
			_has_walls_around = false;
			int i = 0;
			for (int num = neighboursAll.Length; i < num; i++)
			{
				if (neighboursAll[i].Type.wall)
				{
					_has_walls_around = true;
					break;
				}
			}
		}
		return _has_walls_around;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isTargeted()
	{
		return _targeted_by != null;
	}

	public bool isTargetedBy(Actor pActor)
	{
		return _targeted_by == pActor;
	}

	public void cleanTargetedBy()
	{
		_targeted_by = null;
	}

	public void setTargetedBy(Actor pActor)
	{
		_targeted_by = pActor;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void doUnits(Action<Actor> pAction)
	{
		List<Actor> units = _units;
		if (units.Count == 0)
		{
			return;
		}
		for (int i = 0; i < units.Count; i++)
		{
			Actor actor = units[i];
			if (actor.isAlive())
			{
				pAction(actor);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void doUnits(Func<Actor, bool> pAction)
	{
		List<Actor> units = _units;
		if (units.Count == 0)
		{
			return;
		}
		for (int i = 0; i < units.Count; i++)
		{
			Actor actor = units[i];
			if (actor.isAlive() && !pAction(actor))
			{
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int countUnits()
	{
		return _units.Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasUnits()
	{
		return _units.Count > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void addUnit(Actor pActor)
	{
		_units.Add(pActor);
	}

	public void resetNeighbourLists()
	{
		neighbours = null;
		neighboursAll = null;
	}

	public void pollinate()
	{
		pollinated++;
		if (pollinated > 5)
		{
			growFlowers();
			pollinated = 0;
		}
	}

	private void growFlowers()
	{
		WorldTile random = Toolbox.getRandomChunkFromTile(this).tiles.GetRandom();
		BiomeAsset biome_asset = random.Type.biome_asset;
		if (biome_asset != null && biome_asset.grow_type_selector_plants != null)
		{
			BuildingActions.tryGrowVegetationRandom(random, VegetationType.Plants);
		}
	}

	public bool canBuildOn(BuildingAsset pNewTemplate)
	{
		if (pNewTemplate.needs_farms_ground && !main_type.can_be_farm)
		{
			return false;
		}
		if (Type.liquid && !pNewTemplate.can_be_placed_on_liquid)
		{
			return false;
		}
		if (pNewTemplate.burnable && isOnFire())
		{
			return false;
		}
		if (pNewTemplate.affected_by_lava && Type.lava)
		{
			return false;
		}
		if (!pNewTemplate.can_be_placed_on_blocks && Type.block)
		{
			return false;
		}
		if (building != null && !building.isUsable() && !building.asset.flora && !pNewTemplate.remove_ruins)
		{
			return false;
		}
		if (building != null && building.isUsable() && pNewTemplate.ignore_same_building_id && building.asset == pNewTemplate)
		{
			return false;
		}
		if (!pNewTemplate.ignore_buildings && building != null && building.isUsable() && !building.asset.ignored_by_cities)
		{
			return false;
		}
		if (pNewTemplate.remove_buildings_when_dropped && building != null)
		{
			if (!building.isUsable() && pNewTemplate.remove_ruins)
			{
				return true;
			}
			if (!pNewTemplate.remove_civ_buildings && building.asset.city_building)
			{
				return false;
			}
		}
		if (!pNewTemplate.ignore_buildings && building != null && building.asset.city_building && building.isUsable() && building.asset.priority >= pNewTemplate.priority)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool hasBuilding()
	{
		return building != null;
	}

	public void setRoad()
	{
		World.world.roads_calculator.setDirty(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isSameIsland(WorldTile pTile)
	{
		return pTile.region.island == region.island;
	}

	public Color32 getColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Type.color;
	}

	internal void addNeighbour(WorldTile pNeighbour, TileDirection pDirection, List<WorldTile> pNeighbours, List<WorldTile> pNeighboursAll, bool pDiagonal = false)
	{
		if (pNeighbour == null)
		{
			world_edge = true;
			return;
		}
		pNeighboursAll.Add(pNeighbour);
		if (!pDiagonal)
		{
			pNeighbours.Add(pNeighbour);
			switch (pDirection)
			{
			case TileDirection.Up:
				tile_up = pNeighbour;
				break;
			case TileDirection.Down:
				tile_down = pNeighbour;
				break;
			case TileDirection.Left:
				tile_left = pNeighbour;
				break;
			case TileDirection.Right:
				tile_right = pNeighbour;
				break;
			}
		}
	}

	public BiomeAsset getBiome()
	{
		if (Type.is_biome)
		{
			return Type.biome_asset;
		}
		return null;
	}

	internal bool IsOceanAround()
	{
		for (int i = 0; i < neighbours.Length; i++)
		{
			if (neighbours[i].Type.layer_type == TileLayerType.Ocean)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool isGoodForBoat()
	{
		return Type.layer_type == TileLayerType.Ocean;
	}

	internal bool IsTypeAround(TileTypeBase pType)
	{
		for (int i = 0; i < neighbours.Length; i++)
		{
			if (neighbours[i].Type == pType)
			{
				return true;
			}
		}
		return false;
	}

	internal bool startFire(bool pForce = false)
	{
		if (Type.explodable)
		{
			World.world.explosion_layer.explodeBomb(this);
		}
		if (!pForce && (!Type.burnable || isOnFire()))
		{
			return false;
		}
		if (Type.liquid)
		{
			return false;
		}
		unfreeze(99);
		bool flag = false;
		if (building != null && building.isBurnable())
		{
			ActionLibrary.addBurningEffectOnTarget(null, building);
			flag = true;
		}
		if (Type.burnable | flag | pForce)
		{
			flag = true;
			if (Type.IsType("fireworks"))
			{
				EffectsLibrary.spawn("fx_fireworks", this);
			}
			data.fire_timestamp = World.world.getCurWorldTime();
			if (Type.burnable)
			{
				health -= Type.burn_rate;
				setBurned();
				World.world.flash_effects.flashPixel(this, 10);
				if (health <= 0)
				{
					MapAction.decreaseTile(this, pDamage: true);
				}
			}
			setFireData(pVal: true);
		}
		return flag;
	}

	public void setFireData(bool pVal)
	{
		World.world.tile_manager.fires[data.tile_id] = pVal;
		if (isOnFire())
		{
			WorldBehaviourActionFire.addFire(this);
		}
		else
		{
			WorldBehaviourActionFire.removeFire(this);
		}
	}

	public void updateStats()
	{
		if (top_type != null)
		{
			cur_tile_type = top_type;
		}
		else
		{
			cur_tile_type = main_type;
		}
		WorldTile[] array = neighboursAll;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].wall_check_dirty = true;
		}
		if (!isTemporaryFrozen())
		{
			return;
		}
		if (!cur_tile_type.can_be_frozen)
		{
			data.frozen = false;
			return;
		}
		TopTileType topTileType = AssetManager.top_tiles.get(main_type.freeze_to_id);
		if (topTileType == null)
		{
			if (!main_type.can_be_frozen && cur_tile_type.can_be_frozen)
			{
				Debug.LogError((object)"TILE SETTINGS CONFILICT! SET TOP TILE TO can_be_frozen FALSE!");
			}
			Debug.LogError((object)("TILE 1 f:" + cur_tile_type.freeze_to_id + " m: " + cur_tile_type.id));
			Debug.LogError((object)("TILE 2 f:" + main_type.freeze_to_id + " m: " + main_type.id));
		}
		else
		{
			cur_tile_type = topTileType;
		}
	}

	public void setTopTileType(TopTileType pTopTile, bool pUpdateStats = true)
	{
		if (top_type != pTopTile)
		{
			if (top_type != null)
			{
				zone.removeTileType(top_type, this);
			}
			if (pTopTile != null)
			{
				zone.addTileType(pTopTile, this);
			}
		}
		if (top_type != null)
		{
			top_type.hashsetRemove(this);
		}
		top_type = pTopTile;
		if (top_type != null)
		{
			top_type.hashsetAdd(this);
		}
		if (pUpdateStats)
		{
			World.world.setTileDirty(this);
			updateStats();
		}
	}

	public void setTileTypes(TileType pType, TopTileType pTopTile, bool pSetDirty = true)
	{
		setTopTileType(pTopTile, pUpdateStats: false);
		setTileType(pType, pSetDirty);
	}

	public void setTileTypes(string pType, TopTileType pTopTile)
	{
		setTopTileType(pTopTile, pUpdateStats: false);
		setTileType(pType);
	}

	public void setTileType(TileType pType, bool pSetDirty = true)
	{
		health = 10;
		if (zone != null)
		{
			if (main_type != pType)
			{
				if (main_type != null)
				{
					zone.removeTileType(main_type, this);
				}
				zone.addTileType(pType, this);
			}
			if (main_type == null)
			{
				if (pType.liquid)
				{
					zone.tiles_with_liquid++;
				}
				if (pType.ground)
				{
					zone.tiles_with_ground++;
				}
			}
			else
			{
				if (!main_type.liquid && pType.liquid)
				{
					zone.tiles_with_liquid++;
				}
				else if (main_type.liquid && !pType.liquid)
				{
					zone.tiles_with_liquid--;
				}
				if (!main_type.ground && pType.ground)
				{
					zone.tiles_with_ground++;
				}
				else if (main_type.ground && !pType.ground)
				{
					zone.tiles_with_ground--;
				}
			}
		}
		if (main_type != null)
		{
			main_type.hashsetRemove(this);
		}
		main_type = pType;
		main_type.hashsetAdd(this);
		updateStats();
		if (pSetDirty)
		{
			World.world.setTileDirty(this);
		}
		timestamp_type_changed = World.world.getCurWorldTime();
	}

	public void setTileType(string pType)
	{
		TileType tileType = AssetManager.tiles.get(pType);
		if (tileType == null)
		{
			tileType = TileLibrary.soil_low;
		}
		setTileType(tileType);
	}

	public void setBurned(int pForceVal = -1)
	{
		if (Type.can_be_set_on_fire)
		{
			if (pForceVal == -1)
			{
				setBurnedStage(15 - Randy.randomInt(0, 10));
			}
			else
			{
				setBurnedStage(burned_stages);
			}
			World.world.burned_layer.setTileDirty(this);
		}
	}

	public void setBurnedStage(int pValue)
	{
		if (burned_stages != 0 || pValue != 0)
		{
			burned_stages = pValue;
			WorldBehaviourActionBurnedTiles.addTile(this);
		}
	}

	public void removeBurn()
	{
		if (burned_stages != 0)
		{
			setBurnedStage(0);
			World.world.burned_layer.setTileDirty(this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isOnFire()
	{
		return World.world.tile_manager.fires[data.tile_id];
	}

	internal void stopFire()
	{
		if (isOnFire())
		{
			setFireData(pVal: false);
			data.fire_timestamp = -1.0;
			setBurned();
		}
	}

	internal bool canGrow()
	{
		if (!isOnFire())
		{
			return burned_stages == 0;
		}
		return false;
	}

	public void removeTrees(bool pFlash = true)
	{
		if (pFlash)
		{
			World.world.flash_effects.flashPixel(this, 20);
		}
		World.world.setTileDirty(this);
	}

	public void removeGrass(bool pFlash = true)
	{
		if (pFlash)
		{
			World.world.flash_effects.flashPixel(this, 20);
		}
		MapAction.removeGreens(this);
	}

	public void topTileEaten(int pTicks = 5)
	{
		removeGrass();
	}

	public bool isTileRank(TileRank pRank)
	{
		return main_type.rank_type == pRank;
	}

	internal void clearUnits()
	{
		_units.Clear();
	}

	internal void clear()
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		last_rendered_tile_type = null;
		health = 10;
		minimap_building_x = 0;
		minimap_building_y = 0;
		clearUnits();
		cleanTargetedBy();
		explosion_wave = 0;
		explosion_power = 0;
		pollinated = 0;
		setTileTypes(TileLibrary.deep_ocean, null, pSetDirty: false);
		delayed_timer_bomb = 0f;
		Height = 0;
		current_rendered_tile_graphics = null;
		heat = 0;
		flash_state = 0;
		burned_stages = 0;
		building = null;
		data.clear();
		explosion_fx_stage = 0;
		region = null;
		last_rendered_pos_tile = WorldTilemap.EMPTY_TILE_POS;
		world_tile_zone_border.reset();
	}

	public void Dispose()
	{
		clear();
		wall_check_dirty = false;
		_has_walls_around = false;
		if (main_type != null)
		{
			main_type.hashsetRemove(this);
		}
		main_type = null;
		if (top_type != null)
		{
			top_type.hashsetRemove(this);
		}
		top_type = null;
		cur_tile_type = null;
		color_array = null;
		tile_up = null;
		tile_down = null;
		tile_left = null;
		tile_right = null;
		neighbours = null;
		neighboursAll = null;
		road_island = null;
		world_tile_zone_border = null;
		region = null;
		zone = null;
		chunk = null;
		UnsafeCollectionPool<List<Actor>, Actor>.Release(_units);
		_units = null;
		data = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return data.tile_id;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(WorldTile pTile)
	{
		return data.tile_id == pTile.data.tile_id;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool reachableFrom(WorldTile pFromTile)
	{
		if (isSameIsland(pFromTile))
		{
			return true;
		}
		return region.island.reachableByCityFrom(pFromTile.region.island);
	}

	public bool freeze(int pDamage = 1)
	{
		if (!canBeFrozen())
		{
			return false;
		}
		if (building != null && building.isUsable() && building.asset.prevent_freeze)
		{
			return false;
		}
		data.frozen = true;
		if (Type.fast_freeze)
		{
			for (int i = 0; i < neighbours.Length; i++)
			{
				WorldTile worldTile = neighbours[i];
				if (worldTile.Type.fast_freeze && worldTile.canBeFrozen() && Randy.randomChance(0.35f))
				{
					worldTile.freeze(pDamage);
				}
			}
		}
		health = 10;
		World.world.setTileDirty(this);
		if (zone.visible)
		{
			World.world.flash_effects.flashPixel(this, 20);
		}
		if (Type.chunk_dirty_when_temperature)
		{
			MapAction.checkTileState(this, main_type, pForceMapChunk: true);
			updateStats();
		}
		return true;
	}

	public void unfreeze(int pDamage = 1)
	{
		if (!canBeUnFrozen())
		{
			return;
		}
		if (health > 0)
		{
			health -= pDamage;
			if (health > 0)
			{
				return;
			}
		}
		data.frozen = false;
		health = 10;
		World.world.setTileDirty(this);
		if (zone.visible)
		{
			World.world.flash_effects.flashPixel(this, 20);
		}
		if (Type.chunk_dirty_when_temperature)
		{
			MapAction.checkTileState(this, main_type, pForceMapChunk: true);
			updateStats();
		}
		if (!Type.fast_freeze)
		{
			return;
		}
		for (int i = 0; i < neighbours.Length; i++)
		{
			WorldTile worldTile = neighbours[i];
			if (worldTile.canBeUnFrozen() && Randy.randomChance(0.2f))
			{
				worldTile.unfreeze(pDamage);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool isSameLayer(WorldTile pTile1, WorldTile pTile2)
	{
		return pTile1.Type.layer_type == pTile2.Type.layer_type;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool canBeFrozen()
	{
		if (isFrozen())
		{
			return false;
		}
		return Type.can_be_frozen;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool canBeUnFrozen()
	{
		if (data.frozen && Type.can_be_unfrozen)
		{
			return !Type.forever_frozen;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isTemporaryFrozen()
	{
		return data.frozen;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool isFrozen()
	{
		if (!data.frozen)
		{
			return Type.forever_frozen;
		}
		return true;
	}

	public TileRank getCreepTileRank()
	{
		return main_type.creep_rank_type;
	}

	public bool hasCity()
	{
		return zone_city != null;
	}

	public void tryToBreak()
	{
		health = 0;
		unfreeze(99);
	}

	public WorldTile getWalkableTileAround(WorldTile pFrom)
	{
		foreach (WorldTile item in neighboursAll.LoopRandom())
		{
			if (item.isSameIsland(pFrom))
			{
				return item;
			}
		}
		return null;
	}

	public IEnumerable<WorldTile> getTilesAround(int pRadius)
	{
		for (int iX = -pRadius; iX <= pRadius; iX++)
		{
			for (int iY = -pRadius; iY <= pRadius; iY++)
			{
				int pX = x + iX;
				int pY = y + iY;
				yield return World.world.GetTile(pX, pY);
			}
		}
	}

	public WorldTile getTileAroundThisOnSameIsland(WorldTile pTileFrom)
	{
		foreach (WorldTile item in neighboursAll.LoopRandom())
		{
			if (item.isSameIsland(this))
			{
				return item;
			}
		}
		return null;
	}

	public WorldTile getTileAroundThisOnSameIsland(WorldTile pTileFrom, bool pClosest)
	{
		if (!pClosest)
		{
			return getTileAroundThisOnSameIsland(pTileFrom);
		}
		int num = int.MaxValue;
		WorldTile result = null;
		WorldTile[] array = neighboursAll;
		foreach (WorldTile worldTile in array)
		{
			int num2 = Toolbox.SquaredDistTile(pTileFrom, worldTile);
			if (num2 < num && worldTile.isSameIsland(this))
			{
				num = num2;
				result = worldTile;
			}
		}
		return result;
	}

	public bool isDiagonal(WorldTile pTile)
	{
		int num = Math.Abs(pTile.x - x);
		int num2 = Math.Abs(pTile.y - y);
		if (num == 1 && num2 == 1)
		{
			return true;
		}
		return false;
	}

	public bool isSameCityHere(City pCity)
	{
		return zone.isSameCityHere(pCity);
	}

	public bool isWaterAround()
	{
		if (!has_tile_down || !has_tile_up || !has_tile_left || !has_tile_right)
		{
			return true;
		}
		if (tile_down.Type.liquid || tile_up.Type.liquid || tile_left.Type.liquid || tile_right.Type.liquid)
		{
			return true;
		}
		return false;
	}

	public float distanceTo(WorldTile pTile)
	{
		return Toolbox.DistTile(this, pTile);
	}

	public WorldTile getNeighbourTileSameIsland()
	{
		foreach (WorldTile item in neighboursAll.LoopRandom())
		{
			if (item.isSameIsland(this))
			{
				return item;
			}
		}
		return this;
	}
}
