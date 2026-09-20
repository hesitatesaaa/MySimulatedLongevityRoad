using System;
using System.ComponentModel;

[Serializable]
public class WorldTileData
{
	public string type;

	public int height;

	[DefaultValue(ConwayType.None)]
	public ConwayType conwayType = ConwayType.None;

	[NonSerialized]
	public double fire_timestamp;

	[NonSerialized]
	public bool frozen;

	public readonly int tile_id;

	public WorldTileData(int pTileID)
	{
		tile_id = pTileID;
		clear();
	}

	internal void clear()
	{
		type = null;
		height = 0;
		conwayType = ConwayType.None;
		fire_timestamp = 0.0;
		frozen = false;
	}
}
