using System.Collections.Generic;
using UnityEngine;

public class WorldAgeManager
{
	private WorldAgeEffects _effects;

	private float _night_multiplier;

	private float _timer_special_action;

	private WorldAgeAsset _current_age;

	private float NIGHT_MAX = 0.6f;

	private float NIGHT_SPEED = 0.1f;

	private static MapStats _map_stats => World.world.map_stats;

	public WorldAgeAsset getCurrentAge()
	{
		return _current_age;
	}

	public WorldAgeAsset getNextAge()
	{
		return getAgeFromSlot(getNextSlotIndex());
	}

	public int getCurrentSlotIndex()
	{
		return _map_stats.world_age_slot_index;
	}

	public int getNextSlotIndex()
	{
		return Toolbox.loopIndex(_map_stats.world_age_slot_index + 1, _map_stats.world_ages_slots.Length);
	}

	public WorldAgeAsset getAgeFromSlot(int pIndex)
	{
		string text = _map_stats.world_ages_slots[pIndex];
		if (string.IsNullOrEmpty(text))
		{
			text = "age_unknown";
		}
		return AssetManager.era_library.get(text);
	}

	public void setAgeToSlot(WorldAgeAsset pAsset, int pSlotIndex)
	{
		if (!(_map_stats.world_ages_slots[pSlotIndex] == pAsset.id))
		{
			_map_stats.world_ages_slots[pSlotIndex] = pAsset.id;
			if (pSlotIndex == _map_stats.world_age_slot_index)
			{
				setCurrentAge(pAsset, pOverrideTime: false);
			}
		}
	}

	public float getNightMod()
	{
		if ((Object)(object)_effects == (Object)null)
		{
			return 0f;
		}
		if (_effects.override_night)
		{
			return _effects.night_value_mat;
		}
		return _night_multiplier;
	}

	public bool shouldShowLights()
	{
		if (getNightMod() != 0f)
		{
			return true;
		}
		return false;
	}

	internal void loadAge()
	{
		bool pOverrideTime = false;
		if (string.IsNullOrEmpty(_map_stats.world_age_id) || _map_stats.world_age_id == "age_unknown")
		{
			_map_stats.world_age_id = "age_hope";
			pOverrideTime = true;
		}
		WorldAgeAsset worldAgeAsset = AssetManager.era_library.get(_map_stats.world_age_id);
		if (worldAgeAsset == null)
		{
			worldAgeAsset = WorldAgeLibrary.hope;
			pOverrideTime = true;
		}
		setCurrentAge(worldAgeAsset, pOverrideTime);
	}

	public void update(float pElapsed)
	{
		if (!World.world.isPaused() && !isPaused())
		{
			_map_stats.current_age_progress += pElapsed / (_map_stats.current_world_ages_duration / _map_stats.world_ages_speed_multiplier);
			if (_map_stats.current_age_progress >= 1f)
			{
				startNextAge();
			}
		}
		updateEffects(pElapsed);
	}

	public float getTimeTillNextAge()
	{
		return _map_stats.current_world_ages_duration * (1f - _map_stats.current_age_progress);
	}

	public bool isPaused()
	{
		return _map_stats.is_world_ages_paused;
	}

	public void togglePlay(bool pState)
	{
		_map_stats.is_world_ages_paused = !pState;
	}

	public void setAgesSpeedMultiplier(float pMultiplier)
	{
		_map_stats.world_ages_speed_multiplier = pMultiplier;
	}

	public void debugEndAge()
	{
		_map_stats.current_world_ages_duration = 5f;
	}

	public void startNextAge(float pStartProgress = 0f)
	{
		int nextSlotIndex = getNextSlotIndex();
		setCurrentSlotIndex(nextSlotIndex, pStartProgress);
	}

	public void setCurrentSlotIndex(int pIndex, float pStartProgress = 1f)
	{
		_map_stats.world_age_slot_index = pIndex;
		_map_stats.current_age_progress = pStartProgress;
		WorldAgeAsset ageFromSlot = getAgeFromSlot(pIndex);
		ageFromSlot = checkAge(ageFromSlot);
		setCurrentAge(ageFromSlot);
	}

	private void updateEffects(float pElapsed)
	{
		if ((Object)(object)_effects == (Object)null)
		{
			_effects = WorldAgeEffects.instance;
			return;
		}
		_effects.update(World.world.delta_time);
		if (World.world.isPaused() || _current_age == null)
		{
			return;
		}
		if (_current_age.overlay_darkness)
		{
			float num = NIGHT_MAX * ((float)PlayerConfig.getIntValue("age_night_effect") / 100f);
			_night_multiplier += pElapsed * NIGHT_SPEED;
			if (_night_multiplier > num)
			{
				_night_multiplier = num;
			}
		}
		else
		{
			_night_multiplier -= pElapsed * NIGHT_SPEED;
			if (_night_multiplier < 0f)
			{
				_night_multiplier = 0f;
			}
		}
		if (_current_age.special_effect_action != null)
		{
			_timer_special_action -= pElapsed;
			if (_timer_special_action <= 0f)
			{
				_current_age.special_effect_action();
				_timer_special_action = _current_age.special_effect_interval;
			}
		}
	}

	private void calcNextEraTime(WorldAgeAsset pAsset, bool pForceMax = false)
	{
		int num = (int)((float)Randy.randomInt(pAsset.years_min, pAsset.years_max) * 12f * 5f);
		if (pForceMax)
		{
			num = (int)((float)pAsset.years_max * 12f * 5f);
		}
		_map_stats.current_world_ages_duration = num;
	}

	private void setCurrentAge(WorldAgeAsset pAsset, bool pOverrideTime = true)
	{
		pAsset = checkAge(pAsset);
		_map_stats.world_age_id = pAsset.id;
		_map_stats.world_age_started_at = World.world.getCurWorldTime();
		if (_current_age != pAsset)
		{
			_map_stats.same_world_age_started_at = World.world.getCurWorldTime();
		}
		if (pOverrideTime)
		{
			calcNextEraTime(pAsset);
		}
		_current_age = pAsset;
		WorldBehaviourClouds.setEra(pAsset);
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int i = 0; i < simpleList.Count; i++)
		{
			simpleList[i].setStatsDirty();
		}
		_timer_special_action = pAsset.special_effect_interval;
	}

	public void clear()
	{
		_current_age = null;
	}

	public void prepare()
	{
		if (_current_age == null)
		{
			setCurrentAge(WorldAgeLibrary.hope);
		}
	}

	public bool isWinter()
	{
		if (_current_age == null)
		{
			return false;
		}
		return _current_age.flag_winter;
	}

	public bool isNight()
	{
		if (_current_age == null)
		{
			return false;
		}
		return _current_age.flag_night;
	}

	public bool isChaosAge()
	{
		if (_current_age == null)
		{
			return false;
		}
		return _current_age.flag_chaos;
	}

	public bool isLightAge()
	{
		if (_current_age == null)
		{
			return false;
		}
		return _current_age.flag_light_age;
	}

	public bool isCurrentAge(WorldAgeAsset pAgeAsset)
	{
		return _current_age == pAgeAsset;
	}

	private WorldAgeAsset checkAge(WorldAgeAsset pAsset)
	{
		if (pAsset.id == "age_unknown")
		{
			pAsset = AssetManager.era_library.list_only_normal.GetRandom();
		}
		return pAsset;
	}

	public int calculateMoonsLeft()
	{
		return (int)((_map_stats.current_world_ages_duration * (1f - _map_stats.current_age_progress) / 5f + 1f) / _map_stats.world_ages_speed_multiplier);
	}

	public void setDefaultAges()
	{
		for (int i = 1; i < 9; i++)
		{
			using ListPool<WorldAgeAsset> listPool = new ListPool<WorldAgeAsset>(AssetManager.era_library.pool_by_slots[i]);
			WorldAgeAsset random = listPool.GetRandom();
			if (random.link_default_slots)
			{
				bool flag = false;
				foreach (int default_slot in random.default_slots)
				{
					if (default_slot != i && getAgeFromSlot(default_slot - 1) == random)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					listPool.Remove(random);
					random = listPool.GetRandom();
				}
			}
			setAgeToSlot(random, i - 1);
		}
		setCurrentSlotIndex(0, 0f);
	}
}
