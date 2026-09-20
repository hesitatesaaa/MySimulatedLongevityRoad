using System.Collections.Generic;
using System.ComponentModel;

public class ReligionData : MetaObjectData
{
	public int banner_background_id;

	public int banner_icon_id;

	public string creator_city_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_city_id = -1L;

	public string creator_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_id = -1L;

	public string creator_species_id = string.Empty;

	[DefaultValue(-1L)]
	public long creator_subspecies_id = -1L;

	public string creator_subspecies_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_kingdom_id = -1L;

	public string creator_kingdom_name = string.Empty;

	[DefaultValue(-1L)]
	public long creator_clan_id = -1L;

	public string creator_clan_name = string.Empty;

	public List<string> saved_traits;

	public override void Dispose()
	{
		base.Dispose();
		saved_traits?.Clear();
		saved_traits = null;
	}
}
