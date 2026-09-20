using UnityEngine;

public class DebugHighlightContainer
{
	public Color color;

	public float timer = 0.2f;

	public float interval = 0.2f;

	public TileZone zone;

	public MapChunk chunk;

	public WorldTile tile;

	public void setTimer(float pVal)
	{
		interval = pVal;
		timer = pVal;
	}
}
