using System;
using System.Collections.Generic;

[Serializable]
public class WorldLaws
{
	public List<PlayerOptionData> list;

	[NonSerialized]
	public Dictionary<string, PlayerOptionData> dict;

	public PlayerOptionData add(PlayerOptionData pData)
	{
		foreach (PlayerOptionData item in list)
		{
			if (string.Equals(pData.name, item.name))
			{
				dict.TryAdd(item.name, item);
				item.on_switch = pData.on_switch;
				return item;
			}
		}
		list.Add(pData);
		dict.Add(pData.name, pData);
		return pData;
	}

	public void check()
	{
		init();
	}

	public void updateCaches()
	{
		foreach (WorldLawAsset item in AssetManager.world_laws_library.list)
		{
			item.updateCachedEnabled(this);
		}
	}

	public void init(bool pUpdateCaches = true)
	{
		if (list == null)
		{
			list = new List<PlayerOptionData>();
		}
		if (dict == null)
		{
			dict = new Dictionary<string, PlayerOptionData>();
		}
		foreach (WorldLawAsset item in AssetManager.world_laws_library.list)
		{
			add(new PlayerOptionData(item.id)
			{
				boolVal = item.default_state,
				on_switch = item.on_state_change
			});
		}
		foreach (WorldAgeAsset item2 in AssetManager.era_library.list)
		{
			add(new PlayerOptionData(item2.id)
			{
				boolVal = true
			});
		}
		if (pUpdateCaches)
		{
			updateCaches();
		}
		PowerButton.checkActorSpawnButtons();
	}

	public bool isAgeEnabled(string pID)
	{
		return dict[pID].boolVal;
	}

	public void setAgeEnabled(string pID, bool pValue)
	{
		dict[pID].boolVal = pValue;
	}

	public bool isEnabled(string pId)
	{
		if (!dict.TryGetValue(pId, out var value))
		{
			return false;
		}
		return value.boolVal;
	}

	public void enable(string pID)
	{
		if (dict.TryGetValue(pID, out var value))
		{
			value.boolVal = true;
			updateCaches();
		}
	}
}
