using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BaseStats : ICloneable
{
	[JsonProperty]
	private List<BaseStatsContainer> _stats_list = new List<BaseStatsContainer>();

	private Dictionary<string, BaseStatsContainer> _stats_dict = new Dictionary<string, BaseStatsContainer>();

	private List<BaseStatsContainer> _multipliers_list;

	[JsonProperty]
	private HashSet<string> _tags;

	public float this[string pKey]
	{
		get
		{
			return get(pKey);
		}
		set
		{
			set(pKey, value);
		}
	}

	private void set(string pID, float pAmount)
	{
		BaseStatAsset baseStatAsset = AssetManager.base_stats_library.get(pID);
		if (baseStatAsset.ignore)
		{
			return;
		}
		BaseStatsContainer value2;
		if (pAmount == 0f && !baseStatAsset.normalize)
		{
			if (_stats_dict.TryGetValue(pID, out var value))
			{
				if (baseStatAsset.multiplier)
				{
					_multipliers_list?.Remove(value);
				}
				_stats_list.Remove(value);
				_stats_dict.Remove(pID);
			}
		}
		else if (!_stats_dict.TryGetValue(pID, out value2))
		{
			value2 = new BaseStatsContainer();
			value2.value = pAmount;
			value2.id = pID;
			_stats_dict[pID] = value2;
			_stats_list.Add(value2);
			if (baseStatAsset.multiplier)
			{
				if (_multipliers_list == null)
				{
					_multipliers_list = new List<BaseStatsContainer>();
				}
				_multipliers_list.Add(value2);
			}
		}
		else
		{
			_stats_dict[pID].value = pAmount;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public List<BaseStatsContainer> getList()
	{
		return _stats_list;
	}

	public void checkStatName(string pID)
	{
		if (Config.game_loaded && AssetManager.base_stats_library.get(pID) == null)
		{
			Debug.LogError((object)("base_stats_library.get() - no asset with id: " + pID));
			Debug.LogError((object)"Only call stats with S.id, never with pure strings to avoid typos");
		}
	}

	public float get(string pID)
	{
		if (_stats_dict.TryGetValue(pID, out var value))
		{
			return value.value;
		}
		return 0f;
	}

	public bool hasStat(string pID)
	{
		return _stats_dict.ContainsKey(pID);
	}

	public BaseStatsContainer getContainer(string pID)
	{
		BaseStatsContainer value = null;
		_stats_dict.TryGetValue(pID, out value);
		return value;
	}

	internal void mergeStats(BaseStats pStats, float pMultiplier = 1f)
	{
		for (int i = 0; i < pStats._stats_list.Count; i++)
		{
			BaseStatsContainer baseStatsContainer = pStats._stats_list[i];
			this[baseStatsContainer.id] += baseStatsContainer.value * pMultiplier;
		}
		if (pStats._tags != null)
		{
			if (_tags == null)
			{
				_tags = new HashSet<string>(pStats._tags);
			}
			else
			{
				_tags.UnionWith(pStats._tags);
			}
		}
	}

	public void checkMultipliers()
	{
		if (_multipliers_list == null)
		{
			return;
		}
		for (int i = 0; i < _multipliers_list.Count; i++)
		{
			BaseStatsContainer baseStatsContainer = _multipliers_list[i];
			BaseStatAsset asset = baseStatsContainer.asset;
			BaseStatsContainer container = getContainer(asset.main_stat_to_multiply);
			if (container != null)
			{
				container.value += container.value * baseStatsContainer.value;
			}
		}
	}

	public bool hasTag(string pTag)
	{
		return _tags?.Contains(pTag) ?? false;
	}

	public bool hasTags(string[] pTags)
	{
		return _tags?.Overlaps(pTags) ?? false;
	}

	public void normalize()
	{
		for (int i = 0; i < _stats_list.Count; i++)
		{
			_stats_list[i].normalize();
		}
	}

	internal void clear()
	{
		_multipliers_list?.Clear();
		_stats_list.Clear();
		_stats_dict.Clear();
		_tags?.Clear();
	}

	public void reset()
	{
		clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void addTag(string pTag)
	{
		if (_tags == null)
		{
			_tags = new HashSet<string>();
		}
		_tags.Add(pTag);
	}

	public bool hasTags()
	{
		HashSet<string> tags = _tags;
		if (tags == null)
		{
			return false;
		}
		return tags.Count > 0;
	}

	public bool hasStats()
	{
		List<BaseStatsContainer> stats_list = _stats_list;
		if (stats_list == null)
		{
			return false;
		}
		return stats_list.Count > 0;
	}

	public bool ShouldSerialize_tags()
	{
		return hasTags();
	}

	public bool ShouldSerialize_stats_list()
	{
		return hasStats();
	}

	public void addCombatAction(string pCombatAction)
	{
	}

	public object Clone()
	{
		BaseStats baseStats = new BaseStats();
		baseStats.mergeStats(this);
		return baseStats;
	}
}
