using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using tools;

public class BuildingManager : SimSystemManager<Building, BuildingData>
{
	private List<WorldTile> _temp_list_tiles = new List<WorldTile>();

	private JobManagerBuildings _job_manager;

	private Building[] _array_visible_buildings = new Building[0];

	private int _visible_buildings_count;

	public BuildingRenderData render_data = new BuildingRenderData(4096);

	public HashSet<Building> occupied_buildings = new HashSet<Building>();

	public List<Building> visible_stockpiles = new List<Building>();

	public List<Building> sparkles = new List<Building>();

	public MultiStackPool<BaseBuildingComponent> component_pool = new MultiStackPool<BaseBuildingComponent>();

	private bool _need_normal_check;

	public BuildingManager()
	{
		type_id = "building";
		_job_manager = new JobManagerBuildings("buildings");
	}

	public override void clear()
	{
		_job_manager.clear();
		Array.Clear(_array_visible_buildings, 0, _array_visible_buildings.Length);
		_temp_list_tiles.Clear();
		occupied_buildings.Clear();
		checkContainer();
		scheduleDestroyAllOnWorldClear();
		checkObjectsToDestroy();
		base.clear();
	}

	protected override void destroyObject(Building pBuilding)
	{
		base.destroyObject(pBuilding);
		if (pBuilding.hasHousingLogic())
		{
			event_houses = true;
		}
		pBuilding.setAlive(pValue: false);
		pBuilding.asset.buildings.Remove(pBuilding);
		occupied_buildings.Remove(pBuilding);
		removeObject(pBuilding);
		_job_manager.removeObject(pBuilding, pBuilding.batch);
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		Bench.bench("buildings", "game_total");
		checkContainer();
		_job_manager.updateBase(pElapsed);
		checkContainer();
		Bench.benchEnd("buildings", "game_total", pSaveCounter: false, 0L);
	}

	public override void loadFromSave(List<BuildingData> pList)
	{
		base.loadFromSave(pList);
		checkContainer();
	}

	internal Building addBuilding(string pID, WorldTile pTile, bool pCheckForBuild = false, bool pSfx = false, BuildPlacingType pType = BuildPlacingType.New)
	{
		BuildingAsset pAsset = AssetManager.buildings.get(pID);
		return addBuilding(pAsset, pTile, pCheckForBuild, pSfx, pType);
	}

	internal Building addBuilding(BuildingAsset pAsset, WorldTile pTile, bool pCheckForBuild = false, bool pSfx = false, BuildPlacingType pType = BuildPlacingType.New)
	{
		if (pCheckForBuild && !canBuildFrom(pTile, pAsset, null, pType))
		{
			return null;
		}
		Building building = newObject();
		building.create();
		building.setBuilding(pTile, pAsset, null);
		building.checkStartSpawnAnimation();
		if (building.asset.city_building)
		{
			World.world.map_stats.housesBuilt++;
		}
		return building;
	}

	protected override void addObject(Building pObject)
	{
		base.addObject(pObject);
		_job_manager.addNewObject(pObject);
	}

	public override Building loadObject(BuildingData pData)
	{
		if (pData.state == BuildingState.Removed)
		{
			return null;
		}
		BuildingAsset buildingAsset = AssetManager.buildings.get(pData.asset_id);
		if (buildingAsset == null)
		{
			return null;
		}
		WorldTile tileSimple = World.world.GetTileSimple(pData.mainX, pData.mainY);
		if (!canBuildFrom(tileSimple, buildingAsset, null, BuildPlacingType.Load))
		{
			return null;
		}
		Building building = base.loadObject(pData);
		building.create();
		building.setBuilding(tileSimple, buildingAsset, pData);
		building.loadBuilding(pData);
		return building;
	}

	internal bool canBuildFrom(WorldTile pTile, BuildingAsset pNewBuildingAsset, City pCity, BuildPlacingType pType = BuildPlacingType.New, bool pFloraGrowth = false)
	{
		Subspecies subspecies = pCity?.getMainSubspecies();
		bool flag = subspecies != null && pNewBuildingAsset.city_building && pNewBuildingAsset.check_for_adaptation_tags;
		if (flag && pTile.Type.is_biome)
		{
			string only_allowed_to_build_with_tag = pTile.Type.only_allowed_to_build_with_tag;
			if (only_allowed_to_build_with_tag != null && !subspecies.hasMetaTag(only_allowed_to_build_with_tag))
			{
				return false;
			}
		}
		BuildingFundament fundament = pNewBuildingAsset.fundament;
		int num = pTile.x - fundament.left;
		int num2 = pTile.y - fundament.bottom;
		int width = fundament.width;
		int height = fundament.height;
		bool flag2 = false;
		bool flag3 = false;
		bool docks = pNewBuildingAsset.docks;
		List<WorldTile> temp_list_tiles = _temp_list_tiles;
		temp_list_tiles.Clear();
		bool flag4 = !WorldLawLibrary.world_law_roots_without_borders.isEnabled();
		WorldTile worldTile = pCity?.getTile();
		if (pCity != null && worldTile == null)
		{
			return false;
		}
		bool flag5 = pType == BuildPlacingType.New && Randy.randomChance(0.1f);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				WorldTile tile = World.world.GetTile(num + i, num2 + j);
				if (tile == null)
				{
					return false;
				}
				if (flag)
				{
					string only_allowed_to_build_with_tag2 = tile.Type.only_allowed_to_build_with_tag;
					if (only_allowed_to_build_with_tag2 != null && !subspecies.hasMetaTag(only_allowed_to_build_with_tag2))
					{
						return false;
					}
				}
				temp_list_tiles.Add(tile);
				Building building = tile.building;
				TileTypeBase type = tile.Type;
				if (docks)
				{
					if (type.ocean && OceanHelper.goodForNewDock(tile))
					{
						flag3 = true;
					}
					if (type.ground)
					{
						flag2 = true;
					}
				}
				if (pCity != null)
				{
					if (!docks && !tile.isSameIsland(worldTile))
					{
						return false;
					}
					if (!tile.isSameCityHere(pCity))
					{
						return false;
					}
					if (pNewBuildingAsset.only_build_tiles && !type.can_build_on)
					{
						return false;
					}
				}
				if (((pType != BuildPlacingType.Load || (!(type.id == "frozen_low") && !(type.id == "frozen_high"))) & flag4) && !pNewBuildingAsset.isOverlaysBiomeTags(type))
				{
					if (!pFloraGrowth)
					{
						return false;
					}
					if (!pNewBuildingAsset.isOverlaysBiomeSpreadTags(type))
					{
						return false;
					}
				}
				if (pNewBuildingAsset.flora && building != null)
				{
					if (!building.asset.flora)
					{
						return false;
					}
					if (pNewBuildingAsset.flora_size <= building.asset.flora_size)
					{
						if (flag5 && building.asset.flora_size == FloraSize.Tiny && building.asset.flora_size == pNewBuildingAsset.flora_size)
						{
							if (building.asset == pNewBuildingAsset)
							{
								return false;
							}
						}
						else if (!building.isRuin())
						{
							return false;
						}
					}
					if (!tile.canGrow())
					{
						return false;
					}
				}
				if (type.liquid && !pNewBuildingAsset.can_be_placed_on_liquid)
				{
					return false;
				}
				if (pNewBuildingAsset.destroy_on_liquid && type.ocean)
				{
					return false;
				}
				if (!tile.canBuildOn(pNewBuildingAsset))
				{
					return false;
				}
				if (!pNewBuildingAsset.check_for_close_building || pType != BuildPlacingType.New)
				{
					continue;
				}
				if (i == 0)
				{
					if (isBuildingNearby(tile.tile_left))
					{
						return false;
					}
				}
				else if (i == width - 1 && isBuildingNearby(tile.tile_right))
				{
					return false;
				}
				if (j == 0)
				{
					if (isBuildingNearby(tile.tile_down))
					{
						return false;
					}
					if (tile.has_tile_down && isBuildingNearby(tile.tile_down.tile_down))
					{
						return false;
					}
				}
				else if (j == height - 1)
				{
					if (isBuildingNearby(tile.tile_up))
					{
						return false;
					}
					if (tile.has_tile_up && isBuildingNearby(tile.tile_up.tile_up))
					{
						return false;
					}
				}
			}
		}
		if (docks && pType == BuildPlacingType.New)
		{
			if (flag3 && !flag2)
			{
				for (int k = 0; k < temp_list_tiles.Count; k++)
				{
					WorldTile worldTile2 = temp_list_tiles[k];
					for (int l = 0; l < worldTile2.neighbours.Length; l++)
					{
						WorldTile worldTile3 = worldTile2.neighbours[l];
						if (worldTile3.Type.ground && worldTile3.region.island == worldTile?.region.island)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		return true;
	}

	private bool isBuildingNearby(WorldTile pTile)
	{
		if (pTile == null)
		{
			return true;
		}
		Building building = pTile.building;
		if (building != null && building.isUsable() && building.asset.city_building)
		{
			return true;
		}
		return false;
	}

	public Building getNearbyBuildingToLive(Actor pActor, bool pOnlyBuilt)
	{
		foreach (Building buildingFromZone in getBuildingFromZones(pActor.current_tile, 10f))
		{
			if (!buildingFromZone.asset.hasHousingSlots() || !buildingFromZone.current_tile.isSameIsland(pActor.current_tile) || !buildingFromZone.hasResidentSlots())
			{
				continue;
			}
			if (pOnlyBuilt)
			{
				if (buildingFromZone.isUnderConstruction())
				{
					continue;
				}
			}
			else if (!buildingFromZone.isUnderConstruction())
			{
				continue;
			}
			if (buildingFromZone.kingdom == pActor.kingdom)
			{
				return buildingFromZone;
			}
		}
		return null;
	}

	public IEnumerable<Building> getBuildingFromZones(WorldTile pTile, float pRadius)
	{
		foreach (Building item in checkZoneForBuilding(pTile, pTile.zone, pRadius))
		{
			yield return item;
		}
		float num = pRadius / 8f;
		int tSize = (int)num + 1;
		int startX = pTile.zone.x - tSize;
		int startY = pTile.zone.y - tSize;
		for (int iX = 0; iX < tSize * 2; iX++)
		{
			for (int iY = 0; iY < tSize * 2; iY++)
			{
				TileZone zone = World.world.zone_calculator.getZone(iX + startX, iY + startY);
				if (zone == null)
				{
					continue;
				}
				foreach (Building item2 in checkZoneForBuilding(pTile, zone, pRadius))
				{
					yield return item2;
				}
			}
		}
	}

	private IEnumerable<Building> checkZoneForBuilding(WorldTile pTile, TileZone pZone, float pRadius)
	{
		if (!pZone.buildings_all.Any())
		{
			yield break;
		}
		float tRadius = pRadius * pRadius;
		foreach (Building item in pZone.buildings_all)
		{
			if ((tRadius == 0f || !((float)Toolbox.SquaredDistTile(item.current_tile, pTile) > tRadius)) && !item.isRuin() && item.current_tile.isSameIsland(pTile))
			{
				yield return item;
			}
		}
	}

	public void debugJobManager(DebugTool pTool)
	{
		_job_manager.debug(pTool);
	}

	private void prepareLists()
	{
		_array_visible_buildings = Toolbox.checkArraySize(_array_visible_buildings, Count);
		render_data.checkSize(Count);
		visible_stockpiles.Clear();
		sparkles.Clear();
		checkContainer();
	}

	internal void calculateVisibleBuildings()
	{
		Bench.bench("buildings_prepare", "game_total");
		prepareLists();
		_visible_buildings_count = 0;
		Bench.benchEnd("buildings_prepare", "game_total", pSaveCounter: false, 0L);
		if (!World.world.quality_changer.shouldRenderBuildings())
		{
			Bench.clearBenchmarkEntrySkipMultiple("game_total", "buildings_render_data_parallel_256", "buildings_fill_visible", "buildings_render_data_normal");
			return;
		}
		Bench.bench("buildings_fill_visible", "game_total");
		fillVisibleObjects();
		Bench.benchEnd("buildings_fill_visible", "game_total", pSaveCounter: false, 0L);
		Bench.bench("buildings_render_data_parallel_256", "game_total");
		precalculateRenderDataParallel();
		Bench.benchEnd("buildings_render_data_parallel_256", "game_total", pSaveCounter: false, 0L);
		Bench.bench("buildings_render_data_normal", "game_total");
		precalculateRenderDataNormal();
		Bench.benchEnd("buildings_render_data_normal", "game_total", pSaveCounter: false, 0L);
	}

	private void fillVisibleObjects()
	{
		Building[] array_visible_buildings = _array_visible_buildings;
		List<TileZone> visibleZones = World.world.zone_camera.getVisibleZones();
		int count = visibleZones.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			List<Building> buildings_render_list = visibleZones[i].buildings_render_list;
			int count2 = buildings_render_list.Count;
			buildings_render_list.CopyTo(array_visible_buildings, num);
			num += count2;
		}
		_visible_buildings_count = num;
	}

	private void precalculateRenderDataParallel()
	{
		Building[] tArrayVisibleBuildings = _array_visible_buildings;
		bool tNeedShadows = World.world.quality_changer.shouldRenderBuildingShadows();
		int tTotalVisibleObjects = _visible_buildings_count;
		Vector3[] tRenderScales = render_data.scales;
		Vector3[] tRenderPositions = render_data.positions;
		Vector3[] tRenderRotations = render_data.rotations;
		Material[] tRenderMaterials = render_data.materials;
		bool[] tRenderFlipXStates = render_data.flip_x_states;
		Color[] tRenderColors = render_data.colors;
		Sprite[] tRenderMainSprites = render_data.main_sprites;
		Sprite[] tRenderColoredSprites = render_data.colored_sprites;
		bool[] tRenderShadows = render_data.shadows;
		Sprite[] tRenderShadowSprites = render_data.shadow_sprites;
		int tDynamicBatchSize = 256;
		int toExclusive = ParallelHelper.calcTotalBatches(tTotalVisibleObjects, tDynamicBatchSize);
		bool tNeedNormalCheck = false;
		Parallel.For(0, toExclusive, World.world.parallel_options, delegate(int pBatchIndex)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			int num = ParallelHelper.calculateBatchBeg(pBatchIndex, tDynamicBatchSize);
			int num2 = ParallelHelper.calculateBatchEnd(num, tDynamicBatchSize, tTotalVisibleObjects);
			for (int i = num; i < num2; i++)
			{
				Building building = tArrayVisibleBuildings[i];
				BuildingAsset asset = building.asset;
				tRenderScales[i] = building.getCurrentScale();
				tRenderPositions[i] = building.cur_transform_position;
				tRenderRotations[i] = building.current_rotation;
				tRenderMaterials[i] = building.material;
				tRenderFlipXStates[i] = building.flip_x;
				tRenderColors[i] = building.kingdom.asset.color_building;
				Sprite val = building.calculateMainSprite();
				tRenderMainSprites[i] = val;
				if (building.isColoredSpriteNeedsCheck(val))
				{
					tRenderColoredSprites[i] = null;
					tNeedNormalCheck = true;
				}
				else
				{
					tRenderColoredSprites[i] = building.getLastColoredSprite();
				}
				if (tNeedShadows)
				{
					tRenderShadows[i] = asset.shadow && !building.chopped;
					tRenderShadowSprites[i] = DynamicSprites.getShadowBuilding(building.asset, tRenderMainSprites[i]);
				}
				if (asset.is_stockpile)
				{
					tNeedNormalCheck = true;
				}
				if (asset.sparkle_effect)
				{
					tNeedNormalCheck = true;
				}
			}
		});
		_need_normal_check = tNeedNormalCheck;
	}

	private void precalculateRenderDataNormal()
	{
		if (!_need_normal_check)
		{
			return;
		}
		BuildingRenderData buildingRenderData = render_data;
		int visible_buildings_count = _visible_buildings_count;
		Sprite[] colored_sprites = buildingRenderData.colored_sprites;
		Sprite[] main_sprites = buildingRenderData.main_sprites;
		for (int i = 0; i < visible_buildings_count; i++)
		{
			Building building = _array_visible_buildings[i];
			if (building.asset.is_stockpile)
			{
				visible_stockpiles.Add(building);
			}
			if (building.asset.sparkle_effect)
			{
				sparkles.Add(building);
			}
			if (colored_sprites[i] == null)
			{
				colored_sprites[i] = building.calculateColoredSprite(main_sprites[i]);
			}
		}
	}

	public Building[] getVisibleBuildings()
	{
		return _array_visible_buildings;
	}

	public int countVisibleBuildings()
	{
		return _visible_buildings_count;
	}

	public void checkWobblySetting()
	{
		bool flag = PlayerConfig.optionEnabled("tree_wind", OptionType.Bool);
		foreach (DynamicSpritesAsset item in AssetManager.dynamic_sprites_library.list)
		{
			if (item.check_wobbly_setting)
			{
				item.big_atlas = !flag;
			}
		}
		foreach (DynamicSpritesAsset item2 in AssetManager.dynamic_sprites_library.list)
		{
			if (item2.buildings)
			{
				item2.resetAtlas();
			}
		}
		AssetManager.buildings.checkAtlasLink(flag);
		using IEnumerator<Building> enumerator2 = GetEnumerator();
		while (enumerator2.MoveNext())
		{
			Building current3 = enumerator2.Current;
			current3.checkMaterial();
			current3.clearSprites();
		}
	}

	public JobManagerBuildings getJobManager()
	{
		return _job_manager;
	}
}
