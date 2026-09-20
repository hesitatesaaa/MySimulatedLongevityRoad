using System;
using Newtonsoft.Json;

[Serializable]
public abstract class Asset : IEquatable<Asset>
{
	public const string DEFAULT_ASSET_ID = "ASSET_ID";

	[JsonProperty(Order = -1)]
	public string id = "ASSET_ID";

	private int _hashcode;

	private int _index;

	public virtual void create()
	{
	}

	public void setHash(int pHash)
	{
		_hashcode = pHash;
	}

	public void setIndexID(int pValue)
	{
		_index = pValue;
	}

	public int getIndexID()
	{
		return _index;
	}

	public bool Equals(Asset pAsset)
	{
		return _hashcode == pAsset.GetHashCode();
	}

	public override int GetHashCode()
	{
		return _hashcode;
	}

	public bool isTemplateAsset()
	{
		if (id.StartsWith("$"))
		{
			return true;
		}
		if (id.StartsWith("_"))
		{
			return true;
		}
		return false;
	}
}
