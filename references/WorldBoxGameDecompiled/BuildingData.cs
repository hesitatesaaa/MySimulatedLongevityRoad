using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine.Scripting;

[Serializable]
public class BuildingData : BaseObjectData
{
	[DefaultValue(BuildingState.Normal)]
	public BuildingState state = BuildingState.Normal;

	public int mainX;

	public int mainY;

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string asset_id;

	[DefaultValue(-1L)]
	public long cityID = -1L;

	public float grow_time;

	public CityResources resources;

	public StorageBooks books;

	[DefaultValue(-1)]
	public int frameID = -1;

	[Preserve]
	[Obsolete("use .id instead", true)]
	public long objectID
	{
		set
		{
			if (value.hasValue() && !base.id.hasValue())
			{
				base.id = value;
			}
		}
	}

	[Preserve]
	[Obsolete("use .asset_id instead", true)]
	public string templateID
	{
		set
		{
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(asset_id))
			{
				asset_id = value;
			}
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		resources?.Dispose();
		resources = null;
		books?.Dispose();
		books = null;
	}
}
