using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
public class CityData : MetaObjectData
{
	public CityEquipment equipment;

	public List<ZoneData> zones = new List<ZoneData>();

	public int total_food_consumed;

	public float timer_supply;

	public float timer_trade;

	public List<LeaderEntry> past_rulers;

	[DefaultValue(0)]
	public int total_leaders;

	public double timestamp_kingdom;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string original_actor_asset { get; set; }

	[DefaultValue(-1L)]
	public long kingdomID { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long leaderID { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long founder_id { get; set; } = -1L;

	public string founder_name { get; set; }

	[DefaultValue(-1L)]
	public long last_leader_id { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long last_kingdom_id { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long id_culture { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long id_language { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long id_religion { get; set; } = -1L;

	public long left { get; set; }

	public long joined { get; set; }

	public long moved { get; set; }

	public long migrated { get; set; }

	[Preserve]
	[Obsolete("use .name instead", true)]
	public string cityName
	{
		set
		{
			if (string.IsNullOrEmpty(base.name) && !string.IsNullOrEmpty(value))
			{
				base.name = value;
			}
		}
	}

	[Preserve]
	[Obsolete("use .id instead", true)]
	public long cityID
	{
		set
		{
			if (value.hasValue() && !base.id.hasValue())
			{
				base.id = value;
			}
		}
	}

	[Preserve]
	[Obsolete("use .original_actor_asset instead", true)]
	public string race
	{
		set
		{
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(original_actor_asset))
			{
				original_actor_asset = value;
			}
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		zones.Clear();
		past_rulers?.Clear();
		past_rulers = null;
		equipment?.Dispose();
		equipment = null;
	}
}
