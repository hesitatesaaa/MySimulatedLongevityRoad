using UnityEngine;

public class BaseMapObject : BaseWorldObject
{
	public WorldTile current_tile;

	public float position_height;

	public Vector2 current_position;

	public override void Dispose()
	{
		current_tile = null;
		base.Dispose();
	}
}
