using ai.behaviours;

public class BehFindRandomFrontTileNearHouse : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		Building homeBuilding = pActor.getHomeBuilding();
		if (homeBuilding == null)
		{
			return BehResult.Stop;
		}
		WorldTile door_tile = homeBuilding.door_tile;
		if (!door_tile.isSameIsland(pActor.current_tile))
		{
			if (homeBuilding.current_tile.isSameIsland(pActor.current_tile))
			{
				pActor.beh_tile_target = homeBuilding.current_tile;
				return BehResult.Continue;
			}
			return BehResult.Stop;
		}
		using ListPool<WorldTile> listPool = new ListPool<WorldTile>();
		for (int i = 0; i < 3; i++)
		{
			WorldTile tile = BehaviourActionBase<Actor>.world.GetTile(door_tile.x + i, door_tile.y);
			if (tile != null && door_tile.isSameIsland(tile))
			{
				listPool.Add(tile);
			}
		}
		for (int j = 0; j < 3; j++)
		{
			WorldTile tile2 = BehaviourActionBase<Actor>.world.GetTile(door_tile.x - j, door_tile.y);
			if (tile2 != null && door_tile.isSameIsland(tile2))
			{
				listPool.Add(tile2);
			}
		}
		if (listPool.Count == 0)
		{
			return BehResult.Stop;
		}
		WorldTile random = listPool.GetRandom();
		pActor.beh_tile_target = random;
		return BehResult.Continue;
	}
}
