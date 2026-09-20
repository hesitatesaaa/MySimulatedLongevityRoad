using System.ComponentModel;

public class FamilyData : MetaObjectData
{
	public int banner_background_id;

	public int banner_frame_id;

	[DefaultValue(-1L)]
	public long alpha_id = -1L;

	public string founder_actor_name_1;

	public string founder_actor_name_2;

	[DefaultValue(-1L)]
	public long main_founder_id_1 = -1L;

	[DefaultValue(-1L)]
	public long main_founder_id_2 = -1L;

	[DefaultValue(-1L)]
	public long subspecies_id = -1L;

	public string subspecies_name = string.Empty;

	public string species_id = string.Empty;

	[DefaultValue(-1L)]
	public long founder_city_id = -1L;

	public string founder_city_name = string.Empty;

	[DefaultValue(-1L)]
	public long founder_kingdom_id = -1L;

	public string founder_kingdom_name = string.Empty;

	[DefaultValue(-1L)]
	public long original_family_1 = -1L;

	[DefaultValue(-1L)]
	public long original_family_2 = -1L;
}
