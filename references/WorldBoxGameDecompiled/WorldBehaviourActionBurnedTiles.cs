using System.Collections.Generic;

public static class WorldBehaviourActionBurnedTiles
{
	private static readonly HashSet<WorldTile> _burned_tiles = new HashSet<WorldTile>();

	private static readonly List<WorldTile> _tiles_to_remove = new List<WorldTile>();

	public static void addTile(WorldTile pTile)
	{
		_burned_tiles.Add(pTile);
	}

	public static void clear()
	{
		_burned_tiles.Clear();
		_tiles_to_remove.Clear();
	}

	public static int countBurnedTiles()
	{
		return _burned_tiles.Count;
	}

	public static void updateBurnedTiles()
	{
		if (_burned_tiles.Count == 0)
		{
			return;
		}
		_tiles_to_remove.Clear();
		foreach (WorldTile burned_tile in _burned_tiles)
		{
			if (!burned_tile.isOnFire())
			{
				burned_tile.burned_stages--;
				World.world.burned_layer.setTileDirty(burned_tile);
				if (burned_tile.burned_stages <= 0)
				{
					burned_tile.burned_stages = 0;
					_tiles_to_remove.Add(burned_tile);
				}
			}
		}
		foreach (WorldTile item in _tiles_to_remove)
		{
			_burned_tiles.Remove(item);
		}
	}
}
