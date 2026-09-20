using System;
using System.ComponentModel;

[Serializable]
public class BookData : BaseSystemData
{
	public string book_type;

	public string path_cover;

	public string path_icon;

	public string author_name;

	[DefaultValue(-1L)]
	public long author_id = -1L;

	public string author_clan_name;

	[DefaultValue(-1L)]
	public long author_clan_id = -1L;

	public string author_kingdom_name;

	[DefaultValue(-1L)]
	public long author_kingdom_id = -1L;

	public string author_city_name;

	[DefaultValue(-1L)]
	public long author_city_id = -1L;

	[DefaultValue(-1L)]
	public long language_id = -1L;

	public string language_name;

	[DefaultValue(-1L)]
	public long culture_id = -1L;

	public string culture_name;

	[DefaultValue(-1L)]
	public long religion_id = -1L;

	public string religion_name;

	public int times_read;

	public double timestamp_read_last_time;

	public string trait_id_actor = string.Empty;

	public string trait_id_language = string.Empty;

	public string trait_id_culture = string.Empty;

	public string trait_id_religion = string.Empty;

	[DefaultValue(-1L)]
	public long building_id = -1L;
}
