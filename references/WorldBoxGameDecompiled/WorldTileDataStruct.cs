using System;

public readonly struct WorldTileDataStruct : IEquatable<WorldTileDataStruct>
{
	public readonly string type;

	public readonly int height;

	public readonly ConwayType conwayType;

	public readonly double fire_timestamp;

	public readonly bool frozen;

	public readonly int tile_id;

	public readonly int x;

	public readonly int y;

	public WorldTileDataStruct(string pType, int pHeight, ConwayType pConwayType, bool pFire, double pFireTimestamp, bool pFrozen, int pTileID, int pX, int pY)
	{
		type = pType;
		height = pHeight;
		conwayType = pConwayType;
		fire_timestamp = pFireTimestamp;
		frozen = pFrozen;
		tile_id = pTileID;
		x = pX;
		y = pY;
	}

	public WorldTileDataStruct(WorldTile pTile, int pTileID)
	{
		WorldTileData data = pTile.data;
		type = data.type;
		height = data.height;
		conwayType = data.conwayType;
		fire_timestamp = data.fire_timestamp;
		frozen = data.frozen;
		tile_id = pTileID;
		x = pTile.x;
		y = pTile.y;
	}

	public bool Equals(WorldTileDataStruct pOther)
	{
		return tile_id == pOther.tile_id;
	}

	public override bool Equals(object pObject)
	{
		if (pObject is WorldTileDataStruct pOther)
		{
			return Equals(pOther);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return tile_id;
	}
}
