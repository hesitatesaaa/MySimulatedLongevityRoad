using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
public class ClanData : MetaObjectData
{
	public string motto;

	[DefaultValue(-1L)]
	public long chief_id = -1L;

	[DefaultValue(-1L)]
	public long culture_id = -1L;

	public List<LeaderEntry> past_chiefs;

	public List<string> saved_traits;

	public int books_written;

	public int banner_background_id;

	public int banner_icon_id;

	public string founder_actor_name;

	[DefaultValue(-1L)]
	public long founder_actor_id = -1L;

	public string founder_kingdom_name;

	[DefaultValue(-1L)]
	public long founder_kingdom_id = -1L;

	public string founder_city_name;

	[DefaultValue(-1L)]
	public long founder_city_id = -1L;

	public string creator_species_id = string.Empty;

	public string creator_subspecies_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_subspecies_id = -1L;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string original_actor_asset { get; set; }

	[Preserve]
	[Obsolete("use .original_actor_asset instead", true)]
	public string race_id
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
		saved_traits?.Clear();
		saved_traits = null;
		past_chiefs?.Clear();
		past_chiefs = null;
	}
}
