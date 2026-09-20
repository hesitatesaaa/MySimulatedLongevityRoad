using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
public class KingdomData : MetaObjectData
{
	[DefaultValue(-1.0)]
	public double timestamp_alliance = -1.0;

	public List<string> saved_traits;

	[DefaultValue(-1.0)]
	public double timestamp_last_war = -1.0;

	[DefaultValue(-1.0)]
	public double timestamp_new_conquest = -1.0;

	[DefaultValue(-1.0)]
	public double timestamp_king_rule = -1.0;

	public float timer_new_king = 10f;

	public List<LeaderEntry> past_rulers;

	[DefaultValue(0)]
	public int total_kings;

	[DefaultValue(-1L)]
	public long allianceID { get; set; } = -1L;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string original_actor_asset { get; set; }

	[DefaultValue(-1L)]
	public long royal_clan_id { get; set; } = -1L;

	public string motto { get; set; }

	[DefaultValue(-1L)]
	public long id_culture { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long id_language { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long id_religion { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long kingID { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long capitalID { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long last_capital_id { get; set; } = -1L;

	[DefaultValue(1)]
	public int last_army_id { get; set; } = 1;

	[Preserve]
	[Obsolete("use .color_id instead", true)]
	public int colorId
	{
		set
		{
			if (value != -1 && base.color_id == 0)
			{
				setColorID(value);
			}
		}
	}

	[Preserve]
	[Obsolete("use .original_actor_asset instead", true)]
	public string raceId
	{
		set
		{
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(original_actor_asset))
			{
				original_actor_asset = value;
			}
		}
	}

	[DefaultValue(0)]
	public int banner_background_id { get; set; }

	[DefaultValue(0)]
	public int banner_icon_id { get; set; }

	public long left { get; set; }

	public long joined { get; set; }

	public long moved { get; set; }

	public long migrated { get; set; }

	public override void Dispose()
	{
		base.Dispose();
		saved_traits?.Clear();
		saved_traits = null;
		past_rulers?.Clear();
		past_rulers = null;
	}
}
