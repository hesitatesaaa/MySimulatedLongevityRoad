using System;
using System.Collections.Generic;

[Serializable]
public class DisasterAsset : Asset
{
	public DisasterAction action;

	public int rate = 1;

	public float chance = 1f;

	public string world_log;

	public int min_world_population;

	public int min_world_cities;

	public bool premium_only;

	public DisasterType type = DisasterType.Other;

	public string spawn_asset_unit = string.Empty;

	public string spawn_asset_building = string.Empty;

	public int max_existing_units = 20;

	public int units_min = 1;

	public int units_max = 10;

	public HashSet<string> ages_allow = new HashSet<string>();

	public HashSet<string> ages_forbid = new HashSet<string>();
}
