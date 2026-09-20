using System;

public class WorldBehaviourTilesRunner
{
	internal static WorldTile[] _tiles_to_check;

	internal static int _tile_next_check;

	public static void clearTilesToCheck()
	{
		_tiles_to_check = null;
		_tile_next_check = 0;
	}

	public static void checkTiles()
	{
		if (_tiles_to_check == null || _tile_next_check >= _tiles_to_check.Length - 1)
		{
			if (_tiles_to_check == null)
			{
				_tiles_to_check = new WorldTile[World.world.tiles_list.Length];
				Array.Copy(World.world.tiles_list, _tiles_to_check, World.world.tiles_list.Length);
			}
			_tile_next_check = 0;
		}
	}
}
