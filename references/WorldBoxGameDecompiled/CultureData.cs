using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
public class CultureData : MetaObjectData
{
	public int banner_decor_id;

	public int banner_element_id;

	public string creator_city_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_city_id = -1L;

	[DefaultValue(-1L)]
	public long creator_id = -1L;

	public string creator_name = string.Empty;

	public string creator_species_id = string.Empty;

	public string creator_subspecies_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_subspecies_id = -1L;

	[DefaultValue(-1L)]
	public long creator_kingdom_id = -1L;

	public string creator_kingdom_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_clan_id = -1L;

	public string creator_clan_name = string.Empty;

	public List<string> saved_traits;

	public double timestamp_last_written_book;

	public Dictionary<MetaType, string> onomastics;

	[JsonProperty("year")]
	[Preserve]
	[Obsolete("not used anymore", false)]
	public int year_obsolete;

	[DefaultValue("")]
	public string name_template_set = "";

	[DefaultValue(-1L)]
	public long parent_culture_id = -1L;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string original_actor_asset { get; set; }

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
		saved_traits?.Clear();
		saved_traits = null;
		onomastics?.Clear();
		onomastics = null;
		base.Dispose();
	}
}
