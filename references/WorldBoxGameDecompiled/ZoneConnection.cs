using System;
using System.Runtime.CompilerServices;

public readonly struct ZoneConnection(TileZone pZone, MapRegion pRegion) : IEquatable<ZoneConnection>
{
	public readonly TileZone zone = pZone;

	public readonly MapRegion region = pRegion;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ZoneConnection pObject)
	{
		if (zone.Equals(pObject.zone))
		{
			return region.Equals(pObject.region);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return zone.GetHashCode() + region.GetHashCode() * 100000;
	}
}
