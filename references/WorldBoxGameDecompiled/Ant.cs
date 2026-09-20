using System.Collections.Generic;

internal static class Ant
{
	private static List<WorldTile> _axis_neighbours = new List<WorldTile>(4);

	public static WorldTile getNextTile(WorldTile pTile, ActorDirection pDirection)
	{
		return pDirection switch
		{
			ActorDirection.Up => pTile.tile_up, 
			ActorDirection.UpRight => pTile?.tile_up?.tile_right, 
			ActorDirection.UpLeft => pTile?.tile_up?.tile_left, 
			ActorDirection.Right => pTile.tile_right, 
			ActorDirection.Down => pTile.tile_down, 
			ActorDirection.DownRight => pTile?.tile_down?.tile_right, 
			ActorDirection.DownLeft => pTile?.tile_down?.tile_left, 
			ActorDirection.Left => pTile.tile_left, 
			_ => null, 
		};
	}

	public static WorldTile randomNeighbour(WorldTile pTile)
	{
		try
		{
			_axis_neighbours.Add(pTile.tile_up);
			_axis_neighbours.Add(pTile.tile_right);
			_axis_neighbours.Add(pTile.tile_left);
			_axis_neighbours.Add(pTile.tile_down);
			foreach (WorldTile item in _axis_neighbours.LoopRandom())
			{
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}
		finally
		{
			_axis_neighbours.Clear();
		}
	}

	internal static void antUseOnTile(WorldTile pTile, string pType)
	{
		MapAction.terraformMain(pTile, AssetManager.tiles.get(pType), TerraformLibrary.destroy);
		MusicBox.playSound("event:/SFX/UNIQUE/langton/ant_step", pTile);
	}
}
