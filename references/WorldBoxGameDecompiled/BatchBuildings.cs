using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class BatchBuildings : Batch<Building>
{
	public ObjectContainer<Building> c_main;

	public ObjectContainer<Building> c_scale;

	public ObjectContainer<Building> c_angle;

	public ObjectContainer<Building> c_components;

	public ObjectContainer<Building> c_spread_trees;

	public ObjectContainer<Building> c_spread_plants;

	public ObjectContainer<Building> c_spread_fungi;

	public ObjectContainer<Building> c_poop;

	public ObjectContainer<Building> c_resource_shaker;

	public ObjectContainer<Building> c_shake;

	public ObjectContainer<Building> c_position_dirty;

	public ObjectContainer<Building> c_tiles_dirty;

	public ObjectContainer<Building> c_stats_dirty;

	public ObjectContainer<Building> c_auto_remove;

	public Random rnd;

	private float _timer_spread_trees;

	private float _timer_spread_plants;

	private float _timer_poop_flora;

	private float _timer_spread_fungi;

	public List<Action> actions_to_run;

	protected override void createJobs()
	{
		addJob(null, prepare, JobType.Parallel, "prepare");
		createJob(out c_scale, updateScale, JobType.Parallel, "update_scale");
		createJob(out c_angle, updateAngle, JobType.Parallel, "update_angle");
		createJob(out c_resource_shaker, updateResourceShaker, JobType.Parallel, "update_resource_shaker");
		createJob(out c_stats_dirty, updateStatsDirty, JobType.Parallel, "update_dirty_stats");
		createJob(out c_shake, updateShake, JobType.Parallel, "update_shake");
		createJob(out c_main, updateVisibility, JobType.Parallel, "update_visibility");
		createJob(out c_tiles_dirty, updateTilesDirty, JobType.Post, "update_dirty_tiles");
		createJob(out c_auto_remove, updateAutoRemove, JobType.Post, "update_auto_remove");
		createJob(out c_components, updateComponents, JobType.Post, "update_components");
		createJob(out c_spread_trees, updateSpreadTrees, JobType.Post, "update_spread_trees");
		createJob(out c_spread_plants, updateSpreadPlants, JobType.Post, "update_spread_plants");
		createJob(out c_spread_fungi, updateSpreadFungi, JobType.Post, "update_spread_fungi");
		createJob(out c_poop, updatePoopTurningIntoFlora, JobType.Post, "update_poop_turning_into_flora");
		createJob(out c_position_dirty, updatePositionsDirty, JobType.Post, "update_dirty_positions");
		main = c_main;
		applyParallelResults = (JobUpdater)Delegate.Combine(applyParallelResults, new JobUpdater(applyTweenActions));
	}

	public void applyTweenActions()
	{
		if (actions_to_run.Count != 0)
		{
			for (int i = 0; i < actions_to_run.Count; i++)
			{
				actions_to_run[i]();
			}
			actions_to_run.Clear();
		}
	}

	internal override void clear()
	{
		base.clear();
		clearParallelResults?.Invoke();
		actions_to_run.Clear();
	}

	private void updateScale()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateScale();
			}
		}
	}

	private void updateAngle()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateAngle(_elapsed);
			}
		}
	}

	private void updateVisibility()
	{
		if (!check(_cur_container))
		{
			return;
		}
		bool flag = MapBox.isRenderGameplay();
		bool flag2 = World.world.quality_changer.shouldRenderBuildings();
		if (!DebugConfig.isOn(DebugOption.ScaleEffectEnabled) && flag2 && !flag)
		{
			flag2 = false;
		}
		Building[] array = _array;
		int count = _count;
		if (flag)
		{
			for (int i = 0; i < count; i++)
			{
				Building obj = array[i];
				obj.is_visible = obj.current_tile.zone.visible;
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				array[j].is_visible = flag2;
			}
		}
	}

	private void updateTilesDirty()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].checkDirtyTiles();
			}
		}
	}

	private void updateAutoRemove()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateAutoRemove(_elapsed);
			}
		}
	}

	private void updateStatsDirty()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateStats();
			}
		}
	}

	private void updateComponents()
	{
		if (!check(_cur_container) || World.world.isPaused())
		{
			return;
		}
		Building[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Building building = array[i];
			if (building.isUsable())
			{
				building.updateComponents(_elapsed);
			}
		}
	}

	private void updateSpreadTrees()
	{
		if (!check(_cur_container) || World.world.isPaused() || !WorldLawLibrary.world_law_spread_trees.isEnabled())
		{
			return;
		}
		if (_timer_spread_trees >= 0f)
		{
			_timer_spread_trees -= _elapsed;
			if (_timer_spread_trees > 0f)
			{
				return;
			}
			_timer_spread_trees = WorldLawLibrary.getIntervalSpreadTrees();
		}
		Building[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Building building = array[i];
			if (building.isUsable())
			{
				building.checkVegetationSpread(_elapsed);
			}
		}
	}

	private void updateSpreadPlants()
	{
		if (!check(_cur_container) || World.world.isPaused() || !WorldLawLibrary.world_law_spread_plants.isEnabled())
		{
			return;
		}
		if (_timer_spread_plants >= 0f)
		{
			_timer_spread_plants -= _elapsed;
			if (_timer_spread_plants > 0f)
			{
				return;
			}
			_timer_spread_plants = WorldLawLibrary.getIntervalSpreadPlants();
		}
		Building[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Building building = array[i];
			if (building.isUsable())
			{
				building.checkVegetationSpread(_elapsed);
			}
		}
	}

	private void updatePoopTurningIntoFlora()
	{
		if (!check(_cur_container) || World.world.isPaused())
		{
			return;
		}
		if (_timer_poop_flora >= 0f)
		{
			_timer_poop_flora -= _elapsed;
			if (_timer_poop_flora > 0f)
			{
				return;
			}
			_timer_poop_flora = 5f;
		}
		Building[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Building building = array[i];
			if (building.isUsable() && !(building.getExistenceMonths() < (float)SimGlobals.m.months_till_pool_turns_into_flora) && !Randy.randomChance(0.7f))
			{
				WorldTile current_tile = building.current_tile;
				BiomeAsset biome_asset = current_tile.Type.biome_asset;
				if (biome_asset != null && biome_asset.grow_type_selector_plants != null)
				{
					building.startDestroyBuilding();
					BuildingActions.tryGrowVegetationRandom(current_tile, VegetationType.Plants, pOnStart: false, pCheckLimit: false, pCheckRandom: false);
				}
			}
		}
	}

	private void updateSpreadFungi()
	{
		if (!check(_cur_container) || World.world.isPaused() || !WorldLawLibrary.world_law_spread_fungi.isEnabled())
		{
			return;
		}
		if (_timer_spread_fungi >= 0f)
		{
			_timer_spread_fungi -= _elapsed;
			if (_timer_spread_fungi > 0f)
			{
				return;
			}
			_timer_spread_fungi = WorldLawLibrary.getIntervalSpreadFungi();
		}
		Building[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Building building = array[i];
			if (building.isUsable())
			{
				building.checkVegetationSpread(_elapsed);
			}
		}
	}

	private void updateResourceShaker()
	{
		if (!check(_cur_container) || World.world.isPaused())
		{
			return;
		}
		Building[] array = _array;
		int count = _count;
		for (int i = 0; i < count; i++)
		{
			Building building = array[i];
			if (building.isUsable())
			{
				building.updateTimerShakeResources(_elapsed);
			}
		}
	}

	private void updateShake()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updateShake(_elapsed);
			}
		}
	}

	private void updatePositionsDirty()
	{
		if (check(_cur_container))
		{
			Building[] array = _array;
			int count = _count;
			for (int i = 0; i < count; i++)
			{
				array[i].updatePosition();
			}
		}
	}

	internal override void add(Building pBuilding)
	{
		base.add(pBuilding);
		pBuilding.batch = this;
	}

	internal override void remove(Building pObject)
	{
		base.remove(pObject);
		pObject.batch = null;
	}

	public BatchBuildings()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		rnd = new Random(10u);
		actions_to_run = new List<Action>();
		base._002Ector();
	}
}
