using System;
using Newtonsoft.Json;

[Serializable]
public class CityStorageSlot
{
	public string id;

	public int amount;

	[JsonIgnore]
	public ResourceAsset asset => AssetManager.resources.get(id);

	public CityStorageSlot(string pID)
	{
		create(pID);
	}

	public void create(string pID)
	{
		id = pID;
	}
}
