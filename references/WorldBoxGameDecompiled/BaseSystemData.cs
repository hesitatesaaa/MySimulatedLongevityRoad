using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using SQLite;
using UnityEngine.Scripting;
using UnityPools;

[Serializable]
public abstract class BaseSystemData : IDisposable
{
	[DefaultValue(null)]
	public List<NameEntry> past_names;

	[DefaultValue(null)]
	public CustomDataContainer<int> custom_data_int;

	[DefaultValue(null)]
	public CustomDataContainer<long> custom_data_long;

	[DefaultValue(null)]
	public CustomDataContainer<float> custom_data_float;

	[DefaultValue(null)]
	public CustomDataContainer<bool> custom_data_bool;

	[DefaultValue(null)]
	public CustomDataContainer<string> custom_data_string;

	[DefaultValue(null)]
	public HashSet<string> custom_data_flags;

	[JsonIgnore]
	public bool from_db;

	[PrimaryKey]
	[NotNull]
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[DefaultValue(-1L)]
	public long id { get; set; } = -1L;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string name { get; set; }

	public bool custom_name { get; set; }

	[DefaultValue(-1L)]
	public long name_culture_id { get; set; } = -1L;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[DefaultValue(0.0)]
	public double created_time { get; set; }

	[DefaultValue(0.0)]
	public double died_time { get; set; }

	[DefaultValue(false)]
	public bool favorite { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	[Preserve]
	[DefaultValue(1)]
	[Obsolete("Use created_time instead")]
	public int age
	{
		set
		{
			if (value >= 1 && !(created_time > 0.0))
			{
				created_time = (float)(-1 * value) * 60f;
			}
		}
	}

	[JsonIgnore]
	[Ignore]
	public float this[string pKey]
	{
		get
		{
			get(pKey, out var pResult, 0f);
			return pResult;
		}
		set
		{
			set(pKey, value);
		}
	}

	[JsonIgnore]
	public string obsidian_name_id => name + "(" + id + ")";

	public void cloneCustomDataFrom(BaseSystemData pTarget)
	{
		if (pTarget.custom_data_int != null)
		{
			foreach (KeyValuePair<string, int> item in pTarget.custom_data_int.dict)
			{
				set(item.Key, item.Value);
			}
		}
		if (pTarget.custom_data_long != null)
		{
			foreach (KeyValuePair<string, long> item2 in pTarget.custom_data_long.dict)
			{
				set(item2.Key, item2.Value);
			}
		}
		if (pTarget.custom_data_float != null)
		{
			foreach (KeyValuePair<string, float> item3 in pTarget.custom_data_float.dict)
			{
				set(item3.Key, item3.Value);
			}
		}
		if (pTarget.custom_data_bool != null)
		{
			foreach (KeyValuePair<string, bool> item4 in pTarget.custom_data_bool.dict)
			{
				set(item4.Key, item4.Value);
			}
		}
		if (pTarget.custom_data_string != null)
		{
			foreach (KeyValuePair<string, string> item5 in pTarget.custom_data_string.dict)
			{
				set(item5.Key, item5.Value);
			}
		}
		if (pTarget.custom_data_flags == null)
		{
			return;
		}
		foreach (string custom_data_flag in pTarget.custom_data_flags)
		{
			addFlag(custom_data_flag);
		}
	}

	public Dictionary<string, string> debug()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (custom_data_int != null)
		{
			foreach (KeyValuePair<string, int> item in custom_data_int.dict)
			{
				dictionary.Add(item.Key, item.Value.ToString());
			}
		}
		if (custom_data_long != null)
		{
			foreach (KeyValuePair<string, long> item2 in custom_data_long.dict)
			{
				dictionary.Add(item2.Key, item2.Value.ToString());
			}
		}
		if (custom_data_float != null)
		{
			foreach (KeyValuePair<string, float> item3 in custom_data_float.dict)
			{
				dictionary.Add(item3.Key, item3.Value.ToString(CultureInfo.InvariantCulture));
			}
		}
		if (custom_data_bool != null)
		{
			foreach (KeyValuePair<string, bool> item4 in custom_data_bool.dict)
			{
				dictionary.Add(item4.Key, item4.Value.ToString());
			}
		}
		if (custom_data_string != null)
		{
			foreach (KeyValuePair<string, string> item5 in custom_data_string.dict)
			{
				dictionary.Add(item5.Key, item5.Value);
			}
		}
		if (custom_data_flags != null)
		{
			foreach (string custom_data_flag in custom_data_flags)
			{
				dictionary.Add("Flag", custom_data_flag);
			}
		}
		return dictionary;
	}

	public void save()
	{
		checkInt();
		checkLong();
		checkFloat();
		checkBool();
		checkString();
		checkFlags();
	}

	public void checkInt()
	{
		CustomDataContainer<int> customDataContainer = custom_data_int;
		if (customDataContainer != null && customDataContainer.dict.Count == 0)
		{
			custom_data_int.Dispose();
			custom_data_int = null;
		}
	}

	public void checkLong()
	{
		CustomDataContainer<long> customDataContainer = custom_data_long;
		if (customDataContainer != null && customDataContainer.dict.Count == 0)
		{
			custom_data_long.Dispose();
			custom_data_long = null;
		}
	}

	public void checkFloat()
	{
		CustomDataContainer<float> customDataContainer = custom_data_float;
		if (customDataContainer != null && customDataContainer.dict.Count == 0)
		{
			custom_data_float.Dispose();
			custom_data_float = null;
		}
	}

	public void checkBool()
	{
		CustomDataContainer<bool> customDataContainer = custom_data_bool;
		if (customDataContainer != null && customDataContainer.dict.Count == 0)
		{
			custom_data_bool.Dispose();
			custom_data_bool = null;
		}
	}

	public void checkString()
	{
		CustomDataContainer<string> customDataContainer = custom_data_string;
		if (customDataContainer != null && customDataContainer.dict.Count == 0)
		{
			custom_data_string.Dispose();
			custom_data_string = null;
		}
	}

	public void checkFlags()
	{
		if (custom_data_flags != null && custom_data_flags.Count == 0)
		{
			UnsafeCollectionPool<HashSet<string>, string>.Release(custom_data_flags);
			custom_data_flags = null;
		}
	}

	public void load()
	{
	}

	public void get(string pKey, out int pResult, int pDefault = 0)
	{
		if (custom_data_int == null || !custom_data_int.TryGetValue(pKey, out pResult))
		{
			pResult = pDefault;
		}
	}

	public void get(string pKey, out long pResult, long pDefault = 0L)
	{
		if (custom_data_long == null || !custom_data_long.TryGetValue(pKey, out pResult))
		{
			pResult = pDefault;
		}
	}

	public void get(string pKey, out float pResult, float pDefault = 0f)
	{
		if (custom_data_float == null || !custom_data_float.TryGetValue(pKey, out pResult))
		{
			pResult = pDefault;
		}
	}

	public void get(string pKey, out string pResult, string pDefault = null)
	{
		if (custom_data_string == null || !custom_data_string.TryGetValue(pKey, out pResult))
		{
			pResult = pDefault;
		}
	}

	public void get(string pKey, out bool pResult, bool pDefault = false)
	{
		if (custom_data_bool == null || !custom_data_bool.TryGetValue(pKey, out pResult))
		{
			pResult = pDefault;
		}
	}

	public int set(string pKey, int pData)
	{
		if (custom_data_int == null)
		{
			custom_data_int = new CustomDataContainer<int>();
		}
		custom_data_int[pKey] = pData;
		return pData;
	}

	public long set(string pKey, long pData)
	{
		if (custom_data_long == null)
		{
			custom_data_long = new CustomDataContainer<long>();
		}
		custom_data_long[pKey] = pData;
		return pData;
	}

	public float set(string pKey, float pData)
	{
		if (custom_data_float == null)
		{
			custom_data_float = new CustomDataContainer<float>();
		}
		custom_data_float[pKey] = pData;
		return pData;
	}

	public string set(string pKey, string pData)
	{
		if (custom_data_string == null)
		{
			custom_data_string = new CustomDataContainer<string>();
		}
		custom_data_string[pKey] = pData;
		return pData;
	}

	public bool set(string pKey, bool pData)
	{
		if (custom_data_bool == null)
		{
			custom_data_bool = new CustomDataContainer<bool>();
		}
		custom_data_bool[pKey] = pData;
		return pData;
	}

	public void change(string pKey, int pValue, int pMin = 0, int pMax = 1000)
	{
		get(pKey, out var pResult, 0);
		pResult += pValue;
		if (pResult < pMin)
		{
			pResult = pMin;
		}
		if (pResult > pMax)
		{
			pResult = pMax;
		}
		set(pKey, pResult);
	}

	public void removeInt(string pKey)
	{
		if (custom_data_int != null)
		{
			custom_data_int.Remove(pKey);
			checkInt();
		}
	}

	public void removeLong(string pKey)
	{
		if (custom_data_long != null)
		{
			custom_data_long.Remove(pKey);
			checkLong();
		}
	}

	public void removeFloat(string pKey)
	{
		if (custom_data_float != null)
		{
			custom_data_float.Remove(pKey);
			checkFloat();
		}
	}

	public void removeString(string pKey)
	{
		if (custom_data_string != null)
		{
			custom_data_string.Remove(pKey);
			checkString();
		}
	}

	public void removeBool(string pKey)
	{
		if (custom_data_bool != null)
		{
			custom_data_bool.Remove(pKey);
			checkBool();
		}
	}

	public bool addFlag(string pID)
	{
		if (custom_data_flags == null)
		{
			custom_data_flags = UnsafeCollectionPool<HashSet<string>, string>.Get();
		}
		return custom_data_flags.Add(pID);
	}

	public bool hasFlag(string pID)
	{
		return custom_data_flags?.Contains(pID) ?? false;
	}

	public void removeFlag(string pID)
	{
		if (custom_data_flags != null)
		{
			custom_data_flags.Remove(pID);
			checkFlags();
		}
	}

	public virtual void Dispose()
	{
		custom_data_int?.Dispose();
		custom_data_long?.Dispose();
		custom_data_float?.Dispose();
		custom_data_bool?.Dispose();
		custom_data_string?.Dispose();
		custom_data_int = null;
		custom_data_long = null;
		custom_data_float = null;
		custom_data_bool = null;
		custom_data_string = null;
		custom_data_flags?.Clear();
		checkFlags();
		past_names?.Clear();
		past_names = null;
	}
}
