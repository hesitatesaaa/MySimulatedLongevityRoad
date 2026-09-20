using System;
using Newtonsoft.Json;

[Serializable]
public struct ResourceContainer
{
	[JsonProperty]
	public readonly string id;

	public int amount;

	[JsonIgnore]
	public ResourceAsset asset => AssetManager.resources.get(id);

	public ResourceContainer(string pID, int pAmount)
	{
		id = pID;
		amount = pAmount;
	}
}
