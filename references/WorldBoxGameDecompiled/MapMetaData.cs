using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

[Serializable]
public class MapMetaData
{
	[NonSerialized]
	public string temp_date_string = "";

	public int saveVersion;

	public int width;

	public int height;

	public MapStats mapStats;

	public int cities;

	public int units;

	public int population;

	public int structures;

	public int mobs;

	public int vegetation;

	public long deaths;

	public int kingdoms;

	public int buildings;

	public int equipment;

	public int books;

	public int wars;

	public int alliances;

	public int families;

	public int clans;

	public int cultures;

	public int religions;

	public int languages;

	public int subspecies;

	public int favorites;

	public int favorite_items;

	public bool cursed;

	[DefaultValue(false)]
	public bool modded;

	[DefaultValue(null)]
	public List<string> modsActive = new List<string>();

	public double timestamp;

	public void prepareForSave()
	{
		modded = Config.MODDED;
		if (modded)
		{
			modsActive = new List<string>(ModLoader.getModsLoaded());
		}
		timestamp = Epoch.Current();
	}

	public string toJson()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		return JsonConvert.SerializeObject((object)this, new JsonSerializerSettings
		{
			DefaultValueHandling = (DefaultValueHandling)3
		});
	}
}
