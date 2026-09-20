using System;

[Serializable]
public class BuildOrder : Asset
{
	public int required_pop;

	public int required_buildings;

	public int limit_type;

	public bool check_full_village;

	public bool check_house_limit;

	public int min_zones;

	public bool upgrade;

	public string[] requirements_orders;

	public string[] requirements_types;

	public BuildingAsset getBuildingAsset(City pCity, string pOrderID = null)
	{
		if (string.IsNullOrEmpty(pOrderID))
		{
			pOrderID = id;
		}
		return pCity.getActorAsset().architecture_asset.getBuilding(pOrderID);
	}
}
