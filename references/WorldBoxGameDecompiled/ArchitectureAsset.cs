using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class ArchitectureAsset : Asset
{
	public bool generate_buildings;

	public string generation_target;

	public bool spread_biome;

	public string spread_biome_id;

	[DefaultValue("")]
	public string projectile_id = string.Empty;

	[DefaultValue(true)]
	public bool burnable_buildings = true;

	[DefaultValue(true)]
	public bool acid_affected_buildings = true;

	[DefaultValue(true)]
	public bool has_shadows = true;

	[DefaultValue("building")]
	public string material = "building";

	public Dictionary<string, string> building_ids_for_construction;

	public string[] styled_building_orders;

	public (string, string)[] shared_building_orders;

	[DefaultValue("boat_fishing")]
	public string actor_asset_id_boat_fishing = "boat_fishing";

	public string actor_asset_id_trading;

	public string actor_asset_id_transport;

	public void addBuildingOrderKey(string pKey, string pID)
	{
		if (building_ids_for_construction == null)
		{
			building_ids_for_construction = new Dictionary<string, string>();
		}
		building_ids_for_construction[pKey] = pID;
	}

	public void replaceSharedID(string pID, string pNewID)
	{
		for (int i = 0; i < shared_building_orders.Length; i++)
		{
			if (shared_building_orders[i].Item1 == pID)
			{
				shared_building_orders[i].Item2 = pNewID;
				break;
			}
		}
	}

	public BuildingAsset getBuilding(string pOrderID)
	{
		string buildingID = getBuildingID(pOrderID);
		return AssetManager.buildings.get(buildingID);
	}

	public string getBuildingID(string pOrderID)
	{
		return building_ids_for_construction[pOrderID];
	}
}
