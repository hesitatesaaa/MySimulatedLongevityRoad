using System;

public class RegionLink : IEquatable<RegionLink>
{
	public int id;

	public readonly HashSetMapRegion regions = new HashSetMapRegion();

	public void reset()
	{
		regions.Clear();
		id = -1;
	}

	public override int GetHashCode()
	{
		return id;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as RegionLink);
	}

	public bool Equals(RegionLink pObject)
	{
		return id == pObject.id;
	}
}
